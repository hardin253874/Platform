// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Annotations;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Top level interface that represents the connector service.
    /// </summary>
    public interface IConnectorService
    {
        /// <summary>
        /// Processes an API request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>The response.</returns>
        [NotNull]
        ConnectorResponse HandleRequest( [NotNull] ConnectorRequest request );

        /// <summary>
        /// Processes an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The response.</returns>
        [NotNull]
        ConnectorResponse HandleException( [NotNull] Exception exception );
    }
}
