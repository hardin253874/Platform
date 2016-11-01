// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AuditLog;
using Moq;
using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace EDC.ReadiNow.Test.Security.AuditLogTest
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogTests
    {
        /// <summary>
        ///     Verifies the audit log data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="expectedSuccess">if set to <c>true</c> [expected success].</param>
        /// <param name="expectedTypeAlias">The expected type alias.</param>
        /// <param name="expectedMetadataTypeAlias">The expected metadata type alias.</param>
        /// <param name="expectedParametersCount">The expected parameters count.</param>
        /// <returns></returns>
        private bool VerifyAuditLogData(IAuditLogEntryData data, bool expectedSuccess, string expectedTypeAlias, string expectedMetadataTypeAlias, int expectedParametersCount)
        {
            // Get the severity based on success or failure
            AuditLogSeverityEnum severity = expectedSuccess ? data.AuditLogEntryMetadata.SeveritySuccess : data.AuditLogEntryMetadata.SeverityFailure;
            AuditLogSeverityEnum_Enumeration expectedSeverityEnum = AuditLogSeverityEnum.ConvertAliasToEnum(severity.Alias) ?? AuditLogSeverityEnum_Enumeration.AuditLogInformation;

            return data.Success == expectedSuccess &&
                   data.AuditLogEntryType.Alias == expectedTypeAlias &&
                   data.AuditLogEntryMetadata.Alias == expectedMetadataTypeAlias &&
                   data.SeverityEnum == expectedSeverityEnum &&
                   data.Parameters.Count == expectedParametersCount &&
                   data.Parameters.Values.All(v => v != null);
        }


        /// <summary>
        ///     Creates the mock writer.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="typeAlias">The type alias.</param>
        /// <param name="metadataTypeAlias">The metadata type alias.</param>
        /// <param name="parametersCount">The parameters count.</param>
        /// <returns></returns>
        private Tuple<Mock<IAuditLogWriter>, IAuditLog> CreateMockWriter(bool success, string typeAlias, string metadataTypeAlias, int parametersCount)
        {
            var mockAuditLogWriter = new Mock<IAuditLogWriter>(MockBehavior.Strict);
            mockAuditLogWriter.Setup(writer => writer.Write(It.Is<IAuditLogEntryData>(d => VerifyAuditLogData(d, success, typeAlias, metadataTypeAlias, parametersCount))));

            var auditLog = new AuditLog(new List<IAuditLogWriter> {mockAuditLogWriter.Object});

            return new Tuple<Mock<IAuditLogWriter>, IAuditLog>(mockAuditLogWriter, auditLog);
        }


        /// <summary>
        /// Tests the on change access rule permissions entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnChangeAccessRulePermissionsEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:changeAccessRulePermissionsAuditLogEntry", "core:changeAccessRulePermissionsAuditLogEntryMetadata", 5);

            tuple.Item2.OnChangeAccessRulePermissions(success, "Subject", "SecuredType", "AuthReport", new SortedSet<string> {"Read"}, new SortedSet<string> {"Write"});

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on change access rule query entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnChangeAccessRuleQueryEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:changeAccessRuleQueryAuditLogEntry", "core:changeAccessRuleQueryLogEntryMetadata", 3);

            tuple.Item2.OnChangeAccessRuleQuery(success, "Subject", "SecuredType", "AuthReport");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on change password policy entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnChangePasswordPolicyEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:changePasswordPolicyAuditLogEntry", "core:changePasswordPolicyAuditLogEntryMetadata", 16);

            tuple.Item2.OnChangePasswordPolicy(success, 5, 6, 7, 8, false, true, false, true, false, true, false, false, 20, 21, 22, 23);

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on change user account expiry entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnChangeUserAccountExpiryEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:changeUserAccountExpiryAuditLogEntry", "core:changeUserAccountExpiryAuditLogEntryMetadata", 3);

            tuple.Item2.OnChangeUserAccountExpiry(success, "Admin", DateTime.Now, DateTime.Now);

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on change user account password entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnChangeUserAccountPasswordEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:changeUserAccountPasswordAuditLogEntry", "core:changeUserAccountPasswordAuditLogEntryMetadata", 1);

            tuple.Item2.OnChangeUserAccountPassword(success, "Admin");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on change user account status entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnChangeUserAccountStatusEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:changeUserAccountStatusAuditLogEntry", "core:changeUserAccountStatusAuditLogEntryMetadata", 3);

            tuple.Item2.OnChangeUserAccountStatus(success, "Admin", "Active", "Inactive");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on change user role members entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnChangeUserRoleMembersEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:changeUserRoleMembersAuditLogEntry", "core:changeUserRoleMembersAuditLogEntryMetadata", 3);

            tuple.Item2.OnChangeUserRoleMembers(success, "Users", new SortedSet<string> {"User1"}, new SortedSet<string> {"User2"});

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on create access rule entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnCreateAccessRuleEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:createAccessRuleAuditLogEntry", "core:createAccessRuleAuditLogEntryMetadata", 2);

            tuple.Item2.OnCreateAccessRule(success, "Subject", "SecuredType");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on create application entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnCreateApplicationEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:createApplicationAuditLogEntry", "core:createApplicationAuditLogEntryMetadata", 1);

            tuple.Item2.OnCreateApplication(success, "App1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on create tenant entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnCreateTenantEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:createTenantAuditLogEntry", "core:createTenantAuditLogEntryMetadata", 1);

            tuple.Item2.OnCreateTenant(success, "Tenant1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on create user account entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnCreateUserAccountEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:createUserAccountAuditLogEntry", "core:createUserAccountAuditLogEntryMetadata", 1);

            tuple.Item2.OnCreateUserAccount(success, "User1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on create user role entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnCreateUserRoleEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:createUserRoleAuditLogEntry", "core:createUserRoleAuditLogEntryMetadata", 1);

            tuple.Item2.OnCreateUserRole(success, "NewRole");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on delete access rule entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnDeleteAccessRuleEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:deleteAccessRuleAuditLogEntry", "core:deleteAccessRuleAuditLogEntryMetadata", 3);

            tuple.Item2.OnDeleteAccessRule(success, "Subject", "SecuredType", "AuthReport");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on delete application entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnDeleteApplicationEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:deleteApplicationAuditLogEntry", "core:deleteApplicationAuditLogEntryMetadata", 1);

            tuple.Item2.OnDeleteApplication(success, "App1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on delete tenant entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnDeleteTenantEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:deleteTenantAuditLogEntry", "core:deleteTenantAuditLogEntryMetadata", 1);

            tuple.Item2.OnDeleteTenant(success, "Ten1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on delete user account entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnDeleteUserAccountEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:deleteUserAccountAuditLogEntry", "core:deleteUserAccountAuditLogEntryMetadata", 1);

            tuple.Item2.OnDeleteUserAccount(success, "User1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on delete user role entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnDeleteUserRoleEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:deleteUserRoleAuditLogEntry", "core:deleteUserRoleAuditLogEntryMetadata", 1);

            tuple.Item2.OnDeleteUserRole(success, "Role1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on deploy application entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnDeployApplicationEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:deployApplicationAuditLogEntry", "core:deployApplicationAuditLogEntryMetadata", 2);

            tuple.Item2.OnDeployApplication(success, "App1", "1.0.0.1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on enable access rule entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnEnableAccessRuleEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:enableAccessRuleAuditLogEntry", "core:enableAccessRuleAuditLogEntryMetadata", 5);

            tuple.Item2.OnEnableAccessRule(success, "Subject", "SecuredType", "AuthReport", false, true);

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on expired user account entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnExpiredUserAccountEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:expireUserAccountAuditLogEntry", "core:expireUserAccountAuditLogEntryMetadata", 1);

            tuple.Item2.OnExpiredUserAccount(success, "User1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on lock user account entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnLockUserAccountEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:lockUserAccountAuditLogEntry", "core:lockUserAccountAuditLogEntryMetadata", 1);

            tuple.Item2.OnLockUserAccount(success, "User1");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        /// Tests the on logoff entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]        
        public void TestOnLogoffEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:logoffAuditLogEntry", "core:logoffAuditLogEntryMetadata", 1);

            tuple.Item2.OnLogoff(success, "Admin");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on logon entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnLogonEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:logonAuditLogEntry", "core:logonAuditLogEntryMetadata", 2);

            tuple.Item2.OnLogon(success, "Admin", "test");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on publish application entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnPublishApplicationEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:publishApplicationAuditLogEntry", "core:publishApplicationAuditLogEntryMetadata", 2);

            tuple.Item2.OnPublishApplication(success, "App2", "1.4.5");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on rename user account entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnRenameUserAccountEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:renameUserAccountAuditLogEntry", "core:renameUserAccountAuditLogEntryMetadata", 2);

            tuple.Item2.OnRenameUserAccount(success, "User2", "User3");

            tuple.Item1.VerifyAll();
        }


        /// <summary>
        ///     Tests the on rename user role entry data.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void TestOnRenameUserRoleEntryData(bool success)
        {
            Tuple<Mock<IAuditLogWriter>, IAuditLog> tuple = CreateMockWriter(success, "core:renameUserRoleAuditLogEntry", "core:renameUserRoleAuditLogEntryMetadata", 2);

            tuple.Item2.OnRenameUserRole(success, "Role1", "Role2");

            tuple.Item1.VerifyAll();
        }        
        

        /// <summary>
        /// Tests the that can continue after failure.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestThatCanContinueAfterFailure()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            // Mock writer that throws on write
            Mock<IAuditLogWriter> mockAuditLogWriter1 = mockRepository.Create<IAuditLogWriter>();
            mockAuditLogWriter1.Setup(writer => writer.Write(It.IsAny<IAuditLogEntryData>())).Throws<NullReferenceException>();            

            var auditLog = new AuditLog(new List<IAuditLogWriter> { mockAuditLogWriter1.Object });

            Assert.DoesNotThrow(() => auditLog.OnLogoff(true, "Admin"));

            mockAuditLogWriter1.Verify(writer => writer.Write(It.IsAny<IAuditLogEntryData>()), Times.Once());            

            mockRepository.VerifyAll();
        }
        

        /// <summary>
        /// Tests that an error in one writer does not affect another.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestThatWritersAreIsolatedFromEachOther()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            // Mock enabled writer1 that throws on write
            Mock<IAuditLogWriter> mockAuditLogWriter1 = mockRepository.Create<IAuditLogWriter>();
            mockAuditLogWriter1.Setup(writer => writer.Write(It.IsAny<IAuditLogEntryData>())).Throws<NullReferenceException>();

            // Mock enabled writer2
            Mock<IAuditLogWriter> mockAuditLogWriter2 = mockRepository.Create<IAuditLogWriter>();
            mockAuditLogWriter2.Setup(writer => writer.Write(It.IsAny<IAuditLogEntryData>()));

            var auditLog = new AuditLog(new List<IAuditLogWriter> { mockAuditLogWriter1.Object, mockAuditLogWriter2.Object });

            Assert.DoesNotThrow(() => auditLog.OnLogoff(true, "Admin"));

            // Verify that both writers are called
            mockAuditLogWriter1.Verify(writer => writer.Write(It.IsAny<IAuditLogEntryData>()), Times.Once());
            mockAuditLogWriter2.Verify(writer => writer.Write(It.IsAny<IAuditLogEntryData>()), Times.Once());

            mockRepository.VerifyAll();
        }
    }
}