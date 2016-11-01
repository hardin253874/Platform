// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EDC.Monitoring;
using EDC.Monitoring.Cache;

namespace EDC.Cache.Providers.MetricRepositories
{
    /// <summary>
    /// A <see cref="ILoggingCacheMetricReporter"/> that updates a performance counter.
    /// </summary>
    public abstract class PerformanceCounterLoggingCacheMetricReporter : IDisposable, ILoggingCacheMetricReporter
    {
        /// <summary>
        /// Create a new <see cref="PerformanceCounterLoggingCacheMetricReporter"/> using the given category.
        /// </summary>
        /// <param name="category">
        /// The performance counter category. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="category"/> cannot be null.
        /// </exception>
        public PerformanceCounterLoggingCacheMetricReporter(IMultiInstancePerformanceCounterCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            Category = category;
            SizeCounters = new ConcurrentDictionary<string, NumberOfItems64PerformanceCounter>();
            HitRateCounters = new ConcurrentDictionary<string, PercentageRatePerformanceCounter>();
            TotalHitsCounters = new ConcurrentDictionary<string, NumberOfItems64PerformanceCounter>();
            TotalMissesCounters = new ConcurrentDictionary<string, NumberOfItems64PerformanceCounter>();
            SizeCallbacks = new ConcurrentDictionary<string, Func<long>>();
            HitsAndMissesCallbacks = new ConcurrentDictionary<string, Func<HitsAndMisses>>();
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~PerformanceCounterLoggingCacheMetricReporter()
        {
            CleanUpCounters();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            CleanUpCounters();
            SizeCallbacks.Clear();
            HitsAndMissesCallbacks.Clear();
        }

        /// <summary>
        /// Maps the cache name to the size performance counter.
        /// </summary>
        internal ConcurrentDictionary<string, NumberOfItems64PerformanceCounter> SizeCounters { get; private set; }

        /// <summary>
        /// Maps the cache name to the hit rate performance counter.
        /// </summary>
        internal ConcurrentDictionary<string, PercentageRatePerformanceCounter> HitRateCounters { get; private set; }

        /// <summary>
        /// Maps the cache name to the total hits performance counter.
        /// </summary>
        internal ConcurrentDictionary<string, NumberOfItems64PerformanceCounter> TotalHitsCounters { get; private set; }

        /// <summary>
        /// Maps the cache name to the total misses performance counter.
        /// </summary>
        internal ConcurrentDictionary<string, NumberOfItems64PerformanceCounter> TotalMissesCounters { get; private set; }

        /// <summary>
        /// The performance counter category.
        /// </summary>
        public IMultiInstancePerformanceCounterCategory Category { get; private set; }

        /// <summary>
        /// Callbacks to get the cache sizes.
        /// </summary>
        public ConcurrentDictionary<string, Func<long>> SizeCallbacks { get; private set; }

        /// <summary>
        /// Callbacks to get the cache hit and miss rates.
        /// </summary>
        public ConcurrentDictionary<string, Func<HitsAndMisses>> HitsAndMissesCallbacks { get; private set; }

        /// <summary>
        /// Update the cache size counter.
        /// </summary>
        protected internal void UpdateHitCounter(string name, long hits, long misses)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            PercentageRatePerformanceCounter hitRateCounter;
            NumberOfItems64PerformanceCounter totalHitsCounter;
            NumberOfItems64PerformanceCounter totalMissesCounter;

            hitRateCounter = HitRateCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, name)
            );
            hitRateCounter.AddHits(hits);
            hitRateCounter.AddMisses(misses);

