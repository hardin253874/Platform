// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class SecurityBypassContextTests
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_Nested(bool bypassSecurity)
        {
            Assert.That(SecurityBypassContext.IsActive, Is.False, "Context set before test started");

            using (new SecurityBypassContext(bypassSecurity))
            {
                Assert.That(SecurityBypassContext.IsActive, Is.EqualTo(bypassSecurity), "Context not set after creating first context");
                using (new SecurityBypassContext(bypassSecurity))
                {
                    Assert.That(SecurityBypassContext.IsActive, Is.EqualTo(bypassSecurity), "Context not set after creating second context");
                }
                Assert.That(SecurityBypassContext.IsActive, Is.EqualTo(bypassSecurity), "Context not set after leaving second context");
            }

            Assert.That(SecurityBypassContext.IsActive, Is.False, "Context set after test completed");
        }

        [Test]
        public void Test_Elevate()
        {
            bool elevated;
            elevated = false;
            SecurityBypassContext.Elevate(() => elevated = SecurityBypassContext.IsActive);
            Assert.That(elevated, Is.True, "Not elevated");
        }

        [Test]
        public void Test_Elevate_NullAction()
        {
            Assert.That(() => SecurityBypassContext.Elevate(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("action"));
        }


        [Test]
        public void Test_RunAsUser()
        {
            bool elevated;
            elevated = false;
            SecurityBypassContext.RunAsUser(() => elevated = SecurityBypassContext.IsActive);
            Assert.That(elevated, Is.False, "Elevated");
        }

        [Test]
        public void Test_RunAsUser_NullAction()
        {
            Assert.That(() => SecurityBypassContext.RunAsUser(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("action"));
        }

        [Test]
        public void Test_NoBypassContext()
        {
            bool elevated;
            elevated = false;
            SecurityBypassContext.Elevate(() => elevated = SecurityBypassContext.IsActive);
            Assert.That(elevated, Is.True, "Elevate is not elevated");
            SecurityBypassContext.RunAsUser(() => elevated = SecurityBypassContext.IsActive);
            Assert.That(elevated, Is.False, "RunAsUser is elevated");
        }

        [Test]
        public void Test_OneBypassContext()
        {
            bool elevated;
            elevated = false;
            using (new SecurityBypassContext())
            {
                Assert.That(SecurityBypassContext.IsActive, Is.True, "Not active");

                SecurityBypassContext.Elevate(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.True, "Elevate is not elevated");

                SecurityBypassContext.RunAsUser(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.False, "RunAsUser is elevated");
            }
        }

        [Test]
        public void Test_TwoBypassContexts()
        {
            bool elevated;
            elevated = false;
            using (new SecurityBypassContext())
            {
                using (new SecurityBypassContext())
                {
                    SecurityBypassContext.Elevate(() => elevated = SecurityBypassContext.IsActive);
                    Assert.That(elevated, Is.True, "Elevate is not elevated");

                    SecurityBypassContext.RunAsUser(() => elevated = SecurityBypassContext.IsActive);
                    Assert.That(elevated, Is.False, "RunAsUser is elevated");

                    Assert.That(SecurityBypassContext.IsActive, "Inner not active");
                }

                Assert.That(SecurityBypassContext.IsActive, "Outer not active");
            }
        }

        [Test]
        public void Test_NotBypassing()
        {
            bool elevated;

            elevated = false;
            using (new SecurityBypassContext(false))
            {
                SecurityBypassContext.Elevate(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.True, "Elevate is not elevated");

                SecurityBypassContext.RunAsUser(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.False, "RunAsUser is elevated");

                Assert.That(SecurityBypassContext.IsActive, Is.False, "Active");
            }
        }

        [Test]
        public void Test_NotBypassingInsideBypass()
        {
            bool elevated;

            elevated = false;
            using (new SecurityBypassContext())
            using (new SecurityBypassContext(false))
            {
                SecurityBypassContext.Elevate(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.True, "Elevate is not elevated");

                SecurityBypassContext.RunAsUser(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.False, "RunAsUser is elevated");

                Assert.That(SecurityBypassContext.IsActive, Is.False, "Active");
            }
        }

        [Test]
        public void Test_BypassingInsideNotBypassing()
        {
            bool elevated;

            elevated = false;
            using (new SecurityBypassContext(false))
            using (new SecurityBypassContext())
            {
                SecurityBypassContext.Elevate(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.True, "Elevate is not elevated");

                SecurityBypassContext.RunAsUser(() => elevated = SecurityBypassContext.IsActive);
                Assert.That(elevated, Is.False, "RunAsUser is elevated");

                Assert.That(SecurityBypassContext.IsActive, Is.True, "Active");
            }
        }
    }
}
