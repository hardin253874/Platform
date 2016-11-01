// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Model;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Identifies a column of resource data.
    /// Not necessarily for select.
    /// </summary>
    // type: fieldExpression
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [KnownType(typeof(ResourceExpression)), XmlInclude(typeof(ResourceExpression)), ProtoInclude(100, typeof(ResourceExpression))]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class ResourceDataColumn : EntityExpression, ICompoundExpression
    {
        #region Constructors
        public ResourceDataColumn()
        {
        }

        public ResourceDataColumn(Structured.Entity node, EntityRef field)
        {
            NodeId = node.NodeId;
            FieldId = field;
        }

        public ResourceDataColumn(Structured.Entity node, EntityRef field, EDC.Database.DatabaseType castType)
        {
            NodeId = node.NodeId;
            FieldId = field;
            CastType = castType;
        }
        #endregion

        /// <summary>
        /// Reference to an entity field
        /// </summary>
        // rel: fieldExpressionField
        [DataMember(Order = 1)]
        public EntityRef FieldId { get; set; }

        /// <summary>
        /// Reference to cast type
        /// </summary>
        // not represented in entity model
        [DataMember(Order = 2)]
        public EDC.Database.DatabaseType CastType
        {
            get;
            set;
        }

        [DataMember(Order = 3)]
        [XmlIgnore]
        public long SourceNodeEntityId { get; set; }

        [DataMember(Order = 4)]
        [XmlIgnore]
        public long TargetTypeId { get; set; }

        // used internal to the query engine.
        [XmlIgnore]
        public ScriptExpression ScriptExpression;

        public IEnumerable<ScalarExpression> Children
        {
            get
            {
                if (ScriptExpression != null)
                    yield return ScriptExpression;
            }
        }
    }
}
