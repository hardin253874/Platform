// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Responsible for returning BulkRequestResult objects.
    /// Look in the cache. If not found, get a query, run it, and cache the result.
    /// </summary>
    internal static class BulkResultCache
    {
        /// <summary>
        /// Build, or retrieve from cache, a prepared SQL query.
        /// </summary>
        /// <remarks>
        /// Obsolete
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public static BulkRequestResult GetBulkResult( EntityRequest request )
        {
            var runner = Factory.Current.Resolve<IBulkRequestRunner>( );
            return runner.GetBulkResult( request );
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <remarks>
        /// Obsolete
        /// </remarks>
        public static void Clear( )
        {
            var runner = Factory.Current.Resolve<IBulkRequestRunner>( );
            var cache = ( ICacheService ) runner;
            cache.Clear( );
        }
    }
}
