// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Model
{
	[Serializable]
	public class DuplicateKeyException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DuplicateKeyException" /> class.
		/// </summary>
		public DuplicateKeyException( )
			: base( "Resource key data violation occurred." )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DuplicateKeyException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public DuplicateKeyException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DuplicateKeyException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public DuplicateKeyException( string message, Exception inner )
			: base( message, inner )
		{
		}


		/// <summary>
		///     Initializes a new instance of the <see cref="DuplicateKeyException" /> class.
		/// </summary>
		/// <param name="info">
		///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		///     The <paramref name="info" /> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		///     The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0).
		/// </exception>
		protected DuplicateKeyException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}