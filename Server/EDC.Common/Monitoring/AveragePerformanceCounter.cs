// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;

namespace EDC.Monitoring
{
    /// <summary>
    /// The base class for <see cref="PerformanceCounter"/> that specifies an average.
    /// </summary>
    public abstract class AveragePerformanceCounter: BasePerformanceCounter
    {
        /// <summary>
        /// Provided for mocking only.
        /// </summary>
        protected AveragePerformanceCounter()
        {
            // Do nothing
        }
        
        /// <summary>
        /// Create a new performance counter that represents an averaged value.
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
        protected AveragePerformanceCounter(string categoryName, string counterName, string instanceName = null)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException("categoryName");
            }
            if (string.IsNullOrWhiteSpace(counterName))
            {
                throw new ArgumentNullException("counterName");
            }

            string baseCounterName;

            baseCounterName = counterName + PerformanceCounterConstants.BaseSuffix;
            Base = new PerformanceCounter(categoryName, baseCounterName, instanceName ?? string.Empty, false);

            if (Base.CounterType != PerformanceCounterType.AverageBase)
            {
                throw new InvalidOperationException(
                    string.Format("Unexpected counter type for counter '{0}' in category '{1}'. Expected: '{2}'. Actual: '{3}'.",
                    baseCounterName, categoryName, PerformanceCounterType.AverageBase, Base.CounterType));
            }
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~AveragePerformanceCounter()
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
                if (Base != null)
                {
                    Base.Dispose();
                }                
            }
        }

        /// <summary>
        /// The <see cref="PerformanceCounter"/> used as a denominator for other counters.
        /// </summary>
        public PerformanceCounter Base
        {
            get; 
            private set;
        }
    }
}
