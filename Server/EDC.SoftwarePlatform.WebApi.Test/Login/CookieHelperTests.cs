// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Security;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using Jil;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Login
{
    [TestFixture]
    public class CookieHelperTests
    {
        [Test]
        public void Test_CreateAuthenticationAndXsrfCookies_InvalidTenant()
        {
            Assert.That(() => CookieHelper.CreateAuthenticationAndXsrfCookies(0, 0, "", 0, true, "a"),
                Throws.TypeOf<ArgumentOutOfRangeException>().And.Property("ParamName").EqualTo("tenantId"));
        }

        [Test]
        public void Test_CreateAuthenticationAndXsrfCookies_InvalidIdentityProviderId()
        {
            Assert.That(() => CookieHelper.CreateAuthenticationAndXsrfCookies(5, 0, "", 0, true, "a"),
                Throws.TypeOf<ArgumentOutOfRangeException>().And.Property("ParamName").EqualTo("identityProviderId"));
        }

        [Test]
        public void Test_CreateAuthenticationAndXsrfCookies_NullIdentityProviderUser()
        {
            Assert.That(() => CookieHelper.CreateAuthenticationAndXsrfCookies(5, 5, null, 0, true, "a"),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("identityProviderUser"));
        }

        [Test]
        public void Test_CreateAuthenticationAndXsrfCookies_EmptyIdentityProviderUser()
        {
            Assert.That(() => CookieHelper.CreateAuthenticationAndXsrfCookies(5, 5, "", 0, true, "a"),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("identityProviderUser"));
        }

        [Test]
        public void Test_CreateAuthenticationAndXsrfCookies_InvalidUserId()
        {
            Assert.That(() => CookieHelper.CreateAuthenticationAndXsrfCookies(5, 5, "name", 0, true, "a"),
                Throws.TypeOf<ArgumentOutOfRangeException>().And.Property("ParamName").EqualTo("userAccountId"));
        }        

        [Test]
        [TestCase(110, 120, "Bob", 130, "xx", true)]
        [TestCase(210, 220, "Jimbo", 230, "yy", false)]
        public void Test_CreateAuthenticationAndXsrfCookies(long tenantId, long identityProviderId, string identityProviderUserName, long userAccountId, string xsrfToken, bool persistent)
        {
            HttpCookie authCookie;
            HttpCookie xsrfCookie;
            FormsAuthenticationTicket ticket;
            AuthenticationToken authTicket;

            HttpContext.Current = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));

            CookieHelper.CreateAuthenticationAndXsrfCookies(tenantId, identityProviderId, identityProviderUserName, userAccountId, persistent, xsrfToken);

            authCookie = HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName];
            Assert.That(authCookie, Is.Not.Null, "Null auth cookie");
            Assert.That(authCookie, Has.Property("Values").Count.EqualTo(1), "Incorrect auth cookie values");

            // ReSharper disable once PossibleNullReferenceException
            ticket = FormsAuthentication.Decrypt(authCookie.Value);
            Assert.That(ticket, Has.Property("Version").EqualTo(1));
            Assert.That(ticket, Has.Property("Name").EqualTo(LoginConstants.Cookie.CookieName));
            Assert.That(ticket, Has.Property("IssueDate").EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(10)));
            Assert.That(ticket, Has.Property("Expiration").EqualTo(DateTime.Now.AddMinutes(LoginConstants.Cookie.Timeout)).Within(TimeSpan.FromSeconds(10)));
            Assert.That(ticket, Has.Property("IsPersistent").EqualTo(persistent));

            // ReSharper disable once PossibleNullReferenceException
            authTicket = JSON.Deserialize<AuthenticationToken>(ticket.UserData);
            Assert.That(authTicket, Has.Property("TenantId").EqualTo(tenantId));
            Assert.That(authTicket, Has.Property("IdentityProviderId").EqualTo(identityProviderId));
            Assert.That(authTicket, Has.Property("IdentityProviderUserName").EqualTo(identityProviderUserName));
            Assert.That(authTicket, Has.Property("UserAccountId").EqualTo(userAccountId));
            Assert.That(authTicket, Has.Property("XsrfToken").EqualTo(xsrfToken));
            Assert.That(authTicket, Has.Property("Persist").EqualTo(persistent));

            xsrfCookie = HttpContext.Current.Response.Cookies[LoginConstants.Cookie.AngularDefaultXsrfCookieName];
            Assert.That(xsrfCookie, Is.Not.Null, "Null xsrf cookie");
            Assert.That(xsrfCookie, Has.Property("Values").Count.EqualTo(1), "Incorrect xsrf cookie values");
            Assert.That(xsrfCookie, Has.Property("Value").EqualTo(xsrfToken));
        }

        [Test]
        public void Test_GetUriQueryParameter_NullActionContext()
        {
            Assert.That(() => CookieHelper.GetUriQueryParameter(null, "a"),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("actionContext"));
        }

        [Test]
        public void Test_GetUriQueryParameter_NullParameterName()
        {
            Assert.That(() => CookieHelper.GetUriQueryParameter(new HttpActionContext(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("parameterName"));
        }

        [Test]
        public void Test_GetUriQueryParameter_EmptyParameterName()
        {
            Assert.That(() => CookieHelper.GetUriQueryParameter(new HttpActionContext(), string.Empty),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("parameterName"));
        }

        [Test]
        [TestCase("http://a?a=b", "a", "b")]
        [TestCase("http://a?a=b&c=d", "a", "b")]
        [TestCase("http://a?a=&c=d", "a", "")]
        [TestCase("http://a?a=b&c=def123ghij", "c", "def123ghij")]
        [TestCase("http://a?a=b&c=DEF#aa", "c", "DEF")]
        public void Test_GetUriQueryParameter(string requestUri, string parameterName, string expectedValue)
        {
            HttpActionContext actionContext;

            actionContext = WebApiTestHelpers.BuildHttpActionContext();
            actionContext.Request.RequestUri = new Uri(requestUri, UriKind.Absolute);

            Assert.That(CookieHelper.GetUriQueryParameter(actionContext, parameterName),
                Is.EqualTo(HttpUtility.UrlEncode(expectedValue)));
        }

        [Test]
        public void Test_VerifyXsrfToken_NullActionContext()
        {
            Assert.That(() => CookieHelper.VerifyXsrfToken(null, new AuthenticationToken(), null),
               Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("actionContext"));
        }

        [Test]
        public void Test_VerifyXsrfToken_NullAspxAuthCookie()
        {
	        var actionContext = new HttpActionContext( );
			Assert.That( ( ) => CookieHelper.VerifyXsrfToken( actionContext, null, new Collection<CookieHeaderValue>( ) ),
               Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("authTicket"));
        }

        [Test]
        [TestCase("a", "a", "a", "a", true)]
        [TestCase("b", "a", "a", "a", false)]
        [TestCase("a", "b", "a", "a", false)]
        [TestCase("a", "a", "b", "a", false)]
        [TestCase("a", "a", "a", "b", true)]  // header checked before URI
        [TestCase("b", "b", "a", "a", false)]
        public void Test_VerifyXsrfToken(string authCookieXsrfToken, string cookieXsrfToken,
            string headerXsrfToken, string uriXsrfToken, bool expectedResult)
        {
            HttpActionContext actionContext;
            AuthenticationToken authTicket;
            const string authToken = "authToken";

            actionContext = WebApiTestHelpers.BuildHttpActionContext();
            actionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1}&Persist=False&{2}={3}; {4}={5}",
                    FormsAuthentication.FormsCookieName,
                    authToken,
                    LoginConstants.Cookie.XsrfToken,
                    authCookieXsrfToken,
                    LoginConstants.Cookie.AngularDefaultXsrfCookieName,
                    cookieXsrfToken));
            actionContext.Request.Headers.Add(
                LoginConstants.Headers.AngularJsXsrfToken,
                headerXsrfToken);
            actionContext.Request.RequestUri = new Uri(
                string.Format(
                    "http://a?{0}={1}",
                    LoginConstants.QueryString.XsrfToken,
                    uriXsrfToken),
                UriKind.Absolute);

            authTicket = WebApiTestHelpers.BuildAuthenticationToken(authToken, authCookieXsrfToken);

            Assert.That(
				CookieHelper.VerifyXsrfToken( actionContext, authTicket, actionContext.Request.Headers.GetCookies( ) ),
                Is.EqualTo(expectedResult));
        }

        [Test]
        public void Test_VerifyXsrfToken_NoXsrfCookie()
        {
            HttpActionContext actionContext;
            AuthenticationToken authTicket;
            const string authToken = "authToken";
            const string xsrfToken = "xsrfToken";

            actionContext = WebApiTestHelpers.BuildHttpActionContext();
            actionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1}&Persist=False&{2}={3}",
                    FormsAuthentication.FormsCookieName,
                    authToken,
                    LoginConstants.Cookie.XsrfToken,
                    xsrfToken));
            actionContext.Request.Headers.Add(
                LoginConstants.Headers.AngularJsXsrfToken,
                xsrfToken);
            actionContext.Request.RequestUri = new Uri(
                string.Format(
                    "http://a?{0}={1}",
                    LoginConstants.QueryString.XsrfToken,
                    xsrfToken),
                UriKind.Absolute);

            authTicket = WebApiTestHelpers.BuildAuthenticationToken(authToken, xsrfToken);

            Assert.That(
				CookieHelper.VerifyXsrfToken( actionContext, authTicket, actionContext.Request.Headers.GetCookies( ) ),
                Is.False);
        }

        [Test]
        public void Test_VerifyXsrfToken_NoXsrfTokenInAuthCookie()
        {
            HttpActionContext actionContext;
            AuthenticationToken authTicket;
            const string authToken = "authToken";
            const string xsrfToken = "xsrfToken";

            actionContext = WebApiTestHelpers.BuildHttpActionContext();
            actionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1}&Persist=False&{2}={3}; {4}={5}",
                    FormsAuthentication.FormsCookieName,
                    authToken,
                    LoginConstants.Cookie.XsrfToken,
                    xsrfToken,
                    LoginConstants.Cookie.AngularDefaultXsrfCookieName,
                    xsrfToken));
            actionContext.Request.Headers.Add(
                LoginConstants.Headers.AngularJsXsrfToken,
                xsrfToken);
            actionContext.Request.RequestUri = new Uri(
                string.Format(
                    "http://a?{0}={1}",
                    LoginConstants.QueryString.XsrfToken,
                    xsrfToken),
                UriKind.Absolute);

            authTicket = WebApiTestHelpers.BuildAuthenticationToken(authToken, null);

            Assert.That(
				CookieHelper.VerifyXsrfToken( actionContext, authTicket, actionContext.Request.Headers.GetCookies( ) ),
                Is.False);
        }

        [Test]
        public void Test_VerifyXsrfToken_NoXsrfHeader()
        {
            HttpActionContext actionContext;
            AuthenticationToken authTicket;
            const string authToken = "authToken";
            const string xsrfToken = "xsrfToken";

            actionContext = WebApiTestHelpers.BuildHttpActionContext();
            actionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1}&Persist=False&{2}={3}; {4}={5}",
                    FormsAuthentication.FormsCookieName,
                    authToken,
                    LoginConstants.Cookie.XsrfToken,
                    xsrfToken,
                    LoginConstants.Cookie.AngularDefaultXsrfCookieName,
                    xsrfToken));
            actionContext.Request.Headers.Add(
                LoginConstants.Headers.AngularJsXsrfToken,
                xsrfToken); 
            actionContext.Request.RequestUri = new Uri(
                string.Format(
                    "http://a?{0}={1}",
                    LoginConstants.QueryString.XsrfToken,
                    xsrfToken),
                UriKind.Absolute);

            authTicket = WebApiTestHelpers.BuildAuthenticationToken(authToken, xsrfToken);

            // Will fall back to URL
            Assert.That(
				CookieHelper.VerifyXsrfToken( actionContext, authTicket, actionContext.Request.Headers.GetCookies( ) ),
                Is.True);
        }

        [Test]
        public void Test_VerifyXsrfToken_NoXsrfQueryStringParameter()
        {
            HttpActionContext actionContext;
            AuthenticationToken authTicket;
            const string authToken = "authToken";
            const string xsrfToken = "xsrfToken";

            actionContext = WebApiTestHelpers.BuildHttpActionContext();
            actionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1}&Persist=False&{2}={3}; {4}={5}",
                    FormsAuthentication.FormsCookieName,
                    authToken,
                    LoginConstants.Cookie.XsrfToken,
                    xsrfToken,
                    LoginConstants.Cookie.AngularDefaultXsrfCookieName,
                    xsrfToken));
            actionContext.Request.Headers.Add(
                LoginConstants.Headers.AngularJsXsrfToken,
                xsrfToken);
            actionContext.Request.RequestUri = new Uri(
                "http://a",
                UriKind.Absolute);

            authTicket = WebApiTestHelpers.BuildAuthenticationToken(authToken, xsrfToken);

            // Will use header token instead
            Assert.That(
				CookieHelper.VerifyXsrfToken( actionContext, authTicket, actionContext.Request.Headers.GetCookies( ) ),
                Is.True);
        }

        [Test]
        public void Test_VerifyXsrfToken_NoXsrfHeaderOrQueryStringParameter()
        {
            HttpActionContext actionContext;
            AuthenticationToken authTicket;
            const string authToken = "authToken";
            const string xsrfToken = "xsrfToken";

            actionContext = WebApiTestHelpers.BuildHttpActionContext();
            actionContext.Request.Headers.Add(
                "Cookie",
                string.Format(
                    "{0}={1}&Persist=False&{2}={3}; {4}={5}",
                    FormsAuthentication.FormsCookieName,
                    authToken,
                    LoginConstants.Cookie.XsrfToken,
                    xsrfToken,
                    LoginConstants.Cookie.AngularDefaultXsrfCookieName,
                    xsrfToken));
            actionContext.Request.RequestUri = new Uri(
                "http://a",
                UriKind.Absolute);

            authTicket = WebApiTestHelpers.BuildAuthenticationToken(authToken, xsrfToken);

            Assert.That(
				CookieHelper.VerifyXsrfToken( actionContext, authTicket, actionContext.Request.Headers.GetCookies( ) ),
                Is.False);
        }

        [Test]
        public void Test_GenerateXsrfTokenAsString_IsHex()
        {
            Assert.That(new Regex("^[0-9A-F]+$").IsMatch(CookieHelper.GenerateXsrfTokenAsString()), Is.True,
                "Not a hex dump");
        }
    }
}

