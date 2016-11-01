// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.Reporting.Result;

namespace ReadiNow.Reporting
{
    /// <summary>
    /// A container object that allow us to start processing a report..
    /// .. return half way .. then finish the job.
    /// </summary>
    public class ReportCompletionData
    {
        ReportResult _preparedResult;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportCompletionData( )
        {
        }

        /// <summary>
        /// Constructor for if the result is fully prepared in advance.
        /// </summary>
        /// <param name="preparedResult"></param>
        public ReportCompletionData( ReportResult preparedResult )
        {
            if ( preparedResult == null )
                throw new ArgumentNullException( "preparedResult" );
            _preparedResult = preparedResult;
        }

        /// <summary>
        /// A key that can be used to cache results, or null if the results are not cacheable.
        /// </summary>
        public ReportResultCacheKey ResultCacheKey { get; set; }

        /// <summary>
        /// The cache context that was active during the preparation phase.
        /// </summary>
        public CacheContext CacheContextDuringPreparation { get; set; }

        public ReportResult PerformRun( )
        {
            if ( _preparedResult != null )
                return _preparedResult;

            if ( ResultCallback != null )
            {
                ReportResult result = ResultCallback( );
                return result;
            }

            throw new InvalidOperationException( );
        }

        public Func<ReportResult> ResultCallback { get; set; }

    }
}
