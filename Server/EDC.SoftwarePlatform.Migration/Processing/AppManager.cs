// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using EDC.Database;
using EDC.Diagnostics;
using EDC.ReadiNow.AppLibrary;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Sources;
using EDC.SoftwarePlatform.Migration.Targets;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model.PartialClasses;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Provides high-level entry points to various application management routines.
	/// </summary>
	public static class AppManager
	{
		/// <summary>
		///     The application management log lock
		/// </summary>
		private static readonly object AppMgmtLogLock = new object( );

		/// <summary>
		///     The default timeout
		/// </summary>
		public static readonly int DefaultTimeout = 600;

		/// <summary>
		///     The application management log
		/// </summary>
		private static IEventLog _appMgmtLog;

		/// <summary>
		///     Gets the application.
		/// </summary>
		/// <value>
		///     The application.
		/// </value>
		private static IEventLog ApplicationManagementLog
		{
			get
			{
				if ( _appMgmtLog == null )
				{
					lock ( AppMgmtLogLock )
					{
						List<IEventLogWriter> logWriters = new List<IEventLogWriter>( );

						/////
						// Get access to the diagnostics configuration settings
						/////
						DiagnosticsConfiguration diagnosticsConfiguration = ConfigurationSettings.GetDiagnosticsConfigurationSection( );

						if ( diagnosticsConfiguration != null )
						{
							string folder = SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Log );
							string fileName = diagnosticsConfiguration.AppManagementLogSettings.Filename;

							IEventLogWriter fileLogWriter = new FileEventLogWriter( folder, fileName )
							{
								MaxSize = diagnosticsConfiguration.LogSettings.MaxSize,
								MaxCount = diagnosticsConfiguration.LogSettings.MaxCount,
								MaxRetention = diagnosticsConfiguration.LogSettings.MaxRetention,
							};
							logWriters.Add( fileLogWriter );

							IEventLogWriter syslogWriter = EventLog.GetSyslogEventLogWriter( );
							if ( syslogWriter != null )
							{
								logWriters.Add( syslogWriter );
							}

							_appMgmtLog = new Diagnostics.EventLog( logWriters );
						}
					}
				}

				return _appMgmtLog;
			}
		}

		/// <summary>
		///     Compares the version string of an installed application against its available counterpart and
		///     indicates if the publish option is applicable to the current context.
		/// </summary>
		/// <param name="installedVersion">The installed version string.</param>
		/// <param name="availableVersion">The available version string.</param>
		/// <returns>True if publish is applicable.</returns>
		public static bool CanPublish( string installedVersion, string availableVersion )
		{
			return string.IsNullOrWhiteSpace( availableVersion ) || IsGreaterThan( installedVersion, availableVersion );
		}

		/// <summary>
		///     Compares the version string of an installed application against its available counterpart and
		///     indicates if the repair option is applicable to the current context.
		/// </summary>
		/// <param name="installedVersion">The installed version string.</param>
		/// <param name="availableVersion">The available version string.</param>
		/// <returns>True if repair is applicable.</returns>
		public static bool CanRepair( string installedVersion, string availableVersion )
		{
			return !string.IsNullOrWhiteSpace( installedVersion ) &&
				   !string.IsNullOrWhiteSpace( availableVersion ) &&
				   installedVersion == availableVersion;
		}

		/// <summary>
		///     Compares the version string of an installed application against its available counterpart and
		///     indicates if the upgrade option is applicable to the current context.
		/// </summary>
		/// <param name="installedVersion">The installed version string.</param>
		/// <param name="availableVersion">The available version string.</param>
		/// <returns>True if upgrade is applicable.</returns>
		public static bool CanUpgrade( string installedVersion, string availableVersion )
		{
			return !string.IsNullOrWhiteSpace( installedVersion ) && IsGreaterThan( availableVersion, installedVersion );
		}

		/// <summary>
		///     Deletes the specified application.
		/// </summary>
		/// <param name="appVerId">The application version unique identifier to be deleted.</param>
		/// <param name="context">The processing context.</param>
		public static void DeleteApp( Guid appVerId, IProcessingContext context = null )
		{
			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				if ( context == null )
				{
					context = new ProcessingContext( );
				}

				try
				{
					if ( context.Report.Action == AppLibraryAction.Unknown )
					{
						context.Report.Action = AppLibraryAction.Delete;
						context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application Version Id", appVerId.ToString( "B" ) ) );
					}

					context.Report.StartTime = DateTime.Now;

					using ( DatabaseContext ctx = DatabaseContext.GetContext( true, commandTimeout: DefaultTimeout, transactionTimeout: DefaultTimeout ) )
					{
						using ( var source = new LibraryAppSource
						{
							AppVerId = appVerId
						} )
						{
							source.Delete( context );

							AppPackage package = SystemHelper.GetPackageByVerId( appVerId );

							if ( package != null )
							{
								App app = package.PackageForApplication;

                                GenerateLog(context, appVerId);

                                package.AsWritable<AppPackage>( ).Delete( );

								/////
								// Remove the application if it has no packages.
								/////
								if ( app.ApplicationPackages.Count == 0 )
								{
									app.AsWritable<App>( ).Delete( );
								}
							}
						}

						ctx.CommitTransaction( );
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );

					context.Report.Exception = exc;

					throw;
				}
				finally
				{
					context.Report.EndTime = DateTime.Now;

					GenerateReport( context );
				}
			}
		}

		/// <summary>
		/// Deploys an application from the application library to a tenant
		/// </summary>
		/// <param name="tenantName">Name of the tenant to receive the application.</param>
		/// <param name="application">The name|guid of the application being deployed.</param>
		/// <param name="appVersion">The application version to be deployed.</param>
		/// <param name="context">The processing context.</param>
		/// <param name="updateStats">if set to <c>true</c> update statistics.</param>
		/// <param name="disableFts">if set to <c>true</c> disable FTS.</param>
		/// <param name="installDependencies">if set to <c>true</c> [install dependencies].</param>
		public static void DeployApp( string tenantName, string application, string appVersion = null, IProcessingContext context = null, bool updateStats = true, bool disableFts = true, bool installDependencies = true )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			string applicationName = GetApplicationName( application, tenantName );

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Deploy;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", applicationName ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Version", appVersion ?? "Not specified" ) );
			}

			using ( DatabaseContextInfo.SetContextInfo( $"Deploy app '{applicationName}' to '{tenantName}'" ) )
			{
				UpgradeApp( tenantName, application, appVersion, context, updateStats, disableFts, installDependencies );
			}
		}

		/// <summary>
		///     Removes the application.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="application">The name|guid of the application being removed.</param>
		/// <param name="context">The context.</param>
		/// <exception cref="System.ApplicationException">
		/// </exception>
		public static void RemoveApp( string tenantName, string application, IProcessingContext context = null )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Remove;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", GetApplicationName( application, tenantName ) ) );
			}

			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				long tenantId = TenantHelper.GetTenantId( tenantName );

				if ( tenantId < 0 )
				{
					return;
				}

				/////
				// Create source to load app data from tenant
				/////
				Guid appGuid;

				long appId = Guid.TryParse( application, out appGuid ) ? SystemHelper.GetTenantApplicationIdByGuid( tenantId, appGuid ) : SystemHelper.GetTenantApplicationIdByName( tenantId, application );

				if ( appId < 0 )
				{
					throw new ApplicationException( $"Could not load application: {application}" );
				}

                Guid? packageGuid = appGuid;

                GenerateLog(context, packageGuid, tenantId);

                using ( new TenantAdministratorContext( tenantId ) )
				{
					IList<ApplicationDependency> applicationDependents = SolutionHelper.GetApplicationDependents( appId, true );

					if ( applicationDependents != null && applicationDependents.Count > 0 )
					{
						foreach ( ApplicationDependency dependency in applicationDependents )
						{
							RemoveApp( tenantName, dependency.DependentApplication.UpgradeId.ToString( "B" ) );
						}
					}

					Entity.Delete( appId );
				}
			}
		}

		/// <summary>
		/// Exports an application from a tenant to a package (SqLite).
		/// </summary>
		/// <param name="tenantName">Name of the tenant whose application is to be exported.</param>
		/// <param name="application">The name|guid of the application being exported.</param>
		/// <param name="packagePath">The package path were the exported application will reside.</param>
		/// <param name="packageFormat">The package format.</param>
		/// <param name="context">The processing context.</param>
		public static void ExportAppPackage( string tenantName, string application, string packagePath, Format packageFormat, IProcessingContext context = null )
		{
            long tenantId = 0;

            using ( new GlobalAdministratorContext( ) )
			{
				if ( context == null )
				{
					context = new ProcessingContext( );
				}

				if ( context.Report.Action == AppLibraryAction.Unknown )
				{
					context.Report.Action = AppLibraryAction.Export;
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", GetApplicationName( application, tenantName ) ) );
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Path", packagePath ) );
				}

				context.Report.StartTime = DateTime.Now;

                Guid? prevPackageId = null;

                try
				{
					Guid appGuid;

					/////
					// Get ID details
					/////
					tenantId = TenantHelper.GetTenantId( tenantName, true );

					long appId = Guid.TryParse( application, out appGuid ) ? SystemHelper.GetTenantApplicationIdByGuid( tenantId, appGuid ) : SystemHelper.GetTenantApplicationIdByName( tenantId, application );

                    /////
                    // Get the current package ID of the application
                    /////
                    using ( new TenantAdministratorContext( tenantId ) )
                    {
                        Solution solution = Entity.Get<Solution>( appId );

                        prevPackageId = solution?.PackageId;
                    }

                    /////
                    // Create source to load app data from tenant
                    /////
                    using ( var source = new TenantAppSource
					{
						TenantId = tenantId,
						SolutionId = appId,
                        OriginalPackageId = prevPackageId
                    } )
					{
						if ( packageFormat == Format.Undefined )
						{
							packageFormat = FileManager.GetExportFileFormat( packagePath );
						}

						using ( IDataTarget target = FileManager.CreateDataTarget( packageFormat, packagePath ) )
						{
							var processor = new CopyProcessor( source, target, context );
							processor.MigrateData( );
						}
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );

					context.Report.Exception = exc;

					throw;
				}
				finally
				{
					context.Report.EndTime = DateTime.Now;

                    GenerateLog(context, prevPackageId, tenantId);
                    GenerateReport( context );
				}
			}
		}

		/// <summary>
		/// Exports an application from the application library to a package (SqLite).
		/// </summary>
		/// <param name="appVerId">Identifier of the application to be exported from the application library.</param>
		/// <param name="packagePath">The package path were the exported application will reside.</param>
		/// <param name="packageFormat">The package format.</param>
		/// <param name="context">The processing context.</param>
		/// <exception cref="System.InvalidOperationException">Invalid ApplicationId</exception>
		public static void ExportAppPackage( Guid appVerId, string packagePath, Format packageFormat, IProcessingContext context = null )
		{
			using ( new SecurityBypassContext( ) )
			{
				string appName = string.Empty;
				string publisher = null;
				string publisherUrl = null;
				DateTime releaseDate = DateTime.UtcNow;

				if ( context == null )
				{
					context = new ProcessingContext( );
				}

				if ( context.Report.Action == AppLibraryAction.Unknown )
				{
					context.Report.Action = AppLibraryAction.Export;
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application Version Id", appVerId.ToString( "B" ) ) );
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Package Path", packagePath ) );
				}

				context.Report.StartTime = DateTime.Now;

				try
				{
					string packageName;
					string description;
					Guid appId;
					string packageVersion;
					DateTime publishDate;
					IList<SolutionDependency> dependencies = new List<SolutionDependency>( );

					using ( new GlobalAdministratorContext( ) )
					{
						AppPackage package = SystemHelper.GetPackageByVerId( appVerId );
						if ( package == null )
						{
							throw new InvalidOperationException( $"Failed to get the package '{appVerId}'." );
						}

						packageName = package.Name;
						description = package.Description;
						packageVersion = package.AppVersionString;
						publishDate = package.PublishDate ?? DateTime.UtcNow;

						if ( package.PackageForApplication.ApplicationId == null )
						{
							throw new InvalidOperationException( "Invalid ApplicationId" );
						}

						appId = package.PackageForApplication.ApplicationId.Value;

						App packageForApplication = package.PackageForApplication;

						if ( packageForApplication != null )
						{
							appName = packageForApplication.Name;
							publisher = packageForApplication.Publisher;
							publisherUrl = packageForApplication.PublisherUrl;
							releaseDate = packageForApplication.ReleaseDate ?? DateTime.UtcNow;
						}

						if ( package.DependentAppPackageDetails != null )
						{
							foreach ( AppPackageDependency appPackageDependency in package.DependentAppPackageDetails )
							{
								dependencies.Add( new SolutionDependency( appPackageDependency ) );
							}
						}
					}

					/////
					// Create source to load app data from tenant
					/////
					using ( var source = new LibraryAppSource
					{
						AppId = appId,
						AppVerId = appVerId,
						Name = packageName,
						Description = description,
						AppName = appName,
						Version = packageVersion,
						Publisher = publisher,
						PublisherUrl = publisherUrl,
						ReleaseDate = releaseDate,
						PublishDate = publishDate,
						Dependencies = dependencies
					} )
					{
						if ( packageFormat == Format.Undefined )
						{
							packageFormat = FileManager.GetExportFileFormat( packagePath );
						}

						using ( IDataTarget target = FileManager.CreateDataTarget( packageFormat, packagePath ) )
						{
							/////
							// Copy the data
							/////
							var processor = new CopyProcessor( source, target, context );
							processor.MigrateData( );
						}
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );

					context.Report.Exception = exc;

					throw;
				}
				finally
				{
					context.Report.EndTime = DateTime.Now;

                    GenerateLog(context, appVerId);
                    GenerateReport( context );
				}
			}
		}


		private static Action<IEntityEventDeploy, IEnumerable<IEntity>> FireOnAfterDeployEventFn( IDictionary<string, object> state )
		{
			return ( entityEvent, identifiers ) =>
			{
				if ( entityEvent == null )
				{
					EventLog.Application.WriteWarning( "The specified target does not implement the 'IEntityEvent' interface." );
					return;
				}

				entityEvent.OnAfterDeploy( identifiers, state );
			};
		}


		private static Func<IEntityEventUpgrade, IEnumerable<IEntity>, bool> FireOnBeforeUpgradeEventFn( IDictionary<string, object> state )
		{
			return ( entityEvent, identifiers ) =>
			{
				if ( entityEvent == null )
				{
					EventLog.Application.WriteWarning( "The specified target does not implement the 'IEntityEvent' interface." );
					return false;
				}

				return entityEvent.OnBeforeUpgrade( identifiers, state );
			};
		}

		private static Action<IEntityEventUpgrade, IEnumerable<IEntity>> FireOnAfterUpgradeEventFn( IDictionary<string, object> state )
		{
			return ( entityEvent, identifiers ) =>
			{
				if ( entityEvent == null )
				{
					EventLog.Application.WriteWarning( "The specified target does not implement the 'IEntityEvent' interface." );
					return;
				}

				entityEvent.OnAfterUpgrade( identifiers, state );
			};
		}


		/// <summary>
		///     Fires the on after publish event.
		/// </summary>
		/// <param name="entityEvent">The entity event.</param>
		/// <param name="identifiers">The identifiers.</param>
		private static void FireOnAfterPublishEvent( IEntityEventPublish entityEvent, IEnumerable<IEntity> identifiers )
		{
			if ( entityEvent == null )
			{
				EventLog.Application.WriteWarning( "The specified target does not implement the 'IEntityEvent' interface." );
				return;
			}

			entityEvent.OnAfterPublish( identifiers, null );
		}

		/// <summary>
		///     Fires the on publish failed event.
		/// </summary>
		/// <param name="entityEvent">The entity event.</param>
		/// <param name="identifiers">The identifiers.</param>
		private static void FireOnPublishFailedEvent( IEntityEventPublish entityEvent, IEnumerable<IEntity> identifiers )
		{
			if ( entityEvent == null )
			{
				EventLog.Application.WriteWarning( "The specified target does not implement the 'IEntityEvent' interface." );
				return;
			}

			entityEvent.OnPublishFailed( identifiers, null );
		}

		/// <summary>
		/// Fires the on deploy failed event.
		/// </summary>
		/// <param name="entityEvent">The entity event.</param>
		/// <param name="solutions">The solutions.</param>
		private static void FireOnDeployFailedEvent( IEntityEventDeploy entityEvent, IEnumerable<ISolutionDetails> solutions )
		{
			if ( entityEvent == null )
			{
				EventLog.Application.WriteWarning( "The specified target does not implement the 'IEntityEvent' interface." );
				return;
			}

			entityEvent.OnDeployFailed( solutions, null );
		}

        /// <summary>
        /// Log an entry about an application management operation to the database.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        private static void GenerateLog(IProcessingContext context, Guid? applicationId, long tenantId = 0)
        {
            try
            {
                using (var ctx = DatabaseContext.GetContext())
                {
                    using (var cmd = ctx.CreateCommand("spPlatformHistory", CommandType.StoredProcedure))
                    {
                        string user = null;

                        if (applicationId.HasValue)
                        {
                            cmd.AddParameterWithValue("@packageId", applicationId.Value);
                        }

                        cmd.AddParameterWithValue("@tenantId", tenantId);

                        if (context?.Report != null)
                        {
                            user = context.Report.UserName;

                            cmd.AddParameterWithValue("@operation", context.Report.Action.ToString());

                            if (context.Report.Arguments != null && context.Report.Arguments.Count > 0)
                            {
                                var sb = new StringBuilder();
                                foreach (var arg in context.Report.Arguments)
                                {
                                    var key = arg.Key ?? "";
                                    var value = arg.Value ?? "";
                                    sb.AppendFormat("[{0}: {1}]", key.Replace(']', ' '), value.Replace(']', ' '));
                                }

                                cmd.AddParameterWithValue("@arguments", sb.ToString());
                            }

                            if (context.Report.Exception != null)
                            {
                                cmd.AddParameterWithValue("@exception", context.Report.Exception.ToString());
                            }
                        }

                        //
                        // Process
                        //
                        string process = null;

                        try
                        {
                            process = Process.GetCurrentProcess().ProcessName;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        //
                        // User
                        //
                        if (user == null)
                        {
                            var req = RequestContext.GetContext();
                            user = req?.Identity?.Name;
                        }

                        var windomain = "";
                        try
                        {
                            windomain = Environment.UserDomainName + "\\";
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        var winuser = windomain + Environment.UserName;
                        user = user == null ? winuser : $"{user} ({winuser})";

                        //
                        // Machine
                        //
                        string machine = null;

                        try
                        {
                            machine = Environment.MachineName;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        cmd.AddParameterWithValue("@machine", machine);
                        cmd.AddParameterWithValue("@user", user);
                        cmd.AddParameterWithValue("@process", process);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteWarning("Failed to log entry to platform application history table. {0}", ex.Message);
            }
        }

        /// <summary>
        ///     Generates the report.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void GenerateReport( IProcessingContext context )
		{
			if ( context == null )
			{
				EventLog.Application.WriteWarning( "Unable to generate application management log. Invalid processing context" );
				return;
			}

			var log = ApplicationManagementLog;

			log?.WriteInformation( context.Report.ToString( ) );
		}

		/// <summary>
		/// Gets the name of the application.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		public static string GetApplicationName( string application, string tenantName )
		{
			if ( string.IsNullOrEmpty( application ) )
			{
				return string.Empty;
			}

			Guid appId;

			if ( Guid.TryParse( application, out appId ) )
			{
				if ( appId == Applications.ConsoleApplicationId )
				{
					return "Console Solution";
				}

				if ( appId == Applications.CoreApplicationId )
				{
					return "Core Solution";
				}

				if ( appId == Applications.CoreDataApplicationId )
				{
					return "Core Data Solution";
				}

				if ( appId == Applications.SharedApplicationId )
				{
					return "Shared Solution";
				}

				if ( appId == Applications.TestApplicationId )
				{
					return "Test Solution";
				}

				long tenantId = TenantHelper.GetTenantId( tenantName );

				if ( tenantId >= 0 )
				{
					using ( new TenantAdministratorContext( tenantId ) )
					{
						var id = Entity.GetIdFromUpgradeId( appId );

						if ( id >= 0 )
						{
							string name = Entity.GetName( id );

							if ( !string.IsNullOrEmpty( name ) )
							{
								return name;
							}
						}
					}
				}
			}

			return application;
		}

		/// <summary>
		///     Imports an application package (SQLite db) into the main app library (database).
		/// </summary>
		/// <param name="packagePath">Path to the SQLite database.</param>
		/// <param name="bootstrapMode">Set to true when installing core applications before global tenant is ready.</param>
		/// <param name="context">The processing context.</param>
		/// <returns>
		///     The application version identifier of the imported application.
		/// </returns>
		public static Guid ImportAppPackage( string packagePath, IProcessingContext context = null, bool bootstrapMode = false )
		{
			if ( string.IsNullOrEmpty( packagePath ) )
			{
				throw new ArgumentException( "Database path not specified." );
			}

			if ( !File.Exists( packagePath ) )
			{
				throw new FileNotFoundException( $"The specified file '{packagePath}' cannot be found.", packagePath );
			}

			using ( new DeferredChannelMessageContext( ) )
			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				if ( context == null )
				{
					context = new ProcessingContext( );
				}

				if ( context.Report.Action == AppLibraryAction.Unknown )
				{
					context.Report.Action = bootstrapMode ? AppLibraryAction.BootstrapImport : AppLibraryAction.Import;
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Package Path", packagePath ) );
				}

				context.Report.StartTime = DateTime.Now;

                Guid? applicationVersionId = null;

                try
				{
					using ( IDataSource source = FileManager.CreateDataSource( packagePath ) )
					{

						Metadata metadata = source.GetMetadata( context );

						if ( metadata.Type != SourceType.AppPackage )
						{
							throw new InvalidOperationException( "The package does not contain an application." );
						}

					    applicationVersionId = metadata.AppVerId;

                        if ( PackageExists( metadata.AppId, metadata.AppVerId ) )
                        {
                            context.WriteInfo( $"Skipping import or package at '{packagePath}' as it already exists in the library." );

                            return metadata.AppVerId;
                        }

                        /////
                        // Create target to write to SQLite database
                        /////
                        using ( var target = new LibraryAppTarget
						{
							ApplicationVersionId = source.GetMetadata( context ).AppVerId,
                            SkipMetadata = bootstrapMode
                        } )
						{
							/////
							// Copy the data
							/////
							var processor = new CopyProcessor( source, target, context );
							processor.MigrateData( );

							target.Commit( );

							applicationVersionId = target.ApplicationVersionId;
						}
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );

					context.Report.Exception = exc;

					throw;
				}
				finally
				{
					ForeignKeyHelper.Trust( );

					context.Report.EndTime = DateTime.Now;

                    GenerateLog(context, applicationVersionId);
                    GenerateReport( context );
				}

				return applicationVersionId.Value;
			}
		}

		/// <summary>
		///     Returns true if <see cref="versionA" /> is considered greater than <see cref="versionB" />.
		/// </summary>
		/// <param name="versionA">The version to check.</param>
		/// <param name="versionB">The version to compare against.</param>
		/// <returns>
		///     True if the checked version is the greater of the two. In the case of them being equal,
		///     false is returned.
		/// </returns>
		private static bool IsGreaterThan( string versionA, string versionB )
		{
			if ( string.IsNullOrWhiteSpace( versionA ) || string.IsNullOrWhiteSpace( versionB ) )
			{
				return false;
			}

			string [ ] componentsA = versionA.Split( '.' );
			string [ ] componentsB = versionB.Split( '.' );

			int places = Math.Max( componentsA.Length, componentsB.Length );

			for ( int place = 0; place < places; place++ )
			{
				string partA = componentsA.Length > place ? componentsA [ place ] : "";
				string partB = componentsB.Length > place ? componentsB [ place ] : "";

				// try int compare then if cant, just string comparison
				if ( !string.IsNullOrWhiteSpace( partA ) && !string.IsNullOrWhiteSpace( partB ) && partA != partB )
				{
					int intA;
					int intB;

					if ( int.TryParse( partA, out intA ) && int.TryParse( partB, out intB ) )
					{
						return intA > intB;
					}

					return String.Compare( partA, partB, StringComparison.OrdinalIgnoreCase ) > 0;
				}

				// 1.0.0 vs 1.0 ?
				if ( string.IsNullOrWhiteSpace( partB ) && !string.IsNullOrWhiteSpace( partA ) )
				{
					return true;
				}

				// 1.0 vs 1.0.0 ?
				if ( string.IsNullOrWhiteSpace( partA ) )
				{
					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// Converts the application package.
		/// </summary>
		/// <param name="sourcePath">The package path.</param>
		/// <param name="targetPath">The target path.</param>
		/// <param name="targetFormat">The target format.</param>
		/// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
		/// <param name="context">The context.</param>
		/// <exception cref="System.InvalidOperationException">Application file format not supported.</exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		/// <exception cref="System.FormatException">Invalid file format detected
		/// or
		/// Invalid file format detected</exception>
		public static void ConvertApplicationPackage( string sourcePath, string targetPath, Format targetFormat, bool overwrite = false, IProcessingContext context = null )
		{
			if ( targetFormat == Format.Undefined )
			{
				throw new InvalidOperationException( $"Invalid target format '{targetFormat}'." );
			}

			if ( string.IsNullOrEmpty( sourcePath ) )
			{
				throw new ArgumentNullException( nameof( sourcePath ) );
			}

			if ( !File.Exists( sourcePath ) )
			{
				throw new FileNotFoundException( $"File '{sourcePath}' was not found.", sourcePath );
			}

			if ( File.Exists( targetPath ) )
			{
				if ( overwrite )
				{
					File.Delete( targetPath );
				}
				else
				{
					throw new InvalidOperationException( $"File '{targetPath}' already exists." );
				}
			}

			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			context.Report.Action = AppLibraryAction.Convert;
			context.Report.Arguments.Add( new KeyValuePair<string, string>( "Source Path", sourcePath ) );
			context.Report.Arguments.Add( new KeyValuePair<string, string>( "Target Path", targetPath ) );
			context.Report.Arguments.Add( new KeyValuePair<string, string>( "Target Format", targetFormat.ToString( ) ) );

			context.Report.StartTime = DateTime.Now;

			try
			{
				IDataSource source = null;
				IDataTarget target = null;

				try
				{
					source = FileManager.CreateDataSource( sourcePath );
					target = FileManager.CreateDataTarget( targetFormat, targetPath );

					/////
					// Copy the data
					/////
					var processor = new CopyProcessor( source, target, null );
					processor.MigrateData( );
				}
				finally
				{
					source?.Dispose( );
					target?.Dispose( );
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( exc.ToString( ) );

				context.Report.Exception = exc;

				throw;
			}
			finally
			{
				context.Report.EndTime = DateTime.Now;

                GenerateLog(context, null);
                GenerateReport( context );
			}
		}

		/// <summary>
		/// Packages the exists.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="applicationPackageId">The application package identifier.</param>
		/// <returns></returns>
		public static bool PackageExists( Guid applicationId, Guid applicationPackageId )
		{
			return ListAppPackages( applicationId ).Any( app => app.Packages.Any( pkg => pkg.PackageId == applicationPackageId ) );
		}

		/// <summary>
		/// Lists the packages associated with the application.
		/// </summary>
		/// <param name="appId">The application identifier.</param>
		/// <returns></returns>
		public static List<AppData> ListAppPackages( Guid appId )
		{
			const string query = @"
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
DECLARE @app BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
DECLARE @applicationId BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT )


SELECT
	Application = x.Application,
	ApplicationEntityId = x.ApplicationId,
	PackageId = pid.Data,
	PackageEntityId = p.PackageId,
	Version = p.Version,
	ApplicationId = aid.Data,
	Publisher = p1.Data,
	PublisherUrl = u.Data,
	ReleaseDate = c.Data
FROM
	(
	SELECT
		Application = n.Data,
		ApplicationId = n.EntityId
	FROM
		Relationship r
	JOIN
		Data_NVarChar n ON
			r.TenantId = n.TenantId AND
			r.FromId = n.EntityId AND
			n.FieldId = @name
	WHERE
		r.TenantId = 0 AND
		r.TypeId = @isOfType AND
		r.ToId = @app
	) x
JOIN
(
	SELECT
		dt.PackageId,
		dt.ApplicationId,
		dt.Version,
		dt.RowNumber
	FROM
	(
		SELECT
			ROW_NUMBER( ) OVER
			(
				PARTITION BY
					r.ToId
				ORDER BY
					CAST( '/' + REPLACE( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC
			) AS 'RowNumber',
			PackageId = r.FromId,
			ApplicationId = r.ToId,
			Version = v.Data
		FROM
			Relationship r
		JOIN
			Data_NVarChar v ON
				r.TenantId = v.TenantId AND
				r.FromId = v.EntityId AND
				r.TypeId = @packageForApplication AND
				v.FieldId = @appVersionString
		WHERE
			r.TenantId = 0
	) dt
) p ON
	x.ApplicationId = p.ApplicationId
JOIN
	Data_Guid pid ON
		pid.TenantId = 0 AND
		p.PackageId = pid.EntityId AND
		pid.FieldId = @appVerId
JOIN
	Data_Guid aid ON
		aid.TenantId = 0 AND
		x.ApplicationId = aid.EntityId AND
		aid.FieldId = @applicationId
LEFT JOIN
(
	SELECT
		p.EntityId,
		p.Data
	FROM
		Data_NVarChar p
	JOIN
		Data_Alias ap ON
			p.TenantId = ap.TenantId AND
			p.FieldId = ap.EntityId AND
			ap.Data = 'publisher' AND
			ap.Namespace = 'core'
	WHERE
		p.TenantId = 0
) p1 ON
	x.ApplicationId = p1.EntityId
LEFT JOIN
(
	SELECT
		u.EntityId,
		u.Data
	FROM
		Data_NVarChar u
	JOIN
		Data_Alias au ON
			u.TenantId = au.TenantId AND
			u.FieldId = au.EntityId AND
			au.Data = 'publisherUrl' AND
			au.Namespace = 'core'
	WHERE
		u.TenantId = 0
) u ON
	x.ApplicationId = u.EntityId
LEFT JOIN
(
	SELECT
		c.EntityId,
		c.Data
	FROM
		Data_DateTime c
	JOIN
		Data_Alias ac ON
			c.TenantId = ac.TenantId AND
			c.FieldId = ac.EntityId AND
			ac.Data = 'releaseDate' AND
			ac.Namespace = 'core'
	WHERE
		c.TenantId = 0
) c ON
	x.ApplicationId = c.EntityId
WHERE
	aid.Data = @appId
";
			var results = new Dictionary<Guid, AppData>( );

			using ( var ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( ) )
				{
					command.CommandType = CommandType.Text;
					command.CommandText = query;

					command.AddParameter( "@appId", DbType.Guid, appId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							string name = reader.GetString( 0 );
							Guid packageId = reader.GetGuid( 2 );
							string version = reader.GetString( 4 );
							Guid applicationId = reader.GetGuid( 5 );

							AppData app;

							if ( !results.TryGetValue( applicationId, out app ) )
							{
								app = new AppData( name, applicationId );
								results [ applicationId ] = app;
							}

							app.Packages.Add( new AppPackageData( version, packageId ) );
						}
					}
				}
			}

			return results.Values.ToList( );
		}

		/// <summary>
		///     Lists the apps.
		/// </summary>
		/// <param name="listAll">if set to <c>true</c> [list all].</param>
		/// <returns></returns>
		public static List<AppData> ListApps( bool listAll = false )
		{
			const string query = @"
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
DECLARE @app BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
DECLARE @applicationId BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT )


SELECT
	Application = x.Application,
	ApplicationEntityId = x.ApplicationId,
	PackageId = pid.Data,
	PackageEntityId = p.PackageId,
	Version = p.Version,
	ApplicationId = aid.Data,
	Publisher = p1.Data,
	PublisherUrl = u.Data,
	ReleaseDate = c.Data
FROM
	(
	SELECT
		Application = n.Data,
		ApplicationId = n.EntityId
	FROM
		Relationship r
	JOIN
		Data_NVarChar n ON
			r.TenantId = n.TenantId AND
			r.FromId = n.EntityId AND
			n.FieldId = @name
	WHERE
		r.TenantId = 0 AND
		r.TypeId = @isOfType AND
		r.ToId = @app
	) x
JOIN
(
	SELECT
		dt.PackageId,
		dt.ApplicationId,
		dt.Version,
		dt.RowNumber
	FROM
	(
		SELECT
			ROW_NUMBER( ) OVER
			(
				PARTITION BY
					r.ToId
				ORDER BY
					CAST( '/' + REPLACE( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC
			) AS 'RowNumber',
			PackageId = r.FromId,
			ApplicationId = r.ToId,
			Version = v.Data
		FROM
			Relationship r
		JOIN
			Data_NVarChar v ON
				r.TenantId = v.TenantId AND
				r.FromId = v.EntityId AND
				r.TypeId = @packageForApplication AND
				v.FieldId = @appVersionString
		WHERE
			r.TenantId = 0
	) dt
) p ON
	x.ApplicationId = p.ApplicationId
JOIN
	Data_Guid pid ON
		pid.TenantId = 0 AND
		p.PackageId = pid.EntityId AND
		pid.FieldId = @appVerId
JOIN
	Data_Guid aid ON
		aid.TenantId = 0 AND
		x.ApplicationId = aid.EntityId AND
		aid.FieldId = @applicationId
LEFT JOIN
(
	SELECT
		p.EntityId,
		p.Data
	FROM
		Data_NVarChar p
	JOIN
		Data_Alias ap ON
			p.TenantId = ap.TenantId AND
			p.FieldId = ap.EntityId AND
			ap.Data = 'publisher' AND
			ap.Namespace = 'core'
	WHERE
		p.TenantId = 0
) p1 ON
	x.ApplicationId = p1.EntityId
LEFT JOIN
(
	SELECT
		u.EntityId,
		u.Data
	FROM
		Data_NVarChar u
	JOIN
		Data_Alias au ON
			u.TenantId = au.TenantId AND
			u.FieldId = au.EntityId AND
			au.Data = 'publisherUrl' AND
			au.Namespace = 'core'
	WHERE
		u.TenantId = 0
) u ON
	x.ApplicationId = u.EntityId
LEFT JOIN
(
	SELECT
		c.EntityId,
		c.Data
	FROM
		Data_DateTime c
	JOIN
		Data_Alias ac ON
			c.TenantId = ac.TenantId AND
			c.FieldId = ac.EntityId AND
			ac.Data = 'releaseDate' AND
			ac.Namespace = 'core'
	WHERE
		c.TenantId = 0
) c ON
	x.ApplicationId = c.EntityId
";

			const string latestOnly = @"
WHERE
	p.RowNumber = 1";

			var results = new Dictionary<Guid, AppData>( );

			using ( var ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( ) )
				{
					command.CommandType = CommandType.Text;
					command.CommandText = listAll ? query : query + latestOnly;

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							string name = reader.GetString( 0 );
							Guid packageId = reader.GetGuid( 2 );
							string version = reader.GetString( 4 );
							Guid applicationId = reader.GetGuid( 5 );

							AppData app;

							if ( !results.TryGetValue( applicationId, out app ) )
							{
								app = new AppData( name, applicationId );
								results [ applicationId ] = app;
							}

							app.Packages.Add( new AppPackageData( version, packageId ) );
						}
					}
				}
			}

			return results.Values.ToList( );
		}

		public static List<AppData> ListTenantApps( string tenantName )
		{
			string query = $@"
--find out which applications are installed for a tenant
DECLARE @tenantId BIGINT
SELECT @tenantId = Id FROM _vTenant WHERE LOWER(name) = LOWER('{tenantName}')
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
 
SELECT
    Tenant = n.Data,
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
    Relationship r
JOIN
    Data_NVarChar n ON
		r.FromId = n.EntityId AND
		n.FieldId = @name AND
		r.TenantId = n.TenantId
CROSS APPLY
	dbo.tblFnAliasNsId( 'isOfType', 'core', @tenantId ) iot
CROSS APPLY
	dbo.tblFnAliasNsId( 'solution', 'core', @tenantId ) sol
JOIN
    Relationship s ON
		s.TenantId = r.FromId AND
        s.TypeId = iot.EntityId AND
        s.ToId = sol.EntityId
CROSS APPLY
	dbo.tblFnFieldNVarCharA( s.FromId, s.TenantId, 'solutionVersionString', 'core' ) sv
CROSS APPLY
	dbo.tblFnFieldNVarCharA( s.FromId, s.TenantId, 'name', 'core' ) sn
CROSS APPLY
	dbo.tblFnFieldGuidA( s.FromId, s.TenantId, 'packageId', 'core' ) pid
LEFT JOIN
    Data_Guid p ON
		p.TenantId = 0 AND
		pid.Data = p.Data AND
        p.FieldId = @appVerId
LEFT JOIN
    Relationship pa ON
		pa.TenantId = 0 AND
		p.EntityId = pa.FromId AND
        pa.TypeId = @packageForApplication
LEFT JOIN
    Data_NVarChar pv ON
		pv.TenantId = p.TenantId AND
		p.EntityId = pv.EntityId AND
        pv.FieldId = @appVersionString
OUTER APPLY
	dbo.tblFnFieldNVarCharA( s.FromId, s.TenantId, 'solutionPublisher', 'core' ) p1
OUTER APPLY
	dbo.tblFnFieldNVarCharA( s.FromId, s.TenantId, 'solutionPublisherUrl', 'core' ) u
OUTER APPLY
	dbo.tblFnFieldDateTimeA( s.FromId, s.TenantId, 'solutionReleaseDate', 'core' ) c
JOIN
    Entity aid ON
		s.TenantId = aid.TenantId AND
		s.FromId = aid.Id
WHERE
	r.TenantId = 0 AND
    s.TenantId = @tenantId AND (
        p.EntityId = pa.FromId OR
        p.EntityId IS NULL
    )
ORDER BY
    n.Data,
	sn.Data,
	pv.Data";

			var results = new Dictionary<Guid, AppData>( );

			using ( var ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( ) )
				{
					command.CommandType = CommandType.Text;
					command.CommandText = query;

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							string name = reader.GetString( 1 );
							Guid packageId = reader.GetGuid( 4 );
							string version = reader.GetString( 6 );
							Guid applicationId = reader.GetGuid( 8 );

							AppData app;

							if ( !results.TryGetValue( applicationId, out app ) )
							{
								app = new AppData( name, applicationId );
								results [ applicationId ] = app;
							}

							app.Packages.Add( new AppPackageData( version, packageId ) );
						}
					}
				}
			}

			return results.Values.ToList( );
		}

		public static List<AppData> ListAppAccess( string tenantName )
		{
			string query = $@"-- ListAppAccess
                        DECLARE @tenantId BIGINT
                        SELECT @tenantId = Id FROM _vTenant WHERE LOWER(name) = LOWER('{tenantName}')
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
								Application = n.Data,
								ApplicationId = n.EntityId
							FROM
								Relationship r
							JOIN
								Data_NVarChar n ON
									r.FromId = n.EntityId AND
									n.FieldId = @name AND
									n.TenantId = 0
							WHERE
								r.TypeId = @isOfType AND
								r.TenantId = 0 AND
								r.ToId = @app
							) x
						JOIN
						(
							SELECT
								dt.PackageId,
								dt.ApplicationId,
								dt.Version,
								dt.RowNumber
							FROM
							(
								SELECT
									ROW_NUMBER( ) OVER
									(
										PARTITION BY
											r.ToId
										ORDER BY
											CAST( '/' + REPLACE( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' AS HIERARCHYID ) DESC
									) AS 'RowNumber',
									PackageId = r.FromId,
									ApplicationId = r.ToId,
									Version = v.Data
								FROM
									Relationship r
								JOIN
									Data_NVarChar v ON
										v.TenantId = r.TenantId AND
										r.FromId = v.EntityId AND
										r.TypeId = @packageForApplication AND
										v.FieldId = @appVersionString
								WHERE
									r.TenantId = 0
							) dt
						) p ON x.ApplicationId = p.ApplicationId
						JOIN
							Data_Guid pid ON
								p.PackageId = pid.EntityId AND
								pid.TenantId = 0 AND
								pid.FieldId = @appVerId
						JOIN
							Data_Guid aid ON
								x.ApplicationId = aid.EntityId AND
								aid.TenantId = 0 AND
								aid.FieldId = @applicationId
						OUTER APPLY
							dbo.tblFnFieldNVarCharA( x.ApplicationId, 0, 'publisher', 'core' ) p1
						OUTER APPLY
							dbo.tblFnFieldNVarCharA( x.ApplicationId, 0, 'publisherUrl', 'core' ) u
						OUTER APPLY
							dbo.tblFnFieldDateTimeA( x.ApplicationId, 0, 'releaseDate', 'core' ) c
						LEFT JOIN
						(
							SELECT
								ToId AppId,
								1 CanPublish
							FROM
								Relationship r
							WHERE
								TypeId = @canPublishApplication AND
								TenantId = 0 AND
								FromId = @tenantId
						) cp ON
							x.ApplicationId = cp.AppId
						LEFT JOIN
						(
							SELECT
								ToId AppId,
								1 CanInstall
							FROM
								Relationship r
							WHERE
								TypeId = @canInstallApplication AND
								TenantId = 0 AND
								FromId = @tenantId
						) ci ON
							x.ApplicationId = ci.AppId
						WHERE
							p.RowNumber = 1";

			var result = new List<AppData>( );

			using ( var ctx = DatabaseContext.GetContext( ) )
			using ( var command = ctx.CreateCommand( ) )
			{
				command.CommandType = CommandType.Text;
				command.CommandText = query;

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						string name = reader.GetString( 0 );
						Guid applicationId = reader.GetGuid( 5 );
						bool hasPublishPermission = reader.GetInt32( 9 ) == 1;
						bool hasInstallPermission = reader.GetInt32( 10 ) == 1;

						var app = new AppData( name, applicationId, hasInstallPermission, hasPublishPermission );
						result.Add( app );
					}
				}
			}

			return result;
		}

		/// <summary>
		///     Publishes an application from a tenant to the application library.
		/// </summary>
		/// <param name="tenantName">Name of the tenant where the application resides.</param>
		/// <param name="application">The name|guid of the application being published.</param>
		/// <param name="context">The processing context.</param>
		/// <exception cref="InvalidOperationException">
		///     @Failed to locate a solution with the specified name.
		///     or
		///     Invalid Application Version Id.
		/// </exception>
		/// <exception cref="ApplicationPublishException"></exception>
		/// <exception cref="NotImplementedException"></exception>
		public static void PublishApp( string tenantName, string application, IProcessingContext context = null )
		{
			using ( new DeferredChannelMessageContext( ) )
			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				if ( context == null )
				{
					context = new ProcessingContext( );
				}

				if ( context.Report.Action == AppLibraryAction.Unknown )
				{
					context.Report.Action = AppLibraryAction.Publish;
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", GetApplicationName( application, tenantName ) ) );
				}

				context.Report.StartTime = DateTime.Now;

				long tenantId = -1;
				long solutionId = -1;
                Guid? prevPackageId = null; // If the application originally came from the app library, the package ID of that originating package.
                Guid? newApplicationVersionId = Guid.NewGuid();

                try
				{
					using ( DatabaseContext ctx = DatabaseContext.GetContext( true, DefaultTimeout, DefaultTimeout ) )
					{
						tenantId = TenantHelper.GetTenantId( tenantName, true );

						SolutionDetails details;

						Guid inSolutionUpgradeId;
						Guid solutionUpgradeId;

                        /////
                        // Generate a new package ID for the solution in the tenant
                        /////
						using ( new TenantAdministratorContext( tenantId ) )
						{
							Guid appGuid;

							Solution solution = Guid.TryParse( application, out appGuid ) ? SystemHelper.GetApplicationByGuid( appGuid ) : Entity.GetByName<Solution>( application, true, Solution.PackageId_Field, Solution.SolutionVersionString_Field, Solution.Name_Field, Solution.Description_Field, Solution.SolutionPublisher_Field, Solution.SolutionPublisherUrl_Field, Solution.SolutionReleaseDate_Field, Solution.SolutionVersionString_Field ).FirstOrDefault( );

							if ( solution == null )
							{
								throw new InvalidOperationException( @"Failed to locate a solution with the specified name." );
							}

							if ( solution.PackageId == null )
							{
								throw new InvalidOperationException( "Invalid Application Version Id." );
							}

							details = new SolutionDetails( solution );
							solutionId = solution.Id;

                            prevPackageId = solution.PackageId;
							long inSolutionId = WellKnownAliases.CurrentTenant.InSolution;
							inSolutionUpgradeId = Entity.GetUpgradeId( inSolutionId );
							solutionUpgradeId = Entity.GetUpgradeId( solutionId );

							solution.PackageId = newApplicationVersionId;
							solution.Save( );
						}

						/////
						// Create the new package and link it to the application in the global tenant
						/////
						AppPackage existingPackage = details.ExistingPackage;

						AppPackage newPackage;

						if ( existingPackage == null )
						{
							newPackage = new AppPackage( );

							App app = null;

							if ( details.UpgradeId != Guid.Empty )
							{
								app = Entity.GetByField<App>( details.UpgradeId.ToString( ), new EntityRef( "core:applicationId" ) ).FirstOrDefault( );
							}

							if ( app == null )
							{
								app = new App
								{
									Name = details.Name,
									Description = details.Description,
									ApplicationId = details.UpgradeId,
									Publisher = details.Publisher,
									PublisherUrl = details.PublisherUrl,
									ReleaseDate = details.ReleaseDate
								};

								app.Save( );
							}

							newPackage.PackageForApplication = app;
						}
						else
						{
							if ( existingPackage.PackageForApplication != null && existingPackage.PackageForApplication.Name != details.Name )
							{
								var app = existingPackage.PackageForApplication.AsWritable<App>( );
								app.Name = details.Name;
								app.Save( );
							}

							newPackage = existingPackage.Clone<AppPackage>( CloneOption.Deep );
						}

						string versionString = details.Version.ToString( );

						newPackage.AppVerId = newApplicationVersionId;
						newPackage.AppVersionString = versionString;
						newPackage.Name = $"{details.Name} Application Package {versionString}";
						newPackage.Description = $"Application Package for version {details.Name} of {versionString}.";
						newPackage.PublishDate = DateTime.UtcNow;
						newPackage.AppPackageReleaseDate = details.ReleaseDate;

						if ( details.Dependencies != null && details.Dependencies.Count > 0 )
						{
							EntityCollection<AppPackageDependency> dependencies = new EntityCollection<AppPackageDependency>( );

							foreach ( SolutionDependency dependency in details.Dependencies )
							{
								AppPackageDependency appPackageDependency = new AppPackageDependency
								{
									Name = dependency.Name,
									AppPackageDependencyName = dependency.DependencyName,
									AppPackageDependencyId = dependency.DependencyApplication,
									AppPackageIsRequired = dependency.IsRequired
								};

								if ( dependency.MinimumVersion != null )
								{
									appPackageDependency.AppPackageMinimumVersion = dependency.MinimumVersion.ToString( );
								}

								if ( dependency.MaximumVersion != null )
								{
									appPackageDependency.AppPackageMaximumVersion = dependency.MaximumVersion.ToString( );
								}

								dependencies.Add( appPackageDependency );
							}

							newPackage.DependentAppPackageDetails = dependencies;
						}

						newPackage.Save( );

						var relationshipRestrictions = new List<RelationshipRestriction>
						{
							new RelationshipRestriction( entry =>
							{
								if ( entry.TypeId == inSolutionUpgradeId )
								{
									if ( entry.ToId != solutionUpgradeId )
									{
										return false;
									}
								}

								return true;
							} )
						};

                        /////
                        // Perform the publish
                        /////
                        using ( var source = new TenantAppSource
						{
							SolutionId = solutionId,
							TenantId = tenantId,
							RelationshipRestrictions = relationshipRestrictions,
                            OriginalPackageId = prevPackageId
                        } )
						{
							using ( var target = new LibraryAppTarget
							{
                                ApplicationVersionId = newApplicationVersionId.Value
                            } )
							{
								var processor = new CopyProcessor( source, target, context );
								processor.MigrateData( );

								target.Commit( );
							}
						}

						ctx.CommitTransaction( );
					}

					using ( new TenantAdministratorContext( tenantId ) )
					{
						Entity.FireEvent( EntityEvent.OnAfterPublish, new EntityRef( solutionId ).ToEnumerable( ), ( Action<IEntityEventPublish, IEnumerable<IEntity>> ) FireOnAfterPublishEvent, er => er.Entity );
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );

					context.Report.Exception = exc;

					if ( tenantId > 0 &&
						solutionId > 0 )
					{
						using ( new TenantAdministratorContext( tenantId ) )
						{
							Entity.FireEvent( EntityEvent.OnPublishFailed, new EntityRef( solutionId ).ToEnumerable( ), ( Action<IEntityEventPublish, IEnumerable<IEntity>> ) FireOnPublishFailedEvent, er => er.Entity );
						}
					}

					throw new ApplicationPublishException( exc );
				}
				finally
				{
					ForeignKeyHelper.Trust( );

					context.Report.EndTime = DateTime.Now;

                    GenerateLog(context, newApplicationVersionId, tenantId);
                    GenerateReport( context );
				}
			}
		}

		/// <summary>
		///     Repairs the application.
		/// </summary>
		/// <param name="tenantId">The tenant unique identifier.</param>
		/// <param name="appVerId">The application version unique identifier.</param>
		/// <param name="context">The context.</param>
		public static void RepairApp( long tenantId, Guid appVerId, IProcessingContext context = null )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Repair;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant Id", tenantId.ToString( CultureInfo.InvariantCulture ) ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application Version Id", appVerId.ToString( "B" ) ) );
			}

			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage package = SystemHelper.GetPackageByVerId( appVerId );

				SchedulingHelper.PauseAndRestartTenantJobs( tenantId, ( ) =>
				 {
					 RepairApp_Imp( package, tenantId, context );
				 } );
			}
		}

		/// <summary>
		///     Repairs the application.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="application">The name|guid of the application being repaired.</param>
		/// <param name="context">The context.</param>
		public static void RepairApp( string tenantName, string application, IProcessingContext context = null )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Repair;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", GetApplicationName( application, tenantName ) ) );
			}

			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				long tenantId = TenantHelper.GetTenantId( tenantName, true );

				Guid appGuid;

				/////
				// Get the tenants version of the application
				/////
				AppPackage package = Guid.TryParse( application, out appGuid ) ? SystemHelper.GetTenantCurrentPackageByGuid( tenantId, appGuid ) : SystemHelper.GetTenantCurrentPackageByName( tenantId, application );

				SchedulingHelper.PauseAndRestartTenantJobs( tenantId, ( ) =>
				 {
					 RepairApp_Imp( package, tenantId, context );
				 } );
			}
		}

		/// <summary>
		///     Repairs the app_ implementation.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="tenantId">The tenant unique identifier.</param>
		/// <param name="context">The context.</param>
		private static void RepairApp_Imp( AppPackage package, long tenantId, IProcessingContext context )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Repair;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application Package Id", package == null ? "Unknown" : package.AppVerId?.ToString( "B" ) ?? "Unknown" ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant Id", tenantId.ToString( "B" ) ) );
			}

			context.Report.StartTime = DateTime.Now;

            Guid? appVerId = null;

            try
			{
				if ( package == null )
				{
					throw new ArgumentNullException( nameof( package ) );
				}

				if ( package.AppVerId == null )
				{
					throw new InvalidOperationException( "Invalid package AppVerId." );
				}

                appVerId = package.AppVerId;

                if ( package.PackageForApplication == null )
				{
					throw new InvalidOperationException( "Invalid package application." );
				}

				if ( package.PackageForApplication.ApplicationId == null )
				{
					throw new InvalidOperationException( "Invalid package application id." );
				}


				using ( var source = new LibraryAppSource
				{
					AppVerId = package.AppVerId.Value
				} )
				{
					IDataSource existing = new EmptySource( );

					/////
					// Create target to write to SQLite database
					/////
					using ( var target = new TenantRepairTarget
					{
						TenantId = tenantId,
						ApplicationId = package.PackageForApplication.ApplicationId.Value
					} )
					{
						/////
						// Copy the data
						/////
						using ( var processor = new MergeProcessor( context )
						{
							OldVersion = existing,
							NewVersion = source,
							Target = target
						} )
						{
							processor.MergeData( );

							target.Commit( );
						}
					}

					/////
					// Dispose of the existing data source.
					/////
					existing.Dispose( );
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( exc.ToString( ) );

				context.Report.Exception = exc;

				throw new ApplicationDeployException( exc );
			}
			finally
			{
				context.Report.EndTime = DateTime.Now;

				ForeignKeyHelper.Trust( );

                GenerateLog(context, appVerId, tenantId);
                GenerateReport( context );
			}
		}

		/// <summary>
		///     Stages the application.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="application">The name|guid of the application being staged.</param>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">
		///     @Failed to locate a solution with the specified name.
		///     or
		///     Invalid Application Version Id.
		/// </exception>
		/// <exception cref="ApplicationPublishException"></exception>
		public static StatisticsReport StageApp( string tenantName, string application, IProcessingContext context = null )
		{
            long tenantId = 0;

            using ( new GlobalAdministratorContext( ) )
			{
				if ( context == null )
				{
					context = new StagingContext( );
				}

				if ( context.Report.Action == AppLibraryAction.Unknown )
				{
					context.Report.Action = AppLibraryAction.Stage;
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", GetApplicationName( application, tenantName ) ) );
				}

				context.Report.StartTime = DateTime.Now;

                Guid? packageId = null;

                try
				{
					tenantId = TenantHelper.GetTenantId( tenantName, true );

					long solutionId;
					Guid appGuid;

					using ( new TenantAdministratorContext( tenantId ) )
					{
						Solution solution = Guid.TryParse( application, out appGuid ) ? SystemHelper.GetApplicationByGuid( appGuid ) : Entity.GetByName<Solution>( application, true, Solution.PackageId_Field, Solution.SolutionVersionString_Field, Solution.Name_Field, Solution.Description_Field, Solution.SolutionPublisher_Field, Solution.SolutionPublisherUrl_Field, Solution.SolutionReleaseDate_Field, Solution.SolutionVersionString_Field ).FirstOrDefault( );

						if ( solution == null )
						{
							throw new InvalidOperationException( @"Failed to locate a solution with the specified name." );
						}

						solutionId = solution.Id;
					}

					IDataSource previousSource;

					AppPackage package = appGuid != Guid.Empty ? SystemHelper.GetLatestPackageByGuid( appGuid ) : SystemHelper.GetLatestPackageByName( application );

					if ( package?.AppVerId != null )
					{
						previousSource = new LibraryAppStagingSource
						{
							AppVerId = package.AppVerId.Value
						};

                        packageId = package.AppVerId;
                    }
					else
					{
						previousSource = new EmptySource( );
					}

					using ( var source = new TenantAppStagingSource
					{
						SolutionId = solutionId,
						TenantId = tenantId
					} )
					{
						var processor = new StagingProcessor( context )
						{
							NewVersion = source,
							OldVersion = previousSource
						};
						processor.StageData( );
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );

					context.Report.Exception = exc;

					throw new ApplicationPublishException( exc );
				}
				finally
				{
					context.Report.EndTime = DateTime.Now;

                    GenerateLog(context, packageId, tenantId);
                    GenerateReport( context );
				}
			}

			return context.Report;
		}

		/// <summary>
		/// Transforms the configuration files into a SqLite application package.
		/// </summary>
		/// <param name="xmlPath">Path to the application configuration files.</param>
		/// <param name="packagePath">Path to where the resulting application package will reside.</param>
		/// <param name="sourceHashFile">The source hash file.</param>
		/// <param name="destinationHashFile">The destination hash file.</param>
		/// <param name="version">The version string to be embedded into the application package.</param>
		/// <param name="context">The processing context.</param>
		/// <exception cref="System.ArgumentNullException">xmlPath</exception>
		/// <exception cref="System.ArgumentException">@The specified xmlPath does not exist.;xmlPath</exception>
		/// <exception cref="System.InvalidOperationException">Specified solution xml files does not contain a hash value.</exception>
		public static void TransformConfig( string xmlPath, string packagePath, string sourceHashFile, string destinationHashFile, string version, IProcessingContext context = null )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Transform;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Xml Path", xmlPath ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Package Path", packagePath ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Source Hash file", sourceHashFile ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Destination Hash file", destinationHashFile ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Version", version ) );
			}

			context.Report.StartTime = DateTime.Now;

			try
			{
				using ( new GlobalAdministratorContext( ) )
				{
					if ( string.IsNullOrEmpty( xmlPath ) )
					{
						throw new ArgumentNullException( nameof( xmlPath ) );
					}

					if ( string.IsNullOrEmpty( packagePath ) )
					{
						throw new ArgumentNullException( nameof( packagePath ) );
					}

					if ( string.IsNullOrEmpty( sourceHashFile ) )
					{
						throw new ArgumentNullException( nameof( sourceHashFile ) );
					}

					if ( string.IsNullOrEmpty( destinationHashFile ) )
					{
						throw new ArgumentNullException( nameof( destinationHashFile ) );
					}

					if ( !File.Exists( xmlPath ) )
					{
						throw new ArgumentException( @"The specified xmlPath does not exist.", nameof( xmlPath ) );
					}

					/////
					// If either the output package does not exist or the hash file does not exist, proceed with the transform.
					/////
					if ( !Directory.Exists( packagePath ) )
					{
						Directory.CreateDirectory( packagePath );
					}

					string sourceHash = null;
					string destinationHash = null;

					bool sourceHashFileExists = File.Exists( sourceHashFile );

					if ( sourceHashFileExists )
					{
						sourceHash = File.ReadAllText( sourceHashFile ) + " " + version;
					}

					bool destinationHashFileExists = File.Exists( destinationHashFile );

					if ( destinationHashFileExists )
					{
						destinationHash = File.ReadAllText( destinationHashFile );
					}

					bool hashMatch = false;

					if ( sourceHashFileExists && destinationHashFileExists )
					{
						hashMatch = string.Equals( sourceHash, destinationHash );
					}

					if ( !hashMatch || Debugger.IsAttached )
					{
						using ( var source = new XmlSource
						{
							SolutionPath = xmlPath
						} )
						{
							/////
							// Set the solution versions to be the specified version string.
							/////
							source.SetSolutionVersions( version );

							foreach ( string availableSolution in source.AvailableSolutions )
							{
								try
								{
									source.ActiveSolution = availableSolution;

									string fileName = availableSolution.Split( ':' ).Last( ) + ".xml";

									using ( var target = FileManager.CreateDataTarget( Format.Xml, Path.Combine( packagePath, fileName ), version ) )
									{
										var processor = new CopyProcessor( source, target, context );
										processor.MigrateData( );
									}
								}
								catch ( Exception ex )
								{
									Console.Error.WriteLine( availableSolution + ": " + ex );
									throw;
								}
							}

							if ( sourceHash != null )
							{
								File.WriteAllText( destinationHashFile, sourceHash );
							}
						}
					}

					context.Report.EndTime = DateTime.Now;
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( exc.ToString( ) );

				context.Report.Exception = exc;

				throw;
			}
			finally
			{
				context.Report.EndTime = DateTime.Now;

                GenerateLog(context, null);
                GenerateReport( context );
			}
		}

		/// <summary>
		///		Upgrades the tenants version of the specified application to the latest.
		/// </summary>
		/// <param name="tenantId">The tenant to upgrade.</param>
		/// <param name="appVerId">Identifier of the application to be upgraded.</param>
		/// <param name="context">The processing context.</param>
		/// <param name="updateStats">if set to <c>true</c> [update stats].</param>
		/// <param name="disableFts">if set to <c>true</c> [disable FTS].</param>
		public static void UpgradeApp( long tenantId, Guid appVerId, IProcessingContext context = null, bool updateStats = true, bool disableFts = true )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Upgrade;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant Id", tenantId.ToString( CultureInfo.InvariantCulture ) ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application Version Id", appVerId.ToString( "B" ) ) );
			}

			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage package = SystemHelper.GetPackageByVerId( appVerId );
				AppPackage existingPackage = null;

				if ( package != null )
				{
					App app = package.PackageForApplication;
					existingPackage = SystemHelper.GetTenantCurrentPackageByName( tenantId, app.Name );
				}

				SchedulingHelper.PauseAndRestartTenantJobs( tenantId, ( ) =>
				 {
					 UpgradeApp_Impl( package, existingPackage, tenantId, context, updateStats, disableFts );
				 } );
			}
		}

		/// <summary>
		/// Upgrades the application.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="application">The name|guid of the application being upgraded.</param>
		/// <param name="appVersion">The application version.</param>
		/// <param name="context">The context.</param>
		/// <param name="updateStats">if set to <c>true</c> [update stats].</param>
		/// <param name="disableFts">if set to <c>true</c> [disable FTS].</param>
		/// <param name="installDependencies">if set to <c>true</c> [install dependencies].</param>
		/// <exception cref="ApplicationException">
		/// Could not load package: {application}
		/// or
		/// Could not load package: {application} version: {appVersion}"
		/// </exception>
		/// <exception cref="System.ApplicationException"></exception>
		public static void UpgradeApp( string tenantName, string application, string appVersion = null, IProcessingContext context = null, bool updateStats = true, bool disableFts = true, bool installDependencies = true )
		{
			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			string applicationName = GetApplicationName( application, tenantName );

			if ( context.Report.Action == AppLibraryAction.Unknown )
			{
				context.Report.Action = AppLibraryAction.Upgrade;
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant", tenantName ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application", applicationName ) );
				context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application Version", appVersion ) );
			}

			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				long tenantId = TenantHelper.GetTenantId( tenantName );

				if ( tenantId < 0 )
				{
					return;
				}

				/////
				// Create source to load app data from tenant
				/////
				AppPackage package;

				Guid appGuid;

				Guid.TryParse( application, out appGuid );

				if ( string.IsNullOrEmpty( appVersion ) )
				{
					package = appGuid != Guid.Empty ? SystemHelper.GetLatestPackageByGuid( appGuid ) : SystemHelper.GetLatestPackageByName( application );

					if ( package == null )
					{
						throw new ApplicationException( $"Could not load package: {application}" );
					}
				}
				else
				{
					package = appGuid != Guid.Empty ? SystemHelper.GetPackageByGuidAndVersion( appGuid, appVersion ) : SystemHelper.GetPackageByNameAndVersion( application, appVersion );

					if ( package == null )
					{
						throw new ApplicationException( $"Could not load package: {application} version: {appVersion}" );
					}
				}

				AppPackage existingPackage = appGuid != Guid.Empty ? SystemHelper.GetTenantCurrentPackageByGuid( tenantId, appGuid ) : SystemHelper.GetTenantCurrentPackageByName( tenantId, application );

				Guid? sourceAppVerId = package.AppVerId;
				Guid? targetAppVerId = existingPackage?.AppVerId;

				if ( sourceAppVerId != null && targetAppVerId != null && sourceAppVerId.Value == targetAppVerId.Value )
				{
					Console.WriteLine( $@"Tenant '{tenantName}' already contains application '{applicationName}' with package id '{sourceAppVerId.Value:B}'." );
					return;
				}

				SchedulingHelper.PauseAndRestartTenantJobs( tenantId, ( ) =>
				 {
					 UpgradeApp_Impl( package, existingPackage, tenantId, context, updateStats, disableFts, installDependencies );
				 } );

			}
		}

		/// <summary>
		/// Upgrades the existing application package to the specified application package for the specified tenant.
		/// </summary>
		/// <param name="package">The package version being upgraded to.</param>
		/// <param name="existingPackage">The package version to be upgraded.</param>
		/// <param name="tenantId">The tenant whose application is being upgraded.</param>
		/// <param name="context">The processing context.</param>
		/// <param name="updateStats">if set to <c>true</c> [update stats].</param>
		/// <param name="disableFts">if set to <c>true</c> [disable FTS].</param>
		/// <param name="installDependencies">if set to <c>true</c> [install dependencies].</param>
		/// <exception cref="ArgumentNullException">package</exception>
		/// <exception cref="InvalidOperationException">Invalid AppVerId
		/// or
		/// Invalid AppVerId</exception>
		/// <exception cref="ApplicationDeployException"></exception>
		private static void UpgradeApp_Impl( AppPackage package, AppPackage existingPackage, long tenantId, IProcessingContext context, bool updateStats = true, bool disableFts = true, bool installDependencies = true )
		{
			using ( new DeferredChannelMessageContext( ) )
			using ( new SecurityBypassContext( ) )
			using ( new GlobalAdministratorContext( ) )
			{
				if ( context == null )
				{
					context = new ProcessingContext( );
				}

				if ( context.Report.Action == AppLibraryAction.Unknown )
				{
					context.Report.Action = AppLibraryAction.Upgrade;
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Application Package Id", package == null ? "Unknown" : package.AppVerId?.ToString( "B" ) ?? "Unknown" ) );
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Existing Application Package Id", existingPackage == null ? "Unknown" : existingPackage.AppVerId?.ToString( "B" ) ?? "Unknown" ) );
					context.Report.Arguments.Add( new KeyValuePair<string, string>( "Tenant Id", tenantId.ToString( CultureInfo.InvariantCulture ) ) );
				}

				context.Report.StartTime = DateTime.Now;

				int fullTextSearchChangeTrackingState = -1;

                Guid? newPackageId = null;

                try
				{
					long newSolutionEntityId = -1;

					if ( package == null )
					{
						throw new ArgumentNullException( nameof( package ) );
					}

					if ( package.AppVerId == null )
					{
						throw new InvalidOperationException( "Invalid package AppVerId." );
					}

                    newPackageId = package.AppVerId;

                    if ( package.PackageForApplication == null )
					{
						throw new InvalidOperationException( "Invalid package application." );
					}

					if ( package.PackageForApplication.ApplicationId == null )
					{
						throw new InvalidOperationException( "Invalid package application id." );
					}

					IList<DependencyFailure> failures = GetMissingDependencies( package, tenantId, context );

					if ( failures != null && failures.Count > 0 )
					{
						if ( installDependencies )
						{
							// Figure out the app package for all immediate parents then call UpgradeApp passing those in.
							foreach ( DependencyFailure failure in failures )
							{
								if ( failure.Reason == DependencyFailureReason.NotInstalled )
								{
									throw new ApplicationDependencyException( $"The required dependency application '{failure.DependencyName}' could not be found in the application library." );
								}

								if ( failure.Reason == DependencyFailureReason.BelowMinVersion )
								{
									SolutionHelper.EnsureUpgradePath( tenantId, failure );

									if ( failure.Reason == DependencyFailureReason.NoUpgradePathAvailable )
									{
										throw new ApplicationDependencyException( $"The required version of dependency application '{failure.DependencyName}' could not be found in the application library." );
									}

									if ( failure.Reason == DependencyFailureReason.IncompatibleUpgradePath )
									{
										if ( !string.IsNullOrEmpty( failure.DependentName ) )
										{
											throw new ApplicationDependencyException( $"Upgrading the selected application requires the application '{failure.DependencyName}' to also be upgraded however the installed version of '{failure.DependentName}' is incompatible with that version of '{failure.DependencyName}'." );
										}

										throw new ApplicationDependencyException( $"Upgrading the selected application requires the application '{failure.DependencyName}' to also be upgraded however this will cause compatibility issues with other installed applications." );
									}
								}

								long dependencyPackageId = SystemHelper.GetPackageIdByGuid( failure.ApplicationId, failure.MinVersion, failure.MaxVersion );

								if ( dependencyPackageId >= 0 )
								{
									AppPackage dependencyPackage = Entity.Get<AppPackage>( dependencyPackageId );
									AppPackage existingDependencyPackage = SystemHelper.GetTenantCurrentPackageByGuid( tenantId, failure.ApplicationId );

									UpgradeApp_Impl( dependencyPackage, existingDependencyPackage, tenantId, context, updateStats, disableFts );
								}
								else
								{
									StringBuilder errorMessage = new StringBuilder( $"Application {package.PackageForApplication.Name} {package.AppVersionString} requires application {failure.DependencyName}, Version {( failure.MinVersion == null ? "Any" : failure.MinVersion.ToString( 4 ) )} to {( failure.MaxVersion == null ? "Any" : failure.MaxVersion.ToString( 4 ) )} to be installed but no appropriate package can be found." );

									throw new ApplicationDependencyException( errorMessage.ToString( ) );
								}
							}
						}
						else
						{
							StringBuilder errorMessage = new StringBuilder( $"Application {package.PackageForApplication.Name} {package.AppVersionString} requires the following applications to be present:\n" );

							foreach ( DependencyFailure failure in failures )
							{
								errorMessage.AppendLine( $" - {failure.DependencyName}, Version {(failure.MinVersion == null ? "Any" : failure.MinVersion.ToString( 4 ))} to {( failure.MaxVersion == null ? "Any" : failure.MaxVersion.ToString( 4 ) )}" );
							}

							throw new ApplicationDependencyException( errorMessage.ToString( ) );
						}
					}

					if ( disableFts )
					{
						fullTextSearchChangeTrackingState = DisableFullTextSearchChangeTracking( );
					}

					var eventTargetState = new Dictionary<string, object>( );

					using ( var source = new LibraryAppSource
					{
						AppVerId = package.AppVerId.Value
					} )
					{
						IDataSource existing;

						if ( existingPackage == null )
						{
							existing = new EmptySource( );
						}
						else
						{
							if ( existingPackage.AppVerId == null )
							{
								throw new InvalidOperationException( "Invalid AppVerId" );
							}

							var existingLibraryAppSource = new LibraryAppSource
							{
								AppVerId = existingPackage.AppVerId.Value
							};

							existing = existingLibraryAppSource;

							// Fire off the event
							if ( existingPackage.PackageForApplication.ApplicationId != null )
							{
								var appId = ( Guid ) existingPackage.PackageForApplication.ApplicationId;

								using ( new TenantAdministratorContext( tenantId ) )
								{
									var appInTentantId = Entity.GetIdFromUpgradeId( appId );

									if ( appInTentantId == -1 )
										throw new InvalidOperationException( "The upgrade Id for the solution should exist within the tenant" );

									Entity.FireEvent( EntityEvent.OnBeforeUpgrade, new EntityRef( appInTentantId ).ToEnumerable( ), FireOnBeforeUpgradeEventFn( eventTargetState ), er => er.Entity );
								}
							}
						}



						/////
						// Create target to write to SQLite database
						/////
						using ( var target = new TenantMergeTarget
						{
							TenantId = tenantId,
							ApplicationId = package.PackageForApplication.ApplicationId.Value
						} )
						{
							var contextInfo = DatabaseContextInfo.GetContextInfo( );

							if ( contextInfo != null )
							{
								long transactionId = DatabaseChangeTracking.CreateRestorePoint( DatabaseContextInfo.GetMessageChain( 0 ), tenantId );

								contextInfo.TransactionId = transactionId;
							}

							/////
							// Copy the data
							/////
							using ( CallData<Guid>.SetValue( "ExistingAppVerUid", existingPackage?.AppVerId ?? Guid.Empty ) )
							using ( CallData<Guid>.SetValue( "NewAppVerUid", source.AppVerId ) )
							using ( CallData<long>.SetValue( "TargetTenantId", tenantId ) )
							using ( var processor = new MergeProcessor( context )
							{
								OldVersion = existing,
								NewVersion = source,
								Target = target,
							} )
							{
								processor.MergeData( );

								Guid packageId = Guid.Empty;

								string solutionName = package.PackageForApplication.Name;

								if ( package.AppVerId != null )
								{
									packageId = package.AppVerId.Value;
								}

								using ( DatabaseContextInfo.SetContextInfo( "Upgrade app package id" ) )
								using ( IDbCommand command = target.CreateCommand( ) )
								{
									command.CommandText = @"
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @solution BIGINT = dbo.fnAliasNsId( 'solution', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @package BIGINT = dbo.fnAliasNsId( 'packageId', 'core', @tenantId )

DECLARE @output TABLE
(
	EntityId BIGINT
)

IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END

UPDATE g
SET g.Data = @packageId
OUTPUT inserted.EntityId
INTO @output
FROM Data_Guid g
JOIN Relationship r ON g.TenantId = r.TenantId AND g.EntityId = r.FromId AND r.TypeId = @isOfType AND r.ToId = @solution
JOIN Data_NVarChar n ON n.TenantId = r.TenantId AND r.FromId = n.EntityId AND n.Data_StartsWith = @appName AND n.FieldId = @name
WHERE g.TenantId = @tenantId AND g.FieldId = @package

SELECT EntityId FROM @output";

									long userId;
									RequestContext.TryGetUserId( out userId );

									command.AddParameterWithValue( "@packageId", packageId );
									command.AddParameterWithValue( "@appName", solutionName );
									command.AddParameterWithValue( "@tenantId", tenantId );
									command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

									object solutionId = command.ExecuteScalar( );

									if ( solutionId != null && solutionId != DBNull.Value )
									{
										newSolutionEntityId = ( long ) solutionId;
									}

									if ( updateStats )
									{
										try
										{
											command.CommandText = "EXEC msdb.dbo.sp_start_job @job_name = N'SoftwarePlatform Nightly Statistics Update'";
											command.ExecuteNonQuery( );
										}
										catch ( Exception exc )
										{
											/////
											// Job already running.
											/////
											if ( exc.HResult != -2146232060 )
											{
												EventLog.Application.WriteError( "Failed to update database statistics. {0}", exc.Message );
											}
										}
									}
								}

								target.Commit( );
							}
						}

						/////
						// Dispose of the existing data source.
						/////
						existing.Dispose( );
					}

					if ( newSolutionEntityId != -1 )
					{
						using ( new TenantAdministratorContext( tenantId ) )
						{
							if ( existingPackage != null ) // Is upgrade ?
							{
								Entity.FireEvent( EntityEvent.OnAfterUpgrade, new EntityRef( newSolutionEntityId ).ToEnumerable( ), FireOnAfterUpgradeEventFn( eventTargetState ), er => er.Entity );
							}
							else
							{
								Entity.FireEvent( EntityEvent.OnAfterDeploy, new EntityRef( newSolutionEntityId ).ToEnumerable( ), FireOnAfterDeployEventFn( eventTargetState ), er => er.Entity );
							}
						}
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );

					context.Report.Exception = exc;

					if ( tenantId > 0 )
					{
						string solutionName = "<Unknown>";
						string version = "<Unknown>";

						if ( package != null )
						{
							if ( package.PackageForApplication != null )
							{
								solutionName = package.PackageForApplication.Name;
							}

							version = package.AppVersionString;
						}

						using ( new TenantAdministratorContext( tenantId ) )
						{
							// At this point the solution is not available for the current tenant so use the stored solution details.
							var solutionsGroupedByType = new Dictionary<EntityType, ISet<SolutionIdentityDetails>>( );
							var solutionDetailsSet = new HashSet<SolutionIdentityDetails> { new SolutionIdentityDetails { Name = solutionName, Version = version } };
							solutionsGroupedByType.Add( Entity.Get<EntityType>( "solution" ), solutionDetailsSet );

							Entity.FireEventByGroupedIds( EntityEvent.OnDeployFailed, solutionsGroupedByType, ( Action<IEntityEventDeploy, IEnumerable<ISolutionDetails>> ) FireOnDeployFailedEvent );
						}
					}

					throw new ApplicationDeployException( exc );
				}
				finally
				{
					if ( disableFts )
					{
						RestoreFullTextSearchChangeTracking( fullTextSearchChangeTrackingState );
					}

					ForeignKeyHelper.Trust( );

					context.Report.EndTime = DateTime.Now;

                    GenerateLog(context, newPackageId, tenantId);
                    GenerateReport( context );
				}
			}
		}

	    /// <summary>
		/// Gets the missing dependencies.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		private static IList<DependencyFailure> GetMissingDependencies( AppPackage package, long tenantId, IProcessingContext context )
		{
			if ( package == null )
			{
				throw new ArgumentNullException( nameof( package ) );
			}

			if ( context == null )
			{
				throw new ArgumentNullException( nameof( context ) );
			}

			return package.GetMissingDependencies( tenantId, true );
		}

		/// <summary>
		/// Restores the full text search change tracking.
		/// </summary>
		/// <param name="fullTextSearchChangeTrackingState">Full state of the text search change tracking.</param>
		private static void RestoreFullTextSearchChangeTracking( int fullTextSearchChangeTrackingState )
		{
			if ( fullTextSearchChangeTrackingState > 0 )
			{
				try
				{
					using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
					{
						using ( IDbCommand command = ctx.CreateCommand( ) )
						{
							command.CommandText = fullTextSearchChangeTrackingState == 1 ? "ALTER FULLTEXT INDEX ON Data_NVarChar SET CHANGE_TRACKING MANUAL" : "ALTER FULLTEXT INDEX ON Data_NVarChar SET CHANGE_TRACKING AUTO";

							command.ExecuteNonQuery( );
						}
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteWarning( "Failed to re-enable full text search during application deployment. This is a performance optimization any nothing to be worried about. " + exc );
				}
			}
		}

		/// <summary>
		/// Disables the full text search change tracking.
		/// </summary>
		/// <returns></returns>
		private static int DisableFullTextSearchChangeTracking( )
		{
			int state = -1;

			try
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					/////
					// Cannot modify the full text index from within a transaction.
					/////
					if ( !ctx.TransactionActive )
					{
						using ( IDbCommand command = ctx.CreateCommand( "SELECT ISNULL( OBJECTPROPERTY( OBJECT_ID( 'Data_NVarChar' ), 'TableFullTextBackgroundUpdateIndexon' ), 0 ) + ISNULL( OBJECTPROPERTY( OBJECT_ID( 'Data_NVarChar' ), 'TableFullTextChangeTrackingon'), 0 )" ) )
						{
							state = ( int ) command.ExecuteScalar( );

							if ( state > 0 )
							{
								command.CommandText = "ALTER FULLTEXT INDEX ON Data_NVarChar SET CHANGE_TRACKING OFF";

								command.ExecuteNonQuery( );
							}
						}
					}
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteWarning( "Failed to disable full text search during application deployment. This is a performance optimization any nothing to be worried about. " + exc );
			}

			return state;
		}
	}
}