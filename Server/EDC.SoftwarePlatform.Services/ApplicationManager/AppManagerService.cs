// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Configuration;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Services.ApplicationManager
{
	/// <summary>
	///     Service for retrieving and working with the available applications.
	/// </summary>
	public class AppManagerService
	{
		#region Public Methods

		/// <summary>
		///     Deploys the application.
		/// </summary>
		/// <param name="applicationId">The application id.</param>
		public void DeployApplication( Guid applicationId )
		{
            if ( !CheckIfPossible( applicationId, CanDeploy ) )
                return;

			var tenantName = RequestContext.GetContext( ).Tenant.Name;

			using ( new TenantAdministratorContext( 0 ) )
			{
				AppManager.DeployApp( tenantName, applicationId.ToString( "B" ) );
			}

            TenantHelper.Invalidate(new EntityRef(RequestContext.TenantId));
		}

		/// <summary>
		///     Exports the application to an sqlite database file and loads it into the document library.
		/// </summary>
		/// <param name="applicationVersionId">The application version id.</param>
		/// <returns>A token used to access the export for download.</returns>
		public string ExportApplication( Guid applicationVersionId )
		{
            string token;

			var appFile = $"{DateTime.Now:yyyyMMddhhmmssfff}.xml";
			var db = Path.Combine( Path.GetTempPath( ), appFile );

			AppManager.ExportAppPackage( applicationVersionId, db, Format.Undefined );

			try
			{
				using ( var source = new FileStream( db, FileMode.Open, FileAccess.Read, FileShare.Read ) )
				{
					token = FileRepositoryHelper.AddTemporaryFile( source );
				}
			}
			finally
			{
				File.Delete( db );
			}

			return token;
		}

		/// <summary>
		///     Gets the available applications.
		/// </summary>
        /// <param name="applicationId">Optionally filter results to this ID only.</param>
        /// <returns>A list of the applications available.</returns>
		public IList<AvailableApplication> GetAvailableApplications( Guid? applicationId = null )
		{
			var applications = new List<AvailableApplication>( );

			using ( var ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( ) )
				{
                    const string sql = @"-- GetAvailableApplications
						DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
                        DECLARE @app BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
                        DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
                        DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
                        DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
                        DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
                        DECLARE @applicationId BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT )
                        DECLARE @canInstallApplication BIGINT = dbo.fnAliasNsId( 'canInstallApplication', 'core', DEFAULT )
                        DECLARE @canPublishApplication BIGINT = dbo.fnAliasNsId( 'canPublishApplication', 'core', DEFAULT )

                        SELECT
	                        Application = x.Application,
                            ApplicationEntityId = x.ApplicationId,
                            PackageId = pid.Data,
                            PackageEntityId = p.PackageId,
                            Version = p.Version,
                            ApplicationId = aid.Data,
                            Publisher = p1.Data,
                            PublisherUrl = u.Data,
                            ReleaseDate = c.Data,
	                        ISNULL(cp.CanPublish, 0) CanPublish,
	                        ISNULL(ci.CanInstall, 0) CanInstall
                        FROM
	                        (
	                        SELECT
		                        Application = n.Data, ApplicationId = n.EntityId
	                        FROM
		                        Relationship r
	                        JOIN
		                        Data_NVarChar n ON r.FromId = n.EntityId AND n.FieldId = @name AND n.TenantId = 0
	                        WHERE
		                        r.TypeId = @isOfType
								AND r.TenantId = 0
		                        AND r.ToId = @app
	                        ) x
                        JOIN
                        (
	                        SELECT
		                        dt.PackageId, dt.ApplicationId, dt.Version, dt.RowNumber
	                        FROM
	                        (
		                        SELECT
			                        ROW_NUMBER( ) OVER
			                        (
				                        PARTITION BY
					                        r.ToId
				                        ORDER BY
					                        CAST( '/' + REPLACE( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC
			                        ) AS 'RowNumber', PackageId = r.FromId, ApplicationId = r.ToId, Version = v.Data
			                        FROM
				                        Relationship r
			                        JOIN
				                        Data_NVarChar v ON r.FromId = v.EntityId
				                        AND r.TypeId = @packageForApplication
				                        AND v.FieldId = @appVersionString AND
										r.TenantId = v.TenantId
									WHERE
										r.TenantId = 0
	                        ) dt
                        ) p ON x.ApplicationId = p.ApplicationId
                        JOIN
	                        Data_Guid pid ON p.PackageId = pid.EntityId
		                        AND pid.TenantId = 0
		                        AND pid.FieldId = @appVerId
                        JOIN
	                        Data_Guid aid ON x.ApplicationId = aid.EntityId
		                        AND aid.TenantId = 0
		                        AND aid.FieldId = @applicationId
                        LEFT JOIN
                        (
	                        SELECT
		                        p.EntityId, p.Data
	                        FROM
		                        Data_NVarChar p
	                        JOIN
		                        Data_Alias ap ON p.FieldId = ap.EntityId
									AND ap.TenantId = 0
									AND ap.Data = 'publisher'
									AND ap.Namespace = 'core'
							WHERE
								p.TenantId = 0
	                        ) p1 ON x.ApplicationId = p1.EntityId
                        LEFT JOIN
                        (
	                        SELECT
		                        u.EntityId, u.Data
	                        FROM
		                        Data_NVarChar u
	                        JOIN
		                        Data_Alias au ON u.FieldId = au.EntityId
									AND au.TenantId = 0
									AND au.Data = 'publisherUrl'
									AND au.Namespace = 'core'
							WHERE
								u.TenantId = 0
                        ) u ON x.ApplicationId = u.EntityId
                        LEFT JOIN
                        (
	                        SELECT
		                        c.EntityId, c.Data
	                        FROM
		                        Data_DateTime c
	                        JOIN
		                        Data_Alias ac ON c.FieldId = ac.EntityId
									AND ac.TenantId = 0
									AND ac.Data = 'releaseDate'
									AND ac.Namespace = 'core'
							WHERE
								c.TenantId = 0
                        ) c ON x.ApplicationId = c.EntityId
                        LEFT JOIN
                        (
	                        SELECT
		                        ToId AppId,
		                        1 CanPublish
	                        FROM
		                        Relationship r
	                        WHERE
		                        TypeId = @canPublishApplication
	                        AND
		                        TenantId = 0
	                        AND
		                        FromId = @tenantId
                        ) cp ON x.ApplicationId = cp.AppId
                        LEFT JOIN
                        (
	                        SELECT
		                        ToId AppId,
		                        1 CanInstall
	                        FROM
		                        Relationship r
	                        WHERE
		                        TypeId = @canInstallApplication
	                        AND
		                        TenantId = 0
	                        AND
		                        FromId = @tenantId
                        ) ci ON x.ApplicationId = ci.AppId

                        WHERE
                            p.RowNumber = 1
                            AND
                            (
                                @applicationGuid = convert(uniqueidentifier, '00000000-0000-0000-0000-000000000000')
                                OR
                                @applicationGuid = aid.Data
							)";

					command.CommandText = sql;

                    ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
                    ctx.AddParameter( command, "@applicationGuid", DbType.Guid, applicationId ?? Guid.Empty );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var application = new AvailableApplication
							{
								Name = reader.GetString( 0 ),
								ApplicationEntityId = reader.GetInt64( 1 ),
								ApplicationVersionId = reader.GetGuid( 2 ),
								PackageEntityId = reader.GetInt64( 3 ),
								PackageVersion = reader.GetString( 4 ),
								ApplicationId = reader.GetGuid( 5 ),
								Publisher = reader.GetString( 6, null ),
								PublisherUrl = reader.GetString( 7, null ),
								ReleaseDate = reader.GetDateTime( 8, DateTime.MinValue ),
                                HasPublishPermission = reader.GetInt32( 9 ) == 1,
                                HasInstallPermission = reader.GetInt32( 10 ) == 1
							};

							applications.Add( application );
						}
					}
				}
			}

			return applications.GroupBy( a => a.ApplicationId ).Select( a => a.OrderByDescending( x => x.PackageEntityId ).First( ) ).ToList( );
		}

		/// <summary>
		///     Gets the installed applications.
		/// </summary>
        /// <param name="applicationId">Optionally filter results to this ID only.</param>
		/// <returns></returns>
        public IList<InstalledApplication> GetInstalledApplications( Guid? applicationId = null )
		{
			var applications = new List<InstalledApplication>( );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
                    const string sql = @"-- GetInstalledApplications
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @solutionVersionString BIGINT = dbo.fnAliasNsId( 'solutionVersionString', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @solution BIGINT = dbo.fnAliasNsId( 'solution', 'core', @tenantId )

SELECT
	Solution = sn.Data,
    SolutionEntityId = s.FromId,
    SolutionVersion = sv.Data,
    PackageId = pid.Data,
    PackageEntityId = p.EntityId,
    Version = pv.Data,
    ApplicationEntityId = pa.ToId,
    ApplicationId = aid.UpgradeId,
    Publisher = p1.Data,
    PublisherUrl = u.Data,
    ReleaseDate = c.Data
FROM
	Relationship s   -- isOfType solution
JOIN
	Data_NVarChar sv ON s.FromId = sv.EntityId
	AND sv.FieldId = @solutionVersionString
	AND sv.TenantId = @tenantId
CROSS APPLY
	dbo.tblFnFieldNVarCharA( s.FromId, s.TenantId, 'name', 'core' ) sn
CROSS APPLY
	dbo.tblFnFieldGuidA( s.FromId, s.TenantId, 'packageId', 'core' ) pid
LEFT JOIN
	Data_Guid p ON pid.Data = p.Data
	AND p.FieldId = @appVerId
	AND p.TenantId = 0
LEFT JOIN
	Relationship pa ON p.EntityId = pa.FromId
	AND pa.TypeId = @packageForApplication
	AND pa.TenantId = 0
LEFT JOIN
	Data_NVarChar pv ON p.EntityId = pv.EntityId
	AND pv.FieldId = @appVersionString
	AND pv.TenantId = 0
OUTER APPLY
	dbo.tblFnFieldNVarCharA( s.FromId, s.TenantId, 'solutionPublisher', 'core' ) p1
OUTER APPLY
	dbo.tblFnFieldNVarCharA( s.FromId, s.TenantId, 'solutionPublisherUrl', 'core' ) u
OUTER APPLY
	dbo.tblFnFieldDateTimeA( s.FromId, s.TenantId, 'solutionReleaseDate', 'core' ) c
JOIN
	Entity aid ON s.FromId = aid.Id AND s.TenantId = aid.TenantId
WHERE
	s.TenantId = @tenantId
	AND s.TypeId = @isOfType
	AND s.ToId = @solution
	AND
	(
		p.EntityId = pa.FromId
		OR
		p.EntityId IS NULL
	)
    AND
    (
        @applicationGuid = convert(uniqueidentifier, '00000000-0000-0000-0000-000000000000')
        OR
        @applicationGuid = aid.UpgradeId
    )
ORDER BY
	sn.Data, pv.Data";

					command.CommandText = sql;

                    ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
                    ctx.AddParameter( command, "@applicationGuid", DbType.Guid, applicationId ?? Guid.Empty );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var application = new InstalledApplication
							{
								Name = reader.GetString( 0 ),
								SolutionEntityId = reader.GetInt64( 1 ),
								SolutionVersion = reader.GetString( 2 ),
								ApplicationVersionId = reader.GetGuid( 3 ),
								PackageEntityId = reader.GetInt64( 4, -1 ),
								PackageVersion = reader.GetString( 5, null ),
								ApplicationEntityId = reader.GetInt64( 6, -1 ),
								ApplicationId = reader.GetGuid( 7, Guid.Empty ),
								Publisher = reader.GetString( 8, null ),
								PublisherUrl = reader.GetString( 9, null ),
								ReleaseDate = reader.GetDateTime( 10, DateTime.MinValue )
							};

							applications.Add( application );
						}
					}
				}
			}

			return applications.GroupBy( a => a.ApplicationId ).Select( a => a.OrderByDescending( x => x.PackageEntityId ).First( ) ).ToList( );
		}

		/// <summary>
		///     Publishes the application.
		/// </summary>
		/// <param name="applicationId">The application id.</param>
		public void PublishApplication( Guid applicationId )
		{
            if ( !CheckIfPossible( applicationId, CanPublish ) )
                return;
            
			string tenantName = RequestContext.GetContext( ).Tenant.Name;

			using ( new TenantAdministratorContext( 0 ) )
			{
				AppManager.PublishApp( tenantName, applicationId.ToString( "B" ) );
			}
		}

		/// <summary>
		///     Repairs the application, undoing any changes made since install.
		/// </summary>
		/// <param name="applicationVersionId">The application version id.</param>
		public void RepairApplication( Guid applicationVersionId )
		{
			long tenantId = RequestContext.TenantId;
			using ( new TenantAdministratorContext( 0 ) )
			{
				AppManager.RepairApp( tenantId, applicationVersionId );
			}

            TenantHelper.Invalidate(new EntityRef(tenantId));
		}

		/// <summary>
		///     Stages the application.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		public StatisticsReport StageApplication( Guid applicationId )
		{
			string tenantName = RequestContext.GetContext( ).Tenant.Name;

			using ( new TenantAdministratorContext( 0 ) )
			{
				return AppManager.StageApp( tenantName, applicationId.ToString( "B" ) );
			}
		}

		/// <summary>
		///     Removes the application from the database and deletes all related entities.
		/// </summary>
		/// <param name="applicationVersionId">The application version id.</param>
		public void UninstallApplication( Guid applicationVersionId )
		{
			string tenantName = RequestContext.GetContext( ).Tenant.Name;

			using ( new TenantAdministratorContext( 0 ) )
			{
				AppManager.RemoveApp( tenantName, applicationVersionId.ToString( "B" ) );
			}

            TenantHelper.Invalidate(new EntityRef(RequestContext.TenantId));
		}

		/// <summary>
		///     Upgrades the application.
		/// </summary>
		/// <param name="applicationVersionId">The application version id.</param>
		public void UpgradeApplication( Guid applicationVersionId )
		{
			long tenantId = RequestContext.TenantId;
			using ( new TenantAdministratorContext( 0 ) )
			{
				AppManager.UpgradeApp( tenantId, applicationVersionId );
			}

            TenantHelper.Invalidate(new EntityRef(tenantId));
		}

        /// <summary>
        /// Returns true if all checks are passed to permit an app to be published.
        /// </summary>
        /// <param name="installedApp">Entry from tenant, or null if not in tenant</param>
        /// <param name="availableApp">Entry from app-library, or null if not in library</param>
        /// <param name="securityModel">Security method to apply.</param>
        public bool CanPublish( InstalledApplication installedApp, AvailableApplication availableApp, AppSecurityModel securityModel )
        {
            bool isAvailable = !string.IsNullOrWhiteSpace( availableApp?.PackageVersion );
            bool isInstalled = !string.IsNullOrWhiteSpace( installedApp?.SolutionVersion );

            if ( !isInstalled )
                return false;
            if ( !isAvailable )
                return true;        // todo: add special permission for first-time publish of new apps

            bool somethingToPublish = AppManager.CanPublish( installedApp.SolutionVersion, availableApp.PackageVersion );
            if ( !somethingToPublish )
                return false;

            switch ( securityModel )
            {
                case AppSecurityModel.Restricted:
                    return false;
                case AppSecurityModel.Full:
                    return true;
                case AppSecurityModel.PerTenant:
                    return availableApp.HasPublishPermission;
                default:
                    throw new InvalidOperationException( "Unknown application security setting" );
            }
        }

        /// <summary>
        /// Returns true if all checks are passed to permit an app to be deployed.
        /// </summary>
        /// <param name="installedApp">Entry from tenant, or null if not in tenant</param>
        /// <param name="availableApp">Entry from app-library, or null if not in library</param>
        /// <param name="securityModel">Security method to apply.</param>
        public bool CanDeploy( InstalledApplication installedApp, AvailableApplication availableApp, AppSecurityModel securityModel )
        {
            bool isAvailable = !string.IsNullOrWhiteSpace( availableApp?.PackageVersion );
            bool isInstalled = !string.IsNullOrWhiteSpace( installedApp?.SolutionVersion );

            if ( isInstalled )
                return false;
            if ( !isAvailable )
                return false;

            switch ( securityModel )
            {
                case AppSecurityModel.Restricted:
                    return false;
                case AppSecurityModel.Full:
                    return true;
                case AppSecurityModel.PerTenant:
                    return availableApp.HasInstallPermission;
                default:
                    throw new InvalidOperationException( "Unknown application security setting" );
            }
        }

        /// <summary>
        /// Returns true if all checks are passed to permit an app to be exported.
        /// </summary>
        /// <param name="installedApp">Entry from tenant, or null if not in tenant</param>
        /// <param name="availableApp">Entry from app-library, or null if not in library</param>
        /// <param name="securityModel">Security method to apply.</param>
        public bool CanExport( InstalledApplication installedApp, AvailableApplication availableApp, AppSecurityModel securityModel )
        {
            bool isAvailable = availableApp != null;

            if ( !isAvailable )
                return false;

            switch ( securityModel )
            {
                case AppSecurityModel.Restricted:
                    return false;
                case AppSecurityModel.Full:
                    return true;
                case AppSecurityModel.PerTenant:
                    return availableApp.HasPublishPermission; // can probably improve this
                default:
                    throw new InvalidOperationException( "Unknown application security setting" );
            }
        }

        /// <summary>
        /// Returns true if all checks are passed to permit an app to be upgraded.
        /// </summary>
        /// <param name="installedApp">Entry from tenant, or null if not in tenant</param>
        /// <param name="availableApp">Entry from app-library, or null if not in library</param>
        /// <param name="securityModel">Security method to apply.</param>
        public bool CanUpgrade( InstalledApplication installedApp, AvailableApplication availableApp, AppSecurityModel securityModel )
        {
            bool isAvailable = !string.IsNullOrWhiteSpace( availableApp?.PackageVersion );
            bool isInstalled = !string.IsNullOrWhiteSpace( installedApp?.SolutionVersion );

            if ( !(isInstalled && isAvailable) )
                return false;

            bool somethingToUpgrade = AppManager.CanUpgrade( installedApp.SolutionVersion, availableApp.PackageVersion );
            if ( !somethingToUpgrade )
                return false;

            switch ( securityModel )
            {
                case AppSecurityModel.Restricted:
                case AppSecurityModel.Full:
                    return true;
                case AppSecurityModel.PerTenant:
                    return availableApp.HasInstallPermission;
                default:
                    throw new InvalidOperationException( "Unknown application security setting" );
            }
        }

        /// <summary>
        /// Returns true if all checks are passed to permit an app to be repaired.
        /// </summary>
        /// <param name="installedApp">Entry from tenant, or null if not in tenant</param>
        /// <param name="availableApp">Entry from app-library, or null if not in library</param>
        /// <param name="securityModel">Security method to apply.</param>
        public bool CanRepair( InstalledApplication installedApp, AvailableApplication availableApp, AppSecurityModel securityModel )
        {
            bool isAvailable = !string.IsNullOrWhiteSpace( availableApp?.PackageVersion );
            bool isInstalled = !string.IsNullOrWhiteSpace( installedApp?.SolutionVersion );

            if ( !( isInstalled && isAvailable ) )
                return false;

            bool possibleToRepair = AppManager.CanRepair( installedApp.SolutionVersion, availableApp.PackageVersion );
            if ( !possibleToRepair )
                return false;

            switch ( securityModel )
            {
                case AppSecurityModel.Restricted:
                case AppSecurityModel.Full:
                    return true;
                case AppSecurityModel.PerTenant:
                    return availableApp.HasInstallPermission;
                default:
                    throw new InvalidOperationException( "Unknown application security setting" );
            }
        }

        /// <summary>
        /// Returns true if all checks are passed to permit an app to be uninstalled.
        /// </summary>
        /// <param name="installedApp">Entry from tenant, or null if not in tenant</param>
        /// <param name="availableApp">Entry from app-library, or null if not in library</param>
        /// <param name="securityModel">Security method to apply.</param>
        public bool CanUninstall( InstalledApplication installedApp, AvailableApplication availableApp, AppSecurityModel securityModel )
        {
            bool isInstalled = !string.IsNullOrWhiteSpace( installedApp?.SolutionVersion );

            if ( !isInstalled )
                return false;

            switch ( securityModel )
            {
                case AppSecurityModel.Restricted:
                    return false;
                case AppSecurityModel.Full:
                    return true;
                case AppSecurityModel.PerTenant:
                    return availableApp?.HasInstallPermission == true;
                default:
                    throw new InvalidOperationException( "Unknown application security setting" );
            }
        }

        /// <summary>
        /// Returns true if all checks are passed to permit a tenant to see an app.
        /// </summary>
        /// <param name="installedApp">Entry from tenant, or null if not in tenant</param>
        /// <param name="availableApp">Entry from app-library, or null if not in library</param>
        /// <param name="securityModel">Security method to apply.</param>
        public bool CanSee( InstalledApplication installedApp, AvailableApplication availableApp, AppSecurityModel securityModel )
        {
            Guid systemSolution = new Guid( "3e67c1c4-aa65-4a9f-95d2-908a9f3614d1" );
            if ( availableApp?.ApplicationId == systemSolution )
                return false;

            bool isAvailable = !string.IsNullOrWhiteSpace( availableApp?.PackageVersion );
            bool isInstalled = !string.IsNullOrWhiteSpace( installedApp?.SolutionVersion );

            if ( !isInstalled && !isAvailable )
                return false;

            switch ( securityModel )
            {
                case AppSecurityModel.Restricted:
                    return isInstalled;
                case AppSecurityModel.Full:
                    return true;
                case AppSecurityModel.PerTenant:
                    return isInstalled || availableApp.HasInstallPermission;
                default:
                    throw new InvalidOperationException( "Unknown application security setting" );
            }
        }

        /// <summary>
        /// Helper function to load application records and validate if OK to proceed with an operation.
        /// </summary>
        /// <param name="applicationId">The application Guid. (Not the package guid)</param>
        /// <param name="validationFunction">Validation callback.</param>
        /// <returns>True if we can proceed.</returns>
        private bool CheckIfPossible( Guid applicationId, Func<InstalledApplication, AvailableApplication, AppSecurityModel, bool> validationFunction )
        {
            if ( validationFunction == null )
                throw new ArgumentNullException( nameof( validationFunction ) );

            // Get application security setting
            var security = ApplicationSecuritySettings.Current == null
                ? AppSecurityModel.Restricted
                : ApplicationSecuritySettings.Current.AppSecurityModel;

            // Find application records
	        InstalledApplication installedApp = GetInstalledApplications( applicationId ).SingleOrDefault( );
	        AvailableApplication availableApp = GetAvailableApplications( applicationId ).SingleOrDefault( );

            bool result = validationFunction( installedApp, availableApp, security );
            return result;
        }

		#endregion
	}
}