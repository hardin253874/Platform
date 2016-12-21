// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using EDC.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Test;

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
			TimeSpan timeout = TimeSpan.FromSeconds( 1 );

			var cache = CreateCache<string, string>( false, TimeSpan.FromMilliseconds( 100 ) );

			using ( AutoResetEvent evt = new AutoResetEvent( false ) )
			{
				int removedItemCount = 0;

				ItemsRemovedEventHandler<string> itemsRemoved = ( sender, args ) =>
				{
					removedItemCount += args.Items.Count;

					// ReSharper disable once AccessToDisposedClosure
					evt.Set( );
				};

				cache.ItemsRemoved += itemsRemoved;

				/////
				// Add 3 elements.
				/////
				cache.Add( "a", "A" );
				cache.Add( "b", "B" );
				cache.Add( "c", "C" );

				/////
				// Wait (may remove variable number of items depending on timing)
				/////
				evt.WaitOne( timeout );

				/////
				// Add another 3 elements.
				/////
				cache.Add( "d", "D" );
				cache.Add( "e", "E" );
				cache.Add( "f", "F" );

				/////
				// Wait (may remove variable number of items depending on timing)
				/////
				evt.WaitOne( timeout );

				/////
				// Add 3 elements.
				/////
				cache.Add( "g", "G" );
				cache.Add( "h", "H" );
				cache.Add( "i", "I" );

				/////
				// Wait (may remove variable number of items depending on timing)
				/////
				evt.WaitOne( timeout );

				/////
				// If not all items have been removed (due to timing), wait for the next eviction pass.
				/////
				if ( removedItemCount < 9 )
				{
					evt.WaitOne( timeout );
				}

				Assert.AreEqual( 0, cache.Count );

				Assert.IsFalse( cache.ContainsKey( "a" ) );
				Assert.IsFalse( cache.ContainsKey( "b" ) );
				Assert.IsFalse( cache.ContainsKey( "c" ) );

				cache.ItemsRemoved -= itemsRemoved;
			}
        }

        [Test]
        public void Test_ItemsRemoved()
        {
            var cacheExpiry = TimeSpan.FromMilliseconds(100);
            var itemsRemoved = new List<int>();
            var expectedRemoved = new List<int>();

			using ( AutoResetEvent evt = new AutoResetEvent( false ) )
			{
				var cache = CreateCache<int, int>( false, cacheExpiry );

				ItemsRemovedEventHandler<int> itemsRemovedHandler = ( sender, args ) =>
				{
					itemsRemoved.AddRange( args.Items );

					// ReSharper disable once AccessToDisposedClosure
					evt.Set( );
				};

				cache.ItemsRemoved += itemsRemovedHandler;

				foreach ( var key in new [ ] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } )
				{
					cache.Add( key, key );
					expectedRemoved.Add( key );
				}

				evt.WaitOne( cacheExpiry );

				int loopCounter = 0;

				while ( itemsRemoved.Count < 10 )
				{
					evt.WaitOne( cacheExpiry );

					loopCounter++;

					if ( loopCounter > 20 )
					{
						break;
					}
				}

				// Generate strings so that the error message doesn't get confusing when the removedList is mutating underneath us.
				var expectedString = string.Join( ", ", expectedRemoved );
				var removedString = string.Join( ", ", itemsRemoved.OrderBy( a => a ) );

				Assert.That( removedString, Is.EqualTo( expectedString ), "Not removed" );

				cache.ItemsRemoved -= itemsRemovedHandler;
			}
        }

        [Test]
        public void Test_Purging()
        {
            int testCount = 1000;

	        var cacheExpiry = TimeSpan.FromMilliseconds(100);

			using ( AutoResetEvent evt = new AutoResetEvent( false ) )
			{
				int removedItemCount = 0;

				ItemsRemovedEventHandler<string> itemsRemoved = ( sender, args ) =>
				{
					removedItemCount += args.Items.Count;

					// ReSharper disable once AccessToDisposedClosure
					evt.Set( );
				};

				var cache = CreateCache<string, string>( false, cacheExpiry );

				cache.ItemsRemoved += itemsRemoved;

				for ( int i = 0; i < testCount; i++ )
					cache.Add( Guid.NewGuid( ).ToString( ), "a" );

				evt.WaitOne( 100 );

				int loopCounter = 0;

				while ( removedItemCount < testCount )
				{
					evt.WaitOne( 100 );

					loopCounter++;

					if ( loopCounter > 20 )
					{
						break;
					}
				}

				cache.Add( Guid.NewGuid( ).ToString( ), "a" );      // Purging is triggered by an add

				// check that it is all purged
				Assert.That( cache.Count, Is.EqualTo( 1 ) );

				cache.ItemsRemoved -= itemsRemoved;
			}
        }


        [Test]
        public void Test_MultithreadedPurging()
        {
	        var cacheExpiry = TimeSpan.FromMilliseconds(500);

            var cache = CreateCache<string, string>(false, cacheExpiry);

            // just making sure that removing entries that don't exist doesn't cause a problem.
            cache.Remove(new[] { "a", "b" });
        }
    }
}