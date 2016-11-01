// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Sources;
using EDC.SoftwarePlatform.Migration.Targets;
using ReadiNow.ImportExport;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml
{
    /// <summary>
    /// Imports entity XML files.
    /// </summary>
    class EntityXmlImporter : IEntityXmlImporter
    {
        private ISecurityProcessor SecurityProcessor { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userRoleRepository"></param>
        public EntityXmlImporter( ISecurityProcessor securityProcessor )
        {
            if ( securityProcessor == null )
                throw new ArgumentNullException( nameof( securityProcessor ) );
            SecurityProcessor = securityProcessor;
        }


        /// <summary>
        /// Interface for providing XML import.
        /// </summary>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="xmlWriter">Xml Writer to write the exported entity to.</param>
        /// <param name="settings">Export settings.</param>
        public EntityXmlImportResult ImportXml( XmlReader xmlReader, EntityXmlImportSettings settings )
        {
            if ( xmlReader == null )
                throw new ArgumentNullException( nameof( xmlReader ) );
            if ( settings == null )
                settings = EntityXmlImportSettings.Default;

            var context = new ProcessingContext( );

            context.Report.StartTime = DateTime.Now;

            long tenantId = RequestContext.TenantId;
            IEnumerable<Guid> rootGuids;

            /////
            // Create source to load app data from tenant
            /////
            using ( IDataSource source = CreateDataSource( xmlReader, settings ) )
            {
                CheckImportSecurity( source, context );

                rootGuids = ImportEntity( tenantId, source, context );
            }

            context.Report.EndTime = DateTime.Now;


            // Prepare result
            List<long> rootIds = rootGuids.Select( guid => Entity.GetIdFromUpgradeId( guid ) ).ToList( );

            return new EntityXmlImportResult( rootIds );
        }

        /// <summary>
        /// Interface for providing XML import.
        /// </summary>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="settings">Export settings.</param>
        public EntityXmlImportResult ImportXml( string xml, EntityXmlImportSettings settings )
        {
            using ( StringReader stringReader = new StringReader( xml ) )
            using ( XmlReader xmlReader = XmlReader.Create( stringReader ) )
            {
                return ImportXml( xmlReader, settings );
            }
        }

        /// <summary>
        ///     Imports the entity 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="importSource"></param>
        /// <param name="context"></param>
        internal IEnumerable<Guid> ImportEntity( long tenantId, IDataSource importSource, IProcessingContext context )
        {
            Guid rootGuid = GetRootGuidFromMetadata( importSource, context );
            
            using ( IDataSource baseline = GetBaselineSourceForImport( tenantId, rootGuid ) )
            using ( TenantMergeTarget target = new TenantMergeTarget
            {
                TenantId = tenantId
            } )
            {
                /////
                // Copy the data
                /////
                using ( var processor = new MergeProcessor( context )
                {
                    OldVersion = baseline,
                    NewVersion = importSource,
                    Target = target
                } )
                {
                    processor.MergeData( );

                    target.Commit( );
                }
            }

            CacheManager.ClearCaches( tenantId );

            return new[ ] { rootGuid };
        }

        /// <summary>
        /// Gets the root GUID from the metadata.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="importSource"></param>
        /// <returns></returns>
        private Guid GetRootGuidFromMetadata( IDataSource importSource, IProcessingContext context )
        {
            Guid rootGuid = importSource.GetMetadata( context ).RootEntityId;
            if ( rootGuid == Guid.Empty )
                throw new ArgumentException( "Not a valid file to import. Root entry missing." );
            return rootGuid;
        }

        /// <summary>
        ///     Create a baseline source for importing data into a tenant.
        /// </summary>
        /// <remarks>
        ///     Check the tenant to see if the root entity is present.
        ///     If it is, then perform an export to use the existing contents as a baseline, so it gets updated.
        ///     If it's not, then use an empty source so that everything gets added.
        /// </remarks>
        /// <param name="tenantId">Target tenant.</param>
        /// <param name="rootGuid">UpgradeID of entity to be targeted, if present.</param>
        /// <returns></returns>
        private IDataSource GetBaselineSourceForImport( long tenantId, Guid rootGuid )
        {
            IDataSource baseline;
            using ( new TenantAdministratorContext( tenantId ) )
            {
                long rootId = Entity.GetIdFromUpgradeId( rootGuid );
                if ( rootId == -1 )
                    baseline = new EmptySource( );
                else
                    baseline = new TenantGraphSource
                    {
                        TenantId = tenantId,
                        RootEntities = new[]
                        {
                            rootId
                        }
                    };
            }
            return baseline;
        }

        /// <summary>
        /// Create a data source object.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="settings">The reader settings.</param>
        /// <returns>A data source.</returns>
        private IDataSource CreateDataSource( XmlReader xmlReader, EntityXmlImportSettings settings )
        {
            return new XmlPackageSource
            {
                XmlReader = xmlReader,
                Version = Format.XmlVer2
            };
        }

        /// <summary>
        ///     Verify that the current user has permission to perform an import.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="context"></param>
        private void CheckImportSecurity( IDataSource source, IProcessingContext context  )
        {
            SecurityProcessor.CheckUserInRole( new EntityRef( "core:importExportRole" ) );

            SecurityProcessor.CheckTypeCreatePermissions( source, context );

            SecurityProcessor.CheckEntityPermissions( source, new[ ] { Permissions.Modify }, context );
        }
    }
}
