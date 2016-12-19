// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EDC.Annotations;
using ProtoBuf;

namespace EDC.Cache.Providers
{
	/// <summary>
	///     Base class implementation of a Least Recently Used cache.
	///     Values of type TValue are stored in entry objects of type LastAccessedEntry and are uniquely identified
	///     by a key of type TKey. The entry is used to stored additional meta data describing the value.
	///     The cache will store a maximum of <seealso cref="MaximumSize" /> entries at any given time.
	///     If an attempt to add more than <seealso cref="MaximumSize" /> elements to the cache is made, the
	///     least recently used element(s) are evicted on a background thread. It is possible for the
	///     count exceed <seealso cref="MaximumSize" /> between eviction runs.
	///     Individual lookup speed ( this[ ], TryGetValue ) is O(1).
	///     Multiple lookup speed ( Keys, Values, Count ) is O(n).
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
	public class LruCache<TKey, TValue> : ICache<TKey, TValue>
	{
        /// <summary>
        ///     Default maximum cache size. (10000)
        /// </summary>
        public const int DefaultMaximumCacheSize = 10000;

	    /// <summary>
	    ///     The inner cache
	    /// </summary>
		private readonly ICache<TKey, LastAccessedEntry> _cache;

		/// <summary>
		/// Thread synchronization
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		/// The cache size
		/// </summary>
		private int _cacheSize;

		/// <summary>
		/// The eviction thread
		/// </summary>
		private volatile Thread _evictionThread;

		/// <summary>
		/// The eviction timer
		/// </summary>
		[UsedImplicitly]
		private Timer _evictionTimer;

		/// <summary>
		/// The eviction event
		/// </summary>
		private ManualResetEvent _evictionEvent;

		/// <summary>
		///     Protected constructor called from derived classes.
		/// </summary>
        /// <param name="innerCache">The cache being wrapped. This cannot be null.</param>
		/// <param name="cacheName">Name of the cache.</param>
        /// <param name="maxSize">The maximum number of elements the cache can hold.</param>
		/// <param name="evictionFrequency">The eviction frequency.</param>
		/// <remarks>
		///     If the cache is transaction aware, then any objects that are added or retrieved
		///     from the cache during a transaction are removed if the transaction is rolled back.
		///     If the cache is used to cache database objects consider making the cache transaction aware.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="innerCache"/> cannot be null.
		/// </exception>
		public LruCache( ICache<TKey, LastAccessedEntry> innerCache, string cacheName, int maxSize = DefaultMaximumCacheSize, TimeSpan? evictionFrequency = null )
		{
		    if (innerCache == null)
		    {
		        throw new ArgumentNullException( nameof( innerCache ) );
		    }

            _cache = innerCache;
			MaximumSize = maxSize;
			CacheName = cacheName;

			EvictionFrequency = evictionFrequency == null ? TimeSpan.FromSeconds( 5 ) : evictionFrequency.Value;
		}

		/// <summary>
		/// Gets the eviction frequency.
		/// </summary>
		/// <value>
		/// The eviction frequency.
		/// </value>
		public TimeSpan EvictionFrequency
		{
			get;
		}

		/// <summary>
		/// The cache name.
		/// </summary>
		public string CacheName
		{
			get;
		}


	    /// <summary>
	    ///     Gets the maximum number of entries the cache can hold. Once this value is reached, each
	    ///     additional entry replaces the least recently used entry.
	    /// </summary>
	    public int MaximumSize
	    {
			get;
	    }

		/// <summary>
		/// Increments the size of the cache.
		/// </summary>
		/// <param name="count">The count.</param>
		private void IncrementCacheSize( int count = 1 )
		{
			int size = Interlocked.Add( ref _cacheSize, count );

			if ( size > MaximumSize )
			{
				if ( _evictionThread == null )
				{
					lock ( _syncRoot )
					{
						if ( _evictionThread == null )
						{
							_evictionEvent = new ManualResetEvent( false );

							var thread = new Thread( Evict )
							{
								IsBackground = true,
								Name = CacheName + " LRU Eviction Thread"
							};

							thread.Start( );

							_evictionThread = thread;

							_evictionTimer = new Timer( s =>
							{
								try
								{
									if ( _cacheSize > MaximumSize )
									{
										_evictionEvent.Set( );
									}
								}
								catch( Exception exc )
								{
									Debug.WriteLine( "LRU eviction timer encountered and error. " + exc );
								}
							}, null, 0, ( long ) EvictionFrequency.TotalMilliseconds );
						}
					}
				}

				
			}
		}

		/// <summary>
		/// Eviction routine
		/// </summary>
		/// <param name="state">The state.</param>
		private void Evict( object state )
		{
			while ( true )
			{
				try
				{
					_evictionEvent.WaitOne( );

					Debug.WriteLine( CacheName + " LRU eviction starting..." );

					int excess = _cacheSize - MaximumSize;

					if ( excess <= 0 )
					{
						continue;
					}

					var sw = new Stopwatch( );

					sw.Start( );

					/////
					// This will evict back to the maximum size (on this pass) keeping in mind that other threads can
					// concurrently be adding new items in the background.
					/////
					List<TKey> keys = _cache.OrderBy( p => p.Value.LastAccess ).Take( excess ).Select( p => p.Key ).ToList( );

					int count = 0;

					foreach ( TKey key in keys )
					{
						if ( Remove( key ) )
						{
							count++;
						}
					}

					sw.Stop( );

					Debug.WriteLine( CacheName + " LRU eviction took " + sw.ElapsedMilliseconds + "ms. " + count + " items evicted." );
				}
				catch ( ThreadAbortException )
				{
					break;
				}
				catch ( Exception exc )
				{
					Debug.WriteLine( CacheName + " LRU eviction thread encountered an error. " + exc );
				}
				finally
				{
					_evictionEvent.Reset( );
				}
			}
		}

