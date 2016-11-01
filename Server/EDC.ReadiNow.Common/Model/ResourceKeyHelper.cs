// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using EDC.Collections.Generic;
using EDC.Common;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.EventClasses;
using System.Threading;
using EDC.Security;
using System.Globalization;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Diagnostics;
using EDC.Database;

namespace EDC.ReadiNow.Model
{
    public static class ResourceKeyHelper
    {
        #region Constants

        /// <summary>
        ///     The name of the unsaved data hashes by id state key.
        /// </summary>
        private const string UnsavedDataHashesByIdStateKey = "ResourceKeyHelper_UnsavedDataHashesByIdStateKey";


        /// <summary>
        ///     The name of the unsaved data hashes by datahash state key.
        /// </summary>
        private const string UnsavedDataHashesByDataHashStateKey = "ResourceKeyHelper_UnsavedDataHashesByDataHashStateKey";


        /// <summary>
        ///     The name of the merged duplicates state key.
        /// </summary>
        public static readonly string MergedDuplicatesStateKey = "ResourceKeyHelper_MergedDuplicates";


        /// <summary>
        ///     The name of the datahash entities to delete state key.
        /// </summary>
        public static readonly string DataHashEntitiesToDeleteStateKey = "ResourceKeyHelper_DataHashEntitiesToDelete";


        /// <summary>
        ///     The modified relationships state key.
        /// </summary>
        public static readonly string ModifiedRelationshipsStateKey = "ResourceKeyHelper_ModifiedRelationships";
       
        /// <summary>
        ///     Value used for null fields and relationships.
        /// </summary>
        private const string NullValue = "[NULL]";

        #endregion Constants

        #region Public Methods

        /// <summary>
        ///     Saves the resource key data hashes.
        ///     This method is to be called to recalculate the data hashes
        ///     when resources that have keys are saved.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="saveContext">The save context.</param>
        /// <param name="state">The state.</param>
        internal static void SaveResourceKeyDataHashes(IEnumerable<IEntity> entities, SaveContext saveContext, IDictionary<string, object> state)
        {
			if ( !state.ContainsKey( UnsavedDataHashesByIdStateKey ) )
			{
				state [ UnsavedDataHashesByIdStateKey ] = new Dictionary<long, ResourceKeyDataHash>( );
			}

			if ( !state.ContainsKey( UnsavedDataHashesByDataHashStateKey ) )
			{
				state [ UnsavedDataHashesByDataHashStateKey ] = new Dictionary<Tuple<string, long>, Resource>( );
			}

			if ( !state.ContainsKey( DataHashEntitiesToDeleteStateKey ) )
			{
				state [ DataHashEntitiesToDeleteStateKey ] = new Dictionary<long, ResourceKeyDataHash>( );
			}

            foreach (IEntity entity in entities)
            {
                if (entity == null)
                {
                    continue;
                }

                switch (saveContext)
                {
                    case SaveContext.Resource:
                        SaveResourceKeyDataHashesForResource(entity, state);
                        break;

                    case SaveContext.ResourceKey:                    
                        SaveResourceKeyDataHashesForResourceKey(entity, state);
                        break;
                }
            }
        }


        /// <summary>
        /// Saves the resource key data hashes when entities at the other
        /// end of resource key relationships have changed.
        /// For example a report has a key relationship of resource in folder.
        /// When a folder is saved this function will regenerate the hash for any reports.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        internal static void SaveResourceKeyDataHashesForReverseRelationships(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {            
            state[UnsavedDataHashesByIdStateKey] = new Dictionary<long, ResourceKeyDataHash>();
            state[UnsavedDataHashesByDataHashStateKey] = new Dictionary<Tuple<string, long>, Resource>();
            state[DataHashEntitiesToDeleteStateKey] = new Dictionary<long, ResourceKeyDataHash>();

            // Dictionary of resources, to a dictionary of resource keys
            var resourcesNeedingDataHashUpdate = new Dictionary<long, ResourceToResourceKeys>();

            // Get the modified relationships from the state
            var modifiedRelationshipsState = EventTargetStateHelper.GetValue<ModifiedRelationshipsState>(state, ModifiedRelationshipsStateKey);

            // There are no modifications so return
            if (modifiedRelationshipsState == null ||
				 ( modifiedRelationshipsState.ForwardRelationshipChanges.Count <= 0 &&
				   modifiedRelationshipsState.ReverseRelationshipChanges.Count <= 0 ) )
            {
                return;
            }

            // For all the updated resources get all the resources with resource keys at the other
            // end of modified relationships that need their resource data key hash updated.
            foreach (IEntity entity in entities)
            {
                if (entity == null)
                {
                    continue;
                }

                GetResourcesNeedingDataHashUpdate(entity, modifiedRelationshipsState, resourcesNeedingDataHashUpdate, state);
            }

            // Update the data hashes for the resources and throw an exception if any duplicates are found
            UpdateKeyDataHashesForResources(resourcesNeedingDataHashUpdate, state);
        }

        /// <summary>
        ///     Gets the resource key data hashes.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        internal static ISet<string> GetResourceKeyDataHashes(this ResourceKey resourceKey, IEntity entity)
        {
            var dataHashes = new HashSet<string>();

            var fieldsDataBuilder = new StringBuilder();

            // Create the hash input for any key fields.
            bool haveValues = resourceKey.GenerateKeyFieldsHashInput(entity, fieldsDataBuilder);

            // Create the hash input for any key relationships.
            // Note: key relationships are cross joined together. This is required to handle xToMany relationships.
            // e.g. say we have a report that belongs to two folders f1 and f2 and the report in folder
            // relationship is a key. Also assume that the report to application relationship is also a key and that the report
            // only belongs to app1.
            // Two data hashes will be created using the cross join of the key relationships as input, i.e. (f1, app1) and (f2, app2).            
            IEnumerable<StringBuilder> relationshipDataBuilders = resourceKey.GenerateKeyRelationshipsHashInput(entity);

	        var stringBuilders = relationshipDataBuilders as IList<StringBuilder> ?? relationshipDataBuilders.ToList( );

			if ( stringBuilders.Count > 0 )
            {
                haveValues = true;
            }

            if (!haveValues) return dataHashes;

			if ( stringBuilders.Count > 0 )
            {
                foreach (StringBuilder relationshipDataBuilder in stringBuilders)
                {
                    // Prepend any fields input to each relationship input
                    if (fieldsDataBuilder.Length > 0)
                    {
                        relationshipDataBuilder.Insert(0, fieldsDataBuilder);
                    }

                    // Calculate the hashes
                    dataHashes.Add(CryptoHelper.GetSha1Hash(relationshipDataBuilder.ToString()));
                }
            }
            else if (fieldsDataBuilder.Length > 0)
            {
                // Hash the key fields
                dataHashes.Add(CryptoHelper.GetSha1Hash(fieldsDataBuilder.ToString()));
            }

            return dataHashes;
        }

        #endregion Public Methods

        #region Non-Public Methods

        /// <summary>
        ///     Adds the merged duplicate entity to the state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="entityId">The entity id.</param>
        private static void AddMergedDuplicateToState(IDictionary<string, object> state, long entityId)
        {
            object duplicates;
            HashSet<long> duplicatesHashSet;

            if (!state.TryGetValue(MergedDuplicatesStateKey, out duplicates))
            {
                duplicatesHashSet = new HashSet<long>();
                state[MergedDuplicatesStateKey] = duplicatesHashSet;
            }
            else
            {
                duplicatesHashSet = duplicates as HashSet<long>;
                if (duplicatesHashSet == null)
                {
                    duplicatesHashSet = new HashSet<long>();
                    state[MergedDuplicatesStateKey] = duplicatesHashSet;
                }
            }

            duplicatesHashSet.Add(entityId);
        }


        /// <summary>
        ///     Adds the specified resource key datahashes to be deleted the state.
        /// </summary>
        /// <param name="state">The state.</param>        
        /// <param name="dataHashesToDelete">The datahashes to delete.</param>
        private static void AddResourceKeyDataHashToDeleteToState(IDictionary<string, object> state, IEnumerable<ResourceKeyDataHash> dataHashesToDelete)
        {
            Dictionary<long, ResourceKeyDataHash> dataHashesDictionary = GetResourceKeyDataHashToDeleteState(state);            

            foreach(ResourceKeyDataHash dataHash in dataHashesToDelete)
            {                
                dataHashesDictionary[dataHash.Id] = dataHash;
            }
        }

