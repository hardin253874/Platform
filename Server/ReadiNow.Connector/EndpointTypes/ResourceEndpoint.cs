// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Net;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Service;
using EDC.ReadiNow.Common.Workflow;
using EDC.Exceptions;
using ReadiNow.Connector.Processing;

namespace ReadiNow.Connector.EndpointTypes
{
    /// <summary>
    /// Represents an endpoint for performing CRUD operations on resources.
    /// </summary>
    class ResourceEndpoint
    {
        private readonly IReaderToEntityAdapterProvider _readerToEntityAdapterProvider;
        private readonly IResourceResolverProvider _resourceResolverProvider;
        private readonly IResourceUriGenerator _resourceUriGenerator;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerToEntityAdapterProvider">The reader to entity adapter provider.</param>
		/// <param name="resourceResolverProvider">The resource resolver provider.</param>
		/// <param name="resourceUriGenerator">The resource URI generator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// readerToEntityAdapterProvider
		/// or
		/// resourceResolverProvider
		/// or
		/// resourceUriGenerator
		/// </exception>
        public ResourceEndpoint( IReaderToEntityAdapterProvider readerToEntityAdapterProvider, IResourceResolverProvider resourceResolverProvider, IResourceUriGenerator resourceUriGenerator )
        {
            if ( readerToEntityAdapterProvider == null )
                throw new ArgumentNullException( "readerToEntityAdapterProvider" );
            if ( resourceResolverProvider == null )
                throw new ArgumentNullException( "resourceResolverProvider" );
            if ( resourceUriGenerator == null )
                throw new ArgumentNullException( "resourceUriGenerator" );
            
            _readerToEntityAdapterProvider = readerToEntityAdapterProvider;
            _resourceResolverProvider = resourceResolverProvider;
            _resourceUriGenerator = resourceUriGenerator;
        }

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <remarks>
        /// Assumes that user context has already been set.
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public ConnectorResponse HandleRequest( ConnectorRequest request, ApiResourceEndpoint endpoint )
        {
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( endpoint == null )
                throw new ArgumentNullException( "endpoint" );

            // Get resource mapping object
            ApiResourceMapping mapping = GetResourceMapping( endpoint );

            // Disable triggers if applicable
            bool disableTriggers;
            using (new SecurityBypassContext())
            { 
                disableTriggers = mapping.MappingSuppressWorkflows == true;
            }

            using (new WorkflowRunContext { DisableTriggers = disableTriggers })
            {
                // Handle verbs
                if (request.Verb == ConnectorVerb.Post && SecurityBypassContext.Elevate(() => endpoint.EndpointCanCreate == true))
                {
                    return HandlePost(request, mapping);
                }

                if (request.Verb == ConnectorVerb.Put && SecurityBypassContext.Elevate(() => endpoint.EndpointCanUpdate == true))
                {
                    return HandlePut(request, mapping);
                }

                if (request.Verb == ConnectorVerb.Delete && SecurityBypassContext.Elevate(() => endpoint.EndpointCanDelete == true))
                {
                    return HandleDelete(request, mapping);
                }

                return new ConnectorResponse(HttpStatusCode.MethodNotAllowed);
            }
        }

        /// <summary>
        /// Create
        /// </summary>
        private ConnectorResponse HandlePost( ConnectorRequest request, ApiResourceMapping mapping )
        {
            IReaderToEntityAdapter adapter = GetReaderAdapter( mapping );
            IObjectReader reader = request.Payload;

            // Check payload
            CheckPayload(reader);

            // Create instance
            IEntity instance = adapter.CreateEntity( reader, ConnectorRequestExceptionReporter.Instance );
            SaveInstance( instance );

            // Create URI to identify instance
            string instanceUri = _resourceUriGenerator.CreateResourceUri( instance, request, mapping );

            var response = new ConnectorResponse( HttpStatusCode.Created, "Created" );
            response.Headers.Add( "Location", instanceUri );
            return response;
        }

