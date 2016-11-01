// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Collections.Generic
{
    /// <summary>
    /// A thread-safe, many-to-many dictionary.
    /// </summary>
    /// <typeparam name="TKey">
    /// The key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    public class BidirectionalMultidictionary<TKey, TValue>
    {
        /// <summary>
        /// May keys to values.
        /// </summary>
        internal readonly ConcurrentDictionary<TKey, HashSet<TValue>> KeysToValues;

        /// <summary>
        /// Map values to keys.
        /// </summary>
        internal readonly ConcurrentDictionary<TValue, HashSet<TKey>> ValuesToKeys;

        /// <summary>
        /// Create a new <see cref="BidirectionalMultidictionary{TKey, TValue}"/>.
        /// </summary>
        public BidirectionalMultidictionary()
        {
            // NOTE: May want a constructor that accepts IEqualityComparers.

            KeysToValues = new ConcurrentDictionary<TKey, HashSet<TValue>>();
            ValuesToKeys = new ConcurrentDictionary<TValue, HashSet<TKey>>();
        }

        /// <summary>
        /// Add a key and value if not in the collection. Otherwise,
        /// update the value.
        /// </summary>
        /// <param name="key">
        /// The new key.
        /// </param>
        /// <param name="value">
        /// The new value.
        /// </param>
        public void AddOrUpdate(TKey key, TValue value)
        {
            HashSet<TValue> values = KeysToValues.GetOrAdd(key, k => new HashSet<TValue>());
            lock (values)
            {
                values.Add(value);
            }

            HashSet<TKey> keys = ValuesToKeys.GetOrAdd(value, k => new HashSet<TKey>());
            lock (keys)
            {
                keys.Add(key);
            }
        }

        /// <summary>
        /// Add the given keys and values to the collection. If any key is already
        /// present, update the value instead of adding it.
        /// </summary>
        /// <param name="keys">
        /// The keys to add. This cannot be null.
        /// </param>
        /// <param name="value">
        /// The value to add keys for.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keys"/> cannot be null.
        /// </exception>
        public void AddOrUpdateKeys(IEnumerable<TKey> keys, TValue value)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            foreach (TKey key in keys)
            {
                AddOrUpdate(key, value);
            }
        }

        /// <summary>
        /// Remove all values for the given key.
        /// </summary>
        /// <param name="key">
        /// The key to remove values for.
        /// </param>
        public void RemoveKey(TKey key)
        {
            HashSet<TValue> values;
            TValue[] valuesArray;
            HashSet<TKey> oldKeys;

            if (KeysToValues.TryRemove(key, out values) &&
                values != null)
            {
                lock (values)
                {
                    valuesArray = values.ToArray();                    
                }

                foreach (TValue value in valuesArray)
                {
                    if (ValuesToKeys.TryGetValue(value, out oldKeys)
                        && oldKeys != null)
                    {
                        bool remove = false;

                        lock (oldKeys)
                        {
                            oldKeys.Remove(key);
                            if (oldKeys.Count == 0)
                            {
                                remove = true;
                            }
                        }

                        if (remove)
                        {
                            ValuesToKeys.TryRemove(value, out oldKeys);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove all keys for the given value.
        /// </summary>
        /// <param name="value">
        /// The value to remove keys for.
        /// </param>
        public void RemoveValue(TValue value)
        {
            HashSet<TKey> keys;
            TKey[] keysArray;
            HashSet<TValue> oldValues;

            if (ValuesToKeys.TryRemove(value, out keys) &&
                keys != null)
            {
                lock (keys)
                {
                    keysArray = keys.ToArray();                    
                }

                foreach (TKey key in keysArray)
                {
                    if (KeysToValues.TryGetValue(key, out oldValues)
                        && oldValues != null)
                    {
                        bool remove = false;

                        lock (oldValues)
                        {
                            oldValues.Remove(value);
                            if (oldValues.Count == 0)
                            {
                                remove = true;
                            }
                        }

                        if (remove)
                        {
                            KeysToValues.TryRemove(key, out oldValues);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove all keys for the given values.
        /// </summary>
        /// <param name="values">
        /// The values to remove keys for. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> cannot be null.
        /// </exception>
        public void RemoveValues(IEnumerable<TValue> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            foreach (TValue value in values)
            {
                RemoveValue(value);
            }
        }

        /// <summary>
        /// The keys.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                return GetKeys(KeysToValues);
            }
        }

        /// <summary>
        /// The values.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                return GetKeys(ValuesToKeys);
            }
        }

        /// <summary>
        /// Get the values for a key.
        /// </summary>
        /// <param name="key">
        /// The key to get the values for.
        /// </param>
        /// <returns>
        /// The values for a key. An empty enumeration when there are no values for that key.
        /// </returns>
        public IEnumerable<TValue> GetValues(TKey key)
        {
            HashSet<TValue> internalValues;

            if (KeysToValues.TryGetValue(key, out internalValues) &&
                internalValues != null)
            {
                lock (internalValues)
                {
                    return internalValues.ToArray();                                        
                }
            }
            else
            {
                return new TValue[0];
            }
        }

        /// <summary>
        /// Get the keys for a value.
        /// </summary>
        /// <param name="value">
        /// The value to get the keys for.
        /// </param>
        /// <returns>
        /// The keys for a value. An empty enumeration when there are no keys for that value.
        /// </returns>
        public IEnumerable<TKey> GetKeys(TValue value)
        {
            HashSet<TKey> internalKeys;

            if (ValuesToKeys.TryGetValue(value, out internalKeys) &&
                internalKeys != null)
            {
                lock (internalKeys)
                {
                    return internalKeys.ToArray();                    
                }
            }
            else
            {
                return new TKey[0];
            }
        }

		/// <summary>
		/// Return the keys of the dictionary.
		/// </summary>
		/// <typeparam name="TKey1">The type of the key1.</typeparam>
		/// <typeparam name="TValue1">The type of the value1.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <returns></returns>
		/// <remarks>
		/// Calling .Keys is really bad for concurrency because it forces locks on all buckets to be acquired.
		/// This method does not force all locks - but at the tradeoff of the enumeration possibly being altered by another thread.
		/// (However, the enumeration can be altered after calling .Keys anyway)
		/// </remarks>
        private static IEnumerable<TKey1> GetKeys<TKey1, TValue1>(ConcurrentDictionary<TKey1, TValue1> dictionary)
        {
            return dictionary.Select(item => item.Key);
        }
    }
}
