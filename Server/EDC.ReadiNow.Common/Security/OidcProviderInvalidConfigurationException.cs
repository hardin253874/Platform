// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Security.Authentication;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    ///     Thrown when the identity provider configuration is invalid.
    /// </summary>
    public class OidcProviderInvalidConfigurationException : AuthenticationException
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public OidcProviderInvalidConfigurationException() : this(null)
        {
        }


        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="innerException"></param>
        public OidcProviderInvalidConfigurationException(Exception innerException) : base(
            "The identity provider configuration appears to be invalid, please contact your administrator.", innerException)
        {
        }
    }
}