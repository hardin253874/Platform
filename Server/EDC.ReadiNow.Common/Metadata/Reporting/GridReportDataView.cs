// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using System.Runtime.Serialization;
using Autofac;
using EDC.Core;
using EDC.ReadiNow.Metadata.Reporting.Helpers;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Metadata.Reporting
{
    /// <summary>
    /// This class represents a grid report data view.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class GridReportDataView : ReportDataView
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridReportDataView"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">Invalid report object to load the report</exception>
        public GridReportDataView(Report report)
        {
            if (report == null)
            {
                throw new ArgumentNullException();
            }

            // Get a conversion context
            var converter = Factory.Current.Resolve<IReportToQueryPartsConverter>();
            IReportToQueryContext context = converter.CreateContext( report );

            // If the report to load up the structured query from is still not an entity model thing then something bad has happened so throw up.
            if ( report.RootNode == null)
            {
                throw new ArgumentException("Invalid report object to load the report");
            }
            // Build the list of report column formats
            ColumnFormats = ReportingEntityHelper.BuildColumnFormats( report.ReportColumns, context);

            // Then build the list of grouped columns (for client aggregates soon to be just aggregates) 
            // This functionality is not defined just yet so we will leave it for now)

            // Done.
        }

        /// <summary>
        /// Constructs a new instance of a GridReportDataView object.
        /// </summary>        
        public GridReportDataView():
            base()
        {
            ColumnFormats = new List<ColumnFormatting>();
            GroupedQueryColumnIds = new List<Guid>();
        }
        #endregion


        /// <summary>
        /// The list of column formats for this grid report data view.
        /// </summary>
        [DataMember]
        public IList<ColumnFormatting> ColumnFormats { get; set; }


        /// <summary>
        /// The list of grouped columns
        /// </summary>
        [DataMember]
        public IList<Guid> GroupedQueryColumnIds { get; set; }
    }
}
