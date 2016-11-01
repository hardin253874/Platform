// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Represents an RFC 5545 "BYDAY" value.
	/// </summary>
	[Serializable]
	public sealed class WeekDay : EncodableDataType, IWeekDay
	{
		/// <summary>
		///     Number.
		/// </summary>
		private int _num = int.MinValue;

		/// <summary>
		///     Day of week.
		/// </summary>
		private DayOfWeek _dayOfWeek;

		/// <summary>
		///     Gets or sets the offset.
		/// </summary>
		/// <value>
		///     The offset.
		/// </value>
		public int Offset
		{
			get
			{
				return _num;
			}
			set
			{
				_num = value;
			}
		}

		/// <summary>
		///     Gets or sets the day of week.
		/// </summary>
		/// <value>
		///     The day of week.
		/// </value>
		public DayOfWeek DayOfWeek
		{
			get
			{
				return _dayOfWeek;
			}
			set
			{
				_dayOfWeek = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="WeekDay" /> class.
		/// </summary>
		public WeekDay( )
		{
			Offset = int.MinValue;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="WeekDay" /> class.
		/// </summary>
		/// <param name="day">The day.</param>
		public WeekDay( DayOfWeek day )
			: this( )
		{
			DayOfWeek = day;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="WeekDay" /> class.
		/// </summary>
		/// <param name="day">The day.</param>
		/// <param name="num">The num.</param>
		public WeekDay( DayOfWeek day, int num )
			: this( day )
		{
			Offset = num;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="WeekDay" /> class.
		/// </summary>
		/// <param name="day">The day.</param>
		/// <param name="type">The type.</param>
		public WeekDay( DayOfWeek day, FrequencyOccurrence type )
			: this( day, ( int ) type )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="WeekDay" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public WeekDay( string value )
		{
			var serializer = new WeekDaySerializer( );

			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
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
			var weekDay = obj as WeekDay;

			if ( weekDay != null )
			{
				WeekDay ds = weekDay;
				return ds.Offset == Offset &&
				       ds.DayOfWeek == DayOfWeek;
			}

// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			return Offset.GetHashCode( ) ^ DayOfWeek.GetHashCode( );
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var weekDay = obj as IWeekDay;

			if ( weekDay != null )
			{
				IWeekDay bd = weekDay;
				Offset = bd.Offset;
				DayOfWeek = bd.DayOfWeek;
			}
		}

		#region IComparable Members

		/// <summary>
		///     Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		///     A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes
		///     <paramref
		///         name="obj" />
		///     in the sort order. Zero This instance occurs in the same position in the sort order as
		///     <paramref
		///         name="obj" />
		///     . Greater than zero This instance follows <paramref name="obj" /> in the sort order.
		/// </returns>
		/// <exception cref="System.ArgumentException"></exception>
		public int CompareTo( object obj )
		{
			IWeekDay bd = null;

			if ( obj is string )
			{
				bd = new WeekDay( obj.ToString( ) );
			}
			else
			{
				var weekDay = obj as IWeekDay;

				if ( weekDay != null )
				{
					bd = weekDay;
				}
			}

			if ( bd == null )
			{
				throw new ArgumentException( );
			}

			int compare = DayOfWeek.CompareTo( bd.DayOfWeek );

			if ( compare == 0 )
			{
				compare = Offset.CompareTo( bd.Offset );
			}
			return compare;
		}

		#endregion
	}
}