// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	public interface ITimeZone :
		ICalendarComponent
	{
		/// <summary>
		///     Gets or sets the id.
		/// </summary>
		/// <value>
		///     The id.
		/// </value>
		string Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the last modified.
		/// </summary>
		/// <value>
		///     The last modified.
		/// </value>
		IDateTime LastModified
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the time zone info's.
		/// </summary>
		/// <value>
		///     The time zone info's.
		/// </value>
		ICalendarObjectList<ITimeZoneInfo> TimeZoneInfos
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the time zone id.
		/// </summary>
		/// <value>
		///     The time zone id.
		/// </value>
		string TzId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the time zone URL.
		/// </summary>
		/// <value>
		///     The time zone URL.
		/// </value>
		Uri TzUrl
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
		///     Gets the time zone observance.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		TimeZoneObservance? GetTimeZoneObservance( IDateTime dt );
	}
}