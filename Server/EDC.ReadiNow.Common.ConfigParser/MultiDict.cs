// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Common.ConfigParser
{
    /// <summary>
    /// Convenient collection for mapping one key to multiple values.
    /// </summary>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    internal class MultiDict<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        /// <summary>
        /// Adds a new value to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            List<TValue> _values;
            if (!TryGetValue(key, out _values))
            {
                _values = new List<TValue>();
                this.Add(key, _values);
            }
            _values.Add(value);
        }

    }


}
