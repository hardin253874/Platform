// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;

namespace EDC.Monitoring
{
    /// <summary>
    /// A <see cref="PerformanceCounter"/> that counts a 64-bit integer.
    /// </summary>
    public class NumberOfItems64PerformanceCounter: BasePerformanceCounter
    {
        /// <summary>
        /// Use <see cref="SingleInstancePerformanceCounterCategory"/> or <see cref="MultiInstancePerformanceCounterCategory"/>
        /// to create performance counters. This is supplied for mocking only.
        /// </summary>
        protected NumberOfItems64PerformanceCounter()
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new performance counter that represents a count of items or tasks.
        /// </summary>
        /// <param name="categoryName">
        /// The name of the performance counter category to access. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="counterName">
        /// The name of the performance counter to access. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="instanceName">
        /// (Optional) The instance name. Omit, pass null or pass an empty string for a single instance counter.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Neither <paramref name="categoryName"/> nor <paramref name="counterName"/> can be null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The counter is not of type <see cref="PerformanceCounterType.AverageTimer32"/>.
        /// </exception>
        internal NumberOfItems64PerformanceCounter(string categoryName, string counterName, string instanceName = null)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException( nameof( categoryName ) );
            }
            if (string.IsNullOrWhiteSpace(counterName))
            {
                throw new ArgumentNullException( nameof( counterName ) );
            }

            Counter = new PerformanceCounter(categoryName, counterName, instanceName ?? string.Empty, false);

            if (Counter.CounterType != PerformanceCounterType.NumberOfItems64)
            {
                throw new InvalidOperationException(
                    string.Format("Unexpected counter type for counter '{0}' in category '{1}'. Expected: '{2}'. Actual: '{3}'.",
                    counterName, categoryName, PerformanceCounterType.NumberOfItems64, Counter.CounterType));
            }            
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~NumberOfItems64PerformanceCounter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            base.Dispose();
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        /// <param name="disposing">
        /// True if being called from Dispose, false if from the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Counter != null)
                {
                    Counter.Dispose();
                    Counter = null;
                }
            }
        }

        /// <summary>
        /// The <see cref="PerformanceCounter"/> representing the timer.
        /// </summary>
        /// <remarks>
        /// Call <see cref="Increment"/> rather than accessing the performance counter directly.
        /// </remarks>
        /// <seealso cref="Increment"/>
        protected internal PerformanceCounter Counter
        {
            get; 
            private set; 
        }

        /// <summary>
        /// Increment the counter. This is threadsafe.
        /// </summary>
        public virtual void Increment()
        {
            Counter.Increment();
        }

        /// <summary>
        /// Increment the counter by <paramref name="value"/>. This is threadsafe.
        /// </summary>
        /// <param name="value">
        /// Increment the counter by this value.
        /// </param>
        public virtual void IncrementBy(long value)
        {
            Counter.IncrementBy(value);
        }

        /// <summary>
        /// Set the value. This is threadsafe.
        /// </summary>
        /// <param name="value">
        /// The new value.
        /// </param>
        public virtual void SetValue(long value)
        {
            Counter.RawValue = value;
        }
    }
}
