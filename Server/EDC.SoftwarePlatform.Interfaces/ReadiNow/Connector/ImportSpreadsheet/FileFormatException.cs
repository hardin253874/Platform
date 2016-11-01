// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Exception for an invalid file.
    /// </summary>
    public class FileFormatException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The error message.</param>
        public FileFormatException( string message ) : base( message )
        {
        }
    }
}
