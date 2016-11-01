// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.AppLibrary
{
	/// <summary>
	///     Application Publish exception.
	/// </summary>
	public class ApplicationPublishException : ApplicationLibraryException
	{
		/// <summary>
		///     Default message.
		/// </summary>
		private const string DefaultMessage = "A problem has been encountered publishing the application. Please retry at a later time or check server logs for details.";

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationPublishException" /> class.
		/// </summary>
		public ApplicationPublishException( )
			: base( DefaultMessage )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationPublishException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ApplicationPublishException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationPublishException" /> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public ApplicationPublishException( string message, Exception innerException )
			: base( message, innerException )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationPublishException" /> class.
		/// </summary>
		/// <param name="innerException">The inner exception.</param>
		public ApplicationPublishException( Exception innerException )
			: base( DefaultMessage, innerException )
		{
		}

		/// <summary>
		///     A constructor is needed for serialization when an exception propagates from a remoting server to the client.
		/// </summary>
		/// <param name="info">
		///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.
		/// </param>
		protected ApplicationPublishException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}