// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
	/// <summary>
	///     Timeout cache tests
	/// </summary>
	[TestFixture]
	public class TimeoutCacheTests
	{
		/// <summary>
		///     Creates the timeout cache.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cacheName">Name of the cache.</param>
		/// <param name="expirationInterval">The expiration interval.</param>
		/// <returns></returns>
		private static ICache<TKey, TValue> CreateTimeoutCache<TKey, TValue>( string cacheName, int expirationInterval = 2500 )
		{
			var fact = new CacheFactory
			{
				CacheName = cacheName,
				Logging = false,
				ExpirationInterval = TimeSpan.FromMilliseconds( expirationInterval )
			};

			return fact.Create<TKey, TValue>( );
		}

		/// <summary>
		///     Tests the single threaded.
		/// </summary>
		[Test]
		public void TestSingleThreaded( )
		{
			int hits = 1000;

			var cache = CreateTimeoutCache<int, int>( "Test cache" );

			for ( int i = 0; i < hits; i++ )
			{
				cache.Add( i, i );
			}

			Assert.AreEqual( 1000, cache.Count );

			Thread.Sleep( 5000 );

			Assert.AreEqual( 0, cache.Count );
		}

		[Test]
		public void TestMultiThreaded( )
		{
			const int size = 50000;

			var cache = CreateTimeoutCache<int, int>( "LRU Test" );

			for ( int i = 0; i < size; i++ )
			{
				cache.Add( i, i );
			}

			const int threadCount = 2;
			const int hits = 10000;

			var addThreads = new Thread [ threadCount ];
			var removeThreads = new Thread [ threadCount ];

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
				addThreads [ i ] = new Thread( addThreadStart )
				{
					IsBackground = true
				};

				addThreads [ i ].Start( );

				removeThreads [ i ] = new Thread( removeThreadStart )
				{
					IsBackground = true
				};

				removeThreads [ i ].Start( );
			}

			for ( int i = 0; i < threadCount; i++ )
			{
				addThreads [ i ].Join( );
				removeThreads [ i ].Join( );
			}

			Thread.Sleep( 5000 );

			Assert.That( cache.Count, Is.EqualTo( 0 ) );
		}
	}
}