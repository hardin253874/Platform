// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Annotations;

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Resolves the endpoint resource that backs an API URI.
    /// </summary>
    public interface IEndpointResolver
    {
        /// <summary>
        /// Determine the API and Endpoint entities that an apiPath is referring to.
        /// </summary>
        /// <param name="apiPath">The relative API, relative to the API controller address.</param>
        /// <param name="apiOnly">If true, only resolve the API portion.</param>
        /// <returns>Entity IDs of the API and Endpoint. Returns ID of zero for items that were absent from path.</returns>
        /// <exception cref="ConnectorConfigException">
        /// Thrown if there is some configuration problem, such as two endpoints with the requested name.
        /// </exception>
        /// <exception cref="EndpointNotFoundException">
        /// Thrown if no API or endpoint matches the requested path.
        /// </exception>
        [NotNull]
        EndpointAddressResult ResolveEndpoint( string[] apiPath, bool apiOnly );
    }


    /// <summary>
    /// A result from resolving an API URI.
    /// </summary>
    public class EndpointAddressResult : Tuple<long, long>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiId"></param>
        /// <param name="endpointId"></param>
        public EndpointAddressResult( long apiId, long endpointId ) : base(apiId, endpointId)
        {
        }

        /// <summary>
        /// The entity ID of the API.
        /// </summary>
        /// <remarks>
        /// Zero if no API was identified.
        /// </remarks>
        public long ApiId
        {
            get { return Item1; }
        }

        /// <summary>
        /// The entity ID of the endpoint.
        /// </summary>
        /// <remarks>
        /// Zero if no endpoint was identified.
        /// </remarks>
        public long EndpointId
        {
            get { return Item2; }
        }
    }


    /// <summary>
    /// Exception.
    /// </summary>
    public class EndpointNotFoundException : ConnectorRequestException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public EndpointNotFoundException( string message )
            : base( message )
        {
        }
    }

}
