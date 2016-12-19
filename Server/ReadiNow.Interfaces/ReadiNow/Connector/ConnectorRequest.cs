// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Connector
{
    /// <summary>
    /// The HTTP verb being executed.
    /// </summary>
    public enum ConnectorVerb
    {
        Get,
        Post,
        Put,
        Delete
    }

    /// <summary>
    /// The request going into the connector service.
    /// </summary>
    public class ConnectorRequest
    {
        /// <summary>
        /// The root path to the controller. E.g. https://my.readiapp.com/SpApi/api/
        /// </summary>
        public string ControllerRootPath { get; set; }

        /// <summary>
        /// The tenant name in the URI.
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// The endpoint path relative to the root API controller.
        /// E.g. "crm/customer"
        /// </summary>
        public string[] ApiPath { get; set; }

        /// <summary>
        /// HTTP verb associated with this request.
        /// </summary>
        public ConnectorVerb Verb { get; set; }

        /// <summary>
        /// Data associated with this request, or null.
        /// </summary>
        public IObjectReader Payload { get; set; }

        /// <summary>
        /// Data associated with this request, or null.
        /// </summary>
        public IDictionary<string, string> QueryString { get; set; }

        /// <summary>
        /// Token of temporary file that was uploaded.
        /// </summary>
        public string FileUploadToken { get; set; }
    }
}
