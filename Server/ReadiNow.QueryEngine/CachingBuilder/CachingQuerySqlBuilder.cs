// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using EDC.Cache;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model.CacheInvalidation;
using Autofac.Extras.AttributeMetadata;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Core.Cache;

namespace ReadiNow.QueryEngine.CachingBuilder
{
    public class CachingQuerySqlBuilder : IQuerySqlBuilder, ICacheService
    {
        private readonly CachingQuerySqlBuilderInvalidator _cacheInvalidator;

        /// <summary>
        /// Create a new <see cref="CachingQuerySqlBuilder"/>.
        /// </summary>
        /// <param name="querySqlBuilder">
        /// The <see cref="IQuerySqlBuilder"/> that will actually generate the SQL. This cannot be null. 
        /// </param>
        /// <param name="userRuleSetProvider"></param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="querySqlBuilder"/> cannot be null.
        /// </exception>
        public CachingQuerySqlBuilder( [WithKey( Factory.NonCachedKey )] IQuerySqlBuilder querySqlBuilder, IUserRuleSetProvider userRuleSetProvider )
        {
            if ( querySqlBuilder == null )
            {
                throw new ArgumentNullException( "querySqlBuilder" );
            }
            if ( userRuleSetProvider == null )
            {
                throw new ArgumentNullException( "userRuleSetProvider" );
            }

            QuerySqlBuilder = querySqlBuilder;
            UserRuleSetProvider = userRuleSetProvider;

            var fact = new CacheFactory
            {
                CacheName = "Query SQL",
                BlockIfPending = true
            };
            Cache = fact.Create<CachingQuerySqlBuilderKey, CachingQuerySqlBuilderValue>( "Query SQL" );

            _cacheInvalidator = new CachingQuerySqlBuilderInvalidator( Cache );
        }

        /// <summary>
        /// The inner <see cref="IQuerySqlBuilder"/> that actually converts queries.
        /// </summary>
        internal IQuerySqlBuilder QuerySqlBuilder { get; }

        /// <summary>
        /// The <see cref="IUserRuleSetProvider"/> that can be used to group users by access rules.
        /// </summary>
        internal IUserRuleSetProvider UserRuleSetProvider { get; }

