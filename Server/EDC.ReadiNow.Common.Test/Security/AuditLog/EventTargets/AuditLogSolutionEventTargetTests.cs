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
    public class AuditLogSolutionEventTargetTests
    {
        /// <summary>
        ///     Tests that creating a solution writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnCreateApplication()
        {
            bool success = true;
            string solutionName = "Solution" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnCreateApplication(success, solutionName));

            var eventTarget = new AuditLogSolutionEventTarget(mockAuditLog.Object);

            var solution = new Solution {Name = solutionName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(solution, state);
            eventTarget.WriteSaveAuditLogEntries(success, solution.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests that deleting a solution writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnDeleteApplication()
        {
            bool success = true;
            string solutionName = "Solution" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnDeleteApplication(success, solutionName));

            var eventTarget = new AuditLogSolutionEventTarget(mockAuditLog.Object);

            var solution = new Solution {Name = solutionName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForDelete(solution, state);
            eventTarget.WriteDeleteAuditLogEntries(success, solution.Id, state);

            mockAuditLog.VerifyAll();
        }
    }
}