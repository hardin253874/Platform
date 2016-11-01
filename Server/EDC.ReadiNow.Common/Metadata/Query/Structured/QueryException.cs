// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
	/// <summary>
	///     Query exception class.
	/// </summary>
	public class QueryException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="QueryException" /> class.
		/// </summary>
		public QueryException( )
			: base( "A query exception has occurred." )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="QueryException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public QueryException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="QueryException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public QueryException( string message, Exception inner )
			: base( message, inner )
		{
		}

		/// <summary>
		///     A constructor is needed for serialization when an exception propagates from a remoting server to the client.
		/// </summary>
		/// <param name="info">
		///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the
		///     exception being thrown.
		/// </param>
		/// <param name="context">
		///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the
		///     source or destination.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		///     The <paramref name="info" /> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		///     The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0).
		/// </exception>
		protected QueryException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}