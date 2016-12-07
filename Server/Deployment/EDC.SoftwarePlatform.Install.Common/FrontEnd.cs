// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml;
using EDC.Database;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging.Redis;
using EDC.ReadiNow.Metadata.Solutions;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.SoftwarePlatform.Migration;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.Xml;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     The front end class.
	/// </summary>
	public class FrontEnd
	{
		/// <summary>
		///     The front end root path
		/// </summary>
		private string _frontEndRootPath;

		/// <summary>
		///     The front-end mode.
		/// </summary>
		private FrontEndMode _mode = FrontEndMode.Unknown;

		/// <summary>
		///     The software platform configuration path
		/// </summary>
		private string _softwarePlatformConfigPath;

		/// <summary>
		///     Initializes a new instance of the <see cref="FrontEnd" /> class.
		/// </summary>
		/// <param name="parser">The parser.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FrontEnd( IFrontEndArgumentParser parser )
		{
			if ( parser == null )
			{
				throw new ArgumentNullException( nameof( parser ) );
			}

			ArgumentParser = parser;

			//if ( !string.IsNullOrEmpty( ArgumentParser.LogPath ) )
			//{
			//	_fileLogger = new FileLogger( ArgumentParser.LogPath );
			//}

			if ( !Console.IsOutputRedirected )
			{
				/////
				// Disable wordwrap.
				/////
				Console.BufferWidth = 500;
			}
		}

		/// <summary>
		///     Gets the argument parser.
		/// </summary>
		/// <value>
		///     The argument parser.
		/// </value>
		private IFrontEndArgumentParser ArgumentParser
		{
			get;
		}

		/// <summary>
		///     Gets the front end root path.
		/// </summary>
		/// <returns></returns>
		private string FrontEndRootPath
		{
			get
			{
				if ( _frontEndRootPath == null )
				{
					string path = ArgumentParser.Path;

					if ( string.IsNullOrEmpty( path ) )
					{
						path = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );

						if ( string.IsNullOrEmpty( path ) )
						{
							throw new FrontEndException( "Failed to determine assembly directory." );
						}

						DirectoryInfo directory = new DirectoryInfo( path );

						while ( !Directory.Exists( Path.Combine( directory.FullName, "Applications" ) ) )
						{
							directory = directory.Parent;

							if ( directory == null )
							{
								break;
							}
						}

						if ( directory == null )
						{
							throw new FrontEndException( $"No 'Applications' folder found at path '{path}'." );
						}

						path = directory.FullName;

						if ( string.IsNullOrEmpty( path ) )
						{
							throw new FrontEndException( "Failed to determine front end directory." );
						}
					}

					_frontEndRootPath = path;
				}

				return _frontEndRootPath;
			}
		}

		/// <summary>
		///     Gets the software platform configuration path.
		/// </summary>
		/// <returns></returns>
		private string SoftwarePlatformConfigPath
		{
			get
			{
				if ( _softwarePlatformConfigPath == null )
				{
					string rootPath = FrontEndRootPath;

					string path = Path.Combine( rootPath, @"Configuration\SoftwarePlatform.config" );

					if ( _mode == FrontEndMode.Activate )
					{
						string symbolicLinkPath = GetPath( ArgumentParser.ConfigSymbolicLinkPath );

						if ( !string.IsNullOrEmpty( symbolicLinkPath ) )
						{
							if ( File.Exists( symbolicLinkPath ) )
							{
								WriteLine( $"Deleting '{symbolicLinkPath}'..." );
								File.Delete( symbolicLinkPath );
								WriteLine( $"File '{symbolicLinkPath}' successfully deleted." );
							}

							try
							{
								SymbolicLink.CreateFileLink( symbolicLinkPath, path );

								WriteLine( $"Created symbolic link from '{symbolicLinkPath}' to '{path}'." );
							}
							catch ( Exception exc )
							{
								WriteLine( $"Failed to create symbolic link from '{symbolicLinkPath}' to '{path}'. {exc}", Severity.Warning );
							}
						}

						if ( !File.Exists( path ) )
						{
							throw new FileNotFoundException( "Failed to locate configuration file.", path );
						}
					}

					_softwarePlatformConfigPath = path;
				}

				return _softwarePlatformConfigPath;
			}
		}

		/// <summary>
		///     Adds the directory security.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="account">The account.</param>
		/// <param name="rights">The rights.</param>
		private static void AddDirectorySecurity( string directory, string domain, string account, FileSystemRights rights )
		{
			try
			{
				if ( string.IsNullOrWhiteSpace( directory ) || string.IsNullOrWhiteSpace( domain ) || string.IsNullOrWhiteSpace( account ) )
				{
					return;
				}

				/////
				// Update the Acl
				/////
				var directoryInfo = new DirectoryInfo( directory );

				DirectorySecurity directorySecurity = directoryInfo.GetAccessControl( );

				directorySecurity.AddAccessRule( new FileSystemAccessRule( new NTAccount( domain, account ), rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow ) );

				directoryInfo.SetAccessControl( directorySecurity );
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( "An error occurred setting the permission for directory {0}. Error {1}.", directory, ex.ToString( ) );
			}
		}

		/// <summary>
		///     Applies the upload folder security.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="account">The account.</param>
		private void ApplyFolderSecurity( string path, string domain, string account )
		{
			/////
			// Check access rules for the specified security account
			/////
			if ( !string.IsNullOrEmpty( account ) )
			{
				string fqGroup = domain == null ? account : $"{domain}\\{account}";

				WriteLine( $"Checking security on folder '{path}' for {fqGroup}..." );

				if ( !DirectorySecurityExists( path, domain, account, FileSystemRights.FullControl ) )
				{
					AddDirectorySecurity( path, domain, account, FileSystemRights.FullControl );

					WriteLine( $"Granted Full control to {fqGroup} on folder '{path}'." );
				}
				else
				{
					WriteLine( $"Full control has already been granted to {fqGroup} on folder '{path}'." );
				}
			}
			else
			{
				WriteLine( $"No Security Group specified. Security configuration for folder '{path}' has been skipped.", Severity.Warning );
			}

			WriteLine( $"Checking security on folder '{path}' for Domain Users..." );

			/////
			// Add 'Domain Users' access rule
			/////
			if ( !DirectorySecurityExists( path, null, "Domain Users", FileSystemRights.ReadAndExecute ) )
			{
				AddDirectorySecurity( path, null, "Domain Users", FileSystemRights.ReadAndExecute );

				WriteLine( $"Granted Read/Execute to Domain Users on folder '{path}'." );
			}
			else
			{
				WriteLine( $"Read/Execute has already been granted to Domain Users on folder '{path}'." );
			}
		}

		/// <summary>
		///     Cleanups the IIS.
		/// </summary>
		private void CleanupWebServer( )
		{
			string path = ArgumentParser.VirtualDirectoryPath;
			string spVirtualPath = ArgumentParser.SpVirtualPath;
			string spApiVirtualPath = ArgumentParser.SpApiVirtualPath;

			if ( string.IsNullOrEmpty( path ) )
			{
				WriteLine( "No virtual directory path specified. Skipping cleanup of IIS.", Severity.Warning );
				return;
			}

			/////
			// Ensure the path is of the form /path
			/////
			path = $"/{path.Trim( '/' )}";

			try
			{
				WriteLine( "Connecting to IIS..." );

				using ( WebManager webManager = new WebManager( WriteLine, FrontEndRootPath ) )
				{
					if ( webManager.Connect( ) )
					{
						webManager.RemoveApplication( path, "sperrors" );
						webManager.RemoveApplication( path, spApiVirtualPath );
						webManager.RemoveApplication( path, spVirtualPath );
						webManager.RemoveAutoStartProvider( "SpApiPreload" );
						webManager.RemoveVirtualDirectory( path );
						webManager.RemoveAppPool( "SoftwarePlatformAppPool" );
					}
				}
			}
			catch ( Exception exc )
			{
				WriteLine( $"Failed to cleanup IIS.\n{exc}", Severity.Error );
			}
		}

		/// <summary>
		///     Configures the database.
		/// </summary>
		private void ConfigureDatabase( )
		{
			EnsureDatabaseExists( );

			EnsureDatabaseUserExists( );
		}


		/// <summary>
		///     Configures the log folder.
		/// </summary>
		private void ConfigureLogFolder( )
		{
			string logFolderPath = Path.Combine( FrontEndRootPath, "Log" );

			CreateLogDirectory( logFolderPath );

			ApplyFolderSecurity( logFolderPath, ArgumentParser.SecurityDomain, ArgumentParser.SecurityGroup );
		}

		/// <summary>
		///     Configures the redis.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		private void ConfigureRedis( )
		{
			string redisServer = ArgumentParser.RedisServer;
			string redisPort = ArgumentParser.RedisPort;

			if ( string.IsNullOrEmpty( redisServer ) )
			{
				WriteLine( "No redis server has been specified. Skipping.", Severity.Warning );

				return;
			}

			bool isLocalRedisServer = FrontEndHelper.IsLocalAddress( redisServer );

			if ( isLocalRedisServer )
			{
				TestLocalRedisServer( redisServer, redisPort );
			}
			else
			{
				TestRemoteRedisServer( redisServer, redisPort );
			}
		}

		/// <summary>
		///     Configures the scheduler.
		/// </summary>
		private void ConfigureScheduler( )
		{
			string serviceName = ArgumentParser.SchedulerServiceName;
			string serviceDisplayName = ArgumentParser.SchedulerServiceDisplayName;

			if ( string.IsNullOrEmpty( serviceName ) )
			{
				WriteLine( "No scheduler service name specified. Skipping scheduler installation.", Severity.Warning );
				return;
			}

			if ( ServiceManager.ServiceIsInstalled( serviceName ) )
			{
				WriteLine( $"Found existing scheduler service '{serviceName}'." );
				return;
			}

			string exePath = Path.Combine( FrontEndRootPath, "Scheduler\\SchedulerService.exe" );

			if ( !File.Exists( exePath ) )
			{
				WriteLine( $"Failed to locate scheduler binary at {exePath}. Skipping", Severity.Error );
				return;
			}

			WriteLine( $"Found scheduler binary at {exePath}." );

			string path = $"{exePath}";

			string domain = ArgumentParser.RedisDomain;
			string user = ArgumentParser.RedisUser;
			string password = ArgumentParser.RedisPassword;

			string username = !string.IsNullOrEmpty( domain ) ? $"{domain}\\{user}" : user;

			WriteLine( $"Installing scheduler service '{serviceName}' into the service control manager..." );

			ServiceManager.InstallAndStart( serviceName, serviceDisplayName, "The ReadiNow Scheduler Service.", path, username, password, ServiceType.ServiceWin32OwnProcess, 1000 );

			WriteLine( $"Scheduler service '{serviceName}' successfully installed into the service control manager." );
		}

		/// <summary>
		///     Configures the upload directory.
		/// </summary>
		private void ConfigureUploadDirectory( )
		{
			string path = GetPath( ArgumentParser.UploadDirectory );

			if ( string.IsNullOrEmpty( path ) )
			{
				WriteLine( "No upload directory specified. Skipping", Severity.Warning );
				return;
			}

			Uri uri = new Uri( path );

			if ( !uri.IsUnc )
			{
				if ( !Directory.Exists( path ) )
				{
					try
					{
						WriteLine( $"No upload directory found. Creating '{path}'..." );
						Directory.CreateDirectory( path );
						WriteLine( $"Upload directory '{path}' successfully created." );
					}
					catch ( Exception exc )
					{
						WriteLine( $"Failed to create upload directory.\n{exc}", Severity.Error );
					}
				}
				else
				{
					WriteLine( $"Upload directory '{path}' already exists." );
				}

				ApplyFolderSecurity( path, ArgumentParser.AppPoolDomain, ArgumentParser.AppPoolUser );
			}
		}

		/// <summary>
		///     Configures IIS.
		/// </summary>
		private void ConfigureWebServer( )
		{
			string virtualDirectoryPath = ArgumentParser.VirtualDirectoryPath;
			string domain = ArgumentParser.AppPoolDomain;
			string username = ArgumentParser.AppPoolUser;
			string password = ArgumentParser.AppPoolPassword;
			string spVirtualPath = ArgumentParser.SpVirtualPath;
			string spPhysicalPath = ArgumentParser.SpPhysicalPath;
			string spApiVirtualPath = ArgumentParser.SpApiVirtualPath;
			string spApiPhysicalPath = ArgumentParser.SpApiPhysicalPath;

			if ( string.IsNullOrEmpty( virtualDirectoryPath ) )
			{
				WriteLine( "No virtual directory name specified. Skipping IIS configuration.", Severity.Warning );
				return;
			}

			string path = $"/{virtualDirectoryPath.Trim( '/' )}";

			if ( !string.IsNullOrEmpty( spPhysicalPath ) )
			{
				spPhysicalPath = GetPath( spPhysicalPath );
			}

			if ( !string.IsNullOrEmpty( spApiPhysicalPath ) )
			{
				spApiPhysicalPath = GetPath( spApiPhysicalPath );
			}

			try
			{
				Type autoStartType = GetAutoStartProviderType( );

				WriteLine( "Connecting to IIS..." );

				using ( WebManager webManager = new WebManager( WriteLine, FrontEndRootPath ) )
				{
					if ( webManager.Connect( ) )
					{
						webManager.CreateAppPool( "SoftwarePlatformAppPool", domain, username, password );
						webManager.ConfigureVirtualDirectory( path );
						webManager.CreateAutoStartProvider( "SpApiPreload", autoStartType );
						webManager.CreateApplication( path, spVirtualPath, spPhysicalPath );
						webManager.CreateApplication( path, spApiVirtualPath, spApiPhysicalPath, "SoftwarePlatformAppPool", true, "SpApiPreload" );
						webManager.StartAppPool( "SoftwarePlatformAppPool" );
					}
				}
			}
			catch ( Exception exc )
			{
				WriteLine( $"Failed to configure IIS.\n{exc}", Severity.Error );
			}
		}

		/// <summary>
		///     Creates the log directory.
		/// </summary>
		/// <param name="path">The log folder path.</param>
		private void CreateLogDirectory( string path )
		{
			WriteLine( "Searching for log directory..." );

			/////
			// Ensure the log folder exists
			/////
			if ( !Directory.Exists( path ) )
			{
				try
				{
					WriteLine( $"No log directory found. Creating '{path}'..." );
					Directory.CreateDirectory( path );
					WriteLine( $"Log directory '{path}' successfully created." );
				}
				catch ( Exception exc )
				{
					WriteLine( $"Failed to create log directory.\n{exc}", Severity.Error );
				}
			}
			else
			{
				WriteLine( $"Found log directory at '{path}'." );
			}
		}

		/// <summary>
		///     Deploys the application.
		/// </summary>
		/// <param name="applications">The applications.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="applicationName">Name of the application.</param>
		private void DeployApplication( IEnumerable<AppData> applications, string tenantName, Guid applicationId, string applicationName )
		{
			if ( applications == null || applications.All( app => app.ApplicationId != applicationId ) )
			{
				WriteLine( $"Deploying '{applicationName}' solution to tenant '{tenantName}'..." );

				FrontEndProcessingContext context = new FrontEndProcessingContext( s => WriteLine( s ) );

				AppManager.DeployApp( tenantName, applicationId.ToString( "B" ), null, context );

				InvalidateTenant( tenantName );

				WriteLine( $"'{applicationName}' solution successfully deployed to tenant '{tenantName}'." );
			}
			else
			{
				WriteLine( $"'{applicationName}' solution already exists in tenant '{tenantName}'." );
			}
		}

		/// <summary>
		///     Deploys the core applications.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		private void DeployCoreApplications( string tenantName )
		{
			if ( string.IsNullOrEmpty( tenantName ) )
			{
				WriteLine( "No tenant name specified. Core applications will not be deployed.", Severity.Warning );
				return;
			}

			var tenantApps = AppManager.ListTenantApps( tenantName );

			DeployApplication( tenantApps, tenantName, Applications.CoreApplicationId, "Core" );
			DeployApplication( tenantApps, tenantName, Applications.ConsoleApplicationId, "Console" );
			DeployApplication( tenantApps, tenantName, Applications.CoreDataApplicationId, "Core Data" );
		}

		/// <summary>
		///     Determines the root path.
		/// </summary>
		private void DetermineRootPath( )
		{
			WriteLine( $"Root path: {FrontEndRootPath}" );
			WriteLine( $"Config path: {SoftwarePlatformConfigPath}" );
		}

		/// <summary>
		///     Directories the security exists.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="account">The account.</param>
		/// <param name="rights">The rights.</param>
		/// <returns></returns>
		private bool DirectorySecurityExists( string directory, string domain, string account, FileSystemRights rights )
		{
			if ( string.IsNullOrWhiteSpace( directory ) || string.IsNullOrWhiteSpace( account ) )
			{
				return false;
			}

			domain = domain ?? Environment.UserDomainName;

			var directoryInfo = new DirectoryInfo( directory );
			DirectorySecurity directorySecurity = directoryInfo.GetAccessControl( );

			AuthorizationRuleCollection accessRules = directorySecurity.GetAccessRules( true, true, typeof( NTAccount ) );

			foreach ( AuthorizationRule rule in accessRules )
			{
				FileSystemAccessRule accessRule = rule as FileSystemAccessRule;

				if ( accessRule != null )
				{
					/////
					// Discard Deny rules
					/////
					if ( accessRule.AccessControlType != AccessControlType.Allow )
					{
						continue;
					}

					NTAccount ntAccount = accessRule.IdentityReference as NTAccount;

					if ( ntAccount == null )
					{
						continue;
					}

					if ( !ntAccount.Value.Equals( $"{domain}\\{account}", StringComparison.OrdinalIgnoreCase ) )
					{
						continue;
					}

					if ( accessRule.FileSystemRights != FileSystemRights.FullControl && accessRule.FileSystemRights != rights )
					{
						continue;
					}

					return true;
				}
			}

			return false;
		}


		/// <summary>
		///     Perform first-time installation bootstrapping tasks.
		/// </summary>
		/// <remarks>
		///     This is a no-op if global tenant is already present.
		/// </remarks>
		/// <exception cref="FileNotFoundException"></exception>
		public void InstallBootstrap( )
		{
			string databaseServer = ArgumentParser.DatabaseServer;
			string databaseCatalog = ArgumentParser.DatabaseCatalog;

			if ( string.IsNullOrEmpty( databaseServer ) )
			{
				WriteLine( "No Sql Server specified. Skipping bootstrap solution installation.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( databaseCatalog ) )
			{
				WriteLine( "No Sql Server database specified. Skipping bootstrap solution installation.", Severity.Warning );
				return;
			}

			WriteLine( $"Detecting if the global tenant exists in the '{databaseCatalog}' database on '{databaseServer}'..." );

			if ( Database.GlobalTenantExists( databaseServer, databaseCatalog ) )
			{
				WriteLine( "Found the global tenant. Bootstrap solution installation skipped." );
				return;
			}

			WriteLine( "No global tenant found. Installing via bootstrap..." );

			using ( new InstallationContext( ) )
			{
				TenantManager.InstallGlobalTenant( );

				WriteLine( "Rebuilding full text catalog..." );

			    Database.SetPlatformInstallInfo( databaseServer, databaseCatalog );

                Database.RebuildFullTextCatalog( databaseServer, databaseCatalog, false );
			}

			WriteLine( "Core solution successfully installed." );
		}

		/// <summary>
		/// Upgrades bootstrap operations.
		/// </summary>
		public void UpgradeBootstrap( )
		{
			string databaseServer = ArgumentParser.DatabaseServer;
			string databaseCatalog = ArgumentParser.DatabaseCatalog;

			if ( string.IsNullOrEmpty( databaseServer ) )
			{
				WriteLine( "No Sql Server specified. Skipping bootstrap solution upgrade.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( databaseCatalog ) )
			{
				WriteLine( "No Sql Server database specified. Skipping bootstrap solution upgrade.", Severity.Warning );
				return;
			}

			using ( new InstallationContext( ) )
			{
			    Database.RepairPackageIdInGlobal( databaseServer, databaseCatalog );

                Database.RepairResourceKeyHashesInGlobal( databaseServer, databaseCatalog );

            }
		}

		/// <summary>
		///     Ensures the database exists.
		/// </summary>
		/// <exception cref="FileNotFoundException">Unable to locate dacpac file.</exception>
		/// <exception cref="FileNotFoundException">Unable to locate dacpac file.</exception>
		private void EnsureDatabaseExists( )
		{
			string databaseServer = ArgumentParser.DatabaseServer;
			string databaseCatalog = ArgumentParser.DatabaseCatalog;

			if ( string.IsNullOrEmpty( databaseServer ) )
			{
				WriteLine( "No Sql Server specified. Skipping database creation.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( databaseCatalog ) )
			{
				WriteLine( "No Sql Server database specified. Skipping database creation.", Severity.Warning );
				return;
			}

			WriteLine( $"Checking whether the '{databaseCatalog}' database exists on the Sql Server '{databaseServer}'..." );

			if ( !DatabaseHelper.DatabaseExists( new SqlDatabaseInfo( databaseServer, "master" ), databaseCatalog ) )
			{
				WriteLine( $"Database '{databaseCatalog}' was not found on on the Sql Server '{databaseServer}'. Preparing dacpac install..." );

				string rootPath = FrontEndRootPath;

				string dacPacPath = ArgumentParser.DacpacPath ?? Path.Combine( rootPath, "Database\\Dacpac\\ReadiNowDatabase.dacpac" );

				if ( !File.Exists( dacPacPath ) )
				{
					WriteLine( $"Cannot find dacpac file '{dacPacPath}'. Skipping database creation.", Severity.Error );
					return;
				}

				string dataDirectory = ArgumentParser.DataDirectory ?? Path.Combine( rootPath, "Database" );

				WriteLine( $"Dacpac deploy using data directory {dataDirectory}..." );

				if ( FrontEndHelper.IsLocalAddress( databaseServer ) && !Directory.Exists( dataDirectory ) )
				{
					try
					{
						WriteLine( $"Creating data directory '{dataDirectory}'..." );

						Directory.CreateDirectory( dataDirectory );
					}
					catch ( Exception exc )
					{
						WriteLine( $"Failed to create data directory.\n{exc}", Severity.Error );
						return;
					}
				}

				WriteLine( $"Starting dacpac deploy of database {databaseCatalog} to server {databaseServer}..." );

				DatabaseHelper.DeployDatabase( dacPacPath, databaseServer, databaseCatalog, "SoftwarePlatform", null, null, dataDirectory, dataDirectory, s => WriteLine( s ) );

				WriteLine( "Dacpac deploy complete..." );
			}
			else
			{
				WriteLine( $"Found existing database {databaseCatalog} on Sql Server {databaseServer}..." );
			}
		}

		/// <summary>
		///     Ensures the database user exists.
		/// </summary>
		private void EnsureDatabaseUserExists( )
		{
			string databaseServer = ArgumentParser.DatabaseServer;
			string databaseCatalog = ArgumentParser.DatabaseCatalog;

			if ( string.IsNullOrEmpty( databaseServer ) )
			{
				WriteLine( "No Sql Server specified. Skipping database user creation.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( databaseCatalog ) )
			{
				WriteLine( "No Sql Server database specified. Skipping database user creation.", Severity.Warning );
				return;
			}

			string databaseDomain = ArgumentParser.AppPoolDomain;
			string databaseUser = ArgumentParser.AppPoolUser;
			string databasePassword = ArgumentParser.AppPoolPassword;
			string databaseRole = ArgumentParser.DatabaseRole;

			if ( string.IsNullOrEmpty( databaseUser ) )
			{
				WriteLine( "No Sql Server database user specified. Skipping database user creation.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( databasePassword ) )
			{
				WriteLine( "No Sql Server database user password specified. Skipping database user creation.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( databaseRole ) )
			{
				WriteLine( "No Sql Server database role specified. Skipping database user creation.", Severity.Warning );
				return;
			}

			string username = databaseDomain == null ? databaseUser : $"{databaseDomain}\\{databaseUser}";

			WriteLine( $"Creating database role '{databaseRole}'..." );

			Database.CreateDatabaseRole( databaseServer, databaseCatalog, databaseRole );

			WriteLine( $"Database role '{databaseRole}' successfully created." );

			WriteLine( $"Creating database user '{username}' and assigning to role '{databaseRole}'..." );

			Database.AddDatabaseUser( username, databaseRole, databaseServer, databaseCatalog, null, null );

			WriteLine( $"Database user '{username}' successfully created and assigned to role '{databaseRole}'." );
		}

		/// <summary>
		///     Ensures the default tenant exists.
		/// </summary>
		private void EnsureDefaultTenantExists( )
		{
			string databaseServer = ArgumentParser.DatabaseServer;
			string databaseCatalog = ArgumentParser.DatabaseCatalog;

			if ( string.IsNullOrEmpty( databaseServer ) )
			{
				WriteLine( "No Sql Server specified. Skipping default tenant creation.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( databaseCatalog ) )
			{
				WriteLine( "No Sql Server database specified. Skipping default tenant creation.", Severity.Warning );
				return;
			}

			string tenantName = ArgumentParser.DefaultTenant;

			if ( string.IsNullOrEmpty( tenantName ) )
			{
				WriteLine( "No default tenant specified. Skipping default tenant creation.", Severity.Warning );
				return;
			}

			if ( !Database.TenantExists( databaseServer, databaseCatalog, tenantName ) )
			{
				Database.CreateTenant( tenantName );

				WriteLine( $"Tenant '{tenantName}' successfully created." );
			}
			else
			{
				WriteLine( $"Tenant '{tenantName}' already exists." );
			}

			DeployCoreApplications( tenantName );
		}

		/// <summary>
		///     Ensures the redis configuration.
		/// </summary>
		/// <param name="redisPort">The redis port.</param>
		/// <exception cref="FileNotFoundException"></exception>
		private void EnsureRedisConfiguration( string redisPort )
		{
			WriteLine( "Searching for local redis configuration file..." );

			string configPath = Path.Combine( FrontEndRootPath, "Redis\\redis.windows.conf" );

			if ( !File.Exists( configPath ) )
			{
				WriteLine( $"Failed to locate redis configuration file at {configPath}", Severity.Error );
				return;
			}

			WriteLine( $"Found redis configuration file at {configPath}" );

			bool update = false;
			bool found = false;

			string[ ] lines = File.ReadAllLines( configPath );

			for ( int index = 0; index < lines.Length; index++ )
			{
				string line = lines[ index ];

				if ( line.StartsWith( "port " ) )
				{
					found = true;

					string[ ] split = line.Split( ' ' );

					if ( split.Length < 2 || !string.Equals( split[ 1 ], redisPort ) )
					{
						WriteLine( $"Updating redis configuration file. Changing port from {split[ 1 ]} to {redisPort}." );

						lines[ index ] = $"port {redisPort}";
						update = true;
					}

					break;
				}
			}

			if ( !found )
			{
				File.AppendAllLines( configPath, new[ ]
				{
					$"port {redisPort}"
				} );

				WriteLine( $"No redis port specified in configuration file. Adding {redisPort}." );
			}
			else
			{
				if ( update )
				{
					File.WriteAllLines( configPath, lines );
				}
			}
		}

		/// <summary>
		///     Gets the type of the automatic start provider.
		/// </summary>
		/// <returns></returns>
		private Type GetAutoStartProviderType( )
		{
			string typeName = "EDC.SoftwarePlatform.WebApi.AppPreload";

			string path = Path.Combine( FrontEndRootPath, "SpApi\\Bin\\EDC.SoftwarePlatform.WebApi.dll" );

			try
			{
				Assembly assembly = Assembly.LoadFrom( path );

				if ( assembly == null )
				{
					throw new FileNotFoundException( "Unable to locate required assembly file.", path );
				}

				return assembly.GetType( typeName );
			}
			catch ( Exception exc )
			{
				WriteLine( $"Failed to obtain auto start provider type '{typeName}' from assembly '{path}'.\n{exc}", Severity.Error );
			}

			return null;
		}

		/// <summary>
		///     Gets the path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		private string GetPath( string path )
		{
			if ( string.IsNullOrEmpty( path ) )
			{
				return path;
			}

			if ( !Path.IsPathRooted( path ) )
			{
				path = Path.Combine( FrontEndRootPath, path );
			}

			return Path.GetFullPath( path );
		}

		private void InstallRedisService( )
		{
			string serviceName = ArgumentParser.RedisServiceName;
			string serviceDisplayName = ArgumentParser.RedisServiceDisplayName;

			if ( string.IsNullOrEmpty( serviceName ) )
			{
				WriteLine( "No redis service name specified. Skipping redis installation.", Severity.Warning );
				return;
			}

			if ( ServiceManager.ServiceIsInstalled( serviceName ) )
			{
				WriteLine( $"Found existing redis service '{serviceName}'." );
				return;
			}

			string exePath = Path.Combine( FrontEndRootPath, "Redis\\redis-server.exe" );

			if ( !File.Exists( exePath ) )
			{
				WriteLine( $"Failed to locate redis binary at {exePath}. Skipping", Severity.Error );
				return;
			}

			WriteLine( $"Found redis binary at {exePath}." );

			string path = $"{exePath} --service-run \"redis.windows.conf\"";

			string domain = ArgumentParser.RedisDomain;
			string user = ArgumentParser.RedisUser;
			string password = ArgumentParser.RedisPassword;

			string username = !string.IsNullOrEmpty( domain ) ? $"{domain}\\{user}" : user;

			WriteLine( $"Installing redis service '{serviceName}' into the service control manager..." );

			ServiceManager.InstallAndStart( serviceName, serviceDisplayName, "The ReadiNow Redis Service.", path, username, password );

			WriteLine( $"Redis service '{serviceName}' successfully installed into the service control manager." );
		}

		/// <summary>
		///     Invalidates the tenant.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <exception cref="ArgumentNullException"></exception>
		private void InvalidateTenant( string tenantName )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				if ( string.IsNullOrEmpty( tenantName ) )
				{
					throw new ArgumentNullException( nameof( tenantName ) );
				}

				ReadiNow.Model.Tenant tenant = TenantHelper.Find( tenantName );

				if ( tenant != null )
				{
					TenantHelper.Invalidate( tenant );
				}
			}
		}

		/// <summary>
		///     Removes the sym link files.
		/// </summary>
		private void RemoveSymLinkFiles( )
		{
			string symbolicLinkPath = GetPath( ArgumentParser.ConfigSymbolicLinkPath );

			if ( !string.IsNullOrEmpty( symbolicLinkPath ) )
			{
				if ( File.Exists( symbolicLinkPath ) )
				{
					WriteLine( $"Deleting symbolic link '{symbolicLinkPath}'..." );
					File.Delete( symbolicLinkPath );
					WriteLine( $"Symbolic link '{symbolicLinkPath}' successfully deleted." );
				}
			}
		}

		/// <summary>
		///     Sets the configuration value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="get">The get.</param>
		/// <param name="set">The set.</param>
		/// <param name="value">The value.</param>
		/// <param name="checkDirectory">if set to <c>true</c> [check directory].</param>
		/// <returns></returns>
		private bool SetConfigurationValue( string name, Func<string> get, Action<string> set, string value, bool checkDirectory = false )
		{
			if ( !checkDirectory || Directory.Exists( value ) )
			{
				if ( !string.Equals( get( ), value, StringComparison.InvariantCultureIgnoreCase ) )
				{
					WriteLine( $"Setting '{name}' configuration value to '{value}'..." );

					set( value );
					return true;
				}

				WriteLine( $"Configuration setting '{name}' is already set to '{value}'. Skipping." );
			}
			else
			{
				WriteLine( $"Skipping '{name}' configuration value as the path '{value}' cannot be found.", Severity.Warning );
			}

			return false;
		}

		/// <summary>
		///     Sets the configuration value from assembly.
		/// </summary>
		/// <param name="assemblyType">Type of the assembly.</param>
		/// <param name="name">The name.</param>
		/// <param name="get">The get.</param>
		/// <param name="set">The set.</param>
		/// <param name="checkDirectory">if set to <c>true</c> [check directory].</param>
		/// <returns></returns>
		private bool SetConfigurationValueFromAssembly( Type assemblyType, string name, Func<string> get, Action<string> set, bool checkDirectory = false )
		{
			CustomAttributeData assemblyConfiguration = Assembly.GetEntryAssembly( ).CustomAttributes.FirstOrDefault( attrib => attrib.AttributeType == assemblyType );

			if ( assemblyConfiguration != null && assemblyConfiguration.ConstructorArguments.Count > 0 && assemblyConfiguration.ConstructorArguments[ 0 ].ArgumentType == typeof( string ) )
			{
				string value = ( string ) assemblyConfiguration.ConstructorArguments[ 0 ].Value;

				if ( !string.IsNullOrEmpty( value ) )
				{
					return SetConfigurationValue( name, get, set, value, checkDirectory );
				}

				WriteLine( $"Invalid '{name}' value detected in assembly '{Assembly.GetEntryAssembly( ).FullName}'. Unable to set '{name}' value.", Severity.Warning );
			}
			else
			{
				WriteLine( $"No '{name}' value detected in assembly '{Assembly.GetEntryAssembly( ).FullName}'. Unable to set '{name}' value.", Severity.Warning );
			}

			return false;
		}

		/// <summary>
		///     Starts the redis.
		/// </summary>
		/// <exception cref="FileNotFoundException"></exception>
		private void StartRedisService( )
		{
			string serviceName = ArgumentParser.RedisServiceName;

			if ( string.IsNullOrEmpty( serviceName ) )
			{
				WriteLine( "No redis service name specified. Skipping redis service start.", Severity.Warning );
				return;
			}

			if ( !ServiceManager.ServiceIsInstalled( serviceName ) )
			{
				WriteLine( $"No existing redis service '{serviceName}' found.", Severity.Error );
				return;
			}

			WriteLine( $"Starting redis service '{serviceName}'..." );

			ServiceManager.StartService( serviceName );
		}

		/// <summary>
		///     Tests the local redis server.
		/// </summary>
		/// <param name="redisServer">The redis server.</param>
		/// <param name="redisPort">The redis port.</param>
		private void TestLocalRedisServer( string redisServer, string redisPort )
		{
			WriteLine( $"Testing connection to Redis server {redisServer} on port {redisPort}..." );

			if ( !RedisManager.TestConnection( redisServer, redisPort ) )
			{
				WriteLine( $"Connection to Redis server {redisServer} on port {redisPort} failed." );

				EnsureRedisConfiguration( redisPort );

				string redisFolderPath = Path.Combine( FrontEndRootPath, "Redis" );

				ApplyFolderSecurity( redisFolderPath, ArgumentParser.RedisDomain, ArgumentParser.RedisUser );

				InstallRedisService( );

				StartRedisService( );
			}
			else
			{
				WriteLine( $"Successfully established a connection to the Redis server {redisServer} on port {redisPort}." );
			}
		}

		/// <summary>
		///     Tests the remote redis server.
		/// </summary>
		/// <param name="redisServer">The redis server.</param>
		/// <param name="redisPort">The redis port.</param>
		/// <exception cref="RedisConnectionException"></exception>
		private void TestRemoteRedisServer( string redisServer, string redisPort )
		{
			WriteLine( $"Testing connection to Redis server {redisServer} on port {redisPort}..." );

			if ( !RedisManager.TestConnection( redisServer, redisPort ) )
			{
				WriteLine( $"Connection to Redis server {redisServer} on port {redisPort} failed." );
			}
		}

		/// <summary>
		///     Shutdowns the redis instance.
		/// </summary>
		private void UninstallRedis( )
		{
			string serviceName = ArgumentParser.RedisServiceName;

			if ( string.IsNullOrEmpty( serviceName ) )
			{
				WriteLine( "No redis service name specified. Skipping redis uninstall.", Severity.Warning );
				return;
			}

			if ( !ServiceManager.ServiceIsInstalled( serviceName ) )
			{
				WriteLine( $"No existing redis service '{serviceName}' found." );
				return;
			}

			ServiceState state = ServiceManager.GetServiceStatus( serviceName );

			if ( state != ServiceState.Running )
			{
				WriteLine( $"Stopping service '{serviceName}'..." );

				ServiceManager.StopService( serviceName );

				WriteLine( $"Service '{serviceName}' successfully stopped." );
			}

			WriteLine( $"Uninstalling redis service '{serviceName}'..." );

			ServiceManager.Uninstall( serviceName );

			WriteLine( $"Redis service '{serviceName}' successfully uninstalled." );
		}

		/// <summary>
		///     Uninstalls the scheduler.
		/// </summary>
		private void UninstallScheduler( )
		{
			string serviceName = ArgumentParser.SchedulerServiceName;

			if ( string.IsNullOrEmpty( serviceName ) )
			{
				WriteLine( "No scheduler service name specified. Skipping scheduler uninstall.", Severity.Warning );
				return;
			}

			if ( !ServiceManager.ServiceIsInstalled( serviceName ) )
			{
				WriteLine( $"No existing scheduler service '{serviceName}' found." );
				return;
			}

			ServiceState state = ServiceManager.GetServiceStatus( serviceName );

			if ( state != ServiceState.Running )
			{
				WriteLine( $"Stopping service '{serviceName}'..." );

				ServiceManager.StopService( serviceName );

				WriteLine( $"Service '{serviceName}' successfully stopped." );
			}

			WriteLine( $"Uninstalling scheduler service '{serviceName}'..." );

			ServiceManager.Uninstall( serviceName );

			WriteLine( $"Scheduler service '{serviceName}' successfully uninstalled." );
		}

		/// <summary>
		///     Updates the configuration file.
		/// </summary>
		private void UpdateConfigFile( )
		{
			if ( !File.Exists( SoftwarePlatformConfigPath ) )
			{
				WriteLine( $"Cannot locate configuration file at {SoftwarePlatformConfigPath}. Skipping update...", Severity.Error );
				return;
			}

			ExeConfigurationFileMap map = new ExeConfigurationFileMap
			{
				ExeConfigFilename = SoftwarePlatformConfigPath
			};

			var configuration = ConfigurationManager.OpenMappedExeConfiguration( map, ConfigurationUserLevel.None );

			if ( !configuration.HasFile )
			{
				WriteLine( $"Failed to open configuration file at {SoftwarePlatformConfigPath}.", Severity.Error );
				return;
			}

			bool modified = false;

			modified |= UpdateDatabaseConfig( configuration );
			modified |= UpdateSiteConfig( configuration );
			modified |= UpdateServerConfig( configuration );
			modified |= UpdateFileRepositoryConfig( configuration );
			modified |= UpdateRedisConfig( configuration );

			if ( modified )
			{
				WriteLine( @"Configuration file modified. Saving changes..." );
				configuration.Save( ConfigurationSaveMode.Modified );
			}
		}

		/// <summary>
		///     Updates the database configuration.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <returns></returns>
		private bool UpdateDatabaseConfig( Configuration configuration )
		{
			bool modified = false;

			DatabaseConfiguration databaseSettingsSection = ( DatabaseConfiguration ) configuration.GetSection( "databaseSettings" );

			if ( databaseSettingsSection == null )
			{
				WriteLine( "Failed to open 'databaseSettings' section of configuration file.", Severity.Error );
				return false;
			}

			if ( !string.Equals( databaseSettingsSection.ConnectionSettings.Server, ArgumentParser.DatabaseServer, StringComparison.InvariantCultureIgnoreCase ) )
			{
				databaseSettingsSection.ConnectionSettings.Server = ArgumentParser.DatabaseServer;
				modified = true;
			}

			if ( !string.Equals( databaseSettingsSection.ConnectionSettings.Database, ArgumentParser.DatabaseCatalog, StringComparison.InvariantCultureIgnoreCase ) )
			{
				databaseSettingsSection.ConnectionSettings.Database = ArgumentParser.DatabaseCatalog;
				modified = true;
			}

			if ( modified )
			{
				WriteLine( "Database settings updated..." );
			}

			return modified;
		}

		/// <summary>
		///     Updates the file repository configuration.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <returns></returns>
		private bool UpdateFileRepositoryConfig( Configuration configuration )
		{
			bool modified = false;

			FileRepositoryConfiguration fileRepositorySettingsSection = ( FileRepositoryConfiguration ) configuration.GetSection( "fileRepositorySettings" );

			string basePath = ArgumentParser.FileRepoDirectory;

			if ( string.IsNullOrEmpty( basePath ) )
			{
				basePath = "PlatformFileRepos";
			}

			if ( !Path.IsPathRooted( basePath ) )
			{
				basePath = Path.Combine( FrontEndRootPath, basePath );
			}

			modified |= VerifyFileRepository( fileRepositorySettingsSection, "Application Library", basePath, ArgumentParser.AppLibFileRepoDirectory );
			modified |= VerifyFileRepository( fileRepositorySettingsSection, "Binary", basePath, ArgumentParser.BinFileRepoDirectory );
			modified |= VerifyFileRepository( fileRepositorySettingsSection, "Document", basePath, ArgumentParser.DocFileRepoDirectory );
			modified |= VerifyFileRepository( fileRepositorySettingsSection, "Temporary", basePath, ArgumentParser.TempFileRepoDirectory );

			if ( modified )
			{
				WriteLine( "File repositories settings updated..." );
			}

			return modified;
		}

		private bool UpdateRedisConfig( Configuration configuration )
		{
			bool modified = false;

			RedisConfiguration redisSettingsSection = ( RedisConfiguration ) configuration.GetSection( "redisSettings" );

			if ( redisSettingsSection == null )
			{
				WriteLine( "Failed to open 'redisSettings' section of configuration file.", Severity.Error );
				return false;
			}

			string server = ArgumentParser.RedisServer;

			if ( string.IsNullOrEmpty( server ) )
			{
				WriteLine( "No redis server specified.", Severity.Warning );
				return false;
			}

			string port = ArgumentParser.RedisPort;

			if ( string.IsNullOrEmpty( port ) )
			{
				WriteLine( "No redis port specified.", Severity.Warning );
				return false;
			}

			int portNum;

			if ( !int.TryParse( port, out portNum ) )
			{
				WriteLine( $"Invalid redis port number '{port}'.", Severity.Error );
				return false;
			}

			RedisServer redis = null;

			foreach ( RedisServer redisServer in redisSettingsSection.Servers )
			{
				if ( string.Equals( redisServer.HostName, server, StringComparison.InvariantCultureIgnoreCase ) )
				{
					redis = redisServer;
					break;
				}
			}

			if ( redis == null )
			{
				redis = new RedisServer( server, portNum );
				redisSettingsSection.Servers.Add( redis );
				modified = true;
			}
			else
			{
				if ( redis.Port != portNum )
				{
					redis.Port = portNum;
					modified = true;
				}
			}

			if ( modified )
			{
				WriteLine( "Redis settings updated..." );
			}

			return modified;
		}

		/// <summary>
		///     Updates the server configuration.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <returns></returns>
		private bool UpdateServerConfig( Configuration configuration )
		{
			bool modified = false;

			ServerConfiguration serverSettingsSection = ( ServerConfiguration ) configuration.GetSection( "serverSettings" );

			if ( serverSettingsSection == null )
			{
				WriteLine( "Failed to open 'serverSettings' section of configuration file.", Severity.Error );
				return false;
			}

			modified |= SetConfigurationValue( "UploadDirectory", ( ) => serverSettingsSection.UploadDirectory.Path, val => serverSettingsSection.UploadDirectory.Path = val, GetPath( ArgumentParser.UploadDirectory ), true );

			var systemInfoSettings = serverSettingsSection.SystemInfo;

			if ( systemInfoSettings != null )
			{
				modified |= SetConfigurationValue( "ActivationDate", ( ) => systemInfoSettings.ActivationDate, val => systemInfoSettings.ActivationDate = val, DateTime.UtcNow.ToString( "u" ) );
				modified |= SetConfigurationValue( "InstallFolder", ( ) => systemInfoSettings.InstallFolder, val => systemInfoSettings.InstallFolder = val, FrontEndRootPath, true );
				modified |= SetConfigurationValue( "LogFilePath", ( ) => systemInfoSettings.LogFilePath, val => systemInfoSettings.LogFilePath = val, Path.Combine( FrontEndRootPath, "Log" ), true );
				modified |= SetConfigurationValueFromAssembly( typeof( AssemblyConfigurationAttribute ), "CurrentBranch", ( ) => systemInfoSettings.CurrentBranch, val => systemInfoSettings.CurrentBranch = val );
				modified |= SetConfigurationValueFromAssembly( typeof( AssemblyFileVersionAttribute ), "CurrentVersion", ( ) => systemInfoSettings.CurrentVersion, val => systemInfoSettings.CurrentVersion = val );
			}
			else
			{
				WriteLine( "Failed to open systemInfo configuration section.", Severity.Error );
			}

			if ( modified )
			{
				WriteLine( "Server settings updated..." );
			}

			return modified;
		}

		/// <summary>
		///     Updates the site configuration.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <returns></returns>
		private bool UpdateSiteConfig( Configuration configuration )
		{
			bool modified = false;

			SiteConfiguration siteSettingsSection = ( SiteConfiguration ) configuration.GetSection( "siteSettings" );

			if ( siteSettingsSection == null )
			{
				WriteLine( "Failed to open 'siteSettings' section of configuration file.", Severity.Error );
				return false;
			}

			string name = ArgumentParser.SiteName ?? Dns.GetHostName( );

			if ( !string.Equals( siteSettingsSection.SiteSettings.Name, name, StringComparison.InvariantCultureIgnoreCase ) )
			{
				siteSettingsSection.SiteSettings.Name = name;
				modified = true;
			}

			string address = ArgumentParser.SiteAddress ?? FrontEndHelper.GetFqdn( );

			if ( !string.Equals( siteSettingsSection.SiteSettings.Address, address, StringComparison.InvariantCultureIgnoreCase ) )
			{
				siteSettingsSection.SiteSettings.Address = address;
				modified = true;
			}

			string root = ArgumentParser.SiteRoot ?? "/SoftwarePlatform/Services";

			if ( !string.Equals( siteSettingsSection.SiteSettings.ServiceRootAddress, root, StringComparison.InvariantCultureIgnoreCase ) )
			{
				siteSettingsSection.SiteSettings.ServiceRootAddress = root;
				modified = true;
			}

			if ( modified )
			{
				WriteLine( "Site settings updated..." );
			}

			return modified;
		}

		/// <summary>
		///     Verifies the file repository.
		/// </summary>
		/// <param name="fileRepositorySettingsSection">The file repository settings section.</param>
		/// <param name="name">The name.</param>
		/// <param name="basePath">The base path.</param>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		private bool VerifyFileRepository( FileRepositoryConfiguration fileRepositorySettingsSection, string name, string basePath, string path )
		{
			bool modified = false;

			if ( string.IsNullOrEmpty( path ) )
			{
				WriteLine( $"No {name} file repository location specified.", Severity.Warning );
			}
			else
			{
				if ( !Path.IsPathRooted( path ) )
				{
					path = Path.Combine( basePath, path );
				}

				Uri uri = new Uri( path );

				if ( !uri.IsUnc )
				{
					if ( !Directory.Exists( path ) )
					{
						try
						{
							WriteLine( $"Creating {name} file repository path at '{path}'." );
							Directory.CreateDirectory( path );
							WriteLine( $"Successfully created file repository path at '{path}'." );
						}
						catch ( Exception exc )
						{
							WriteLine( $"Failed to create file repository path.\n{exc}", Severity.Error );
						}
					}
					else
					{
						WriteLine( $"{name} file repository path '{path}' already exists." );
					}

					ApplyFolderSecurity( path, ArgumentParser.AppPoolDomain, ArgumentParser.AppPoolUser );
				}

				FileRepositoryConfigElement element = null;

				foreach ( FileRepositoryConfigElement item in fileRepositorySettingsSection.FileRepositories )
				{
					if ( string.Equals( item.Name, name, StringComparison.InvariantCultureIgnoreCase ) )
					{
						element = item;
						break;
					}
				}

				if ( element != null )
				{
					if ( !string.Equals( element.Path, path, StringComparison.InvariantCultureIgnoreCase ) )
					{
						element.Path = path;
						modified = true;
					}
				}
				else
				{
					element = new FileRepositoryConfigElement( name, path );
					fileRepositorySettingsSection.FileRepositories.Add( element );
					modified = true;
				}
			}

			return modified;
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="severity">The severity.</param>
		private void WriteLine( string line = null, Severity severity = Severity.Info )
		{
			if ( line == null )
			{
				Console.WriteLine( );
			}
			else
			{
				
				Console.WriteLine( line );
			}
		}
	}
}