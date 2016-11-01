// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Event Evaluator.
	/// </summary>
	public class EventEvaluator : RecurringEvaluator
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EventEvaluator" /> class.
		/// </summary>
		/// <param name="evt">The event.</param>
		public EventEvaluator( IEvent evt )
			: base( evt )
		{
		}

		/// <summary>
		///     Gets or sets the event.
		/// </summary>
		/// <value>
		///     The event.
		/// </value>
		protected IEvent Event
		{
			get
			{
				return Recurrable as IEvent;
			}
			set
			{
				Recurrable = value;
			}
		}

		/// <summary>
		///     Evaluates this event to determine the dates and times for which the event occurs.
		///     This method only evaluates events which occur between <paramref name="periodStart" />
		///     and <paramref name="periodEnd" />; therefore, if you require a list of events which
		///     occur outside of this range, you must specify a <paramref name="periodStart" /> and
		///     <paramref name="periodEnd" /> which encapsulate the date(s) of interest.
		///     <note type="caution">
		///         For events with very complex recurrence rules, this method may be a bottleneck
		///         during processing time, especially when this method in called for a large number
		///         of events, in sequence, or for a very large time span.
		///     </note>
		/// </summary>
		/// <param name="referenceTime">The reference time.</param>
		/// <param name="periodStart">The period start.</param>
		/// <param name="periodEnd">The period end.</param>
		/// <param name="includeReferenceDateInResults">
		///     if set to <c>true</c> [include reference date in results].
		/// </param>
		/// <returns></returns>
		public override ISet<IPeriod> Evaluate( IDateTime referenceTime, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults )
		{
			// Evaluate recurrences normally
			base.Evaluate( referenceTime, periodStart, periodEnd, includeReferenceDateInResults );

			// Ensure each period has a duration
			foreach ( IPeriod p in Periods )
			{
				if ( p.EndTime == null )
				{
					p.Duration = Event.Duration;
					p.EndTime = p.StartTime.Add( Event.Duration );
				}
			}

			return Periods;
		}
	}
}