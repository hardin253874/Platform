// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Diagnostics;
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
    public class AuditLogEventLogWriterTests
    {
        /// <summary>
        /// Tests initialising with a null event log.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]        
        public void TestCtrNullEventLog()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogEventLogWriter(null));
        }        


        /// <summary>
        ///     Tests the write audit log entry to event log.
        /// </summary>
        /// <param name="severity">The severity.</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(AuditLogSeverityEnum_Enumeration.AuditLogError)]
        [TestCase(AuditLogSeverityEnum_Enumeration.AuditLogInformation)]
        [TestCase(AuditLogSeverityEnum_Enumeration.AuditLogWarning)]
        [ClearCaches(ClearCachesAttribute.Caches.EntityCache | ClearCachesAttribute.Caches.EntityFieldCache | ClearCachesAttribute.Caches.EntityRelationshipCache, ClearCachesAttribute.Clear.AfterTest)]
        public void TestWriteAuditLogEntryToEventLog(AuditLogSeverityEnum_Enumeration severity)
        {
            var eventLogSettings = ConfigurationSettings.GetAuditLogConfigurationSection().EventLogSettings;
            bool isEnabled = eventLogSettings.IsEnabled;

            try
            {
                var mockEventLog = new Mock<IEventLog>(MockBehavior.Strict);

                switch (severity)
                {
                    case AuditLogSeverityEnum_Enumeration.AuditLogError:                        
                        mockEventLog.Setup(l => l.WriteError(It.Is<string>(s => s.StartsWith("Audit log entry.") && s.Contains("p1Value") && s.Contains("p2Value"))));
                        break;
                    case AuditLogSeverityEnum_Enumeration.AuditLogInformation:
                        mockEventLog.Setup(l => l.WriteInformation(It.Is<string>(s => s.StartsWith("Audit log entry.") && s.Contains("p1Value") && s.Contains("p2Value"))));
                        break;
                    case AuditLogSeverityEnum_Enumeration.AuditLogWarning:
                        mockEventLog.Setup(l => l.WriteWarning(It.Is<string>(s => s.StartsWith("Audit log entry.") && s.Contains("p1Value") && s.Contains("p2Value"))));
                        break;
                }

                // Ensure event log is enabled
                eventLogSettings.IsEnabled = true;
                var eventLogWriter = new AuditLogEventLogWriter(mockEventLog.Object);

                // Override severity
                AuditLogEntryMetadata metaData = Entity.Get<AuditLogEntryMetadata>("logonAuditLogEntryMetadata", true);
                metaData.SeverityFailure_Enum = severity;
                metaData.Save();

                IAuditLogEntryData auditLogEventData = new AuditLogEntryData(false, "logonAuditLogEntryMetadata", new Dictionary<string, object>
                {
                    {"p1", "p1Value"},
                    {"p2", "p2Value"}
                });                

                eventLogWriter.Write(auditLogEventData);

                mockEventLog.VerifyAll();
            }
            finally
            {
                eventLogSettings.IsEnabled = isEnabled;
            }
        }
        

        /// <summary>
        /// Tests writing a null event throws an exception.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestWriteNullEvent()
        {
            var eventLogSettings = ConfigurationSettings.GetAuditLogConfigurationSection().EventLogSettings;
            bool isEnabled = eventLogSettings.IsEnabled;

            try
            {
                var mockEventLog = new Mock<IEventLog>(MockBehavior.Strict);

                // Ensure event log is enabled
                eventLogSettings.IsEnabled = true;
                var eventLogWriter = new AuditLogEventLogWriter(mockEventLog.Object);

                Assert.Throws<ArgumentNullException>(() => eventLogWriter.Write(null));                
            }
            finally
            {
                eventLogSettings.IsEnabled = isEnabled;
            }
        }
    }
}