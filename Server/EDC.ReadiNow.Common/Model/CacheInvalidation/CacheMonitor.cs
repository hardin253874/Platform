// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.Common;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// Monitor the given cache and maintain a list of any removed
    /// items.
    /// </summary>
    /// <typeparam name="TKey">
    /// The cache key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The cache value type.
    /// </typeparam>
    public class CacheMonitor<TKey, TValue> : IDisposable
    {
        /// <summary>
        /// Create a new <see cref="CacheMonitor{TKey, TValue}"/>.
        /// </summary>
        /// <param name="cache">
        /// The <see cref="ICache{TKey, TValue}"/> to monitor. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cache"/> cannot be null.
        /// </exception>
        public CacheMonitor(ICache<TKey, TValue> cache)
        {
            if (cache == null)
            {
                throw new ArgumentNullException("cache");
            }

            Cache = cache;
            ItemsRemoved = new List<TKey>();
            Cache.ItemsRemoved += OnItemsRemovedAction;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~CacheMonitor()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        public void Dispose()
        {
            // Yes, this is not the usual pattern but the following can be called multiple times
            // without side effect.
            if (Cache != null)
            {
                Cache.ItemsRemoved -= OnItemsRemovedAction;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The cache being monitored.
        /// </summary>
        public ICache<TKey, TValue> Cache { get; private set; }

        /// <summary>
        /// Items removed from the cache.
        /// </summary>
        public IList<TKey> ItemsRemoved { get; private set; }

        /// <summary>
        /// Called when an item is removed from the cache.
        /// </summary>
        /// <param name="sender">
        /// The object that raised the event.
        /// </param>
        /// <param name="args">
        /// The event args.
        /// </param>
        private void OnItemsRemovedAction(object sender, ItemsRemovedEventArgs<TKey> args)
        {
            ItemsRemoved.AddRange(args.Items);
        }
    }
}
