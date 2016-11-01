// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;
using EDC.Database;
using NUnit.Framework;

namespace EDC.Test.Database
{
	/// <summary>
	///     This is the test case for the CacheTransactionEventNotificationManager class.
	/// </summary>
	[TestFixture]
	public class TransactionEventNotificationManagerTests
	{
		/// <summary>
		/// </summary>
		private class TestCache : ITransactionEventNotification
		{
			private readonly Dictionary<string, HashSet<string>> _cachedItems = new Dictionary<string, HashSet<string>>( );
			private readonly object _syncRoot = new object( );

			/// <summary>
			/// </summary>
			/// <param name="transactionEvent"></param>
			void ITransactionEventNotification.OnTransactionEvent( TransactionEventNotificationArgs transactionEvent )
			{
				lock ( _syncRoot )
				{
					switch ( transactionEvent.EventType )
					{
						case TransactionEventType.Commit:
							// Remove the cache from the list of caches
							HashSet<string> cache;
							if ( _cachedItems.TryGetValue( transactionEvent.Transactionid, out cache ) )
							{
								_cachedItems.Remove( transactionEvent.Transactionid );

								// Commit all the data from the transaction specific cache
								// to the default cache.
								foreach ( string s in cache )
								{
									HashSet<string> defaultCache = GetCache( true );
									defaultCache.Add( s );
								}
							}
							break;

						case TransactionEventType.Rollback:
							_cachedItems.Remove( transactionEvent.Transactionid );
							break;
					}
				}
			}


			/// <summary>
			///     Adds an item to the cache
			/// </summary>
			/// <param name="key"></param>
			public void AddItem( string key )
			{
				lock ( _syncRoot )
				{
					HashSet<string> transCache = GetCache( false );
					if ( transCache != null )
					{
						transCache.Add( key );
					}
				}
			}

			/// <summary>
			///     True if the cache contains the item false otherwise.
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public bool Contains( string key )
			{
				lock ( _syncRoot )
				{
					HashSet<string> transCache = GetCache( false );
					if ( transCache != null )
					{
						if ( transCache.Contains( key ) )
						{
							return true;
						}
					}

					HashSet<string> defaultCache = GetCache( true );
					if ( defaultCache != null )
					{
						if ( defaultCache.Contains( key ) )
						{
							return true;
						}
					}

					return false;
				}
			}


			/// <summary>
			///     Get the per transaction cache.
			/// </summary>
			/// <param name="defaultCache">True to get the default cache, false to get the transaction specified copy.</param>
			/// <returns></returns>
			private HashSet<string> GetCache( bool defaultCache )
			{
				HashSet<string> cache;
				string cacheKey = "DEFAULT";

				lock ( _syncRoot )
				{
					if ( !defaultCache )
					{
						// The cache is transaction aware and we are in a transaction.
						if ( Transaction.Current != null )
						{
							cacheKey = Transaction.Current.TransactionInformation.LocalIdentifier;
						}
					}

					if ( !_cachedItems.TryGetValue( cacheKey, out cache ) )
					{
						cache = new HashSet<string>( );
						_cachedItems[ cacheKey ] = cache;
					}
				}

				return cache;
			}
		}

