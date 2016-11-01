// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Migration.Storage
{
	/// <summary>
	///     The InvalidDatabaseException class.
	/// </summary>
	/// <seealso cref="Exception" />
	public class InvalidDatabaseException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="InvalidDatabaseException" /> class.
		/// </summary>
		public InvalidDatabaseException( )
			: base( "The specified file is not a valid application database." )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="InvalidDatabaseException" /> class.
		/// </summary>
		/// <param name="databaseFilePath">The database file path.</param>
		public InvalidDatabaseException( string databaseFilePath )
			: base( string.Format( "The specified file '{0}' is not a valid application database.", databaseFilePath ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="InvalidDatabaseException" /> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">
		///     The exception that is the cause of the current exception, or a null reference (Nothing in
		///     Visual Basic) if no inner exception is specified.
		/// </param>
		public InvalidDatabaseException( string message, Exception innerException )
			: base( message, innerException )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="InvalidDatabaseException" /> class.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <param name="context">The context.</param>
		protected InvalidDatabaseException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}