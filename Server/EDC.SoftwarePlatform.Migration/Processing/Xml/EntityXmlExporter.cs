// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using ReadiNow.ImportExport;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing.Xml.Version1;
using EDC.SoftwarePlatform.Migration.Targets;
using EDC.SoftwarePlatform.Migration.Sources;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml
{
    /// <summary>
    ///     Implementation of IEntityXmlExporter that uses the migration logic.
    /// </summary>
    class EntityXmlExporter : IEntityXmlExporter
    {
        private ISecurityProcessor SecurityProcessor { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userRoleRepository"></param>
        public EntityXmlExporter( ISecurityProcessor securityProcessor )
        {
            if ( securityProcessor == null )
                throw new ArgumentNullException( nameof( securityProcessor ) );
            SecurityProcessor = securityProcessor;
        }

        /// <summary>
        /// Interface for providing XML export.
        /// </summary>
        /// <remarks>
        /// This is the entry point for export requests that come via the console. c.f. EntityManager.ExportEntity.
        /// </remarks>
        /// <param name="entityIds">ID of entity to export.</param>
        /// <param name="xmlWriter">Xml Writer to write the exported entity to.</param>
        /// <param name="settings">Export settings.</param>
        public void GenerateXml( IEnumerable<long> entityIds, XmlWriter xmlWriter, EntityXmlExportSettings settings )
        {
            if ( xmlWriter == null )
                throw new ArgumentNullException( nameof( xmlWriter ) );
            if ( settings == null )
                settings = EntityXmlExportSettings.Default;

            var context = new ProcessingContext( );

            context.Report.StartTime = DateTime.Now;

            long tenantId = RequestContext.TenantId;

            /////
            // Create source to load app data from tenant
            /////
            using ( IDataTarget target = CreateDataTarget( xmlWriter, settings ) )
            {
                ExportEntity( tenantId, entityIds, target, context, true );
            }

            context.Report.EndTime = DateTime.Now;
        }

        /// <summary>
        /// Implementation of exporting an entity.
        /// </summary>
        /// <remarks>
        /// This has been split to try and capture the common code shared by a PlatformConfigure export and a console export. 
        /// </remarks>
        /// <param name="tenantId">The tenant</param>
        /// <param name="entityIds">The entity</param>
        /// <param name="target">The target</param>
        /// <param name="context">Processing context.</param>
        /// <param name="demandReadPermission">If true, perform a read demand as the current user.</param>
        internal void ExportEntity( long tenantId, IEnumerable<long> entityIds, IDataTarget target, IProcessingContext context, bool demandReadPermission )
        {
            using ( IDataSource source = new TenantGraphSource {
                TenantId = tenantId,
                RootEntities = entityIds.ToList( ),
                DemandReadPermission = demandReadPermission
            } )
            {
                if ( demandReadPermission )
                {
                    CheckExportSecurity( source, context );
                }
                    
                /////
                // Copy the data
                /////
                var processor = new CopyProcessor( source, target, context );
                processor.MigrateData( );
            }
        }

        /// <summary>
        /// Interface for providing XML export.
        /// </summary>
        /// <param name="entityIds">ID of entity to export.</param>
        /// <param name="textWriter">Xml Writer to write the exported entity to.</param>
        /// <param name="settings">Export settings.</param>
        public string GenerateXml( IEnumerable<long> entityIds, EntityXmlExportSettings settings )
        {
            XmlWriterSettings xmlWriterSettings = GetXmlWriterSettings( );

            using ( StringWriter stringWriter = new StringWriter( ) )
            using ( XmlWriter xmlWriter = XmlWriter.Create( stringWriter, xmlWriterSettings ) )
            {
                GenerateXml( entityIds, xmlWriter, settings );

                xmlWriter.Flush( );
                stringWriter.Flush( );
                return stringWriter.ToString( );
            }
        }

        /// <summary>
        /// Gets the default Xml writer settings to use.
        /// </summary>
        /// <returns></returns>
        internal static XmlWriterSettings GetXmlWriterSettings( )
        {
            return new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = true,
                NewLineHandling = NewLineHandling.None
            };
        }

        /// <summary>
        /// Create a data target object.
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IDataTarget CreateDataTarget( XmlWriter xmlWriter, EntityXmlExportSettings settings )
        {
            return new XmlPackageTarget
            {
                XmlWriter = xmlWriter,
                NameResolver = new UpgradeMapNameResolver( ),
                EntityXmlExportSettings = settings,
                Format = Format.XmlVer2
            };
        }

        /// <summary>
        ///     Verify that the current user has permission to perform an import.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="context"></param>
        private void CheckExportSecurity( IDataSource source, IProcessingContext context )
        {
            SecurityProcessor.CheckUserInRole( new EntityRef( "core:importExportRole" ) );

            // Note: read check is done in the TenantGraphSource, because it already has the local IDs
        }
    }
}
