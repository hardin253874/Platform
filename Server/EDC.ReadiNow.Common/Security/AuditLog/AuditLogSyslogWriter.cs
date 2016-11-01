// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.Syslog;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Writes audit log entries to the sysLog.
    /// </summary>
    internal class AuditLogSyslogWriter : IAuditLogWriter
    {
        /// <summary>
        ///     The syslog message writer
        /// </summary>
        private readonly ISyslogMessageWriter _syslogMessageWriter;

        /// <summary>
        /// The _enterprise identifier
        /// </summary>
        private readonly int _enterpriseId;

        /// <summary>
        /// The _application name
        /// </summary>
        private readonly string _applicationName;

        /// <summary>
        /// This process
        /// </summary>
        private readonly string _processName;

        /// <summary>
        /// This machines ip host entry.
        /// </summary>
        private readonly IPHostEntry _ipHostEntry;

        /// <summary>
        /// The host name.
        /// </summary>
        private readonly string _hostName;

        /// <summary>
        /// The install folder.
        /// </summary>
        private readonly string _installFolder;

        /// <summary>
        /// The database name.
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        /// The database server.
        /// </summary>
        private readonly string _databaseServer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogSyslogWriter" /> class.
        /// </summary>
        /// <param name="syslogMessageWriter">The syslog message writer.</param>
        /// <exception cref="System.ArgumentNullException">syslogMessageWriter</exception>
        public AuditLogSyslogWriter(ISyslogMessageWriter syslogMessageWriter)
        {
            if (syslogMessageWriter == null)
            {
                throw new ArgumentNullException(nameof(syslogMessageWriter));
            }

            _syslogMessageWriter = syslogMessageWriter;

            SyslogConfiguration syslogConfiguration = ConfigurationSettings.GetSyslogConfigurationSection();
            if (syslogConfiguration?.SyslogApplicationSettings != null)
            {
                _enterpriseId = syslogConfiguration.SyslogApplicationSettings.EnterpriseId;
                _applicationName = syslogConfiguration.SyslogApplicationSettings.ApplicationName;
            }

            // Fallback
            if (_enterpriseId == 0)
            {
                _enterpriseId = SyslogReadiNowConstants.EnterpriseId;    
            }

            if (string.IsNullOrEmpty(_applicationName))
            {
                _applicationName = SyslogReadiNowConstants.ApplicationName;               
            }

            _databaseName = string.Empty;
            _databaseServer = string.Empty;
            var databaseConfiguration = ConfigurationSettings.GetDatabaseConfigurationSection();
            if (databaseConfiguration?.ConnectionSettings != null)
            {
                _databaseName = databaseConfiguration.ConnectionSettings.Database;
                _databaseServer = databaseConfiguration.ConnectionSettings.Server;
            }

            _processName = Process.GetCurrentProcess().MainModule.ModuleName;
            _hostName = GetHostName();

            try
            {
                _ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            }
            catch
            {
                // ignored
            }

            if (!string.IsNullOrEmpty(SystemInfo.InstallFolder))
            {
                _installFolder = SystemInfo.InstallFolder.Replace("\\", "/");
            }            
        }


        #region IAuditLogWriter Members


        /// <summary>
        ///     Writes the specified audit log entry.
        /// </summary>
        /// <param name="entryData">The entry data.</param>
        /// <exception cref="System.ArgumentNullException">entryData</exception>
        public void Write(IAuditLogEntryData entryData)
        {
            if (entryData == null)
            {
                throw new ArgumentNullException(nameof(entryData));
            }            

            using (new SecurityBypassContext())
            {                
                var syslogMessage = new SyslogMessage
                {
                    Facility = SyslogFacility.LogAudit,
                    Severity = ConvertToSyslogSeverity(entryData.SeverityEnum),
                    Timestamp = new DateTimeOffset(entryData.CreatedDate),
                    HostName = _hostName,
                    AppName = _applicationName,
                    ProcId = _processName,
                    MsgId = entryData.AuditLogEntryMetadata.MessageId
                };

                syslogMessage.StructuredDataElements.Add(CreateBaseMsgData(entryData));                
                syslogMessage.StructuredDataElements.Add(CreateSpecificMsgData(entryData));
                syslogMessage.StructuredDataElements.Add(CreateSystemInfoData());
                syslogMessage.StructuredDataElements.Add(CreateOriginData(_ipHostEntry));
                
                _syslogMessageWriter.Write(syslogMessage);                    
            }
        }


        #endregion


        /// <summary>
        ///     Creates the syslog structured data containing parameters common to all audit log messages.
        /// </summary>
        /// <param name="entryData">The entry data.</param>
        /// <returns></returns>
        private SyslogSdElement CreateBaseMsgData(IAuditLogEntryData entryData)
        {
            var sdElement = new SyslogSdElement($"audit@{_enterpriseId.ToString(CultureInfo.InvariantCulture)}");

            sdElement.Parameters.Add(new SyslogSdParameter("msgId", entryData.AuditLogEntryMetadata.MessageId));
            sdElement.Parameters.Add(new SyslogSdParameter("success", entryData.Success.ToString()));
            sdElement.Parameters.Add(new SyslogSdParameter("tenant", RequestContext.GetContext().Tenant.Name));
            sdElement.Parameters.Add(new SyslogSdParameter("user", entryData.UserName));

            return sdElement;
        }

        /// <summary>
        /// Creates the syslog structured data containing system information.
        /// </summary>
        /// <returns></returns>
        private SyslogSdElement CreateSystemInfoData()
        {
            var sdElement = new SyslogSdElement($"systemInfo@{_enterpriseId.ToString(CultureInfo.InvariantCulture)}");

            sdElement.Parameters.Add(new SyslogSdParameter("installDirectory", _installFolder));
            sdElement.Parameters.Add(new SyslogSdParameter("databaseName", _databaseName));
            sdElement.Parameters.Add(new SyslogSdParameter("databaseServer", _databaseServer));

            return sdElement;
        }


        /// <summary>
        ///     Creates the syslog structured data containing parameters specific to the audit log message.
        /// </summary>
        /// <param name="entryData">The entry data.</param>
        /// <returns></returns>
        private SyslogSdElement CreateSpecificMsgData(IAuditLogEntryData entryData)
        {
            var sdElement = new SyslogSdElement($"{entryData.AuditLogEntryMetadata.MessageId}@{_enterpriseId.ToString(CultureInfo.InvariantCulture)}");

            // Set type specific fields                
            foreach (var kvp in entryData.Parameters)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                string value = kvp.Value.ToString();

                if (kvp.Value is DateTime?)
                {
                    value = $"{kvp.Value:o}";
                }

                sdElement.Parameters.Add(new SyslogSdParameter(kvp.Key, value));
            }

            return sdElement;
        }


        /// <summary>
        ///     Creates the syslog structured data containing origin parameters.
        /// </summary>
        /// <param name="ipHostEntry">The ip host entry.</param>
        /// <returns></returns>
        private SyslogSdElement CreateOriginData(IPHostEntry ipHostEntry)
        {
            var sdElement = new SyslogSdElement(SyslogOriginConstants.Origin);

            sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.EnterpriseId, _enterpriseId.ToString(CultureInfo.InvariantCulture)));
            sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.SwVersion, SystemInfo.PlatformVersion));
            sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.Software, _applicationName));

            if (ipHostEntry?.AddressList != null && ipHostEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ipAddress in ipHostEntry.AddressList)
                {
                    sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.Ip, ipAddress.ToString()));
                }
            }

            return sdElement;
        }


        /// <summary>
        ///     Converts to syslog severity.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">severity</exception>
        private SyslogSeverity ConvertToSyslogSeverity(AuditLogSeverityEnum_Enumeration severity)
        {
            switch (severity)
            {
                case AuditLogSeverityEnum_Enumeration.AuditLogInformation:
                    return SyslogSeverity.Informational;
                case AuditLogSeverityEnum_Enumeration.AuditLogWarning:
                    return SyslogSeverity.Warning;
                case AuditLogSeverityEnum_Enumeration.AuditLogError:
                    return SyslogSeverity.Error;
                default:
                    throw new ArgumentException("severity");
            }
        }

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <returns></returns>
        private string GetHostName()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).HostName;
            }
            catch
            {
                return Environment.MachineName;
            }
        }
    }
}