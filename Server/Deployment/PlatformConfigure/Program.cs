// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using EDC.Database;
using EDC.Diagnostics;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.SoftwarePlatform.Install.Common;
using EDC.SoftwarePlatform.Migration;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.Threading;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using Solution = EDC.SoftwarePlatform.Install.Common.Solution;
using Tenant = EDC.SoftwarePlatform.Install.Common.Tenant;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.FeatureSwitch;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Messaging;

namespace PlatformConfigure
{
	/// <summary>
	///     Configure program.
	/// </summary>
	public class Program
	{
		/// <summary>
		///     The _parser
		/// </summary>
		private readonly CommandLineParser _parser;

		/// <summary>
		///		The _attribute argument parser
		/// </summary>
		private AttributeArgParser _attributeArgParser;

		/// <summary>
		///     Prevents a default instance of the <see cref="Program" /> class from being created.
		/// </summary>
		/// <param name="args">The arguments.</param>
		private Program( string[ ] args )
		{
			_parser = new CommandLineParser( args );

			if ( args.Length > 0 )
			{
				string arg = args.FirstOrDefault( a => !a.Equals( "debug", StringComparison.InvariantCultureIgnoreCase ) );

				if ( ! string.IsNullOrEmpty( arg ) )
				{
					_attributeArgParser = new AttributeArgParser( MethodBase.GetCurrentMethod( ).DeclaringType, arg );
				}
			}
		}

