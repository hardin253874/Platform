// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     DateTime serializer.
	/// </summary>
	public class DateTimeSerializer :
		EncodableDataTypeSerializer
	{
		/// <summary>
		///     Date only regex pattern.
		/// </summary>
		private const string DateOnlyPattern = @"^((\d{4})(\d{2})(\d{2}))?$";

		/// <summary>
		///     Full regex pattern.
		/// </summary>
		private const string FullPattern = @"^((\d{4})(\d{2})(\d{2}))T((\d{2})(\d{2})(\d{2})(Z)?)$";

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
				return typeof ( iCalDateTime );
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

			var dt = CreateAndAssociate( ) as IDateTime;
			if ( dt != null )
			{
				// Decode the value as necessary
				value = Decode( dt, value );

				Match match = Regex.Match( value, FullPattern, RegexOptions.IgnoreCase );
				if ( !match.Success )
				{
					match = Regex.Match( value, DateOnlyPattern, RegexOptions.IgnoreCase );
				}

				if ( !match.Success )
				{
					return null;
				}

				DateTime now = DateTime.Now;

				int year = now.Year;
				int month = now.Month;
				int date = now.Day;
				int hour = 0;
				int minute = 0;
				int second = 0;

				if ( match.Groups[ 1 ].Success )
				{
					dt.HasDate = true;
					dt.HasTime = false;
					year = Convert.ToInt32( match.Groups[ 2 ].Value );
					month = Convert.ToInt32( match.Groups[ 3 ].Value );
					date = Convert.ToInt32( match.Groups[ 4 ].Value );
				}
				if ( match.Groups.Count >= 6 && match.Groups[ 5 ].Success )
				{
					// If 'VALUE=DATE' is present, then
					// let's force dt.HasTime to false.
					// NOTE: Fixes bug #3534283 - DateTimeSerializer bug on deserializing dates without time
					if ( dt.Parameters.ContainsKey( "VALUE" ) )
					{
						string valueType = dt.Parameters.Get( "VALUE" );
						if ( valueType != "DATE" )
						{
							dt.HasTime = true;
						}
					}
					else
					{
						dt.HasTime = true;
					}

					if ( dt.HasTime )
					{
						hour = Convert.ToInt32( match.Groups[ 6 ].Value );
						minute = Convert.ToInt32( match.Groups[ 7 ].Value );
						second = Convert.ToInt32( match.Groups[ 8 ].Value );
					}
				}

				if ( match.Groups[ 9 ].Success )
				{
					dt.IsUniversalTime = true;
				}

				dt.Value = CoerceDateTime( year, month, date, hour, minute, second, DateTimeKind.Utc );
				return dt;
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
			var dateTime = obj as IDateTime;

			if ( dateTime != null )
			{
				var dt = dateTime.Copy<IDateTime>( );

				// Assign the TZID for the date/time value.
				if ( dt.TzId != null )
				{
					dt.Parameters.Set( "TZID", dt.TzId );
				}

				// FIXME: what if DATE is the default value type for this?
				// Also, what if the DATE-TIME value type is specified on something
				// where DATE-TIME is the default value type?  It should be removed
				// during serialization, as it's redundant...
				if ( !dt.HasTime )
				{
					dt.SetValueType( "DATE" );
				}
				else
				{
					// Remove the 'VALUE' parameter, as it's already at
					// its default value
					if ( dt.Parameters.ContainsKey( "VALUE" ) )
					{
						dt.Parameters.Remove( "VALUE" );
					}
				}

				string value = string.Format( "{0:0000}{1:00}{2:00}", dt.Year, dt.Month, dt.Day );
				if ( dt.HasTime )
				{
					value += string.Format( "T{0:00}{1:00}{2:00}", dt.Hour, dt.Minute, dt.Second );
					if ( dt.IsUniversalTime )
					{
						value += "Z";
					}
				}

				// Encode the value as necessary
				return Encode( dt, value );
			}
			return null;
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
	}
}