// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Diagnostics;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.IO;
using EDC.IO;
using EDC.Syslog;
using System.Collections.Generic;

namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     Defines the base methods and properties for the application event log.
	/// </summary>
	public static class EventLog
	{
		private static IEventLog _appLog;
		private static readonly object AppLogLock = new object( );

		/// <summary>
		///     Gets access to the application event log.
		/// </summary>
		public static IEventLog Application
		{
			get
			{
				if ( _appLog == null )
				{
					lock ( AppLogLock )
					{						                        
                        var eventLogWriters = new List<IEventLogWriter>();

                        IEventLogWriter fileEventLogWriter = GetFileEventLogWriter();
                        if (fileEventLogWriter != null)
                        {
                            eventLogWriters.Add(fileEventLogWriter);
                        }

                        IEventLogWriter syslogEventLogWriter = GetSyslogEventLogWriter();
                        if (syslogEventLogWriter != null)
                        {
                            eventLogWriters.Add(syslogEventLogWriter);
                        }

                        _appLog = new EDC.Diagnostics.EventLog(eventLogWriters);
					}
				}

				return _appLog;
			}
		}

        /// <summary>
        /// Get the file event log writer.
        /// </summary>
        /// <returns>The event log writer.</returns>
        private static IEventLogWriter GetFileEventLogWriter()
        {
            DiagnosticsConfiguration diagnosticsConfiguration = ConfigurationSettings.GetDiagnosticsConfigurationSection();

            LogSettings logSettings = diagnosticsConfiguration?.LogSettings;

            if (logSettings == null || !logSettings.IsEnabled)
            {
                return null;
            }

            string folder = SpecialFolder.GetSpecialFolderPath(SpecialMachineFolders.Log);
            string fileName = String.IsNullOrEmpty(logSettings.Filename) ? "log.xml" : logSettings.Filename;            

            return new FileEventLogWriter(folder, fileName)
            {
                MaxSize = logSettings.MaxSize,
                MaxCount = logSettings.MaxCount,
                MaxRetention = logSettings.MaxRetention,
                TraceEnabled = logSettings.TraceEnabled,
                InformationEnabled = logSettings.InformationEnabled,
                WarningEnabled = logSettings.WarningEnabled,
                ErrorEnabled = logSettings.ErrorEnabled
            };
        }

        /// <summary>
        /// Get the syslog writer.
        /// </summary>
        /// <returns>The syslog writer.</returns>
        public static IEventLogWriter GetSyslogEventLogWriter()
        {
            DiagnosticsConfiguration diagnosticsConfiguration = ConfigurationSettings.GetDiagnosticsConfigurationSection();

            EventLogSyslogSettings syslogSettings = diagnosticsConfiguration?.SyslogSettings;

            if (syslogSettings == null || !syslogSettings.IsEnabled || string.IsNullOrEmpty(syslogSettings.HostName) || syslogSettings.Port <= 0)
            {
                return null;
            }
                
            int enterpriseId = 0;
            string applicationName = string.Empty;

            SyslogConfiguration syslogConfiguration = ConfigurationSettings.GetSyslogConfigurationSection();
            if (syslogConfiguration?.SyslogApplicationSettings != null)
            {
                enterpriseId = syslogConfiguration.SyslogApplicationSettings.EnterpriseId;
                applicationName = syslogConfiguration.SyslogApplicationSettings.ApplicationName;
            }

            // Fallback
            if (enterpriseId == 0)
            {
                enterpriseId = SyslogReadiNowConstants.EnterpriseId;
            }

            if (string.IsNullOrEmpty(applicationName))
            {
                applicationName = SyslogReadiNowConstants.ApplicationName;
            }

            string databaseName = string.Empty;
            string databaseServer = string.Empty;
            var databaseConfiguration = ConfigurationSettings.GetDatabaseConfigurationSection();
            if (databaseConfiguration?.ConnectionSettings != null)
            {
                databaseName = databaseConfiguration.ConnectionSettings.Database;
                databaseServer = databaseConfiguration.ConnectionSettings.Server;
            }

            IStreamProvider tcpStreamProvider = new TcpStreamProvider(syslogSettings.HostName, syslogSettings.Port, true, syslogSettings.IsSecure, syslogSettings.IgnoreSslErrors);
            ISyslogMessageSerializer syslogMsgSerializer = new SyslogMessageSerializer();
            ISyslogMessageWriter streamWriter = new SyslogStreamWriter(tcpStreamProvider, syslogMsgSerializer);
            ISyslogMessageWriter queueingMessageWriter = new SyslogQueueingMessageWriter(streamWriter, 0);
            return new EventLogSyslogWriter(queueingMessageWriter)
            {
                EnterpriseId = enterpriseId,
                ApplicationName = applicationName,
                SoftwareVersion = SystemInfo.PlatformVersion,
                ErrorEnabled = syslogSettings.ErrorEnabled,
                WarningEnabled = syslogSettings.WarningEnabled,
                InformationEnabled = syslogSettings.InformationEnabled,
                TraceEnabled = syslogSettings.TraceEnabled,
                InstallFolder = SystemInfo.InstallFolder,
                DatabaseName = databaseName,
                DatabaseServer = databaseServer
            };            
        }
    }
}