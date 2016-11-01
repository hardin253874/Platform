// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Cache;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace ReadiNow.QueryEngine.ReportConverter
{
    /// <summary>
    /// Coordinate invalidating the report to query cache.
    /// </summary>
    internal class ReportToQueryCacheInvalidator : CacheInvalidator<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue>
    {
        /// <summary>
        /// Create a new <see cref="ReportToQueryCacheInvalidator"/>.
        /// </summary>
        /// <param name="cache"></param>
        public ReportToQueryCacheInvalidator( ICache<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue> cache )
            : base(cache, "Report To Query")
        {
            // Do nothing
        }
    }
}
