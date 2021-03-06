// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Collections.Generic;
using EDC.Database;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    ///     Resource event target.
    /// </summary>
    public class ResourceEventTarget : IEntityEventSave, IEntityEventDelete
    {
        #region IEntityEventSave

        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            object duplicates;
            HashSet<long> entitiesToDeleteSet = new HashSet<long>();

            // Get any merged duplicates to delete
            if (state.TryGetValue(ResourceKeyHelper.MergedDuplicatesStateKey, out duplicates))
            {
                var duplicatesAsHashSet = duplicates as HashSet<long>;

                if (duplicatesAsHashSet != null && duplicatesAsHashSet.Count != 0)
                {
                    entitiesToDeleteSet.UnionWith(duplicatesAsHashSet.Select(id => EventTargetStateHelper.GetIdFromTemporaryId(state, id)));
                }
            }

            Dictionary<long, ResourceKeyDataHash> dataHashesToDelete = ResourceKeyHelper.GetResourceKeyDataHashToDeleteState(state);

            // Get any datahashes to delete
            if (dataHashesToDelete != null && dataHashesToDelete.Count != 0)
            {
                entitiesToDeleteSet.UnionWith(dataHashesToDelete.Keys);
            }

            if (entitiesToDeleteSet.Count != 0)
            {
                Entity.Delete(entitiesToDeleteSet);
            }

	        object movedEntitiesObject;
			if ( state.TryGetValue( "movedEntities", out movedEntitiesObject ) )
			{
				Dictionary<long, ISet<long>> movedEntities = movedEntitiesObject as Dictionary<long, ISet<long>>;

				if ( movedEntities != null )
				{
					using ( DatabaseContext context = DatabaseContext.GetContext( ) )
					{
						foreach ( KeyValuePair<long, ISet<long>> pair in movedEntities )
						{
							using ( IDbCommand command = context.CreateCommand( "spPostEntityMove", CommandType.StoredProcedure ) )
							{
								command.AddIdListParameter( "@entityIds", pair.Value );
								command.AddParameter( "@tenantId", DbType.Int64, RequestContext.TenantId );
								command.AddParameter( "@solutionId", DbType.Int64, pair.Key );

								command.ExecuteNonQuery( );
							}
						}
					}
				}
			}

			// Save the resource key data hashes for any resources at the reverse end
			// of any key relationships.            
			ResourceKeyHelper.SaveResourceKeyDataHashesForReverseRelationships(entities, state);
        }

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
			if ( entities != null )
			{
				long inSolutionId = WellKnownAliases.CurrentTenant.InSolution;

				Dictionary<long, ISet<long>> movedEntities = new Dictionary<long, ISet<long>>( );

				foreach ( IEntity entity in entities )
				{
					IEntityFieldValues fields;
					IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
					IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;
					entity.GetChanges( out fields, out forwardRelationships, out reverseRelationships );

					IChangeTracker<IMutableIdKey> inSolutionChanges;
					if ( forwardRelationships != null && forwardRelationships.TryGetValue( inSolutionId, out inSolutionChanges ) )
					{
						if ( inSolutionChanges != null && inSolutionChanges.Count > 0 )
						{
							var newInSolutionValue = inSolutionChanges.FirstOrDefault( );

							if ( newInSolutionValue != null )
							{
								IReadOnlyDictionary<long, ISet<long>> cacheValues;
								if ( EntityRelationshipCache.Instance.TryGetValue( new EntityRelationshipCacheKey( entity.Id, Direction.Forward ), out cacheValues ) )
								{
									ISet<long> inSolutionValues;
									if ( cacheValues.TryGetValue( inSolutionId, out inSolutionValues ) )
									{
										if ( inSolutionValues != null && inSolutionValues.Count > 0 )
										{
											long existingInSolutionValue = inSolutionValues.FirstOrDefault( );

											if ( existingInSolutionValue != newInSolutionValue.Key )
											{
												ISet<long> set;
												if ( !movedEntities.TryGetValue( existingInSolutionValue, out set ) )
												{
													set = new HashSet<long>( );
													movedEntities [ existingInSolutionValue ] = set;
												}

												set.Add( entity.Id );
											}
										}
									}
								}
							}
						}
					}
				}

				if ( movedEntities.Count > 0 )
				{
					state[ "movedEntities" ] = movedEntities;
				}
			}

            ResourceKeyHelper.SaveResourceKeyDataHashes(entities, ResourceKeyHelper.SaveContext.Resource, state);

            return false;            
        }        

        #endregion IEntityEvent

        #region IEntityDelete


        /// <summary>
        ///     Called before deleting an enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <returns>
        ///     True to cancel the delete operation; false otherwise.
        /// </returns>
        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
	        return false;
        }

        /// <summary>
        ///     Called after deletion of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
        }

        #endregion

        /// <summary>
        /// Ensures that when adding the specified relationship to the specified entity that any other existing relationships (for the specified relationship)
        /// are removed from the related entity.
        /// </summary>
        /// <param name="relationship">The relationship being added .</param>
        /// <param name="isReverse"></param>
        /// <param name="entity">The entity being saved.</param>
        /// <param name="changedRelationships">The changed relationships.</param>
        /// <param name="saveGraph">The save graph.</param>
        internal static void EnsureIsOnlyRelationship(EntityRef relationship, bool isReverse, IEntity entity, IDictionary<long, IChangeTracker<IMutableIdKey>> changedRelationships, SaveGraph saveGraph)
        {
            IChangeTracker<IMutableIdKey> relationshipChanges;

            if (!changedRelationships.TryGetValue(relationship.Id, out relationshipChanges)) return;

            // Check for additions only
            if (relationshipChanges.Added == null || !relationshipChanges.Added.Any()) return;

            foreach (var addedType in relationshipChanges.Added)
            {
                // Get the entity at the other end of the relationship
                var targetEntity = Entity.Get(addedType.Key);
                var existingSourceEntity = targetEntity.GetRelationships(relationship, isReverse ? Direction.Forward : Direction.Reverse).FirstOrDefault();

                if (existingSourceEntity == null || existingSourceEntity.Id == entity.Id) continue;

                // The targetEntity has another entity with the specified relationship
                // Delete the relationships to the other entities
                // Note: we are making changes to the source not the target as making changes to the target will
                // fail with security errors if the target is a type
                IEntity entitySaveGraph;

                var writeableExistingSource = saveGraph.Entities.TryGetValue(existingSourceEntity.Id, out entitySaveGraph)
                    ? entitySaveGraph.AsWritable()
                    : existingSourceEntity.AsWritable();

                writeableExistingSource.GetRelationships(relationship, isReverse ? Direction.Reverse : Direction.Forward).Entities.Remove(targetEntity);

                // Add to save graph so change happens efficiently
                saveGraph.Entities[existingSourceEntity.Id] = writeableExistingSource;
            }
        }
    }
}