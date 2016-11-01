// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Specialized;
using System.ServiceModel;
using System.ServiceModel.Channels;
using EDC.Globalization;
using EDC.ReadiNow.AppLibrary;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Resources;
using EDC.ReadiNow.Security;
using RequestContext = EDC.ReadiNow.IO.RequestContext;

namespace EDC.ReadiNow.Services
{
	using AppRequestContext = RequestContext;

	/// <summary>
	///     Provides helper methods for interacting with services.
	/// </summary>
	public static class ServicesHelper
	{
		/// <summary>
		///     Gets whether the request context data is set.
		/// </summary>
		public static bool IsRequestContextDataSet
		{
			get
			{
				return AppRequestContext.IsSet;
			}
		}

		/// <summary>
		///     Adds any custom headers to the client channel.
		/// </summary>
		/// <param name="client">
		///     An object representing a service client.
		/// </param>
		public static void AddCustomHeaders<TChannel>( ClientBase<TChannel> client ) where TChannel : class
		{
			if ( client == null )
			{
				throw new ArgumentNullException( "client" );
			}

			// Add the custom application culture header
			string culture = CultureHelper.GetUiThreadCulture( CultureType.Neutral );
			MessageHeader header = MessageHeader.CreateHeader( "application-culture", "", culture );

			OperationContext.Current.OutgoingMessageHeaders.Add( header );
		}

		/// <summary>
		///     Adds any custom headers to the client channel.
		/// </summary>
		/// <typeparam name="TChannel">The type of the channel.</typeparam>
		/// <param name="client">An object representing a service client.</param>
		/// <param name="headers">The headers.</param>
		/// <exception cref="System.ArgumentNullException">
		///     client
		///     or
		///     headers
		/// </exception>
		public static void AddCustomHeaders<TChannel>( ClientBase<TChannel> client, StringDictionary headers ) where TChannel : class
		{
			if ( client == null )
			{
				throw new ArgumentNullException( "client" );
			}

			if ( headers == null )
			{
				throw new ArgumentNullException( "headers" );
			}

			foreach ( string key in headers.Keys )
			{
				MessageHeader header = MessageHeader.CreateHeader( key, "", headers[ key ] );
				OperationContext.Current.OutgoingMessageHeaders.Add( header );
			}
		}

		/// <summary>
		///     Converts an exception to a fault.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns></returns>
		public static FaultException ConvertToFault( Exception exception )
		{
			Exception ex = exception;

			if ( ex is ResourceNotFoundException )
			{
				return new FaultException<NotFoundFault>( new NotFoundFault( ), ex.Message );
			}

			if ( ex is ResourceAlreadyExistsException )
			{
				return new FaultException<AlreadyExistsFault>( new AlreadyExistsFault( ), ex.Message );
			}

			if ( ex is CircularRelationshipException )
			{
				return new FaultException<CircularRelationshipFault>( new CircularRelationshipFault( ), ex.Message );
			}

			if ( ex is ValidationException )
			{
				var vEx = ex as ValidationException;
				string message = vEx.FieldName != null ? string.Format( "{0}: {1}", vEx.FieldName, vEx.Message ) : vEx.Message;

				return new FaultException<ValidationFault>( new ValidationFault( ), message );
			}

			if ( ex is DuplicateKeyException )
			{
				return new FaultException<DuplicateKeyFault>( new DuplicateKeyFault( ), ex.Message );
			}

			if ( ex is PlatformSecurityException )
			{
				return new FaultException<PlatformSecurityFault>( new PlatformSecurityFault( ), ex.Message );
			}

			if ( ex is ApplicationLibraryException )
			{
				return new FaultException<ApplicationLibraryFault>( new ApplicationLibraryFault( ), ex.Message );
			}

			return new FaultException( GlobalStrings.InternalError );
		}

		/// <summary>
		///     Frees the request data associated with the service request.
		/// </summary>
		public static void FreeRequestContextData( )
		{
			AppRequestContext.FreeContext( );
		}

		/// <summary>
		///     Gets the client time zone.
		/// </summary>
		/// <returns></returns>
		public static string GetClientTimeZone( )
		{
			if ( OperationContext.Current != null && OperationContext.Current.IncomingMessageHeaders != null )
			{
				int headerIndex = OperationContext.Current.IncomingMessageHeaders.FindHeader( "application-timezone", string.Empty );

				if ( headerIndex >= 0 )
				{
					return OperationContext.Current.IncomingMessageHeaders.GetHeader<string>( headerIndex );
				}
			}

			return null;
		}

