// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// An interface for finding entities.
    /// </summary>
    /// <remarks>
    /// Information about what type of entity to find, and what type of field to search is captured in the resolver instance.
    /// Use <see cref="IEntityResolverProvider"/> to specify these.
    /// </remarks>
    public interface IEntityResolver
    {
        /// <summary>
        /// Find entities with a field of a particular value.
        /// </summary>
        /// <param name="fieldValue">The values to find.</param>
        /// <returns>Dictionary matching field values to one or more entities that were found. N, or null if none were found.</returns>
        [NotNull]
        ILookup<object, long> GetEntitiesByField( [NotNull] IReadOnlyCollection<object> fieldValue );
    }
}
