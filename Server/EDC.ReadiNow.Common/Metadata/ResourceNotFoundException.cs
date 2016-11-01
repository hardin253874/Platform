// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Metadata
{
    /// <summary>
    /// This exception indicates that a resource could not be found.
    /// </summary>
    public class ResourceNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ResourceNotFoundException class.
        /// </summary>
        public ResourceNotFoundException() 
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the ResourceNotFoundException class.
        /// </summary>
        public ResourceNotFoundException(string message) 
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the ResourceNotFoundException class.
        /// </summary>
        public ResourceNotFoundException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}
