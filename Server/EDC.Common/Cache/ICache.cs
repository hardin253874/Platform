// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;

namespace EDC.Cache
{
    /// <summary>
    /// Delegate for <see cref="ICache{TKey, TValue}.ItemsRemoved"/>.
    /// </summary>
    /// <param name="sender">
    /// Object that raised the event.
    /// </param>
    /// <param name="e">
    /// Event-specific args.
    /// </param>
    public delegate void ItemsRemovedEventHandler<T>(object sender, ItemsRemovedEventArgs<T> e);


    /// <summary>
    /// Typeless base interface for ICache[key,value].
    /// </summary>
    public interface ICache
    {
        /// <summary>
        ///     Clears the cache.
        /// </summary>
        void Clear( );

        /// <summary>
        ///     Returns the number of entries in the cache.
        /// </summary>
        /// <returns>
        ///     The number of entries - however they may not all be valid.
        /// </returns>
        int Count { get; }
    }


    /// <summary>
    /// A software platform cache provider or layer.
    /// </summary>
    /// <typeparam name="TKey">
    /// The key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    public interface ICache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, ICache
    {
        /// <summary>
        ///     Gets or sets the <see cref="System.Object" /> with the specified key.
        /// </summary>
        /// <value>
        ///     The <see cref="System.Object" />.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     The cached value if found; null otherwise.
        /// </returns>
        TValue this[TKey key]
        {
            get;
            set;
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
        bool Add(TKey key, TValue value);

        /// <summary>
        ///     Removes the specified key's cache value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     True if the value was removed; False otherwise.
        /// </returns>
        bool Remove(TKey key);

		/// <summary>
		/// Removes the specified keys' cache values.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <returns>
		/// Keys that were actually removed.
		/// </returns>
        IReadOnlyCollection<TKey> Remove(IEnumerable<TKey> keys);

        /// <summary>
        ///     Attempts to retrieve the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the cache contains the specified key; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Raised when items are removed from the set. Note, this may be called
        /// after the items are no longer present in the collection.
        /// </summary>
        event ItemsRemovedEventHandler<TKey> ItemsRemoved;

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
        bool TryGetOrAdd( TKey key, out TValue value, Func<TKey, TValue> valueFactory );
    }
}
