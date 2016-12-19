// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.ReadiNow.CAST.Test
{
    [TestFixture]
    [Category( "ExtendedTests" )]
    public class TenantServiceTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void TestGetTenant()
        {
            // Arrange
            var ts = new TenantService();

            // Act
            var tenant = ts.GetTenant(RunAsDefaultTenant.DefaultTenantName);

            // Assert
            tenant.Should().NotBeNull();
            tenant.Disabled.Should().BeFalse();
            tenant.RemoteId.Should().BeGreaterThan(0);
            tenant.Apps.Should().NotBeNullOrEmpty().And.NotContainNulls();
            tenant.Roles.Should().NotBeNullOrEmpty().And.NotContainNulls();
            tenant.Users.Should().NotBeNullOrEmpty().And.NotContainNulls();
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestGetTenants()
        {
            // Arrange
            var ts = new TenantService();

            // Act
            var tenants = ts.GetTenants();

            // Assert
            tenants.Should().NotBeNullOrEmpty().And.NotContainNulls();
            tenants.Select(t => t.Name).Should().Contain(RunAsDefaultTenant.DefaultTenantName);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestCreate()
        {
            const string test = "foo";

            try
            {
                // Arrange
                var ts = new TenantService();

                using (new GlobalAdministratorContext())
                {
                    TenantHelper.GetTenantId(test).Should().BeLessThan(0);
                }

                // Act
                ts.Create(test);

                // Assert
                using (new GlobalAdministratorContext())
                {
                    TenantHelper.GetTenantId(test).Should().BeGreaterThan(0);
                }

                Action a1 = () => ts.Create(null);
                a1.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: tenantName");

                Action a2 = () => ts.Create(string.Empty);
                a2.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: tenantName");
            }
            finally
            {
                using (new GlobalAdministratorContext())
                {
                    var testId = TenantHelper.GetTenantId(test);
                    if (testId > 0)
                    {
                        TenantHelper.DeleteTenant(testId);
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestDelete()
        {
            const string test = "foo";

            try
            {
                // Arrange
                var ts = new TenantService();

                using (new GlobalAdministratorContext())
                {
                    var id = TenantHelper.CreateTenant(test);
                    id.Should().BeGreaterThan(0);
                }

                // Act
                ts.Delete(test);

                // Assert
                using (new GlobalAdministratorContext())
                {
                    TenantHelper.GetTenantId(test).Should().BeLessThan(0);
                }

                Action a1 = () => ts.Delete(null);
                a1.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: tenantName");

                Action a2 = () => ts.Delete(string.Empty);
                a2.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: tenantName");
            }
            finally
            {
                using (new GlobalAdministratorContext())
                {
                    var testId = TenantHelper.GetTenantId(test);
                    if (testId > 0)
                    {
                        TenantHelper.DeleteTenant(testId);
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestDisable()
        {
            const string test = "foo";

            try
            {
                // Arrange
                var ts = new TenantService();

                using (new GlobalAdministratorContext())
                {
                    var id = TenantHelper.CreateTenant(test);
                    id.Should().BeGreaterThan(0);
                }

                // Act
                ts.Disable(test);

                // Assert
                using (new GlobalAdministratorContext())
                {
                    var tid = TenantHelper.GetTenantId(test);
                    tid.Should().BeGreaterThan(0);

                    var result = Entity.Get<Tenant>(tid);
                    result.Should().NotBeNull();
                    result.IsTenantDisabled.Should().BeTrue();
                }

                Action a1 = () => ts.Disable(null);
                a1.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                Action a2 = () => ts.Disable(string.Empty);
                a2.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                var notATenant = Guid.NewGuid().ToString();
                Action a3 = () => ts.Disable(notATenant);
                a3.ShouldThrow<Exception>().WithMessage("Tenant " + notATenant + " not found.");
            }
            finally
            {
                using (new GlobalAdministratorContext())
                {
                    var testId = TenantHelper.GetTenantId(test);
                    if (testId > 0)
                    {
                        TenantHelper.DeleteTenant(testId);
                    }
                }
            }
        }
        
        [Test]
        [RunAsDefaultTenant]
        public void TestEnable()
        {
            const string test = "foo";

            try
            {
                // Arrange
                var ts = new TenantService();

                using (new GlobalAdministratorContext())
                {
                    var id = TenantHelper.CreateTenant(test);
                    id.Should().BeGreaterThan(0);

                    var tenant = Entity.Get<Tenant>(id).AsWritable<Tenant>();
                    tenant.IsTenantDisabled = true;
                    tenant.Save();
                }

                // Act
                ts.Enable(test);

                // Assert
                using (new GlobalAdministratorContext())
                {
                    var tid = TenantHelper.GetTenantId(test);
                    tid.Should().BeGreaterThan(0);

                    var result = Entity.Get<Tenant>(tid);
                    result.Should().NotBeNull();
                    result.IsTenantDisabled.Should().BeFalse();
                }

                Action a1 = () => ts.Enable(null);
                a1.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                Action a2 = () => ts.Enable(string.Empty);
                a2.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                var notATenant = Guid.NewGuid().ToString();
                Action a3 = () => ts.Enable(notATenant);
                a3.ShouldThrow<Exception>().WithMessage("Tenant " + notATenant + " not found.");
            }
            finally
            {
                using (new GlobalAdministratorContext())
                {
                    var testId = TenantHelper.GetTenantId(test);
                    if (testId > 0)
                    {
                        TenantHelper.DeleteTenant(testId);
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRename()
        {
            const string test = "foo";
            const string name = "new";

            try
            {
                // Arrange
                var ts = new TenantService();

                long id;
                using (new GlobalAdministratorContext())
                {
                    id = TenantHelper.CreateTenant(test);
                }

                id.Should().BeGreaterThan(0);

                // Act
                ts.Rename(id, name);

                // Assert
                long nid;
                using (new GlobalAdministratorContext())
                {
                    var tid = TenantHelper.GetTenantId(test);
                    tid.Should().BeLessThan(0);

                    nid = TenantHelper.GetTenantId(name);
                    nid.Should().BeGreaterThan(0);
                }
                
                Action a1 = () => ts.Rename(-1, null);
                a1.ShouldThrow<ArgumentException>().WithMessage("New tenant name may not be null or empty.\r\nParameter name: name");

                Action a2 = () => ts.Rename(-1, string.Empty);
                a2.ShouldThrow<ArgumentException>().WithMessage("New tenant name may not be null or empty.\r\nParameter name: name");

                Action a3 = () => ts.Rename(-1, name);
                a3.ShouldThrow<Exception>().WithMessage("Tenant not found.");

                Action a4 = () => ts.Rename(nid, name); // same
                a4.ShouldNotThrow();
            }
            finally
            {
                using (new GlobalAdministratorContext())
                {
                    var testId = TenantHelper.GetTenantId(test);
                    if (testId > 0)
                    {
                        TenantHelper.DeleteTenant(testId);
                    }

                    var newId = TenantHelper.GetTenantId(name);
                    if (newId > 0)
                    {
                        TenantHelper.DeleteTenant(newId);
                    }
                }
            }
        }
    }
}
