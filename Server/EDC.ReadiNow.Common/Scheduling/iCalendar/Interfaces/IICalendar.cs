// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IICalendar interface.
	/// </summary>
	public interface IICalendar : ICalendarComponent, IGetOccurrencesTyped, IGetFreeBusy, IMergeable
	{
		/// <summary>
		///     Gets/sets the calendar version.  Defaults to "2.0".
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		string Version
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the product ID for the calendar.
		/// </summary>
		/// <value>
		///     The product ID.
		/// </value>
		string ProductId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the scale of the calendar.
		/// </summary>
		/// <value>
		///     The scale.
		/// </value>
		string Scale
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the calendar method.
		/// </summary>
		/// <value>
		///     The method.
		/// </value>
		Methods Method
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the restriction on how evaluation of
		///     recurrence patterns occurs within this calendar.
		/// </summary>
		/// <value>
		///     The recurrence restriction.
		/// </value>
		RecurrenceRestrictionType RecurrenceRestriction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the evaluation mode during recurrence
		///     evaluation.  Default is ThrowException.
		/// </summary>
		/// <value>
		///     The recurrence evaluation mode.
		/// </value>
		RecurrenceEvaluationModeType RecurrenceEvaluationMode
		{
			get;
			set;
		}

		/// <summary>
		///     Creates a new component, and adds it
		///     to the calendar.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T Create<T>( ) where T : ICalendarComponent;

		/// <summary>
		///     Returns the time zone object that corresponds
		///     to the provided TZID, or null of no matching
		///     time zone could be found.
		/// </summary>
		/// <param name="timeZoneId">The time zone id.</param>
		/// <returns></returns>
		ITimeZone GetTimeZone( string timeZoneId );

		/// <summary>
		///     Gets a list of unique components contained in the calendar.
		/// </summary>
		/// <value>
		///     The unique components.
		/// </value>
		IUniqueComponentList<IUniqueComponent> UniqueComponents
		{
			get;
		}

		/// <summary>
		///     Gets a list of Events contained in the calendar.
		/// </summary>
		/// <value>
		///     The events.
		/// </value>
		IUniqueComponentList<IEvent> Events
		{
			get;
		}

		/// <summary>
		///     Gets a list of Free/Busy components contained in the calendar.
		/// </summary>
		/// <value>
		///     The free busy.
		/// </value>
		IUniqueComponentList<IFreeBusy> FreeBusy
		{
			get;
		}

		/// <summary>
		///     Gets a list of Journal entries contained in the calendar.
		/// </summary>
		/// <value>
		///     The journals.
		/// </value>
		ICalendarObjectList<IJournal> Journals
		{
			get;
		}

		/// <summary>
		///     Gets a list of time zones contained in the calendar.
		/// </summary>
		/// <value>
		///     The time zones.
		/// </value>
		ICalendarObjectList<ITimeZone> TimeZones
		{
			get;
		}

		/// <summary>
		///     Gets a list of To-do items contained in the calendar.
		/// </summary>
		/// <value>
		///     The to-dos.
		/// </value>
		IUniqueComponentList<ITodo> Todos
		{
			get;
		}

		/// <summary>
		///     Adds a system time zone to the iCalendar.  This time zone may
		///     then be used in date/time objects contained in the
		///     calendar.
		/// </summary>
		/// <param name="tzi">A System.TimeZoneInfo object to add to the calendar.</param>
		/// <returns>
		///     The time zone added to the calendar.
		/// </returns>
		ITimeZone AddTimeZone( TimeZoneInfo tzi );

		/// <summary>
		///     Adds the time zone.
		/// </summary>
		/// <param name="timeZoneInfo">The time zone info.</param>
		/// <param name="earliestDateTimeToSupport">The earliest date time to support.</param>
		/// <param name="includeHistoricalData">
		///     if set to <c>true</c> [include historical data].
		/// </param>
		/// <returns></returns>
		ITimeZone AddTimeZone( TimeZoneInfo timeZoneInfo, DateTime earliestDateTimeToSupport, bool includeHistoricalData );

		/// <summary>
		///     Adds the local system time zone to the iCalendar.
		///     This time zone may then be used in date/time
		///     objects contained in the calendar.
		/// </summary>
		/// <returns>
		///     The time zone added to the calendar.
		/// </returns>
		ITimeZone AddLocalTimeZone( );

		/// <summary>
		///     Adds the local time zone.
		/// </summary>
		/// <param name="earliestDateTimeToSupport">The earliest date time to support.</param>
		/// <param name="includeHistoricalData">
		///     if set to <c>true</c> [include historical data].
		/// </param>
		/// <returns></returns>
		ITimeZone AddLocalTimeZone( DateTime earliestDateTimeToSupport, bool includeHistoricalData );

	}
}