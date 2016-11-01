// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using EDC.Security;
using TimeoutException = System.ServiceProcess.TimeoutException;

namespace EDC.Services
{
	/// <summary>
	///     Provides helper methods for interacting with Windows services.
	/// </summary>
	public static class ServiceHelper
	{
		/// <summary>
		///     Installs the service(s) in the specified assembly.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the service assembly.
		/// </param>
		public static void InstallService( string path )
		{
			InstallService( path, ServiceStartMode.Manual, ServiceAccount.LocalSystem, null, null );
		}

		/// <summary>
		///     Installs the service(s) in the specified assembly.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the service assembly.
		/// </param>
		/// <param name="startMode">
		///     An enumeration that describes how and when this service is started.
		/// </param>
		/// <param name="account">
		///     An enumeration that describes the type of account under which the service will run.
		/// </param>
		/// <param name="credentials">
		///     The user credentials of the account under which the service will run.
		/// </param>
		/// <param name="parameters">
		///     A dictionary of parameters passed to the service's installer.
		/// </param>
		public static void InstallService( string path, ServiceStartMode startMode, ServiceAccount account, NetworkCredential credentials, StringDictionary parameters )
		{
			if ( string.IsNullOrEmpty( path ) )
			{
				throw new ArgumentException( "The specified path parameter is invalid." );
			}

			string filename = Path.GetFileNameWithoutExtension( path );

			try
			{
				// Initialize the service installer
				Assembly serviceAssembly = Assembly.LoadFrom( path );
				var serviceInstaller = new AssemblyInstaller( serviceAssembly, null );

				var commandLine = new ArrayList
					{
						string.Format( "StartMode={0}", startMode.ToString( "g" ) )
					};

				// Set the service start mode

				// Set the service account
				switch ( account )
				{
					case ServiceAccount.LocalService:
					case ServiceAccount.NetworkService:
					case ServiceAccount.LocalSystem:
						{
							commandLine.Add( string.Format( "Account={0}", account.ToString( "g" ) ) );
							break;
						}

					case ServiceAccount.User:
						{
							commandLine.Add( string.Format( "Account={0}", CredentialHelper.GetFullyQualifiedName( credentials ) ) );
							commandLine.Add( string.Format( "Password={0}", credentials.Password ) );
							break;
						}
				}

				// Set any parameters
				if ( parameters != null )
				{
					foreach ( string key in parameters.Keys )
					{
						commandLine.Add( string.Format( "{0}={1}", key, parameters[ key ] ) );
					}
				}

				// Initialize the service installer
				serviceInstaller.CommandLine = ( string[] ) commandLine.ToArray( typeof ( string ) );

				// Initialize the base installer
				var transactedInstaller = new TransactedInstaller( );
				transactedInstaller.Installers.Add( serviceInstaller );
				transactedInstaller.Context = new InstallContext( string.Format( "{0}.log", filename ), ( string[] ) commandLine.ToArray( typeof ( string ) ) );

				// Install the service
				var savedState = new Hashtable( );
				transactedInstaller.Install( savedState );
			}
			catch ( Exception exception )
			{
				throw new Exception( "Unable to install the specified service.", exception );
			}
		}

		/// <summary>
		///     Checks whether the specified service is running.
		/// </summary>
		/// <param name="serviceName">
		///     A string containing the service's name.
		/// </param>
		/// <returns>
		///     A boolean flag indicating whether the specified service is running.
		/// </returns>
		public static bool IsServiceActive( string serviceName )
		{
			if ( string.IsNullOrEmpty( serviceName ) )
			{
				throw new ArgumentException( "The specified serviceName parameter is invalid." );
			}

			bool active = false;

			// Get the installed services on the specified computer
			ServiceController[] services = ServiceController.GetServices( );

			// Iterate through the installed services
			foreach ( ServiceController service in services )
			{
				if ( String.Compare(service.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase) == 0 )
				{
					if ( service.Status == ServiceControllerStatus.Running )
					{
						active = true;
					}

					break;
				}
			}

			return active;
		}

		/// <summary>
		///     Checks whether the specified service is installed.
		/// </summary>
		/// <param name="serviceName">
		///     A string containing the service's name.
		/// </param>
		/// <returns>
		///     A boolean flag indicating whether the specified service is installed.
		/// </returns>
		public static bool IsServiceInstalled( string serviceName )
		{
			if ( string.IsNullOrEmpty( serviceName ) )
			{
				throw new ArgumentException( "The specified serviceName parameter is invalid." );
			}

			// Get the installed services on the specified computer
			ServiceController[] services = ServiceController.GetServices( );

			// Iterate through the installed services
			return services.Any( service => String.Compare(service.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase) == 0 );
		}

