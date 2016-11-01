// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
    /// <summary>
    /// Extension method class for CallbackAtEnd.
    /// </summary>
    public static class CallbackAtEndExtension
    {
        /// <summary>
        /// Wraps an enumeration so that a callback will be invoked once the enumeration is complete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration">The enumeration.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>The same enumeration, but wrapped.</returns>
        public static IEnumerable<T> CallbackAtEnd<T>(this IEnumerable<T> enumeration, Action callback)
        {
            if (enumeration == null)
                throw new ArgumentNullException("inner");
            if (callback == null)
                throw new ArgumentNullException("callback");
            return new CallbackAtEnd<T>(enumeration, callback);
        }
    }

    /// <summary>
    /// Implementation for CallbackAtEnd.
    /// </summary>
    class CallbackAtEnd<T> : IEnumerable<T>
    {
        private IEnumerable<T> _inner;
        private Action _callback;

        public CallbackAtEnd(IEnumerable<T> inner, Action callback)
        {
            _inner = inner;
            _callback = callback;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T value in _inner)
                yield return value;
            _callback();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (T value in _inner)
                yield return value;
            _callback();
        }
    }
}
