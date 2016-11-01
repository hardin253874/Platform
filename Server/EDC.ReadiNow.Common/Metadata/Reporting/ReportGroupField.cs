// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EDC.ReadiNow.Metadata.Reporting
{
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum GroupMethod
    {
        [EnumMember]
        List,
        [EnumMember]
        Year,
        [EnumMember]
        Month,
        [EnumMember]
        YearMonth,
        [EnumMember]
        Day,
        [EnumMember]
        YearMonthDay,
        [EnumMember]
        Quarter,
        [EnumMember]
        Hour,
        [EnumMember]
        FirstLetter
    }

    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ReportGroupField
    {
        /// <summary>
        /// Gets or sets the report column id.
        /// </summary>
        /// <value>
        /// The report column id.
        /// </value>
        [DataMember]
        public Guid ReportColumnId
        {
            get { return StructuredQueryHashingContext.GetGuid( _reportColumnId ); }
            set { _reportColumnId = value; }
        }

        [XmlIgnore]
        private Guid _reportColumnId;

        /// <summary>
        /// Gets or sets the Aggregate Method
        /// </summary>
        /// <value>
        /// The Aggregate Method
        /// </value>
        [DataMember]
        public GroupMethod GroupMethod { get; set; }

        /// <summary>
        /// The flag to show sub total on aggregate expression
        /// </summary>
        [DataMember]
        public bool ShowSubTotals { get; set; }

        /// <summary>
        /// The flag to show grand total on aggregate expression
        /// </summary>
        [DataMember]
        public bool ShowGrandTotals { get; set; }

        [XmlIgnore]
        [DataMember]
        public bool ShowRowCounts { get; set; }

        [XmlIgnore]
        [DataMember]
        public bool ShowRowLabels { get; set; }
        
        [XmlIgnore]
        [DataMember]
        public bool ShowOptionLabel { get; set; }
        
        [XmlIgnore]
        public long ReportColumnEntityId { get; set; }
        
        [XmlIgnore]
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group <see cref="ReportGroupField"/> is collapsed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collapsed; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        [DataMember]
        public bool Collapsed { get; set; }
    }
}
