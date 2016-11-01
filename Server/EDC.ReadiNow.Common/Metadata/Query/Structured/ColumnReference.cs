// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Used in circumstances where we want to use an existing column as an expression.
    /// Note: this is typically (but not necessarily) intended for use by interractive changes.
    /// For example, ad-hoc filtering on a column, or ad-hoc sorting a column.
    /// </summary>
    /// <remarks>
    /// The implementation is typically that the column is looked up and its own expression is substututed.
    /// </remarks>
    // type: columnReferenceExpression
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class ColumnReference : ScalarExpression
    {
        /// <summary>
        /// The LocalId of the column to reference.
        /// </summary>
        [DataMember(Order = 1)]
        public Guid ColumnId
        {
            get { return StructuredQueryHashingContext.GetGuid( _columnId ); }
            set { _columnId = value; }
        }

        [XmlIgnore]
        private Guid _columnId;
    }
}
