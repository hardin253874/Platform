// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Diagnostics;
using EDC.Monitoring;

namespace EDC.ReadiNow.Monitoring.AccessControl
{
    /// <summary>
    /// Performance counters used for access control cache invalidation.
    /// </summary>
    public class AccessControlCacheInvalidationPerformanceCounters
    {
        /// <summary>
        /// Access control counters.
        /// </summary>
        public static readonly string CategoryName = AccessControlPerformanceCounters.CategoryName 
            + " Cache Invalidation";

        /// <summary>
        /// Description and help message for the access control counters.
        /// </summary>
        private static readonly string CategoryHelp = "Software platform access control counters.";

        /// <summary>
        /// Cache invalidation rate name.
        /// </summary>
        public static readonly string RateCounterName = "Cache Invalidation Rate";

        /// <summary>
        /// Cache invalidation rate help.
        /// </summary>
        private static readonly string RateCounterHelp = "Cache invalidation rate broken down by type.";

        /// <summary>
        /// Cache invalidation count name.
        /// </summary>
        public static readonly string CountCounterName = "Cache Invalidation Count";

        /// <summary>
        /// Cache invalidation count help.
        /// </summary>
        private static readonly string CountCounterHelp = "Total cache invalidations broken down by type.";

        /// <summary>
        /// Whole cache instance.
        /// </summary>
        public static readonly string CacheInstanceName = "Entire Cache";

        /// <summary>
        /// Entity instance.
        /// </summary>
        public static readonly string EntityInstanceName = "Entity";

        /// <summary>
        /// Group instance.
        /// </summary>
        public static readonly string GroupInstanceName = "Group";

        /// <summary>
        /// Group instance.
        /// </summary>
        public static readonly string PermissionInstanceName = "Permission";

        /// <summary>
        /// Entity instance.
        /// </summary>
        public static readonly string RoleInstanceName = "Role";

        /// <summary>
        /// Group instance.
        /// </summary>
        public static readonly string TypeInstanceName = "Type";

        /// <summary>
        /// Group instance.
        /// </summary>
        public static readonly string UserInstanceName = "User";

        /// <summary>
        /// Create a new <see cref="AccessControlCacheInvalidationPerformanceCounters"/>.
        /// </summary>
        public AccessControlCacheInvalidationPerformanceCounters()
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
