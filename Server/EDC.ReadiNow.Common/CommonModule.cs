// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Core.AsyncRunner;
using ReadiNow.Common;
using ReadiNow.Core;

namespace EDC.ReadiNow
{
    /// <summary>
    /// Autofac dependency injection module for document generation engine.
    /// </summary>
    public class CommonModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // Asynchronous runner.
            builder.RegisterType<AsyncRunner>( )
                .As<IAsyncRunner>( );

            // Date-time provider
            builder.RegisterType<DateTimeProvider>( )
                .As<IDateTime>( );
        }
    }
}
