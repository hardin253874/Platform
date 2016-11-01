// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Exception that is thrown if there is some problem with the way that connector has been configured by the
    /// application author or tenant administrator.
    /// </summary>
    public class ConnectorConfigException : Exception
    {
        public ConnectorConfigException( string message )
            : base( message )
        {
        }
    }
}
