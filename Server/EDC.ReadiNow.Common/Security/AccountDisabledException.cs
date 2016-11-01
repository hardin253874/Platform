// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when a user attempts to authenticate with a disabled account.
    /// </summary>
    public class AccountDisabledException : AuthenticationException
    {
        public AccountDisabledException() : base("Your account has been disabled, please contact your administrator.") { }
    }
}