// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Loads types for groups of entities.
    /// </summary>
    /// <remarks>
    /// Rather than "types to entities of that type", a better way might be "query to entities controlled by that query".
    /// However, queries are not shared between types so this would not be worthwhile.
    /// </remarks>
    public interface IEntityTypeRepository
    {
        /// <summary>
        /// Return the type IDs for the given entities;
        /// </summary>
        /// <param name="entityRefs">
        /// The entities to get the type of.
        /// </param>
        /// <returns>
        /// A map of the unique sets of types to  a set of entities of that type.
        /// </returns>
        IDictionary<long, ISet<EntityRef>> GetEntityTypes(IEnumerable<EntityRef> entityRefs);
    }
}