        /// <summary>
        ///     Determines whether this instance can merge the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///     <c>true</c> if this instance can merge the specified source; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanMerge(IEntity source, IEntity target)
        {
            bool canMerge = false;

            // Can only merge if it will not result in a loss of data.
            // Check that the merge is a promote, i.e. base type -> derived type.
            // Check that the source entity is not a derived type of the target.                        

            HashSet<long> targetAssignableTypes = EntityTypeCache.GetAssignableTypes(target.TypeIds);

            if (source.TypeIds.All(targetAssignableTypes.Contains))
            {
                canMerge = true;
            }

            return canMerge;
        }

        /// <summary>
        /// Gets the unsaved resource key data hash dictionary key.
        /// </summary>
        /// <param name="dataHash">The data hash.</param>
        /// <param name="resourceKeyId">The resource key identifier.</param>
        /// <returns></returns>
        private static Tuple<string, long> GetUnsavedResourceKeyDataHashDictionaryKey(string dataHash, long resourceKeyId)
        {
            return new Tuple<string, long>(dataHash, resourceKeyId);
        }

        /// <summary>
        /// Gets the state of the unsaved data hashes by identifier.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private static Dictionary<long, ResourceKeyDataHash> GetUnsavedDataHashesByIdState(IDictionary<string, object> state)
        {
            return EventTargetStateHelper.GetValue<Dictionary<long, ResourceKeyDataHash>>(state, UnsavedDataHashesByIdStateKey);  
        }        

        /// <summary>
        /// Gets the state of the unsaved data hashes by data hash.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private static Dictionary<Tuple<string, long>, Resource> GetUnsavedDataHashesByDataHashState(IDictionary<string, object> state)
        {
            return EventTargetStateHelper.GetValue<Dictionary<Tuple<string, long>, Resource>>(state, UnsavedDataHashesByDataHashStateKey);
        }        

        /// <summary>
        /// Gets the resource key datahash to delete state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        internal static Dictionary<long, ResourceKeyDataHash> GetResourceKeyDataHashToDeleteState(IDictionary<string, object> state)
        {
            return EventTargetStateHelper.GetValue<Dictionary<long, ResourceKeyDataHash>>(state, DataHashEntitiesToDeleteStateKey);
        }

        /// <summary>
        /// Updates the unsaved data hash state dictionaries.
        /// </summary>
        /// <param name="resourceKeyDataHash">The resource key data hash.</param>
        /// <param name="dataHash">The data hash.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="state">The state.</param>
        private static void UpdateUnsavedDataHashStateDictionaries(ResourceKeyDataHash resourceKeyDataHash, string dataHash, Resource resource, ResourceKey resourceKey, IDictionary<string, object> state)
        {
            Dictionary<long, ResourceKeyDataHash> unsavedDataHashesByIdState = GetUnsavedDataHashesByIdState(state);
            Dictionary<Tuple<string, long>, Resource> unsavedDataHashesByDataHashState = GetUnsavedDataHashesByDataHashState(state);

            unsavedDataHashesByIdState[resourceKeyDataHash.Id] = resourceKeyDataHash;
            Tuple<string, long> key = GetUnsavedResourceKeyDataHashDictionaryKey(dataHash, resourceKey.Id);
            unsavedDataHashesByDataHashState[key] = resource;
        }        

        /// <summary>
        /// Removes any unsaved data hash from state dictionaries.
        /// </summary>
        /// <param name="resourceKeyDataHashId">The resource key data hash identifier.</param>
        /// <param name="dataHash">The data hash.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="state">The state.</param>
        private static void RemoveUnsavedDataHashFromStateDictionaries(long resourceKeyDataHashId, string dataHash, ResourceKey resourceKey, IDictionary<string, object> state)
        {
            Dictionary<long, ResourceKeyDataHash> unsavedDataHashesByIdState = GetUnsavedDataHashesByIdState(state);
            Dictionary<Tuple<string, long>, Resource> unsavedDataHashesByDataHashState = GetUnsavedDataHashesByDataHashState(state);

            unsavedDataHashesByIdState.Remove(resourceKeyDataHashId);
            Tuple<string, long> key = GetUnsavedResourceKeyDataHashDictionaryKey(dataHash, resourceKey.Id);
            unsavedDataHashesByDataHashState.Remove(key);
        }

        /// <summary>
        /// Creates the update resource key data hash.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="dataHashes">The data hashes.</param>
        /// <param name="state">The state.</param>
        private static void CreateUpdateResourceKeyDataHashes(Resource resource, ResourceKey resourceKey, ISet<string> dataHashes, IDictionary<string, object> state)
        {
            SaveGraph saveGraph = EventTargetStateHelper.GetSaveGraph(state);

            // Get the key data hashes that apply to this resource and key
            IEnumerable<ResourceKeyDataHash> existingResourceKeyDataHashes = resource.ResourceHasResourceKeyDataHashes.Where(dh => dh.ResourceKeyDataHashAppliesToResourceKey != null && dh.ResourceKeyDataHashAppliesToResourceKey.Id == resourceKey.Id).ToList();

            // Find existing data hashes
            ISet<string> existingDataHashes = existingResourceKeyDataHashes.Select(dh => dh.DataHash).ToSet();

            // Find deleted data hashes
            IDictionary<long, ResourceKeyDataHash> dataHashesToDelete = existingResourceKeyDataHashes.Where(dh => !dataHashes.Contains(dh.DataHash)).ToDictionary(dh => dh.Id);

            // Find new data hashes
            List<string> dataHashesToCreate = dataHashes.Except(existingDataHashes).ToList();

            // Delete any datahashes that don't exist
			if ( dataHashesToDelete.Count > 0 )
            {
                foreach (ResourceKeyDataHash dataHashToDelete in dataHashesToDelete.Values)
                {                    
                    RemoveUnsavedDataHashFromStateDictionaries(dataHashToDelete.Id, dataHashToDelete.DataHash, resourceKey, state);

                    saveGraph.Entities.Remove(dataHashToDelete.Id);
                }               

                AddResourceKeyDataHashToDeleteToState(state, dataHashesToDelete.Values);
            }

            // Create new data hashes
			if ( dataHashesToCreate.Count > 0 )
            {
                foreach (string dataHash in dataHashesToCreate)
                {
                    // The resource data hash entity does not exist.
                    // create it if we have a hash
                    if (string.IsNullOrEmpty(dataHash)) continue;

                    var resourceKeyDataHash = new ResourceKeyDataHash
                    {
                        DataHash = dataHash
                    };

                    saveGraph.Entities[resourceKeyDataHash.Id] = resourceKeyDataHash;

                    UpdateUnsavedDataHashStateDictionaries(resourceKeyDataHash, dataHash, resource, resourceKey, state);

                    // Assign the data hash to the key
                    resourceKeyDataHash.ResourceKeyDataHashAppliesToResourceKey = resourceKey;
                    // Assign the data hash to the resource                           
                    resourceKeyDataHash.ResourceKeyDataHashAppliesToResource = resource;
                }
            }           
        }


