// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.Cache;
using ERC = EDC.ReadiNow.Cache;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Core.Cache.Providers
{
    /// <summary>
    ///     A cache that isolates values for each tenant.
    /// </summary>
    /// <remarks>
    ///     This class is thread-safe if the inner cache is thread-safe.
    /// </remarks>
    /// <typeparam name="TKey">
    ///     The cache key.
    /// </typeparam>
    /// <typeparam name="TValue">
    ///     The cache value.
    /// </typeparam>
    public class MetadataCache<TKey, TValue> : PerUserRuleSetCache<TKey, TValue>, ERC.ICacheService
    {

        ICacheInvalidator _invalidator;

        public MetadataCache(ICache<Tuple<int, TKey>, TValue> innerCache, IUserRuleSetProvider ruleSetProvider, ICacheInvalidator invalidator) : base(innerCache, ruleSetProvider)
        {
            _invalidator = invalidator;
        }

        ICacheInvalidator ERC.ICacheService.CacheInvalidator
        {
            get
            {
                return _invalidator;
            }
        }
    }


    internal class MetadataCacheInvalidator : ICacheInvalidator
    {
        public const long AllTenants = 0;

        private readonly object _syncRoot = new object();

        static readonly MetadataCacheInvalidator _instance = new MetadataCacheInvalidator();

        readonly HashSet<Tuple<ICache, long>> _caches;

        public static MetadataCacheInvalidator Instance { get { return _instance; } }

        protected MetadataCacheInvalidator()
        {
            _caches = new HashSet<Tuple<ICache, long>>() ;
        }

        /// <summary>
        /// Registers a metadata cache for invalidation.
        /// </summary>
        /// <param name="cache">The cache</param>
        /// <param name="tenantId">The tenant ID to invalidate on, or zero to invalidate regardless of tenant.</param>
        public void RegisterCache(ICache cache, long tenantId = 0)
        {
            lock(_syncRoot)
            {
                _caches.Add(new Tuple<ICache, long>(cache, tenantId));
            }
        }

        public void OnEntityChange(IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities)
        {
            if (_caches.Count > 0 && AreMetadata(entities))
            {
                InvalidateMetadataCaches(RequestContext.TenantId);
            }
        }

        /// <summary>
        /// Cause all registered metadata caches to be invalidated.
        /// </summary>
        public void InvalidateMetadataCaches(long tenantId)
        {
            IEnumerable<Tuple<ICache, long>> caches;
            lock (_syncRoot)
            {
                caches = _caches.ToArray();
            }

            foreach (var entry in caches)
            {
                ICache cache = entry.Item1;
                long cacheTenant = entry.Item2;
                if (cacheTenant == tenantId || cacheTenant == 0 || tenantId == 0)
                {
                    cache.Clear();
                }
            }   
        }


        /// <summary>
        /// Are any of the entities metadata
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private static bool AreMetadata(IList<IEntity> entities)
        {
            try
            {
                using ( new SecurityBypassContext( ) )
                {
                    IEnumerable<long> entityTypeIds = entities.SelectMany( e => e.TypeIds ).Distinct();
                    
                    foreach ( long entityTypeId in entityTypeIds )
                    {
                        EntityType entityType = Entity.Get<EntityType>( entityTypeId );
                        if ( entityType != null && entityType.IsMetadata == true )
                        {
                            EventLog.Application.WriteTrace( "MetadataCaches will be invalidated due change on instance of '{0}' ({1})", entityType.Name, entityTypeId );
                
                            return true;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                EventLog.Application.WriteError( "MetadataCacheInvalidator failed in AreMetadata: {0}", ex );
            }
            return false;
        }

        public string Name
        {
            get { return "MetadataCacheInvalidator"; }
        }

        public void OnRelationshipChange(IList<EntityRef> relationshipTypes)
        {
            // Do nothing
        }

        public void OnFieldChange(IList<long> fieldTypes)
        {
            // Do nothing
        }
    }


    /// <summary>
    /// Creates a per tenant cache
    /// </summary>
    public class MetadataCacheFactory : ICacheFactory
    {

        public MetadataCacheFactory()
        {
            RuleSetProvider = Factory.UserRuleSetProvider;
        }

        /// <summary>
        /// The factory for the inner cache.
        /// </summary>
        public ICacheFactory Inner { get; set; }

        /// <summary>
        /// The Rule set provider to use when diving the cache
        /// </summary>
        public IUserRuleSetProvider RuleSetProvider { get; set; }


        /// <summary>
        /// Is this cache thread-safe.
        /// </summary>
        public bool ThreadSafe
        {
            get { return Inner.ThreadSafe; }
        }


        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
            var innerCache = Inner.Create<Tuple<int, TKey>, TValue>( cacheName );
            var result = new MetadataCache<TKey, TValue>(innerCache, RuleSetProvider, MetadataCacheInvalidator.Instance);

            MetadataCacheInvalidator.Instance.RegisterCache(result, _associateNewCachesWithTenant);

            return result;
        }

        public static IDisposable AssociateNewCachesWithTenant(long tenantId)
        {
            long old = _associateNewCachesWithTenant;
            _associateNewCachesWithTenant = tenantId;

            return ContextHelper.Create( () =>
                {
                    _associateNewCachesWithTenant = old;
                } );
        }
        private static long _associateNewCachesWithTenant;
        
    }

}


