// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using Jil;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Secured attribute.
	/// </summary>
	public class SecuredAttribute : AuthorizeAttribute
	{
		/// <summary>
		///     Message used for the InvalidCredentialException when the authentication cookie is missing.
		/// </summary>
		public const string AuthenticationCookieInvalidMessage = "Authentication cookie invalid";

		/// <summary>
		///     Message used for the InvalidCredentialException when the authentication cookie is missing.
		/// </summary>
		public const string AuthenticationCookieMissingMessage = "Authentication cookie missing";

		/// <summary>
		///     Uri for the challenge web method.
		/// </summary>
		public const string ChallengeRequestUri = "/spapi/data/v1/login/challenge";        

        /// <summary>
        ///     Processes requests that fail authorization.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        protected override void HandleUnauthorizedRequest( HttpActionContext actionContext )
		{
			actionContext.Response = actionContext.Request.CreateResponse( HttpStatusCode.Unauthorized );
		}

		/// <summary>
		///     Indicates whether the specified control is authorized.
		/// </summary>
		/// <param name="actionContext">The context.</param>
		/// <returns>
		///     true if the control is authorized; otherwise, false.
		/// </returns>
		protected override bool IsAuthorized( HttpActionContext actionContext )
		{
			if ( actionContext == null || actionContext.Request == null )
			{
				// This is intentionally an ArgumentNullException instead of a WebArgumentNullException because
				// this should never be called with a null actionContext by ASP.NET WebAPI.
				throw new ArgumentNullException( "actionContext" );
			}

			// 
			// Integration Test Authorization
			// if we are in integrated testing mode and a test token has been provided, use it. If it fails, pretend it never happened.
			// Otherwise perform a normal test.
			//
			if ( TestAuthorization.IsEnabled )
			{
				var authorizationHeader = HttpContext.Current.Request.Headers.Get( "Authorization" );
				if ( !string.IsNullOrEmpty( authorizationHeader ) )
				{
					long tenantId;
				    long providerId;
				    string userName;
					if ( TestAuthorization.Instance.TryGetIdentifier( authorizationHeader, out tenantId, out providerId, out userName) )
					{					    
                        var testRequestContextData = Factory.IdentityProviderRequestContextCache.GetRequestContextData(tenantId, providerId, userName, false);                        
						RequestContext.SetContext( testRequestContextData );
						return true;
					}
				}
				// fall back to usual login
			}

			Collection<CookieHeaderValue> cookies = actionContext.Request.Headers.GetCookies( );

			// 
			// Normal authorization
			//
			CookieState aspxAuthCookie = cookies.Select( c => c[ FormsAuthentication.FormsCookieName ] ).FirstOrDefault( );

			if ( aspxAuthCookie == null )
			{
				throw new InvalidCredentialException( AuthenticationCookieMissingMessage );
			}

			string ticketData = aspxAuthCookie.Value;
			if ( string.IsNullOrEmpty( ticketData ) )
			{
				throw new InvalidCredentialException( AuthenticationCookieInvalidMessage );
			}

			FormsAuthenticationTicket authenticationTicket;
			try
			{
				authenticationTicket = FormsAuthentication.Decrypt( ticketData );
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( "Error decrypting authentication ticket. " + ex.Message );
				throw new InvalidCredentialException( UserAccountValidator.InvalidUserNameOrPasswordMessage, ex );
			}
			if ( authenticationTicket == null )
			{
				throw new InvalidCredentialException( UserAccountValidator.InvalidUserNameOrPasswordMessage );
			}

			/////
			// Check whether the ticket has expired.
			/////
			if ( authenticationTicket.Expired )
			{
				throw new AuthenticationTokenExpiredException( );
			}

			// Get the encrypted cookie payload
			AuthenticationToken authTicket;
			try
			{
				using ( var input = new StringReader( authenticationTicket.UserData ) )
				{
					authTicket = JSON.Deserialize<AuthenticationToken>( input );
				}
			}
			catch ( Exception ex )
			{
				throw new InvalidCredentialException( AuthenticationCookieInvalidMessage, ex );
			}
			if ( authTicket == null )
			{
				throw new InvalidCredentialException( AuthenticationCookieInvalidMessage );
			}

			// Prevent XSRF attacks
			bool noXsrfCheck =
				actionContext.ActionDescriptor.GetCustomAttributes<NoXsrfCheckAttribute>( ).Count > 0 ||
				actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<NoXsrfCheckAttribute>( ).Count > 0;
			if ( !noXsrfCheck &&
			     !CookieHelper.VerifyXsrfToken( actionContext, authTicket, cookies ) )
			{
			    EventLog.Application.WriteWarning( $"Invalid XSRF token detected. Headers:\n{actionContext?.Request?.Headers}" );
				throw new XsrfValidationException( );
			}

			if ( !CookieHelper.VerifySessionNotHijacked( authTicket ) )
			{
				EventLog.Application.WriteWarning( $"Possible session hijack detected.\nExpected HostIp: '{authTicket.HostIp}', Actual HostIp: '{HttpContext.Current.Request.UserHostAddress ?? string.Empty}'\nExpected UserAgent: '{authTicket.UserAgent}, Actual UserAgent: '{HttpContext.Current.Request.UserAgent}'" );
				throw new InvalidCredentialException( AuthenticationCookieInvalidMessage );
			}

		    RequestContextData requestContextData = Factory.IdentityProviderRequestContextCache.GetRequestContextData(authTicket.TenantId,
                authTicket.IdentityProviderId, authTicket.IdentityProviderUserName, false);
            if (requestContextData == null)
            {
                throw new InvalidCredentialException(UserAccountValidator.InvalidUserNameOrPasswordMessage);
            }

		    if (authTicket.UserAccountId != requestContextData.Identity.Id)
		    {
                throw new InvalidCredentialException(UserAccountValidator.InvalidUserNameOrPasswordMessage);
            }
            
			RequestContext.SetContext( requestContextData );

			/////
			// Don't extend the cookie timeout for challenge requests for the native identity provider
			/////
			if (actionContext.Request.RequestUri.LocalPath.ToLowerInvariant() == ChallengeRequestUri &&
                authTicket.IdentityProviderId == WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance)
			{
				return true;
			}

			/////
			// Sliding window expiry.
			// Note* If the Web page is accessed before half of the expiration time passes, the ticket expiration
			// time will not be reset. As per http://support.microsoft.com/kb/910443/en-gb
			/////
			if ( DateTime.Now <= authenticationTicket.Expiration.AddMinutes( -( LoginConstants.Cookie.Timeout / 2 ) ) )
			{
				return true;
			}

			/////
			// Do not require the caller to have access to the authentication entities e.g. Open ID provider.
			/////
			using ( new SecurityBypassContext( ) )
			{
				CookieHelper.CreateAuthenticationAndXsrfCookies(authTicket.TenantId, authTicket.IdentityProviderId, authTicket.IdentityProviderUserName, authTicket.UserAccountId, authTicket.Persist, authTicket.XsrfToken );
			}

			return true;
		}
	}
}