// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.EntityRequests
{
    public partial class EntityInfoService : IEntityInfoWrite
    {
        /// <summary>
        /// Create a new <see cref="EntityInfoService"/>.
        /// </summary>
        public EntityInfoService()
        {
            EntityAccessControlService = Factory.EntityAccessControlService;
        }
        
        private IEntityAccessControlService EntityAccessControlService { get; set; }

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        public void DeleteEntity(EntityRef entityId)
        {
            // Validate arguments
            if (entityId == null)
                throw new ArgumentNullException("entityId");

            // Process delete
            DatabaseContext.RunWithRetry(() => Entity.Delete(entityId));
        }

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="entities">The entities to delete.</param>
        public void DeleteEntities(IEnumerable<EntityRef> entities)
        {
            // Validate arguments
            if (entities == null)
                throw new ArgumentNullException("entities");
                                
            // Process delete
            DatabaseContext.RunWithRetry(() => Entity.Delete(entities));
        }
        
        /// <summary>
        /// Creates a new entity.
        /// </summary>
        public EntityRef CreateEntity(EntityData entityData)
        {
            try
            {
				var entityMap = CreateEntityGetMap( entityData );
	            IEntity entity = entityMap[ entityData.Id.Id ];
                long id = entity == null ? -1 : entity.Id;

                return new EntityRef(id);
            }
            catch (CardinalityViolationException cvEx)
            {
                EventLog.Application.WriteWarning("Create Entity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), cvEx.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("CreateEntity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), ex);
                throw;
            }
        }

		/// <summary>
		/// Creates a new entity returning the map of old to new entity ids.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <returns>A map of old to new entity ids.</returns>
		/// <exception cref="System.ArgumentNullException">entityData</exception>
		/// <exception cref="System.ArgumentException">entityData.TypeId must be set.</exception>
		public IDictionary<long, IEntity> CreateEntityGetMap( EntityData entityData )
		{
			try
			{
				// Validate arguments
				if ( entityData == null )
					throw new ArgumentNullException( "entityData" );

				if ( entityData.TypeIds == null || entityData.TypeIds.Count < 0 )
					throw new ArgumentException( "entityData.TypeId must be set." );

				// Process create
				entityData.DataState = DataState.Create;

				return CreateUpdateDeleteImpl( entityData, true ); 
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( "CreateEntity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug( entityData ), ex );
				throw;
			}
        }

        /// <summary>
        /// Clones the entity at the root of the request then updates it or any cloned or original related entities as needed.
        /// </summary>
        /// <param name="entityData">The entity data.</param>
        /// <returns></returns>
        public IDictionary<long, long> CloneAndUpdateEntity(EntityData entityData)
        {
            try
            {
                // Validate arguments
                if (entityData == null)
                    throw new ArgumentNullException(nameof(entityData));

                // Get the entity to clone
                IEntity entity = Entity.Get(entityData.Id);

                if (entity.IsTemporaryId)
                {
                    throw new InvalidOperationException("Cannot clone a temporary entity.");
                }

                // Clone it 
                IEntity clone = entity.Clone(CloneOption.Deep);                

                // Do a save and store the mapping of old to new ids
                var clonedIdsMap = clone.Save();

                // Update the cloned ids in the entity graph
                UpdateEntityDataClonedIds(entityData, clonedIdsMap);

                UpdateEntity(entityData);

                if (!string.IsNullOrWhiteSpace(entity.Alias))
                {
                    // Clear alias to prevent duplicates
                    clone.SetField("core:alias", null);
                    clone.Save();
                }

                return clonedIdsMap;
            }
            catch (CardinalityViolationException cvEx)
            {
                EventLog.Application.WriteWarning("Clone Entity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), cvEx.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("CloneEntity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), ex);
                throw;
            }            
        }

        /// <summary>
        /// Updates an entity.
        /// </summary>
        public void UpdateEntity(EntityData entityData)
        {
            try
            {
                // Validate arguments
                if (entityData == null)
                    throw new ArgumentNullException("entityData");

                // Process update
                entityData.DataState = DataState.Update;

                CreateUpdateDeleteImpl(entityData, true);

                NotifyRootModification( entityData.Id );
            }
            catch (CardinalityViolationException cvEx)
            {
                EventLog.Application.WriteWarning("Update Entity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), cvEx.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("UpdateEntity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), ex);
                throw;
            }
        }

        /// <summary>
        /// Decodes entity data.
        /// </summary>
        /// <param name="entityData">The data</param>
        /// <param name="persistChanges">If true, the nugget is intended to be persisted.</param>
        public IEntity DecodeEntity(EntityData entityData, bool persistChanges = false)
        {
            try
            {
                if (entityData == null)
                    return null;

                var entity = CreateUpdateDeleteImpl(entityData, persistChanges);
                return entity[entityData.Id.Id];
            }
            catch (CardinalityViolationException cvEx)
            {
                EventLog.Application.WriteWarning("Decode Entity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), cvEx.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("DecodeEntity failed:\n{0}\n\nException:\n{1}", EntityDataHelper.GetDebug(entityData), ex);
                throw;
            }
        }

		/// <summary>
		/// Walk the entity data tree, first creating any new entities, then updating
		/// the entities and finally deleting any marked for delete.
		/// </summary>
		/// <param name="entityDataRoot">The entity data root.</param>
		/// <param name="persistChanges">If false, the entities are created in memory, but not persisted.</param>
		/// <param name="retries">The retries.</param>
		/// <returns>
		/// A mapping of old to new entity ids.
		/// </returns>
		/// <exception cref="System.ArgumentException">entityData.TypeIds must be set when creating new instances.
		/// or
		/// Attempted to create an entity using a temporary entity Id</exception>
		/// <exception cref="System.InvalidOperationException">Entity ID was not set and DataState was not create.
		/// or
		/// Entity not found. Id= + entityDataNode.Id
		/// or
		/// We have a newly created object that was not marked as DataState.Create. Id= + entityDataNode.Id</exception>
		private Dictionary<long, IEntity> CreateUpdateDeleteImpl( EntityData entityDataRoot, bool persistChanges, int retries = 0 )
        {
            Dictionary<long, IEntity> entityMap = null;

            DatabaseContext.RunWithRetry(() =>
            {


                var originalRootState = entityDataRoot.DataState;

                // get the distinct nodes so we only have to walk the tree once
                var distinctNodes = WalkEntityDataTree(entityDataRoot, null);

                // ensure any deleted or created items cause entities that refer to them to be marked as changed 
                CascadeUpRelationshipChanges(distinctNodes);

                /////
                // Track the created id's.
                /////
                entityMap = new Dictionary<long, IEntity>();

                var toSave = new List<IEntity>();
                var toDelete = new HashSet<long>();

                // 
                // Create the entities but do not save them
                //
                foreach (var entityDataNode in distinctNodes)
                {
                    if (entityDataNode.DataState == DataState.Create)
                    {
                        bool hasId = entityDataNode.Id != null && entityDataNode.Id.HasId;
                        long id = hasId ? entityDataNode.Id.Id : -1;

                        if (!persistChanges && !hasId && !string.IsNullOrWhiteSpace(entityDataNode.Id?.Alias))
                        {
                            // entity ref has no id but has an alias
                            // resolve alias to id rather than creating a temp
                            id = entityDataNode.Id.ResolveId();
                            hasId = true;
                        }

                        if (entityDataNode.TypeIds == null || entityDataNode.TypeIds.Count <= 0)
                            throw new ArgumentException("entityData.TypeIds must be set when creating new instances.");

                        if (entityDataNode.TypeIds.Any(tid => tid.Id > 0 && EntityTemporaryIdAllocator.IsAllocatedId(tid.Id)))
                            throw new ArgumentException("Attempted to create an entity using a temporary entity Id");

                        Entity newEntity = !persistChanges && hasId ? new Entity(entityDataNode.TypeIds, id) : new Entity(entityDataNode.TypeIds);

                        // if we were not given an Id to use as a reference, use new temporary one. 
                        if (!hasId)
                        {
                            id = newEntity.Id;
                            entityDataNode.Id = id;
                        }

                        toSave.Add(newEntity);
                        entityMap[id] = newEntity;
                        entityDataNode.DataState = DataState.Update;
                    }
                    else
                    {
                        if (entityDataNode.Id == null)
                            throw new InvalidOperationException("Entity ID was not set and DataState was not create.");
                        long id = entityDataNode.Id.Id;

                        if (id > 0 && !entityMap.ContainsKey(id))
                        {
                            entityMap[id] = Entity.Get(id);
                            if (entityMap[id] == null && entityDataNode.DataState != DataState.Delete)
                            {
                                throw new InvalidOperationException("Entity not found. Id=" + entityDataNode.Id);
                            }
                        }

                        // This is not a nice test for a new node, but I don't have a better one
                        if (entityDataNode.DataState == DataState.Update && id > EntityId.MinTemporary)
                            throw new InvalidOperationException("We have a newly created object that was not marked as DataState.Create. Id=" + entityDataNode.Id);

                    }
                }

                // Update the entities in memory
                UpdateImpl(distinctNodes, entityMap, toSave, toDelete);

                // Persist changes
                if (persistChanges)
                {
                    // Gather the ids for deletion
                    var delete = toDelete.Select(d => new EntityRef(d)).ToList();

                    delete.AddRange(distinctNodes.Where(n => n.DataState == DataState.Delete).Select(n => n.Id));
                    
                    // Check delete permissions before the update takes place
                    EntityAccessControlService.Demand(delete, new[] { Permissions.Read, Permissions.Delete });                    

                    // Save all the created and changed entities (allowing entities to move from nodes deleted above)
                    Entity.Save(toSave.Distinct());

                    using (new SecurityBypassContext())
                    {
                        Entity.Delete(delete);
                    }
                }

                // Return the root entity
                if (originalRootState == DataState.Delete)
                    entityMap = null;
            });

            return entityMap;
        }        

        /// <summary>
        /// Updates the entity data entity ids using the specified dictionary.
        /// </summary>
        /// <param name="node">The node to update.</param>
        /// <param name="clonedIdsMap">The map of cloned ids.</param>
        private void UpdateEntityDataClonedIds(EntityData node, IDictionary<long, long> clonedIdsMap)
        {
            if (node == null || clonedIdsMap == null || clonedIdsMap.Count == 0)
            {
                return;
            }

            var processedNodes = new HashSet<EntityData>();
            var nodesToProcess = new Queue<EntityData>();

            nodesToProcess.Enqueue(node);

            while (nodesToProcess.Count > 0)
            {
                var currentNode = nodesToProcess.Dequeue();

                if (currentNode == null)
                {
                    continue;
                }

                // Prevent infinite loops
                if (processedNodes.Contains(currentNode))
                {
                    continue;
                }

                processedNodes.Add(currentNode);

                if (currentNode.DataState == DataState.Create)
                {
                    continue;
                }

                // Update node id
                if (currentNode.Id != null)
                {
                    long id = currentNode.Id.Id;

                    if (id > 0)
                    {
                        long clonedId;
                        if (clonedIdsMap.TryGetValue(id, out clonedId))
                        {
                            currentNode.Id = new EntityRef(clonedId);
                        }
                    }                    
                }

                // Update field and relationship ids
                UpdateFieldsClonedIds(currentNode.Fields, clonedIdsMap);
                UpdateRelationshipsClonedIds(currentNode.Relationships, clonedIdsMap, nodesToProcess);                                                               
            }
        }

        /// <summary>
        /// Updates the fields cloned ids.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="clonedIdsMap">The cloned ids map.</param>
        private void UpdateFieldsClonedIds(IReadOnlyCollection<FieldData> fields, IDictionary<long, long> clonedIdsMap)
        {
            if (fields == null || fields.Count == 0)
            {
                return;
            }

            // Update field ids            
            foreach (var field in fields)
            {
                if (field.FieldId != null)
                {
                    long id = field.FieldId.Id;

                    if (id > 0)
                    {
                        long clonedId;
                        if (clonedIdsMap.TryGetValue(id, out clonedId))
                        {
                            field.FieldId = new EntityRef(clonedId);
                        }
                    }                    
                }                
            }            
        }

        /// <summary>
        /// Updates the relationships cloned ids.
        /// </summary>
        /// <param name="relationships">The relationships.</param>
        /// <param name="clonedIdsMap">The cloned ids map.</param>
        /// <param name="nodesToProcess">The nodes to process.</param>
        private void UpdateRelationshipsClonedIds(IReadOnlyCollection<RelationshipData> relationships, IDictionary<long, long> clonedIdsMap, Queue<EntityData> nodesToProcess)
        {
            if (relationships == null || relationships.Count == 0)
            {
                return;
            }

            // Update relationship ids            
            foreach (var relationship in relationships)
            {
                if (relationship.RelationshipTypeId != null)
                {
                    long id = relationship.RelationshipTypeId.Id;

                    if (id > 0)
                    {
                        long clonedId;
                        if (clonedIdsMap.TryGetValue(id, out clonedId))
                        {
                            relationship.RelationshipTypeId = new EntityRef(clonedId);
                        }
                    }                    
                }

                if (relationship.Instances != null)
                {
                    // Get more nodes
                    foreach (var instance in relationship.Instances)
                    {
                        nodesToProcess.Enqueue(instance.Entity);
                        nodesToProcess.Enqueue(instance.RelationshipInstanceEntity);
                    }
                }

                if (relationship.Entities != null)
                {
                    // Get more nodes        
                    foreach (var entity in relationship.Entities)
                    {
                        nodesToProcess.Enqueue(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Walk the tree, if there are deleted entities, mark the referee relationship as deleted. If there are relationship changes, mark the entity as changed. 
        /// Create entities while we are at it
        /// </summary>
        private void CascadeUpRelationshipChanges(List<EntityData> distinctNodes)
        {
            foreach (var entityData in distinctNodes)
            {

                bool isChanged = false;

                if (entityData.Relationships != null)
                {
                    foreach (RelationshipData receivedRelationshipDefn in entityData.Relationships)
                    {
                        foreach (RelationshipInstanceData instance in receivedRelationshipDefn.Instances)
                        {
                            // Make sure our state is correct
                            switch (instance.DataState)
                            {
                                case DataState.Create:
                                case DataState.Delete:
                                case DataState.Update:
                                    isChanged = true;
                                    break;

                                case DataState.Unchanged:
                                    if (instance.Entity.DataState == DataState.Delete)
                                    {
                                        instance.DataState = DataState.Delete;
                                        isChanged = true;
                                    }
                                    break;
                            }

                        }
                    }
                }

                if (entityData.DataState == DataState.Unchanged && isChanged)
                    entityData.DataState = DataState.Update;
            }
        }
        
        /// <summary>
        /// Walk the EntityDataTree performing an action on either the entity or the relationship instance.
        /// Each node will be passed only once even if loops are present.
        /// </summary>
        /// <returns>A distict list of the EntityData objects passed.</returns>
        private List<EntityData> WalkEntityDataTree(EntityData entityData, Action<EntityData> actionOnEntity)
        {
            var passedNodes = new HashSet<EntityData>();
            WalkEntityDataTree_inner(entityData, actionOnEntity, passedNodes);

            return passedNodes.ToList();
        }

        private void WalkEntityDataTree_inner(EntityData entityData, Action<EntityData> actionOnEntity, HashSet<EntityData> passedNodes)
        {
            passedNodes.Add(entityData);

            if (actionOnEntity != null)
                actionOnEntity(entityData);

            if (entityData.Relationships != null)
            {
                foreach (RelationshipData receivedRelationshipDefn in entityData.Relationships)
                {
                    foreach (RelationshipInstanceData instance in receivedRelationshipDefn.Instances)
                    {
                        if (!passedNodes.Contains(instance.Entity))
                            WalkEntityDataTree_inner(instance.Entity, actionOnEntity, passedNodes);
                        if (instance.RelationshipInstanceEntity != null && !passedNodes.Contains(instance.RelationshipInstanceEntity))
                            WalkEntityDataTree_inner(instance.RelationshipInstanceEntity, actionOnEntity, passedNodes);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the recursive processing of creating/updating/deleting entities.
        /// </summary>
        private void UpdateImpl(List<EntityData> distinctNodes, Dictionary<long, IEntity> entityMap, List<IEntity> saveList, HashSet<long> toDelete)
        {
            foreach (var entityDataNode in distinctNodes)
            {
                if (entityDataNode.DataState == DataState.Update || entityDataNode.DataState == DataState.Delete)
                {
                    if (entityDataNode.Id == null)
                        throw new ArgumentException("entityData.Id must be set when updating instances.");

                    var entity = entityMap[entityDataNode.Id.Id];
                    if (entity == null && entityDataNode.DataState != DataState.Delete)
                        throw new ArgumentException("Unable to find referenced Entity. EntityId = " + entityDataNode.Id);

                    // Visit relationships and update them
                    if (entityDataNode.Relationships != null)
                    {
                        foreach (RelationshipData receivedRelationshipDefn in entityDataNode.Relationships)
                        {
                            UpdateEntityFromRelationshipData(ref entity, receivedRelationshipDefn, entityMap, saveList, toDelete);
                        }
                    }

					if ( entityDataNode.DataState == DataState.Update )
					{
						// Set field values
						if ( entityDataNode.Fields != null )
						{
							foreach ( FieldData fieldData in entityDataNode.Fields )
							{
                                UpdateEntityFromFieldData(ref entity, fieldData, entityMap);
							}
						}
					}

					saveList.Add(entity);
                }
            }
        }

        /// <summary>
        /// Update an individual field
        /// </summary>
        private static void UpdateEntityFromFieldData(ref IEntity entity, FieldData fieldData, Dictionary<long, IEntity> entityMap)
        {
            if ( fieldData == null )
                throw new NullReferenceException( "Unexpected null field" );
            if ( fieldData.FieldId == null )
                throw new NullReferenceException( "Unexpected null field id" );            

            Field field = Entity.Get<Field>( fieldData.FieldId );
            if ( field == null )
                throw new Exception( string.Format( "Specified field was not actually a field. {0}", fieldData.FieldId ) );

            if ( fieldData.Value == null )
                throw new NullReferenceException( string.Format( "Unexpected null Value for field {0}", fieldData.FieldId ) );

            if (field.IsFieldVirtual == true)
            {
                return;    
            }

            object updateValue = fieldData.Value.Value;
            object newValue = null;

            if ( updateValue != null )
            {
                // Get the field definition then convert the (unknown) value into the right sort of value. This is a little messy
                var fieldType = field.ConvertToDatabaseType( );
                newValue = fieldType.ConvertFromString( fieldData.Value.Value.ToString( ) );
            }

            if (entity.IsReadOnly)
            {
                entity = entity.AsWritable();
                entityMap[entity.Id] = entity;
            }

            entity.SetField( field, newValue );
        }

		/// <summary>
		/// Update the relationships in an entity from the provided relationshipData
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="receivedRelationshipDefn">The received relationship defn.</param>
		/// <param name="entityMap">The entity map.</param>
		/// <param name="saveList">The save list.</param>
		/// <param name="toDelete">To delete.</param>
		/// <exception cref="System.InvalidOperationException">
		/// Relationship had null entity instance
		/// or
		/// or
		/// Updating relationship instance entities is no longer supported.
		/// </exception>
		/// <exception cref="System.NotImplementedException"></exception>
        private static void UpdateEntityFromRelationshipData(ref IEntity entity, RelationshipData receivedRelationshipDefn, Dictionary<long, IEntity> entityMap, List<IEntity> saveList, HashSet<long> toDelete)
        {
            // Check if anything actually got changed with this relationship
            bool anySpecificAddOrRemove = receivedRelationshipDefn.Instances.Any( inst => inst.DataState != DataState.Unchanged );
            bool anyChanges = receivedRelationshipDefn.DeleteExisting
                              || receivedRelationshipDefn.RemoveExisting
                              || anySpecificAddOrRemove;

            if (!anyChanges)
                return;

            if (entity.IsReadOnly)
            {
                entity = entity.AsWritable();
                entityMap[entity.Id] = entity;
            }

            // Get relationship metadata
            Relationship relationshipDefn = Entity.Get<Relationship>( receivedRelationshipDefn.RelationshipTypeId.Id );
            Direction direction = Entity.GetDirection(receivedRelationshipDefn.RelationshipTypeId, receivedRelationshipDefn.IsReverse);

            var relatedEntities = entity.GetRelationships(relationshipDefn, direction);

            bool removeExistingOnForeignEntity = false;

            // Clear existing values if requested
            HashSet<long> considerDeleting = null;

            if (receivedRelationshipDefn.DeleteExisting)
            {
                // Delete existing entities from source
                considerDeleting = new HashSet<long>(relatedEntities.Select(ri => ri.Entity.Id));
                relatedEntities.Clear();
            }
            else if (receivedRelationshipDefn.RemoveExisting)
            {
                // Remove existing relationships from source
                relatedEntities.Clear( );
            }
            else if (receivedRelationshipDefn.AutoCardinality)
            {
                // Remove existing relationships from source, if required by cardinality
                bool toOne = relationshipDefn.IsLookup( direction );
                if ( toOne && anySpecificAddOrRemove )
                {
                    receivedRelationshipDefn.RemoveExisting = true;
                    relatedEntities.Clear( );
                }
            }
            if (receivedRelationshipDefn.AutoCardinality)
            {
                // Remove existing relationships from target, if required by cardinality
                bool fromOne = relationshipDefn.IsLookup( direction.Reverse( ) );
                removeExistingOnForeignEntity = fromOne;
            }


            // Apply specific relationship instance changes
            foreach (RelationshipInstanceData instance in receivedRelationshipDefn.Instances)
            {
                if (instance.Entity == null)
                    throw new InvalidOperationException("Relationship had null entity instance");

                long foreignId = instance.Entity.Id.Id;

                if (considerDeleting != null && (instance.DataState == DataState.Create || instance.DataState == DataState.Update))
                {
                    if (considerDeleting.Contains(foreignId))
                        considerDeleting.Remove(foreignId);
                }

                switch (instance.DataState)
                {
                    case DataState.Unchanged:
                        break; 

                    case DataState.Create:

                        var foreignEntity = entityMap[foreignId];
                        if (foreignEntity == null)
                        {
                            throw new InvalidOperationException(string.Format("No entity was passed in for ID {0} on relationship {1}", foreignId, receivedRelationshipDefn.RelationshipTypeId));
                        }
                        if ( removeExistingOnForeignEntity )
                        {
                            IEntity foreignWritable = foreignEntity;
                            if ( foreignWritable.IsReadOnly )
                            {
                                foreignWritable = foreignWritable.AsWritable( );
                                saveList.Add( foreignWritable );
                                entityMap [ foreignId ] = foreignWritable;
                            }
                            var otherEntities = foreignWritable.GetRelationships( relationshipDefn, direction.Reverse( ) );

                            var entityId = entity.Id;

                            /////
                            // Ensure the relationship is not removed from the other direction when the instance being created already exists
                            /////
                            foreach (var otherEntity in otherEntities.Where(e => e.Id != entityId).ToList( ) )
	                        {
		                        otherEntities.Remove( otherEntity );
	                        }
                        }

                        // Note that the underlying storage system deals with flipping "from" and "to" when writing reverse relations to the DB 
                        relatedEntities.Add(foreignEntity);

                        break;

                    case DataState.Delete:
                        foreach (var relInst in relatedEntities)
                        {
                            if (relInst == null || relInst.Entity == null)
                                continue; // assert false
                            if (relInst.Entity.Id == foreignId)
                            {
                                relatedEntities.Remove(relInst);
                                break;
                            }
                        }

                        break;

                    case DataState.Update:  // replace the instance
                        throw new InvalidOperationException( "Updating relationship instance entities is no longer supported." );

                    default:
                        throw new NotImplementedException();
                }

            }

            // If deleteExisting = true, then delete all entities that were not re-added.
            if (considerDeleting != null)
            {
                foreach (long id in considerDeleting)
                {
                    toDelete.Add(id);
                }
            }
        }

        /// <summary>
        /// Modifies the type(s) of an entity.
        /// </summary>
        /// <remarks>
        /// An entity may be of multiple type, so long as the types are willing to be shared.
        /// Specify the Id and TypeIds of the EntityData parameter. No other data is required.
        /// </remarks>
        /// <param name="updatedEntityData">The list of entities to load.</param>
        public void UpdateEntityType(EntityData updatedEntityData)
        {
            IEntity entity = Entity.Get(updatedEntityData.Id, true);

            var newTypes = updatedEntityData.TypeIds.Select(t => t.Id);

            Entity.ChangeType(entity, newTypes);
            entity.Save();
        }

        /// <summary>
        /// Check if a cache invalidation is required on the basis of the root entity, even if it has not been modified.
        /// </summary>
        /// <param name="entityId">The ID of the root entity.</param>
        private void NotifyRootModification( EntityRef entityId )
        {
            // Note: if user did not have perm to modify the root entity, then we would not have reached this point.
            // (And we're just invalidating caches)
            using ( new SecurityBypassContext( ) )
            {
                IEntity entity = entityId.Entity;

                foreach ( IEntity ientityType in entity.EntityTypes )
                {
                    EntityType entityType = ientityType.As<EntityType>( );
                    if ( entityType == null )
                        continue;
                    if ( entityType.IsMetadata == true )
                    {
                        Factory.MultiCacheInvalidator.OnEntityChange( new [ ] { entity }, Model.CacheInvalidation.InvalidationCause.Save, null );
                        break;
                    }
                }
            }
        }
    }
}
