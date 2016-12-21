// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EDC.Collections.Generic;

namespace EDC.Cache.Providers
{
    /// <summary>
    ///     A simple concurrent-dictionary based cache.
    /// </summary>
    /// <remarks>
    ///     This class is thread-safe, with the possible exception of the enumerators.
    /// </remarks>
    public class DictionaryCache<TKey, TValue> : ICache<TKey, TValue>
    {
        /// <summary>
        /// The cache being wrapped.
        /// </summary>
        private readonly ConcurrentDictionary<TKey, TValue> _cache = new ConcurrentDictionary<TKey, TValue>();

		/// <summary>
		/// Sync root.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryCache{TKey, TValue}"/> class.
		/// </summary>
		/// <param name="innerCache">The inner cache.</param>
	    public DictionaryCache( ICache<TKey, TValue> innerCache = null )
	    {
		    InnerCache = innerCache;
	    }

		/// <summary>
		/// Gets the inner cache.
		/// </summary>
		/// <value>
		/// The inner cache.
		/// </value>
		public ICache<TKey, TValue> InnerCache
		{
			get;
		}

	    /// <summary>
        /// Access a cache entry.
        /// </summary>
        public TValue this[TKey key]
        {
            get { return this.Get(key); }
            set { Add(key, value); }
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
            bool added;
			bool updated;

			/////
			// Correctly determine whether the value was added or updated
			// rather than just assuming since the value factory was called.
			/////
            TValue newValue = _cache.AddOrUpdate(key, k => value, (k, v) => value, out added, out updated );

	        if ( InnerCache != null )
	        {
				InnerCache.Add( key, newValue );
	        }

            return added;
        }


		/// <summary>
		/// Attempts to get a value from cache.
		/// If it is found in cache, return true, and the value.
		/// If it is not found in cache, return false, and use the valueFactory callback to generate and return the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value"></param>
		/// <param name="valueFactory">A callback that can create the value.</param>
		/// <returns>
		/// True if the value came from cache, otherwise false.
		/// </returns>
		/// <remarks>
		/// If this cache is backed by a redis memory store, this method will still return False
		/// even if the value was retrieved from Redis (technically a cache hit) as it was still
		/// *added* to the dictionary cache. This is to ensure the logging cache (if in use)
		/// has correct counts etc.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">valueFactory</exception>
        public bool TryGetOrAdd( TKey key, out TValue value, Func<TKey, TValue> valueFactory )
        {
            if ( valueFactory == null )
                throw new ArgumentNullException( nameof( valueFactory ) );

            bool added;

			/////
			// Correctly determine whether the value was added
			// rather than just assuming since the value factory was called.
			/////
	        value = _cache.GetOrAdd( key, k =>
	        {
		        if ( InnerCache != null )
		        {
			        TValue redisValue;
			        InnerCache.TryGetOrAdd( k, out redisValue, valueFactory );
					return redisValue;
		        }

		        return valueFactory( k );
	        }, out added );

            return !added;
        }


        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
			// Prevent multiple threads calling this code concurrently.
            // Threads will skip clearing the cache if it is being cleared by another thread.
            // If this code is called concurrently the OnItemsRemoved callback
            // maybe called multiple times when in reality the items were removed once.
            if (!Monitor.TryEnter(_syncRoot)) return;

            try
            {
				var itemsRemovedEventArgs = new ItemsRemovedEventArgs<TKey>(_cache.Keys.ToList());

				_cache.Clear();

				if ( InnerCache != null )
				{
					InnerCache.Clear( );
				}

				OnItemsRemoved(itemsRemovedEventArgs);
			}
			finally
			{
				Monitor.Exit( _syncRoot );
			}
        }


        /// <summary>
        /// Remove an entry from the cache.
        /// </summary>
        public bool Remove(TKey key)
        {
	        if ( InnerCache != null )
	        {
		        InnerCache.Remove( key );
	        }

	        TValue value;
	        bool result = _cache.TryRemove(key, out value);

            if (result)
            {
                OnItemsRemoved(new ItemsRemovedEventArgs<TKey>(new [] { key }));
            }

            return result;
        }


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
            if ( keys == null )
                throw new ArgumentNullException( nameof( keys ) );


	        TKey[] keyArray = keys as TKey[ ] ?? keys.ToArray( );

	        if ( InnerCache != null )
	        {
				InnerCache.Remove( keyArray );
	        }

			List<TKey> removed = keyArray.Where( key =>
            {
                TValue value;
                return _cache.TryRemove( key, out value );
            } ).ToList( );

            if ( removed.Count > 0 )
            {
				OnItemsRemoved( new ItemsRemovedEventArgs<TKey>( removed ) );
            }

            return removed;
        }


        /// <summary>
        /// TryGetValue
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result =_cache.TryGetValue(key, out value);

	        if ( ! result && InnerCache != null )
	        {
		        if ( InnerCache.TryGetValue( key, out value ) )
		        {
			        _cache.TryAdd( key, value );

			        return true;
		        }
	        }

	        return result;
        }


        /// <summary>
        ///     Returns the number of entries in the cache.
        /// </summary>
        public int Count
        {
            get { return _cache.Count; }
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            System.Collections.IEnumerable ienum = _cache;
            return ienum.GetEnumerator();
        }

        /// <summary>
        /// Raised when items are removed from the set. Note, this may be called
        /// after the items are already removed.
        /// </summary>
        public event ItemsRemovedEventHandler<TKey> ItemsRemoved;

        /// <summary>
        /// Raise the <see cref="ItemsRemoved"/> event.
        /// </summary>
        /// <param name="itemsRemovedEventArgs">
        /// Event specific args. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemsRemovedEventArgs"/> cannot be null.
        /// </exception>
        protected void OnItemsRemoved(ItemsRemovedEventArgs<TKey> itemsRemovedEventArgs)
        {
            if (itemsRemovedEventArgs == null)
            {
                throw new ArgumentNullException( nameof( itemsRemovedEventArgs ) );
            }

	        ItemsRemovedEventHandler<TKey> itemsRemovedEventHandler = ItemsRemoved;

            if (itemsRemovedEventHandler != null)
            {
                itemsRemovedEventHandler(this, itemsRemovedEventArgs);
            }
        }
    }



    /// <summary>
    /// Creates a dictionary cache
    /// </summary>
    public class DictionaryCacheFactory : ICacheFactory
    {
        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
			ICache<TKey, TValue> innerCache = null;

	        if ( Inner != null )
	        {
		        innerCache = Inner.Create<TKey, TValue>( cacheName );
	        }

			return new DictionaryCache<TKey, TValue>( innerCache );
        }

		/// <summary>
		///     The factory for the inner cache.
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
            get { return true; }
        }
    }
}
