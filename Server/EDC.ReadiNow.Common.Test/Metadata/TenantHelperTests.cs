// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Test.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class TenantHelperTests
    {
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void InvalidateTenant( )
        {
            TenantHelper.Invalidate( );
        }


        [Test]
        [RunAsGlobalTenant]
        
        public void AddTenant()
        {
            long tenantId = 0;

            try
            {
                tenantId = TenantHelper.CreateTenant("TenantHelperTests_AddTenant");
            }
            finally
            {
                if (tenantId != 0)
                    TenantHelper.DeleteTenant(tenantId);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetDefaultTenantName()
        {            
            string tenantName = TenantHelper.GetTenantName(RequestContext.TenantId);
            long tenantId = TenantHelper.GetTenantId(tenantName);

            Assert.AreEqual(tenantId, RequestContext.TenantId);
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetGlobalTenantName()
        {
            string tenantName = TenantHelper.GetTenantName(0);            

            Assert.AreEqual(SpecialStrings.GlobalTenant, tenantName);
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetInvalidTenantName()
        {
            string tenantName = TenantHelper.GetTenantName(-1);

            Assert.AreEqual(string.Empty, tenantName);
        }
    }
}
