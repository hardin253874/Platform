// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Web;
using System.Web.Http;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.ReadiNow.Marketplace.WebApi
{
    /// <summary>
    /// Global entry point to the web application.
    /// </summary>
    public class Global : HttpApplication
    {
        /// <summary>
        /// Handles the start of the web application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configuration.Filters.Add(new SecuredAttribute());
            GlobalConfiguration.Configure(config => config.MapHttpAttributeRoutes());

#if DEBUG
            EventLog.Application.WriteError("Marketplace is running in debug mode. This message should not appear in production.");
#endif

            if (HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled)
            {
                EventLog.Application.WriteError("Marketplace web has debug mode set in web.config. This message should not appear in production.");
            }

            EventLog.Application.WriteInformation("Marketplace web has started.");
        }

        /// <summary>
        /// Handles the stopping of the web application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected void Application_End(object sender, EventArgs e)
        {
            EventLog.Application.WriteInformation("Marketplace web has stopped.");
        }

        /// <summary>
        /// Handles the start of the web session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected void Session_Start(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles when the web session has ended.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected void Session_End(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles when a web request has been made.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles when a web request is being authenticated.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles when an application error has occurred.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected void Application_Error(object sender, EventArgs e)
        {

        }
    }
}