// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Exception representing a registry key not found.
	/// </summary>
	/// <seealso cref="RegistryException" />
	public class RegistryKeyNotFoundException : RegistryException
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryKeyNotFoundException" /> class.
		/// </summary>
		public RegistryKeyNotFoundException( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryKeyNotFoundException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public RegistryKeyNotFoundException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryKeyNotFoundException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public RegistryKeyNotFoundException( string message, Exception inner )
			: base( message, inner )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RegistryKeyNotFoundException" /> class.
		/// </summary>
		/// <param name="info">
		///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
		///     data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
		///     information about the source or destination.
		/// </param>
		public RegistryKeyNotFoundException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}