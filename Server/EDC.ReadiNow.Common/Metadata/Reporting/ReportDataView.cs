// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting
{
    /// <summary>
    /// The base class for report data views.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [KnownType(typeof(GridReportDataView))]
    [KnownType(typeof(MatrixReportDataView))]
    [KnownType(typeof(ChartDataView))]
    public class ReportDataView
    {               
        #region Properties       
        /// <summary>
        /// The id of the report view.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }
        #endregion
    }
}
