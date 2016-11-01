// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting
{
    /// <summary>
    /// This class represents a matirx report data view.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class MatrixReportDataView : ReportDataView
    {
         #region Constructor
        /// <summary>
        /// Constructs a new instance of a MatrixReportDataView object.
        /// </summary>        
        public MatrixReportDataView() :
            base()
        {
            ColumnFormats = new List<ColumnFormatting>();
            ColumnHeaderColumnIds = new List<Guid>();
            RowHeaderColumnIds = new List<Guid>();
            MatrixHeadersVisibility = Reporting.MatrixHeadersVisibility.Both;
        }
        #endregion


        /// <summary>
        /// The list of column formats for this grid report data view.
        /// </summary>
        [DataMember]
        public IList<ColumnFormatting> ColumnFormats { get; set; }


        /// <summary>
        /// The list of column header columns
        /// </summary>
        [DataMember]
        public IList<Guid> ColumnHeaderColumnIds { get; set; }

        /// <summary>
        /// The list of row header columns
        /// </summary>
        [DataMember]
        public IList<Guid> RowHeaderColumnIds { get; set; }


        /// <summary>
        /// Gets or sets the matrix headers visibility.
        /// </summary>
        /// <value>
        /// The matrix headers visibility.
        /// </value>
        [DataMember]
        public MatrixHeadersVisibility MatrixHeadersVisibility { get; set; }
    }
}