		/// <summary>
		///     Starts the specified service if not active
		/// </summary>
		/// <param name="serviceName">
		///     A string containing the service's name.
		/// </param>
		public static void StartService( string serviceName )
		{
			StartService( serviceName, TimeSpan.FromSeconds( 30 ) );
		}

		/// <summary>
		///     Starts the specified service if not active
		/// </summary>
		/// <param name="serviceName">
		///     A string containing the service's name.
		/// </param>
		/// <param name="timeout">
		///     The amount of time to wait for the service to start.
		/// </param>
		public static void StartService( string serviceName, TimeSpan timeout )
		{
			if ( string.IsNullOrEmpty( serviceName ) )
			{
				throw new ArgumentException( "The specified serviceName parameter is invalid." );
			}

			// Check that the service is installed
			if ( !IsServiceInstalled( serviceName ) )
			{
				throw new Exception( "The specified service cannot be found." );
			}

			try
			{
				var controller = new ServiceController( serviceName );

				// Check the status of the service
				if ( ( controller.Status != ServiceControllerStatus.Running ) && ( controller.Status != ServiceControllerStatus.StartPending ) )
				{
					// Attempt to start the service
					controller.Start( );
					controller.WaitForStatus( ServiceControllerStatus.Running, timeout );
				}
			}
			catch ( TimeoutException exception )
			{
				throw new Exception( "Unable to start the specified service within the time interval.", exception );
			}
			catch ( Exception exception )
			{
				throw new Exception( "Unable to start the specified service.", exception );
			}
		}

		/// <summary>
		///     Stops the specified service if active.
		/// </summary>
		/// <param name="serviceName">
		///     A string containing the service's name.
		/// </param>
		public static void StopService( string serviceName )
		{
			StopService( serviceName, TimeSpan.FromSeconds( 30 ) );
		}

		/// <summary>
		///     Stops the specified service if active.
		/// </summary>
		/// <param name="serviceName">
		///     A string containing the service's name.
		/// </param>
		/// <param name="timeout">
		///     The amount of time to wait for the service to stop.
		/// </param>
		public static void StopService( string serviceName, TimeSpan timeout )
		{
			if ( string.IsNullOrEmpty( serviceName ) )
			{
				throw new ArgumentException( "The specified serviceName parameter is invalid." );
			}

			// Check that the service is installed
			if ( !IsServiceInstalled( serviceName ) )
			{
				throw new Exception( "The specified service cannot be found." );
			}

			try
			{
				var controller = new ServiceController( serviceName );

				// Check the status of the service
				if ( ( controller.Status != ServiceControllerStatus.Stopped ) && ( controller.Status != ServiceControllerStatus.StopPending ) )
				{
					// Attempt to stop the service
					controller.Stop( );
					controller.WaitForStatus( ServiceControllerStatus.Stopped, timeout );
				}
			}
			catch ( TimeoutException exception )
			{
				throw new Exception( "Unable to stop the specified service within the time interval.", exception );
			}
			catch ( Exception exception )
			{
				throw new Exception( "Unable to stop the specified service.", exception );
			}
		}

		/// <summary>
		///     Uninstalls the service(s) in the specified assembly.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the service assembly.
		/// </param>
		public static void UninstallService( string path )
		{
			UninstallService( path, null );
		}

		/// <summary>
		///     Uninstalls the service(s) in the specified assembly.
		/// </summary>
		/// <param name="path">
		///     A string containing the path to the service assembly.
		/// </param>
		/// <param name="parameters">
		///     A dictionary of parameters passed to the service's uninstaller.
		/// </param>
		public static void UninstallService( string path, StringDictionary parameters )
		{
			if ( string.IsNullOrEmpty( path ) )
			{
				throw new ArgumentException( "The specified path parameter is invalid." );
			}

			try
			{
				// Initialize the service installer
				Assembly serviceAssembly = Assembly.LoadFrom( path );
				var serviceInstaller = new AssemblyInstaller( serviceAssembly, null );

				// Initialize the installation context
				var context = new InstallContext( );

				// Set any parameters
				if ( parameters != null )
				{
					foreach ( string key in parameters.Keys )
					{
						context.Parameters.Add( key, parameters[ key ] );
					}
				}

				// Initialize the base installer
				var transactedInstaller = new TransactedInstaller( );
				transactedInstaller.Installers.Add( serviceInstaller );
				transactedInstaller.Context = context;

				// Uninstall the service
				transactedInstaller.Uninstall( null );
			}
			catch ( Exception exception )
			{
				throw new Exception( "Unable to uninstall the specified service.", exception );
			}
		}
	}
}