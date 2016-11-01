// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when a user attempts to authentication with an expired account.
    /// </summary>
    public class AccountExpiredException : AuthenticationException
    {
        public AccountExpiredException() : base("Your account has expired, please contact your administrator.") { }
    }
}