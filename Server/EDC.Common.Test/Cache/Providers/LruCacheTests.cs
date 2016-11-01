// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using EDC.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Core.Cache;

namespace EDC.Test.Cache.Providers
{
	/// <summary>
	///     Tests the methods and properties of the Lru Cache class.
	/// </summary>
	[TestFixture]
	public class LruCacheTests
	{
        private ICache<TKey, TValue> CreateCache<TKey, TValue>(bool transactionAware, int size = 10000)
	    {
            CacheFactory fact = new CacheFactory
            {
                CacheName = "LRUCache tests",
                MaxCacheEntries = size,
                TransactionAware = transactionAware
            };
            return fact.Create<TKey, TValue>( );
        }


        /// <summary>
        ///     This test creates two threads that add items to the cache
        ///     in transactions. Both threads commit their transaction.
        ///     The items added by both threads should be present in the cache.
        /// </summary>
        [Test]
        public void AddEntryDuringTranCommitMultipleThreadsTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true);
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            var startEvent = new ManualResetEvent(false);

            var t1 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext context = DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(3, "3");
                        cache.Add(4, "4");

                        Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
                        Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
                        Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
                        Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");

                        // Commit the transaction
                        context.CommitTransaction();
                    }
                })
                {
                    IsBackground = true
                };
            t1.Start();

            var t2 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext context = DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(5, "5");
                        cache.Add(6, "6");

                        Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
                        Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
                        Assert.IsTrue(cache.ContainsKey(5), "Entry with key 5 should exist.");
                        Assert.IsTrue(cache.ContainsKey(6), "Entry with key 6 should exist.");

                        // Commit the transaction
                        context.CommitTransaction();
                    }
                })
                {
                    IsBackground = true
                };
            t2.Start();

            startEvent.Set();

            t1.Join();
            t2.Join();

            Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
            Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
            Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
            Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");
            Assert.IsTrue(cache.ContainsKey(5), "Entry with key 5 should exist.");
            Assert.IsTrue(cache.ContainsKey(6), "Entry with key 6 should exist.");
        }

        /// <summary>
        ///     This test creates two threads that add items to the cache
        ///     in transactions. One thread rolls back it's transaction and
        ///     the other commits.
        ///     The items added by the thread that rolls back should not be present in the cache.
        /// </summary>
        [Test]
        public void AddEntryDuringTranCommitRollbackMultipleThreadsTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true);
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            var startEvent = new ManualResetEvent(false);

            var t1 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext context = DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(3, "3");
                        cache.Add(4, "4");

                        Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
                        Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
                        Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
                        Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");

                        // Commit the transaction
                        context.CommitTransaction();
                    }
                })
                {
                    IsBackground = true
                };
            t1.Start();

            var t2 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(5, "5");
                        cache.Add(6, "6");

                        Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
                        Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
                        Assert.IsTrue(cache.ContainsKey(5), "Entry with key 5 should exist.");
                        Assert.IsTrue(cache.ContainsKey(6), "Entry with key 6 should exist.");

                        // Rollback the transaction. Entries 5 and 6 should not be
                        // in the cache.
                    }
                })
                {
                    IsBackground = true
                };
            t2.Start();

            startEvent.Set();

            t1.Join();
            t2.Join();

            Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
            Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
            Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
            Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");
            Assert.IsFalse(cache.ContainsKey(5), "Entry with key 5 should exist.");
            Assert.IsFalse(cache.ContainsKey(6), "Entry with key 6 should exist.");
        }

        /// <summary>
        ///     This test verifies that any entries that are added to the cache
        ///     during a transaction are valid on commit.
        /// </summary>
        [Test]
        public void AddEntryDuringTranCommitTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true);
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            // Create a transaction
            using (DatabaseContext context = DatabaseContext.GetContext(true))
            {
                // Add two items to the cache
                cache.Add(3, "3");
                cache.Add(4, "4");

                Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
                Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
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
        ///     This test creates two threads.
        ///     One thread adds items in a transaction which are
        ///     retrieved by the other thread which is not
        ///     in a transaction
        /// </summary>
        [Test]
        public void AddEntryDuringTranOnThreadRollbackOtherThreadNonTranTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true);
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            var startEvent = new ManualResetEvent(false);
            var addedEvent = new ManualResetEvent(false);
            var canRollBackEvent = new ManualResetEvent(false);

            var t1 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(3, "3");
                        cache.Add(4, "4");

                        addedEvent.Set();

                        Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
                        Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");
                        Assert.IsTrue(cache.ContainsKey(3), "Value for entry 3 is invalid.");
                        Assert.IsTrue(cache.ContainsKey(4), "Value for entry 4 is invalid.");

                        // Rollback the transaction. Entries 3 and 4 should not be
                        // in the cache.
                        canRollBackEvent.WaitOne();
                    }
                })
                {
                    IsBackground = true
                };
            t1.Start();

            var t2 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    // wait for 3 and 4 to be added by thread1
                    addedEvent.WaitOne();

                    // 3 and 4 should not be visible to this thread
                    Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
                    Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");
                    Assert.IsFalse(cache.ContainsKey(3), "Value for entry 3 is invalid.");
                    Assert.IsFalse(cache.ContainsKey(4), "Value for entry 4 is invalid.");

                    canRollBackEvent.Set();
                })
                {
                    IsBackground = true
                };
            t2.Start();

            startEvent.Set();

            t1.Join();
            t2.Join();

            Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
            Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");
            Assert.IsFalse(cache.ContainsKey(3), "Value for entry 3 is invalid.");
            Assert.IsFalse(cache.ContainsKey(4), "Value for entry 4 is invalid.");
        }

        /// <summary>
        ///     This test creates two threads that add items to the cache
        ///     in transactions which are rolled back.
        ///     The items added by the threads should not be present in the cache.
        /// </summary>
        [Test]
        public void AddEntryDuringTranRollbackMultipleThreadsTest()
        {
            // Create our transaction aware cache            
            var cache = CreateCache<int, string>(true);
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            var startEvent = new ManualResetEvent(false);

            var t1 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(3, "3");
                        cache.Add(4, "4");

                        Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
                        Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
                        Assert.IsTrue(cache.ContainsKey(3), "Entry with key 3 should exist.");
                        Assert.IsTrue(cache.ContainsKey(4), "Entry with key 4 should exist.");

                        // Rollback the transaction. Entries 3 and 4 should not be
                        // in the cache.
                    }
                })
                {
                    IsBackground = true
                };
            t1.Start();

            var t2 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(5, "5");
                        cache.Add(6, "6");

                        Assert.IsTrue(cache.ContainsKey(1), "Entry with key 3 should exist.");
                        Assert.IsTrue(cache.ContainsKey(2), "Entry with key 4 should exist.");
                        Assert.IsTrue(cache.ContainsKey(5), "Entry with key 5 should exist.");
                        Assert.IsTrue(cache.ContainsKey(6), "Entry with key 6 should exist.");

                        // Rollback the transaction. Entries 5 and 6 should not be
                        // in the cache.
                    }
                })
                {
                    IsBackground = true
                };
            t2.Start();

            startEvent.Set();

            t1.Join();
            t2.Join();

            Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
            Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
            Assert.IsFalse(cache.ContainsKey(3), "Entry with key 3 should exist.");
            Assert.IsFalse(cache.ContainsKey(4), "Entry with key 4 should exist.");
            Assert.IsFalse(cache.ContainsKey(5), "Entry with key 5 should exist.");
            Assert.IsFalse(cache.ContainsKey(6), "Entry with key 6 should exist.");
        }

        /// <summary>
        ///     This test verifies that any entries that are added to the cache
        ///     during a transaction are invalidated on rollback.
        /// </summary>
        [Test]
        public void AddEntryDuringTranRollbackTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true);
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

                Assert.IsTrue(cache.ContainsKey(1), "Entry with key 1 should exist.");
                Assert.IsTrue(cache.ContainsKey(2), "Entry with key 2 should exist.");
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
        ///     This test verifies that any entries that are added and remove
        ///     from the cache during a transaction are invalidated on rollback.
        /// </summary>
        [Test]
        public void AddRemoveEntryDuringTranRollbackTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true);
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            // Create a transaction
            using (DatabaseContext.GetContext(true))
            {
                // Add two items to the cache
                cache.Add(3, "3");
                cache.Add(4, "4");

                Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
                Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");
                Assert.IsTrue(cache.ContainsKey(3), "Value for entry 3 is invalid.");
                Assert.IsTrue(cache.ContainsKey(4), "Value for entry 4 is invalid.");

                Assert.IsTrue(cache.Remove(3));
                Assert.IsFalse(cache.Remove(3));

                Assert.IsFalse(cache.ContainsKey(3), "Value for entry 3 is invalid.");

                // Rollback the transaction. Entries 3 and 4 should not be
                // in the cache.
            }

            Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
            Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");
            Assert.IsFalse(cache.ContainsKey(3), "Value for entry 3 is invalid.");
            Assert.IsFalse(cache.ContainsKey(4), "Value for entry 4 is invalid.");

            Assert.AreEqual(2, cache.Count, "The count is invalid");

            Assert.IsTrue(cache.ContainsKey(1));
            Assert.IsTrue(cache.ContainsKey(2));
        }

        /// <summary>
        ///     This test creates two threads which add an
        ///     item with the same key but different value
        ///     to the cache.
        /// </summary>
        [Test]
        public void AddSameEntryDuringTranMultipleThreadsTest()
        {
            // Create our transaction aware cache
            var cache = CreateCache<int, string>(true);
            cache.Add(1, "1");
            cache.Add(2, "2");

            // Add some items outside the transaction

            var startEvent = new ManualResetEvent(false);
            var addedEvent = new ManualResetEvent(false);
            var canRollBackEvent = new ManualResetEvent(false);

            var t1 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(3, "3A");
                        cache.Add(4, "4A");

                        addedEvent.Set();

                        Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
                        Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");

                        string v3;
                        string v4;
                        cache.TryGetValue(3, out v3);
                        cache.TryGetValue(4, out v4);

                        Assert.AreEqual("3A", v3, "Value for entry 3 is invalid.");
                        Assert.AreEqual("4A", v4, "Value for entry 4 is invalid.");

                        // Rollback the transaction. Entries 3 and 4 should not be
                        // in the cache.
                        canRollBackEvent.WaitOne();
                    }
                })
                {
                    IsBackground = true
                };
            t1.Start();

            var t2 = new Thread(() =>
                {
                    startEvent.WaitOne();

                    using (DatabaseContext.GetContext(true))
                    {
                        // Add two items to the cache
                        cache.Add(3, "3B");
                        cache.Add(4, "4B");

                        // wait for 3 and 4 to be added by thread1
                        addedEvent.WaitOne();

                        // 3 and 4 should not be visible to this thread
                        Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
                        Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");

                        string v3;
                        string v4;
                        cache.TryGetValue(3, out v3);
                        cache.TryGetValue(4, out v4);

                        Assert.AreEqual("3B", v3, "Value for entry 3 is invalid.");
                        Assert.AreEqual("4B", v4, "Value for entry 4 is invalid.");
                    }
                    canRollBackEvent.Set();
                })
                {
                    IsBackground = true
                };
            t2.Start();

            startEvent.Set();

            t1.Join();
            t2.Join();

            Assert.IsTrue(cache.ContainsKey(1), "Value for entry 1 is invalid.");
            Assert.IsTrue(cache.ContainsKey(2), "Value for entry 2 is invalid.");
            Assert.IsFalse(cache.ContainsKey(3), "Value for entry 3 is invalid.");
            Assert.IsFalse(cache.ContainsKey(4), "Value for entry 4 is invalid.");
        }

		/// <summary>
		///     Tests adding elements into the cache and ensuring they are retained.
		/// </summary>
		[Test]
		public void TestAdd( )
		{
			var cache = CreateCache<string, string>( false );

			try
			{
				/////
				// Should throw an ArgumentNullException.
				/////
				cache.Add( null, null );
			}
			catch ( ArgumentNullException )
			{
			}

			/////
			// Add 'a'
			/////
			cache.Add( "a", "A" );

			/////
			// Test 'a'
			/////
			Assert.AreEqual( cache.Count, 1 );
			Assert.AreEqual( cache.Keys().Count(), 1 );
			Assert.AreEqual( cache.Values().Count(), 1 );

			string val = cache[ "a" ];

			Assert.AreEqual( val, "A" );

			Assert.IsTrue( cache.TryGetValue( "a", out val ) );
			Assert.AreEqual( val, "A" );

			/////
			// Add 'b'
			/////
			cache.Add( "b", "B" );

			/////
			// Test 'b'
			/////
			Assert.AreEqual( cache.Count, 2 );

			val = cache[ "b" ];

			Assert.AreEqual( val, "B" );

			Assert.IsTrue( cache.TryGetValue( "b", out val ) );
			Assert.AreEqual( val, "B" );

			/////
			// Add 'c'
			/////
			cache.Add( "c", "C" );

			/////
			// Test 'c'
			/////
			Assert.AreEqual( cache.Count, 3 );

			val = cache[ "c" ];

			Assert.AreEqual( val, "C" );

			Assert.IsTrue( cache.TryGetValue( "c", out val ) );
			Assert.AreEqual( val, "C" );

			/////
			// Add 'd'
			/////
			cache.Add( "d", "D" );

			/////
			// Test 'd'
			/////
			Assert.AreEqual( cache.Count, 4 );

			val = cache[ "d" ];

			Assert.AreEqual( val, "D" );

			Assert.IsTrue( cache.TryGetValue( "d", out val ) );
			Assert.AreEqual( val, "D" );

			/////
			// Add 'e'
			/////
			cache.Add( "e", "E" );

			/////
			// Test 'e'
			/////
			Assert.AreEqual( cache.Count, 5 );

			val = cache[ "e" ];

			Assert.AreEqual( val, "E" );

			Assert.IsTrue( cache.TryGetValue( "e", out val ) );
			Assert.AreEqual( val, "E" );

			/////
			// Add 'f'
			/////
			cache.Add("f", "F" );

			/////
			// Test 'f'
			/////
			Assert.AreEqual( cache.Count, 6 );

			val = cache[ "f" ];

			Assert.AreEqual( val, "F" );

			Assert.IsTrue( cache.TryGetValue( "f", out val ) );
			Assert.AreEqual( val, "F" );

			/////
			// Attempt to re-add 'a'. Should throw an ArgumentException.
			/////
			try
			{
				cache.Add("a", "Different A");
			}
			catch ( ArgumentException )
			{
			}
		}

        /// <summary>
        ///     Tests the TryGetValue method of the cache.
        /// </summary>
        [Test]
        public void TestTryGetOrAdd( )
        {
            var cache = CreateCache<string, string>( false );

            Assert.AreEqual( cache.Count, 0 );

            /////
            // Add a single element.
            /////
            string value;
            bool result = cache.TryGetOrAdd( "a", out value, key => 
                {
                    Assert.That(key, Is.EqualTo("a"));
                    return "A";
                });

            Assert.That( result, Is.False );
            Assert.That( value, Is.EqualTo( "A" ) );

            /////
            // Request the same again
            /////
            result = cache.TryGetOrAdd( "a", out value, key => 
            {
                throw new InvalidOperationException( "The factory should not be called here" );
            } );

            Assert.That( result, Is.True );
            Assert.That( value, Is.EqualTo( "A" ) );
        }

		/// <summary>
		///     Tests clearing all the elements from the cache.
		/// </summary>
		[Test]
		public void TestClear( )
		{
			var cache = CreateCache<string, string>( false );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache.Add( "a", "A" );

			Assert.AreEqual( cache.Count, 1 );

			/////
			// Clear the elements.
			/////
			cache.Clear( );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add multiple elements.
			/////
			cache.Add( "a", "A" );
			cache.Add( "b", "B" );
			cache.Add( "c", "C" );
			cache.Add( "d", "D" );

			Assert.AreEqual( cache.Count, 4 );

			/////
			// Clear the elements.
			/////
			cache.Clear( );

			Assert.AreEqual( cache.Count, 0 );
		}

		/// <summary>
		///     Tests the Contains method of the cache.
		/// </summary>
		[Test]
		public void TestContains( )
		{
			var cache = CreateCache<string, string>( false );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache.Add( "a", "A" );

			Assert.AreEqual( cache.Count, 1 );

			Assert.IsTrue( cache.Contains( new KeyValuePair<string, string>( "a", "A" ) ) );
			Assert.IsFalse( cache.Contains( new KeyValuePair<string, string>( "a", "a" ) ) );
		}

		/// <summary>
		///     Tests the ContainsKey method of the cache.
		/// </summary>
		[Test]
		public void TestContainsKey( )
		{
			var cache = CreateCache<string, string>( false );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache.Add( "a", "A" );

			Assert.AreEqual( cache.Count, 1 );

			Assert.IsTrue( cache.ContainsKey( "a" ) );
			Assert.IsFalse( cache.ContainsKey( "A" ) );
		}

		/// <summary>
		///     Tests the cache enumerators.
		/// </summary>
		[Test]
		public void TestEnumerators( )
		{
		    var cache = CreateCache<string, string>(false, 5);
		    cache.Add("a", "A");
            cache.Add("b", "B");
            cache.Add("c", "C");
            cache.Add("d", "D");
            cache.Add("e", "E");

			int count = cache.Count( );

			Assert.AreEqual( 5, count );

			count = cache.Count( );

			Assert.AreEqual( 5, count );
		}

		/// <summary>
		///     Tests the Indexer property (both get and set) of the cache.
		/// </summary>
		[Test]
		public void TestIndexer( )
		{
			var cache = CreateCache<string, string>( false );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache[ "a" ] = "A";

			Assert.AreEqual( cache.Count, 1 );

			/////
			// Attempt to obtain a non-existent element.
			/////
			Assert.IsNull( cache[ "z" ] );

			Assert.AreEqual( cache.Count, 1 );

			Assert.AreEqual( cache[ "a" ], "A" );

			/////
			// Change the value.
			/////
			cache[ "a" ] = "Z";

			Assert.AreEqual( cache[ "a" ], "Z" );

			Assert.AreEqual( cache.Count, 1 );

			cache[ "b" ] = "B";
			cache[ "c" ] = "C";

			Assert.AreEqual( cache.Count, 3 );

			Assert.AreEqual( cache[ "b" ], "B" );
			Assert.AreEqual( cache[ "c" ], "C" );

			cache[ "b" ] = "Z";

			Assert.AreEqual( cache[ "a" ], "Z" );
			Assert.AreEqual( cache[ "b" ], "Z" );
		}

		/// <summary>
		///     Tests the Keys property of the cache.
		/// </summary>
		[Test]
		public void TestKeys( )
		{
			var cache = CreateCache<string, string>( false );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache.Add( "a", "A" );

			Assert.AreEqual( cache.Count, 1 );

			Assert.IsNotNull( cache.Keys() );

			Assert.AreEqual( cache.Keys().Count(), 1 );
			Assert.AreEqual( cache.Keys().ElementAt( 0 ), "a" );

			cache.Add( "b", "B" );
			cache.Add( "c", "C" );

			Assert.AreEqual( cache.Count, 3 );

			Assert.IsNotNull( cache.Keys() );

			Assert.AreEqual( cache.Keys().Count(), 3 );

			Assert.IsTrue( cache.Keys().Contains( "a" ) );
			Assert.IsTrue( cache.Keys().Contains( "b" ) );
			Assert.IsTrue( cache.Keys().Contains( "c" ) );
		}

		/// <summary>
		///     Tests the MaxSize property of the cache.
		/// </summary>
		[Test]
		public void TestMaxSize( )
		{
			var cache = CreateCache<string, string>( false, 3 );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache[ "a" ] = "A";

			Assert.AreEqual( cache.Count, 1 );

			cache[ "b" ] = "B";
			cache[ "c" ] = "C";

			/////
			// Cache is now full.
			/////
			Assert.AreEqual( cache.Count, 3 );

			/////
			// Cache is now past full.
			/////
			cache[ "d" ] = "D";

			Assert.AreEqual( cache.Count, 3 );

			Assert.IsFalse( cache.ContainsKey( "a" ) );
			Assert.IsTrue( cache.ContainsKey( "b" ) );
			Assert.IsTrue( cache.ContainsKey( "c" ) );
			Assert.IsTrue( cache.ContainsKey( "d" ) );

			cache[ "e" ] = "E";

			Assert.AreEqual( cache.Count, 3 );

			Assert.IsFalse( cache.ContainsKey( "a" ) );
			Assert.IsFalse( cache.ContainsKey( "b" ) );
			Assert.IsTrue( cache.ContainsKey( "c" ) );
			Assert.IsTrue( cache.ContainsKey( "d" ) );
			Assert.IsTrue( cache.ContainsKey( "e" ) );

			/////
			// Move this element to the front of the Lru cache.
			/////
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			cache.ContainsKey( "c" );
// ReSharper restore ReturnValueOfPureMethodIsNotUsed

			/////
			// Add another element.
			/////
			cache[ "f" ] = "F";

			Assert.IsFalse( cache.ContainsKey( "a" ) );
			Assert.IsFalse( cache.ContainsKey( "b" ) );
			Assert.IsTrue( cache.ContainsKey( "c" ) );
			Assert.IsFalse( cache.ContainsKey( "d" ) );
			Assert.IsTrue( cache.ContainsKey( "e" ) );
			Assert.IsTrue( cache.ContainsKey( "f" ) );
		}

		/// <summary>
		///     Tests the Remove method of the cache.
		/// </summary>
		[Test]
		public void TestRemove( )
		{
			var cache = CreateCache<string, string>( false );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache.Add( "a", "A" );

			Assert.AreEqual( cache.Count, 1 );

			/////
			// Remove non-existing element.
			/////
			Assert.IsFalse( cache.Remove( "z" ) );

			Assert.AreEqual( cache.Count, 1 );

			/////
			// Remove only element.
			/////
			Assert.IsTrue( cache.Remove( "a" ) );

			Assert.AreEqual( cache.Count, 0 );

			cache.Add( "a", "A" );
			cache.Add( "b", "B" );
			cache.Add( "c", "C" );

			Assert.AreEqual( cache.Count, 3 );

			/////
			// Remove valid element.
			/////
			Assert.IsTrue( cache.Remove( "b" ) );

			Assert.AreEqual( cache.Count, 2 );

			Assert.IsTrue( cache.ContainsKey( "a" ) );
			Assert.IsTrue( cache.ContainsKey( "c" ) );

			/////
			// Remove valid element.
			/////
			Assert.IsTrue( cache.Remove( "c" ) );

			Assert.AreEqual( cache.Count, 1 );

			Assert.IsTrue( cache.ContainsKey( "a" ) );
		}

		/// <summary>
		///     Tests the TryGetValue method of the cache.
		/// </summary>
		[Test]
		public void TestTryGetValue( )
		{
			var cache = CreateCache<string, string>( false );

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache.Add( "a", "A" );

			Assert.AreEqual( cache.Count, 1 );

			string val;

			/////
			// Attempt to obtain a non-existent element.
			/////
			Assert.IsFalse( cache.TryGetValue( "z", out val ) );

			Assert.AreEqual( cache.Count, 1 );

			/////
			// Obtain a valid element.
			/////
			Assert.IsTrue( cache.TryGetValue( "a", out val ) );
			Assert.AreEqual( val, "A" );

			Assert.AreEqual( cache.Count, 1 );

			cache.Add( "b", "B" );
			cache.Add( "c", "C" );

			Assert.AreEqual( cache.Count, 3 );

			/////
			// Obtain a valid element.
			/////
			Assert.IsTrue( cache.TryGetValue( "b", out val ) );
			Assert.AreEqual( val, "B" );

			Assert.AreEqual( cache.Count, 3 );

			/////
			// Obtain a valid element.
			/////
			Assert.IsTrue( cache.TryGetValue( "c", out val ) );
			Assert.AreEqual( val, "C" );

			Assert.AreEqual( cache.Count, 3 );

			Assert.IsTrue( cache.ContainsKey( "a" ) );
			Assert.IsTrue( cache.ContainsKey( "b" ) );
			Assert.IsTrue( cache.ContainsKey( "c" ) );
		}

		/// <summary>
		///     Tests the Values property of the cache.
		/// </summary>
		[Test]
		public void TestValues( )
		{
			var cache = CreateCache<string, string>(false);

			Assert.AreEqual( cache.Count, 0 );

			/////
			// Add a single element.
			/////
			cache.Add( "a", "A" );

			Assert.AreEqual( cache.Count, 1 );
            
			Assert.IsNotNull( cache.Values() );

            Assert.AreEqual(cache.Count, 1);
            Assert.AreEqual(cache.Values().ElementAt(0), "A");

			/////
			// Add more elements.
			/////
			cache.Add( "b", "B" );
			cache.Add( "c", "C" );

			Assert.AreEqual( cache.Count, 3 );

			Assert.IsNotNull( cache.Values() );

			Assert.AreEqual( cache.Count, 3 );

			Assert.IsTrue(cache.Values().Contains("A"));
            Assert.IsTrue(cache.Values().Contains("B"));
            Assert.IsTrue(cache.Values().Contains("C"));
		}

	    [Test]
	    public void Test_ItemsRemoved()
	    {
            ICache<string, string> cache;
	        string[] testKeys;
	        const int maxCacheSize = 2;
            List<string> itemsRemoved;

	        testKeys = new[] {"foo", "bar", "baz"};
            itemsRemoved = new List<string>();
            cache = CreateCache<string, string>(false, maxCacheSize);
            cache.ItemsRemoved += (sender, args) => itemsRemoved.AddRange(args.Items);

	        for(int i = 0; i < testKeys.Length; i++)
	        {
	            cache.Add(testKeys[i], testKeys[i]);

	            if (i >= maxCacheSize)
	            {
                    Assert.That(itemsRemoved, Is.EquivalentTo(testKeys.Take(i - maxCacheSize + 1)), "Not removed");
                }
	        }
	    }
	}
}