		/// <summary>
		///     Converts the report.
		/// </summary>
		/// <remarks>
		///		Command line parameters
		///		-convertReport [-server svr] [-database db]
		/// </remarks>
		[Function("convertReport", "Converts the reports from xml into their entity model equivalent.", "cr" )]
		[FunctionArgument( "server", "refers to the server on which the reports will be converted.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the reports will be converted.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ConvertReport( )
		{
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( "Convert report" ) )
			{
				ReportMigration.ConvertReports( );
			}
		}

		/// <summary>
		///     Creates the database.
		/// </summary>
		/// <remarks>
		///		Command line parameters
		///		-createDatabase [-server svr] [-database db] [-dataDirectory dataDir]
		/// </remarks>
		[Function( "createDatabase", "Creates a new database.", "cdb" )]
		[FunctionArgument( "server", "refers to the server on which the database will be created.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the new database (catalog).", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dataDirectory", "is the location on disk the new database will be created.", "C:\\PlatformDatabase", FunctionArgumentOptions.Optional )]		
		private void CreateDatabase( )
		{
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );
			var dataDirectory = GetArgument<string>( "dataDirectory" );			

			/////
			// SoftwarePlatform is always installed to this location.
			/////
			var programFiles = Environment.GetFolderPath( Environment.SpecialFolder.ProgramFiles );

			var dacPacPath = Path.Combine( programFiles, "EDC\\SoftwarePlatform\\Bin\\ReadiNowDatabase.dacpac" );

			if ( !File.Exists( dacPacPath ) )
			{
				throw new FileNotFoundException( "Unable to locate the ReadiNowDatabase.dacpac file.", dacPacPath );
			}

			using ( DatabaseContextInfo.SetContextInfo( "Create database" ) )
			{
				DatabaseHelper.DeployDatabase( @"C:\Program Files\EDC\SoftwarePlatform\Bin\ReadiNowDatabase.dacpac", server, database, "SoftwarePlatform", dbUser, dbPassword, dataDirectory, dataDirectory, value => ConsoleLogger.WriteLine( value ) );
			}
		}

		/// <summary>
		/// Deploys the database.
		/// </summary>
		/// <exception cref="System.IO.FileNotFoundException">Unable to locate the specified dacpac file.</exception>
		[Function( "deployDatabase", "Deploy the specified dacpac database to the specified database server.", "dd" )]
		[FunctionArgument( "path", "path to the dacpac file." )]
		[FunctionArgument( "server", "refers to the server on which the database will be created.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the new database (catalog).", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dataDirectory", "is the location on disk the new database will be created.", "C:\\PlatformDatabase", FunctionArgumentOptions.Optional )]
		private void DeployDatabase( )
		{
			var path = GetArgument<string>( "path" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );
			var dataDirectory = GetArgument<string>( "dataDirectory" );

			if ( !File.Exists( path ) )
			{
				throw new FileNotFoundException( "Unable to locate the specified dacpac file.", path );
			}

			using ( DatabaseContextInfo.SetContextInfo( "Deploy database" ) )
			{
				DatabaseHelper.DeployDatabase( path, server, database, "SoftwarePlatform", dbUser, dbPassword, dataDirectory, dataDirectory, value => ConsoleLogger.WriteLine( value ) );
			}
		}

        /// <summary>
        ///     Update database master key.
        /// </summary>
        /// <remarks>
        ///		Command line parameters
        ///		-updateMasterKey [-server svr] [-database db] -password keystring -force
        /// </remarks>
        [Function("cycleMasterKey", "Cycle a database master key.", "cmk")]
        [FunctionArgument( "server", "refers to the server on which the database will be created.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the new database (catalog).", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "password", "the password to use to generate the key" )]
        private void CycleMasterKey()
        {
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");
            var password = GetArgument<string>("password");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( "Cycle master key" ) )
			{
				var dbInfo = EDC.ReadiNow.Database.DatabaseContext.GetContext( ).DatabaseInfo;

				DatabaseHelper.CycleMasterKey( dbInfo, password );
			}
        }


        /// <summary>
        ///     Restore database master key.
        /// </summary>
        /// <remarks>
        ///		Command line parameters
        ///		-urestoreMasterKey [-server svr] [-database db] -password keystring -force
        /// </remarks>
        [Function("restoreMasterKey", "Restore database master key.", "rmk")]
        [FunctionArgument( "server", "refers to the server on which the database will be created.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the new database (catalog).", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "password", "the password to use to generate the key" )]
        private void RestoreMasterKey()
        {
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");
            var password = GetArgument<string>("password");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( "Restore master key" ) )
			{
				var dbInfo = EDC.ReadiNow.Database.DatabaseContext.GetContext( ).DatabaseInfo;

				DatabaseHelper.RestoreMasterKey( dbInfo, password );
			}
        }

        /// <summary>
        ///     Creates the database user.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -dbuser -account act [-server svr] [-database db]
        /// </remarks>
        [Function( "createdbuser", "Creates a new user account in the specified database with db_owner access.", "cdbu" )]
		[FunctionArgument( "account", "is the username of the  account to create." )]
		[FunctionArgument( "server", "refers to the server on which the database user will be created.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the user will be created.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void CreateDatabaseUser( )
		{
			var accountName = GetArgument<string>( "account" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseContextInfo.SetContextInfo( $"Create database user '{accountName}'" ) )
			{
				Database.AddDatabaseUser( accountName, @"db_owner", server, database, dbUser, dbPassword );
			}
		}

		/// <summary>
		///     Creates the tenant.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -createTenant -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "createTenant", "Creates a new tenant on the system with the specified name.", "ct" )]
		[FunctionArgument( "tenant", "is the name of the new tenant." )]
		[FunctionArgument( "server", "refers to the server on which the tenant will be created.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the tenant will be created.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void CreateTenant( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Create tenant '{tenant}'" ) )
			{
				Tenant.CreateTenant( tenant );
			}
		}

		/// <summary>
		///     Creates the user account.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -createUser -user usr -password pwd -tenant tnt [-server svr] [-database db] [-role roleName]
		/// </remarks>
		[Function( "createUser", "Creates a new user on the system for the specified tenant having the specified name and password.", "cu" )]
		[FunctionArgument( "user", "is the name of the new user." )]
		[FunctionArgument( "password", "is the users password." )]
		[FunctionArgument( "tenant", "is the name of the tenant." )]
		[FunctionArgument( "server", "refers to the server on which the user will be created.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the user will be created.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "role", "add the role to the user account", null, FunctionArgumentOptions.Optional )]
		private void CreateUser( )
		{
			var user = GetArgument<string>( "user" );
			var password = GetArgument<string>( "password" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
            var dbPassword = GetArgument<string>("dbPassword");
            var roleName = GetArgument<string>("role");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Create user '{user}'" ) )
			{
				Tenant.CreateUser( user, password, tenant, roleName );
			}
		}

		/// <summary>
		///     Deletes the application package.
		/// </summary>
		[Function( "deleteApplicationPackage", "Deletes the specified application package from the application library.", "dap" )]
		[FunctionArgument( "appPackage", "guid representing the application package to delete. (Package Id)" )]
		[FunctionArgument( "server", "refers to the server on which the application package will be deleted.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the application will be deleted.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void DeleteAppPackage( )
		{
			var appPkg = GetArgument<Guid>( "appPackage" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Delete app package '{appPkg:B}'" ) )
			{
				AppManager.DeleteApp( appPkg );
			}
		}

		/// <summary>
		///     Deletes the application.
		/// </summary>
		[Function( "deleteApplication", "Deletes the entire application including all packages associated with it.", "delapp" )]
		[FunctionArgument( "app", "guid representing the application to delete. (App Id)" )]
		[FunctionArgument( "server", "refers to the server on which the application will be deleted.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the application will be deleted.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void DeleteApp( )
		{
			var app = GetArgument<Guid>( "app" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Delete app '{app:B}'" ) )
			{
				var applications = AppManager.ListAppPackages( app );

				if ( applications == null || applications.Count == 0 )
				{
					return;
				}

				if ( applications.Count > 1 )
				{
					throw new InvalidOperationException( $"More than one application found with Id '{app.ToString( "B" )}'" );
				}

				var application = applications.First( );

				foreach ( var package in application.Packages )
				{
					AppManager.DeleteApp( package.PackageId );
				}
			}
		}

		/// <summary>
		///     Deletes the tenant.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -deleteTenant -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "deleteTenant", "Deletes the specified tenant from the system.", "dt" )]
		[FunctionArgument( "tenant", "is the name of the tenant to delete." )]
		[FunctionArgument( "server", "refers to the server on which the tenant will be deleted.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the tenant will be deleted.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void DeleteTenant( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Delete tenant '{tenant}'" ) )
			{
				Tenant.DeleteTenant( tenant );
			}
		}

		/// <summary>
		///     Deletes the user.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -deleteUser -user usr -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "deleteUser", "Deletes the specified user from the specified tenant.", "du" )]
		[FunctionArgument( "user", "is the name of the user to delete." )]
		[FunctionArgument( "tenant", "is the name of the tenant the user belongs to." )]
		[FunctionArgument( "server", "refers to the server on which the user will be deleted.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the user will be deleted.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void DeleteUser( )
		{
			var user = GetArgument<string>( "user" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Delete user '{user}'" ) )
			{
				Tenant.DeleteUser( user, tenant );
			}
		}

        /// <summary>
        ///     Disables the tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -disableTenant -tenant tnt [-server svr] [-database db]
        /// </remarks>
        [Function("disableTenant", "Disables the specified tenant in the system.", "xt")]
        [FunctionArgument( "tenant", "is the name of the tenant to disable." )]
        [FunctionArgument( "server", "refers to the server on which the tenant will be disabled.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database where the tenant will be disabled.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
	    private void DisableTenant( )
	    {
            var tenant = GetArgument<string>("tenant");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Disable tenant '{tenant}'" ) )
			{
				Tenant.DisableTenant( tenant );
			}
	    }

        /// <summary>
        ///     Enables the tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -enableTenant -tenant tnt [-server svr] [-database db]
        /// </remarks>
        [Function("enableTenant", "Enables the specified tenant in the system.", "nt")]
        [FunctionArgument( "tenant", "is the name of the tenant to enable." )]
        [FunctionArgument( "server", "refers to the server on which the tenant will be enabled.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database where the tenant will be enabled.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void EnableTenant()
        {
            var tenant = GetArgument<string>("tenant");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Enable tenant '{tenant}'" ) )
			{
				Tenant.EnableTenant( tenant );
			}
        }

		/// <summary>
		///     Deploys an application from the app library to a tenant.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -deployApp -tenant tnt -app app [-ver ver] [-server svr] [-database db]
		/// </remarks>
		[Function("deployApp", "Deploys the specified application (with optional version) to the specified tenant.", "da" )]
		[FunctionArgument( "tenant", "refers to the name of the tenant for which the application will be deployed." )]
		[FunctionArgument( "app", "is the name|guid of the application to deploy." )]
		[FunctionArgument( "ver", "is the optional version to deploy.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "updateStats", "whether statistics should be updated.", "True", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "disableFts", "whether Full-Text indexing should be disabled.", "True", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server on which the application will be deployed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database (catalog) where the application will be deployed.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void DeployApp( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var app = GetArgument<string>( "app" );
			var ver = GetArgument<string>( "ver" );
			var updateStats = GetArgument<bool>( "updateStats" );
			var disableFts = GetArgument<bool>( "disableFts" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Deploy app '{app}' to '{tenant}'" ) )
			{
				AppManager.DeployApp( tenant, app, ver, null, updateStats, disableFts );
			}

		    InvalidateTenant(tenant);
		}

		/// <summary>
		///     Removes an application from the specified tenant.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -removeApp -tenant tnt -app app [-server svr] [-database db]
		/// </remarks>
		[Function( "removeApp", "Removes the specified application (with optional version) from the specified tenant.", "xa" )]
		[FunctionArgument( "tenant", "refers to the name of the tenant for which the application will be removed." )]
		[FunctionArgument( "app", "is the name|guid of the application to remove. (Application Id)" )]
		[FunctionArgument( "server", "refers to the server on which the application will be removed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database (catalog) where the application will be removed.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void RemoveApp( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var app = GetArgument<string>( "app" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Remove app '{app}' from '{tenant}'" ) )
			{
				AppManager.RemoveApp( tenant, app );
			}
		}

		/// <summary>
		///     Dumps the apps.
		/// </summary>
		/// <param name="apps">The apps.</param>
		/// <param name="listAll">if set to <c>true</c> [list all].</param>
		private static void DumpApps( List<AppData> apps, bool listAll = false )
		{
			const int padding = 2;

			if ( apps != null && apps.Count > 0 )
			{
				int maxVersionLength = 0;

				int maxNameLength = apps.Max( app =>
				{
					int maxVersion = app.Packages.Max( pkg => pkg.Version.Length );

					if ( maxVersion > maxVersionLength )
					{
						maxVersionLength = maxVersion;
					}

					return app.Name.Length;
				} );

				Console.WriteLine( @"{0} {1} {2}", @"Name".PadRight( maxNameLength + padding ), @"Version".PadRight( maxVersionLength + padding ), listAll ? "PackageId" : "ApplicationId" );
				ConsoleLogger.WriteLine( @"{0} {1} {2}", @"----".PadRight( maxNameLength + padding ), @"-------".PadRight( maxVersionLength + padding ), listAll ? "---------" : "-------------" );

				foreach ( AppData app in apps.OrderBy( app => app.Name ) )
				{
					foreach ( AppPackageData pkg in app.Packages.OrderBy( pkg => new Version( pkg.Version ) ) )
					{
						ConsoleLogger.WriteLine( @"{0} {1} {2}", app.Name.PadRight( maxNameLength + padding ), pkg.Version.PadRight( maxVersionLength + padding ), listAll ? pkg.PackageId.ToString( "B" ) : app.ApplicationId.ToString( "B" ) );
					}
				}
			}
		}

		/// <summary>
		///     Executes this instance.
		/// </summary>
		private void Execute( )
		{
			RequestContext.SetSystemAdministratorContext( );

			/////
			// Attach debugger
			/////
			if ( _parser.ContainsArgument( @"debug" ) || _parser.ContainsArgument( @"d" ) )
			{
				Debugger.Launch( );
			}

			if ( _parser.ContainsArgument( @"repl" ) )
			{
				RunRepl( );
				return;
			}

			/////
			// Database creation
			/////
			if ( RunAction( "createDatabase", "cdb", CreateDatabase ) )
				return;

			/////
			// Create database user
			/////
			if ( RunAction( "createDbUser", "cdbu", CreateDatabaseUser ) )
				return;

			/////
			// Deploy database
			/////
			if ( RunAction( "deployDatabase", "dd", DeployDatabase ) )
				return;

			/////
			// Install solution
			/////
			if ( RunAction( "installSolution", "is", InstallSolution ) )
				return;

			/////
			// Import app package (SqLite) to app library
			/////
			if ( RunAction( "importApp", "ia", ImportAppPackage ) )
				return;

			/////
			// Export app from tenant to app package (SqLite)
			/////
			if ( RunAction( "exportTenantApp", "eta", ExportTenantPackage ) )
				return;

			/////
			// Export app from Application Library to app package (SqLite)
			/////
			if ( RunAction( "exportApp", "ea", ExportPackage ) )
				return;

			/////
			// Deploy an application from the app library to an individual tenant
			/////
			if ( RunAction( "deployApp", "da", ( ) =>
			{
				DeployApp( );
				ReportMigration.ConvertReports( );
			} ) )
				return;

			/////
			// Upgrade a tenant application from one version to another (where both versions must be present in the application library)
			/////
			if ( RunAction( "upgradeApp", "ua", UpgradeApp ) )
				return;

			/////
			// Publish an app version from a tenant to the app library
			/////
			if ( RunAction( "publishApp", "pa", PublishApp ) )
				return;

			/////
			// Repair an application installation.
			/////
			if ( RunAction( "repairApp", "ra", RepairApp ) )
				return;

			/////
			// Remove an application installation.
			/////
			if ( RunAction( "removeApp", "xa", RemoveApp ) )
				return;

			/////
			// Transform configuration files
			/////
			if ( RunAction( "transformConfig", "tc", TransformConfig ) )
				return;

			/////
			// Convert Report entities from old XML format to the newer entity model form.
			/////
			if ( RunAction( "convertReport", "cr", ConvertReport ) )
				return;

			/////
			// Create Tenant
			/////
			if ( RunAction( "createTenant", "ct", CreateTenant ) )
				return;

			/////
			// Delete Tenant
			/////
			if ( RunAction( "deleteTenant", "dt", DeleteTenant ) )
				return;

			/////
			// Export Tenant
			/////
			if ( RunAction( "exportTenant", "et", ExportTenant ) )
				return;

            /////
            // Export Entity
            /////
            if ( RunAction( "exportEntity", "ee", ExportEntity ) )
                return;

            /////
            // Export Entity
            /////
            if ( RunAction( "importEntity", "ie", ImportEntity ) )
                return;

            /////
            // Import Tenant
            /////
            if ( RunAction( "importTenant", "it", ImportTenant ) )
				return;

			/////
			// Overwrite Tenant
			/////
			if ( RunAction( "overwriteTenant", "ot", OverwriteTenant ) )
				return;

			/////
			// Disable Tenant
			/////
			if ( RunAction( "disableTenant", "xt", DisableTenant ) )
				return;

			/////
			// Enable Tenant
			/////
			if ( RunAction( "enableTenant", "nt", EnableTenant ) )
				return;

			/////
			// List Tenants
			/////
			if ( RunAction( "listTenants", "lt", ListTenants ) )
				return;

			/////
			// Tenant exists
			/////
			if ( RunAction( "tenantExists", "te", TenantExists ) )
				return;

			/////
			// Provision a tenant
			/////
			if ( RunAction( "provisionTenant", "pt", ProvisionTenant ) )
				return;

			/////
			// Rename a tenant
			/////
			if ( RunAction( "renameTenant", "rt", RenameTenant ) )
				return;

			/////
			// Create user.
			/////
			if ( RunAction( "createUser", "cu", CreateUser ) )
				return;

			/////
			// Delete user.
			/////
			if ( RunAction( "deleteUser", "du", DeleteUser ) )
				return;

			/////
			// List users.
			/////
			if ( RunAction( "listUsers", "lu", ListUsers ) )
				return;

			/////
			// User exists.
			/////
			if ( RunAction( "userExists", "ue", UserExists ) )
				return;

			/////
			// Set Password.
			/////
			if ( RunAction( "setPassword", "sp", SetPassword ) )
				return;

			/////
			// Reset Account Logings.
			/////
			if ( RunAction( "resetAccountLogins", "resetact", ResetAccountLogins ) )
				return;

			/////
			// Enable Account.
			/////
			if ( RunAction( "enableAccount", "enableact", EnableAccount ) )
				return;

			/////
			// Disable Account.
			/////
			if ( RunAction( "disableAccount", "disableact", DisableAccount ) )
				return;

			/////
			// Lock Account.
			/////
			if ( RunAction( "lockAccount", "lockact", LockAccount ) )
				return;

			/////
			// Stage Application
			/////
			if ( RunAction( "stageApp", "sa", StageApp ) )
				return;

			/////
			// Turn on integration test mode
			/////
			if ( RunAction( "intgTestModeOn", null, IntgTestModeOn ) )
				return;

			/////
			// Turn off integration test mode
			/////
			if ( RunAction( "intgTestModeOff", null, IntgTestModeOff ) )
				return;

			/////
			// Turn on processing of the SoftwarePlatform inbox
			/////
			if ( RunAction( "processInboxOn", null, ProcessInboxOn ) )
				return;

			/////
			// Turn off processing of the SoftwarePlatform inbox
			/////
			if ( RunAction( "processInboxOff", null, ProcessInboxOff ) )
				return;

			/////
			// Set the interval for processing the inbox
			/////
			if ( RunAction( "processInboxInterval", null, ProcessInboxInterval ) )
				return;

			/////
			// List applications.
			/////
			if ( RunAction( "listApplications", "la", ListApps ) )
				return;

			/////
			// Delete Application
			/////
			if ( RunAction( "deleteApplication", "delapp", DeleteApp ) )
				return;

			/////
			// Delete Application Package
			/////
			if ( RunAction( "deleteApplicationPackage", "dap", DeleteAppPackage ) )
				return;

			/////
			// List Tenant Applications
			/////
			if ( RunAction( "listTenantApplications", "lta", ListTenantApps ) )
				return;

			/////
			// List App Access
			/////
			if ( RunAction( "listAppAccess", "laa", ListAppAccess ) )
				return;

			/////
			// List Tenant Applications
			/////
			if ( RunAction( "grantInstall", "gi", GrantInstall ) )
				return;

			/////
			// List Tenant Applications
			/////
			if ( RunAction( "denyInstall", "di", DenyInstall ) )
				return;

			/////
			// List Tenant Applications
			/////
			if ( RunAction( "grantPublish", "gp", GrantPublish ) )
				return;

			/////
			// List Tenant Applications
			/////
			if ( RunAction( "denyPublish", "dp", DenyPublish ) )
				return;


            /////
            // Turn off integration test mode
            /////
            if (RunAction("featureSwitch", "fs", FeatureSwitch))
                return;


            /////
            // Update master key
            /////
            if (RunAction("cycleMasterKey", "cmk", CycleMasterKey))
                return;

            /////
            // Update master key
            /////
            if (RunAction("restoreMasterKey", "rmk", RestoreMasterKey))
                return;

            /////
            // Converts an application
            /////
            if ( RunAction( "convertApplicationPackage", "cap", ConvertApplicationPackage ) )
				return;

            // Grant can modify application
            if (RunAction("grantModifyApp", "gma", GrantModifyApp))
                return;

            // Deny can modify application
            if (RunAction("denyModifyApp", "dma", DenyModifyApp))
		        return;

			/////
			// Install bootstrap.
			/////
			if ( RunAction( "installBootstrap", "ib", InstallBootstrap) )
				return;

			/////
			// Upgrade bootstrap.
			/////
			if ( RunAction( "upgradeBootstrap", "ub", UpgradeBootstrap ) )
				return;

            /////
            // Flush caches
            /////
            if (RunAction("flushCaches", "fc", FlushCaches) )
                return;

			/////
			// Create Restore Point
			/////
			if ( RunAction( "createRestorePoint", "crp", CreateRestorePoint ) )
				return;

			/////
			// Revert
			/////
			if ( RunAction( "revert", "rvt", Revert ) )
				return;

			/////
			// RevertTo
			/////
			if ( RunAction( "revertTo", "rvtto", RevertTo ) )
				return;

			/////
            // BackgroundTasks
            /////
            if (RunAction("backgroundTasks", "bt", BackgroundTasks))
                return;


            /////
			// Revert Range
			/////
			if ( RunAction( "revertRange", "rvtrng", RevertRange ) )
				return;

			/////
			// Get Last Transaction Id
			/////
			if ( RunAction( "getLastTransactionId", "gltid", GetLastTransactionId ) )
				return;

			/////
			// Enable change tracking
			/////
			if ( RunAction( "enableChangeTracking", "ect", EnableChangeTracking ) )
				return;

			/////
			// Disable change tracking
			/////
			if ( RunAction( "disableChangeTracking", "dct", DisableChangeTracking ) )
				return;

			/////
			// Help or no parameters
			/////
			if ( _parser.Count == 0 || _parser.ContainsArgument( @"help" ) || _parser.ContainsArgument( @"?" ) || _parser.ContainsArgument( @"-help" ) || _parser.ContainsArgument( "help" ) )
			{
				PrintHelp(full: _parser.Count != 0);
				return;
			}

			ConsoleLogger.WriteLine( @"Invalid Parameters specified." );
			ConsoleLogger.WriteLine( @"Use -help for further options" );
			ConsoleLogger.WriteLine( );
		}

		/// <summary>
		/// Disables the change tracking.
		/// </summary>
		[Function( "disableChangeTracking", "Disable datbase change tracking.", "ect" )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void DisableChangeTracking( )
		{
			DatabaseChangeTracking.Enabled = false;
		}

		/// <summary>
		/// Enables the change tracking.
		/// </summary>
		[Function( "enableChangeTracking", "Enable datbase change tracking.", "ect" )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void EnableChangeTracking( )
		{
			DatabaseChangeTracking.Enabled = true;
		}

		/// <summary>
		/// Gets the last transaction identifier.
		/// </summary>
		[Function( "getLastTransactionId", "Retrieves the latest transaction id.", "gltid" )]
		[FunctionArgument( "tenant", "is the name of the tenant whose transaction id is to be retrieved.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void GetLastTransactionId( )
		{
			var tenant = GetArgument<string>( "tenant" );

			long tenantId = -1;

			if ( !string.IsNullOrEmpty( tenant ) )
			{
				tenantId = TenantHelper.GetTenantId( tenant );
			}

			ConsoleLogger.WriteLine( DatabaseChangeTracking.GetLastTransactionId( tenantId ).ToString( ) );
		}

		/// <summary>
		/// Reverts the transaction id range.
		/// </summary>
		[Function( "revertRange", "Reverts the specified range of transaction ids.", "rvtrng" )]
		[FunctionArgument( "fromId", "is the start transaction id." )]
		[FunctionArgument( "toId", "is the end transaction id." )]
		[FunctionArgument( "tenant", "is the name of the tenant whose transaction range is to be reverted.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void RevertRange( )
		{
			var fromId = GetArgument<long>( "fromId" );
			var toId = GetArgument<long>( "toId" );
			var tenant = GetArgument<string>( "tenant" );

			long tenantId = -1;

			if ( !string.IsNullOrEmpty( tenant ) )
			{
				tenantId = TenantHelper.GetTenantId( tenant );
			}

			DatabaseChangeTracking.RevertRange( fromId, toId, tenantId );
		}

		/// <summary>
		/// Reverts to a specified transaction id.
		/// </summary>
		[Function( "revertTo", "Reverts to the specified transaction id.", "rvtto" )]
		[FunctionArgument( "transactionId", "is transaction id to revert to." )]
		[FunctionArgument( "tenant", "is the name of the tenant whose transactions are to be reverted.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void RevertTo( )
		{
			var transactionId = GetArgument<long>( "transactionId" );
			var tenant = GetArgument<string>( "tenant" );

			long tenantId = -1;

			if ( !string.IsNullOrEmpty( tenant ) )
			{
				tenantId = TenantHelper.GetTenantId( tenant );
			}

			DatabaseChangeTracking.RevertTo( transactionId, tenantId );
		}

		/// <summary>
		/// Reverts a transaction id.
		/// </summary>
		[Function( "revert", "Reverts the specified transaction id.", "rvt" )]
		[FunctionArgument( "transactionId", "is the transaction id to revert." )]
		[FunctionArgument( "tenant", "is the name of the tenant whose transaction id is to be reverted.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void Revert( )
		{
			var transactionId = GetArgument<long>( "transactionId" );
			var tenant = GetArgument<string>( "tenant" );

			long tenantId = -1;

			if ( !string.IsNullOrEmpty( tenant ) )
			{
				tenantId = TenantHelper.GetTenantId( tenant );
			}

			DatabaseChangeTracking.Revert( transactionId, tenantId );
		}

		/// <summary>
		/// Creates the restore point.
		/// </summary>
		[Function( "createRestorePoint", "Creates a system restore point.", "crp" )]
		[FunctionArgument( "tenant", "is the name of the tenant creating the system restore point.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "context", "is the context to associate with the transaction.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "userDefined", "whether the restore point is user defined.", "true", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "systemUpgrade", "whether the restore point references a system upgrade.", "false", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "revertTo", "whether the restore point references a 'revert to' operation.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void CreateRestorePoint( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var context = GetArgument<string>( "context" );
			var userDefinedString = GetArgument<string>( "userDefined" );
			var systemUpgradeString = GetArgument<string>( "systemUpgrade" );
			var revertToString = GetArgument<string>( "revertTo" );

			long tenantId = -1;

			if ( !string.IsNullOrEmpty( tenant ) )
			{
				tenantId = TenantHelper.GetTenantId( tenant );
			}

			bool systemUpgrade;
			bool.TryParse( systemUpgradeString, out systemUpgrade );

			bool userDefined;
			bool.TryParse( userDefinedString, out userDefined );

			long? revertTo = null;

			if ( !string.IsNullOrEmpty( revertToString ) )
			{
				long revertToVal;

				if ( long.TryParse( revertToString, out revertToVal ) )
				{
					revertTo = revertToVal;
				}
			}

			ConsoleLogger.WriteLine( DatabaseChangeTracking.CreateRestorePoint( context, tenantId, userDefined, systemUpgrade, revertTo ).ToString( ) );
		}

		/// <summary>
		///     Converts an application package from one format to another
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -convertApplicationPackage -source src -target trg -format fmt
		/// </remarks>
		[Function( "convertApplicationPackage", "Converts an application package from one format to another.", "cap" )]
		[FunctionArgument( "source", "is the location on disk of the source package to be converted." )]
		[FunctionArgument( "target", "is the location on disk of the target package." )]
		[FunctionArgument( "format", "is the format of the resulting file. [xml|sql]", "xml", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "overwrite", "overwrite the target if it exists.", "false", FunctionArgumentOptions.Optional )]
		private void ConvertApplicationPackage( )
		{
			var source = GetArgument<string>( "source" );
			var target = GetArgument<string>( "target" );
			var format = GetArgument<string>( "format" );
			var overwrite = GetArgument<bool>( "overwrite" );

            Format fileFormat = Helpers.GetFileFormat( format );

            using ( DatabaseContextInfo.SetContextInfo( $"Convert app package '{source}' to '{target}' ({format})" ) )
			{
				AppManager.ConvertApplicationPackage( source, target, fileFormat, overwrite );
			}
		}

		/// <summary>
		///     Exports an application from a tenant to a package (SqLite)
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -exportTenantApp -tenant tnt -app app -package pkg [-server svr] [-database db]
		/// </remarks>
		[Function( "exportTenantApp", "Exports the specified application for the specified tenant to the specified location on disk.", "eta" )]
		[FunctionArgument( "tenant", "refers to the tenant name that owns the package to export." )]
		[FunctionArgument( "app", "is the name|guid of the application that is to be exported." )]
		[FunctionArgument( "package", "is the location on disk the package is to be written to." )]
		[FunctionArgument( "format", "is the format of the resulting file.", "xml", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server on which the package will be imported.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database (catalog) where the package will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ExportTenantPackage( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var app = GetArgument<string>( "app" );
			var package = GetArgument<string>( "package" );
			var format = GetArgument<string>( "format" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

            Format fileFormat = Helpers.GetFileFormat( format );

            using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Export tenant '{tenant}' package '{app}'" ) )
			{
				AppManager.ExportAppPackage( tenant, app, package, fileFormat );
			}
		}

		/// <summary>
		///     Exports an application from the application library to a package (SqLite)
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -exportApp -app app -package pkg [-server svr] [-database db]
		/// </remarks>
		[Function( "exportApp", "Exports the specified application for the application library to the specified location on disk.", "ea" )]
		[FunctionArgument( "app", "is the guid of the application that is to be exported." )]
		[FunctionArgument( "package", "is the location on disk the package is to be written to." )]
		[FunctionArgument( "format", "is the format of the resulting file.", "xml", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server on which the package will be imported.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database (catalog) where the package will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ExportPackage( )
		{
			var app = GetArgument<Guid>( "app" );
			var package = GetArgument<string>( "package" );
			var format = GetArgument<string>( "format" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			Format fileFormat = Helpers.GetFileFormat( format );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Export app package '{app}'" ) )
			{
				AppManager.ExportAppPackage( app, package, fileFormat );
			}
		}

	    /// <summary>
		///     Exports the tenant.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -exportTenant -tenant tnt -package pkg [-server svr] [-database db]
		/// </remarks>
		[Function( "exportTenant", "Exports the specified tenant from the system.", "et" )]
		[FunctionArgument( "tenant", "is the name of the tenant to export." )]
		[FunctionArgument( "package", "is the location on disk the package is to be written to." )]
		[FunctionArgument( "server", "refers to the server on which the tenant currently resides.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the tenant currently resides.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ExportTenant( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var package = GetArgument<string>( "package" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Export tenant '{tenant}'" ) )
			{
				TenantManager.ExportTenant( tenant, package );
			}
        }

        /// <summary>
        ///     Exports the tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -exportTenant -tenant tnt -package pkg [-server svr] [-database db]
        /// </remarks>
        [Function("exportEntity", "Exports the specified entity from the system.", "ee")]
        [FunctionArgument("tenant", "is the name of the tenant to export from.")]
        [FunctionArgument("id", "is the ID number of the entity to export.")]
        [FunctionArgument("package", "is the location on disk the export is to be written to.")]
        [FunctionArgument("server", "refers to the server on which the tenant currently resides.", "localhost", FunctionArgumentOptions.Optional)]
        [FunctionArgument("database", "refers to the name of the database where the tenant currently resides.", "SoftwarePlatform", FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional)]
        private void ExportEntity()
        {
            var tenant = GetArgument<string>("tenant");
            var entity = GetArgument<long>("id");
            var package = GetArgument<string>("package");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

            using (DatabaseInfo.Override(server, database, dbUser, dbPassword))
            {
                EntityManager.ExportEntity(tenant, entity, package);
            }
        }



        /// <summary>
        ///     Grants the tenant permission to install/repair/upgrade the app.
        /// </summary>
        [Function( "grantInstall", "Grants the tenant permission to install/repair/upgrade the app.", "gi" )]
        [FunctionArgument( "tenant", "refers to the tenant name that owns the package to export." )]
        [FunctionArgument( "app", "is the name|guid of the application that is to be exported." )]
        [FunctionArgument( "server", "refers to the server on which the package will be imported.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database (catalog) where the package will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void GrantInstall( )
        {
            var tenant = GetArgument<string>( "tenant" );
            var app = GetArgument<string>( "app" );
            var server = GetArgument<string>( "server" );
            var database = GetArgument<string>( "database" );
            var dbUser = GetArgument<string>( "dbUser" );
            var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Grant install to app '{app}' for '{tenant}'" ) )
			{
				ApplicationAccess.ChangeAppAccess( tenant, app, AppPermission.Install, true );
			}
        }

        /// <summary>
        ///     Ungrants the tenant permission to install/repair/upgrade the app.
        /// </summary>
        [Function( "denyInstall", "Ungrants the tenant permission to install/repair/upgrade the app.", "di" )]
        [FunctionArgument( "tenant", "refers to the tenant name that owns the package to export." )]
        [FunctionArgument( "app", "is the name|guid of the application that is to be exported." )]
        [FunctionArgument( "server", "refers to the server on which the package will be imported.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database (catalog) where the package will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void DenyInstall( )
        {
            var tenant = GetArgument<string>( "tenant" );
            var app = GetArgument<string>( "app" );
            var server = GetArgument<string>( "server" );
            var database = GetArgument<string>( "database" );
            var dbUser = GetArgument<string>( "dbUser" );
            var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Deny install to app '{app}' for '{tenant}'" ) )
			{
				ApplicationAccess.ChangeAppAccess( tenant, app, AppPermission.Install, false );
			}
        }

        /// <summary>
        ///     Denies the tenant permission to modify the app.
        /// </summary>
        [Function("denyModifyApp", "Denies the tenant permission to modify the application.", "dma")]
        [FunctionArgument( "tenant", "refers to the tenant name that owns the package." )]
        [FunctionArgument( "app", "is the name of the application that is to be modified." )]
        [FunctionArgument( "server", "refers to the server on which the tenant currently resides.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database where the tenant currently resides.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void DenyModifyApp()
        {
            var tenant = GetArgument<string>("tenant");
            var app = GetArgument<string>("app");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Deny modify app '{app}' for '{tenant}'" ) )
			{
				ApplicationAccess.ChangeTenantApplicationCanModify( tenant, app, false );
			}
        }

        /// <summary>
        ///     Grants the tenant permission to modify the app.
        /// </summary>
        [Function("grantModifyApp", "Grants the tenant permission to modify the application.", "gma")]
        [FunctionArgument( "tenant", "refers to the tenant name that owns the package." )]
        [FunctionArgument( "app", "is the name of the application that is to be modified." )]        
        [FunctionArgument( "server", "refers to the server on which the tenant currently resides.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database where the tenant currently resides.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void GrantModifyApp()
        {
            var tenant = GetArgument<string>("tenant");
            var app = GetArgument<string>("app");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Grant modify app '{app}' to '{tenant}'" ) )
			{
				ApplicationAccess.ChangeTenantApplicationCanModify( tenant, app, true );
			}
        }

        /// <summary>
        ///     Grants the tenant permission to install/repair/upgrade the app.
        /// </summary>
        [Function( "grantPublish", "Grants the tenant permission to publish/export.", "gp" )]
        [FunctionArgument( "tenant", "refers to the tenant name that owns the package to export." )]
        [FunctionArgument( "app", "is the name|guid of the application that is to be exported." )]
        [FunctionArgument( "server", "refers to the server on which the package will be imported.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database (catalog) where the package will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void GrantPublish( )
        {
            var tenant = GetArgument<string>( "tenant" );
            var app = GetArgument<string>( "app" );
            var server = GetArgument<string>( "server" );
            var database = GetArgument<string>( "database" );
            var dbUser = GetArgument<string>( "dbUser" );
            var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Grant publish for app '{app}' to '{tenant}'" ) )
			{
				ApplicationAccess.ChangeAppAccess( tenant, app, AppPermission.Publish, true );
			}
        }

        /// <summary>
        ///     Ungrants the tenant permission to publish/export.
        /// </summary>
        [Function( "denyPublish", "Ungrants the tenant permission to publish/export.", "dp" )]
        [FunctionArgument( "tenant", "refers to the tenant name that owns the package to export." )]
        [FunctionArgument( "app", "is the name|guid of the application that is to be exported." )]
        [FunctionArgument( "server", "refers to the server on which the package will be imported.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the database (catalog) where the package will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void DenyPublish( )
        {
            var tenant = GetArgument<string>( "tenant" );
            var app = GetArgument<string>( "app" );
            var server = GetArgument<string>( "server" );
            var database = GetArgument<string>( "database" );
            var dbUser = GetArgument<string>( "dbUser" );
            var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Deny publish for app '{app}' to '{tenant}'" ) )
			{
				ApplicationAccess.ChangeAppAccess( tenant, app, AppPermission.Publish, false );
			}
        }

        /// <summary>
		/// Gets the argument.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException"></exception>
		private T GetArgument<T>( string argumentName )
        {
	        return _attributeArgParser.GetArgument<T>( _parser, argumentName );
		}

		/// <summary>
		///     Imports an application from a package (SqLite) to the app library.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -importApp -package pkg [-server svr] [-database db]
		/// </remarks>
		[Function( "importApp", "Imports the specified package from the file-system to the application library.", "ia" )]
		[FunctionArgument( "package", "is the path to the .db package on the file-system." )]
		[FunctionArgument( "server", "refers to the server on which the package will be imported.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database (catalog) where the package will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ImportAppPackage( )
		{
			var packagePath = GetArgument<string>( "package" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Import app '{packagePath}'" ) )
			{
				AppManager.ImportAppPackage( packagePath );
			}

			Database.RebuildFullTextCatalog( server, database, false );
        }

        /// <summary>
        ///     Exports the tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -exportTenant -tenant tnt -package pkg [-server svr] [-database db]
        /// </remarks>
        [Function("importEntity", "Imports the specified entity into the system.", "ie")]
        [FunctionArgument("tenant", "is the name of the tenant to import into.")]
        [FunctionArgument("package", "is the location on disk the export is to be written to.")]
        [FunctionArgument("server", "refers to the server on which the tenant currently resides.", "localhost", FunctionArgumentOptions.Optional)]
        [FunctionArgument("database", "refers to the name of the database where the tenant currently resides.", "SoftwarePlatform", FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional)]
        private void ImportEntity()
        {
            var tenant = GetArgument<string>("tenant");
            var package = GetArgument<string>("package");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

            using (DatabaseInfo.Override(server, database, dbUser, dbPassword))
            {
                EntityManager.ImportEntity(tenant, package);
            }
        }

        /// <summary>
        ///     Imports the tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -importTenant -package pkg -tenant tnt [-server svr] [-database db]
        /// </remarks>
        [Function( "importTenant", "Imports the specified tenant from the file-system.", "it" )]
		[FunctionArgument( "package", "is the location on disk the package is to be read from." )]
		[FunctionArgument( "tenant", "is the optional name of the tenant to import. If not specified, the tenant name stored in the package will be used.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server where the tenant will be imported.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the tenant will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ImportTenant( )
		{
			var package = GetArgument<string>( "package" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			if ( !File.Exists( package ) )
			{
				throw new FileNotFoundException( "Package not found" );
			}

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Import tenant '{tenant}'" ) )
			{
				TenantManager.ImportTenant( package, tenant );
			}
		}

		/// <summary>
		///     Installs the solution.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -installSolution [-solution sln] [-server svr] [-database db]
		/// </remarks>
		[Function( "installSolution", "Installs the specified solution from Xml and rebuilds the full text catalog.", "is" )]
		[FunctionArgument( "solution", "is the path to the xml solution file to install.", "C:\\Program Files\\EDC\\SoftwarePlatform\\Solutions\\Core\\Solution.xml", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server on which the solution will be installed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the new database the solution will be installed on.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void InstallSolution( )
		{
			var solutionName = GetArgument<string>( "solution" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			if ( !File.Exists( solutionName ) )
			{
				throw new FileNotFoundException( "The specified solution file cannot be found." );
			}

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Install solution '{solutionName}'" ) )
			{

				Solution.InstallSolution( solutionName );

				/////
				// Perform the report migration for the solution
				/////
				ReportMigration.ConvertReports( );
			}

			Database.RebuildFullTextCatalog( server, database, false );
		}

		/// <summary>
		///		Installs the bootstrap solution.
		/// </summary>
		[Function( "installBootstrap", "Installs the bootstrap solution if not already installed.", "ib" )]
		[FunctionArgument( "path", "is the folder location on disk where the bootstrap solution resides." )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void InstallBootstrap( )
		{
			FrontEndArgumentParser argumentParser = new FrontEndArgumentParser( _parser, _attributeArgParser );

			using ( FrontEnd frontEnd = new FrontEnd( argumentParser ) )
			using ( DatabaseContextInfo.SetContextInfo( "Install bootstrap" ) )
			{
				frontEnd.EnsureBootstrapSolutionExists( );
			}
		}

		/// <summary>
		///		Upgrades the bootstrap solution.
		/// </summary>
		[Function( "upgradeBootstrap", "Upgrades the bootstrap solution.", "ub" )]
		[FunctionArgument( "path", "is the folder location on disk where the bootstrap solution resides." )]
		[FunctionArgument( "databaseServer", "is the name of the database server." )]
		[FunctionArgument( "databaseCatalog", "is the name of the database catalog." )]
		private void UpgradeBootstrap( )
		{
			FrontEndArgumentParser argumentParser = new FrontEndArgumentParser( _parser, _attributeArgParser );

			using ( FrontEnd frontEnd = new FrontEnd( argumentParser ) )
			using ( DatabaseContextInfo.SetContextInfo( "Upgrade bootstrap" ) )
			{
				frontEnd.UpgradeBootstrapSolution( );
			}
		}

		/// <summary>
		///		Sets the integration test mode.
		/// </summary>
		/// <param name="value">if set to <c>true</c> turns integration test mode on.</param>
		private void IntgTestMode( bool value )
		{
			ServerConfiguration configSection = ConfigurationSettings.GetServerConfigurationSection( );
			configSection.Security.IntegratedTestingModeEnabled = value;

			ConfigurationSettings.UpdateServerConfigurationSection( configSection );
		}

		/// <summary>
		///     Turn off integrated testing mode
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -intgTestModeOff
		/// </remarks>
		[Function( "intgTestModeOff", "Turn off integrated testing mode." )]
		private void IntgTestModeOff( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "Integration test mode off" ) )
			{
				IntgTestMode( false );
			}
		}

		/// <summary>
		///     Turn on integrated testing mode
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -intgTestModeOn
		/// </remarks>
		[Function( "intgTestModeOn", "Turn on integrated testing mode." )]
		private void IntgTestModeOn( )
		{
			using ( DatabaseContextInfo.SetContextInfo( $"Integration test most on" ) )
			{
				IntgTestMode( true );
			}
		}

		/// <summary>
		///     Lists the applications.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -listApplications -all all [-server svr] [-database db]
		/// </remarks>
		[Function( "listApplications", "Lists the applications currently stored in the application library.", "la" )]
		[FunctionArgument( "all", "Lists all application versions instead of only latest.", "false", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server on which the applications will be listed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the new database the applications will be listed on.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ListApps( )
		{
			var listAll = GetArgument<bool>( "all" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( "List apps" ) )
			{
				List<AppData> apps = AppManager.ListApps( listAll );

				DumpApps( apps, listAll );
			}
		}

		/// <summary>
		///     Lists the tenant applications.
		/// </summary>
		[Function( "listTenantApplications", "Lists the applications currently assigned to the specified tenant.", "lta" )]
		[FunctionArgument( "tenant", "specified the name of the tenant whose applications are to be listed." )]
		[FunctionArgument( "server", "refers to the server on which the tenant applications will be listed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the new database the tenant applications will be listed on.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ListTenantApps( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( "List tenant apps" ) )
			{
				List<AppData> apps = AppManager.ListTenantApps( tenant );

				DumpApps( apps );
			}
		}

        /// <summary>
        ///     Lists the tenant applications.
        /// </summary>
        [Function( "listAppAccess", "Lists the level of access that a tenant has to each application.", "laa" )]
        [FunctionArgument( "tenant", "specified the name of the tenant whose access is to be listed." )]
        [FunctionArgument( "server", "refers to the server on which the tenant applications will be listed.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the new database the tenant applications will be listed on.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        private void ListAppAccess( )
        {
            var tenant = GetArgument<string>( "tenant" );
            var server = GetArgument<string>( "server" );
            var database = GetArgument<string>( "database" );
            var dbUser = GetArgument<string>( "dbUser" );
            var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"List app access for tenant '{tenant}'" ) )
			{
				List<AppData> apps = AppManager.ListAppAccess( tenant );

				ConsoleLogger.WriteLine( @"{0,-30} {1,-10} {2,-10}", @"Application", @"CanInstall", @"CanPublish" );
				ConsoleLogger.WriteLine( @"{0,-30} {1,-10} {2,-10}", @"-----------", @"----------", @"----------" );
				foreach ( var app in apps.OrderBy( a => a.Name ) )
				{
					ConsoleLogger.WriteLine( @"{0,-30} {1,-10} {2,-10}", app.Name, app.HasInstallPermission ? "yes" : "no", app.HasPublishPermission ? "yes" : "no" );
				}
			}
        }

		/// <summary>
		///     Lists the tenants.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -listTenants [-server svr] [-database db]
		/// </remarks>
		[Function( "listTenants", "Lists the tenants available on the server.", "lt" )]
		[FunctionArgument( "server", "refers to the server on which the tenants will be listed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the new database where the tenants will be listed.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ListTenants( )
		{
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"List tenants" ) )
			{
				IEnumerable<string> tenants = TenantManager.GetTenants( );

				foreach ( string tenant in tenants )
				{
					ConsoleLogger.WriteLine( tenant );
				}
			}
		}

		/// <summary>
		///     Lists the users.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -listUsers -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "listUsers", "Lists the users that belong to the specified tenant.", "lu" )]
		[FunctionArgument( "tenant", "is the name of the tenant whose users are to be retrieved." )]
		[FunctionArgument( "server", "refers to the server on which the tenants users will be listed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the new database where the tenants users will be listed.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ListUsers( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"List users" ) )
			{
				IEnumerable<string> users = TenantManager.GetUsers( tenant );

				foreach ( string user in users )
				{
					ConsoleLogger.WriteLine( user );
				}
			}
		}

        /// <summary>
        ///     List and turn on and off feature switches.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -featureSwitch [-tenant tnt] [-server svr] [-database db] [-feature]
        /// </remarks>
        [Function("featureSwitch", "List and turn on/off feature switches.", "fs")]
        [FunctionArgument( "tenant", "is the name of the tenant whose feature switches are to be toggled." )]
        [FunctionArgument( "server", "refers to the server on which the tenant feature switches are to be toggled.", "localhost", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "database", "refers to the name of the new database where the tenant feature switches are to be toggled.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "list", "A list of features with optional values. If a value is not given true is assumed: 'feature1,feature2=true,feature3=false'", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "feature", "the alias of the feature", null, FunctionArgumentOptions.Optional )]
        [FunctionArgument( "set", "set a feature", null, FunctionArgumentOptions.Optional )]
        private void FeatureSwitch()
        {
            var tenant = GetArgument<string>("tenant");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");
            var feature = GetArgument<string>("feature");
            var set = GetArgument<bool?>("set");
            var featureList = GetArgument<string>("list");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Feature swtch - feature: {feature}, set: {set}, list: {featureList}" ) )
			{
				var featureSwitch = Factory.Current.Resolve<IFeatureSwitch>( );

				if ( feature == null && featureList == null )
				{
					ListFeatures( featureSwitch, tenant );
				}
				else
				{
					var tenantId = TenantHelper.GetTenantId( tenant );

					using ( new TenantAdministratorContext( tenantId ) )
					{
						if ( featureList != null )
						{
							try
							{
								featureSwitch.Set( featureList );
							}
							catch ( ArgumentException ex )
							{
								ConsoleLogger.WriteLine( ex.Message );
							}
						}
						else if ( set != null )
						{
							featureSwitch.Set( feature, ( bool ) set );
						}
						else
						{
							ConsoleLogger.WriteLine( @"Invalid Parameters specified." );
							ConsoleLogger.WriteLine( @"Use -help for further options" );
							ConsoleLogger.WriteLine( );
						}
					}
				}
			}
        }

        void ListFeatures(IFeatureSwitch featureSwitch, string tenant)
        {
            ConsoleLogger.WriteLine();
            ConsoleLogger.WriteLine(@"Feature switches:");

            var tenantId = TenantHelper.GetTenantId(tenant);

            using (new TenantAdministratorContext(tenantId))
            {
                ListFeatures(featureSwitch);
            }

        }

        void ListFeatures(IFeatureSwitch featureSwitch)
        {
            foreach (var feature in featureSwitch.List())
            {
                var fieldValue = featureSwitch.Get(feature.Name);
                ConsoleLogger.WriteLine(@"	{0}: 	{1}: 	{2}", feature.Name, fieldValue, feature.Description);
            }
        }


        /// <summary>
        ///     Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main( string[ ] args )
        {
			try
	        {
		        IEventLog eventLog = null;

		        try
		        {
			        eventLog = EventLog.Application;
		        }
		        catch ( DirectoryNotFoundException ex )
		        {
			        // Handle cases where PlatformConfigure.exe is run on a machine without the product installed.
			        Trace.TraceError( "An error occurred getting the event log. Error: {0}", ex );
		        }

		        // The app exit rendezvous point will wait a maximum of 30 seconds for actions to complete.
		        using ( new RendezvousPoint( WellKnownRendezvousPoints.ApplicationExit, 30000, eventLog ) )
		        {
			        var p = new Program( args );
			        p.Execute( );
		        }
	        }
	        catch ( DirectoryNotFoundException e )
	        {
				/////
				// Handle cases where PlatformConfigure.exe is run on a machine without the product installed.
				/////
				Trace.TraceError( e.Message );
			}
	        catch ( Exception e )
	        {
		        ConsoleLogger.WriteLine( );

		        Exception ex = e;

		        while ( ex != null )
		        {
					ConsoleLogger.WriteError( ex.Message );

			        ex = ex.InnerException;
		        }

				ConsoleLogger.WriteLine( );

		        return 1;
	        }
	        finally
	        {
		        try
		        {
                    if (SchedulingHelper.HasInstanceBeenCreated())
                    {
					var instance = SchedulingHelper.Instance;

			        instance?.Shutdown( true ); // shutdown the instance so that no threads are left running
		        }
		        }
		        catch ( Exception ex )
		        {
			        // Handle cases where PlatformConfigure.exe is run on a machine without the product installed.
			        Trace.TraceError( "An error occurred stopping the scheduler (occurs if platform is not installed). Error: {0}", ex );
		        }
	        }

			return 0;
		}

        /// <summary>
        ///     Flush the caches for a tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -flushCaches -tenant tnt [-server svr] [-database db]
        /// </remarks>
        [Function("flushCaches", "Flush the caches for the specified tenant.", "fc")]
        [FunctionArgument("tenant", "name of the tenant to flush.", "", FunctionArgumentOptions.Optional )]
        [FunctionArgument("server", "refers to the server where the tenant will be imported.", "localhost", FunctionArgumentOptions.Optional)]
        [FunctionArgument("database", "refers to the name of the database where the tenant will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional)]
        private void FlushCaches()
        {
            var tenant = GetArgument<string>("tenant");
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Flush caches" ) )
			{
                if ( string.IsNullOrEmpty( tenant ) )
                {
				    CacheManager.ClearCaches( );
			    }
                else
                {
                    long tenantId = TenantHelper.GetTenantId( tenant );
                    CacheManager.ClearCaches( tenantId );
                }
            }
        }

        /// <summary>
        ///     Overwrites the tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -overwriteTenant -package pkg [-tenant tnt] [-server svr] [-database db]
        /// </remarks>
        [Function( "overwriteTenant", "Imports the specified tenant from the file-system overwriting any existing tenant with the same name.", "ot" )]
		[FunctionArgument( "package", "is the location on disk the package is to be read from." )]
		[FunctionArgument( "tenant", "is the optional name of the tenant to import. If not specified, the tenant name stored in the package will be used.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server where the tenant will be imported.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database where the tenant will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void OverwriteTenant( )
		{
			var package = GetArgument<string>( "package" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			if ( !File.Exists( package ) )
			{
				throw new FileNotFoundException( "Package not found" );
			}

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Overwrite tenant '{tenant}' - Package: '{package}'" ) )
			{
				TenantManager.OverwriteTenant( package, tenant );
			}
		}

		/// <summary>
        ///     Flush the caches for a tenant.
        /// </summary>
        /// <remarks>
        ///     Command line parameters
        ///     -flushCaches -tenant tnt [-server svr] [-database db]
        /// </remarks>
        [Function("backgroundTasks", "Deal with background tasks", "bt")]
        [FunctionArgument("server", "refers to the server where the tenant will be imported.", "localhost", FunctionArgumentOptions.Optional)]
        [FunctionArgument("database", "refers to the name of the database where the tenant will be imported.", "SoftwarePlatform", FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional)]
        [FunctionArgument("dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional)]
        private void BackgroundTasks()
        {
            var server = GetArgument<string>("server");
            var database = GetArgument<string>("database");
            var dbUser = GetArgument<string>("dbUser");
            var dbPassword = GetArgument<string>("dbPassword");

            using (DatabaseInfo.Override(server, database, dbUser, dbPassword))
            {
                var queueLengths = Factory.BackgroundTaskManager.QueueLengths();


                ConsoleLogger.WriteLine($@"Queues: ");
                ConsoleLogger.WriteLine($@"Tenant       Id   Length  Name");

                foreach (var entry in queueLengths)
                {
                    ConsoleLogger.WriteLine($@"{entry.TenantName,-9} {entry.TenantId, 5} {entry.Length, 8}  ""{entry.QueueName}""");
                }
            }


        }

        /// <summary>
		///		Prints the help.
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="full">if set to <c>true</c> [full].</param>
		private void PrintHelp( string topic = null, bool full = true )
		{
			ConsoleLogger.WriteLine( @"" );
			ConsoleLogger.WriteLine( @"ReadiNow Software Platform Configuration Utility" );
			ConsoleLogger.WriteLine( @"Copyright 2011-2016 Global Software Innovation Pty Ltd." );
			ConsoleLogger.WriteLine( @"" );
			ConsoleLogger.WriteLine( @"" );
			ConsoleLogger.WriteLine( @"Usage: PlatformConfigure <command> [ <options> ]" );
			ConsoleLogger.WriteLine( @"" );
			ConsoleLogger.WriteLine( @"Commands:" );
			ConsoleLogger.WriteLine( @"" );

			var type = MethodBase.GetCurrentMethod( ).DeclaringType;

			if ( type == null )
			{
				return;
			}

			var methods = type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

			foreach ( var method in methods.OrderBy( method => method.Name ) )
			{
				var functionAttribute = method.GetCustomAttribute<FunctionAttribute>( );

				if ( functionAttribute == null || ( !string.IsNullOrEmpty( topic ) && !functionAttribute.Name.Equals( topic, StringComparison.CurrentCultureIgnoreCase ) ) )
				{
					continue;
				}

				var functionArgumentAttributes = method.GetCustomAttributes<FunctionArgumentAttribute>( ).ToList( );

				var commandLine = new List<string>
				{
					functionAttribute.ToString( )
				};

				commandLine.AddRange( functionArgumentAttributes.Select( functionArgumentAttribute => functionArgumentAttribute.ToString( ) ) );

				PrintHelpLine( commandLine, 2, 4, ConsoleColor.White );

                if (full)
                {

                    if (!string.IsNullOrEmpty(functionAttribute.Description))
                    {
                        var descriptionLine = new List<string>(functionAttribute.Description.Split(' '));

                        PrintHelpLine(descriptionLine, 4, 0, ConsoleColor.DarkGreen);
                    }

                    if (functionArgumentAttributes.Count > 0)
                    {
                        int padding = functionArgumentAttributes.Max(arg => arg.Name.Length + 3);

                        foreach (var functionArgumentAttribute in functionArgumentAttributes)
                        {
                            var argumentLine = new List<string>(functionArgumentAttribute.ToLongString(padding).Split(' '));

                            PrintHelpLine(argumentLine, 6, padding);
                        }
                    }
                }
               
                ConsoleLogger.WriteLine( );
            }

            if (!full)
            {
	            List<string> commandLine = new List<string>
	            {
		            "-help"
	            };

	            PrintHelpLine( commandLine, 2, 4, ConsoleColor.White );
            }
        }

		/// <summary>
		///     Prints the help line.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="indent">The indent.</param>
		/// <param name="overflowIntent">The overflow intent.</param>
		/// <param name="color">The color.</param>
		private void PrintHelpLine( List<string> values, int indent, int overflowIntent, ConsoleColor color = ConsoleColor.Gray )
		{
			var line = new StringBuilder( );
			bool overflow = false;

			for ( int i = 0; i < values.Count; i++ )
			{
				string value = values[ i ];

				bool newLine = false;

				if ( value.Contains( "\r\n" ) )
				{
					var strings = value.Split( new[ ]
					{
						"\r\n"
					}, StringSplitOptions.RemoveEmptyEntries );
					value = strings[ 0 ];

					if ( strings.Length > 1 )
					{
						newLine = true;
						values[ i ] = string.Join( "\r\n", strings, 1, strings.Length - 1 );
					}
				}

				if ( line.Length == 0 )
				{
					line.Append( !overflow ? new String( ' ', indent ) : new String( ' ', indent + overflowIntent ) );

					line.Append( value );
				}
				else if ( line.Length + value.Length >= 79 )
				{
					if ( newLine )
					{
						ConsoleLogger.WriteLine( line.ToString( ), color );
						line.Length = 0;
						line.Append( new String( ' ', indent + overflowIntent ) );
						line.Append( value );
					}
					else
					{
						newLine = true;
					}
				}
				else
				{
					line.Append( " " + value );
				}

				if ( newLine )
				{
					ConsoleLogger.WriteLine( line.ToString( ), color );
					line.Length = 0;
					i--;
					overflow = true;
				}
			}

			if ( line.Length > 0 )
			{
				ConsoleLogger.WriteLine( line.ToString( ), color );
			}
		}

		/// <summary>
		///		Set the interval for processing inboxes.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -processInboxInterval [-interval int]
		/// </remarks>
		/// <exception cref="CommandLineArgException">
		///     @Invalid interval. Interval must be an
		///     integer between 0 and 60.
		/// </exception>
		[Function( "processInboxInterval", "Set the integrated SoftwarePlatform in-box processing schedule." )]
		[FunctionArgument( "interval", "The interval (in minutes) to check the inbox.", "5", FunctionArgumentOptions.Optional )]
		private void ProcessInboxInterval( )
		{
			var interval = GetArgument<int>( "interval" );

			if ( interval > 0 && interval <= 59 )
			{
				using ( new GlobalAdministratorContext( ) )
				using ( DatabaseContextInfo.SetContextInfo( $"Set process inbox interval to '{interval}'" ) )
				{
					var scheduleProcessInbox = EDC.ReadiNow.Model.Entity.Get<EDC.ReadiNow.Model.Entity>( "core:scheduleProcessInbox", true );
					var cron = $"0 0/{interval} * * * ?";
					scheduleProcessInbox.SetField( "core:cronDefinition", cron );
					scheduleProcessInbox.Save( );
				}
			}
			else
			{
				throw new CommandLineArgException( @"Invalid interval. Interval must be an integer between 0 and 60." );
			}
		}

		/// <summary>
		///		Turns processing of inboxes on.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -processInboxOn
		/// </remarks>
		[Function( "processInboxOn", "Turn integrated SoftwarePlatform in-box processing on." )]
		private void ProcessInboxOn( )
		{
			using ( DatabaseContextInfo.SetContextInfo( $"Process inbox on" ) )
			{
				ProcessInboxSet( true );
			}
		}

		/// <summary>
		///		Turns processing of inboxes off.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -processInboxOff
		/// </remarks>
		[Function( "processInboxOff", "Turn integrated SoftwarePlatform in-box processing off." )]
		private void ProcessInboxOff( )
		{
			using ( DatabaseContextInfo.SetContextInfo( $"Process inbox off" ) )
			{
				ProcessInboxSet( false );
			}
		}

		/// <summary>
		///     Turn on and off the processing of the SoftwarePlatform inbox
		/// </summary>
		private void ProcessInboxSet( bool isOn )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				EDC.ReadiNow.Model.IEntity processInboxesAction = EDC.ReadiNow.Model.Entity.Get<EDC.ReadiNow.Model.Entity>( "core:processInboxInstance", true );
				processInboxesAction.SetField( "core:triggerEnabled", isOn );
				processInboxesAction.Save( );
			}
		}

		/// <summary>
		///     Provisions the tenant.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -provisionTenant -tenant tnt [-all all] [-server svr] [-database db]
		/// </remarks>
		[Function("provisionTenant", "Creates a new tenant on the system with the specified name and deploys the core applications (core/console/coreData) to the tenant.", "pt" )]
		[FunctionArgument( "tenant", "is the name of the new tenant." )]
		[FunctionArgument( "all", "deploy all applications found in the library to the new tenant.", "false", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "updateStats", "whether statistics should be updated.", "True", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "disableFts", "whether full-text search should be disabled.", "False", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "server", "refers to the server on which the tenant will be provisioned.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the tenant will be provisioned.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ProvisionTenant( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var all = GetArgument<bool>( "all" );
			var updateStats = GetArgument<bool>( "updateStats" );
			var disableFts = GetArgument<bool>( "disableFts" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Provision tenant '{tenant}'" ) )
			{
				ConsoleLogger.WriteHeading( "Creating tenant '" + tenant + "'" );
				Tenant.CreateTenant( tenant );

				ConsoleLogger.WriteHeading( "Deploying Core" );
				AppManager.DeployApp( tenant, Applications.CoreApplicationId.ToString( "B" ), null, null, updateStats, disableFts );

				InvalidateTenant( tenant );

				ConsoleLogger.WriteHeading( "Deploying Console" );
				AppManager.DeployApp( tenant, Applications.ConsoleApplicationId.ToString( "B" ), null, null, updateStats, disableFts );

				InvalidateTenant( tenant );

				ConsoleLogger.WriteHeading( "Deploying Core Data" );
				AppManager.DeployApp( tenant, Applications.CoreDataApplicationId.ToString( "B" ), null, null, updateStats, disableFts );

				InvalidateTenant( tenant );

				if ( all )
				{
					try
					{
						ConsoleLogger.WriteHeading( "Deploying Shared" );
						AppManager.DeployApp( tenant, Applications.SharedApplicationId.ToString( "B" ), null, null, updateStats, disableFts );

						InvalidateTenant( tenant );
					}
					catch ( Exception exc )
					{
						ConsoleLogger.WriteLine( $"Failed to deploy Shared.\n{exc.Message}", ConsoleColor.DarkRed );
					}

					try
					{
						ConsoleLogger.WriteHeading( "Deploying Test" );
						AppManager.DeployApp( tenant, Applications.TestApplicationId.ToString( "B" ), null, null, updateStats, disableFts );

						InvalidateTenant( tenant );
					}
					catch ( Exception exc )
					{
						ConsoleLogger.WriteLine( $"Failed to deploy Test.\n{exc.Message}", ConsoleColor.DarkRed );
					}

					List<AppData> apps = AppManager.ListApps( );

					foreach ( AppData app in apps )
					{
						if ( app.ApplicationId != Applications.CoreApplicationId && app.ApplicationId != Applications.ConsoleApplicationId && app.ApplicationId != Applications.CoreDataApplicationId && app.ApplicationId != Applications.SharedApplicationId && app.ApplicationId != Applications.TestApplicationId )
						{
							try
							{
								ConsoleLogger.WriteHeading( "Deploying " + app.Name );
								AppManager.DeployApp( tenant, app.ApplicationId.ToString( "B" ), null, null, updateStats, disableFts );

								InvalidateTenant( tenant );
							}
							catch ( Exception exc )
							{
								ConsoleLogger.WriteLine( $"Failed to deploy {app.Name}.\n{exc.Message}", ConsoleColor.DarkRed );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Renames the tenant.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -renameTenant -tenant tnt -name newName [-server svr] [-database db]
		/// </remarks>
		[Function( "renameTenant", "Renames an existing tenant.", "rt" )]
		[FunctionArgument( "tenant", "is the name of the existing tenant." )]
		[FunctionArgument( "name", "new name of the tenant." )]
		[FunctionArgument( "server", "refers to the server on which the tenant will be renamed.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the tenant will be renamed.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void RenameTenant( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var name = GetArgument<string>( "name" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Rename tenant '{tenant}' to '{name}'" ) )
			{
				Tenant.RenameTenant( tenant, name );

				ConsoleLogger.WriteLine( @"Successfully renamed tenant '{0}' to '{1}'.", tenant, name );
			}
		}

		/// <summary>
		///     Publishes an application from a tenant to the app library.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -publishApp -tenant tnt -app app [-server svr] [-database db]
		/// </remarks>
		[Function("publishApp", "Publishes the specified application from the specified tenant to the application library.", "pa" )]
		[FunctionArgument( "tenant", "is the name of the tenant whose application is being published." )]
		[FunctionArgument( "app", "refers to the name|guid of the application to be published." )]
		[FunctionArgument( "server", "refers to the server on which the application will be published.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the application will be published.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void PublishApp( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var app = GetArgument<string>( "app" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Publish app '{app}' for tenant '{tenant}'" ) )
			{
				AppManager.PublishApp( tenant, app );
			}
		}

		/// <summary>
		///     Repairs a tenants install of an application.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -repairApp -tenant tnt -app app [-server svr] [-database db]
		/// </remarks>
		[Function( "repairApp", "Repairs the specified application for the specified tenant.", "ra" )]
		[FunctionArgument( "tenant", "is the name of the tenant whose application is to be repaired." )]
		[FunctionArgument( "app", "refers to the name|guid of the application to be repaired." )]
		[FunctionArgument( "server", "refers to the server on which the application will be repaired.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the application will be repaired.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void RepairApp( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var app = GetArgument<string>( "app" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Repair app '{app}' for tenant '{tenant}'" ) )
			{
				AppManager.RepairApp( tenant, app );
			}

		    InvalidateTenant(tenant);
		}

		/// <summary>
		///     Runs the action.
		/// </summary>
		/// <param name="actionName">Name of the action.</param>
		/// <param name="shortcut">The shortcut.</param>
		/// <param name="action">The action.</param>
		/// <returns></returns>
		private bool RunAction( string actionName, string shortcut, Action action )
		{
			if ( string.IsNullOrEmpty( actionName ) )
			{
				return false;
			}

			if ( !_parser.ContainsArgument( actionName ) && ( string.IsNullOrEmpty( shortcut ) || !_parser.ContainsArgument( shortcut ) ) )
			{
				return false;
			}

			if ( _parser.ContainsArgument( @"help" ) || _parser.ContainsArgument( @"?" ) || _parser.ContainsArgument( @"-help" ) || _parser.ContainsArgument( "help" ) )
			{
				PrintHelp( actionName );
				return true;
			}

		    DeferredChannelMessageContext deferredContext = null;

            try
            {
                deferredContext = new DeferredChannelMessageContext();
            }
            catch
            {
                // Ignore
                // Handle cases where platform configure is run on machines without the platform installed
            }

            using (deferredContext)
		    {
                action();
            }
                
			return true;
		}

		/// <summary>
		///     Sets the password.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -setPassword -user usr -tenant tnt -password pwd [-server svr] [-database db]
		/// </remarks>
		[Function( "setPassword", "Sets the password for the specified user.", "sp" )]
		[FunctionArgument( "user", "is the name of the user whose password is to be set." )]
		[FunctionArgument( "tenant", "is the name of the tenant that the user belongs to." )]
		[FunctionArgument( "password", "is the password to set." )]
		[FunctionArgument( "server", "refers to the server on which the password will be set.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the password will be set.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void SetPassword( )
		{
			var user = GetArgument<string>( "user" );
			var tenant = GetArgument<string>( "tenant" );
			var password = GetArgument<string>( "password" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Set password for user '{user}' int tenant '{tenant}'" ) )
			{
				ConsoleLogger.WriteLine( TenantManager.SetUserPassword( user, tenant, password ) ? @"Password successfully changed." : @"Password change failed." );
			}
		}

		/// <summary>
		///     Reset the account login attempts to zero.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -resetAccountLogins -user usr -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "resetAccountLogins", "Resets the account logins to zero.", "resetact" )]
		[FunctionArgument( "user", "is the name of the user whose account is to be reset." )]
		[FunctionArgument( "tenant", "is the name of the tenant that the user belongs to." )]
		[FunctionArgument( "server", "refers to the server on which the account will be reset.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the account will be reset.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void ResetAccountLogins( )
		{
			var user = GetArgument<string>( "user" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Reset login count for '{user}' in tenant '{tenant}'" ) )
			{
				ConsoleLogger.WriteLine( TenantManager.ResetAccount( user, tenant ) ? @"Account successfully reset." : @"Account reset failed." );
			}
		}

		/// <summary>
		///     Enables locked/disabled accounts.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -enableAccount -user usr -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "enableAccount", "Enables the user account.", "enableact" )]
		[FunctionArgument( "user", "is the name of the user whose account is to be enabled." )]
		[FunctionArgument( "tenant", "is the name of the tenant that the user belongs to." )]
		[FunctionArgument( "server", "refers to the server on which the account will be enabled.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the account will be enabled.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void EnableAccount( )
		{
			var user = GetArgument<string>( "user" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Enable account '{user}' in tenant '{tenant}'" ) )
			{
				ConsoleLogger.WriteLine( TenantManager.EnableAccount( user, tenant ) ? @"Account successfully enabled." : @"Failed to enable account." );
			}
		}

		/// <summary>
		///     Disable account.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -disableAccount -user usr -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "disableAccount", "Disables the user account.", "disableact" )]
		[FunctionArgument( "user", "is the name of the user whose account is to be disabled." )]
		[FunctionArgument( "tenant", "is the name of the tenant that the user belongs to." )]
		[FunctionArgument( "server", "refers to the server on which the account will be disabled.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the account will be disabled.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void DisableAccount( )
		{
			var user = GetArgument<string>( "user" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Disable account '{user}' in tenant '{tenant}'" ) )
			{
				ConsoleLogger.WriteLine( TenantManager.DisableAccount( user, tenant ) ? @"Account successfully disabled." : @"Failed to disable account." );
			}
		}

		/// <summary>
		///     Lock account.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -lockAccount -user usr -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "lockAccount", "Locks the user account.", "disableact" )]
		[FunctionArgument( "user", "is the name of the user whose account is to be locked." )]
		[FunctionArgument( "tenant", "is the name of the tenant that the user belongs to." )]
		[FunctionArgument( "server", "refers to the server on which the account will be locked.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the account will be locked.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void LockAccount( )
		{
			var user = GetArgument<string>( "user" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Lock account '{user}' in tenant '{tenant}'" ) )
			{
				ConsoleLogger.WriteLine( TenantManager.LockAccount( user, tenant ) ? @"Account successfully locked." : @"Failed to lock account." );
			}
		}

		/// <summary>
		///     Stages the application.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -stageApp -tenant tnt -app app [-server svr] [-database db]
		/// </remarks>
		[Function( "stageApp", "Stage the application.", "sa" )]
		[FunctionArgument( "tenant", "is the name of the tenant for which the application is to be staged." )]
		[FunctionArgument( "app", "is the name|guid of the application to be staged." )]
		[FunctionArgument( "server", "refers to the server on which the application will be staged.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the application will be staged.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void StageApp( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var app = GetArgument<string>( "app" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Stage app '{app}' in tenant '{tenant}'" ) )
			{
				AppManager.StageApp( tenant, app );
			}
		}

		/// <summary>
		///     Check whether the tenant exists.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -tenantExists -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "tenantExists", "Determines whether the specified tenant exists.", "te")]
		[FunctionArgument( "tenant", "is the name of the tenant to check." )]
		[FunctionArgument( "server", "refers to the server on which to check for the tenant.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which to check for the tenant.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void TenantExists( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Tenant exists '{tenant}'" ) )
			{
				ConsoleLogger.WriteLine( TenantManager.TenantExists( tenant ).ToString( ) );
			}
		}

		/// <summary>
		///     Transforms the specified configuration file into the corresponding application databases.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -transformConfig -config cfg -output out -hashFile hsh [-ver ver]
		/// </remarks>
		[Function( "transformConfig", "Transforms the specified configuration file into separate applications (.db files), each having the specified version, storing the output in the specified location. The transform only occurs when the solution hash does not match the hash stored in the specified hash file or the specified hash file does not exist.", "tc" )]
		[FunctionArgument( "config", "is the path to the solution xml file to transform." )]
		[FunctionArgument( "output", "is the output location that the transformed applications will be written to." )]
		[FunctionArgument( "srcHash", "is the path to the source hash file that records the current state of the xml applications to avoid transforming identical solutions." )]
		[FunctionArgument( "dstHash", "is the path to the destination hash file that records the current state of the db applications to avoid transforming identical solutions." )]
		[FunctionArgument( "ver", "is the optional version to inject into each application during the transform process.", null, FunctionArgumentOptions.Optional )]
		private void TransformConfig( )
		{
			var configPath = GetArgument<string>( "config" );
			var outputDir = GetArgument<string>( "output" );
			var sourceHashFile = GetArgument<string>( "srcHash" );
			var destinationHashFile = GetArgument<string>( "dstHash" );
			var version = GetArgument<string>( "ver" );

            if ( version.EndsWith( ".*.*" ) )
            {
                // Used by UpgradeXml.bat to perform adhoc upgrades in dev environment
                version = version.Replace( ".*.*", $".{( DateTime.Today - new DateTime( 2000, 1, 1 ) ).TotalDays}.{( int ) DateTime.Now.TimeOfDay.TotalSeconds}" );
            }

			using ( DatabaseContextInfo.SetContextInfo( $"Transform config '{configPath}'" ) )
			{
				AppManager.TransformConfig( configPath, outputDir, sourceHashFile, destinationHashFile, version );
			}
		}

		/// <summary>
		///     Upgrades an application within a tenant based on the differential of two applications in the app library
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -upgradeApp -tenant tnt -app app [-server svr] [-database db]
		/// </remarks>
		[Function("upgradeApp", "Upgrades the specified tenant to the latest version of the specified application.", "ua" )]
		[FunctionArgument( "tenant", "is the name of the tenant for which the application is to be upgraded." )]
		[FunctionArgument( "app", "is the name|guid of the application to be upgraded." )]
		[FunctionArgument( "server", "refers to the server on which the application will be upgraded.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which the application will be upgraded.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void UpgradeApp( )
		{
			var tenant = GetArgument<string>( "tenant" );
			var app = GetArgument<string>( "app" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Upgrade app '{app}' for tenant '{tenant}'" ) )
			{
				AppManager.UpgradeApp( tenant, app );
			}

		    InvalidateTenant(tenant);
		}

		/// <summary>
		///     User exists.
		/// </summary>
		/// <remarks>
		///     Command line parameters
		///     -userExists -user usr -tenant tnt [-server svr] [-database db]
		/// </remarks>
		[Function( "userExists", "Determines whether the specified user exists.", "ue" )]
		[FunctionArgument( "user", "is the name of the user to check." )]
		[FunctionArgument( "tenant", "is the name of the tenant to check." )]
		[FunctionArgument( "server", "refers to the server on which to check for the user.", "localhost", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "database", "refers to the name of the database on which to check for the user.", "SoftwarePlatform", FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbUser", "the username used to connect to the sql server.", null, FunctionArgumentOptions.Optional )]
		[FunctionArgument( "dbPassword", "the password used to connect to the database.", null, FunctionArgumentOptions.Optional )]
		private void UserExists( )
		{
			var user = GetArgument<string>( "user" );
			var tenant = GetArgument<string>( "tenant" );
			var server = GetArgument<string>( "server" );
			var database = GetArgument<string>( "database" );
			var dbUser = GetArgument<string>( "dbUser" );
			var dbPassword = GetArgument<string>( "dbPassword" );

			using ( DatabaseInfo.Override( server, database, dbUser, dbPassword ) )
			using ( DatabaseContextInfo.SetContextInfo( $"User '{user}' exists in tenant '{tenant}'" ) )
			{
				ConsoleLogger.WriteLine( TenantManager.UserExists( user, tenant ).ToString( ) );
			}
		}

		/// <summary>
		/// Runs the REPL.
		/// </summary>
		private void RunRepl( )
		{
			while ( true )
			{
				try
				{
					string command = Console.ReadLine( );

					if ( string.IsNullOrEmpty( command ) )
					{
						break;
					}

					command = command.Trim( ).ToLowerInvariant( );

					ConsoleLogger.WriteLine( $"Repl Command: {command}" );

					if ( command == "exit" || command == "quit" || command == "repl" )
					{
						break;
					}

					var type = MethodBase.GetCurrentMethod( ).DeclaringType;

					if ( type == null )
					{
						return;
					}

					var methods = type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

					FunctionAttribute function = null;
					List<FunctionArgumentAttribute> arguments = null;

					foreach ( var method in methods )
					{
						var functionAttribute = method.GetCustomAttribute<FunctionAttribute>( );

						if ( functionAttribute == null )
						{
							continue;
						}

						if ( functionAttribute.Name.Equals( command, StringComparison.CurrentCultureIgnoreCase ) || functionAttribute.Aliases.Contains( command ) )
						{
							function = functionAttribute;
							arguments = method.GetCustomAttributes<FunctionArgumentAttribute>( ).ToList( );
							break;
						}
					}

					if ( function != null )
					{
						_parser.Clear( );

						_parser.Add( function.Name.ToLowerInvariant( ), string.Empty );

						_attributeArgParser = new AttributeArgParser( MethodBase.GetCurrentMethod( ).DeclaringType, function.Name );

						foreach ( FunctionArgumentAttribute arg in arguments )
						{
							string value = Console.ReadLine( );

							if ( !string.IsNullOrEmpty( value ) )
							{
								ConsoleLogger.WriteLine( $"Repl Arg ({arg.Name}): {value}" );
							}

							if ( !string.IsNullOrEmpty( value ) )
							{
								_parser.Add( arg.Name.ToLowerInvariant( ), value.Trim( ) );
							}
						}

						Execute( );

						ConsoleLogger.WriteLine( );
						ConsoleLogger.WriteLine( @"DONE" );
						ConsoleLogger.WriteLine( );
					}
				}
				catch ( Exception exc )
				{
					ConsoleLogger.WriteLine( $"ERROR: {exc}" );
					ConsoleLogger.WriteLine( @"DONE" );
					ConsoleLogger.WriteLine( );
				}
			}
		}

		/// <summary>
        /// Invalidates tenant related cache entries.
        /// </summary>
        /// <param name="tenant">The tenant name.</param>
	    private static void InvalidateTenant(string tenant)
	    {
            var t = TenantHelper.Find(tenant);
            if (t != null)
            {
                TenantHelper.Invalidate(t);
            }
	    }
	}


	/// <summary>
	///     Thrown when an argument is missing
	/// </summary>
	public class CommandLineArgException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CommandLineArgException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public CommandLineArgException( string message )
			: base( message )
		{
		}
	}
}