		/// <summary>
		///     Gets the current culture.
		/// </summary>
		/// <returns></returns>
		public static string GetCurrentCulture( )
		{
			string culture = CultureHelper.GetUiThreadCulture( CultureType.Neutral );

			if ( OperationContext.Current != null && OperationContext.Current.IncomingMessageHeaders != null )
			{
				int headerIndex = OperationContext.Current.IncomingMessageHeaders.FindHeader( "application-culture", string.Empty );

				if ( headerIndex >= 0 )
				{
					culture = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>( headerIndex );
				}
			}

			return culture;
		}

		/// <summary>
		///     Initializes a service client for a request to the specified server type.
		/// </summary>
		/// <param name="client">
		///     An object representing a service client.
		/// </param>
		/// <param name="serviceAddress">
		///     A string containing the service address.
		/// </param>
		/// <param name="credentials">
		///     The credentials required to access the server.
		/// </param>
		public static void InitializeClient<TChannel>( ClientBase<TChannel> client, string serviceAddress, Credentials credentials ) where TChannel : class
		{
			InitializeClient( client, ConfigurationSettings.GetSiteConfigurationSection( ).SiteSettings.Address, serviceAddress, credentials );
		}

		/// <summary>
		///     Initializes a service client for a request to the specified server type.
		/// </summary>
		/// <param name="client">
		///     An object representing a service client.
		/// </param>
		/// <param name="server">
		///     An ID identifying the server to process the service request.
		/// </param>
		/// <param name="serviceAddress">
		///     A string containing the service address.
		/// </param>
		/// <param name="credentials">
		///     The credentials required to access the server.
		/// </param>
		public static void InitializeClient<TChannel>( ClientBase<TChannel> client, string server, string serviceAddress, Credentials credentials ) where TChannel : class
		{
			if ( client == null )
			{
				throw new ArgumentNullException( "client" );
			}

			if ( String.IsNullOrEmpty( server ) )
			{
				throw new ArgumentException( "The specified server parameter is invalid." );
			}

			if ( String.IsNullOrEmpty( serviceAddress ) )
			{
				throw new ArgumentException( "The specified serviceAddress parameter is invalid." );
			}

			// Get the server's default service address
			string address = string.Format( @"https://{0}{1}", server, ConfigurationSettings.GetSiteConfigurationSection( ).SiteSettings.ServiceRootAddress );

			// Initialize the endpoint address
			client.Endpoint.Address = new EndpointAddress( string.Format( "{0}/{1}.svc", address, serviceAddress ) );

			// Initialize the client credentials
			if ( client.ClientCredentials != null )
			{
				client.ClientCredentials.UserName.UserName = Credentials.GetFullyQualifiedName( credentials );
				client.ClientCredentials.UserName.Password = credentials.Password;
			}
		}

		/// <summary>
		///     Retrieves the fault exception that the specified exception maps to.
		/// </summary>
		/// <param name="ex">
		///     Exception to be retrieved as a fault.
		/// </param>
		/// <returns>
		///     A fault exception that represents the specified exception.
		/// </returns>
		public static FaultException RetrieveAsFault( Exception ex )
		{
			if ( ex == null )
			{
				throw new ArgumentNullException( "ex" );
			}

			EventLog.Application.WriteError( ex.ToString( ) );

			FaultException fault = ConvertToFault( ex );

			// assert fault != null
			return fault;
		}

		/// <summary>
		///     Sets the request context data associated with the service request.
		/// </summary>
		public static void SetRequestContextData( )
		{
			string culture = GetCurrentCulture( );
			string timeZone = GetClientTimeZone( );

			// Get the original context (set by the custom username validator)
			AppRequestContext requestContext = AppRequestContext.GetContext( );
			if ( requestContext != null )
			{
				// Extend the original context with culture information
				AppRequestContext.SetContext( requestContext.Identity, requestContext.Tenant, culture, timeZone );
			}
		}

		/// <summary>
		///     Usage:
		///     catch (Exception ex)
		///     {
		///     ServicesHelper.ThrowAsFault(ex);
		///     throw; // rethrow unknown exceptions
		///     }
		///     Note: the second 'throw' is there to rethrow exceptions that cannot be converted to standard faults.
		/// </summary>
		/// <param name="ex"></param>
		public static void ThrowAsFault( Exception ex )
		{
			throw RetrieveAsFault( ex );
		}

		/// <summary>
		///     Sets the request context data associated with the service request.
		/// </summary>
		internal static void SetRequestContextData( RequestContextData contextData )
		{
			AppRequestContext.SetContext( contextData );
		}
	}
}