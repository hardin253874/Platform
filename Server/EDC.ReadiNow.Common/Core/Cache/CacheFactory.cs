// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Cache.Providers;
using EDC.Cache;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core.Cache.Providers;
using System.Collections.Concurrent;

namespace EDC.ReadiNow.Core.Cache
{
    /// <summary>
    /// A configurable cache factory.
    /// </summary>
    public class CacheFactory : ICacheFactory
    {
        /// <summary>
        /// The default maximum cache size.
        /// </summary>
        public const int DefaultMaximumCacheSize = 10000;

        /// <summary>
        /// Cache configuration dictionary, keyed off cache name.
        /// </summary>
        private static ConcurrentDictionary<string, CacheConfigElement> _cacheConfigDictionary = new ConcurrentDictionary<string, CacheConfigElement>();

        /// <summary>
        /// Defaults constructor with default settings.
        /// </summary>
        public CacheFactory()
        {
            // On by default
            Logging = true;
            ThreadSafe = true;
			Dictionary = true;
			Lru = true;
            IsolateTenants = true;

            // Off by default
            Distributed = false;
            TransactionAware = false;
            BlockIfPending = false;
            ExpirationInterval = TimeSpan.Zero;
            MaxCacheEntries = 0;            
			Redis = false;
			RedisKeyCompression = false;
			RedisValueCompression = false;
            DelayedInvalidates = false;
            
            MetadataCache = false;
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static CacheFactory()
        {
            try
            {
                foreach(CacheConfigElement cacheConfigElement in ConfigurationSettings.GetCacheConfigurationSection().Caches)
                {
                    _cacheConfigDictionary[cacheConfigElement.CacheName] = cacheConfigElement;
                }                
            }
            catch(Exception ex)
            {
                // Don't allow exceptions to leave static constructor
                Diagnostics.EventLog.Application.WriteError("An error occurred in CacheFactory. {0}", ex.ToString());                
            }
        }

        /// <summary>
        /// Cache name.
        /// </summary>
        public string CacheName { get; set; }

        /// <summary>
        /// True by default.
        /// </summary>
        public bool ThreadSafe { get; set; }

		/// <summary>
		/// True by default.
		/// </summary>
	    public bool Dictionary { get; set; }

	    /// <summary>
        /// True by default.
        /// </summary>
        public bool Logging { get; set; }

		/// <summary>
		/// False by default.
		/// </summary>
		public bool Redis { get; set; }

		/// <summary>
		/// False by default.
		/// </summary>
		public bool Lru{ get; set; }

		/// <summary>
		/// Null by default
		/// </summary>
		public TimeSpan? LruEvictionFrequency { get; set; }

		/// <summary>
		/// False by default
		/// </summary>
		public bool RedisKeyCompression { get; set; }

		/// <summary>
		/// False by default
		/// </summary>
		public bool RedisValueCompression { get; set; }

		/// <summary>
		/// Null by default.
		/// </summary>
	    public TimeSpan? RedisKeyExpiry { get; set; }

	    /// <summary>
        /// False by default.
        /// Blocks calls on 'get' if another thread is already in the process of calculating a cache value.
        /// </summary>
        public bool BlockIfPending { get; set; }

        /// <summary>
        /// False by default.
        /// </summary>
        public bool TransactionAware { get; set; }

        /// <summary>
        /// Allow invalidates to be delayed
        /// </summary>
        public bool DelayedInvalidates { get; set; }

        /// <summary>
        /// Set to zero for no limit.
        /// </summary>
        public int MaxCacheEntries { get; set; }        

        /// <summary>
        /// Set to TimeSpan.Zero for no limit.
        /// </summary>
        public TimeSpan ExpirationInterval { get; set; }

        /// <summary>
        /// Set to TimeSpan.Zero for no limit.
        /// </summary>
        public bool IsolateTenants { get; set; }

        /// <summary>
        /// If true, use a shared or distributed cache across multiple front end servers. False to
        /// use a single machine cache.
        /// </summary>
        public bool Distributed { get; set; }

        /// <summary>
        /// Partition data by user-rule-set and entirely invalidate on any application meta data changes
        /// </summary>
        public bool MetadataCache { get; set; }

		/// <summary>
		/// Gets or sets the timeout eviction frequency.
		/// </summary>
		/// <value>
		/// The timeout eviction frequency.
		/// </value>
	    public TimeSpan? TimeoutEvictionFrequency
	    {
		    get;
		    set;
	    }

	    /// <summary>
        /// Create a transaction aware LRU cache.
        /// This method is only intended to help transition legacy code that was using the LruCache directly.
        /// </summary>
        /// <typeparam name="TKey">Type of cache key.</typeparam>
        /// <typeparam name="TValue">Type of cache value.</typeparam>
        /// <returns>A cache.</returns>
        public ICache<TKey, TValue> Create<TKey, TValue>()
        {
            return Create<TKey, TValue>(CacheName);
        }

		/// <summary>
		/// Create cache, according to the configuration of the factory.
		/// </summary>
		/// <typeparam name="TKey">Type of cache key.</typeparam>
		/// <typeparam name="TValue">Type of cache value.</typeparam>
		/// <param name="cacheName">Name of the cache.</param>
		/// <returns>
		/// A cache.
		/// </returns>
		/// <exception cref="System.InvalidOperationException"></exception>
        public ICache<TKey, TValue> Create<TKey, TValue>(string cacheName)
        {
			ICacheFactory fact = null;

            //
            // The first caches encountered will be lowest in the stack
            //
            if (Redis)
			{
				if ( ConfigurationSettings.GetCacheConfigurationSection().RedisCacheSettings.Enabled )
				{
					fact = new RedisCacheFactory { Inner = null, CompressKey = RedisKeyCompression, CompressValue = RedisValueCompression, KeyExpiry = RedisKeyExpiry };
				}
				else
				{
					Diagnostics.EventLog.Application.WriteWarning( "Cache '{0}' requested a Redis layer but was denied due to the SoftwarePlatform.config settings.", cacheName );
				}
			}

            if (Dictionary)
			{
				fact = new DictionaryCacheFactory { Inner = fact };
			}

            if (MaxCacheEntries > 0)
            {                
                // Attempt to get the maximum value from the configuration settings
                int configMaxCacheSize = GetMaximumCacheSize(cacheName);
                if (configMaxCacheSize > 0)
                {
                    MaxCacheEntries = configMaxCacheSize;
                }

	            if ( Lru )
	            {
					fact = new LruCacheFactory { Inner = fact, MaxSize = MaxCacheEntries, EvictionFrequency = LruEvictionFrequency };
	            }
				else
				{
                fact = new StochasticCacheFactory { Inner = fact, MaxSize = MaxCacheEntries };
            }
            }
            if (ExpirationInterval != TimeSpan.Zero)
            {
                fact = new TimeoutCacheFactory { Inner = fact, Expiration = ExpirationInterval, EvictionFrequency = TimeoutEvictionFrequency };
            }
            if (TransactionAware)
            {
                fact = new TransactionAwareCacheFactory { Inner = fact };
            }
            if (Logging)
            {
                fact = new LoggingCacheFactory { Inner = fact, MaxSize = MaxCacheEntries };
            }
            if (fact != null && (ThreadSafe && !fact.ThreadSafe))
            {
                fact = new ThreadSafeCacheFactory { Inner = fact };
            }
            if (BlockIfPending || DelayedInvalidates)
            {
                fact = new BlockIfPendingCacheFactory { Inner = fact };
            }
            if (DelayedInvalidates)
            {
                // Must be below BlockIfPending
                fact = new DelayedInvalidateCacheFactory { Inner = fact, ExpirationInterval = new TimeSpan(0, 1, 0) };
            }
            if (MetadataCache)
            {
                fact = new MetadataCacheFactory { Inner = fact };
            }
            if (IsolateTenants)
            {
                fact = new PerTenantNonSharingCacheFactory { Inner = fact };
            }


            if (Distributed)
            {
				fact = new RedisPubSubCacheFactory<TKey>( fact, IsolateTenants );
            }

            // Final safety check
            if ( fact == null || ( ThreadSafe && !fact.ThreadSafe ) )
            {
                throw new InvalidOperationException();
            }
            var cache = fact.Create<TKey, TValue>( cacheName );
            return cache;
        }

        /// <summary>
        /// Create simple thread-safe cache.
        /// </summary>
        /// <param name="cacheName">
        ///     The cache name (used for logging and partitioning).
        /// </param>
        /// <param name="distributed">
        ///     True if the cache should be distributed or shared across multiple front end servers, false otherwise.
        /// </param>
        /// <param name="maxSize">
        ///     The cache size.
        /// </param>
		/// <param name="logging">
		///		True if logging is enabled.
		/// </param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static ICache<TKey, TValue> CreateSimpleCache<TKey, TValue>(string cacheName, bool distributed = false,
            int maxSize = DefaultMaximumCacheSize, bool logging = true)
        {
            var fact = new CacheFactory
            {
                CacheName = cacheName,
                MaxCacheEntries = maxSize,
                Distributed = distributed,
				Logging = logging
            };
            return fact.Create<TKey, TValue>(cacheName);
        }

        /// <summary>
        /// Gets the maximum size of the cache.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        /// <returns></returns>
        private int GetMaximumCacheSize(string cacheName)
        {
            if (string.IsNullOrWhiteSpace(cacheName))
            {
                return 0;
            }

            int maximumCacheSize = 0;
            CacheConfigElement cacheConfigElement;

            if (_cacheConfigDictionary.TryGetValue(cacheName, out cacheConfigElement))
            {
                // The cache configuration is available
                maximumCacheSize = cacheConfigElement.MaximumSize > 0 ? cacheConfigElement.MaximumSize : 0;                
            }            

            return maximumCacheSize;
        }
    }
}
