// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.Diagnostics;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Diagnostics.ActivityLog
{
    [TestFixture]
    [FailOnEvent]
    public class RateLimitedPurgerTests
    {
        class DummyPurger : IActivityLogPurger
        {
            public Action action;

            public DummyPurger(Action act = null)
            {
                action = act;
            }

            public void Purge()
            {
                if (action != null)
                    action();
            }
        }

        
        [Test]
        public void DefaultMaxPurgeRate()
        {
            var purger = new RateLimitedPurger(new DummyPurger(() => { }));

            Assert.That(purger.MaxPurgeRate, Is.EqualTo(120*1000));
        }


        [Test]
        public void RateOfZeroAlwaysPurges()
        {
            int purgeCount = 0;
            var purger = new RateLimitedPurger(new DummyPurger(() => { purgeCount++; }), 0);

            purger.Purge();
            purger.Purge();

            Assert.That(purgeCount, Is.EqualTo(2));
        }

        [Test]
        public void AlwaysPurgesFirstTime()
        {
            int purgeCount = 0;
            var purger = new RateLimitedPurger(new DummyPurger(() => { purgeCount++; }), 1000);

            purger.Purge();

            Assert.That(purgeCount, Is.EqualTo(1));
        }


        [Test]
        public void WaitsToPurge()
        {
            int purgeCount = 0;
            var purger = new RateLimitedPurger(new DummyPurger(() => { purgeCount++; }), 1000);

            purger.Purge(); 
            Assert.That(purgeCount, Is.EqualTo(1));

            purger.Purge();
            Assert.That(purgeCount, Is.EqualTo(1));

            Thread.Sleep(1500);

            purger.Purge();
            Assert.That(purgeCount, Is.EqualTo(2));
        }


        [Test]
        public void WillPurgeTwice()
        {
            int purgeCount = 0;
            var purger = new RateLimitedPurger(new DummyPurger(() => { purgeCount++; }), 1000);

            purger.Purge();
            Assert.That(purgeCount, Is.EqualTo(1));

            Thread.Sleep(1500);

            purger.Purge();
            Assert.That(purgeCount, Is.EqualTo(2));

            Thread.Sleep(1500);

            purger.Purge();
            Assert.That(purgeCount, Is.EqualTo(3));
        }


    }
}
