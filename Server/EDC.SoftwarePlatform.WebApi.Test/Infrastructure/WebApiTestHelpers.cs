// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Security;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApi.Test.Infrastructure
{
    /// <summary>
    /// Helpers for testing classes using Web API classes.
    /// </summary>
    public static class WebApiTestHelpers
    {
        public static HttpActionContext BuildHttpActionContext()
        {
            HttpControllerContext httpControllerContext;
            HttpRequestMessage httpRequestMessage;
            HttpActionContext actionContext;

            httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, string.Empty);

            httpControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), httpRequestMessage);

            actionContext = new HttpActionContext(httpControllerContext, new ReflectedHttpActionDescriptor());

            return actionContext;
        }

        public static AuthenticationToken BuildAuthenticationToken(string authToken, string xsrfToken)
        {
            return new AuthenticationToken
            {                
                IdentityProviderUserName = authToken,                
                XsrfToken = xsrfToken,
                Persist = true
            };
        }
    }
}
