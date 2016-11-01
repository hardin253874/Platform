// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AuditLog;
using Moq;
using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace EDC.ReadiNow.Test.Security.AuditLogTest
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogEntityModelWriterTests
    {
        /// <summary>
        ///     Tests initialising with a null deleter.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestCtrNullDeleter()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogEntityModelWriter(null));
        }


        /// <summary>
        ///     Tests writing an audit log entry to entity model.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestWriteAuditLogEntryToEntityModel()
        {
            var entityModelSettings = ConfigurationSettings.GetAuditLogConfigurationSection().EntityModelSettings;
            bool isEnabled = entityModelSettings.IsEnabled;

            try
            {
                var mockDeleter = new Mock<IAuditLogDeleter>(MockBehavior.Loose);                

                // Ensure event log is enabled
                entityModelSettings.IsEnabled = true;
                var entityModelWriter = new AuditLogEntityModelWriter(mockDeleter.Object);

                string userName = "User" + Guid.NewGuid();

                IAuditLogEntryData auditLogEventData = new AuditLogEntryData(false, "logonAuditLogEntryMetadata", new Dictionary<string, object>
                {
                    {"loggedOnUserName", userName}
                });                

                entityModelWriter.Write(auditLogEventData);                

                // Verify that logon event was created
                IEnumerable<LogonAuditLogEntry> logonEvents = Entity.GetInstancesOfType<LogonAuditLogEntry>();
                LogonAuditLogEntry logonEvent = logonEvents.FirstOrDefault(l => l.LoggedOnUserName == userName);

                Assert.IsNotNull(logonEvent, "The logon event was not found");
                Assert.AreEqual(auditLogEventData.Success, logonEvent.AuditLogEntrySuccess, "Success is invalid");
                Assert.AreEqual(auditLogEventData.UserName, logonEvent.AuditLogEntryUser, "Name is invalid");
                Assert.AreEqual(auditLogEventData.Message, logonEvent.AuditLogEntryMessage, "Message is invalid");
                Assert.AreEqual(auditLogEventData.CreatedDate, logonEvent.AuditLogEntryCreatedDate, "Created Date is invalid");
                Assert.AreEqual(auditLogEventData.SeverityEnum, logonEvent.AuditLogEntrySeverity_Enum, "Severity is invalid");
            }
            finally
            {
                entityModelSettings.IsEnabled = isEnabled;
            }
        }


        /// <summary>
        ///     Tests writing a null event throws an exception.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestWriteNullEvent()
        {
            var entityModelSettings = ConfigurationSettings.GetAuditLogConfigurationSection().EntityModelSettings;
            bool isEnabled = entityModelSettings.IsEnabled;

            try
            {
                var mockDeleter = new Mock<IAuditLogDeleter>(MockBehavior.Loose);

                // Ensure event log is enabled
                entityModelSettings.IsEnabled = true;
                var entityModelWriter = new AuditLogEntityModelWriter(mockDeleter.Object);

                Assert.Throws<ArgumentNullException>(() => entityModelWriter.Write(null));
            }
            finally
            {
                entityModelSettings.IsEnabled = isEnabled;
            }
        }
        

        /// <summary>
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [ClearCaches(ClearCachesAttribute.Caches.EntityCache | ClearCachesAttribute.Caches.EntityFieldCache, ClearCachesAttribute.Clear.AfterTest)]        
        public void TestFilterEventByMinimumSeveritySetting()
        {
            // Set minimum log level to error
            var auditLogSettings = Entity.Get<AuditLogSettings>("tenantAuditLogSettingsInstance", true);
            auditLogSettings.MinAuditLogSeverity_Enum = AuditLogSeverityEnum_Enumeration.AuditLogError;
            auditLogSettings.Save();

            var entityModelSettings = ConfigurationSettings.GetAuditLogConfigurationSection().EntityModelSettings;
            bool isEnabled = entityModelSettings.IsEnabled;

            try
            {
                var mockDeleter = new Mock<IAuditLogDeleter>(MockBehavior.Loose);

                // Ensure event log is enabled
                entityModelSettings.IsEnabled = true;
                var entityModelWriter = new AuditLogEntityModelWriter(mockDeleter.Object);

                Guid userName = Guid.NewGuid();

                IAuditLogEntryData auditLogEventData = new AuditLogEntryData(true, "logonAuditLogEntryMetadata", new Dictionary<string, object>
                {
                    {"loggedOnUserName", userName.ToString()}
                });                

                entityModelWriter.Write(auditLogEventData);

                // Verify that logon event was not created
                IEnumerable<LogonAuditLogEntry> logonEvents = Entity.GetInstancesOfType<LogonAuditLogEntry>();
                LogonAuditLogEntry logonEvent = logonEvents.FirstOrDefault(l => l.LoggedOnUserName == userName.ToString());

                Assert.IsNull(logonEvent, "The logon event was found");                
            }
            finally
            {
                entityModelSettings.IsEnabled = isEnabled;
            }
        }
    }
}