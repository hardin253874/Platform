// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Monitoring.Cache
{
    /// <summary>
    /// Cache (per instance) performance counters.
    /// </summary>
    public class CachePerformanceCounters
    {
        /// <summary>
        /// Category.
        /// </summary>
        public static readonly string CategoryName = PerformanceCounterConstants.SoftwarePlatformCategoryPrefix + "Caches";

        /// <summary>
        /// Category help.
        /// </summary>
        private static readonly string CategoryHelp = "Software platform cache counters.";

        /// <summary>
        /// Cache size (in number of entries).
        /// </summary>
        public static readonly string SizeCounterName = "Cache Size";

        /// <summary>
        /// Cache size help.
        /// </summary>
        private static readonly string SizeCounterHelp = "Number of entries in the cache";

        /// <summary>
        /// Cache hit rate.
        /// </summary>
        public static readonly string HitRateCounterName = "Hit Rate";

        /// <summary>
        /// Cache hit rate help.
        /// </summary>
        private static readonly string HitRateCounterHelp = "Cache hit rate";

        /// <summary>
        /// Cache hit rate.
        /// </summary>
        public static readonly string TotalHitsCounterName = "Total Hits";

        /// <summary>
        /// Cache hit rate help.
        /// </summary>
        private static readonly string TotalHitsCounterHelp = "Total hits over the life of the cache";

        /// <summary>
        /// Cache hit rate.
        /// </summary>
        public static readonly string TotalMissesCounterName = "Total Misses";

        /// <summary>
        /// Cache hit rate help.
        /// </summary>
        private static readonly string TotalMissesCounterHelp = "Total misses over the life of the cache";

        /// <summary>
        /// Create the performance counters if they have not already been registered.
        /// Note the calling user must be a member of the local administrators group.
        /// </summary>
        public void CreateCategory()
        {
            new PerformanceCounterCategoryFactory()
                .AddNumberOfItems64(SizeCounterName, SizeCounterHelp)
                .AddPercentageRate(HitRateCounterName, HitRateCounterHelp)
                .AddNumberOfItems64(TotalHitsCounterName, TotalHitsCounterHelp)
                .AddNumberOfItems64(TotalMissesCounterName, TotalMissesCounterHelp)
                .CreateCategory(CategoryName, CategoryHelp, PerformanceCounterCategoryType.MultiInstance);
        }
    }
}
