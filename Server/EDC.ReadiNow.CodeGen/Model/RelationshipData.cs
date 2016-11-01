// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Common.ConfigParser.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Templates
{
    /// <summary>
    /// Holds information about a parsed relationship, in the context of some particular entity type that returned it.
    /// </summary>
    public class RelationshipData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal RelationshipData(Entity relationship, bool isReverse, bool isInherited)
        {
            if (relationship == null)
                throw new ArgumentNullException("relationship");
            RelationshipEntity = relationship;
            IsReverse = isReverse;
            IsInherited = isInherited;
        }

        /// <summary>
        /// The parsed entity that represents this relationship.
        /// </summary>
        public Entity RelationshipEntity { get; set; }

        /// <summary>
        /// True if it was inherited by the type that returned it.
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// True if it is being followed in reverse relative to the type that returned it.
        /// </summary>
        public bool IsReverse { get; set; }

        /// <summary>
        /// The type that is being pointed to by the relationship.
        /// That is, the to-type for relationships being followed forward, or the from-type for relationships being followed in reverse.
        /// </summary>
        public string Name
        {
            get
            {
                if (IsReverse)
                {
                    return Model.GetRelationshipReverseAliasDisplayName(RelationshipEntity);
                }
                else
                {
                    return Model.GetTypeDisplayName(RelationshipEntity);
                }
            }
        }

        /// <summary>
        /// The type that is declaring the entity.
        /// That is, the from-type for relationships being followed forward, or the to-type for relationships being followed in reverse.
        /// </summary>
        public Entity SourceTypeEntity
        {
            get
            {
                if (IsReverse)
                {
                    return Model.SchemaResolver.GetRelationshipToType(RelationshipEntity);
                }
                else
                {
                    return Model.SchemaResolver.GetRelationshipFromType(RelationshipEntity);
                }
            }            
        }

        /// <summary>
        /// The type that is declaring the entity.
        /// That is, the from-type for relationships being followed forward, or the to-type for relationships being followed in reverse.
        /// </summary>
        public string SourceName
        {
            get { return Model.GetTypeDisplayName(SourceTypeEntity); }
        }

        /// <summary>
        /// The type that is being pointed to by the relationship.
        /// That is, the to-type for relationships being followed forward, or the from-type for relationships being followed in reverse.
        /// </summary>
        public Entity TargetTypeEntity
        {
            get
            {
                if (IsReverse)
                {
                    return Model.SchemaResolver.GetRelationshipFromType(RelationshipEntity);
                }
                else
                {
                    return Model.SchemaResolver.GetRelationshipToType(RelationshipEntity);
                }
            }
        }

        /// <summary>
        /// The type that is declaring the entity.
        /// That is, the from-type for relationships being followed forward, or the to-type for relationships being followed in reverse.
        /// </summary>
        public string TargetName
        {
            get { return Model.GetTypeDisplayName(TargetTypeEntity); }
        }



        /// <summary>
        /// Wha??
        /// Formerly GetForwardRelationshipTypeDisplayName and GetReverseRelationshipTypeDisplayName
        /// </summary>
        public string TypeName
        {
            get
            {
                if (RelationshipEntity.Alias != null && !RelationshipEntity.Alias.RelationshipInstance)
                {
                    /////
                    // Instance display name.
                    /////
                    return Model.GetTypeDisplayName(RelationshipEntity);
                }

                /////
                // Type display name.
                /////
                return Model.GetTypeDisplayName(Model.SchemaResolver.GetEntityType(RelationshipEntity));
            }
        }

        /// <summary>
        /// Text description of the inheritance.
        /// </summary>
        public string InheritedText
        {
            get
            {
                return IsInherited ? "inherited " : "direct";
            }
        }

        /// <summary>
        /// Text description of the direction.
        /// </summary>
        public string DirectionText
        {
            get
            {
                return IsReverse ? "reverse" : "forward";
            }
        }

        /// <summary>
        /// Text description of the direction.
        /// </summary>
        public string DirectionEnum
        {
            get
            {
                return IsReverse ? "Direction.Reverse" : "Direction.Forward";
            }
        }

        /// <summary>
        /// True if the relationship is acting as a collection, false if acting as a lookup.
        /// </summary>
        public bool IsCollection
        {
            get
            {
                if (IsReverse)
                    return Model.IsReverseRelationshipCollection(RelationshipEntity);
                else
                    return Model.IsForwardRelationshipCollection(RelationshipEntity);
            }
        }

        /// <summary>
        /// The namespace:alias for the relationship, in the direction being followed.
        /// </summary>
        public string NsAlias
        {
            get
            {
                if (IsReverse)
                    return RelationshipEntity.ReverseAlias.NsAlias;
                else
                    return RelationshipEntity.Alias.NsAlias;
            }
        }
    }
}