        /// <summary>
        ///     Deletes the resource key data hashes.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns></returns>
        private static bool DeleteResourceKeyDataHashes(ResourceKey resourceKey)
        {
            bool continueProcessing = true;

            bool hasKeyAppliesToTypeChanged = HasKeyAppliesToTypeChanged(resourceKey);
            bool haveKeyFieldsOrRelationshipsChanged = HaveKeyFieldsOrRelationshipsChanged(resourceKey);

            if (!haveKeyFieldsOrRelationshipsChanged && !hasKeyAppliesToTypeChanged)
            {
                // Have no key field changes
                // and no key applies to type changes.
                continueProcessing = false;
            }

			bool haveKeyFieldsOrRelationships = ( resourceKey.KeyFields != null && resourceKey.KeyFields.Count > 0 ) ||
												( resourceKey.ResourceKeyRelationships != null && resourceKey.ResourceKeyRelationships.Count > 0 );

            if (hasKeyAppliesToTypeChanged || !haveKeyFieldsOrRelationships)
            {
                // The type that the key applies to has changed or the key has no key fields.
                // Delete all the key hashes that apply to this key.
                IList<long> dataHashIds = resourceKey.ResourceKeyDataHashes.Select(dh => dh.Id).ToList();
                if (dataHashIds.Count > 0)
                {
                    Entity.Delete(dataHashIds);
                }
                resourceKey.ResourceKeyDataHashes.Clear();

                if (!haveKeyFieldsOrRelationships)
                {
                    continueProcessing = false;
                }
            }

            return continueProcessing;
        }

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        private static string GetDisplayNameFromRelationship(Relationship relationship, Direction direction)
        {
            string displayName = string.Empty;

            if (direction == Direction.Forward && relationship.ToType != null)
            {
                displayName = !string.IsNullOrEmpty(relationship.ToName) ? relationship.ToName : relationship.Name;
            }
            else if (direction == Direction.Reverse && relationship.FromType != null)
            {
                displayName = !string.IsNullOrEmpty(relationship.FromName) ? relationship.FromName : relationship.Name;
            }

            return displayName;
        }


        /// <summary>
        /// Gets the duplicate resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="dataHashes">The data hashes.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private static Resource GetDuplicateResource(IEntity resource, ResourceKey resourceKey, ISet<string> dataHashes, IDictionary<string, object> state)
        {
            if (dataHashes == null || dataHashes.Count == 0)
            {
                return null;
            }
            
            Dictionary<Tuple<string, long>, Resource> unsavedDataHashesByDataHashState = GetUnsavedDataHashesByDataHashState(state);

            IEnumerable<Tuple<string, long>> hashKeys = dataHashes.Select(dataHash => GetUnsavedResourceKeyDataHashDictionaryKey(dataHash, resourceKey.Id));            

            Resource duplicateResource = null;

            if (hashKeys.Any(key => unsavedDataHashesByDataHashState.TryGetValue(key, out duplicateResource) &&
                                    duplicateResource.Id != resource.Id))
            {
                return duplicateResource;
            }

            Dictionary<long, ResourceKeyDataHash> dataHashesToDelete = GetResourceKeyDataHashToDeleteState(state);

            int countHashes = dataHashes.Count;            

            // Use a query here for performance reasons. There may be many relationships
            // from a resource key to hashes so going via the entity model is not as efficient.
            using (DatabaseContext dbContext = DatabaseContext.GetContext())
            {
                // See if the current key already has an associated data hash with the same value
                // but for a different entity.
                const string commandTextMany =
@"DECLARE @dataHashesTable TABLE ( DataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL )

INSERT INTO @dataHashesTable ( DataHash )
SELECT NString FROM dbo.fnListToTable( @dataHashes, default, default, default )

SELECT
    relResourceToDataHash.FromId AS Id, relDataHashToKey.FromId AS DataHashId
FROM [dbo].[Relationship] relDataHashToKey	
    JOIN [dbo].[Relationship] relResourceToDataHash ON 
		relResourceToDataHash.ToId = relDataHashToKey.FromId AND 
		relResourceToDataHash.TypeId = @resourceHasResourceKeyDataHashesId AND 
		relResourceToDataHash.TenantId = @tenant
    JOIN [dbo].[Data_NVarChar] dataHash ON 
		dataHash.EntityId = relDataHashToKey.FromId AND 
		dataHash.FieldId = @dataHashId AND 
		dataHash.TenantId = @tenant    
    JOIN @dataHashesTable ht ON
        ht.DataHash = dataHash.Data_StartsWith
WHERE    
	relDataHashToKey.ToId = @resourceKeyId AND 
	relDataHashToKey.TypeId = @resourceKeyDataHashAppliesToResourceKeyId AND 
	relDataHashToKey.TenantId = @tenant AND
	relResourceToDataHash.FromId != @resourceId";

                const string commandTextSingle =
@"SELECT
    relResourceToDataHash.FromId AS Id, relDataHashToKey.FromId AS DataHashId  
FROM [dbo].[Relationship] relDataHashToKey	
    JOIN [dbo].[Relationship] relResourceToDataHash ON 
		relResourceToDataHash.ToId = relDataHashToKey.FromId AND 
		relResourceToDataHash.TypeId = @resourceHasResourceKeyDataHashesId AND 
		relResourceToDataHash.TenantId = @tenant
    JOIN [dbo].[Data_NVarChar] dataHash ON 
		dataHash.EntityId = relDataHashToKey.FromId AND 
		dataHash.FieldId = @dataHashId AND 
		dataHash.TenantId = @tenant        
WHERE    
    dataHash.Data_StartsWith = @dataHash AND
	relDataHashToKey.ToId = @resourceKeyId AND 
	relDataHashToKey.TypeId = @resourceKeyDataHashAppliesToResourceKeyId AND 
	relDataHashToKey.TenantId = @tenant AND
	relResourceToDataHash.FromId != @resourceId";

                IDbCommand command = dbContext.CreateCommand(countHashes == 1 ? commandTextSingle : commandTextMany);
                command.AddParameter("@tenant", DbType.Int64, RequestContext.TenantId);
                command.AddParameter("@resourceKeyId", DbType.Int64, resourceKey.Id);
                if (countHashes == 1)
                {
                    command.AddParameter("@dataHash", DbType.String, dataHashes.First(), 50);
                }
                else
                {
                    command.AddParameter("@dataHashes", DbType.String, string.Join(",", dataHashes));
                }
                
                command.AddParameter("@resourceId", DbType.Int64, resource.Id);

                command.AddParameter("@resourceKeyDataHashAppliesToResourceKeyId", DbType.Int64, Entity.GetId("core:resourceKeyDataHashAppliesToResourceKey"));
                command.AddParameter("@resourceHasResourceKeyDataHashesId", DbType.Int64, Entity.GetId("core:resourceHasResourceKeyDataHashes"));
                command.AddParameter("@dataHashId", DbType.Int64, Entity.GetId("core:dataHash"));
                
                long duplicateResourceId = -1;

                using (IDataReader reader = command.ExecuteReader())
                {                    
                    while (reader.Read())
                    {                        
                        long dataHashResourceId = reader.GetInt64(1);

                        // See if the found datahash duplicate is marked for deletion
                        if (dataHashesToDelete == null || !dataHashesToDelete.ContainsKey(dataHashResourceId))
                        {
                            // not marked for deletion so it is a duplicate.
                            duplicateResourceId = reader.GetInt64(0);
                            break;
                        }                                                
                    }
                }
                
                if (duplicateResourceId != -1)
                {
                    duplicateResource = Entity.Get<Resource>(duplicateResourceId);
                }                                
            }           

            return duplicateResource;
        }

        /// <summary>
        ///     Gets the resource key details for message.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string GetResourceKeyDetailsForMessage(ResourceKey key)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Resource Key: '{0}'", key.Name);

            if (key.KeyFields != null &&
                 key.KeyFields.Count > 0)
            {
                builder.Append(", Key Fields: ");

                bool firstField = true;
                foreach (Field field in key.KeyFields)
                {
                    if (!firstField)
                    {
                        builder.Append(", ");
                    }
                    firstField = false;
                    builder.AppendFormat("'{0}'", field.Name);
                }
            }

