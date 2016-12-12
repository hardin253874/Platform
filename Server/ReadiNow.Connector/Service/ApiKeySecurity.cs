// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Model;
using System.Security.Authentication;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Diagnostics;

namespace ReadiNow.Connector.Service
{
    /// <summary>
    /// Layer that applies API Key security model to requests
    /// </summary>
    class ApiKeySecurity : IConnectorService
    {
        private const string ApiKeyParamName = "key";

        private readonly IConnectorService _innerService;
        private readonly IEndpointResolver _endpointResolver;

        private static readonly Lazy<Regex> KeyRegex = new Lazy<Regex>( ( ) => new Regex( "^[a-fA-F0-9]{32}|[-a-fA-F0-9]{36}$" ) );
        
        string InvalidApiKeyOrTenantMessage = "Invalid API key or tenant.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerService">The service that will be wrapped and called by this service.</param>
        /// <param name="endpointResolver">The service that resolve endpoints.</param>
        /// <param name="entityRepository">The service that load entities.</param>
        public ApiKeySecurity( IConnectorService innerService, IEndpointResolver endpointResolver, IEntityRepository entityRepository )
        {
            if ( innerService == null )
                throw new ArgumentNullException( nameof( innerService ) );
            if ( endpointResolver == null )
                throw new ArgumentNullException( nameof( endpointResolver ) );
            
            _innerService = innerService;
            _endpointResolver = endpointResolver;
        }

        /// <summary>
        /// Inner service, for diagnostics.
        /// </summary>
        internal IConnectorService InnerService
        {
            get { return _innerService; }
        }

        /// <summary>
        /// Extract authentication information (tenant & API key) from the request.
        /// Run the inner request in the context of the identified user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public ConnectorResponse HandleRequest( ConnectorRequest request )
        {
            if ( request == null )
                throw new ArgumentNullException( nameof( request ) );
            if ( request.QueryString == null )
                throw new ArgumentException( "QueryString is not set.", nameof( request ) );

            // Diagnostic - no tenant
            if ( string.IsNullOrEmpty( request.TenantName ) )
            {
                return new ConnectorResponse( HttpStatusCode.NotFound, "No tenant specified" );
            }
            
            // Get the API key
            string apiKey;
            if ( !request.QueryString.TryGetValue( ApiKeyParamName, out apiKey ) || string.IsNullOrEmpty(apiKey) )
            {
                return new ConnectorResponse( HttpStatusCode.Unauthorized, "An API key is required as a 'key' argument in the query string." );
            }

            // Get the tenant
            IDisposable tenantAdminContext = GetTenantContext( request.TenantName );
            if ( tenantAdminContext == null )
            {
                // If we can't resolve a tenant, then return forbidden.
                // This is to avoid tenant discovery.
                // I.e.   wrong key on the right tenant returns Unauthorized
                // So the wrong key on the wrong tenant returns Unauthorized also. 
                EventLog.Application.WriteWarning( "Connector called with invalid tenant: " + request.TenantName );
                return new ConnectorResponse( HttpStatusCode.Unauthorized, InvalidApiKeyOrTenantMessage );
            }

            using ( tenantAdminContext )
            {
                // Get the API key entity            
                ApiKey apiKeyEntity = GetApiKey( apiKey );
                if ( apiKeyEntity == null || apiKeyEntity.ApiKeyEnabled != true )
                {
                    return new ConnectorResponse( HttpStatusCode.Unauthorized, InvalidApiKeyOrTenantMessage );
                }

                // Get the user account
                UserAccount userAccount = GetValidUserAccount( apiKeyEntity );
                if ( userAccount == null )
                {
                    EventLog.Application.WriteWarning( "Connector API key failed due to account problem. Tenant={0} Key={1}", request.TenantName, apiKey );
                    return new ConnectorResponse( HttpStatusCode.Unauthorized, "Invalid account status." );
                }

                // Verify key can access API
                if (!CanApiKeyAccessApi(apiKeyEntity, request))
                {
                    return new ConnectorResponse( HttpStatusCode.Forbidden, "Cannot access this API." );
                }

                // Impersonate user
                using ( new SetUser( userAccount ) )
                {
                    return _innerService.HandleRequest( request );
                }
            }
        }


        /// <summary>
        /// Processes an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The response.</returns>
        public ConnectorResponse HandleException(Exception exception)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Create an admin context block for the tenant, to use in determining the user account.
        /// </summary>
        private IDisposable GetTenantContext( string tenantName )
        {
            Tenant tenant;

            if ( string.IsNullOrEmpty( tenantName ) )
                return null;

            using ( new GlobalAdministratorContext( ) )
            {
                tenant = TenantHelper.Find( tenantName );
                if ( tenant == null )
                    return null;
            }

            return new TenantAdministratorContext( tenant.Id );
        }

        /// <summary>
        /// Determine if the ApiKey has permission to access the API.
        /// </summary>
        private ApiKey GetApiKey( string apiKey )
        {
            // Validate API key format
            if ( !KeyRegex.Value.IsMatch( apiKey ) )
            {
                EventLog.Application.WriteWarning( "Connector API key did not pass regex test. Key={0}", apiKey );
                return null;
            }

            // Look up API keys by name
            List<ApiKey> apiKeys = Entity.GetByName<ApiKey>( apiKey ).Where( key => key != null ).Take( 2 ).ToList( );
            if ( apiKeys.Count > 1 )
            {
                EventLog.Application.WriteWarning( "Connector API key matched duplicate resources. Key={0}", apiKey );
                throw new ConnectorConfigException( Messages.MultipleApiKeysHaveSameValue );
            }
            if ( apiKeys.Count < 1 )
            {
                EventLog.Application.WriteWarning("Connector API key does not exist. Key={0}", apiKey );
                return null;
            }

            ApiKey apiKeyEntity = apiKeys [ 0 ];

            return apiKeyEntity;
        }

        /// <summary>
        /// Locates the ApiKey resource. Or returns null if it is invalid or not found.
        /// </summary>
        /// <param name="apiKey">The API key</param>
        /// <param name="request">The request</param>
        /// <returns>True if the API key is associated with the API.</returns>
        private bool CanApiKeyAccessApi( ApiKey apiKey, ConnectorRequest request )
        {
            EndpointAddressResult apiDetails;
            try
            {
                apiDetails = _endpointResolver.ResolveEndpoint( request.ApiPath, true );
            }
            catch ( EndpointNotFoundException )
            {
                return false;
            }
            if ( apiDetails == null )
                return false;

            long apiId = apiDetails.ApiId;
            if ( apiId <= 0 )
                return false;

            bool result = apiKey.KeyForApis.Any( api => api.Id == apiId );
            return result;
        }


        /// <summary>
        /// Determine the user account associated with this API key.
        /// Return null if the user account is invalid.
        /// </summary>
        /// <param name="apiKey">The API key</param>
        /// <returns>The user account, or null if the user account is somehow unavailable or locked.</returns>
        private UserAccount GetValidUserAccount( ApiKey apiKey )
        {
            // Get the user account
            UserAccount userAccount = apiKey.ApiKeyUserAccount;
            if ( userAccount == null )
                return null;

            // Validate the account status
            try
            {
                UserAccountValidator.ValidateAccountStatus(userAccount, true);
            }
            catch (AuthenticationException) // note: there are various types that derive from this
            {
                return null;
            }

            return userAccount;
        }
        
    }
}
