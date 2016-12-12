// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using ReadiNow.Reporting.Definitions;

namespace ReadiNow.Reporting.Result
{
    /// <summary>
    /// Top level object for static metadata about the report.
    /// </summary>
    public class ReportMetadata
    {
        /// <summary>
        /// Gets or sets the report title.
        /// </summary>
        /// <value>The report title.</value>
        public string ReportTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide add button].
        /// </summary>
        /// <value><c>true</c> if [hide add button]; otherwise, <c>false</c>.</value>
        public bool HideAddButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide new button].
        /// </summary>
        /// <value><c>true</c> if [hide new button]; otherwise, <c>false</c>.</value>
        public bool HideNewButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide delete button].
        /// </summary>
        /// <value><c>true</c> if [hide delete button]; otherwise, <c>false</c>.</value>
        public bool HideDeleteButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide action menu].
        /// </summary>
        /// <value><c>true</c> if [hidden action menu]; otherwise, <c>false</c>.</value>
        public bool HideActionBar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide report header].
        /// </summary>
        /// <value><c>true</c> if [show report header]; otherwise, <c>false</c>.</value>
        public bool HideReportHeader { get; set; }

        /// <summary>
        /// Gets or sets the report style.
        /// </summary>
        /// <value>report style</value>
        public string ReportStyle { get; set; }

        /// <summary>
        /// Gets or sets the default form.
        /// </summary>
        /// <value>The default form.</value>
        public long? DefaultFormId { get; set; }

        /// <summary>
        /// Gets or sets the resource viewer form id.
        /// </summary>
        /// <value>The default form.</value>
        public long? ResourceViewerFormId { get; set; }

        /// <summary>
        /// Gets or sets the default data view unique identifier.
        /// </summary>
        /// <value>The default data view unique identifier.</value>
        public string DefaultReportViewId { get; set; }

        /// <summary>
        /// Gets or sets the formats for the reported data types.
        /// </summary>
        /// <value>The type formats.</value>
        public Dictionary<string, List<ConditionalFormatStyleEnum>> TypeConditionalFormatStyles { get; set; }

        /// <summary>
        /// Gets or sets the report columns.
        /// </summary>
        /// <value>The report columns.</value>
        public Dictionary<string, ReportColumn> ReportColumns { get; set; }

        /// <summary>
        /// Gets or sets the sort orders.
        /// </summary>
        /// <value>The sort orders.</value>
        public List<ReportSortOrder> SortOrders { get; set; }

        /// <summary>
        /// Gets or sets the choice selections.
        /// </summary>
        /// <value>The choice selections.</value>
        public Dictionary<long, List<ChoiceItemDefinition>> ChoiceSelections { get; set; }

        /// <summary>
        /// Gets or sets the inline report pickers.
        /// </summary>
        /// <value>The inline report pickers.</value>
        public Dictionary<long, long> InlineReportPickers { get; set; }

        /// <summary>
        /// Gets or sets the analyser columns.
        /// </summary>
        /// <value>The analyser columns.</value>
        public Dictionary<string, ReportAnalyserColumn> AnalyserColumns { get; set; }

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
        /// Gets or sets the format value type selectors.
        /// </summary>
        /// <value>The format value type selectors.</value>
        public Dictionary<string, Dictionary<string, string>> FormatValueTypeSelectors { get; set; }

        /// <summary>
        /// Gets or sets the aggregate metadata.
        /// </summary>
        /// <value>The aggregate metadata.</value>
        public ReportMetadataAggregate AggregateMetadata { get; set; }

        /// <summary>
        /// Gets or sets the aggregate data.
        /// </summary>
        /// <value>The aggregate data.</value>
        public List<ReportDataAggregate> AggregateData { get; set; }

        /// <summary>
        /// Gets or sets the  Invalid Report information.
        /// </summary>
        /// <value>The Invalid Report information.</value>
        public Dictionary<string, Dictionary<long, string>> InvalidReportInformation { get; set; }

        /// <summary>
        /// Gets or sets the time that the report was last modified.
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// The report alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        public string Alias { get; set; }
    }
}
