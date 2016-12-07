// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// Invalidate the caches when an <see cref="AccessRule"/> entity is saved.
    /// </summary>
    public class CacheInvalidationEventTarget : IEntityEventSave, IEntityEventDelete
    {
        /// <summary>
        /// Create a new <see cref="CacheInvalidationEventTarget"/>.
        /// </summary>
        public CacheInvalidationEventTarget()
            : this(new CacheInvalidatorFactory().CacheInvalidators)
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="CacheInvalidationEventTarget"/>
        /// </summary>
        /// <param name="cacheInvalidators">
        /// The cache invalidators to use. This cannot be or contain null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cacheInvalidators"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="cacheInvalidators"/> cannot contain null.
        /// </exception>
        internal CacheInvalidationEventTarget(IEnumerable<ICacheInvalidator> cacheInvalidators)
        {
            if (cacheInvalidators == null)
            {
                throw new ArgumentNullException("cacheInvalidators");
            }
            if (cacheInvalidators.Contains(null))
            {
                throw new ArgumentException("Cannot contain null", "cacheInvalidators");
            }

            Invalidators = cacheInvalidators;
        }

        /// <summary>
        /// The cache invalidators used to invalidate caches.
        /// </summary>
        internal IEnumerable<ICacheInvalidator> Invalidators { get; private set; }

        /// <summary>
        /// Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {            
            object entry;
            IDictionary<long, EntityChanges> modifiedRelatedEntities;

            // Filter out certain entity changes (namely modifying softwarePlatformOpenIdProvider, which keeps invalidating our caches)
            IList<IEntity> entityList = FilterEntityChangeList(entities);

            if (entityList.Count == 0)
                return;

            // Get the SaveGraph, for mappings between TempIDs and real IDs.
            SaveGraph saveGraph = EventTargetStateHelper.GetSaveGraph( state );

            IList<EntityRef> relationshipTypes = null;
            IList<long> fieldTypes = null;

            foreach (ICacheInvalidator cacheInvalidator in Invalidators.ToList( ))
            {
                modifiedRelatedEntities = null;
                if (state.TryGetValue(cacheInvalidator.Name, out entry))
                {
                    modifiedRelatedEntities = entry as IDictionary<long, EntityChanges>;
                }

                Func<long, EntityChanges> entityChangeGetter = CreateEntityChangesCallback( saveGraph, modifiedRelatedEntities );

                cacheInvalidator.OnEntityChange( entityList, InvalidationCause.Save, entityChangeGetter );

                if (modifiedRelatedEntities != null)
                {
                    if ( relationshipTypes == null )
                    {
                        relationshipTypes = modifiedRelatedEntities.Where( kvp => kvp.Value != null )
                                                .SelectMany(kvp => kvp.Value.RelationshipTypesToEntities.Select(rte => rte.RelationshipType))
                                            .ToList( );
                    }
                    cacheInvalidator.OnRelationshipChange( relationshipTypes );

                    if ( fieldTypes == null )
                    {
                        fieldTypes = modifiedRelatedEntities.Where( kvp => kvp.Value != null )
                                                .SelectMany(kvp => kvp.Value.FieldTypes)
                                            .ToList( );
                    }
                    cacheInvalidator.OnFieldChange( fieldTypes );
                }

                // Invalidate if this is a field or relationship
                InvalidateFieldOrRelationshipIfEntityIsFieldOrRelationship(entityList, cacheInvalidator);
            }            
        }

        /// <summary>
        /// Generate a callback that can be used to look up changes for a given entity.
        /// </summary>
        /// <param name="saveGraph">The save graph object that can be used to map from temporary IDs to real IDs.</param>
        /// <param name="entityChanges">Dictionary of changes for each entity. Where the keys may be temporary IDs.</param>
        /// <returns>A callback that will accept a real (saved) ID and return the entity changes associated with that entity id.</returns>
        private static Func<long, EntityChanges> CreateEntityChangesCallback( SaveGraph saveGraph, IDictionary<long, EntityChanges> entityChanges )
        {
            if ( entityChanges == null || entityChanges.Count <= 0 )
                return null;

            return ( savedEntityId ) =>
                {
                    // Map from the requested (real saved) ID to the original ID, which may be a temporary ID
                    long possiblyTemporaryId;
                    if ( !saveGraph.Mapping.TryGetByValue( savedEntityId, out possiblyTemporaryId ) )
                        possiblyTemporaryId = savedEntityId;
                    
                    // Look up changes
                    EntityChanges result;
                    if ( !entityChanges.TryGetValue( possiblyTemporaryId, out result ) )
                        result = null;
                    return result;                    
                };
        }

        /// <summary>
        /// Filter the list of entities that we are doing cache invalidations for.
        /// Exclude special cases for performance reasons.
        /// </summary>
        /// <param name="entities">Unfiltered list of entities.</param>
        /// <returns>Filtered list of entities.</returns>
        private static IList<IEntity> FilterEntityChangeList(IEnumerable<IEntity> entities)
        {
            return entities.ToList();
        }

        /// <summary>
        /// Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        /// True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            object entry;
            IDictionary<long, EntityChanges> newModifiedRelatedEntities;
            IDictionary<long, EntityChanges> existingModifiedRelatedEntities;
            IList<IEntity> entityList;

            entityList = entities.ToList();
            foreach (ICacheInvalidator cacheInvalidator in Invalidators.ToList( ))
            {
                newModifiedRelatedEntities = GetEntityChanges(entityList, InvalidationCause.Save);
				if ( newModifiedRelatedEntities != null && newModifiedRelatedEntities.Count > 0 )
                {
                    if (state.TryGetValue(cacheInvalidator.Name, out entry))
                    {
                        // Merge with an existing entry
                        existingModifiedRelatedEntities = entry as IDictionary<long, EntityChanges>;
                        if (existingModifiedRelatedEntities != null)
                        {
                            newModifiedRelatedEntities = existingModifiedRelatedEntities.Union(newModifiedRelatedEntities)
                                .ToLookup(kvp => kvp.Key, kvp => kvp.Value)
                                .ToDictionary(kvp => kvp.Key, kvp => new EntityChanges(
                                    new HashSet<RelationshipTypeToEntity>(kvp.SelectMany(x => x.RelationshipTypesToEntities)),
                                    new HashSet<long>(kvp.SelectMany(x => x.FieldTypes))));
                        }

                        state[cacheInvalidator.Name] = newModifiedRelatedEntities;
                    }
                    else
                    {
                        state.Add(cacheInvalidator.Name, newModifiedRelatedEntities);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Called after deletion of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            // Do nothing
        }

        /// <summary>
        /// Called before deleting an enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <returns>
        /// True to cancel the delete operation; false otherwise.
        /// </returns>
        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            IDictionary<long, EntityChanges> modifiedRelatedEntities;
            IList<IEntity> entityList;

            entityList = entities.ToList();
            foreach (ICacheInvalidator cacheInvalidator in Invalidators.ToList( ))
            {
                modifiedRelatedEntities = GetEntityChanges(entityList, InvalidationCause.Delete);

                // Callback that simply performs a dictionary lookup
                Func<long, EntityChanges> entityChangeGetter = entityId =>
                {
                    EntityChanges changes;
                    modifiedRelatedEntities.TryGetValue( entityId, out changes );
                    return changes;
                };

                cacheInvalidator.OnEntityChange( entityList, InvalidationCause.Delete, entityChangeGetter );

                InvalidateFieldOrRelationshipIfEntityIsFieldOrRelationship(entityList, cacheInvalidator);
            }

            return false;
        }

        /// <summary>
        /// Call <see cref="ICacheInvalidator.OnFieldChange"/> or 
        /// <see cref="ICacheInvalidator.OnRelationshipChange"/> for any <see cref="FieldType"/>
        /// or <see cref="Relationship"/> saved or deleted directly.
        /// </summary>
        /// <param name="entities">
        /// The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="cacheInvalidator">
        /// The <see cref="ICacheInvalidator"/> to notify. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entities"/> cannot contain null.
        /// </exception>
        protected internal static void InvalidateFieldOrRelationshipIfEntityIsFieldOrRelationship(
            IList<IEntity> entities, ICacheInvalidator cacheInvalidator)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (entities.Contains(null))
            {
                throw new ArgumentException("Cannot contain null", "entities");
            }
            if (cacheInvalidator == null)
            {
                throw new ArgumentNullException("cacheInvalidator");
            }

            if (entities.Any(e => e.Is<Relationship>()))
            {
                cacheInvalidator.OnRelationshipChange(
                    entities.Where(e => e.Is<Relationship>()).Select(e => new EntityRef(e)).ToList());
            }
            if (entities.Any(e => e.Is<FieldType>()))
            {
                cacheInvalidator.OnFieldChange(
                    entities.Where(e => e.Is<FieldType>()).Select(e => e.Id).ToList());
            }
        }

        /// <summary>
        /// Find changed, related entities before the entities are saved or deleted.
        /// </summary>
        /// <param name="entities">
        /// The entities supplied to OnBeforeSave or OnBeforeDelete.
        /// </param>
        /// <param name="cause">
        /// Whether the operation is a save or delete.
        /// </param>
        /// <returns>
        /// Any entities, along with modified fields and related entities.
        /// </returns>
        protected internal static IDictionary<long, EntityChanges> GetEntityChanges(IList<IEntity> entities, InvalidationCause cause)
        {
            IDictionary<long, EntityChanges> result;
            EntityChanges entityChanges;
            IEntityFieldValues fields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;

            result = new Dictionary<long, EntityChanges>();
            foreach (IEntity entity in entities)
            {
                entityChanges = new EntityChanges();
                entity.GetChanges(out fields, out forwardRelationships, out reverseRelationships);
                if (forwardRelationships != null)
                {
                    entityChanges.RelationshipTypesToEntities.UnionWith(
                        GetModifiedEntitiesForRelationships(forwardRelationships));
                }
                if (reverseRelationships != null)
                {
                    entityChanges.RelationshipTypesToEntities.UnionWith(
                        GetModifiedEntitiesForRelationships(reverseRelationships));
                }
                if (fields != null)
                {
                    entityChanges.FieldTypes.UnionWith(fields.FieldIds);
                }

                result.Add(entity.Id, entityChanges);
            }

            return result;
        }

        /// <summary>
        /// Get the entities affected by relationship changes.
        /// </summary>
        /// <param name="relationships">
        /// The list of changes to check. This cannot be null.
        /// </param>
        /// <returns>
        /// Affected entities.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        protected internal static IList<RelationshipTypeToEntity> GetModifiedEntitiesForRelationships(
            IDictionary<long, IChangeTracker<IMutableIdKey>> relationships)
        {
            if (relationships == null)
            {
                throw new ArgumentNullException("relationships");
            }

            List<RelationshipTypeToEntity> result;

            result = new List<RelationshipTypeToEntity>();
            foreach (
                KeyValuePair<long, IChangeTracker<IMutableIdKey>> keyValuePair in
                    relationships.Where(e => e.Value.IsChanged))
            {
                result.AddRange(keyValuePair.Value.Added.Union(keyValuePair.Value.Removed)
                                            .Where(g => g != null)
                                            .Select(x => new RelationshipTypeToEntity(x.Key, new EntityRef(keyValuePair.Key))));
            }

            return result;
        }

    }
}