		/// <summary>
		///     Test creating a CacheTransactionEventNotificationManager with a null
		///     cache
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void CacheTransactionEventNotificationManagerNullCache( )
		{
// ReSharper disable ObjectCreationAsStatement
			new TransactionEventNotificationManager( null );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Test that clearing the transaction event notifiers
		///     does not respond to transaction events.
		/// </summary>
		[Test]
		public void ClearTransactionEventNotifiersTest( )
		{
			var testCache = new TestCache( );

			using ( var scope = new TransactionScope( TransactionScopeOption.Required ) )
			{
				var cacheManager = new TransactionEventNotificationManager( testCache );

				// Add some items to the cache while inside a transaction
				testCache.AddItem( "A" );
				cacheManager.EnlistTransaction( Transaction.Current );

				testCache.AddItem( "B" );
				cacheManager.EnlistTransaction( Transaction.Current );

				testCache.AddItem( "C" );
				cacheManager.EnlistTransaction( Transaction.Current );

				// Clear the event notifiers
				cacheManager.ClearTransactionEventNotifiers( );

				// Commit the transaction
				// as the. As the event notifiers have been removed
				// the items will not be commited
				scope.Complete( );
			}

			Assert.IsFalse( testCache.Contains( "A" ) );
			Assert.IsFalse( testCache.Contains( "B" ) );
			Assert.IsFalse( testCache.Contains( "C" ) );
		}


		/// <summary>
		///     Test that a commit keeps items in the cache.
		/// </summary>
		[Test]
		public void EnlistCommitTransactionTest( )
		{
			var testCache = new TestCache( );

			using ( var scope = new TransactionScope( TransactionScopeOption.Required ) )
			{
				var cacheManager = new TransactionEventNotificationManager( testCache );

				// Add some items to the cache while inside a transaction
				testCache.AddItem( "A" );
				cacheManager.EnlistTransaction( Transaction.Current );

				testCache.AddItem( "B" );
				cacheManager.EnlistTransaction( Transaction.Current );

				testCache.AddItem( "C" );
				cacheManager.EnlistTransaction( Transaction.Current );

				// Commit the transaction
				scope.Complete( );
			}

			Assert.IsTrue( testCache.Contains( "A" ) );
			Assert.IsTrue( testCache.Contains( "B" ) );
			Assert.IsTrue( testCache.Contains( "C" ) );
		}

		/// <summary>
		///     Test Enlisting a null transaction.
		/// </summary>
		[Test]
		public void EnlistNullTransactionTest( )
		{
			var testCache = new TestCache( );

			var cacheManager = new TransactionEventNotificationManager( testCache );
			cacheManager.EnlistTransaction( null );
		}


		/// <summary>
		///     Test that a rollback rolls back items in the cache.
		/// </summary>
		[Test]
		public void EnlistRolledbackTransactionTest( )
		{
			var testCache = new TestCache( );

			using ( new TransactionScope( TransactionScopeOption.Required ) )
			{
				var cacheManager = new TransactionEventNotificationManager( testCache );

				// Add some items to the cache while inside a transaction
				testCache.AddItem( "A" );
				cacheManager.EnlistTransaction( Transaction.Current );

				testCache.AddItem( "B" );
				cacheManager.EnlistTransaction( Transaction.Current );

				testCache.AddItem( "C" );
				cacheManager.EnlistTransaction( Transaction.Current );

				// Roll back transaction                
			}

			Assert.IsFalse( testCache.Contains( "A" ) );
			Assert.IsFalse( testCache.Contains( "B" ) );
			Assert.IsFalse( testCache.Contains( "C" ) );
		}


		/// <summary>
		///     Test that clearing the transaction event notifiers
		///     does not respond to transaction events.
		/// </summary>
		[Test]
		public void TransactionExceedingTimeoutTest( )
		{
			var testCache = new TestCache( );

			using ( var scope = new TransactionScope( TransactionScopeOption.Required ) )
			{
				// Create a notification manager with a time out interval of 1 seconds.
				var cacheManager = new TransactionEventNotificationManager( testCache, 1 );

				// Add an items to the cache while inside a transaction
				testCache.AddItem( "A" );
				cacheManager.EnlistTransaction( Transaction.Current );

				testCache.AddItem( "B" );
				cacheManager.EnlistTransaction( Transaction.Current );

				// Simulate a transaction taking longer than the timeout interval.
				Thread.Sleep( 1500 );

				testCache.AddItem( "C" );
				cacheManager.EnlistTransaction( Transaction.Current );

				scope.Complete( );

				// As the transaction exceeded the timeout, its data will not
				// be committed
			}

			Assert.IsFalse( testCache.Contains( "A" ) );
			Assert.IsFalse( testCache.Contains( "B" ) );
			Assert.IsFalse( testCache.Contains( "C" ) );
		}
	}
}