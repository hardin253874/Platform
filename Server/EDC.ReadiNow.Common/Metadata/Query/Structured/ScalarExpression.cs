// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using System.ComponentModel;
using EDC.ReadiNow.Annotations;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Scalar expressions that are composed of child expressions.
    /// </summary>
    interface ICompoundExpression
    {
        IEnumerable<ScalarExpression> Children { get; }
    }

    /// <summary>
    /// A type of query value.
    /// Could be a resource data column, constant, or other.
    /// </summary>
    // type: reportExpression
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [KnownType(typeof(ResourceExpression)),      XmlInclude(typeof(ResourceExpression))]
    [KnownType(typeof(AggregateExpression)),     XmlInclude(typeof(AggregateExpression))]
    [KnownType(typeof(EntityExpression)),        XmlInclude(typeof(EntityExpression)),ProtoInclude(102, typeof(EntityExpression))]
    [KnownType(typeof(ResourceDataColumn)),      XmlInclude(typeof(ResourceDataColumn))]
    [KnownType(typeof(StructureViewExpression)), XmlInclude(typeof(StructureViewExpression)),ProtoInclude(104, typeof(StructureViewExpression))]
    [KnownType(typeof(ColumnReference)),         XmlInclude(typeof(ColumnReference)),ProtoInclude(105, typeof(ColumnReference))]
    [KnownType(typeof(CalculationExpression)),   XmlInclude(typeof(CalculationExpression)),ProtoInclude(106, typeof(CalculationExpression))]
    [KnownType(typeof(ScriptExpression)),        XmlInclude(typeof(ScriptExpression))]
    [KnownType(typeof(IfElseExpression)),        XmlInclude(typeof(IfElseExpression)),ProtoInclude(108, typeof(IfElseExpression))]
    [KnownType(typeof(LiteralExpression)),       XmlInclude(typeof(LiteralExpression)),ProtoInclude(109, typeof(LiteralExpression))]
    [KnownType(typeof(ComparisonExpression)),    XmlInclude(typeof(ComparisonExpression)),ProtoInclude(110, typeof(ComparisonExpression))]
    [KnownType(typeof(LogicalExpression)),       XmlInclude(typeof(LogicalExpression)),ProtoInclude(111, typeof(LogicalExpression))]
    [KnownType(typeof(MutateExpression)),        XmlInclude(typeof(MutateExpression)),ProtoInclude(112, typeof(MutateExpression))]    
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public abstract class ScalarExpression
    {
        /// <summary>
        /// An identifier that may be used to match or equate expressions.
        /// </summary>
        /// <remarks>
        /// Note: by default this is not set. Only set it when you need it, such as when an expression is used
        /// both in a group-by clause and is also selected.
        /// </remarks>
        [DataMember(Order = 1)]
        [XmlAttribute("id")]
        public Guid ExpressionId
        {
            get { return StructuredQueryHashingContext.GetGuid( _expressionId ); }
            set { _expressionId = value; }
        }

        [XmlIgnore]
        private Guid _expressionId;

        [DataMember(Order = 2)]
        [XmlIgnore]
        public long EntityId;

        /// <summary>
        /// Any cluster operation to perform on this node.
        /// (Would prefer to add as its own expression node, but presents too much risk at the moment)
        /// </summary>
        [DataMember(Order = 3)]
        [DefaultValue( ClusterOperation.None )]
        public ClusterOperation ClusterOperation { get; set; }

        #region XML Formatters
        /// <summary>
        /// Returns true if the expression Id should be serialized.
        /// </summary>
		public bool ShouldSerializeExpressionId()
        {
            return ExpressionId != Guid.Empty;
        }
        #endregion

        public override bool Equals(object obj)
        {
            var alt = obj as ScalarExpression;
            return alt != null && ExpressionId != Guid.Empty && ExpressionId == alt.ExpressionId;
        }

        public override int GetHashCode()
        {
            return ExpressionId.GetHashCode();
        }
    }

}
