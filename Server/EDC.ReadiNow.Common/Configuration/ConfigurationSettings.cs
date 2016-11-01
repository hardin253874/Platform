// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using EDC.ReadiNow.IO;
using AppConfiguration = System.Configuration.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Provides access to configuration files that influences the application.
	/// </summary>
	public class ConfigurationSettings
	{
		internal const string CacheSectionKey = "cacheSettings";
		internal const string DatabaseSectionKey = "databaseSettings";
		internal const string DiagnosticsSectionKey = "diagnosticsSettings";
		internal const string PlatformConfigurationFile = @"SoftwarePlatform.config";
		internal const string ServerSectionKey = "serverSettings";
		internal const string SiteSectionKey = "siteSettings";
		internal const string SiteSettingsKey = "siteSettings";
        internal const string WorkflowSectionKey = "workflowSettings";
        internal const string AuditLogSectionKey = "auditLogSettings";
		internal const string RedisSectionKey = "redisSettings";
        internal const string SyslogSectionKey = "syslogSettings";
        internal const string AppLibrarySectionKey = "applicationLibrary";
	    internal const string RabbitMqSectionKey = "rabbitMqSettings";
	    internal const string CastSectionKey = "castSettings";
		internal const string PrewarmSectionKey = "prewarm";
        internal const string FileRepositorySectionKey = "fileRepositorySettings";

		/// <summary>
		/// Synchronization object.
		/// </summary>
		private static readonly object SyncLock = new object();

		/// <summary>
		///     Cached copy of the application configuration.
		/// </summary>
		private static AppConfiguration _appConfiguration;

		/// <summary>
		/// File system watcher
		/// </summary>
		private static FileSystemWatcher _fileSystemWatcher;

		/// <summary>
		/// Occurs when the configuration settings are changed.
		/// </summary>
		public static event EventHandler Changed;

		/// <summary>
		///     Initialize the ConfigurationSettings type.
		/// </summary>
		static ConfigurationSettings( )
		{
			try
			{
				/////
				// Setup a watcher so we can get notification when the configuration file changes rather than having to poll it continually.
				/////
				_fileSystemWatcher = new FileSystemWatcher( )
				{
					Path = SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Configuration ),
					NotifyFilter = NotifyFilters.LastWrite,
					Filter = PlatformConfigurationFile
				};

				_fileSystemWatcher.Changed += FileSystemWatcher_Changed;
				_fileSystemWatcher.EnableRaisingEvents = true;
			}
			catch ( Exception ex )
			{
				/////
				// Will happen if the product is not installed.
				/////
				Trace.WriteLine( "Failed to setup file watcher. " + ex );
			}
		}

		/// <summary>
		/// Handles the Changed event of the FileSystemWatcher control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
		private static void FileSystemWatcher_Changed( object sender, FileSystemEventArgs e )
		{
			var changedHandler = Changed;

			changedHandler?.Invoke( _appConfiguration, new EventArgs( ) );
		}

		/// <summary>
		/// Gets the name of the configuration file.
		/// </summary>
		/// <value>
		/// The name of the configuration file.
		/// </value>
		private static string ConfigurationFileName
		{
			get
			{
				string path = Path.Combine( SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Configuration ), PlatformConfigurationFile );

				string targetPath;

				if ( SymbolicLink.TryGetTarget( path, out targetPath ) )
				{
					return targetPath;
				}

				return path;
			}
		}

		/// <summary>
		///     Gets the cache section from the application configuration file.
		/// </summary>
		/// <returns>
		///     An object representing the cache section.
		/// </returns>
		public static CacheConfiguration GetCacheConfigurationSection( )
		{
            return GetConfigurationSection<CacheConfiguration>(CacheSectionKey);
		}

		/// <summary>
		///     Gets the database section from the application configuration file.
		/// </summary>
		/// <returns>
		///     An object representing the database section.
		/// </returns>
		public static DatabaseConfiguration GetDatabaseConfigurationSection( )
		{
            return GetConfigurationSection<DatabaseConfiguration>(DatabaseSectionKey);
		}

		/// <summary>
		///     Gets the diagnostics section from the application configuration file.
		/// </summary>
		/// <returns>
		///     An object representing the diagnostics section.
		/// </returns>
		public static DiagnosticsConfiguration GetDiagnosticsConfigurationSection( )
		{
            return GetConfigurationSection<DiagnosticsConfiguration>(DiagnosticsSectionKey);
		}
		
		/// <summary>
		///		Gets the Redis configuration section.
		/// </summary>
		/// <returns>
		///     An object representing the redis section.
		/// </returns>
		public static RedisConfiguration GetRedisConfigurationSection( )
		{
			return GetConfigurationSection<RedisConfiguration>( RedisSectionKey );
		}

		/// <summary>
		///     Gets the server section from the application configuration file.
		/// </summary>
		/// <returns>
		///     An object representing the server section.
		/// </returns>
		public static ServerConfiguration GetServerConfigurationSection( )
		{
            return GetConfigurationSection<ServerConfiguration>(ServerSectionKey);
		}

		/// <summary>
		///     Gets the diagnostics section from the application configuration file.
		/// </summary>
		/// <returns>
		///     An object representing the diagnostics section.
		/// </returns>
		public static SiteConfiguration GetSiteConfigurationSection( )
		{
            return GetConfigurationSection<SiteConfiguration>(SiteSettingsKey);
        }

        /// <summary>
        ///     Gets the workflow section from the application configuration file.
        /// </summary>
        /// <returns>
        ///     An object representing the workflow section.
        /// </returns>
        public static WorkflowConfiguration GetWorkflowConfigurationSection()
        {
            return GetConfigurationSection<WorkflowConfiguration>(WorkflowSectionKey);
        }

        /// <summary>
        ///     Gets the audit log section from the application configuration file.
        /// </summary>
        /// <returns>
        ///     An object representing the audit log section.
        /// </returns>
        public static AuditLogConfiguration GetAuditLogConfigurationSection()
        {
            return GetConfigurationSection<AuditLogConfiguration>(AuditLogSectionKey);
        }

        /// <summary>
        ///     Gets the syslog section from the application configuration file.
        /// </summary>
        /// <returns>
        ///     An object representing the syslog section.
        /// </returns>
        public static SyslogConfiguration GetSyslogConfigurationSection()
        {
            return GetConfigurationSection<SyslogConfiguration>(SyslogSectionKey);
        }

        /// <summary>
        ///     Gets the syslog section from the application configuration file.
        /// </summary>
        /// <returns>
        ///     An object representing the syslog section.
        /// </returns>
        public static AppLibraryConfiguration GetAppLibraryConfigurationSection( )
        {
            return GetConfigurationSection<AppLibraryConfiguration>( AppLibrarySectionKey );
        }

        /// <summary>
        /// Gets the section containing RabbitMQ configuration settings.
        /// </summary>
        /// <returns>The configuration object.</returns>
	    public static RabbitMqConfiguration GetRabbitMqConfigurationSection()
	    {
	        return GetConfigurationSection<RabbitMqConfiguration>(RabbitMqSectionKey);
	    }

        /// <summary>
        /// Gets the section containing configuration settings for the purposes of CAST communication.
        /// </summary>
        /// <returns>The configuration object.</returns>
	    public static CastConfiguration GetCastConfigurationSection()
	    {
	        return GetConfigurationSection<CastConfiguration>(CastSectionKey);
	    }

        /// <summary>
		///     Gets the cache section from the application configuration file.
		/// </summary>
		/// <returns>
		///     An object representing the file repository configuration section.
		/// </returns>
		public static FileRepositoryConfiguration GetFileRepositoryConfigurationSection()
        {
            return GetConfigurationSection<FileRepositoryConfiguration>(FileRepositorySectionKey);
        }
        
		/// <summary>
		/// Gets the prewarm configuration section.
		/// </summary>
		/// <returns></returns>
		public static PrewarmConfiguration GetPrewarmConfigurationSection( )
		{
			return GetConfigurationSection<PrewarmConfiguration>( PrewarmSectionKey );
		}

		/// <summary>
		/// Get the configuration section
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sectionKey"></param>
		/// <returns></returns>
		private static T GetConfigurationSection<T>(string sectionKey) where T : ConfigurationSection
        {
            T configSection = null;

			try
			{
				// Get the application configuration object
				AppConfiguration appConfig = GetConfigurationSettings( );

				if ( appConfig == null || !appConfig.HasFile )
				{
					appConfig = GetConfigurationSettings( true );
				}

				if ( appConfig != null )
				{
					// Attempt to get the section of the application configuration
					configSection = ( T ) appConfig.GetSection( sectionKey );
				}
			}
			catch ( ConfigurationErrorsException )
			{
				// Get the application configuration object
				AppConfiguration appConfig = GetConfigurationSettings( true );
				if ( appConfig != null )
				{
					// Attempt to get the section of the application configuration
					configSection = ( T ) appConfig.GetSection( sectionKey );
				}
			}

            return configSection;
        }


		/// <summary>
		///     Updates the cache configuration section.
		/// </summary>
		/// <returns>
		///     An object representing the diagnostics configuration section.
		/// </returns>
		public static void UpdateCacheConfigurationSection( CacheConfiguration section )
		{
            UpdateServerConfigurationSection(section, currentSection =>
            {
                currentSection.TypeCacheSettings = section.TypeCacheSettings;
                currentSection.ResourceCacheSettings = section.ResourceCacheSettings;
                currentSection.LocalizedResourceCacheSettings = section.LocalizedResourceCacheSettings;
                currentSection.TenantResourceCacheSettings = section.TenantResourceCacheSettings;
                currentSection.Caches = section.Caches;
            });
		}

		/// <summary>
		///     Updates the database configuration section.
		/// </summary>
		/// <returns>
		///     An object representing the database configuration section.
		/// </returns>
		public static void UpdateDatabaseConfigurationSection( DatabaseConfiguration section )
		{
            UpdateServerConfigurationSection(section, currentSection =>
            {
                currentSection.ConnectionSettings = section.ConnectionSettings;
            });
		}

		/// <summary>
		///     Updates the diagnostics configuration section.
		/// </summary>
		/// <returns>
		///     An object representing the diagnostics configuration section.
		/// </returns>
		public static void UpdateDiagnosticsConfigurationSection( SiteConfiguration section )
		{
            UpdateServerConfigurationSection(section, currentSection =>
            {
                currentSection.SiteSettings = section.SiteSettings;
            });
		}

		/// <summary>
		///     Updates the diagnostics configuration section.
		/// </summary>
		/// <returns>
		///     An object representing the diagnostics configuration section.
		/// </returns>
		public static void UpdateDiagnosticsConfigurationSection( DiagnosticsConfiguration section )
		{
            UpdateServerConfigurationSection(section, currentSection =>
            {
                currentSection.LogSettings = section.LogSettings;
            });
		}
		
		/// <summary>
		///		Updates the redis configuration section.
		/// </summary>
		/// <param name="section">The section.</param>
		public static void UpdateRedisConfigurationSection( RedisConfiguration section )
		{
			UpdateServerConfigurationSection( section, currentSection =>
			{
				currentSection.Servers = section.Servers;
			} );
		}

        /// <summary>
        ///     Updates the database configuration section.
        /// </summary>
        /// <returns>
        ///     An object representing the database configuration section.
        /// </returns>
        public static void UpdateWorkflowConfigurationSection(WorkflowConfiguration section)
        {
            UpdateServerConfigurationSection(section, currentSection =>
            {
                currentSection.Triggers = section.Triggers;

            });
        }


        /// <summary>
        ///     Updates the server configuration section.
        /// </summary>
        /// <returns>
        ///     An object representing the database configuration section.
        /// </returns>
        public static void UpdateServerConfigurationSection(ServerConfiguration section)
        {
            UpdateServerConfigurationSection(section, currentSection =>
                {
                    currentSection.Security = section.Security;
                    currentSection.EntityWebApi = section.EntityWebApi;
                });
        }

		/// <summary>
		/// Update a configuration. If  it does not currently exist, add it. If it does exist use the update action to update it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="section">The section.</param>
		/// <param name="updateAction">An action to run to copy over the configuration settings.</param>
		/// <exception cref="System.ArgumentNullException">section</exception>
		/// <exception cref="System.Exception">Unable to access the configuration settings file.</exception>
		private static void UpdateServerConfigurationSection<T>(T section, Action<T> updateAction) where T : ConfigurationSection
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }

            lock (SyncLock)
            {

                // Get the application configuration object
                AppConfiguration appConfig = GetConfigurationSettings(true);
                if (appConfig == null)
                {
                    throw new Exception("Unable to access the configuration settings file.");
                }

                // Check if the section already exists
                var currentSection = (T)appConfig.GetSection(section.SectionInformation.Name);

                // Update the section
                if (currentSection != null)
                {
                    updateAction(currentSection);
                }
                else
                {
                    appConfig.Sections.Add(section.SectionInformation.Name, section);
                }

                // Updates the application configuration
                appConfig.Save( ConfigurationSaveMode.Modified );

                _appConfiguration = null;
            }
        }

		/// <summary>
		///     Returns the application configuration file as an object.
		/// </summary>
		/// <param name="reload">True to reload from disk.</param>
		/// <returns>
		///     An object representing the application configuration settings
		/// </returns>
		public static AppConfiguration GetConfigurationSettings( bool reload = false )
		{
            if (reload || _appConfiguration == null)
            {
                lock (SyncLock)
                {
                    if (_appConfiguration == null || reload)
                    {
                        _appConfiguration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                            {
                                ExeConfigFilename = ConfigurationFileName
                            }, ConfigurationUserLevel.None);
                    }
                }
            }

			return _appConfiguration;
		}
	
	}
}