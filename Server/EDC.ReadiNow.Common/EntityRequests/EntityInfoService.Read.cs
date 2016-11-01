// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using StructuredQuery = EDC.ReadiNow.Metadata.Query.Structured.StructuredQuery;
using TypedValue = EDC.ReadiNow.Metadata.TypedValue;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.EntityRequests
{
    [Obsolete("Use BulkRequestRunner instead (for reads)")]
    public partial class EntityInfoService : IEntityInfoRead
    {
        /// <summary>
        /// The maximum number of related entities that can be loaded before results start getting clipped.
        /// </summary>
        private static readonly int MaxRelatedEntities = EntityWebApiSettings.Current.MaxRelatedLimit;
        private static readonly int MaxRelatedEntitiesWarning = EntityWebApiSettings.Current.MaxRelatedWarning;

        /// <summary>
        /// Loads structured data for the specified entity.
        /// </summary>
        /// <param name="entityId">The entity to load.</param>
        /// <param name="request">The description of fields and related entities to load.</param>
        /// <returns>The requested data. Or null if the entity does not exist.</returns>
        /// <exception cref="PlatformSecurityException">
        /// The user lacks read access to the given entity.
        /// </exception>
        public EntityData GetEntityData(EntityRef entityId, EntityMemberRequest request)
        {
            try
            {
                // Validate input
                if (entityId == null)
                    throw new ArgumentNullException("entityId");
                
                try
                {
                    long id = entityId.Id;  // ensure we can resolve any alias
                }
                catch (ArgumentException)
                {
                    return null;    // alias does not exist .. return null
                }

                request = ValidateRequest(request);

                // Place request
                var resultList = GetEntitiesData(entityId.ToEnumerable(), request, SecurityOption.DemandAll);

                var result = resultList.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("Failed GetEntityData. Entity: #{0}\nRequest details:\n{1}\n\nException:\n{2}",
                    entityId, request == null ? "null" : request.Debug(), ex);
                throw;
            }
        }


        /// <summary>
        /// Runs a query and returns all entities that match the query.
        /// </summary>
        /// <param name="query">The query to execute. Do not specify columns or ordering. Only specify conditions and a root entity (and any joins required to apply the condition).</param>
        /// <param name="request">The description of fields and related entities to load.</param>
        /// <returns>The requested data for all entities matched by the query.</returns>
        public IEnumerable<EntityData> QueryEntityData(StructuredQuery query, EntityMemberRequest request)
        {
            try
            {
                // Validate input
                if (query == null)
                    throw new ArgumentNullException("query");
                if (query.SelectColumns.Count > 0)
                    throw new ArgumentException("query.SelectColumns must be empty", "query");
                request = ValidateRequest(request);

                // Create context
                Context context = new Context();

                // Query for top-level entities
                IEntityRef[] members = RequestedMembers(request, context);
                IEnumerable<IEntity> entities = Entity.GetMatches(query, members);

                // Fill data
                IEnumerable<EntityData> result = PackageEntities(context, request, entities, true);

                var list = result.Where(e => e != null).ToList();
                return list;
            }
            catch (Exception ex)
            {
                string xml = "";
                try { xml = EDC.ReadiNow.Metadata.Query.Structured.StructuredQueryHelper.ToXml(query); }
                catch { }
                EventLog.Application.WriteError("Failed QueryEntityData:\nRequest:\n{0}\n\nQuery:\n{1}\n\nException:\n{2}",
                    request == null ? "null" : request.Debug(), xml, ex);
                throw;
            }
        }


        /// <summary>
        /// Loads structured data for all entities of the specified type.
        /// </summary>
        /// <param name="entityType">The type of the entities to be loaded.</param>
        /// <param name="includeDerivedTypes">If true, then instances of types that directly or indirectly derive from 'entityType' are also returned.</param>
        /// <param name="request">The description of fields and related entities to load.</param>
        /// <returns>All instances of type 'entityType', and optionally all instances of types that derive from 'entityType'.</returns>
        public IEnumerable<EntityData> GetEntitiesByType(EntityRef entityType, bool includeDerivedTypes, EntityMemberRequest request)
        {
            // Note: this is only called by the legacy entity info service.

            try
            {
                if (entityType == null)
                    throw new ArgumentNullException("entityType");
                request = ValidateRequest(request);

                // Create context
                Context context = new Context();

                // Load top-level entities
                IEntityRef[] members = RequestedMembers(request, context);
                IEnumerable<IEntity> entities = Entity.GetInstancesOfType(entityType, includeDerivedTypes);

                // Fill data
                IEnumerable<EntityData> result = PackageEntities(context, request, entities, true);

                var list = result.Where(e => e != null).ToList();
                return list;
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("Failed GetEntitiesByType:\n{0}\nRequest:\n{1}\n\nException:\n{2}",
                    entityType, request == null ? "null" : request.Debug(), ex);
                throw;
            }             
        }


        /// <summary>
        /// Loads structured data for the specified entity.
        /// </summary>
        /// <param name="entityIds">The IDs of the entities to load.</param>
        /// <param name="request">The description of fields and related entities to load.</param>
        /// <param name="securityOption">How to handle access denied situations.</param>
        /// <returns>The requested data.</returns>
        /// <exception cref="PlatformSecurityException">
        /// Thrown if the user lacks read access to the given entities and <paramref name="securityOption"/> is <see cref="SecurityOption.DemandAll"/>.
        /// </exception>
        public IEnumerable<EntityData> GetEntitiesData(IEnumerable<EntityRef> entityIds, EntityMemberRequest request, SecurityOption securityOption = SecurityOption.SkipDenied)
        {
            try
            {
                if (entityIds == null)
                    throw new ArgumentNullException("entityIds");
                if (entityIds.Any(e => e == null))
                    throw new ArgumentNullException("entityIds", "One of the entityIds was null.");
                request = ValidateRequest(request);

                // Create context
                Context context = new Context();

                // Load top-level entities
                IEntityRef[] members = RequestedMembers(request, context);
                IEnumerable<IEntity> entities = Entity.Get(entityIds, false, securityOption, true, members);

                // Fill data
                IEnumerable<EntityData> result = PackageEntities(context, request, entities, true);

                var list = result.Where(e => e != null).ToList();
                return list;
            }
            catch (Exception ex)
            {
	            if (entityIds != null)
                    string.Join(", ", entityIds);
                EventLog.Application.WriteError("Failed GetEntitiesData:\n{0}\nRequest:\n{1}\n\nException:\n{2}",
                    entityIds, request == null ? "null" : request.Debug(), ex);
                throw;
            }
        }


		/// <summary>
		/// Loads structured data for the specified entities.
		/// This is an internal helper method and is not exposed as a service method.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="request">The description of fields and related entities to load.</param>
		/// <returns>
		/// The requested data.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// entities
		/// or
		/// entities;One of the entities was null.
		/// </exception>
        internal IEnumerable<EntityData> ToEntitiesData(IEnumerable<IEntity> entities, EntityMemberRequest request)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");
                if (entities.Any(e => e == null))
                    throw new ArgumentNullException("entities", "One of the entities was null.");
                request = ValidateRequest(request);

                // Create context
                Context context = new Context();

                // Load top-level entities
                IEntityRef[] members = RequestedMembers(request, context);                

                // Fill data
                IEnumerable<EntityData> result = PackageEntities(context, request, entities, true);

                var list = result.Where(e => e != null).ToList();
                return list;
            }
            catch (Exception ex)
            {
	            List<EntityRef> entityRefs = new List<EntityRef>();

                if (entities != null)
                {
                    entityRefs = entities.Select(e => new EntityRef(e as Entity)).ToList();
                }

                string.Join(", ", entityRefs);
                EventLog.Application.WriteError("Failed GetEntitiesData:\n{0}\nRequest:\n{1}\n\nException:\n{2}",
                    entityRefs, request == null ? "null" : request.Debug(), ex);                
                throw;
            }
        }


        /// <summary>
        /// Visits each entity and adds it to the set.
        /// </summary>
        /// <param name="context">The context of the overall query, including the resulting EntitySet. This cannot be null.</param>
        /// <param name="request">The request. This cannot be null.</param>
        /// <param name="entities">The entities. This cannot be null but may contain null.</param>
        /// <param name="membersPreloaded">If false, then PackageEntities will do a preload of entity fields.</param>
        /// <exception cref="ArgumentNullException">No argument can be null.</exception>
        private IEnumerable<EntityData> PackageEntities(Context context, EntityMemberRequest request, IEnumerable<IEntity> entities, bool membersPreloaded)
        {
            if (context == null)
            {
                throw new ArgumentException("context");
            }
            if (request == null)
            {
                throw new ArgumentException("request");
            }
            if (entities == null)
            {
                throw new ArgumentException("entities");
            }

            // Note: it is an intentional behavior that the result set must match the input set item-for-item, including nulls,
            // because PackageEntityRelationships needs to call this twice, once for entities, and once for relationship instances, and then
            // stitch the two back together. Therefore, no 'continue' without yielding a value, even if its null.

            // Bulk pre-load field data
            //if (!membersPreloaded && entities.Any( e => e != null ))
            //{
            //    IEntityRef[] members = RequestedMembers(request, context);
            //    Entity.PreloadData(entities, members);
            //}

            // No need for a security check here - since you are already passing in IEntity objects, 
            // they must have already been checked.
            //IEntityAccessControlService entityAccessControlService = new EntityAccessControlFactory().Service;
            //var checkedEntityIds = entityAccessControlService
            //    .Check(
            //        entities.Where(e => e != null).Select(e => new EntityRef(e)),
            //        new[] { Permissions.Read });
            //var filteredEntities = entities.Select(e => (e == null || !checkedEntityIds[e.Id]) ? null : e);

			// Translate each entity
            foreach (IEntity entity in entities)
            {
                // Skip nulls (but preserve symmetry in result)
                if (entity == null)
                {
                    yield return null;
                    continue;
                }

                // Create or get EntityData
                // Note: a node may have already been processed for a different request object (i.e. visitied twice asking for different fields)
                // So we still need to look up existing references

                EntityData entityData;
                bool isNew = false;
                if (!context.LoadedEntities.TryGetValue(entity.Id, out entityData))
                {
                    // Create a new EntityData
                    entityData = new EntityData();
                    entityData.Id = entity.Id;
                    entityData.TypeIds = entity.TypeIds.Select(typeId => new EntityRef(typeId)).ToList();

                    // And register it into entity set
                    context.LoadedEntities.Add(entity.Id, entityData);
                    isNew = true;
                }

                // Determine if this entity has already been processed for this member request
                var hashKey = new KeyValuePair<long, EntityMemberRequest>(entity.Id, request);
                if (isNew || !context.Processed.Contains(hashKey))
                {
                    context.Processed.Add(hashKey);

                    // Add field data
                    PackageEntityFields(context, request, entity, entityData, isNew);

                    // Add relationship data
                    PackageEntityRelationships(context, request, entity, entityData, isNew);
                }

                yield return entityData;
            }
        }


        /// <summary>
        /// Populates the field data for an EntityData.
        /// </summary>
        /// <param name="context">Overall request context.</param>
        /// <param name="request">Current request node to be loaded.</param>
        /// <param name="entity">Source entity to get the field data from.</param>
        /// <param name="entityData">Target entityData to store field data into.</param>
        /// <param name="isNew">Flag indicating that EntityData is fresh, so we don't need to worry about checking if data is already loaded.</param>
        private static void PackageEntityFields(Context context, EntityMemberRequest request, IEntity entity, EntityData entityData, bool isNew)
        {
            // Overall process:
            // - determine list of fields to load for this request
            // - determine if a particular field has already been loaded
            // - get the fields from entity
            // - store the field in entityData

            // Determine fields to load
            IEnumerable<IEntityRef> fields = null;
            if (request.AllFields)
                fields = entity.GetAllFields().Select<Field, IEntityRef>(f => new EntityRef(f, f.Alias));
            else
                fields = request.Fields;

            // Prepare container
            if (entityData.Fields == null)
                entityData.Fields = new List<FieldData>();

            // Translate fields
            foreach (EntityRef fieldId in fields)
            {
                // Check if already loaded
                if (!isNew && entityData.Fields.Any(f => f.FieldId.Id == fieldId.Id))
                    continue;

                // Get value in field
                // Note: hopefully all the fields got pre-loaded                
                object value = entity.GetField(fieldId);

                // Get/convert the type info & writeability for the field
                DatabaseType typeInfo;
                if (!context.FieldDatabaseTypeCache.TryGetValue(fieldId.Id, out typeInfo))
                {
                    Field field = Entity.Get<Field>(fieldId);
                    typeInfo = field.ConvertToDatabaseType();
                    context.FieldDatabaseTypeCache.Add(fieldId.Id, typeInfo);
                    
                    if (field.IsFieldWriteOnly ?? false)
                    {
                        context.WriteOnlyFields.Add(fieldId.Id);   
                    }
                    if (BulkRequests.BulkRequestHelper.IsVirtualAccessControlField(fieldId))
                    {
                        context.VirtualFields.Add(fieldId.Id);
                    }
                }

                // The field is writeonly so we set its value to null
                if (context.WriteOnlyFields.Contains(fieldId.Id))
                {
                    value = new WriteOnlyFieldReadValueGenerator().GenerateValue(fieldId.Id, typeInfo).Value;
                }
                else if (context.VirtualFields.Contains(fieldId.Id))
                {
                    value = BulkRequests.BulkRequestResultConverter.TryGetAccessControlField(entity.Id, fieldId).Value;
                }

                // And store it in the result
                var fieldData = new FieldData();
                fieldData.FieldId = fieldId;

                fieldData.Value = new TypedValue()
                {
                    Type = typeInfo,
                    Value = value
                };
                entityData.Fields.Add(fieldData);
            }
        }


        /// <summary>
        /// Populates the relationships for an EntityData. Recursively load entities.
        /// </summary>
        /// <param name="context">Overall request context.</param>
        /// <param name="request">Current request node to be loaded.</param>
        /// <param name="entity">Source entity to get the field data from.</param>
        /// <param name="entityData">Target entityData to store field data into.</param>
        /// <param name="isNew">Flag indicating that EntityData is fresh, so we don't need to worry about checking if data is already loaded.</param>
        private void PackageEntityRelationships(Context context, EntityMemberRequest request, IEntity entity, EntityData entityData, bool isNew)
        {
            // Overall process:
            // - determine list of fields to load for this request
            // - determine if a particular field has already been loaded
            // - get the fields from entity
            // - store the field in entityData

            // Prepare container
            if (isNew)
                entityData.Relationships = new List<RelationshipData>();


            // Translate relationships
            foreach (RelationshipRequest relReq in request.Relationships)
            {
                // Establish relationship identity and direction
                IEntity relationshipDefn = relReq.RelationshipTypeId.Entity;
                Direction direction = GetDirection(relReq.RelationshipTypeId, relReq.IsReverse);

                // Create object to represent results for this relationship type
                // Note: immediately register the relationship, as this method is re-entrant.
                var relData = new RelationshipData();
                relData.RelationshipTypeId = relReq.RelationshipTypeId;
                relData.IsReverse = relReq.IsReverse;
                relData.IsReverseActual = direction == Direction.Reverse;

                var relEntity = relationshipDefn.As<Relationship>();
                if (relEntity != null && relEntity.Cardinality != null && relEntity.Cardinality.Alias != null)
                {
                    relData.IsLookup = relEntity.IsLookup(direction);
                }

                // Hmm.. we may be processing the same relationship twice, via different member requests.
                // We need to do this so that any additional members get loaded on the related entities, but we don't want to
                // include the relationship list twice, so just remove the existing. (This is a bit hacky)
                entityData.Relationships.RemoveAll(r => SameRelationship(r, relData));
                entityData.Relationships.Add(relData);

                if (relReq.MetadataOnly)
                {
                    relData.Instances = new List<RelationshipInstanceData>();
                }
                else
                {
                    // Get the list of related entities from the entity model
                    var relationshipsRaw = Entity.GetRelationships(entity, relationshipDefn, direction);

                    // Throttle list size
                    var relationships = relationshipsRaw.Take(MaxRelatedEntities + 1).ToList();
                    long relCount = relationships.Count;

                    if (relCount > MaxRelatedEntities && !EntityDataBuilder<EDC.ReadiNow.Model.IEntity>.BypassMaxRelatedEntities(relReq, entityData.Id.Id))
                    {
                        EventLog.Application.WriteError(
                            "Exceeded maximum number ({0} > {1}) of related entities for EntityInfoService. Relationship: {2}. {3} entity: {4}",
                            relationships.Count,
                            MaxRelatedEntities,
                            relReq.RelationshipTypeId,
                            relReq.IsReverse ? "To" : "From",
                            entity.Id);

                        throw new InvalidOperationException("Exceeded maximum number of related entities for EntityInfoService.");
                    }
                    if (relCount > MaxRelatedEntities)
                    {
                        EventLog.Application.WriteWarning(
                            "Large number ({0} > {1}) of related entities for EntityInfoService. Relationship: {2}. {3} entity: {4}",
                            relationships.Count,
                            MaxRelatedEntitiesWarning,
                            relReq.RelationshipTypeId,
                            relReq.IsReverse ? "To" : "From",
                            entity.Id);
                    }

                    var entityIds = relationships.Where(p => p != null).Select(p => new EntityRef(p.Key));


                    var entityList =
                        Entity.Get(entityIds, false, true, relReq.RequestedMembers.Fields.Cast<IEntityRef>().ToArray())
                              .ToList();

                    // Package related entities and relationship instances
                    var entities = PackageEntities(context, relReq.RequestedMembers, entityList, false);

                    // Convert results into RelationshipInstanceData list
                    relData.Instances = entities.Select(relatedEntity =>
                                        new RelationshipInstanceData()
                                            {
                                                Entity = relatedEntity
                                            }).ToList();

                    // Rerun again with recursive request (note: this will fill into the existing entities)
                    if (relReq.IsRecursive)
                    {
                        var tmpEnum = PackageEntities(context, request, entityList, false);
                        ForceVisitAll(tmpEnum);
                    }
                }
            }
        }

        #region Context Helper Class

        /// <summary>
        /// Context class passed through recursive calls.
        /// Lifetime is only for the current service request.
        /// </summary>
        class Context
        {
            public Context()
            {
                this.LoadedEntities = new Dictionary<long, EntityData>();
                this.FieldDatabaseTypeCache = new Dictionary<long, DatabaseType>();
            }

            /// <summary>
            /// The result set.
            /// </summary>
            public Dictionary<long, EntityData> LoadedEntities;

            /// <summary>
            /// Lookup of field IDs to the database types that represent them.
            /// </summary>
            public Dictionary<long, DatabaseType> FieldDatabaseTypeCache;

            /// <summary>
            /// Holds a list of entities that have been processed.
            /// They must match both on the entity-id, and the data requested.
            /// </summary>
            public HashSet<KeyValuePair<long, EntityMemberRequest>> Processed = new HashSet<KeyValuePair<long, EntityMemberRequest>>();

            /// <summary>
            /// Holds the set of members that are required for a particular request.
            /// </summary>
            public Dictionary<EntityMemberRequest, IEntityRef[]> RequestMembers = new Dictionary<EntityMemberRequest, IEntityRef[]>();

            /// <summary>
            /// Holds a set of writeonly fields.            
            /// </summary>
            public HashSet<long> WriteOnlyFields = new HashSet<long>();

            /// <summary>
            /// Holds a set of writeonly fields.            
            /// </summary>
            public HashSet<long> VirtualFields = new HashSet<long>();
        }
        #endregion

        #region Helpers

		/// <summary>
		/// Determines the set of fields and relationships required for a particular request node.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="context">The context.</param>
		/// <returns></returns>
        private static IEntityRef[] RequestedMembers(EntityMemberRequest request, Context context)
        {
            // TODO: it'd be nice if we could somehow detect that request.AllFields is set at this point, then pass something back in the array.
            // Problem is that the total set of fields varies from instance to instance as instances may be of multiple types.

            IEntityRef[] result;
            var cache = context.RequestMembers;

            // Check if cached
            if (!cache.TryGetValue(request, out result))
            {
                // Get list of fields and relationships
                result =
                    request.Fields
                   // .Concat(request.Relationships.Select(r => r.RelationshipTypeId))      // TODO: preloading relationships doesn't seem to be working at the moment
                    .ToArray();

                // Cache
                cache.Add(request, result);
            }
            return result;
        }


        /// <summary>
        /// Reads the list of related entity IDs out of an entity for a given relationship type.
        /// </summary>
        private static IEnumerable<IEntity> GetRelationshipList(IEntity entity, IEntity relationshipDefn, Direction direction, string alias)
        {
            IEntityRelationshipCollection<IEntity> oRelated = entity.GetRelationships(relationshipDefn, direction);

            if (oRelated == null)
            {
                return Enumerable.Empty<IEntity>();
            }

            return oRelated.Where(ri => ri.Entity != null);
        }

        
        /// <summary>
        /// Gets the direction of an entity ref.
        /// </summary>
        public static Direction GetDirection(IEntityRef entityRef, bool isReverse)
        {
            return Entity.GetDirection(entityRef, isReverse);
        }


        /// <summary>
        /// Determine if two relationship data refer to the same relationship in the same direction.
        /// </summary>
        private static bool SameRelationship(RelationshipData rel1, RelationshipData rel2)
        {
            if (rel1.RelationshipTypeId.Id != rel2.RelationshipTypeId.Id)
                return false;

            var dir1 = GetDirection(rel1.RelationshipTypeId, rel1.IsReverse);
            var dir2 = GetDirection(rel2.RelationshipTypeId, rel2.IsReverse);

            return dir1 == dir2;
        }


        /// <summary>
        /// Validates the member request. Note: null requests are allowed.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="validatedEntityMemberRequests">
        /// Requests that have already been validated. Used for cycle detection.
        /// </param>
        private static EntityMemberRequest ValidateRequest(EntityMemberRequest request, ISet<EntityMemberRequest> validatedEntityMemberRequests = null )
        {
            if (request == null)
                return new EntityMemberRequest();

            if (validatedEntityMemberRequests == null)
            {
                validatedEntityMemberRequests = new HashSet<EntityMemberRequest>();
            }
            validatedEntityMemberRequests.Add(request);

            foreach (var fieldRequest in request.Fields)
            {
                if (fieldRequest == null)
                    throw new ArgumentException("One or more requested field entries was null.", "request");

                if (fieldRequest.Id == 0)
                    throw new ArgumentException("One or more requested field entries could not be resolved.", "request");
            }

            foreach (var relRequest in request.Relationships)
            {
                if (relRequest == null)
                    throw new ArgumentException("One or more requested relationship entries was null.", "request");

                if (relRequest.RelationshipTypeId == null)
                    throw new ArgumentException("One or more requested relationship entries had a null RelationshipTypeId.", "request");

                if (relRequest.RelationshipTypeId.Id == 0)
                    throw new ArgumentException("One or more requested relationship entries could not be resolved.", "request");

                if (!validatedEntityMemberRequests.Contains(relRequest.RequestedMembers))
                {
                    relRequest.RequestedMembers = ValidateRequest(relRequest.RequestedMembers, validatedEntityMemberRequests);
                }
            }
            return request;
        }

        /// <summary>
        /// Forces the enumerable to evaluate, without allocating a result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        private static void ForceVisitAll<T>(IEnumerable<T> enumerable)
        {
            foreach (var value in enumerable)
            {
            }
        }


        /// <summary>
        /// Return an entity ref for an entity, or null if the entity is null.
        /// </summary>
        private static EntityRef EntityRefOrNull(IEntity entity)
        {
            if (entity == null)
                return null;
            return new EntityRef(entity.Id);
        }

        #endregion

        /// <summary>
        /// Convert the list of Entities into EntityData populated with the provided query
        /// </summary>
        /// <param name="entities">Teh entities to convert</param>
        /// <param name="query">Additional information to collect</param>
        /// <returns></returns>
        public static IEnumerable<EntityData> ToEntityData(IEnumerable<IEntity> entities, string query)
        {
            EntityInfoService service = new EntityInfoService();

            EntityMemberRequest rq = Factory.RequestParser.ParseRequestQuery(query);

            IEnumerable<EntityRef> entityIds = entities.Select(e => new EntityRef(e.Id));

            IEnumerable<EntityData> entityDataList = service.GetEntitiesData(entityIds, rq);

            return entityDataList;
        }        
    }
}
