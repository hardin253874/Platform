// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Exceptions
{
    /// <summary>
    /// Wrapper around ArgumentException for identifying problems with arguments passed to web services.
    /// That is, the external client passed incorrect data, rather than an internal call passing incorrect data.
    /// </summary>
    public class WebArgumentException : ArgumentException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WebArgumentException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public WebArgumentException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public WebArgumentException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="paramName">Parameter name</param>
        public WebArgumentException(string message, string paramName) : base(message, paramName) { }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="paramName">Parameter name</param>
        /// <param name="innerException">Inner exception</param>
        public WebArgumentException(string message, string paramName, Exception innerException) : base(message, paramName, innerException) { }
    }
}