            totalHitsCounter = TotalHitsCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, name)
            );
            totalHitsCounter.IncrementBy(hits);

            totalMissesCounter = TotalMissesCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, name)
            );
            totalMissesCounter.IncrementBy(misses);
        }

        /// <summary>
        /// Update the cache size counter.
        /// </summary>
        protected internal void UpdateSizeCounter(string name, long size)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            NumberOfItems64PerformanceCounter sizeCounter; 

            sizeCounter = SizeCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, name)
            );
            sizeCounter.SetValue(size);
        }

        /// <summary>
        /// Set the hit and miss rate counters to 0.
        /// </summary>
        protected internal void ZeroHitCounter(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            PercentageRatePerformanceCounter hitRateCounter;
            NumberOfItems64PerformanceCounter totalHitsCounter;
            NumberOfItems64PerformanceCounter totalMissesCounter;

            hitRateCounter = HitRateCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, name)
            );
            hitRateCounter.Zero();

            totalHitsCounter = TotalHitsCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, name)
            );
            totalHitsCounter.SetValue(0);

            totalMissesCounter = TotalMissesCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, name)
            );
            totalMissesCounter.SetValue(0);            
        }

        /// <summary>
        /// Set the size counters to 0.
        /// </summary>
        protected internal void ZeroSizeCounter(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            NumberOfItems64PerformanceCounter sizeCounter;

            sizeCounter = SizeCounters.GetOrAdd(
                name,
                n => Category.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, name)
            );
            sizeCounter.SetValue(0);
        }

        /// <summary>
        /// Clean up any cached performance counters.
        /// </summary>
        private void CleanUpCounters()
        {
            foreach (NumberOfItems64PerformanceCounter sizePerformanceCounter in SizeCounters.Values)
            {
                sizePerformanceCounter.Dispose();
            }
            foreach (PercentageRatePerformanceCounter percentageRatePerformanceCounter in HitRateCounters.Values)
            {
                percentageRatePerformanceCounter.Dispose();
            }
            foreach (NumberOfItems64PerformanceCounter totalHitsPerformanceCounter in TotalHitsCounters.Values)
            {
                totalHitsPerformanceCounter.Dispose();
            }
            foreach (NumberOfItems64PerformanceCounter totalMissesPerformanceCounter in TotalMissesCounters.Values)
            {
                totalMissesPerformanceCounter.Dispose();
            }
        }

        /// <summary>
        /// Add a callback to get the named cache's current size.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="getSize">
        /// A function that returns the cache size. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// <paramref name="getSize"/> cannot be null.
        /// </exception>
        public void AddSizeCallback(string name, Func<long> getSize)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            if (getSize == null)
            {
                throw new ArgumentNullException("getSize");
            }

            SizeCallbacks.AddOrUpdate(name, n => getSize, (n, gs) => getSize);
            ZeroSizeCounter(name);
        }

        /// <summary>
        /// Add a callback to get the named cache's hit and miss counts since the last call.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="getHitsAndMisses">
        /// A function that returns the hit and miss counts. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// <paramref name="getHitsAndMisses"/> cannot be null.
        /// </exception>
        public void AddHitsAndMissesCallback(string name, Func<HitsAndMisses> getHitsAndMisses)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            if (getHitsAndMisses == null)
            {
                throw new ArgumentNullException("getHitsAndMisses");
            }

            HitsAndMissesCallbacks.AddOrUpdate(name, n => getHitsAndMisses, (n, ghm) => getHitsAndMisses);
            ZeroHitCounter(name);
        }

        /// <summary>
        /// Remove the size callback added through <see cref="AddSizeCallback"/>.
        /// Do nothing if there is no callback for <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        public void RemoveSizeCallback(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            Func<long> oldGetSize;
            SizeCallbacks.TryRemove(name, out oldGetSize);
        }

        /// <summary>
        /// Remove the hits and misses callback through <see cref="RemoveHitsAndMissesCallback"/>.
        /// Do nothing if there is no callback for <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        public void RemoveHitsAndMissesCallback(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            Func<HitsAndMisses> oldGetHitsAndMisses;
            HitsAndMissesCallbacks.TryRemove(name, out oldGetHitsAndMisses);
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
        public abstract void NotifySizeChange(string name);

        /// <summary>
        /// Notify the metric reporter that there are more hits and misses.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        public abstract void NotifyHitsAndMissesChange(string name);
    }
}
