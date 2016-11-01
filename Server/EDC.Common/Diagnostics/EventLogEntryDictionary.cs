// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Represents a dictionary of event log entries.
	/// </summary>
	[Serializable]
	public class EventLogEntryDictionary : SortedDictionary<Guid, EventLogEntry>
	{
		/// <summary>
		///     Creates and returns an identical copy of the dictionary.
		/// </summary>
		/// <returns>
		///     A copy of the dictionary.
		/// </returns>
		public EventLogEntryDictionary Copy( )
		{
			var copy = new EventLogEntryDictionary( );

			foreach ( Guid tag in Keys )
			{
				copy[ tag ] = this[ tag ];
			}

			return copy;
		}
	}
}