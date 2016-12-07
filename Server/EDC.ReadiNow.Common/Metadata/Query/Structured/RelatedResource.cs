// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // enum not included in entity model
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum RecursionMode
    {
        /// <summary>
        /// Do not follow this relationship recursively.
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// Follow this relationship recursively, but do not include the parent node. 
        /// That is, this is the transitive closure of the relationship.
        /// Does not include the original resource itself. Use, for example to show all managers above an employee.
        /// </summary>
        [EnumMember]
        Recursive,

        /// <summary>
        /// Follow this relationship recursively, and also include the current node. 
        /// That is, this is the transitive closure of the relationship unioned to the indentity relationship.
        /// Use, for example to show all documents in this folder, and subfolders.
        /// </summary>
        [EnumMember]
        RecursiveWithSelf,
    }

    // type: relationshipReportNode
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class RelatedResource : ResourceEntity
    {
        #region Constructors
        public RelatedResource()
        {
        }

        public RelatedResource(EntityRef relationshipType)
        {
            RelationshipTypeId = relationshipType;
            RelationshipDirection = RelationshipDirection.Forward;
        }

        public RelatedResource(EntityRef relationshipType, RelationshipDirection direction)
        {
            RelationshipTypeId = relationshipType;
            RelationshipDirection = direction;
        }
        #endregion

        /// <summary>
        /// Reference to the entity relationship type.
        /// </summary>
        /// <remarks>This obsoletes RelationshipId</remarks>
        // rel: followRelationship
        [DataMember(Order = 1)]
        public EntityRef RelationshipTypeId { get; set; }

        /// <summary>
        /// The direction that the relationship is being followed.
        /// </summary>
        /// <remarks>
        /// This is particularly relevant if both ends of the relationship are of the same type.
        /// </remarks>
        // field: followInReverse
        [DataMember(Order = 2)]
        public RelationshipDirection RelationshipDirection { get; set; }

        /// <summary>
        /// If true then a row will only be shown if the target exists (inner join).
        /// If false, then the row will be shown regardless (left join).
        /// </summary>
        // field: targetMustExist
        [DataMember(Order = 3)]
        [DefaultValue(false)]
        public bool ResourceMustExist { get; set; }

        /// <summary>
        /// If true then this relationship will never constrain the parent node (forced left join),
        /// even if a child node or expression has requires.
        /// </summary>
        // field: targetMustExist
        [DataMember(Order = 4)]
        [DefaultValue(false)]
        // field: resourceNeedNotExist
        public bool ResourceNeedNotExist { get; set; }

        /// <summary>
        /// If true then a row from the parent node will only be shown if the target exists.
        /// I.e. it forces an inner join to the immediate parent.
        /// (c.f. with ResourceMustExist, which inner joins all the way to the root)
        /// </summary>
        // field: constrainParent
        [DataMember(Order = 5)]
        [DefaultValue(false)]
        public bool ConstrainParent { get; set; }

        /// <summary>
        /// If true then a row will only be shown if the target exists (inner join).
        /// If false, then the row will be shown regardless (left join).
        /// </summary>
        // fields: followRecursive, includeSelfInRecursive
        [DataMember(Order = 6)]
        [DefaultValue(RecursionMode.None)]
        public RecursionMode Recursive { get; set; }

        /// <summary>
        /// If true then the relationship is represented in the query as an 'exists' clause rather than a join.
        /// This is useful where the relationship is being used to enforce some sort of constraint/filter, but no actual data is being selected from it.
        /// (As, joining may have the result of multiple result rows appearing when all we want to do is use it as a constriant).
        /// </summary>
        // field: checkExistenceOnly
        [DataMember(Order = 7)]
        [DefaultValue(false)]
        public bool CheckExistenceOnly { get; set; }

        /// <summary>
        /// Mechanism to allow relationships to be injected or removed on an adhoc basis.
        /// This allows for showing a report of related resources, when the list of related resources is currently being edited and is unsaved.
        /// </summary>
        // not included in entity model
        [DataMember(Order = 8)]
        public FauxRelationships FauxRelationships { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether to exclude this relationship if is entity is not referenced in the query.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to exclude the relationship if it is unreferenced; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Order = 9)]
        [DefaultValue(false)]
        // not sure if we want to include this in entity model
        public bool ExcludeIfNotReferenced { get; set; }

        /// <summary>
        /// If true then all related items are returned, not just those linked from the parent.
        /// </summary>
        // field: parentNeedNotExist
        [DataMember( Order = 10 )]
        [DefaultValue( false )]
        public bool ParentNeedNotExist { get; set; }


        #region XML Formatters
        [UsedImplicitly]
		private bool ShouldSerializeFauxRelatedResources()
        {
            return false;
        }
        #endregion
    }

    /// <summary>
    /// Mechanism to allow relationships to be injected or removed on an adhoc basis.
    /// This allows for showing a report of related resources, when the list of related resources is currently being edited and is unsaved.
    /// Typical use case is for a resource form to show a report of related entities. The report is on the related resources,
    /// but follows a relationship back to the main resource. This relationship is then constrained to only show the main resource.
    /// That is, the query is back-to-front when compared to the form it is being used in.
    /// </summary>
    // not included in entity model
    [DataContract( Namespace = Constants.DataContractNamespace )]
    public class FauxRelationships
    {
        /// <summary>
        /// Resource at this end of the relationship that faux-relationships will point to.
        /// </summary>
        [DataMember(Order = 1)]
        public bool HasTargetResource { get; set; }

        /// <summary>
        /// Resource at this end of the relationship that faux-relationships will point to has temporary Id (has not been created yet).
        /// </summary>
        [DataMember(Order = 2)]
        public bool IsTargetResourceTemporary { get; set; }

        /// <summary>
        /// Resources on the *parent* query entity that will get added.
        /// </summary>
        [DataMember(Order = 3)]
        public bool HasIncludedResources { get; set; }

        /// <summary>
        /// Resources on the *parent* query entity that will get excluded.
        /// </summary>
        [DataMember(Order = 4)]
        public bool HasExcludedResources { get; set; }
    }
}
