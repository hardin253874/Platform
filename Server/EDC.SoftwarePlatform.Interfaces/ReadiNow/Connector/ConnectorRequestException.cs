// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Exception that is thrown if there is some problem with the way that connector has been called by the
    /// application author or tenant administrator.
    /// </summary>
    public class ConnectorRequestException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="message">This message will get returned to the caller.</param>
        public ConnectorRequestException(string message)
            : base(message)
        {
        }
    }
}
