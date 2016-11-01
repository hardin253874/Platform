// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Recurrence utility class.
	/// </summary>
	public class RecurrenceUtil
	{
		/// <summary>
		///     Clears the evaluation.
		/// </summary>
		/// <param name="recurrable">The recurrable.</param>
		public static void ClearEvaluation( IRecurrable recurrable )
		{
			var evaluator = recurrable.GetService( typeof ( IEvaluator ) ) as IEvaluator;
			if ( evaluator != null )
			{
				evaluator.Clear( );
			}
		}

		/// <summary>
		///     Gets the expand behavior list.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		public static bool?[] GetExpandBehaviorList( IRecurrencePattern p )
		{
			// See the table in RFC 5545 Section 3.3.10 (Page 43).
			switch ( p.Frequency )
			{
				case FrequencyType.Minutely:
					return new bool?[]
						{
							false, null, false, false, false, false, false, true, false
						};
				case FrequencyType.Hourly:
					return new bool?[]
						{
							false, null, false, false, false, false, true, true, false
						};
				case FrequencyType.Daily:
					return new bool?[]
						{
							false, null, null, false, false, true, true, true, false
						};
				case FrequencyType.Weekly:
					return new bool?[]
						{
							false, null, null, null, true, true, true, true, false
						};
				case FrequencyType.Monthly:
					{
						var row = new bool?[]
							{
								false, null, null, true, true, true, true, true, false
							};

						// Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
						if ( p.ByMonthDay.Count > 0 )
						{
							row[ 4 ] = false;
						}

						return row;
					}
				case FrequencyType.Yearly:
					{
						var row = new bool?[]
							{
								true, true, true, true, true, true, true, true, false
							};

						// Limit if BYYEARDAY or BYMONTHDAY is present; otherwise,
						// special expand for WEEKLY if BYWEEKNO present; otherwise,
						// special expand for MONTHLY if BYMONTH present; otherwise,
						// special expand for YEARLY.
						if ( p.ByYearDay.Count > 0 || p.ByMonthDay.Count > 0 )
						{
							row[ 4 ] = false;
						}

						return row;
					}
				default:
					return new bool?[]
						{
							false, null, false, false, false, false, false, false, false
						};
			}
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="recurrable">The recurrable.</param>
		/// <param name="dt">The date time.</param>
		/// <param name="includeReferenceDateInResults">
		///     if set to <c>true</c> [include reference date in results].
		/// </param>
		/// <returns></returns>
		public static IList<Occurrence> GetOccurrences( IRecurrable recurrable, IDateTime dt, bool includeReferenceDateInResults )
		{
			return GetOccurrences(
				recurrable,
				new iCalDateTime( dt.Local.Date ),
				new iCalDateTime( dt.Local.Date.AddDays( 1 ).AddSeconds( -1 ) ),
				includeReferenceDateInResults );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="recurrable">The recurrable.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		/// <param name="includeReferenceDateInResults">
		///     if set to <c>true</c> [include reference date in results].
		/// </param>
		/// <returns></returns>
		public static IList<Occurrence> GetOccurrences( IRecurrable recurrable, IDateTime periodStart, IDateTime periodEnd, bool includeReferenceDateInResults )
		{
			var occurrences = new List<Occurrence>( );

			var evaluator = recurrable.GetService( typeof ( IEvaluator ) ) as IEvaluator;
			if ( evaluator != null )
			{
				// Ensure the start time is associated with the object being queried
				var start = recurrable.Start.Copy<IDateTime>( );
				start.AssociatedObject = recurrable as ICalendarObject;

				// Change the time zone of periodStart/periodEnd as needed 
				// so they can be used during the evaluation process.
				periodStart = DateUtil.MatchTimeZone( start, periodStart );
				periodEnd = DateUtil.MatchTimeZone( start, periodEnd );

				ISet<IPeriod> periods = evaluator.Evaluate(
					start,
					DateUtil.GetSimpleDateTimeData( periodStart ),
					DateUtil.GetSimpleDateTimeData( periodEnd ),
					includeReferenceDateInResults );

				// Filter the resulting periods to only contain those 
				// that occur sometime between startTime and endTime.
				// NOTE: fixes bug #3007244 - GetOccurences not returning long spanning all-day events 
				occurrences.AddRange( from p in periods
				                      let endTime = p.EndTime ?? p.StartTime
				                      where endTime.GreaterThan( periodStart ) && p.StartTime.LessThanOrEqual( periodEnd )
				                      select new Occurrence( recurrable, p ) );

				occurrences.Sort( );
			}
			return occurrences;
		}
	}
}