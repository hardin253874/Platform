// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.Exceptions
{
    /// <summary>
    /// For identifying problems with arguments passed to web services.
    /// Throw this when the *client* requests a resource that does not exist.
    /// That is, the external client passed incorrect data, rather than an internal call passing incorrect data.
    /// This will result in a HTTP 404 NotFound response code.
    /// </summary>
    public class WebArgumentNotFoundException : WebArgumentException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WebArgumentNotFoundException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public WebArgumentNotFoundException(string message) : base(message) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public WebArgumentNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="message">Exception message</param>
        public WebArgumentNotFoundException(string paramName, string message) : base(message, paramName) { }
        // Note: .Net ArgumentException and ArgumentOutOfRangeException have different parameter orders for this constructor.
    }
}
