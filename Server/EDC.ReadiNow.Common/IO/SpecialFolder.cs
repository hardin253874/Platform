// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Resources;
using Microsoft.Win32;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Provides the paths of special or reserved folders.
	/// </summary>
	public static class SpecialFolder
	{
		private static readonly Dictionary<SpecialMachineFolders, string> MachineFolderCache = new Dictionary<SpecialMachineFolders, string>( );
		private static readonly object MachineFolderCacheLock = new object( );

		private static readonly Dictionary<SpecialUserFolders, string> UserFolderCache = new Dictionary<SpecialUserFolders, string>( );
		private static readonly object UserFolderCacheLock = new object( );

		/// <summary>
		///     Gets the path of the special folder.
		/// </summary>
		/// <param name="folder">
		///     An enumeration identifying the special folder to examine.
		/// </param>
		/// <returns>
		///     A string containing the path of the special folder.
		/// </returns>
		public static string GetSpecialFolderPath( SpecialMachineFolders folder )
		{
			string folderPath;

			if ( !MachineFolderCache.ContainsKey( folder ) )
			{
				lock ( MachineFolderCacheLock )
				{
					string installPath = GetMachineInstallFolderPath( );

					switch ( folder )
					{
						case SpecialMachineFolders.Install:
							{
								folderPath = installPath;
								break;
							}

						case SpecialMachineFolders.InQueue:
							{
								folderPath = Path.Combine( installPath, "Queue\\In" );
								break;
							}

						case SpecialMachineFolders.OutQueue:
							{
								folderPath = Path.Combine( installPath, "Queue\\Out" );
								break;
							}

						case SpecialMachineFolders.BadQueue:
							{
								folderPath = Path.Combine( installPath, "Queue\\Bad" );
								break;
							}

						case SpecialMachineFolders.Log:
							folderPath = Path.Combine( installPath, folder.ToString( "g" ) );
							break;


                        case SpecialMachineFolders.MailDrop:
                            folderPath = Path.Combine(installPath, "OutgoingEmail");
                            break;

						default:
							{
								folderPath = Path.Combine( installPath, folder.ToString( "g" ) );
								break;
							}
					}

					MachineFolderCache[ folder ] = folderPath;
				}
			}
			else
			{
				folderPath = MachineFolderCache[ folder ];
			}

			return folderPath;
		}

		/// <summary>
		///     Gets the path of the special folder.
		/// </summary>
		/// <param name="folder">
		///     An enumeration identifying the special folder to examine.
		/// </param>
		/// <returns>
		///     A string containing the path of the special folder.
		/// </returns>
		public static string GetSpecialFolderPath( SpecialUserFolders folder )
		{
			string folderPath;

			if ( !UserFolderCache.ContainsKey( folder ) )
			{
				lock ( UserFolderCacheLock )
				{
					string installPath = GetUserInstallFolderPath( );
					folderPath = Path.Combine( installPath, folder.ToString( "g" ) );

					UserFolderCache[ folder ] = folderPath;
				}
			}
			else
			{
				folderPath = UserFolderCache[ folder ];
			}

			return folderPath;
		}

		/// <summary>
		///     Gets the path to the machine install folder.
		/// </summary>
		/// <returns>
		///     A string containing the current install folder.
		/// </returns>
		private static string GetMachineInstallFolderPath( )
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

		/// <summary>
		///     Gets the path to the user install folder.
		/// </summary>
		/// <returns>
		///     A string containing the current install folder.
		/// </returns>
		private static string GetUserInstallFolderPath( )
		{
			string applicationDirectory = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			string installPath = Path.Combine( applicationDirectory, string.Format( "{0}\\{1}", GlobalStrings.CompanyPath, GlobalStrings.ProductPath ) );

			return installPath;
		}
	}
}