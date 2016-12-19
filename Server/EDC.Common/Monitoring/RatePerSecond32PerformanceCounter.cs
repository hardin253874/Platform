// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;

namespace EDC.Monitoring
{
    /// <summary>
    /// A <see cref="PerformanceCounter"/> that represents an average rate for a task.
    /// </summary>
    public class RatePerSecond32PerformanceCounter: BasePerformanceCounter
    {
        /// <summary>
        /// Use <see cref="SingleInstancePerformanceCounterCategory"/> or <see cref="MultiInstancePerformanceCounterCategory"/>
        /// to create performance counters. This is supplied for mocking only.
        /// </summary>
        protected RatePerSecond32PerformanceCounter()
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new performance counter that represents an average time for a task.
        /// </summary>
        /// <param name="categoryName">
        /// The name of the performance counter category to access. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="counterName">
        /// The name of the performance counter to access. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="instanceName">
        /// (Optional) The instance name. Omit, pass null or pass an empty string for no instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Neither <paramref name="categoryName"/> nor <paramref name="counterName"/> can be null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The counter is not of type <see cref="PerformanceCounterType.RateOfCountsPerSecond32"/>.
        /// </exception>
        internal RatePerSecond32PerformanceCounter(string categoryName, string counterName, string instanceName = null)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException( nameof( categoryName ) );
            }
            if (string.IsNullOrWhiteSpace(counterName))
            {
                throw new ArgumentNullException( nameof( counterName ) );
            }

            Rate = new PerformanceCounter(categoryName, counterName, instanceName ?? string.Empty, false);

            if (Rate.CounterType != PerformanceCounterType.RateOfCountsPerSecond32)
            {
                throw new InvalidOperationException(
                    string.Format("Unexpected counter type for counter '{0}' in category '{1}'. Expected: '{2}'. Actual: '{3}'.",
                    counterName, categoryName, PerformanceCounterType.RateOfCountsPerSecond32, Rate.CounterType));
            }            
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~RatePerSecond32PerformanceCounter()
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
                if (Rate != null)
                {
                    Rate.Dispose();
                    Rate = null;
                }
            }
        }

        /// <summary>
        /// The <see cref="PerformanceCounter"/> representing the rate. 
        /// </summary>
        /// <remarks>
        /// Call <see cref="Increment"/> rather than accessing the performance counter directly.
        /// </remarks>
        /// <seealso cref="Increment"/>
        protected internal PerformanceCounter Rate
        {
            get;
            private set;
        }

        /// <summary>
        /// Increment the counter. Note that this is threadsafe.
        /// </summary>
        public virtual void Increment()
        {
            Rate.Increment();
        }

        /// <summary>
        /// Add <paramref name="value"/> to the counter. Note that this is threadsafe.
        /// </summary>
        /// <param name="value">
        /// The value to add to the counter. This can be zero or negative.
        /// </param>
        public virtual void IncrementBy(long value)
        {
            Rate.IncrementBy(value);
        }
    }
}
