// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Model.Client;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    /// <summary>
    /// Console DI classes.
    /// </summary>
    public class ConsoleModule: Module
    {
        /// <summary>
        /// Register DI classes.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleTreeRepository>()
                .As<ConsoleTreeRepository>()
                .SingleInstance();

            // IQueryRepository  (role-checking, caching)
            builder.Register(
                c => new CachingConsoleTreeRepository(c.Resolve<ConsoleTreeRepository>()))
                .As<IConsoleTreeRepository>()
                .As<ICacheService>()
                .SingleInstance();

            // Note that metdata caches are secured by security rule-set and tenant
            builder.Register(cc => new CacheFactory { MetadataCache = true }.Create<long, EntityData>("TreeRequest Secured Result"))
                .Named<EDC.Cache.ICache<long, EntityData>>("TreeRequest Secured Result")
                .SingleInstance();
        }
    }
}