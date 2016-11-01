// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IAttendee interface.
	/// </summary>
	public interface IAttendee : IEncodableDataType
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
		///     Gets the delegated from.
		/// </summary>
		/// <value>
		///     The delegated from.
		/// </value>
		IList<string> DelegatedFrom
		{
			get;
		}

		/// <summary>
		///     Gets the delegated to.
		/// </summary>
		/// <value>
		///     The delegated to.
		/// </value>
		IList<string> DelegatedTo
		{
			get;
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
		///     Gets the members.
		/// </summary>
		/// <value>
		///     The members.
		/// </value>
		IList<string> Members
		{
			get;
		}

		/// <summary>
		///     Gets or sets the participation status.
		/// </summary>
		/// <value>
		///     The participation status.
		/// </value>
		ParticipationStatus ParticipationStatus
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the role.
		/// </summary>
		/// <value>
		///     The role.
		/// </value>
		string Role
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="IAttendee" /> is RSVP.
		/// </summary>
		/// <value>
		///     <c>true</c> if RSVP; otherwise, <c>false</c>.
		/// </value>
		bool Rsvp
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
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		string Type
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