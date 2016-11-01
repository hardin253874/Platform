// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;

namespace ReadiNow.Database
{
    /// <summary>
    /// Autofac dependency injection module for the database module.
    /// </summary>
    public class EntityModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder">The autofac container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            // Register DatabaseProvider
            builder.RegisterType<DatabaseProvider>()
                .As<IDatabaseProvider>();
        }
    }
}
