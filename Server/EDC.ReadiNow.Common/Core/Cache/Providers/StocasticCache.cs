// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using System.Collections.Concurrent;
using System.Threading;
using EDC.ReadiNow.Diagnostics;
using ProtoBuf;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	/// Stochastic cache. Fixed number of slots. Each addition randomly picks a location to displace.
	/// This cache will perform equally well regardless of size.
	/// </summary>
	/// <typeparam name="TKey">
	///     The type of key value used to uniquely identify elements in this cache.
	/// </typeparam>
	/// <typeparam name="TValue">
	///     The type of value stored in the cache.
	/// </typeparam>
	/// <remarks>
	///     This class is not thread-safe. Use in conjunction with ThreadSafeCache if required.
	/// </remarks>
	public class StochasticCache<TKey, TValue> : ICache<TKey, TValue>
	{
		/// <summary>
		/// Note: used so we can specify a specific seed (of 0) so that we get deterministic behavior during automated tests of the StochasticCache
		/// </summary>
		internal static bool IsTestMode;

		/// <summary>
		/// Random is not thread safe. Making it thread local so that each thread gets it's own instance without needing a lock.        
		/// Using a random seed during normal usage so different threads use different seeds.
		/// </summary>        
		private ThreadLocal<Random> _random = new ThreadLocal<Random>( ( ) => new Random( IsTestMode ? 0 : Guid.NewGuid( ).GetHashCode( ) ) );

		/// <summary>
		/// Using a long here so we can use InterLocked methods to read/write.
		/// </summary>
		long _areFull;      // has the cache been filled yet?

		int _fillIndex;         // used when initially filling the cache

		long _countSlots;   // The actual number of slots.

		/// <summary>
		///     Default maximum cache size. (10000)
		/// </summary>
		private const int DefaultMaximumCacheSize = 10000;

		private ConcurrentDictionary<int, TKey> _slots;

		/// <summary>
		///     The inner cache
		/// </summary>
		private readonly ICache<TKey, TimestampedValue> _cache;

		/// <summary>
		/// Protected constructor called from derived classes.
		/// </summary>
		/// <param name="innerCache">The cache being wrapped. This cannot be null.</param>
		/// <param name="cacheName">Name of the cache.</param>
		/// <param name="maxSize">The maximum number of elements the cache can hold.</param>
		/// <param name="numHistoryFetches">The number of values that will be compared the find the oldest when throwing out a value.
		/// The default of 5 gives roughly the same performance as LRU when the cache is 20% the size of the key space and 20% of the key space is accessed 80% of the time.</param>
		/// <exception cref="System.ArgumentNullException">innerCache</exception>
		/// <exception cref="System.ArgumentException">maxSize</exception>
		/// <exception cref="ArgumentNullException"><paramref name="innerCache" /> cannot be null.</exception>
		/// <remarks>
		/// If the cache is transaction aware, then any objects that are added or retrieved
		/// from the cache during a transaction are removed if the transaction is rolled back.
		/// If the cache is used to cache database objects consider making the cache transaction aware.
		/// </remarks>
		public StochasticCache( ICache<TKey, TimestampedValue> innerCache, string cacheName, int maxSize = DefaultMaximumCacheSize, int numHistoryFetches = 5 )
		{
			if ( innerCache == null )
			{
				throw new ArgumentNullException( "innerCache" );
			}

			if ( maxSize < 0 )
			{
				throw new ArgumentException( "maxSize" );
			}

			EventLog.Application.WriteWarning( "StochasticCache '" + ( cacheName ?? "Unnamed" ) + "' detected. Consider using an LRU cache instead." );

			_cache = innerCache;
			_slots = CreateSlotsDictionary( maxSize );
			MaximumSize = maxSize;
			NumHistoryFetches = numHistoryFetches;
			CacheName = cacheName;
		}

		/// <summary>
		/// The cache name.
		/// </summary>
		public string CacheName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the maximum number of entries the cache can hold. Once this value is reached, each
		///     additional entry replaces the least recently used entry.
		/// </summary>
		public int MaximumSize
		{
			get;
			private set;
		}

		/// <summary>
		///     The number of values that will be compared to find the oldest when the cache is full.
		/// </summary>
		public int NumHistoryFetches
		{
			get;
			private set;
		}

		/// <summary>
		///     Adds or updates the specified key/value pair.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     True if the specified key/value pair was added; 
		///     False if the specified key/value pair was updated.
		/// </returns>
		public bool Add( TKey key, TValue value )
		{
			bool added = _cache.Add( key, new TimestampedValue( value ) );

			FillOrReplaceSlot( key );

			return added;
		}

		/// <summary>
		/// Attempts to get a value from cache.
		/// If it is found in cache, return true, and the value.
		/// If it is not found in cache, return false, and use the valueFactory callback to generate and return the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="valueFactory">A callback that can create the value.</param>
		/// <returns>
		/// True if the value came from cache, otherwise false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">valueFactory</exception>
		public bool TryGetOrAdd( TKey key, out TValue value, Func<TKey, TValue> valueFactory )
		{
			if ( valueFactory == null )
				throw new ArgumentNullException( "valueFactory" );

			TimestampedValue tsValue;

			bool found = _cache.TryGetOrAdd( key, out tsValue, key_ => new TimestampedValue( valueFactory( key_ ) ) );

			value = tsValue == null ? default( TValue ) : tsValue.Value;

			if ( found )
			{
				UpdateTimestamp( tsValue );
			}
			else
			{
				FillOrReplaceSlot( key );
			}

			return found;
		}

		private static void UpdateTimestamp( TimestampedValue tsValue )
		{
			tsValue.Timestamp = DateTime.UtcNow;     // no need to lock because at worse another thread will update with only a small difference in time stamp.
		}

		/// <summary>
		/// Find a slot and flush it of any old value
		/// </summary>
		/// <param name="key"></param>
		private void FillOrReplaceSlot( TKey key )
		{
			long areFull = Interlocked.Read( ref _areFull );
			if ( areFull == 1 )
			{
				ReplaceSlot( key );
			}
			else
			{
				areFull = 0;

				int fillIndexInc = Interlocked.Increment( ref _fillIndex );
				int diff = 0;

				if ( fillIndexInc >= MaximumSize )
				{
					diff = fillIndexInc - MaximumSize;
					areFull = Interlocked.Exchange( ref _areFull, 1 );
				}

				if ( areFull == 1 )
				{
					ReplaceSlot( key );
				}
				else
				{
					_slots [ fillIndexInc - ( 1 + diff ) ] = key;
					Interlocked.Increment( ref _countSlots );
				}
			}
		}

		private void ReplaceSlot( TKey key )
		{
			int oldestIndex = 0;

			long countSlots = Interlocked.Read( ref _countSlots );

			if ( countSlots == 0 )
			{
				throw new InvalidOperationException( "The number of available slots should be greater than zero." );
			}

			bool removed = false;

			int removeCounter = 0;

			while ( !removed && removeCounter <= 5 )
			{
				TKey oldestKey;

				FindOldestIndexKey( ( int ) countSlots, out oldestIndex, out oldestKey );

				if ( oldestKey != null )
				{
					using ( new RedisCacheMessageSuppressionContext( CacheName ) )
					{
						removed = _cache.Remove( oldestKey );

						if ( !removed )
						{
							removeCounter++;
						}
					}
				}
			}

			_slots [ oldestIndex ] = key;
		}

		private void FindOldestIndexKey( int countSlots, out int oldestIndex, out TKey oldestKey )
		{
			DateTime oldestTs;

			GetRandKeyTsForIndex( countSlots, out oldestIndex, out oldestKey, out oldestTs );

			for ( int i = 0; i < NumHistoryFetches; i++ )
			{
				int testIndex;
				TKey testKey;
				DateTime testTs;

				GetRandKeyTsForIndex( countSlots, out testIndex, out testKey, out testTs );

				if ( testTs < oldestTs )
				{
					oldestIndex = testIndex;
					oldestKey = testKey;
					oldestTs = testTs;
				}
			}
		}

		/// <summary>
		/// Get a random index and corresponding key along with it's time stamp. If the cache does not have the value the time stamp will be DateTime.Max.
		/// </summary>
		private void GetRandKeyTsForIndex( int countSlots, out int index, out TKey key, out DateTime timestamp )
		{
			index = _random.Value.Next( countSlots );

			bool foundKey = _slots.TryGetValue( index, out key );

			TimestampedValue tsValue;

			if ( foundKey &&
				key != null &&
				_cache.TryGetValue( key, out tsValue ) )
			{
				timestamp = tsValue.Timestamp;
			}
			else
			{
				timestamp = DateTime.MinValue;      // Looks like the item has already been removed from the cache
			}
		}


		/// <summary>
		///     Remove the entry from the cache with the specified key.
		/// </summary>
		/// <param name="key">
		///     Unique identifier of the entry to be removed from the cache.
		/// </param>
		/// <returns>
		///     True if the specified entry was removed from the cache; False otherwise.
		/// </returns>
		public bool Remove( TKey key )
		{
			return _cache.Remove( key );


			// we can ignore the slot. That will eventually be flushed.
		}


		/// <summary>
		/// Remove entries from the cache.
		/// </summary>
		public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
		{
			if ( keys == null )
				throw new ArgumentNullException( "keys" );

			return _cache.Remove( keys );

			// we can ignore the slots. They will eventually be flushed.
		}


		/// <summary>
		///     Retrieve a value from the cache if it exists.
		/// </summary>
		/// <param name="key">
		///     Key of the entry to be retrieved.
		/// </param>
		/// <param name="value">
		///     Value retrieved from the cache if the specified key was found; Null otherwise.
		/// </param>
		/// <returns>
		///     True if the specified entry was retrieved from the cache; False otherwise.
		/// </returns>
		public bool TryGetValue( TKey key, out TValue value )
		{
			TimestampedValue tsValue;

			var found = _cache.TryGetValue( key, out tsValue );

			value = tsValue == null ? default( TValue ) : tsValue.Value;

			if ( found )
			{
				UpdateTimestamp( tsValue );
			}

			return found;
		}

		/// <summary>
		///     Gets/Sets the specified entry within the cache.
		/// </summary>
		/// <param name="key">
		///     Uniquely identifies the entry to retrieve or store in the cache.
		/// </param>
		/// <returns>
		///     The specified value if found within the cache; default value otherwise.
		/// </returns>
		public TValue this [ TKey key ]
		{
			get
			{
				return this.Get( key );
			}
			set
			{
				Add( key, value );
			}
		}

		/// <summary>
		///     Clears all entries from the cache.
		/// </summary>
		public void Clear( )
		{
			_cache.Clear( );
			_slots = CreateSlotsDictionary( MaximumSize );
			Interlocked.Exchange( ref _areFull, 0 );
			Interlocked.Exchange( ref _fillIndex, 0 );
			Interlocked.Exchange( ref _countSlots, 0 );
		}

		/// <summary>
		/// GetEnumerator
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator( )
		{
			var col = _cache.Select( pair => new KeyValuePair<TKey, TValue>( pair.Key, pair.Value.Value ) );
			return col.GetEnumerator( );
		}


		/// <summary>
		/// GetEnumerator
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return GetEnumerator( );
		}


		/// <summary>
		///     Returns the number of entries in the cache.
		/// </summary>
		public int Count
		{
			get
			{
				return _cache.Count;
			}
		}

		/// <summary>
		/// Raised when items are removed from the set. Note, this may be called
		/// after the items are already removed.
		/// </summary>
		public event ItemsRemovedEventHandler<TKey> ItemsRemoved
		{
			add
			{
				_cache.ItemsRemoved += value;
			}
			remove
			{
				_cache.ItemsRemoved -= value;
			}
		}

		[ProtoContract]
		public class TimestampedValue
		{
			private TimestampedValue( )
			{
			}

			[ProtoMember( 2 )]
			public TValue Value
			{
				get;
				private set;
			}

			[ProtoMember( 1 )]
			public DateTime Timestamp;

			public TimestampedValue( TValue val )
				: this( )
			{
				Value = val;
				Timestamp = DateTime.UtcNow;
			}
		}


		/// <summary>
		/// Create a preallocated concurrent dictionary to avoid future resizing.    
		/// </summary>
		/// <param name="maxSize"></param>
		/// <returns></returns>
		private ConcurrentDictionary<int, TKey> CreateSlotsDictionary( int maxSize )
		{
			return new ConcurrentDictionary<int, TKey>( Enumerable.Range( 0, maxSize ).Select( k => new KeyValuePair<int, TKey>( k, default( TKey ) ) ) );
		}
	}



	/// <summary>
	/// Creates an stochastic cache
	/// </summary>
	public class StochasticCacheFactory : ICacheFactory
	{
		/// <summary>
		/// The factory for the inner cache.
		/// </summary>
		public ICacheFactory Inner
		{
			get;
			set;
		}

		/// <summary>
		/// Is this cache thread-safe.
		/// </summary>
		public bool ThreadSafe
		{
			get
			{
				return Inner.ThreadSafe;
			}
		}

		/// <summary>
		/// The maximum size of the created cache.
		/// </summary>
		public int MaxSize
		{
			get;
			set;
		}

		/// <summary>
		///     The number of values that will be compared to find the oldest when the cache is full.
		/// </summary>
		public int NumHistoryFetches
		{
			get;
			set;
		}

		/// <summary>
		/// Create a new <see cref="StochasticCacheFactory"/>.
		/// </summary>
		public StochasticCacheFactory( )
		{
			NumHistoryFetches = 1;
		}

        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
		{
			var innerCache = Inner.Create<TKey, StochasticCache<TKey, TValue>.TimestampedValue>( cacheName );

			if ( MaxSize == 0 )
			{
				return new StochasticCache<TKey, TValue>( innerCache, cacheName );
			}

			return new StochasticCache<TKey, TValue>( innerCache, cacheName, MaxSize, NumHistoryFetches );
		}


	}

}