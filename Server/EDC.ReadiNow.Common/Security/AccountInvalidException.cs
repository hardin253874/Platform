// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when the user attempts to authenticate with an account that does not exist or is otherwise invalid.
    /// </summary>
    public class AccountInvalidException : AuthenticationException
    {
        public AccountInvalidException(string message) : base("The specified account is invalid: " + message) { }
    }
}