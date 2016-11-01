// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Model.CacheInvalidation;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Core.Cache;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A <see cref="IQueryRepository"/> that caches results. The cache is threadsafe.
    /// </summary>
    public class CachingQueryRepository : IQueryRepository, ICacheService
    {
        private readonly SecurityQueryCacheInvalidator _cacheInvalidator;

        /// <summary>
        /// Create a new <see cref="CachingQueryRepository"/>.
        /// </summary>
        /// <param name="queryRepository">
        /// The <see cref="IQueryRepository"/> that will actually load the
        /// security queries. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="queryRepository"/> cannot be null.
        /// </exception>
        public CachingQueryRepository(IQueryRepository queryRepository)
        {
            if (queryRepository == null)
            {
                throw new ArgumentNullException("queryRepository");
            }

            Repository = queryRepository;
            Cache = CacheFactory.CreateSimpleCache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>( "Access Control Query" );
            _cacheInvalidator = new SecurityQueryCacheInvalidator(Cache);
        }

        /// <summary>
        /// The inner <see cref="IQueryRepository"/> that actually loads queries.
        /// </summary>
        internal IQueryRepository Repository { get; private set; }

        /// <summary>
        /// The cache itself.
        /// </summary>
        internal ICache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>> Cache { get; private set; }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator { get { return _cacheInvalidator; } }

        /// <summary>
        /// Get the queries for a given user and permission or operation.
        /// </summary>
        /// <param name="subjectId">
        /// The ID of the <see cref="Subject"/>, that is a <see cref="UserAccount"/> or <see cref="Role"/> instance.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>. Or null to match all permissions.
        /// </param>
        /// <param name="securableEntityTypes">
        /// The IDs of <see cref="SecurableEntity"/> types being accessed. Or null to match all entity types.
        /// </param>
        /// <returns>
        /// The queries to run.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="subjectId"/> does not exist. Also, <paramref name="permission"/> should
        /// be one of <see cref="Permissions.Read"/>, <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Neither <paramref name="permission"/> nor <paramref name="securableEntityTypes"/> can be null.
        /// </exception>
        public IEnumerable<AccessRuleQuery> GetQueries(long subjectId, [CanBeNull] Model.EntityRef permission, [CanBeNull] IList<long> securableEntityTypes)
        {
            SubjectPermissionTypesTuple tuple;
            IEnumerable<AccessRuleQuery> result;

            result = null;
            tuple = new SubjectPermissionTypesTuple(subjectId, permission, securableEntityTypes);
            
            if ( !Cache.TryGetValue( tuple, out result ) )
            {
                using ( CacheContext cacheContext = new CacheContext( ) )
                {
                    result = Repository.GetQueries( subjectId, permission, securableEntityTypes ).ToList( );
                    try
                    {
                        Cache.Add( tuple, result );

                        // Add the cache context entries to the appropriate CacheInvalidator
                        _cacheInvalidator.AddInvalidations( cacheContext, tuple );
                    }
                    catch ( ArgumentException )
                    {
                        Trace.WriteLine( "CachingQueryRepository: Entity already in cache" );
                        throw;
                    }
                }
            }
            else if ( CacheContext.IsSet( ) )
            {
                using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                {
                    // Add the already stored changes that should invalidate this cache
                    // entry to any outer or containing cache contexts.
                    cacheContext.AddInvalidationsFor( _cacheInvalidator, tuple );
                }
            }

            return result;
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            Trace.WriteLine( "CachingQueryRepository: Cache cleared" );
            Cache.Clear( );
        }
    }
}
