// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache.Providers;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class PerTenantCacheInvalidatorTests
    {
        [Test]
        public void Test_Activation( )
        {
            IPerTenantCacheInvalidator perTenantCacheInvalidator = Factory.Current.Resolve<IPerTenantCacheInvalidator>( );
            Assert.That( perTenantCacheInvalidator, Is.Not.Null );
        }

        [Test]
        public void Test_Invalidation( )
        {
            long tenantId = 123;
            Mock<IPerTenantCache> mock1 = new Mock<IPerTenantCache>( MockBehavior.Strict );
            Mock<IPerTenantCache> mock2 = new Mock<IPerTenantCache>( MockBehavior.Strict );

            mock1.Setup( c => c.InvalidateTenant( tenantId ) );
            mock2.Setup( c => c.InvalidateTenant( tenantId ) );

            IPerTenantCacheInvalidator perTenantCacheInvalidator = new PerTenantCacheInvalidator( );
            perTenantCacheInvalidator.RegisterCache( mock1.Object );
            perTenantCacheInvalidator.RegisterCache( mock2.Object );
            perTenantCacheInvalidator.InvalidateTenant( tenantId );

            mock1.VerifyAll( );
            mock2.VerifyAll( );
        }
    }
}
