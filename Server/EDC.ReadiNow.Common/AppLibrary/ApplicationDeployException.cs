// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.AppLibrary
{
	/// <summary>
	///     Application deploy exception.
	/// </summary>
	public class ApplicationDeployException : ApplicationLibraryException
	{
		/// <summary>
		///     Default message.
		/// </summary>
		private const string DefaultMessage = "A problem has been encountered deploying the application. Please retry at a later time or check server logs for details.";

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDeployException" /> class.
		/// </summary>
		public ApplicationDeployException( )
			: base( DefaultMessage )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDeployException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ApplicationDeployException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDeployException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public ApplicationDeployException( string message, Exception innerException )
			: base( message, innerException )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDeployException" /> class.
		/// </summary>
		/// <param name="innerException">The inner exception.</param>
		public ApplicationDeployException( Exception innerException )
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
		protected ApplicationDeployException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}