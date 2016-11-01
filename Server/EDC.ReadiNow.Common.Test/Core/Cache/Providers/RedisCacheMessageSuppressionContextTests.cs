// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core.Cache.Providers;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    [TestFixture]
    public class RedisCacheMessageSuppressionContextTests
    {
        [Test]
        public void Nesting_SameCache()
        {
            const string cacheName = "foo";

            Assert.That(RedisCacheMessageSuppressionContext.IsSet(cacheName), Is.False, "Set beforehand");

            using (RedisCacheMessageSuppressionContext outerContext = new RedisCacheMessageSuppressionContext(cacheName))
            {
                Assert.That(outerContext, Has.Property("CacheName").EqualTo(cacheName));
                Assert.That(RedisCacheMessageSuppressionContext.IsSet(cacheName), Is.True, "Not set before inner");

                using (RedisCacheMessageSuppressionContext innerContext = new RedisCacheMessageSuppressionContext(cacheName))
                {
                    Assert.That(innerContext, Has.Property("CacheName").EqualTo(cacheName));
                    Assert.That(RedisCacheMessageSuppressionContext.IsSet(cacheName), Is.True, "Not set inner");
                }

                Assert.That(RedisCacheMessageSuppressionContext.IsSet(cacheName), Is.True, "Not set after inner");
            }

            Assert.That(RedisCacheMessageSuppressionContext.IsSet(cacheName), Is.False, "Set afterwards");
        }

        [Test]
        public void Nesting_DifferentCache()
        {
            const string outerCacheName = "foo";
            const string innerCacheName = "bar";

            Assert.That(RedisCacheMessageSuppressionContext.IsSet(outerCacheName), Is.False, "Outer set beforehand");
            Assert.That(RedisCacheMessageSuppressionContext.IsSet(innerCacheName), Is.False, "Inner set beforehand");

            using (RedisCacheMessageSuppressionContext outerContext = new RedisCacheMessageSuppressionContext(outerCacheName))
            {
                Assert.That(outerContext, Has.Property("CacheName").EqualTo(outerCacheName));
                Assert.That(RedisCacheMessageSuppressionContext.IsSet(outerCacheName), Is.True, "Outer not set before inner");
                Assert.That(RedisCacheMessageSuppressionContext.IsSet(innerCacheName), Is.False, "Inner set before inner");

                using (RedisCacheMessageSuppressionContext innerContext = new RedisCacheMessageSuppressionContext(innerCacheName))
                {
                    Assert.That(innerContext, Has.Property("CacheName").EqualTo(innerCacheName));
                    Assert.That(RedisCacheMessageSuppressionContext.IsSet(innerCacheName), Is.True, "Inner not set in inner");
                    Assert.That(RedisCacheMessageSuppressionContext.IsSet(outerCacheName), Is.True, "Outer not set in inner");
                }

                Assert.That(RedisCacheMessageSuppressionContext.IsSet(outerCacheName), Is.True, "Outer not set after inner");
                Assert.That(RedisCacheMessageSuppressionContext.IsSet(innerCacheName), Is.False, "Inner set after inner");
            }

            Assert.That(RedisCacheMessageSuppressionContext.IsSet(outerCacheName), Is.False, "Outer set afterwards");
            Assert.That(RedisCacheMessageSuppressionContext.IsSet(innerCacheName), Is.False, "Inner set afterwards"); 
        }
    }
}
