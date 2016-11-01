// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    /// <summary>
    /// Set of conditional formatting rules that collectively result in a format result for one value/column.
    /// </summary>
    public class ReportColumnConditionalFormat
    {
        /// <summary>
        /// Style of formatting.
        /// </summary>
        public ConditionalFormatStyleEnum Style { get; set; }

        /// <summary>
        /// Should the text value be overlayed on the formatted cell.
        /// </summary>
        public bool ShowValue { get; set; }

        /// <summary>
        /// Individual formatting rules.
        /// </summary>
        public List<ReportConditionalFormatRule> Rules { get; set; }
    }
}
