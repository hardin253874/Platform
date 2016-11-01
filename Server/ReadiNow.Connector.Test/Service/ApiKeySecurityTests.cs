// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Connector.Service;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Test;
using System.Net;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Core;

namespace ReadiNow.Connector.Test.Service
{
    /// <summary>
    /// Test ApiKeySecurity
    /// </summary>
    [TestFixture]
    public class ApiKeySecurityTests
    {
        [TestCase( true, HttpStatusCode.OK )]
        [TestCase( false, HttpStatusCode.Forbidden )]
        [RunWithTransaction]
        public void Test_Successful_Impersonation( bool apiKeyCanSeeApi, HttpStatusCode expectCode )
        {
            string apiKey = "6cb36a1cd60e460bbbfce5af03eb9507"; // or whatever
            string tenantName = RunAsDefaultTenant.DefaultTenantName;
            Mock<IConnectorService> mockService;
            Mock<IEndpointResolver> mockEndpointResolver;
            IConnectorService apiKeyService;
            ConnectorRequest request;
            ConnectorResponse response;
            UserAccount userAccount;
            ApiKey key;
            Api api;
            long tenantId;
            EndpointAddressResult endpoint;

            // Define key and user
            using ( new TenantAdministratorContext( tenantName ) )
            {
                tenantId = RequestContext.TenantId;

                userAccount = new UserAccount( );
                userAccount.Name = "Test user " + Guid.NewGuid( );
                userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
                userAccount.Password = "HelloWorld123!@#";
                userAccount.Save( );

                api = new Api();
                api.Name = "Test API";
                api.Save( );

                key = new ApiKey();
                key.Name = apiKey;
                key.ApiKeyEnabled = true;
                key.ApiKeyUserAccount = userAccount;
                if ( apiKeyCanSeeApi )
                {
                    key.KeyForApis.Add( api );
                }
                key.Save();
                
            }

            // Define service and mock
            mockService = new Mock<IConnectorService>( MockBehavior.Strict );
            mockEndpointResolver = new Mock<IEndpointResolver>( MockBehavior.Strict );
            apiKeyService = new ApiKeySecurity( mockService.Object, mockEndpointResolver.Object, Factory.EntityRepository );
            
            // Define request
            request = new ConnectorRequest
            {
                TenantName = tenantName,
                QueryString = new Dictionary<string, string> { { "key", apiKey } },
                ApiPath = new[] { "whatever" }
            };

            // Setup 
            if ( apiKeyCanSeeApi )
            {
                mockService.Setup( connector => connector.HandleRequest( request ) ).Returns( ( ) =>
                    {
                        Assert.That( RequestContext.TenantId, Is.EqualTo( tenantId ) );
                        return new ConnectorResponse( HttpStatusCode.OK );
                    } ).Verifiable( );
            }
            endpoint = new EndpointAddressResult( api.Id, 0);
            mockEndpointResolver.Setup( resolver => resolver.ResolveEndpoint( request.ApiPath, true ) ).Returns( endpoint ).Verifiable( );

            // Place request
            response = apiKeyService.HandleRequest(request);

            Assert.That( response.StatusCode, Is.EqualTo( expectCode ) );

            mockService.VerifyAll( );
        }

        [TestCase( "DoesntExist", HttpStatusCode.Unauthorized )]    // must be unauthorized for non-existant entries to avoid tenant name discovery
        [TestCase( "", HttpStatusCode.NotFound )]
        [TestCase( null, HttpStatusCode.NotFound )]
        [RunWithTransaction]
        public void Test_Tenant_Invalid( string tenantName, HttpStatusCode expectedCode )
        {
            string apiKey = "6cb36a1cd60e460bbbfce5af03eb9507"; // or whatever
            Mock<IConnectorService> mockService;
            Mock<IEndpointResolver> mockEndpointResolver;
            IConnectorService apiKeyService;
            ConnectorRequest request;
            ConnectorResponse response;

            // Define service and mock
            mockService = new Mock<IConnectorService>( MockBehavior.Strict );
            mockEndpointResolver = new Mock<IEndpointResolver>( MockBehavior.Strict );
            apiKeyService = new ApiKeySecurity( mockService.Object, mockEndpointResolver.Object, Factory.EntityRepository );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = tenantName,
                QueryString = new Dictionary<string, string> { { "key", apiKey } }
            };

            // Place request
            response = apiKeyService.HandleRequest( request );

            // This is to avoid tenant discovery.
            // See notes in ApiKeySecurity.HandleRequest
            Assert.That( response.StatusCode, Is.EqualTo( expectedCode ) );

            mockService.VerifyAll( );
        }

