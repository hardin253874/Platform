// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Metadata.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ReportAggregateField
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
        public AggregateMethod AggregateMethod { get; set; }

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
        [DataMember] // does this need to be here? It might interfere with cache key hashes
        public long ReportColumnEntityId { get; set; }

        [XmlIgnore]
        [DataMember]
        public int Order { get; set; }

        [XmlIgnore]
        [DataMember]
        public bool IncludedCount { get; set; }
    }
}
