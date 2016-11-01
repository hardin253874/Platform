// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using EDC.Database;

namespace EDC.Cache.Providers
{
	/// <summary>
	///     Implementation of a cache wrapper that makes a cache Transaction Aware.
	///     Individual lookup speed ( this[ ], TryGetValue ) is O(1).
	///     Multiple lookup speed ( Keys, Values ) is O(n).
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
    public class TransactionAwareCache<TKey, TValue> : ITransactionEventNotification, ICache<TKey, TValue>
	{
        /// <summary>
        ///     The cache name.
        /// </summary>
        private readonly string _cacheName;

        /// <summary>
        ///     The public cache.
        /// </summary>
        private readonly ICache<TKey, TValue> _publicCache;

	    /// <summary>
	    ///     The private cache factory callback.
	    /// </summary>
        private readonly Func<string, ICache<TKey, TValue>> _privateCacheFactory;

		/// <summary>
		///     The encapsulated per transaction caches. The key to the dictionary is the
		///     transaction identifier, or the PublicCacheKey for the public cache.
		/// </summary>
        private readonly ConcurrentDictionary<string, ICache<TKey, TValue>> _privateCaches;

		/// <summary>
		///     Transaction event notification manager for transaction aware caches.
		/// </summary>
		private readonly TransactionEventNotificationManager _transactionEventNotificationManager;