        /// <summary>
        /// Update
        /// </summary>
        private ConnectorResponse HandlePut( ConnectorRequest request, ApiResourceMapping mapping )
        {
            IReaderToEntityAdapter adapter = GetReaderAdapter( mapping );
            IObjectReader reader = request.Payload;
            IEntity instance;

            // Check payload
            CheckPayload( reader );

            // Get instance
            ConnectorResponse errorResponse;
            instance = LocateResource( request, mapping, out errorResponse );
            if ( errorResponse != null )
                return errorResponse;

            // Modify instance
            instance = instance.AsWritable( );
            adapter.FillEntity( reader, instance, ConnectorRequestExceptionReporter.Instance );
            SaveInstance( instance );

            return new ConnectorResponse( HttpStatusCode.OK, "Updated" );
        }

        /// <summary>
        /// Delete
        /// </summary>
        private ConnectorResponse HandleDelete( ConnectorRequest request, ApiResourceMapping mapping )
        {
            IEntity instance;

            // Get instance
            ConnectorResponse errorResponse;
            instance = LocateResource( request, mapping, out errorResponse );
            if ( errorResponse != null )
                return errorResponse;

            // Delete instance
            instance = instance.AsWritable( );
            instance.Delete( );

            return new ConnectorResponse( HttpStatusCode.OK, "Deleted" );
        }

        /// <summary>
        /// Check that the payload has some content.
        /// </summary>
        /// <param name="reader"></param>
        private static void CheckPayload(IObjectReader reader)
        {
            if (reader == null || !reader.GetKeys().Any())
                throw new ConnectorRequestException( Messages.EmptyMessageBody );
        }

        /// <summary>
        /// Handle saving the entity
        /// </summary>
        /// <param name="instance"></param>
        private static void SaveInstance( IEntity instance )
        {
            try
            {
                instance.Save( );
            }
            catch ( CardinalityViolationException )
            {
                throw new ConnectorRequestException( Messages.CardinalityViolation );
            }
            catch ( ValidationException )
            {
                throw new ConnectorRequestException( Messages.FieldValidation );
            }
        }

        private ApiResourceMapping GetResourceMapping( ApiResourceEndpoint endpoint )
        {
            using ( new SecurityBypassContext( ) )
            {
                ApiResourceMapping mapping = endpoint.EndpointResourceMapping;            

                if ( mapping == null )
                    throw new ConnectorConfigException( Messages.EndpointHasNoResourceMapping );

                if ( mapping.MappedType == null )
                    throw new ConnectorConfigException( Messages.ResourceMappingHasNoType );

                return mapping;
            }
        }

        private IReaderToEntityAdapter GetReaderAdapter( ApiResourceMapping mapping )
        {
            using ( new SecurityBypassContext( ) )
            {
                var settings = new ReaderToEntityAdapterSettings( );

                // Get adapter
                IReaderToEntityAdapter adapter = _readerToEntityAdapterProvider.GetAdapter( mapping.Id, settings );

                return adapter;
            }
        }

        private IEntity LocateResource( ConnectorRequest request, ApiResourceMapping mapping, out ConnectorResponse errorResponse )
        {
            // Check path
            if ( request.ApiPath == null || request.ApiPath.Length < 3 )
            {
                errorResponse = new ConnectorResponse( HttpStatusCode.MethodNotAllowed );
                return null;
            }

            if ( request.ApiPath.Length > 3 )
            {
                errorResponse = new ConnectorResponse( HttpStatusCode.NotFound );
                return null;
            }

            // Get resource ID field value (i.e. the string being used to locate the resource, e.g. name)
            string resourceIdFieldValue = request.ApiPath [ 2 ];
            if ( string.IsNullOrEmpty( resourceIdFieldValue ) )
                throw new Exception( "Empty resource identity was received." ); // assert false .. this should be blocked earlier.

            // Resolve resource from string
            IResourceResolver resolver = _resourceResolverProvider.GetResolverForResourceMapping( mapping );
            ResourceResolverEntry resolveResult = resolver.ResolveResource( resourceIdFieldValue );
            if ( resolveResult.Entity == null )
            {
                throw new WebArgumentNotFoundException( );   // Resource could not be located, so ensure a 404 is thrown
            }
            errorResponse = null;
            return resolveResult.Entity;
        }

    }
}
