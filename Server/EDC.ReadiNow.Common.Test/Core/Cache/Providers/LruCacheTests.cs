// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
	/// <summary>
	///     LRU Cache tests
	/// </summary>
	[TestFixture]
	public class LruCacheTests
	{
		/// <summary>
		/// Creates the LRU cache.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cacheName">Name of the cache.</param>
		/// <param name="maxSize">The maximum size.</param>
		/// <param name="redis">if set to <c>true</c> [redis].</param>
		/// <returns></returns>
		private static ICache<TKey, TValue> CreateLruCache<TKey, TValue>( string cacheName, int maxSize = 10, bool redis = false, int evictionFrequency = 5000 )
		{
			var fact = new CacheFactory
			{
				CacheName = cacheName,
				MaxCacheEntries = maxSize,
				Lru = true,
				LruEvictionFrequency = TimeSpan.FromMilliseconds( evictionFrequency ),
				Logging = false,
				Redis = redis
			};

			if ( redis )
			{
				fact.RedisKeyExpiry = TimeSpan.FromMinutes( 5 );
			}

			return fact.Create<TKey, TValue>( );
		}

		/// <summary>
		///     Tests the LRU cache in a multi threaded environment.
		/// </summary>
		[Test]
		public void TestMultiThreaded( )
		{
			const int size = 5;

			var cache = CreateLruCache<int, int>( "LRU Test", size );

			for ( int i = 0; i < size; i++ )
			{
				cache.Add( i, i );
			}

			const int threadCount = 2;
			const int hits = 10000;

			var addThreads = new Thread[threadCount];
			var removeThreads = new Thread[threadCount];

			ParameterizedThreadStart addThreadStart = o =>
			{
				var rand = new Random( new Guid( ).GetHashCode( ) );

				for ( int i = 0; i < hits; i++ )
				{
					var randVal = rand.Next( size );

					int val;
					cache.TryGetOrAdd( randVal, out val, e => e );
				}
			};

			ParameterizedThreadStart removeThreadStart = o =>
			{
				var rand = new Random( new Guid( ).GetHashCode( ) );

				for ( int i = 0; i < hits; i++ )
				{
					var randVal = rand.Next( size );

					cache.Remove( randVal );
				}
			};

			for ( int i = 0; i < threadCount; i++ )
			{
				addThreads[ i ] = new Thread( addThreadStart )
				{
					IsBackground = true
				};

				addThreads[ i ].Start( );

				removeThreads[ i ] = new Thread( removeThreadStart )
				{
					IsBackground = true
				};

				removeThreads[ i ].Start( );
			}

			for ( int i = 0; i < threadCount; i++ )
			{
				addThreads[ i ].Join( );
				removeThreads[ i ].Join( );
			}

			Assert.That( cache.Count, Is.GreaterThanOrEqualTo( 0 ) );
			Assert.That( cache.Count, Is.LessThanOrEqualTo( size ) );
		}

		/// <summary>
		///     Tests the LRU cache in a single threaded environment.
		/// </summary>
		[Test]
		public void TestSingleThreaded( )
		{
			const int size = 50000;
			const int evictionFrequency = 1000;

			var cache = CreateLruCache<int, int>( "LRU Test", size, false, evictionFrequency );

			for ( int i = 0; i < size * 2; i++ )
			{
				cache.Add( i, i );
			}

			cache.Add( 6, 6 );

			Thread.Sleep( evictionFrequency * 2 );

			Assert.AreEqual( size, cache.Count );

			bool removed = false;

			var rand = new Random( );

			removed = cache.Remove( rand.Next( size ) );

			Assert.AreEqual( removed ? size - 1 : size, cache.Count );

			cache.Add( 2, 2 );

			Thread.Sleep( evictionFrequency * 2 );

			Assert.AreEqual( size, cache.Count );
		}

		/// <summary>
		///     Tests the LRU cache in a single threaded environment with a redis backing.
		/// </summary>
		[Test]
		public void TestRedisBackedSingleThreaded( )
		{
			const int size = 5;
			const int evictionFrequency = 1000;

			var cache = CreateLruCache<int, int>( "LRU Test", size, true, evictionFrequency );

			for ( int i = 0; i < size; i++ )
			{
				cache.Add( i, i );
			}

			cache.Add( 6, 6 );

			Thread.Sleep( evictionFrequency * 2 );

			Assert.AreEqual( size, cache.Count );

			cache.Remove( 2 );
			cache.Remove( 2 );

			Assert.AreEqual( size - 1, cache.Count );

			cache.Add( 2, 2 );

			Assert.AreEqual( size, cache.Count );
        }

        /// <summary>
        /// Create a transaction aware LRU cache.
        /// This method is only intended to help transition legacy code that was using the LruCache directly.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cacheName">
        /// The cache name (used for logging and partitioning).
        /// </param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        private static ICache<TKey, TValue> CreateLruCache<TKey, TValue>(string cacheName, int maxSize = CacheFactory.DefaultMaximumCacheSize)
        {
            var fact = new CacheFactory
            {
                CacheName = cacheName,
                MaxCacheEntries = maxSize,
                Lru = true
            };
            return fact.Create<TKey, TValue>();
        }
    }
}