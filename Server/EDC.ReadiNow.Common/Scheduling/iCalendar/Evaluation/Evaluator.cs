// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Evaluator class.
	/// </summary>
	public abstract class Evaluator : IEvaluator
	{
		/// <summary>
		///     Associated Data Type.
		/// </summary>
		private readonly ICalendarDataType _associatedDataType;

		/// <summary>
		/// </summary>
		protected HashSet<IPeriod> PeriodsMember;

		/// <summary>
		///     Associated Object.
		/// </summary>
		private ICalendarObject _associatedObject;

		/// <summary>
		///     Calendar.
		/// </summary>
		private Calendar _calendar;

		/// <summary>
		///     Evaluation End Bounds.
		/// </summary>
		private DateTime _evaluationEndBounds = DateTime.MinValue;

		/// <summary>
		///     Evaluation Start Bounds.
		/// </summary>
		private DateTime _evaluationStartBounds = DateTime.MaxValue;

		/// <summary>
		///     Initializes a new instance of the <see cref="Evaluator" /> class.
		/// </summary>
		protected Evaluator( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Evaluator" /> class.
		/// </summary>
		/// <param name="associatedObject">The associated object.</param>
		protected Evaluator( ICalendarObject associatedObject )
		{
			_associatedObject = associatedObject;

			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Evaluator" /> class.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		protected Evaluator( ICalendarDataType dataType )
		{
			_associatedDataType = dataType;

			Initialize( );
		}

		/// <summary>
		///     Converts to I date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="referenceDate">The reference date.</param>
		/// <returns></returns>
		protected IDateTime ConvertToIDateTime( DateTime dt, IDateTime referenceDate )
		{
			IDateTime newDt = new iCalDateTime( dt, referenceDate.TzId );
			newDt.AssociateWith( referenceDate );
			return newDt;
		}

		/// <summary>
		///     Increments the date.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="interval">The interval.</param>
		/// <exception cref="System.Exception">
		///     Cannot evaluate with an interval of zero.  Please use an interval other than zero.
		///     or
		///     FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.
		/// </exception>
		protected void IncrementDate( ref DateTime dt, IRecurrencePattern pattern, int interval )
		{
			// FIXME: use a more specific exception.
			if ( interval == 0 )
			{
				throw new Exception( "Cannot evaluate with an interval of zero.  Please use an interval other than zero." );
			}

			DateTime old = dt;
			switch ( pattern.Frequency )
			{
				case FrequencyType.Secondly:
					dt = old.AddSeconds( interval );
					break;
				case FrequencyType.Minutely:
					dt = old.AddMinutes( interval );
					break;
				case FrequencyType.Hourly:
					dt = old.AddHours( interval );
					break;
				case FrequencyType.Daily:
					dt = old.AddDays( interval );
					break;
				case FrequencyType.Weekly:
					dt = DateUtil.AddWeeks( old, interval, pattern.FirstDayOfWeek );
					break;
				case FrequencyType.Monthly:
					dt = old.AddDays( -old.Day + 1 ).AddMonths( interval );
					break;
				case FrequencyType.Yearly:
					dt = old.AddDays( -old.DayOfYear + 1 ).AddYears( interval );
					break;
					// FIXME: use a more specific exception.
				default:
					throw new Exception( "FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence." );
			}
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			_calendar = CultureInfo.CurrentCulture.Calendar;
			PeriodsMember = new HashSet<IPeriod>( );
		}

		#region IEvaluator Members

		/// <summary>
		///     The system calendar that governs the evaluation rules.
		/// </summary>
		public Calendar Calendar
		{
			get
			{
				return _calendar;
			}
		}

		/// <summary>
		///     The start bounds of the evaluation.  This gives
		///     the first date/time that is covered by the evaluation.
		///     This together with EvaluationEndBounds determines
		///     what time frames have already been evaluated, so
		///     duplicate evaluation doesn't occur.
		/// </summary>
		public virtual DateTime EvaluationStartBounds
		{
			get
			{
				return _evaluationStartBounds;
			}
			set
			{
				_evaluationStartBounds = value;
			}
		}

		/// <summary>
		///     The end bounds of the evaluation.
		///     See <see cref="EvaluationStartBounds" /> for more info.
		/// </summary>
		public virtual DateTime EvaluationEndBounds
		{
			get
			{
				return _evaluationEndBounds;
			}
			set
			{
				_evaluationEndBounds = value;
			}
		}

		/// <summary>
		///     Gets the object associated with this evaluator.
		/// </summary>
		public virtual ICalendarObject AssociatedObject
		{
			get
			{
				if ( _associatedObject != null )
				{
					return _associatedObject;
				}

				if ( _associatedDataType != null )
				{
					return _associatedDataType.AssociatedObject;
				}

				return null;
			}
			protected set
			{
				_associatedObject = value;
			}
		}

		/// <summary>
		///     Gets a list of periods collected so far during
		///     the evaluation process.
		/// </summary>
		public virtual ISet<IPeriod> Periods
		{
			get
			{
				return PeriodsMember;
			}
		}

		/// <summary>
		///     Clears the evaluation, eliminating all data that has
		///     been collected up to this point.  Since this data is cached
		///     as needed, this method can be useful to gather information
		///     that is guaranteed to not be out-of-date.
		/// </summary>
		public virtual void Clear( )
		{
			_evaluationStartBounds = DateTime.MaxValue;
			_evaluationEndBounds = DateTime.MinValue;
			PeriodsMember.Clear( );
		}

		/// <summary>
		///     Evaluates this item to determine the dates and times for which it occurs/recurs.
		///     This method only evaluates items which occur/recur between <paramref name="periodStart" />
		///     and <paramref name="periodEnd" />; therefore, if you require a list of items which
		///     occur outside of this range, you must specify a <paramref name="periodStart" /> and
		///     <paramref name="periodEnd" /> which encapsulate the date(s) of interest.
		///     This method evaluates using the <paramref name="referenceDate" /> as the beginning
		///     point.  For example, for a WEEKLY occurrence, the <paramref name="referenceDate" />
		///     determines the day of week that this item will recur on.
		///     <note type="caution">
		///         For events with very complex recurrence rules, this method may be a bottleneck
		///         during processing time, especially when this method is called for a large number
		///         of items, in sequence, or for a very large time span.
		///     </note>
		/// </summary>
		/// <param name="referenceDate"></param>
		/// <param name="periodStart"></param>
		/// <param name="periodEnd"></param>
		/// <param name="includeReferenceDateInResults"></param>
		/// <returns>
		///     A list of <see cref="System.DateTime" /> objects for
		///     each date/time when this item occurs/recurs.
		/// </returns>
		public abstract ISet<IPeriod> Evaluate(
			IDateTime referenceDate,
			DateTime periodStart,
			DateTime periodEnd,
			bool includeReferenceDateInResults );

		#endregion
	}
}