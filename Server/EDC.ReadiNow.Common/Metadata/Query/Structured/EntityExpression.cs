// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Any expression that specifically operates in the context of some entity node in
    /// the 'from' tree. (As oppose to literals, parameters, calculations etc, that do not)
    /// </summary>
    // type: nodeExpression
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [KnownType(typeof(AggregateExpression)), XmlInclude(typeof(AggregateExpression)), ProtoInclude(100, typeof(AggregateExpression))]
    [KnownType(typeof(ResourceExpression)), XmlInclude(typeof(ResourceExpression))]
    [KnownType(typeof(ResourceDataColumn)), XmlInclude(typeof(ResourceDataColumn)), ProtoInclude(101, typeof(ResourceDataColumn))]
    [KnownType(typeof(StructureViewExpression)), XmlInclude(typeof(StructureViewExpression))]
    [KnownType(typeof(IdExpression)), XmlInclude(typeof(IdExpression)), ProtoInclude(102, typeof(IdExpression))]
    [KnownType(typeof(ScriptExpression)), XmlInclude(typeof(ScriptExpression)), ProtoInclude(103, typeof(ScriptExpression))]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class EntityExpression : ScalarExpression
    {
        /// <summary>
        /// The local Id of the node within the QueryRelations tree of the current query that represents this resource.
        /// </summary>
        // rel: sourceNode
        [DataMember (Order = 1)]
        public Guid NodeId
        {
            get { return StructuredQueryHashingContext.GetGuid( _nodeId ); }
            set { _nodeId = value; }
        }

        [XmlIgnore]
        private Guid _nodeId;
    }
}
