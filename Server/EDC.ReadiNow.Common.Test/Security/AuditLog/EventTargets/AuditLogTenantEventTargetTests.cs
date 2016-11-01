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
    public class AuditLogTenantEventTargetTests
    {
        /// <summary>
        /// Tests creating a tenant writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsGlobalTenant]
        public void TestOnCreateTenant()
        {
            bool success = true;
            string tenantName = "Tenant" + Guid.NewGuid();            

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnCreateTenant(success, tenantName));

            var eventTarget = new AuditLogTenantEventTarget(mockAuditLog.Object);

            var tenant = new Tenant {Name = tenantName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(tenant, state);
            eventTarget.WriteSaveAuditLogEntries(success, tenant.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        /// Tests deleting a tenant writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsGlobalTenant]
        public void TestOnDeleteTenant()
        {
            bool success = true;
            string tenantName = "Tenant" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnDeleteTenant(success, tenantName));

            var eventTarget = new AuditLogTenantEventTarget(mockAuditLog.Object);

            var tenant = new Tenant {Name = tenantName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForDelete(tenant, state);
            eventTarget.WriteDeleteAuditLogEntries(success, tenant.Id, state);

            mockAuditLog.VerifyAll();
        }
    }
}