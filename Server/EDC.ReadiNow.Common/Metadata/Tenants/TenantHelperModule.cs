// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Metadata.Tenants
{
    class TenantHelperModule : Module
    {
        public static string TenantIdCacheName = "Tenant ID cache";
        public static string TenantNameCacheName = "Tenant Name cache";
         /// <summary>
         /// Loads the registrations.
         /// </summary>
         /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            // Cache of tenant names to IDs
            // Note* This needs to be distributed so that modifications from other
            // app domains (i.e. PlatformConfigure) get processed in the WebApi app domain.

            builder.Register(c => { return new CacheFactory { Distributed = true, IsolateTenants = false }.Create<string, long>(TenantIdCacheName); })
               .As<ICache<string, long>>()
               .Named<ICache<string, long>>(TenantIdCacheName)
               .SingleInstance();

            builder.Register(c => {
                return new CacheFactory { Distributed = true, IsolateTenants = false }.Create<long, string>(TenantNameCacheName); })
                .As<ICache<long, string>>()
                .Named<ICache<long, string>>(TenantNameCacheName)
                .SingleInstance();
        }
    }
}
