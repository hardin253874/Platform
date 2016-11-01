// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect
{
    /// <summary>
    ///     Interface IOpenIdConnectConfigurationManager
    /// </summary>
    public interface IOpenIdConnectConfigurationManager
    {
        /// <summary>
        ///     Gets the identity provider configuration asynchronously.
        /// </summary>
        /// <param name="configurationUrl">The configuration URL.</param>
        /// <returns>Task&lt;OpenIdConnectConfiguration&gt;.</returns>
        Task<OpenIdConnectConfiguration> GetIdentityProviderConfigurationAsync(string configurationUrl);

        /// <summary>
        /// Removes the configuration for the specified identity provider.
        /// </summary>
        /// <param name="configurationUrl">The configuration URL.</param>
        void RemoveIdentityProviderConfiguration(string configurationUrl);
    }
}