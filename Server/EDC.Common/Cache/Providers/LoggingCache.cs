// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using EDC.Cache.Providers.MetricRepositories;

namespace EDC.Cache.Providers
{
    /// <summary>
    /// Wrap another cache and provide performance counters and other diagnostics.
    /// </summary>
    /// <typeparam name="TKey">
    /// The key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    public class LoggingCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private long _hits;
        private long _misses;
        private long _count;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Create a new <see cref="LoggingCache{TKey,TValue}"/> using the specified cache and name.
        /// </summary>
        /// <param name="innerCache">
        /// The cache to wrap. This cannot be null.
        /// </param>
        /// <param name="name">
        /// The cache name. This should be unique and cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public LoggingCache(ICache<TKey, TValue> innerCache, string name)
            : this(innerCache, name, new SynchronousLoggingCacheMetricReporter())
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="LoggingCache{TKey,TValue}"/> using the specified cache, name and counter.
        /// </summary>
        /// <param name="innerCache">
        /// The cache to wrap. This cannot be null.
        /// </param>
        /// <param name="name">
        /// The cache name. This should be unique and cannot be null, empty or whitespace.
        /// </param>
        /// <param name="metricReporter">
        /// Updated with size changes and hit/miss rates. This cannot be null.
        /// </param>        
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        protected internal LoggingCache(ICache<TKey, TValue> innerCache, string name, 
            ILoggingCacheMetricReporter metricReporter)
        {
            if (innerCache == null)
            {
                throw new ArgumentNullException( nameof( innerCache ) );
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof( name ) );
            }
            if (metricReporter == null)
            {
                throw new ArgumentNullException( nameof( metricReporter ) );
            }

            InnerCache = innerCache;
            Name = name;
            MetricReporter = metricReporter;
            InnerCache.ItemsRemoved += InnerCache_ItemsRemoved;

            metricReporter.AddSizeCallback(name, () => _count);
            metricReporter.AddHitsAndMissesCallback(name, () =>
                new HitsAndMisses(Interlocked.Exchange(ref _hits, 0), Interlocked.Exchange(ref _misses, 0)));
        }        

        /// <summary>
        /// The wrapped cache.
        /// </summary>
        internal ICache<TKey, TValue> InnerCache { get; }

        /// <summary>
        /// The cache name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The metric repository to write metrics to.
        /// </summary>
        public ILoggingCacheMetricReporter MetricReporter { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return InnerCache.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object" /> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object" />.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The cached value if found; null otherwise.
        /// </returns>
        public TValue this[TKey key]
        {
            get
            {
                bool result;
                TValue value;

                result = InnerCache.TryGetValue(key, out value);

	            if ( result )
	            {
		            Interlocked.Increment( ref _hits );
	            }
	            else
	            {
					Interlocked.Increment( ref _misses );
	            }

	            MetricReporter.NotifyHitsAndMissesChange(Name);

                return value;
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
            bool added = InnerCache.Add(key, value);

            if (added)
            {
                IncrementCount();
                MetricReporter.NotifySizeChange(Name);
            }            

            return added;
        }

        /// <summary>
        ///     Attempts to get a value from cache.
        ///     If it is found in cache, return true, and the value.
        ///     If it is not found in cache, return false, and use the valueFactory callback to generate and return the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueFactory">A callback that can create the value.</param>
        /// <returns>
        ///     True if the value came from cache, otherwise false.
        /// </returns>
        public bool TryGetOrAdd( TKey key, out TValue value, Func<TKey, TValue> valueFactory )
        {
            if ( valueFactory == null )
                throw new ArgumentNullException( nameof( valueFactory ) );
            
            bool cameFromCache = InnerCache.TryGetOrAdd( key, out value, valueFactory );

            if ( cameFromCache )
            {
                Interlocked.Increment(ref _hits);
            }
            else
            {
                Interlocked.Increment(ref _misses);
                IncrementCount();
                MetricReporter.NotifySizeChange(Name);
            }

            MetricReporter.NotifyHitsAndMissesChange(Name);

            return cameFromCache;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            InnerCache.Clear();            
        }

        /// <summary>
        /// Removes the specified key's cache value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// True if the value was removed; False otherwise.
        /// </returns>
        public bool Remove(TKey key)
        {
            return InnerCache.Remove(key);            
        }


        /// <summary>
        /// Remove entries from the cache.
        /// </summary>
        public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
        {
            return InnerCache.Remove( keys );
        }

        /// <summary>
        /// Attempts to retrieve the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the cache contains the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result;
            
            result = InnerCache.TryGetValue(key, out value);

	        if ( result )
	        {
		        Interlocked.Increment( ref _hits );
	        }
	        else
	        {
		        Interlocked.Increment( ref _misses );
	        }

	        MetricReporter.NotifyHitsAndMissesChange(Name);

            return result;
        }

        /// <summary>
        /// Returns the number of entries in the cache.
        /// </summary>
        /// <returns>
        /// The number of entries - however they may not all be valid.
        /// </returns>
        public int Count
        {
            get { return InnerCache.Count; }
        }

        /// <summary>
        /// Raised when items are removed from the set. Note, this may be called
        /// after the itmes are already removed.
        /// </summary>
        public event ItemsRemovedEventHandler<TKey> ItemsRemoved
        {
            add
            {
                InnerCache.ItemsRemoved += value;
            }
            remove
            {
                InnerCache.ItemsRemoved -= value;
            }
        }

		/// <summary>
		/// Raised when items are removed from the cache.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The <see cref="Collections.Generic.ItemsRemovedEventArgs{TKey}"/> instance containing the event data.</param>
        private void InnerCache_ItemsRemoved(object sender, Collections.Generic.ItemsRemovedEventArgs<TKey> e)
        {
		    lock (_syncRoot)
		    {
		        _count -= e.Items.Count;
		    }
            
            MetricReporter.NotifySizeChange(Name);
        }


        /// <summary>
        /// Increment cache count
        /// </summary>
        private void IncrementCount()
        {
            lock (_syncRoot)
            {
                _count++;
            }            
        }
    }

    /// <summary>
    /// Creates a logging cache
    /// </summary>
    public class LoggingCacheFactory : ICacheFactory
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
        /// The maximum size of the created cache.
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Used to store metrics. Use a shared instance for all caches. Ideally, this should be
        /// handled by DI.
        /// </summary>
        public static Lazy<ILoggingCacheMetricReporter> MetricReporter =
            new Lazy<ILoggingCacheMetricReporter>(() => new AsynchronousLoggingCacheMetricReporter(), false);

        /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
            var innerCache = Inner.Create<TKey, TValue>( cacheName );
            return new LoggingCache<TKey, TValue>(innerCache, cacheName, MetricReporter.Value);
        }
    }
}
