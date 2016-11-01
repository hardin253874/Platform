// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;


namespace EDC.ReadiNow.EntityRequests
{
    /// <summary>
    /// EntityInfoService methods for reading data.
    /// </summary>
    public interface IEntityInfoRead
    {
        /// <summary>
        /// Loads structured data for the specified entity.
        /// </summary>
        /// <param name="entity">The entity to load.</param>
        /// <param name="requestedData">The description of fields and related entities to load.</param>
        /// <returns>The requested data.</returns>
        /// <exception cref="PlatformSecurityException">
        /// The user lacks read access to the given entity.
        /// </exception>
        EntityData GetEntityData(EntityRef entity, EntityMemberRequest requestedData);

        /// <summary>
        /// Loads structured data for the specified entity.
        /// </summary>
        /// <param name="entities">The list of entities to load.</param>
        /// <param name="requestedData">The description of fields and related entities to load.</param>
        /// <param name="securityOption">How to handle access denied situations.</param>
        /// <returns>The requested data.</returns>
        /// <exception cref="PlatformSecurityException">
        /// Thrown if the user lacks read access to the given entities and <paramref name="securityOption"/> is <see cref="SecurityOption.DemandAll"/>.
        /// </exception>
        IEnumerable<EntityData> GetEntitiesData(IEnumerable<EntityRef> entities, EntityMemberRequest requestedData, SecurityOption securityOption = SecurityOption.SkipDenied);

        /// <summary>
        /// Loads structured data for all entities of the specified type.
        /// </summary>
        /// <param name="entityType">The type of the entities to be loaded.</param>
        /// <param name="includeDerivedTypes">If true, then instances of types that directly or indirectly derive from 'entityType' are also returned.</param>
        /// <param name="requestedData">The description of fields and related entities to load.</param>
        /// <returns>All instances of type 'entityType', and optionally all instances of types that derive from 'entityType'.</returns>
        IEnumerable<EntityData> GetEntitiesByType(EntityRef entityType, bool includeDerivedTypes, EntityMemberRequest requestedData);

        /// <summary>
        /// Runs a query and returns all entities that match the query.
        /// </summary>
        /// <param name="query">The query to execute. Do not specify columns or ordering. Only specify conditions and a root entity (and any joins required to apply the condition).</param>
        /// <param name="requestedData">The description of fields and related entities to load.</param>
        /// <returns>The requested data for all entities matched by the query.</returns>
        IEnumerable<EntityData> QueryEntityData(StructuredQuery query, EntityMemberRequest requestedData);
    }

    /// <summary>
    /// EntityInfoService methods for modifying data.
    /// </summary>
    public interface IEntityInfoWrite
    {
        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <param name="newEntityData">The entity graph.</param>
        /// <returns>The ID of the new root entity.</returns>
        EntityRef CreateEntity(EntityData newEntityData);

        /// <summary>
        /// Updates an entity graph.
        /// </summary>
        /// <param name="updatedEntityData">The list of entities to load.</param>
        void UpdateEntity(EntityData updatedEntityData);

        /// <summary>
        /// Modifies the type(s) of an entity.
        /// </summary>
        /// <remarks>
        /// An entity may be of multiple type, so long as the types are willing to be shared.
        /// Specify the Id and TypeIds of the EntityData parameter. No other data is required.
        /// </remarks>
        /// <param name="updatedEntityData">The list of entities to load.</param>
        void UpdateEntityType(EntityData updatedEntityData);

        /// <summary>
        /// Deletes an individual entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void DeleteEntity(EntityRef entity);

        /// <summary>
        /// Deletes an individual entity.
        /// </summary>
        /// <param name="entities">The entity to delete.</param>
        void DeleteEntities(IEnumerable<EntityRef> entities);

    }

}
