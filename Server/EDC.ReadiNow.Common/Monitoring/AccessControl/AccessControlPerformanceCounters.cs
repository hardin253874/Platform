// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Diagnostics;
using EDC.Monitoring;

namespace EDC.ReadiNow.Monitoring.AccessControl
{
    /// <summary>
    /// Performance counters used for access control.
    /// </summary>
    public class AccessControlPerformanceCounters
    {
        /// <summary>
        /// Access control counters.
        /// </summary>
        public static readonly string CategoryName = PerformanceCounterConstants.SoftwarePlatformCategoryPrefix + "Access Control";

        /// <summary>
        /// Description and help message for the access control counters.
        /// </summary>
        private static readonly string CategoryHelp = "Software platform access control counters.";

        /// <summary>
        /// Average check duration.
        /// </summary>
        public static readonly string CheckDurationCounterName = "Average Check Duration";

        /// <summary>
        /// Average check time help.
        /// </summary>
        private static readonly string CheckDurationCounterHelp = "Average duration of access control checks";

        /// <summary>
        /// Access control check rate.
        /// </summary>
        public static readonly string CheckRateCounterName = "Check Rate";

        /// <summary>
        /// Access control check rate.
        /// </summary>
        private static readonly string CheckRateCounterHelp = "Number of access control checks per second";

        /// <summary>
        /// Cache invalidation rate.
        /// </summary>
        public static readonly string CheckCountCounterName = "Check Count";

        /// <summary>
        /// Cache invalidation rate.
        /// </summary>
        private static readonly string CheckCountCounterHelp = "Total number of security checks";

        /// <summary>
        /// Cache invalidation rate.
        /// </summary>
        public static readonly string CacheHitPercentageCounterName = "Cache Hit Percentage";

        /// <summary>
        /// Cache invalidation rate.
        /// </summary>
        private static readonly string CacheHitPercentageCounterHelp = "Percentage of access control cache checks that were resolved from the cache";

        /// <summary>
        /// Create a new <see cref="AccessControlPerformanceCounters"/>.
        /// </summary>
        public AccessControlPerformanceCounters()
        {
           // Do nothing
        }

        /// <summary>
        /// Create the performance counters if they have not already been registered.
        /// Note the calling user must be a member of the local administrators group.
        /// </summary>
        public void CreateCategory()
        {
            new PerformanceCounterCategoryFactory()
                .AddAverageTimer32(CheckDurationCounterName, CheckDurationCounterHelp)
                .AddRatePerSecond32(CheckRateCounterName, CheckRateCounterHelp)
                .AddNumberOfItems64(CheckCountCounterName, CheckCountCounterHelp)
                .AddPercentageRate(CacheHitPercentageCounterName, CacheHitPercentageCounterHelp)
                .CreateCategory(CategoryName, CategoryHelp, PerformanceCounterCategoryType.SingleInstance);
        }
    }
}
