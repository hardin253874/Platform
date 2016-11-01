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
    public class AuditLogRoleEventTargetTests
    {
        /// <summary>
        ///     Tests changing the role members writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeUserRoleMembersIncludedByRolesChanged()
        {
            bool success = true;
            string roleName = "Role" + Guid.NewGuid();

            string role1Name = "Role1" + Guid.NewGuid();
            string role2Name = "Role2" + Guid.NewGuid();

            ISet<string> roleMembers = new SortedSet<string> {roleName};

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
			mockAuditLog.Setup( al => al.OnChangeUserRoleMembers( success, role2Name, It.Is<ISet<string>>( m => m.SetEquals( roleMembers ) ), It.Is<ISet<string>>( m => m.Count <= 0 ) ) );

			mockAuditLog.Setup( al => al.OnChangeUserRoleMembers( success, role1Name, It.Is<ISet<string>>( m => m.Count <= 0 ), It.Is<ISet<string>>( m => m.SetEquals( roleMembers ) ) ) );

            var eventTarget = new AuditLogRoleEventTarget(mockAuditLog.Object);

            var role1 = new Role {Name = role1Name};
            var role2 = new Role {Name = role2Name};

            var role = new Role {Name = roleName};
            role.IncludedByRoles.Add(role1);
            role.Save();

            role.IncludedByRoles.Add(role2);
            role.IncludedByRoles.Remove(role1);

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(role, state);
            eventTarget.WriteSaveAuditLogEntries(success, role.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests changing the role members writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangeUserRoleMembersIncludedRolesAndUsersChanged()
        {
            bool success = true;
            string roleName = "Role" + Guid.NewGuid();
            string user1Name = "User1" + Guid.NewGuid();
            string user2Name = "User2" + Guid.NewGuid();

            string role1Name = "Role1" + Guid.NewGuid();
            string role2Name = "Role2" + Guid.NewGuid();

            ISet<string> addedMembers = new SortedSet<string> {user2Name, role2Name};
            ISet<string> removedMembers = new SortedSet<string> {user1Name, role1Name};

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnChangeUserRoleMembers(success, roleName, It.Is<ISet<string>>(m => m.SetEquals(addedMembers)), It.Is<ISet<string>>(m => m.SetEquals(removedMembers))));

            var eventTarget = new AuditLogRoleEventTarget(mockAuditLog.Object);

            var user1 = new UserAccount {Name = user1Name};
            var user2 = new UserAccount {Name = user2Name};
            var role1 = new Role {Name = role1Name};
            var role2 = new Role {Name = role2Name};

            var role = new Role {Name = roleName};
            role.RoleMembers.Add(user1);
            role.IncludesRoles.Add(role1);
            role.Save();

            role.RoleMembers.Add(user2);
            role.RoleMembers.Remove(user1);

            role.IncludesRoles.Add(role2);
            role.IncludesRoles.Remove(role1);

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(role, state);
            eventTarget.WriteSaveAuditLogEntries(success, role.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests creating a role writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnCreateUserRole()
        {
            bool success = true;
            string roleName = "Role" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnCreateUserRole(success, roleName));

            var eventTarget = new AuditLogRoleEventTarget(mockAuditLog.Object);

            var role = new Role {Name = roleName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(role, state);
            eventTarget.WriteSaveAuditLogEntries(success, role.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests deleting a role writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnDeleteUserRole()
        {
            bool success = true;
            string roleName = "Role" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnDeleteUserRole(success, roleName));

            var eventTarget = new AuditLogRoleEventTarget(mockAuditLog.Object);

            var role = new Role {Name = roleName};

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForDelete(role, state);
            eventTarget.WriteDeleteAuditLogEntries(success, role.Id, state);

            mockAuditLog.VerifyAll();
        }


        /// <summary>
        ///     Tests renaming a role writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnRenameUserRole()
        {
            bool success = true;
            string roleName = "Role" + Guid.NewGuid();
            string newRoleName = "Role" + Guid.NewGuid();

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnRenameUserRole(success, roleName, newRoleName));

            var eventTarget = new AuditLogRoleEventTarget(mockAuditLog.Object);

            var role = new Role {Name = roleName};
            role.Save();

            role.Name = newRoleName;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(role, state);
            eventTarget.WriteSaveAuditLogEntries(success, role.Id, state);

            mockAuditLog.VerifyAll();
        }
    }
}