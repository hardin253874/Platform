// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
    public class CacheMonitorTests
    {
        [Test]
        public void Test_Ctor()
        {
            ICache<string, int> cache;
            CacheMonitor<string, int> cacheMonitor;
            
            cache = CacheFactory.CreateSimpleCache<string, int>("Test");
            cacheMonitor = new CacheMonitor<string, int>(cache);

            Assert.That(cacheMonitor, Has.Property("Cache").SameAs(cache));
            Assert.That(cacheMonitor, Has.Property("ItemsRemoved").Empty);
        }

        [Test]
        public void Test_Removal()
        {
            ICache<string, int> cache;
            const string testKey = "Test Key";
            const int testValue = 1234;

            cache = CacheFactory.CreateSimpleCache<string, int>( "Test" );
            using (CacheMonitor<string, int> cacheMonitor = new CacheMonitor<string, int>(cache))
            {
                Assert.That(cacheMonitor.ItemsRemoved, Is.Empty, "Not empty initially");
                cache.Add(testKey, testValue);
                Assert.That(cacheMonitor.ItemsRemoved, Is.Empty, "Not empty after all");
                cache.Remove(testKey);
                Assert.That(cacheMonitor.ItemsRemoved, Is.EquivalentTo(new [] { testKey }), "Incorrect after remove");
            }
        }
    }
}
