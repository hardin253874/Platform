// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security
{
    [TestFixture]
	[RunWithTransaction]
    public class TestSetUser
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Using()
        {
            UserAccount userAccount;
            RequestContext oldRequestContext;
            RequestContext newRequestContext;

            userAccount = new UserAccount();
            userAccount.Name = "foo" + Guid.NewGuid().ToString();
            userAccount.Save();

            oldRequestContext = RequestContext.GetContext();

            using (SetUser setUser = new SetUser(userAccount))
            {
                Assert.That(setUser, Has.Property("UserAccount").SameAs(userAccount));
                Assert.That(setUser, Has.Property("OldContext").Not.Null);

                Assert.That(RequestContext.GetContext(), 
                    Has.Property("Tenant").EqualTo(oldRequestContext.Tenant));
                Assert.That(RequestContext.GetContext(), 
                    Has.Property("Culture").EqualTo(oldRequestContext.Culture));

                newRequestContext = RequestContext.GetContext();
            }

            // Cannot compare to old context since the context may have changed in the 
            // interim - it gets set with every access control check.
            Assert.That(oldRequestContext, Is.Not.SameAs(newRequestContext),
                "Context not restored");

            // Do checks here so userAccount does not need security queries
            Assert.That(newRequestContext.Identity, Has.Property("Name").EqualTo(userAccount.Name));
            Assert.That(newRequestContext.Identity, Has.Property("Id").EqualTo(userAccount.Id));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Ctor_Null()
        {
            Assert.That(() => new SetUser(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("userAccount"));
        }

        [Test]
        public void Test_Ctor_NoRequestContext()
        {
            Assert.That(() => new SetUser(new UserAccount()),
                Throws.InvalidOperationException);
        }
    }
}
