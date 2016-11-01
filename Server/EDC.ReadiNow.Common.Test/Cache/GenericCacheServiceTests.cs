// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Cache;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Test.Cache
{
    [TestFixture]
    /// <summary>
    /// 
    /// </summary>
    public class GenericCacheServiceTests
    {
        [Test]
        public void Test_Constructor()
        {
            var cacheService = new TestCacheService();
            Assert.That(cacheService, Is.Not.Null);
            Assert.That(cacheService.CacheInvalidator, Is.Not.Null);
        }

        [Test]
        public void Test_GetOrAdd()
        {
            var cacheService = new TestCacheService();

            string res;
            res = cacheService.Test_GetOrAdd(1, x => "res");
            Assert.That(res, Is.EqualTo("res"));
        }

        [Test]
        public void Test_GetOrAdd_Recall()
        {
            var cacheService = new TestCacheService();

            string res;
            int count = 0;
            res = cacheService.Test_GetOrAdd(1, x => { count++; return "res"; });
            res = cacheService.Test_GetOrAdd(1, x => { count++; return "res"; });
            Assert.That(res, Is.EqualTo("res"));
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Test_GetOrAddMultiple()
        {
            var cacheService = new TestCacheService();

            cacheService.Test_GetOrAdd(1, x => "1");
            cacheService.Test_GetOrAdd(3, x => "3");

            var res = cacheService.Test_GetOrAddMultiple(new long[] { 1, 2, 3, 4 }, keys =>
            {
                // Callback only called with missing entries
                Assert.That(keys, Is.EquivalentTo(new long[] { 2, 4 }));
                return keys.Select(key => key.ToString());
            });
            Assert.That(res.Select(p => p.Key), Is.EquivalentTo(new long[] { 1, 2, 3, 4 }));
            Assert.That(res.Select(p => p.Value), Is.EquivalentTo(new string[] { "1", "2", "3", "4" }));
        }

        [Test]
        public void Test_Clear()
        {
            var cacheService = new TestCacheService();

            string res;
            int count = 0;
            res = cacheService.Test_GetOrAdd(1, x => { count++; return "res"; });
            cacheService.Clear();
            res = cacheService.Test_GetOrAdd(1, x => { count++; return "res"; });
            Assert.That(res, Is.EqualTo("res"));
            Assert.That(count, Is.EqualTo(2));
        }
    }

    class TestCacheService : GenericCacheService<long, string>
    {
        public TestCacheService() : base("TestCacheService") { }

        public string Test_GetOrAdd(long key, Func<long, string> valueFactory )
        {
            return GetOrAdd(key, valueFactory);
        }
        public IReadOnlyCollection<KeyValuePair<long, string>> Test_GetOrAddMultiple(IEnumerable<long> keys, Func<IEnumerable<long>, IEnumerable<string>> valueFactory)
        {
            return GetOrAddMultiple(keys, valueFactory);
        }
    }
}
