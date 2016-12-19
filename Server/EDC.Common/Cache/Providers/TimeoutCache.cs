// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EDC.Annotations;
using ProtoBuf;

namespace EDC.Cache.Providers
{
    /// <summary>
    ///     A simple timeout based cache.
    /// </summary>
    /// <remarks>
    ///     This class is not thread-safe. Use in conjunction with ThreadSafeCache if required.
    /// </remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class TimeoutCache<TKey, TValue> : ICache<TKey, TValue>
    {
	    /// <summary>
        /// The cache being wrapped.
        /// </summary>
		private readonly ICache<TKey, TimeoutEntry> _cache;

        /// <summary>
        /// The timeout interval
        /// </summary>
        private readonly long _expirationInterval;

        /// <summary>
        /// Eviction synchronization
        /// </summary>
        private readonly object _evictionSync = new object( );

		/// <summary>
		/// The eviction timer
		/// </summary>
		[UsedImplicitly]
		private volatile Timer _evictionTimer;

	    /// <summary>
	    /// Make an existing cache thread safe.
	    /// </summary>
	    /// <param name="innerCache">
	    /// The cache being wrapped. This cannot be null.
	    /// </param>
		/// <param name="cacheName">
		/// Name of the cache.
		/// </param>
	    /// <param name="expirationInterval">
	    /// The maximum age of a cache entry.
	    /// </param>
	    /// <param name="evictionFrequency">
	    /// The eviction frequency.
	    /// </param>
	    /// <exception cref="ArgumentNullException">
	    /// <paramref name="innerCache"/> cannot be null.
	    /// </exception>
		public TimeoutCache( ICache<TKey, TimeoutEntry> innerCache, string cacheName, TimeSpan expirationInterval, TimeSpan? evictionFrequency = null )
	    {
		    if ( innerCache == null )
		    {
			    throw new ArgumentNullException( nameof( innerCache ) );
		    }

		    if ( expirationInterval == TimeSpan.Zero )
		    {
			    throw new ArgumentException( @"Invalid expiration interval", nameof( expirationInterval ) );
		    }

		    if ( evictionFrequency != null && evictionFrequency.Value == TimeSpan.Zero )
		    {
				throw new ArgumentException( @"Invalid eviction frequency", nameof( evictionFrequency ) );
		    }

		    _cache = innerCache;
			CacheName = cacheName;
		    _expirationInterval = expirationInterval.Ticks;

		    TimeSpan frequency = evictionFrequency ?? expirationInterval;

			_evictionTimer = new Timer( Evict, null, ( int ) frequency.TotalMilliseconds, ( int ) frequency.TotalMilliseconds );
	    }

		/// <summary>
		/// Evicts the entries that are expired.
		/// </summary>
		/// <param name="state">The state.</param>
	    private void Evict( object state )
	    {
		    try
		    {
			    lock ( _evictionSync )
			    {
				    var sw = new Stopwatch( );

				    sw.Start( );

				    long currentTime = DateTime.UtcNow.Ticks;

				    /////
				    // This will evict entries that have expired at this point in time keeping in mind that other entries could
				    // expire while this is running in which case they will be evicted next run.
				    /////
				    List<TKey> keys = _cache.Where( p => p.Value.CreationTime + _expirationInterval < currentTime ).Select( p => p.Key ).ToList( );

				    int count = 0;

				    foreach ( TKey key in keys )
				    {
					    if ( Remove( key ) )
					    {
						    count++;
					    }
				    }

				    sw.Stop( );

				    Debug.WriteLine( CacheName + " timeout eviction took " + sw.ElapsedMilliseconds + "ms. " + count + " items evicted." );
			    }
		    }
		    catch ( Exception exc )
		    {
				Debug.WriteLine( CacheName + " timeout eviction thread encountered an error. " + exc );
		    }
	    }

		/// <summary>
		/// The cache name.
		/// </summary>
		public string CacheName
		{
			get;
		}

	    /// <summary>
        /// Access a cache entry.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                Add(key, value);
            }
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
        public bool Add(TKey key, TValue value)
        {
            return _cache.Add(key, TimeoutEntry.Create(value));
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
        public bool TryGetOrAdd(TKey key, out TValue value, Func<TKey, TValue> valueFactory)
        {
			if ( valueFactory == null )
			{
				throw new ArgumentNullException( nameof( valueFactory ) );
			}

			Func<TKey, TimeoutEntry> callback = key1 =>
            {
                TValue newValue = valueFactory(key1);

				var newEntry = TimeoutEntry.Create( newValue );

                return newEntry;
            };

			TimeoutEntry entry = null;

			bool isValid = false;
			bool fromCache = false;

			while ( ! isValid )
			{
				fromCache = _cache.TryGetOrAdd( key, out entry, callback );

				/////
				// Check time stamp
				/////
				if ( fromCache )
				{
					isValid = IsValid( entry );

					if ( !isValid )
					{
						Remove( key );
					}
				}
				else
				{
					isValid = true;
				}
			}

			value = entry.Value;

            return fromCache;
        }


        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }


