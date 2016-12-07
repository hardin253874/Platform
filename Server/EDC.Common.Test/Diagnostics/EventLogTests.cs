// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Diagnostics;
using NUnit.Framework;

namespace EDC.Test.Diagnostics
{
    [TestFixture]
    public class EventLogTests
    {
        private class TestLogWriter : IEventLogWriter
        {
            public EventLogEntry LastEntry { get; private set; }
            public bool ErrorEnabled { get; set; }
            public bool InformationEnabled { get; set; }
            public bool TraceEnabled { get; set; }
            public bool WarningEnabled { get; set; }

            public void WriteEntry(EventLogEntry entryData)
            {
                LastEntry = entryData;
            }
        }

        [Test]
        public void EnsureErrorMessageIsTruncated()
        {
            var testLogWriter = new TestLogWriter
            {
                ErrorEnabled = true,
                InformationEnabled = true,
                TraceEnabled = true,
                WarningEnabled = true
            };

            var logWriters = new List<IEventLogWriter> {testLogWriter};

            var eventLog = new EventLog(logWriters);

            var msg = "E" + Guid.NewGuid();
            eventLog.WriteError(msg);
            Assert.AreEqual(msg, testLogWriter.LastEntry.Message);

            msg = string.Join("", Enumerable.Repeat("E", 15000));
            var truncatedMsg = msg.Substring(0, 10000);
            eventLog.WriteError(msg);
            Assert.AreEqual(truncatedMsg, testLogWriter.LastEntry.Message);
        }

        [Test]
        public void EnsureInfoMessageIsTruncated()
        {
            var testLogWriter = new TestLogWriter
            {
                ErrorEnabled = true,
                InformationEnabled = true,
                TraceEnabled = true,
                WarningEnabled = true
            };

            var logWriters = new List<IEventLogWriter> {testLogWriter};

            var eventLog = new EventLog(logWriters);

            var msg = "I" + Guid.NewGuid();
            eventLog.WriteInformation(msg);
            Assert.AreEqual(msg, testLogWriter.LastEntry.Message);

            msg = string.Join("", Enumerable.Repeat("I", 15000));
            var truncatedMsg = msg.Substring(0, 10000);
            eventLog.WriteInformation(msg);
            Assert.AreEqual(truncatedMsg, testLogWriter.LastEntry.Message);
        }

        [Test]
        public void EnsureTraceMessageIsTruncated()
        {
            var testLogWriter = new TestLogWriter
            {
                ErrorEnabled = true,
                InformationEnabled = true,
                TraceEnabled = true,
                WarningEnabled = true
            };

            var logWriters = new List<IEventLogWriter> {testLogWriter};

            var eventLog = new EventLog(logWriters);

            var msg = "T" + Guid.NewGuid();
            eventLog.WriteTrace(msg);
            Assert.AreEqual(msg, testLogWriter.LastEntry.Message);

            msg = string.Join("", Enumerable.Repeat("T", 15000));
            var truncatedMsg = msg.Substring(0, 10000);
            eventLog.WriteTrace(msg);
            Assert.AreEqual(truncatedMsg, testLogWriter.LastEntry.Message);
        }

        [Test]
        public void EnsureWarningMessageIsTruncated()
        {
            var testLogWriter = new TestLogWriter
            {
                ErrorEnabled = true,
                InformationEnabled = true,
                TraceEnabled = true,
                WarningEnabled = true
            };

            var logWriters = new List<IEventLogWriter> {testLogWriter};

            var eventLog = new EventLog(logWriters);

            var msg = "W" + Guid.NewGuid();
            eventLog.WriteWarning(msg);
            Assert.AreEqual(msg, testLogWriter.LastEntry.Message);

            msg = string.Join("", Enumerable.Repeat("W", 15000));
            var truncatedMsg = msg.Substring(0, 10000);
            eventLog.WriteWarning(msg);
            Assert.AreEqual(truncatedMsg, testLogWriter.LastEntry.Message);
        }
    }
}