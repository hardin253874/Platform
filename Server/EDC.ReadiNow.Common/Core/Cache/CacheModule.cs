// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Core.Cache.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Core.Cache
{
    /// <summary>
    /// Module for registering platform cache classes.
    /// </summary>
    class CacheModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder">The autofac container builder.</param>
        protected override void Load( ContainerBuilder builder )
        {
            // Register PerTenantCacheInvalidator
            builder.RegisterType<PerTenantCacheInvalidator>( )
                .As<IPerTenantCacheInvalidator>( )
                .SingleInstance( );
        }
    }
}
