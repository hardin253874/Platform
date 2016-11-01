// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Thrown when a user attempts to authenticate with a disabled account.
    /// </summary>
    public class TenantDisabledException : AuthenticationException
    {
        /// <summary>
        /// Create a new <see cref="TenantDisabledException"/>
        /// </summary>
        /// <param name="tenantName">
        /// The name of the disabled tenant. This cannot be null or empty.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tenantName"/> cannot be null or empty.
        /// </exception>
        public TenantDisabledException(string tenantName)
            : base(string.Format("Tenant '{0}' is disabled", tenantName ?? "(null)"))
        {
            if (string.IsNullOrEmpty(tenantName))
            {
                throw new ArgumentNullException("tenantName");
            }

            TenantName = tenantName;
        }

        /// <summary>
        /// The tenant name.
        /// </summary>
        public string TenantName { get; private set; }
    }
}