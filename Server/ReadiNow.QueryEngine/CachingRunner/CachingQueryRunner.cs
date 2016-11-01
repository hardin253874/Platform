// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using EDC.Cache;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model.CacheInvalidation;
using Autofac.Extras.AttributeMetadata;
using EDC.ReadiNow.Core;
using ReadiNow.QueryEngine.CachingBuilder;
using EDC.ReadiNow.Diagnostics;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Core.Cache;

namespace ReadiNow.QueryEngine.CachingRunner
{
    /// <summary>
    /// A caching implemetation of IQueryRunner.
    /// </summary>
    /// <remarks>
    /// Unfortunately the nice abstraction layers needed to be broken slightly because the caching layer needs to build the query in order to
    /// determine if the run result is cacheable.
    /// </remarks>
    public class CachingQueryRunner : IQueryRunner, ICacheService, IQueryRunnerCacheKeyProvider
    {
        /// <summary>
        /// User ID to use for cache entries that will be shared between users.
        /// </summary>
        private const int ShareAcrossUsersCacheKey = -1;

        private readonly CachingQueryRunnerInvalidator _cacheInvalidator;

	    /// <summary>
        /// Create a new <see cref="CachingQueryRunner"/>.
        /// </summary>
        /// <param name="queryRunner">
        /// The <see cref="IQueryRunner"/> that will actually generate the SQL. This cannot be null. 
        /// </param>
        /// <param name="userRuleSetProvider"></param>
        /// <param name="querySqlBuilder"></param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="queryRunner"/> cannot be null.
        /// </exception>
        public CachingQueryRunner( [WithKey( Factory.NonCachedKey )] IQueryRunner queryRunner, IUserRuleSetProvider userRuleSetProvider, IQuerySqlBuilder querySqlBuilder )
        {
            if ( queryRunner == null )
            {
                throw new ArgumentNullException( "queryRunner" );
            }
            if ( userRuleSetProvider == null )
            {
                throw new ArgumentNullException( "userRuleSetProvider" );
            }

            QueryRunner = queryRunner;
            QuerySqlBuilder = querySqlBuilder;
            UserRuleSetProvider = userRuleSetProvider;            

            // Create cache
            var fact = new CacheFactory
            {
                CacheName = "Query Result",
				MaxCacheEntries = CacheFactory.DefaultMaximumCacheSize
            };
            Cache = fact.Create<CachingQueryRunnerKey, CachingQueryRunnerValue>( );

            _cacheInvalidator = new CachingQueryRunnerInvalidator( Cache );
        }

        /// <summary>
        /// The inner <see cref="IQueryRunner"/> that actually executes queries.
        /// </summary>
        internal IQueryRunner QueryRunner { get; }

        /// <summary>
        /// The inner <see cref="IQuerySqlBuilder"/> that actually builds SQL queries.
        /// </summary>
        internal IQuerySqlBuilder QuerySqlBuilder { get; }

        /// <summary>
        /// The <see cref="IUserRuleSetProvider"/> that can be used to group users by access rules.
        /// </summary>
        internal IUserRuleSetProvider UserRuleSetProvider { get; }

        /// <summary>
        /// The cache itself.
        /// </summary>
        internal ICache<CachingQueryRunnerKey, CachingQueryRunnerValue> Cache { get; }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator => _cacheInvalidator;

        /// <summary>
        /// Build the SQL, or collect it from cache.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public QueryResult ExecuteQuery( StructuredQuery query, QuerySettings settings )
        {
            // Validate
            if ( query == null )
            {
                throw new ArgumentNullException( "query" );
            }
            if ( QueryRunner == null )
            {
                throw new InvalidOperationException( "QueryRunner not set." );
            }
            if ( settings == null )
            {
                settings = new QuerySettings( );
            }

            // Determine if we should cache .. and the cache key
            QueryBuild builtQuery;
            CachingQueryRunnerKey key;
            CacheContext queryBuilderCacheContext;
            using (queryBuilderCacheContext = new CacheContext())
            {
                key = CreateCacheKeyAndQuery(query, settings, out builtQuery);
            }

            // A null key means that the ersult should not participate in caching
            if ( key == null )
            {
                return RunQueryImpl( query, settings, builtQuery );
            }

            CachingQueryRunnerValue cacheValue;

            using ( MessageContext msg = new MessageContext( "Reports" ) )
            {
                // Check for force recalculation
                if ( settings.RefreshCachedResult )
                {
                    msg.Append( ( ) => "CachingQueryRunner refreshed forced" );
                    Cache.Remove( key );
                }

                // Run query
                bool fromCache = TryGetOrAdd( key, msg, out cacheValue, callbackKey =>
                {
                    using ( CacheContext cacheContext = new CacheContext( ) )
                    {
                        QueryResult queryResult = RunQueryImpl( query, settings, builtQuery );
                        cacheValue = new CachingQueryRunnerValue( query, queryResult );

                        // Add the cache context entries to the appropriate CacheInvalidator
                        _cacheInvalidator.AddInvalidations( cacheContext, callbackKey );
                        _cacheInvalidator.AddInvalidations( queryBuilderCacheContext, callbackKey );
                    }

                    return cacheValue;

                });

                if ( fromCache && CacheContext.IsSet() )
                {
                    using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                    {
                        // Add the already stored changes that should invalidate this cache
                        // entry to any outer or containing cache contexts.
                        cacheContext.AddInvalidationsFor( _cacheInvalidator, key );
                    }
                }
            }

            if ( cacheValue == null )
            {
                throw new Exception( "Assert false" );
            }

            // Mutate returned result to be suitable for current query
            QueryResult result;
            if ( cacheValue.OriginalQuery == query )
            {
                result = cacheValue.QueryResult;
            }
            else
            {
                result = MutateResultToMatchCurrentQuery( cacheValue, query );
            }

            return result;
        }
        /// <summary>
        /// Refer to CachingQuerySqlBuilder.MutateResultToMatchCurrentQuery
        /// </summary>
        internal static QueryResult MutateResultToMatchCurrentQuery( CachingQueryRunnerValue cacheValue, StructuredQuery currentQuery )
        {
            if ( cacheValue == null )
                throw new ArgumentNullException( "cacheValue" );
            if ( currentQuery == null )
                throw new ArgumentNullException( "currentQuery" );

            // Clone result
            QueryBuild queryBuild = CachingQuerySqlBuilder.MutateResultToMatchCurrentQuery( cacheValue.QueryResult.QueryBuild, cacheValue.OriginalQuery, currentQuery );
            QueryResult result = cacheValue.QueryResult.ShallowClone( queryBuild );
            return result;
        }


