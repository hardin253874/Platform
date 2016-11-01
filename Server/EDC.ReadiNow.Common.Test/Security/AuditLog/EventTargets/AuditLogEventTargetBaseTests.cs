// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Security.AuditLog.EventTargets;
using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace EDC.ReadiNow.Test.Security.AuditLogTest.EventTargets
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogEventTargetBaseTests
    {
        /// <summary>
        ///     Tests the constructor of the audit log event target base throws when a null audit log is used.
        /// </summary>
        [Test]
        public void TestCtrAuditLogEventTargetBaseNullLog()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogAccessRuleEventTarget(null));
        }
    }
}