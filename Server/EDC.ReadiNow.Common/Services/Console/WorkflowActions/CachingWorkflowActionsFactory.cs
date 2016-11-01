// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.AttributeMetadata;
using EDC.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Diagnostics;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Core.Cache;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Services.Console.WorkflowActions
{
    /// <summary>
    /// Find the workflows that apply as actions to the given types
    /// </summary>
    public class CachingWorkflowActionsFactory : ICacheService, IWorkflowActionsFactory
    {
        private readonly object _syncRoot;
        private readonly WorkflowActionsFactoryInvalidator _cacheInvalidator;
        private IWorkflowActionsFactory _fetcher;

        /// <summary>
		/// Create a new <see cref="CachingWorkflowActionsFactory"/>.
        /// </summary>
        /// <param name="fetcher">
        /// The fetcher for the info.
        /// </param>
        public CachingWorkflowActionsFactory(IWorkflowActionsFactory fetcher)
        {
            if (fetcher == null)
            {
                throw new ArgumentNullException("fetcher");
            }

            _fetcher = fetcher;

            Cache = CacheFactory.CreateSimpleCache<CachingWorkflowActionsFactoryKey, CachingWorkflowActionsFactoryValue>( "Workflow Action for Type" );

            _cacheInvalidator = new WorkflowActionsFactoryInvalidator(Cache);
            _syncRoot = new object();
        }


        /// <summary>
        /// The cache.
        /// </summary>
        internal ICache<CachingWorkflowActionsFactoryKey, CachingWorkflowActionsFactoryValue> Cache { get; private set; }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator { get { return _cacheInvalidator; } }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            System.Diagnostics.Trace.WriteLine("CachingWorkflowActionsForType: Cache cleared");
            Cache.Clear();
        }


		/// <summary>
		/// Convert a <see cref="Report" /> to a <see cref="StructuredQuery" />.
		/// </summary>
		/// <param name="typeIds">The type ids.</param>
		/// <returns>
		/// The converted report.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">report</exception>
        public IEnumerable<Workflow> Fetch(ISet<long> typeIds)
        {
            if (typeIds == null)
            {
                throw new ArgumentNullException("typeIds");
            }
           

            CachingWorkflowActionsFactoryValue cacheValue;

            var key = new CachingWorkflowActionsFactoryKey( typeIds );
            
            // Check cache
            bool found = Cache.TryGetOrAdd(key, out cacheValue, ( innerKey ) =>
                {
                    using ( CacheContext cacheContext = new CacheContext( ) )
                    {
                        var result = new CachingWorkflowActionsFactoryValue( _fetcher.Fetch(innerKey.TypeIds));

                        // Add the cache context entries to the appropriate CacheInvalidator
                        _cacheInvalidator.AddInvalidations( cacheContext, key );
                        return result;
                    }
                }
            );

            if (found && CacheContext.IsSet( ) )
            {
                // Add the already stored changes that should invalidate this cache
                // entry to any outer or containing cache contexts.
                using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                {
                    cacheContext.AddInvalidationsFor( _cacheInvalidator, key );
                }
            }

            return cacheValue.Workflows;
        }

       
    }

    /// <summary>
    /// Coordinate invalidating the report to query cache.
    /// </summary>
    internal class WorkflowActionsFactoryInvalidator : CacheInvalidator<CachingWorkflowActionsFactoryKey, CachingWorkflowActionsFactoryValue>
    {
        /// <summary>
		/// Create a new <see cref="WorkflowActionsFactoryInvalidator"/>.
        /// </summary>
        /// <param name="cache"></param>
        public WorkflowActionsFactoryInvalidator(ICache<CachingWorkflowActionsFactoryKey, CachingWorkflowActionsFactoryValue> cache)
            : base(cache, "Workflow Action for Type")
        {
            // Do nothing
        }
    }

    /// <remarks>The key is order dependent. This may result in additional cache misses but as most of the tests will  only be against a single
    /// type it should be a rare occurence</remarks>
    [DataContract]
    public class CachingWorkflowActionsFactoryKey : IEquatable<CachingWorkflowActionsFactoryKey>
    {
        private int _hash;

        /// <summary>
        /// Constructor 
        /// </summary>
        internal CachingWorkflowActionsFactoryKey(ISet<long> typeIds)
        {
            TypeIds = typeIds;
            GetHashCode();
        }

        /// <summary>
        /// Parameterless constructor used by serialization only.
        /// </summary>
        private CachingWorkflowActionsFactoryKey()
        {
            // Do nothing
        }

		/// <summary>
		/// Called after deserialization.
		/// </summary>
		[OnDeserialized]
		private void OnAfterDeserialization( )
		{
			GetHashCode( );
		}

        /// <summary>
        /// The report ID.
        /// </summary>
        [DataMember(Order = 1)]
        public ISet<long> TypeIds { get; private set; }

        
        public bool Equals(CachingWorkflowActionsFactoryKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TypeIds.SequenceEqual(other.TypeIds);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CachingWorkflowActionsFactoryKey)obj);
        }

        public override int GetHashCode()
        {
            if (_hash == 0)
            {

                // cofuzzing with Berstein hash
                unchecked
                {
                    int hash = 17;

                    foreach(var l in TypeIds)
						hash = hash * 92821 + l.GetHashCode( );
                    
                    _hash = hash;
                }
            }

            return _hash;
        }

        public static bool operator ==(CachingWorkflowActionsFactoryKey left, CachingWorkflowActionsFactoryKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CachingWorkflowActionsFactoryKey left, CachingWorkflowActionsFactoryKey right)
        {
            return !Equals(left, right);
        }
    }


    /// <summary>
    /// Cache value container for CachingReportToQueryConverter
    /// </summary>
    internal class CachingWorkflowActionsFactoryValue
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CachingWorkflowActionsFactoryValue(IEnumerable<Workflow> workflows)
        {
            if (workflows == null)
                throw new ArgumentNullException("workflows");

            Workflows = workflows;
            CacheTime = DateTime.UtcNow;
        }

        /// <summary>
        /// The query originally used to generate the cached result.
        /// </summary>
        public IEnumerable<Workflow> Workflows
        {
            get;
            private set;
        }

        /// <summary>
        /// The cached result.
        /// </summary>
        public DateTime CacheTime
        {
            get;
            private set;
        }
    }
    
}
