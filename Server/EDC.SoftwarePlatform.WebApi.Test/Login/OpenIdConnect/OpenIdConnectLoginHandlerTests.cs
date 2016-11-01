// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using EDC.Security;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using Jil;
using Microsoft.IdentityModel.Protocols;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.WebApi.Test.Login.OpenIdConnect
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class OpenIdConnectLoginHandlerTests
    {
        private OpenIdConnectConfiguration _oidcConfig;
        private string _openIdTokenResponse;

        [TestFixtureSetUp]
        public void Setup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var openIdProviderKeys = string.Empty;

            using (var configStream = assembly.GetManifestResourceStream("EDC.SoftwarePlatform.WebApi.Test.Login.OpenIdConnect.OpenIdProviderKeys.json"))
                if (configStream != null)
                    using (var reader = new StreamReader(configStream))
                    {
                        openIdProviderKeys = reader.ReadToEnd();
                    }

            using (var configStream = assembly.GetManifestResourceStream("EDC.SoftwarePlatform.WebApi.Test.Login.OpenIdConnect.OpenIdProviderConfig.json"))
                if (configStream != null)
                    using (var reader = new StreamReader(configStream))
                    {
                        _oidcConfig = new OpenIdConnectConfiguration(reader.ReadToEnd()) {JsonWebKeySet = new JsonWebKeySet(openIdProviderKeys)};
                    }

            using (var configStream = assembly.GetManifestResourceStream("EDC.SoftwarePlatform.WebApi.Test.Login.OpenIdConnect.OpenIdProviderTokenResponse.json"))
                if (configStream != null)
                    using (var reader = new StreamReader(configStream))
                    {
                        _openIdTokenResponse = reader.ReadToEnd();
                    }
        }

        private bool ValidateTokenRequestMessage(OpenIdConnectMessage expectedMessage, HttpContent actualMessage)
        {
            var encodedMsg = actualMessage as FormUrlEncodedContent;

            var data = Task.Run(() => encodedMsg.ReadAsFormDataAsync()).Result;

            var receivedMessage = new OpenIdConnectMessage(data);

            return expectedMessage.GrantType == receivedMessage.GrantType &&
                   expectedMessage.Code == receivedMessage.Code &&
                   expectedMessage.ClientSecret == receivedMessage.ClientSecret &&
                   expectedMessage.ClientId == receivedMessage.ClientId &&
                   expectedMessage.RedirectUri == receivedMessage.RedirectUri;
        }

        private void CreateEntityModel(string idpUserName, bool active, out OidcIdentityProvider idProvider, out OidcIdentityProviderUser idProviderUser, out UserAccount userAccount)
        {
            var entitiesToSave = new List<IEntity>();

            // Setup provider, provider user and user account
            idProvider = new OidcIdentityProvider
            {
                Name = "ID Provider " + Guid.NewGuid(),
                IsProviderEnabled = true,
                OidcClientId = "5E9762FD-8EB8-4626-AB5A-A52A05041DC0",
                OidcClientSecret = "cZ7xNZiBvPB41Clw0i0FnR4LRqfHvj2H3tBt6c8l",
                OidcIdentityProviderConfigurationUrl = "https://rndev20adfs.sp.local/adfs/.well-known/openid-configuration",
                OidcUserIdentityClaim = "upn"
            };
            entitiesToSave.Add(idProvider);

            userAccount = new UserAccount {Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = active ? UserAccountStatusEnum_Enumeration.Active : UserAccountStatusEnum_Enumeration.Disabled};
            entitiesToSave.Add(userAccount);

            idProviderUser = new OidcIdentityProviderUser
            {
                AssociatedUserAccount = userAccount,
                Name = idpUserName,
                IdentityProviderForUser = idProvider.As<IdentityProvider>()
            };
            entitiesToSave.Add(idProviderUser);

            Entity.Save(entitiesToSave);
        }

        private void CreateEntityModelNoUserAccount(string idpUserName, out OidcIdentityProvider idProvider, out OidcIdentityProviderUser idProviderUser)
        {
            var entitiesToSave = new List<IEntity>();

            // Setup provider, provider user and user account
            idProvider = new OidcIdentityProvider
            {
                Name = "ID Provider " + Guid.NewGuid(),
                IsProviderEnabled = true,
                OidcClientId = "5E9762FD-8EB8-4626-AB5A-A52A05041DC0",
                OidcClientSecret = "cZ7xNZiBvPB41Clw0i0FnR4LRqfHvj2H3tBt6c8l",
                OidcIdentityProviderConfigurationUrl = "https://rndev20adfs.sp.local/adfs/.well-known/openid-configuration",
                OidcUserIdentityClaim = "upn"
            };
            entitiesToSave.Add(idProvider);

            idProviderUser = new OidcIdentityProviderUser
            {
                Name = idpUserName,
                IdentityProviderForUser = idProvider.As<IdentityProvider>()
            };
            entitiesToSave.Add(idProviderUser);

            Entity.Save(entitiesToSave);
        }


        /// <summary>
        ///     Deletes the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void DeleteEntity(IEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    return;
                }
                Entity.Delete(entity.Id);
            }
            catch
            {
                // ignored
            }
        }

        [Test]        
        public void GetAuthorizationCodeRequestUrl()
        {
            var context = RequestContext.GetContext();

            OidcIdentityProvider idProvider;
            OidcIdentityProviderUser idProviderUser;
            UserAccount userAccount;

            // Setup entity model
            CreateEntityModel("IDP User" + Guid.NewGuid(), false, out idProvider, out idProviderUser, out userAccount);

            // Create mock
            var mockRepo = new MockRepository(MockBehavior.Strict);
            var configUrl = idProvider.OidcIdentityProviderConfigurationUrl;
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            configManager.Setup(w => w.GetIdentityProviderConfigurationAsync(configUrl)).Returns(Task.FromResult(_oidcConfig));

            var idpLoginRequest = new IdentityProviderLoginRequest
            {
                Tenant = context.Tenant.Name,
                IdentityProviderId = idProvider.Id,
                RedirectUrl = "https://test.com/login/callback"
            };
            var baseUri = new Uri("https://test.com");

            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            var timeStampBefore = DateTime.UtcNow;

            var uri = Task.Run(() => oidcLoginHandler.GetAuthorizationCodeRequestUrl(idpLoginRequest, baseUri)).Result;

            var timeStampAfter = DateTime.UtcNow;

            // Validate url
            Assert.AreEqual("rndev20adfs.sp.local", uri.Host);
            Assert.AreEqual("/adfs/oauth2/authorize/", uri.AbsolutePath);

            var queryValues = uri.ParseQueryString();
            Assert.AreEqual(idProvider.OidcClientId, queryValues["client_id"], "The client_id is invalid");
            Assert.AreEqual("https://test.com/spapi/data/v1/login/oidc/authresponse/" + context.Tenant.Name.ToLowerInvariant(), queryValues["redirect_uri"], "The redirect_uri is invalid");
            Assert.AreEqual("openid email", queryValues["scope"], "The scope is invalid");
            Assert.AreEqual("code", queryValues["response_type"], "The response_type is invalid");
            Assert.AreEqual("login", queryValues["prompt"], "The prompt is invalid");

            var nonce = queryValues["nonce"];
            Assert.IsNotNullOrEmpty(nonce, "The nonce is invalid.");

            var state = queryValues["state"];
            Assert.IsNotNullOrEmpty(state, "The state is invalid.");

            var validatedState = oidcLoginHandler.ValidateAuthState(state);
            Assert.AreEqual(context.Tenant.Id, validatedState.TenantId, "The state TenantId is invalid");
            Assert.AreEqual(idProvider.Id, validatedState.IdentityProviderId, "The state IdentityProviderId is invalid");
            Assert.AreEqual(nonce, validatedState.Nonce, "The state Nonce is invalid");
            Assert.AreEqual(idpLoginRequest.RedirectUrl, validatedState.RedirectUrl, "The state RedirectUrl is invalid");

            var timeStamp = new DateTime(validatedState.Timestamp, DateTimeKind.Utc);

            // Verify timestamp is between times
            Assert.GreaterOrEqual(timeStamp, timeStampBefore);
            Assert.LessOrEqual(timeStamp, timeStampAfter);

            mockRepo.VerifyAll();
        }

        [Test]        
        public void GetAuthorizationCodeRequestUrl_InvalidLoginRequest()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            configManager.Setup(w => w.GetIdentityProviderConfigurationAsync(It.IsAny<string>())).Returns(Task.FromResult(_oidcConfig));
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            Assert.That(async () => await oidcLoginHandler.GetAuthorizationCodeRequestUrl(new IdentityProviderLoginRequest {IdentityProviderId = 0, Tenant = "EDC"}, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("requestBaseUrl"));

            Assert.That(
                async () => await oidcLoginHandler.GetAuthorizationCodeRequestUrl(new IdentityProviderLoginRequest {IdentityProviderId = 0, Tenant = "EDC", RedirectUrl = ""}, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("requestBaseUrl"));

            Assert.That(async () => await oidcLoginHandler.GetAuthorizationCodeRequestUrl(null, new Uri("http://test.com")),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("idpLoginRequest"));

            Assert.That(
                async () =>
                    await
                        oidcLoginHandler.GetAuthorizationCodeRequestUrl(new IdentityProviderLoginRequest {IdentityProviderId = 100, RedirectUrl = "http://test.com"},
                            new Uri("http://test.com")),
                Throws.TypeOf<ArgumentException>().And.Property("ParamName").EqualTo("idpLoginRequest"));

            Assert.That(
                async () =>
                    await
                        oidcLoginHandler.GetAuthorizationCodeRequestUrl(new IdentityProviderLoginRequest {IdentityProviderId = 0, Tenant = "EDC", RedirectUrl = "http://test.com"},
                            new Uri("http://test.com")),
                Throws.TypeOf<ArgumentException>().And.Property("ParamName").EqualTo("idpLoginRequest"));

            Assert.That(
                async () =>
                    await oidcLoginHandler.GetAuthorizationCodeRequestUrl(new IdentityProviderLoginRequest {IdentityProviderId = 0, Tenant = "EDC"}, new Uri("http://test.com")),
                Throws.TypeOf<ArgumentException>().And.Property("ParamName").EqualTo("idpLoginRequest"));

            Assert.That(
                async () =>
                    await
                        oidcLoginHandler.GetAuthorizationCodeRequestUrl(
                            new IdentityProviderLoginRequest {IdentityProviderId = 10000, Tenant = "EDC", RedirectUrl = "http://test.com"}, new Uri("http://test.com")),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The identity provider does not exist."));

            Assert.That(
                async () =>
                    await
                        oidcLoginHandler.GetAuthorizationCodeRequestUrl(
                            new IdentityProviderLoginRequest {IdentityProviderId = 10000, Tenant = Guid.NewGuid().ToString(), RedirectUrl = "http://test.com"},
                            new Uri("http://test.com")),
                Throws.TypeOf<EntityNotFoundException>());
        }

        [Test]        
        [TestCase("id", "secret", "http://test.com", "claim", false)]
        [TestCase(null, "secret", "http://test.com", "claim", true)]
        [TestCase("", "secret", "http://test.com", "claim", true)]
        [TestCase("id", null, "http://test.com", "claim", true)]
        [TestCase("id", "", "http://test.com", "claim", true)]
        [TestCase("id", "secret", null, "claim", true)]
        [TestCase("id", "secret", "", "claim", true)]
        [TestCase("id", "secret", "http://test.com", null, true)]
        [TestCase("id", "secret", "http://test.com", "", true)]
        public void GetAuthorizationCodeRequestUrl_InvalidProviderSetup(string clientId, string clientSecret, string configUrl, string claim, bool enabled)
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            var provider = new OidcIdentityProvider
            {
                Name = "ID Provider " + Guid.NewGuid(),
                IsProviderEnabled = enabled,
                OidcClientId = clientId,
                OidcClientSecret = clientSecret,
                OidcIdentityProviderConfigurationUrl = configUrl,
                OidcUserIdentityClaim = claim
            };

            Assert.That(
                async () =>
                    await
                        oidcLoginHandler.GetAuthorizationCodeRequestUrl(
                            new IdentityProviderLoginRequest {IdentityProviderId = provider.Id, Tenant = "EDC", RedirectUrl = "http://test.com"}, new Uri("http://test.com")),
                Throws.TypeOf<AuthenticationException>());
        }

        [Test]        
        public void OpenIdConnectLoginHandlerConstructor()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);
            Assert.IsNotNull(oidcLoginHandler, "Handler should not be null");
        }

        [Test]        
        public void OpenIdConnectLoginHandlerConstructor_NullConfigurationManager()
        {
            Assert.That(() => new OpenIdConnectLoginHandler(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("configurationManager"));
        }

        private string GetCurrentTenantName()
        {
            var context = RequestContext.GetContext();
            return context.Tenant.Name.ToLowerInvariant();
        }

        [Test]
        [RunWithoutTransaction]
        public void ProcessOidcAuthorizationResponse()
        {
            var context = RequestContext.GetContext();

            OidcIdentityProvider idProvider = null;
            OidcIdentityProviderUser idProviderUser = null;
            UserAccount userAccount = null;

            try
            {
                string tenantName = GetCurrentTenantName();
                CreateEntityModel("cc.admin@sp.local", true, out idProvider, out idProviderUser, out userAccount);

                var code = Guid.NewGuid().ToString();

                // Mock config provider and http client            

                var expectedTokenRequestMessage = new OpenIdConnectMessage
                {
                    GrantType = "authorization_code",
                    Code = code,
                    ClientSecret = Factory.SecuredData.Read((Guid) idProvider.OidcClientSecretSecureId),
                    ClientId = idProvider.OidcClientId,
                    RedirectUri = "https://test.com/spapi/data/v1/login/oidc/authresponse/" + tenantName
                };

                var validResponseMessage = new HttpResponseMessage {Content = new StringContent(_openIdTokenResponse)};

                var mockRepo = new MockRepository(MockBehavior.Strict);
                var configUrl = idProvider.OidcIdentityProviderConfigurationUrl;
                var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
                configManager.Setup(w => w.GetIdentityProviderConfigurationAsync(configUrl)).Returns(Task.FromResult(_oidcConfig));
                var httpClient = mockRepo.Create<IHttpClient>();
                httpClient.Setup(w => w.PostAsync(new Uri(_oidcConfig.TokenEndpoint), It.Is<HttpContent>(c => ValidateTokenRequestMessage(expectedTokenRequestMessage, c))))
                    .Returns(Task.FromResult(validResponseMessage));

                var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object, true);

                var idpLoginRequest = new IdentityProviderLoginRequest
                {
                    Tenant = context.Tenant.Name,
                    IdentityProviderId = idProvider.Id,
                    RedirectUrl = "https://test.com/login/callback"
                };
                var baseUri = new Uri("https://test.com");

                // Get code auth uri, get state
                var uri = Task.Run(() => oidcLoginHandler.GetAuthorizationCodeRequestUrl(idpLoginRequest, baseUri)).Result;

                var queryValues = uri.ParseQueryString();
                var state = queryValues["state"];
                var validatedState = oidcLoginHandler.ValidateAuthState(state);

                var result =
                    Task.Run(() => oidcLoginHandler.ProcessOidcAuthorizationResponse(tenantName, code, validatedState, baseUri, httpClient.Object)).Result;

                Assert.IsNotNull(result, "The authorization result is invalid.");
                Assert.IsNotNull(result.RequestContextData, "The authorization result context data is invalid.");
                Assert.IsNotNull(result.IdentityProviderUserName, "The authorization result user name is invalid.");

                Assert.AreEqual(RequestContext.TenantId, result.RequestContextData.Tenant.Id, "The tenant is invalid");
                Assert.AreEqual(userAccount.Id, result.RequestContextData.Identity.Id, "The user account is invalid");
                Assert.AreEqual(idProvider.Id, result.RequestContextData.Identity.IdentityProviderId, "The identity provider id is invalid");
                Assert.AreEqual(userAccount.Name, result.RequestContextData.Identity.Name, "The identity provider id is invalid");
                Assert.AreEqual(idProviderUser.Name, result.IdentityProviderUserName, "The identity provider id is invalid");

                mockRepo.VerifyAll();
            }
            finally
            {
                DeleteEntity(idProvider);
                DeleteEntity(idProviderUser);
                DeleteEntity(userAccount);
            }
        }

        public IEnumerable<TestCaseData> FailedTokenRequests()
        {
            List<TestCaseData> testCaseData = new List<TestCaseData>();

            var token = @"{                
                ""token_type"": ""bearer"",
                ""expires_in"": 3600,        
                ""scope"": ""openid"",
                ""id_token"": ""eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkZnZldrTkVJSmtBekdrZVVyWG9qRHhHcUxpWSIsImtpZCI6IkZnZldrTkVJSmtBekdrZVVyWG9qRHhHcUxpWSJ9.eyJhdWQiOiI1RTk3NjJGRC04RUI4LTQ2MjYtQUI1QS1BNTJBMDUwNDFEQzAiLCJpc3MiOiJodHRwczovL3JuZGV2MjBhZGZzLnNwLmxvY2FsL2FkZnMiLCJpYXQiOjE0NjMwMDU3MDAsImV4cCI6MTQ2MzAwOTMwMCwiYXV0aF90aW1lIjoxNDYzMDA1NzAwLCJub25jZSI6IjYzNTk4NjAyNDI3NjgxOTU4OS5ZekE1T0RGaU56TXRNMk0xTWkwMFpqWXdMV0l5TVRFdE56STBNMll5T1dGaE5XRm1Nak16TTJVMk9HVXRNMlF5TmkwME1XSXpMVGxrWVdFdFkyUTNOR0kwWlRnM1pqQXgiLCJzdWIiOiJlelhqTUFDN0ZHQVg0bEVCMVlOaUxKcDNDTk5OS0RBYTgwT2NVVVF2SVhzPSIsInVwbiI6ImNjLmFkbWluQHNwLmxvY2FsIiwidW5pcXVlX25hbWUiOiJTUFxcY2MuYWRtaW4ifQ.U1ZkQqZsDAmi7Xbo0pjCuV6yy6e6TKJpLhevGQEUmblfpHM4oy0v7aPLTtAdwCgmo7ynVjTqyQRSP3lb7TGcV9IT1DtQZseVb4i9saqWmV33W4_8Dfv1c7-63rrmSNeB9VGoyhbHH0QPfz729tZiQmUm4QDgQ1h-rKUMGCYEGTzjcPU-3g_OIs26zudcxaBrNvMXx-e0kE5MIpeRDi1Bp6zvIe0qUMZevMIdRYcrwTZJ0DrdeWSscUGP0DpOz3dc4yqOGfYSLW_pNyP_ANTW0OhTYRJ1XqGEXN9DcvmAGYq81DTQiqnIBXoNS59gj3z1wPG1aRJcCrCwlJ1xXpkJYw""
            }";

            testCaseData.Add(new TestCaseData(token, HttpStatusCode.BadRequest, typeof(OidcProviderInvalidConfigurationException), "The identity provider configuration appears to be invalid, please contact your administrator."));

            var invalidTokenType = @"{                
                ""token_type"": ""invalid token type"",
                ""expires_in"": 3600,        
                ""scope"": ""openid"",
                ""id_token"": ""eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkZnZldrTkVJSmtBekdrZVVyWG9qRHhHcUxpWSIsImtpZCI6IkZnZldrTkVJSmtBekdrZVVyWG9qRHhHcUxpWSJ9.eyJhdWQiOiI1RTk3NjJGRC04RUI4LTQ2MjYtQUI1QS1BNTJBMDUwNDFEQzAiLCJpc3MiOiJodHRwczovL3JuZGV2MjBhZGZzLnNwLmxvY2FsL2FkZnMiLCJpYXQiOjE0NjMwMDU3MDAsImV4cCI6MTQ2MzAwOTMwMCwiYXV0aF90aW1lIjoxNDYzMDA1NzAwLCJub25jZSI6IjYzNTk4NjAyNDI3NjgxOTU4OS5ZekE1T0RGaU56TXRNMk0xTWkwMFpqWXdMV0l5TVRFdE56STBNMll5T1dGaE5XRm1Nak16TTJVMk9HVXRNMlF5TmkwME1XSXpMVGxrWVdFdFkyUTNOR0kwWlRnM1pqQXgiLCJzdWIiOiJlelhqTUFDN0ZHQVg0bEVCMVlOaUxKcDNDTk5OS0RBYTgwT2NVVVF2SVhzPSIsInVwbiI6ImNjLmFkbWluQHNwLmxvY2FsIiwidW5pcXVlX25hbWUiOiJTUFxcY2MuYWRtaW4ifQ.U1ZkQqZsDAmi7Xbo0pjCuV6yy6e6TKJpLhevGQEUmblfpHM4oy0v7aPLTtAdwCgmo7ynVjTqyQRSP3lb7TGcV9IT1DtQZseVb4i9saqWmV33W4_8Dfv1c7-63rrmSNeB9VGoyhbHH0QPfz729tZiQmUm4QDgQ1h-rKUMGCYEGTzjcPU-3g_OIs26zudcxaBrNvMXx-e0kE5MIpeRDi1Bp6zvIe0qUMZevMIdRYcrwTZJ0DrdeWSscUGP0DpOz3dc4yqOGfYSLW_pNyP_ANTW0OhTYRJ1XqGEXN9DcvmAGYq81DTQiqnIBXoNS59gj3z1wPG1aRJcCrCwlJ1xXpkJYw""
            }";

            testCaseData.Add(new TestCaseData(invalidTokenType, HttpStatusCode.OK, typeof(AuthenticationException), "The token type is invalid."));

            var missingIdToken = @"{                
                ""token_type"": ""bearer"",
                ""expires_in"": 3600,        
                ""scope"": ""openid""                
            }";

            testCaseData.Add(new TestCaseData(missingIdToken, HttpStatusCode.OK, typeof(AuthenticationException), "The Id token is missing."));

            var invalidIdToken = @"{                
                ""token_type"": ""bearer"",
                ""expires_in"": 3600,        
                ""scope"": ""openid"",
                ""id_token"": ""blahblahblah""           
            }";

            testCaseData.Add(new TestCaseData(invalidIdToken, HttpStatusCode.OK, typeof(AuthenticationException), "An error occurred validating the Id token."));

            return testCaseData;
        }

        [Test]        
        [TestCaseSource("FailedTokenRequests")]
        public void ProcessOidcAuthorizationResponse_FailedTokenRequest(string response, HttpStatusCode httpStatusCode, Type exceptionType, string message)
        {
            var context = RequestContext.GetContext();

            OidcIdentityProvider idProvider;
            OidcIdentityProviderUser idProviderUser;
            UserAccount userAccount;
            string tenantName = GetCurrentTenantName();

            CreateEntityModel("cc.admin@sp.local", true, out idProvider, out idProviderUser, out userAccount);

            var code = Guid.NewGuid().ToString();

            // Mock config provider and http client            
            
            var errorResponseMessage = new HttpResponseMessage { Content = new StringContent(response), StatusCode = httpStatusCode};

            var mockRepo = new MockRepository(MockBehavior.Strict);
            var configUrl = idProvider.OidcIdentityProviderConfigurationUrl;
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            configManager.Setup(w => w.GetIdentityProviderConfigurationAsync(configUrl)).Returns(Task.FromResult(_oidcConfig));
            configManager.Setup(w => w.RemoveIdentityProviderConfiguration(configUrl));
            var httpClient = mockRepo.Create<IHttpClient>();
            httpClient.Setup(w => w.PostAsync(new Uri(_oidcConfig.TokenEndpoint), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(errorResponseMessage));

            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            var idpLoginRequest = new IdentityProviderLoginRequest
            {
                Tenant = context.Tenant.Name,
                IdentityProviderId = idProvider.Id,
                RedirectUrl = "https://test.com/login/callback"
            };
            var baseUri = new Uri("https://test.com");

            // Get code auth uri, get state
            var uri = Task.Run(() => oidcLoginHandler.GetAuthorizationCodeRequestUrl(idpLoginRequest, baseUri)).Result;

            var queryValues = uri.ParseQueryString();
            var state = queryValues["state"];
            var validatedState = oidcLoginHandler.ValidateAuthState(state);

            Assert.That(
                async () =>
                    await
                        oidcLoginHandler.ProcessOidcAuthorizationResponse(tenantName, code, validatedState, baseUri, httpClient.Object),
                Throws.TypeOf(exceptionType).And.Message.EqualTo(message));

            mockRepo.VerifyAll();            
        }

        [Test]        
        public void ProcessOidcAuthorizationResponse_EnabledTokenValidation()
        {
            var context = RequestContext.GetContext();

            OidcIdentityProvider idProvider;
            OidcIdentityProviderUser idProviderUser;
            UserAccount userAccount;
            string tenantName = GetCurrentTenantName();

            CreateEntityModel("cc.admin@sp.local", true, out idProvider, out idProviderUser, out userAccount);

            var code = Guid.NewGuid().ToString();

            // Mock config provider and http client            
            var expectedTokenRequestMessage = new OpenIdConnectMessage
            {
                GrantType = "authorization_code",
                Code = code,
                ClientSecret = Factory.SecuredData.Read((Guid)idProvider.OidcClientSecretSecureId),
                ClientId = idProvider.OidcClientId,
                RedirectUri = "https://test.com/spapi/data/v1/login/oidc/authresponse/" + tenantName
            };

            var validResponseMessage = new HttpResponseMessage {Content = new StringContent(_openIdTokenResponse)};

            var mockRepo = new MockRepository(MockBehavior.Strict);
            var configUrl = idProvider.OidcIdentityProviderConfigurationUrl;
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            configManager.Setup(w => w.GetIdentityProviderConfigurationAsync(configUrl)).Returns(Task.FromResult(_oidcConfig));
            configManager.Setup(w => w.RemoveIdentityProviderConfiguration(configUrl));
            var httpClient = mockRepo.Create<IHttpClient>();
            httpClient.Setup(w => w.PostAsync(new Uri(_oidcConfig.TokenEndpoint), It.Is<HttpContent>(c => ValidateTokenRequestMessage(expectedTokenRequestMessage, c))))
                .Returns(Task.FromResult(validResponseMessage));

            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            var idpLoginRequest = new IdentityProviderLoginRequest
            {
                Tenant = context.Tenant.Name,
                IdentityProviderId = idProvider.Id,
                RedirectUrl = "https://test.com/login/callback"
            };
            var baseUri = new Uri("https://test.com");

            // Get code auth uri, get state
            var uri = Task.Run(() => oidcLoginHandler.GetAuthorizationCodeRequestUrl(idpLoginRequest, baseUri)).Result;

            var queryValues = uri.ParseQueryString();
            var state = queryValues["state"];
            var validatedState = oidcLoginHandler.ValidateAuthState(state);

            Assert.That(
                async () =>
                    await
                        oidcLoginHandler.ProcessOidcAuthorizationResponse(tenantName, code, validatedState, baseUri, httpClient.Object),
                Throws.TypeOf<AuthenticationException>());
        }

        [Test]        
        public void ProcessOidcAuthorizationResponse_InvalidAuthState()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);
            string tenantName = GetCurrentTenantName();

            var authState1 = new OpenIdConnectAuthorizationState
            {
                TenantId = RequestContext.TenantId,
                Timestamp = DateTime.UtcNow.Ticks,
                IdentityProviderId = 1000,
                Nonce = "xx",
                RedirectUrl = "http://test.com"
            };

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse(tenantName, "code", authState1, new Uri("http://test.com"), new BasicHttpClient()),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The identity provider does not exist."));

            var authState2 = new OpenIdConnectAuthorizationState
            {
                TenantId = 100,
                Timestamp = DateTime.UtcNow.Ticks,
                IdentityProviderId = 1000,
                Nonce = "xx",
                RedirectUrl = "http://test.com"
            };

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse(tenantName, "code", authState2, new Uri("http://test.com"), new BasicHttpClient()),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The tenant is invalid."));
        }

        [Test]        
        public void ProcessOidcAuthorizationResponse_InvalidParams()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse(null, null, new OpenIdConnectAuthorizationState(), new Uri("http://test.com"), new BasicHttpClient()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("tenant"));

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse("EDC", null, new OpenIdConnectAuthorizationState(), new Uri("http://test.com"), new BasicHttpClient()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("code"));

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse("EDC", string.Empty, new OpenIdConnectAuthorizationState(), new Uri("http://test.com"), new BasicHttpClient()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("code"));

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse("EDC", "code", null, new Uri("http://test.com"), new BasicHttpClient()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("authState"));

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse("EDC", "code", new OpenIdConnectAuthorizationState(), null, new BasicHttpClient()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("requestBaseUrl"));

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse("EDC", "code", new OpenIdConnectAuthorizationState(), new Uri("http://test.com"), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("httpClient"));
        }

        [Test]        
        [TestCase("id", "secret", "http://test.com", "claim", false)]
        [TestCase(null, "secret", "http://test.com", "claim", true)]
        [TestCase("", "secret", "http://test.com", "claim", true)]
        [TestCase("id", null, "http://test.com", "claim", true)]
        [TestCase("id", "", "http://test.com", "claim", true)]
        [TestCase("id", "secret", null, "claim", true)]
        [TestCase("id", "secret", "", "claim", true)]
        [TestCase("id", "secret", "http://test.com", null, true)]
        [TestCase("id", "secret", "http://test.com", "", true)]
        public void ProcessOidcAuthorizationResponse_InvalidProviderSetup(string clientId, string clientSecret, string configUrl, string claim, bool enabled)
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);
            string tenantName = GetCurrentTenantName();

            var provider = new OidcIdentityProvider
            {
                Name = "ID Provider " + Guid.NewGuid(),
                IsProviderEnabled = enabled,
                OidcClientId = clientId,
                OidcClientSecret = clientSecret,
                OidcIdentityProviderConfigurationUrl = configUrl,
                OidcUserIdentityClaim = claim
            };

            var authState = new OpenIdConnectAuthorizationState
            {
                TenantId = RequestContext.TenantId,
                IdentityProviderId = provider.Id
            };

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse(tenantName, "code", authState, new Uri("http://test.com"), new BasicHttpClient()),
                Throws.TypeOf<AuthenticationException>());
        }


        [Test]        
        [TestCase("NoIdpUser")]
        [TestCase("NoUserAccount")]
        [TestCase("DisabledUserAccount")]
        public void ProcessOidcAuthorizationResponse_InvalidUser(string test)
        {
            var context = RequestContext.GetContext();

            OidcIdentityProvider idProvider = null;
            OidcIdentityProviderUser idProviderUser;
            UserAccount userAccount;
            string tenantName = GetCurrentTenantName();

            switch (test)
            {
                case "NoIdpUser":
                    // Create user account with a different name                    
                    CreateEntityModel("testUser", true, out idProvider, out idProviderUser, out userAccount);
                    break;

                case "NoUserAccount":
                    // Create idp user with matching name but without associated account
                    CreateEntityModelNoUserAccount("cc.admin@sp.local", out idProvider, out idProviderUser);
                    break;

                case "DisabledUserAccount":
                    // Create user account with a disabled account
                    CreateEntityModel("cc.admin@sp.local", false, out idProvider, out idProviderUser, out userAccount);
                    break;
            }           

            var code = Guid.NewGuid().ToString();

            // Mock config provider and http client            
            var expectedTokenRequestMessage = new OpenIdConnectMessage
            {
                GrantType = "authorization_code",
                Code = code,
                ClientSecret = Factory.SecuredData.Read((Guid) idProvider.OidcClientSecretSecureId),
                ClientId = idProvider.OidcClientId,
                RedirectUri = "https://test.com/spapi/data/v1/login/oidc/authresponse/" + tenantName
            };

            var validResponseMessage = new HttpResponseMessage {Content = new StringContent(_openIdTokenResponse)};

            var mockRepo = new MockRepository(MockBehavior.Strict);
            var configUrl = idProvider.OidcIdentityProviderConfigurationUrl;
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            configManager.Setup(w => w.GetIdentityProviderConfigurationAsync(configUrl)).Returns(Task.FromResult(_oidcConfig));
            var httpClient = mockRepo.Create<IHttpClient>();
            httpClient.Setup(w => w.PostAsync(new Uri(_oidcConfig.TokenEndpoint), It.Is<HttpContent>(c => ValidateTokenRequestMessage(expectedTokenRequestMessage, c))))
                .Returns(Task.FromResult(validResponseMessage));

            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object, true);

            var idpLoginRequest = new IdentityProviderLoginRequest
            {
                Tenant = context.Tenant.Name,
                IdentityProviderId = idProvider.Id,
                RedirectUrl = "https://test.com/login/callback/"
            };
            var baseUri = new Uri("https://test.com");

            // Get code auth uri, get state
            var uri = Task.Run(() => oidcLoginHandler.GetAuthorizationCodeRequestUrl(idpLoginRequest, baseUri)).Result;

            var queryValues = uri.ParseQueryString();
            var state = queryValues["state"];
            var validatedState = oidcLoginHandler.ValidateAuthState(state);

            Assert.That(
                async () =>
                    await oidcLoginHandler.ProcessOidcAuthorizationResponse(tenantName, code, validatedState, baseUri, httpClient.Object),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The request context could not be found. The user name may be incorrect or the account may be locked, disabled or expired."));

            mockRepo.VerifyAll();
        }

        [Test]        
        public void ValidateAuthState_ExpiredState()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            // Create invalid auth state
            var authState = new OpenIdConnectAuthorizationState
            {
                Timestamp = DateTime.UtcNow.AddHours(10).Ticks,
                RedirectUrl = "http://test.com",
                IdentityProviderId = 100,
                TenantId = 100,
                Nonce = "xx"
            };

            // Serialize and encrypt state
            var stateJson = JSON.Serialize(authState);
            var cryptoProvider = new EncodingCryptoProvider();
            var encryptedState1 = cryptoProvider.EncryptAndEncode(stateJson);

            Assert.That(
                () =>
                    oidcLoginHandler.ValidateAuthState(encryptedState1),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The authorization state has expired."));

            // Create invalid auth state
            authState = new OpenIdConnectAuthorizationState
            {
                Timestamp = DateTime.UtcNow.AddHours(-10).Ticks,
                RedirectUrl = "http://test.com",
                IdentityProviderId = 100,
                TenantId = 100,
                Nonce = "xx"
            };

            // Serialize and encrypt state
            stateJson = JSON.Serialize(authState);
            cryptoProvider = new EncodingCryptoProvider();
            var encryptedState2 = cryptoProvider.EncryptAndEncode(stateJson);

            Assert.That(
                () =>
                    oidcLoginHandler.ValidateAuthState(encryptedState2),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The authorization state has expired."));
        }

        [Test]
        [TestCase(-1, "http://test.com", 100, 100, "1111")]
        [TestCase(100, null, 100, 100, "1111")]
        [TestCase(100, "", 100, 100, "1111")]
        [TestCase(100, "http://test.com", -1, 100, "1111")]
        [TestCase(100, "http://test.com", 100, -1, "1111")]
        [TestCase(100, "http://test.com", 100, 101, null)]
        [TestCase(100, "http://test.com", 100, 101, "")]        
        public void ValidateAuthState_InvalidState(long timestamp, string redirectUrl, long identityProviderId, long tenantId, string nonce)
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            // Create invalid auth state
            var authState = new OpenIdConnectAuthorizationState
            {
                Timestamp = timestamp,
                RedirectUrl = redirectUrl,
                IdentityProviderId = identityProviderId,
                TenantId = tenantId,
                Nonce = nonce
            };

            // Serialize and encrypt state
            var stateJson = JSON.Serialize(authState);
            var cryptoProvider = new EncodingCryptoProvider();
            var encryptedState = cryptoProvider.EncryptAndEncode(stateJson);

            Assert.That(
                () =>
                    oidcLoginHandler.ValidateAuthState(encryptedState),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The authorization state has invalid data."));
        }

        [Test]        
        public void ValidateAuthState_NullEmptyState()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            Assert.That(
                () =>
                    oidcLoginHandler.ValidateAuthState(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("state"));

            Assert.That(
                () =>
                    oidcLoginHandler.ValidateAuthState(string.Empty),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("state"));
        }

        [Test]        
        public void ValidateAuthState_CorruptState()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var configManager = mockRepo.Create<IOpenIdConnectConfigurationManager>();
            var oidcLoginHandler = new OpenIdConnectLoginHandler(configManager.Object);

            Assert.That(
                () =>
                    oidcLoginHandler.ValidateAuthState("xx"),
                Throws.TypeOf<AuthenticationException>().And.Message.EqualTo("The authorization state is invalid."));            
        }
    }
}