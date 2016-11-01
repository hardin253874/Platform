// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Diagnostics.ActivityLog
{
    /// <summary>
    /// Purges activity log entries.
    /// </summary>
    public class ActivityLogPurger: IActivityLogPurger
    {
        /// <summary>
        /// The default maximum number of log entries allowed.
        /// </summary>
        public static readonly int DefaultMaxEventLogEntries = 10000;

        /// <summary>
        /// The maximum number of log entries allowed.
        /// </summary>
        public static readonly int EventLogEntriesLimit = 10000;

        /// <summary>
        /// Alias for the event log settings instance.
        /// </summary>
        internal static readonly string EventLogSettingsAlias = "core:tenantEventLogSettingsInstance";

        /// <summary>
        /// Purge any excess entries.
        /// </summary>
        public void Purge()
        {
            EventLogSettings eventLogSettings;
            int maxEventLogEntries;
            ICollection<long> eventLogEntitiesToDelete;

            using (new SecurityBypassContext())
            {
                eventLogSettings = Entity.Get<EventLogSettings>(EventLogSettingsAlias, 
                    new IEntityRef[] { EventLogSettings.MaxEventLogEntries_Field });
                if (eventLogSettings != null)
                {
                    maxEventLogEntries = eventLogSettings.MaxEventLogEntries ?? DefaultMaxEventLogEntries;
                    maxEventLogEntries = Math.Max(0, maxEventLogEntries);
                    maxEventLogEntries = Math.Min(maxEventLogEntries, EventLogEntriesLimit);

                    if (maxEventLogEntries > 0)
                    {
                        eventLogEntitiesToDelete = GetEventLogEntitiesToDelete(maxEventLogEntries);
                        if (eventLogEntitiesToDelete.Count > 0)
                        {
                            EventLog.Application.WriteInformation(
                                "Deleting {0} excess event log entry entities.",
                                eventLogEntitiesToDelete.Count());

                            Entity.Delete(eventLogEntitiesToDelete);
                        }
                    }
                }
                else
                {
                    EventLog.Application.WriteError("Event Log Settings entity missing");
                }
            }
        }

        /// <summary>
        /// Get the event log entities to delete, usually the oldest ones.
        /// </summary>
        /// <param name="maxEventLogEntities">
        /// The maximum number of event log rows to retain.
        /// </param>
        /// <returns>
        /// The IDs of the event log entities to delete.
        /// </returns>
        public ICollection<long> GetEventLogEntitiesToDelete(int maxEventLogEntities)
        {
            if (maxEventLogEntities <= 0)
            {
                throw new ArgumentException("Must be positive", "maxEventLogEntities");
            }

            string sql = @"					
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @createdDate BIGINT = dbo.fnAliasNsId( 'createdDate', 'core', @tenantId )
DECLARE @eventLogEntryType BIGINT = dbo.fnAliasNsId( 'workflowRunLogEntry', 'core', @tenantId )

CREATE TABLE #derived ( RowNum BIGINT IDENTITY(1,1), Id BIGINT PRIMARY KEY )
INSERT INTO #derived SELECT Id FROM dbo.fnDerivedTypes(@eventLogEntryType, @tenantId);

WITH auditLogRows AS
(
    SELECT ROW_NUMBER() OVER(ORDER BY dt.Data DESC) RowNum, e.Id, dt.Data
    FROM Entity e
    JOIN Data_DateTime dt ON dt.EntityId = e.Id AND dt.TenantId = @tenantId AND dt.FieldId = @createdDate 
    JOIN Relationship et ON e.Id = et.FromId AND et.TypeId = @isOfType AND et.TenantId = @tenantId
    JOIN #derived d on d.Id = et.ToId
	WHERE e.TenantId = @tenantId
)
SELECT auditLogRows.Id FROM auditLogRows
WHERE auditLogRows.RowNum > @maximumEventLogRows

DROP TABLE #derived";

            var entitiesToDelete = new List<long>();

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand command = ctx.CreateCommand(sql))
                {
                    ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);
                    ctx.AddParameter(command, "@maximumEventLogRows", DbType.Int32, maxEventLogEntities);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entitiesToDelete.Add(reader.GetInt64(0));
                        }
                    }
                }
            }

            return entitiesToDelete;
        }
    }
}
