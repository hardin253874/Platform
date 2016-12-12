// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Diagnostics;
using IEntityRef = EDC.ReadiNow.Model.IEntityRef;
using Entity = EDC.ReadiNow.Model.Entity;        // only for Entity.Direction
using EntityRef = EDC.ReadiNow.Model.EntityRef;
using TypedValue = EDC.ReadiNow.Metadata.TypedValue;
using Direction = EDC.ReadiNow.Model.Direction;
using EDC.ReadiNow.Configuration;

namespace EDC.ReadiNow.EntityRequests
{
	/// <summary>
	/// Mechanism for building the recursive EntityData objects.
	/// Abstracts building the EntityData away from collecting the data.
	/// Usage: Create an instance; set the various callback properties; then call PackageEntities.
	/// </summary>
	/// <typeparam name="TEntity">Type of object that represents an entity handle, which gets passed to the callbacks.</typeparam>
	public class EntityDataBuilder<TEntity>
	{
		/// <summary>
		/// Callback - given an entity and a field reference, returns the typed-value container for that field.
		/// </summary>
		public Func<TEntity, IEntityRef, TypedValue> GetFieldValue { get; set; }

		/// <summary>
		/// Callback - given an entity, relationship ID, direction, return the related entities as a list of pairs, which contain (instance,entity).
		/// </summary>
		public Func<TEntity, IEntityRef, Direction, IEnumerable<TEntity>> GetRelationships { get; set; }

		/// <summary>
		/// Callback - given an entity, returns true if we're allowed to access it.
		/// </summary>
		public Func<TEntity, bool> CanAccess { get; set; }

		/// <summary>
		/// Callback - given a relationship ID, and direction, is the relationship a lookup.
		/// </summary>
		public Func<IEntityRef, Direction, bool> GetIsLookup { get; set; }

		/// <summary>
		/// Callback - given an entity, return its ID.
		/// </summary>
		public Func<TEntity, long> GetId { get; set; }

		/// <summary>
		/// Callback - given an entity, return its ID.
		/// </summary>
		public Func<TEntity, IEnumerable<EntityRef>> GetAllFields { get; set; }

		/// <summary>
		/// Gets or sets the is field applicable.
		/// </summary>
		/// <value>
		/// The is field applicable.
		/// </value>
		public Func<EntityRef, TEntity, bool> IsFieldApplicable { get; set; }

        /// <summary>
        /// Visits each entity and adds it to the set.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entities">The entities.</param>
        public IEnumerable<EntityData> PackageEntities(EntityMemberRequest request, IEnumerable<TEntity> entities)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (entities == null)
                throw new ArgumentNullException("entities");

            Context context = new Context();
            var result = PackageEntities(context, request, entities);
            return result;
        }

