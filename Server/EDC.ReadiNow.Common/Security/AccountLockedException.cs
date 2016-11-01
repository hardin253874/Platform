// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when a user attempts to authenticate with a locked account.
    /// </summary>
    public class AccountLockedException : AuthenticationException
    {
        public AccountLockedException(int lockoutMinutes) : base(string.Format("Your account has been locked due to too many failed attempts to log in. Wait {0} minutes before trying again.", lockoutMinutes)) { }
    }
}