// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     An iCalendar representation of the <c>RRULE</c> property.
	/// </summary>
	[Serializable]
	public class RecurrencePattern : EncodableDataType, IRecurrencePattern
	{
		[NonSerialized]
		private FrequencyType _frequency;

		/// <summary>
		///     Until.
		/// </summary>
		private DateTime _until = DateTime.MinValue;

		/// <summary>
		///     Count.
		/// </summary>
		private int _count = int.MinValue;

		/// <summary>
		///     Interval.
		/// </summary>
		private int _interval = int.MinValue;

		/// <summary>
		///     BySecond.
		/// </summary>
		private IList<int> _bySecond = new List<int>( );

		/// <summary>
		///     ByMinute.
		/// </summary>
		private IList<int> _byMinute = new List<int>( );

		/// <summary>
		///     ByHour.
		/// </summary>
		private IList<int> _byHour = new List<int>( );

		/// <summary>
		///     ByDay.
		/// </summary>
		private IList<IWeekDay> _byDay = new List<IWeekDay>( );

		/// <summary>
		///     ByMonthDay.
		/// </summary>
		private IList<int> _byMonthDay = new List<int>( );

		/// <summary>
		///     ByYearDay.
		/// </summary>
		private IList<int> _byYearDay = new List<int>( );

		/// <summary>
		///     ByWeekNo.
		/// </summary>
		private IList<int> _byWeekNo = new List<int>( );

		/// <summary>
		///     ByMonth.
		/// </summary>
		private IList<int> _byMonth = new List<int>( );

		/// <summary>
		///     BySetPosition.
		/// </summary>
		private IList<int> _bySetPosition = new List<int>( );

		/// <summary>
		///     FirstDayOfWeek.
		/// </summary>
		private DayOfWeek _firstDayOfWeek = DayOfWeek.Monday;

		/// <summary>
		///     Restriction Type.
		/// </summary>
		private RecurrenceRestrictionType? _restrictionType;

		/// <summary>
		///     Evaluation Mode.
		/// </summary>
		private RecurrenceEvaluationModeType? _evaluationMode;

		/// <summary>
		///     Gets or sets the frequency.
		/// </summary>
		/// <value>
		///     The frequency.
		/// </value>
		public FrequencyType Frequency
		{
			get
			{
				return _frequency;
			}
			set
			{
				_frequency = value;
			}
		}

		/// <summary>
		///     Gets or sets the until.
		/// </summary>
		/// <value>
		///     The until.
		/// </value>
		public DateTime Until
		{
			get
			{
				return _until;
			}
			set
			{
				_until = value;
			}
		}

		/// <summary>
		///     Gets or sets the count.
		/// </summary>
		/// <value>
		///     The count.
		/// </value>
		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				_count = value;
			}
		}

		/// <summary>
		///     Gets or sets the interval.
		/// </summary>
		/// <value>
		///     The interval.
		/// </value>
		public int Interval
		{
			get
			{
				if ( _interval == int.MinValue )
				{
					return 1;
				}
				return _interval;
			}
			set
			{
				_interval = value;
			}
		}

		/// <summary>
		///     Gets or sets the by second.
		/// </summary>
		/// <value>
		///     The by second.
		/// </value>
		public IList<int> BySecond
		{
			get
			{
				return _bySecond;
			}
			set
			{
				_bySecond = value;
			}
		}

		/// <summary>
		///     Gets or sets the by minute.
		/// </summary>
		/// <value>
		///     The by minute.
		/// </value>
		public IList<int> ByMinute
		{
			get
			{
				return _byMinute;
			}
			set
			{
				_byMinute = value;
			}
		}

		/// <summary>
		///     Gets or sets the by hour.
		/// </summary>
		/// <value>
		///     The by hour.
		/// </value>
		public IList<int> ByHour
		{
			get
			{
				return _byHour;
			}
			set
			{
				_byHour = value;
			}
		}

		/// <summary>
		///     Gets or sets the by day.
		/// </summary>
		/// <value>
		///     The by day.
		/// </value>
		public IList<IWeekDay> ByDay
		{
			get
			{
				return _byDay;
			}
			set
			{
				_byDay = value;
			}
		}

		/// <summary>
		///     Gets or sets the by month day.
		/// </summary>
		/// <value>
		///     The by month day.
		/// </value>
		public IList<int> ByMonthDay
		{
			get
			{
				return _byMonthDay;
			}
			set
			{
				_byMonthDay = value;
			}
		}

		/// <summary>
		///     Gets or sets the by year day.
		/// </summary>
		/// <value>
		///     The by year day.
		/// </value>
		public IList<int> ByYearDay
		{
			get
			{
				return _byYearDay;
			}
			set
			{
				_byYearDay = value;
			}
		}

		/// <summary>
		///     Gets or sets the by week no.
		/// </summary>
		/// <value>
		///     The by week no.
		/// </value>
		public IList<int> ByWeekNo
		{
			get
			{
				return _byWeekNo;
			}
			set
			{
				_byWeekNo = value;
			}
		}

		/// <summary>
		///     Gets or sets the by month.
		/// </summary>
		/// <value>
		///     The by month.
		/// </value>
		public IList<int> ByMonth
		{
			get
			{
				return _byMonth;
			}
			set
			{
				_byMonth = value;
			}
		}

		/// <summary>
		///     Gets or sets the by set position.
		/// </summary>
		/// <value>
		///     The by set position.
		/// </value>
		public IList<int> BySetPosition
		{
			get
			{
				return _bySetPosition;
			}
			set
			{
				_bySetPosition = value;
			}
		}

		/// <summary>
		///     Gets or sets the first day of week.
		/// </summary>
		/// <value>
		///     The first day of week.
		/// </value>
		public DayOfWeek FirstDayOfWeek
		{
			get
			{
				return _firstDayOfWeek;
			}
			set
			{
				_firstDayOfWeek = value;
			}
		}

		/// <summary>
		///     Gets or sets the type of the restriction.
		/// </summary>
		/// <value>
		///     The type of the restriction.
		/// </value>
		public RecurrenceRestrictionType RestrictionType
		{
			get
			{
				// NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
				if ( _restrictionType != null )
				{
					return _restrictionType.Value;
				}

				if ( Calendar != null )
				{
					return Calendar.RecurrenceRestriction;
				}

				return RecurrenceRestrictionType.Default;
			}
			set
			{
				_restrictionType = value;
			}
		}

		/// <summary>
		///     Gets or sets the evaluation mode.
		/// </summary>
		/// <value>
		///     The evaluation mode.
		/// </value>
		public RecurrenceEvaluationModeType EvaluationMode
		{
			get
			{
				// NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
				if ( _evaluationMode != null )
				{
					return _evaluationMode.Value;
				}

				if ( Calendar != null )
				{
					return Calendar.RecurrenceEvaluationMode;
				}
				return RecurrenceEvaluationModeType.Default;
			}
			set
			{
				_evaluationMode = value;
			}
		}

		///// <summary>
		///// Returns the next occurrence of the pattern,
		///// given a valid previous occurrence, <paramref name="lastOccurrence"/>.
		///// As long as the recurrence pattern is valid, and
		///// <paramref name="lastOccurrence"/> is a valid previous 
		///// occurrence within the pattern, this will return the
		///// next occurrence.  NOTE: This will not give accurate results
		///// when COUNT or BYSETVAL are used.
		///// </summary>
		//virtual public IPeriod GetNextOccurrence(IDateTime lastOccurrence)
		//{
		//    RecurrencePatternEvaluator evaluator = GetService<RecurrencePatternEvaluator>();
		//    if (evaluator != null)
		//        return evaluator.GetNext(lastOccurrence);

		//    return null;
		//}

		/// <summary>
		///     Initializes a new instance of the <see cref="RecurrencePattern" /> class.
		/// </summary>
		public RecurrencePattern( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RecurrencePattern" /> class.
		/// </summary>
		/// <param name="frequency">The frequency.</param>
		public RecurrencePattern( FrequencyType frequency )
			: this( frequency, 1 )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RecurrencePattern" /> class.
		/// </summary>
		/// <param name="frequency">The frequency.</param>
		/// <param name="interval">The interval.</param>
		public RecurrencePattern( FrequencyType frequency, int interval )
			:
				this( )
		{
			Frequency = frequency;
			Interval = interval;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RecurrencePattern" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public RecurrencePattern( string value )
			:
				this( )
		{
			if ( value != null )
			{
				var serializer = new RecurrencePatternSerializer( );
				CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
			}
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			SetService( new RecurrencePatternEvaluator( this ) );
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
			var recurrencePattern = obj as RecurrencePattern;

			if ( recurrencePattern != null )
			{
				RecurrencePattern r = recurrencePattern;
				if ( !CollectionEquals( r.ByDay, ByDay ) ||
				     !CollectionEquals( r.ByHour, ByHour ) ||
				     !CollectionEquals( r.ByMinute, ByMinute ) ||
				     !CollectionEquals( r.ByMonth, ByMonth ) ||
				     !CollectionEquals( r.ByMonthDay, ByMonthDay ) ||
				     !CollectionEquals( r.BySecond, BySecond ) ||
				     !CollectionEquals( r.BySetPosition, BySetPosition ) ||
				     !CollectionEquals( r.ByWeekNo, ByWeekNo ) ||
				     !CollectionEquals( r.ByYearDay, ByYearDay ) )
				{
					return false;
				}
				if ( r.Count != Count )
				{
					return false;
				}
				if ( r.Frequency != Frequency )
				{
					return false;
				}
				if ( r.Interval != Interval )
				{
					return false;
				}
				if ( r.Until != DateTime.MinValue )
				{
					if ( !r.Until.Equals( Until ) )
					{
						return false;
					}
				}
				else if ( Until != DateTime.MinValue )
				{
					return false;
				}
				if ( r.FirstDayOfWeek != FirstDayOfWeek )
				{
					return false;
				}
				return true;
			}
// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			var serializer = new RecurrencePatternSerializer( );
			return serializer.SerializeToString( this );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			int hashCode =
				ByDay.GetHashCode( ) ^ ByHour.GetHashCode( ) ^ ByMinute.GetHashCode( ) ^
				ByMonth.GetHashCode( ) ^ ByMonthDay.GetHashCode( ) ^ BySecond.GetHashCode( ) ^
				BySetPosition.GetHashCode( ) ^ ByWeekNo.GetHashCode( ) ^ ByYearDay.GetHashCode( ) ^
				Count.GetHashCode( ) ^ Frequency.GetHashCode( );

			if ( Interval.Equals( 1 ) )
			{
				hashCode ^= 0x1;
			}
			else
			{
				hashCode ^= Interval.GetHashCode( );
			}

			hashCode ^= Until.GetHashCode( );
			hashCode ^= FirstDayOfWeek.GetHashCode( );
			return hashCode;
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var recurrencePattern = obj as IRecurrencePattern;

			if ( recurrencePattern != null )
			{
				IRecurrencePattern r = recurrencePattern;

				Frequency = r.Frequency;
				Until = r.Until;
				Count = r.Count;
				Interval = r.Interval;
				BySecond = new List<int>( r.BySecond );
				ByMinute = new List<int>( r.ByMinute );
				ByHour = new List<int>( r.ByHour );
				ByDay = new List<IWeekDay>( r.ByDay );
				ByMonthDay = new List<int>( r.ByMonthDay );
				ByYearDay = new List<int>( r.ByYearDay );
				ByWeekNo = new List<int>( r.ByWeekNo );
				ByMonth = new List<int>( r.ByMonth );
				BySetPosition = new List<int>( r.BySetPosition );
				FirstDayOfWeek = r.FirstDayOfWeek;
				RestrictionType = r.RestrictionType;
				EvaluationMode = r.EvaluationMode;
			}
		}

		/// <summary>
		///     Collections the equals.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c1">The c1.</param>
		/// <param name="c2">The c2.</param>
		/// <returns></returns>
		private bool CollectionEquals<T>( IList<T> c1, IList<T> c2 )
		{
			// NOTE: fixes a bug where collections weren't properly compared
			if ( c1 == null ||
			     c2 == null )
			{
				if ( Equals( c1, c2 ) )
				{
					return true;
				}

				return false;
			}
			if ( !c1.Count.Equals( c2.Count ) )
			{
				return false;
			}

			IEnumerator e1 = c1.GetEnumerator( );
			IEnumerator e2 = c2.GetEnumerator( );

			while ( e1.MoveNext( ) && e2.MoveNext( ) )
			{
				if ( !Equals( e1.Current, e2.Current ) )
				{
					return false;
				}
			}
			return true;
		}
	}
}