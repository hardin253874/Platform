// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IOrganizer interface.
	/// </summary>
	public interface IOrganizer : IEncodableDataType
	{
		/// <summary>
		///     Gets or sets the name of the common.
		/// </summary>
		/// <value>
		///     The name of the common.
		/// </value>
		string CommonName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the directory entry.
		/// </summary>
		/// <value>
		///     The directory entry.
		/// </value>
		Uri DirectoryEntry
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the sent by.
		/// </summary>
		/// <value>
		///     The sent by.
		/// </value>
		Uri SentBy
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		Uri Value
		{
			get;
			set;
		}
	}
}