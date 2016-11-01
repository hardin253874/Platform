// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// Invalidate a cache on after an entity save or delete.
    /// </summary>
    public interface ICacheInvalidator
    {
        /// <summary>
        /// A unique name for this cache invalidator.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Notify a cache when an entity is saved or deleted.
        /// </summary>
        /// <param name="entities">
        ///     The entities being saved or deleted. This cannot be null.
        /// </param>
        /// <param name="cause">
        ///     Whether the operation is a save or delete.
        /// </param>
        /// <param name="preActionModifiedRelatedEntities">
        ///     Callback to get modified fields and related entities. This may be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entities"/> cannot contain null.
        /// </exception>
        void OnEntityChange(IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities);

        /// <summary>
        /// Notify a chance when a relationship is created or deleted.
        /// </summary>
        /// <param name="relationshipTypes">
        ///     The changed relationship types. This cannot be null or contain null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="relationshipTypes"/> cannot contain null.
        /// </exception>
        void OnRelationshipChange(IList<EntityRef> relationshipTypes);

        /// <summary>
        /// Notify a chance when a field is modified.
        /// </summary>
        /// <param name="fieldTypes">
        ///     The changed field types. This cannot be null or contain null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        void OnFieldChange(IList<long> fieldTypes);
    }
}
