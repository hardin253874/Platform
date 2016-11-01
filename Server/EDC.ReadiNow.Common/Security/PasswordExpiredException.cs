// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when a user attempts to authenticate with an expired password.
    /// </summary>
    public class PasswordExpiredException : AuthenticationException
    {
        public PasswordExpiredException() : base("Your password has expired. You must change your password before signing in.") { }
    }
}
