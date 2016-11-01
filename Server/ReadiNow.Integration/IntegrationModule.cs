// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using Autofac.Extras.AttributeMetadata;
using System;

using EDC.ReadiNow.Core;

using EDC.ReadiNow.Model;
using ReadiNow.Integration.Sms;

namespace ReadiNow.Integration
{
    /// <summary>
    /// Autofac dependency injection module for Integration.
    /// </summary>
    public class IntegrationModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            builder.RegisterType<TwilioSmsReceiver>()
                .As<ITwilioSmsReceiver>( )
                .SingleInstance();

            builder.RegisterType<TwilioApi>()
                .As<ISmsProvider>()
                .SingleInstance();
        }
    }
}
