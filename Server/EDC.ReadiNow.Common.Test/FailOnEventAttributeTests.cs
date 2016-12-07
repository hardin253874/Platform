// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EDC.Diagnostics;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
    [TestFixture]
    public class FailOnEventAttributeTests
    {
        [Test]
        public void Test_Ctor_NoArgs()
        {
            using (FailOnEventAttribute failOnEventAttribute = new FailOnEventAttribute())
            {
                Assert.That(failOnEventAttribute,
                    Has.Property("EventLog").EqualTo(EDC.ReadiNow.Diagnostics.EventLog.Application));
                Assert.That(failOnEventAttribute, Has.Property("FailingLevels").Not.Null);
            }
        }

        [Test]
        public void Test_Ctor_Args()
        {
            IEventLog eventLog;
            EventLogLevel[] failureLevels;

            eventLog = new Mock<IEventLog>().Object;
            failureLevels = new[] { EventLogLevel.Information };

            using (FailOnEventAttribute failOnEventAttribute = new FailOnEventAttribute(eventLog, failureLevels))
            {
                Assert.That(failOnEventAttribute, Has.Property("EventLog").EqualTo(eventLog));
                Assert.That(failOnEventAttribute, Has.Property("FailingLevels").EqualTo(failureLevels));
            }
        }

        [Test]
        public void Test_DefaultFailsTest()
        {
            IEventLog eventLog;

            eventLog = EDC.ReadiNow.Diagnostics.EventLog.Application;
            using (FailOnEventAttribute failOnEventAttribute = new FailOnEventAttribute())
            {
                failOnEventAttribute.BeforeTest(null);
                Assert.That(() => eventLog.WriteTrace("Test"),
                    Throws.Nothing);
                Assert.That(() => eventLog.WriteInformation("Test"),
                    Throws.Nothing);
                Assert.That(() => eventLog.WriteWarning("Test"),
                    Throws.Nothing);
                Assert.That(() => eventLog.WriteError("Test"),
                    Throws.TypeOf<AssertionException>());
                failOnEventAttribute.AfterTest(null);
            }
        }

        [Test]
        public void Test_CustomFailsTest()
        {
            IEventLog eventLog;

            eventLog = EDC.ReadiNow.Diagnostics.EventLog.Application;
            using (FailOnEventAttribute failOnEventAttribute = new FailOnEventAttribute(new[] { EventLogLevel.Warning }))
            {
                failOnEventAttribute.BeforeTest(null);
                Assert.That(() => eventLog.WriteTrace("Test"),
                    Throws.Nothing);
                Assert.That(() => eventLog.WriteInformation("Test"),
                    Throws.Nothing);
                Assert.That(() => eventLog.WriteWarning("Test"),
                    Throws.TypeOf<AssertionException>());
                Assert.That(() => eventLog.WriteError("Test"),
                    Throws.Nothing);
                failOnEventAttribute.AfterTest(null);
            }
        }

        [Test]
        [Explicit("Will intentionally fail")]
        [FailOnEvent]
        public void Test_Failing()
        {
            EDC.ReadiNow.Diagnostics.EventLog.Application.WriteError("Test");
        }
    }
}
