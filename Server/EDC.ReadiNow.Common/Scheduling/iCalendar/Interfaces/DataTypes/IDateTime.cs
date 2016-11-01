// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	public interface IDateTime : IEncodableDataType, IComparable<IDateTime>, IFormattable
	{
		/// <summary>
		///     Gets the date portion of the date/time value.
		/// </summary>
		DateTime Date
		{
			get;
		}

		/// <summary>
		///     Gets the day for this date/time value.
		/// </summary>
		int Day
		{
			get;
		}

		/// <summary>
		///     Gets the DayOfWeek for this date/time value.
		/// </summary>
		DayOfWeek DayOfWeek
		{
			get;
		}

		/// <summary>
		///     Gets the DayOfYear for this date/time value.
		/// </summary>
		int DayOfYear
		{
			get;
		}

		/// <summary>
		///     Gets the first day of the month currently represented by the IDateTime instance.
		/// </summary>
		IDateTime FirstDayOfMonth
		{
			get;
		}

		/// <summary>
		///     Gets the first day of the year currently represented by the IDateTime instance.
		/// </summary>
		IDateTime FirstDayOfYear
		{
			get;
		}

		/// <summary>
		///     Gets/sets whether or not this date/time value contains a 'date' part.
		/// </summary>
		bool HasDate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets whether or not this date/time value contains a 'time' part.
		/// </summary>
		bool HasTime
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the hour for this date/time value.
		/// </summary>
		int Hour
		{
			get;
		}

		/// <summary>
		///     Gets/sets whether the Value of this date/time represents
		///     a universal time.
		/// </summary>
		bool IsUniversalTime
		{
			get;
			set;
		}

		/// <summary>
		///     Converts the date/time to this computer's local date/time.
		/// </summary>
		DateTime Local
		{
			get;
		}

		/// <summary>
		///     Gets the millisecond for this date/time value.
		/// </summary>
		int Millisecond
		{
			get;
		}

		/// <summary>
		///     Gets the minute for this date/time value.
		/// </summary>
		int Minute
		{
			get;
		}

		/// <summary>
		///     Gets the month for this date/time value.
		/// </summary>
		int Month
		{
			get;
		}

		/// <summary>
		///     Gets the second for this date/time value.
		/// </summary>
		int Second
		{
			get;
		}

		/// <summary>
		///     Gets the ticks for this date/time value.
		/// </summary>
		long Ticks
		{
			get;
		}

		/// <summary>
		///     Gets the time portion of the date/time value.
		/// </summary>
		TimeSpan TimeOfDay
		{
			get;
		}

		/// <summary>
		///     Gets the time zone name this time is in, if it references a time zone.
		/// </summary>
		string TimeZoneName
		{
			get;
		}

		/// <summary>
		///     Retrieves the <see cref="iCalTimeZoneInfo" /> object for the time
		///     zone set by <see cref="TzId" />.
		/// </summary>
		TimeZoneObservance? TimeZoneObservance
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the time zone ID for this date/time value.
		/// </summary>
		string TzId
		{
			get;
			set;
		}

		/// <summary>
		///     Converts the date/time to UTC (Coordinated Universal Time)
		/// </summary>
		DateTime Utc
		{
			get;
		}

		/// <summary>
		///     Gets/sets the underlying DateTime value stored.  This should always
		///     use DateTimeKind.Utc, regardless of its actual representation.
		///     Use IsUniversalTime along with the TZID to control how this
		///     date/time is handled.
		/// </summary>
		DateTime Value
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the year for this date/time value.
		/// </summary>
		int Year
		{
			get;
		}

		IDateTime Add( TimeSpan ts );
		IDateTime AddDays( int days );
		IDateTime AddHours( int hours );
		IDateTime AddMilliseconds( int milliseconds );
		IDateTime AddMinutes( int minutes );
		IDateTime AddMonths( int months );
		IDateTime AddSeconds( int seconds );
		IDateTime AddTicks( long ticks );
		IDateTime AddYears( int years );
		void AssociateWith( IDateTime dt );

		bool GreaterThan( IDateTime dt );
		bool GreaterThanOrEqual( IDateTime dt );
		bool LessThan( IDateTime dt );
		bool LessThanOrEqual( IDateTime dt );
		IDateTime SetTimeZone( ITimeZone tz );
		IDateTime Subtract( TimeSpan ts );
		TimeSpan Subtract( IDateTime dt );

		string ToString( string format );

		/// <summary>
		///     Converts the date/time value to a local time
		///     within the specified time zone.
		/// </summary>
		IDateTime ToTimeZone( TimeZoneObservance tzo );

		/// <summary>
		///     Converts the date/time value to a local time
		///     within the specified time zone.
		/// </summary>
		IDateTime ToTimeZone( string tzid );

		IDateTime ToTimeZone( ITimeZone tz );
	}
}