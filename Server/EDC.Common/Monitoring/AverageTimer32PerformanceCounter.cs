// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;

namespace EDC.Monitoring
{
    /// <summary>
    /// A <see cref="PerformanceCounter"/> that represents an average time for a task.
    /// </summary>
    public class AverageTimer32PerformanceCounter: AveragePerformanceCounter
    {
        /// <summary>
        /// Use <see cref="SingleInstancePerformanceCounterCategory"/> or <see cref="MultiInstancePerformanceCounterCategory"/>
        /// to create performance counters. This is supplied for mocking only.
        /// </summary>
        protected AverageTimer32PerformanceCounter()
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
        /// The counter is not of type <see cref="PerformanceCounterType.AverageTimer32"/>.
        /// </exception>
        internal AverageTimer32PerformanceCounter(string categoryName, string counterName, string instanceName = null)
            : base(categoryName, counterName, instanceName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException("categoryName");
            }
            if (string.IsNullOrWhiteSpace(counterName))
            {
                throw new ArgumentNullException("counterName");
            }

            Timer = new PerformanceCounter(categoryName, counterName, instanceName ?? string.Empty, false);

            if (Timer.CounterType != PerformanceCounterType.AverageTimer32)
            {
                throw new InvalidOperationException(
                    string.Format("Unexpected counter type for counter '{0}' in category '{1}'. Expected: '{2}'. Actual: '{3}'.",
                    counterName, categoryName, PerformanceCounterType.AverageTimer32, Timer.CounterType));
            }
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~AverageTimer32PerformanceCounter()
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Timer != null)
                {
                    Timer.Dispose();
                    Timer = null;
                }
            }
        }

        /// <summary>
        /// The <see cref="PerformanceCounter"/> representing the timer.
        /// </summary>
        /// <remarks>
        /// Call <see cref="AddTiming"/> rather than accessing the performance counter directly.
        /// </remarks>
        /// <seealso cref="AddTiming"/>
        protected internal PerformanceCounter Timer
        {
            get; 
            private set;
        }

        /// <summary>
        /// Add another timing to the performance counter. Note that this is threadsafe.
        /// </summary>
        /// <remarks>
        /// This takes a <see cref="Stopwatch"/> rather than a <see cref="TimeSpan"/>
        /// because only the Stopwatch version of ticks are compatible with 
        /// performance counters.
        /// </remarks>
        /// <param name="stopwatch">
        /// The <see cref="Stopwatch"/> containing the timing.
        /// </param>
        public virtual void AddTiming(Stopwatch stopwatch)
        {
            if (stopwatch == null)
            {
                throw new ArgumentNullException("stopwatch");
            }

            // These are threadsafe
            Timer.IncrementBy(stopwatch.ElapsedTicks);
            Base.Increment();
        }
    }
}

