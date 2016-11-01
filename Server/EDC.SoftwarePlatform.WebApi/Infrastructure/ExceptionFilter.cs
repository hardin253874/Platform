// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.Exceptions;
using EDC.ReadiNow.Diagnostics;
using System.IO;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using Jil;
using EDC.ReadiNow.Services.Exceptions;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Exception filter class.
	/// </summary>
	/// <remarks>
	///     By default, all exceptions raised on a 'release' build will output the message 'An error has occurred' to the
	///     client, regardless.
	///     On debug builds, the exception message along with its stack trace are preserved in the errors data object.
	///     This filter will replace the standard message with the exception message.
	/// </remarks>
	public class ExceptionFilter : ExceptionFilterAttribute
	{
        /// <summary>
        /// Generic exception message for invalid login.
        /// </summary>
        public static string InvalidLoginMessage = "Login failed. The user name or password may be incorrect or the account may be locked, disabled or expired.";

        /// <summary>
        /// Generic message for invalid identity provider config;
        /// </summary>
	    public static string InvalidIdentityProviderConfigMessage = "The identity provider configuration appears to be invalid, please contact your administrator.";

        /// <summary>
        /// The additional information key
        /// </summary>
        public static readonly string AdditionalInfoKey = "AdditionalInfo";

		/// <summary>
		/// The password expired key
		/// </summary>
		public static readonly string PasswordExpiredKey = "PasswordExpired";

		/// <summary>
		/// The validation error key
		/// </summary>
		public static readonly string ValidationErrorKey = "ValidationError";


        /// <summary>
        ///     Raises the exception event.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException( HttpActionExecutedContext actionExecutedContext ) 
		{
			Exception exception = actionExecutedContext.Exception;

			/////
			// Create the HTTP error
			/////
			var httpError = new HttpError( exception.Message );

#if DEBUG
            /////
			// Provide some additional debug information when debugging is enabled.
			/////
			if ( HttpContext.Current.IsDebuggingEnabled )
			{
				httpError.Add( HttpErrorKeys.ExceptionMessageKey, exception.Message );
				httpError.Add( HttpErrorKeys.ExceptionTypeKey, exception.GetType( ).FullName );
				httpError.Add( HttpErrorKeys.StackTraceKey, exception.StackTrace );
            }
#endif

            /////
			// Set the response to our HTTP error
			/////
			if ( exception is PlatformSecurityException )
			{
			    actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, httpError);
			}
			else if (exception is WebArgumentNotFoundException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, httpError );
            }
            else if (exception is WebArgumentException)
			{
				actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, httpError );
			}
            else if (exception is AuthenticationTokenExpiredException)
            {
				httpError.Add( AdditionalInfoKey, "TokenExpired" );
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, httpError);
            }
            else if (exception is OidcProviderInvalidConfigurationException)
            {
                httpError.Message = InvalidIdentityProviderConfigMessage;
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, httpError);
            }
            else if (exception is AuthenticationException)
            {
                // Includes InvalidCredentialException, AccountLockedException, AccountDisabledException,
                // AccountInvalidException, PasswordExpiredException and XsrfValidationException
                if (exception is PasswordExpiredException)
                {
					httpError.Add( PasswordExpiredKey, "1" );
                }
                httpError.Message = InvalidLoginMessage;
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, httpError);
            }
            else if ( exception is Jil.DeserializationException )
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "JSON Error: " + exception.Message );
            }
            else if (exception is TenantResourceLimitException)
            {
                var limitEx = (TenantResourceLimitException) exception;
                httpError.Add(AdditionalInfoKey, limitEx.ReasonCode);
                httpError.Message = limitEx.CustomerMessage;
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, httpError);
            }
            else
            {
                if (exception is ValidationException)
                {
					httpError.Add( ValidationErrorKey, "1" );
                }

                // Generic unhandled exception
                actionExecutedContext.Response =
                    actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, httpError);

                string message = CaptureRequestDetails( actionExecutedContext, exception );
                EventLog.Application.WriteError( message );
            }

		    /////
			// flow through to the base
			/////
			base.OnException( actionExecutedContext );
		}

        /// <summary>
        /// Create a text description of the request
        /// </summary>
        private static string CaptureRequestDetails( HttpActionExecutedContext context, object preamble = null )
        {
            const int maxMessageSize = 20000;
                
            // Handle null request (just in case?)
            if ( context == null || context.Request == null)
                return preamble == null ? "" : preamble.ToString( );

            // Set up string builder
            using (TextWriter writer = new StringWriter())
            {
                try
                {
                    if ( preamble != null )
                    {
                        writer.WriteLine( preamble.ToString( ) );
                    }

                    // Write method and URI
                    HttpRequestMessage request = context.Request;
                    writer.WriteLine( );
                    writer.WriteLine( "Uri: {0} {1}", request.Method, request.RequestUri );

                    // Log the controller
                    writer.WriteLine( "Controller: {0}", context.ActionContext.ControllerContext.Controller.GetType( ).FullName );

                    // Log decoded parameters
                    // (Unfortunately we can't easily get to the raw request body, so we have to settle for reserializing them)
                    if ( !( context.ActionContext.ControllerContext.Controller is LoginController ) )
                    {
                        writer.WriteLine( "Action Arguments:" );
                        writer.WriteLine( "(caution: reserialized data, not original message!)" );
	                    var options = new Options( true, false, false, DateTimeFormat.ISO8601, true );
                        
                        foreach ( var pair in context.ActionContext.ActionArguments )
                        {
                            writer.Write( pair.Key );
                            writer.Write( " = " );
                            try
                            {
								JSON.Serialize( pair.Value, writer, options );
                            }
                            catch
                            {
                                writer.Write( "can't reserialize" );
                            }
                            writer.WriteLine( );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    writer.WriteLine( );
                    writer.WriteLine( "Failed to log request:" );
                    writer.WriteLine( ex );
                }

                string message = writer.ToString();
                if ( message.Length > maxMessageSize )
                    message = message.Substring( 0, maxMessageSize );
                return message;
            }
        }

		/// <summary>
		///		Deserializes the HTTP error.
		/// </summary>
		/// <param name="existingError">The existing error.</param>
		/// <returns></returns>
		public static HttpError DeserializeHttpError( HttpError existingError )
		{
			if ( existingError == null )
			{
				return null;
			}

			var error = new HttpError( );

			object value;

			if ( existingError.TryGetValue( HttpErrorKeys.MessageKey, out value ) )
			{
				error.Message = GetStringValue( value );
			}

			if ( existingError.TryGetValue( HttpErrorKeys.ExceptionMessageKey, out value ) )
			{
				error.ExceptionMessage = GetStringValue( value );
			}

			if ( existingError.TryGetValue( HttpErrorKeys.ExceptionTypeKey, out value ) )
			{
				error.ExceptionType = GetStringValue( value );
			}

			if ( existingError.TryGetValue( HttpErrorKeys.StackTraceKey, out value ) )
			{
				error.StackTrace = GetStringValue( value );
			}

			if ( existingError.TryGetValue( AdditionalInfoKey, out value ) )
			{
				error [ AdditionalInfoKey ] = GetStringValue( value );
			}

			if ( existingError.TryGetValue( PasswordExpiredKey, out value ) )
			{
				error [ PasswordExpiredKey ] = GetStringValue( value );
			}

			if ( existingError.TryGetValue( ValidationErrorKey, out value ) )
			{
				error [ ValidationErrorKey ] = GetStringValue( value );
			}

			return error;
		}

		/// <summary>
		///		Gets the string value.
		/// </summary>
		/// <param name="jsonObject">The JSON object.</param>
		/// <returns></returns>
		private static string GetStringValue( object jsonObject )
		{
			if ( jsonObject == null )
			{
				return null;
			}

			string stringValue = jsonObject.ToString( );

			if ( string.IsNullOrEmpty( stringValue ) )
			{
				return stringValue;
			}

			if ( stringValue.StartsWith( "\"" ) )
			{
				stringValue = stringValue.Substring( 1 );
			}

			if ( stringValue.EndsWith( "\"" ) )
			{
				stringValue = stringValue.Substring( 0, stringValue.Length - 1 );
			}

			return stringValue;
		}
	}
}