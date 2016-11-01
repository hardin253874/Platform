// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Annotations;
using EDC.ReadiNow.Model;

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Uri Factory
    /// </summary>
    public interface IResourceUriGenerator
    {
        /// <summary>
        /// Generates a URI for an instance.
        /// </summary>
        /// <param name="instance">The instance to create the URI for.</param>
        /// <param name="request">The request</param>
        /// <param name="mapping">The mapping object</param>
        /// <returns>The URI</returns>
        [NotNull]
        string CreateResourceUri( [NotNull] IEntity instance, [NotNull] ConnectorRequest request, [NotNull] ApiResourceMapping mapping );
    }
}
