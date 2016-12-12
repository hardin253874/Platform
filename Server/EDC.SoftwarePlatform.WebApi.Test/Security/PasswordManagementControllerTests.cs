// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using EDC.Security;
using EDC.SoftwarePlatform.WebApi.Controllers.EventHandler;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using EDC.SoftwarePlatform.WebApi.Controllers.Security;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Security
{
    [TestFixture]
    public class PasswordManagementControllerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_PasswordChange()
        {
            UserAccount userAccount;
            const string initialPassword = "foobar123!@#";
            const string newPassword = "batbaz456$%^";
            HttpWebResponse response;

            userAccount = null;
            try
            {
                userAccount = new UserAccount
                {
                    Name = "Test User " + Guid.NewGuid(),
                    Password = initialPassword,
                    AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                    ChangePasswordAtNextLogon = true
                };
                userAccount.Save();

                // Ensure the user can logon
                using (PlatformHttpRequest request = new PlatformHttpRequest("data/v1/login/spsignin", PlatformHttpMethod.Post, null, true))
                {
                    request.PopulateBody(new JsonLoginCredential
                    {
                        Tenant = "EDC",
                        Username = userAccount.Name,
                        Password = initialPassword,
                        Persistent = false
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                        "Initial logon failed");
                }

                // Change the password
                using (PlatformHttpRequest request = new PlatformHttpRequest("data/v1/password", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new PasswordChangeInfo
                    {
						CurrentPassword = initialPassword,
						Password = newPassword
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                        "Password reset failed");
                }

                // Ensure the user can logon with the new password
                using (PlatformHttpRequest request = new PlatformHttpRequest("data/v1/login/spsignin", PlatformHttpMethod.Post, null, true))
                {
                    request.PopulateBody(new JsonLoginCredential
                    {
                        Tenant = "EDC",
                        Username = userAccount.Name,
                        Password = newPassword,
                        Persistent = false
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                        "Second logon failed");
                }

                userAccount = Entity.Get<UserAccount>(userAccount);
                Assert.That(userAccount, Has.Property("ChangePasswordAtNextLogon").False);
                Assert.That(CryptoHelper.ValidatePassword(newPassword, userAccount.Password), Is.True,
                    "Password mismatch");
            }
            finally
            {
                try
                {
                    Entity.Delete(userAccount);
                }
                catch (Exception)
                {
                    // Ignore any errors
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_PasswordChange_MissingPassword()
        {
            UserAccount userAccount;
            const string initialPassword = "foobar123!@#";
            HttpWebResponse response;

            userAccount = null;
            try
            {
                userAccount = new UserAccount
                {
                    Name = "Test User " + Guid.NewGuid(),
                    Password = initialPassword,
                    AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active
                };
                userAccount.Save();

                // Change the password
                using (PlatformHttpRequest request = new PlatformHttpRequest("data/v1/password", PlatformHttpMethod.Post, userAccount))
                {
                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.LengthRequired));
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(userAccount);
                }
                catch (Exception)
                {
                    // Ignore any errors
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_PasswordChange_EmptyPassword()
        {
            UserAccount userAccount;
            const string initialPassword = "foobar123!@#";
            HttpWebResponse response;
			HttpError responseBody;

            userAccount = null;
            try
            {
                userAccount = new UserAccount
                {
                    Name = "Test User " + Guid.NewGuid(),
                    Password = initialPassword,
                    AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active
                };
                userAccount.Save();

                // Change the password
                using (PlatformHttpRequest request = new PlatformHttpRequest("data/v1/password", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new PasswordChangeInfo
                    {
						CurrentPassword = initialPassword,
						Password = string.Empty
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                    responseBody = request.DeserialiseResponseBody<HttpError>();
                    Assert.That(responseBody.Message, Is.EqualTo("A password is required."));
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(userAccount);
                }
                catch (Exception)
                {
                    // Ignore any errors
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_PasswordChange_TooSimplePassword()
        {
            UserAccount userAccount;
            const string initialPassword = "foobar123!@#";
            HttpWebResponse response;
            HttpError responseBody;

            userAccount = null;
            try
            {
                userAccount = new UserAccount
                {
                    Name = "Test User " + Guid.NewGuid(),
                    Password = initialPassword,
                    AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active
                };
                userAccount.Save();

                // Change the password
                using (PlatformHttpRequest request = new PlatformHttpRequest("data/v1/password", PlatformHttpMethod.Post, userAccount))
                {
                    // Does not meet minimum complexity requirements for the default password complexity rule. This may fail if the 
                    // default minimum password complexity requirements change.
                    request.PopulateBody(new PasswordChangeInfo
                    {
						CurrentPassword = initialPassword,
						Password = "a"
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

					responseBody = request.DeserialiseResponseBody<HttpError>( );
                    Assert.That(responseBody.Message, 
                        Is.StringStarting("The password is too short; the minimum length is "));
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(userAccount);
                }
                catch (Exception)
                {
                    // Ignore any errors
                }
            }
        }
    }
}

