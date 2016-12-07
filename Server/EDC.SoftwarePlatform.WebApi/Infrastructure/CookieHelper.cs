// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Security;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using Jil;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Cookie helper class.
	/// </summary>
	public static class CookieHelper
	{
		/// <summary>
		/// The adc header
		/// </summary>
		public static readonly string AdcHeader = "adcAddress";

		/// <summary>
		/// Gets the adc header.
		/// </summary>
		/// <returns></returns>
		private static string GetAdcHeader( )
		{
			string [ ] adcAddresses = HttpContext.Current.Request.Headers.GetValues( AdcHeader );

			string adcAddress = null;

			if ( adcAddresses != null && adcAddresses.Length > 0 )
			{
				adcAddress = adcAddresses [ 0 ];

				if ( string.IsNullOrEmpty( adcAddress ) )
				{
					adcAddress = null;
				}
			}

			return adcAddress;
		}

        /// <summary>
        /// Creates both ASPXAUTH authentication and XSRF prevention token cookies and adds to current HttpResponse instance.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="identityProviderId">The identity provider identifier.</param>
        /// <param name="identityProviderUser">The identity provider user.</param>
        /// <param name="userAccountId">The user account identifier.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <param name="xsrfToken">(Optional) An XSRF token. If omitted, one is generated (recommended).</param>        
        public static void CreateAuthenticationAndXsrfCookies(long tenantId, long identityProviderId, string identityProviderUser, long userAccountId, bool persistent, string xsrfToken = null)
		{
			HttpCookie authCookie;
			HttpCookie xsrfCookie;

			string adcAddress = GetAdcHeader( );

			CreateAuthenticationAndXsrfCookies(
                tenantId, 
                identityProviderId, 
                identityProviderUser, 
                userAccountId,
                persistent,
				xsrfToken,
				DateTime.Now,
				DateTime.Now.AddMinutes( LoginConstants.Cookie.Timeout ),
				adcAddress ?? HttpContext.Current.Request.UserHostAddress,
				HttpContext.Current.Request.UserAgent,
				out authCookie,
				out xsrfCookie );

			HttpContext.Current.Response.Cookies.Add( authCookie );
			HttpContext.Current.Response.Cookies.Add( xsrfCookie );
		}

		/// <summary>
		/// Creates both ASPXAUTH authentication and XSRF prevention token cookies.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="identityProviderId">The identity provider identifier.</param>
		/// <param name="identityProviderUser">The identity provider user.</param>
		/// <param name="userAccountId">The user account identifier.</param>
		/// <param name="persistent">if set to <c>true</c> [persistent].</param>
		/// <param name="xsrfToken">(Optional) An XSRF token. If omitted, one is generated (recommended).</param>
		/// <param name="issueDateTime">When the cookie is issued (normally DateTime.Now);</param>
		/// <param name="expiryDateTime">When the cookie expires.</param>
		/// <param name="userHostAddress">The user host address.</param>
		/// <param name="userAgent">The user agent.</param>
		/// <param name="authCookie">The initialized authentication cookie.</param>
		/// <param name="xsrfCookie">The initialized xsrfCookie.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// tenantId
		/// or
		/// identityProviderId
		/// or
		/// userAccountId
		/// </exception>
		/// <exception cref="System.ArgumentNullException">identityProviderUser</exception>
		public static void CreateAuthenticationAndXsrfCookies( long tenantId, long identityProviderId, string identityProviderUser, long userAccountId, bool persistent,
			string xsrfToken, DateTime issueDateTime, DateTime expiryDateTime, string userHostAddress, string userAgent, out HttpCookie authCookie, out HttpCookie xsrfCookie )
		{
            if (tenantId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tenantId));
            }

            if (identityProviderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(identityProviderId));
            }

            if (string.IsNullOrWhiteSpace(identityProviderUser))
            {
                throw new ArgumentNullException(nameof(identityProviderUser));
            }

            if (userAccountId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userAccountId));
            }

            if ( xsrfToken == null )
			{
				xsrfToken = GenerateXsrfTokenAsString( );
			}

			var authTicket = new AuthenticationToken
			{
				XsrfToken = xsrfToken,
				Persist = persistent,
                TenantId = tenantId,
                IdentityProviderId = identityProviderId,
                IdentityProviderUserName = identityProviderUser,
                UserAccountId = userAccountId,
				HostIp = userHostAddress,
				UserAgent = userAgent
			};

			string userData;

			using ( var output = new StringWriter( ) )
			{
				JSON.Serialize( authTicket, output );
				userData = output.ToString( );
			}

			var ticket = new FormsAuthenticationTicket(
				1 /*version*/,
				LoginConstants.Cookie.CookieName,
				issueDateTime,
				expiryDateTime,
				persistent,
				userData
				);

			string ticketString = FormsAuthentication.Encrypt( ticket );
			authCookie = new HttpCookie( FormsAuthentication.FormsCookieName, ticketString )
			{
				HttpOnly = true,
				Secure = true
			};
			if ( persistent )
			{
				authCookie.Expires = ticket.Expiration;
			}

			xsrfCookie = new HttpCookie( LoginConstants.Cookie.AngularDefaultXsrfCookieName, xsrfToken )
			{
				HttpOnly = false, //must allow JavaScript access so that AngularJS preps HTTP Header based on XSRF token cookie per each request.
				Secure = true, //only transfer token via secure medium
			};
			if ( persistent )
			{
				xsrfCookie.Expires = ticket.Expiration;
			}
		}

		/// <summary>
		///     Creates a new status cookie and adds it to the response.
		/// </summary>
		/// <param name="status">The status.</param>
		public static void CreateStatusCookie( string status )
		{
			var cookie = new HttpCookie( LoginConstants.Cookie.OpenIdStatus )
			{
				Expires = DateTime.Now.AddMinutes( 1 )
			};

			SetCookieValue( cookie, LoginConstants.Cookie.Status, status );

			HttpContext.Current.Response.Cookies.Add( cookie );
		}

		/// <summary>
		///     Request browser to clear both ASPXAUTH and XSRF_TOKEN cookies
		/// </summary>
		public static void DeleteAuthenticationAndXsrfCookies( )
		{
			DeleteXsrfCookie( );
			FormsAuthentication.SignOut( );
		}

		internal static void DeleteXsrfCookie( )
		{
			HttpContext.Current.Response.Cookies.Add( new HttpCookie( LoginConstants.Cookie.AngularDefaultXsrfCookieName, "" )
			{
				HttpOnly = true,
				Secure = true,
				Expires = new DateTime( 1999, 10, 12 ), //any past time
			} );
		}

		/// <summary>
		///     Generate an XSRF token as a hex encoded string.
		/// </summary>
		/// <returns>
		///     The token as a hex encoded string.
		/// </returns>
		public static string GenerateXsrfTokenAsString( )
		{
			var token = new byte[64];

			using ( var rand = new RNGCryptoServiceProvider( ) )
			{
				rand.GetBytes( token );
			}

			// Hex dump rather than Base64 encode because some Base 64 characters are not suitable
			// for inclusion in certain parts of an HTTP header.
			var result = new StringBuilder( );

			foreach ( byte b in token )
			{
				result.Append( b.ToString( "X" ) );
			}

			return result.ToString( );
		}

		/// <summary>
		///     Extract the query parameter called <paramref name="parameterName" />
		///     from <see cref="actionContext" />.
		/// </summary>
		/// <param name="actionContext">
		///     The <see cref="HttpActionContext" /> to extract the query parameter from.
		///     This cannot be null.
		/// </param>
		/// <param name="parameterName">
		///     The name of the parameter to extract from <paramref name="actionContext" />.
		///     This cannot be null, empty or whitespace.
		/// </param>
		/// <returns>
		///     The value of the parameter, if present, or null, if not.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="actionContext" /> cannot be null. <paramref name="parameterName" /> cannot be
		///     null, empty or whitespace.
		/// </exception>
		public static string GetUriQueryParameter( HttpActionContext actionContext, string parameterName )
		{
			if ( actionContext == null )
			{
				throw new ArgumentNullException( "actionContext" );
			}
			if ( string.IsNullOrWhiteSpace( parameterName ) )
			{
				throw new ArgumentNullException( "parameterName" );
			}

			string result = null;

			Match match = new Regex( string.Format( "{0}=([^&]*)", parameterName ) ).Match( actionContext.Request.RequestUri.Query );

			if ( match.Groups[ 1 ].Success )
			{
				result = match.Groups[ 1 ].Captures[ 0 ].Value;
			}

			return result;
		}

		private static void SetCookieValue( HttpCookie cookie, string name, string value )
		{
			if ( value == null )
			{
				cookie.Values.Remove( name );
			}
			else
			{
				cookie.Values[ name ] = value;
			}
		}

		/// <summary>
		///     Ensure the XSRF (Cross Site Request Forgery) cookie matches
		/// </summary>
		/// <param name="actionContext">
		///     The <see cref="HttpActionContext" /> representing the current web operation.
		///     This cannot be null.
		/// </param>
		/// <param name="authTicket">The decrypted contents of the ASPX authentication token. This cannot be null.</param>
		/// <param name="cookies">The cookies.</param>
		/// <returns>
		///     True if the token is valid, false otherwise.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		///     actionContext
		///     or
		///     authTicket
		/// </exception>
		/// <exception cref="ArgumentNullException">No argument can be null.</exception>
		public static bool VerifyXsrfToken( HttpActionContext actionContext, AuthenticationToken authTicket, Collection<CookieHeaderValue> cookies )
		{
			if ( actionContext == null )
			{
				throw new ArgumentNullException( "actionContext" );
			}
			if ( authTicket == null )
			{
				throw new ArgumentNullException( "authTicket" );
			}

			if ( cookies == null )
			{
				throw new ArgumentNullException( "cookies" );
			}

			bool result;

			CookieState xsrfTokenCookie = cookies.Select( c => c[ LoginConstants.Cookie.AngularDefaultXsrfCookieName ] ).FirstOrDefault( );
			if ( xsrfTokenCookie == null || string.IsNullOrEmpty( xsrfTokenCookie.Value ) )
			{
				result = false;
			}
			else
			{
				string xsrfTokenFromXsrfCookie = xsrfTokenCookie.Value;
				string xsrfTokenFromAspxAuthCookie = authTicket.XsrfToken;
				string xsrfTokenFromRequestHeader = null;
				try
				{
					IEnumerable<string> tokens;

					if ( actionContext.Request.Headers.TryGetValues( LoginConstants.Headers.AngularJsXsrfToken, out tokens ) )
					{
						xsrfTokenFromRequestHeader = tokens.FirstOrDefault( );
					}
				}
				catch ( Exception )
				{
					xsrfTokenFromRequestHeader = null;
				}
				string xsrfTokenFromQueryString = GetUriQueryParameter( actionContext, LoginConstants.QueryString.XsrfToken );

				string xsrfTokenSuppliedByClientSameOriginJsCode = xsrfTokenFromRequestHeader ?? xsrfTokenFromQueryString;

				//ensure tokens are supplied and they all match
				//common possibilities:
				// - missing XSRF_TOKEN cookie: client not authenticated or tampered cookie
				// - missing X-XSRF-TOKEN HTTP Header: client not authenticated, non-AngularJS request, potential failed XSRF attach due to Same-origin policy requirements not being met: http://en.wikipedia.org/wiki/Same-origin_policy
				// - XSRF_TOKEN cookie does NOT match X-XSRF-TOKEN HTTP Header: evident failed XSRF attach by trying to supply incorrect XSRF token
				result =
					xsrfTokenFromAspxAuthCookie != null &&
					xsrfTokenFromAspxAuthCookie == xsrfTokenFromXsrfCookie &&
					xsrfTokenFromAspxAuthCookie == xsrfTokenSuppliedByClientSameOriginJsCode;
			}

			return result;
		}

		/// <summary>
		/// Verifies the session has not been hijacked.
		/// </summary>
		/// <param name="authTicket">The authentication ticket.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">authTicket</exception>
		public static bool VerifySessionNotHijacked( AuthenticationToken authTicket )
		{
			if ( authTicket == null )
			{
				throw new ArgumentNullException( nameof( authTicket ) );
			}

			string adcAddress = GetAdcHeader( );

			if ( authTicket.HostIp != null && !string.Equals( authTicket.HostIp, adcAddress ?? HttpContext.Current.Request.UserHostAddress, StringComparison.InvariantCultureIgnoreCase ) )
			{
				return false;
			}

			if ( authTicket.UserAgent != null && !string.Equals( authTicket.UserAgent, HttpContext.Current.Request.UserAgent, StringComparison.InvariantCultureIgnoreCase ) )
			{
				return false;
			}

			return true;
		}
	}
}