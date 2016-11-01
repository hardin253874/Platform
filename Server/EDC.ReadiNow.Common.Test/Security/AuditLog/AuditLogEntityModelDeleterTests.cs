// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Data;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AuditLog;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.ReadiNow.Test.Security.AuditLogTest
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogEntityModelDeleterTests
    {
        /// <summary>
        ///     Gets the count entity model audit log entries.
        /// </summary>
        /// <returns></returns>
        private int GetCountEntityModelAuditLogEntries()
        {
            using (DatabaseContext context = DatabaseContext.GetContext())
            {
                string sql = @"DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @auditLogCreatedDate BIGINT = dbo.fnAliasNsId( 'auditLogEntryCreatedDate', 'core', @tenantId )
DECLARE @auditLogEntryType BIGINT = dbo.fnAliasNsId( 'auditLogEntry', 'core', @tenantId )

CREATE TABLE #derived ( RowNum BIGINT IDENTITY(1,1), Id BIGINT PRIMARY KEY )
INSERT INTO #derived SELECT Id FROM dbo.fnDerivedTypes(@auditLogEntryType, @tenantId);

SELECT COUNT(*)
FROM Entity e
JOIN Relationship et ON e.Id = et.FromId AND et.TypeId = @isOfType AND et.TenantId = @tenantId
JOIN #derived d on d.Id = et.ToId

DROP TABLE #derived";

                IDbCommand command = context.CreateCommand(sql);
                context.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);
                var count = (int) command.ExecuteScalar();

                return count;
            }
        }        


        /// <summary>
        ///     Tests the deleter.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [ClearCaches(ClearCachesAttribute.Caches.EntityCache | ClearCachesAttribute.Caches.EntityFieldCache, ClearCachesAttribute.Clear.AfterTest)]        
        public void TestPurge()
        {
            AuditLogEntityModelSettings entityModelSettings = ConfigurationSettings.GetAuditLogConfigurationSection().EntityModelSettings;
            bool isEnabled = entityModelSettings.IsEnabled;
            const int createdLogEntries = 10;
            const int maximumLogEntries = 5;

            try
            {
                var auditLogEntries = new List<LogonAuditLogEntry>();
                // Create 10 audit log entries
                for (int i = 0; i < createdLogEntries; i++)
                {
                    auditLogEntries.Add(new LogonAuditLogEntry());
                }

                Entity.Save(auditLogEntries);

                Assert.GreaterOrEqual(GetCountEntityModelAuditLogEntries(), createdLogEntries, "The number of log entries is invalid");

                var auditLogSettings = Entity.Get<AuditLogSettings>("tenantAuditLogSettingsInstance", true);
                auditLogSettings.MaxAuditLogEntries = maximumLogEntries;
                auditLogSettings.Save();

                entityModelSettings.IsEnabled = true;
                var deleter = new AuditLogEntityModelDeleter();
                Assert.Greater(deleter.Purge(), 0, "The number of log entries purged is invalid");

                Assert.AreEqual(maximumLogEntries, GetCountEntityModelAuditLogEntries(), "The number of remaining log entries is invalid");
            }
            finally
            {                
                entityModelSettings.IsEnabled = isEnabled;
            }
        }
    }
}