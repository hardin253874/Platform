// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Interface to create a resource resolver.
    /// </summary>
    public interface IResourceResolverProvider
    {
        /// <summary>
        /// Creates a resolver that can look up an entity according to its identifier and any rules specified in the mapping resource.
        /// </summary>
        /// <param name="mapping">The mapping rules.</param>
        /// <returns>The entity</returns>
        [NotNull]
        IResourceResolver GetResolverForResourceMapping( [NotNull] ApiResourceMapping mapping );

        /// <summary>
        /// Creates a resolver that can look up an entity according to its identifier and any rules specified in a relationship mapping.
        /// </summary>
        /// <param name="mapping">The mapping rules.</param>
        /// <returns>The entity</returns>
        IResourceResolver GetResolverForRelationshipMapping( ApiRelationshipMapping mapping );

        /// <summary>
        /// Create a resolver that can look up entities of a particular type by name or GUID.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <returns>
        /// The entity
        /// </returns>
        [NotNull]
        IResourceResolver GetResolverForType( long typeId );
    }
}
