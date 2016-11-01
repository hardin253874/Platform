// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Settings for controlling the behavior of the ReportToQueryConverter.
    /// </summary>
    public class ReportToQueryConverterSettings
    {
        /// <summary>
        /// If true, indicates that the converter should not attempt to preload the query - because it has already been done.
        /// </summary>
        public bool SuppressPreload { get; set; }

        /// <summary>
        /// If true, indicates that the converter should only be concerned with converting filter aspects of the query (and only invalidating on those).
        /// That is, the consumer (e.g. security) is not interested in the generated column list, or ordering information.
        /// </summary>
        public bool ConditionsOnly { get; set; }

        /// <summary>
        /// If true, request that any cached conversion result be recalculated and recached.
        /// </summary>
        public bool RefreshCachedStructuredQuery { get; set; }

        /// <summary>
        /// If true, indicates that the converter should only convert the report object to structuredQuery object with invalid information.        
        /// </summary>
        public bool SchemaOnly { get; set; }
        /// <summary>
        /// Default settings.
        /// </summary>
        public static ReportToQueryConverterSettings Default = new ReportToQueryConverterSettings( );
    }
}
