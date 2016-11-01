// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ReadiMon.Properties;

namespace ReadiMon
{
	/// <summary>
	///     Class reprsenting the UpdateHelper type.
	/// </summary>
	public static class UpdateHelper
	{
		/// <summary>
		///     Doeses an update exist.
		/// </summary>
		/// <returns></returns>
		public static bool DoesUpdateExist( )
		{
			string updateLocation = Settings.Default.UpdatePath;

			if ( !Directory.Exists( updateLocation ) )
			{
				return false;
			}

			const string primaryFile = "ReadiMon.exe";

			var primaryFileNetworkPath = Path.Combine( updateLocation, primaryFile );

			if ( !File.Exists( primaryFileNetworkPath ) )
			{
				return false;
			}

			Version networkVersion = AssemblyName.GetAssemblyName( primaryFileNetworkPath ).Version;

			var localDirectory = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );

			if ( localDirectory != null )
			{
				var primaryFileLocalPath = Path.Combine( localDirectory, primaryFile );

				if ( !File.Exists( primaryFileLocalPath ) )
				{
					return false;
				}

				Version localVersion = AssemblyName.GetAssemblyName( primaryFileLocalPath ).Version;

				return networkVersion > localVersion;
			}

			return false;
		}

		/// <summary>
		/// Starts the upgrade.
		/// </summary>
		/// <param name="restartReadiMon">if set to <c>true</c> [restart readi mon].</param>
		public static void StartUpgrade( bool restartReadiMon = false )
		{
			ShutdownInstances( );

			string updateLocation = Settings.Default.UpdatePath;

			var localDirectory = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );

			if ( localDirectory != null )
			{
				byte[ ] bytes = Resources.ReadiMonUpdater;

				string updaterPath = Path.Combine( localDirectory, "ReadiMonUpdater.exe" );

				File.WriteAllBytes( updaterPath, bytes );

				string args = updateLocation;

				if ( restartReadiMon )
				{
					args += " restart";
				}

				Process.Start( updaterPath, args );
			}
		}

		/// <summary>
		/// Shuts down other instances.
		/// </summary>
		private static void ShutdownInstances( )
		{
			var currentProcess = Process.GetCurrentProcess( );

			var currentProcessId = currentProcess.Id;

			var processes = Process.GetProcessesByName( currentProcess.ProcessName );

			foreach ( Process process in processes )
			{
				try
				{
					if ( process.Id != currentProcessId )
					{
						process.CloseMainWindow( );
						{
							var exited = process.WaitForExit( 2500 );

							if ( !exited )
							{
								process.Kill( );
							}
						}
					}
				}
				catch ( Exception exc )
				{
					Trace.WriteLine( "Failed to terminate process. " + exc.Message );
				}
			}
		}
	}
}