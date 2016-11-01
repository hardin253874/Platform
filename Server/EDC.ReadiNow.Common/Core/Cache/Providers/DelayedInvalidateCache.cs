// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.IO;
using ERC = EDC.ReadiNow.Cache;
using System.Threading;

namespace EDC.ReadiNow.Core.Cache.Providers
{

    /// <summary>
    ///     A cache that isolates values for each tenant.
    /// </summary>
    /// <remarks>
    ///     This class is thread-safe if the inner cache is thread-safe.
    /// </remarks>
    /// <typeparam name="TKey">
    ///     The cache key.
    /// </typeparam>
    /// <typeparam name="TValue">
    ///     The cache value.
    /// </typeparam>
    public class DelayedInvalidateCache<TKey, TValue> : ICache<TKey, TValue>, IDisposable
    {

        /// <summary>
        /// The cache being wrapped.
        /// </summary>
        private readonly ICache<TKey, TValue> _innerCache;

        private readonly ICache<TKey, Tuple<TValue, DateTime>> _recentCache;

        private readonly TimeSpan _globalThresholdTicks;

        /// <summary>
        /// Users that have recently made changes that cause some cache entry to invalidate.
        /// (and should therefore not have to wait too long before fetching a new value).
        /// </summary>
        private readonly ICache<Tuple<long, TKey>, bool> _recentUpdatingUsers;           // this doesn't need to be a cache I'm just lazy and using the expiration function.


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="innerCache">The inner cache.</param>
		/// <param name="cacheName">Name of the cache.</param>
		/// <param name="expirationInterval">The expiration interval.</param>
		/// <param name="globalThresholdTicks">The global threshold ticks.</param>
		/// <exception cref="System.ArgumentNullException">cacheName</exception>
        public DelayedInvalidateCache(ICache<TKey, TValue> innerCache, string cacheName, TimeSpan expirationInterval, TimeSpan globalThresholdTicks)
        {
			if ( string.IsNullOrEmpty( cacheName ) )
			{
				throw new ArgumentNullException( "cacheName" );
			}

            _innerCache = innerCache;
			CacheName = cacheName;
            CacheFactory factory = new CacheFactory { ExpirationInterval = expirationInterval, IsolateTenants = false };
            _recentCache = factory.Create<TKey, Tuple<TValue,DateTime>>(CacheName + " Delayed Invalidation Recent Cache");
            _recentUpdatingUsers = factory.Create<Tuple<long, TKey>, bool>(CacheName + " Delayed Invalidation Recent Updating Users Cache");
            _globalThresholdTicks = globalThresholdTicks;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~DelayedInvalidateCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

				/// <summary>
		/// Gets the name of the cache.
		/// </summary>
		/// <value>
		/// The name of the cache.
		/// </value>
	    public string CacheName
	    {
		    get;
		    private set;
	    }

        /// <summary>
        /// Dispose managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// True if called from <see cref="Dispose"/>, false otherwise.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
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
        /// Add an entry to the cache.
        /// </summary>
        public bool Add(TKey key, TValue value)
        {
            var entry = new Tuple<TValue, DateTime>( value, DateTime.UtcNow );

            _recentCache.Add( key, entry );

            return _innerCache.Add(key, value);
        }

        /// <summary>
        ///     Attempts to get a value from cache.
        ///     If it is found in cache, return true, and the value.
        ///     If it is not found in cache, return false, and use the valueFactory callback to generate and return the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="valueFactory">A callback that can create the value.</param>
        /// <returns>
        ///     True if the value came from cache, otherwise false.
        /// </returns>
        public bool TryGetOrAdd(TKey key, out TValue value, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null)
                throw new ArgumentNullException("valueFactory");

            Tuple<TValue, DateTime> entry;
            bool fromCache = _recentCache.TryGetOrAdd( key, out entry, key1 =>
            {
                TValue innerValue;
                _innerCache.TryGetOrAdd( key1, out innerValue, valueFactory );

                var entry1 = new Tuple<TValue, DateTime>( innerValue, DateTime.UtcNow );
                return entry1;
            } );

            DateTime timestamp = entry.Item2;
            value = entry.Item1;

            if ( !fromCache )
            {
                return false;
            }

            // If we have reached this point, then entry came from cache, but it may be too old

            // Check if user edited data recently .. if so then re-evaluate
            if ( BypassRecentCache( key ) )
            {
				TimeSpan ticksUntilGlobalExpiry = timestamp.Add( _globalThresholdTicks ) - DateTime.UtcNow;   

                // We want to make sure that multiple requests for the item across the threads are batched so only one request is made. (Throttling.)
                // The lower BlockIfPendingCache ensures only one request is made we just need to make sure all the requests happen at around the same time.
                if ( ticksUntilGlobalExpiry > TimeSpan.Zero )       
                {
                    Thread.Sleep( ticksUntilGlobalExpiry );
                }
                // Note: multiple threads may pass this point at the same time. Ensure the BlockIfPendingCache layer is below this one.

                fromCache = _innerCache.TryGetOrAdd( key, out value, valueFactory );
                _recentCache [ key ] = new Tuple<TValue,DateTime>( value, DateTime.UtcNow );
            }
            return fromCache;
        }
        

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            _recentCache.Clear();
            _innerCache.Clear();
        }


