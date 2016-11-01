// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Much of this code comes from iCal4j, as Ben Fortuna has done an
	///     excellent job with the recurrence pattern evaluation there.
	///     Here's the iCal4j license:
	///     ==================
	///     iCal4j - License
	///     ==================
	///     Copyright (c) 2009, Ben Fortuna
	///     All rights reserved.
	///     Redistribution and use in source and binary forms, with or without
	///     modification, are permitted provided that the following conditions
	///     are met:
	///     o Redistributions of source code must retain the above copyright
	///     notice, this list of conditions and the following disclaimer.
	///     o Redistributions in binary form must reproduce the above copyright
	///     notice, this list of conditions and the following disclaimer in the
	///     documentation and/or other materials provided with the distribution.
	///     o Neither the name of Ben Fortuna nor the names of any other contributors
	///     may be used to endorse or promote products derived from this software
	///     without specific prior written permission.
	///     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
	///     "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
	///     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
	///     A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
	///     CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
	///     EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
	///     PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
	///     PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
	///     LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
	///     NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	///     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	/// </summary>
	public class RecurrencePatternEvaluator : Evaluator
	{
		// FIXME: in ical4j this is configurable.
		private const int MaxIncrementCount = 1000;

		/// <summary>
		///     Initializes a new instance of the <see cref="RecurrencePatternEvaluator" /> class.
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		public RecurrencePatternEvaluator( IRecurrencePattern pattern )
		{
			Pattern = pattern;
		}

		/// <summary>
		///     Gets or sets the pattern.
		/// </summary>
		/// <value>
		///     The pattern.
		/// </value>
		protected IRecurrencePattern Pattern
		{
			get;
			set;
		}

		/// <summary>
		///     Evaluates the specified reference date.
		/// </summary>
		/// <param name="referenceDate">The reference date.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		/// <param name="includeReferenceDateInResults">
		///     if set to <c>true</c> [include reference date in results].
		/// </param>
		/// <returns></returns>
		public override ISet<IPeriod> Evaluate( IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults )
		{
			// Create a recurrence pattern suitable for use during evaluation.
			IRecurrencePattern pattern = ProcessRecurrencePattern( referenceDate );

			// Enforce evaluation restrictions on the pattern.
			EnforceEvaluationRestrictions( pattern );

			Periods.Clear( );
			foreach ( DateTime dt in GetDates( referenceDate, periodStart, periodEnd, -1, pattern, includeReferenceDateInResults ) )
			{
				// Create a period from the date/time.
				IPeriod p = CreatePeriod( dt, referenceDate );

				Periods.Add( p );
			}

			return Periods;
		}

		/// <summary>
		///     Applies the set pos rules.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <returns></returns>
		private List<DateTime> ApplySetPosRules( List<DateTime> dates, IRecurrencePattern pattern )
		{
			// return if no SETPOS rules specified..
			if ( pattern.BySetPosition.Count == 0 )
			{
				return dates;
			}

			// sort the list before processing..
			dates.Sort( );

			var setPosDates = new List<DateTime>( );
			int size = dates.Count;

			foreach ( int pos in pattern.BySetPosition )
			{
				if ( pos > 0 && pos <= size )
				{
					setPosDates.Add( dates[ pos - 1 ] );
				}
				else if ( pos < 0 && pos >= -size )
				{
					setPosDates.Add( dates[ size + pos ] );
				}
			}
			return setPosDates;
		}

		/// <summary>
		///     Creates the period.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="referenceDate">The reference date.</param>
		/// <returns></returns>
		private IPeriod CreatePeriod( DateTime dt, IDateTime referenceDate )
		{
			// Turn each resulting date/time into an IDateTime and associate it
			// with the reference date.
			IDateTime newDt = new iCalDateTime( dt, referenceDate.TzId );

			// NOTE: fixes bug #2938007 - hasTime missing
			newDt.HasTime = referenceDate.HasTime;

			newDt.AssociateWith( referenceDate );

			// Create a period from the new date/time.
			return new Period( newDt );
		}

		/// <summary>
		///     Enforces the evaluation restrictions.
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		/// <exception cref="EvaluationEngineException">
		/// </exception>
		private void EnforceEvaluationRestrictions( IRecurrencePattern pattern )
		{
			RecurrenceEvaluationModeType? evaluationMode = pattern.EvaluationMode;
			RecurrenceRestrictionType? evaluationRestriction = pattern.RestrictionType;

			if ( evaluationRestriction != RecurrenceRestrictionType.NoRestriction )
			{
				switch ( evaluationMode )
				{
					case RecurrenceEvaluationModeType.AdjustAutomatically:
						switch ( pattern.Frequency )
						{
							case FrequencyType.Secondly:
								{
									switch ( evaluationRestriction )
									{
										case RecurrenceRestrictionType.Default:
										case RecurrenceRestrictionType.RestrictSecondly:
											pattern.Frequency = FrequencyType.Minutely;
											break;
										case RecurrenceRestrictionType.RestrictMinutely:
											pattern.Frequency = FrequencyType.Hourly;
											break;
										case RecurrenceRestrictionType.RestrictHourly:
											pattern.Frequency = FrequencyType.Daily;
											break;
									}
								}
								break;
							case FrequencyType.Minutely:
								{
									switch ( evaluationRestriction )
									{
										case RecurrenceRestrictionType.RestrictMinutely:
											pattern.Frequency = FrequencyType.Hourly;
											break;
										case RecurrenceRestrictionType.RestrictHourly:
											pattern.Frequency = FrequencyType.Daily;
											break;
									}
								}
								break;
							case FrequencyType.Hourly:
								{
									switch ( evaluationRestriction )
									{
										case RecurrenceRestrictionType.RestrictHourly:
											pattern.Frequency = FrequencyType.Daily;
											break;
									}
								}
								break;
						}
						break;
					case RecurrenceEvaluationModeType.ThrowException:
					case RecurrenceEvaluationModeType.Default:
						switch ( pattern.Frequency )
						{
							case FrequencyType.Secondly:
								{
									switch ( evaluationRestriction )
									{
										case RecurrenceRestrictionType.Default:
										case RecurrenceRestrictionType.RestrictSecondly:
										case RecurrenceRestrictionType.RestrictMinutely:
										case RecurrenceRestrictionType.RestrictHourly:
											throw new EvaluationEngineException( );
									}
								}
								break;
							case FrequencyType.Minutely:
								{
									switch ( evaluationRestriction )
									{
										case RecurrenceRestrictionType.RestrictMinutely:
										case RecurrenceRestrictionType.RestrictHourly:
											throw new EvaluationEngineException( );
									}
								}
								break;
							case FrequencyType.Hourly:
								{
									switch ( evaluationRestriction )
									{
										case RecurrenceRestrictionType.RestrictHourly:
											throw new EvaluationEngineException( );
									}
								}
								break;
						}
						break;
				}
			}
		}

		/// <summary>
		///     Gets the abs week days.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <param name="weekDay">The week day.</param>
		/// <param name="pattern">The pattern.</param>
		/// <returns></returns>
		private IEnumerable<DateTime> GetAbsWeekDays( DateTime date, IWeekDay weekDay, IRecurrencePattern pattern )
		{
			var days = new List<DateTime>( );

			DayOfWeek dayOfWeek = weekDay.DayOfWeek;
			if ( pattern.Frequency == FrequencyType.Daily )
			{
				if ( date.DayOfWeek == dayOfWeek )
				{
					days.Add( date );
				}
			}
			else if ( pattern.Frequency == FrequencyType.Weekly || pattern.ByWeekNo.Count > 0 )
			{
				// Rewind to the first day of the week
				while ( date.DayOfWeek != pattern.FirstDayOfWeek )
				{
					date = date.AddDays( -1 );
				}

				// Step forward until we're on the day of week we're interested in
				while ( date.DayOfWeek != dayOfWeek )
				{
					date = date.AddDays( 1 );
				}

				days.Add( date );
			}
			else if ( pattern.Frequency == FrequencyType.Monthly || pattern.ByMonth.Count > 0 )
			{
				int month = date.Month;

				// construct a list of possible month days..
				date = date.AddDays( -date.Day + 1 );
				while ( date.DayOfWeek != dayOfWeek )
				{
					date = date.AddDays( 1 );
				}

				while ( date.Month == month )
				{
					days.Add( date );
					date = date.AddDays( 7 );
				}
			}
			else if ( pattern.Frequency == FrequencyType.Yearly )
			{
				int year = date.Year;

				// construct a list of possible year days..
				date = date.AddDays( -date.DayOfYear + 1 );
				while ( date.DayOfWeek != dayOfWeek )
				{
					date = date.AddDays( 1 );
				}

				while ( date.Year == year )
				{
					days.Add( date );
					date = date.AddDays( 7 );
				}
			}
			return GetOffsetDates( days, weekDay.Offset );
		}

		/// <summary>
		///     Returns a list of possible dates generated from the applicable BY* rules, using the specified date as a seed.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expandBehaviors">The expand behaviors.</param>
		/// <returns></returns>
		private List<DateTime> GetCandidates( DateTime date, IRecurrencePattern pattern, bool?[] expandBehaviors )
		{
			var dates = new List<DateTime>
				{
					date
				};
			dates = GetMonthVariants( dates, pattern, expandBehaviors[ 0 ] );
			dates = GetWeekNoVariants( dates, pattern, expandBehaviors[ 1 ] );
			dates = GetYearDayVariants( dates, pattern, expandBehaviors[ 2 ] );
			dates = GetMonthDayVariants( dates, pattern, expandBehaviors[ 3 ] );
			dates = GetDayVariants( dates, pattern, expandBehaviors[ 4 ] );
			dates = GetHourVariants( dates, pattern, expandBehaviors[ 5 ] );
			dates = GetMinuteVariants( dates, pattern, expandBehaviors[ 6 ] );
			dates = GetSecondVariants( dates, pattern, expandBehaviors[ 7 ] );
			dates = ApplySetPosRules( dates, pattern );
			return dates;
		}

		/// <summary>
		///     Gets the dates.
		/// </summary>
		/// <param name="seed">The seed.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		/// <param name="maxCount">The max count.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="includeReferenceDateInResults">
		///     if set to <c>true</c> [include reference date in results].
		/// </param>
		/// <returns></returns>
		private IEnumerable<DateTime> GetDates( IDateTime seed, DateTime periodStart, DateTime periodEnd, int maxCount, IRecurrencePattern pattern, bool includeReferenceDateInResults )
		{
			var dates = new List<DateTime>( );
			DateTime originalDate = DateUtil.GetSimpleDateTimeData( seed );
			DateTime seedCopy = DateUtil.GetSimpleDateTimeData( seed );

			if ( includeReferenceDateInResults )
			{
				dates.Add( seedCopy );
			}

			// If the interval is set to zero, or our count prevents us
			// from getting additional items, then return with the reference
			// date only.
			if ( pattern.Interval == 0 ||
			     ( pattern.Count != int.MinValue && pattern.Count <= dates.Count ) )
			{
				return dates;
			}

			// optimize the start time for selecting candidates
			// (only applicable where a COUNT is not specified)
			if ( pattern.Count == int.MinValue )
			{
				DateTime incremented = seedCopy;
				// FIXME: we can more aggressively increment here when
				// the difference between dates is greater.
				IncrementDate( ref incremented, pattern, pattern.Interval );
				while ( incremented < periodStart )
				{
					seedCopy = incremented;
					IncrementDate( ref incremented, pattern, pattern.Interval );
				}
			}

			bool?[] expandBehavior = RecurrenceUtil.GetExpandBehaviorList( pattern );

			int invalidCandidateCount = 0;
			int noCandidateIncrementCount = 0;
			DateTime candidate = DateTime.MinValue;
			while ( ( maxCount < 0 ) || ( dates.Count < maxCount ) )
			{
				if ( pattern.Until != DateTime.MinValue && candidate != DateTime.MinValue && candidate > pattern.Until )
				{
					break;
				}

				if ( candidate != DateTime.MinValue && candidate > periodEnd )
				{
					break;
				}

				if ( pattern.Count >= 1 && ( dates.Count + invalidCandidateCount ) >= pattern.Count )
				{
					break;
				}

				List<DateTime> candidates = GetCandidates( seedCopy, pattern, expandBehavior );
				if ( candidates.Count > 0 )
				{
					noCandidateIncrementCount = 0;

					// sort candidates for identifying when UNTIL date is exceeded..
					candidates.Sort( );

					foreach ( DateTime t in candidates )
					{
						candidate = t;

						// don't count candidates that occur before the original date..
						if ( candidate >= originalDate )
						{
							// candidates MAY occur before periodStart
							// For example, FREQ=YEARLY;BYWEEKNO=1 could return dates
							// from the previous year.
							//
							// candidates exclusive of periodEnd..
							if ( candidate >= periodEnd )
							{
								invalidCandidateCount++;
							}
							else if ( pattern.Count >= 1 && ( dates.Count + invalidCandidateCount ) >= pattern.Count )
							{
								break;
							}
							else if ( pattern.Until == DateTime.MinValue || candidate <= pattern.Until )
							{
								if ( !dates.Contains( candidate ) )
								{
									dates.Add( candidate );
								}
							}
						}
					}
				}
				else
				{
					noCandidateIncrementCount++;
					if ( ( noCandidateIncrementCount > MaxIncrementCount ) )
					{
						break;
					}
				}

				IncrementDate( ref seedCopy, pattern, pattern.Interval );
			}

			// sort final list..
			dates.Sort( );
			return dates;
		}

		/**
         * Applies BYSETPOS rules to <code>dates</code>. Valid positions are from 1 to the size of the date list. Invalid
         * positions are ignored.
         * @param dates
         */

		/**
         * Applies BYDAY rules specified in this Recur instance to the specified date list. If no BYDAY rules are specified
         * the date list is returned unmodified.
         * @param dates
         * @return
         */

		/// <summary>
		///     Gets the day variants.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		private List<DateTime> GetDayVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.ByDay.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				var weekDayDates = new List<DateTime>( );
				foreach ( DateTime date in dates )
				{
					foreach ( IWeekDay t in pattern.ByDay )
					{
						weekDayDates.AddRange( GetAbsWeekDays( date, t, pattern ) );
					}
				}

				return weekDayDates;
			}

			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( IWeekDay weekDay in pattern.ByDay )
				{
					if ( weekDay.DayOfWeek.Equals( date.DayOfWeek ) )
					{
						// If no offset is specified, simply test the day of week!
						// FIXME: test with offset...
						if ( date.DayOfWeek.Equals( weekDay.DayOfWeek ) )
						{
							goto Next;
						}
					}
				}
				dates.RemoveAt( i );
				Next:
				;
			}

			return dates;
		}

		/// <summary>
		///     Applies BYHOUR rules specified in this Recur instance to the specified date list. If no BYHOUR rules are specified the date list is returned unmodified.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		private List<DateTime> GetHourVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.ByHour.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				return ( from date in dates
				         from hour in pattern.ByHour
				         select date.AddHours( -date.Hour + hour ) ).ToList( );
			}

			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( int hour in pattern.ByHour )
				{
					if ( date.Hour == hour )
					{
						goto Next;
					}
				}
				// Remove unmatched dates
				dates.RemoveAt( i );
				Next:
				;
			}
			return dates;
		}

		/// <summary>
		///     Applies BYMINUTE rules specified in this Recur instance to the specified date list. If no BYMINUTE rules are specified the date list is returned unmodified.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		private List<DateTime> GetMinuteVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.ByMinute.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				return ( from date in dates
				         from minute in pattern.ByMinute
				         select date.AddMinutes( -date.Minute + minute ) ).ToList( );
			}

			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( int minute in pattern.ByMinute )
				{
					if ( date.Minute == minute )
					{
						goto Next;
					}
				}
				// Remove unmatched dates
				dates.RemoveAt( i );
				Next:
				;
			}
			return dates;
		}

		/// <summary>
		///     Gets the month day variants.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Invalid day of month:  + date +  (day  + monthDay + )</exception>
		private List<DateTime> GetMonthDayVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.ByMonthDay.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				return ( from date in dates
				         from monthDay in pattern.ByMonthDay
				         let daysInMonth = Calendar.GetDaysInMonth( date.Year, date.Month )
				         where Math.Abs( monthDay ) <= daysInMonth
				         select monthDay > 0 ? date.AddDays( -date.Day + monthDay ) : date.AddDays( -date.Day + 1 ).AddMonths( 1 ).AddDays( monthDay ) ).ToList( );
			}

			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( int monthDay in pattern.ByMonthDay )
				{
					int daysInMonth = Calendar.GetDaysInMonth( date.Year, date.Month );
					if ( Math.Abs( monthDay ) > daysInMonth )
					{
						throw new ArgumentException( "Invalid day of month: " + date + " (day " + monthDay + ")" );
					}

					// Account for positive or negative numbers
					DateTime newDate = monthDay > 0 ? date.AddDays( -date.Day + monthDay ) : date.AddDays( -date.Day + 1 ).AddMonths( 1 ).AddDays( monthDay );

					if ( newDate.Day.Equals( date.Day ) )
					{
						goto Next;
					}
				}

				Next:
				dates.RemoveAt( i );
			}

			return dates;
		}

		/// <summary>
		///     Gets the month variants.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		private List<DateTime> GetMonthVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.ByMonth.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				return ( from date in dates
				         from month in pattern.ByMonth
				         select date.AddMonths( month - date.Month ) ).ToList( );
			}
			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( int t in pattern.ByMonth )
				{
					if ( date.Month == t )
					{
						goto Next;
					}
				}
				dates.RemoveAt( i );
				Next:
				;
			}
			return dates;
		}

		/// <summary>
		///     Gets the offset dates.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="offset">The offset.</param>
		/// <returns></returns>
		private IEnumerable<DateTime> GetOffsetDates( List<DateTime> dates, int offset )
		{
			if ( offset == int.MinValue )
			{
				return dates;
			}

			var offsetDates = new List<DateTime>( );
			int size = dates.Count;
			if ( offset < 0 && offset >= -size )
			{
				offsetDates.Add( dates[ size + offset ] );
			}
			else if ( offset > 0 && offset <= size )
			{
				offsetDates.Add( dates[ offset - 1 ] );
			}
			return offsetDates;
		}

		/// <summary>
		///     Applies BYSECOND rules specified in this Recur instance to the specified date list. If no BYSECOND rules are specified the date list is returned unmodified.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		private List<DateTime> GetSecondVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.BySecond.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				return ( from date in dates
				         from second in pattern.BySecond
				         select date.AddSeconds( -date.Second + second ) ).ToList( );
			}

			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( int second in pattern.BySecond )
				{
					if ( date.Second == second )
					{
						goto Next;
					}
				}
				// Remove unmatched dates
				dates.RemoveAt( i );
				Next:
				;
			}
			return dates;
		}

		/// <summary>
		///     Gets the week no variants.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		private List<DateTime> GetWeekNoVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.ByWeekNo.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				var weekNoDates = new List<DateTime>( );
				foreach ( DateTime t in dates )
				{
					DateTime date = t;
					foreach ( int weekNo in pattern.ByWeekNo )
					{
						// Determine our current week number
						int currWeekNo = Calendar.GetWeekOfYear( date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek );
						while ( currWeekNo > weekNo )
						{
							// If currWeekNo > weekNo, then we're likely at the start of a year
							// where currWeekNo could be 52 or 53.  If we simply step ahead 7 days
							// we should be back to week 1, where we can easily make the calculation
							// to move to weekNo.
							date = date.AddDays( 7 );
							currWeekNo = Calendar.GetWeekOfYear( date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek );
						}

						// Move ahead to the correct week of the year
						date = date.AddDays( ( weekNo - currWeekNo ) * 7 );

						// Step backward single days until we're at the correct DayOfWeek
						while ( date.DayOfWeek != pattern.FirstDayOfWeek )
						{
							date = date.AddDays( -1 );
						}

						for ( int k = 0; k < 7; k++ )
						{
							weekNoDates.Add( date );
							date = date.AddDays( 1 );
						}
					}
				}
				return weekNoDates;
			}

			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( int weekNo in pattern.ByWeekNo )
				{
					// Determine our current week number
					int currWeekNo = Calendar.GetWeekOfYear( date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek );

					if ( weekNo == currWeekNo )
					{
						goto Next;
					}
				}

				dates.RemoveAt( i );
				Next:
				;
			}
			return dates;
		}

		/// <summary>
		///     Applies BYYEARDAY rules specified in this Recur instance to the specified date list. If no BYYEARDAY rules are specified the date list is returned unmodified.
		/// </summary>
		/// <param name="dates">The dates.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="expand">The expand.</param>
		/// <returns></returns>
		private List<DateTime> GetYearDayVariants( List<DateTime> dates, IRecurrencePattern pattern, bool? expand )
		{
			if ( expand == null || pattern.ByYearDay.Count == 0 )
			{
				return dates;
			}

			if ( expand.Value )
			{
				// Expand behavior
				return ( from date in dates
				         from yearDay in pattern.ByYearDay
				         select yearDay > 0 ? date.AddDays( -date.DayOfYear + yearDay ) : date.AddDays( -date.DayOfYear + 1 ).AddYears( 1 ).AddDays( yearDay ) ).ToList( );
			}

			// Limit behavior
			for ( int i = dates.Count - 1; i >= 0; i-- )
			{
				DateTime date = dates[ i ];
				foreach ( int yearDay in pattern.ByYearDay )
				{
					DateTime newDate = yearDay > 0 ? date.AddDays( -date.DayOfYear + yearDay ) : date.AddDays( -date.DayOfYear + 1 ).AddYears( 1 ).AddDays( yearDay );

					if ( newDate.DayOfYear == date.DayOfYear )
					{
						goto Next;
					}
				}

				dates.RemoveAt( i );
				Next:
				;
			}

			return dates;
		}

		/// <summary>
		///     Processes the recurrence pattern.
		/// </summary>
		/// <param name="referenceDate">The reference date.</param>
		/// <returns></returns>
		private IRecurrencePattern ProcessRecurrencePattern( IDateTime referenceDate )
		{
			var r = new RecurrencePattern( );
			r.CopyFrom( Pattern );

			// Convert the UNTIL value to a local date/time based on the time zone information that
			// is in the reference date
			if ( r.Until != DateTime.MinValue )
			{
				// Build an iCalDateTime with the correct time zone & calendar
				var until = new iCalDateTime( r.Until, referenceDate.TzId )
					{
						AssociatedObject = referenceDate.AssociatedObject
					};

				// Convert back to local time so time zone comparisons match
				r.Until = until.Local;
			}

			if ( r.Frequency > FrequencyType.Secondly &&
			     r.BySecond.Count == 0 &&
			     referenceDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */ )
			{
				r.BySecond.Add( referenceDate.Second );
			}
			if ( r.Frequency > FrequencyType.Minutely &&
			     r.ByMinute.Count == 0 &&
			     referenceDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */ )
			{
				r.ByMinute.Add( referenceDate.Minute );
			}
			if ( r.Frequency > FrequencyType.Hourly &&
			     r.ByHour.Count == 0 &&
			     referenceDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */ )
			{
				r.ByHour.Add( referenceDate.Hour );
			}

			// If BYDAY, BYYEARDAY, or BYWEEKNO is specified, then
			// we don't default BYDAY, BYMONTH or BYMONTHDAY
			if ( r.ByDay.Count == 0 )
			{
				// If the frequency is weekly, use the original date's day of week.
				// NOTE: fixes WeeklyCount1() and WeeklyUntil1() handling
				// If BYWEEKNO is specified and BYMONTHDAY/BYYEARDAY is not specified,
				// then let's add BYDAY to BYWEEKNO.
				// NOTE: fixes YearlyByWeekNoX() handling
				if ( r.Frequency == FrequencyType.Weekly ||
				     (
					     r.ByWeekNo.Count > 0 &&
					     r.ByMonthDay.Count == 0 &&
					     r.ByYearDay.Count == 0
				     ) )
				{
					r.ByDay.Add( new WeekDay( referenceDate.DayOfWeek ) );
				}

				// If BYMONTHDAY is not specified,
				// default to the current day of month.
				// NOTE: fixes YearlyByMonth1() handling, added BYYEARDAY exclusion
				// to fix YearlyCountByYearDay1() handling
				if ( r.Frequency > FrequencyType.Weekly &&
				     r.ByWeekNo.Count == 0 &&
				     r.ByYearDay.Count == 0 &&
				     r.ByMonthDay.Count == 0 )
				{
					r.ByMonthDay.Add( referenceDate.Day );
				}

				// If BYMONTH is not specified, default to
				// the current month.
				// NOTE: fixes YearlyCountByYearDay1() handling
				if ( r.Frequency > FrequencyType.Monthly &&
				     r.ByWeekNo.Count == 0 &&
				     r.ByYearDay.Count == 0 &&
				     r.ByMonth.Count == 0 )
				{
					r.ByMonth.Add( referenceDate.Month );
				}
			}

			return r;
		}
	}
}