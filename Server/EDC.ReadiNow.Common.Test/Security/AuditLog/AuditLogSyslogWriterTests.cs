// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AuditLog;
using EDC.Syslog;
using Moq;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.ReadiNow.Test.Security.AuditLogTest
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogSyslogWriterTests
    {
        /// <summary>
        ///     Validates the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="auditLogEventData">The audit log event data.</param>
        /// <returns></returns>
        private bool ValidateMessage(SyslogMessage message, IAuditLogEntryData auditLogEventData)
        {
            SyslogApplicationSettings syslogApplicationSettings = ConfigurationSettings.GetSyslogConfigurationSection().SyslogApplicationSettings;
            SyslogSeverity severity;

            string databaseName = string.Empty;
            string databaseServer = string.Empty;
            var databaseConfiguration = ConfigurationSettings.GetDatabaseConfigurationSection();
            if (databaseConfiguration?.ConnectionSettings != null)
            {
                databaseName = databaseConfiguration.ConnectionSettings.Database;
                databaseServer = databaseConfiguration.ConnectionSettings.Server;
            }

            switch (auditLogEventData.SeverityEnum)
            {
                case AuditLogSeverityEnum_Enumeration.AuditLogInformation:
                    severity = SyslogSeverity.Informational;
                    break;
                case AuditLogSeverityEnum_Enumeration.AuditLogWarning:
                    severity = SyslogSeverity.Warning;
                    break;
                case AuditLogSeverityEnum_Enumeration.AuditLogError:
                    severity = SyslogSeverity.Error;
                    break;
                default:
                    throw new ArgumentException("auditLogEventData");
            }

            Assert.AreEqual(1, message.Version, "The version is incorrect.");
            Assert.AreEqual(SyslogFacility.LogAudit, message.Facility, "The facility is incorrect");
            Assert.AreEqual(severity, message.Severity, "The severity is incorrect");
            Assert.AreEqual(syslogApplicationSettings.ApplicationName, message.AppName, "The app name is incorrect");
            Assert.AreEqual(auditLogEventData.AuditLogEntryMetadata.MessageId, message.MsgId, "The message id is invalid");
            Assert.AreEqual(4, message.StructuredDataElements.Count, "The number of structured data elements is invalid");
            Assert.AreEqual("audit@" + syslogApplicationSettings.EnterpriseId, message.StructuredDataElements[0].SdId);
            Assert.AreEqual(auditLogEventData.AuditLogEntryMetadata.MessageId + "@" + syslogApplicationSettings.EnterpriseId, message.StructuredDataElements[1].SdId);
            Assert.AreEqual("systemInfo@" + syslogApplicationSettings.EnterpriseId, message.StructuredDataElements[2].SdId);
            Assert.AreEqual("installDirectory", message.StructuredDataElements[2].Parameters[0].Name);
            Assert.AreEqual(SystemInfo.InstallFolder.Replace("\\", "/"), message.StructuredDataElements[2].Parameters[0].Value);

            Assert.AreEqual("databaseName", message.StructuredDataElements[2].Parameters[1].Name);
            Assert.AreEqual(databaseName, message.StructuredDataElements[2].Parameters[1].Value);

            Assert.AreEqual("databaseServer", message.StructuredDataElements[2].Parameters[2].Name);
            Assert.AreEqual(databaseServer, message.StructuredDataElements[2].Parameters[2].Value);

            Assert.AreEqual(SyslogOriginConstants.Origin, message.StructuredDataElements[3].SdId);            

            return true;
        }


        /// <summary>
        /// Tests initialising with a null syslog writer log.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestCtrNullSyslogWriter()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogSyslogWriter(null));
        }


        /// <summary>
        /// Tests the write null audit log entry to syslog.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestWriteNullAuditLogEntryToSyslog()
        {
            var mockSyslogMessageWriter = new Mock<ISyslogMessageWriter>(MockBehavior.Loose);

            var syslogWriter = new AuditLogSyslogWriter(mockSyslogMessageWriter.Object);

            Assert.Throws<ArgumentNullException>(() => syslogWriter.Write(null));
        }


        /// <summary>
        /// Tests the write audit log entry to syslog.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]        
        public void TestWriteAuditLogEntryToSyslog()
        {
            AuditLogSyslogSettings sysLogSettings = ConfigurationSettings.GetAuditLogConfigurationSection().SyslogSettings;
            bool isEnabled = sysLogSettings.IsEnabled;

            try
            {
                var mockSyslogMessageWriter = new Mock<ISyslogMessageWriter>(MockBehavior.Strict);

                // Ensure event log is enabled
                sysLogSettings.IsEnabled = true;
                var syslogWriter = new AuditLogSyslogWriter(mockSyslogMessageWriter.Object);

                IAuditLogEntryData auditLogEventData = new AuditLogEntryData(false, "logonAuditLogEntryMetadata", new Dictionary<string, object>
                {
                    {"p1", "p1Value"},
                    {"p2", "p2Value"}
                });               

                mockSyslogMessageWriter.Setup(w => w.Write(It.Is<SyslogMessage>(m => ValidateMessage(m, auditLogEventData))));

                syslogWriter.Write(auditLogEventData);

                mockSyslogMessageWriter.VerifyAll();
            }
            finally
            {
                sysLogSettings.IsEnabled = isEnabled;
            }
        }
    }
}