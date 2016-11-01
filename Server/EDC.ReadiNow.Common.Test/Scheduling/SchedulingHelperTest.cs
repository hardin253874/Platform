// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using NUnit.Framework;
using Quartz;

namespace EDC.ReadiNow.Test.Scheduling
{
    [TestFixture]
    public class SchedulingHelperTest
    {

        [Test]
        [RunAsDefaultTenant]
        public void SameDefaultPassiveInstance()
        {
            var first = SchedulingHelper.Instance;
            var second = SchedulingHelper.Instance;

            Assert.That(first, Is.EqualTo(second));

        }


        [Test]
        [RunAsDefaultTenant]
        public void DefaultPassiveInstanceIsInStandby()
        {
            Assert.That(SchedulingHelper.Instance.InStandbyMode, Is.True);
        }

        [Test]
        [RunAsDefaultTenant]
        [Description("We need this behavior because the factory always returns the same instance.")]
        public void InstanceDealingWithShutdown ()
        {
            var first = SchedulingHelper.Instance;
            first.Shutdown();
            var second = SchedulingHelper.Instance;

            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.InStandbyMode, Is.True);

        }


        [Test]
        [RunAsDefaultTenant]
        public void PauseAndRestart()
        {
            bool ran = false;
            Action act = () => ran = true;

            SchedulingHelper.PauseAndRestartTenantJobs(RequestContext.TenantId, act);

            Assert.That(ran, Is.True);
        }
        
    }
}
