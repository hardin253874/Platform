// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect
{
    /// <summary>
    ///     Class OpenIdConnectRequestContextResult.
    /// </summary>
    public class OpenIdConnectRequestContextResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdConnectRequestContextResult" /> class.
        /// </summary>
        /// <param name="requestContextData">The request context data.</param>
        /// <param name="identityProviderUserName">Name of the identity provider user.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public OpenIdConnectRequestContextResult(RequestContextData requestContextData, string identityProviderUserName)
        {
            if (requestContextData == null)
            {
                throw new ArgumentNullException(nameof(requestContextData));
            }

            if (string.IsNullOrWhiteSpace(identityProviderUserName))
            {
                throw new ArgumentNullException(nameof(identityProviderUserName));
            }

            RequestContextData = requestContextData;
            IdentityProviderUserName = identityProviderUserName;
        }

        /// <summary>
        ///     Gets the request context data.
        /// </summary>
        /// <value>The request context data.</value>
        public RequestContextData RequestContextData { get; }

        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string IdentityProviderUserName { get; }
    }
}