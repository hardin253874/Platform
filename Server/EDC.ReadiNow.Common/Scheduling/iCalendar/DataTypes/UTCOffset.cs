// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Represents a time offset from UTC (Coordinated Universal Time).
	/// </summary>
	[Serializable]
	public class UtcOffset : EncodableDataType, IUtcOffset
	{
		/// <summary>
		///     Positive.
		/// </summary>
		private bool _positive;

		/// <summary>
		///     Hours.
		/// </summary>
		private int _hours;

		/// <summary>
		///     Minutes.
		/// </summary>
		private int _minutes;

		/// <summary>
		///     Seconds.
		/// </summary>
		private int _seconds;

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="UtcOffset" /> is positive.
		/// </summary>
		/// <value>
		///     <c>true</c> if positive; otherwise, <c>false</c>.
		/// </value>
		public bool Positive
		{
			get
			{
				return _positive;
			}
			set
			{
				_positive = value;
			}
		}

		/// <summary>
		///     Gets or sets the hours.
		/// </summary>
		/// <value>
		///     The hours.
		/// </value>
		public int Hours
		{
			get
			{
				return _hours;
			}
			set
			{
				_hours = value;
			}
		}

		/// <summary>
		///     Gets or sets the minutes.
		/// </summary>
		/// <value>
		///     The minutes.
		/// </value>
		public int Minutes
		{
			get
			{
				return _minutes;
			}
			set
			{
				_minutes = value;
			}
		}

		/// <summary>
		///     Gets or sets the seconds.
		/// </summary>
		/// <value>
		///     The seconds.
		/// </value>
		public int Seconds
		{
			get
			{
				return _seconds;
			}
			set
			{
				_seconds = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="UtcOffset" /> class.
		/// </summary>
		public UtcOffset( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="UtcOffset" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public UtcOffset( string value )
			: this( )
		{
			var serializer = new UtcOffsetSerializer( );
			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="UtcOffset" /> class.
		/// </summary>
		/// <param name="ts">The time span.</param>
		public UtcOffset( TimeSpan ts )
		{
			if ( ts.Ticks >= 0 )
			{
				Positive = true;
			}
			Hours = Math.Abs( ts.Hours );
			Minutes = Math.Abs( ts.Minutes );
			Seconds = Math.Abs( ts.Seconds );
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
			var o = obj as UtcOffset;
			if ( o != null )
			{
				return Equals( Positive, o.Positive ) &&
				       Equals( Hours, o.Hours ) &&
				       Equals( Minutes, o.Minutes ) &&
				       Equals( Seconds, o.Seconds );
			}
// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var offset = obj as IUtcOffset;

			if ( offset != null )
			{
				IUtcOffset utco = offset;
				Positive = utco.Positive;
				Hours = utco.Hours;
				Minutes = utco.Minutes;
				Seconds = utco.Seconds;
			}
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return ( Positive ? "+" : "-" ) +
			       Hours.ToString( "00" ) +
			       Minutes.ToString( "00" ) +
			       ( Seconds != 0 ? Seconds.ToString( "00" ) : string.Empty );
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
			hash = ( hash * 7 ) + Positive.GetHashCode( );
			hash = ( hash * 7 ) + Hours.GetHashCode( );
			hash = ( hash * 7 ) + Minutes.GetHashCode( );
			hash = ( hash * 7 ) + Seconds.GetHashCode( );

			return hash;
		}

		/// <summary>
		///     UTCs the offset.
		/// </summary>
		/// <param name="ts">The time span.</param>
		/// <returns></returns>
		public static implicit operator UtcOffset( TimeSpan ts )
		{
			return new UtcOffset( ts );
		}

		/// <summary>
		///     Times the span.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <returns></returns>
		public static explicit operator TimeSpan( UtcOffset o )
		{
			var ts = new TimeSpan( 0 );
			ts = ts.Add( TimeSpan.FromHours( o.Positive ? o.Hours : -o.Hours ) );
			ts = ts.Add( TimeSpan.FromMinutes( o.Positive ? o.Minutes : -o.Minutes ) );
			ts = ts.Add( TimeSpan.FromSeconds( o.Positive ? o.Seconds : -o.Seconds ) );
			return ts;
		}

		/// <summary>
		///     Offsets the specified date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="positive">
		///     if set to <c>true</c> [positive].
		/// </param>
		/// <returns></returns>
		private DateTime Offset( DateTime dt, bool positive )
		{
			if ( ( dt == DateTime.MinValue && positive ) ||
			     ( dt == DateTime.MaxValue && !positive ) )
			{
				return dt;
			}

			int mult = positive ? 1 : -1;
			dt = dt.AddHours( Hours * mult );
			dt = dt.AddMinutes( Minutes * mult );
			dt = dt.AddSeconds( Seconds * mult );
			return dt;
		}

		#region IUTCOffset Members

		/// <summary>
		///     To the UTC.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public virtual DateTime ToUtc( DateTime dt )
		{
			return DateTime.SpecifyKind( Offset( dt, !Positive ), DateTimeKind.Utc );
		}

		/// <summary>
		///     To the local.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public virtual DateTime ToLocal( DateTime dt )
		{
			return DateTime.SpecifyKind( Offset( dt, Positive ), DateTimeKind.Local );
		}

		#endregion
	}
}