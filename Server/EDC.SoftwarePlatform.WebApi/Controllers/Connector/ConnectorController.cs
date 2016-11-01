// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Web.Http;
using EDC.Common;
using EDC.Exceptions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using ReadiNow.Connector;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Connector
{
    /// <summary>
    ///     Controller for APIs exposed to customer applications.
    /// </summary>
    /// <remarks>
    /// - Do not put any application logic in here.
    ///   It should only contain logic to decode requests and format responses.    /// 
    /// - The apiPath argument for each contains the URI as a string, relative to the controller.
    ///   E.g. if adddress is https://server/spapi/api/tenant/myapi/myendpoint then path contains myapi/myendpoint.
    /// </remarks>
    [RoutePrefix( "" )]
    public class ConnectorController : ApiController
    {
        private const string RoutePrefix = "api";

        private const string UploadPrefix = "upload";

        private readonly IDynamicObjectReaderService _objectReaderService;
        private readonly IConnectorService _connectorService;
   

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectorController( )
        {
            // Resolve services
            _objectReaderService = Factory.Current.Resolve<IDynamicObjectReaderService>( );
            _connectorService = Factory.ConnectorService;
        }

        /// <summary>
        /// Handle GET verb
        /// </summary>
        [Route( RoutePrefix + "/{tenant}/{*apiPath}" )]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get( [FromUri] string tenant, [FromUri] string apiPath )
        {
            try
            {
                // Run service
                using ( Profiler.Measure( "ConnectorController.Get" ) )
                {
                    // Prepare request
                    ConnectorRequest connectorRequest = new ConnectorRequest
                    {
                        Verb = ConnectorVerb.Get,
                        ApiPath = GetApiPath( apiPath ),
                        Payload = null,
                        TenantName = tenant,
                        QueryString = GetQueryString( ),
                        ControllerRootPath = GetControllerAddress( )
                    };

                    // Run request
                    ConnectorResponse response = _connectorService.HandleRequest( connectorRequest );

                    // Response
                    return ConvertResponse( response );
                }
            }
            catch ( WebArgumentException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                return UnhandledException( ex );                
            }
        }


        /// <summary>
        /// Handle POST verb
        /// </summary>
        [Route( RoutePrefix + "/{tenant}/{*apiPath}" )]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage Post( [FromUri] string tenant, [FromUri] string apiPath, [FromBody] object request )
        {
            try
            {
                // Run service
                using ( Profiler.Measure( "ConnectorController.Post" ) )
                {
                    // Prepare request
                    ConnectorRequest connectorRequest = new ConnectorRequest
                    {
                        Verb = ConnectorVerb.Post,
                        ApiPath = GetApiPath( apiPath ),
                        Payload = GetPayload( request ),
                        TenantName = tenant,
                        QueryString = GetQueryString( ),
                        ControllerRootPath = GetControllerAddress( )
                    };

                    // Run request
                    ConnectorResponse response = _connectorService.HandleRequest( connectorRequest );

                    // Response
                    return ConvertResponse( response );
                }
            }
            catch ( WebArgumentException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                return UnhandledException( ex );
            }
        }


        /// <summary>
        /// Handle POST verb for file uploads
        /// </summary>
        [Route( UploadPrefix + "/{tenant}/{*apiPath}" )]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Upload( [FromUri] string tenant, [FromUri] string apiPath )
        {
            try
            {
                using ( Profiler.Measure( "ConnectorController.Upload" ) )
                {
                    // Capture file
                    string fileToken;
                    using ( Stream fileStream = await Request.Content.ReadAsStreamAsync( ) )
                    {
                        fileToken = FileRepositoryHelper.AddTemporaryFile( fileStream );
                    }   

                    // Run service
                    // Prepare request
                    ConnectorRequest connectorRequest = new ConnectorRequest
                    {
                        Verb = ConnectorVerb.Post,
                        ApiPath = GetApiPath( apiPath ),
                        Payload = null,
                        TenantName = tenant,
                        QueryString = GetQueryString( ),
                        ControllerRootPath = GetControllerAddress( UploadPrefix ),
                        FileUploadToken = fileToken
                    };

                    // Run request
                    ConnectorResponse response = _connectorService.HandleRequest( connectorRequest );

                    // Response
                    return ConvertResponse( response );
                }
            }
            catch ( WebArgumentException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                return UnhandledException( ex );
            }
        }


        /// <summary>
        /// Handle PUT verb
        /// </summary>
        [Route( RoutePrefix + "/{tenant}/{*apiPath}" )]
        [HttpPut]
        [AllowAnonymous]
        public HttpResponseMessage Put( [FromUri] string tenant, [FromUri] string apiPath, [FromBody] object request )
        {
            try
            {
                // Run service
                using ( Profiler.Measure( "ConnectorController.Put" ) )
                {
                    // Prepare request
                    ConnectorRequest connectorRequest = new ConnectorRequest
                    {
                        Verb = ConnectorVerb.Put,
                        ApiPath = GetApiPath( apiPath ),
                        Payload = GetPayload( request ),
                        TenantName = tenant,
                        QueryString = GetQueryString( ),
                        ControllerRootPath = GetControllerAddress( )
                    };

                    // Run request
                    ConnectorResponse response = _connectorService.HandleRequest( connectorRequest );

                    // Response
                    return ConvertResponse( response );
                }
            }
            catch ( WebArgumentException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                return UnhandledException( ex );
            }
        }


        /// <summary>
        /// Handle DELETE verb
        /// </summary>
        [Route( RoutePrefix + "/{tenant}/{*apiPath}" )]
        [HttpDelete]
        [AllowAnonymous]
        public HttpResponseMessage Delete( [FromUri] string tenant, [FromUri] string apiPath )
        {
            try
            {
                // Run service
                using ( Profiler.Measure( "ConnectorController.Delete" ) )
                {
                    // Prepare request
                    ConnectorRequest connectorRequest = new ConnectorRequest
                    {
                        Verb = ConnectorVerb.Delete,
                        ApiPath = GetApiPath( apiPath ),
                        Payload = null,
                        TenantName = tenant,
                        QueryString = GetQueryString( ),
                        ControllerRootPath = GetControllerAddress( )
                    };

                    // Run request
                    ConnectorResponse response = _connectorService.HandleRequest( connectorRequest );

                    // Response
                    return ConvertResponse( response );
                }
            }
            catch ( WebArgumentException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                return UnhandledException( ex );
            }
        }



        /// <summary>
        /// Wrap body/payload so the service can read it.
        /// </summary>
        private IObjectReader GetPayload( object payload )
        {
            // Cast payload
            IDynamicMetaObjectProvider dynamicProvider = null;
            if ( payload != null )
            {
                dynamicProvider = payload as IDynamicMetaObjectProvider;
                if ( dynamicProvider == null )
                    throw new Exception( "Expected serializer to produce dynamic object" );
            }

            // Read payload
            IObjectReader result = _objectReaderService.GetObjectReader( dynamicProvider );
            return result;                
        }

        /// <summary>
        /// Get the query string parameters.
        /// </summary>
        private IDictionary<string, string> GetQueryString( )
        {
            return Request.RequestUri.ParseQueryString( ).ToDictionary( );
        }

        /// <summary>
        /// Convert response for HTTP.
        /// </summary>
        private HttpResponseMessage ConvertResponse( ConnectorResponse connectorResponse )
        {
            if ( connectorResponse == null )
                throw new ArgumentNullException( "connectorResponse" );

            HttpResponseMessage response = new HttpResponseMessage( connectorResponse.StatusCode );
            if (connectorResponse.MessageResponse != null)
            {
                response.Content = new ObjectContent(connectorResponse.MessageResponse.GetType(), connectorResponse.MessageResponse, HttpResponseMessageFormatter.Formatter);
            }
            if (connectorResponse.Message != null)
            {
                response.Content = new StringContent(connectorResponse.Message);
            }
            if ( connectorResponse.Headers != null )
            {
                foreach ( KeyValuePair<string, string> header in connectorResponse.Headers )
                {
                    response.Headers.Add( header.Key, header.Value );
                }
            }
            return response;
        }

        /// <summary>
        /// Split the API path.
        /// </summary>
        /// <param name="apiPath"></param>
        /// <returns></returns>
        private string [ ] GetApiPath( string apiPath )
        {
            if ( apiPath == null )
                return null;
            return apiPath.Split( '/' );
        }

        /// <summary>
        /// Handle exceptions
        /// </summary>
        /// <param name="ex">The exceptions.</param>
        /// <returns></returns>
        private HttpResponseMessage UnhandledException( Exception ex )
        {
            // Note: the ConnectorService has its own exception layer.
            // This just really processes exceptions that handle inside this controller.

            ConnectorResponse response = _connectorService.HandleException(ex);
            return ConvertResponse(response);
        }

        /// <summary>
        /// Create a path to the controller entry points.
        /// </summary>
        /// <remarks>
        /// For example: https://my.readiapp.com/SpApi/api/
        /// </remarks>
        /// <returns></returns>
        private string GetControllerAddress( string routePrefix = RoutePrefix )
        {
            string scheme = Request.RequestUri.Scheme;              // e.g. "https"
            string host = Request.RequestUri.Host;                  // e.g. "my.readiapp.com"
            string virtualPath = RequestContext.VirtualPathRoot;    // e.g. "/SpApi"

            string address = string.Concat( scheme, "://", host, virtualPath, "/", routePrefix, "/" );
            return address;
        }

    }
}