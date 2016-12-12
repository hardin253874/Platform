// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// Thrown if the caller attempts to load data from the graph entity model that was never requested.
    /// </summary>
    public class DataNotLoadedException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public DataNotLoadedException( string message )
            : base( message )
        {
        }
    }
}
