// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;

namespace EDC.Monitoring
{
    public class MultiInstancePerformanceCounterCategory: IMultiInstancePerformanceCounterCategory
    {
        // Counter then instance
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, BasePerformanceCounter>> PerformanceCounters;

        private bool disposed;

        /// <summary>
        /// Create a new <see cref="MultiInstancePerformanceCounterCategory"/> with the given 
        /// category name.
        /// </summary>
        /// <param name="categoryName">
        /// The category name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="categoryName"/> cannot be null, empty or whitespace.
        /// </exception>
        public MultiInstancePerformanceCounterCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException( nameof( categoryName ) );
            }

            CategoryName = categoryName;
            PerformanceCounters = new ConcurrentDictionary<string, ConcurrentDictionary<string, BasePerformanceCounter>>();
            disposed = false;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~MultiInstancePerformanceCounterCategory()
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
                    foreach (ConcurrentDictionary<string, BasePerformanceCounter> counterDictionary in PerformanceCounters.Values)
                    {
                        if (counterDictionary != null)
                        {
                            foreach (BasePerformanceCounter basePerformanceCounter in counterDictionary.Values)
                            {
                                if (basePerformanceCounter != null)
                                {
                                    basePerformanceCounter.Dispose();
                                }
                            }
                        }
                    }
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Category name.
        /// </summary>
        public string CategoryName { get;
        }

        /// <summary>
        /// Get the requested performance counter, constructing it on the first access.
        /// </summary>
        /// <typeparam name="T">
        /// The performance counter type.
        /// </typeparam>
        /// <param name="counterName">
        /// The name of the counter. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="instanceName">
        /// The instance name of the counter. This cannot be null, empty or whitespace.
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
        public T GetPerformanceCounter<T>(string counterName, string instanceName)
            where T : BasePerformanceCounter
        {
            if (string.IsNullOrWhiteSpace(counterName))
            {
                throw new ArgumentNullException( nameof( counterName ) );
            }
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                throw new ArgumentNullException( nameof( instanceName ) );
            }

            PerformanceCounters.GetOrAdd(counterName, new ConcurrentDictionary<string, BasePerformanceCounter>());

            return (T)PerformanceCounters[counterName].GetOrAdd(instanceName, i =>
                {
                    BasePerformanceCounter newPerformanceCounter;

                    if (typeof(T) == typeof(AverageTimer32PerformanceCounter))
                    {
                        newPerformanceCounter = new AverageTimer32PerformanceCounter(CategoryName, counterName, i);
                    }
                    else if (typeof(T) == typeof(RatePerSecond32PerformanceCounter))
                    {
                        newPerformanceCounter = new RatePerSecond32PerformanceCounter(CategoryName, counterName, i);
                    }
                    else if (typeof(T) == typeof(NumberOfItems64PerformanceCounter))
                    {
                        newPerformanceCounter = new NumberOfItems64PerformanceCounter(CategoryName, counterName, i);
                    }
                    else if (typeof(T) == typeof(PercentageRatePerformanceCounter))
                    {
                        newPerformanceCounter = new PercentageRatePerformanceCounter(CategoryName, counterName, i);
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
