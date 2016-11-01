// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.Globalization;
using EDC.Diagnostics;
using EDC.ReadiNow.Test;
using EDC.Syslog;
using Moq;
using NUnit.Framework;
using EventLogEntry = EDC.Diagnostics.EventLogEntry;

namespace EDC.Test.Diagnostics
{
    [TestFixture]
    public class EventLogSyslogWriterTests
    {
        /// <summary>
        ///     Validates the message.
        /// </summary>
        /// <param name="syslogWriter"></param>
        /// <param name="message">The message.</param>
        /// <param name="logEntry">The event log data.</param>
        /// <returns></returns>
        private bool ValidateMessage(EventLogSyslogWriter syslogWriter, SyslogMessage message, EventLogEntry logEntry)
        {
            SyslogSeverity severity;

            switch (logEntry.Level)
            {
                case EventLogLevel.Error:
                    severity = SyslogSeverity.Error;
                    break;
                case EventLogLevel.Warning:
                    severity = SyslogSeverity.Warning;
                    break;
                case EventLogLevel.Information:
                    severity = SyslogSeverity.Informational;
                    break;
                case EventLogLevel.Trace:
                    severity = SyslogSeverity.Debug;
                    break;
                default:
                    throw new ArgumentException("logEntry");
            }

            Assert.AreEqual(1, message.Version, "The version is incorrect.");
            Assert.AreEqual(SyslogFacility.UserLevelMessages, message.Facility, "The facility is incorrect");
            Assert.AreEqual(severity, message.Severity, "The severity is incorrect");
            Assert.AreEqual(syslogWriter.ApplicationName, message.AppName, "The app name is incorrect");
            Assert.AreEqual(logEntry.Machine, message.HostName, "The host name is incorrect");
            Assert.AreEqual(logEntry.Process, message.ProcId, "The process id is incorrect");
            Assert.AreEqual(logEntry.Message, message.Message, "The message is incorrect");
            Assert.AreEqual("eventLog", message.MsgId, "The message id is invalid");

            Assert.AreEqual(3, message.StructuredDataElements.Count, "The number of structured data elements is invalid");

            Assert.AreEqual("eventLog@" + syslogWriter.EnterpriseId, message.StructuredDataElements[0].SdId);
            Assert.AreEqual("msgId", message.StructuredDataElements[0].Parameters[0].Name);
            Assert.AreEqual("eventLog", message.StructuredDataElements[0].Parameters[0].Value);
            Assert.AreEqual("tenant", message.StructuredDataElements[0].Parameters[1].Name);
            Assert.AreEqual(logEntry.TenantName, message.StructuredDataElements[0].Parameters[1].Value);
            Assert.AreEqual("tenantId", message.StructuredDataElements[0].Parameters[2].Name);
            Assert.AreEqual(logEntry.TenantId.ToString(CultureInfo.InvariantCulture), message.StructuredDataElements[0].Parameters[2].Value);
            Assert.AreEqual("user", message.StructuredDataElements[0].Parameters[3].Name);
            Assert.AreEqual(logEntry.UserName, message.StructuredDataElements[0].Parameters[3].Value);
            Assert.AreEqual("logEntrySource", message.StructuredDataElements[0].Parameters[4].Name);
            Assert.AreEqual(logEntry.Source, message.StructuredDataElements[0].Parameters[4].Value);
            Assert.AreEqual("threadId", message.StructuredDataElements[0].Parameters[5].Name);
            Assert.AreEqual(logEntry.ThreadId.ToString(CultureInfo.InvariantCulture), message.StructuredDataElements[0].Parameters[5].Value);            

            Assert.AreEqual("systemInfo@" + syslogWriter.EnterpriseId, message.StructuredDataElements[1].SdId);
            Assert.AreEqual("installDirectory", message.StructuredDataElements[1].Parameters[0].Name);
            Assert.AreEqual(syslogWriter.InstallFolder.Replace("\\", "/"), message.StructuredDataElements[1].Parameters[0].Value);
            Assert.AreEqual("databaseName", message.StructuredDataElements[1].Parameters[1].Name);
            Assert.AreEqual(syslogWriter.DatabaseName, message.StructuredDataElements[1].Parameters[1].Value);
            Assert.AreEqual("databaseServer", message.StructuredDataElements[1].Parameters[2].Name);
            Assert.AreEqual(syslogWriter.DatabaseServer, message.StructuredDataElements[1].Parameters[2].Value);

            Assert.AreEqual(SyslogOriginConstants.Origin, message.StructuredDataElements[2].SdId);

            return true;
        }


        /// <summary>
        ///     Tests initialising with a null syslog writer log.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestCtrNullSyslogWriter()
        {
            Assert.Throws<ArgumentNullException>(() => new EventLogSyslogWriter(null));
        }


        /// <summary>
        ///     Tests writing an event log entry to syslog.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(EventLogLevel.Error)]
        [TestCase(EventLogLevel.Warning)]
        [TestCase(EventLogLevel.Information)]
        [TestCase(EventLogLevel.Trace)]
        public void TestWriteEventLogEntryToSyslog(EventLogLevel severity)
        {
            var mockSyslogMessageWriter = new Mock<ISyslogMessageWriter>(MockBehavior.Strict);

            var syslogWriter = new EventLogSyslogWriter(mockSyslogMessageWriter.Object)
            {
                InstallFolder = @"C:\InstallFolder",
                ApplicationName = "AppName",
                DatabaseName = "DbName",
                DatabaseServer = "DbServerName",
                EnterpriseId = 11111,
                SoftwareVersion = "1.0",
                ErrorEnabled = severity == EventLogLevel.Error,
                InformationEnabled = severity == EventLogLevel.Information,
                WarningEnabled = severity == EventLogLevel.Warning,
                TraceEnabled = severity == EventLogLevel.Trace
            };

            var logEntry = new EventLogEntry(Guid.NewGuid(), DateTime.Now, Stopwatch.GetTimestamp(), severity, "MachineName", "ProcessName", 10, "MsgSource", "Test Message", 5555,
                "FRED", "Bob");

            mockSyslogMessageWriter.Setup(w => w.Write(It.Is<SyslogMessage>(m => ValidateMessage(syslogWriter, m, logEntry))));

            syslogWriter.WriteEntry(logEntry);

            mockSyslogMessageWriter.VerifyAll();
        }


        /// <summary>
        ///     Tests writing a null event log entry to syslog.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestWriteNullEventLogEntryToSyslog()
        {
            var mockSyslogMessageWriter = new Mock<ISyslogMessageWriter>(MockBehavior.Loose);

            var syslogWriter = new EventLogSyslogWriter(mockSyslogMessageWriter.Object);

            Assert.DoesNotThrow(() => syslogWriter.WriteEntry(null));
        }
    }
}