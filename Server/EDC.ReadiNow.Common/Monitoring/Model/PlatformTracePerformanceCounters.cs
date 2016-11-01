// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Diagnostics;
using EDC.Monitoring;

namespace EDC.ReadiNow.Monitoring.Model
{
    /// <summary>
    /// Performance counters used caches.
    /// </summary>
    public class PlatformTracePerformanceCounters
    {
        /// <summary>
        /// Access control counters.
        /// </summary>
        public static readonly string CategoryName = PerformanceCounterConstants.SoftwarePlatformCategoryPrefix + "PlatformTrace";
        private static readonly string CategoryHelp = "Software platform platform trace counters.";

        /// <summary>
        /// hit rate.
        /// </summary>
        public static readonly string EntityCacheHitRateCounterName = "Entity Cache Hit Rate";
        private static readonly string EntityCacheHitRateCounterHelp = "Number of cache hits per second";

        /// <summary>
        /// hit rate.
        /// </summary>
        public static readonly string EntityCacheMissRateCounterName = "Entity Cache Miss Rate";
        private static readonly string EntityCacheMissRateCounterHelp = "Number of entity cache misses per second";

        /// <summary>
        /// Run rate.
        /// </summary>
        public static readonly string EntitySaveRateCounterName = "Entity Save  Rate";
        private static readonly string EntitySaveRateCounterHelp = "Number of entity saves per second";

        /// <summary>
        /// Run rate.
        /// </summary>
        public static readonly string IpcBroadcastRateCounterName = "Ipc Broadcast Rate";
        private static readonly string IpcBroadcastRateCounterHelp = "Number of Ipc broadcasts per second";

       
        /// <summary>
        /// Create a new <see cref="PlatformTracePerformanceCounters"/>.
        /// </summary>
        public PlatformTracePerformanceCounters()
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
                .AddRatePerSecond32(EntityCacheHitRateCounterName, EntityCacheHitRateCounterHelp)
                .AddRatePerSecond32(EntityCacheMissRateCounterName, EntityCacheMissRateCounterHelp)
                .AddRatePerSecond32(EntitySaveRateCounterName, EntitySaveRateCounterHelp)
                .AddRatePerSecond32(IpcBroadcastRateCounterName, IpcBroadcastRateCounterHelp)
                .CreateCategory(CategoryName, CategoryHelp, PerformanceCounterCategoryType.SingleInstance);
        }
    }
}
