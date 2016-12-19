// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Threading;
using EDC.Monitoring;
using EDC.Monitoring.Cache;

namespace EDC.Cache.Providers.MetricRepositories
{
    /// <summary>
    /// Background thread to handle updating perfmon counters for <see cref="LoggingCache{TKey, TValue}"/>
    /// </summary>
    internal class AsynchronousLoggingCacheMetricReporter : PerformanceCounterLoggingCacheMetricReporter
    {
        /// <summary>
        /// Create a new <see cref="AsynchronousLoggingCacheMetricReporter"/> using the default performance counter category.
        /// </summary>
        public AsynchronousLoggingCacheMetricReporter()
            : this(new MultiInstancePerformanceCounterCategory(CachePerformanceCounters.CategoryName))
        {
            // Do nothing    
        }

        /// <summary>
        /// Create a new <see cref="AsynchronousLoggingCacheMetricReporter"/> using the given <see cref="IMultiInstancePerformanceCounterCategory"/>.
        /// </summary>
        /// <param name="category">
        /// The performance counter category. This cannot be null.
        /// </param>
        /// <param name="startThread">
        /// True if the background thread should be started, false if not. This option is only used for testing.
        /// </param>
        /// <param name="logHitRates">
        /// True if hit rates should be logged, false otherwise.
        /// </param>
        public AsynchronousLoggingCacheMetricReporter(IMultiInstancePerformanceCounterCategory category, bool startThread = true, bool logHitRates = false)
            : base(category)
        {
            Stopping = false;
            ThreadSyncObject = new object();
            BackgroundThread = new Thread(ThreadStart);
            LogHitRates = logHitRates;
            if (startThread)
            {
                BackgroundThread.IsBackground = true;
                BackgroundThread.Start(this);
            }
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~AsynchronousLoggingCacheMetricReporter()
        {
            StopBackgroundThread();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        public override void Dispose()
        {
            StopBackgroundThread();
            base.Dispose();
        }

        /// <summary>
        /// The background thread.
        /// </summary>
        internal Thread BackgroundThread{ get; }

        /// <summary>
        /// Whether to log hit rates.
        /// </summary>
        internal bool LogHitRates { get; }

        /// <summary>
        /// Synchronization object used to inform background thread to report new metrics.
        /// </summary>
        internal object ThreadSyncObject { get; }

        /// <summary>
        /// Is the background thread stopping?
        /// </summary>
        public bool Stopping { get; private set; }

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
                throw new ArgumentNullException( nameof( name ) );
            }

            lock (ThreadSyncObject)
            {
                Monitor.Pulse(ThreadSyncObject);
            }
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
                throw new ArgumentNullException( nameof( name ) );
            }

            if (LogHitRates)
            {
                lock (ThreadSyncObject)
                {
                    Monitor.Pulse(ThreadSyncObject);
                }
            }
        }

        /// <summary>
        /// Flush background updates.
        /// </summary>
        public void Flush()
        {
            lock (ThreadSyncObject)
            {
                Monitor.Pulse(ThreadSyncObject);
            }
        }

        /// <summary>
        /// Stop the background thread. If it is already stopped, do nothing.
        /// </summary>
        private void StopBackgroundThread()
        {
            if (BackgroundThread.IsAlive)
            {
                Stopping = true;
                Flush();
                BackgroundThread.Join();
            }
        }

        /// <summary>
        /// Background thread.
        /// </summary>
        internal static void ThreadStart(object o)
        {
            AsynchronousLoggingCacheMetricReporter metricRepository;

            metricRepository = o as AsynchronousLoggingCacheMetricReporter;
            if (metricRepository != null)
            {
                while (!metricRepository.Stopping)
                {
                    lock (metricRepository.ThreadSyncObject)
                    {
                        Monitor.Wait(metricRepository.ThreadSyncObject);
                    }

                    UpdateSizes(metricRepository);
                    UpdateHitRates(metricRepository);
                }

                // Final update to ensure values are correct
                UpdateSizes(metricRepository);
                UpdateHitRates(metricRepository);
            }
        }

        internal static void UpdateSizes(AsynchronousLoggingCacheMetricReporter metricRepository)
        {
            foreach (KeyValuePair<string, Func<long>> keyValuePair in metricRepository.SizeCallbacks)
            {
                metricRepository.UpdateSizeCounter(keyValuePair.Key, keyValuePair.Value());
            }
        }

        internal static void UpdateHitRates(AsynchronousLoggingCacheMetricReporter metricRepository)
        {
            HitsAndMisses current;

            foreach(KeyValuePair<string, Func<HitsAndMisses>> keyValuePair in metricRepository.HitsAndMissesCallbacks)
            {
                current = keyValuePair.Value();
                metricRepository.UpdateHitCounter(keyValuePair.Key, current.Hits, current.Misses);
            }
        }
    }
}
