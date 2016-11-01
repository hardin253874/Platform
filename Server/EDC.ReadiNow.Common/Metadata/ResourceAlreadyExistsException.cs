// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Metadata
{
    /// <summary>
    /// This exception indicates that a resource already exists.
    /// </summary>
    public class ResourceAlreadyExistsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ResourceAlreadyExistsException class.
        /// </summary>
        public ResourceAlreadyExistsException() 
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the ResourceAlreadyExistsException class.
        /// </summary>
        public ResourceAlreadyExistsException(string message) 
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the ResourceAlreadyExistsException class.
        /// </summary>
        public ResourceAlreadyExistsException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}
