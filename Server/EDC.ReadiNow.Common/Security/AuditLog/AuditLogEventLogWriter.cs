// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Text;
using EDC.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Writes audit log entries to the event log.
    /// </summary>
    internal class AuditLogEventLogWriter : IAuditLogWriter
    {
        /// <summary>
        ///     The event log
        /// </summary>
        private readonly IEventLog _eventLog;


        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogEventLogWriter" /> class.
        /// </summary>
        /// <param name="eventLog">The event log.</param>
        /// <exception cref="System.ArgumentNullException">eventLog</exception>
        public AuditLogEventLogWriter(IEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException(nameof(eventLog));
            }

            _eventLog = eventLog;
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

            // Writes an entry to the event log
            var eventLogMessage = new StringBuilder();
            eventLogMessage.AppendLine($"Audit log entry. Type: '{entryData.AuditLogEntryType.Name}'");
            eventLogMessage.AppendLine($"AuditLogMessage: '{entryData.Message}'");
            eventLogMessage.AppendLine($"Success: '{entryData.Success}'");
            eventLogMessage.AppendLine($"Severity: '{entryData.SeverityEnum}'");
            eventLogMessage.AppendLine($"UserName: '{entryData.UserName}'");
            eventLogMessage.AppendLine($"CreatedDate: '{entryData.CreatedDate.ToLocalTime().ToString("o")}'");

			if ( entryData.Parameters.Count > 0 )
            {
                eventLogMessage.AppendLine("Parameters:");
                foreach (var kvp in entryData.Parameters)
                {
                    eventLogMessage.AppendLine($"{kvp.Key}: '{kvp.Value}'");
                }
            }

            switch (entryData.SeverityEnum)
            {
                case AuditLogSeverityEnum_Enumeration.AuditLogError:
                    _eventLog.WriteError(eventLogMessage.ToString());
                    break;

                case AuditLogSeverityEnum_Enumeration.AuditLogInformation:
                    _eventLog.WriteInformation(eventLogMessage.ToString());
                    break;

                case AuditLogSeverityEnum_Enumeration.AuditLogWarning:
                    _eventLog.WriteWarning(eventLogMessage.ToString());
                    break;
            }
        }


        #endregion
    }
}