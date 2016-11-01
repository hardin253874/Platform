// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A class that loads types for groups of entities.
    /// </summary>
    public class EntityTypeRepository : IEntityTypeRepository
    {
        /// <summary>
        /// The type ID given to entities that have no type, usually temporary entities that
        /// have not yet been saved.
        /// </summary>
        public static readonly long TypelessId = -1;

        /// <summary>
        /// Create a new <see cref="EntityTypeRepository"/>.
        /// </summary>
        public EntityTypeRepository()
            : this(PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf)
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="EntityTypeRepository"/>.
        /// </summary>
        /// <param name="getTypes">
        /// A function to get the types of an entity.
        /// </param>
        internal EntityTypeRepository( Func<IEntity, ISet<long>> getTypes )
        {
            if (getTypes == null)
            {
                throw new ArgumentNullException("getTypes");
            }

            GetTypes = getTypes;
        }

        /// <summary>
        /// Used to get an entity's types.
        /// </summary>
        public Func<IEntity, ISet<long>> GetTypes { get; }

        /// <summary>
        /// Return the type IDs for the given entities;
        /// </summary>
        /// <param name="entityRefs">
        /// The entities to get the type of. This cannot be null or contain null.
        /// </param>
        /// <returns>
        /// A map of the type ID to entities of that type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityRefs"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entityRefs"/> cannot contain null.
        /// </exception>
        public IDictionary<long, ISet<EntityRef>> GetEntityTypes(IEnumerable<EntityRef> entityRefs)
        {
            if (entityRefs == null)
            {
                throw new ArgumentNullException("entityRefs");
            }
            if (entityRefs.Any(x => x == null))
            {
                throw new ArgumentException("Cannot load types for null entities", "entityRefs");
            }

            Dictionary<long, ISet<EntityRef>> result;
            ISet<long> entityTypes;
            ISet<EntityRef> entitiesOfThatType;
            IDictionary<EntityRef, IEntity> entityDict;

            // Get entities as a dictionary to ensure a result is returned
            // for every supplied entity. Load them at once for efficiency.
            // Use the Entity stored on the EntityRef where possible.
            entityDict = Entity.Get( entityRefs.Where( eref => eref != null && !eref.HasEntity ) )
                               .Where(e => e != null)
                               .ToDictionarySafe(e => new EntityRef(e));
            foreach ( EntityRef entityRef in entityRefs )
            {
                if ( entityRef != null && entityRef.HasEntity )
                    entityDict [ entityRef ] = entityRef.Entity;
            }


            result = new Dictionary<long, ISet<EntityRef>>();
            using (new SecurityBypassContext())
            {
                foreach (EntityRef entityRef in entityRefs)
                {
                    IEntity entity;
                    if ( entityDict.TryGetValue( entityRef.Id, out entity ) )
                    {
                        entityTypes = GetTypes( entity );
                    }
                    else
                    {
                        entityTypes = new HashSet<long>();
                    }

                    // Handle typeless IDs
					if ( entityTypes.Count <= 0 )
                    {
                        entityTypes.Add(TypelessId);
                    }

                    foreach (long entityTypeId in entityTypes)
                    {
                        if (result.TryGetValue(entityTypeId, out entitiesOfThatType))
                        {
                            entitiesOfThatType.Add(entityRef);
                        }
                        else
                        {
                            result.Add(entityTypeId, new HashSet<EntityRef>(new[] { entityRef }));
                        }
                    }
                }
            }

            return result;
        }
    }
}
