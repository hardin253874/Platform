// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using Microsoft.Win32;

namespace ApplicationManager.Core
{
	/// <summary>
	///     Configuration settings.
	/// </summary>
	public static class Config
	{
		/// <summary>
		///     Gets the application cache.
		/// </summary>
		/// <value>
		///     The application cache.
		/// </value>
		public static string ApplicationCache => Path.Combine( InstallLocation, "Applications" );

		/// <summary>
		///     Gets or sets the name of the database.
		/// </summary>
		/// <value>
		///     The name of the database.
		/// </value>
		public static string DatabaseName
		{
			get
			{
				return GetRegistryString( @"SOFTWARE\ReadiNow\ApplicationManager", "databaseName", "SoftwarePlatform" );
			}
			set
			{
				SetRegistryString( @"SOFTWARE\ReadiNow\ApplicationManager", "databaseName", value, "SoftwarePlatform" );
			}
		}

		/// <summary>
		///     Gets the install location.
		/// </summary>
		/// <value>
		///     The install location.
		/// </value>
		public static string InstallLocation
		{
			get
			{
				DirectoryInfo info = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );

				bool found = false;

				/////
				// Probing paths...
				/////
				while ( !found )
				{
					string configFolder = Path.Combine( info.FullName, "Configuration" );

					if ( Directory.Exists( configFolder ) && File.Exists( Path.Combine( configFolder, "SoftwarePlatform.config" ) ) )
					{
						found = true;
					}
					else
					{
						string buildPath = Path.Combine( info.FullName, "Build" );

						if ( Directory.Exists( buildPath ) )
						{
							DirectoryInfo buildInfo = new DirectoryInfo( buildPath );

							configFolder = Path.Combine( buildInfo.FullName, "Configuration" );
							if ( Directory.Exists( configFolder ) && File.Exists( Path.Combine( configFolder, "SoftwarePlatform.config" ) ) )
							{
								info = buildInfo;
								found = true;
							}
							else
							{
								var buildConfigurationDirectories = Directory.GetDirectories( buildPath );

								foreach ( string buildConfigurationDirectory in buildConfigurationDirectories )
								{
									configFolder = Path.Combine( buildConfigurationDirectory, "Configuration" );
									if ( Directory.Exists( configFolder ) && File.Exists( Path.Combine( configFolder, "SoftwarePlatform.config" ) ) )
									{
										info = new DirectoryInfo( buildConfigurationDirectory );
										found = true;
									}
								}
							}
						}
					}

					if ( !found )
					{
						info = info.Parent;

						if ( info == null )
						{
							break;
						}
					}
				}

				if ( info == null )
				{
					throw new DirectoryNotFoundException( "Failed to determine the installation folder" );
				}

				return info.FullName;
			}
		}

		/// <summary>
		///     Gets or sets the name of the server.
		/// </summary>
		/// <value>
		///     The name of the server.
		/// </value>
		public static string ServerName
		{
			get
			{
				return GetRegistryString( @"SOFTWARE\ReadiNow\ApplicationManager", "serverName", Environment.MachineName );
			}
			set
			{
				string server = value;

				if ( string.IsNullOrEmpty( server ) )
				{
					server = Environment.MachineName;
				}

				SetRegistryString( @"SOFTWARE\ReadiNow\ApplicationManager", "serverName", value, server );
			}
		}

		/// <summary>
		///     Gets the registry string.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		private static string GetRegistryString( string key, string name, string defaultValue )
		{
			string data = null;

			using ( RegistryKey baseKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Default ) )
			{
				using ( RegistryKey subkey = baseKey.OpenSubKey( key ) )
				{
					if ( subkey != null )
					{
						data = subkey.GetValue( name ) as string;
					}
				}
			}

			if ( string.IsNullOrEmpty( data ) )
			{
				return defaultValue;
			}

			return data;
		}

		/// <summary>
		///     Sets the registry string.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="defaultValue">The default value.</param>
		private static void SetRegistryString( string key, string name, string value, string defaultValue )
		{
			using ( RegistryKey baseKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Default ) )
			{
				RegistryKey subkey = null;

				try
				{
					subkey = baseKey.OpenSubKey( key, true );

					if ( subkey == null )
					{
						subkey = baseKey.CreateSubKey( key );
					}

					if ( subkey != null )
					{
						if ( string.IsNullOrEmpty( value ) )
						{
							value = defaultValue;
						}

						subkey.SetValue( name, value );
					}

				}
				finally
				{
					if ( subkey != null )
					{
						subkey.Dispose( );
					}
				}
			}
		}
	}
}