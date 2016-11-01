// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ISerializerFactory interface.
	/// </summary>
	public interface ISerializerFactory
	{
		/// <summary>
		///     Builds the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="ctx">The CTX.</param>
		/// <returns></returns>
		ISerializer Build( Type objectType, ISerializationContext ctx );
	}
}