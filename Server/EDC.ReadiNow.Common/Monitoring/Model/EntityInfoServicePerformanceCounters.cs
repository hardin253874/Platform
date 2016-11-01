// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Diagnostics;
using EDC.Monitoring;

namespace EDC.ReadiNow.Monitoring.Model
{
    /// <summary>
    /// Performance counters used caches.
    /// </summary>
    public class EntityInfoServicePerformanceCounters
    {
        /// <summary>
        /// Access control counters.
        /// </summary>
        public static readonly string CategoryName = PerformanceCounterConstants.SoftwarePlatformCategoryPrefix + "EntityInfo";
        private static readonly string CategoryHelp = "Software platform platform trace counters.";

        public static readonly string RequestCountersPrefix = "Request";
        public static readonly string RequestCountersHelp = "entity info service requests";

        public static readonly string BulkSqlQueryCachePrefix = "BulkSqlQueryCache";
        public static readonly string BulkSqlQueryCacheHelp = "BulkSqlQueryCache";

        public static readonly string BulkResultCachePrefix = "BulkResultCache";
        public static readonly string BulkResultCacheHelp = "BulkSqlQueryCache";


        /// <summary>
        /// Create a new <see cref="PlatformTracePerformanceCounters"/>.
        /// </summary>
        public EntityInfoServicePerformanceCounters()
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
                .AddCodeBlockCounters(RequestCountersPrefix, RequestCountersHelp)
                .CreateCategory(CategoryName, CategoryHelp, PerformanceCounterCategoryType.SingleInstance);
        }
    }
}
