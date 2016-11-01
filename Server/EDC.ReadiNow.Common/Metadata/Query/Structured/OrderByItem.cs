// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Represents the type of item to match and include in the query results.
    /// </summary>
    // enum not in entity model. see reverseOrder
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public enum OrderByDirection
    {
        /// <summary>
        /// Specifies an ascending order for the query results. This is the default value.
        /// </summary>
        [EnumMember]
        Ascending,

        /// <summary>
        /// Specifies a descending order for the query results.
        /// </summary>
        [EnumMember]
        Descending
    }


    /// <summary>
    /// Represents an item used to sort the final query result set.
    /// </summary>
    //reportOrderBy
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class OrderByItem
    {
        // field: orderPriority used to sort the items

        /// <summary>
        /// Optional. The expression to sort on.
        /// Either QueryColumnId or Expression must be set.
        /// </summary>
        // rel: orderByExpression
        [DataMember(Order = 1)]
        public ScalarExpression Expression { get; set; }

        /// <summary>
        /// Gets or sets the sorting direction.
        /// </summary>
        // field: reverseOrder
        [DataMember(Order = 2)]
        public OrderByDirection Direction { get; set; }
    }
}
