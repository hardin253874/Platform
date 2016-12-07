// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Infrastructure
{
    [TestFixture]    
    public class SecuredAttributeTests
    {        
        [Test]
        public void Test_MissingAuthCookie()
        {
            SecuredAttribute securedAttribute;
            HttpActionContext httpActionContext;

            httpActionContext = BuildHttpActionContext();
            HttpContext.Current = BuildHttpContext();

            securedAttribute = new SecuredAttribute();
            Assert.That(() => securedAttribute.OnAuthorization(httpActionContext),
                Throws.TypeOf<InvalidCredentialException>().And.Property("Message").EqualTo(SecuredAttribute.AuthenticationCookieMissingMessage));
        }

        [Test]
        public void Test_EmptyAuthCookie()
        {
            SecuredAttribute securedAttribute;
            HttpActionContext httpActionContext;

            httpActionContext = BuildHttpActionContext();
            httpActionContext.Request.Headers.Add("Cookie",
                string.Format("{0}=", FormsAuthentication.FormsCookieName));
            HttpContext.Current = BuildHttpContext();

            securedAttribute = new SecuredAttribute();
            Assert.That(() => securedAttribute.OnAuthorization(httpActionContext),
                Throws.TypeOf<InvalidCredentialException>().And.Property("Message").EqualTo(SecuredAttribute.AuthenticationCookieInvalidMessage));
        }

        [Test]
        public void Test_InvalidAuthCookie()
        {
            SecuredAttribute securedAttribute;
            HttpActionContext httpActionContext;

            httpActionContext = BuildHttpActionContext();
            httpActionContext.Request.Headers.Add("Cookie",
                string.Format("{0}=aa", FormsAuthentication.FormsCookieName));
            HttpContext.Current = BuildHttpContext();

            securedAttribute = new SecuredAttribute();
            Assert.That(() => securedAttribute.OnAuthorization(httpActionContext),
                Throws.TypeOf<InvalidCredentialException>().And.Property("Message").EqualTo(UserAccountValidator.InvalidUserNameOrPasswordMessage));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ExpiredAuthCookie()
        {
            SecuredAttribute securedAttribute;
            HttpActionContext httpActionContext;
            HttpCookie authCookie;
            HttpCookie xsrfCookie;

            var requestContext = RequestContext.GetContext();

            CookieHelper.CreateAuthenticationAndXsrfCookies(
                RequestContext.TenantId,
                WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance,
                requestContext.Identity.Name,
                requestContext.Identity.Id,
                true,                                                   // Persistent
                null,                                                   // Create new XSRF token,
                DateTime.Now.AddMinutes(-10),                           // Issue date
                DateTime.Now.AddMinutes(-1),                            // Expiry,
				null,
				null,
                out authCookie,
                out xsrfCookie);

            httpActionContext = BuildHttpActionContext();
            httpActionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1};{2}={3}", 
                    authCookie.Name,
                    authCookie.Value,
                    xsrfCookie.Name,
                    xsrfCookie.Value));
            HttpContext.Current = BuildHttpContext();

            securedAttribute = new SecuredAttribute();
            Assert.That(() => securedAttribute.OnAuthorization(httpActionContext),
                Throws.TypeOf<AuthenticationTokenExpiredException>());
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_IdentifierNotInOpenIdCache()
        {
            SecuredAttribute securedAttribute;
            HttpActionContext httpActionContext;
            HttpCookie authCookie;
            HttpCookie xsrfCookie;

            CookieHelper.CreateAuthenticationAndXsrfCookies(
                RequestContext.TenantId,
                WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance,
                Guid.NewGuid().ToString(),
                1234,
                true,                                                   // Persistent
                null,                                                   // Create new XSRF token,
                DateTime.Now,                                           // Issue date
                DateTime.Now.AddMinutes(10),                            // Expiry
				null,
				null,
				out authCookie,
                out xsrfCookie);

            httpActionContext = BuildHttpActionContext();
            httpActionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1};{2}={3}",
                    authCookie.Name,
                    authCookie.Value,
                    xsrfCookie.Name,
                    xsrfCookie.Value));
            httpActionContext.Request.Headers.Add(
                LoginConstants.Headers.AngularJsXsrfToken,
                xsrfCookie.Value);
            httpActionContext.Request.RequestUri = new Uri(
                string.Format(
                    "http://a?{0}={1}",
                    LoginConstants.QueryString.XsrfToken,
                    xsrfCookie.Value),
                UriKind.Absolute); 
            
            HttpContext.Current = BuildHttpContext();

            securedAttribute = new SecuredAttribute();
            Assert.That(() => securedAttribute.OnAuthorization(httpActionContext),
                Throws.TypeOf<InvalidCredentialException>().And.Property("Message").EqualTo(UserAccountValidator.InvalidUserNameOrPasswordMessage));
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("", -1, true)]
        [TestCase("", 1, false)]
        [TestCase(SecuredAttribute.ChallengeRequestUri, -1, false)] // Do not extend for Challenges
        [TestCase(SecuredAttribute.ChallengeRequestUri, 1, false)]
        public void Test_ExtendWindow(string requestPath, double offset, bool expectExtension)
        {
            SecuredAttribute securedAttribute;
            HttpActionContext httpActionContext;
            HttpCookie authCookie;
            HttpCookie xsrfCookie;
            DateTime issueDate;
            DateTime expiryDate;

            // Ensure the ticket is over half way through its window
            issueDate = DateTime.Now.AddMinutes(-(LoginConstants.Cookie.Timeout / 2) + offset);
            expiryDate = issueDate.AddMinutes(LoginConstants.Cookie.Timeout);

            var requestContext = RequestContext.GetContext();

            CookieHelper.CreateAuthenticationAndXsrfCookies(
                RequestContext.TenantId,
                WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance,
                requestContext.Identity.Name,
                requestContext.Identity.Id,
                true,                                                   // Persistent
                null,                                                   // Create new XSRF token,
                issueDate,
                expiryDate,
				null,
				null,
				out authCookie,
                out xsrfCookie);

            httpActionContext = BuildHttpActionContext();
            httpActionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1};{2}={3}",
                    authCookie.Name,
                    authCookie.Value,
                    xsrfCookie.Name,
                    xsrfCookie.Value));
            httpActionContext.Request.Headers.Add(
                LoginConstants.Headers.AngularJsXsrfToken,
                xsrfCookie.Value);
            httpActionContext.Request.RequestUri = new Uri(
                string.Format(
                    "http://host{0}?{1}={2}",
                    requestPath,
                    LoginConstants.QueryString.XsrfToken,
                    xsrfCookie.Value),
                UriKind.Absolute);

            HttpContext.Current = BuildHttpContext();

            securedAttribute = new SecuredAttribute();
            securedAttribute.OnAuthorization(httpActionContext);

            if (expectExtension)
            {
                Assert.That(HttpContext.Current.Response.Cookies.AllKeys,
                    Is.EquivalentTo(new[]
                    {FormsAuthentication.FormsCookieName, LoginConstants.Cookie.AngularDefaultXsrfCookieName}));
            }
            else
            {
                Assert.That(HttpContext.Current.Response.Cookies.AllKeys,
                    Is.Empty);
            }
        }

        private HttpActionContext BuildHttpActionContext()
        {
            HttpControllerContext httpControllerContext;
            HttpRequestMessage httpRequestMessage;
            HttpConfiguration httpConfiguration;
            HttpActionDescriptor httpActionDescriptor;
            HttpControllerDescriptor httpControllerDescriptor;
            IHttpController httpController;

            Type controllerType = typeof (LoginController);
            const string controllerMethodName = "SigninSoftwarePlatformWithCookie_Get";

            httpConfiguration = new HttpConfiguration();

            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://server/sp/data/v1/login/spsignincookie");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("scheme", "parameter");

            httpControllerDescriptor = new HttpControllerDescriptor(
                httpConfiguration,
                controllerType.Name,
                controllerType);
            // ReSharper disable once PossibleNullReferenceException
            httpController = (IHttpController) controllerType.GetConstructor(new Type[0]).Invoke(new object[0]);
            httpControllerContext = new HttpControllerContext(
                new HttpRequestContext(), 
                httpRequestMessage, 
                httpControllerDescriptor, 
                httpController);
            httpActionDescriptor = new ReflectedHttpActionDescriptor(
                httpControllerDescriptor,
                controllerType.GetMethod(controllerMethodName));

            return new HttpActionContext(httpControllerContext, httpActionDescriptor);
        }

        private HttpContext BuildHttpContext()
        {
            return new HttpContext(new HttpRequest("filename", "http://host", ""), new HttpResponse(new StringWriter()));
        }
    }
}


