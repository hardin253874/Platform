// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Login
{
    [TestFixture]
    public class LoginControllerTests
    {
        UserAccount _userAccount;

        [TestFixtureSetUp]
        public void Setup( )
        {
            using ( new TenantAdministratorContext( RunAsDefaultTenant.DefaultTenantName ) )
            {
                UserAccount userAccount;

                userAccount = Entity.Create<UserAccount>( );
                userAccount.Name = "TestUser1";
                userAccount.Password = "TestPass1"; // this gets hashed before being stored
                userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
                userAccount.ChangePasswordAtNextLogon = false;
                userAccount.PasswordNeverExpires = true;
                userAccount.Save( );
                _userAccount = userAccount;
            }
        }

        [TestFixtureTearDown]
        public void Teardown( )
        {
            using ( new TenantAdministratorContext( RunAsDefaultTenant.DefaultTenantName ) )
            {
                _userAccount?.Delete( );
            }
        }

        /// <summary>
        /// Verifies `spsignin` signs in successfully, standard ASPXAUTH cookie is returned to browser along with `Secure` and `HttpOnly` attributes
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void LoginReturnsAspxauthCookieWithSecureAndHttpOnlyAttributes( )
        {
            //Arrange
            using (var request = new PlatformHttpRequest(@"data/v1/login/spsignin", PlatformHttpMethod.Post, doNotAuthenticate:true))
            {
                //Act
                LoginUsingTestCredentials(request);
                var response = request.GetResponse();

                //Assert
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                var cookies = GetCookiesFromResponse(response);
                Assert.That(cookies, Is.Not.Null);

                var aspxAuthCookie = cookies[".ASPXAUTH"];

                Assert.That(aspxAuthCookie, Is.Not.EqualTo("").And.Property("Secure").EqualTo(true).And.Property("HttpOnly").EqualTo(true));

                var jsonResult = request.DeserialiseResponseBody<LoginResult>();
                Assert.That(jsonResult, Is.Not.Null);
                Assert.That(jsonResult.ActiveAccountInfo, Is.Not.Null.And.Property("Tenant").EqualTo("EDC").And.Property("Username").EqualTo( "TestUser1" ) );
            }
        }

        /// <summary>
        /// Verifies that successful logon generates XSRF cookie with 'Secure' attribute set
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void LoginReturnsXsrfTokenWithSecureAttribute()
        {
            //Arrange
            using (var request = new PlatformHttpRequest("data/v1/login/spsignin", PlatformHttpMethod.Post, doNotAuthenticate: true))
            {
                //Act
                LoginUsingTestCredentials( request);
                var response = request.GetResponse();

                if(response.StatusCode != HttpStatusCode.OK)
                    Assert.Inconclusive("Login unexpectedly failed.");

                //Assert
                var cookies = GetCookiesFromResponse(response);
                Assert.That(cookies, Is.Not.Null);

                var xsrfCookie = cookies["XSRF-TOKEN"];
                Assert.That(xsrfCookie, Is.Not.Null.And.Property("HttpOnly").EqualTo(false).And.Property("Secure").EqualTo(true).And.Property("Value").Not.Null);
            }
        }

        /// <summary>
        /// Verifies that XSRF prevention is effective once XSRF-TOKEN is tampered with
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ApiAccessProhibitedOnceXsrfTokenIsTampered()
        {
            //Arrange
            using (var loginRequest = new PlatformHttpRequest("data/v1/login/spsignin", PlatformHttpMethod.Post, doNotAuthenticate: true))
            using (var sessionInfoRequest= new PlatformHttpRequest("data/v1/console/sessioninfo", PlatformHttpMethod.Get, doNotAuthenticate: true))
            {
                LoginUsingTestCredentials( loginRequest);
                var loginResponse = loginRequest.GetResponse();

                if (loginResponse.StatusCode != HttpStatusCode.OK)
                    Assert.Inconclusive("Login unexpectedly failed.");

                var cookies = GetCookiesFromResponse(loginResponse);
                if(cookies == null)
                    Assert.Inconclusive("Cookies expected.");

                var xsrfCookie = cookies["XSRF-TOKEN"];
                if(xsrfCookie == null)
                    Assert.Inconclusive("XSRF-TOKEN expected.");

                //Act
                xsrfCookie.Value = "Modified " + xsrfCookie.Value;
                var response = sessionInfoRequest.GetResponse();

                //Assert
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            }
        }

        /// <summary>
        /// Verifies that disabling of tenant prevents users from logging in.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void LoginFailsWithDisabledTenant()
        {
            //Arrange
            var ctx = RequestContext.GetContext();

            using (var request = new PlatformHttpRequest("data/v1/login/spsignin", PlatformHttpMethod.Post, doNotAuthenticate: true))
            {
                var tenant = default(Tenant);

                try
                {
                    using (new GlobalAdministratorContext())
                    {
                        tenant = Entity.Get<Tenant>(ctx.Tenant.Id, new IEntityRef[] { Tenant.Name_Field, Tenant.IsTenantDisabled_Field }).AsWritable<Tenant>();
                        tenant.IsTenantDisabled = true;
                        tenant.Save();
                    }

                    //Act
                    LoginUsingTestCredentials( request);
                    var response = request.GetResponse();

                    //Assert
                    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "Login should not succeed when Tenant is disabled.");
                }
                finally
                {
                    tenant.Should().NotBeNull("The tenant was not initialized correctly. Test will be invalid!");

                    using (new GlobalAdministratorContext())
                    {
                        if (tenant != null)
                        {
                            tenant.IsTenantDisabled = false;
                            tenant.Save();
                        }
                    }
                }
            }
        }

        static void LoginUsingTestCredentials(PlatformHttpRequest request)
        {
            request.PopulateBody(new JsonLoginCredential
            {
                Tenant = "EDC",
                Username = "TestUser1",
                Password = "TestPass1",
                Persistent = false
            });
        }

        static CookieCollection GetCookiesFromResponse(WebResponse response)
        {
            var cookieHeader = response.Headers["Set-Cookie"];
            if (string.IsNullOrEmpty(cookieHeader)) return null;

            var container = new CookieContainer();
            var loopbackUri = new Uri("https://localhost");
            container.SetCookies(loopbackUri, cookieHeader);
            return container.GetCookies(loopbackUri);
        }


        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource("Test_InvalidLogin_Source")]
        public void Test_InvalidLogin(Func<UserAccount> userAccountFactory, string password, HttpStatusCode expectedHttpStatusCode, string expectedMessage)
        {
            UserAccount userAccount;
            HttpWebResponse response;
            HttpError httpError;

            userAccount = null;
            try
            {
                using (new SecurityBypassContext())
                {
                    userAccount = userAccountFactory();
                }
                using (PlatformHttpRequest loginRequest = new PlatformHttpRequest("data/v1/login/spsignin", PlatformHttpMethod.Post,
                        doNotAuthenticate: true))
                {
                    loginRequest.PopulateBody(new JsonLoginCredential
                    {
                        Username = userAccount.Name,
                        Password = password,
                        Tenant = RequestContext.GetContext().Tenant.Name,
                        Persistent = false
                    });

                    response = loginRequest.GetResponse();
                    Assert.That(response, Has.Property("StatusCode").EqualTo(expectedHttpStatusCode));

                    if (expectedMessage != null)
                    {
                        httpError = loginRequest.DeserialiseResponseBody<HttpError>();
                        Assert.That(httpError, Has.Property("Message").EqualTo(expectedMessage));
                    }
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    try
                    {
                        SecurityBypassContext.Elevate(() => Entity.Delete(userAccount));                                                    
                    }
                    catch (Exception)
                    {
                        // Do nothing on an error. This is just clean up code.
                    }
                }
            }
        }

        public IEnumerable<TestCaseData> Test_InvalidLogin_Source()
        {
            const string password = "Foobar123!@#";

            yield return new TestCaseData(
                (Func<UserAccount>) (() =>
                {
                    UserAccount userAccount;
                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = password
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                password,
                HttpStatusCode.OK,
                null
            ).SetName("Active");
            yield return new TestCaseData(
                (Func<UserAccount>) (() =>
                {
                    UserAccount userAccount;
                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Locked,
                        Password = password
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                password,
                HttpStatusCode.Unauthorized,
                ExceptionFilter.InvalidLoginMessage
            ).SetName("Locked");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    UserAccount userAccount;
                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Disabled,
                        Password = password
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                password,
                HttpStatusCode.Unauthorized,
                ExceptionFilter.InvalidLoginMessage
            ).SetName("Disabled");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    UserAccount userAccount;
                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Expired,
                        Password = password
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                password,
                HttpStatusCode.Unauthorized,
                ExceptionFilter.InvalidLoginMessage
            ).SetName("Expired"); 
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    UserAccount userAccount;
                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        Password = password
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                password,
                HttpStatusCode.Unauthorized,
                ExceptionFilter.InvalidLoginMessage
            ).SetName("No Account Status");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    PasswordPolicy passwordPolicy;
                    UserAccount userAccount;

                    passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");
                    Assert.That(passwordPolicy, Is.Not.Null, "No password policy");

                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = password,
                        BadLogonCount = passwordPolicy.AccountLockoutThreshold
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                password,
                HttpStatusCode.OK,
                null
            ).SetName("At bad password count but correct password");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    PasswordPolicy passwordPolicy;
                    UserAccount userAccount;

                    passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");
                    Assert.That(passwordPolicy, Is.Not.Null, "No password policy");

                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = password + "_",
                        BadLogonCount = passwordPolicy.AccountLockoutThreshold + 1
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                password,
                HttpStatusCode.Unauthorized,
                ExceptionFilter.InvalidLoginMessage
            ).SetName("Locking account");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    PasswordPolicy passwordPolicy;
                    UserAccount userAccount;

                    passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");
                    Assert.That(passwordPolicy, Is.Not.Null, "No password policy");

                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = password,
                        PasswordNeverExpires = false
                    };
                    userAccount.Save();

                    int maxPasswordAge = passwordPolicy.MaximumPasswordAge.Value;
                    userAccount.PasswordLastChanged = DateTime.UtcNow.AddDays(-1 * (maxPasswordAge + 1));
                    userAccount.Save();

                    return userAccount;
                }),
                password,
                HttpStatusCode.Unauthorized,
                ExceptionFilter.InvalidLoginMessage
            ).SetName("Expired password");
        }

        /*[Test]
        [RunAsDefaultTenant]
        [Ignore]
        public void Login_SingleOpenIdProvider()
        {
            List<EntityRef> entitiesToDelete;
            UserAccount userAccount;
            const string password = "Password1!";
            HttpWebResponse response;

            entitiesToDelete = new List<EntityRef>();

            try
            {
                userAccount = new UserAccount
                {
                    Name = "Test user " + Guid.NewGuid(),
                    AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                    Password = password
                };
                userAccount.Save();
                entitiesToDelete.Add(userAccount);

                using (PlatformHttpRequest loginRequest = new PlatformHttpRequest("data/v1/login/spsignin", PlatformHttpMethod.Post,
                        doNotAuthenticate: true))
                {
                    loginRequest.PopulateBody(new JsonLoginCredential
                    {
                        Username = userAccount.Name,
                        Password = password,
                        Tenant = RequestContext.GetContext().Tenant.Name,
                        Persistent = false
                    });

                    // Ensure an open ID login is no created
                    response = loginRequest.GetResponse();
                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK));                    

                    using (new SecurityBypassContext())
                    {
                        EntityCache.Instance.Clear();
                        EntityRelationshipCache.Instance.Clear();

                        userAccount = Entity.Get<UserAccount>(userAccount.Id, false,
                            UserAccount.OpenIdLogins_Field);
                        Assert.That(userAccount.OpenIdLogins, Has.Count.EqualTo(1));
                        Assert.That(
                            userAccount.OpenIdLogins.First(), 
                            Has.Property("Provider").Property("Alias").EqualTo(LoginConstants.SoftwarePlatformOpenIdProviderAlias));

                        // Login again and ensure another open ID login is not created
                        response = loginRequest.GetResponse();
                        Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK));

                        EntityCache.Instance.Clear();
                        EntityRelationshipCache.Instance.Clear();

                        userAccount = Entity.Get<UserAccount>(userAccount.Id, false,
                            UserAccount.OpenIdLogins_Field);
                        Assert.That(userAccount.OpenIdLogins, Has.Count.EqualTo(1));
                        Assert.That(
                            userAccount.OpenIdLogins.First(),
                            Has.Property("Provider").Property("Alias").EqualTo(LoginConstants.SoftwarePlatformOpenIdProviderAlias));
                    }
                }
            }
            finally
            {
                Entity.Delete(entitiesToDelete);
            }
        }*/


        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource("Test_ChangePassword_Source")]
        public void Test_ChangePassword(Func<UserAccount> userAccountFactory, string oldPassword, string newPassword, HttpStatusCode expectedHttpStatusCode, string expectedMessage)
        {
            UserAccount userAccount;
            HttpWebResponse response;
            HttpError httpError;

            userAccount = null;
            try
            {
                using (new SecurityBypassContext())
                {
                    userAccount = userAccountFactory();
                }                    
                using (PlatformHttpRequest loginRequest = new PlatformHttpRequest("data/v1/login/spchangepassword", PlatformHttpMethod.Post,
                        doNotAuthenticate: true))
                {
                    loginRequest.PopulateBody(new JsonPasswordChangeRequest
                    {
                        Username = userAccount.Name,
                        OldPassword = oldPassword,
                        NewPassword = newPassword,
                        Tenant = RequestContext.GetContext().Tenant.Name                        
                    });

                    response = loginRequest.GetResponse();
                    Assert.That(response, Has.Property("StatusCode").EqualTo(expectedHttpStatusCode));

                    if (expectedMessage != null)
                    {
                        httpError = loginRequest.DeserialiseResponseBody<HttpError>();
                        Assert.That(httpError, Has.Property("Message").EqualTo(expectedMessage));
                    }
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    try
                    {
                        SecurityBypassContext.Elevate(() => Entity.Delete(userAccount));
                    }
                    catch (Exception)
                    {                        
                        // Ignore errors
                    }                                    
                }
            }
        }

        public IEnumerable<TestCaseData> Test_ChangePassword_Source()
        {
            const string oldPassword = "Foobar123!@#";
            const string newPassword = "Foobar123!@#New";

            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    UserAccount userAccount;
                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = oldPassword
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                oldPassword,
                newPassword,
                HttpStatusCode.Unauthorized,
                null
            ).SetName("Password not expired");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    PasswordPolicy passwordPolicy;
                    UserAccount userAccount;

                    passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");
                    Assert.That(passwordPolicy, Is.Not.Null, "No password policy");

                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = oldPassword,
                        PasswordNeverExpires = false
                    };
                    userAccount.Save();

                    int maxPasswordAge = passwordPolicy.MaximumPasswordAge.Value;
                    userAccount.PasswordLastChanged = DateTime.UtcNow.AddDays(-1 * (maxPasswordAge + 1));
                    userAccount.Save();

                    return userAccount;
                }),
                oldPassword,
                newPassword,
                HttpStatusCode.OK,
                null
            ).SetName("Password is expired");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    UserAccount userAccount;
                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = oldPassword + "_"
                    };
                    userAccount.Save();
                    return userAccount;
                }),
                oldPassword,
                newPassword,
                HttpStatusCode.Unauthorized,
                ExceptionFilter.InvalidLoginMessage
            ).SetName("Invalid old password");
            yield return new TestCaseData(
                (Func<UserAccount>)(() =>
                {
                    PasswordPolicy passwordPolicy;
                    UserAccount userAccount;

                    passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");
                    Assert.That(passwordPolicy, Is.Not.Null, "No password policy");

                    userAccount = new UserAccount
                    {
                        Name = "Test " + Guid.NewGuid(),
                        AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                        Password = oldPassword,
                        PasswordNeverExpires = false
                    };
                    userAccount.Save();

                    int maxPasswordAge = passwordPolicy.MaximumPasswordAge.Value;
                    userAccount.PasswordLastChanged = DateTime.UtcNow.AddDays(-1 * (maxPasswordAge + 1));
                    userAccount.Save();

                    return userAccount;
                }),
                oldPassword,
                "A",
                HttpStatusCode.InternalServerError,
                null
            ).SetName("Invalid new password");            
        }
    }
}