        /// <summary>
        /// The cache itself.
        /// </summary>
        internal ICache<CachingQuerySqlBuilderKey, CachingQuerySqlBuilderValue> Cache { get; }

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
        public QueryBuild BuildSql( StructuredQuery query, QuerySqlBuilderSettings settings )
        {
            // Validate
            if ( query == null )
            {
                throw new ArgumentNullException( "query" );
            }
            if ( QuerySqlBuilder == null )
            {
                throw new InvalidOperationException( "QuerySqlBuilder not set." );
            }
            if ( settings == null )
            {
                settings = new QuerySqlBuilderSettings( );
            }

            // Check if query can even participate in cache
            if ( !CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( query, settings ) )
            {
                return BuildSqlImpl( query, settings );
            }

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
            CachingQuerySqlBuilderKey key = new CachingQuerySqlBuilderKey( query, settings, userRuleSet );
            CachingQuerySqlBuilderValue cacheValue;

            using ( MessageContext msg = new MessageContext( "Reports" ) )
            {
                // Check for force recalculation
                if ( settings.RefreshCachedSql )
                {
                    msg.Append( ( ) => "CachingQuerySqlBuilder refreshed forced" );
                    _cacheInvalidator.DebugInvalidations.Add( key );
                    Cache.Remove( key );
                }

                // In some circumstances, a result will be uncacheable, so we just return 'null' in the delegate instead.
                // However, on the first access, we will still be doing the calculation, so store it here.
                CachingQuerySqlBuilderValue calculatedOnThisAccess = null;

                // Check cache
                bool fromCache = TryGetOrAdd( key, msg, out cacheValue, callbackKey =>
                {
                    // This callback is called if we have a cache miss

                    using ( CacheContext cacheContext = new CacheContext( ) )
                    {
                        QueryBuild queryResult = BuildSqlImpl( query, settings );
                        cacheValue = new CachingQuerySqlBuilderValue( query, queryResult );
                        calculatedOnThisAccess = cacheValue;

                        if ( queryResult.SqlIsUncacheable )
                        {
                            return null;
                        }
                        else
                        {
                            // Add the cache context entries to the appropriate CacheInvalidator
                            _cacheInvalidator.AddInvalidations( cacheContext, callbackKey );
                            return cacheValue;
                        }
                    }
                } );

                // cacheValue will be null if the result was uncacheable
                if ( cacheValue == null )
                {
                    if ( calculatedOnThisAccess != null )
                    {
                        // In this path, the result was uncacheable, so the cache returned a 'null',
                        // but it was the initial calculation run anyway, so we can get the value from callbackValue.
                        cacheValue = calculatedOnThisAccess;
                    }
                    else
                    {
                        // In this path, the result was uncacheable, but someone had asked previously, and stored
                        // the null, so we need to actually build the SQL again for this scenario.
                        // Note: don't need to do anything with cache context, because this cache is not participating.
                        // And if there's a parent context set, then the call to BuildSqlImpl will just talk directly to that context.
                        QueryBuild queryResult = BuildSqlImpl( query, settings );
                        cacheValue = new CachingQuerySqlBuilderValue( query, queryResult );
                    }
                }
                else if ( fromCache && CacheContext.IsSet( ) )
                {
                    // Add the already stored changes that should invalidate this cache
                    // entry to any outer or containing cache contexts.
                    using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                    {
                        cacheContext.AddInvalidationsFor( _cacheInvalidator, key );
                    }
                }
            }

            if (cacheValue == null)
            {
                throw new Exception("Assert false");
            }

            // Mutate returned result to be suitable for current query
            QueryBuild result;
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
        /// Actually build the SQL
        /// </summary>
        private QueryBuild BuildSqlImpl( StructuredQuery query, QuerySqlBuilderSettings settings )
        {
            // Unless otherwise specified, use the cache provider for creating inner security queries as well.
            if ( settings.SecurityQuerySqlBuilder == null )
            {
                settings.SecurityQuerySqlBuilder = this;
            }

            return QuerySqlBuilder.BuildSql( query, settings );
        }


        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear( )
        {
            Trace.WriteLine( "CachingQuerySqlBuilder: Cache cleared" );
            Cache.Clear( );
        }


        /// <summary>
        /// Try to get the value from cache, with logging.
        /// </summary>
        private bool TryGetOrAdd( CachingQuerySqlBuilderKey key, MessageContext msg, out CachingQuerySqlBuilderValue result, Func<CachingQuerySqlBuilderKey, CachingQuerySqlBuilderValue> callback )
        {
            bool foundValue;

            foundValue = Cache.TryGetOrAdd( key, out result, callback );

            msg.Append( ( ) => "CachingQuerySqlBuilder key:" + key );

            if ( foundValue )
            {
                var cacheValue = result;
                msg.Append( ( ) => "CachingQuerySqlBuilder cache hit" );
                msg.Append( ( ) => $"Entry originally cached at {cacheValue.CacheTime}" );
            }
            else
            {
                msg.Append( ( ) => "CachingQuerySqlBuilder cache miss" );
            }

            return foundValue;
        }


        /// <summary>
        /// The problem: we want to cache queries, but the StructuredQueries contain GUIDs that vary from call to call.
        /// The CachingQuerySqlBuilderKey happily handles this via StructuredQuery.CacheKeyToken. However, the result contains
        /// references back to the original query. And stuff down stream relies on having the correct GUIDs in the referenced columns.
        /// The solution: we take a shallowing copy of the result, and substutite in the columns from our current query.
        /// </summary>
        private static QueryBuild MutateResultToMatchCurrentQuery( CachingQuerySqlBuilderValue cacheValue, StructuredQuery currentQuery )
        {
            return MutateResultToMatchCurrentQuery( cacheValue.QueryResult, cacheValue.OriginalQuery, currentQuery );
        }


        /// <summary>
        /// The problem: we want to cache queries, but the StructuredQueries contain GUIDs that vary from call to call.
        /// The CachingQuerySqlBuilderKey happily handles this via StructuredQuery.CacheKeyToken. However, the result contains
        /// references back to the original query. And stuff down stream relies on having the correct GUIDs in the referenced columns.
        /// The solution: we take a shallowing copy of the result, and substutite in the columns from our current query.
        /// </summary>
        internal static QueryBuild MutateResultToMatchCurrentQuery( QueryBuild queryBuild, StructuredQuery originalQuery, StructuredQuery currentQuery )
        {
            if ( queryBuild == null )
                throw new ArgumentNullException( "queryBuild" );
            if ( originalQuery == null)
                throw new ArgumentNullException( "originalQuery" );
            if (currentQuery == null)
                throw new ArgumentNullException("currentQuery");

            // Clone result
            QueryBuild result = queryBuild.ShallowClone();
            result.Columns = MutateColumnList( result.Columns, originalQuery.SelectColumns, currentQuery.SelectColumns );
            result.AggregateColumns = MutateColumnList( result.AggregateColumns, originalQuery.SelectColumns, currentQuery.SelectColumns );            
            return result;
        }

        
        /// <summary>
        /// Clone a list of result columns, performing mapping of original select column to current select column.
        /// See notes in MutateResultToMatchCurrentQuery.
        /// </summary>
        private static List<ResultColumn> MutateColumnList( List<ResultColumn> columnList, List<SelectColumn> originalRequestColumns, List<SelectColumn> currentRequestColumns )
        {
            if ( columnList == null )
                return null;

            List<ResultColumn> result = new List<ResultColumn>( columnList.Count );

            foreach ( ResultColumn resultColumn in columnList )
            {
                ResultColumn cloneColumn = resultColumn.ShallowClone( );

                // Faster than setting up a dictionary when there are so few columns
                int index = -1;
                if ( cloneColumn.RequestColumn != null )
                {
                    index = originalRequestColumns.FindIndex( column => column.ColumnId == cloneColumn.RequestColumn.ColumnId );
                }
                if ( index != -1 )
                {                    
                    cloneColumn.RequestColumn = currentRequestColumns [ index ];
                }
                
                result.Add( cloneColumn );
            }

            return result;
        }
    }
}
