// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Security.AuditLog;
using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace EDC.ReadiNow.Test.Security.AuditLogTest
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogInstanceTests
    {
        /// <summary>
        ///     Tests the audit log instance.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestAuditLogInstance()
        {
            Assert.IsInstanceOf<AuditLog>(AuditLogInstance.Get());
        }
    }
}