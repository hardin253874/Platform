// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Writes audit log entries to the entity model.
    /// </summary>
    internal class AuditLogEntityModelWriter : IAuditLogWriter
    {
        private readonly IAuditLogDeleter _auditLogDeleter;


        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogEntityModelWriter" /> class.
        /// </summary>
        /// <param name="auditLogDeleter">The audit log deleter.</param>
        /// <exception cref="System.ArgumentNullException">auditLogDeleter</exception>
        public AuditLogEntityModelWriter(IAuditLogDeleter auditLogDeleter)
        {
            if (auditLogDeleter == null)
            {
                throw new ArgumentNullException(nameof(auditLogDeleter));
            }

            _auditLogDeleter = auditLogDeleter;
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

			using ( DatabaseContextInfo.SetContextInfo( "Write Audit Log" ) )
			using (new SecurityBypassContext())
            {
                if (!CanWrite(entryData))
                {
                    return;
                }

                IEntity logEntry = Entity.Create(entryData.AuditLogEntryType);                

                // Set type specific fields                
                foreach (var kvp in entryData.Parameters)
                {
                    logEntry.SetField(kvp.Key, kvp.Value);
                }

                // Set base type fields and relationships
                logEntry.SetField("name", entryData.AuditLogEntryType.Name);
                logEntry.SetField("auditLogEntrySuccess", entryData.Success);
                logEntry.SetField("auditLogEntryUser", entryData.UserName);
                logEntry.SetField("auditLogEntryCreatedDate", entryData.CreatedDate);
                logEntry.SetRelationships("auditLogEntrySeverity", new EntityRelationship<AuditLogSeverityEnum>(entryData.Severity).ToEntityRelationshipCollection(), Direction.Forward);
                logEntry.SetField("auditLogEntryMessage", entryData.Message);

                logEntry.Save();

                _auditLogDeleter.Purge();
            }
        }


        #endregion


        /// <summary>
        ///     Determines whether the specified audit log severity can be logged to the entity model.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <returns></returns>
        private bool CanWrite(AuditLogSeverityEnum severity)
        {
            if (severity == null)
            {
                return false;
            }

            var auditLogSettings = Entity.Get<AuditLogSettings>("tenantAuditLogSettingsInstance");

            AuditLogSeverityEnum minAuditLogSeverity = auditLogSettings?.MinAuditLogSeverity;

            if (minAuditLogSeverity == null)
            {
                return false;
            }

            int minAuditLogSeverityOrder = minAuditLogSeverity.EnumOrder ?? -1;
            int severityOrder = severity.EnumOrder ?? -1;

            return severityOrder >= minAuditLogSeverityOrder &&
                   (severityOrder >= 0 && minAuditLogSeverityOrder >= 0);
        }


        /// <summary>
        ///     Determines whether this instance can write the specified entry data.
        /// </summary>
        /// <param name="entryData">The entry data.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entryData</exception>
        private bool CanWrite(IAuditLogEntryData entryData)
        {
            if (entryData == null)
            {
                throw new ArgumentNullException(nameof(entryData));
            }

            // When running as the global tenant do not write to the entity model.
            // This can occur when creating/deleting tenants        
            if (RequestContext.GetContext().Tenant.Id == 0)
            {
                return false;
            }

            return CanWrite(entryData.Severity);
        }
    }
}