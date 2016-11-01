// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Autofac dependency injection module for document generation engine.
    /// </summary>
    public class DocGenModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Document Generator
            builder.RegisterType<Generator>()
                .As<IDocumentGenerator>();

            // StaticBuilder
            builder.RegisterType<ExternalServices>()
                .As<ExternalServices>();         
        }
    }
}
