// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Defines the reference points for retrieval in the event log.
	/// </summary>
	[Serializable]
	public enum EventLogOrigin
	{
		/// <summary>
		///     Specifies the reference point is unknown.
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     Specifies the first event log entry
		/// </summary>
		First,

		/// <summary>
		///     Specifies the last event log entry
		/// </summary>
		Last
	}
}