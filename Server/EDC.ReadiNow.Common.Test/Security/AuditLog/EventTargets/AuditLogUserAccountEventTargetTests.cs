// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AuditLogUserAccountEventTargetTests
    {
        /// <summary>
        ///     Tests that changing the account expiry for a user account writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeUserAccountExpiry()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();
            DateTime oldExpirationDate = DateTime.UtcNow;
            DateTime newExpirationDate = DateTime.UtcNow.AddDays(1);

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnChangeUserAccountExpiry(success, userName, oldExpirationDate, newExpirationDate));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};
            user.AccountExpiration = oldExpirationDate;
            user.Save();

            user.AccountExpiration = newExpirationDate;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests that changing a user account password writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeUserAccountPassword()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnChangeUserAccountPassword(success, userName));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};
            user.Password = "P@ssw0rd1";
            user.Save();

            user.Password = "P@ssw0rd12";

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests that changing a user account status writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeUserAccountStatus()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnChangeUserAccountStatus(success, userName, UserAccountStatusEnum_Enumeration.Active.ToString(), UserAccountStatusEnum_Enumeration.Disabled.ToString()));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};
            user.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            user.Save();

            user.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Disabled;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests that changing a user account status writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeUserRoleMembers()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();
            string roleName1 = "Role1" + Guid.NewGuid();
            string roleName2 = "Role2" + Guid.NewGuid();

            ISet<string> addedMembers = new SortedSet<string> {userName};

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
			mockAuditLog.Setup( al => al.OnChangeUserRoleMembers( success, roleName1, It.Is<ISet<string>>( m => m.Count <= 0 ), It.Is<ISet<string>>( m => m.SetEquals( addedMembers ) ) ) );

			mockAuditLog.Setup( al => al.OnChangeUserRoleMembers( success, roleName2, It.Is<ISet<string>>( m => m.SetEquals( addedMembers ) ), It.Is<ISet<string>>( m => m.Count <= 0 ) ) );

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var role1 = new Role {Name = roleName1};
            var role2 = new Role {Name = roleName2};

            var user = new UserAccount {Name = userName};
            user.UserHasRole.Add(role1);
            user.Save();

            user.UserHasRole.Remove(role1);
            user.UserHasRole.Add(role2);

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests creating a user account writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnCreateUserAccount()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnCreateUserAccount(success, userName));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests deleting a user writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnDeleteUserAccount()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnDeleteUserAccount(success, userName));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForDelete(user, state);
            eventTarget.WriteDeleteAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests that expiring a user account writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnExpiredUserAccount()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnExpiredUserAccount(success, userName));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};
            user.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            user.Save();

            user.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Expired;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests that locking a user account writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnLockUserAccount()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnLockUserAccount(success, userName));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};
            user.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            user.Save();

            user.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Locked;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests renaming a user account writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnRenameUserAccount()
        {
            bool success = true;
            string userName = "User" + Guid.NewGuid();
            string newUserName = "User" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnRenameUserAccount(success, userName, newUserName));

            var eventTarget = new AuditLogUserAccountEventTarget(mockAuditLog.Object);

            var user = new UserAccount {Name = userName};
            user.Save();

            user.Name = newUserName;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(user, state);
            eventTarget.WriteSaveAuditLogEntries(success, user.Id, state);

            mockAuditLog.VerifyAll();
        }
    }
}