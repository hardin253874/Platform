// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Messaging;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.SoftwarePlatform.WebApi
{
	/// <summary>
	///     Global entry point to the web application.
	/// </summary>
	public class Global : HttpApplication
	{
		/// <summary>
		///     Handles the AuthenticateRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		protected void Application_AuthenticateRequest( object sender, EventArgs e )
		{
		}

		/// <summary>
		///     Handles the BeginRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		protected void Application_BeginRequest( object sender, EventArgs e )
		{
			if ( HttpContext.Current.Request.IsSecureConnection.Equals( false ) )
			{
				bool allow = false;
				string setting = WebConfigurationManager.AppSettings[ "AllowNonSsl" ];
				if ( setting != null )
				{
					bool.TryParse( setting, out allow );
				}
				if ( !allow )
				{
					Response.Redirect( "https://" + Request.ServerVariables[ "HTTP_HOST" ] + HttpContext.Current.Request.RawUrl );
				}
			}

			/////
			// Need to store the stack separately due to ASP thread agility.
			/////
			HttpContext.Current.Items[ "ReadiNow Database Context Global" ] = DatabaseContext.GetContext( );
			HttpContext.Current.Items[ "ReadiNow Database Context" ] = CallContext.GetData( "ReadiNow Database Context" );

			HttpContext.Current.Items[ DeferredChannelMessageContext.HostContextKeyName ] = new DeferredChannelMessageContext( );
		}

		/// <summary>
		///     Handles the End event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		protected void Application_End( object sender, EventArgs e )
		{
		}

		/// <summary>
		///     Handles the EndRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
		protected void Application_EndRequest( object sender, EventArgs args )
		{
			var databaseContext = HttpContext.Current.Items[ "ReadiNow Database Context Global" ] as DatabaseContext;

			databaseContext?.Dispose( );

			var deferredChannelMessageContext = HttpContext.Current.Items[ DeferredChannelMessageContext.HostContextKeyName ] as DeferredChannelMessageContext;

			deferredChannelMessageContext?.Dispose( );
		}

		/// <summary>
		///     Handles the Error event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		protected void Application_Error( object sender, EventArgs e )
		{
		}

		/// <summary>
		///     Handles the Start event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		protected void Application_Start( object sender, EventArgs e )
		{
			ConfigureWebApiRouting( );

			AreaRegistration.RegisterAllAreas( );

			FilterConfig.RegisterGlobalFilters( GlobalFilters.Filters );
			FilterConfig.RegisterHttpFilters( GlobalConfiguration.Configuration.Filters );

			MessageConfig.RegisterMessageHandlers( GlobalConfiguration.Configuration.MessageHandlers );

			FormatterConfig.ConfigureFormatters( GlobalConfiguration.Configuration.Formatters );

#if DEBUG
			EventLog.Application.WriteError( "System is running in debug mode. This message should not appear in production." );
#endif
			if ( HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled )
			{
				EventLog.Application.WriteError( "Debug mode is set in web.config. This message should not appear in production." );
			}

			/////
			// NOTE: Any application/tenant logic should be placed in the AppPreload rather than here...
			/////
		}

		/// <summary>
		///     Handles the End event of the Session control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		protected void Session_End( object sender, EventArgs e )
		{
		}

		/// <summary>
		///     Handles the Start event of the Session control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		protected void Session_Start( object sender, EventArgs e )
		{
		}

		/// <summary>
		///     Configures the web API routing.
		/// </summary>
		static void ConfigureWebApiRouting( )
		{
			ReplaceDefaultAssemblyResolverToIncludeTestControllers( );
			GlobalConfiguration.Configure( config =>
			{
				config.MapHttpAttributeRoutes( );
			} );
		}

		static void ReplaceDefaultAssemblyResolverToIncludeTestControllers( )
		{
			GlobalConfiguration.Configuration.Services.Replace( typeof( IAssembliesResolver ), new TestControllersAssemblyResolver( ) );
		}
	}
}