		/// <summary>
		///     Constructor that initializes both the internal cache and lru list.
		/// </summary>
		/// <remarks>
		///     The cache is transaction aware. Any objects that are added or retrieved
		///     from the cache during a transaction are removed if the transaction is rolled back.
		///     If the cache is used to cache database objects consider making the cache transaction aware.
		/// </remarks>
		/// <param name="cacheName">
		/// Name of the cache.
		/// </param>
		/// <param name="publicCache">
		/// The cache being wrapped. This cannot be null.
		/// </param>
		/// <param name="privateCacheFactory">
		/// A function to create a new cache instance. Used to create a new cache for each transaction. 
		/// This cannot be null.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// No argument can be null.
		/// </exception>
		public TransactionAwareCache(string cacheName, ICache<TKey, TValue> publicCache, Func<string, ICache<TKey, TValue>> privateCacheFactory)
		{
            if (publicCache == null)
            {
                throw new ArgumentNullException("publicCache");
            }
            if (privateCacheFactory == null)
            {
                throw new ArgumentNullException("privateCacheFactory");
            }

		    _cacheName = cacheName;
            _publicCache = publicCache;
		    _privateCacheFactory = privateCacheFactory;

		    _privateCaches = new ConcurrentDictionary<string, ICache<TKey, TValue>>();
            _transactionEventNotificationManager = new TransactionEventNotificationManager(this);
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
		    var cache = GetPrivateCache();
		    if (cache == null)
		    {
		        cache = GetPublicCache();
		    }
            return Add(key, value, cache, true);
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

            // TODO: This implementation can be improved
            if ( !TryGetValue( key, out value ) )
            {
                value = valueFactory( key );
                Add( key, value );
                return false;
            }
            else
            {
                return true; // came from cache
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
			bool removed = false;

            // Remove the entry from the public cache                
            var publicCache = GetPublicCache();
            if (publicCache != null)
            {
                removed |= publicCache.Remove(key);
            }

            // Remove the entry from the private cache.
            var privateCache = GetPrivateCache();
            if (privateCache != null)
            {
                removed |= privateCache.Remove(key);
            }

			return removed;
		}


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
            IEnumerable<TKey> toRemove = keys;
            IReadOnlyCollection<TKey> removed = null;

            // Remove the entry from the public cache                
            var publicCache = GetPublicCache( );
            if ( publicCache != null )
            {
                removed = publicCache.Remove( toRemove );
                toRemove = keys.Except( removed );
            }

            // Remove the entry from the private cache.
            var privateCache = GetPrivateCache( );
            if ( privateCache != null )
            {
                removed = privateCache.Remove( toRemove );
            }

            return removed ?? new TKey [ 0 ];
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
		    value = default(TValue);
            bool found = false;

            // Lookup the transaction specific cache first.
            var privateCache = GetPrivateCache();
            if (privateCache != null)
            {
                found = privateCache.TryGetValue(key, out value);
            }

            if (!found)
            {
                // We still don't have an entry. Lookup the public cache.                                        
                var publicCache = GetPublicCache();
                if (publicCache != null)
                {
                    found = publicCache.TryGetValue(key, out value);
                }
                if (!found)
                {
                    value = default(TValue);
                }
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
            _privateCaches.Clear();

            _publicCache.Clear();

            _transactionEventNotificationManager.ClearTransactionEventNotifiers();
		}


		/// <summary>
		///     Returns an enumerator that can be used to iterate over the valid entries currently stored in the cache.
		/// </summary>
		/// <returns>
		///     An enumerator that parses over valid KeyValuePairs.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
            var values = new Dictionary<TKey, TValue>();

            // Get all the entries from the private cache
            var privateCache = GetPrivateCache();
            if (privateCache != null)
            {
                foreach (var kvp in privateCache)
                {
                    values[kvp.Key] = kvp.Value;
                }
            }

            // Get all the entries from the public cache
            var publicCache = GetPublicCache();
            if (publicCache != null)
            {
                foreach (var kvp in publicCache)
                {
                    if (!values.ContainsKey(kvp.Key))
                    {
                        values[kvp.Key] = kvp.Value;
                    }
                }
            }

            return values.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that can be used to iterate over the valid entries currently stored in the cache.
        /// </summary>
        /// <returns>
        ///     An enumerator that parses over valid KeyValuePairs.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

	    /// <summary>
		///     This method is called when a transaction event occurs.
		/// </summary>
		/// <param name="transactionEvent">The transaction event</param>
		void ITransactionEventNotification.OnTransactionEvent( TransactionEventNotificationArgs transactionEvent )
		{
			switch ( transactionEvent.EventType )
			{
				case TransactionEventType.Commit:
					ICache<TKey, TValue> cache;

                    // Remove the cache from the list of caches
                    _privateCaches.TryRemove(transactionEvent.Transactionid, out cache);

                    // If there was a private cache, then commit it to the public cache
					if ( cache != null )
					{
                        var publicCache = GetPublicCache();
						foreach ( var kvp in cache )
						{
							TKey key = kvp.Key;
							TValue value = kvp.Value;

							Add( key, value, publicCache, false );
						}
					}
					break;

				case TransactionEventType.Rollback:
                    // Remove the transaction specific data.
                    _privateCaches.TryRemove(transactionEvent.Transactionid, out cache);
					break;
			}
		}


		/// <summary>
		///     Gets the public cache.
		/// </summary>
		/// <returns></returns>
        private ICache<TKey, TValue> GetPublicCache()
		{
		    return _publicCache;
		}

		/// <summary>
		///     Get the per transaction cache.
		///     If no transaction is active
		///     a null value is returned.
		/// </summary>
		/// <returns></returns>
        private ICache<TKey, TValue> GetPrivateCache()
		{
            ICache<TKey, TValue> cache = null;

            // Get the cache key
            if (Transaction.Current != null)
            {
                // The cache is transaction aware and we are in a transaction.                    
                string cacheKey = Transaction.Current.TransactionInformation.LocalIdentifier;

                // Get the cache
                if (!string.IsNullOrEmpty(cacheKey) &&
                     !_privateCaches.TryGetValue(cacheKey, out cache))
                {
                    //string innerName = string.Concat(_cacheName, ":PrivateTrans " + cacheKey);
                    cache = _privateCacheFactory(_cacheName);
                    _privateCaches[cacheKey] = cache;
                }
            }

			return cache;
		}


        /// <summary>
        ///     Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="enlistTran">
        ///     if set to <c>true</c> [enlist tran].
        /// </param>
        private bool Add(TKey key, TValue value, ICache<TKey, TValue> cache, bool enlistTran)
        {
            /////
            // ReSharper disable CompareNonConstrainedGenericWithNull
            /////

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (cache == null)
            {
                throw new ArgumentNullException("cache");
            }

            if (enlistTran)
            {
                // Enlist in the current transaction so that a commit will 
                // add all the new entries to the public cache.                
                // Ensure this is called outside any locks to prevent 
                // deadlocks.
                _transactionEventNotificationManager.EnlistTransaction(Transaction.Current);
            }

            return cache.Add(key, value);            

            /////
            // ReSharper restore CompareNonConstrainedGenericWithNull
            /////
        }

        /// <summary>
        ///     Returns the number of entries in the cache.
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;

                // Get all the entries from the private cache
                var privateCache = GetPrivateCache();
                if (privateCache != null)
                {
                    count += privateCache.Count;
                }

                // Get all the entries from the public cache
                var publicCache = GetPublicCache();
                if (publicCache != null)
                {
                    count += publicCache.Count;
                }

                return count;
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
                GetPublicCache().ItemsRemoved += value;
            }
            remove
            {
                GetPublicCache().ItemsRemoved -= value;
            }
        }
    }



    /// <summary>
    /// Creates a transaction-aware cache
    /// </summary>
    public class TransactionAwareCacheFactory : ICacheFactory
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
            get { return false; }
        }

        /// <summary>
        /// The factory for the cache of uncommitted data.
        /// </summary>
        public ICacheFactory Private { get; set; }

        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
            if (Private == null)
                Private = new DictionaryCacheFactory();
            var innerCache = Inner.Create<TKey, TValue>( cacheName );
            return new TransactionAwareCache<TKey, TValue>( cacheName, innerCache, Private.Create<TKey, TValue> );
        }
    }
}