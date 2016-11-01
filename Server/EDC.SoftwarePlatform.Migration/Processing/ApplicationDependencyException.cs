// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     The application dependency exception class.
	/// </summary>
	/// <seealso cref="System.Exception" />
	public class ApplicationDependencyException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDependencyException" /> class.
		/// </summary>
		public ApplicationDependencyException( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDependencyException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ApplicationDependencyException( string message ) : base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDependencyException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public ApplicationDependencyException( string message, Exception innerException ) : base( message, innerException )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationDependencyException" /> class.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <param name="context">The context.</param>
		public ApplicationDependencyException( SerializationInfo info, StreamingContext context ) : base( info, context )
		{
		}
	}
}