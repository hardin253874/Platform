// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Model;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Diagnostics.ActivityLog
{
    [TestFixture]
    public class ActivityLogWriterTests
    {
        [Test]
        public void Ctor()
        {
            ActivityLogWriter activityLogWriter;
            IActivityLogPurger activityLogPurger;

            activityLogPurger = new ActivityLogPurger();
            activityLogWriter = new ActivityLogWriter(activityLogPurger);

            Assert.That(activityLogWriter, Has.Property("Purger").EqualTo(activityLogPurger));
        }

        [Test]
        public void Ctor_NullPurger()
        {
            Assert.That(() => new ActivityLogWriter(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("activityLogPurger"));
        }

        [Test]
        public void WriteLogEntry_NullLogEntry()
        {
            Assert.That(() => new ActivityLogWriter(new ActivityLogPurger()).WriteLogEntry(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entry"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void WriteLogEntry_PurgerCalled()
        {
            ActivityLogWriter activityLogWriter;
            Mock<IActivityLogPurger> activityLogPurgerMock;
            LogActivityLogEntry logActivityLogEntry;
            Expression<Action<IActivityLogPurger>> purgeCall;

            activityLogPurgerMock = new Mock<IActivityLogPurger>(MockBehavior.Strict);
            purgeCall = alp => alp.Purge();
            activityLogPurgerMock.Setup(purgeCall);

            logActivityLogEntry = new LogActivityLogEntry();

            activityLogWriter = new ActivityLogWriter(activityLogPurgerMock.Object);
            activityLogWriter.WriteLogEntry(logActivityLogEntry.As<TenantLogEntry>());

            activityLogPurgerMock.Verify(purgeCall, Times.Once());
        }
    }
}
