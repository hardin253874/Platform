// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.IO;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.Syslog;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Represents the audit log.
    /// </summary>
    public static class AuditLogInstance
    {
        /// <summary>
        ///     The audit log instance
        /// </summary>
        private static readonly IAuditLog AuditLogInstanceInternal;


        /// <summary>
        ///     Initializes the <see cref="AuditLogInstance" /> class.
        /// </summary>
        static AuditLogInstance()
        {
            IList<IAuditLogWriter> auditLogWriters = new List<IAuditLogWriter>();

            try
            {
                AuditLogConfiguration auditLogConfiguration = ConfigurationSettings.GetAuditLogConfigurationSection();

                if (auditLogConfiguration?.EventLogSettings != null && auditLogConfiguration.EventLogSettings.IsEnabled)
                {
                    auditLogWriters.Add(new AuditLogEventLogWriter(EventLog.Application));
                }

                if (auditLogConfiguration?.EntityModelSettings != null && auditLogConfiguration.EntityModelSettings.IsEnabled)
                {
                    auditLogWriters.Add(new AuditLogEntityModelWriter(new AuditLogEntityModelDeleter()));
                }

                if (auditLogConfiguration?.SyslogSettings != null && auditLogConfiguration.SyslogSettings.IsEnabled && !string.IsNullOrEmpty(auditLogConfiguration.SyslogSettings.HostName) && auditLogConfiguration.SyslogSettings.Port > 0)
                {
                    IStreamProvider tcpStreamProvider = new TcpStreamProvider(auditLogConfiguration.SyslogSettings.HostName, auditLogConfiguration.SyslogSettings.Port, true, auditLogConfiguration.SyslogSettings.IsSecure, auditLogConfiguration.SyslogSettings.IgnoreSslErrors);
                    ISyslogMessageSerializer syslogMsgSerializer = new SyslogMessageSerializer();
                    ISyslogMessageWriter streamWriter = new SyslogStreamWriter(tcpStreamProvider, syslogMsgSerializer);
                    ISyslogMessageWriter queueingMessageWriter = new SyslogQueueingMessageWriter(streamWriter);

                    auditLogWriters.Add(new AuditLogSyslogWriter(queueingMessageWriter));
                }
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("AuditLogInstance failed to initialize. Error: {0}.", ex.ToString());
            }

            AuditLogInstanceInternal = new AuditLog(auditLogWriters);
        }


        /// <summary>
        ///     Gets the audit log.
        /// </summary>
        /// <value>
        ///     The audit log.
        /// </value>
        public static IAuditLog Get()
        {
            return AuditLogInstanceInternal;
        }
    }
}