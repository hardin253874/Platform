// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when a user attempts to authentication with an expired authentication token.
    /// </summary>
    public class AuthenticationTokenExpiredException : AuthenticationException
    {
        public AuthenticationTokenExpiredException() : base("Your account has locked due to inactivity, please re-enter your password to continue.") { }
    }
}