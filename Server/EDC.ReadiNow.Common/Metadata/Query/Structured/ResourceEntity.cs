// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Model;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// An entity that represents a resource.
    /// Note: used directly in the case of the root node.
    /// </summary>
    // type: resourceReportNode
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [KnownType(typeof(RelatedResource)), XmlInclude(typeof(RelatedResource)), ProtoInclude(100, typeof(RelatedResource))]
    [KnownType(typeof(DownCastResource)), XmlInclude(typeof(DownCastResource)), ProtoInclude(101, typeof(DownCastResource))]
    [KnownType(typeof(JoinToSelfEntity)), XmlInclude(typeof(JoinToSelfEntity)), ProtoInclude(102, typeof(JoinToSelfEntity))]
    [KnownType(typeof(CustomJoinNode)), XmlInclude(typeof(CustomJoinNode)), ProtoInclude(103, typeof(CustomJoinNode))]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class ResourceEntity : Entity
    {
        #region Constructors
        public ResourceEntity()
        {
        }

        public ResourceEntity(EntityRef entityType)
        {
            EntityTypeId = entityType;
        }
        #endregion

        /// <summary>
        /// Reference to the entity type.
        /// </summary>
        //[XmlIgnore]
        // rel: resourceReportNodeType
        [DataMember(Order = 1)]
        public EntityRef EntityTypeId { get; set; }

        /// <summary>
        /// If true, only the exact entity type will be matched.
        /// If false, inherited types are also included.
        /// </summary>
        // field: exactType
        [DataMember(Order = 2)]
        [DefaultValue(false)]
        public bool ExactType { get; set; }

        /// <summary>
        /// If true then a row will only be shown if the target exists (inner join).
        /// If false, then the row will be shown regardless (left join).
        /// </summary>
        // field: targetMustExist
        [DataMember( Order = 3 )]
        [DefaultValue( false )]
        public bool ResourceMustExist { get; set; }

        /// <summary>
        /// If true then this relationship will never constrain the parent node (forced left join),
        /// even if a child node or expression has requires.
        /// </summary>
        // field: targetMustExist
        [DataMember( Order = 4 )]
        [DefaultValue( false )]
        // field: resourceNeedNotExist
        public bool ResourceNeedNotExist { get; set; }

        /// <summary>
        /// Human readable representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} Entity: '{1}' Exact Type: {2}", 
                MethodBase.GetCurrentMethod().ReflectedType.Name, EntityTypeId ?? "(null)", ExactType);
        }
    }
}
