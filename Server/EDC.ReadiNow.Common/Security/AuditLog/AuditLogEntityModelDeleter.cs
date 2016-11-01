// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    /// </summary>
    internal class AuditLogEntityModelDeleter : IAuditLogDeleter
    {
        #region IAuditLogDeleter Members


        /// <summary>
        ///     Deletes the oldest audit log entries when the maximum number is exceeded.
        /// </summary>
        /// <returns></returns>
        public int Purge()
        {
            var auditLogSettings = Entity.Get<AuditLogSettings>("tenantAuditLogSettingsInstance");

            if (auditLogSettings == null)
            {
                return 0;
            }

            int maxAuditLogEntries = auditLogSettings.MaxAuditLogEntries ?? 10000;

            if (maxAuditLogEntries < 0)
            {
                return 0;
            }

            IEnumerable<long> auditLogEntitiesToDelete = GetAuditLogEntitiesToDelete(maxAuditLogEntries);

	        var logEntitiesToDelete = auditLogEntitiesToDelete as IList<long> ?? auditLogEntitiesToDelete.ToList( );

			if ( logEntitiesToDelete.Count <= 0 )
            {
                return 0;
            }

            EventLog.Application.WriteInformation("Deleting {0} excess audit log entry entities.", logEntitiesToDelete.Count);

            Entity.Delete(logEntitiesToDelete);

            return logEntitiesToDelete.Count;
        }


        #endregion


        /// <summary>
        ///     Gets the audit log entities to delete.
        /// </summary>
        /// <param name="maximumAuditLogRows">The maximum audit log rows.</param>
        /// <returns></returns>
        private IEnumerable<long> GetAuditLogEntitiesToDelete(int maximumAuditLogRows)
        {
            string sql = @"					
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @createdDate BIGINT = dbo.fnAliasNsId( 'createdDate', 'core', @tenantId )
DECLARE @auditLogEntryType BIGINT = dbo.fnAliasNsId( 'auditLogEntry', 'core', @tenantId )

CREATE TABLE #derived ( RowNum BIGINT IDENTITY(1,1), Id BIGINT PRIMARY KEY )
INSERT INTO #derived SELECT Id FROM dbo.fnDerivedTypes(@auditLogEntryType, @tenantId);

WITH auditLogRows AS
(
    SELECT ROW_NUMBER() OVER(ORDER BY dt.Data ASC) RowNum, e.Id, dt.Data
    FROM Entity e
    LEFT JOIN Data_DateTime dt ON dt.EntityId = e.Id AND dt.TenantId = @tenantId AND dt.FieldId = @createdDate 
    JOIN Relationship et ON e.Id = et.FromId AND et.TypeId = @isOfType AND et.TenantId = @tenantId
    JOIN #derived d on d.Id = et.ToId
	WHERE e.TenantId = @tenantId
)
SELECT auditLogRows.Id FROM auditLogRows
WHERE auditLogRows.RowNum > @maximumAuditLogRows

DROP TABLE #derived";

            var entitiesToDelete = new List<long>();

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand command = ctx.CreateCommand(sql))
                {
                    ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);
                    ctx.AddParameter(command, "@maximumAuditLogRows", DbType.Int32, maximumAuditLogRows);

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