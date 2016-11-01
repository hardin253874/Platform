// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ITimeZoneInfo interface.
	/// </summary>
	public interface ITimeZoneInfo :
		ICalendarComponent,
		IRecurrable
	{
		/// <summary>
		///     Gets or sets the offset from.
		/// </summary>
		/// <value>
		///     The offset from.
		/// </value>
		IUtcOffset OffsetFrom
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the offset to.
		/// </summary>
		/// <value>
		///     The offset to.
		/// </value>
		IUtcOffset OffsetTo
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the time zone.
		/// </summary>
		/// <value>
		///     The name of the time zone.
		/// </value>
		string TimeZoneName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the time zone names.
		/// </summary>
		/// <value>
		///     The time zone names.
		/// </value>
		IList<string> TimeZoneNames
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the time zone id.
		/// </summary>
		/// <value>
		///     The time zone id.
		/// </value>
		string TzId
		{
			get;
		}

		/// <summary>
		///     Returns true if this time zone info represents
		///     the observed time zone for the IDateTime value
		///     provided.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified date time]; otherwise, <c>false</c>.
		/// </returns>
		bool Contains( IDateTime dt );

		/// <summary>
		///     Returns the observance that corresponds to
		///     the date/time provided, or null if no matching
		///     observance could be found within this TimeZoneInfo.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		TimeZoneObservance? GetObservance( IDateTime dt );
	}
}