// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing.Xml;
using EDC.ReadiNow.Core;
using ReadiNow.ImportExport;

namespace EDC.SoftwarePlatform.Migration.Processing
{
    /// <summary>
    ///     Coordinates operations for importing/exporting graphs of entities
    /// </summary>
    public static class EntityManager
    {
        /// <summary>
        ///     Exports the tenant.
        /// </summary>
        /// <remarks>
        /// This is the entry point for export requests that come via PlatformConfigure. c.f. EntityXmlExporter.GenerateXml.
        /// </remarks>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="entityId">Root entity to export.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="exportSettings">Export settings.</param>
        /// <param name="context">The context.</param>
        public static void ExportEntity( string tenantName, long entityId, string packagePath, IProcessingContext context = null )
        {
            if ( string.IsNullOrEmpty( tenantName ) )
                throw new ArgumentNullException( nameof( tenantName ) );
            if ( string.IsNullOrEmpty( packagePath ) )
                throw new ArgumentNullException( nameof( packagePath ) );

            if ( context == null )
                context = new ProcessingContext( );

            context.Report.StartTime = DateTime.Now;

            long tenantId = TenantHelper.GetTenantId( tenantName, true );

            /////
            // Create source to load app data from tenant
            /////
            using ( IDataTarget target = FileManager.CreateDataTarget( Format.XmlVer2, packagePath ) )
            {
                var exporter = ( EntityXmlExporter )Factory.EntityXmlExporter;
                exporter.ExportEntity( tenantId, new[] { entityId }, target, context, false );
            }

            context.Report.EndTime = DateTime.Now;
        }

        /// <summary>
        ///     Imports the entity
        /// </summary>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="ignoreMissingDeps"></param>
        /// <param name="context">The context.</param>
        public static void ImportEntity(string tenantName, string packagePath, bool ignoreMissingDeps, IProcessingContext context = null)
        {
            if (string.IsNullOrEmpty(tenantName))
                throw new ArgumentNullException(nameof(tenantName));
            if (string.IsNullOrEmpty(packagePath))
                throw new ArgumentNullException(nameof(packagePath));

            if (context == null)
                context = new ProcessingContext();

            context.Report.StartTime = DateTime.Now;

            long tenantId = TenantHelper.GetTenantId(tenantName, true);

            /////
            // Create source to load app data from tenant
            /////
            using ( IDataSource importSource = FileManager.CreateDataSource( packagePath ) )
            {
                EntityXmlImportSettings settings = new EntityXmlImportSettings
                {
                    IgnoreMissingDependencies = ignoreMissingDeps
                };
                EntityXmlImporter importer = ( EntityXmlImporter )Factory.EntityXmlImporter;
                try
                {
                    importer.ImportEntity( tenantId, importSource, settings, context );
                }
                catch ( EntityXmlImporter.ImportDependencyException )
                {
                    context.WriteError( "Import failed due to missing dependencies. Use -ignoreMissingDeps True to ignore." );
                }
            }

            context.Report.EndTime = DateTime.Now;
        }
    }
}
