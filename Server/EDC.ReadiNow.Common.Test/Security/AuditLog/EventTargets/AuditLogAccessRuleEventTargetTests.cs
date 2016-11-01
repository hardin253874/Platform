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
    public class AuditLogAccessRuleEventTargetTests
    {        
        /// <summary>
        ///     Tests changing an access rule's permissions writes the correct audit log message
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeAccessRulePermissions()
        {
            bool success = true;
            string subjectName = "Role" + Guid.NewGuid();
            string typeName = "Type" + Guid.NewGuid();
            string reportName = "Report" + Guid.NewGuid();
            var read = Entity.Get<Permission>("read");
            var delete = Entity.Get<Permission>("delete");
            ISet<string> oldPerm = new SortedSet<string> {read.Name};
            ISet<string> newPerm = new SortedSet<string> {read.Name, delete.Name};

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnChangeAccessRulePermissions(success, subjectName, typeName, reportName, It.Is<ISet<string>>(p => oldPerm.SetEquals(p)), It.Is<ISet<string>>(p => newPerm.SetEquals(p))));

            var eventTarget = new AuditLogAccessRuleEventTarget(mockAuditLog.Object);

            var subject = new Role {Name = subjectName};
            var type = new EntityType {Name = typeName};
            var report = new Report {Name = reportName};

            var accessRule = new AccessRule {AllowAccessBy = subject.As<Subject>(), ControlAccess = type.As<SecurableEntity>(), AccessRuleReport = report};
            accessRule.PermissionAccess.Add(read);
            accessRule.Save();

            // Change permissions            
            accessRule.PermissionAccess.Add(delete);

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(accessRule, state);
            eventTarget.WriteSaveAuditLogEntries(success, accessRule.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests changing an access rule's query writes the correct audit log message
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

            var eventTarget = new AuditLogAccessRuleEventTarget(mockAuditLog.Object);

            var subject = new Role {Name = subjectName};
            var type = new EntityType {Name = typeName};
            var report = new Report {Name = reportName};
            var newReport = new Report {Name = newReportName};

            var accessRule = new AccessRule { AllowAccessBy = subject.As<Subject>(), ControlAccess = type.As<SecurableEntity>(), AccessRuleReport = report };
            accessRule.Save();

            // Change the report
            accessRule.AccessRuleReport = newReport;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(accessRule, state);
            eventTarget.WriteSaveAuditLogEntries(success, accessRule.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests creating an access rule writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnCreateAccessRule()
        {
            bool success = true;
            string subjectName = "Role" + Guid.NewGuid();
            string typeName = "Type" + Guid.NewGuid();
            string reportName = "Report" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnCreateAccessRule(success, subjectName, typeName));

            var eventTarget = new AuditLogAccessRuleEventTarget(mockAuditLog.Object);

            var subject = new Role {Name = subjectName};
            var type = new EntityType {Name = typeName};
            var report = new Report {Name = reportName};
            var accessRule = new AccessRule { AllowAccessBy = subject.As<Subject>(), ControlAccess = type.As<SecurableEntity>(), AccessRuleReport = report, AccessRuleEnabled = true };

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(accessRule, state);
            eventTarget.WriteSaveAuditLogEntries(success, accessRule.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests deleting an access rule writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnDeleteAccessRule()
        {
            bool success = true;
            string subjectName = "Role" + Guid.NewGuid();
            string typeName = "Type" + Guid.NewGuid();
            string reportName = "Report" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnDeleteAccessRule(success, subjectName, typeName, reportName));

            var eventTarget = new AuditLogAccessRuleEventTarget(mockAuditLog.Object);

            var subject = new Role {Name = subjectName};
            var type = new EntityType {Name = typeName};
            var report = new Report {Name = reportName};
            var accessRule = new AccessRule { AllowAccessBy = subject.As<Subject>(), ControlAccess = type.As<SecurableEntity>(), AccessRuleReport = report };

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForDelete(accessRule, state);
            eventTarget.WriteDeleteAuditLogEntries(success, accessRule.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests changing an access rule's enabled state writes the correct audit log message
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnEnableAccessRule()
        {
            bool success = true;
            string subjectName = "Role" + Guid.NewGuid();
            string typeName = "Type" + Guid.NewGuid();
            string reportName = "Report" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnEnableAccessRule(success, subjectName, typeName, reportName, true, false));

            var eventTarget = new AuditLogAccessRuleEventTarget(mockAuditLog.Object);

            var subject = new Role {Name = subjectName};
            var type = new EntityType {Name = typeName};
            var report = new Report {Name = reportName};

            var accessRule = new AccessRule { AllowAccessBy = subject.As<Subject>(), ControlAccess = type.As<SecurableEntity>(), AccessRuleReport = report, AccessRuleEnabled = true };
            accessRule.Save();

            // Change the enabled state
            accessRule.AccessRuleEnabled = false;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(accessRule, state);
            eventTarget.WriteSaveAuditLogEntries(success, accessRule.Id, state);

            mockAuditLog.VerifyAll();
        }
    }
}