// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Test.Model
{
    /// <summary>
    /// Test WellKnownAliases class.
    /// </summary>
    [TestFixture]
    public class WellKnownAliasesTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_IsOfType()
        {
            long isOfType = WellKnownAliases.CurrentTenant.IsOfType;

            Assert.That( isOfType, Is.GreaterThan( 0 ) );

            IEntity entity = Entity.Get( isOfType );
            Assert.That( entity, Is.Not.Null );
            Assert.That( entity.Alias, Is.EqualTo("isOfType") );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Concurrent( )
        {        
            bool failed = false;

            TestHelpers.TestConcurrent(100, ( ) =>
                {
                    long isOfType = WellKnownAliases.CurrentTenant.IsOfType;
                    if (isOfType <= 0)
                        failed = true;
                });

            Assert.That( failed, Is.False );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_VariesPerContext( )
        {
            // Load in one tenant
            long isOfType = WellKnownAliases.CurrentTenant.IsOfType;
            Assert.That( isOfType, Is.GreaterThan( 0 ) );

            // Load in a different tenant
            long isOfType2;
            using (new TenantAdministratorContext(0))
            {
                isOfType2 = WellKnownAliases.CurrentTenant.IsOfType;
            }
            Assert.That( isOfType2, Is.Not.EqualTo(isOfType) );
        }
    }
}
