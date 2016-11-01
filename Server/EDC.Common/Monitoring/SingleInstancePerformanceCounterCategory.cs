// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;

namespace EDC.Monitoring
{
    /// <summary>
    /// A wrapper class for multi-instance performance counters.
    /// </summary>
    public class SingleInstancePerformanceCounterCategory: ISingleInstancePerformanceCounterCategory
    {
        private readonly ConcurrentDictionary<string, BasePerformanceCounter> PerformanceCounters;

        private bool disposed;

        /// <summary>
        /// Create a new <see cref="SingleInstancePerformanceCounterCategory"/> with the given 
        /// category name.
        /// </summary>
        /// <param name="categoryName">
        /// The category name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="categoryName"/> cannot be null, empty or whitespace.
        /// </exception>
        public SingleInstancePerformanceCounterCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException("categoryName");
            }

            CategoryName = categoryName;
            PerformanceCounters = new ConcurrentDictionary<string, BasePerformanceCounter>();
            disposed = false;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~SingleInstancePerformanceCounterCategory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        /// <param name="disposing">
        /// True if being called from Dispose, false if from the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                if (PerformanceCounters != null)
                {
                    foreach (BasePerformanceCounter basePerformanceCounter in PerformanceCounters.Values)
                    {
                        if (basePerformanceCounter != null)
                        {
                            basePerformanceCounter.Dispose();
                        }
                    }
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Category name.
        /// </summary>
        public string CategoryName { get; private set; }

        /// <summary>
        /// Get the requested performance counter, constructing it on the first access.
        /// </summary>
        /// <typeparam name="T">
        /// The performance counter type.
        /// </typeparam>
        /// <param name="counterName">
        /// The name of the counter. This cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// The requested type of performance counter. Do not Dispose of the returned object.
        /// This class manages its lifecycle.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="counterName"/> cannot be null, empty or whitespace.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <typeparamref name="T"/> is not a supported type.
        /// </exception>
        public T GetPerformanceCounter<T>(string counterName)
            where T : BasePerformanceCounter
        {
            if (string.IsNullOrWhiteSpace(counterName))
            {
                throw new ArgumentNullException("counterName");
            }

            return (T)PerformanceCounters.GetOrAdd(counterName, c =>
            {
                BasePerformanceCounter newPerformanceCounter = null;
                if (typeof(T) == typeof(AverageTimer32PerformanceCounter))
                {
                    newPerformanceCounter = new AverageTimer32PerformanceCounter(CategoryName, c);
                }
                else if (typeof(T) == typeof(RatePerSecond32PerformanceCounter))
                {
                    newPerformanceCounter = new RatePerSecond32PerformanceCounter(CategoryName, c);
                }
                else if (typeof(T) == typeof(NumberOfItems64PerformanceCounter))
                {
                    newPerformanceCounter = new NumberOfItems64PerformanceCounter(CategoryName, c);
                }
                else if (typeof(T) == typeof(PercentageRatePerformanceCounter))
                {
                    newPerformanceCounter = new PercentageRatePerformanceCounter(CategoryName, c);
                }
                else
                {
                    throw new ArgumentException(string.Format("Unsupported type of performance counter: {0}", typeof(T).Name));
                }
                return newPerformanceCounter;
            });
        }
    }
}
