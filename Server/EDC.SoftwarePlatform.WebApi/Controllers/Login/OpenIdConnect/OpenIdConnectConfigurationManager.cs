// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect
{
    /// <summary>
    /// Class OpenIdConnectConfigurationManager.
    /// </summary>
    /// <seealso cref="EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect.IOpenIdConnectConfigurationManager" />
    public class OpenIdConnectConfigurationManager : IOpenIdConnectConfigurationManager
    {
        private readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>
            _configurationManagers =
                new ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();

        /// <summary>
        /// Gets the identity provider configuration as an asynchronous operation.
        /// </summary>
        /// <param name="configurationUrl">The configuration URL.</param>
        /// <returns>Task&lt;OpenIdConnectConfiguration&gt;.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task<OpenIdConnectConfiguration> GetIdentityProviderConfigurationAsync(string configurationUrl)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            var configurationManager = _configurationManagers.GetOrAdd(configurationUrl.ToLowerInvariant(),
                new ConfigurationManager<OpenIdConnectConfiguration>(configurationUrl, new HttpClient())
                {
                    AutomaticRefreshInterval = TimeSpan.FromHours(12)
                });

            return await configurationManager.GetConfigurationAsync();
        }

        /// <summary>
        /// Removes the configuration for the specified identity provider.
        /// </summary>
        /// <param name="configurationUrl">The configuration URL.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void RemoveIdentityProviderConfiguration(string configurationUrl)
        {
            if (string.IsNullOrWhiteSpace(configurationUrl))
            {
                throw new ArgumentNullException(nameof(configurationUrl));
            }

            ConfigurationManager<OpenIdConnectConfiguration> configurationManager;

            _configurationManagers.TryRemove(configurationUrl.ToLowerInvariant(), out configurationManager);
        }
    }
}