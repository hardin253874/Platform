// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Diagnostics;
using EDC.Monitoring;

namespace EDC.ReadiNow.Monitoring.AccessControl
{
    /// <summary>
    /// Permission specific (by instance) performance counters.
    /// </summary>
    public class AccessControlPermissionPerformanceCounters
    {
        /// <summary>
        /// Access control counters.
        /// </summary>
        public static readonly string CategoryName = AccessControlPerformanceCounters.CategoryName 
            + " Permission Checks";

        /// <summary>
        /// Description and help message for the access control counters.
        /// </summary>
        private static readonly string CategoryHelp = "Software platform access control counters.";

        /// <summary>
        /// Permission check rate.
        /// </summary>
        public static readonly string RateCounterName = "Check Rate";

        /// <summary>
        /// Permission check help.
        /// </summary>
        private static readonly string RateCounterHelp = "Access control checks broken down by permission";

        /// <summary>
        /// Permission check rate.
        /// </summary>
        public static readonly string CountCounterName = "Count";

        /// <summary>
        /// Permission check help.
        /// </summary>
        private static readonly string CountCounterHelp = "Total number of access control checks broken down by permission";

        /// <summary>
        /// Create a new <see cref="AccessControlPermissionPerformanceCounters"/>.
        /// </summary>
        public AccessControlPermissionPerformanceCounters()
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
                .AddRatePerSecond32(RateCounterName, RateCounterHelp)
                .AddNumberOfItems64(CountCounterName, CountCounterHelp)
                .CreateCategory(CategoryName, CategoryHelp, PerformanceCounterCategoryType.MultiInstance);
        }
    }
}
