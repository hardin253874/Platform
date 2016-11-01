// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AuditLog;
using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace EDC.ReadiNow.Test.Security.AuditLogTest
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogEntryDataTests
    {
        /// <summary>
        ///     Tests the type of the null metadata.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestCtrNullMetadataType()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogEntryData(false, null, null));
        }


        /// <summary>
        ///     Tests the setup audit log entry data default properties.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestSetupAuditLogEntryDataDefaultProperties()
        {
            DateTime now = DateTime.UtcNow;

            var userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            AuditLogEntryData logEntryData;
            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"p1", "p1Value"},
                {"p2", "p2Value"}
            };

            using (new SetUser(userAccount))
            {
                logEntryData = new AuditLogEntryData(false, "logonAuditLogEntryMetadata", parameters);
            }

            Assert.IsFalse(logEntryData.Success, "Success is invalid");
            Assert.AreEqual(userAccount.Name, logEntryData.UserName, "The UserName is invalid");
            Assert.AreEqual(RunAsDefaultTenant.DefaultTenantName, logEntryData.TenantName, "TenantName is invalid");
            Assert.GreaterOrEqual(logEntryData.CreatedDate, now, "CreatedDate is invalid");
            Assert.AreEqual(parameters.Count, logEntryData.Parameters.Count, "Parameters count is invalid");
            Assert.AreEqual(parameters["p1"], logEntryData.Parameters["p1"], "Parameter p1 is invalid");
            Assert.AreEqual(parameters["p2"], logEntryData.Parameters["p2"], "Parameter p2 is invalid");
            Assert.IsNotNullOrEmpty(logEntryData.Message, "The Message is invalid");
        }
    }
}