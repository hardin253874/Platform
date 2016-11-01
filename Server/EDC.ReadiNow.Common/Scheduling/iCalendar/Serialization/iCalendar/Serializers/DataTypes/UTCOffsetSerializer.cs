// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     UtcOffsetSerializer class.
	/// </summary>
	public class UtcOffsetSerializer : EncodableDataTypeSerializer
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
				return typeof ( UtcOffset );
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

			var offset = CreateAndAssociate( ) as IUtcOffset;
			if ( offset != null )
			{
				// Decode the value as necessary
				value = Decode( offset, value );

				Match match = Regex.Match( value, @"(\+|-)(\d{2})(\d{2})(\d{2})?" );
				if ( match.Success )
				{
					try
					{
						// NOTE: Fixes bug #1874174 - TimeZone positive UTCOffsets don't parse correctly
						if ( match.Groups[ 1 ].Value == "+" )
						{
							offset.Positive = true;
						}
						offset.Hours = Int32.Parse( match.Groups[ 2 ].Value );
						offset.Minutes = Int32.Parse( match.Groups[ 3 ].Value );
						if ( match.Groups[ 4 ].Success )
						{
							offset.Seconds = Int32.Parse( match.Groups[ 4 ].Value );
						}
					}
					catch
					{
						return null;
					}
					return offset;
				}

				return false;
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
			var offset = obj as IUtcOffset;
			if ( offset != null )
			{
				string value =
					( offset.Positive ? "+" : "-" ) +
					offset.Hours.ToString( "00" ) +
					offset.Minutes.ToString( "00" ) +
					( offset.Seconds != 0 ? offset.Seconds.ToString( "00" ) : string.Empty );

				// Encode the value as necessary
				return Encode( offset, value );
			}
			return null;
		}
	}
}