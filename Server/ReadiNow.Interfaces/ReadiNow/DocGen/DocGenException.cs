// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Exception thrown during the processing of anXmlReader
    /// </summary>
    public class DocGenException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message.</param>
        public DocGenException(string message) : base(message)
        {
        }
    }
}
