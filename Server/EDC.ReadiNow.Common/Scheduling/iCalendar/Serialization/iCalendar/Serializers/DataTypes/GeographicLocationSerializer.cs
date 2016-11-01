// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     GeographicLocationSerializer class.
	/// </summary>
	public class GeographicLocationSerializer : EncodableDataTypeSerializer
	{
		/// <summary>
		///     Gets the type of the target.
		/// </summary>
		/// <value>
		///     The type of the target.
		/// </value>
		public override Type TargetType
		{
			get
			{
				return typeof ( GeographicLocation );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			string value = tr.ReadToEnd( );

			var g = CreateAndAssociate( ) as IGeographicLocation;
			if ( g != null )
			{
				// Decode the value, if necessary!
				value = Decode( g, value );

				string[] values = value.Split( ';' );
				if ( values.Length != 2 )
				{
					return false;
				}

				double lat;
				double lon;
				double.TryParse( values[ 0 ], out lat );
				double.TryParse( values[ 1 ], out lon );
				g.Latitude = lat;
				g.Longitude = lon;

				return g;
			}

			return null;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var g = obj as IGeographicLocation;
			if ( g != null )
			{
				string value = g.Latitude.ToString( "0.000000" ) + ";" + g.Longitude.ToString( "0.000000" );
				return Encode( g, value );
			}
			return null;
		}
	}
}