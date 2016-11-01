// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ServiceModel.PeerResolvers;
using EDC.Monitoring;
using EDC.Monitoring.Cache;

namespace EDC.Cache.Providers.MetricRepositories
{
    /// <summary>
    /// A <see cref="ILoggingCacheMetricReporter"/> that writes results out synchronously. This should
    /// only be used for testing.
    /// </summary>
    public class SynchronousLoggingCacheMetricReporter: PerformanceCounterLoggingCacheMetricReporter
    {
        /// <summary>
        /// Create a new <see cref="SynchronousLoggingCacheMetricReporter"/> using the default category.
        /// </summary>
        public SynchronousLoggingCacheMetricReporter()
            : this(new MultiInstancePerformanceCounterCategory(CachePerformanceCounters.CategoryName))
        {
            // Do nothing    
        }

        /// <summary>
        /// Create a new <see cref="SynchronousLoggingCacheMetricReporter"/> using the given category.
        /// </summary>
        /// <param name="category"></param>
        public SynchronousLoggingCacheMetricReporter(IMultiInstancePerformanceCounterCategory category)
            : base(category)
        {
            // Do nothing
        }

        /// <summary>
        /// Notify the metric reporter that the cache size has changed.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        public override void NotifySizeChange(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            UpdateSizeCounter(name, SizeCallbacks[name]());
        }

        /// <summary>
        /// Notify the metric reporter that there are more hits and misses.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        public override void NotifyHitsAndMissesChange(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            HitsAndMisses hitsAndMisses;

            hitsAndMisses = HitsAndMissesCallbacks[name]();
            UpdateHitCounter(name, hitsAndMisses.Hits, hitsAndMisses.Misses);            
        }
    }
}
