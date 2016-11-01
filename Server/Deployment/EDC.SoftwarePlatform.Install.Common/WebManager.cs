using System;
using System.IO;
using System.Linq;
using System.Threading;
using EDC.ReadiNow.Diagnostics;
using Microsoft.Web.Administration;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     The IIS manager class.
	/// </summary>
	public class WebManager : IDisposable
	{
		/// <summary>
		///     The server manager
		/// </summary>
		private ServerManager _serverManager;

		/// <summary>
		///     Initializes a new instance of the <see cref="WebManager" /> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="rootPath">The root path.</param>
		public WebManager( Action<string, Severity> logger, string rootPath )
		{
			WriteLineCallback = logger;
			RootPath = rootPath;
		}

		/// <summary>
		///     Gets the root path.
		/// </summary>
		/// <value>
		///     The root path.
		/// </value>
		public string RootPath
		{
			get;
		}

		/// <summary>
		///     Gets the default application.
		/// </summary>
		/// <value>
		///     The default application.
		/// </value>
		private Application DefaultApplication
		{
			get
			{
				return DefaultSite?.Applications.FirstOrDefault( application => application.Path == "/" );
			}
		}

		/// <summary>
		///     Gets the default site.
		/// </summary>
		/// <value>
		///     The default site.
		/// </value>
		private Site DefaultSite
		{
			get
			{
				return _serverManager?.Sites.FirstOrDefault( site => site.Id == 1 );
			}
		}

		/// <summary>
		///     Gets the write line.
		/// </summary>
		/// <value>
		///     The write line.
		/// </value>
		private Action<string, Severity> WriteLineCallback
		{
			get;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Disconnect( );
		}

		/// <summary>
		///     Configures the virtual directory.
		/// </summary>
		/// <param name="path">The path.</param>
		public void ConfigureVirtualDirectory( string path )
		{
			if ( string.IsNullOrEmpty( path ) )
			{
				WriteLine( "No virtual directory path specified. Skipping removal of application.", Severity.Error );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to configure virtual directory '{path}'. No connection to IIS.", Severity.Error );
				return;
			}

			Application defaultApplication = DefaultApplication;

			if ( defaultApplication == null )
			{
				WriteLine( $"Failed to configure virtual directory '{path}'. No IIS root application found. Please ensure IIS is configured and running on the local machine.", Severity.Error );
				return;
			}

			bool modified = false;

			/////
			// Search for the virtual directory by name.
			/////
			VirtualDirectory baseVirtualDirectory = defaultApplication.VirtualDirectories.FirstOrDefault( vDir => string.Equals( vDir.Path, path, StringComparison.InvariantCultureIgnoreCase ) );

			if ( baseVirtualDirectory == null )
			{
				modified = true;
				WriteLine( $"Virtual directory '{path}' not found." );

				WriteLine( $"Creating virtual directory '{path}' mapped to physical path '{RootPath}'..." );

				/////
				// Create the virtual directory.
				/////
				defaultApplication.VirtualDirectories.Add( $"{path}", RootPath );

				WriteLine( $"Virtual directory '{path}' created." );
			}
			else
			{
				WriteLine( $"Virtual directory '{path}' already exists at physical path '{baseVirtualDirectory.PhysicalPath}'. Skipping." );
			}

			if ( modified )
			{
				_serverManager.CommitChanges( );
			}
		}

		/// <summary>
		///     Connects this instance.
		/// </summary>
		public bool Connect( )
		{
			_serverManager = new ServerManager( );

			Site defaultSite = DefaultSite;

			if ( defaultSite == null )
			{
				WriteLine( "No default IIS site found. Please ensure IIS is configured and running on the local machine.", Severity.Error );
				return false;
			}

			WriteLine( $"Found '{defaultSite.Name}' site." );

			Application defaultApplication = DefaultApplication;

			if ( defaultApplication == null )
			{
				WriteLine( "No IIS root application found. Please ensure IIS is configured and running on the local machine.", Severity.Error );
				return false;
			}

			WriteLine( $"Found '{defaultApplication.Path}' application." );

			return true;
		}

		/// <summary>
		///     Creates the application.
		/// </summary>
		/// <param name="virtualDirectoryPath">The virtual directory path.</param>
		/// <param name="applicationName">Name of the application.</param>
		/// <param name="physicalPath">The physical path.</param>
		/// <param name="applicationPoolName">Name of the application pool.</param>
		/// <param name="enablePreload">if set to <c>true</c> [enable preload].</param>
		/// <param name="serviceAutoStartProvider">The service automatic start provider.</param>
		public void CreateApplication( string virtualDirectoryPath, string applicationName, string physicalPath, string applicationPoolName = "DefaultAppPool", bool enablePreload = false, string serviceAutoStartProvider = "" )
		{
			if ( string.IsNullOrEmpty( virtualDirectoryPath ) )
			{
				WriteLine( "No virtual directory path specified. Skipping creation of application.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( applicationName ) )
			{
				WriteLine( "No application name specified. Skipping creation of application.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( physicalPath ) )
			{
				WriteLine( "No physical path specified. Skipping creation of application.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to configure application '{applicationName}'. No connection to IIS.", Severity.Error );
				return;
			}

			Site defaultSite = DefaultSite;

			if ( defaultSite == null )
			{
				WriteLine( $"Failed to configure application '{applicationName}'. No IIS default site found. Please ensure IIS is configured and running on the local machine.", Severity.Error );
				return;
			}

			string virtualPath = $"{virtualDirectoryPath.TrimEnd( '/' )}/{applicationName.TrimStart( '/' )}";

			if ( !Directory.Exists( physicalPath ) )
			{
				WriteLine( $"Physical directory not found at '{physicalPath}'. Skipping creation of '{applicationName}' application.", Severity.Warning );
			}
			else
			{
				bool modified = false;

				/////
				// Search for the application by name.
				/////
				Application application = defaultSite.Applications.FirstOrDefault( app => string.Equals( app.Path, virtualPath, StringComparison.InvariantCultureIgnoreCase ) );

				if ( application == null )
				{
					modified = true;

					WriteLine( $"Application '{applicationName}' not found." );

					WriteLine( $"Creating application '{virtualPath}' mapped to physical path '{physicalPath}'." );

					/////
					// Create the application
					/////
					application = defaultSite.Applications.Add( virtualPath, physicalPath );

					WriteLine( $"Application '{virtualPath}' created." );
				}
				else
				{
					WriteLine( $"Application '{application.Path}' already exists. Skipping." );
				}

				modified |= EnablePreload( application, enablePreload, applicationName );
				modified |= EnableServiceAutoStart( application, serviceAutoStartProvider != "", applicationName );
				modified |= SetAutoStartProvider( application, serviceAutoStartProvider, applicationName );
				modified |= AssignApplicationToApplicationPool( application, applicationPoolName, virtualPath );

				if ( modified )
				{
					_serverManager.CommitChanges( );
				}
			}
		}

		/// <summary>
		///     Creates the application pool.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		public void CreateAppPool( string name, string domain, string username, string password )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				WriteLine( "No name specified. Skipping creation of application pool.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to configure application pool '{name}'. No connection to IIS.", Severity.Error );
				return;
			}

			bool modified = false;

			/////
			// Search for the application pool by name.
			/////
			ApplicationPool appPool = _serverManager.ApplicationPools.FirstOrDefault( ap => string.Equals( ap.Name, name, StringComparison.InvariantCultureIgnoreCase ) );

			if ( appPool == null )
			{
				modified = true;

				WriteLine( $"Application pool '{name}' not found." );

				WriteLine( $"Creating application pool '{name}'..." );

				/////
				// Create the application pool.
				/////
				appPool = _serverManager.ApplicationPools.Add( name );

				WriteLine( $"Application pool '{name}' created." );
			}
			else
			{
				WriteLine( $"Application pool '{name}' already exists. Skipping." );
			}

			modified |= SetApplicationPoolIdentity( appPool, domain, username, password );
			modified |= SetApplicationPoolPipelineMode( appPool );
			modified |= SetApplicationPoolRuntimeVersion( appPool );
			modified |= SetApplicationPoolStartMode( appPool );
			modified |= SetApplicationPoolIdleTimeout( appPool );

			if ( modified )
			{
				_serverManager.CommitChanges( );
			}
		}

		/// <summary>
		///     Configures the automatic start provider.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		public void CreateAutoStartProvider( string name, Type type )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				WriteLine( "No auto start provider name specified. Skipping.", Severity.Warning );
				return;
			}

			if ( type == null )
			{
				WriteLine( "No auto start provider type specified. Skipping.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to configure auto-start provider '{name}'. No connection to IIS.", Severity.Error );
				return;
			}

			bool modified = false;

			var hostConfiguration = _serverManager.GetApplicationHostConfiguration( );
			var serviceAutoStartProvidersSection = hostConfiguration.GetSection( "system.applicationHost/serviceAutoStartProviders" );
			var serviceAutoStartProvidersCollection = serviceAutoStartProvidersSection.GetCollection( );

			ConfigurationElement serviceAutoStartProvider = null;

			foreach ( var item in serviceAutoStartProvidersCollection )
			{
				ConfigurationAttribute attribute = item.Attributes [ "name" ];

				if ( attribute != null && string.Equals( attribute.Value.ToString( ), name, StringComparison.InvariantCultureIgnoreCase ) )
				{
					serviceAutoStartProvider = item;
					break;
				}
			}

			if ( serviceAutoStartProvider == null )
			{
				WriteLine( $"Creating service auto-start provider '{name}' with type '{type.AssemblyQualifiedName}'." );

				serviceAutoStartProvider = serviceAutoStartProvidersCollection.CreateElement( "add" );
				serviceAutoStartProvider [ "name" ] = name;
				serviceAutoStartProvider [ "type" ] = type.AssemblyQualifiedName;
				serviceAutoStartProvidersCollection.Add( serviceAutoStartProvider );
				modified = true;
			}
			else
			{
				WriteLine( $"Found existing service auto-start provider with name '{name}'." );
			}

			if ( modified )
			{
				_serverManager.CommitChanges( );
			}
		}

		/// <summary>
		///     Disconnects this instance.
		/// </summary>
		public void Disconnect( )
		{
			_serverManager.Dispose( );
			_serverManager = null;
		}

		/// <summary>
		///     Removes the application.
		/// </summary>
		/// <param name="virtualDirectoryPath">The virtual directory path.</param>
		/// <param name="name">Name of the application.</param>
		public void RemoveApplication( string virtualDirectoryPath, string name )
		{
			if ( string.IsNullOrEmpty( virtualDirectoryPath ) )
			{
				WriteLine( "No virtual directory path specified. Skipping removal of application.", Severity.Warning );
				return;
			}

			if ( string.IsNullOrEmpty( name ) )
			{
				WriteLine( "No application name specified. Skipping removal of application.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to remove application '{name}'. No connection to IIS.", Severity.Error );
				return;
			}

			Site defaultSite = DefaultSite;

			if ( defaultSite == null )
			{
				WriteLine( $"Failed to remove application '{name}'. No IIS default site found. Please ensure IIS is configured and running on the local machine.", Severity.Error );
				return;
			}

			bool modified = false;

			string path = $"{virtualDirectoryPath.TrimEnd( '/' )}/{name.TrimStart( '/' )}";

			/////
			// Search for the application by name.
			/////
			Application application = defaultSite.Applications.FirstOrDefault( app => string.Equals( app.Path, path, StringComparison.InvariantCultureIgnoreCase ) );

			if ( application == null )
			{
				WriteLine( $"Application with path '{path}' not found. Skipping." );
			}
			else
			{
				WriteLine( $"Removing application '{path}'..." );

				defaultSite.Applications.Remove( application );

				modified = true;
			}

			if ( modified )
			{
				_serverManager.CommitChanges( );

				WriteLine( $"Application '{path}' successfully removed." );
			}
		}

		/// <summary>
		///     Shutdowns the application pool.
		/// </summary>
		/// <param name="name">The name.</param>
		public void RemoveAppPool( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				WriteLine( "No application pool name specified. Skipping removal of application pool.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to shutdown application pool '{name}'. No connection to IIS.", Severity.Error );
				return;
			}

			Site defaultSite = DefaultSite;

			bool modified = false;

			int count = 0;

			foreach ( Application app in defaultSite.Applications )
			{
				if ( string.Equals( app.ApplicationPoolName, name, StringComparison.InvariantCultureIgnoreCase ) )
				{
					WriteLine( $"Found application '{app.Path}' in use by application pool '{name}'." );
					count++;
				}
			}

			if ( count > 0 )
			{
				WriteLine( $"Application pool '{name}' is currently in use and will not be removed.", Severity.Warning );
			}
			else
			{
				/////
				// Search for the application pool by name.
				/////
				ApplicationPool appPool = _serverManager.ApplicationPools.FirstOrDefault( ap => string.Equals( ap.Name, name, StringComparison.InvariantCultureIgnoreCase ) );

				if ( appPool == null )
				{
					WriteLine( $"Application pool '{name}' not found." );
				}
				else
				{
					WriteLine( $"Removing application pool '{name}'..." );

					_serverManager.ApplicationPools.Remove( appPool );

					modified = true;
				}
			}

			if ( modified )
			{
				_serverManager.CommitChanges( );

				WriteLine( $"Application pool '{name}' has been successfully removed." );
			}
		}

		/// <summary>
		///     Removes the automatic start provider.
		/// </summary>
		/// <param name="name">The name.</param>
		public void RemoveAutoStartProvider( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				WriteLine( "No auto start provider name specified. Skipping removal of auto start provider.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to remove auto-start provider '{name}'. No connection to IIS.", Severity.Error );
				return;
			}

			Site defaultSite = DefaultSite;

			if ( defaultSite == null )
			{
				WriteLine( $"Failed to remove auto start provider '{name}'. No IIS default site found. Please ensure IIS is configured and running on the local machine.", Severity.Error );
				return;
			}

			int count = 0;

			foreach ( Application app in defaultSite.Applications )
			{
				object autoStartProviderObject = app.GetAttributeValue( "serviceAutoStartProvider" );

				if ( autoStartProviderObject != null )
				{
					if ( string.Equals( autoStartProviderObject.ToString( ), name, StringComparison.InvariantCultureIgnoreCase ) )
					{
						WriteLine( $"Found auto start provider '{name}' in use by application '{app.Path}'." );
						count++;
					}
				}
			}

			bool modified = false;

			if ( count > 0 )
			{
				WriteLine( $"Auto start provider '{name}' currently in use and will not be removed.", Severity.Warning );
			}
			else
			{
				var hostConfiguration = _serverManager.GetApplicationHostConfiguration( );
				var serviceAutoStartProvidersSection = hostConfiguration.GetSection( "system.applicationHost/serviceAutoStartProviders" );
				var serviceAutoStartProvidersCollection = serviceAutoStartProvidersSection.GetCollection( );

				ConfigurationElement serviceAutoStartProvider = null;

				foreach ( var item in serviceAutoStartProvidersCollection )
				{
					ConfigurationAttribute attribute = item.Attributes [ "name" ];

					if ( attribute != null && string.Equals( attribute.Value.ToString( ), name, StringComparison.InvariantCultureIgnoreCase ) )
					{
						serviceAutoStartProvider = item;
						break;
					}
				}

				if ( serviceAutoStartProvider == null )
				{
					WriteLine( $"Service auto-start provider '{name}' not found. Skipping." );
				}
				else
				{
					WriteLine( $"Removing service auto start provider '{name}'..." );

					serviceAutoStartProvidersCollection.Remove( serviceAutoStartProvider );

					modified = true;
				}
			}

			if ( modified )
			{
				_serverManager.CommitChanges( );

				WriteLine( $"Service auto start provider '{name}' successfully removed." );
			}
		}

		/// <summary>
		///     Removes the virtual directory.
		/// </summary>
		/// <param name="path">The path.</param>
		public void RemoveVirtualDirectory( string path )
		{
			if ( string.IsNullOrEmpty( path ) )
			{
				WriteLine( "No virtual directory path specified. Skipping removal of application.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to remove virtual directory '{path}'. No connection to IIS.", Severity.Error );
				return;
			}

			Application defaultApplication = DefaultApplication;

			if ( defaultApplication == null )
			{
				WriteLine( $"Failed to remove virtual directory '{path}'. No IIS root application found. Please ensure IIS is configured and running on the local machine.", Severity.Error );
				return;
			}

			bool modified = false;

			/////
			// Search for the virtual directory by name.
			/////
			VirtualDirectory baseVirtualDirectory = defaultApplication.VirtualDirectories.FirstOrDefault( vDir => string.Equals( vDir.Path, $"{path}", StringComparison.InvariantCultureIgnoreCase ) );

			if ( baseVirtualDirectory == null )
			{
				WriteLine( $"Virtual directory '{path}' not found." );
			}
			else
			{
				WriteLine( $"Found virtual directory '{path}'. Removing..." );

				defaultApplication.VirtualDirectories.Remove( baseVirtualDirectory );

				modified = true;
			}

			if ( modified )
			{
				_serverManager.CommitChanges( );

				WriteLine( $"Virtual directory '{path}' successfully removed." );
			}
		}

		/// <summary>
		///     Starts the application pool.
		/// </summary>
		/// <param name="name">The name.</param>
		public void StartAppPool( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				WriteLine( "No name specified. Skipping application pool start.", Severity.Warning );
				return;
			}

			if ( _serverManager == null )
			{
				WriteLine( $"Failed to start application pool '{name}'. No connection to IIS.", Severity.Error );
				return;
			}

			/////
			// Search for the application pool by name.
			/////
			ApplicationPool appPool = _serverManager.ApplicationPools.FirstOrDefault( ap => string.Equals( ap.Name, name, StringComparison.InvariantCultureIgnoreCase ) );

			if ( appPool == null )
			{
				WriteLine( $"Application pool '{name}' not found.", Severity.Warning );
			}
			else
			{
				bool poll = false;
				bool start = false;

				switch ( appPool.State )
				{
					case ObjectState.Started:
						WriteLine( $"Application pool '{name}' already started." );
						break;
					case ObjectState.Starting:
						WriteLine( $"Application pool '{name}' is currently starting." );
						break;
					case ObjectState.Stopped:
						WriteLine( $"Application pool '{name}' is currently stopped. Starting..." );
						appPool.Start( );
						poll = true;
						break;
					case ObjectState.Stopping:
						WriteLine( $"Application pool '{name}' is currently stopping." );
						poll = true;
						start = true;
						break;
					case ObjectState.Unknown:
						WriteLine( $"Application pool '{name}' is in an unknown state. Attempting start..." );
						appPool.Start( );
						poll = true;
						break;
				}

				int pollCount = 0;

				while ( poll && pollCount < 5 )
				{
					Thread.Sleep( 1000 );

					if ( appPool.State == ObjectState.Started )
					{
						WriteLine( $"Application	pool '{name}' successfully started." );
						break;
					}

					if ( appPool.State == ObjectState.Stopped && start )
					{
						appPool.Start( );
					}

					pollCount++;
				}
			}
		}

		/// <summary>
		///     Assigns the application to application pool.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="applicationPoolName">Name of the application pool.</param>
		/// <param name="virtualPath">The virtual path.</param>
		/// <returns></returns>
		private bool AssignApplicationToApplicationPool( Application application, string applicationPoolName, string virtualPath )
		{
			bool modified = false;

			/////
			// Determine if the application needs to be assigned to a different application pool.
			/////
			if ( !string.Equals( application.ApplicationPoolName, applicationPoolName, StringComparison.InvariantCultureIgnoreCase ) )
			{
				modified = true;

				WriteLine( $"Assigning application '{virtualPath}' to application pool '{applicationPoolName}'." );

				application.ApplicationPoolName = applicationPoolName;
			}
			else
			{
				WriteLine( $"Application '{virtualPath}' is already assigned to application pool '{applicationPoolName}'. Skipping." );
			}

			return modified;
		}

		/// <summary>
		///     Enables the preload.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="enable">if set to <c>true</c> [enable].</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private bool EnablePreload( Application application, bool enable, string name )
		{
			bool modified = false;

			var preloadEnabledAttribute = application.Attributes [ "preloadEnabled" ];

			if ( preloadEnabledAttribute == null || ( preloadEnabledAttribute.Value != null && ( bool ) preloadEnabledAttribute.Value != enable ) )
			{
				application.SetAttributeValue( "preloadEnabled", enable );
				modified = true;
				WriteLine( $"Setting 'preloadEnabled' value to '{enable}' for application '{name}'..." );
			}
			else
			{
				WriteLine( $"The 'preloadEnabled' value for application '{name}' is already set to '{enable}'." );
			}

			return modified;
		}

		/// <summary>
		///     Enables the service automatic start.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="enable">if set to <c>true</c> [enable].</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private bool EnableServiceAutoStart( Application application, bool enable, string name )
		{
			bool modified = false;

			var serviceAutoStartEnabledAttribute = application.Attributes [ "serviceAutoStartEnabled" ];

			if ( serviceAutoStartEnabledAttribute == null || ( serviceAutoStartEnabledAttribute.Value != null && ( bool ) serviceAutoStartEnabledAttribute.Value != enable ) )
			{
				application.SetAttributeValue( "serviceAutoStartEnabled", enable );
				modified = true;
				WriteLine( $"Setting 'serviceAutoStartEnabled' to '{enable}' for application '{name}'..." );
			}
			else
			{
				WriteLine( $"The 'serviceAutoStartEnabled' value for application '{name}' is already set to '{enable}'." );
			}

			return modified;
		}

		/// <summary>
		///     Sets the application pool identity.
		/// </summary>
		/// <param name="appPool">The application pool.</param>
		/// <param name="domain">The domain.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		private bool SetApplicationPoolIdentity( ApplicationPool appPool, string domain, string username, string password )
		{
			bool modified = false;

			if ( !string.IsNullOrEmpty( username ) )
			{
				/////
				// Check the itentity type.
				/////
				if ( appPool.ProcessModel.IdentityType != ProcessModelIdentityType.SpecificUser )
				{
					WriteLine( $"Application pool '{appPool.Name}' is not set to run as a specific user. Changing..." );
					appPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
					modified = true;
				}
				else
				{
					WriteLine( $"Application pool '{appPool.Name}' identity type is correctly set to '{appPool.ProcessModel.IdentityType}'." );
				}

				string user = string.IsNullOrEmpty( domain ) ? username : $"{domain}\\{username}";

				/////
				// Check the identity.
				/////
				if ( !string.Equals( appPool.ProcessModel.UserName, user, StringComparison.InvariantCultureIgnoreCase ) )
				{
					WriteLine( $"Identity for application pool '{appPool.Name}' is not set '{user}'. Changing..." );
					appPool.ProcessModel.UserName = user;
					modified = true;
				}
				else
				{
					WriteLine( $"Application pool '{appPool.Name}' identity is correctly set to '{appPool.ProcessModel.UserName}'." );
				}

				/////
				// Check the password.
				/////
				if ( appPool.ProcessModel.Password != password )
				{
					WriteLine( $"Identity password for application pool '{appPool.Name}' is incorrect. Updating..." );
					appPool.ProcessModel.Password = password;
					modified = true;
				}
				else
				{
					WriteLine( $"Application pool '{appPool.Name}' password is correctly set." );
				}
			}
			else
			{
				WriteLine( $"No application pool username specified for pool '{appPool.Name}'. Skipping identity configuration." );
			}

			return modified;
		}

		/// <summary>
		///     Sets the application pool idle timeout.
		/// </summary>
		/// <param name="appPool">The application pool.</param>
		/// <returns></returns>
		private bool SetApplicationPoolIdleTimeout( ApplicationPool appPool )
		{
			bool modified = false;

			/////
			// Set the idle timeout.
			/////
			if ( appPool.ProcessModel.IdleTimeout != TimeSpan.FromMinutes( 480 ) )
			{
				WriteLine( $"Idle timeout for application pool '{appPool.Name}' is not set to '480 minutes'. Updating..." );
				appPool.ProcessModel.IdleTimeout = TimeSpan.FromMinutes( 480 );
				modified = true;
			}
			else
			{
				WriteLine( $"Application pool '{appPool.Name}' idle timeout is correctly set to '{appPool.ProcessModel.IdleTimeout.TotalMinutes} minutes'." );
			}

			return modified;
		}

		/// <summary>
		///     Sets the application pool pipeline mode.
		/// </summary>
		/// <param name="appPool">The application pool.</param>
		/// <returns></returns>
		private bool SetApplicationPoolPipelineMode( ApplicationPool appPool )
		{
			bool modified = false;

			/////
			// Set the pipeline mode.
			/////
			if ( appPool.ManagedPipelineMode != ManagedPipelineMode.Integrated )
			{
				WriteLine( $"Managed pipline mode for application pool '{appPool.Name}' is not set to Integrated. Updating..." );
				appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
				modified = true;
			}
			else
			{
				WriteLine( $"Application pool '{appPool.Name}' managed pipeline mode is correctly set to '{appPool.ManagedPipelineMode}'." );
			}

			return modified;
		}

		/// <summary>
		///     Sets the application pool runtime version.
		/// </summary>
		/// <param name="appPool">The application pool.</param>
		/// <returns></returns>
		private bool SetApplicationPoolRuntimeVersion( ApplicationPool appPool )
		{
			bool modified = false;

			/////
			// Set the runtime version.
			/////
			if ( appPool.ManagedRuntimeVersion != "v4.0" )
			{
				WriteLine( $"Managed runtime version for application pool '{appPool.Name}' is not set to 'v4.0'. Updating..." );
				appPool.ManagedRuntimeVersion = "v4.0";
				modified = true;
			}
			else
			{
				WriteLine( $"Application pool '{appPool.Name}' managed runtime version is correctly set to '{appPool.ManagedRuntimeVersion}'." );
			}

			return modified;
		}

		/// <summary>
		///     Sets the application pool start mode.
		/// </summary>
		/// <param name="appPool">The application pool.</param>
		/// <returns></returns>
		private bool SetApplicationPoolStartMode( ApplicationPool appPool )
		{
			bool modified = false;

			/////
			// Set the start mode.
			/////
			if ( appPool.StartMode != StartMode.AlwaysRunning )
			{
				WriteLine( $"Start mode for application pool '{appPool.Name}' is not set to 'AlwaysRunning'. Updating..." );
				appPool.StartMode = StartMode.AlwaysRunning;
				modified = true;
			}
			else
			{
				WriteLine( $"Application pool '{appPool.Name}' start-mode is correctly set to '{appPool.StartMode}'." );
			}

			return modified;
		}

		/// <summary>
		///     Sets the automatic start provider.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="providerName">Name of the provider.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private bool SetAutoStartProvider( Application application, string providerName, string name )
		{
			bool modified = false;

			var serviceAutoStartProviderAttribute = application.Attributes [ "serviceAutoStartProvider" ];

			if ( serviceAutoStartProviderAttribute == null || ( serviceAutoStartProviderAttribute.Value != null && !string.Equals( serviceAutoStartProviderAttribute.Value.ToString( ), providerName, StringComparison.InvariantCultureIgnoreCase ) ) )
			{
				application.SetAttributeValue( "serviceAutoStartProvider", providerName );
				modified = true;
				WriteLine( $"Setting 'serviceAutoStartProvider' to '{providerName}' for application '{name}'..." );
			}
			else
			{
				WriteLine( $"The 'serviceAutoStartProvider' value for application '{name}' is already set to '{providerName}'." );
			}

			return modified;
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="severity">The severity.</param>
		private void WriteLine( string line, Severity severity = Severity.Info )
		{
			WriteLineCallback?.Invoke( line, severity );
		}
	}
}
