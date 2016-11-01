// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using Quartz;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Base class for <see cref="CacheInvalidator{TKey, TValue}"/> used for security. This handles 
    /// </summary>
    /// <typeparam name="TKey">
    /// The cache key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The cache value type.
    /// </typeparam>
    public class SecurityCacheInvalidatorBase<TKey, TValue> : CacheInvalidator<TKey, TValue>
    {
        /// <summary>
        /// Create a new <see cref="SecurityCacheInvalidatorBase{TKey, TValue}"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate. This cannot be null.
        /// </param>
        /// <param name="name">
        /// A unique name for the cache. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public SecurityCacheInvalidatorBase(ICache<TKey, TValue> cache, string name)
            :base (cache, name)
        {
            // Do nothing    
        }

        /// <summary>
        /// Invalidate any relationships eminating from the from or to type of a 
        /// relationship with the securesFrom or securesTo flags set, respectively.
        /// </summary>
        /// <param name="entities">
        /// The entities being saved or deleted.
        /// </param>
        /// <param name="cause">
        /// Whether the operation is a save or delete.
        /// </param>
        /// <param name="preActionModifiedRelatedEntities">
        /// A map of entity ID to any changes. This may be null. Entities that appear in
        /// <paramref name="entities"/> may lack a corresponding entry. Similarly, there
        /// may be additional entities
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entities"/> may be null.
        /// </exception>
        public override void OnEntityChange(IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities)
        {
            Relationship relationship;
            EntityType fromType;
            EntityType toType;

            base.OnEntityChange(entities, cause, preActionModifiedRelatedEntities);

            // There should be no need to be recursive since all relationships involved
            // in a security check are added. Similarly, this invalidates relationships
            // without the appropriate flag but these will not be cached, anyway.
            foreach (IEntity entity in entities)
            {
                relationship = entity.As<Relationship>();
                if (relationship != null)
                {
                    fromType = relationship.FromType;
                    toType = relationship.ToType;

                    if ( ( relationship.SecuresFrom ?? false ) && fromType != null )
                    {
                        IEnumerable<long> fromTypeRelationships = fromType.Relationships.Concat( fromType.ReverseRelationships ).Select( e => e.Id );
                        InvalidateRelationshipTypes( fromTypeRelationships );
                    }
                    if ( ( relationship.SecuresTo ?? false ) && toType != null )
                    {
                        IEnumerable<long> toTypeRelationships = toType.Relationships.Concat( toType.ReverseRelationships ).Select( e => e.Id );
                        InvalidateRelationshipTypes( toTypeRelationships );
                    }
                }
            }
        }
    }
}
