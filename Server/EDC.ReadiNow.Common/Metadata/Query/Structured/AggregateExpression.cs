// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Type of aggregation being performed.
    /// </summary>
    /// <remarks>
    /// 'Count' is the only method that does not accept some expression.
    /// 'Count' and 'Sum' will each return zero if no rows are matched. All others will return NULL in this scenario.
    /// </remarks>
    // enum: aggregateMethodEnum
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum AggregateMethod
    {
        [EnumMember]
        Count,
        [EnumMember]
        Sum,
        [EnumMember]
        Average,
        [EnumMember]
        Min,
        [EnumMember]
        Max,
        [EnumMember]
        StandardDeviation,
        [EnumMember]
        PopulationStandardDeviation,
        [EnumMember]
        Variance,
        [EnumMember]
        PopulationVariance,
        [EnumMember]
        CountWithValues,
        [EnumMember]
        CountUniqueItems,
        [EnumMember]
        CountUniqueNotBlanks,
        [EnumMember]
        List
    }


    /// <summary>
    /// An aggregate calculation.
    /// </summary>
    /// <remarks>
    /// This translates to a T-SQL function such as 'count' or 'sum'.
    /// The entity referred to by EntityExpression.NodeId must have IsGrouped=true.
    /// Any expression or sub-expressions appearing within Expression that point to entity tree nodes must point to nodes that are at or under NodeId.
    /// </remarks>
    // type: aggregateExpression
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class AggregateExpression : EntityExpression, ICompoundExpression
    {
        public AggregateExpression()
        {
            AggregateMethod = AggregateMethod.Count;
        }


        /// <summary>
        /// The type of aggregation.
        /// </summary>
        // rel: aggregateMethod
        [DataMember(Order = 1)]
        public AggregateMethod AggregateMethod { get; set; }
     

        /// <summary>
        /// The expression being aggregated.
        /// </summary>
        /// <remarks>
        /// Set to null if AggregateMethod is Count.
        /// The expression tree here cannot refer to any entity nodes at or above the aggregate entity identified by the NodeId property of this object.
        /// That is, aggregation can only be done on fields within the subquery.
        /// </remarks>
        // rel: aggregatedExpression
        [DataMember(Order = 2)]
        public ScalarExpression Expression { get; set; }

        public IEnumerable<ScalarExpression> Children
        {
            get
            {
                yield return Expression;
            }
        }
    }
}
