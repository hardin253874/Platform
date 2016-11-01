// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Exceptions
{
    /// <summary>
    /// Wrapper around ArgumentNullException for identifying problems with arguments passed to web services.
    /// That is, the external client passed incorrect data, rather than an internal call passing incorrect data.
    /// </summary>
    public class WebArgumentNullException : WebArgumentException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WebArgumentNullException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public WebArgumentNullException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public WebArgumentNullException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="message">Exception message</param>
        public WebArgumentNullException(string paramName, string message) : base(message, paramName) { }
        // Note: .Net ArgumentException and ArgumentOutOfRangeException have different parameter orders for this constructor.
    }
}
