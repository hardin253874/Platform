// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     PeriodListEvaluator class.
	/// </summary>
	public class PeriodListEvaluator : Evaluator
	{
		/// <summary>
		///     Period list.
		/// </summary>
		private readonly IPeriodList _periodList;

		/// <summary>
		///     Initializes a new instance of the <see cref="PeriodListEvaluator" /> class.
		/// </summary>
		/// <param name="rdt">The RDT.</param>
		public PeriodListEvaluator( IPeriodList rdt )
		{
			_periodList = rdt;
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
			var periods = new HashSet<IPeriod>( );

			if ( includeReferenceDateInResults )
			{
				IPeriod p = new Period( referenceDate );
				periods.Add( p );
			}

			if ( periodEnd < periodStart )
			{
				return periods;
			}

			foreach ( IPeriod p in _periodList )
			{
				periods.Add( p );
			}

			return periods;
		}
	}
}