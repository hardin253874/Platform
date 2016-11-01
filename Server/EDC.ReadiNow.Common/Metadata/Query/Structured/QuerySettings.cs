// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Settings to be used when generating SQL for a given query.
    /// </summary>
    /// <remarks>
    /// IMPORTANT : Any additional fields should be considered for CachingQueryRunnerKey !!
    /// </remarks>
    public class QuerySettings : QuerySqlBuilderSettings
    {
        /// <summary>
        /// Zero-based index of first row to return. Requires SupportPaging=true.
        /// </summary>
        public int FirstRow { get; set; }

        /// <summary>
        /// Number of rows to return. Requires SupportPaging=true.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the quick search term.
        /// </summary>
        /// <value>The quick search term.</value>
        public string QuickSearchTerm { get; set; }

        /// <summary>
        /// List of IDs to filter the overall query to.
        /// </summary>
        public IEnumerable<long> RootIdFilterList { get; set; }

        /// <summary>
        /// Resource at this end of the relationship that a faux-relationships will point to.
        /// </summary>
        public long TargetResource { get; set; }

        /// <summary>
        /// List of resources to include when constraining a report to a resource by a relationship. (i.e. edit form tab)
        /// </summary>
        public IEnumerable<long> IncludeResources { get; set; }

        /// <summary>
        /// List of resources to exclude when constraining a report to a resource by a relationship. (i.e. edit form tab)
        /// </summary>
        public IEnumerable<long> ExcludeResources { get; set; }

        /// <summary>
        /// If true, request that any cached result be recalculated and recached.
        /// </summary>
        public bool RefreshCachedResult { get; set; }

        /// <summary>
        /// If true, get the result schema, but don't actually get data.
        /// </summary>
        public bool ResultSchemaOnly { get; set; }

        /// <summary>
        /// Possible values to match the valueList parameter.
        /// </summary>
        public IReadOnlyCollection<string> ValueList { get; set; }
    }
}