        /// <summary>
        /// Visits each entity and adds it to the set.
        /// </summary>
        /// <param name="context">The context of the overall query, including the resulting EntitySet.</param>
        /// <param name="request">The request.</param>
        /// <param name="entities">The entities.</param>
        private IEnumerable<EntityData> PackageEntities(Context context, EntityMemberRequest request, IEnumerable<TEntity> entities)
        {
            // Note: it is an intentional behavior that the result set must match the input set item-for-item, including nulls,
            // because PackageEntityRelationships needs to call this twice, once for entities, and once for relationship instances, and then
            // stitch the two back together. Therefore, no 'continue' without yielding a value, even if its null.

            // Translate each entity
            foreach (TEntity entity in entities)
            {
                // Skip nulls (but preserve symmetry in result)
                if (Equals(entity, default(TEntity)))
                {
                    yield return null;
                    continue;
                }

                // Call security callback
                if (!CanAccess(entity))
                {
                    yield return null;
                    continue;
                }

                // Return nulls if request is null, which can happen if we encounter a non-null relationship instance with a null relationship instance request
                if (request == null)
                {
                    yield return null;
                    continue;
                }

                // Create or get EntityData
                // Note: a node may have already been processed for a different request object (i.e. visitied twice asking for different fields)
                // So we still need to look up existing references

                bool noShare = request.DisallowInstanceReuse;
                Context useContext = noShare ? context.InternalContext : context;

                EntityData entityData = null;
                bool isNew = false;
                long id = GetId(entity);
                if (!useContext.LoadedEntities.TryGetValue(id, out entityData))
                {
                    // Create a new EntityData
                    entityData = new EntityData();
                    entityData.Id = id;

                    // And register it into entity set
                    useContext.LoadedEntities.AddOrUpdate(id, entityData, ( k, v ) => v );
                    isNew = true;
                }

                // Determine if this entity has already been processed for this member request
                var hashKey = new KeyValuePair<long, EntityMemberRequest>(id, request);
                if (isNew || !useContext.Processed.Contains(hashKey))
                {
                    useContext.Processed.Add(hashKey);

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
        private void PackageEntityFields(Context context, EntityMemberRequest request, TEntity entity, EntityData entityData, bool isNew)
        {
            // Overall process:
            // - determine list of fields to load for this request
            // - determine if a particular field has already been loaded
            // - get the fields from entity
            // - store the field in entityData

            // Determine fields to load
            IEnumerable<IEntityRef> fields;
            if (request.AllFields)
                fields = GetAllFields(entity);
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

				if ( ! IsFieldApplicable( fieldId, entity ) )
					continue;

                // And store it in the result
                var fieldData = new FieldData();
                fieldData.FieldId = fieldId;
                fieldData.Value = GetFieldValue(entity, fieldId);
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
        private void PackageEntityRelationships(Context context, EntityMemberRequest request, TEntity entity, EntityData entityData, bool isNew)
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
                Direction direction = Entity.GetDirection(relReq.RelationshipTypeId, relReq.IsReverse);

                // Create object to represent results for this relationship type
                // Note: immediately register the relationship, as this method is re-entrant.
                var relData = new RelationshipData();
                relData.RelationshipTypeId = relReq.RelationshipTypeId;
                relData.IsReverse = relReq.IsReverse;
                relData.IsReverseActual = direction == Direction.Reverse;
                relData.IsLookup = GetIsLookup(relReq.RelationshipTypeId, direction);

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
                    var relationshipsRaw = GetRelationships(entity, relReq.RelationshipTypeId, direction);

                    var relationships = relationshipsRaw
                        .Where(p => !Equals(p, default(TEntity)))
                        .Take( FanoutHelper.MaxRelatedEntities + 1)
                        .ToList();

                    // Throttle list size
                    if (!BulkPreloader.TenantWarmupRunning)
                    {
                        int relCount = relationships.Count;
                        FanoutHelper.CheckFanoutLimit( relReq, GetId( entity ), relCount );
                    }
                    
                    // Split lists
                    var entityList = relationships.Select(p => p);

                    // Package related entities and relationship instances
                    var entities = PackageEntities(context, relReq.RequestedMembers, entityList);

                    // Convert results into RelationshipInstanceData list
                    relData.Instances = entities.Select(relatedEntity =>
                                        new RelationshipInstanceData()
                                            {
                                                Entity = relatedEntity
                                            })
                        .Where(rid => rid.Entity != null)
                        .ToList();

                    // Rerun again with recursive request (note: this will fill into the existing entities)
                    if (relReq.IsRecursive)
                    {
                        var tmpEnum = PackageEntities(context, request, entityList);
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
            public Context(bool createInternal = true)
            {
				LoadedEntities = new ConcurrentDictionary<long, EntityData>( );

                if (createInternal)
                {
                    InternalContext = new Context(false);
                }
            }

            /// <summary>
            /// The result set.
            /// </summary>
            public readonly ConcurrentDictionary<long, EntityData> LoadedEntities;

            /// <summary>
            /// Holds a list of entities that have been processed.
            /// They must match both on the entity-id, and the data requested.
            /// </summary>
            public readonly HashSet<KeyValuePair<long, EntityMemberRequest>> Processed = new HashSet<KeyValuePair<long, EntityMemberRequest>>();

            /// <summary>
            /// An internal context for holding nodes they should not be exposed through the API, but are just used as an internal convenience for loading types.
            /// </summary>
            public readonly Context InternalContext;
        }
        #endregion

        #region Helpers


        /// <summary>
        /// Validates the member request. Note: null requests are allowed.
        /// </summary>
        /// <param name="request">The request.</param>
        private static EntityMemberRequest ValidateRequest(EntityMemberRequest request)
        {
            if (request == null)
                return new EntityMemberRequest();

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

                relRequest.RequestedMembers = ValidateRequest(relRequest.RequestedMembers);
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
            enumerable.Count();
        }

        /// <summary>
        /// Determine if two relationship data refer to the same relationship in the same direction.
        /// </summary>
        private static bool SameRelationship(RelationshipData rel1, RelationshipData rel2)
        {
            if (rel1.RelationshipTypeId.Id != rel2.RelationshipTypeId.Id)
                return false;

            var dir1 = Entity.GetDirection(rel1.RelationshipTypeId, rel1.IsReverse);
            var dir2 = Entity.GetDirection(rel2.RelationshipTypeId, rel2.IsReverse);

            return dir1 == dir2;
        }

        /// <summary>
        /// Return true if we should special-case bypass the max fanout limit.
        /// </summary>
        /// <remarks>
        /// Ordinarily we really want to lock down the fanout. But there's already too much code that
        /// relies on following 'derivedTypes' from resource, which is too many. TODO: Fix this one day.
        /// </remarks>
        /// <returns>True to bypass, otherwise false.</returns>
        internal static bool BypassMaxRelatedEntities(RelationshipRequest request, long originEntityId)
        {
            // only bypass core:derivedTypes in the forward direction. (noting that the alias itself is reverse .. but we are following the reverse alias forwards)
            if (request.RelationshipTypeId.Alias != "derivedTypes" || request.RelationshipTypeId.Namespace != "core" || request.IsReverse)
                return false;

            // only bypass if we're coming from core:resource
            IEntity entity = Entity.Get(originEntityId);
            if (entity == null)
                return false;

            bool result = entity.Alias == "resource" && entity.Namespace == "core";

            return result;
        }

        #endregion       
    }
}
