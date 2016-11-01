// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // type: reportColumn
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType("Column", Namespace = Constants.StructuredQueryNamespace)]
    public class SelectColumn
    {
        public SelectColumn()
        {
            ColumnId = Guid.NewGuid();
        }

        /// <summary>
        /// An identifier for this column within this query. Can be used by mechanism such as conditional formatting to identify a
        /// column, but otherwise has no further meaning.
        /// </summary>
        // not in entity model
        [XmlAttribute("id")]
        [DataMember(Order = 1)]
        public Guid ColumnId
        {
            get { return StructuredQueryHashingContext.GetGuid( _columnId ); }
            set { _columnId = value; }
        }

        [XmlIgnore]
        private Guid _columnId;

        /// <summary>
        /// The data to display in this select column.
        /// </summary>
        // rel: columnExpression
        [DataMember(Order = 2)]
        public ScalarExpression Expression { get; set; }

        /// <summary>
        /// The programatic column alias given in the generated SQL.
        /// </summary>
        // not in entity model
        [DataMember(Order = 3)]
        public string ColumnName { get; set; }

        /// <summary>
        /// The recommended name to appear to the user.
        /// Does not participate in SQL generation.
        /// </summary>
        // field: name
        [DataMember(Order = 4)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Indicates whether the column is intended to be hidden.
        /// Does not participate in SQL generation.
        /// </summary>
        // field: columnIsHidden
        [DataMember(Order = 5)]
        [DefaultValue(false)]
        public bool IsHidden { get; set; }

        [DataMember(Order = 6)]
        [XmlIgnore]
        public long EntityId;
    }
}
