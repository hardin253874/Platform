// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Diagnostics;
using EDC.Monitoring;

namespace EDC.ReadiNow.Monitoring.Reports
{
    /// <summary>
    /// Performance counters used for reports.
    /// </summary>
    public class ReportsPerformanceCounters
    {
        /// <summary>
        /// Access control counters.
        /// </summary>
        public static readonly string CategoryName = PerformanceCounterConstants.SoftwarePlatformCategoryPrefix + "Reports";
        private static readonly string CategoryHelp = "Software platform reports counters.";

        /// <summary>
        /// Average run duration.
        /// </summary>
        public static readonly string RunCounterPrefix = "Run";
        private static readonly string RunCounterHelp = "report runs";

        /// <summary>
        /// Create a new <see cref="ReportsPerformanceCounters"/>.
        /// </summary>
        public ReportsPerformanceCounters()
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
                .AddCodeBlockCounters(RunCounterPrefix, RunCounterHelp)
                .CreateCategory(CategoryName, CategoryHelp, PerformanceCounterCategoryType.SingleInstance);
        }
    }
}
