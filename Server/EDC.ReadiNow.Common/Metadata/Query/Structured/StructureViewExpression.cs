// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // not yet represented in entity model
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType("StructureView", Namespace = Constants.StructuredQueryNamespace)]
    public class StructureViewExpression : ScalarExpression
    {
        /// <summary>
        /// The local Id of the node within the QueryRelations tree of the current query that represents this resource.
        /// </summary>
        [DataMember(Order = 1)]
        public Guid NodeId { get; set; }

        /// <summary>
        /// The Id of the structure view being represented.
        /// </summary>
        [DataMember(Order = 2)]
        public EntityRef StructureViewId { get; set; }

    }
}