        /// <summary>
        /// Remove an entry from the cache.
        /// </summary>
        public bool Remove(TKey key)
        {
            AddRecentlyUpdatingUsers(key);

            return _innerCache.Remove(key);
        }




        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove(IEnumerable<TKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

	        var list = keys as IList<TKey> ?? keys.ToList( );

	        AddRecentlyUpdatingUsers(list);

            return _innerCache.Remove(list);
        }


        private void AddRecentlyUpdatingUsers(TKey key)
        {
            var context = RequestContext.GetContext();
            AddRecentlyUpdatingUsers(context.Identity, key);
            AddRecentlyUpdatingUsers(context.SecondaryIdentity, key);
        }


        private void AddRecentlyUpdatingUsers(IEnumerable<TKey> keys)
        {
            var context = RequestContext.GetContext();

	        var list = keys as IList<TKey> ?? keys.ToList( );

	        AddRecentlyUpdatingUsers(context.Identity, list);
            AddRecentlyUpdatingUsers(context.SecondaryIdentity, list);
        }

        private void AddRecentlyUpdatingUsers(Security.IdentityInfo identity, IEnumerable<TKey> keys)
        {
            if (identity != null && identity.Id > 0)
            {
                long userId = identity.Id;

                foreach (var key in keys)
                {
                    AddRecentlyUpdatingUsers(userId, key);
                }
            }
        }

        private void AddRecentlyUpdatingUsers(Security.IdentityInfo identity, TKey key)
        {
            if (identity != null && identity.Id > 0)
            {
                AddRecentlyUpdatingUsers(identity.Id, key);
            }
        }

        private void AddRecentlyUpdatingUsers(long userId, TKey key)
        {
            var recentUpdateKey = new Tuple<long, TKey>(userId, key);
            _recentUpdatingUsers.Add(recentUpdateKey, true);
        }

