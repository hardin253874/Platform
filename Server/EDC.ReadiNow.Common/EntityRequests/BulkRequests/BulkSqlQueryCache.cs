// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Provides a cache for pre-prepared SQL statements for loading BulkEntity data.
    /// It is *critical* that information stored within only relates to the query string,
    /// and does not relate either to the specific entities being loaded, or the specific user making the query.
    /// Queries are however cached per tenant.
    /// </summary>
    static class BulkSqlQueryCache
    {
        /// <summary>
        /// Per-tenant cache mapping request strings to prepared BulkSqlQuery requests.
        /// </summary>
        private static readonly ICache<string, BulkSqlQuery> _requestCache =
            new CacheFactory { BlockIfPending = true }.Create<string, BulkSqlQuery>( "Bulk SQL Query" );


        /// <summary>
        /// Build, or retrieve from cache, a prepared SQL query.
        /// </summary>
        /// <param name="request">The query to load.</param>
        /// <returns>An object containing the sql query and other lookup data.</returns>
        public static BulkSqlQuery GetBulkSqlQuery(EntityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (request.Request == null && request.RequestString == null)
                throw new ArgumentException("Either Request or RequestString must be specified", "request");

            // Check the cache
            string key = GetCacheKey( request );
            BulkSqlQuery query = _requestCache.GetOrAdd(key, k => CreateQuery(request));

            return query;
        }


        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="request">The query to load.</param>
        /// <returns>An object containing the sql query and other lookup data.</returns>
        private static BulkSqlQuery CreateQuery(EntityRequest request)
        {
            BulkSqlQuery query;

            // Do not require access to the individual query components
            using (new SecurityBypassContext())
            {
                try
                {
                    // Parse the request
                    EntityMemberRequest memberRequest;
                    if (request.Request != null)
                        memberRequest = request.Request;
                    else
                        memberRequest = Factory.RequestParser.ParseRequestQuery(request.RequestString);

                    // Tweak it to handle request-type-specific hacks
                    BulkRequestRunner.AdjustEntityMemberRequest(request, memberRequest);

                    // Convert the request into a bulk SQL query.
                    query = BulkRequestSqlBuilder.BuildSql(memberRequest, request.Hint);
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError(ex.ToString());
                    throw;
                }
            }
            return query;
        }


        /// <summary>
        /// Build a key for caching request results.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetCacheKey( EntityRequest request )
        {
            string requestCacheKey = null;

            if ( !string.IsNullOrEmpty( request.RequestString ) )
            {
                requestCacheKey = request.RequestString;
            }
            else if ( request.Request != null )
            {
                requestCacheKey = request.Request.GetCacheKey( );
            }
            else
            {
                throw new ArgumentException( "Neither request string nor request object were provided." );
            }

            string key = string.Concat( request.QueryType, requestCacheKey );
            return key;
        }



		/// <summary>
		/// Reset the cache.
		/// </summary>
        public static void Clear( )
        {
            _requestCache.Clear( );
        }
    }
}
