// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.IO;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents the geographical location of an
	///     <see cref="Event" /> or <see cref="Todo" /> item.
	/// </summary>
	[DebuggerDisplay( "{Latitude};{Longitude}" )]
	[Serializable]
	public class GeographicLocation : EncodableDataType, IGeographicLocation
	{
		/// <summary>
		///     Latitude.
		/// </summary>
		private double _latitude;

		/// <summary>
		///     Longitude.
		/// </summary>
		private double _longitude;

		/// <summary>
		///     Gets or sets the latitude.
		/// </summary>
		/// <value>
		///     The latitude.
		/// </value>
		public double Latitude
		{
			get
			{
				return _latitude;
			}
			set
			{
				_latitude = value;
			}
		}

		/// <summary>
		///     Gets or sets the longitude.
		/// </summary>
		/// <value>
		///     The longitude.
		/// </value>
		public double Longitude
		{
			get
			{
				return _longitude;
			}
			set
			{
				_longitude = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GeographicLocation" /> class.
		/// </summary>
		public GeographicLocation( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GeographicLocation" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public GeographicLocation( string value )
			: this( )
		{
			var serializer = new GeographicLocationSerializer( );

			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GeographicLocation" /> class.
		/// </summary>
		/// <param name="latitude">The latitude.</param>
		/// <param name="longitude">The longitude.</param>
		public GeographicLocation( double latitude, double longitude )
		{
			Latitude = latitude;
			Longitude = longitude;
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
			var geographicLocation = obj as IGeographicLocation;

			if ( geographicLocation != null )
			{
				IGeographicLocation g = geographicLocation;
				return g.Latitude.Equals( Latitude ) && g.Longitude.Equals( Longitude );
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
			return Latitude.GetHashCode( ) ^ Longitude.GetHashCode( );
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var geographicLocation = obj as IGeographicLocation;

			if ( geographicLocation != null )
			{
				IGeographicLocation g = geographicLocation;
				Latitude = g.Latitude;
				Longitude = g.Longitude;
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
			return Latitude.ToString( "0.000000" ) + ";" + Longitude.ToString( "0.000000" );
		}
	}
}