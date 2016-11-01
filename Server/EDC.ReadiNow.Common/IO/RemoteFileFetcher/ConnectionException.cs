// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;


namespace EDC.ReadiNow.IO.RemoteFileFetcher
{
    /// <summary>
    /// An exception caused by failure to connect to download a file.
    /// </summary>
    public class ConnectionException : Exception
    {
        public ConnectionException(string details, Exception ex = null) : base(details, ex)
        { }
    }
}
