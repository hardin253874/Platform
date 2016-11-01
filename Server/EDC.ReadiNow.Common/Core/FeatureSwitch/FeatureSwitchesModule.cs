// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;


namespace EDC.ReadiNow.Core.FeatureSwitch
{

    public class FeatureSwitchModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder">The autofac container builder.</param>
        protected override void Load( ContainerBuilder builder )
        {
            // Register PerTenantCacheInvalidator
            builder.RegisterType<EntityFeatureSwitch>( )
                .As<IFeatureSwitch>( )
                .SingleInstance( );
        }
    }
}
