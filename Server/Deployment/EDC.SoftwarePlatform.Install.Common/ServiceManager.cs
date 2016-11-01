// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     The service manager class.
	/// </summary>
	public static class ServiceManager
	{
		/// <summary>
		///     The service configuration description
		/// </summary>
		private const int ServiceConfigDescription = 0x01;

		/// <summary>
		///     Changes the service config2.
		/// </summary>
		/// <param name="hService">The h service.</param>
		/// <param name="dwInfoLevel">The dw information level.</param>
		/// <param name="lpInfo">The lp information.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool ChangeServiceConfig2( IntPtr hService, int dwInfoLevel, [MarshalAs( UnmanagedType.Struct )] ref ServiceDescription lpInfo );

		/// <summary>
		///     Gets the service status.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <returns></returns>
		public static ServiceState GetServiceStatus( string serviceName )
		{
			IntPtr serviceControlManager = OpenServiceControlManager( ServiceControlManagerAccessRights.Connect );

			try
			{
				IntPtr service = OpenService( serviceControlManager, serviceName, ServiceAccessRights.QueryStatus );

				if ( service == IntPtr.Zero )
				{
					return ServiceState.NotFound;
				}

				try
				{
					return GetServiceStatus( service );
				}
				finally
				{
					CloseServiceHandle( service );
				}
			}
			finally
			{
				CloseServiceHandle( serviceControlManager );
			}
		}

		/// <summary>
		///     Installs the and start.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <param name="displayName">The display name.</param>
		/// <param name="description">The description.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <param name="serviceType">Type of the service.</param>
		/// <param name="wait">The wait.</param>
		/// <exception cref="Win32Exception">Failed to install service.</exception>
		public static void InstallAndStart( string serviceName, string displayName, string description, string fileName, string username = null, string password = null, ServiceType serviceType = ServiceType.ServiceWin32OwnProcess, int wait = 0 )
		{
			IntPtr serviceControlManager = OpenServiceControlManager( ServiceControlManagerAccessRights.AllAccess );

			try
			{
				IntPtr service = OpenService( serviceControlManager, serviceName, ServiceAccessRights.AllAccess );

				if ( service == IntPtr.Zero )
				{
					service = CreateService( serviceControlManager, serviceName, displayName, ServiceAccessRights.AllAccess, serviceType, ServiceBootFlag.AutoStart, ServiceError.Normal, fileName, null, IntPtr.Zero, null, username, password );

					Thread.Sleep( wait );
				}

				if ( service == IntPtr.Zero )
				{
					throw new Win32Exception( "Failed to install service." );
				}

				var info = new ServiceDescription
				{
					description = description
				};

				ChangeServiceConfig2( service, ServiceConfigDescription, ref info );

				Thread.Sleep( wait );

				try
				{
					int count = 0;

					while ( count < 5 )
					{
						try
						{
							StartService( service );
							break;
						}
						catch
						{
							Thread.Sleep( 1000 );
							count++;
						}
					}

					Thread.Sleep( wait );
				}
				finally
				{
					CloseServiceHandle( service );
				}
			}
			finally
			{
				CloseServiceHandle( serviceControlManager );
			}
		}

		/// <summary>
		///     Services the is installed.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <returns></returns>
		public static bool ServiceIsInstalled( string serviceName )
		{
			IntPtr serviceControlManager = OpenServiceControlManager( ServiceControlManagerAccessRights.Connect );

			try
			{
				IntPtr service = OpenService( serviceControlManager, serviceName, ServiceAccessRights.QueryStatus );

				if ( service == IntPtr.Zero )
				{
					return false;
				}

				CloseServiceHandle( service );

				return true;
			}
			finally
			{
				CloseServiceHandle( serviceControlManager );
			}
		}

		/// <summary>
		///     Starts the service.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <exception cref="Win32Exception">Could not open service.</exception>
		public static void StartService( string serviceName )
		{
			IntPtr serviceControlManager = OpenServiceControlManager( ServiceControlManagerAccessRights.Connect );

			try
			{
				IntPtr service = OpenService( serviceControlManager, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Start );

				if ( service == IntPtr.Zero )
				{
					throw new Win32Exception( "Could not open service." );
				}

				try
				{
					StartService( service );
				}
				finally
				{
					CloseServiceHandle( service );
				}
			}
			finally
			{
				CloseServiceHandle( serviceControlManager );
			}
		}

		/// <summary>
		///     Stops the service.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <exception cref="Win32Exception">Could not open service.</exception>
		public static void StopService( string serviceName )
		{
			IntPtr serviceControlManager = OpenServiceControlManager( ServiceControlManagerAccessRights.Connect );

			try
			{
				IntPtr service = OpenService( serviceControlManager, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop );

				if ( service == IntPtr.Zero )
				{
					throw new Win32Exception( "Could not open service." );
				}

				try
				{
					StopService( service );
				}
				finally
				{
					CloseServiceHandle( service );
				}
			}
			finally
			{
				CloseServiceHandle( serviceControlManager );
			}
		}

		/// <summary>
		///     Uninstalls the specified service name.
		/// </summary>
		/// <param name="serviceName">Name of the service.</param>
		/// <exception cref="Win32Exception">
		///     Service not installed.
		///     or
		///     Could not delete service  + Marshal.GetLastWin32Error( )
		/// </exception>
		public static void Uninstall( string serviceName )
		{
			IntPtr serviceControlManager = OpenServiceControlManager( ServiceControlManagerAccessRights.AllAccess );

			try
			{
				IntPtr service = OpenService( serviceControlManager, serviceName, ServiceAccessRights.AllAccess );

				if ( service == IntPtr.Zero )
				{
					throw new Win32Exception( "Service not installed." );
				}

				try
				{
					StopService( service );

					if ( !DeleteService( service ) )
					{
						throw new Win32Exception( "Could not delete service " + Marshal.GetLastWin32Error( ) );
					}
				}
				finally
				{
					CloseServiceHandle( service );
				}
			}
			finally
			{
				CloseServiceHandle( serviceControlManager );
			}
		}

		/// <summary>
		///     Closes the service handle.
		/// </summary>
		/// <param name="serviceControlManagerObject">The service control manager object.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool CloseServiceHandle( IntPtr serviceControlManagerObject );

		/// <summary>
		///     Controls the service.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="controlCode">The control code.</param>
		/// <param name="serviceStatus">The service status.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll" )]
		private static extern int ControlService( IntPtr service, ServiceControl controlCode, ServiceStatus serviceStatus );

		/// <summary>
		///     Creates the service.
		/// </summary>
		/// <param name="serviceControlManager">The service control manager.</param>
		/// <param name="serviceName">Name of the service.</param>
		/// <param name="displayName">The display name.</param>
		/// <param name="desiredAccess">The desired access.</param>
		/// <param name="serviceType">Type of the service.</param>
		/// <param name="startType">The start type.</param>
		/// <param name="errorControl">The error control.</param>
		/// <param name="binaryPathName">Name of the binary path.</param>
		/// <param name="loadOrderGroup">The load order group.</param>
		/// <param name="tagId">The tag identifier.</param>
		/// <param name="dependencies">The dependencies.</param>
		/// <param name="serviceStartName">Start name of the service.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
		private static extern IntPtr CreateService( IntPtr serviceControlManager, string serviceName, string displayName, ServiceAccessRights desiredAccess, ServiceType serviceType, ServiceBootFlag startType, ServiceError errorControl, string binaryPathName, string loadOrderGroup, IntPtr tagId, string dependencies, string serviceStartName, string password );

		/// <summary>
		///     Deletes the service.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool DeleteService( IntPtr service );

		/// <summary>
		///     Gets the service status.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <returns></returns>
		/// <exception cref="Win32Exception">Failed to query service status.</exception>
		private static ServiceState GetServiceStatus( IntPtr service )
		{
			ServiceStatus status = new ServiceStatus( );

			if ( QueryServiceStatus( service, status ) == 0 )
			{
				throw new Win32Exception( "Failed to query service status." );
			}

			return status.CurrentState;
		}

		/// <summary>
		///     Opens the service control manager.
		/// </summary>
		/// <param name="machineName">Name of the machine.</param>
		/// <param name="databaseName">Name of the database.</param>
		/// <param name="desiredAccess">The desired access.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true )]
		private static extern IntPtr OpenSCManager( string machineName, string databaseName, ServiceControlManagerAccessRights desiredAccess );

		/// <summary>
		///     Opens the service.
		/// </summary>
		/// <param name="serviceControlManager">The service control manager.</param>
		/// <param name="serviceName">Name of the service.</param>
		/// <param name="desiredAccess">The desired access.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
		private static extern IntPtr OpenService( IntPtr serviceControlManager, string serviceName, ServiceAccessRights desiredAccess );

		/// <summary>
		///     Opens the service control manager.
		/// </summary>
		/// <param name="rights">The rights.</param>
		/// <returns></returns>
		/// <exception cref="Win32Exception">Could not connect to service control manager.</exception>
		private static IntPtr OpenServiceControlManager( ServiceControlManagerAccessRights rights )
		{
			IntPtr serviceControlManager = OpenSCManager( null, null, rights );

			if ( serviceControlManager == IntPtr.Zero )
			{
				throw new Win32Exception( "Could not connect to service control manager." );
			}

			return serviceControlManager;
		}

		/// <summary>
		///     Queries the service status.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="serviceStatus">The service status.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll" )]
		private static extern int QueryServiceStatus( IntPtr service, ServiceStatus serviceStatus );

		/// <summary>
		///     Starts the service.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="serviceArgCount">The service argument count.</param>
		/// <param name="serviceArgVectors">The service argument vectors.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", SetLastError = true )]
		private static extern int StartService( IntPtr service, int serviceArgCount, int serviceArgVectors );

		/// <summary>
		///     Starts the service.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <exception cref="Win32Exception">Unable to start service</exception>
		private static void StartService( IntPtr service )
		{
			//Thread.Sleep( 1000 );

			StartService( service, 0, 0 );

			//Thread.Sleep( 1000 );

			var changedStatus = WaitForServiceStatus( service, ServiceState.StartPending, ServiceState.Running );

			if ( !changedStatus )
			{
				throw new Win32Exception( "Unable to start service" );
			}
		}

		/// <summary>
		///     Stops the service.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <exception cref="Win32Exception">Unable to stop service</exception>
		private static void StopService( IntPtr service )
		{
			ServiceStatus status = new ServiceStatus( );

			ControlService( service, ServiceControl.Stop, status );

			var changedStatus = WaitForServiceStatus( service, ServiceState.StopPending, ServiceState.Stopped );

			if ( !changedStatus )
			{
				throw new Win32Exception( "Unable to stop service" );
			}
		}

		/// <summary>
		///     Waits for service status.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="waitStatus">The wait status.</param>
		/// <param name="desiredStatus">The desired status.</param>
		/// <returns></returns>
		private static bool WaitForServiceStatus( IntPtr service, ServiceState waitStatus, ServiceState desiredStatus )
		{
			ServiceStatus status = new ServiceStatus( );

			QueryServiceStatus( service, status );

			if ( status.CurrentState == desiredStatus )
			{
				return true;
			}

			int dwStartTickCount = Environment.TickCount;
			int dwOldCheckPoint = status.CheckPoint;

			while ( status.CurrentState == waitStatus )
			{
				// Do not wait longer than the wait hint. A good interval is
				// one tenth the wait hint, but no less than 1 second and no
				// more than 10 seconds.

				int dwWaitTime = status.WaitHint / 10;

				if ( dwWaitTime < 1000 )
				{
					dwWaitTime = 1000;
				}
				else if ( dwWaitTime > 10000 )
				{
					dwWaitTime = 10000;
				}

				Thread.Sleep( dwWaitTime );

				/////
				// Check the status again.
				/////
				if ( QueryServiceStatus( service, status ) == 0 )
				{
					break;
				}

				if ( status.CheckPoint > dwOldCheckPoint )
				{
					/////
					// The service is making progress.
					/////
					dwStartTickCount = Environment.TickCount;
					dwOldCheckPoint = status.CheckPoint;
				}
				else
				{
					if ( Environment.TickCount - dwStartTickCount > dwWaitTime )
					{
						/////
						// No progress made within the wait hint
						/////
						break;
					}
				}
			}

			return status.CurrentState == desiredStatus;
		}

		/// <summary>
		///     Service description
		/// </summary>
		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
		public struct ServiceDescription
		{
			/// <summary>
			///     The description
			/// </summary>
			public string description;
		}

#pragma warning disable 169

		/// <summary>
		///     The service status class.
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		private class ServiceStatus
		{
			/// <summary>
			///     The service type
			/// </summary>
			public int ServiceType = 0;

			/// <summary>
			///     The current state
			/// </summary>
			public readonly ServiceState CurrentState = 0;

			/// <summary>
			///     The controls accepted
			/// </summary>
			public int ControlsAccepted = 0;

			/// <summary>
			///     The win32 exit code
			/// </summary>
			public int Win32ExitCode = 0;

			/// <summary>
			///     The service specific exit code
			/// </summary>
			public int ServiceSpecificExitCode = 0;

			/// <summary>
			///     The check point
			/// </summary>
			public readonly int CheckPoint = 0;

			/// <summary>
			///     The wait hint
			/// </summary>
			public readonly int WaitHint = 0;
		}

#pragma warning restore 169
	}


	/// <summary>
	///     Service state.
	/// </summary>
	public enum ServiceState
	{
		/// <summary>
		///     The service is not known on the host server.
		/// </summary>
		NotFound = 0,

		/// <summary>
		///     The service is currently stopped.
		/// </summary>
		Stopped = 1,

		/// <summary>
		///     The service is about to start.
		/// </summary>
		StartPending = 2,

		/// <summary>
		///     The service is about to stop.
		/// </summary>
		StopPending = 3,

		/// <summary>
		///     The service is currently running.
		/// </summary>
		Running = 4,

		/// <summary>
		///     The service is about to continue from a paused state.
		/// </summary>
		ContinuePending = 5,

		/// <summary>
		///     The service is about to become paused.
		/// </summary>
		PausePending = 6,

		/// <summary>
		///     The service is currently paused.
		/// </summary>
		Paused = 7
	}

	[Flags]
	public enum ServiceControlManagerAccessRights
	{
		/// <summary>
		///     Connect access rights.
		/// </summary>
		Connect = 0x0001,

		/// <summary>
		///     Create service access rights.
		/// </summary>
		CreateService = 0x0002,

		/// <summary>
		///     Enumerate service access rights.
		/// </summary>
		EnumerateService = 0x0004,

		/// <summary>
		///     Lock access rights.
		/// </summary>
		Lock = 0x0008,

		/// <summary>
		///     Query lock status access rights.
		/// </summary>
		QueryLockStatus = 0x0010,

		/// <summary>
		///     Modify boot configuration access rights.
		/// </summary>
		ModifyBootConfig = 0x0020,

		/// <summary>
		///     Standard rights required access rights.
		/// </summary>
		StandardRightsRequired = 0xF0000,

		/// <summary>
		///     All access.
		/// </summary>
		AllAccess = StandardRightsRequired | Connect | CreateService |
		            EnumerateService | Lock | QueryLockStatus | ModifyBootConfig
	}

	/// <summary>
	///     Service access rights.
	/// </summary>
	[Flags]
	public enum ServiceAccessRights
	{
		/// <summary>
		///     Query configuration access rights.
		/// </summary>
		QueryConfig = 0x1,

		/// <summary>
		///     Change configuration access rights.
		/// </summary>
		ChangeConfig = 0x2,

		/// <summary>
		///     Query status access rights.
		/// </summary>
		QueryStatus = 0x4,

		/// <summary>
		///     Enumerate dependants access rights.
		/// </summary>
		EnumerateDependants = 0x8,

		/// <summary>
		///     Start access rights.
		/// </summary>
		Start = 0x10,

		/// <summary>
		///     Stop access rights.
		/// </summary>
		Stop = 0x20,

		/// <summary>
		///     Pause/continue access rights.
		/// </summary>
		PauseContinue = 0x40,

		/// <summary>
		///     Interrogate access rights.
		/// </summary>
		Interrogate = 0x80,

		/// <summary>
		///     User defined control access rights.
		/// </summary>
		UserDefinedControl = 0x100,

		/// <summary>
		///     Delete access rights.
		/// </summary>
		Delete = 0x00010000,

		/// <summary>
		///     Standard rights required access rights.
		/// </summary>
		StandardRightsRequired = 0xF0000,

		/// <summary>
		///     All access.
		/// </summary>
		AllAccess = StandardRightsRequired | QueryConfig | ChangeConfig |
		            QueryStatus | EnumerateDependants | Start | Stop | PauseContinue |
		            Interrogate | UserDefinedControl
	}

	/// <summary>
	///     Service boot flag.
	/// </summary>
	public enum ServiceBootFlag
	{
		/// <summary>
		///     Start
		/// </summary>
		Start = 0x00000000,

		/// <summary>
		///     System start
		/// </summary>
		SystemStart = 0x00000001,

		/// <summary>
		///     Automatic start
		/// </summary>
		AutoStart = 0x00000002,

		/// <summary>
		///     On demand start
		/// </summary>
		DemandStart = 0x00000003,

		/// <summary>
		///     Disabled
		/// </summary>
		Disabled = 0x00000004
	}

	/// <summary>
	///     Service control request.
	/// </summary>
	public enum ServiceControl
	{
		/// <summary>
		///     Stop service.
		/// </summary>
		Stop = 0x00000001,

		/// <summary>
		///     Pause service.
		/// </summary>
		Pause = 0x00000002,

		/// <summary>
		///     Continue service.
		/// </summary>
		Continue = 0x00000003,

		/// <summary>
		///     Interrogate service.
		/// </summary>
		Interrogate = 0x00000004,

		/// <summary>
		///     Shutdown service.
		/// </summary>
		Shutdown = 0x00000005,

		/// <summary>
		///     Request a parameter change.
		/// </summary>
		ParamChange = 0x00000006,

		/// <summary>
		///     Add a net binding.
		/// </summary>
		NetBindAdd = 0x00000007,

		/// <summary>
		///     Remvoe a net binding.
		/// </summary>
		NetBindRemove = 0x00000008,

		/// <summary>
		///     Enable net binding.
		/// </summary>
		NetBindEnable = 0x00000009,

		/// <summary>
		///     Disable net binding.
		/// </summary>
		NetBindDisable = 0x0000000A
	}

	/// <summary>
	///     Service error.
	/// </summary>
	public enum ServiceError
	{
		/// <summary>
		///     Ignore error.
		/// </summary>
		Ignore = 0x00000000,

		/// <summary>
		///     Normal error.
		/// </summary>
		Normal = 0x00000001,

		/// <summary>
		///     Severe error.
		/// </summary>
		Severe = 0x00000002,

		/// <summary>
		///     Critical error.
		/// </summary>
		Critical = 0x00000003
	}

	/// <summary>
	///     Service type
	/// </summary>
	public enum ServiceType
	{
		/// <summary>
		///     The service kernel driver.
		/// </summary>
		ServiceKernelDriver = 0x00000001,

		/// <summary>
		///     The service file system driver.
		/// </summary>
		ServiceFileSystemDriver = 0x00000002,

		/// <summary>
		///     The service adapter.
		/// </summary>
		ServiceAdapter = 0x00000004,

		/// <summary>
		///     The service recognizer driver.
		/// </summary>
		ServiceRecognizerDriver = 0x00000008,

		/// <summary>
		///     The service win32 own process.
		/// </summary>
		ServiceWin32OwnProcess = 0x00000010,

		/// <summary>
		///     The service win32 share process.
		/// </summary>
		ServiceWin32ShareProcess = 0x00000020
	}
}