        /// <summary>
        /// Unsupported interface member that takes a pre-built query.
        /// </summary>
        public QueryResult ExecutePrebuiltQuery( StructuredQuery query, QuerySettings settings, QueryBuild prebuiltQuery )
        {
            throw new NotSupportedException( );
        }

        /// <summary>
        /// Try to get the value from cache, with logging.
        /// </summary>
        private bool TryGetOrAdd( CachingQueryRunnerKey key, MessageContext msg, out CachingQueryRunnerValue result, Func<CachingQueryRunnerKey, CachingQueryRunnerValue> valueFactory )
        {           
            bool foundValue;

            foundValue = Cache.TryGetOrAdd( key, out result, valueFactory );

            msg.Append( ( ) => "CachingQueryRunner key:" + key );
            if ( foundValue )
            {
                var cacheValue = result;
                msg.Append( ( ) => "CachingQueryRunner cache hit" );
                msg.Append( ( ) => $"Entry originally cached at {cacheValue.CacheTime}" );
            }
            else
            {
                msg.Append( ( ) => "CachingQueryRunner cache miss" );
            }

            return foundValue;
        }


        /// <summary>
        /// Actually build the SQL
        /// </summary>
        private QueryBuild BuildQueryImpl( StructuredQuery query, QuerySettings settings )
        {
            // Unless otherwise specified, use the cache provider for creating inner security queries as well.
            if ( settings.SecurityQueryRunner == null )
            {
                settings.SecurityQueryRunner = this;
            }

            return QuerySqlBuilder.BuildSql( query, settings );
        }


        /// <summary>
        /// Actually run the SQL
        /// </summary>
        private QueryResult RunQueryImpl( StructuredQuery query, QuerySettings settings, QueryBuild prebuiltQuery )
        {
            // Unless otherwise specified, use the cache provider for creating inner security queries as well.
            if ( settings.SecurityQueryRunner == null )
            {
                settings.SecurityQueryRunner = this;
            }

            return QueryRunner.ExecutePrebuiltQuery( query, settings, prebuiltQuery );
        }


        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear( )
        {
            Trace.WriteLine( "CachingQueryRunner: Cache cleared" );
            Cache.Clear( );
        }

        /// <summary>
        /// Returns a suitable key for comparing query runs.
        /// </summary>
        /// <returns>A key, or null if the query should not be cached.</returns>
        IQueryRunnerCacheKey IQueryRunnerCacheKeyProvider.CreateCacheKey( StructuredQuery query, QuerySettings settings )
        {
            QueryBuild builtQuery;
            return CreateCacheKeyAndQuery( query, settings, out builtQuery );
        }

        /// <summary>
        /// Returns a suitable key for comparing query runs.
        /// </summary>
        /// <returns>A key, or null if the query should not be cached.</returns>
        private CachingQueryRunnerKey CreateCacheKeyAndQuery( StructuredQuery query, QuerySettings settings, out QueryBuild builtQuery )
        {
            // Build the SQL for the query
            // Note: unfortunately this may mutate the query, so keep a copy
            StructuredQuery queryCopy = query.DeepCopy( );
            builtQuery = BuildQueryImpl( queryCopy, settings );

            // Check if query can even participate in cache
            bool doNotCacheResult = builtQuery.SqlIsUncacheable
                                || builtQuery.DataIsUncacheable
                                || !CachingQueryRunnerKey.DoesRequestAllowForCaching( query, settings );

            if ( doNotCacheResult )
            {
                return null;
            }

            // If we get to this point, then the cache is definitely participating
            // Determine the cache key - either shared across users, or specific to the current user.
            CachingQueryRunnerKey key = CreateCacheKeyImpl( query, settings, builtQuery.DataReliesOnCurrentUser );
            return key;
        }


        /// <summary>
        /// Create cache keys
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="settings">The query settings</param>
        /// <param name="perUserKey">True if the key is for a specific user; or false if it is to be shared across users.</param>
        /// <returns>The cache key</returns>
        private CachingQueryRunnerKey CreateCacheKeyImpl( StructuredQuery query, QuerySettings settings, bool perUserKey )
        {
            // Get a user-set key
            // (Users may share the same report SQL if they have the same set of read-rules)
            UserRuleSet userRuleSet = null;
            if ( settings.RunAsUser != 0 )
            {
                userRuleSet = UserRuleSetProvider.GetUserRuleSet( settings.RunAsUser, Permissions.Read );
                if ( userRuleSet == null )
                    throw new InvalidOperationException( "Expected userRuleSet" ); // Assert false
            }

            // Create cache key
            // Cached with userId = -1 if the user 
            long userId = perUserKey ? settings.RunAsUser : ShareAcrossUsersCacheKey;
            var key = new CachingQueryRunnerKey( query, settings, userRuleSet, userId );
            return key;
        }
    }
}
