// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Base class for registry exceptions.
	/// </summary>
	/// <seealso cref="Exception" />
	public class RegistryException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryException" /> class.
		/// </summary>
		public RegistryException( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public RegistryException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public RegistryException( string message, Exception inner )
			: base( message, inner )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryException" /> class.
		/// </summary>
		/// <param name="info">
		///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
		///     data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
		///     information about the source or destination.
		/// </param>
		public RegistryException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}