		/// <summary>
		/// Decrements the size of the cache.
		/// </summary>
		/// <param name="count">The count.</param>
		private void DecrementCacheSize( int count = 1 )
		{
			Interlocked.Add( ref _cacheSize, -count );
		}

		/// <summary>
		/// Resets the size of the cache.
		/// </summary>
		private void ResetCacheSize( )
		{
			Interlocked.Exchange( ref _cacheSize, 0 );
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
            if (key == null)
            {
                throw new ArgumentNullException( nameof( key ) );
            }

			bool added = _cache.Add( key, LastAccessedEntry.Create( value ) );

			if ( added )
			{
				IncrementCacheSize( );
			}

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
			{
				throw new ArgumentNullException( nameof( valueFactory ) );
			}

			Func<TKey, LastAccessedEntry> factory = k =>
			{
				TValue val = valueFactory( key );

				return LastAccessedEntry.Create( val );
			};

			LastAccessedEntry lastAccessed;

			bool cameFromCache = _cache.TryGetOrAdd( key, out lastAccessed, factory );

			if ( ! cameFromCache )
			{
				IncrementCacheSize( );
			}
			else
			{
				lastAccessed.LastAccess = DateTime.UtcNow.Ticks;
			}

			value = lastAccessed.Value;

			return cameFromCache;
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
			bool removed = _cache.Remove( key );

			if ( removed )
			{
				DecrementCacheSize( );
			}

			return removed;
		}


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
	        if ( keys == null )
	        {
		        throw new ArgumentNullException( nameof( keys ) );
	        }

	        TKey [ ] keyArray = keys.ToArray( );

			List<TKey> removed = keyArray.Where( Remove ).ToList( );

	        if ( removed.Count > 0 )
	        {
		        DecrementCacheSize( removed.Count );
	        }

	        return removed;
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
			LastAccessedEntry lastAccessed;

		    bool found = _cache.TryGetValue(key, out lastAccessed);

			if ( found )
			{
				value = lastAccessed.Value;
				lastAccessed.LastAccess = DateTime.UtcNow.Ticks;
			}
			else
			{
				value = default( TValue );
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
		public TValue this[ TKey key ]
		{
			get
			{
				LastAccessedEntry value;

				bool found = _cache.TryGetValue( key, out value );

				if ( found )
				{
					value.LastAccess = DateTime.UtcNow.Ticks;

					return value.Value;
				}

				return default( TValue );
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
			lock ( _syncRoot )
			{
				_cache.Clear( );

				ResetCacheSize( );
			}
		}

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var col = _cache.Select(pair => new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Value));

            return col.GetEnumerator();
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


		/// <summary>
		///     Base class implementation of the Least Recently Used cache entry.
		///     This class encapsulates the value as well as any additional meta data associated with the entry.
		/// </summary>
		[ProtoContract]
		public class LastAccessedEntry
		{
			/// <summary>
			/// Prevents a default instance of the <see cref="LastAccessedEntry"/> class from being created.
			/// </summary>
			private LastAccessedEntry( )
			{

			}

			/// <summary>
			///     Protected constructor that derived classes call.
			/// </summary>
			private LastAccessedEntry( TValue value )
				: this( )
			{
				Value = value;
				LastAccess = DateTime.UtcNow.Ticks;
			}

			/// <summary>
			///     Gets the value associated with this entry in the cache.
			/// </summary>
			[ProtoMember( 1 )]
			public TValue Value
			{
				get;
                set;
			}

			/// <summary>
			/// Gets or sets the last access.
			/// </summary>
			/// <value>
			/// The last access.
			/// </value>
			[ProtoMember( 2 )]
			public long LastAccess
			{
				get;
				set;
			}

			/// <summary>
			/// Creates the specified value.
			/// </summary>
			/// <param name="value">The value.</param>
			/// <returns></returns>
			public static LastAccessedEntry Create( TValue value )
			{
				return new LastAccessedEntry( value );
			}
		}

		/// <summary>
        ///     Returns the number of entries in the cache.
        /// </summary>
        public int Count
        {
            get { return _cache.Count; }
        }

        /// <summary>
        /// Raised when items are removed from the set. Note, this may be called
        /// after the itmes are already removed.
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

    }



    /// <summary>
    /// Creates an LRU cache
    /// </summary>
    public class LruCacheFactory : ICacheFactory
    {
        /// <summary>
        /// The factory for the inner cache.
        /// </summary>
        public ICacheFactory Inner { get; set; }

        /// <summary>
        /// Is this cache thread-safe.
        /// </summary>
        public bool ThreadSafe
        {
            get { return Inner.ThreadSafe; }
        }

        /// <summary>
        /// The maximum size of the created cache.
        /// </summary>
        public int MaxSize { get; set; }

		/// <summary>
		/// Gets or sets the eviction frequency.
		/// </summary>
		/// <value>
		/// The eviction frequency.
		/// </value>
	    public TimeSpan? EvictionFrequency
	    {
		    get;
		    set;
	    }

        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
			ICache<TKey, LruCache<TKey, TValue>.LastAccessedEntry> innerCache = Inner.Create<TKey, LruCache<TKey, TValue>.LastAccessedEntry>( cacheName );

		    return new LruCache<TKey, TValue>( innerCache, cacheName, MaxSize <= 0 ? LruCache<TKey, TValue>.DefaultMaximumCacheSize : MaxSize, EvictionFrequency );
        }
    }

}