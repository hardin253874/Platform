// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     The iCalendar equivalent of the .NET <see cref="DateTime" /> class.
	///     <remarks>
	///         In addition to the features of the <see cref="DateTime" /> class, the <see cref="iCalDateTime" />
	///         class handles time zone differences, and integrates seamlessly into the iCalendar framework.
	///     </remarks>
	/// </summary>
	[Serializable]
// ReSharper disable InconsistentNaming
	public sealed class iCalDateTime : EncodableDataType, IDateTime
// ReSharper restore InconsistentNaming
	{
		/// <summary>
		///     Gets the now.
		/// </summary>
		/// <value>
		///     The now.
		/// </value>
		public static iCalDateTime Now
		{
			get
			{
				return new iCalDateTime( DateTime.Now );
			}
		}

		/// <summary>
		///     Gets the today.
		/// </summary>
		/// <value>
		///     The today.
		/// </value>
		public static iCalDateTime Today
		{
			get
			{
				return new iCalDateTime( DateTime.Today );
			}
		}

		/// <summary>
		///     Value.
		/// </summary>
		private DateTime _value;

		/// <summary>
		///     Has Date.
		/// </summary>
		private bool _hasDate;

		/// <summary>
		///     Has Time.
		/// </summary>
		private bool _hasTime;

		/// <summary>
		///     TimeZone Observance.
		/// </summary>
		private TimeZoneObservance? _timeZoneObservance;

		/// <summary>
		///     Is Universal Time.
		/// </summary>
		private bool _isUniversalTime;

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		public iCalDateTime( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public iCalDateTime( IDateTime value )
		{
			Initialize( value.Value, value.TzId, null );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public iCalDateTime( DateTime value )
			: this( value, null )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="tzid">The time zone id.</param>
		public iCalDateTime( DateTime value, string tzid )
		{
			Initialize( value, tzid, null );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="tzo">The time zone observance.</param>
		public iCalDateTime( DateTime value, TimeZoneObservance tzo )
		{
			Initialize( value, tzo );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="hour">The hour.</param>
		/// <param name="minute">The minute.</param>
		/// <param name="second">The second.</param>
		public iCalDateTime( int year, int month, int day, int hour, int minute, int second )
		{
			Initialize( year, month, day, hour, minute, second, null, null );
			HasTime = true;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="hour">The hour.</param>
		/// <param name="minute">The minute.</param>
		/// <param name="second">The second.</param>
		/// <param name="tzid">The time zone id.</param>
		public iCalDateTime( int year, int month, int day, int hour, int minute, int second, string tzid )
		{
			Initialize( year, month, day, hour, minute, second, tzid, null );
			HasTime = true;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="hour">The hour.</param>
		/// <param name="minute">The minute.</param>
		/// <param name="second">The second.</param>
		/// <param name="tzid">The time zone id.</param>
		/// <param name="iCal">The i cal.</param>
		public iCalDateTime( int year, int month, int day, int hour, int minute, int second, string tzid, IICalendar iCal )
		{
			Initialize( year, month, day, hour, minute, second, tzid, iCal );
			HasTime = true;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		public iCalDateTime( int year, int month, int day )
			: this( year, month, day, 0, 0, 0 )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="hasTime">
		///     if set to <c>true</c> [has time].
		/// </param>
		public iCalDateTime( int year, int month, int day, bool hasTime )
			: this( year, month, day, 0, 0, 0 )
		{
			HasTime = hasTime;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="tzid">The time zone id.</param>
		public iCalDateTime( int year, int month, int day, string tzid )
			: this( year, month, day, 0, 0, 0, tzid )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="tzid">The time zone id.</param>
		/// <param name="hasTime">
		///     if set to <c>true</c> [has time].
		/// </param>
		public iCalDateTime( int year, int month, int day, string tzid, bool hasTime )
			: this( year, month, day, 0, 0, 0, tzid )
		{
			HasTime = hasTime;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalDateTime" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public iCalDateTime( string value )
		{
			var serializer = new DateTimeSerializer( );
			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Initializes the specified year.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="hour">The hour.</param>
		/// <param name="minute">The minute.</param>
		/// <param name="second">The second.</param>
		/// <param name="tzid">The time zone id.</param>
		/// <param name="iCal">The i cal.</param>
		private void Initialize( int year, int month, int day, int hour, int minute, int second, string tzid, IICalendar iCal )
		{
			Initialize( CoerceDateTime( year, month, day, hour, minute, second, DateTimeKind.Local ), tzid, iCal );
		}

		/// <summary>
		///     Initializes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="tzid">The time zone id.</param>
		/// <param name="iCal">The i cal.</param>
		private void Initialize( DateTime value, string tzid, IICalendar iCal )
		{
			if ( value.Kind == DateTimeKind.Utc )
			{
				IsUniversalTime = true;
			}

			// Convert all incoming values to UTC.
			Value = DateTime.SpecifyKind( value, DateTimeKind.Utc );
			HasDate = true;
			HasTime = ( value.Second != 0 || value.Minute != 0 || value.Hour != 0 );
			TzId = tzid;
			AssociatedObject = iCal;
		}

		/// <summary>
		///     Initializes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="tzo">The time zone observance.</param>
		private void Initialize( DateTime value, TimeZoneObservance tzo )
		{
			if ( value.Kind == DateTimeKind.Utc )
			{
				IsUniversalTime = true;
			}

			// Convert all incoming values to UTC.
			Value = DateTime.SpecifyKind( value, DateTimeKind.Utc );
			HasDate = true;
			HasTime = ( value.Second != 0 || value.Minute != 0 || value.Hour != 0 );
			if ( tzo.TimeZoneInfo != null )
			{
				TzId = tzo.TimeZoneInfo.TzId;
			}
			TimeZoneObservance = tzo;
			AssociatedObject = tzo.TimeZoneInfo;
		}

		/// <summary>
		///     Coerces the date time.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="hour">The hour.</param>
		/// <param name="minute">The minute.</param>
		/// <param name="second">The second.</param>
		/// <param name="kind">The kind.</param>
		/// <returns></returns>
		private DateTime CoerceDateTime( int year, int month, int day, int hour, int minute, int second, DateTimeKind kind )
		{
			DateTime dt = DateTime.MinValue;

			// NOTE: determine if a date/time value exceeds the representable date/time values in .NET.
			// If so, let's automatically adjust the date/time to compensate.
			// FIXME: should we have a parsing setting that will throw an exception
			// instead of automatically adjusting the date/time value to the
			// closest representable date/time?
			try
			{
				if ( year > 9999 )
				{
					dt = DateTime.MaxValue;
				}
				else if ( year > 0 )
				{
					dt = new DateTime( year, month, day, hour, minute, second, kind );
				}
			}
// ReSharper disable EmptyGeneralCatchClause
			catch
// ReSharper restore EmptyGeneralCatchClause
			{
			}

			return dt;
		}

		/// <summary>
		///     Gets the time zone observance.
		/// </summary>
		/// <returns></returns>
		private TimeZoneObservance? GetTimeZoneObservance( )
		{
			if ( _timeZoneObservance == null &&
			     TzId != null &&
			     Calendar != null )
			{
				ITimeZone tz = Calendar.GetTimeZone( TzId );
				if ( tz != null )
				{
					_timeZoneObservance = tz.GetTimeZoneObservance( this );
				}
			}
			return _timeZoneObservance;
		}

		/// <summary>
		///     Gets or sets the associated object.
		/// </summary>
		/// <value>
		///     The associated object.
		/// </value>
		public override ICalendarObject AssociatedObject
		{
			get
			{
				return base.AssociatedObject;
			}
			set
			{
				if ( !Equals( AssociatedObject, value ) )
				{
					base.AssociatedObject = value;
				}
			}
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var dt = obj as IDateTime;
			if ( dt != null )
			{
				_value = dt.Value;
				_isUniversalTime = dt.IsUniversalTime;
				_hasDate = dt.HasDate;
				_hasTime = dt.HasTime;

				AssociateWith( dt );
			}
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
			var dateTime = obj as IDateTime;

			if ( dateTime != null )
			{
				AssociateWith( dateTime );
				return dateTime.Utc.Equals( Utc );
			}

			if ( obj is DateTime )
			{
				var dt = new iCalDateTime( ( DateTime ) obj );
				AssociateWith( dt );
				return Equals( dt.Utc, Utc );
			}
			return false;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			return Value.GetHashCode( );
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return ToString( null, null );
		}

		#region Operators

		/// <summary>
		///     Less than operator.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <( iCalDateTime left, IDateTime right )
		{
			left.AssociateWith( right );

			if ( left.HasTime || right.HasTime )
			{
				return left.Utc < right.Utc;
			}

			return left.Utc.Date < right.Utc.Date;
		}

		/// <summary>
		///     >s the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static bool operator >( iCalDateTime left, IDateTime right )
		{
			left.AssociateWith( right );

			if ( left.HasTime || right.HasTime )
			{
				return left.Utc > right.Utc;
			}

			return left.Utc.Date > right.Utc.Date;
		}

		/// <summary>
		///     Less than or equal to operator.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator <=( iCalDateTime left, IDateTime right )
		{
			left.AssociateWith( right );

			if ( left.HasTime || right.HasTime )
			{
				return left.Utc <= right.Utc;
			}

			return left.Utc.Date <= right.Utc.Date;
		}

		/// <summary>
		///     >=s the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static bool operator >=( iCalDateTime left, IDateTime right )
		{
			left.AssociateWith( right );

			if ( left.HasTime || right.HasTime )
			{
				return left.Utc >= right.Utc;
			}

			return left.Utc.Date >= right.Utc.Date;
		}

		/// <summary>
		///     ==s the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static bool operator ==( iCalDateTime left, IDateTime right )
		{
			if ( left != null )
			{
				left.AssociateWith( right );

				if ( right != null && ( left.HasTime || right.HasTime ) )
				{
					return left.Utc.Equals( right.Utc );
				}

				return right != null && left.Utc.Date.Equals( right.Utc.Date );
			}

			return false;
		}

		/// <summary>
		///     !=s the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static bool operator !=( iCalDateTime left, IDateTime right )
		{
			if ( left != null )
			{
				left.AssociateWith( right );

				if ( right != null && ( left.HasTime || right.HasTime ) )
				{
					return !left.Utc.Equals( right.Utc );
				}

				return right != null && !left.Utc.Date.Equals( right.Utc.Date );
			}

			return true;
		}

		/// <summary>
		///     -s the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static TimeSpan operator -( iCalDateTime left, IDateTime right )
		{
			left.AssociateWith( right );
			return left.Utc - right.Utc;
		}

		/// <summary>
		///     -s the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static IDateTime operator -( iCalDateTime left, TimeSpan right )
		{
			var copy = left.Copy<IDateTime>( );
			copy.Value -= right;
			return copy;
		}

		/// <summary>
		///     +s the specified left.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static IDateTime operator +( iCalDateTime left, TimeSpan right )
		{
			var copy = left.Copy<IDateTime>( );
			copy.Value += right;
			return copy;
		}

		/// <summary>
		///     Is the cal date time.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <returns></returns>
		public static implicit operator iCalDateTime( DateTime left )
		{
			return new iCalDateTime( left );
		}

		#endregion

		#region IDateTime Members

		/// <summary>
		///     Converts the date/time to local time, according to the time
		///     zone specified in the TZID property.
		/// </summary>
		public DateTime Local
		{
			get
			{
				if ( IsUniversalTime &&
				     TzId != null )
				{
					DateTime value = HasTime ? Value : Value.Date;

					// Get the Time Zone Observance, if possible
					TimeZoneObservance? tzi = TimeZoneObservance ?? GetTimeZoneObservance( );

					if ( tzi != null )
					{
						Debug.Assert( tzi.Value.TimeZoneInfo.OffsetTo != null );
						return DateTime.SpecifyKind( tzi.Value.TimeZoneInfo.OffsetTo.ToLocal( value ), DateTimeKind.Local );
					}
				}
				return DateTime.SpecifyKind( HasTime ? Value : Value.Date, DateTimeKind.Local );
			}
		}

		/// <summary>
		///     Converts the date/time to UTC (Coordinated Universal Time)
		/// </summary>
		public DateTime Utc
		{
			get
			{
				if ( IsUniversalTime )
				{
					return DateTime.SpecifyKind( Value, DateTimeKind.Utc );
				}

				if ( TzId != null )
				{
					DateTime value = Value;

					// Get the Time Zone Observance, if possible
					TimeZoneObservance? tzi = TimeZoneObservance ?? GetTimeZoneObservance( );

					if ( tzi != null )
					{
						Debug.Assert( tzi.Value.TimeZoneInfo.OffsetTo != null );
						return DateTime.SpecifyKind( tzi.Value.TimeZoneInfo.OffsetTo.ToUtc( value ), DateTimeKind.Utc );
					}
				}

				// Fallback to the OS-conversion
				return DateTime.SpecifyKind( Value, DateTimeKind.Local ).ToUniversalTime( );
			}
		}

		/// <summary>
		///     Gets/sets the <see cref="iCalTimeZoneInfo" /> object for the time
		///     zone set by <see cref="TzId" />.
		/// </summary>
		public TimeZoneObservance? TimeZoneObservance
		{
			get
			{
				return _timeZoneObservance;
			}
			set
			{
				_timeZoneObservance = value;
				if ( value != null &&
				     value.Value.TimeZoneInfo != null )
				{
					TzId = value.Value.TimeZoneInfo.TzId;
				}
				else
				{
					TzId = null;
				}
			}
		}

		/// <summary>
		///     Gets/sets whether the Value of this date/time represents
		///     a universal time.
		/// </summary>
		public bool IsUniversalTime
		{
			get
			{
				return _isUniversalTime;
			}
			set
			{
				_isUniversalTime = value;
			}
		}

		/// <summary>
		///     Gets the time zone name this time is in, if it references a time zone.
		/// </summary>
		public string TimeZoneName
		{
			get
			{
				if ( IsUniversalTime )
				{
					return "UTC";
				}

				if ( _timeZoneObservance != null )
				{
					return _timeZoneObservance.Value.TimeZoneInfo.TimeZoneName;
				}
				return string.Empty;
			}
		}

		/// <summary>
		///     Gets/sets the underlying DateTime value stored.  This should always
		///     use DateTimeKind.Utc, regardless of its actual representation.
		///     Use IsUniversalTime along with the TZID to control how this
		///     date/time is handled.
		/// </summary>
		public DateTime Value
		{
			get
			{
				return _value;
			}
			set
			{
				if ( !Equals( _value, value ) )
				{
					_value = value;

					// Reset the time zone info if the new date/time doesn't
					// fall within this time zone observance.
					if ( _timeZoneObservance != null &&
					     !_timeZoneObservance.Value.Contains( this ) )
					{
						_timeZoneObservance = null;
					}
				}
			}
		}

		/// <summary>
		///     Gets/sets whether or not this date/time value contains a 'date' part.
		/// </summary>
		public bool HasDate
		{
			get
			{
				return _hasDate;
			}
			set
			{
				_hasDate = value;
			}
		}

		/// <summary>
		///     Gets/sets whether or not this date/time value contains a 'time' part.
		/// </summary>
		public bool HasTime
		{
			get
			{
				return _hasTime;
			}
			set
			{
				_hasTime = value;
			}
		}

		/// <summary>
		///     Gets/sets the time zone ID for this date/time value.
		/// </summary>
		public string TzId
		{
			get
			{
				return Parameters.Get( "TZID" );
			}
			set
			{
				if ( !Equals( TzId, value ) )
				{
					Parameters.Set( "TZID", value );

					// Set the time zone observance to null if the TZID
					// doesn't match.
					if ( value != null &&
					     _timeZoneObservance != null &&
					     _timeZoneObservance.Value.TimeZoneInfo != null &&
					     !Equals( _timeZoneObservance.Value.TimeZoneInfo.TzId, value ) )
					{
						_timeZoneObservance = null;
					}
				}
			}
		}

		/// <summary>
		///     Gets the year for this date/time value.
		/// </summary>
		public int Year
		{
			get
			{
				return Value.Year;
			}
		}

		/// <summary>
		///     Gets the month for this date/time value.
		/// </summary>
		public int Month
		{
			get
			{
				return Value.Month;
			}
		}

		/// <summary>
		///     Gets the day for this date/time value.
		/// </summary>
		public int Day
		{
			get
			{
				return Value.Day;
			}
		}

		/// <summary>
		///     Gets the hour for this date/time value.
		/// </summary>
		public int Hour
		{
			get
			{
				return Value.Hour;
			}
		}

		/// <summary>
		///     Gets the minute for this date/time value.
		/// </summary>
		public int Minute
		{
			get
			{
				return Value.Minute;
			}
		}

		/// <summary>
		///     Gets the second for this date/time value.
		/// </summary>
		public int Second
		{
			get
			{
				return Value.Second;
			}
		}

		/// <summary>
		///     Gets the millisecond for this date/time value.
		/// </summary>
		public int Millisecond
		{
			get
			{
				return Value.Millisecond;
			}
		}

		/// <summary>
		///     Gets the ticks for this date/time value.
		/// </summary>
		public long Ticks
		{
			get
			{
				return Value.Ticks;
			}
		}

		/// <summary>
		///     Gets the DayOfWeek for this date/time value.
		/// </summary>
		public DayOfWeek DayOfWeek
		{
			get
			{
				return Value.DayOfWeek;
			}
		}

		/// <summary>
		///     Gets the DayOfYear for this date/time value.
		/// </summary>
		public int DayOfYear
		{
			get
			{
				return Value.DayOfYear;
			}
		}

		/// <summary>
		///     Gets the first day of the year currently represented by the IDateTime instance.
		/// </summary>
		public IDateTime FirstDayOfYear
		{
			get
			{
				var dt = Copy<IDateTime>( );
				dt.Value = Value.AddDays( -Value.DayOfYear + 1 ).Date;
				return dt;
			}
		}

		/// <summary>
		///     Gets the first day of the month currently represented by the IDateTime instance.
		/// </summary>
		public IDateTime FirstDayOfMonth
		{
			get
			{
				var dt = Copy<IDateTime>( );
				dt.Value = Value.AddDays( -Value.Day + 1 ).Date;
				return dt;
			}
		}

		/// <summary>
		///     Gets the date portion of the date/time value.
		/// </summary>
		public DateTime Date
		{
			get
			{
				return Value.Date;
			}
		}

		/// <summary>
		///     Gets the time portion of the date/time value.
		/// </summary>
		public TimeSpan TimeOfDay
		{
			get
			{
				return Value.TimeOfDay;
			}
		}

		/// <summary>
		///     Converts the date/time value to a local time
		///     within the specified time zone.
		/// </summary>
		/// <param name="tzo"></param>
		/// <returns></returns>
		public IDateTime ToTimeZone( TimeZoneObservance tzo )
		{
			ITimeZoneInfo tzi = tzo.TimeZoneInfo;
			if ( tzi != null )
			{
				return new iCalDateTime( tzi.OffsetTo.ToLocal( Utc ), tzo );
			}
			return null;
		}

		/// <summary>
		///     To the time zone.
		/// </summary>
		/// <param name="tz">The time zone.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">@You must provide a valid time zone to the ToTimeZone() method;tz</exception>
		public IDateTime ToTimeZone( ITimeZone tz )
		{
			if ( tz != null )
			{
				TimeZoneObservance? tzi = tz.GetTimeZoneObservance( this );
				if ( tzi != null )
				{
					return ToTimeZone( tzi.Value );
				}

				// FIXME: if the time cannot be resolved, should we
				// just provide a copy?  Is this always appropriate?
				return Copy<IDateTime>( );
			}

			throw new ArgumentException( @"You must provide a valid time zone to the ToTimeZone() method", "tz" );
		}

		/// <summary>
		///     Converts the date/time value to a local time
		///     within the specified time zone.
		/// </summary>
		/// <param name="tzid"></param>
		/// <returns></returns>
		/// <exception cref="System.Exception">The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones.</exception>
		/// <exception cref="System.ArgumentException">@You must provide a valid TZID to the ToTimeZone() method;time zone id</exception>
		public IDateTime ToTimeZone( string tzid )
		{
			if ( tzid != null )
			{
				if ( Calendar != null )
				{
					ITimeZone tz = Calendar.GetTimeZone( tzid );
					if ( tz != null )
					{
						return ToTimeZone( tz );
					}

					// FIXME: sometimes a calendar is perfectly valid but the time zone
					// could not be resolved.  What should we do here?
					//throw new Exception("The '" + time zone id + "' time zone could not be resolved.");
					return Copy<IDateTime>( );
				}

				throw new Exception( "The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones." );
			}

			throw new ArgumentException( @"You must provide a valid TZID to the ToTimeZone() method", "tzid" );
		}

		/// <summary>
		///     Sets the time zone.
		/// </summary>
		/// <param name="tz">The time zone.</param>
		/// <returns></returns>
		public IDateTime SetTimeZone( ITimeZone tz )
		{
			if ( tz != null )
			{
				TzId = tz.TzId;
			}
			return this;
		}

		/// <summary>
		///     Adds the specified time span.
		/// </summary>
		/// <param name="ts">The time span.</param>
		/// <returns></returns>
		public IDateTime Add( TimeSpan ts )
		{
			return this + ts;
		}

		/// <summary>
		///     Subtracts the specified time span.
		/// </summary>
		/// <param name="ts">The time span.</param>
		/// <returns></returns>
		public IDateTime Subtract( TimeSpan ts )
		{
			return this - ts;
		}

		/// <summary>
		///     Subtracts the specified date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public TimeSpan Subtract( IDateTime dt )
		{
			return this - dt;
		}

		/// <summary>
		///     Adds the years.
		/// </summary>
		/// <param name="years">The years.</param>
		/// <returns></returns>
		public IDateTime AddYears( int years )
		{
			var dt = Copy<IDateTime>( );
			dt.Value = Value.AddYears( years );
			return dt;
		}

		/// <summary>
		///     Adds the months.
		/// </summary>
		/// <param name="months">The months.</param>
		/// <returns></returns>
		public IDateTime AddMonths( int months )
		{
			var dt = Copy<IDateTime>( );
			dt.Value = Value.AddMonths( months );
			return dt;
		}

		/// <summary>
		///     Adds the days.
		/// </summary>
		/// <param name="days">The days.</param>
		/// <returns></returns>
		public IDateTime AddDays( int days )
		{
			var dt = Copy<IDateTime>( );
			dt.Value = Value.AddDays( days );
			return dt;
		}

		/// <summary>
		///     Adds the hours.
		/// </summary>
		/// <param name="hours">The hours.</param>
		/// <returns></returns>
		public IDateTime AddHours( int hours )
		{
			var dt = Copy<IDateTime>( );
			if ( !dt.HasTime && hours % 24 > 0 )
			{
				dt.HasTime = true;
			}
			dt.Value = Value.AddHours( hours );
			return dt;
		}

		/// <summary>
		///     Adds the minutes.
		/// </summary>
		/// <param name="minutes">The minutes.</param>
		/// <returns></returns>
		public IDateTime AddMinutes( int minutes )
		{
			var dt = Copy<IDateTime>( );
			if ( !dt.HasTime && minutes % 1440 > 0 )
			{
				dt.HasTime = true;
			}
			dt.Value = Value.AddMinutes( minutes );
			return dt;
		}

		/// <summary>
		///     Adds the seconds.
		/// </summary>
		/// <param name="seconds">The seconds.</param>
		/// <returns></returns>
		public IDateTime AddSeconds( int seconds )
		{
			var dt = Copy<IDateTime>( );
			if ( !dt.HasTime && seconds % 86400 > 0 )
			{
				dt.HasTime = true;
			}
			dt.Value = Value.AddSeconds( seconds );
			return dt;
		}

		/// <summary>
		///     Adds the milliseconds.
		/// </summary>
		/// <param name="milliseconds">The milliseconds.</param>
		/// <returns></returns>
		public IDateTime AddMilliseconds( int milliseconds )
		{
			var dt = Copy<IDateTime>( );
			if ( !dt.HasTime && milliseconds % 86400000 > 0 )
			{
				dt.HasTime = true;
			}
			dt.Value = Value.AddMilliseconds( milliseconds );
			return dt;
		}

		/// <summary>
		///     Adds the ticks.
		/// </summary>
		/// <param name="ticks">The ticks.</param>
		/// <returns></returns>
		public IDateTime AddTicks( long ticks )
		{
			var dt = Copy<IDateTime>( );
			dt.HasTime = true;
			dt.Value = Value.AddTicks( ticks );
			return dt;
		}

		/// <summary>
		///     Less than.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public bool LessThan( IDateTime dt )
		{
			return this < dt;
		}

		/// <summary>
		///     Greater than.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public bool GreaterThan( IDateTime dt )
		{
			return this > dt;
		}

		/// <summary>
		///     Lesser than or equal.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public bool LessThanOrEqual( IDateTime dt )
		{
			return this <= dt;
		}

		/// <summary>
		///     Greater than or equal.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public bool GreaterThanOrEqual( IDateTime dt )
		{
			return this >= dt;
		}

		/// <summary>
		///     Associates the with.
		/// </summary>
		/// <param name="dt">The date time.</param>
		public void AssociateWith( IDateTime dt )
		{
			if ( AssociatedObject == null && dt.AssociatedObject != null )
			{
				AssociatedObject = dt.AssociatedObject;
			}
			else if ( AssociatedObject != null && dt.AssociatedObject == null )
			{
				dt.AssociatedObject = AssociatedObject;
			}

			// If these share the same TZID, then let's see if we
			// can share the time zone observance also!
			if ( TzId != null && string.Equals( TzId, dt.TzId ) )
			{
				if ( TimeZoneObservance != null && dt.TimeZoneObservance == null )
				{
					IDateTime normalizedDt = new iCalDateTime( TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUtc( dt.Value ) );
					if ( TimeZoneObservance.Value.Contains( normalizedDt ) )
					{
						dt.TimeZoneObservance = TimeZoneObservance;
					}
				}
				else if ( dt.TimeZoneObservance != null && TimeZoneObservance == null )
				{
					IDateTime normalizedDt = new iCalDateTime( dt.TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUtc( Value ) );
					if ( dt.TimeZoneObservance.Value.Contains( normalizedDt ) )
					{
						TimeZoneObservance = dt.TimeZoneObservance;
					}
				}
			}
		}

		#endregion

		#region IComparable Members

		/// <summary>
		///     Compares to.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">An error occurred while comparing two IDateTime values.</exception>
		public int CompareTo( IDateTime dt )
		{
			if ( Equals( dt ) )
			{
				return 0;
			}

			if ( this < dt )
			{
				return -1;
			}

			if ( this > dt )
			{
				return 1;
			}
			throw new Exception( "An error occurred while comparing two IDateTime values." );
		}

		#endregion

		#region IFormattable Members

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public string ToString( string format )
		{
			return ToString( format, null );
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public string ToString( string format, IFormatProvider formatProvider )
		{
			string tz = TimeZoneName;
			if ( !string.IsNullOrEmpty( tz ) )
			{
				tz = " " + tz;
			}

			if ( format != null )
			{
				return Value.ToString( format, formatProvider ) + tz;
			}

			if ( HasTime && HasDate )
			{
				return Value.ToString( Thread.CurrentThread.CurrentCulture ) + tz;
			}

			if ( HasTime )
			{
				return Value.TimeOfDay.ToString( ) + tz;
			}

			return Value.ToShortDateString( ) + tz;
		}

		#endregion
	}
}