// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version2
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityHierarchyEntry
    {
        /// <summary>
        ///     The entity being represented.
        ///     Null for root entity.
        /// </summary>
        [CanBeNull]
        public EntityEntry Entity
        {
            get;
            set;
        }

        /// <summary>
        ///     The parent, or null if no parent.
        /// </summary>
        public EntityHierarchyEntry ParentEntity
        {
            get;
            set;
        }

        /// <summary>
        ///     The relationship followed from the parent to reach this node.
        /// </summary>
        [CanBeNull]
        public RelationshipEntry RelationshipFromParent
        {
            get;
            set;
        }

        /// <summary>
        ///     The relationship that represents 'is of type' for the current entity.
        ///     Null if not found.
        /// </summary>
        [CanBeNull]
        public RelationshipEntry TypeRelationship
        {
            get;
            set;
        }

        /// <summary>
        ///     The direction that the relationship was followed from the parent, to this node.
        ///     That is, if Direction=Forward, then RelationshipFromParent.ToId is equal to Entity.EntityId
        /// </summary>
        public Direction Direction
        {
            get;
            set;
        }

        /// <summary>
        ///     Child entities. I.e. entities that are subcomponents.
        ///     Or null.
        /// </summary>
        [CanBeNull]
        public List<EntityHierarchyEntry> Children
        {
            get;
            set;
        }

        [CanBeNull]
        public List<RelationshipEntry> ForwardRelationships
        {
            get;
            set;
        }

        [CanBeNull]
        public List<RelationshipEntry> ReverseRelationships
        {
            get;
            set;
        }
    }
}
