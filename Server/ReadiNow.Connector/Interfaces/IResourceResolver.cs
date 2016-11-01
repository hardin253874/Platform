// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Annotations;
using EDC.ReadiNow.Model;

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Determines a web resource from a name and context
    /// </summary>
    public interface IResourceResolver
    {
		/// <summary>
		/// Look up an entity according to its identifier.
		/// </summary>
		/// <param name="identity">The identity.</param>
		/// <returns>
		/// The entity
		/// </returns>
        ResourceResolverEntry ResolveResource( [NotNull] string identity );

        /// <summary>
        /// Look up multiple entities by their identifiers.
        /// </summary>
        /// <param name="identities">The identities.</param>
        /// <returns>
        /// The entity
        /// </returns>
        [NotNull]
        IDictionary<object, ResourceResolverEntry> ResolveResources( [NotNull] IReadOnlyCollection<object> identities );
    }

    /// <summary>
    /// Holds the result of an individual lookup.
    /// </summary>
    public struct ResourceResolverEntry
    {
        /// <summary>
        /// Construcot
        /// </summary>
        /// <param name="entity">The entity.</param>
        public ResourceResolverEntry( IEntity entity )
        {
            Entity = entity;
            Error = ResourceResolverError.None;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="error">The error</param>
        public ResourceResolverEntry( ResourceResolverError error )
        {
            Entity = null;
            Error = error;
        }

        public IEntity Entity { get; }

        public ResourceResolverError Error { get; }

    }

    public enum ResourceResolverError
    {
        /// <summary>
        /// There is no error.
        /// </summary>
        None,

        ResourceNotFoundByField,

        ResourceNotUniqueByField,

        ResourceNotFoundByGuid
    }
}
