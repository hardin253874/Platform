// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Metadata
{
    /// <summary>
    /// Raised if the user attempts to save a relationship that would result in a disallowed circular dependency.
    /// For example, if a structure level is moved to be under itself, or one of its descendants.
    /// </summary>
    public class CircularRelationshipException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the CircularRelationshipException class.
        /// </summary>
        public CircularRelationshipException() 
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the CircularRelationshipException class.
        /// </summary>
        public CircularRelationshipException(string message) 
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the CircularRelationshipException class.
        /// </summary>
        public CircularRelationshipException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}
