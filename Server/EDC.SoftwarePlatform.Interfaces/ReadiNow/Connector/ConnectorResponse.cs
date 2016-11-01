// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Net;

namespace ReadiNow.Connector
{
    /// <summary>
    /// The response coming back from the connector service.
    /// </summary>
    public class ConnectorResponse
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectorResponse( ) : this( HttpStatusCode.OK )
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectorResponse( HttpStatusCode statusCode, string message = null )
        {
            StatusCode = statusCode;
            Message = message;
            Headers = new Dictionary<string, string>( );
        }

        /// <summary>
        /// The HTTP status code to respond with.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// The HTTP message to respond with.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The object that will be serialized as part of a JSON response.
        /// </summary>
        public object Response { get; private set; }

        /// <summary>
        /// The object that will be serialized as part of a JSON response.
        /// </summary>
        public MessageResponse MessageResponse
        {
            get { return Response as MessageResponse; }
            set { Response = value; }
        }

        /// <summary>
        /// The HTTP message to respond with.
        /// </summary>
        public Dictionary<string, string> Headers { get; private set; }
    }
}
