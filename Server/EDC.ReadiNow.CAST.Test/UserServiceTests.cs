// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.ReadiNow.CAST.Test
{
    [TestFixture]
    public class UserServiceTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void TestGetRoles()
        {
            var toDelete = new List<long>();

            try
            {
                // Arrange
                var userService = new UserService();

                // Act
                var roles = userService.GetRoles(RunAsDefaultTenant.DefaultTenantName);

                // Assert
                roles.Should().NotBeNullOrEmpty().And.NotContainNulls();

                var n = roles.Count;

                var roleName = Guid.NewGuid().ToString();
                var role = Entity.Create<Role>();

                role.Name = roleName;
                role.Save();

                var roleId = role.Id;

                toDelete.Add(roleId);

                roles = userService.GetRoles(RunAsDefaultTenant.DefaultTenantName);
                roles.Count.Should().Be(n + 1);
                roles.Select(r => r.Name).Should().Contain(roleName);

                role.Delete();

                toDelete.Remove(roleId);

                roles = userService.GetRoles(RunAsDefaultTenant.DefaultTenantName);
                roles.Count.Should().Be(n);
                roles.Select(r => r.Name).Should().NotContain(roleName);

                Action a1 = () => userService.GetRoles(null);
                a1.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                var notTenantName = Guid.NewGuid().ToString();
                Action a2 = () => userService.GetRoles(notTenantName);
                a2.ShouldThrow<EntityNotFoundException>().WithMessage("Unable to locate Tenant with name '" + notTenantName + "'.");
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestGetUser()
        {
            var toDelete = new List<long>();

            try
            {
                // Arrange
                var userService = new UserService();

                var userName = Guid.NewGuid().ToString();
                var user = Entity.Create<UserAccount>();

                user.Name = userName;
                user.Password = "Abc123!";
                user.Save();

                var userId = user.Id;

                toDelete.Add(userId);

                // Act
                var userInfo = userService.GetUser(userName, RunAsDefaultTenant.DefaultTenantName);

                // Assert
                userInfo.Should().NotBeNull();
                userInfo.Name.Should().Be(userName);
                userInfo.Status.Should().Be(UserStatus.Unknown);
                userInfo.Roles.Should().NotBeNull().And.BeEmpty();

                user.Delete();

                toDelete.Remove(userId);

                userInfo = userService.GetUser(userName, RunAsDefaultTenant.DefaultTenantName);
                userInfo.Should().BeNull();

                Action a1 = () => userService.GetUser(null, null);
                a1.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                var notTenantName = Guid.NewGuid().ToString();
                Action a2 = () => userService.GetUser(null, notTenantName);
                a2.ShouldThrow<EntityNotFoundException>().WithMessage("Unable to locate Tenant with name '" + notTenantName + "'.");
                
                Action a3 = () => userService.GetUser(null, RunAsDefaultTenant.DefaultTenantName);
                a3.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: name");
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestGetUsers()
        {
            var toDelete = new List<long>();

            try
            {
                // Arrange
                var userService = new UserService();

                // Act
                var users = userService.GetUsers(RunAsDefaultTenant.DefaultTenantName);

                // Assert
                users.Should().NotBeNullOrEmpty().And.NotContainNulls();

                var n = users.Count;

                var userName = Guid.NewGuid().ToString();
                var user = Entity.Create<UserAccount>();

                user.Name = userName;
                user.Password = "Abc123!";
                user.Save();

                var userId = user.Id;

                toDelete.Add(userId);

                users = userService.GetUsers(RunAsDefaultTenant.DefaultTenantName);
                users.Count.Should().Be(n + 1);
                users.Select(u => u.RemoteId).Should().Contain(userId);

                var userInfo = users.FirstOrDefault(u => u.RemoteId == userId);
                userInfo.Should().NotBeNull();
                userInfo.Name.Should().Be(userName);
                userInfo.Status.Should().Be(UserStatus.Unknown);
                userInfo.Roles.Should().NotBeNull().And.BeEmpty();

                user.Delete();

                toDelete.Remove(userId);

                users = userService.GetUsers(RunAsDefaultTenant.DefaultTenantName);
                users.Count.Should().Be(n);
                users.Select(u => u.RemoteId).Should().NotContain(userId);

                Action a1 = () => userService.GetUsers(null);
                a1.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                var notTenantName = Guid.NewGuid().ToString();
                Action a2 = () => userService.GetUsers(notTenantName);
                a2.ShouldThrow<EntityNotFoundException>().WithMessage("Unable to locate Tenant with name '" + notTenantName + "'.");
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestCreate()
        {
            var toDelete = new List<long>();

            try
            {
                // Arrange
                var userService = new UserService();
                var userName = Guid.NewGuid().ToString();
                var userPassword = "Abc123!";

                // Act
                userService.Create(userName, userPassword, RunAsDefaultTenant.DefaultTenantName, null);

                // Assert
                var user = Entity.GetByName<UserAccount>(userName).FirstOrDefault();
                user.Should().NotBeNull();

                var userId = user.Id;

                toDelete.Add(userId);

                user.Password.Should().NotBeNullOrEmpty().And.NotBe(userPassword); // encrypted
                user.AccountStatus_Enum.Should().Be(UserAccountStatusEnum_Enumeration.Active);
                user.UserHasRole.Should().NotBeNull().And.BeEmpty();

                // create again
                Action a = () => userService.Create(userName, userPassword, RunAsDefaultTenant.DefaultTenantName, null);
                a.ShouldNotThrow();

                Entity.GetByName<UserAccount>(userName).Count().Should().Be(1);

                user.AsWritable<UserAccount>().Delete();

                toDelete.Remove(userId);

                // create with Administrators role
                userService.Create(userName, userPassword, RunAsDefaultTenant.DefaultTenantName, new List<string> { "Administrators" });
                user = Entity.GetByName<UserAccount>(userName).FirstOrDefault();
                user.Should().NotBeNull();
                user.Id.Should().NotBe(userId);

                userId = user.Id;

                toDelete.Add(userId);
                
                user.UserHasRole.Should().NotBeNullOrEmpty();
                user.UserHasRole.Count.Should().Be(1);
                user.UserHasRole.Select(r => r.Name).Should().Contain("Administrators");

                user.AsWritable<UserAccount>().Delete();

                toDelete.Remove(userId);
                
                Action a1 = () => userService.Create(null, null, null, null);
                a1.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                Action a2 = () => userService.Create(null, null, RunAsDefaultTenant.DefaultTenantName, null);
                a2.ShouldThrow<ArgumentException>().WithMessage("Invalid argument.\r\nParameter name: name");

                Action a3 = () => userService.Create(userName, null, RunAsDefaultTenant.DefaultTenantName, null);
                a3.ShouldThrow<ValidationException>().WithMessage("A password is required.");

                var notARole = Guid.NewGuid().ToString();
                Action a4 = () => userService.Create(userName, userPassword, RunAsDefaultTenant.DefaultTenantName, new List<string> { notARole });
                a4.ShouldThrow<Exception>().WithMessage("The expected user does not exist.");

                Action a5 = () => userService.Create(userName, userPassword, RunAsDefaultTenant.DefaultTenantName, new List<string> { "Administrators", notARole });
                a5.ShouldNotThrow();

                toDelete.AddRange(Entity.GetByName<UserAccount>(userName).Select(u => u.Id));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestDelete()
        {
            var toDelete = new List<long>();

            try
            {
                // Arrange
                var userService = new UserService();

                var userName = Guid.NewGuid().ToString();
                var user = Entity.Create<UserAccount>();

                user.Name = userName;
                user.Password = "Abc123!";
                user.Save();

                var userId = user.Id;

                toDelete.Add(userId);

                // Act
                userService.Delete(userName, RunAsDefaultTenant.DefaultTenantName);

                // Assert
                user = Entity.Get<UserAccount>(userId);
                user.Should().BeNull();

                toDelete.Remove(userId);

                // double delete
                Action a1 = () => userService.Delete(userName, RunAsDefaultTenant.DefaultTenantName);
                a1.ShouldNotThrow();

                Action a2 = () => userService.Delete(userName, null);
                a2.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                Action a3 = () => userService.Delete(null, RunAsDefaultTenant.DefaultTenantName);
                a3.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: user");
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }
    }
}
