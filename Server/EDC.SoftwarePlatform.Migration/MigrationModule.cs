// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Processing.Xml;
using ReadiNow.ImportExport;

namespace ReadiNow.Migration
{
    /// <summary>
    /// Autofac dependency injection module for query engine.
    /// </summary>
    public class MigrationModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // SecurityProcessor
            builder.RegisterType<SecurityProcessor>( )
                .As<ISecurityProcessor>( );

            // EntityXmlExporter 
            builder.RegisterType<EntityXmlExporter>( )
                .As<IEntityXmlExporter>( );

            // EntityXmlImporter
            builder.RegisterType<EntityXmlImporter>( )
                .As<IEntityXmlImporter>( );
        }
    }
}
