// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.Cache.Providers
{
    public class ThreadSafeCache<TKey, TValue> : ICache<TKey, TValue>
    {
        /// <summary>
        /// Sync root for this class.
        /// </summary>
        private readonly object _syncRoot = new object();


        /// <summary>
        /// The cache being wrapped.
        /// </summary>
        private readonly ICache<TKey, TValue> _cache;


        /// <summary>
        /// Make an existing cache threadsafe.
        /// </summary>
        /// <param name="innerCache">
        /// The cache being wrapped. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="innerCache"/> cannot be null.
        /// </exception>
        public ThreadSafeCache(ICache<TKey, TValue> innerCache)
        {
            if (innerCache == null)
            {
                throw new ArgumentNullException( nameof( innerCache ) );
            }

            _cache = innerCache;
        }


        /// <summary>
        /// Access a cache entry.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _cache[key];
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    _cache[key] = value;
                }
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
            lock (_syncRoot)
            {
                return _cache.Add(key, value);
            }
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
                throw new ArgumentNullException( nameof( valueFactory ) );

            lock ( _syncRoot )
            {
                return _cache.TryGetOrAdd( key, out value, valueFactory );
            }
        }


        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _cache.Clear();
            }
        }


        /// <summary>
        /// Remove an entry from the cache.
        /// </summary>
        public bool Remove(TKey key)
        {
            lock (_syncRoot)
            {
                bool result = _cache.Remove(key);
                return result;
            }
        }


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
            if ( keys == null )
                throw new ArgumentNullException( nameof( keys ) );

            lock ( _syncRoot )
            {
                return _cache.Remove( keys );
            }
        }


        /// <summary>
        /// TryGetValue
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                bool result = _cache.TryGetValue(key, out value);
                return result;
            }
        }


        /// <summary>
        ///     Returns the number of entries in the cache.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _cache.Count;
                }
            }
        }

        /// <summary>
        /// Raised when items are removed from the set. Note, this may be called
        /// after the itmes are already removed.
        /// </summary>
        public event ItemsRemovedEventHandler<TKey> ItemsRemoved
        {
            add
            {
                lock (_syncRoot)
                {
                    _cache.ItemsRemoved += value;
                }
            }
            remove
            {
                lock (_syncRoot)
                {
                    _cache.ItemsRemoved -= value;
                }               
            }
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _cache.ToList().GetEnumerator();
            }
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (_syncRoot)
            {
                System.Collections.IEnumerable ienum = _cache.ToList();
                return ienum.GetEnumerator();
            }
        }
    }



    /// <summary>
    /// Creates a thread-safe cache
    /// </summary>
    public class ThreadSafeCacheFactory : ICacheFactory
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
            get { return true; }
        }

        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
            var innerCache = Inner.Create<TKey, TValue>( cacheName );
            return new ThreadSafeCache<TKey, TValue>(innerCache);
        }
    }
}
