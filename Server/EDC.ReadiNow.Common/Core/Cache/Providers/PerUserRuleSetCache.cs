// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.AccessControl;

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
    public class PerUserRuleSetCache<TKey, TValue> : ICache<TKey, TValue>, IDisposable
    {
        /// <summary>
        /// The cache being wrapped.
        /// </summary>
        private readonly ICache<Tuple<int, TKey>, TValue> _cache;

        private readonly IUserRuleSetProvider _ruleSetProvider;

        private readonly UserRuleSetsHasher _rulesSetsHasher;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PerUserRuleSetCache( ICache<Tuple<int, TKey>, TValue> innerCache, IUserRuleSetProvider ruleSetProvider)
        {
            if (ruleSetProvider == null)
                throw new ArgumentException("ruleSetProvider");

            _ruleSetProvider = ruleSetProvider;
            _rulesSetsHasher = new UserRuleSetsHasher(ruleSetProvider);
            _cache = innerCache;
            _cache.ItemsRemoved += CacheOnItemsRemoved;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~PerUserRuleSetCache()
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
        /// Dispose managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// True if called from <see cref="Dispose"/>, false otherwise.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            _cache.ItemsRemoved -= CacheOnItemsRemoved;
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
            var tKey = GetKey(key);
            return _cache.Add(tKey, value);
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
        public bool TryGetOrAdd( TKey key, out TValue value, Func<TKey, TValue> valueFactory )
        {
            if ( valueFactory == null )
                throw new ArgumentNullException( "valueFactory" );

            var tKey = GetKey( key );
            return _cache.TryGetOrAdd( tKey, out value, perTenantKey => valueFactory( key ) );
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
            var tKey = GetKey(key);
            return _cache.Remove(tKey);
        }


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
            if ( keys == null )
                throw new ArgumentNullException( "keys" );

            var tKeys = GetKeys( keys );
            var removed = _cache.Remove( tKeys );

            return removed.Select( entry => entry.Item2 ).ToList();
        }


        /// <summary>
        /// TryGetValue
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var tKey = GetKey(key);
            return _cache.TryGetValue(tKey, out value);
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var pairs = _cache.Select(p => new KeyValuePair<TKey, TValue>(p.Key.Item2, p.Value));
            return pairs.GetEnumerator();
        }


        /// <summary>
        /// GetEnumerator
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            var pairs = _cache.Select(p => new KeyValuePair<TKey, TValue>(p.Key.Item2, p.Value));
            System.Collections.IEnumerable ienum = pairs;   
            return ienum.GetEnumerator();
        }


        /// <summary>
        ///     Returns the number of entries in the cache.
        ///     Caution: Cost is O(N)
        /// </summary>
        public int Count
        {
            get { return _cache.Count(); }
        }


        /// <summary>
        /// Creates a key that contains the current tenant ID.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private Tuple<int, TKey> GetKey(TKey key)
        {
            int ruleSetHash = GetUserRuleSetHash();

            return new Tuple<int, TKey>(ruleSetHash, key);
        }


        /// <summary>
        /// Creates keys that contains the current tenant ID.
        /// </summary>
        /// <param name="keys">
        /// The keys whose values are returned.
        /// </param>
        /// <returns></returns>
        private IEnumerable<Tuple<int, TKey>> GetKeys( IEnumerable<TKey> keys )
        {
            int ruleSetHash = GetUserRuleSetHash();

            return keys.Select(key => new Tuple<int, TKey>(ruleSetHash, key));
        }


        /// <summary>
        /// Get the hash for the rule sets that apply to this user
        /// </summary>
        /// <returns></returns>
        private int GetUserRuleSetHash()
        {
            var userId = RequestContext.GetContext().Identity.Id;

            if (userId <= 0)
                return 0;

            return _rulesSetsHasher.GetUserRuleSetsHash(userId);
           
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
        protected void RaiseItemsRemoved(ItemsRemovedEventArgs<TKey> args)
        {
            if (ItemsRemoved != null)
            {
                ItemsRemoved(this, args);
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
    /// Creates a per tenant cache
    /// </summary>
    public class PerUserRuleSetCacheFactory : ICacheFactory
    {
        /// <summary>
        /// The factory for the inner cache.
        /// </summary>
        public ICacheFactory Inner { get; set; }

        /// <summary>
        /// The Rule set provider to use when diving the cache
        /// </summary>
        public IUserRuleSetProvider RuleSetProvider { get; set; }

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
            var innerCache = Inner.Create<Tuple<int, TKey>, TValue>( cacheName );
            return new PerUserRuleSetCache<TKey, TValue>(innerCache, RuleSetProvider);
        }
    }

}

