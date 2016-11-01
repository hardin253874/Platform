// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	public interface IEvent : IRecurringComponent
	{
		/// <summary>
		///     The end date/time of the event.
		///     <note>
		///         If the duration has not been set, but
		///         the start/end time of the event is available,
		///         the duration is automatically determined.
		///         Likewise, if an end time and duration are available,
		///         but a start time has not been set, the start time
		///         will be extrapolated.
		///     </note>
		/// </summary>
		IDateTime DtEnd
		{
			get;
			set;
		}

		/// <summary>
		///     The duration of the event.
		///     <note>
		///         If a start time and duration is available,
		///         the end time is automatically determined.
		///         Likewise, if the end time and duration is
		///         available, but a start time is not determined,
		///         the start time will be extrapolated from
		///         available information.
		///     </note>
		/// </summary>
		// NOTE: Duration is not supported by all systems,
		// (i.e. iPhone) and cannot co-exist with DTEnd.
		// RFC 5545 states:
		//
		//      ; either 'dtend' or 'duration' may appear in
		//      ; a 'eventprop', but 'dtend' and 'duration'
		//      ; MUST NOT occur in the same 'eventprop'
		//
		// Therefore, Duration is not serialized, as DTEnd
		// should always be extrapolated from the duration.
		TimeSpan Duration
		{
			get;
			set;
		}

		/// <summary>
		///     An alias to the DTEnd field (i.e. end date/time).
		/// </summary>
		/// <value>
		///     The end.
		/// </value>
		IDateTime End
		{
			get;
			set;
		}

		/// <summary>
		///     The geographic location (lat/long) of the event.
		/// </summary>
		/// <value>
		///     The geographic location.
		/// </value>
		IGeographicLocation GeographicLocation
		{
			get;
			set;
		}

		/// <summary>
		///     Returns true if the event is an all-day event.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is all day; otherwise, <c>false</c>.
		/// </value>
		bool IsAllDay
		{
			get;
			set;
		}

		/// <summary>
		///     The location of the event.
		/// </summary>
		/// <value>
		///     The location.
		/// </value>
		string Location
		{
			get;
			set;
		}

		/// <summary>
		///     Resources that will be used during the event.
		///     <example>Conference room #2</example>
		///     <example>Projector</example>
		/// </summary>
		/// <value>
		///     The resources.
		/// </value>
		IList<string> Resources
		{
			get;
			set;
		}

		/// <summary>
		///     The status of the event.
		/// </summary>
		/// <value>
		///     The status.
		/// </value>
		EventStatus Status
		{
			get;
			set;
		}

		/// <summary>
		///     The transparency of the event.  In other words,
		///     whether or not the period of time this event
		///     occupies can contain other events (transparent),
		///     or if the time cannot be scheduled for anything
		///     else (opaque).
		/// </summary>
		/// <value>
		///     The transparency.
		/// </value>
		TransparencyType Transparency
		{
			get;
			set;
		}

		/// <summary>
		///     Determines whether this instance is active.
		/// </summary>
		/// <returns>
		///     <c>true</c> if this instance is active; otherwise, <c>false</c>.
		/// </returns>
		bool IsActive( );
	}
}