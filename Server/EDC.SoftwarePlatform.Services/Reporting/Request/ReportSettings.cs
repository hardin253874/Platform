// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace ReadiNow.Reporting.Request
{
    /// <summary>
    /// Class ReportSettings.
    /// </summary>
    public class ReportSettings
    {
        // Full meta data including analyser choice values
        public bool RequireFullMetadata { get; set; }
        
        // Metadata with stuff omitted such as choice field enumerations
        public bool RequireBasicMetadata { get; set; }

        // Metadata with column information only. Used in picker reports
        public bool RequireColumnBasicMetadata { get; set; }

        // Metadata with report schema only. Used in report schema build
        public bool RequireSchemaMetadata { get; set; }

#region Paging Support
        /// <summary>
        /// Gets or sets a value indicating whether paging is required.
        /// </summary>
        /// <value><c>true</c> if paging support; otherwise, <c>false</c>.</value>
        public bool SupportPaging { get; set; }
        /// <summary>
        /// Gets or sets the initial row.
        /// </summary>
        /// <value>The initial row.</value>
        public int InitialRow { get; set; }
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize { get; set; }
#endregion Paging Support

        /// <summary>
        /// Gets or sets the column count.
        /// </summary>
        /// <value>The column count.</value>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the report parameters.
        /// </summary>
        /// <value>The report parameters.</value>
        public ReportParameters ReportParameters { get; set; }

        /// <summary>
        /// Gets or sets the timezone.
        /// </summary>
        /// <value>The timezone.</value>
        public TimeZoneInfo Timezone { get; set; }

        /// <summary>
        /// Gets or sets the type of the report configuration.
        /// </summary>
        /// <value>The type of the report configuration.</value>
        public long? ReportOnType { get; set; }

        ///// <summary>
        ///// Gets or sets the report relationship.
        ///// </summary>
        ///// <value>The report relationship.</value>
        public ReportRelationshipSettings ReportRelationship { get; set; }

        public string QuickSearch { get; set; }

		///// <summary>
        ///// Gets or sets the related entity filters.
        ///// </summary>
        ///// <value>The related entity filters.</value>
        public List<RelatedEntityFilterSettings> RelatedEntityFilters { get; set; }

        ///// <summary>
        ///// Gets or sets the filtered entity ids.
        ///// </summary>
        ///// <value>The filtered entity ids.</value>
        public List<long> FilteredEntityIdentifiers { get; set; }

        /// <summary>
        /// Indicates if we are permitting use of the structured query cache.
        /// </summary>
        /// <value>The timezone.</value>
        public bool UseStructuredQueryCache { get; set; }

        /// <summary>
        /// Indicates that we should refresh any cached report data.
        /// Intended for diagnostics.
        /// </summary>
        public bool RefreshCachedResult { get; set; }

        /// <summary>
        /// Indicates that we should refresh any cached generated SQL.
        /// Intended for diagnostics.
        /// </summary>
        public bool RefreshCachedSql { get; set; }

        /// <summary>
        /// Indicates that we should refresh any cached report-to-structured-query conversion.
        /// Intended for diagnostics.
        /// </summary>
        public bool RefreshCachedStructuredQuery { get; set; } 
        
        /// <summary>
        /// Limit the maximum CPU that the report run will consume while running the query
        /// </summary>
        public int CpuLimitSeconds { get; set; }
    }
}
