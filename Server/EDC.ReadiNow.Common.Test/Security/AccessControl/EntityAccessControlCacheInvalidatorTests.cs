// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Monitoring;
using EDC.ReadiNow.Diagnostics.Tracing;
using EDC.ReadiNow.Monitoring;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class EntityAccessControlCacheInvalidatorTests
    {
        [Test]
        public void Test_Creation()
        {
            EntityAccessControlCacheInvalidator invalidator;

            invalidator = new EntityAccessControlCacheInvalidator();
            Assert.That(invalidator, Has.Property("Caches").Empty);
            Assert.That(invalidator, Has.Property("Trace").TypeOf<PlatformTrace>());
            Assert.That(invalidator, Has.Property("AccessControlCacheInvalidationCounters").InstanceOf<IMultiInstancePerformanceCounterCategory>());
        }

        [Test]
        public void Test_Creation_Caches()
        {
            Mock<ICacheService> mockCache;
            ICacheService cache;
            EntityAccessControlCacheInvalidator invalidator;

            mockCache = new Mock<ICacheService>(MockBehavior.Strict);
            cache = mockCache.Object;

            invalidator = new EntityAccessControlCacheInvalidator(cache);
            Assert.That(invalidator, Has.Property("Caches").Count.EqualTo(1));
            Assert.That(invalidator, Has.Property("Caches").Exactly(1).EqualTo(cache));

            mockCache.VerifyAll();
        }

        [Test]
        public void Test_Creation_Null()
        {
            Assert.That(() => new EntityAccessControlCacheInvalidator(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("caches"));
        }

        [Test]
        public void Test_Invalidate()
        {
            Mock<ICacheService> mockCache;
            ICacheService cache;
            EntityAccessControlCacheInvalidator invalidator;

            mockCache = new Mock<ICacheService>(MockBehavior.Strict);
            mockCache.Setup(c => c.Clear());
            cache = mockCache.Object;

            invalidator = new EntityAccessControlCacheInvalidator(cache);
            invalidator.Invalidate();

            mockCache.VerifyAll();
        }
    }
}
