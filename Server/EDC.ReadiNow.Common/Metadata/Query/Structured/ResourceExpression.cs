// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // type: resourceExpression
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class ResourceExpression : ResourceDataColumn
    {
         #region Constructors
        public ResourceExpression()
        {
        }

        public ResourceExpression(Structured.Entity node, EntityRef field)
        {
            NodeId = node.NodeId;
            FieldId = field;
        }

        public ResourceExpression(Structured.Entity node, EntityRef field, EntityRef orderFieldId)
        {
            NodeId = node.NodeId;
            FieldId = field;
            OrderFieldId = orderFieldId;
        }
        #endregion
            
        /// <summary>
        /// The Id of the order field being represented.
        /// </summary>
        // not represented in entity model
        [DataMember(Order = 1)]
        public EntityRef OrderFieldId { get; set; }
    }
}
