// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Web;
using System.Web.Http;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using Jil;
using System.Collections.Concurrent;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Test.Infrastructure
{
    public class PlatformHttpRequest : IDisposable
    {
        // Keyed on user
        static readonly ConcurrentDictionary<string, CookieInfo> Cookies = new ConcurrentDictionary<string, CookieInfo>( );

        public HttpWebRequest HttpWebRequest;
        public HttpWebResponse HttpWebResponse;
        private static string _host;
	    private static readonly Options Options = JilFormatter.GetDefaultOptions( );

        /// <summary>
        /// Create a new <see cref="PlatformHttpRequest" />.
        /// </summary>
        /// <param name="uri">The URL to access. This cannot be null, empty or whitespace.</param>
        /// <param name="method">The HTTP method to use. Defaults to <see cref="PlatformHttpMethod.Get" /> if
        /// omitted.</param>
        /// <param name="userAccount">The user account.</param>
        /// <param name="doNotAuthenticate">Flag indicating whether automatic authentication should occur or it should stay anonymous</param>
        /// <param name="isFullUri">The Uri is complete including protocol, machine name.</param>
        /// <exception cref="System.ArgumentNullException">uri</exception>
        /// <exception cref="System.InvalidOperationException">RequestContext not set. Use [RunAsDefaultTenant].</exception>
        /// <exception cref="ArgumentNullException">uri</exception>
        public PlatformHttpRequest(string uri, PlatformHttpMethod method = PlatformHttpMethod.Get, UserAccount userAccount = null, bool doNotAuthenticate = false, bool isFullUri = false)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException("uri");
            if (!doNotAuthenticate && !RequestContext.IsSet)
            {
                throw new InvalidOperationException("RequestContext not set. Use [RunAsDefaultTenant].");
            }

            var host = GetHost();
            string requestUri = isFullUri ? uri : string.Concat(host, "/SpApi/", uri);
            
            HttpWebRequest = WebRequest.CreateHttp(requestUri);
            HttpWebRequest.Method = method.ToString().ToUpperInvariant();
            HttpWebRequest.Headers["Tz"] = "Australia/Sydney";

            if (doNotAuthenticate) return;

            AddFakeLoginTokens(HttpWebRequest, host, userAccount);
        }

        private static void AddFakeLoginTokens(HttpWebRequest request, string host, UserAccount userAccount)
        {
            var context = RequestContext.GetContext( );            

            string userName;
            long userId;

            if (userAccount == null)
            {
                userName = context.Identity.Name;
                userId = context.Identity.Id;
            }
            else
            {
                userName = userAccount.Name;
                userId = userAccount.Id;
            }

            string key = userName;
            long readiNowIdentityProvider = WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance;

            Func<string, CookieInfo> getCookie = key1 =>
                {
                    HttpCookie authCookie;
                    HttpCookie xsrfCookie;

                    CookieHelper.CreateAuthenticationAndXsrfCookies(
                        context.Tenant.Id,
                        readiNowIdentityProvider,
                        userName,
                        userId,
                        false,                                                  // Not persistent
                        null,                                                   // Create new XSRF token,
                        DateTime.Now,                                           // Issue date
                        DateTime.Now.AddMinutes( LoginConstants.Cookie.Timeout ), // Expiry
                        out authCookie,
                        out xsrfCookie );

                    Cookie authCookie2 = ToCookie( authCookie );
                    Cookie xsrfCookie2 = ToCookie( xsrfCookie );

                    var cookieInfo = new CookieInfo
                    {
                        AuthCookie = authCookie2,
                        XsrfCookie = xsrfCookie2,
                        Expires = authCookie2.Expires
                    };
                    return cookieInfo;
                };

            // Get the cookieData
	        if ( key != null )
	        {
		        CookieInfo cookieData = Cookies.GetOrAdd( key, getCookie );
		        if ( cookieData.Expires < DateTime.Now.AddSeconds( 10 ) )
		        {
			        // Refetch if expired
			        cookieData = Cookies.AddOrUpdate( key, getCookie, (key2,old)=>getCookie(key2) ); 
		        }

		        var uri = new Uri(host);
		        request.CookieContainer = new CookieContainer();
		        request.CookieContainer.Add( uri, cookieData.AuthCookie);
		        request.CookieContainer.Add(uri, cookieData.XsrfCookie);
		        request.Headers.Add( LoginConstants.Headers.AngularJsXsrfToken, cookieData.XsrfCookie.Value );
	        }
        }

        private static Cookie ToCookie(HttpCookie httpCookie)
        {
            return new Cookie(httpCookie.Name, httpCookie.Values.ToString())
            {
                HttpOnly = httpCookie.HttpOnly,
                Secure = httpCookie.Secure
            };    
        }

        public static string GetHost()
        {
            if (_host == null)
            {
                lock ( typeof( PlatformHttpRequest ) )
                {
                    if ( _host == null )
                    {
                        IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties( );

                        string hostName = ipProperties.HostName;

                        if ( !string.IsNullOrEmpty( ipProperties.DomainName ) )
                        {
                            hostName += "." + ipProperties.DomainName;
                        }

                        _host = string.Format( "https://{0}", hostName ).ToLowerInvariant( );
                    }
                }
            }
            return _host;
        }

        public void PopulateBody(object body)
        {
            var data = JSON.Serialize( body, Options );
            PopulateBodyString(data);
        }

        public void PopulateBody<T>(T body)
        {
            PopulateBody((object)body);
        }

        public void PopulateBodyString(string data)
        {
            var byteArray = Encoding.UTF8.GetBytes(data);
            HttpWebRequest.ContentType = @"application/json; charset=utf-8";
            HttpWebRequest.ContentLength = byteArray.Length;
            using (var dataStream = HttpWebRequest.GetRequestStream())
                dataStream.Write(byteArray, 0, byteArray.Length);
        }

        public void PopulateBodyStream( Stream stream, string contentType )
        {
            HttpWebRequest.ContentType = contentType;
            HttpWebRequest.ContentLength = stream.Length;
            using ( var dataStream = HttpWebRequest.GetRequestStream( ) )
            {
                stream.CopyTo( dataStream );
            }
        }

        public T DeserialiseResponseBody<T>()
        {
            if (HttpWebResponse == null)
                throw new InvalidOperationException("No web response is available; have you submitted your web request?");

            var stream = HttpWebResponse.GetResponseStream();
            if (stream == null)
                throw new InvalidOperationException("No web response stream is available; have you submitted your web request?");

	        T obj;
	        if ( ! stream.CanSeek || stream.Length > 0 )
	        {
		        var reader = new StreamReader( stream );
		        var responseAsString = reader.ReadToEnd( );
		        obj = JSON.Deserialize<T>( responseAsString, Options );

		        if ( typeof ( T ) == typeof ( HttpError ) )
		        {
			        var existingError = obj as HttpError;

			        if ( existingError != null )
			        {
				        obj = ( T ) ( object ) ExceptionFilter.DeserializeHttpError( existingError );
			        }
		        }
	        }
	        else
	        {
		        obj = default( T );
	        }

	        return obj;
        }

        public HttpWebResponse GetResponse()
        {
            try
            {
                HttpWebResponse = (HttpWebResponse)HttpWebRequest.GetResponse();
            }
            catch (WebException webException)
            {
                HttpWebResponse = (HttpWebResponse)webException.Response;
            }
            return HttpWebResponse;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                if (HttpWebResponse != null)
                {
                    HttpWebResponse.Close();
                }
            }
        }

        #endregion
    }
}
