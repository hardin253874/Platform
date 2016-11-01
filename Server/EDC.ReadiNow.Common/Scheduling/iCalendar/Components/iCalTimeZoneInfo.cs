// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that contains time zone information, and is usually accessed
	///     from an iCalendar object using the <see cref="EDC.ReadiNow.Scheduling.iCalendar.iCalendar.GetTimeZone" /> method.
	/// </summary>
	[Serializable]
// ReSharper disable InconsistentNaming
	public sealed class iCalTimeZoneInfo : CalendarComponent, ITimeZoneInfo
// ReSharper restore InconsistentNaming
	{
		/// <summary>
		///     Evaluator.
		/// </summary>
		private TimeZoneInfoEvaluator _evaluator;

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalTimeZoneInfo" /> class.
		/// </summary>
		public iCalTimeZoneInfo( )
		{
			// FIXME: how do we ensure SEQUENCE doesn't get serialized?
			//base.Sequence = null;
			// iCalTimeZoneInfo does not allow sequence numbers
			// Perhaps we should have a custom serializer that fixes this?

			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalTimeZoneInfo" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public iCalTimeZoneInfo( string name )
			: this( )
		{
			Name = name;
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			_evaluator = new TimeZoneInfoEvaluator( this );
			SetService( _evaluator );
		}

		/// <summary>
		///     Called when [deserializing].
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserializing( StreamingContext context )
		{
			base.OnDeserializing( context );

			Initialize( );
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var tzi = obj as iCalTimeZoneInfo;

			if ( tzi != null )
			{
				return Equals( TimeZoneName, tzi.TimeZoneName ) &&
				       Equals( OffsetFrom, tzi.OffsetFrom ) &&
				       Equals( OffsetTo, tzi.OffsetTo );
			}

			return base.Equals( obj );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			int hash = 13;

			if ( TimeZoneName != null )
			{
				hash = ( hash * 7 ) + TimeZoneName.GetHashCode( );
			}

			if ( OffsetFrom != null )
			{
				hash = ( hash * 7 ) + OffsetFrom.GetHashCode( );
			}

			if ( OffsetTo != null )
			{
				hash = ( hash * 7 ) + OffsetTo.GetHashCode( );
			}

			return hash;
		}

		#region ITimeZoneInfo Members

		/// <summary>
		///     Gets the TZID.
		/// </summary>
		/// <value>
		///     The TZID.
		/// </value>
		public string TzId
		{
			get
			{
				var tz = Parent as ITimeZone;
				if ( tz != null )
				{
					return tz.TzId;
				}
				return null;
			}
		}

		/// <summary>
		///     Returns the name of the current Time Zone.
		///     <example>
		///         The following are examples:
		///         <list type="bullet">
		///             <item>EST</item>
		///             <item>EDT</item>
		///             <item>MST</item>
		///             <item>MDT</item>
		///         </list>
		///     </example>
		/// </summary>
		public string TimeZoneName
		{
			get
			{
				if ( TimeZoneNames.Count > 0 )
				{
					return TimeZoneNames[ 0 ];
				}
				return null;
			}
			set
			{
				TimeZoneNames.Clear( );
				TimeZoneNames.Add( value );
			}
		}

		/// <summary>
		///     Gets or sets the time zone offset from.
		/// </summary>
		/// <value>
		///     The time zone offset from.
		/// </value>
		public IUtcOffset TzOffsetFrom
		{
			get
			{
				return OffsetFrom;
			}
			set
			{
				OffsetFrom = value;
			}
		}

		/// <summary>
		///     Gets or sets the offset from.
		/// </summary>
		/// <value>
		///     The offset from.
		/// </value>
		public IUtcOffset OffsetFrom
		{
			get
			{
				return Properties.Get<IUtcOffset>( "TZOFFSETFROM" );
			}
			set
			{
				Properties.Set( "TZOFFSETFROM", value );
			}
		}

		/// <summary>
		///     Gets or sets the offset to.
		/// </summary>
		/// <value>
		///     The offset to.
		/// </value>
		public IUtcOffset OffsetTo
		{
			get
			{
				return Properties.Get<IUtcOffset>( "TZOFFSETTO" );
			}
			set
			{
				Properties.Set( "TZOFFSETTO", value );
			}
		}

		/// <summary>
		///     Gets or sets the time zone offset to.
		/// </summary>
		/// <value>
		///     The time zone offset to.
		/// </value>
		public IUtcOffset TzOffsetTo
		{
			get
			{
				return OffsetTo;
			}
			set
			{
				OffsetTo = value;
			}
		}

		/// <summary>
		///     Gets or sets the time zone names.
		/// </summary>
		/// <value>
		///     The time zone names.
		/// </value>
		public IList<string> TimeZoneNames
		{
			get
			{
				return Properties.GetMany<string>( "TZNAME" );
			}
			set
			{
				Properties.Set( "TZNAME", value );
			}
		}

		/// <summary>
		///     Returns the observance that corresponds to
		///     the date/time provided, or null if no matching
		///     observance could be found within this TimeZoneInfo.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Cannot call GetObservance() on a TimeZoneInfo whose Parent property is null.</exception>
		public TimeZoneObservance? GetObservance( IDateTime dt )
		{
			if ( Parent == null )
			{
				throw new Exception( "Cannot call GetObservance() on a TimeZoneInfo whose Parent property is null." );
			}

			if ( string.Equals( dt.TzId, TzId ) )
			{
				// Normalize date/time values within this time zone to a local value.
				DateTime normalizedDt = dt.Value;

				// Let's evaluate our time zone observances to find the 
				// observance that applies to this date/time value.
				var parentEval = Parent.GetService( typeof ( IEvaluator ) ) as IEvaluator;
				if ( parentEval != null )
				{
					// Evaluate the date/time in question.
					parentEval.Evaluate( Start, DateUtil.GetSimpleDateTimeData( Start ), normalizedDt, true );

					// NOTE: We avoid using period.Contains here, because we want to avoid
					// doing an inadvertent time zone lookup with it.
					IPeriod period = _evaluator
						.Periods
						.FirstOrDefault( p =>
						                 p.StartTime.Value <= normalizedDt &&
						                 p.EndTime.Value > normalizedDt
						);

					if ( period != null )
					{
						return new TimeZoneObservance( period, this );
					}
				}
			}

			return null;
		}

		/// <summary>
		///     Returns true if this time zone info represents
		///     the observed time zone for the IDateTime value
		///     provided.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public bool Contains( IDateTime dt )
		{
			TimeZoneObservance? retval = GetObservance( dt );

			return ( retval != null );
		}

		#endregion

		#region IRecurrable Members

		/// <summary>
		///     Gets or sets the DT start.
		/// </summary>
		/// <value>
		///     The DT start.
		/// </value>
		public IDateTime DtStart
		{
			get
			{
				return Start;
			}
			set
			{
				Start = value;
			}
		}

		/// <summary>
		///     Gets/sets the start date/time of the component.
		/// </summary>
		public IDateTime Start
		{
			get
			{
				return Properties.Get<IDateTime>( "DTSTART" );
			}
			set
			{
				Properties.Set( "DTSTART", value );
			}
		}

		/// <summary>
		///     Gets or sets the exception dates.
		/// </summary>
		/// <value>
		///     The exception dates.
		/// </value>
		public IList<IPeriodList> ExceptionDates
		{
			get
			{
				return Properties.GetMany<IPeriodList>( "EXDATE" );
			}
			set
			{
				Properties.Set( "EXDATE", value );
			}
		}

		/// <summary>
		///     Gets or sets the exception rules.
		/// </summary>
		/// <value>
		///     The exception rules.
		/// </value>
		public IList<IRecurrencePattern> ExceptionRules
		{
			get
			{
				return Properties.GetMany<IRecurrencePattern>( "EXRULE" );
			}
			set
			{
				Properties.Set( "EXRULE", value );
			}
		}

		/// <summary>
		///     Gets or sets the recurrence dates.
		/// </summary>
		/// <value>
		///     The recurrence dates.
		/// </value>
		public IList<IPeriodList> RecurrenceDates
		{
			get
			{
				return Properties.GetMany<IPeriodList>( "RDATE" );
			}
			set
			{
				Properties.Set( "RDATE", value );
			}
		}

		/// <summary>
		///     Gets or sets the recurrence rules.
		/// </summary>
		/// <value>
		///     The recurrence rules.
		/// </value>
		public IList<IRecurrencePattern> RecurrenceRules
		{
			get
			{
				return Properties.GetMany<IRecurrencePattern>( "RRULE" );
			}
			set
			{
				Properties.Set( "RRULE", value );
			}
		}

		/// <summary>
		///     Gets or sets the recurrence ID.
		/// </summary>
		/// <value>
		///     The recurrence ID.
		/// </value>
		public IDateTime RecurrenceId
		{
			get
			{
				return Properties.Get<IDateTime>( "RECURRENCE-ID" );
			}
			set
			{
				Properties.Set( "RECURRENCE-ID", value );
			}
		}

		#endregion

		#region IRecurrable Members

		/// <summary>
		///     Clears a previous evaluation, usually because one of the
		///     key elements used for evaluation has changed
		///     (Start, End, Duration, recurrence rules, exceptions, etc.).
		/// </summary>
		public void ClearEvaluation( )
		{
			RecurrenceUtil.ClearEvaluation( this );
		}

		/// <summary>
		///     Returns all occurrences of this component that start on the date provided.
		///     All components starting between 12:00:00AM and 11:59:59 PM will be
		///     returned.
		///     <note>
		///         This will first Evaluate() the date range required in order to
		///         determine the occurrences for the date provided, and then return
		///         the occurrences.
		///     </note>
		/// </summary>
		/// <param name="dt">The date for which to return occurrences.</param>
		/// <returns>
		///     A list of Periods representing the occurrences of this object.
		/// </returns>
		public IList<Occurrence> GetOccurrences( IDateTime dt )
		{
			return RecurrenceUtil.GetOccurrences( this, dt, true );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences( DateTime dt )
		{
			return RecurrenceUtil.GetOccurrences( this, new iCalDateTime( dt ), true );
		}

		/// <summary>
		///     Returns all occurrences of this component that start within the date range provided.
		///     All components occurring between <paramref name="startTime" /> and <paramref name="endTime" />
		///     will be returned.
		/// </summary>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences( IDateTime startTime, IDateTime endTime )
		{
			return RecurrenceUtil.GetOccurrences( this, startTime, endTime, true );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		public IList<Occurrence> GetOccurrences( DateTime startTime, DateTime endTime )
		{
			return RecurrenceUtil.GetOccurrences( this, new iCalDateTime( startTime ), new iCalDateTime( endTime ), true );
		}

		#endregion
	}
}