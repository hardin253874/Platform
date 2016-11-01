// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Reporting;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Captures all information that can be extracted from a converted report entity model.
    /// </summary>
    public class ReportInfo
    {
        /// <summary>
        /// The main query object.
        /// </summary>
        public StructuredQuery StructuredQuery { get; set; }

        /// <summary>
        /// Holds the style and presentation information for the report.
        /// </summary>
        public GridReportDataView GridView { get; set; }

        /// <summary>
        /// Represents the contents of the analyzer
        /// </summary>
        public AnalyzerInfo Analyzer { get; set; }
    }


    /// <summary>
    /// Represents the contents of the analyzer
    /// </summary>
    public class AnalyzerInfo
    {
        public AnalyzerInfo()
        {
            AnalyzerFields = new List<ReportAnalyzerField>();
        }

        /// <summary>
        /// Represents the fields of the analyzer
        /// </summary>
        public IList<ReportAnalyzerField> AnalyzerFields { get; set; }
    }
}
