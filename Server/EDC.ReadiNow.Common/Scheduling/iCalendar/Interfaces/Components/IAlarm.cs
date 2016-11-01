// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IAlarm interface.
	/// </summary>
	public interface IAlarm : ICalendarComponent
	{
		/// <summary>
		///     Gets or sets the action.
		/// </summary>
		/// <value>
		///     The action.
		/// </value>
		AlarmAction Action
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the attachment.
		/// </summary>
		/// <value>
		///     The attachment.
		/// </value>
		IAttachment Attachment
		{
			get;
			set;
		}

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
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the duration.
		/// </summary>
		/// <value>
		///     The duration.
		/// </value>
		TimeSpan Duration
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the repeat.
		/// </summary>
		/// <value>
		///     The repeat.
		/// </value>
		int Repeat
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the summary.
		/// </summary>
		/// <value>
		///     The summary.
		/// </value>
		string Summary
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the trigger.
		/// </summary>
		/// <value>
		///     The trigger.
		/// </value>
		ITrigger Trigger
		{
			get;
			set;
		}

		/// <summary>
		///     Gets a list of alarm occurrences for the given recurring component, <paramref name="rc" />
		///     that occur between <paramref name="fromDate" /> and <paramref name="toDate" />.
		/// </summary>
		IList<AlarmOccurrence> GetOccurrences( IRecurringComponent rc, IDateTime fromDate, IDateTime toDate );

		/// <summary>
		///     Polls the <see cref="Alarm" /> component for alarms that have been triggered
		///     since the provided <paramref name="fromDate" /> date/time.  If <paramref name="fromDate" />
		///     is null, all triggered alarms will be returned.
		/// </summary>
		/// <param name="fromDate">The earliest date/time to poll triggered alarms for.</param>
		/// <param name="toDate">The latest date/time to poll triggered alarms for.</param>
		/// <returns>
		///     A list of <see cref="AlarmOccurrence" /> objects, each containing a triggered alarm.
		/// </returns>
		IList<AlarmOccurrence> Poll( IDateTime fromDate, IDateTime toDate );
	}
}