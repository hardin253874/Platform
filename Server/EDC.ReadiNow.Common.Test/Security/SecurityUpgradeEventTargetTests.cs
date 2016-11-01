// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security
{
    [TestFixture]
    public class SecurityUpgradeEventTargetTests
    {
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent(new[] { EventLogLevel.Error, EventLogLevel.Warning })]
        [TestCase("core:adminFullAuthorization", false)]
        [TestCase("core:testUserAccount", false)]
        [TestCase("core:reportTestPageType", false)]
        [TestCase("core:abc", false)]
        public void OnAfterUpgrade(string alias, bool expectExists)
        {
            SecurityUpgradeEventTarget securityUpgradeEventTarget;

            securityUpgradeEventTarget = new SecurityUpgradeEventTarget();
            securityUpgradeEventTarget.OnAfterUpgrade(null, null);

            Assert.That(
                Entity.Exists(new EntityRef(alias)),
                Is.EqualTo(expectExists));
        }
    }
}
