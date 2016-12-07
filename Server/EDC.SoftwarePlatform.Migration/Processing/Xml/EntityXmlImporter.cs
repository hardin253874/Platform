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
using System.Text;
using EDC.ReadiNow.Diagnostics;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml
{
    /// <summary>
    /// Imports entity XML files.
    /// </summary>
    class EntityXmlImporter : IEntityXmlImporter
    {
        private ISecurityProcessor SecurityProcessor { get; }
        private IUpgradeIdProvider UpgradeIdProvider { get; }

        private EntityXmlImportSettings Settings { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userRoleRepository"></param>
        public EntityXmlImporter( ISecurityProcessor securityProcessor, IUpgradeIdProvider upgradeIdProvider )
        {
            if ( securityProcessor == null )
                throw new ArgumentNullException( nameof( securityProcessor ) );
            if ( upgradeIdProvider == null )
                throw new ArgumentNullException( nameof( upgradeIdProvider ) );
            SecurityProcessor = securityProcessor;
            UpgradeIdProvider = upgradeIdProvider;
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

            Settings = settings;
            var context = new ProcessingContext( );

            context.Report.StartTime = DateTime.Now;

            long tenantId = RequestContext.TenantId;
            IEnumerable<Guid> rootGuids;

            /////
            // Create source to load app data from tenant
            /////
            try
            {
                using ( IDataSource source = CreateDataSource( xmlReader, settings ) )
                {
                    CheckImportSecurity( source, context );

                    rootGuids = ImportEntity( tenantId, source, context );
                }
            }
            catch ( ImportDependencyException )
            {
                return new EntityXmlImportResult( new long[] { } )
                {
                    ErrorMessage = "The import failed because some dependencies could not be found."
                };
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
        /// <param name="settings"></param>
        internal IEnumerable<Guid> ImportEntity( long tenantId, IDataSource importSource, IProcessingContext context )
        {
            IList<Guid> rootGuids = GetRootGuidsFromMetadata( importSource, context );
            
            using ( IDataSource baseline = GetBaselineSourceForImport( tenantId, rootGuids ) )
            using ( TenantMergeTarget target = new TenantMergeTarget
            {
                TenantId = tenantId,
                IgnoreExternalReferences = true
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

                    //CheckForMissingDependencies( context );

                    target.Commit( );
                }
            }

            CacheManager.ClearCaches( tenantId );

            return rootGuids;
        }

        /// <summary>
        /// Check the context for missing dependencies, and if so report them.
        /// Exception will roll back the transaction.
        /// </summary>
        /// <param name="context"></param>
        private void CheckForMissingDependencies( IProcessingContext context )
        {
            if ( Settings.IgnoreMissingDependencies )
                return;
            if ( context.Report.MissingDependencies.Count == 0 )
                return;

            StringBuilder sb = new StringBuilder( );
            sb.AppendLine( "The import failed because there are missing dependencies." );
            foreach ( object obj in context.Report.MissingDependencies )
            {
                IMissingDependency dep = obj as IMissingDependency;
                if ( dep == null )
                    continue;
                sb.AppendLine( dep.GetLogMessage( ) );
            }
            EventLog.Application.WriteWarning( sb.ToString( ) );
            throw new ImportDependencyException( );
        }

        /// <summary>
        /// Gets the root GUID from the metadata.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="importSource"></param>
        /// <returns></returns>
        private IList<Guid> GetRootGuidsFromMetadata( IDataSource importSource, IProcessingContext context )
        {
            var rootEntities = importSource.GetMetadata( context ).RootEntities;
            if ( rootEntities == null || rootEntities.Count == 0 )
                throw new ArgumentException( "Not a valid file to import. Root entry missing." );
            return rootEntities;
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
        /// <param name="rootGuids">UpgradeID of entity to be targeted, if present.</param>
        /// <returns></returns>
        private IDataSource GetBaselineSourceForImport( long tenantId, IEnumerable<Guid> rootGuids )
        {
            IDataSource baseline;
            using ( new TenantAdministratorContext( tenantId ) )
            {
                IDictionary<Guid, long> ids = UpgradeIdProvider.GetIdsFromUpgradeIds( rootGuids );

                if ( ids.Count == 0 )
                {
                    baseline = new EmptySource( );
                }
                else
                {
                    baseline = new TenantGraphSource
                    {
                        TenantId = tenantId,
                        RootEntities = ids.Values.ToList( )
                    };
                }
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

        /// <summary>
        ///     Exception caused by missing dependencies.
        /// </summary>
        private class ImportDependencyException : Exception
        {
            
        }
    }
}
