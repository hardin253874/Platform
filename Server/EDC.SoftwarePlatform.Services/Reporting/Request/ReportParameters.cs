// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using ReadiNow.Reporting.Definitions;

namespace ReadiNow.Reporting.Request
{
    public class ReportParameters
    {
        /// <summary>
        /// Gets or sets the analyser conditions used for running the report.
        /// </summary>
        /// <value>The analyser conditions.</value>
        public List<SelectedColumnCondition> AnalyserConditions { get; set; }

        /// <summary>
        /// Gets or sets the sort columns.
        /// </summary>
        /// <value>The sort columns.</value>
        public List<ReportSortOrder> SortColumns { get; set; }

        /// <summary>
        /// Gets or sets the conditional format rules.
        /// </summary>
        /// <value>The conditional format rules.</value>
        public Dictionary<string, ReportColumnConditionalFormat> ConditionalFormatRules { get; set; }

        /// <summary>
        /// Gets or sets the value format rules.
        /// </summary>
        /// <value>The value format rules.</value>
        public Dictionary<string, ReportColumnValueFormat> ValueFormatRules { get; set; }

        /// <summary>
        /// Gets or sets the group and aggregate rules.
        /// </summary>
        /// <value>The group and aggregate rules.</value>
        public ReportMetadataAggregate GroupAggregateRules { get; set; }

        /// <summary>
        /// Gets or sets the group and aggregate rules.
        /// </summary>
        /// <value>The group and aggregate rules.</value>
        public bool IsReset { get; set; }
    }
}
