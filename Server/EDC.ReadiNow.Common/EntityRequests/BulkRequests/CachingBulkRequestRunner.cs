// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Model.CacheInvalidation;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Run instances of the <see cref="EntityRequest"/> class storing the results in a cache.
    /// </summary>
    internal class CachingBulkRequestRunner : IBulkRequestRunner, ICacheService
    {
        private readonly CacheInvalidator<CachingBulkRequestRunnerKey, CachingBulkRequestRunnerValue> _cacheInvalidator;

		/// <summary>
		/// Create a new <see cref="CachingBulkRequestRunner" />.
		/// </summary>
        public CachingBulkRequestRunner( )
        {
            var cacheFactory = new CacheFactory { BlockIfPending = true };
            Cache = cacheFactory.Create<CachingBulkRequestRunnerKey, CachingBulkRequestRunnerValue>( "Bulk Request Runner" );

            _cacheInvalidator = new CacheInvalidator<CachingBulkRequestRunnerKey, CachingBulkRequestRunnerValue>( Cache, "Bulk Request Runner" );
        }

        /// <summary>
        /// The cache.
        /// </summary>
        internal ICache<CachingBulkRequestRunnerKey, CachingBulkRequestRunnerValue> Cache { get; private set; }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator { get { return _cacheInvalidator; } }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            System.Diagnostics.Trace.WriteLine( "CachingBulkRequestRunner: Cache cleared" );
            Cache.Clear();
        }

        /// <summary>
        /// Execute a request for bulk data from the SQL database.
        /// </summary>
        /// <param name="request">The requested data</param>
        /// <returns></returns>
        public BulkRequestResult GetBulkResult( EntityRequest request )
        {
            if ( request == null )
            {
                throw new ArgumentNullException( "request" );
            }

            // Bypass cache
            if ( request.IgnoreResultCache )
            {
                return CreateResult( request );
            }

            BulkRequestResult result;
            CachingBulkRequestRunnerValue cacheValue;
            CachingBulkRequestRunnerKey key = CachingBulkRequestRunnerKey.Create( request );

	        // Check cache
            bool inCache = Cache.TryGetValue( key, out cacheValue );

            // Should parent cache contexts be notified of invalidations
            // .. no for now, for compatibility with previous system. Consider changing
            bool notifyParentCacheContext = false;

            if ( !inCache )
            {
				using ( var cacheContext = new CacheContext( notifyParentCacheContext ? ContextType.New : ContextType.Detached ) )  // Detached for now..
				{
					result = CreateResult( request );
					cacheValue = new CachingBulkRequestRunnerValue( result );

					Cache.Add( key, cacheValue );

                    // Add the cache context entries to the appropriate CacheInvalidator
                    cacheContext.Entities.Add( result.AllEntities.Keys );
                    cacheContext.EntityInvalidatingRelationshipTypes.Add( GetRelationshipTypesUsed(result) );
 
                    _cacheInvalidator.AddInvalidations( cacheContext, key );
                }
            }
            else
            {
                if ( notifyParentCacheContext && CacheContext.IsSet( ) )
                {
                    using ( CacheContext cacheContext = new CacheContext( ContextType.Attached ) )
                    {
                        // Add the already stored changes that should invalidate this cache
                        // entry to any outer or containing cache contexts.
                        cacheContext.AddInvalidationsFor( _cacheInvalidator, key );
                    }
                }
            }

            result = cacheValue.BulkRequestResult;
            request.ResultFromCache = inCache;  // TODO: Find a better channel to return this info. (It can't be in the response, because that's cached)

            return result;
        }


        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private BulkRequestResult CreateResult( EntityRequest request )
        {
            // Get/create a query object
            BulkSqlQuery query = BulkSqlQueryCache.GetBulkSqlQuery( request );

            // Run query on SQL server
            BulkRequestResult unsecuredResult = BulkRequestSqlRunner.RunQuery( query, request.EntityIDsCanonical );

            return unsecuredResult;
        }


        /// <summary>
        /// Extract the list of relationship types referenced in the query.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private IEnumerable<long> GetRelationshipTypesUsed( BulkRequestResult result )
        {
            return result.BulkSqlQuery.Relationships.Keys.Select( Math.Abs );
        }
    }
}
