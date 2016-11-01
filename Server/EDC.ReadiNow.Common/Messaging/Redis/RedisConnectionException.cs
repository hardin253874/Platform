// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Redis Connection exception.
	/// </summary>
	public class RedisConnectionException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisConnectionException" /> class.
		/// </summary>
		public RedisConnectionException( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisConnectionException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public RedisConnectionException( string message ) : base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisConnectionException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">
		///     The exception that is the cause of the current exception, or a null reference (Nothing in
		///     Visual Basic) if no inner exception is specified.
		/// </param>
		public RedisConnectionException( string message, Exception innerException ) : base( message, innerException )
		{
		}
	}
}