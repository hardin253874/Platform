// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Net;
using EDC.ReadiNow.Diagnostics;
using EDC.Exceptions;

namespace ReadiNow.Connector.Service
{
    /// <summary>
    /// Layer that applies API Key security model to requests
    /// </summary>
    class ExceptionServiceLayer : IConnectorService
    {
        private readonly IConnectorService _innerService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerService">The service that will be wrapped and called by this service.</param>
        public ExceptionServiceLayer(IConnectorService innerService)
        {
            if (innerService == null)
                throw new ArgumentNullException("innerService");

            _innerService = innerService;
        }

        /// <summary>
        /// Inner service, for diagnostics.
        /// </summary>
        internal IConnectorService InnerService
        {
            get { return _innerService; }
        }

        /// <summary>
        /// Extract authentication information (tenant & API key) from the request.
        /// Run the inner request in the context of the identified user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public ConnectorResponse HandleRequest(ConnectorRequest request)
        {
            try
            {
                return _innerService.HandleRequest(request);
            }
            catch (WebArgumentException)
            {
                // For now, just let these ones pass through for the general ExceptionFilter to handle.
                throw;
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        /// <summary>
        /// Processes an exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>The response.</returns>
        public ConnectorResponse HandleException(Exception ex)
        {
            HttpStatusCode httpCode;
            string platformCode = null;
            string message = null;

            if (ex is ConnectorRequestException)
            {
                EventLog.Application.WriteTrace("Connector: Bad request by caller: " + ex.ToString());
                httpCode = HttpStatusCode.BadRequest;
                message = ex.Message;
            }
            else if (ex is ConnectorConfigException)
            {
                EventLog.Application.WriteWarning("Connector: Tenant config error " + ex.ToString());
                httpCode = HttpStatusCode.InternalServerError;
                message = ex.Message;
            }
            else
            {
                EventLog.Application.WriteError("Connector: Unhandled internal exception: " + ex.ToString());
                httpCode = HttpStatusCode.InternalServerError;
                message = Messages.PlatformInternalError;
            }

            // Extract error code from message (TODO: Make it an exception parameter)
            platformCode = Messages.GetPlatformCode( message );
            if ( platformCode != null )
            {
                message = Messages.GetErrorText( message );
            }

            ConnectorResponse response = new ConnectorResponse(httpCode);
            response.MessageResponse = new MessageResponse
            {
                PlatformMessageCode = platformCode,
                Message = message
            };

            return response;
        }
    }
}
