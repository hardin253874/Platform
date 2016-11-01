// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.CAST.Marketplace.Services;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.Test;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.ReadiNow.CAST.Test
{
    [Ignore]
    [TestFixture]
    public class MarketplaceServiceTests
    {
        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestGetCustomer()
        {
            var ms = new MarketplaceService();

            Action f1 = () => ms.GetCustomer(null);
            f1.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: email");

            Action f2 = () => ms.GetCustomer("invalidemail.address");
            f2.ShouldThrow<ArgumentException>().WithMessage("The email address is not correctly structured.");

            var c = ms.GetCustomer("a@e.con");
            c.Should().BeNull();

            var mc = new ManagedCustomer
            {
                Name = "Alan Elbow",
                FirstName = "Alan",
                LastName = "Elbow",
                Email = "a@e.con"
            };
            mc.Save();

            var r = ms.GetCustomer("a@e.con");
            r.Should().NotBeNull();
            r.Name.Should().Be(mc.Name);
            r.FirstName.Should().Be(mc.FirstName);
            r.LastName.Should().Be(mc.LastName);
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestCreateCustomer()
        {
            var ms = new MarketplaceService();

            Action f1 = () => ms.CreateCustomer(null, null, null);
            f1.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: email");

            Action f2 = () => ms.CreateCustomer("foo@bar.com", null, null);
            f2.ShouldNotThrow();

            var mc = ms.CreateCustomer("foo@bar.com", null, null);
            mc.Should().NotBeNull();
            mc.Name.Should().Be("foo@bar.com");
            mc.IsTemporaryId.Should().BeTrue();

            mc = ms.CreateCustomer("foo@bar.com", "Foo", null);
            mc.Should().NotBeNull();
            mc.Name.Should().Be("foo@bar.com");
            mc.IsTemporaryId.Should().BeTrue();

            mc = ms.CreateCustomer("foo@bar.com", null, "Bar");
            mc.Should().NotBeNull();
            mc.Name.Should().Be("Bar");
            mc.IsTemporaryId.Should().BeTrue();

            mc = ms.CreateCustomer("foo@bar.com", "Foo", "Bar");
            mc.Should().NotBeNull();
            mc.Name.Should().Be("Foo Bar");
            mc.IsTemporaryId.Should().BeTrue();

            Action a = () => mc.Save();
            a.ShouldNotThrow();

            mc.IsTemporaryId.Should().BeFalse();
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestGetPlatform()
        {
            // Arrange
            var ms = new MarketplaceService();

            // there should be no platforms in the system, normally
            var t = DateTime.Now;
            var mp = new ManagedPlatform
            {
                Name = "MyServer",
                DatabaseId = "myserver.foo.con",
                LastContact = t
            };

            mp.Save();

            // Act
            Action a = () => ms.GetPlatform(null);
            a.ShouldNotThrow();

            // Should pull the most recently heard from platform... for now
            var p = ms.GetPlatform(null);

            // Assert
            p.Should().NotBeNull();
            p.Name.Should().Be(mp.Name);
            p.DatabaseId.Should().Be(mp.DatabaseId);
            p.LastContact.Should().Be(t);
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestCreateTenant()
        {
            // Arrange
            var ms = new MarketplaceService();
            var c = new ManagedCustomer
            {
                Email = "foo@bar.com",
                FirstName = "Foo",
                LastName = "Bar"
            };

            c.Save();

            Action f1 = () => ms.CreateTenant(null);
            f1.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: customer");

            // Act
            var t = ms.CreateTenant(c);

            // Assert
            t.Should().NotBeNull();
            t.Customer.Should().NotBeNull();
            t.Customer.Id.Should().Be(c.Id);
            t.IsTemporaryId.Should().BeTrue();
            t.Name.Should().Be("FOOBARCOM");

            // The means for determining the tenant name... not 100% solid yet.
            c.Name = "Foo Bar";
            t = ms.CreateTenant(c);
            t.Name.Should().Be("FOOBAR");

            c.Company = "Elbow Inc.";
            t = ms.CreateTenant(c);
            t.Name.Should().Be("ELBOWINC");

            // If the tenant name already exists?... not 100% solid yet.
            t.Save();
            t = ms.CreateTenant(c);
            t.Name.Should().NotBe("ELBOWINC");
            t.Name.Should().StartWith("ELBOWINC_");
        }
    }
}
