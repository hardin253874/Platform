// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     RecurrencePatternSerializer class.
	/// </summary>
	public class RecurrencePatternSerializer : EncodableDataTypeSerializer
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
				return typeof ( RecurrencePattern );
			}
		}

		/// <summary>
		///     Checks the mutually exclusive.
		/// </summary>
		/// <typeparam name="TFirst">The type of the first.</typeparam>
		/// <typeparam name="TSecond">The type of the second.</typeparam>
		/// <param name="name1">The name1.</param>
		/// <param name="name2">The name2.</param>
		/// <param name="obj1">The obj1.</param>
		/// <param name="obj2">The obj2.</param>
		/// <exception cref="System.ArgumentException">Both  + name1 +  and  + name2 +  cannot be supplied together; they are mutually exclusive.</exception>
		public virtual void CheckMutuallyExclusive<TFirst, TSecond>( string name1, string name2, TFirst obj1, TSecond obj2 )
		{
			if ( Equals( obj1, default( TFirst ) ) || Equals( obj2, default( TSecond ) ) )
			{
				return;
			}

			// If the object is MinValue instead of its default, consider
			// that to be unassigned.

			Type t1 = obj1.GetType( );
			Type t2 = obj2.GetType( );

			FieldInfo fi1 = t1.GetField( "MinValue" );
			FieldInfo fi2 = t2.GetField( "MinValue" );

			bool isMin1 = fi1 != null && obj1.Equals( fi1.GetValue( null ) );
			bool isMin2 = fi2 != null && obj2.Equals( fi2.GetValue( null ) );
			if ( isMin1 || isMin2 )
			{
				return;
			}

			throw new ArgumentException( "Both " + name1 + " and " + name2 + " cannot be supplied together; they are mutually exclusive." );
		}

		/// <summary>
		///     Checks the range.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="values">The values.</param>
		/// <param name="min">The min.</param>
		/// <param name="max">The max.</param>
		public virtual void CheckRange( string name, IList<int> values, int min, int max )
		{
			bool allowZero = ( min == 0 || max == 0 );
			foreach ( int value in values )
			{
				CheckRange( name, value, min, max, allowZero );
			}
		}

		/// <summary>
		///     Checks the range.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="min">The min.</param>
		/// <param name="max">The max.</param>
		public virtual void CheckRange( string name, int value, int min, int max )
		{
			CheckRange( name, value, min, max, ( min == 0 || max == 0 ) );
		}

		/// <summary>
		///     Checks the range.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="min">The min.</param>
		/// <param name="max">The max.</param>
		/// <param name="allowZero">
		///     if set to <c>true</c> [allow zero].
		/// </param>
		/// <exception cref="System.ArgumentException"></exception>
		public virtual void CheckRange( string name, int value, int min, int max, bool allowZero )
		{
			if ( value != int.MinValue && ( value < min || value > max || ( !allowZero && value == 0 ) ) )
			{
				throw new ArgumentException( name + " value " + value + " is out of range. Valid values are between " + min + " and " + max + ( allowZero ? "" : ", excluding zero (0)" ) + "." );
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

			// Instantiate the data type
			var r = CreateAndAssociate( ) as IRecurrencePattern;
			var factory = GetService<ISerializerFactory>( );

			if ( r != null && factory != null )
			{
				// Decode the value, if necessary
				value = Decode( r, value );

				Match match = Regex.Match( value, @"FREQ=(SECONDLY|MINUTELY|HOURLY|DAILY|WEEKLY|MONTHLY|YEARLY);?(.*)", RegexOptions.IgnoreCase );
				if ( match.Success )
				{
					// Parse the frequency type
					r.Frequency = ( FrequencyType ) Enum.Parse( typeof ( FrequencyType ), match.Groups[ 1 ].Value, true );

					// NOTE: fixed a bug where the group 2 match
					// resulted in an empty string, which caused
					// an error.
					if ( match.Groups[ 2 ].Success &&
					     match.Groups[ 2 ].Length > 0 )
					{
						string[] keywordPairs = match.Groups[ 2 ].Value.Split( ';' );
						foreach ( string keywordPair in keywordPairs )
						{
							string[] keyValues = keywordPair.Split( '=' );
							string keyword = keyValues[ 0 ];
							string keyValue = keyValues[ 1 ];

							switch ( keyword.ToUpper( ) )
							{
								case "UNTIL":
									{
										var serializer = factory.Build( typeof ( IDateTime ), SerializationContext ) as IStringSerializer;
										if ( serializer != null )
										{
											var dt = serializer.Deserialize( new StringReader( keyValue ) ) as IDateTime;
											if ( dt != null )
											{
												r.Until = dt.Value;
											}
										}
									}
									break;
								case "COUNT":
									r.Count = Convert.ToInt32( keyValue );
									break;
								case "INTERVAL":
									r.Interval = Convert.ToInt32( keyValue );
									break;
								case "BYSECOND":
									AddInt32Values( r.BySecond, keyValue );
									break;
								case "BYMINUTE":
									AddInt32Values( r.ByMinute, keyValue );
									break;
								case "BYHOUR":
									AddInt32Values( r.ByHour, keyValue );
									break;
								case "BYDAY":
									{
										string[] days = keyValue.Split( ',' );
										foreach ( string day in days )
										{
											r.ByDay.Add( new WeekDay( day ) );
										}
									}
									break;
								case "BYMONTHDAY":
									AddInt32Values( r.ByMonthDay, keyValue );
									break;
								case "BYYEARDAY":
									AddInt32Values( r.ByYearDay, keyValue );
									break;
								case "BYWEEKNO":
									AddInt32Values( r.ByWeekNo, keyValue );
									break;
								case "BYMONTH":
									AddInt32Values( r.ByMonth, keyValue );
									break;
								case "BYSETPOS":
									AddInt32Values( r.BySetPosition, keyValue );
									break;
								case "WKST":
									r.FirstDayOfWeek = GetDayOfWeek( keyValue );
									break;
							}
						}
					}
				}
                
					//
					// This matches strings such as:
					//
					// "Every 6 minutes"
					// "Every 3 days"
					//
				else if ( ( match = Regex.Match( value, @"every\s+(?<Interval>other|\d+)?\w{0,2}\s*(?<Freq>second|minute|hour|day|week|month|year)s?,?\s*(?<More>.+)", RegexOptions.IgnoreCase ) ).Success )
				{
					if ( match.Groups[ "Interval" ].Success )
					{
						int interval;
						r.Interval = !int.TryParse( match.Groups[ "Interval" ].Value, out interval ) ? 2 : interval;
					}
					else
					{
						r.Interval = 1;
					}

					switch ( match.Groups[ "Freq" ].Value.ToLower( ) )
					{
						case "second":
							r.Frequency = FrequencyType.Secondly;
							break;
						case "minute":
							r.Frequency = FrequencyType.Minutely;
							break;
						case "hour":
							r.Frequency = FrequencyType.Hourly;
							break;
						case "day":
							r.Frequency = FrequencyType.Daily;
							break;
						case "week":
							r.Frequency = FrequencyType.Weekly;
							break;
						case "month":
							r.Frequency = FrequencyType.Monthly;
							break;
						case "year":
							r.Frequency = FrequencyType.Yearly;
							break;
					}

					string[] values = match.Groups[ "More" ].Value.Split( ',' );
					foreach ( string item in values )
					{
						if ( ( match = Regex.Match( item, @"(?<Num>\d+)\w\w\s+(?<Type>second|minute|hour|day|week|month)", RegexOptions.IgnoreCase ) ).Success ||
						     ( match = Regex.Match( item, @"(?<Type>second|minute|hour|day|week|month)\s+(?<Num>\d+)", RegexOptions.IgnoreCase ) ).Success )
						{
							int num;
							if ( int.TryParse( match.Groups[ "Num" ].Value, out num ) )
							{
								switch ( match.Groups[ "Type" ].Value.ToLower( ) )
								{
									case "second":
										r.BySecond.Add( num );
										break;
									case "minute":
										r.ByMinute.Add( num );
										break;
									case "hour":
										r.ByHour.Add( num );
										break;
									case "day":
										switch ( r.Frequency )
										{
											case FrequencyType.Yearly:
												r.ByYearDay.Add( num );
												break;
											case FrequencyType.Monthly:
												r.ByMonthDay.Add( num );
												break;
										}
										break;
									case "week":
										r.ByWeekNo.Add( num );
										break;
									case "month":
										r.ByMonth.Add( num );
										break;
								}
							}
						}
						else if ( ( match = Regex.Match( item, @"(?<Num>\d+\w{0,2})?(\w|\s)+?(?<First>first)?(?<Last>last)?\s*((?<Day>sunday|monday|tuesday|wednesday|thursday|friday|saturday)\s*(and|or)?\s*)+", RegexOptions.IgnoreCase ) ).Success )
						{
							int num = int.MinValue;
							if ( match.Groups[ "Num" ].Success )
							{
								if ( int.TryParse( match.Groups[ "Num" ].Value, out num ) )
								{
									if ( match.Groups[ "Last" ].Success )
									{
										// Make number negative
										num *= -1;
									}
								}
							}
							else if ( match.Groups[ "Last" ].Success )
							{
								num = -1;
							}
							else if ( match.Groups[ "First" ].Success )
							{
								num = 1;
							}

							foreach ( Capture capture in match.Groups[ "Day" ].Captures )
							{
								var ds = new WeekDay( ( DayOfWeek ) Enum.Parse( typeof ( DayOfWeek ), capture.Value, true ) )
									{
										Offset = num
									};
								r.ByDay.Add( ds );
							}
						}
						else if ( ( match = Regex.Match( item, @"at\s+(?<Hour>\d{1,2})(:(?<Minute>\d{2})((:|\.)(?<Second>\d{2}))?)?\s*(?<Meridian>(a|p)m?)?", RegexOptions.IgnoreCase ) ).Success )
						{
							int hour;

							if ( int.TryParse( match.Groups[ "Hour" ].Value, out hour ) )
							{
								// Adjust for PM
								if ( match.Groups[ "Meridian" ].Success &&
								     match.Groups[ "Meridian" ].Value.ToUpper( ).StartsWith( "P" ) )
								{
									hour += 12;
								}

								r.ByHour.Add( hour );

								int minute;
								if ( match.Groups[ "Minute" ].Success &&
								     int.TryParse( match.Groups[ "Minute" ].Value, out minute ) )
								{
									r.ByMinute.Add( minute );
									int second;
									if ( match.Groups[ "Second" ].Success &&
									     int.TryParse( match.Groups[ "Second" ].Value, out second ) )
									{
										r.BySecond.Add( second );
									}
								}
							}
						}
						else if ( ( match = Regex.Match( item, @"^\s*until\s+(?<DateTime>.+)$", RegexOptions.IgnoreCase ) ).Success )
						{
							DateTime dt = DateTime.Parse( match.Groups[ "DateTime" ].Value );
							DateTime.SpecifyKind( dt, DateTimeKind.Utc );

							r.Until = dt;
						}
						else if ( ( match = Regex.Match( item, @"^\s*for\s+(?<Count>\d+)\s+occurrences\s*$", RegexOptions.IgnoreCase ) ).Success )
						{
							int count;
							if ( !int.TryParse( match.Groups[ "Count" ].Value, out count ) )
							{
								return false;
							}

							r.Count = count;
						}
					}
				}
				else
				{
					// Couldn't parse the object, return null!
					r = null;
				}

				if ( r != null )
				{
					CheckMutuallyExclusive( "COUNT", "UNTIL", r.Count, r.Until );
					CheckRange( "INTERVAL", r.Interval, 0, int.MaxValue );
					CheckRange( "COUNT", r.Count, 0, int.MaxValue );
					CheckRange( "BYSECOND", r.BySecond, 0, 59 );
					CheckRange( "BYMINUTE", r.ByMinute, 0, 59 );
					CheckRange( "BYHOUR", r.ByHour, 0, 23 );
					CheckRange( "BYMONTHDAY", r.ByMonthDay, -31, 31 );
					CheckRange( "BYYEARDAY", r.ByYearDay, -366, 366 );
					CheckRange( "BYWEEKNO", r.ByWeekNo, -53, 53 );
					CheckRange( "BYMONTH", r.ByMonth, 1, 12 );
					CheckRange( "BYSETPOS", r.BySetPosition, -366, 366 );
				}
			}

			return r;
		}

		/// <summary>
		///     Gets the day of week.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException"></exception>
		public static DayOfWeek GetDayOfWeek( string value )
		{
			switch ( value.ToUpper( ) )
			{
				case "SU":
					return DayOfWeek.Sunday;
				case "MO":
					return DayOfWeek.Monday;
				case "TU":
					return DayOfWeek.Tuesday;
				case "WE":
					return DayOfWeek.Wednesday;
				case "TH":
					return DayOfWeek.Thursday;
				case "FR":
					return DayOfWeek.Friday;
				case "SA":
					return DayOfWeek.Saturday;
			}
			throw new ArgumentException( value + " is not a valid iCal day-of-week indicator." );
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var recur = obj as IRecurrencePattern;
			var factory = GetService<ISerializerFactory>( );
			if ( recur != null && factory != null )
			{
				// Push the recurrence pattern onto the serialization stack
				SerializationContext.Push( recur );

				string name = Enum.GetName( typeof ( FrequencyType ), recur.Frequency );

				if ( name != null )
				{
					var values = new List<string>
						{
							"FREQ=" + name.ToUpper( )
						};

					//-- FROM RFC2445 --
					//The INTERVAL rule part contains a positive integer representing how
					//often the recurrence rule repeats. The default value is "1", meaning
					//every second for a SECONDLY rule, or every minute for a MINUTELY
					//rule, every hour for an HOURLY rule, every day for a DAILY rule,
					//every week for a WEEKLY rule, every month for a MONTHLY rule and
					//every year for a YEARLY rule.
					int interval = recur.Interval;
					if ( interval == int.MinValue )
					{
						interval = 1;
					}

					if ( interval != 1 )
					{
						values.Add( "INTERVAL=" + interval );
					}

					if ( recur.Until != DateTime.MinValue )
					{
						var serializer = factory.Build( typeof ( IDateTime ), SerializationContext ) as IStringSerializer;
						if ( serializer != null )
						{
							IDateTime until = new iCalDateTime( recur.Until );
							until.HasTime = true;
							values.Add( "UNTIL=" + serializer.SerializeToString( until ) );
						}
					}

					if ( recur.FirstDayOfWeek != DayOfWeek.Monday )
					{
						string s = Enum.GetName( typeof ( DayOfWeek ), recur.FirstDayOfWeek );
						if ( s != null )
						{
							values.Add( "WKST=" + s.ToUpper( ).Substring( 0, 2 ) );
						}
					}

					if ( recur.Count != int.MinValue )
					{
						values.Add( "COUNT=" + recur.Count );
					}

					if ( recur.ByDay.Count > 0 )
					{
						var bydayValues = new List<string>( );

						var serializer = factory.Build( typeof ( IWeekDay ), SerializationContext ) as IStringSerializer;
						if ( serializer != null )
						{
							bydayValues.AddRange( from WeekDay byday in recur.ByDay
							                      select serializer.SerializeToString( byday ) );
						}

						values.Add( "BYDAY=" + string.Join( ",", bydayValues.ToArray( ) ) );
					}

					SerializeByValue( values, recur.ByHour, "BYHOUR" );
					SerializeByValue( values, recur.ByMinute, "BYMINUTE" );
					SerializeByValue( values, recur.ByMonth, "BYMONTH" );
					SerializeByValue( values, recur.ByMonthDay, "BYMONTHDAY" );
					SerializeByValue( values, recur.BySecond, "BYSECOND" );
					SerializeByValue( values, recur.BySetPosition, "BYSETPOS" );
					SerializeByValue( values, recur.ByWeekNo, "BYWEEKNO" );
					SerializeByValue( values, recur.ByYearDay, "BYYEARDAY" );

					// Pop the recurrence pattern off the serialization stack
					SerializationContext.Pop( );

					return Encode( recur, string.Join( ";", values.ToArray( ) ) );
				}
			}
			return null;
		}

		/// <summary>
		///     Adds the int32 values.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="value">The value.</param>
		protected static void AddInt32Values( IList<int> list, string value )
		{
			string[] values = value.Split( ',' );
			foreach ( string v in values )
			{
				list.Add( Convert.ToInt32( v ) );
			}
		}

		/// <summary>
		///     Serializes the by value.
		/// </summary>
		/// <param name="aggregate">The aggregate.</param>
		/// <param name="byValue">The by value.</param>
		/// <param name="name">The name.</param>
		private void SerializeByValue( List<string> aggregate, IList<int> byValue, string name )
		{
			if ( byValue.Count > 0 )
			{
				aggregate.Add( name + "=" + string.Join( ",", byValue.Select( i => i.ToString( CultureInfo.InvariantCulture ) ).ToArray( ) ) );
			}
		}
	}
}