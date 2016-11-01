// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.IO
{
    /// <summary>
    /// Test the request context
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class RequestContextTests
    {
        /// <summary>
        /// Tests the set actual user request context.
        /// </summary>
        [Test]
        public void TestSetActualUserRequestContext()
        {
            try
            {
                // Ensure the initial context is clear
                RequestContext.FreeContext();

                // Create a context.
                var identity1 = new IdentityInfo(1234, "TestUser1");
                var tenant1 = new TenantInfo(1111);
                RequestContext.SetContext(identity1, tenant1, "en-US");

                var context1 = RequestContext.GetContext();

                // This should contain the actual user context
                var actualUserContext1 = ActualUserRequestContext.GetContext();

                Assert.IsTrue(actualUserContext1.IsValid);
                Assert.AreSame(context1.Identity, actualUserContext1.Identity);
                Assert.AreSame(context1.Tenant, actualUserContext1.Tenant);

                // Create a sys admin context.
                var identity2 = new IdentityInfo(0, "Admin");
                var tenant2 = new TenantInfo(1111);
                RequestContext.SetContext(identity2, tenant2, "en-US");

                // This should contain the initial actual user context
                var actualUserContext2 = ActualUserRequestContext.GetContext();
                Assert.IsTrue(actualUserContext2.IsValid);
                Assert.AreSame(context1.Identity, actualUserContext2.Identity);
                Assert.AreSame(context1.Tenant, actualUserContext2.Tenant);

                // Create a new user context.
                var identity3 = new IdentityInfo(456, "TestUser1");
                var tenant3 = new TenantInfo(1111);
                RequestContext.SetContext(identity3, tenant3, "en-US");

                var context3 = RequestContext.GetContext();

                // Set and reset an admin context
                using (new AdministratorContext())
                {                    
                }

                // This should still contain the new actual user context
                var actualUserContext3 = ActualUserRequestContext.GetContext();
                Assert.IsTrue(actualUserContext3.IsValid);
                Assert.AreSame(context3.Identity, actualUserContext3.Identity);
                Assert.AreSame(context3.Tenant, actualUserContext3.Tenant);

                RequestContext.FreeContext();

                var actualUserContext4 = ActualUserRequestContext.GetContext();
                Assert.IsFalse(actualUserContext4.IsValid);
            }
            finally
            {
                RequestContext.FreeContext();
            }
        }
    }
}
