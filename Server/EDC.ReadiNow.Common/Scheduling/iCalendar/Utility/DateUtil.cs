// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Date Utility class.
	/// </summary>
	public class DateUtil
	{
		/// <summary>
		///     Adds the weeks.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="interval">The interval.</param>
		/// <param name="firstDayOfWeek">The first day of week.</param>
		/// <returns></returns>
		public static DateTime AddWeeks( DateTime dt, int interval, DayOfWeek firstDayOfWeek )
		{
			// NOTE: fixes WeeklyUntilWkst2() evaluation.
			// NOTE: simplified the execution of this - fixes bug #3119920 - missing weekly occurrences also
			dt = dt.AddDays( interval * 7 );

			while ( dt.DayOfWeek != firstDayOfWeek )
			{
				dt = dt.AddDays( -1 );
			}

			return dt;
		}

		/// <summary>
		///     Ends the of day.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public static IDateTime EndOfDay( IDateTime dt )
		{
			return StartOfDay( dt ).AddDays( 1 ).AddTicks( -1 );
		}

		/// <summary>
		///     Firsts the day of week.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="firstDayOfWeek">The first day of week.</param>
		/// <param name="offset">The offset.</param>
		/// <returns></returns>
		public static DateTime FirstDayOfWeek( DateTime dt, DayOfWeek firstDayOfWeek, out int offset )
		{
			offset = 0;
			while ( dt.DayOfWeek != firstDayOfWeek )
			{
				dt = dt.AddDays( -1 );
				offset++;
			}
			return dt;
		}

		/// <summary>
		///     Gets the simple date time data.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public static DateTime GetSimpleDateTimeData( IDateTime dt )
		{
			return DateTime.SpecifyKind( dt.Value, dt.IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local );
		}

		/// <summary>
		///     Matches the time zone.
		/// </summary>
		/// <param name="dt1">The DT1.</param>
		/// <param name="dt2">The DT2.</param>
		/// <returns></returns>
		public static IDateTime MatchTimeZone( IDateTime dt1, IDateTime dt2 )
		{
			Debug.Assert( dt1 != null && dt2 != null );

			// Associate the date/time with the first.
			var copy = dt2.Copy<IDateTime>( );
			copy.AssociateWith( dt1 );

			// If the dt1 time does not occur in the same time zone as the
			// dt2 time, then let's convert it so they can be used in the
			// same context (i.e. evaluation).
			if ( dt1.TzId != null )
			{
				if ( !string.Equals( dt1.TzId, copy.TzId ) )
				{
					return ( dt1.TimeZoneObservance != null ) ? copy.ToTimeZone( dt1.TimeZoneObservance.Value ) : copy.ToTimeZone( dt1.TzId );
				}

				return copy;
			}

			if ( dt1.IsUniversalTime )
			{
				// The first date/time is in UTC time, convert!
				return new iCalDateTime( copy.Utc );
			}

			// The first date/time is in local time, convert!
			return new iCalDateTime( copy.Local );
		}

		/// <summary>
		///     Simples the date time to match.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="toMatch">To match.</param>
		/// <returns></returns>
		public static DateTime SimpleDateTimeToMatch( IDateTime dt, IDateTime toMatch )
		{
			if ( toMatch.IsUniversalTime && dt.IsUniversalTime )
			{
				return dt.Value;
			}

			if ( toMatch.IsUniversalTime )
			{
				return dt.Value.ToUniversalTime( );
			}

			if ( dt.IsUniversalTime )
			{
				return dt.Value.ToLocalTime( );
			}

			return dt.Value;
		}

		/// <summary>
		///     Starts the of day.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public static IDateTime StartOfDay( IDateTime dt )
		{
			return dt.
				AddHours( -dt.Hour ).
				AddMinutes( -dt.Minute ).
				AddSeconds( -dt.Second );
		}
	}
}