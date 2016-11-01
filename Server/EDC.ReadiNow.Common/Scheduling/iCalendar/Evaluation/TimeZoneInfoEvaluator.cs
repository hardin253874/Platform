// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     TimeZoneInfoEvaluator class.
	/// </summary>
	public class TimeZoneInfoEvaluator :
		RecurringEvaluator
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TimeZoneInfoEvaluator" /> class.
		/// </summary>
		/// <param name="tzi">The time zone info.</param>
		public TimeZoneInfoEvaluator( ITimeZoneInfo tzi )
			: base( tzi )
		{
		}

		/// <summary>
		///     Gets or sets the time zone info.
		/// </summary>
		/// <value>
		///     The time zone info.
		/// </value>
		protected ITimeZoneInfo TimeZoneInfo
		{
			get
			{
				return Recurrable as ITimeZoneInfo;
			}
			set
			{
				Recurrable = value;
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
			// Time zones must include an effective start date/time
			// and must provide an evaluator.
			if ( TimeZoneInfo != null )
			{
				// Always include the reference date in the results
				ISet<IPeriod> periods = base.Evaluate( referenceDate, periodStart, periodEnd, true );
				return periods;
			}

			return new HashSet<IPeriod>( );
		}
	}
}