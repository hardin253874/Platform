// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Monitoring
{
    /// <summary>
    /// Extension methods, etc for performance counters.
    /// </summary>
    public static class PerformanceCounterHelpers
    {
		/// <summary>
		/// Register all counters associated with a PerformanceCounters.Measure using block.
		/// </summary>
		/// <param name="factory">The counter category factory.</param>
		/// <param name="blockPrefix">The prefix string to the counter names.</param>
		/// <param name="blockHelp">The block help.</param>
		/// <returns></returns>
        public static IPerformanceCounterCategoryFactory AddCodeBlockCounters(this IPerformanceCounterCategoryFactory factory, string blockPrefix, string blockHelp)
        {
            return factory
                .AddRatePerSecond32(blockPrefix + PerformanceCounters.RateSuffix, "Number of " + blockHelp + " per second")
                .AddNumberOfItems64(blockPrefix + PerformanceCounters.CountSuffix, "Total number of " + blockHelp)
                .AddAverageTimer32(blockPrefix + PerformanceCounters.DurationSuffix, "Average duration of " + blockHelp);               

        }
    }
}