        [TestCase( "11111111111111111111111111111111", HttpStatusCode.Unauthorized )]
        [TestCase( "BadFormat", HttpStatusCode.Unauthorized )]
        [TestCase( "", HttpStatusCode.Unauthorized )]
        [TestCase( null, HttpStatusCode.Unauthorized )]
        [RunWithTransaction]
        public void Test_ApiKey_Invalid( string apiKey, HttpStatusCode expectedHttpCode )
        {
            string tenantName = RunAsDefaultTenant.DefaultTenantName;
            Mock<IConnectorService> mockService;
            Mock<IEndpointResolver> mockEndpointResolver;
            IConnectorService apiKeyService;
            ConnectorRequest request;
            ConnectorResponse response;

            // Define service and mock
            mockService = new Mock<IConnectorService>( MockBehavior.Strict );
            mockEndpointResolver = new Mock<IEndpointResolver>( MockBehavior.Strict );
            apiKeyService = new ApiKeySecurity( mockService.Object, mockEndpointResolver.Object, Factory.EntityRepository );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = tenantName,
                QueryString = new Dictionary<string, string> { { "key", apiKey } }
            };

            // Place request
            response = apiKeyService.HandleRequest( request );

            Assert.That( response.StatusCode, Is.EqualTo( expectedHttpCode ) );

            mockService.VerifyAll( );
        }        

        [Test]
        [RunWithTransaction]
        public void Test_ApiKey_Disabled( )
        {
            string apiKey = "6cb36a1cd60e460bbbfce5af03eb9507"; // or whatever
            string tenantName = RunAsDefaultTenant.DefaultTenantName;
            Mock<IConnectorService> mockService;
            Mock<IEndpointResolver> mockEndpointResolver;
            IConnectorService apiKeyService;
            ConnectorRequest request;
            ConnectorResponse response;
            ApiKey key;

            // Define key and user
            using ( new TenantAdministratorContext( tenantName ) )
            {
                key = new ApiKey( );
                key.Name = apiKey;
                key.Save( );
            }

            // Define service and mock
            mockService = new Mock<IConnectorService>( MockBehavior.Strict );
            mockEndpointResolver = new Mock<IEndpointResolver>( MockBehavior.Strict );
            apiKeyService = new ApiKeySecurity( mockService.Object, mockEndpointResolver.Object, Factory.EntityRepository );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = tenantName,
                QueryString = new Dictionary<string, string> { { "key", apiKey } }
            };

            // Place request
            response = apiKeyService.HandleRequest( request );

            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.Unauthorized ) );

            mockService.VerifyAll( );
        }

        [Test]
        [RunWithTransaction]
        public void Test_UserAccount_NotSet( )
        {
            string apiKey = "6cb36a1cd60e460bbbfce5af03eb9507"; // or whatever
            string tenantName = RunAsDefaultTenant.DefaultTenantName;
            Mock<IConnectorService> mockService;
            Mock<IEndpointResolver> mockEndpointResolver;
            IConnectorService apiKeyService;
            ConnectorRequest request;
            ConnectorResponse response;
            ApiKey key;

            // Define key and user
            using ( new TenantAdministratorContext( tenantName ) )
            {
                key = new ApiKey( );
                key.Name = apiKey;
                key.ApiKeyEnabled = true;
                key.Save( );
            }

            // Define service and mock
            mockService = new Mock<IConnectorService>( MockBehavior.Strict );
            mockEndpointResolver = new Mock<IEndpointResolver>( MockBehavior.Strict );
            apiKeyService = new ApiKeySecurity( mockService.Object, mockEndpointResolver.Object, Factory.EntityRepository );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = tenantName,
                QueryString = new Dictionary<string, string> { { "key", apiKey } }
            };

            // Place request
            response = apiKeyService.HandleRequest( request );

            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.Unauthorized ) );

            mockService.VerifyAll( );
        }

        [TestCase( UserAccountStatusEnum_Enumeration.Disabled )]
        [TestCase( UserAccountStatusEnum_Enumeration.Expired  )]
        [TestCase( UserAccountStatusEnum_Enumeration.Locked )]
        [RunWithTransaction]
        public void Test_UserAccount_Invalid( UserAccountStatusEnum_Enumeration accountStatus )
        {
            string apiKey = "6cb36a1cd60e460bbbfce5af03eb9507"; // or whatever
            string tenantName = RunAsDefaultTenant.DefaultTenantName;
            Mock<IConnectorService> mockService;
            Mock<IEndpointResolver> mockEndpointResolver;
            IConnectorService apiKeyService;
            ConnectorRequest request;
            ConnectorResponse response;
            UserAccount userAccount;
            ApiKey key;

            // Define key and user
            using ( new TenantAdministratorContext( tenantName ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = "Test user " + Guid.NewGuid( );
                userAccount.AccountStatus_Enum = accountStatus;
                userAccount.Password = "HelloWorld123!@#";
                userAccount.Save( );

                key = new ApiKey( );
                key.Name = apiKey;
                key.ApiKeyEnabled = true;
                key.Save( );
            }

            // Define service and mock
            mockService = new Mock<IConnectorService>( MockBehavior.Strict );
            mockEndpointResolver = new Mock<IEndpointResolver>( MockBehavior.Strict );
            apiKeyService = new ApiKeySecurity( mockService.Object, mockEndpointResolver.Object, Factory.EntityRepository );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = tenantName,
                QueryString = new Dictionary<string, string> { { "key", apiKey } }
            };

            // Place request
            response = apiKeyService.HandleRequest( request );

            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.Unauthorized ) );

            mockService.VerifyAll( );
        }

    }
}
