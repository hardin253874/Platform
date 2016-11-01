// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EDC.Serialization.Surrogates;
using ProtoBuf.Meta;

namespace EDC.Cache.Providers
{
    /// <summary>
    ///     A cache layer that ensures that if a missing entry is encountered by two separate threads at the same time,
    ///     then only one of them performs the actual evaluation. The other blocks.
    /// </summary>
    /// <remarks>
    ///     This class is thread-safe if its inner cache is thread safe (which is recommended, otherwise this layer is kinda pointless).
    /// </remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class BlockIfPendingCache<TKey, TValue> : ICache<TKey, TValue>
    {
        /// <summary>
        /// Thread mode to use
        /// </summary>
        private const LazyThreadSafetyMode LazyMode = LazyThreadSafetyMode.ExecutionAndPublication;

        /// <summary>
        /// The cache being wrapped.
        /// </summary>
        private readonly ICache<TKey, Lazy<TValue>> _cache;

		/// <summary>
		/// Initializes the <see cref="BlockIfPendingCache{TKey, TValue}"/> class.
		/// </summary>
		static BlockIfPendingCache( )
		{
			RuntimeTypeModel.Default.Add( typeof( Lazy<TValue> ), false )
				.SetSurrogate( typeof( LazySurrogate<TValue> ) );
		}

		/// <summary>
		/// Make an existing cache thread safe.
		/// </summary>
		/// <param name="innerCache">The cache being wrapped. It should be thread safe. This cannot be null.</param>
		/// <exception cref="System.ArgumentNullException">innerCache</exception>
		/// <exception cref="ArgumentNullException"><paramref name="innerCache" /> cannot be null.</exception>
        public BlockIfPendingCache( ICache<TKey, Lazy<TValue>> innerCache )
        {
            if (innerCache == null)
            {
                throw new ArgumentNullException("innerCache");
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
            // If you're calling Add, then you're probably not getting the benefit of this layer. Use TryGetOrAdd instead.

            var lazy = new Lazy<TValue>( ( ) => value, LazyMode );
            return _cache.Add( key, lazy );
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
		/// <remarks>
		/// If two entries call this at the same time, then only one of them will execute their valueFactory.
		/// Caution: if two threads hit this, then only one of them will return 'false' (as expected), however due to
		/// thread scheduling, the one that returns true (from cache) may return before the one that returns false (not-from cache).
		/// Therefore it is never thread-safe to perform processing if the result is false. Any 'non-cached' (such as registering for invalidations)
		/// processing must be done in the valueFactory callback.
		/// </remarks>
        public bool TryGetOrAdd( TKey key, out TValue value, Func<TKey, TValue> valueFactory )
        {
			if ( valueFactory == null )
			{
				throw new ArgumentNullException( "valueFactory" );
			}

			Lazy<TValue> lazy;

			// Note: we are using Lazy because with ExecutionAndPublication set, it will block subsequent callers
			// but guarantee that everyone gets a result.
			// Note: This will throw a InvalidOperationException if we are reentrant
            bool fromCache = _cache.TryGetOrAdd( key, out lazy, key1 => new Lazy<TValue>( () => valueFactory( key1 ), LazyMode) );

#if DEBUG
            // In debug builds, if an exception got cached, then run the factory explicitly.
            // Otherwise this is a nuisance to debug callbacks.
            try
            {
                value = lazy.Value;
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException && ex.Message == "ValueFactory attempted to access the Value property of this instance.")
                    throw; // exception was thrown by Lazy itself, instead of the calling code - prevent circular exception loops
                if ( fromCache )
                    value = valueFactory( key );
                else
                    throw;
            }
#else
            value = lazy.Value;
#endif

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
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
            if ( keys == null )
                throw new ArgumentNullException( "keys" );
            
            return _cache.Remove( keys );
        }


        /// <summary>
        /// TryGetValue
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            // Check underlying cache
            Lazy<TValue> lazy;
            bool found = _cache.TryGetValue( key, out lazy );

            // Return result
            value = found ? lazy.Value : default( TValue );
            return found;
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <remarks>
        /// Caution .. enumeration.MoveNext will block if entries are currently being calculated.
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var pairs = _cache.Select(p => new KeyValuePair<TKey, TValue>(p.Key, p.Value.Value));
            return pairs.GetEnumerator();
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <remarks>
        /// Caution .. enumeration.MoveNext will block if entries are currently being calculated.
        /// </remarks>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
        {
            var pairs = _cache.Select(p => new KeyValuePair<TKey, TValue>(p.Key, p.Value.Value));
            System.Collections.IEnumerable ienum = pairs;
            return ienum.GetEnumerator();
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
        
    }



    /// <summary>
    /// Creates a block-if-pending cache
    /// </summary>
    public class BlockIfPendingCacheFactory : ICacheFactory
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
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
            var innerCache = Inner.Create<TKey, Lazy<TValue>>( cacheName );

            return new BlockIfPendingCache<TKey, TValue>(innerCache);
        }
    }
}
