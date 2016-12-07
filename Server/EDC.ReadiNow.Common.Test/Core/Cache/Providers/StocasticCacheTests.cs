// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading;
using EDC.ReadiNow.Core.Cache.Providers;
using NUnit.Framework;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.Cache.Providers;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
	/// <summary>
	///     Tests the methods and properties of the Stochastic Cache class.
	/// </summary>
	[TestFixture]
	public class StochasticCacheTests
	{
        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            StochasticCache<int, int>.IsTestMode = true;
        }

        /// <summary>
        /// Teardown
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            StochasticCache<int, int>.IsTestMode = false;
        }

        /// <summary>
        ///     This test ensures that the stochastic cache does not grow any larger than the maximum.
        /// </summary>
        [Test]
        public void WhatYouPutInIsWhatYouGetOut()
        {
            var max = 10;
            var cache = CreateStochasticCache<int, int>("stoc test", max);

            Assert.That(cache.Count, Is.EqualTo(0));

            for (int i = 0; i < 1000; i++)
            {
                var v = i + 1000;
                cache.Add(i, v);

                Assert.That(cache[i], Is.EqualTo(v));
            }
        }

      
        /// <summary>
        ///     This test ensures that values are only kicked out once maximum growth is achieved.
        /// </summary>
        [Test]
        public void GrowsUntilFull()
        {
            const int max = 5;
            var cache = CreateStochasticCache<int, int>("stoc test", max);

            cache.GetOrAdd(1, key => 11);
            cache.GetOrAdd(2, key => 22);
            cache.GetOrAdd(3, key => 33);
            cache.GetOrAdd(4, key => 44);
            cache.GetOrAdd(5, key => 55);

            Assert.That(cache.Count, Is.EqualTo(5));

            cache.GetOrAdd(6, key => 66);
            Assert.That(cache.Count, Is.EqualTo(max));
                       
         }


        /// <summary>
        ///     This test ensures that values are only kicked out once maximum growth is achieved.
        /// </summary>
        [Test]
        [Explicit]
        public void CacheHistoryWorking()
        {
            var rand = new Random();
            const int max = 200; // our cache size covers 20% of our key space
            const int refetches = 5;
            var innerFactory = new CacheFactory() { CacheName = "inner" };

            var fac0 = new StochasticCacheFactory() { MaxSize = max, NumHistoryFetches = 0, Inner = innerFactory };
            var fac5 = new StochasticCacheFactory() { MaxSize = max, NumHistoryFetches = refetches, Inner = innerFactory };
            var lruFac = new LruCacheFactory() { MaxSize = max, Inner = innerFactory };

            var cache0 = fac0.Create<int, int>( "StochasticCacheTest" );
            var cache5 = fac5.Create<int, int>( "StochasticCacheTest" );
            var lru = lruFac.Create<int, int>( "StochasticCacheTest" );

            var cache0Count = 0;
            var cache5Count = 0;
            var lruCount = 0;

            for (int i=0; i<100000; i++ )
            {
                var index = i % 5 != 0 ? rand.Next(200) : rand.Next(1000);    // 80 % of hits come out of 20% of the key space. 

                cache0.GetOrAdd(index, (key) => { cache0Count++; return index; });
                cache5.GetOrAdd(index, (key) => { cache5Count++; return index; });
                lru.GetOrAdd(index, (key) => { lruCount++; return index; });
            }

            Console.WriteLine("Cache misses, smaller is better  0: {0}, {1}: {2}, lru: {3}", cache0Count, refetches, cache5Count, lruCount);
            Assert.That(cache0Count, Is.GreaterThan(cache5Count));  // It is possible but very very unlikley that this will fail.
        }

		/// <summary>
		///		Tests the stochastic cache in a multi threaded environment.
		/// </summary>
		[Test]
		public void TestMultiThreaded( )
		{

			StochasticCache<int, int>.IsTestMode = false;

			const int cacheSize = 100;
			const int hits = 1000;
			var cache = CreateStochasticCache<int, int>( "stoc test", cacheSize );

			Assert.That( cache.Count, Is.EqualTo( 0 ) );

			for ( int i = 0; i < hits; i++ )
			{
				var v = i + 1000;
				cache.Add( i, v );

				Assert.That( cache [ i ], Is.EqualTo( v ) );
			}

			Assert.That( cache.Count, Is.EqualTo( cacheSize ) );

			ParameterizedThreadStart action = o =>
			{
				var rand = new Random( new Guid( ).GetHashCode( ) );

				for ( int i = 0; i < hits; i++ )
				{
					var randVal = rand.Next( hits );

					int val;
					cache.TryGetOrAdd( randVal, out val, e => e + 1000 );

					if ( cache.Count != cacheSize )
					{

					}
				}
			};

			const int threadCount = 2;

			for ( int j = 0; j < 1; j++ )
			{
				var threads = new Thread[threadCount];

				for ( int i = 0; i < threadCount; i++ )
				{
					threads[ i ] = new Thread( action )
					{
						IsBackground = true
					};

					threads[ i ].Start( );
				}

				for ( int i = 0; i < threadCount; i++ )
				{
					threads[ i ].Join( );
				}
			}

			Assert.That( cache.Count, Is.EqualTo( cacheSize ) );
		}

		/// <summary>
		///		Tests filling then removing.
		/// </summary>
		[Test]
		public void TestFillThenRemove( )
		{
			StochasticCache<int, int>.IsTestMode = false;

			const int cacheSize = 10;
			const int hits = 1000;
			var cache = CreateStochasticCache<int, int>( "stoc test", cacheSize );

			Assert.That( cache.Count, Is.EqualTo( 0 ) );

			for ( int i = 0; i <= hits; i++ )
			{
				var v = i + 1000;
				cache.Add( i, v );

				Assert.That( cache [ i ], Is.EqualTo( v ) );
			}

			Assert.That( cache.Count, Is.EqualTo( cacheSize ) );

			for ( int i = hits; i > hits - 5; i-- )
			{
				cache.Remove( i );
			}

			int val2;
			cache.TryGetOrAdd( hits, out val2, k => k + 1000 );

			Assert.That( cache.Count, Is.GreaterThanOrEqualTo( 5 ) );
			Assert.That( cache.Count, Is.LessThanOrEqualTo( cacheSize ) );
		}

		/// <summary>
		///		Small fill test.
		/// </summary>
		[Test]
		public void SmallTest( )
		{
			StochasticCache<int, int>.IsTestMode = false;

			const int cacheSize = 10;

			var cache = CreateStochasticCache<int, int>( "stoc test", cacheSize );

			for ( int i = 0; i < 10; i++ )
			{
				cache.Add( i, i );
			}

			cache.Add( 5, 6 );
			cache.Add( 3, 4);

			Assert.That( cache.Count, Is.GreaterThanOrEqualTo( 0 ) );
			Assert.That( cache.Count, Is.LessThanOrEqualTo( cacheSize ) );
		}



        /// <summary>
        /// Create a transaction aware Stochastic cache.
        /// This method is only intended to help transition legacy code that was using the LruCache directly.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cacheName">
        /// The cache name (used for logging and partitioning).
        /// </param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        private static ICache<TKey, TValue> CreateStochasticCache<TKey, TValue>(string cacheName, int maxSize = CacheFactory.DefaultMaximumCacheSize)
        {
            var fact = new CacheFactory
            {
                CacheName = cacheName,
                MaxCacheEntries = maxSize,
                Lru = false
            };
            return fact.Create<TKey, TValue>();
        }
    }
}