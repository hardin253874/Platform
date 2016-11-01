// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Cache;

namespace EDC.ReadiNow.Security.SecuredData
{
    /// <summary>
    ///     Autofac module for access control.
    /// </summary>
    public class SecuredDataModule : Module
    {
        /// <summary>
        ///     Load the registrations.
        /// </summary>
        /// <param name="builder">
        ///     The Autofac provided <see cref="ContainerBuilder" />.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType< DbSecuredData>()
                .As<ISecuredData>()
                .SingleInstance();

            builder.RegisterType< SecuredDataSaveHelper>()
                .As<ISecuredDataSaveHelper>()
                .SingleInstance();
        }
    }
}