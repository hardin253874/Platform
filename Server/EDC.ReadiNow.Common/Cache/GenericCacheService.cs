// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using EDC.Cache;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Security.AccessControl;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Cache
{
    /// <summary>
    /// Provides the common implementation of a cache service, including cache invalidator.
    /// </summary>
    /// <remarks>
    /// This is a base-class to use when providing a caching implementation of services that implement some interface.
    /// E.g. it makes it convenient to provide a CachingCalculatedFieldProvider, whose primary job is to implement ICalculatedFieldProvider
    /// backed by a cache. Cache services allow for things such as discovery, and clearing.
    /// C.f. ICache[K,V] which provides a raw cache layer/dictionary.
    /// </remarks>
    public abstract class GenericCacheService<TKey, TValue> : ICacheService
    {
        CacheInvalidator<TKey, TValue> _cacheInvalidator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheName">Name of the cache service.</param>
        protected GenericCacheService(string cacheName)
            : this(cacheName, new CacheFactory() )
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheName">Name of the cache service.</param>
        /// <param name="cacheFactory">Cache factory to use.</param>
        protected GenericCacheService(string cacheName, CacheFactory cacheFactory)
        {
            if (string.IsNullOrEmpty(cacheName))
                throw new ArgumentNullException("cacheName");
            if (cacheFactory == null)
                throw new ArgumentNullException("cacheFactory");

            CacheName = cacheName;

            Cache = cacheFactory.Create<TKey, TValue>(cacheName);

            _cacheInvalidator = new CacheInvalidator<TKey, TValue>(Cache, CacheName);
        }

        /// <summary>
        /// The name of the cache.
        /// </summary>
        protected string CacheName { get; private set; }

        /// <summary>
        /// The cache itself.
        /// </summary>
        protected ICache<TKey, TValue> Cache { get; private set; }

        /// <summary>
        /// The cache invalidator.
        /// </summary>
        public ICacheInvalidator CacheInvalidator
        {
            get { return _cacheInvalidator; }
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            Trace.WriteLine(CacheName + ": Cache cleared");

            Cache.Clear();
        }

        /// <summary>
        /// Check the cache.
        /// </summary>
        /// <param name="key">Key to look up.</param>
        /// <param name="valueFactory">A callback that will provide the values.</param>
        /// <returns>The value result - either from cache or freshly determined.</returns>

        protected TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue result;
            TryGetOrAdd(key, out result, valueFactory);
            return result;
        }

        /// <summary>
        /// Check the cache.
        /// </summary>
        /// <param name="key">Key to look up.</param>
        /// <param name="result">The value result - either from cache or freshly determined.</param>
        /// <param name="valueFactory">A callback that will provide the values.</param>
        /// <returns></returns>

        protected bool TryGetOrAdd(TKey key, out TValue result, Func<TKey, TValue> valueFactory)
        {
            bool fromCache = false;

            // Wrap value-factory to handle cache invalidator
            Func<TKey, TValue> valueFactoryImpl = (k) =>
            {
                TValue innerResult;

                using (CacheContext cacheContext = new CacheContext())
                {
                    innerResult = valueFactory(key);

                    // Add the cache context entries to the appropriate CacheInvalidator
                    _cacheInvalidator.AddInvalidations(cacheContext, key);
                }

                return innerResult;
            };

            // Check cache
            fromCache = Cache.TryGetOrAdd(key, out result, valueFactoryImpl);
            if (fromCache && CacheContext.IsSet())
            {
                // Add the already stored changes that should invalidate this cache
                // entry to any outer or containing cache contexts.
                using (CacheContext cacheContext = CacheContext.GetContext())
                {
                    cacheContext.AddInvalidationsFor(_cacheInvalidator, key);
                }
            }

            return fromCache;
        }

        /// <summary>
        /// Helper method to fetch multiple entries from cache, then only call the factory for missing entries.
        /// </summary>
        /// <remarks>
        /// Implementation notes:
        /// 1. The result is guaranteed to always be non-null
        /// 2. And contain the same entries as the keys in the same order.
        /// 3. All misses get evaluated in a single round
        /// 4. Caution: because all misses are evaluated together, they get bound together with the same invalidation.
        /// </remarks>
        /// <param name="keys"></param>
        /// <param name="valuesFactory"></param>
        /// <returns></returns>
        protected IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetOrAddMultiple(IEnumerable<TKey> keys, Func<IEnumerable<TKey>, IEnumerable<TValue>> valuesFactory)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");
            if (valuesFactory == null)
                throw new ArgumentNullException("valuesFactory");

            List<KeyValuePair<TKey, TValue>> result = new List<KeyValuePair<TKey, TValue>>();
            KeyValuePair<TKey, TValue> resultEntry;

            // List of missing entries, including the index position where the result needs to be written
            List<Tuple<TKey, int>> cacheMisses = null;

            using (CacheContext cacheContext = CacheContext.IsSet() ? CacheContext.GetContext() : null)
            {
                // First pass through list
                int pos = 0;
                foreach (TKey key in keys)
                {
                    TValue value;

                    if (Cache.TryGetValue(key, out value))
                    {
                        resultEntry = new KeyValuePair<TKey, TValue>(key, value);

                        // Check cache
                        if (cacheContext != null)
                        {
                            cacheContext.AddInvalidationsFor(_cacheInvalidator, key);
                        }
                    }
                    else
                    {
                        if (cacheMisses == null)
                            cacheMisses = new List<Tuple<TKey, int>>();

                        cacheMisses.Add(new Tuple<TKey, int>(key, pos));

                        // An entry still gets added to the result to maintain ordering - it will be overridden later
                        // (can't use null .. kvp is a struct)
                        resultEntry = new KeyValuePair<TKey, TValue>(key, default(TValue));
                    }
                    result.Add(resultEntry);
                    pos++;
                }
            }

            // Fill in misses
            if (cacheMisses != null)
            {
                using (CacheContext cacheContext = new CacheContext())
                {
                    IEnumerable<TKey> missingKeys = cacheMisses.Select(k => k.Item1);
                    IEnumerable<TValue> missingResults = valuesFactory(missingKeys);

                    Enumerable.Zip(cacheMisses, missingResults, (keyPos, value) =>
                    {
                        TKey key = keyPos.Item1;
                        int resultIndex = keyPos.Item2;
                        resultEntry = new KeyValuePair<TKey, TValue>(key, value);

                        // Include in current result
                        result[resultIndex] = resultEntry;

                        // Add to cache
                        Cache.Add(key, value);

                        // Add the cache context entries to the appropriate CacheInvalidator
                        _cacheInvalidator.AddInvalidations(cacheContext, key);

                        return (object)null;
                    }).Last(); // call Last to force evaluate zip                   
                }                
            }

            return result;
        }

    }
}
