// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	public interface IUniqueComponent : ICalendarComponent
	{
		/// <summary>
		///     Gets or sets the attendees.
		/// </summary>
		/// <value>
		///     The attendees.
		/// </value>
		IList<IAttendee> Attendees
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the comments.
		/// </summary>
		/// <value>
		///     The comments.
		/// </value>
		IList<string> Comments
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the date time stamp.
		/// </summary>
		/// <value>
		///     The date time stamp.
		/// </value>
		IDateTime DtStamp
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the organizer.
		/// </summary>
		/// <value>
		///     The organizer.
		/// </value>
		IOrganizer Organizer
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the request statuses.
		/// </summary>
		/// <value>
		///     The request statuses.
		/// </value>
		IList<IRequestStatus> RequestStatuses
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the UID.
		/// </summary>
		/// <value>
		///     The UID.
		/// </value>
		string Uid
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the URL.
		/// </summary>
		/// <value>
		///     The URL.
		/// </value>
		Uri Url
		{
			get;
			set;
		}

		/// <summary>
		///     Occurs when the UID has changed.
		/// </summary>
		event EventHandler<ObjectEventArgs<string, string>> UidChanged;
	}
}