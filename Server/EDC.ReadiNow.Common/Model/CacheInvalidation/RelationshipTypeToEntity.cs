// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// A pair containing both an entity and a relationship type.
    /// </summary>
    public class RelationshipTypeToEntity
    {
        /// <summary>
        /// Create a new <see cref="RelationshipTypeToEntity"/>.
        /// </summary>
        /// <param name="entity">
        /// The entity in the tuple. This cannot be null.
        /// </param>
        /// <param name="relationshipType">
        /// The relationship type from the given entity. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public RelationshipTypeToEntity(EntityRef entity, EntityRef relationshipType)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            if (relationshipType == null)
            {
                throw new ArgumentNullException("relationshipType");
            }

            Entity = entity;
            RelationshipType = relationshipType;
        }

        /// <summary>
        /// The entity.
        /// </summary>
        public EntityRef Entity { get; private set; }

        /// <summary>
        /// The relationship type from <see cref="Entity"/> to monitor.
        /// </summary>
        public EntityRef RelationshipType { get; private set; }
    }
}