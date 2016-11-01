// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     TimeZoneEvaluator evaluator.
	/// </summary>
	public class TimeZoneEvaluator : Evaluator
	{
		/// <summary>
		///     Occurrences.
		/// </summary>
		private List<Occurrence> _occurrences;

		private HashSet<Occurrence> _occurancesLookup;

		/// <summary>
		///     Initializes a new instance of the <see cref="TimeZoneEvaluator" /> class.
		/// </summary>
		/// <param name="tz">The time zone.</param>
		public TimeZoneEvaluator( ITimeZone tz )
		{
			TimeZone = tz;
			_occurrences = new List<Occurrence>( );
			_occurancesLookup = new HashSet<Occurrence>( );
		}

		/// <summary>
		///     Gets or sets the occurrences.
		/// </summary>
		/// <value>
		///     The occurrences.
		/// </value>
		public virtual List<Occurrence> Occurrences
		{
			get
			{
				return _occurrences;
			}
			set
			{
				_occurrences = value;
			}
		}

		/// <summary>
		///     Gets or sets the time zone.
		/// </summary>
		/// <value>
		///     The time zone.
		/// </value>
		protected ITimeZone TimeZone
		{
			get;
			set;
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public override void Clear( )
		{
			base.Clear( );
			_occurrences.Clear( );
			_occurancesLookup.Clear( );
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
			// Ensure the reference date is associated with the time zone
			if ( referenceDate.AssociatedObject == null )
			{
				referenceDate.AssociatedObject = TimeZone;
			}

			var infos = new List<ITimeZoneInfo>( TimeZone.TimeZoneInfos );

			// Evaluate extra time periods, without re-evaluating ones that were already evaluated
			if ( ( EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue ) ||
			     ( periodEnd.Equals( EvaluationStartBounds ) ) ||
			     ( periodStart.Equals( EvaluationEndBounds ) ) )
			{
				foreach ( ITimeZoneInfo curr in infos )
				{
					var evaluator = curr.GetService( typeof ( IEvaluator ) ) as IEvaluator;
					Debug.Assert( curr.Start != null, "TimeZoneInfo.Start must not be null." );
					Debug.Assert( curr.Start.TzId == null, "TimeZoneInfo.Start must not have a time zone reference." );
					Debug.Assert( evaluator != null, "TimeZoneInfo.GetService(typeof(IEvaluator)) must not be null." );

					// Time zones must include an effective start date/time
					// and must provide an evaluator.
					// Set the start bounds
					if ( EvaluationStartBounds > periodStart )
					{
						EvaluationStartBounds = periodStart;
					}

					// FIXME: 5 years is an arbitrary number, to eliminate the need
					// to recalculate time zone information as much as possible.
					DateTime offsetEnd = periodEnd.AddYears( 5 );

					// Adjust our reference date to never fall out of bounds with
					// the time zone information
					IDateTime tziReferenceDate = referenceDate;
					if ( tziReferenceDate.LessThan( curr.Start ) )
					{
						tziReferenceDate = curr.Start;
					}

					// Determine the UTC occurrences of the Time Zone observances
					ISet<IPeriod> periods = evaluator.Evaluate(
						tziReferenceDate,
						periodStart,
						offsetEnd,
						includeReferenceDateInResults );

					foreach ( IPeriod period in periods )
					{
						Periods.Add( period );

						var o = new Occurrence( curr, period );
						_occurancesLookup.Add( o );
					}

					if ( EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < offsetEnd )
					{
						EvaluationEndBounds = offsetEnd;
					}
				}

				ProcessOccurrences( referenceDate );
			}
			else
			{
				if ( EvaluationEndBounds != DateTime.MinValue && periodEnd > EvaluationEndBounds )
				{
					Evaluate( referenceDate, EvaluationEndBounds, periodEnd, includeReferenceDateInResults );
				}
			}

			return Periods;
		}

		/// <summary>
		///     Processes the occurrences.
		/// </summary>
		/// <param name="referenceDate">The reference date.</param>
		private void ProcessOccurrences( IDateTime referenceDate )
		{
			_occurrences.AddRange( _occurancesLookup );

			// Sort the occurrences by start time
			_occurrences.Sort(
				delegate( Occurrence o1, Occurrence o2 )
					{
						if ( o1.Period == null || o1.Period.StartTime == null )
						{
							return -1;
						}

						if ( o2.Period == null || o2.Period.StartTime == null )
						{
							return 1;
						}

						return o1.Period.StartTime.CompareTo( o2.Period.StartTime );
					}
				);

			for ( int i = 0; i < _occurrences.Count; i++ )
			{
				Occurrence curr = _occurrences[ i ];
				Occurrence? next = i < _occurrences.Count - 1 ? ( Occurrence? ) _occurrences[ i + 1 ] : null;

				// Determine end times for our periods, overwriting previously calculated end times.
				// This is important because we don't want to over-calculate our time zone information,
				// but simply calculate enough to be accurate.  When date/time ranges that are out of
				// normal working bounds are encountered, then occurrences are processed again, and
				// new end times are determined.
				curr.Period.EndTime = next != null ? next.Value.Period.StartTime.AddTicks( -1 ) : ConvertToIDateTime( EvaluationEndBounds, referenceDate );
			}
		}
	}
}