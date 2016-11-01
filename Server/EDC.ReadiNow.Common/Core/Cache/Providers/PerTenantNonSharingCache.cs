// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections;
using System.Collections.Generic;
using EDC.Cache;
using EDC.ReadiNow.IO;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Core.Cache.Providers
{
    /// <summary>
    ///     A cache that isolates values for each tenant.
    ///     Unlike <see cref="PerTenantCache{TKey,TValue}" /> which uses one internal
    ///     cache to store the values this cache uses one cache per tenant.
    /// </summary>
    /// <remarks>
    ///     This class is not thread-safe. Use in conjunction with ThreadSafeCache if required.
    /// </remarks>
    /// <typeparam name="TKey">
    ///     The cache key.
    /// </typeparam>
    /// <typeparam name="TValue">
    ///     The cache value.
    /// </typeparam>
    public class PerTenantNonSharingCache<TKey, TValue> : ICache<TKey, TValue>, IPerTenantCache
    {
        /// <summary>
        ///     The cache of caches. The key to this cache is the tenant id.
        /// </summary>
        private readonly ICache<long, ICache<TKey, TValue>> _cache;


        /// <summary>
        ///     The name of the cache.
        /// </summary>
        private readonly string _cacheName;


        /// <summary>
        ///     Callback used to create inner caches.
        /// </summary>
        private readonly Func<ICache<TKey, TValue>> _createCacheCallback;


        /// <summary>
        /// The sync root.
        /// </summary>
        private object _syncRoot = new object();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cacheName">The name of the cache.</param>
        /// <param name="createCacheCallback">The create cache callback.</param>
        /// <exception cref="System.ArgumentNullException">cacheName</exception>
        public PerTenantNonSharingCache(string cacheName, Func<ICache<TKey, TValue>> createCacheCallback)
        {
            if (string.IsNullOrEmpty(cacheName))
            {
                throw new ArgumentNullException("cacheName");
            }

            _cacheName = cacheName;
            string tenantCacheName = string.Concat(_cacheName, ":Tenants");
            _cache = new CacheFactory { IsolateTenants = false }.Create<long, ICache<TKey, TValue>>( tenantCacheName );
            _createCacheCallback = createCacheCallback ?? CreateDefaultPerTenantCache;
        }


        #region ICache<TKey,TValue> Members

        /// <summary>
        /// The name of the cache.
        /// </summary>
        public string CacheName
        {
            get { return _cacheName; }
        }


        /// <summary>
        ///     Access a cache entry.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                ICache<TKey, TValue> cache = GetPerTenantCache();
                return cache[key];
            }
            set
            {
                ICache<TKey, TValue> cache = GetPerTenantCache();
                cache[key] = value;
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
            ICache<TKey, TValue> cache = GetPerTenantCache();
            return cache.Add(key, value);
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

            ICache<TKey, TValue> cache = GetPerTenantCache( );
            return cache.TryGetOrAdd( key, out value, valueFactory );
        }


        /// <summary>
        ///     Clear the cache.
        /// </summary>
        public void Clear()
        {
            ICache<TKey, TValue> cache = GetPerTenantCache();
            cache.Clear();
        }


        /// <summary>
        /// Clears all the caches across all tenants.
        /// </summary>
        public void ClearAll()
        {
            _cache.Clear();
        }


        /// <summary>
        ///     Remove an entry from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            ICache<TKey, TValue> cache = GetPerTenantCache();
            return cache.Remove(key);
        }


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
            if ( keys == null )
                throw new ArgumentNullException( "keys" );

            ICache<TKey, TValue> cache = GetPerTenantCache( );
            return cache.Remove( keys );
        }


        /// <summary>
        ///     Try to get a value from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the value is in the cache, false otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            ICache<TKey, TValue> cache = GetPerTenantCache();
            return cache.TryGetValue(key, out value);
        }


        /// <summary>
        ///     Returns the number of entries in the cache.
        /// </summary>
        /// <value>
        ///     Returns the number of entries in the cache.
        /// </value>
        public int Count
        {
            get
            {
                ICache<TKey, TValue> cache = GetPerTenantCache();
                return cache.Count;
            }
        }


        /// <summary>
        ///     Occurs when items are removed.
        /// </summary>
        public event ItemsRemovedEventHandler<TKey> ItemsRemoved;


        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ICache<TKey, TValue> cache = GetPerTenantCache();
            return cache.GetEnumerator();
        }


        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            ICache<TKey, TValue> cache = GetPerTenantCache();
            return cache.GetEnumerator();
        }


        #endregion

        /// <summary>
        /// Override ToString for debugging.
        /// </summary>
        public override string ToString()
        {
            return string.Concat(CacheName, " ", base.ToString() );
        }

        /// <summary>
        ///     Default create per tenant cache method.
        /// </summary>
        /// <returns>A per tenant cache.</returns>
        private ICache<TKey, TValue> CreateDefaultPerTenantCache()
        {
            //string cacheName = string.Concat( _cacheName, ":Tenant ", CurrentTenantId );

            return new CacheFactory { IsolateTenants = false }.Create<TKey, TValue>( _cacheName );
        }


        /// <summary>
        ///     Gets the inner per tenant cache for the current tenant.
        /// </summary>
        /// <returns>The per tenant cache for the current tenant.</returns>
        private ICache<TKey, TValue> GetPerTenantCache()
        {
            ICache<TKey, TValue> cache;

            long tenantId = CurrentTenantId;

            if (_cache.TryGetValue(tenantId, out cache)) return cache;

            lock (_syncRoot)
            {
                // Double checked locking to make this call thread safe
                if (_cache.TryGetValue(tenantId, out cache)) return cache;

                cache = _createCacheCallback();
				cache.ItemsRemoved += CacheOnItemsRemoved;
                _cache[tenantId] = cache;
            }            

            return cache;
        }

        /// <summary>
        /// The current tenant ID.
        /// </summary>
        internal static long CurrentTenantId
        {
            get
            {
                long tenantId;
                if (!RequestContext.TryGetTenantId(out tenantId))
                    tenantId = -1;
                return tenantId;
            }
        }

        /// <summary>
        /// Removes all cached data for the specified tenant.
        /// </summary>
        /// <param name="tenantId">The tenant to invalidate.</param>
        public void InvalidateTenant( long tenantId )
        {
            ICache<TKey, TValue> tenantCache;
            if (_cache.TryGetValue(tenantId, out tenantCache))
            {
                // The cache must be cleared before removal, because if it is RedisBacked, then removing the cache isn't sufficient.
                tenantCache.Clear();

                _cache.Remove(tenantId);
            }
        }

        /// <summary>
        /// Called when an item is removed from the inner cache.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CacheOnItemsRemoved( object sender, ItemsRemovedEventArgs<TKey> args )
        {
            if ( ItemsRemoved != null )
            {
                ItemsRemoved( this, args );
            }
        }

    }




    /// <summary>
    /// Creates a PerTenantNonSharingCache
    /// </summary>
    public class PerTenantNonSharingCacheFactory : ICacheFactory
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
            if (Inner == null)
                throw new InvalidOperationException("Inner is null");

            Func<ICache<TKey, TValue>> innerFact = () =>
            {
                long tenantId = PerTenantNonSharingCache<TKey, TValue>.CurrentTenantId;

                using (MetadataCacheFactory.AssociateNewCachesWithTenant(tenantId))
                {
                    //string innerName = cacheName + ":Tenant " + tenantId;
                    return Inner.Create<TKey, TValue>( cacheName );
                }
            };

            var cache = new PerTenantNonSharingCache<TKey, TValue>( cacheName, innerFact);

            Factory.Current.Resolve<IPerTenantCacheInvalidator>( ).RegisterCache( cache );

            return cache;
        }
    }
}