// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Metadata.Reporting
{
    /// <summary>
    /// This class represents a chart data view.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ChartDataView : ReportDataView
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartDataView"/> class.
        /// </summary>
        public ChartDataView() :
            base()
        {
            DependentAxisColumnIds = new List<Guid>();
        }        


        /// <summary>
        /// Gets or sets the type of the chart.
        /// </summary>
        /// <value>
        /// The type of the chart.
        /// </value>
        [DataMember]
        public ChartType ChartType { get; set; }

       
        /// <summary>
        /// Gets or sets a value indicating whether to show the grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> to show the grid; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowGrid { get; set; }


        /// <summary>
        /// Gets or sets the independent axis column id.
        /// </summary>
        /// <value>
        /// The independent axis column id.
        /// </value>
        [DataMember]
        public Guid IndependentAxisColumnId { get; set; }


        /// <summary>
        /// Gets or sets the dependent axis column ids.
        /// </summary>
        /// <value>
        /// The dependent axis column ids.
        /// </value>
        [DataMember]
        public List<Guid> DependentAxisColumnIds { get; set; }


        /// <summary>
        /// Gets or sets the dependent axis column ids.
        /// </summary>
        /// <value>
        /// The dependent axis column ids.
        /// </value>
        [DataMember]
        public List<ChartFormatting> DependentAxisColumns { get; set; }

        /// <summary>
        /// Gets or sets the minimum dependent axis value.
        /// </summary>
        /// <value>
        /// The minimum dependent axis value.
        /// </value>
        [DataMember]
        public double MinimumDependentAxisValue { get; set; }


        /// <summary>
        /// Gets or sets the maximum dependent axis value.
        /// </summary>
        /// <value>
        /// The maximum dependent axis value.
        /// </value>
        [DataMember]
        public double MaximumDependentAxisValue { get; set; }
        #endregion                
    }
}
