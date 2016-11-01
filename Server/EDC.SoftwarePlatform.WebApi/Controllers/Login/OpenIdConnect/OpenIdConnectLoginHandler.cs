// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AuditLog;
using EDC.Security;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using Jil;
using Microsoft.IdentityModel.Protocols;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect
{
    /// <summary>
    ///     This class is an Open Id Connect Login Handler.
    /// </summary>
    public class OpenIdConnectLoginHandler
    {
        private readonly IOpenIdConnectConfigurationManager _configurationManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenIdConnectLoginHandler" /> class.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="disableTokenValidation">if set to <c>true</c> [disable token validation]. Only to be used by unit tests.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public OpenIdConnectLoginHandler(IOpenIdConnectConfigurationManager configurationManager, bool disableTokenValidation = false)
        {
            if (configurationManager == null)
            {
                throw new ArgumentNullException(nameof(configurationManager));
            }

            _configurationManager = configurationManager;
            IsTokenValidationDisabled = disableTokenValidation;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is token validation disabled.
        /// </summary>
        /// <value><c>true</c> if this instance is token validation disabled; otherwise, <c>false</c>.</value>
        public bool IsTokenValidationDisabled { get; }

        /// <summary>
        /// Gets the open id provider fields to load.
        /// </summary>
        /// <returns></returns>
        private IEntityRef[] GetOidcProviderFieldsToLoad()
        {
            return new IEntityRef[]
               {
                    new EntityRef("core:oidcClientId"),
                    new EntityRef("core:oidcIdentityProviderConfigurationUrl"),
                    new EntityRef("core:oidcClientSecretSecureId"),
                    new EntityRef("core:oidcUserIdentityClaim"),
                    new EntityRef("core:oidcAlwaysPrompt"),
                    new EntityRef("core:isProviderEnabled")
               };
        }

        /// <summary>
        ///     Gets the authorization code request URL for the specified identity provider.
        /// </summary>
        /// <param name="idpLoginRequest">The oidc provider.</param>
        /// <param name="requestBaseUrl">The base url message.</param>
        /// <returns>Task&lt;Uri&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public async Task<Uri> GetAuthorizationCodeRequestUrl(IdentityProviderLoginRequest idpLoginRequest, Uri requestBaseUrl)
        {
            if (idpLoginRequest == null)
            {
                throw new ArgumentNullException(nameof(idpLoginRequest));
            }

            if (requestBaseUrl == null)
            {
                throw new ArgumentNullException(nameof(requestBaseUrl));
            }

            if (string.IsNullOrWhiteSpace(idpLoginRequest.Tenant))
            {
                throw new ArgumentException(@"The tenant is invalid.", nameof(idpLoginRequest));
            }

            if (idpLoginRequest.IdentityProviderId <= 0)
            {
                throw new ArgumentException(@"The identity provider is invalid.", nameof(idpLoginRequest));
            }

            if (string.IsNullOrWhiteSpace(idpLoginRequest.RedirectUrl))
            {
                throw new ArgumentException(@"The redirect url is invalid.", nameof(idpLoginRequest));
            }

            long tenantId;
            long oidcProviderId;
            string oidcConfigurationUrl;
            string oidcClientId;
            bool alwaysPrompt;

            using (new SecurityBypassContext())
            using (new TenantAdministratorContext(idpLoginRequest.Tenant))
            {                
                var oidcIdentityProvider = ReadiNow.Model.Entity.Get<OidcIdentityProvider>(idpLoginRequest.IdentityProviderId, GetOidcProviderFieldsToLoad());

                if (oidcIdentityProvider == null)
                {
                    throw new AuthenticationException("The identity provider does not exist.");
                }

                ValidateOidcProviderFields(oidcIdentityProvider);

                // Store any required entity model fields upfront.
                // Any code running after the await statement may run a different thread
                tenantId = RequestContext.TenantId;
                oidcProviderId = oidcIdentityProvider.Id;
                oidcClientId = oidcIdentityProvider.OidcClientId;
                oidcConfigurationUrl = oidcIdentityProvider.OidcIdentityProviderConfigurationUrl;
                alwaysPrompt = oidcIdentityProvider.OidcAlwaysPrompt ?? true;
            }

            OpenIdConnectConfiguration oidcConfig;

            try
            {
                // Get the configuration
                oidcConfig = await _configurationManager.GetIdentityProviderConfigurationAsync(oidcConfigurationUrl);
            }
            catch(Exception ex)
            {
                throw new OidcProviderInvalidConfigurationException(ex);
            }

            var oidcProtocolValidator = new OpenIdConnectProtocolValidator();

            // Create authorization state
            var authStateObject = new OpenIdConnectAuthorizationState
            {
                Timestamp = DateTime.UtcNow.Ticks,
                RedirectUrl = idpLoginRequest.RedirectUrl,
                IdentityProviderId = oidcProviderId,
                TenantId = tenantId,
                Nonce = oidcProtocolValidator.GenerateNonce()
            };

            // Serialize and encrypt state
            var stateJson = JSON.Serialize(authStateObject);
            var cryptoProvider = new EncodingCryptoProvider();
            var encryptedState = cryptoProvider.EncryptAndEncode(stateJson);

            // Create code request oidc message
            var oidcMessage = new OpenIdConnectMessage
            {
                ClientId = oidcClientId,
                IssuerAddress = oidcConfig.AuthorizationEndpoint,
                RedirectUri = GetOidcAuthResponseUri(requestBaseUrl, idpLoginRequest.Tenant),
                Scope = "openid email",
                ResponseType = "code",
                State = encryptedState,
                Nonce = authStateObject.Nonce,
                Prompt = alwaysPrompt ? "login" : null
            };

            // Get request url
            return new Uri(oidcMessage.CreateAuthenticationRequestUrl());
        }


        /// <summary>
        ///     Gets the authorization tokens.
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="code">The code.</param>
        /// <param name="oidcClientSecretSecureId">The oidc client secret.</param>        
        /// <param name="oidcClientId">The oidc client identifier.</param>
        /// <param name="requestBaseUrl">The request base URL.</param>
        /// <param name="oidcConfig">The oidc configuration.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>Task&lt;OpenIdConnectAuthorizationTokenResponse&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="AuthenticationException">
        ///     An error occurred getting the authorization tokens.
        ///     or
        /// </exception>
        private async Task<OpenIdConnectAuthorizationTokenResponse> GetAuthorizationTokens(string tenant, string code, Guid? oidcClientSecretSecureId, string oidcClientId, Uri requestBaseUrl,
            OpenIdConnectConfiguration oidcConfig, IHttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (oidcClientSecretSecureId == null)
            {
                throw new ArgumentNullException(nameof(oidcClientSecretSecureId));
            }

            if (string.IsNullOrWhiteSpace(oidcClientId))
            {
                throw new ArgumentNullException(nameof(oidcClientId));
            }

            if (requestBaseUrl == null)
            {
                throw new ArgumentNullException(nameof(requestBaseUrl));
            }

            if (oidcConfig == null)
            {
                throw new ArgumentNullException(nameof(oidcConfig));
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }            

            var oidcMessage = new OpenIdConnectMessage
            {
                GrantType = "authorization_code",
                Code = code,
                ClientSecret = Factory.SecuredData.Read((Guid) oidcClientSecretSecureId),
                ClientId = oidcClientId,
                RedirectUri = GetOidcAuthResponseUri(requestBaseUrl, tenant)
            };

            var oidcMessageContent = new FormUrlEncodedContent(oidcMessage.Parameters);

            var response = await httpClient.PostAsync(new Uri(oidcConfig.TokenEndpoint), oidcMessageContent);

            if (!response.IsSuccessStatusCode)
            {
                EventLog.Application.WriteError("An error occurred getting the authorization tokens. Error {0}.", response.StatusCode);
                throw new OidcProviderInvalidConfigurationException();
            }

            var responseContents = await response.Content.ReadAsStringAsync();

            OpenIdConnectAuthorizationTokenResponse authResponse;

            try
            {
                authResponse = JSON.Deserialize<OpenIdConnectAuthorizationTokenResponse>(responseContents);
            }
            catch (Exception ex)
            {
                throw new AuthenticationException("An error occurred deserializing the authentication response.", ex);
            }

            if (authResponse.TokenType.ToLowerInvariant() != "bearer")
            {
                throw new AuthenticationException("The token type is invalid.");
            }

            if (string.IsNullOrWhiteSpace(authResponse.IdToken))
            {
                throw new AuthenticationException("The Id token is missing.");
            }

            return authResponse;
        }

        /// <summary>
        ///     Validates the identity token.
        /// </summary>
        /// <param name="idToken">The identifier token.</param>
        /// <param name="oidcClientId">The oidc client identifier.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="oidcConfig">The oidc configuration.</param>
        /// <returns>JwtSecurityToken.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        private JwtSecurityToken ValidateIdentityToken(string idToken, string oidcClientId, string nonce, OpenIdConnectConfiguration oidcConfig)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new ArgumentNullException(nameof(idToken));
            }

            if (string.IsNullOrWhiteSpace(oidcClientId))
            {
                throw new ArgumentNullException(nameof(oidcClientId));
            }

            if (string.IsNullOrWhiteSpace(nonce))
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            if (oidcConfig == null)
            {
                throw new ArgumentNullException(nameof(oidcConfig));
            }

            var idTokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = oidcClientId,
                ValidIssuer = oidcConfig.Issuer,
                IssuerSigningTokens = oidcConfig.JsonWebKeySet.GetSigningTokens()
            };

            if (IsTokenValidationDisabled)
            {
                idTokenValidationParameters.LifetimeValidator = (before, expires, token, parameters) => true;
            }

            try
            {
                SecurityToken validatedToken;
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                jwtTokenHandler.ValidateToken(idToken, idTokenValidationParameters, out validatedToken);

                var validationContext = new OpenIdConnectProtocolValidationContext
                {
                    Nonce = IsTokenValidationDisabled ? null : nonce
                };

                var validatedJwtToken = validatedToken as JwtSecurityToken;

                var oidcProtocolValidator = new OpenIdConnectProtocolValidator();
                if (IsTokenValidationDisabled)
                {
                    oidcProtocolValidator.RequireNonce = false;
                }
                oidcProtocolValidator.Validate(validatedJwtToken, validationContext);

                return validatedJwtToken;
            }
            catch (Exception ex)
            {
                throw new AuthenticationException("An error occurred validating the Id token.", ex);
            }
        }

        /// <summary>
        ///     Gets the request context data.
        /// </summary>
        /// <param name="idToken">The identifier token.</param>
        /// <param name="authState">State of the authentication.</param>
        /// <param name="identityProviderId">The identity provider identifier.</param>
        /// <param name="oidcIdentityClaim">The oidc identity claim.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="AuthenticationException">
        /// </exception>
        private OpenIdConnectRequestContextResult GetRequestContextData(JwtSecurityToken idToken, OpenIdConnectAuthorizationState authState, long identityProviderId,
            string oidcIdentityClaim)
        {
            if (idToken == null)
            {
                throw new ArgumentNullException(nameof(idToken));
            }

            // Get the claim from the id token
            var claim = idToken.Claims.FirstOrDefault(c => c.Type == oidcIdentityClaim);

            if (claim == null)
            {
                EventLog.Application.WriteError("The identity token does not contain claim {0}.", oidcIdentityClaim);
                throw new OidcProviderInvalidConfigurationException();
            }

            var claimValue = claim.Value;

            // Find the identity user for this tenant, provider and claim for this provider with the specified value                        
            var requestContextData = Factory.IdentityProviderRequestContextCache.GetRequestContextData(authState.TenantId, identityProviderId, claimValue, true);

            if (requestContextData == null)
            {
                using (new TenantAdministratorContext(authState.TenantId))
                {
                    AuditLogInstance.Get().OnLogon(false, claimValue, null);        // Didn't seem worth passing the agent information into the method to handle this edge case
                }                
                throw new AuthenticationException("The request context could not be found. The user name may be incorrect or the account may be locked, disabled or expired.");
            }

            return new OpenIdConnectRequestContextResult(requestContextData, claimValue);
        }

        /// <summary>
        /// </summary>
        /// <param name="oidcIdentityProvider"></param>
        // ReSharper disable once UnusedParameter.Local
        private void ValidateOidcProviderFields(OidcIdentityProvider oidcIdentityProvider)
        {
            if (string.IsNullOrWhiteSpace(oidcIdentityProvider.OidcClientId))
            {
                throw new AuthenticationException("The identity provider client id is invalid.");
            }

            if (string.IsNullOrWhiteSpace(oidcIdentityProvider.OidcIdentityProviderConfigurationUrl))
            {
                throw new AuthenticationException("The identity provider configuration url is invalid.");
            }

            if (string.IsNullOrWhiteSpace(oidcIdentityProvider.OidcClientSecret))
            {
                throw new AuthenticationException("The identity provider client secret is invalid.");
            }

            if (string.IsNullOrWhiteSpace(oidcIdentityProvider.OidcUserIdentityClaim))
            {
                throw new AuthenticationException("The identity provider identity claim is invalid.");
            }

            if (!(oidcIdentityProvider.IsProviderEnabled ?? true))
            {
                throw new AuthenticationException("The identity provider is disabled.");
            }
        }

        /// <summary>
        ///     Processes the oidc authorization response.
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="code">The code.</param>
        /// <param name="authState">The authentication state.</param>
        /// <param name="requestBaseUrl">The request base url.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns>Task.</returns>
        public async Task<OpenIdConnectRequestContextResult> ProcessOidcAuthorizationResponse(string tenant, string code, OpenIdConnectAuthorizationState authState, Uri requestBaseUrl,
            IHttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (authState == null)
            {
                throw new ArgumentNullException(nameof(authState));
            }

            if (requestBaseUrl == null)
            {
                throw new ArgumentNullException(nameof(requestBaseUrl));
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            ValidateAuthState(authState);

            long identityProviderId;
            string oidcClientId;
            Guid? oidcClientSecretSecureId;
            string oidcIdentityClaim;
            string oidcConfigurationUrl;

            using (new SecurityBypassContext())
            using (new TenantAdministratorContext(authState.TenantId))
            {
                var requestContext = RequestContext.GetContext();
                if (string.Compare(requestContext.Tenant.Name, tenant, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new AuthenticationException("The tenant is invalid.");
                }

                var oidcIdentityProvider = ReadiNow.Model.Entity.Get<OidcIdentityProvider>(authState.IdentityProviderId, GetOidcProviderFieldsToLoad());

                if (oidcIdentityProvider == null)
                {
                    throw new AuthenticationException("The identity provider does not exist.");
                }

                ValidateOidcProviderFields(oidcIdentityProvider);
                
                // Store any required entity model fields upfront.
                // Any code running after the await statement may run a different thread
                identityProviderId = oidcIdentityProvider.Id;
                oidcClientId = oidcIdentityProvider.OidcClientId;

                oidcClientSecretSecureId = oidcIdentityProvider.OidcClientSecretSecureId;

                oidcIdentityClaim = oidcIdentityProvider.OidcUserIdentityClaim;
                oidcConfigurationUrl = oidcIdentityProvider.OidcIdentityProviderConfigurationUrl;
            }

            OpenIdConnectConfiguration oidcConfig;            

            try
            {
                oidcConfig = await _configurationManager.GetIdentityProviderConfigurationAsync(oidcConfigurationUrl);                
            }
            catch (Exception ex)
            {
                throw new OidcProviderInvalidConfigurationException(ex);
            }

            JwtSecurityToken idToken = null;

			try
			{            
				var authResponse = await GetAuthorizationTokens(tenant, code, oidcClientSecretSecureId, oidcClientId, requestBaseUrl, oidcConfig, httpClient);
			    const int maxRetries = 2;
			    int retryCount = 0;

			    while (retryCount <= maxRetries)
			    {
			        try
			        {
			            if (retryCount > 0)
			            {
                            // Are retrying due to an error.
                            // Remove the config from the cache and get a fresh copy
                            _configurationManager.RemoveIdentityProviderConfiguration(oidcConfigurationUrl);
                            oidcConfig = await _configurationManager.GetIdentityProviderConfigurationAsync(oidcConfigurationUrl);
                        }

                        idToken = ValidateIdentityToken(authResponse.IdToken, oidcClientId, authState.Nonce, oidcConfig);
                        // Got the token, so break out of retry loop.
			            break;
			        }
			        catch
			        {
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            throw;
                        }                        
                    }
                }                
            }            
            catch
            {
                // Id provider may have cycled keys. Remove the configuration so that the next request gets updated config.
                _configurationManager.RemoveIdentityProviderConfiguration(oidcConfigurationUrl);
                throw;
            }

            return GetRequestContextData(idToken, authState, identityProviderId, oidcIdentityClaim);
        }

        /// <summary>
        ///     Gets the oidc auth response URL.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="tenant"></param>
        /// <returns>Uri.</returns>
        private string GetOidcAuthResponseUri(Uri baseUri, string tenant)
        {
            var uri = new Uri(baseUri + "spapi/data/v1/login/oidc/authresponse/" + tenant);
            return uri.ToString().ToLowerInvariant();
        }

        /// <summary>
        ///     Validates the state of the authentication.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>OpenIdConnectAuthorizationState.</returns>
        /// <exception cref="System.ArgumentNullException">state</exception>
        /// <exception cref="AuthenticationException">The authorization state is invalid.</exception>
        public OpenIdConnectAuthorizationState ValidateAuthState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException(nameof(state));
            }

            OpenIdConnectAuthorizationState authState;

            // Decrypt, decode and deserialize
            try
            {
                var cryptoProvider = new EncodingCryptoProvider();
                var decryptedState = cryptoProvider.DecodeAndDecrypt(state);

                authState = JSON.Deserialize<OpenIdConnectAuthorizationState>(decryptedState);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("The authorization state is invalid.", exception);
            }

            return ValidateAuthState(authState);
        }


        /// <summary>
        ///     Validates the state of the authentication.
        /// </summary>
        /// <param name="authState">State of the authentication.</param>
        /// <returns>OpenIdConnectAuthorizationState.</returns>
        /// <exception cref="AuthenticationException">
        ///     The authorization state has invalid data.
        ///     or
        ///     The authorization state has expired.
        /// </exception>
        private OpenIdConnectAuthorizationState ValidateAuthState(OpenIdConnectAuthorizationState authState)
        {
            // Validate fields
            if (authState.Timestamp <= 0 ||
                authState.IdentityProviderId <= 0 ||
                string.IsNullOrWhiteSpace(authState.RedirectUrl) ||
                string.IsNullOrWhiteSpace(authState.Nonce) ||
                authState.TenantId <= 0)
            {
                throw new AuthenticationException("The authorization state has invalid data.");
            }

            // Validate timestamp
            var timestamp = new DateTime(authState.Timestamp, DateTimeKind.Utc);
            var maxValidTime = timestamp.AddHours(1);
            var nowUtc = DateTime.UtcNow;

            if (nowUtc < timestamp || nowUtc > maxValidTime)
            {
                throw new AuthenticationException("The authorization state has expired.");
            }

            return authState;
        }
    }
}