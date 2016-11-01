// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    [TestFixture]
	[RunWithTransaction]
    public class PerTenantNonSharingCacheTests
    {
        private const int Tenant1Id = 123;
        private const int Tenant2Id = 4567;


        private readonly List<Tuple<long, long>> _tenant1Values = new List<Tuple<long, long>>
        {
            new Tuple<long, long>(1, 11),
            new Tuple<long, long>(2, 22),
            new Tuple<long, long>(3, 33)
        };


        private readonly List<Tuple<long, long>> _tenant2Values = new List<Tuple<long, long>>
        {
            new Tuple<long, long>(4, 44),
            new Tuple<long, long>(5, 55),
            new Tuple<long, long>(6, 66),
            new Tuple<long, long>(7, 77)
        };


        /// <summary>
        ///     Populates the cache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        private void PopulateCache(PerTenantNonSharingCache<long, long> cache)
        {
            // Populate the cache for tenant 1
            SetTenant(Tenant1Id);
            foreach (var v in _tenant1Values)
            {
                cache.Add(v.Item1, v.Item2);
            }

            // Populate the cache for tenant 2
            SetTenant(Tenant2Id);
            foreach (var v in _tenant2Values)
            {
                cache.Add(v.Item1, v.Item2);
            }
        }


        /// <summary>
        ///     Populates the cache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        private void PopulateCacheViaIndexer(PerTenantNonSharingCache<long, long> cache)
        {
            // Populate the cache for tenant 1
            SetTenant(Tenant1Id);
            foreach (var v in _tenant1Values)
            {
                cache[v.Item1] = v.Item2;
            }

            // Populate the cache for tenant 2
            SetTenant(Tenant2Id);
            foreach (var v in _tenant2Values)
            {
                cache[v.Item1] = v.Item2;
            }
        }


        /// <summary>
        ///     Sets the tenant.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        private void SetTenant(long tenantId)
        {
            var identity = new IdentityInfo(0, "TestUser");
            var tenant = new TenantInfo(tenantId);

            RequestContext.SetContext(identity, tenant, "en-US");
        }


        /// <summary>
        ///     Creates the per tenant cache.
        /// </summary>
        /// <returns></returns>
        private ICache<long, long> CreatePerTenantCache()
        {
            return CacheFactory.CreateSimpleCache<long, long>( string.Format( "{0}:{1}", "Test Cache", RequestContext.TenantId ) );
        }


        /// <summary>
        ///     Test adding to the cache.
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AddTest(bool useCustomCreateCacheCallback)
        {
            long result;
            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null);

            // Populate the cache
            PopulateCache(cache);

            // Verify that the entries are valid for tenant 1
            SetTenant(Tenant1Id);
            Assert.AreEqual(_tenant1Values.Count, cache.Count);

            foreach (var v in _tenant1Values)
            {
                Assert.IsTrue(cache.TryGetValue(v.Item1, out result));
                Assert.AreEqual(v.Item2, result);
            }

            // Verify that the entries are valid for tenant 2
            SetTenant(Tenant2Id);
            Assert.AreEqual(_tenant2Values.Count, cache.Count);

            foreach (var v in _tenant2Values)
            {
                Assert.IsTrue(cache.TryGetValue(v.Item1, out result));
                Assert.AreEqual(v.Item2, result);
            }
        }


        /// <summary>
        ///     Test clearing the cache.
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClearAllTest(bool useCustomCreateCacheCallback)
        {
            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null);

            // Populate the cache
            PopulateCache(cache);

            cache.ClearAll();

            SetTenant(Tenant1Id);
            // Verify that the cache is empty for tenant 1
            Assert.AreEqual(0, cache.Count);

            // Verify that the cache is empty for tenant 2
            SetTenant(Tenant2Id);
            Assert.AreEqual(0, cache.Count);
        }


        /// <summary>
        ///     Test clearing the cache.
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClearTest(bool useCustomCreateCacheCallback)
        {
            var innerCache = CreatePerTenantCache();

            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(()=> innerCache) : null);

            // Populate the cache
            PopulateCache(cache);

            // Clear the cache for tenant 1
            SetTenant(Tenant1Id);
            cache.Clear();

            // Verify that the cache is empty for tenant 1
            Assert.AreEqual(0, cache.Count);

            // Assert that the inner cache was explicitly cleared (because Redis needs this)
            if (useCustomCreateCacheCallback)
            {
                Assert.AreEqual(0, innerCache.Count);
            }

            // Verify that the cache is not empty for tenant 2
            SetTenant(Tenant2Id);
            Assert.AreEqual(_tenant2Values.Count, cache.Count);
        }


        /// <summary>
        ///     Test creating a cache with an invalid name, throws an exception.
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CreationNullNameTest(bool useCustomCreateCacheCallback)
        {
            Assert.Throws<ArgumentNullException>(() => new PerTenantNonSharingCache<long, long>(null, useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null));
        }


        /// <summary>
        ///     Test creating a cache with a valid name succeeds.
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CreationValidNameTest(bool useCustomCreateCacheCallback)
        {
            Assert.DoesNotThrow(() => new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null));
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEnumeratorTest(bool useCustomCreateCacheCallback)
        {
            int count;
            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null);

            // Populate the cache
            PopulateCache(cache);

            // Verify that the entries are valid for tenant 1
            SetTenant(Tenant1Id);
            Assert.AreEqual(_tenant1Values.Count, cache.Count);

            IEnumerator<KeyValuePair<long, long>> ten1Enumerator = cache.GetEnumerator();
            count = 0;
            while (ten1Enumerator.MoveNext())
            {
                KeyValuePair<long, long> item = ten1Enumerator.Current;

                Assert.AreEqual(1, _tenant1Values.Count(t => t.Item1 == item.Key && t.Item2 == item.Value));
                count++;
            }
            Assert.AreEqual(_tenant1Values.Count, count);

            // Verify that the entries are valid for tenant 2
            SetTenant(Tenant2Id);
            Assert.AreEqual(_tenant2Values.Count, cache.Count);

            IEnumerator<KeyValuePair<long, long>> ten2Enumerator = cache.GetEnumerator();
            count = 0;
            while (ten2Enumerator.MoveNext())
            {
                KeyValuePair<long, long> item = ten2Enumerator.Current;

                Assert.AreEqual(1, _tenant2Values.Count(t => t.Item1 == item.Key && t.Item2 == item.Value));
                count++;
            }
            Assert.AreEqual(_tenant2Values.Count, count);
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEnumeratorIEnumerableTest(bool useCustomCreateCacheCallback)
        {
            int count;
            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null);

            // Populate the cache
            PopulateCache(cache);

            // Verify that the entries are valid for tenant 1
            SetTenant(Tenant1Id);
            Assert.AreEqual(_tenant1Values.Count, cache.Count);

            IEnumerator ten1Enumerator = ((IEnumerable)cache).GetEnumerator();
            count = 0;
            while (ten1Enumerator.MoveNext())
            {
                KeyValuePair<long, long> item = (KeyValuePair<long, long>)ten1Enumerator.Current;

                Assert.AreEqual(1, _tenant1Values.Count(t => t.Item1 == item.Key && t.Item2 == item.Value));
                count++;
            }
            Assert.AreEqual(_tenant1Values.Count, count);

            // Verify that the entries are valid for tenant 2
            SetTenant(Tenant2Id);
            Assert.AreEqual(_tenant2Values.Count, cache.Count);

            IEnumerator ten2Enumerator = ((IEnumerable)cache).GetEnumerator();
            count = 0;
            while (ten2Enumerator.MoveNext())
            {
                KeyValuePair<long, long> item = (KeyValuePair<long, long>)ten2Enumerator.Current;

                Assert.AreEqual(1, _tenant2Values.Count(t => t.Item1 == item.Key && t.Item2 == item.Value));
                count++;
            }
            Assert.AreEqual(_tenant2Values.Count, count);
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RemoveTest(bool useCustomCreateCacheCallback)
        {
            long result;
            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null);

            // Populate the cache
            PopulateCache(cache);

            // Remove an entry from tenant 1
            SetTenant(Tenant1Id);
            cache.Remove(_tenant1Values[0].Item1);

            // Remove two entries from tenant 2
            SetTenant(Tenant2Id);
            cache.Remove(_tenant2Values[0].Item1);
            cache.Remove(_tenant2Values[1].Item1);

            // Verify entries have been removed
            SetTenant(Tenant1Id);
            Assert.AreEqual(_tenant1Values.Count - 1, cache.Count);
            Assert.IsFalse(cache.TryGetValue(_tenant1Values[0].Item1, out result));

            // Verify entries have been removed
            SetTenant(Tenant2Id);
            Assert.AreEqual(_tenant2Values.Count - 2, cache.Count);
            Assert.IsFalse(cache.TryGetValue(_tenant2Values[0].Item1, out result));
            Assert.IsFalse(cache.TryGetValue(_tenant2Values[1].Item1, out result));
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TryGetValueTest(bool useCustomCreateCacheCallback)
        {
            long result;
            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null);

            // Populate the cache
            PopulateCache(cache);

            // Verify that the entries are valid for tenant 1
            SetTenant(Tenant1Id);
            Assert.AreEqual(_tenant1Values.Count, cache.Count);

            foreach (var v in _tenant1Values)
            {
                Assert.IsTrue(cache.TryGetValue(v.Item1, out result));
                Assert.AreEqual(v.Item2, result);
            }

            foreach (var v in _tenant2Values)
            {
                Assert.IsFalse(cache.TryGetValue(v.Item1, out result));
            }

            // Verify that the entries are valid for tenant 2
            SetTenant(Tenant2Id);
            Assert.AreEqual(_tenant2Values.Count, cache.Count);

            foreach (var v in _tenant2Values)
            {
                Assert.IsTrue(cache.TryGetValue(v.Item1, out result));
                Assert.AreEqual(v.Item2, result);
            }

            foreach (var v in _tenant1Values)
            {
                Assert.IsFalse(cache.TryGetValue(v.Item1, out result));
            }
        }

        /// <summary>
        ///     Tests the TryGetValue method of the cache.
        /// </summary>
        [Test]
        [TestCase( true )]
        [TestCase( false )]
        public void TestTryGetOrAdd( bool useCustomCreateCacheCallback )
        {
            var cache = new PerTenantNonSharingCache<long, long>( "Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>( CreatePerTenantCache ) : null );

            long [ ] ids = new long [ ] { Tenant1Id, Tenant2Id };

            foreach ( long tenantId in ids )
            {
                long expectedValue = tenantId * 13;

                SetTenant( tenantId );

                /////
                // Add a single element.
                /////
                long value;
                bool result = cache.TryGetOrAdd( 1, out value, key =>
                {
                    Assert.That( key, Is.EqualTo( 1 ) );
                    return expectedValue;
                } );

                Assert.That( result, Is.False );
                Assert.That( value, Is.EqualTo( expectedValue ) );

                /////
                // Request the same again
                /////
                result = cache.TryGetOrAdd( 1, out value, key =>
                {
                    throw new InvalidOperationException( "The factory should not be called here" );
                } );

                Assert.That( result, Is.True );
                Assert.That( value, Is.EqualTo( expectedValue ) );
            }
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void IndexerTest(bool useCustomCreateCacheCallback)
        {
            var cache = new PerTenantNonSharingCache<long, long>("Test Cache", useCustomCreateCacheCallback ? new Func<ICache<long, long>>(CreatePerTenantCache) : null);

            // Populate the cache
            PopulateCacheViaIndexer(cache);

            // Verify that the entries are valid for tenant 1
            SetTenant(Tenant1Id);
            Assert.AreEqual(_tenant1Values.Count, cache.Count);

            foreach (var v in _tenant1Values)
            {                
                Assert.AreEqual(v.Item2, cache[v.Item1]);
            }

            foreach (var v in _tenant2Values)
            {
                Assert.AreEqual(0, cache[v.Item1]);
            }

            // Verify that the entries are valid for tenant 2
            SetTenant(Tenant2Id);
            Assert.AreEqual(_tenant2Values.Count, cache.Count);

            foreach (var v in _tenant2Values)
            {
                Assert.AreEqual(v.Item2, cache[v.Item1]);
            }

            foreach (var v in _tenant1Values)
            {
                Assert.AreEqual(0, cache[v.Item1]);
            }
        }


        /// <summary>
        /// Handles the ItemsRemoved event of the cache control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Collections.Generic.ItemsRemovedEventArgs{System.Int64}"/> instance containing the event data.</param>
        void cache_ItemsRemoved(object sender, Collections.Generic.ItemsRemovedEventArgs<long> e)
        {
            
        }
    }
}