// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     WeekDaySerializer class.
	/// </summary>
	public class WeekDaySerializer : EncodableDataTypeSerializer
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
				return typeof ( WeekDay );
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

			// Create the day specifier and associate it with a calendar object
			var ds = CreateAndAssociate( ) as IWeekDay;

			// Decode the value, if necessary
			value = Decode( ds, value );

			Match match = Regex.Match( value, @"(\+|-)?(\d{1,2})?(\w{2})" );
			if ( match.Success )
			{
				if ( match.Groups[ 2 ].Success )
				{
					if ( ds != null )
					{
						ds.Offset = Convert.ToInt32( match.Groups[ 2 ].Value );
						if ( match.Groups[ 1 ].Success && match.Groups[ 1 ].Value.Contains( "-" ) )
						{
							ds.Offset *= -1;
						}
					}
				}
				if ( ds != null )
				{
					ds.DayOfWeek = RecurrencePatternSerializer.GetDayOfWeek( match.Groups[ 3 ].Value );
					return ds;
				}
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
			var ds = obj as IWeekDay;
			if ( ds != null )
			{
				string value = string.Empty;
				if ( ds.Offset != int.MinValue )
				{
					value += ds.Offset;
				}

				string name = Enum.GetName( typeof ( DayOfWeek ), ds.DayOfWeek );

				if ( name != null )
				{
					value += name.ToUpper( ).Substring( 0, 2 );
				}

				return Encode( ds, value );
			}
			return null;
		}
	}
}