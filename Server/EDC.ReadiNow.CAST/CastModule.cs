// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.CAST.Marketplace.Services;
using EDC.ReadiNow.CAST.Services;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// DI module for the CAST common library.
    /// </summary>
    public class CastModule : Module
    {
        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CastService>().As<ICastService>();
            builder.RegisterType<CastActivityService>().As<ICastActivityService>();
            builder.RegisterType<CastEntityHelper>().As<ICastEntityHelper>();
            builder.RegisterType<AccountService>().As<IAccountService>();
            builder.RegisterType<MarketplaceService>().As<IMarketplaceService>();
            builder.RegisterType<ApplicationService>().As<IApplicationService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<TenantService>().As<ITenantService>();
            builder.RegisterType<PlatformService>().As<IPlatformService>();
        }
    }
}
