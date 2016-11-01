// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.ReportConverter
{
    /// <summary>
    /// Cache value container for CachingReportToQueryConverter
    /// </summary>
    internal class CachingReportToQueryConverterValue
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CachingReportToQueryConverterValue( StructuredQuery structuredQuery )
        {
            if ( structuredQuery == null )
                throw new ArgumentNullException( "structuredQuery" );

            StructuredQuery = structuredQuery;
            CacheTime = DateTime.Now;
        }

        /// <summary>
        /// The query originally used to generate the cached result.
        /// </summary>
        public StructuredQuery StructuredQuery
        {
            get;
            private set;
        }

        /// <summary>
        /// The cached result.
        /// </summary>
        public DateTime CacheTime
        {
            get;
            private set;
        }
    }
}
