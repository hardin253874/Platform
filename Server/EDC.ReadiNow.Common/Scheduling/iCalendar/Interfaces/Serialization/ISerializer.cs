// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ISerializer interface.
	/// </summary>
	public interface ISerializer : IServiceProvider
	{
		/// <summary>
		///     Gets or sets the serialization context.
		/// </summary>
		/// <value>
		///     The serialization context.
		/// </value>
		ISerializationContext SerializationContext
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the type of the target.
		/// </summary>
		/// <value>
		///     The type of the target.
		/// </value>
		Type TargetType
		{
			get;
		}

		/// <summary>
		///     Deserializes the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		object Deserialize( Stream stream, Encoding encoding );

		/// <summary>
		///     Serializes the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="encoding">The encoding.</param>
		void Serialize( object obj, Stream stream, Encoding encoding );
	}
}