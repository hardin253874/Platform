// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Core;
using EDC.Cache;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Bulk request runner is the entry point for a mechanism that:
    /// - issues entire-graph requests to SQL server in a single round trip
    /// - caches unsecured results
    /// - applies security after cache-lookup
    /// </summary>
    public static class BulkRequestRunner
    {
        /// <summary>
        /// Cache of secured request results
        /// </summary>
        private static ICache<CachingBulkRequestRunnerKey, IEnumerable<EntityData>> _securedResultCache = null;

        /// <summary>
        /// Loads structured data for all entities of the specified type(s).
        /// The 'Entities' are interpreted as the types.
        /// </summary>
        /// <param name="request">The description of fields and related entities to load.</param>
        /// <returns>All instances of type 'entityType', and optionally all instances of types that derive from 'entityType'.</returns>
        public static IEnumerable<EntityData> GetEntities( EntityRequest request )
        {
            IEnumerable<EntityData> result;

            // Ensure cache is ready
            if ( _securedResultCache == null )
            {
                _securedResultCache = Factory.Current.ResolveNamed<ICache<CachingBulkRequestRunnerKey, IEnumerable<EntityData>>>( "BulkRequestRunner Secured Result" );
            }

            // Check request
            if ( ShouldCacheAsMetadata( request ) )
            {
                // Don't use the bulk request runner result cache when we're using the metadata cache.
                request.IgnoreResultCache = true;

                CachingBulkRequestRunnerKey key = CachingBulkRequestRunnerKey.Create( request );

                _securedResultCache.TryGetOrAdd( key, out result, key1 =>
                    {
                        return GetEntitiesRouteRequest( request );
                    } );
            }
            else
            {
                result = GetEntitiesRouteRequest( request );
            }

            return result;
        }

		/// <summary>
		/// A shorthand fetch a filtered list of resources of the provided type alias.
		/// </summary>
		/// <param name="typeAlias">The type of entities to fetch</param>
		/// <param name="filter">The filter to apply</param>
		/// <param name="requestString">The request string.</param>
		/// <returns>
		/// The entity refs that match the filter
		/// </returns>
		public static IEnumerable<EntityRef> FetchFilteredEntities(string typeAlias, string filter, string requestString = null)
        {
            var request = new EntityRequest(typeAlias, "id", QueryType.FilterInstances, "BulkRequestRunner FetchFilteredEntites");
            request.Filter = filter;
            var controls = BulkRequestRunner.GetEntities(request);
            return controls.Select(ed => ed.Id);
        }


        private static IEnumerable<EntityData> GetEntitiesRouteRequest( EntityRequest request )
        {
            if (request == null)
                throw new ArgumentNullException("request");

            IEnumerable<EntityData> result;

            switch (request.QueryType)
            {
                case QueryType.Instances:
                case QueryType.ExactInstances:
                    result = GetEntitiesByType(request);
                    break;
                case QueryType.FilterInstances:
                case QueryType.FilterExactInstances:
                    result = GetEntitiesFiltered(request);
                    break;
                case QueryType.Basic:
                case QueryType.BasicWithDemand:
                    result = GetEntitiesData(request);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown query type {0}", request.QueryType));
            }

            return result;
        }

        /// <summary>
        /// Runs a request. Caches it. Secures it.
        /// </summary>
        /// <param name="entityRef">The entity to load.</param>
        /// <param name="request">The entity member request string.</param>
        /// <param name="hintText">A hint about what this query is doing. Use for logging/diagnostics only.</param>
        /// <returns>The entity data for the requested entity, or null if it was not found.</returns>
        public static EntityData GetEntityData(EntityRef entityRef, string request, string hintText = null)
        {
            if (entityRef == null)
                throw new ArgumentNullException("entityRef");
            if (request == null)
                throw new ArgumentNullException("request");

            // Run query
            var results = GetEntitiesData(entityRef.ToEnumerable(), request, hintText);
            EntityData result = results.FirstOrDefault();

            return result;
        }


        /// <summary>
        /// Runs a request. Caches it. Secures it.
        /// </summary>
        /// <param name="entityRefs">The entities to load.</param>
        /// <param name="request">The entity member request string.</param>
        /// <param name="hintText">A hint about what this query is doing. Use for logging/diagnostics only.</param>
        /// <returns></returns>
        public static IEnumerable<EntityData> GetEntitiesData(IEnumerable<EntityRef> entityRefs, string request, string hintText = null)
        {
            if (entityRefs == null)
                throw new ArgumentNullException("entityRefs");
            if (request == null)
                throw new ArgumentNullException("request");

            var entityRequest = new EntityRequest
                {
                    QueryType = QueryType.Basic,
                    Entities = entityRefs,
                    RequestString = request,
                    Hint = hintText
                };

            return GetEntitiesData(entityRequest);
        }

        /// <summary>
        /// Runs a request. Caches it. Secures it.
        /// </summary>
        /// <param name="request">The entity member request string.</param>
        /// <returns></returns>
        public static IEnumerable<EntityData> GetEntitiesData(EntityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (request.Entities == null)
                throw new ArgumentNullException("request", "request.Entities cannot be null");
            
            if (!request.EntityIDs.Any())
            {
                return Enumerable.Empty<EntityData>();
            }

            // Check cache and/or get unsecured result
            BulkRequestResult unsecuredResult = BulkResultCache.GetBulkResult(request);

            // Bail out if we're not interested in cached results.
            if (request.DontProcessResultIfFromCache && request.ResultFromCache)
            {
                return Enumerable.Empty<EntityData>();
            }

            // Secure the result
            IEnumerable<EntityData> results = BulkRequestResultConverter.BuildAndSecureResults(unsecuredResult, request.EntityIDs, 
                request.QueryType == QueryType.BasicWithDemand ? SecurityOption.DemandAll : SecurityOption.SkipDenied);
            return results;
        }

        /// <summary>
        /// Loads structured data for all entities of the specified type(s).
        /// The 'Entities' are interpreted as the types.
        /// </summary>
        /// <param name="request">The description of fields and related entities to load.</param>
        /// <returns>All instances of type 'entityType', and optionally all instances of types that derive from 'entityType'.</returns>
        public static IEnumerable<EntityData> GetEntitiesByType(EntityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (request.Entities.Count() != 1)
                throw new ArgumentException("Expected exactly one type.", "request");

            string rqString = request.RequestString ?? request.Request.RequestString;

            // Modify query
            string instancesRequest;
            if (request.QueryType == QueryType.ExactInstances)
            {
                instancesRequest = string.Concat("instancesOfType.{", rqString, "}");
                // This request structure corresponds to AdjustEntityMemberRequest below
            }
            else
            {
                instancesRequest = string.Concat("{instancesOfType, derivedTypes*.instancesOfType}.{", rqString, "}");
                // This request structure corresponds to AdjustEntityMemberRequest below
            }

            request.Request = null;
            request.RequestString = instancesRequest;

            // Run as query for type
            // Note: this will secure results
            EntityData typeData = GetEntitiesData(request).FirstOrDefault();
            if (typeData == null)
            {
                return Enumerable.Empty<EntityData>();
            }

            // Get results
            if (request.QueryType == QueryType.ExactInstances)
            {
                var instances = typeData.GetRelationship("instancesOfType").Entities;
                return instances;
            }
            else
            {
                var types = Delegates.WalkGraph(typeData, type => type.Relationships.Count == 2
                    && type.Relationships[0].RelationshipTypeId.Alias == "instancesOfType"
                    && type.Relationships[1].RelationshipTypeId.Alias == "derivedTypes" ? type.Relationships[1].Entities : null);

                var instances = from type in types
                        from instance in type.GetRelationship("instancesOfType").Entities
                        select instance;

                return instances;
            }
        }

        /// <summary>
        /// Apply tweaks to the parsed query.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="memberRequest"></param>
        internal static void AdjustEntityMemberRequest(EntityRequest request, EntityMemberRequest memberRequest)
        {
            // Disallow mixing of EntityData between the actual result, and the structure we're using to walk instances
            // so we don't pollute the result with unrequested instance & derived type relationships.
            switch (request.QueryType)
            {
                case QueryType.ExactInstances:
                    memberRequest.DisallowInstanceReuse = true;
                    break;
                case QueryType.Instances:
                    memberRequest.DisallowInstanceReuse = true;
                    memberRequest.Relationships[1].RequestedMembers.DisallowInstanceReuse = true;
                    break;
            }
        }

        /// <summary>
        /// Loads structured data for all entities of the specified type(s).
        /// The 'Entities' are interpreted as the types.
        /// </summary>
        /// <param name="request">The description of fields and related entities to load.</param>
        /// <returns>All instances of type 'entityType', and optionally all instances of types that derive from 'entityType'.</returns>
        private static IEnumerable<EntityData> GetEntitiesFiltered(EntityRequest request)
        {
            if (request.Entities.Count() != 1)
                throw new ArgumentException("Expected exactly one type.", "request");
            if (string.IsNullOrEmpty(request.Filter))
                throw new ArgumentNullException("Expected Filter property to be set.", "request");
            if (request.QueryType != QueryType.FilterInstances && request.QueryType != QueryType.FilterExactInstances)
                throw new ArgumentNullException("Expected QueryType to be FilterInstances or FilterExactInstances.", "request");

            EntityRef entityType = request.Entities.Single();
            bool derivedTypes = request.QueryType == QueryType.FilterInstances;

            // Get matching entities
            IEnumerable<long> entities =
                Entity.GetCalculationMatchesAsIds(request.Filter, entityType, derivedTypes);
            IEnumerable<EntityRef> entityRefs =
                entities.Select(id => new EntityRef(id));

            // Create a new request to load just that list of entities
            EntityRequest newRequest = new EntityRequest(entityRefs, request.RequestString, request.Hint);
            newRequest.Request = request.Request;

            // Run as a request of specific IDs
            IEnumerable<EntityData> result = GetEntitiesData(newRequest);
            return result;
        }

        /// <summary>
        /// Should we use per-ruleset caching with metadata-change invalidation to cache the secured result of the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static bool ShouldCacheAsMetadata( EntityRequest request )
        {
            if ( request.Isolated )
                return false;

            if ( request.DontProcessResultIfFromCache )
                return false;

            if ( request.IgnoreResultCache )
                return false;

            // Only do metadata caching if a single root ID is involved.
            long[] ids = request.EntityIDs.Take( 2 ).ToArray();
            if ( ids.Length != 1 )
                return false;
            long id = ids [ 0 ];

            using ( new SecurityBypassContext( ) )
            {
                IEntity entity = Entity.Get( id );
                if ( entity == null )
                    return false;

                EntityType entityType = Entity.Get<EntityType>( entity.TypeIds.FirstOrDefault( ) );
                if ( entityType == null )
                    return false;

                if ( entityType.IsMetadata != true )
                {
                    EventLog.Application.WriteTrace( "BulkRequestRunner will not use metadata cache for type : {0}", entityType.Name );
                    return false;
                }

                return true;
            }
        }

    }
}
