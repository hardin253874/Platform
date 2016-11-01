// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Recurring Evaluator.
	/// </summary>
	public class RecurringEvaluator : Evaluator
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RecurringEvaluator" /> class.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public RecurringEvaluator( IRecurrable obj )
		{
			Recurrable = obj;

			// We're not sure if the object is a calendar object
			// or a calendar data type, so we need to assign
			// the associated object manually
			var calendarObject = obj as ICalendarObject;

			if ( calendarObject != null )
			{
// ReSharper disable DoNotCallOverridableMethodsInConstructor
				AssociatedObject = calendarObject;
// ReSharper restore DoNotCallOverridableMethodsInConstructor
			}

// ReSharper disable SuspiciousTypeConversion.Global
			var calendarDataType = obj as ICalendarDataType;
// ReSharper restore SuspiciousTypeConversion.Global

			if ( calendarDataType != null )
			{
				ICalendarDataType dt = calendarDataType;
// ReSharper disable DoNotCallOverridableMethodsInConstructor
				AssociatedObject = dt.AssociatedObject;
// ReSharper restore DoNotCallOverridableMethodsInConstructor
			}
		}

		/// <summary>
		///     Gets or sets the recurrable.
		/// </summary>
		/// <value>
		///     The recurrable.
		/// </value>
		protected IRecurrable Recurrable
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
			// Evaluate extra time periods, without re-evaluating ones that were already evaluated
			if ( ( EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue ) ||
			     ( periodEnd.Equals( EvaluationStartBounds ) ) ||
			     ( periodStart.Equals( EvaluationEndBounds ) ) )
			{
				EvaluateRRule( referenceDate, periodStart, periodEnd, includeReferenceDateInResults );
				EvaluateRDate( referenceDate, periodStart, periodEnd );
				EvaluateExRule( referenceDate, periodStart, periodEnd );
				EvaluateExDate( referenceDate, periodStart, periodEnd );
				if ( EvaluationStartBounds == DateTime.MaxValue || EvaluationStartBounds > periodStart )
				{
					EvaluationStartBounds = periodStart;
				}
				if ( EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < periodEnd )
				{
					EvaluationEndBounds = periodEnd;
				}
			}
			else
			{
				if ( EvaluationStartBounds != DateTime.MaxValue && periodStart < EvaluationStartBounds )
				{
					Evaluate( referenceDate, periodStart, EvaluationStartBounds, includeReferenceDateInResults );
				}
				if ( EvaluationEndBounds != DateTime.MinValue && periodEnd > EvaluationEndBounds )
				{
					Evaluate( referenceDate, EvaluationEndBounds, periodEnd, includeReferenceDateInResults );
				}
			}

			// Sort the list
			// TODO: This was commented out as part of the performance changes. Not sure the consequences.
			//PeriodsMember.Sort( );

			return Periods;
		}

		/// <summary>
		///     Evaluates the ExDate component, and excludes each specified DateTime or
		///     Period from the <see cref="Evaluator.Periods" /> collection.
		/// </summary>
		/// <param name="referenceDate">The reference date.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		protected virtual void EvaluateExDate( IDateTime referenceDate, DateTime periodStart, DateTime periodEnd )
		{
			// Handle EXDATEs
			if ( Recurrable.ExceptionDates != null )
			{
				foreach ( IPeriodList exdate in Recurrable.ExceptionDates )
				{
					var evaluator = exdate.GetService( typeof ( IEvaluator ) ) as IEvaluator;
					if ( evaluator != null )
					{
						ISet<IPeriod> periods = evaluator.Evaluate( referenceDate, periodStart, periodEnd, false );
						foreach ( IPeriod p in periods )
						{
							// If no time was provided for the ExDate, then it excludes the entire day
							if ( !p.StartTime.HasTime || ( p.EndTime != null && !p.EndTime.HasTime ) )
							{
								p.MatchesDateOnly = true;
							}

							Periods.Remove( p );
						}
					}
				}
			}
		}

		/// <summary>
		///     Evaluates the ExRule component, and excludes each specified DateTime
		///     from the <see cref="Evaluator.Periods" /> collection.
		/// </summary>
		/// <param name="referenceDate">The reference date.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		protected virtual void EvaluateExRule( IDateTime referenceDate, DateTime periodStart, DateTime periodEnd )
		{
			// Handle EXRULEs
			if ( Recurrable.ExceptionRules != null )
			{
				foreach ( IRecurrencePattern exrule in Recurrable.ExceptionRules )
				{
					var evaluator = exrule.GetService( typeof ( IEvaluator ) ) as IEvaluator;
					if ( evaluator != null )
					{
						ISet<IPeriod> periods = evaluator.Evaluate( referenceDate, periodStart, periodEnd, false );
						foreach ( IPeriod p in periods )
						{
							Periods.Remove( p );
						}
					}
				}
			}
		}

		/// <summary>
		///     Evaluates the RDate component, and adds each specified DateTime or
		///     Period to the <see cref="Evaluator.Periods" /> collection.
		/// </summary>
		/// <param name="referenceDate">The reference date.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		protected virtual void EvaluateRDate( IDateTime referenceDate, DateTime periodStart, DateTime periodEnd )
		{
			// Handle RDATEs
			if ( Recurrable.RecurrenceDates != null )
			{
				foreach ( IPeriodList rdate in Recurrable.RecurrenceDates )
				{
					var evaluator = rdate.GetService( typeof ( IEvaluator ) ) as IEvaluator;
					if ( evaluator != null )
					{
						ISet<IPeriod> periods = evaluator.Evaluate( referenceDate, periodStart, periodEnd, false );
						foreach ( IPeriod p in periods )
						{
							Periods.Add( p );
						}
					}
				}
			}
		}

		/// <summary>
		///     Evaluates the RRule component, and adds each specified Period
		///     to the <see cref="Evaluator.Periods" /> collection.
		/// </summary>
		/// <param name="referenceDate">The reference date.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		/// <param name="includeReferenceDateInResults">
		///     if set to <c>true</c> [include reference date in results].
		/// </param>
		protected virtual void EvaluateRRule( IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults )
		{
			// Handle RRULEs
			if ( Recurrable.RecurrenceRules != null &&
			     Recurrable.RecurrenceRules.Count > 0 )
			{
				foreach ( IRecurrencePattern rrule in Recurrable.RecurrenceRules )
				{
					var evaluator = rrule.GetService( typeof ( IEvaluator ) ) as IEvaluator;
					if ( evaluator != null )
					{
						ISet<IPeriod> periods = evaluator.Evaluate( referenceDate, periodStart, periodEnd, includeReferenceDateInResults );
						foreach ( IPeriod p in periods )
						{
							Periods.Add( p );
						}
					}
				}
			}
			else if ( includeReferenceDateInResults )
			{
				// If no RRULEs were found, then we still need to add
				// the initial reference date to the results.
				IPeriod p = new Period( referenceDate.Copy<IDateTime>( ) );
				Periods.Add( p );
			}
		}
	}
}