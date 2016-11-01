// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     IStringSerializer interface.
	/// </summary>
	public interface IStringSerializer : ISerializer
	{
		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="textReader">The text reader.</param>
		/// <returns></returns>
		object Deserialize( TextReader textReader );

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		string SerializeToString( object obj );
	}
}