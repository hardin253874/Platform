// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Defines the direction for retrieval in the event log.
	/// </summary>
	[Serializable]
	public enum EventLogDirection
	{
		/// <summary>
		///     Specifies the reference point is unknown.
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     Specifies entries should be retrieved from the first to the last entry.
		/// </summary>
		FirstToLast,

		/// <summary>
		///     Specifies entries should be retrieved from the last to the first entry.
		/// </summary>
		LastToFirst
	}
}