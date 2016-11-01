// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using EDC.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Silverlight.Services.Metadata.Model;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
    /// <summary>
    /// EntityPackageContext data passed around while processing a request.
    /// </summary>
    public class EntityPackageContext
    {
        public EntityPackageContext()
        {
            EntitiesVisited = new HashSet<long>();
            Svc = new EntityInfoService();
;
            Result = new JsonQueryResult();
        }


        /// <summary>
        /// Entities visited *in the current query*.
        /// </summary>
        public HashSet<long> EntitiesVisited { get; set; }

        private EntityInfoService Svc { get; set; }

        public JsonQueryResult Result { get; private set; }


        /// <summary>
        /// Add a request for an entity along with a query
        /// </summary>
        /// <param name="entityRef"></param>
        /// <param name="entityRequest"></param>
        public void AddEntityRequest(EntityRef entityRef, EntityMemberRequest entityRequest)
        {
            EntityData entityData = Svc.GetEntityData(entityRef, entityRequest);

            Result.IDs.Add(entityData.Id.Id);

            Result.Results.Add(new JsonSingleQueryResult
            {
                IDs = new List<long> { entityData.Id.Id },
                Code = HttpStatusCode.OK
            });
            AppendEntityToResult(Result, entityData, this);

        }


        /// <summary>
        /// Add a request for a set of entities and queries.
        /// </summary>
        /// <param name="batch"></param>
        public void AddEntityRequestBatch(JsonQueryBatchRequest batch)
        {
            var result = this.Result;


            var svc = Svc;

            var memberRequests = new EntityMemberRequest[batch.Queries.Length];

            foreach (var request in batch.Requests)
            {
                EntitiesVisited.Clear();

                var singleResult = new JsonSingleQueryResult();

                try
                {
                    // Get/parse the request query. (Either parse, or load from previous cache as part of this request).
                    if (request.QueryIndex < 0 || request.QueryIndex >= batch.Queries.Length)
                    {
                        throw new Exception("Cannot locate query string.");
                    }
                    var memberRequest = memberRequests[request.QueryIndex];
                    if (memberRequest == null)
                    {
                        string queryString = batch.Queries[request.QueryIndex];

                        //// Parse the request
                        try
                        {
                            memberRequest = EntityRequestHelper.BuildRequest(queryString);
                        }
                        catch (Exception ex)
                        {
                            EventLog.Application.WriteError(ex.ToString());
                            singleResult.Code = HttpStatusCode.BadRequest;
                            continue;
                        }
                        memberRequests[request.QueryIndex] = memberRequest;
                    }

                    // Get the entityRefs
                    IEnumerable<EntityRef> entityRefs = request.Ids.Select(objId =>
                        {
                            if (objId is string)
                            {
                                var entityRef = new EntityRef((string) objId);
                                try
                                {
                                    long id = entityRef.Id;
                                }
                                catch (ArgumentException ex)
                                {
                                    singleResult.Code = HttpStatusCode.NotFound;
                                    return new EntityRef(0); // or whatever
                                }
                                return entityRef;
                            }
                            if (objId is long)
                            {
                                return new EntityRef((long) objId);
                            }
                            if (objId is int)
                            {
                                return new EntityRef((long) (int) objId);
                            }
                            throw new InvalidOperationException();
                        });


                    // Run the request
                    IEnumerable<EntityData> entitiesData;
                    try
                    {
                        switch (request.QueryType)
                        {
                            case QueryType.Instances:
                                entitiesData = svc.GetEntitiesByType(entityRefs.Single(), true, memberRequest);
                                break;
                            case QueryType.ExactInstances:
                                entitiesData = svc.GetEntitiesByType(entityRefs.Single(), false, memberRequest);
                                break;
                            case QueryType.Basic:
                            default:
                                entitiesData = svc.GetEntitiesData(entityRefs, memberRequest);
                                break;
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        // sorry world!
                        if (ex.Message.Contains("does not represent a known entity"))
                        {
                            singleResult.Code = HttpStatusCode.BadRequest;
                            continue;
                        }
                        throw;
                    }


                    // Package results
                    singleResult.IDs = new List<long>();

                    foreach (var entityData in entitiesData)
                    {
                        AppendEntityToResult(result, entityData, this);
                        singleResult.IDs.Add(entityData.Id.Id);
                    }

                    // Capture result code
                    singleResult.Code = singleResult.IDs.Count > 0
                                            ? HttpStatusCode.OK
                                            : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError(ex.ToString());
                    singleResult.Code = HttpStatusCode.InternalServerError;
                }
                finally
                {
                    // Place in the finally block so we are certain that it gets run exactly once per iteration
                    result.Results.Add(singleResult);
                }
            }

            if (result.Results.Count != batch.Requests.Length)
            {
                // We are indexing by ID, so we must always ensure that request position matches result position.
                throw new Exception("Internal error: batch query mismatch.");
            }
        }


        /// <summary>
        /// Store an entity into the JSON result structure.
        /// </summary>
        private void AppendEntityToResult(JsonQueryResult result, EntityData entityData, EntityPackageContext entityPackageContext)
        {
            long entityKey = entityData.Id.Id;

            // Avoid circular loops within a query.
            if (entityPackageContext.EntitiesVisited.Contains(entityKey))
                return;
            entityPackageContext.EntitiesVisited.Add(entityKey);

            Dictionary<string, object> entityJson;
            if (!result.Entities.TryGetValue(entityKey, out entityJson))
            {
                entityJson = new Dictionary<string, object>();
                result.Entities.Add(entityKey, entityJson);
            }

            AppendFieldsToEntity(result, entityData, entityPackageContext, entityJson);

            AppendRelationshipsToEntity(result, entityData, entityPackageContext, entityJson);

            if (!entityJson.Any())
            {
                // Note: always add, then remove if not needed. Otherwise it's not present in the dictionary when we recurse,
                // which can lead to infinite loops.
                result.Entities.Remove(entityKey);
            }
        }


        /// <summary>
        /// Store fields into the JSON entity structure.
        /// </summary>
        /// <param name="entityData">The entity being read.</param>
        /// <param name="entityPackageContext"></param>
        /// <param name="entityJson">JSON for holding member data for the individual entity.</param>
        /// <returns></returns>
        private void AppendFieldsToEntity(JsonQueryResult result, EntityData entityData, EntityPackageContext entityPackageContext,
                                          Dictionary<string, object> entityJson)
        {
            var members = result.Members;

            // Visit fields
            foreach (var field in entityData.Fields)
            {
                long fieldId = field.FieldId.Id;
                // Register the field member
                JsonMember member;
                if (members.TryGetValue(fieldId, out member))
                {
                    if (field.FieldId.Alias != null && member.Alias == null)
                    {
                        member.Alias = GetAliasString(field.FieldId);
                    }
                }
                else
                {
                    member = new JsonMember();
                    member.Alias = GetAliasString(field.FieldId);
                    member.DataType = DataTypeHelper.FromDatabaseType(field.Value.Type).ToString();
                    members.Add(fieldId, member);
                }

                object value = field.Value.Value;

                // Ensure empty strings are represented as nulls - this is by design!
                if (value is string && ((string) value).Length == 0)
                    value = null;

                // Store the data in the entity
                entityJson[fieldId.ToString(CultureInfo.InvariantCulture)] = value;
            }
        }


        /// <summary>
        /// Store relationships into the JSON entity structure.
        /// </summary>
        /// <param name="entityData">The entity being read.</param>
        /// <param name="entityPackageContext"></param>
        /// <param name="entityJson">JSON for holding member data for the individual entity.</param>
        /// <returns></returns>
        private void AppendRelationshipsToEntity(JsonQueryResult result, EntityData entityData, EntityPackageContext entityPackageContext,
                                                 Dictionary<string, object> entityJson)
        {
            // Visit relationships
            foreach (var rel in entityData.Relationships)
            {
                // Register the relationship member metadata
                // Note: we need to first see if there is a container for the relationship in either direction
                RegisterRelationshipMember(result, entityPackageContext, rel);

                // Get relationship data.
                // Holds a mixture of either long or JsonRelationshipInstance
                var relResult = new List<object>();
                foreach (var relInst in rel.Instances)
                {
                    // If we have a relationship instance, send a complex object
                    // Otherwise just send the ID of the entity we're relating to.
                    if (relInst.RelationshipInstanceEntity != null)
                    {
                        var relInstResult = new JsonRelationshipInstance();
                        if (relInst.Entity != null)
                        {
                            relInstResult.EntityId = relInst.Entity.Id.Id;
                        }
                        relInstResult.RelationshipInstance = relInst.RelationshipInstanceEntity.Id.Id;
                        relResult.Add(relInstResult);
                        // Recursively visit the relationship instance entity
                        AppendEntityToResult(result, relInst.RelationshipInstanceEntity, entityPackageContext);
                    }
                    else if (relInst.Entity != null)
                    {
                        long relatedId = relInst.Entity.Id.Id;
                        relResult.Add(relatedId);
                    }

                    // Recursively visit the related entity
                    AppendEntityToResult(result, relInst.Entity, entityPackageContext);
                }

                // Store the data in the entity
                StoreRelationshipData(entityJson, rel, relResult);
            }
        }


        /// <summary>
        /// Registers a 'member' entry for the relationship.
        /// </summary>
        private void RegisterRelationshipMember(JsonQueryResult result, EntityPackageContext entityPackageContext, RelationshipData rel)
        {
            var members = result.Members;
            long relId = rel.RelationshipTypeId.Id;

            // Determine if there is an existing member registered for this relationship.
            // And create one if necessary.
            JsonMember member;
            if (!members.TryGetValue(relId, out member))
            {
                member = new JsonMember();
                members.Add(relId, member);
            }

            // Then we ensure there is information for the member in the particular direction that we're interested in.
            JsonRelationshipMember relMember = rel.IsReverseActual ? member.Reverse : member.Forward;
            if (relMember == null)
            {
                relMember = new JsonRelationshipMember();
                relMember.Alias = GetAliasString(rel.RelationshipTypeId);
                relMember.IsLookup = rel.IsLookup;
                if (rel.IsReverseActual)
                    member.Reverse = relMember;
                else
                    member.Forward = relMember;
            }
            else if (relMember.Alias == null && rel.RelationshipTypeId.Alias != null)
            {
                relMember.Alias = GetAliasString(rel.RelationshipTypeId);
            }
        }

        /// <summary>
        /// Stores an array of relationship data into an entity.
        /// </summary>
        /// <param name="entityJson">Dictionary that represents the entity.</param>
        /// <param name="rel">The relationship being stored.</param>
        /// <param name="relResult">The data being stored for that relationship.</param>
        private static void StoreRelationshipData(Dictionary<string, object> entityJson, RelationshipData rel,
                                                  List<object> relResult)
        {
            long relId = rel.RelationshipTypeId.Id;

            // Look up whether there is already data stored for this relationship.
            // (As it may already be stored for the opposite direction).
            object data;
            JsonRelationshipData relPair;
            string relKey = relId.ToString(CultureInfo.InvariantCulture);
            if (entityJson.TryGetValue(relKey, out data))
            {
                relPair = (JsonRelationshipData) data;
            }
            else
            {
                // Create if necessary
                relPair = new JsonRelationshipData();
                entityJson[relKey] = relPair;
            }

            // Store the data on the appropriate direction.
            if (rel.IsReverseActual)
            {
                relPair.Reverse = relResult;
            }
            else
            {
                relPair.Forward = relResult;
            }
        }


        /// <summary>
        /// Creates a string that represents the ns:alias of an EntityRef.
        /// </summary>
        /// <param name="entityRef"></param>
        /// <returns></returns>
        public string GetAliasString(EntityRef entityRef)
        {
            if (entityRef.Alias == null)
                return null;
            string res = entityRef.Namespace + ":" + entityRef.Alias;
            return res;
        }

    }
}