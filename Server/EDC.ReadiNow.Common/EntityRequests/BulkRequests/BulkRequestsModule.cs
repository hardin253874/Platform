// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Autofac dependency injection module for bulk requests engine.
    /// </summary>
    public class BulkRequestsModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // CachingBulkRequestRunner
            builder
                .Register( c => new CachingBulkRequestRunner( ) )
                .As<IBulkRequestRunner>( )
                .As<ICacheService>( )
                .SingleInstance( );

            // Cache of secured metadata results
			// (Shared per rule-set and invalidated on any application metadata changes)
            builder.Register( cc => new CacheFactory { MetadataCache = true }.Create<CachingBulkRequestRunnerKey, IEnumerable<EntityData>>( "BulkRequestRunner Secured Result" ) )
                .Named<EDC.Cache.ICache<CachingBulkRequestRunnerKey, IEnumerable<EntityData>>>( "BulkRequestRunner Secured Result" )
                .SingleInstance( );
        }
    }

}
