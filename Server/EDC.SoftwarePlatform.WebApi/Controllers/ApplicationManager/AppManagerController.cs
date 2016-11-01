// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.PartialClasses;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Services.ApplicationManager;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ApplicationManager
{
    /// <summary>
    ///     Controller for working with platform applications.
    /// </summary>
    [RoutePrefix( "data/v1/appManager" )]
    public class AppManagerController : ApiController
    {
        #region Constructor

        /// <summary>
        ///     Basic constructor (server-side).
        /// </summary>
        public AppManagerController( )
        {
            AppManagerServiceImpl = new AppManagerService( );
        }

        #endregion

        #region Internal Properties

        /// <summary>
        ///     Holds a handle to an instance of the app manager service.
        /// </summary>
        internal AppManagerService AppManagerServiceImpl
        {
            get;
            set;
        }

        #endregion

        #region Service Methods

        /// <summary>
        ///     Combines information about the installed and available applications to return data for
        ///     the Application Management report.
        /// </summary>
        /// <returns>A list of application data.</returns>
        [Route( "" )]
        [HttpGet]
        public HttpResponseMessage<IList<AppManagerData>> GetApplicationData( )
        {
            try
            {
                var availableApps = AppManagerServiceImpl.GetAvailableApplications( );
                var installedApps = AppManagerServiceImpl.GetInstalledApplications( );

                var response = GetAppManagerData( availableApps, installedApps );

                return new HttpResponseMessage<IList<AppManagerData>>( response.OrderBy( a => a.Name ).ToList( ) );
            }
            catch ( Exception e )
            {
                throw new Exception( "Failed to get applications list.", e );
            }
        }

		/// <summary>
		/// Deploys the application.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		[Route( "deploy/{id}" )]
        [HttpPost]
        public HttpResponseMessage DeployApplication( Guid id )
        {
            try
            {
                AppManagerServiceImpl.DeployApplication( id );
                return new HttpResponseMessage( HttpStatusCode.OK );
            }
            catch ( Exception e )
            {
                throw new Exception( $"Failed to deploy application {id}", e );
            }
        }

		/// <summary>
		/// Exports the application.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		[Route( "{token}/{name}/export" )]
        [HttpGet]
        public HttpResponseMessage ExportApplication( string token, string name )
        {
            Stream stream = FileRepositoryHelper.GetTemporaryFileDataStream( token );
            var response = new HttpResponseMessage( HttpStatusCode.OK )
            {
                Content = new StreamContent( stream )
            };
            // Note: We are not setting the content length or the mime type
            // because the CompressionHandler will compress the stream.
            // Specifying a mimetype specified here will cause the browser (Chrome at least) to log a
            // message.
            // Specifying the length here will cause the browser to hang as the actual data it
            // receives (as it is compressed) will be less than the specified content length.
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue( "attachment" )
            {
                FileName = name
            };
            return response;
        }

		/// <summary>
		/// Gets the dependents.
		/// </summary>
		/// <param name="vid">The vid.</param>
		/// <returns></returns>
		[Route( "getDependents/{vid}" )]
		[HttpGet]
		public HttpResponseMessage GetDependents( long vid )
		{
			IEnumerable<Solution> solutions = SolutionHelper.GetApplicationDependents( vid ).Select( ad => ad.DependentApplication );

			List<DependentAppData> data = new List<DependentAppData>( );

			foreach ( Solution solution in solutions )
			{
				data.Add( new DependentAppData( solution.Id, solution.Name, solution.SolutionVersionString ) );
			}

			var response = new HttpResponseMessage<IList<DependentAppData>>( data, HttpStatusCode.OK );

			return response;
		}

		/// <summary>
		/// Gets the dependencies.
		/// </summary>
		/// <param name="vid">The vid.</param>
		/// <returns></returns>
		[Route( "getDependencies/{vid}" )]
		[HttpGet]
		public HttpResponseMessage GetDependencies( long vid )
		{
			IEnumerable<Solution> solutions = SolutionHelper.GetApplicationDependencies( vid ).Select( ad => ad.DependencyApplication );

			List<DependentAppData> data = new List<DependentAppData>( );

			foreach ( Solution solution in solutions )
			{
				data.Add( new DependentAppData( solution.Id, solution.Name, solution.SolutionVersionString ) );
			}

			var response = new HttpResponseMessage<IList<DependentAppData>>( data, HttpStatusCode.OK );

			return response;
		}

		/// <summary>
		/// Gets the package dependencies.
		/// </summary>
		/// <param name="vid">The vid.</param>
		/// <returns></returns>
		[Route( "getPackageDependencies/{vid}" )]
		[HttpGet]
		public HttpResponseMessage GetPackageDependencies( long vid )
		{
			long tenantId = ReadiNow.IO.RequestContext.TenantId;

			using ( new GlobalAdministratorContext( ) )
			{
				List<DependentAppData> results = new List<DependentAppData>( );

				HashSet<Guid> discoveredPackageIds = new HashSet<Guid>( );

				GetPackageDependenciesRecursive( vid, tenantId, results, discoveredPackageIds );
				
				var response = new HttpResponseMessage<IList<DependentAppData>>( results, HttpStatusCode.OK );

				return response;
			}
		}

		/// <summary>
		/// Gets the package dependencies recursive.
		/// </summary>
		/// <param name="packageId">The package identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="results">The results.</param>
		/// <param name="discoveredPackageIds">The discovered package ids.</param>
		/// <exception cref="ApplicationDependencyException">
		/// </exception>
		private void GetPackageDependenciesRecursive( long packageId, long tenantId, List<DependentAppData> results, ISet<Guid> discoveredPackageIds )
		{
			IList<DependencyFailure> applicationDependencies = SolutionHelper.GetMissingPackageDependencies( packageId, tenantId );

			if ( applicationDependencies != null && applicationDependencies.Count > 0 )
			{
				foreach ( DependencyFailure dependency in applicationDependencies )
				{
					if ( dependency.Reason == DependencyFailureReason.NotInstalled )
					{
						throw new ApplicationDependencyException( $"The required dependency application '{dependency.DependencyName}' could not be found in the application library." );
					}

					if ( dependency.Reason == DependencyFailureReason.BelowMinVersion )
					{
						SolutionHelper.EnsureUpgradePath( tenantId, dependency );

						if ( dependency.Reason == DependencyFailureReason.NoUpgradePathAvailable )
						{
							throw new ApplicationDependencyException( $"The required version of dependency application '{dependency.DependencyName}' could not be found in the application library." );
						}

						if ( dependency.Reason == DependencyFailureReason.IncompatibleUpgradePath )
						{
							if ( !string.IsNullOrEmpty( dependency.DependentName ) )
							{
								throw new ApplicationDependencyException( $"Upgrading the selected application requires the application '{dependency.DependencyName}' to also be upgraded however the installed version of '{dependency.DependentName}' is incompatible with that version of '{dependency.DependencyName}'." );
							}

							throw new ApplicationDependencyException( $"Upgrading the selected application requires the application '{dependency.DependencyName}' to also be upgraded however this will cause compatibility issues with other installed applications." );
						}
					}

					long dependencyPackageId = SystemHelper.GetPackageIdByGuid( dependency.ApplicationId, dependency.MinVersion, dependency.MaxVersion );

					AppPackage dependencyPackage = ReadiNow.Model.Entity.Get<AppPackage>( dependencyPackageId );

					string currentVersion = null;

					if ( dependency.CurrentVersion != null )
					{
						currentVersion = dependency.CurrentVersion.ToString( 4 );
					}

					if ( !discoveredPackageIds.Contains( dependency.ApplicationId ) )
					{
						discoveredPackageIds.Add( dependency.ApplicationId );

						GetPackageDependenciesRecursive( dependencyPackageId, tenantId, results, discoveredPackageIds );

						results.Add( new DependentAppData( dependencyPackageId, dependency.DependencyName, dependencyPackage.AppVersionString, dependency.Reason == DependencyFailureReason.BelowMinVersion, currentVersion ) );
					}
				}
			}
		}

		/// <summary>
		/// Exports the application.
		/// </summary>
		/// <param name="vid">The vid.</param>
		/// <returns></returns>
		[Route( "export/{vid}" )]
        [HttpGet]
        public HttpResponseMessage<string> ExportApplication( Guid vid )
        {
            try
            {
                string token = AppManagerServiceImpl.ExportApplication( vid );
                return new HttpResponseMessage<string>( token, HttpStatusCode.OK );
            }
            catch ( Exception e )
            {
                EventLog.Application.WriteError( "Failed to export application {2}: {0} ({1})", e.Message, e.GetType( ).FullName, vid );
                return new HttpResponseMessage<string>( HttpStatusCode.BadRequest );
            }
        }

		/// <summary>
		/// Publishes the application.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		[Route( "publish/{id}" )]
        [HttpPost]
        public HttpResponseMessage PublishApplication( Guid id )
        {
            try
            {
                AppManagerServiceImpl.PublishApplication( id );
                return new HttpResponseMessage( HttpStatusCode.OK );
            }
            catch ( Exception e )
            {
                throw new Exception( $"Failed to publish application {id}", e );
            }
        }

		/// <summary>
		/// Repairs the application.
		/// </summary>
		/// <param name="vid">The vid.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		[Route( "repair/{vid}" )]
        [HttpPost]
        public HttpResponseMessage RepairApplication( Guid vid )
        {
            try
            {
                AppManagerServiceImpl.RepairApplication( vid );
                return new HttpResponseMessage( HttpStatusCode.OK );
            }
            catch ( Exception e )
            {
                throw new Exception( $"Failed to repair application {vid}", e );
            }
        }

		/// <summary>
		/// Stages the application.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		[Route( "stage/{id}" )]
        [HttpPost]
        public HttpResponseMessage StageApplication( Guid id )
        {
            try
            {
                StatisticsReport report = AppManagerServiceImpl.StageApplication( id );

                var data = new AppStagingData( report );

                return new HttpResponseMessage<AppStagingData>( data, HttpStatusCode.OK );
            }
            catch ( Exception e )
            {
                throw new Exception( $"Failed to stage application {id}", e );
            }
        }

		/// <summary>
		/// Uninstall the application.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		[Route( "uninstall/{id}" )]
        [HttpPost]
        public HttpResponseMessage UninstallApplication( Guid id )
        {
            try
            {
                AppManagerServiceImpl.UninstallApplication( id );
                return new HttpResponseMessage( HttpStatusCode.OK );
            }
            catch ( Exception e )
            {
                throw new Exception( $"Failed to uninstall application {id}", e );
            }
        }

		/// <summary>
		/// Upgrades the application.
		/// </summary>
		/// <param name="vid">The vid.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		[Route( "upgrade/{vid}" )]
        [HttpPost]
        public HttpResponseMessage UpgradeApplication( Guid vid )
        {
            try
            {
                AppManagerServiceImpl.UpgradeApplication( vid );
                return new HttpResponseMessage( HttpStatusCode.OK );
            }
            catch ( Exception e )
            {
                throw new Exception( $"Failed to upgrade application {vid}", e );
            }
        }

        #endregion

        #region Private Methods

        private IList<AppManagerData> GetAppManagerData( IList<AvailableApplication> availableApps, IList<InstalledApplication> installedApps )
        {
            // Get app security method
            var security = ApplicationSecuritySettings.Current == null
                ? AppSecurityModel.Restricted
                : ApplicationSecuritySettings.Current.AppSecurityModel;

            // Organise data
            var availableDict = availableApps.ToDictionary( a => a.ApplicationId );
            var installedDict = installedApps.ToDictionary( a => a.ApplicationId );
            var allApplicationIds = availableDict.Keys.Union( installedDict.Keys );

            // Build result
            List<AppManagerData> response = new List<AppManagerData>( );

            foreach ( Guid applicationId in allApplicationIds )
            {
                // Find the apps
                AvailableApplication availableApp;
                InstalledApplication installedApp;
                availableDict.TryGetValue( applicationId, out availableApp );
                installedDict.TryGetValue( applicationId, out installedApp );

                // Check if we can see the app
                bool canSee = AppManagerServiceImpl.CanSee( installedApp, availableApp, security );
                if ( !canSee )
                    continue;

                // Create a result entry
                AvailableApplication app = availableApp ?? installedApp;

	            if ( app != null )
	            {
		            AppManagerData appResultData = new AppManagerData
		            {
			            Name = app.Name,
			            ApplicationEntityId = app.ApplicationEntityId,
			            ApplicationId = app.ApplicationId,
			            ApplicationVersionId = app.ApplicationVersionId,
			            PackageEntityId = app.PackageEntityId,
			            PackageVersion = app.PackageVersion,
			            Publisher = app.Publisher,
			            PublisherUrl = app.PublisherUrl,
			            ReleaseDate = app.ReleaseDate,
			            CanExport = AppManagerServiceImpl.CanExport( installedApp, availableApp, security ),
			            CanPublish = AppManagerServiceImpl.CanPublish(installedApp, availableApp, security),
			            CanDeploy = AppManagerServiceImpl.CanDeploy( installedApp, availableApp, security ),
			            CanUpgrade = AppManagerServiceImpl.CanUpgrade( installedApp, availableApp, security ),
			            CanRepair = AppManagerServiceImpl.CanRepair( installedApp, availableApp, security ),
			            CanUninstall = AppManagerServiceImpl.CanUninstall( installedApp, availableApp, security )
		            };
		            if ( installedApp != null )
		            {
			            appResultData.SolutionEntityId = installedApp.SolutionEntityId;
			            appResultData.SolutionVersion = installedApp.SolutionVersion;
		            }

		            response.Add( appResultData );
	            }
            }
            return response;
        }

        #endregion
    }
}