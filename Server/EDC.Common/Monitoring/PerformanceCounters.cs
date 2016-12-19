// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EDC.Monitoring
{
    /// <summary>
    /// Convenient using-block wrapper for common count/rate/duration performance pattern.
    /// </summary>
    public class PerformanceCounters : IDisposable
    {
        public static string CountSuffix = " Count";
        public static string RateSuffix = " Rate";
        public static string DurationSuffix = " Duration";

        /// <summary>
        /// Dictionary of counters by name.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ISingleInstancePerformanceCounterCategory> Categories = new ConcurrentDictionary<string, ISingleInstancePerformanceCounterCategory>();

        /// <summary>
        /// Duration counter name for the current block.
        /// </summary>
        private string _durationName;

        /// <summary>
        /// Stopwatch for the current block.
        /// </summary>
        private Stopwatch _stopWatch;

        /// <summary>
        /// Counter for the current block.
        /// </summary>
        private ISingleInstancePerformanceCounterCategory _perfCounters;

        /// <summary>
        /// Using block with a common prefix for all counter names.
        /// </summary>
        /// <param name="categoryName">The performance counter category name.</param>
        /// <param name="prefix">The prefix to the counter name, which will get followed by Count/Rate/Duration.</param>
        /// <returns>A disposable object for use in a using block.</returns>
        public static IDisposable Measure(string categoryName, string prefix)
        {
            return Measure(categoryName, prefix + CountSuffix, prefix + RateSuffix, prefix + DurationSuffix);
        }

        /// <summary>
        /// Using block with a common prefix for all counter names.
        /// </summary>
        /// <param name="categoryName">The performance counter category name.</param>
        /// <param name="countName">The name of the 'count' counter.</param>
        /// <param name="rateName">The name of the 'rate' counter.</param>
        /// <param name="durationName">The name of the 'duration' counter.</param>
        /// <returns>A disposable object for use in a using block.</returns>
        public static IDisposable Measure(string categoryName, string countName, string rateName, string durationName)
        {
            return new PerformanceCounters(categoryName, countName, rateName, durationName);
        }

        /// <summary>
        /// Private constructor to start the counter.
        /// </summary>
        private PerformanceCounters(string categoryName, string countName, string rateName, string durationName)
        {
            _durationName = durationName;

            _perfCounters = Categories.GetOrAdd(categoryName, name =>
                new SingleInstancePerformanceCounterCategory(name));

            _perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(rateName).Increment();
            _perfCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(countName).Increment();
            _stopWatch = new Stopwatch();
            _stopWatch.Start();            
        }

        /// <summary>
        /// Dispose to complete the timer counter.
        /// </summary>
        public void Dispose()
        {
            _stopWatch.Stop();
            _perfCounters.GetPerformanceCounter<AverageTimer32PerformanceCounter>(_durationName).AddTiming(_stopWatch);
        }
    }
}
