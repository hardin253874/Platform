// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Connector.EndpointTypes;
using System.Net;

namespace ReadiNow.Connector.Service
{
    /// <summary>
    /// Main processing service.
    /// </summary>
    class ConnectorService : IConnectorService
    {
        private readonly IEndpointResolver _endpointResolver;
        private readonly IEntityRepository _entityRepository;
        private readonly IComponentContext _componentContext;

        public ConnectorService( IEndpointResolver endpointResolver, IEntityRepository entityRepository, IComponentContext componentContext )
        {
            if ( endpointResolver == null )
                throw new ArgumentNullException( "endpointResolver" );
            if ( entityRepository == null )
                throw new ArgumentNullException( "entityRepository" );
            if ( componentContext == null )
                throw new ArgumentNullException( "componentContext" );

            _endpointResolver = endpointResolver;
            _entityRepository = entityRepository;
            _componentContext = componentContext;
        }

        /// <summary>
        /// Main entry point.
        /// </summary>
        public ConnectorResponse HandleRequest( ConnectorRequest request )
        {
            if ( request == null )
                throw new ArgumentNullException( "request" );

            ApiEndpoint endpoint;
            ApiResourceEndpoint resourceEndpoint;
            ApiSpreadsheetEndpoint spreadsheetEndpoint = null;

            using ( new SecurityBypassContext( ) )
            {
                endpoint = GetEndpoint( request );
                if ( endpoint == null )
                {
                    return new ConnectorResponse( HttpStatusCode.NotFound );
                }

                // Check type of endpoint
                resourceEndpoint = endpoint.As<ApiResourceEndpoint>( );

                if ( resourceEndpoint == null )
                    spreadsheetEndpoint = endpoint.As<ApiSpreadsheetEndpoint>( );
            }

            if ( resourceEndpoint != null )
            {
                ResourceEndpoint resourceEndpointService = _componentContext.Resolve<ResourceEndpoint>( );
                return resourceEndpointService.HandleRequest( request, resourceEndpoint );
            }
            if ( spreadsheetEndpoint != null )
            {
                SpreadsheetEndpoint spreadsheetEndpointService = _componentContext.Resolve<SpreadsheetEndpoint>( );
                return spreadsheetEndpointService.HandleRequest( request, spreadsheetEndpoint );
            }
            throw new NotImplementedException( "Unknown ApiEndpoint type" );
        }


		/// <summary>
		/// Processes an exception.
		/// </summary>
		/// <param name="ex">The ex.</param>
		/// <returns>
		/// The response.
		/// </returns>
		/// <exception cref="System.NotSupportedException"></exception>
        public ConnectorResponse HandleException(Exception ex)
        {
            throw new NotSupportedException();
        }

        private ApiEndpoint GetEndpoint( ConnectorRequest request )
        {   
            // Resolve the endpoint entity
            EndpointAddressResult address;
            try
            {
                address = _endpointResolver.ResolveEndpoint( request.ApiPath, false );
            }
            catch ( EndpointNotFoundException )
            {
                return null;    // can't find it, so return null
            }

            // Check the API
            Api apiEntity = _entityRepository.Get<Api>( address.ApiId );
            if ( apiEntity == null )
            {
                return null;
            }

            // Get the endpoint
            ApiEndpoint endpointEntity = _entityRepository.Get<ApiEndpoint>( address.EndpointId );
            return endpointEntity;
        }
    }
}
