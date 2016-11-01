// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Monitoring
{
    /// <summary>
    /// Constants used for performance counters.
    /// </summary>
    public static class PerformanceCounterConstants
    {
        /// <summary>
        /// The name that should prefix any software platform performance category names.
        /// </summary>
        public static readonly string SoftwarePlatformCategoryPrefix = "Software Platform ";

        /// <summary>
        /// Suffix added to base performance counters. 
        /// </summary>
        public static readonly string BaseSuffix = " Base";

        /// <summary>
        /// Total instance for multi-instance counters.
        /// </summary>
        public static readonly string Total = "_Total";
    }
}
