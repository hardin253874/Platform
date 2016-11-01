// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Net;
using EDC.Syslog;

namespace EDC.Diagnostics
{
    /// <summary>
    ///     The event log syslog writer.
    /// </summary>
    public class EventLogSyslogWriter : IEventLogWriter
    {
        /// <summary>
        ///     The event log message id.
        /// </summary>
        private const string EventLogMsgId = "eventLog";


        /// <summary>
        ///     The syslog writer.
        /// </summary>
        private readonly ISyslogMessageWriter _syslogWriter;


        /// <summary>
        /// The IP host entry.
        /// </summary>
        private readonly IPHostEntry _ipHostEntry;


        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="syslogWriter">The syslog writer.</param>
        public EventLogSyslogWriter(ISyslogMessageWriter syslogWriter)
        {
            if (syslogWriter == null)
            {
                throw new ArgumentNullException(nameof(syslogWriter));
            }

            _syslogWriter = syslogWriter;
            try
            {
                _ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            }
            catch
            {
                // ignored
            }
        }


        /// <summary>
        ///     The enterprise id.
        /// </summary>
        public int EnterpriseId { get; set; }


        /// <summary>
        ///     The application name.
        /// </summary>
        public string ApplicationName { get; set; }


        /// <summary>
        ///     The software version.
        /// </summary>
        public string SoftwareVersion { get; set; }


        /// <summary>
        ///     The install folder.
        /// </summary>
        public string InstallFolder
        {
            get
            {
                return _installFolder;
            }
            set
            {
                // Replace backslashes with forward ones as graylog is not parsing the backslash correctly                
                if (!string.IsNullOrEmpty(value))
                {
                    _installFolder = value.Replace("\\", "/");
                }                
            }
        }
        private string _installFolder;


        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the database server.
        /// </summary>
        /// <value>
        /// The database server.
        /// </value>
        public string DatabaseServer { get; set; }


        #region IEventLogWriter Members


        /// <summary>
        ///     Writes the event log entry to syslog.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        public void WriteEntry(EventLogEntry logEntry)
        {
            if (logEntry == null || !CanWriteEntry(logEntry.Level))
            {
                return;
            }

            SyslogMessage sysLogMessage = ConvertToSyslogMessage(logEntry);
            _syslogWriter.Write(sysLogMessage);
        }


        /// <summary>
        ///     Gets or sets whether error messages are logged.
        /// </summary>
        public bool ErrorEnabled { get; set; }


        /// <summary>
        ///     Gets or sets whether informational messages are logged.
        /// </summary>
        public bool InformationEnabled { get; set; }


        /// <summary>
        ///     Gets or sets whether trace messages are logged.
        /// </summary>
        public bool TraceEnabled { get; set; }


        /// <summary>
        ///     Gets or sets whether warning messages are logged.
        /// </summary>
        public bool WarningEnabled { get; set; }


        #endregion


        /// <summary>
        ///     Converts the event log entry to a syslog message.
        /// </summary>
        /// <param name="logEntry">The log entry</param>
        /// <returns></returns>
        private SyslogMessage ConvertToSyslogMessage(EventLogEntry logEntry)
        {            
            var syslogMessage = new SyslogMessage
            {
                Facility = SyslogFacility.UserLevelMessages,
                Severity = ConvertToSyslogSeverity(logEntry.Level),
                Timestamp = new DateTimeOffset(logEntry.Date),
                HostName = logEntry.Machine,
                AppName = ApplicationName,
                ProcId = logEntry.Process,
                MsgId = EventLogMsgId,
                Message = logEntry.Message
            };

            syslogMessage.StructuredDataElements.Add(CreateExtraMsgData(logEntry));
            syslogMessage.StructuredDataElements.Add(CreateSystemInfoData());
            syslogMessage.StructuredDataElements.Add(CreateOriginData(_ipHostEntry));

            return syslogMessage;
        }


        /// <summary>
        ///     Converts the event log severity to a syslog severity.
        /// </summary>
        /// <param name="level">The error level.</param>
        /// <returns></returns>
        private SyslogSeverity ConvertToSyslogSeverity(EventLogLevel level)
        {
            switch (level)
            {
                case EventLogLevel.Error:
                    return SyslogSeverity.Error;

                case EventLogLevel.Information:
                    return SyslogSeverity.Informational;

                case EventLogLevel.Trace:
                    return SyslogSeverity.Debug;

                case EventLogLevel.Warning:
                    return SyslogSeverity.Warning;

                default:
                    return SyslogSeverity.Informational;
            }
        }


        /// <summary>
        ///     Returns true if the specified error level can be written to the log, false otherwise.
        /// </summary>
        /// <param name="level">The error level.</param>
        /// <returns></returns>
        private bool CanWriteEntry(EventLogLevel level)
        {
            bool canWriteEntry = false;

            if ((level == EventLogLevel.Error) && (ErrorEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Warning) && (WarningEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Information) && (InformationEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Trace) && (TraceEnabled))
            {
                canWriteEntry = true;
            }

            return canWriteEntry;
        }

        /// <summary>
        ///     Creates the extra message data from the log entry.
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns></returns>
        private SyslogSdElement CreateExtraMsgData(EventLogEntry logEntry)
        {
            var sdElement = new SyslogSdElement($"{EventLogMsgId}@{EnterpriseId.ToString(CultureInfo.InvariantCulture)}");

            sdElement.Parameters.Add(new SyslogSdParameter("msgId", EventLogMsgId));
            sdElement.Parameters.Add(new SyslogSdParameter("tenant", logEntry.TenantName));            
            sdElement.Parameters.Add(new SyslogSdParameter("tenantId", logEntry.TenantId.ToString(CultureInfo.InvariantCulture)));
            sdElement.Parameters.Add(new SyslogSdParameter("user", logEntry.UserName));
            // Sd parameter is called logEntrySource for source as source appears be used by graylog for the name of the source machine
            sdElement.Parameters.Add(new SyslogSdParameter("logEntrySource", logEntry.Source));
            sdElement.Parameters.Add(new SyslogSdParameter("threadId", logEntry.ThreadId.ToString(CultureInfo.InvariantCulture)));            

            return sdElement;
        }

        /// <summary>
        /// Creates the syslog structured data containing system information.
        /// </summary>
        /// <returns></returns>
        private SyslogSdElement CreateSystemInfoData()
        {
            var sdElement = new SyslogSdElement($"systemInfo@{EnterpriseId.ToString(CultureInfo.InvariantCulture)}");
            
            sdElement.Parameters.Add(new SyslogSdParameter("installDirectory", InstallFolder));
            sdElement.Parameters.Add(new SyslogSdParameter("databaseName", DatabaseName));
            sdElement.Parameters.Add(new SyslogSdParameter("databaseServer", DatabaseServer));

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

            sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.EnterpriseId, EnterpriseId.ToString(CultureInfo.InvariantCulture)));
            sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.SwVersion, SoftwareVersion));
            sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.Software, ApplicationName));

            if (ipHostEntry?.AddressList != null && ipHostEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ipAddress in ipHostEntry.AddressList)
                {
                    sdElement.Parameters.Add(new SyslogSdParameter(SyslogOriginConstants.Ip, ipAddress.ToString()));
                }
            }

            return sdElement;
        }
    }
}