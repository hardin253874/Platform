// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Cache;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    ///     Autofac module for access control.
    /// </summary>
    public class SecurityModule : Module
    {
        /// <summary>
        ///     Load the registrations.
        /// </summary>
        /// <param name="builder">
        ///     The Autofac provided <see cref="ContainerBuilder" />.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(
                c => new IdentityProviderContextCache())
                .As<IdentityProviderContextCache>()
                .As<ICacheService>()
                .As<IIdentityProviderRequestContextCache>()
                .SingleInstance();
        }
    }
}