        /// <summary>
        /// Remove an entry from the cache.
        /// </summary>
        public bool Remove(TKey key)
        {
            return _cache.Remove(key);
        }


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove(IEnumerable<TKey> keys)
        {
            return _cache.Remove(keys);
        }


        /// <summary>
        /// TryGetValue
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            // Check underlying cache
            TimeoutEntry entry;

            bool found = _cache.TryGetValue(key, out entry);

            // Check time stamp
            if (found)
            {
                if (!IsValid(entry))
                {
                    found = false;
                }
            }

            // Return result
            value = found ? entry.Value : default(TValue);
            return found;
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            long threshold = DateTime.UtcNow.Ticks - _expirationInterval;

            var pairs = _cache.Where(p => p.Value.CreationTime > threshold).Select(p => new KeyValuePair<TKey, TValue>(p.Key, p.Value.Value));
            return pairs.GetEnumerator();
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            long threshold = DateTime.UtcNow.Ticks - _expirationInterval;

            var pairs = _cache.Where(p => p.Value.CreationTime > threshold).Select(p => new KeyValuePair<TKey, TValue>(p.Key, p.Value.Value));
            System.Collections.IEnumerable ienum = pairs;
            return ienum.GetEnumerator();
        }


        /// <summary>
        ///     Returns the number of entries in the cache.
        ///     Caution: Cost is O(N)
        /// </summary>
        public int Count
        {
            get
            {
                long threshold = DateTime.UtcNow.Ticks - _expirationInterval;

                return _cache.Count(p => p.Value.CreationTime > threshold);
            }
        }


	    /// <summary>
	    /// Is the entry still valid.
	    /// </summary>
	    /// <param name="entry"></param>
	    /// <returns></returns>
	    private bool IsValid( TimeoutEntry entry )
	    {
		    return entry.CreationTime + _expirationInterval > DateTime.UtcNow.Ticks;
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

        /// <summary>
        ///     Dictionary value.
        /// </summary>
        [ProtoContract]
        public class TimeoutEntry
        {
			/// <summary>
			/// Prevents a default instance of the <see cref="TimeoutEntry"/> class from being created.
			/// </summary>
	        private TimeoutEntry( )
	        {

	        }

	        /// <summary>
            ///     Initializes a new instance of the Entry class.
            /// </summary>
            /// <param name="value">The value.</param>
			private TimeoutEntry( TValue value )
				: this ( )
            {
                Value = value;
                CreationTime = DateTime.UtcNow.Ticks;
            }

	        /// <summary>
	        ///     Gets or sets the creation time.
	        /// </summary>
	        [ProtoMember( 2 )]
	        public long CreationTime
	        {
		        get;
	        }

	        /// <summary>
	        ///     Gets or sets the value.
	        /// </summary>
	        [ProtoMember( 1 )]
	        public TValue Value
	        {
		        get;
	        }

			/// <summary>
			/// Creates the specified value.
			/// </summary>
			/// <param name="value">The value.</param>
			/// <returns></returns>
			public static TimeoutEntry Create( TValue value )
			{
				return new TimeoutEntry( value );
			}
        }
    }



    /// <summary>
    /// Creates a timeout cache
    /// </summary>
    public class TimeoutCacheFactory : ICacheFactory
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
        /// The expiration timeout of created caches.
        /// </summary>
        public TimeSpan Expiration { get; set; }

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
            var innerCache = Inner.Create<TKey, TimeoutCache<TKey, TValue>.TimeoutEntry>( cacheName );

            return new TimeoutCache<TKey, TValue>(innerCache, cacheName, Expiration, EvictionFrequency);
        }
    }
}
