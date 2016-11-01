// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;
using Moq;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.ReadiNow.Test.Security.AuditLogTest.EventTargets
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogReportEventTargetTests
    {
        /// <summary>
        ///     Tests changing an access rule's report writes the correct audit log message
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeAccessRuleQuery()
        {
            bool success = true;
            string subjectName = "Role" + Guid.NewGuid();
            string typeName = "Type" + Guid.NewGuid();
            string reportName = "Report" + Guid.NewGuid();
            string newReportName = "Report" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnChangeAccessRuleQuery(success, subjectName, typeName, newReportName));

            var eventTarget = new AuditLogReportEventTarget(mockAuditLog.Object);

            var subject = new Role {Name = subjectName};
            var type = new EntityType {Name = typeName};
            var report = new Report {Name = reportName};

            var accessRule = new AccessRule {AllowAccessBy = subject.As<Subject>(), ControlAccess = type.As<SecurableEntity>(), AccessRuleReport = report};
            accessRule.Save();

            report.Name = newReportName;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(report, state);
            eventTarget.WriteSaveAuditLogEntries(success, report.Id, state);

            mockAuditLog.VerifyAll();
        }
    }
}