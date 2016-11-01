// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.AppLibrary
{
	/// <summary>
	///     Application Library exception.
	/// </summary>
	public class ApplicationLibraryException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationLibraryException" /> class.
		/// </summary>
		public ApplicationLibraryException( )
			: base( "An unknown exception has occurred within the application library." )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationLibraryException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ApplicationLibraryException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationLibraryException" /> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public ApplicationLibraryException( string message, Exception innerException )
			: base( message, innerException )
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
		protected ApplicationLibraryException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}