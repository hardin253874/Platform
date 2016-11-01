// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Cache;
using EDC.ReadiNow.Security.AccessControl;
using ReadiNow.Reporting;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
    public class ReportResultCacheInvalidator : SecurityCacheInvalidatorBase<ReportResultCacheKey, string>
    {
        /// <summary>
        /// Create a new <see cref="ReportResultCacheInvalidator"/>.
        /// </summary>
        public ReportResultCacheInvalidator( ICache<ReportResultCacheKey, string> cache )
            : base( cache, "Report Result" )
        {
            // Do nothing
        }

    }
}