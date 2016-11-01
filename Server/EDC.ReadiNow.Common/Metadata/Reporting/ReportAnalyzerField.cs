// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ReportAnalyzerField
    {       
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        [DataMember]
        public string ColumnName { get; set; }


        /// <summary>
        /// Gets or sets the display name of the column.
        /// </summary>
        /// <value>
        /// The display name of the column.
        /// </value>
        [DataMember]
        public string DisplayName { get; set; }


        /// <summary>
        /// Gets or sets the query expression id.
        /// </summary>
        /// <value>
        /// The query expression id.
        /// </value>
        [DataMember]
        public Guid QueryExpressionId { get; set; }



        /// <summary>
        /// Gets or sets the name of the field type.
        /// </summary>
        /// <value>
        /// The name of the field type.
        /// </value>
        [DataMember]
        public string FieldTypeName { get; set; }



        /// <summary>
        /// Gets or sets the name of the field type.
        /// </summary>
        /// <value>
        /// The name of the field type.
        /// </value>
        [DataMember(EmitDefaultValue=false)]
        public bool IsHidden { get; set; }

        [XmlIgnore]
        public long EntityId { get; set; }
    }
}
