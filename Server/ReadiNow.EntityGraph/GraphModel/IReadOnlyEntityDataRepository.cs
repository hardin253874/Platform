// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// A read-only interface for accessing field and relationship data.
    /// </summary>
    interface IReadOnlyEntityDataRepository
    {
        /// <summary>
        /// Load a field value
        /// </summary>
        /// <returns>The field value.</returns>
        /// <exception cref="InvalidOperation">Thrown if it is not valid to request the specified field on the specified entity.</exception>
        object GetField( long entityId, long fieldId );

        /// <summary>
        /// Load a relationship
        /// </summary>
        /// <returns>The related entity IDs.</returns>
        /// <exception cref="InvalidOperation">Thrown if it is not valid to request the specified field on the specified entity.</exception>
        IReadOnlyCollection<long> GetRelationship( long entityId, long relTypeId, Direction direction );

        /// <summary>
        /// Try to load a field.
        /// </summary>
        /// <returns>True if the value was loaded, or if the value was originally requested. False if it was never originally loaded.</returns>
        bool TryGetField( long entityId, long fieldId, out object fieldValue );

        /// <summary>
        /// Try to load a relationship.
        /// </summary>
        /// <returns>True if the value was loaded, or if the value was originally requested. False if it was never originally loaded.</returns>
        bool TryGetRelationship( long entityId, long relTypeId, Direction direction, out IReadOnlyCollection<long> relationshipValues );
    }
}