        /// <summary>
        /// TryGetValue
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value) 
        {
            Tuple<TValue, DateTime> entry;
            bool fromCache = _recentCache.TryGetValue( key, out entry );

            if ( !fromCache )
            {
                value = default( TValue );
                return false;
            }

            DateTime timestamp = entry.Item2;
            value = entry.Item1;

            TimeSpan ticksUntilGlobalExpiry = timestamp.Add( _globalThresholdTicks ) - DateTime.UtcNow;

            if ( ticksUntilGlobalExpiry > TimeSpan.Zero )
            {
                return true;
            }

            // If we have reached this point, then entry came from cache, but it may be too old

            // Check if user edited data recently .. if so then re-evaluate
            if ( BypassRecentCache( key ) )
            {
                if ( ticksUntilGlobalExpiry > TimeSpan.Zero )
                {
                    Thread.Sleep( ticksUntilGlobalExpiry );
                }
                // Note: multiple threads may pass this point at the same time. Ensure the BlockIfPendingCache layer is below this one.

                fromCache = _innerCache.TryGetValue( key, out value );
                _recentCache [ key ] = new Tuple<TValue, DateTime>( value, DateTime.UtcNow );
            }
            return fromCache;
        }

        /// <summary>
        /// Should we bypass the recent cache?
        /// </summary>
        /// <returns></returns>
        bool BypassRecentCache(TKey keyToBypass)
        {
            var context = RequestContext.GetContext();

            if (context == null || !context.IsValid)
                return false;

            return BypassRecentCache(keyToBypass, context.Identity) || BypassRecentCache(keyToBypass, context.SecondaryIdentity);
        }

        bool BypassRecentCache(TKey keyToBypass, Security.IdentityInfo identity)
        {
            if (identity == null)
            {
                return false;
            }

            var userId = identity.Id;     

            if (userId == 0)
            {
                return false;
            }

	        var recentUpdateKey = new Tuple<long, TKey>( userId, keyToBypass );

            return _recentUpdatingUsers.ContainsKey(recentUpdateKey);
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            // This is probably not right as it will miss entries in the recent cache, but it may be good enough for all practical purposes.
            return _innerCache.GetEnumerator();         
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            // This is probably not right as it will miss entries in the recent cache, but it may be good enough for all practical purposes.
            return _innerCache.GetEnumerator();
        }


        /// <summary>
        ///     Returns the number of entries in the cache.
        ///     Caution: Cost is O(N)
        /// </summary>
        public int Count
        {
            get { return _innerCache.Count(); }
        }

        /// <summary>
        /// Raised when items are removed from the set. Note, this may be called
        /// after the items are already removed.
        /// </summary>
        public event ItemsRemovedEventHandler<TKey> ItemsRemoved;

	    /// <summary>
	    /// Raise the <see cref="ItemsRemoved"/> event.
	    /// </summary>
	    /// <param name="args">
	    /// Event-specific args.
	    /// </param>
	    protected void RaiseItemsRemoved( ItemsRemovedEventArgs<TKey> args )
	    {
		    var handler = ItemsRemoved;

		    if ( handler != null )
		    {
			    handler( this, args );
		    }
	    }

	    /// <summary>
        /// Called when an item is removed from the inner cache.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="itemsRemovedEventArgs"></param>
        private void CacheOnItemsRemoved(object sender, ItemsRemovedEventArgs<Tuple<int, TKey>> itemsRemovedEventArgs)
        {
            RaiseItemsRemoved(new ItemsRemovedEventArgs<TKey>(itemsRemovedEventArgs.Items.Select(i => i.Item2)));
        }

    }


    /// <summary>
    /// Creates a rate limited cache. This is a cache where the rate at which invalidations are processed is limited to the provided value. In practice this means that a value fetched from the cache 
    /// will remain current for the time period even if invalidations have occurred.
    /// </summary>
    public class DelayedInvalidateCacheFactory : ICacheFactory
    {
        public readonly TimeSpan DefaultGlobalThresholdTicks = TimeSpan.FromSeconds(3);
        public readonly TimeSpan DefaultExpirationInterval = TimeSpan.FromSeconds(30);

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
        ///  How long to wait before invalidations are processed.
        /// </summary>
        public TimeSpan ExpirationInterval { get; set;}


        /// <summary>
        ///  The minimum time between requests for the same item when bypassing due to the current user invalidating.
        /// </summary>
        public TimeSpan GlobalThresholdTicks { get; set; }

        public DelayedInvalidateCacheFactory()
        {
            ExpirationInterval = DefaultExpirationInterval;
            GlobalThresholdTicks = DefaultGlobalThresholdTicks;
        }

        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
            var innerCache = Inner.Create<TKey, TValue>( cacheName );
			var result = new DelayedInvalidateCache<TKey, TValue>( innerCache, cacheName, ExpirationInterval, GlobalThresholdTicks );

            return result;
        }
    }
}

