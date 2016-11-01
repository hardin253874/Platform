// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using EDC.ReadiNow.IO;

namespace ReadiNow.TenantHealth.Test.Components
{
    /// <summary>
    /// Basic mock tests for interracting with a tenant.
    /// </summary>
    [TestFixture]
    public class TenantTests
    {
        /// <summary>
        /// Test impersonating the system administrator (no security policy applied) for each tenant.
        /// </summary>
        [Test]
        [TestCaseSource("GetTestData")]
        public void ImpersonateSystemAdmin( TenantInfo tenant )
        {
            using ( tenant.GetSystemAdminContext( ) )
            {
                Assert.That( RequestContext.GetContext( ).Tenant.Id, Is.EqualTo(tenant.TenantId) );
            }
        }


        /// <summary>
        /// Test impersonating the tenant administrator (select an arbitrary tenant administrator account) for each tenant.
        /// </summary>
        [Test]
        [TestCaseSource( "GetTestData" )]
        public void ImpersonateTenantAdmin( TenantInfo tenant )
        {
            using ( tenant.GetTenantAdminContext( ) )
            {
                Assert.That( RequestContext.GetContext( ).Tenant.Id, Is.EqualTo( tenant.TenantId ) );
            }
        }


        /// <summary>
        /// Test data source
        /// </summary>
        public IEnumerable<TestCaseData> GetTestData( )
        {
            return TenantHealthHelpers.GetTenants( ).Select(
                tenant => new TestCaseData( tenant )
                .SetCategory( tenant.TenantName )
                .SetCategory( "Tenants" )
                );
        }
    }
}