            if (key.ResourceKeyRelationships != null &&
                 key.ResourceKeyRelationships.Count > 0)
            {
                builder.Append(", Relationship Key Fields: ");

                bool firstRelationship = true;

                foreach (var resourceKeyRelationship in key.ResourceKeyRelationships)
                {
                    DirectionEnum_Enumeration directionEnum = resourceKeyRelationship.KeyRelationshipDirection_Enum ?? DirectionEnum_Enumeration.Forward;
                    Direction direction = directionEnum == DirectionEnum_Enumeration.Forward ? Direction.Forward : Direction.Reverse;

                    if (!firstRelationship)
                    {
                        builder.Append(", ");
                    }
                    firstRelationship = false;
                    builder.AppendFormat("'{0}'", GetDisplayNameFromRelationship(resourceKeyRelationship.KeyRelationship, direction));
                }
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Gets the resources needing an updated data hash.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="modifiedRelationshipsState">State of the modified relationships.</param>
        /// <param name="resourcesNeedingDataHashUpdate">The resources needing data hash update.</param>
        /// <param name="state">The state.</param>
        private static void GetResourcesNeedingDataHashUpdate(IEntity entity, ModifiedRelationshipsState modifiedRelationshipsState, Dictionary<long, ResourceToResourceKeys> resourcesNeedingDataHashUpdate, IDictionary<string, object> state)
        {
            // Get the temporary id or the actual id from the state.
            // This to handle the case where a new entity is created.
            // In this scenario the modified relationships that were saved in the state 
            // are keyed off the temporary id.
            long entityId = EventTargetStateHelper.GetTemporaryIdFromId(state, entity.Id);

            // Get the modified forward relationships from the state
            Dictionary<long, ChangeTracker<IMutableIdKey>> forwardRelationshipValues;
            if (modifiedRelationshipsState.ForwardRelationshipChanges.TryGetValue(entityId, out forwardRelationshipValues))
            {
                // Update forward relationships
                GetResourcesNeedingDataHashUpdate(entity, forwardRelationshipValues, Direction.Forward, resourcesNeedingDataHashUpdate);
            }

            // Get the modified reverse relationships from the state
            Dictionary<long, ChangeTracker<IMutableIdKey>> reverseRelationshipValues;
            if (modifiedRelationshipsState.ReverseRelationshipChanges.TryGetValue(entityId, out reverseRelationshipValues))
            {
                // Update reverse relationships
                GetResourcesNeedingDataHashUpdate(entity, reverseRelationshipValues, Direction.Reverse, resourcesNeedingDataHashUpdate);
            }
        }


        /// <summary>
        /// Return true if the key applies to the specified resource, false otherwise.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private static bool DoesKeyApplyToResource(ResourceKey key, Resource resource)
        {
            if (key == null || resource == null)
            {
                return false;
            }

            var keyAppliesToType = key.KeyAppliesToType;

            if (keyAppliesToType == null)
            {
                return false;
            }

            var typeApplicableToKey = new EntityRef(keyAppliesToType);

            // Check if the resource type derives from the type that the key applies to
            return resource.IsOfType.Any(et => et.IsDerivedFrom(typeApplicableToKey));
        }

        /// <summary>
        ///     Gets the resources needing an updated data hash.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="relationshipChanges">The relationship changes.</param>
        /// <param name="changesDirection">The direction.</param>
        /// <param name="resourcesNeedingDataHashUpdate">The resources needing data hash update.</param>
        private static void GetResourcesNeedingDataHashUpdate(IEntity entity, Dictionary<long, ChangeTracker<IMutableIdKey>> relationshipChanges, Direction changesDirection, Dictionary<long, ResourceToResourceKeys> resourcesNeedingDataHashUpdate)
        {
            // Ensure the current entity has relationship changes
			if ( relationshipChanges == null || relationshipChanges.Count <= 0 )
            {
                return;
            }

            foreach (var kvp in relationshipChanges)
            {
                // Get the changed relationship
                long relationshipId = kvp.Key;
                var relationship = Entity.Get<Relationship>(relationshipId);

                if (relationship == null)
                {
                    EventLog.Application.WriteError(
                        string.Format("Unexpected null Relationship for relationship id {0} on entity {1}", relationshipId,
                                      entity.Id));
                    continue;
                }

                // Check to see if the changed relationship belongs to a resource key
                IEntityCollection<ResourceKeyRelationship> relationshipInKeys = relationship.RelationshipInKey;
				if ( relationshipInKeys.Count <= 0 )
                {
                    continue;
                }

                CardinalityEnum_Enumeration cardinality = relationship.Cardinality_Enum ?? CardinalityEnum_Enumeration.ManyToMany;

                // The relationship is used by one or more keys
                foreach (var relationshipInKey in relationshipInKeys)
                {
                    // Get the direction of the relationship that is used by the key
                    DirectionEnum_Enumeration keyDirectionEnum = relationshipInKey.KeyRelationshipDirection_Enum ?? DirectionEnum_Enumeration.Forward;
                    Direction keyDirection = keyDirectionEnum == DirectionEnum_Enumeration.Forward ? Direction.Forward : Direction.Reverse;

                    // Ensure that we are looking at the other end of the relationship
                    if (keyDirection == changesDirection)
                    {
                        continue;
                    }

                    ResourceKey resourceKey = relationshipInKey.RelationshipInResourceKey;

                    // Only get keys where the key direction is the reverse of the relationship
                    // Add the resource at the other end to the list of resources whose hash needs to be updated                                
                    IChangeTracker<IMutableIdKey> changeTracker = kvp.Value;

                    #region Check For Added Relationships

                    // Check for any added relationships
                    foreach (var added in changeTracker.Added)
                    {
                        ResourceToResourceKeys resourceToResourceKeys;

                        bool haveResourceToResourceKeys = resourcesNeedingDataHashUpdate.TryGetValue(added.Key, out resourceToResourceKeys);

                        Resource addedResource = haveResourceToResourceKeys ? resourceToResourceKeys.Resource : Entity.Get<Resource>(added.Key);

                        // Ensure that the key applies to the resource type                        					                            
                        if (!DoesKeyApplyToResource(resourceKey, addedResource))
                        {
                            continue;
                        }

                        if (!haveResourceToResourceKeys)
                        {
                            resourceToResourceKeys = new ResourceToResourceKeys(addedResource.AsWritable<Resource>());
                            resourcesNeedingDataHashUpdate[added.Key] = resourceToResourceKeys;
                        }

                        if (!resourceToResourceKeys.ResourceKeys.ContainsKey(resourceKey.Id))
                        {
                            resourceToResourceKeys.ResourceKeys[resourceKey.Id] = resourceKey;
                        }

                        // Get the resource at the other end & apply the relationship changes       
                        // This is so that when the hash is generated it is correct.
                        Resource entityAtOtherEnd = resourceToResourceKeys.Resource;
                        IEntityRelationshipCollection<IEntity> relationships = entityAtOtherEnd.GetRelationships(relationshipId, keyDirection);

                        if (cardinality == CardinalityEnum_Enumeration.OneToOne)
                        {
                            relationships.Clear();
                        }

                        // Add the current entity to the specified relationship of the resource at the other end                        
                        relationships.Add(entity);
                    }

                    #endregion Check For Added Relationships

                    #region Check For Removed Relationships

                    // Add the resource at the other end to the list of resources whose hash needs to be updated                                
                    foreach (var removed in changeTracker.Removed)
                    {
                        ResourceToResourceKeys resourceToResourceKeys;

                        bool haveResourceToResourceKeys = resourcesNeedingDataHashUpdate.TryGetValue(removed.Key, out resourceToResourceKeys);

                        Resource removedResource = haveResourceToResourceKeys ? resourceToResourceKeys.Resource : Entity.Get<Resource>(removed.Key);

                        // Ensure that the key applies to the resource type                        					                            
                        if (!DoesKeyApplyToResource(resourceKey, removedResource))
                        {
                            continue;
                        }

                        if (!haveResourceToResourceKeys)
                        {
                            resourceToResourceKeys = new ResourceToResourceKeys(removedResource.AsWritable<Resource>());
                            resourcesNeedingDataHashUpdate[removed.Key] = resourceToResourceKeys;
                        }

                        if (!resourceToResourceKeys.ResourceKeys.ContainsKey(resourceKey.Id))
                        {
                            resourceToResourceKeys.ResourceKeys[resourceKey.Id] = resourceKey;
                        }

                        // Get the resource at the other end & apply the relationship changes   
                        // This is so that when the hash is generated it is correct.
                        Resource entityAtOtherEnd = resourceToResourceKeys.Resource;
                        IEntityRelationshipCollection<IEntity> relationships = entityAtOtherEnd.GetRelationships(relationshipId, keyDirection);

                        // Remove the current entity from the specified relationship of the resource at the other end
                        relationships.Remove(entity);
                    }

                    #endregion Check For Removed Relationships
                }
            }
        }


        /// <summary>
        ///     Determines whether the key applies to type has changed.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns>
        ///     <c>true</c> if [has key applies to type changed] [the specified resource key]; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasKeyAppliesToTypeChanged(ResourceKey resourceKey)
        {
            bool keyAppliesToTypeChanged = false;
            IDictionary<long, IChangeTracker<IMutableIdKey>> relationshipValues;

            EntityRelationshipModificationCache.Instance.TryGetValue(new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey((resourceKey as IEntityInternal).ModificationToken, Direction.Forward), out relationshipValues);

            if (relationshipValues != null)
            {
                keyAppliesToTypeChanged = relationshipValues.ContainsKey(ResourceKey.KeyAppliesToType_Field.Id);
            }

            return keyAppliesToTypeChanged;
        }


        /// <summary>
        ///     Determines whether the specified entity has modifications.
        /// </summary>
        /// <param name="fieldValues">The field values.</param>
        /// <param name="forwardRelationshipValues">Forward relationship values.</param>
        /// <param name="reverseRelationshipValues">Reverse relationship values.</param>
        /// <returns>
        ///     <c>true</c> if the specified entity has modifications; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasModifications(IEntityFieldValues fieldValues, IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationshipValues, IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationshipValues)
        {
            return (fieldValues != null && fieldValues.Any() ||
						 forwardRelationshipValues != null && forwardRelationshipValues.Count > 0 ||
						 reverseRelationshipValues != null && reverseRelationshipValues.Count > 0 );
        }

        /// <summary>
        ///     Haves the key field values changed.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="modifiedFieldValues">The modified field values.</param>
        /// <returns></returns>
        private static bool HaveKeyFieldValuesChanged( ResourceKey resourceKey, IEntityFieldValues modifiedFieldValues )
        {
            return resourceKey.KeyFields.Any(field => modifiedFieldValues == null || (modifiedFieldValues.ContainsField(field.Id)));
        }

        /// <summary>
        ///     Haves the key fields or relationships changed.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns></returns>
        private static bool HaveKeyFieldsOrRelationshipsChanged(ResourceKey resourceKey)
        {
            bool haveKeyFieldsChanged = false;
            IDictionary<long, IChangeTracker<IMutableIdKey>> relationshipValues;

            EntityRelationshipModificationCache.Instance.TryGetValue(new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey((resourceKey as IEntityInternal).ModificationToken, Direction.Reverse), out relationshipValues);

            if (relationshipValues != null)
            {
                haveKeyFieldsChanged = relationshipValues.ContainsKey(ResourceKey.KeyFields_Field.Id) ||
                                       relationshipValues.ContainsKey(ResourceKey.ResourceKeyRelationships_Field.Id);
            }

            return haveKeyFieldsChanged;
        }


        /// <summary>
        ///     Haves the key relationship values changed.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="forwardRelationshipValues">The forward relationship values.</param>
        /// <param name="reverseRelationshipValues">The reverse relationship values.</param>
        /// <returns></returns>
        private static bool HaveKeyRelationshipValuesChanged(ResourceKey resourceKey,
                                                              IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationshipValues,
                                                              IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationshipValues)
        {
            bool haveKeyRelationshipValuesChanged = false;

            foreach (var resourceKeyRelationship in resourceKey.ResourceKeyRelationships)
            {
                DirectionEnum_Enumeration directionEnum = resourceKeyRelationship.KeyRelationshipDirection_Enum ?? DirectionEnum_Enumeration.Forward;
                Direction direction = directionEnum == DirectionEnum_Enumeration.Forward ? Direction.Forward : Direction.Reverse;

                switch (direction)
                {
                    case Direction.Forward:
                        if (forwardRelationshipValues == null ||
                             forwardRelationshipValues.ContainsKey(resourceKeyRelationship.KeyRelationship.Id))
                        {
                            haveKeyRelationshipValuesChanged = true;
                        }
                        break;

                    case Direction.Reverse:
                        if (reverseRelationshipValues == null ||
                             reverseRelationshipValues.ContainsKey(resourceKeyRelationship.KeyRelationship.Id))
                        {
                            haveKeyRelationshipValuesChanged = true;
                        }
                        break;
                }

                if (haveKeyRelationshipValuesChanged)
                {
                    break;
                }
            }

            return haveKeyRelationshipValuesChanged;
        }

        /// <summary>
        ///     Determines whether [is relationship singular] [the specified relationship].
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>
        ///     <c>true</c> if [is relationship singular] [the specified relationship]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsRelationshipSingular(Relationship relationship, Direction direction)
        {
            bool isRelationshipSingular = false;
            CardinalityEnum_Enumeration cardinality = relationship.Cardinality_Enum ?? CardinalityEnum_Enumeration.ManyToMany;

            switch (direction)
            {
                case Direction.Forward:
                    if (cardinality == CardinalityEnum_Enumeration.OneToOne || cardinality == CardinalityEnum_Enumeration.ManyToOne)
                    {
                        isRelationshipSingular = true;
                    }
                    break;

                case Direction.Reverse:
                    if (cardinality == CardinalityEnum_Enumeration.OneToOne || cardinality == CardinalityEnum_Enumeration.OneToMany)
                    {
                        isRelationshipSingular = true;
                    }
                    break;
            }

            return isRelationshipSingular;
        }

        /// <summary>
        /// Returns true if merging a relationship in the specified direction could cause a cardinality violation.
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="direction">The direction</param>
        /// <returns></returns>
        private static bool MayCauseCardinalityViolation(Relationship relationship, Direction direction)
        {
            bool mayCauseCardinalityViolation = false;
            CardinalityEnum_Enumeration cardinality = relationship.Cardinality_Enum ?? CardinalityEnum_Enumeration.OneToOne;

            switch (direction)
            {
                case Direction.Forward:
                    if (cardinality == CardinalityEnum_Enumeration.OneToOne || cardinality == CardinalityEnum_Enumeration.OneToMany)
                    {
                        mayCauseCardinalityViolation = true;
                    }
                    break;

                case Direction.Reverse:
                    if (cardinality == CardinalityEnum_Enumeration.OneToOne || cardinality == CardinalityEnum_Enumeration.ManyToOne)
                    {
                        mayCauseCardinalityViolation = true;
                    }
                    break;
            }

            return mayCauseCardinalityViolation;
        }

        /// <summary>
        ///     Merges the entities.
        ///     The source entity is merged to the target and then deleted
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="targetEntity">The target entity.</param>
        private static void MergeEntities(IEntity sourceEntity, IEntity targetEntity)
        {
            IEnumerable<Field> sourceFields = EntityTypeHelper.GetAllFields(sourceEntity);
            IEnumerable<Field> targetFields = EntityTypeHelper.GetAllFields(targetEntity);
            IEnumerable<Relationship> sourceRelationships = EntityTypeHelper.GetAllRelationships(sourceEntity, Direction.Forward);
            IEnumerable<Relationship> sourceReverseRelationships = EntityTypeHelper.GetAllRelationships(sourceEntity, Direction.Reverse);
            IEnumerable<Relationship> targetRelationships = EntityTypeHelper.GetAllRelationships(targetEntity, Direction.Forward);
            IEnumerable<Relationship> targetReverseRelationships = EntityTypeHelper.GetAllRelationships(targetEntity, Direction.Reverse);

            // Merge field values
            foreach (Field sourceField in sourceFields)
            {
                IList<Field> enumerable = targetFields as IList<Field> ?? targetFields.ToList();

                if (enumerable.Any(tf => tf.Id == sourceField.Id))
                {
                    object targetFieldValue = targetEntity.GetField(sourceField);
                    object sourceFieldValue = sourceEntity.GetField(sourceField);

                    bool targetHasValue;
                    bool sourceHasValue;

                    if (sourceFieldValue is string)
                    {
                        targetHasValue = !string.IsNullOrEmpty(targetFieldValue as string);
                        sourceHasValue = !string.IsNullOrEmpty(sourceFieldValue as string);
                    }
                    else
                    {
                        targetHasValue = targetFieldValue != null;
                        sourceHasValue = sourceFieldValue != null;
                    }

                    if (!targetHasValue &&
                         sourceHasValue)
                    {
                        targetEntity.SetField(sourceField, sourceFieldValue);
                    }
                }
            }

            // Merge relationships
            MergeRelationships(sourceEntity, targetEntity, sourceRelationships, targetRelationships, Direction.Forward);
            MergeRelationships(sourceEntity, targetEntity, sourceReverseRelationships, targetReverseRelationships, Direction.Reverse);            
        }


        /// <summary>
        ///     Merges the relationships.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="targetEntity">The target entity.</param>
        /// <param name="sourceRelationships">The source relationships.</param>
        /// <param name="targetRelationships">The target relationships.</param>
        /// <param name="direction">The direction.</param>
        private static void MergeRelationships(IEntity sourceEntity, IEntity targetEntity, IEnumerable<Relationship> sourceRelationships, IEnumerable<Relationship> targetRelationships, Direction direction)
        {
            foreach (Relationship sourceRelationship in sourceRelationships)
            {
                if (sourceRelationship.Alias == "core:isOfType" ||
                     sourceRelationship.Alias == "core:resourceHasResourceKeyDataHashes")
                {
                    continue;
                }                

                IList<Relationship> relationships = targetRelationships as IList<Relationship> ?? targetRelationships.ToList();

                if (relationships.Any(tr => tr.Id == sourceRelationship.Id))
                {
                    IEntityRelationshipCollection<IEntity> targetRelationshipData = targetEntity.GetRelationships(sourceRelationship, direction);
                    IEntityRelationshipCollection<IEntity> sourceRelationshipData = sourceEntity.GetRelationships(sourceRelationship, direction);

                    // Find all the entries that are in the source collection but not the target                    
                    if (IsRelationshipSingular(sourceRelationship, direction))
                    {
                        if ((targetRelationshipData == null ||
                               targetRelationshipData.Count == 0) &&
                             (sourceRelationshipData != null &&
                               sourceRelationshipData.Count == 1))
                        {
                            if (targetRelationshipData != null)
                            {
                                targetRelationshipData.Add(sourceRelationshipData.First());
                                if (MayCauseCardinalityViolation(sourceRelationship, direction))
                                {
                                    sourceRelationshipData.Clear();
                                }                                
                            }
                        }
                    }
                    else
                    {
                        IEnumerable<IEntity> toAdd = sourceRelationshipData.Where(sr => !targetRelationshipData.Any(tr =>
                        {
                            bool entityMatch = (tr.Entity != null &&
                                                 sr.Entity != null &&
                                                 tr.Entity.Id == sr.Entity.Id);
                            return entityMatch;
                        }));
                        targetRelationshipData.AddRange(toAdd);
                        if (MayCauseCardinalityViolation(sourceRelationship, direction))
                        {
                            sourceRelationshipData.Clear();
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Saves the resource key data hashes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="state">The state.</param>
        private static void SaveResourceKeyDataHashesForResource(IEntity entity, IDictionary<string, object> state)
        {
            var entityInternal = entity as IEntityInternal;
            if (entityInternal != null &&
                entityInternal.IsTemporaryId &&
                entityInternal.CloneOption == CloneOption.Deep &&
                entityInternal.CloneSource != null)
            {
                // clear the data hashes. The entity being saved is a clone.
                // We need to ensure that it's data hashes are local and not the clone source's.
                entity.SetRelationships(new EntityRef("core", "resourceHasResourceKeyDataHashes"), null, Direction.Forward);
            }

            IEntityModificationToken token = entityInternal.ModificationToken;
            IEntityFieldValues fieldValues = null;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationshipValues = null;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationshipValues = null;

            if (entityInternal != null)
            {
                EntityFieldModificationCache.Instance.TryGetValue(new EntityFieldModificationCache.EntityFieldModificationCacheKey(token), out fieldValues);
                EntityRelationshipModificationCache.Instance.TryGetValue(new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey(token, Direction.Forward), out forwardRelationshipValues);
                EntityRelationshipModificationCache.Instance.TryGetValue(new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey(token, Direction.Reverse), out reverseRelationshipValues);
            }
                
            if (!HasModifications(fieldValues, forwardRelationshipValues, reverseRelationshipValues))
            {
                return;
            }

            var resource = entity.As<Resource>();
            if (resource == null)
            {
                return;
            }

            // Save the modified relationships for this entity.
            // This method is called during a BeforeSave event so we save the changed relationships in the state.
            // This state is used by the SaveResourceKeyDataHashesForReverseRelationships method which is called
            // in the AfterSave event.
            // This done because after the after save event the changed relationships are not available as they have been
            // flushed to the database.
            var modifiedRelationshipsState = EventTargetStateHelper.GetValue<ModifiedRelationshipsState>(state, ModifiedRelationshipsStateKey);
            if (modifiedRelationshipsState == null)
            {
                modifiedRelationshipsState = new ModifiedRelationshipsState();
                state[ModifiedRelationshipsStateKey] = modifiedRelationshipsState;
            }
            SetModifiedRelationshipsState(resource, modifiedRelationshipsState, forwardRelationshipValues, reverseRelationshipValues);

            // Get all the types that this type implements
            IEnumerable<EntityType> entityTypes = resource.GetAllTypes();            

            // Save the key data hashes for each resource and entity type
            foreach (EntityType entityType in entityTypes)
            {
                SaveResourceKeyDataHashesForResource(resource, entityType, state, fieldValues, forwardRelationshipValues, reverseRelationshipValues);
            }
        }

        /// <summary>
        /// Saves the resource key data hashes.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="state">The state.</param>
        /// <param name="fieldValues">The field values.</param>
        /// <param name="forwardRelationshipValues">Forward relationship values.</param>
        /// <param name="reverseRelationshipValues">Reverse relationship values.</param>
        private static void SaveResourceKeyDataHashesForResource(Resource resource, EntityType entityType, IDictionary<string, object> state, IEntityFieldValues fieldValues, IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationshipValues, IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationshipValues)
        {            
            // Get all the resource keys that apply to the entity type
            foreach (ResourceKey resourceKey in entityType.ResourceKeys)
            {
                // No key fields or relationships have changed so skip processing this key.
                if (!HaveKeyFieldValuesChanged(resourceKey, fieldValues) &&
                     !HaveKeyRelationshipValuesChanged(resourceKey, forwardRelationshipValues, reverseRelationshipValues))
                {
                    continue;
                }

                // Calculate the data hashes
                ISet<string> dataHashes = resourceKey.GetResourceKeyDataHashes(resource);

                // Perform a check for duplicates
                Resource duplicateResource = GetDuplicateResource(resource, resourceKey, dataHashes, state);
                if (duplicateResource != null)
                {
                    bool mergeDuplicates = resourceKey.MergeDuplicates ?? false;
                    if (mergeDuplicates || _overwriteMatchingResources)
                    {
                        if (!CanMerge(duplicateResource, resource))
                        {
                            string resourceKeyDetails = GetResourceKeyDetailsForMessage(resourceKey);
                            resourceKey.Undo();
                            resource.Undo();

                            EventLog.Application.WriteError("A resource with duplicate key fields already exists and a merge will result in a loss of data. {0}.", resourceKeyDetails);
                            throw new DuplicateKeyException(string.Format("Cannot save, a merge will result in loss of data. {0}", resourceKey.ResourceKeyMessage ?? "A resource with duplicate key fields already exists."));
                        }

                        duplicateResource = duplicateResource.AsWritable<Resource>();
                        // Merge the existing resource's fields and relationships
                        // with the new one                        
                        MergeEntities(duplicateResource, resource);
                        AddMergedDuplicateToState(state, duplicateResource.Id);                        

                        SaveGraph saveGraph = EventTargetStateHelper.GetSaveGraph(state);
                        saveGraph.Entities[duplicateResource.Id] = duplicateResource;
                    }
                    else
                    {
                        string resourceKeyDetails = GetResourceKeyDetailsForMessage(resourceKey);
                        resourceKey.Undo();
                        resource.Undo();
                        string logMessage = string.Format("{0} ({1} when saving '{2}' ({3}))",
                            resourceKey.ResourceKeyMessage ?? "A resource with duplicate key fields already exists.",
                            resourceKeyDetails,
                            resource.Alias ?? resource.Name ?? "(no name or alias)",
                            resource.Id
                        );

                        EventLog.Application.WriteError(logMessage);
						throw new DuplicateKeyException(resourceKey.ResourceKeyMessage ?? "A resource with duplicate key fields already exists.");
                    }
                }

                // Create or update the resource key data hashes
                CreateUpdateResourceKeyDataHashes(resource, resourceKey, dataHashes, state);
            }
        }

        /// <summary>
        /// Saves the resource key data hashes for resource key.
        /// </summary>
        /// <param name="resourceKeyEntity">The resource key entity.</param>
        /// <param name="state">The state.</param>
        /// <exception cref="DuplicateKeyException"></exception>
        private static void SaveResourceKeyDataHashesForResourceKey(IEntity resourceKeyEntity, IDictionary<string, object> state)
        {
            var resourceKey = resourceKeyEntity.As<ResourceKey>();
            if (resourceKey == null)
            {
                return;
            }

            EventLog.Application.WriteWarning("Recalculating all resource key hashes for: " + resourceKey.Name);

            using (Profiler.Measure("SaveResourceKeyDataHashesForResourceKey"))
            {
                if (!_isKeyInstall &&
                    !DeleteResourceKeyDataHashes(resourceKey))
                {
                    return;
                }

                // Get the entity type that this key applies to.
                EntityType entityType = resourceKey.KeyAppliesToType;

                if (entityType == null)
                    return;

                // Get all instances of this entity type
                IEnumerable<IEntity> entities = Entity.GetInstancesOfType(new EntityRef(entityType.Id), true);                

                // Regenerate the hash for all resources that applies to this key to see if the key change will cause a key violation.
                foreach (IEntity entity in entities)
                {
                    if (entity == null)
                        continue;

                    // Generate the hash for this resource
                    ISet<string> dataHashes = resourceKey.GetResourceKeyDataHashes(entity);

                    Resource duplicateResource = GetDuplicateResource(entity, resourceKey, dataHashes, state);

                    if (duplicateResource != null)
                    {
                        string keyName = resourceKey.Name;
                        entityType.Undo();
                        resourceKey.Undo();

                        // Throw an error. Key change will result in a duplicate.
                        string message = resourceKey.ResourceKeyMessage ?? string.Format("Failed to save resource key '{0}' as this will result in existing resources having duplicate keys.", keyName);
                        throw new DuplicateKeyException(message);
                    }

                    var resource = entity.As<Resource>();

                    // Create or update the resource key data hashes
                    CreateUpdateResourceKeyDataHashes(resource, resourceKey, dataHashes, state);
                }
            }
        }


        /// <summary>
        ///     Sets the state of the modified relationships.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="state">The state.</param>        
        /// <param name="forwardRelationshipValues">Forward relationship values.</param>
        /// <param name="reverseRelationshipValues">Reverse relationship values.</param>
        private static void SetModifiedRelationshipsState(IEntity entity, ModifiedRelationshipsState state, IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationshipValues, IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationshipValues)
        {            
            if (state == null)
            {
                return;
            }
            
            if (forwardRelationshipValues != null)
            {
                // Store a copy of the changed forward relationships
                // This is due to the fact that the relationship changes are cleared on save
                foreach (var kvp in forwardRelationshipValues)
                {
                    long relationshipId = kvp.Key;
                    IChangeTracker<IMutableIdKey> changes = kvp.Value;

                    Dictionary<long, ChangeTracker<IMutableIdKey>> forwardChangesForEntity;
                    if (!state.ForwardRelationshipChanges.TryGetValue(entity.Id, out forwardChangesForEntity))
                    {
                        forwardChangesForEntity = new Dictionary<long, ChangeTracker<IMutableIdKey>>();
                        state.ForwardRelationshipChanges[entity.Id] = forwardChangesForEntity;
                    }

                    ChangeTracker<IMutableIdKey> forwardChangeTracker;
                    if (!forwardChangesForEntity.TryGetValue(relationshipId, out forwardChangeTracker))
                    {
                        forwardChangeTracker = new ChangeTracker<IMutableIdKey>();
                        forwardChangesForEntity[relationshipId] = forwardChangeTracker;
                    }

                    forwardChangeTracker.AddRange(changes.Added);
                    forwardChangeTracker.RemoveRange(changes.Removed);
                }
            }

            if (reverseRelationshipValues != null)
            {
                // Store a copy of the changed reverse relationships
                // This is due to the fact that the relationship changes are cleared on save
                foreach (var kvp in reverseRelationshipValues)
                {
                    long relationshipId = kvp.Key;
                    IChangeTracker<IMutableIdKey> changes = kvp.Value;

                    Dictionary<long, ChangeTracker<IMutableIdKey>> reverseChangesForEntity;
                    if (!state.ReverseRelationshipChanges.TryGetValue(entity.Id, out reverseChangesForEntity))
                    {
                        reverseChangesForEntity = new Dictionary<long, ChangeTracker<IMutableIdKey>>();
                        state.ReverseRelationshipChanges[entity.Id] = reverseChangesForEntity;
                    }

                    ChangeTracker<IMutableIdKey> reverseChangeTracker;
                    if (!reverseChangesForEntity.TryGetValue(relationshipId, out reverseChangeTracker))
                    {
                        reverseChangeTracker = new ChangeTracker<IMutableIdKey>();
                        reverseChangesForEntity[relationshipId] = reverseChangeTracker;
                    }

                    reverseChangeTracker.AddRange(changes.Added);
                    reverseChangeTracker.RemoveRange(changes.Removed);
                }
            }            
        }
       
        /// <summary>
        /// Updates the key data hashes for resources.
        /// </summary>
        /// <param name="resourcesNeedingDataHashUpdate">The resources needing data hash update.</param>
        /// <param name="state">The state.</param>
        /// <exception cref="DuplicateKeyException"></exception>
        private static void UpdateKeyDataHashesForResources(Dictionary<long, ResourceToResourceKeys> resourcesNeedingDataHashUpdate, IDictionary<string, object> state)
        {
            // Update the data hashes and check for duplicates
			if ( resourcesNeedingDataHashUpdate.Count <= 0 )
            {
                return;
            }            

            // Enumerate the resources which need their hash updated
            foreach (ResourceToResourceKeys resourceToResourceKeys in resourcesNeedingDataHashUpdate.Values)
            {
                // Get the resource to update
                Resource resource = resourceToResourceKeys.Resource;

                // A dictionary of hashes per resource key
                var resourceKeyToDataHashDictionary = new Dictionary<long, ISet<string>>();

                // Enumerate the keys and generate the hashes per key
                foreach (ResourceKey resourceKey in resourceToResourceKeys.ResourceKeys.Values)
                {
                    // Generate the hashes and store the hashes for this key
                    resourceKeyToDataHashDictionary[resourceKey.Id] = resourceKey.GetResourceKeyDataHashes(resource);
                }
                
                // Undo any changes made to the resource.
                // This is necessary to remove any relationships that were set in order for the data hash
                // to be correct
                resource.Undo();

                // Check for duplicates and update hashes
                foreach (KeyValuePair<long, ISet<string>> kvp in resourceKeyToDataHashDictionary)
                {                    
                    ISet<string> resourceKeyDataHashes = kvp.Value;
                    ResourceKey resourceKey = resourceToResourceKeys.ResourceKeys[kvp.Key];                    

                    // Perform a check for duplicates
                    Resource duplicateResource = GetDuplicateResource(resource, resourceKey, resourceKeyDataHashes, state);
                    if (duplicateResource != null)
                    {
                        string resourceKeyDetails = GetResourceKeyDetailsForMessage(resourceKey);

                        EventLog.Application.WriteError(resourceKey.ResourceKeyMessage ?? string.Format("A resource with duplicate key fields already exists. {0}.", resourceKeyDetails));
                        throw new DuplicateKeyException(resourceKey.ResourceKeyMessage ?? "A resource with duplicate key fields already exists.");
                    }

                    // Create or update the resource key data hashes
                    CreateUpdateResourceKeyDataHashes(resource, resourceKey, resourceKeyDataHashes, state);
                }
            }

            Dictionary<long, ResourceKeyDataHash> unsavedDataHashesByIdState = GetUnsavedDataHashesByIdState(state);
			if ( unsavedDataHashesByIdState.Count > 0 )
            {
                Entity.Save(unsavedDataHashesByIdState.Values);   
            }            
        }        


        /// <summary>
        ///     Escapes the field value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string EscapeFieldValue(object value)
        {
            string convertedValue = Convert.ToString(value, CultureInfo.InvariantCulture).ToUpperInvariant();

            convertedValue = convertedValue.Replace("[", "[[");
            convertedValue = convertedValue.Replace("]", "]]");

            return string.Format("[{0}]", convertedValue);
        }

        /// <summary>
        ///     Generates the key fields hash input.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="dataBuilder">The data builder.</param>
        /// <returns></returns>
        private static bool GenerateKeyFieldsHashInput(this ResourceKey resourceKey, IEntity entity, StringBuilder dataBuilder)
        {
            bool haveValues = false;

            if (resourceKey.KeyFields == null)
            {
                return false;
            }

            foreach (Field field in resourceKey.KeyFields.OrderBy(f => f.Id))
            {
                string fieldValueAsString = NullValue;

                object fieldValue = entity.GetField(field);
                if (fieldValue != null)
                {
                    haveValues = true;
                    fieldValueAsString = EscapeFieldValue(fieldValue);
                }

                dataBuilder.Append(fieldValueAsString);
            }

            return haveValues;
        }

        /// <summary>
        ///     Generates a key based on the field values of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="fields">The ordered fields to feed into the key.</param>
        /// <returns>A string of the concatenated field values.</returns>
        private static string FlattenForFieldsKey(IEntity entity, List<Field> fields)
        {
            return string.Concat(fields.Select(f =>
            {
                object fieldValue = entity.GetField(f);

                if (fieldValue != null)
                {
                    return EscapeFieldValue(fieldValue);
                }

                return NullValue;
            }));
        }

        /// <summary>
        ///     Generates the key relationships hash input.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="entity">The entity.</param>        
        /// <returns></returns>
        private static IEnumerable<StringBuilder> GenerateKeyRelationshipsHashInput(this ResourceKey resourceKey, IEntity entity)
        {
            var dataBuilders = new List<StringBuilder>();
            bool haveValues = false;

            IEntityCollection<ResourceKeyRelationship> resourceKeyRelationships = resourceKey.ResourceKeyRelationships;

            if (resourceKeyRelationships == null)
            {
                return dataBuilders;
            }

            var relatedEntitySortedDictionary = new SortedDictionary<long, IList<string>>();           

            foreach (var resourceKeyRelationship in resourceKeyRelationships.OrderBy(r => r.Id))
            {                                
                DirectionEnum_Enumeration directionEnum = resourceKeyRelationship.KeyRelationshipDirection_Enum ?? DirectionEnum_Enumeration.Forward;
                Direction direction = directionEnum == DirectionEnum_Enumeration.Forward ? Direction.Forward : Direction.Reverse;
                Relationship keyRelationship = resourceKeyRelationship.KeyRelationship;                

                var relationshipRef = new EntityRef(keyRelationship);

                IEntityRelationshipCollection<IEntity> relatedEntities = entity.GetRelationships(relationshipRef, direction);
                
				if ( relatedEntities != null && relatedEntities.Count > 0 )
                {
                    haveValues = true;

                    // Is the hash to be generated on fields as opposed to just ids of the related resource?
                    var fields = resourceKeyRelationship.FieldsInRelationshipKey.OrderBy(f => f.Id).ToList();

                    // Order by id and escape as strings
                    var escapedEntityIds = fields.Count > 0 ?
                        relatedEntities.OrderBy(re => re.Entity.Id).Select(e => FlattenForFieldsKey(e, fields)) :
                        relatedEntities.OrderBy(e => e.Entity.Id).Select(e => EscapeFieldValue(e.Entity.Id));

                    relatedEntitySortedDictionary[resourceKeyRelationship.Id] = escapedEntityIds.ToList();
                }
                else
                {
                    relatedEntitySortedDictionary[resourceKeyRelationship.Id] = new List<string> {NullValue};                    
                }
            }

            if (haveValues)
            {
                IEnumerable<string> inputValues = GenerateCrossJoin(relatedEntitySortedDictionary.Values);
                dataBuilders.AddRange(inputValues.Select(s => new StringBuilder(s)));                
            }
            
            return dataBuilders;
        }

        /// <summary>
        /// Generates a cross join of the input values.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">inputs</exception>
        private static IEnumerable<string> GenerateCrossJoin(IEnumerable<IList<string>> inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException("inputs");    
            }

            IEnumerable<string> result = null;

            var inputValues = new Queue<IList<string>>(inputs);

			while ( inputValues.Count > 0 )
            {
                IEnumerable<string> first = result ?? inputValues.Dequeue();
				if ( inputValues.Count > 0 )
                {
                    IEnumerable<string> second = inputValues.Dequeue();
                    result = first.SelectMany(s => second, (p1, p2) => p1 + p2);
                }
                else
                {
                    result = first;
                }                  
            }

            return result != null ? result.ToList() : new List<string>();
        }

        #endregion Non-Public Methods

        #region Save Context For the Resource Key Helper

        /// <summary>
        /// </summary>
        public enum SaveContext
        {
            /// <summary>
            /// </summary>
            Resource,


            /// <summary>
            /// </summary>
            ResourceKey,


            /// <summary>
            /// </summary>
            RelatedResource
        }

        #endregion Save Context For the Resource Key Helper

        #region Public Classes

        /// <summary>
        /// Flag to indicate that merge should explicitly be used.
        /// </summary>
        [ThreadStatic]
        private static bool _overwriteMatchingResources;

        /// <summary>
        /// Using-disposable block for overriding resource key behavior.
        /// </summary>
        public class OverwriteMatchingResources : IDisposable
        {
            public OverwriteMatchingResources()
            {
                _overwriteMatchingResources = true;
            }

            public void Dispose()
            {
                _overwriteMatchingResources = false;
            }
        }

        /// <summary>
        /// Flag to indicate that current context is key installation.
        /// </summary>
        [ThreadStatic]
        private static bool _isKeyInstall;

        /// <summary>
        /// Using-disposable block for overriding resource key behavior.
        /// </summary>
        public class InstallKeysContext : IDisposable
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InstallKeysContext"/> class.
            /// </summary>
            public InstallKeysContext()
            {
                _isKeyInstall = true;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _isKeyInstall = false;
            }
        }        

        #endregion

        #region Non-Public Classes

        private class ModifiedRelationshipsState
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="ModifiedRelationshipsState" /> class.
            /// </summary>
            public ModifiedRelationshipsState()
            {
                ForwardRelationshipChanges = new Dictionary<long, Dictionary<long, ChangeTracker<IMutableIdKey>>>();
                ReverseRelationshipChanges = new Dictionary<long, Dictionary<long, ChangeTracker<IMutableIdKey>>>();
            }


            /// <summary>
            ///     Gets or sets the forward relationship changes.
            /// </summary>
            /// <value>
            ///     The forward relationship changes.
            /// </value>
            public Dictionary<long, Dictionary<long, ChangeTracker<IMutableIdKey>>> ForwardRelationshipChanges
            {
                get;
                private set;
            }


            /// <summary>
            ///     Gets or sets the reverse relationship changes.
            /// </summary>
            /// <value>
            ///     The reverse relationship changes.
            /// </value>
            public Dictionary<long, Dictionary<long, ChangeTracker<IMutableIdKey>>> ReverseRelationshipChanges
            {
                get;
                private set;
            }
        }

        private class ResourceToResourceKeys
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="ResourceToResourceKeys" /> class.
            /// </summary>
            /// <param name="resource">The resource.</param>
            public ResourceToResourceKeys(Resource resource)
            {
                Resource = resource;
                ResourceKeys = new Dictionary<long, ResourceKey>();
            }


            /// <summary>
            ///     Gets or sets the resource.
            /// </summary>
            /// <value>
            ///     The resource.
            /// </value>
            public Resource Resource
            {
                get;
                private set;
            }


            /// <summary>
            ///     Gets or sets the resource keys.
            /// </summary>
            /// <value>
            ///     The resource keys.
            /// </value>
            public Dictionary<long, ResourceKey> ResourceKeys
            {
                get;
                private set;
            }
        }

        #endregion
    }
}