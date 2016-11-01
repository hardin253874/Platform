// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.Cache
{
    public static class CacheHelpers
    {
        /// <summary>
        ///     Gets a value from an ICache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>
        ///     The cached value, or the default for TValue.
        /// </returns>
        public static TValue Get<TKey, TValue>(this ICache<TKey, TValue> cache, TKey key)
        {
            TValue value;
            if (!cache.TryGetValue(key, out value))
            {
                value = default(TValue);    // Or should we throw an exception??!
            }
            return value;
        }

		/// <summary>
		/// Attempts to retrieve the value with the specified key.
		/// Or if it is not present, then use the value factory to create it.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The key.</param>
		/// <param name="valueFactory">A callback that can create the value.</param>
		/// <returns>
		/// The value.
		/// </returns>
        public static TValue GetOrAdd<TKey, TValue>( this ICache<TKey, TValue> cache, TKey key, Func<TKey, TValue> valueFactory )
        {
            TValue value;
            cache.TryGetOrAdd( key, out value, valueFactory );
            return value;
        }

        /// <summary>
        ///     Determines if the dictionary contains the key.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>
        ///     True if the key was found.
        /// </returns>
        public static bool ContainsKey<TKey, TValue>(this ICache<TKey, TValue> cache, TKey key)
        {
            TValue value;
            bool result = cache.TryGetValue(key, out value);
            return result;
        }

        /// <summary>
        ///     Gets all of the keys in the cache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <returns>
        ///     The cached keys.
        /// </returns>
        public static IEnumerable<TKey> Keys<TKey, TValue>(this ICache<TKey, TValue> cache)
        {
            return cache.Select(pair => pair.Key);
        }

        /// <summary>
        ///     Gets all of the values in the cache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <returns>
        ///     The cached values.
        /// </returns>
        public static IEnumerable<TValue> Values<TKey, TValue>(this ICache<TKey, TValue> cache)
        {
            return cache.Select(pair => pair.Value);
        }
    }
}
