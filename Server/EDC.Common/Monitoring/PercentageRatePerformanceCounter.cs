// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;

namespace EDC.Monitoring
{
    public class PercentageRatePerformanceCounter: BasePerformanceCounter
    {
        /// <summary>
        /// Use <see cref="SingleInstancePerformanceCounterCategory"/> or <see cref="MultiInstancePerformanceCounterCategory"/>
        /// to create performance counters. This is supplied for mocking only.
        /// </summary>
        protected PercentageRatePerformanceCounter()
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
        /// (Optional) The instance name. Omit, pass null or pass an empty string for a single instance counter.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Neither <paramref name="categoryName"/> nor <paramref name="counterName"/> can be null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The sample fraction counter is not of type <see cref="PerformanceCounterType.SampleFraction"/> or
        /// the sample base counter is not of type <see cref="PerformanceCounterType.SampleBase"/>.
        /// </exception>
        internal PercentageRatePerformanceCounter(string categoryName, string counterName, string instanceName = null)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException( nameof( categoryName ) );
            }
            if (string.IsNullOrWhiteSpace(counterName))
            {
                throw new ArgumentNullException( nameof( counterName ) );
            }

            string baseCounterName;

            baseCounterName = counterName + PerformanceCounterConstants.BaseSuffix;
            SampleFraction = new PerformanceCounter(categoryName, counterName, instanceName ?? string.Empty, false);
            SampleBase = new PerformanceCounter(categoryName, baseCounterName, instanceName ?? string.Empty, false);

            if (SampleFraction.CounterType != PerformanceCounterType.SampleFraction)
            {
                throw new InvalidOperationException(
                    string.Format("Unexpected counter type for counter '{0}' in category '{1}'. Expected: '{2}'. Actual: '{3}'.",
                    counterName, categoryName, PerformanceCounterType.SampleFraction, SampleFraction.CounterType));
            }
            if (SampleBase.CounterType != PerformanceCounterType.SampleBase)
            {
                throw new InvalidOperationException(
                    string.Format("Unexpected counter type for counter '{0}' in category '{1}'. Expected: '{2}'. Actual: '{3}'.",
                    baseCounterName, categoryName, PerformanceCounterType.SampleBase, SampleBase.CounterType));
            }               
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~PercentageRatePerformanceCounter()
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
                if (SampleFraction != null)
                {
                    SampleFraction.Dispose();
                    SampleFraction = null;
                }
                if (SampleBase != null)
                {
                    SampleBase.Dispose();
                    SampleBase = null;
                }
            }
        }

        /// <summary>
        /// The <see cref="PerformanceCounter"/> representing the percentage numerator. 
        /// </summary>
        /// <remarks>
        /// Call <see cref="AddHit"/> and <see cref="AddMiss"/> rather than accessing the performance counter directly.
        /// </remarks>
        /// <seealso cref="SampleBase"/>
        protected internal PerformanceCounter SampleFraction
        {
            get;
            private set;
        }

        /// <summary>
        /// The <see cref="PerformanceCounter"/> representing the percentage demoninator. 
        /// </summary>
        /// <remarks>
        /// Call <see cref="AddHit"/> and <see cref="AddMiss"/> rather than accessing the performance counter directly.
        /// </remarks>
        /// <seealso cref="SampleFraction"/>
        protected internal PerformanceCounter SampleBase
        {
            get;
            private set;
        }

        /// <summary>
        /// Increment the both the numerator and denominator. Note that this is threadsafe.
        /// </summary>
        public virtual void AddHit()
        {
            SampleFraction.Increment();
            SampleBase.Increment();
        }

        /// <summary>
        /// Increment both the numerator and denominator. Note that this is threadsafe.
        /// </summary>
        /// <param name="value">
        /// The number of hits to add.
        /// </param>
        public virtual void AddHits(long value)
        {
            SampleFraction.IncrementBy(value);
            SampleBase.IncrementBy(value);
        }

        /// <summary>
        /// Increment the denominator only. Note that this is threadsafe.
        /// </summary>
        public virtual void AddMiss()
        {
            SampleBase.Increment();
        }

        /// <summary>
        /// Increment the denominator only. Note that this is threadsafe.
        /// </summary>
        /// <param name="value">
        /// The number of misses to add.
        /// </param>
        public virtual void AddMisses(long value)
        {
            SampleBase.IncrementBy(value);
        }

        /// <summary>
        /// Set the performance counter to 0.
        /// </summary>
        public virtual void Zero()
        {
            SampleFraction.RawValue = 0;
            SampleBase.RawValue = 0;
        }
    }
}
