// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// Notable changes to an entity.
    /// </summary>
    public class EntityChanges
    {
        /// <summary>
        /// Create a new <see cref="EntityChanges"/>.
        /// </summary>
        public EntityChanges()
            : this(new HashSet<RelationshipTypeToEntity>(), new HashSet<long>())
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="EntityChanges"/>.
        /// </summary>
        /// <param name="relationshipTypesToEntities">
        /// The relationship types to use. This cannot be null or contain null.
        /// </param>
        /// <param name="fieldTypes">
        /// The field types to use. This cannot be null.
        /// </param>
        public EntityChanges(ISet<RelationshipTypeToEntity> relationshipTypesToEntities, ISet<long> fieldTypes)
        {
            if (relationshipTypesToEntities == null)
            {
                throw new ArgumentNullException("relationshipTypesToEntities");
            }
            if (relationshipTypesToEntities.Contains(null))
            {
                throw new ArgumentException("Cannot contain null", "relationshipTypesToEntities");
            }
            if (fieldTypes == null)
            {
                throw new ArgumentNullException("fieldTypes");
            }

            RelationshipTypesToEntities = new HashSet<RelationshipTypeToEntity>();
            RelationshipTypesToEntities.UnionWith(relationshipTypesToEntities);
            FieldTypes = new HashSet<long>();
            FieldTypes.UnionWith(fieldTypes);
        }

        /// <summary>
        /// Relationship types associated with the related entities.
        /// </summary>
        public ISet<RelationshipTypeToEntity> RelationshipTypesToEntities { get; private set; }

        /// <summary>
        /// Types of modified fields.
        /// </summary>
        public ISet<long> FieldTypes { get; private set; }
    }
}
