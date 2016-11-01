// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    // TODO: Should group these functions into a class.

    /// <summary>
    /// Methods to manage a stack of data in a per-thread named slot.
    /// </summary>
    /// <remarks>
    /// These methods are threadsafe.
    /// </remarks>
    /// <typeparam name="T">
    /// The data stored in the context.
    /// </typeparam>
    internal static class ContextHelper<T>
        where T:class
    {
        /// <summary>
        /// A dictionary for each thread, each containing the stacks for each of the named contexts.
        /// Remember that static variables in generic types are unique for each value of T.
        /// </summary>
        public static readonly ThreadLocal<ConcurrentDictionary<string, ConcurrentStack<T>>> Context =
            new ThreadLocal<ConcurrentDictionary<string, ConcurrentStack<T>>>(
                () => new ConcurrentDictionary<string, ConcurrentStack<T>>());

        /// <summary>
        /// Store the data in the call context for use across
        /// method calls.
        /// </summary>
        /// <param name="slotName">
        /// The name of the slot used with the <see cref="CallContext"/>. This 
        /// cannot be null, empty or whitespace.
        /// </param>
        /// <param name="data">
        /// The data to push onto the stack in the named slot
        /// in the <see cref="CallContext"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="slotName"/> cannot be null, empty or whitespace.
        /// </exception>
        public static void PushContextData(string slotName, T data)
        {
            if (string.IsNullOrWhiteSpace(slotName))
            {
                throw new ArgumentNullException("slotName");
            }

            GetContextDataStack(slotName).Push(data);
        }

        /// <summary>
        /// Pop the top level data off the call context slot.
        /// </summary>
        /// <param name="slotName">
        /// The name of the slot used with the <see cref="CallContext"/>. This 
        /// cannot be null, empty or whitespace.
        /// </param>
        /// <param name="expectedData">
        /// The data to pop off the stack in the named slot in the
        /// <see cref="CallContext"/>. This is used to ensure the code
        /// pops off the correct value.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="slotName"/> cannot be null or whitespace.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="expectedData"/> does not match the data at the
        /// top of the stack.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// There is no data in the context to pop.
        /// </exception>
        public static void PopContextData(string slotName, T expectedData)
        {
            if (string.IsNullOrWhiteSpace(slotName))
            {
                throw new ArgumentNullException("slotName");
            }

            ConcurrentStack<T> stack;
            T top;

            stack = GetContextDataStack(slotName);

            // Sanity checks
            if (!stack.TryPeek(out top))
            {
                throw new InvalidOperationException("Empty context stack");
            }
            if (top != expectedData)
            {
                throw new ArgumentException("Popping wrong data", "expectedData");
            }

            stack.TryPop(out top);
        }

        /// <summary>
        /// Get the stack of the context data.
        /// </summary>
        /// <param name="slotName">
        /// The name of the slot used with the <see cref="CallContext"/>. This 
        /// cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// All the data in a <see cref="ConcurrentStack{T}"/>.
        /// </returns>
        public static ConcurrentStack<T> GetContextDataStack(string slotName)
        {
            if (string.IsNullOrWhiteSpace(slotName))
            {
                throw new ArgumentNullException("slotName");
            }

            return Context.Value.GetOrAdd(slotName, s => new ConcurrentStack<T>());
        }

        /// <summary>
        /// Is the given named slot used?
        /// </summary>
        /// <param name="slotName">
        /// The name of the slot used with the <see cref="CallContext"/>. This 
        /// cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// True if that slot contains a <see cref="Stack{T}"/>, false otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="slotName"/> cannot be null, empty or whitespace.
        /// </exception>
        public static bool IsSet(string slotName)
        {
            if (string.IsNullOrWhiteSpace(slotName))
            {
                throw new ArgumentNullException("slotName");
            }

            ConcurrentStack<T> stack;
            bool result;

            result = Context.Value.TryGetValue(slotName, out stack);

            return result && !stack.IsEmpty;
        }
    }
}
