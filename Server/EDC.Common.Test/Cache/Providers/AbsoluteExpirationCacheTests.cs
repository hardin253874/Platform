// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using EDC.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Core.Cache;
using System.Diagnostics;

namespace EDC.Test.Cache.Providers
{
    /// <summary>
    ///     This is the test case for the AbsoluteExpirationCache class.
    /// </summary>
    [TestFixture]
    [Category("ExtendedTests")]
    public class AbsoluteExpirationCacheTests
    {
        private ICache<TKey, TValue> CreateCache<TKey, TValue>(bool transactionAware, TimeSpan timeout, TimeSpan? frequency = null)
        {
            CacheFactory fact = new CacheFactory
            {
                CacheName = "Absolute Expiration Tests",
                ExpirationInterval = timeout,
                TransactionAware = transactionAware,
				TimeoutEvictionFrequency = frequency ?? timeout
            };
            return fact.Create<TKey, TValue>();
        }


        /// <summary>
		///     This test verifies that any entries that are added to the cache
		///     during a transaction are valid on commit.
		/// </summary>
		[Test]
        public void AddEntryDuringTranCommitTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true, TimeSpan.FromMinutes(5));
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            // Create a transaction
            using (DatabaseContext context = DatabaseContext.GetContext(true))
            {
                // Add two items to the cache
                cache.Add(3, "3");
                cache.Add(4, "4");

                Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
                Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");

                context.CommitTransaction();
            }

            Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
            Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
            Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
            Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");
        }

        /// <summary>
        ///     This test verifies that any entries that are added to the cache
        ///     during a transaction are invalidated on rollback.
        /// </summary>
        [Test]
        public void AddEntryDuringTranRollbackTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true, TimeSpan.FromMinutes(5));
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            // Create a transaction
            using (DatabaseContext.GetContext(true))
            {
                // Add two items to the cache
                // Use add method
                cache.Add(3, "3");
                // Use indexer
                cache[4] = "4";

                Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
                Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");

                // Rollback the transaction. Entries 3 and 4 should not be
                // in the cache.
            }

            Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
            Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
            Assert.IsFalse(cache.ContainsKey(3), "Entry with key 3 should not exist.");
            Assert.IsFalse(cache.ContainsKey(4), "Entry with key 4 should not exist.");
        }

        /// <summary>
        ///     Tests the cache expiration.
        /// </summary>
        [Test]
        public void TestExpiration()
        {
			var cache = CreateCache<string, string>( false, TimeSpan.FromSeconds( 2 ), TimeSpan.FromMilliseconds( 500 ) );    //,6
            cache.Add("a", "A");
            cache.Add("b", "B");
            cache.Add("c", "C");

            /////
            // Add 3 elements.
            /////

            Assert.AreEqual(3, cache.Count);

            /////
            // Wait .25 seconds.
            /////
            Thread.Sleep(1000);

            /////
            // Add another 3 elements.
            /////
            cache.Add("d", "D");
            cache.Add("e", "E");
            cache.Add("f", "F");

            /////
            // Ensure all elements are present.
            /////
            Assert.AreEqual(6, cache.Count);

            /////
            // Wait another .3 seconds causing the initial 3 elements to expire.
            /////
            Thread.Sleep(1000);

            Assert.AreEqual(3, cache.Count);

            /////
            // Wait another .3 seconds causing the last 3 elements to expire.
            /////
            Thread.Sleep(1000);

            Assert.AreEqual(0, cache.Count);

            /////
            // Add 3 elements.
            /////
            cache.Add("a", "A");
            cache.Add("b", "B");
            cache.Add("c", "C");

            Assert.AreEqual(3, cache.Count);

            /////
            // Wait .25 seconds.
            /////
            Thread.Sleep(1000);

            /////
            // Move the entry to the front of th lru list.
            /////
            Assert.IsTrue(cache.ContainsKey("b"));

            /////
            // Wait .3 seconds.
            /////
            Thread.Sleep(1000);

            Assert.AreEqual(0, cache.Count);

            Assert.IsFalse(cache.ContainsKey("a"));
            Assert.IsFalse(cache.ContainsKey("b"));
            Assert.IsFalse(cache.ContainsKey("c"));
        }

        [Test]
        //[Repeat(100)]
        public void Test_ItemsRemoved()
        {
            var cacheExpiry = TimeSpan.FromMilliseconds(3000);
            var refreshRate = TimeSpan.FromMilliseconds(100);
            var delta = TimeSpan.FromMilliseconds(1000);            // WATCH OUT - as we a relying on other threads anything less than a second tends to generate occasional errors.
            var itemsRemoved = new List<int>();
            var expectedRemoved = new List<int>();

            var cache = CreateCache<int, int>(false, cacheExpiry, refreshRate);
            cache.ItemsRemoved += (sender, args) => itemsRemoved.AddRange(args.Items);

            Thread.Sleep(1000);
            foreach (var key in new int[] { 1,2,3,4,5,6,7,8,9,10 })
            {
                cache.Add(key, key);
                expectedRemoved.Add(key);

                // Wait with a little bit of breathing room for the refresh rate
                Thread.Sleep(cacheExpiry + delta);

                // Generate strings so that the error message doesn't get confusing when the removedList is mutating underneith us.
                var expectedString = String.Join(", ", expectedRemoved);
                var removedString = String.Join(", ", itemsRemoved);

                Assert.That(removedString, Is.EqualTo(expectedString), "Not removed");
            }
        }

        [Test]
        public void Test_Purging()
        {
            int testCount = 1000000;
            ICache<string, string> cache;
            TimeSpan cacheExpiry;

            cacheExpiry = TimeSpan.FromMilliseconds(500);

            cache = CreateCache<string, string>(false, cacheExpiry);

            for (int i = 0; i < testCount; i++)
                cache.Add(Guid.NewGuid().ToString(), "a");


            Thread.Sleep(600);
            //Assert.That(cache.Count, Is.EqualTo(testCount));

            cache.Add(Guid.NewGuid().ToString(), "a");      // Purging is triggered by an add

            // check that it is all purged
            Assert.That(cache.Count, Is.EqualTo(1));
        }


        [Test]
        public void Test_MultithreadedPurging()
        {
            ICache<string, string> cache;
            TimeSpan cacheExpiry;

            cacheExpiry = TimeSpan.FromMilliseconds(500);

            cache = CreateCache<string, string>(false, cacheExpiry);

            // just making sure that removing entries that don't exist doesn't cause a problem.
            cache.Remove(new string[] { "a", "b" });
        }
    }
}