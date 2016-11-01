// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when a possible XSRF (Cross Site Request Forgery) is detected.
    /// </summary>
    public class XsrfValidationException : AuthenticationException
    {
        public XsrfValidationException()
            : base("The request has been tampered with. Possible XSRF detected.")
        {
            // Do nothing
        }
    }
}