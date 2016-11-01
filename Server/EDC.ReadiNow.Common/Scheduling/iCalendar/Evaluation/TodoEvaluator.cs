// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     TodoEvaluator class.
	/// </summary>
	public class TodoEvaluator : RecurringEvaluator
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TodoEvaluator" /> class.
		/// </summary>
		/// <param name="todo">The todo.</param>
		public TodoEvaluator( ITodo todo )
			: base( todo )
		{
		}

		/// <summary>
		///     Gets the todo.
		/// </summary>
		/// <value>
		///     The todo.
		/// </value>
		protected ITodo Todo
		{
			get
			{
				return Recurrable as ITodo;
			}
		}

		/// <summary>
		///     Determines the starting recurrence.
		/// </summary>
		/// <param name="periodList">The period list.</param>
		/// <param name="dt">The date time.</param>
		/// <exception cref="System.Exception">Could not determine starting recurrence: a period evaluator could not be found!</exception>
		public void DetermineStartingRecurrence( IPeriodList periodList, ref IDateTime dt )
		{
			var evaluator = periodList.GetService( typeof ( IEvaluator ) ) as IEvaluator;
			if ( evaluator == null )
			{
				// FIXME: throw a specific, typed exception here.
				throw new Exception( "Could not determine starting recurrence: a period evaluator could not be found!" );
			}

			foreach ( IPeriod p in evaluator.Periods )
			{
				if ( p.StartTime.LessThan( dt ) )
				{
					dt = p.StartTime;
				}
			}
		}

		/// <summary>
		///     Determines the starting recurrence.
		/// </summary>
		/// <param name="recur">The recur.</param>
		/// <param name="dt">The date time.</param>
		public void DetermineStartingRecurrence( IRecurrencePattern recur, ref IDateTime dt )
		{
			if ( recur.Count != int.MinValue )
			{
				dt = Todo.Start.Copy<IDateTime>( );
			}
			else
			{
				DateTime dtVal = dt.Value;
				IncrementDate( ref dtVal, recur, -recur.Interval );
				dt.Value = dtVal;
			}
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
			// TODO items can only recur if a start date is specified
			if ( Todo.Start != null )
			{
				base.Evaluate( referenceDate, periodStart, periodEnd, includeReferenceDateInResults );

				// Ensure each period has a duration
				foreach ( IPeriod p in Periods )
				{
					if ( p.EndTime == null )
					{
						p.Duration = Todo.Duration;
						p.EndTime = p.StartTime.Add( Todo.Duration );
					}
				}

				return Periods;
			}
			return new HashSet<IPeriod>( );
		}

		/// <summary>
		///     Evaluates to previous occurrence.
		/// </summary>
		/// <param name="completedDate">The completed date.</param>
		/// <param name="currentDateTime">The current date time.</param>
		public void EvaluateToPreviousOccurrence( IDateTime completedDate, IDateTime currentDateTime )
		{
			var beginningDate = completedDate.Copy<IDateTime>( );

			if ( Todo.RecurrenceRules != null )
			{
				foreach ( IRecurrencePattern rrule in Todo.RecurrenceRules )
				{
					DetermineStartingRecurrence( rrule, ref beginningDate );
				}
			}
			if ( Todo.RecurrenceDates != null )
			{
				foreach ( IPeriodList rdate in Todo.RecurrenceDates )
				{
					DetermineStartingRecurrence( rdate, ref beginningDate );
				}
			}
			if ( Todo.ExceptionRules != null )
			{
				foreach ( IRecurrencePattern exrule in Todo.ExceptionRules )
				{
					DetermineStartingRecurrence( exrule, ref beginningDate );
				}
			}
			if ( Todo.ExceptionDates != null )
			{
				foreach ( IPeriodList exdate in Todo.ExceptionDates )
				{
					DetermineStartingRecurrence( exdate, ref beginningDate );
				}
			}

			Evaluate( Todo.Start, DateUtil.GetSimpleDateTimeData( beginningDate ), DateUtil.GetSimpleDateTimeData( currentDateTime ).AddTicks( 1 ), true );
		}
	}
}