// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.Database.Types
{
	/// <summary>
	///     Time data ranging in value from 00:00:00.0000000 to 23:59:59.9999999 to an accuracy of 100 nanoseconds.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[XmlType( Namespace = Constants.StructuredQueryNamespace )]
	public class TimeType : DatabaseType
	{
        /// <summary>
        ///     The format string for serialising/deserialising a TimeSpan type.
        /// </summary>
        public static readonly string TimeFormatString = @"HH\:mm\:ss";
        
		/// <summary>
		///     The format string used to convert a Time type to it's supported SQL string format.
		/// </summary>
        private const string SqlTimeFormatString = @"1753-01-01T{0:hh\:mm\:ss}";

		/// <summary>
		///     Returns whether it is possible to transform this value in a manner that places values on a relative linear scale.
		/// </summary>
		public override bool CanLinearScale
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent DbType.
		/// </summary>
		public static DbType DbType
		{
			get
			{
				return DbType.Time;
			}
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		public static string DisplayName
		{
			get
			{
				return "Time";
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent runtime type.
		/// </summary>
		public static Type RunTimeType
		{
			get
			{
				return typeof ( TimeSpan );
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		public static SqlDbType SqlDbType
		{
			get
			{
				return SqlDbType.Time;
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		/// <returns></returns>
		public static string SqlDbTypeString
		{
			get
			{
				return Enum.Format( typeof ( SqlDbType ), SqlDbType, "g" ).ToLower( );
			}
		}

		/// <summary>
		///     Converts a database value from a string encoding to its native type.
		/// </summary>
		/// <param name="value">
		///     A string containing the database value.
		/// </param>
		/// <returns>
		///     An object representing the database value.
		/// </returns>
		public override object ConvertFromString( string value )
		{
			object obj = null;

			if ( !String.IsNullOrEmpty( value ) )
			{
                if (value == DateTimeType.DefaultValueNow )
                {
                    return DateTime.Now; // server time (should be adjusted for local... but isn't)
                }

                TimeSpan temp;
			    if (!TimeSpan.TryParseExact(value, TimeFormatString, CultureInfo.InvariantCulture, TimeSpanStyles.None, out temp))
			    {
                    DateTime dateTimeValue;
                    temp = DateTime.TryParseExact(value, DateTimeType.DateTimeFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeValue)
                        ? dateTimeValue.TimeOfDay
                        : DateTime.Parse(value).TimeOfDay;
			    }
			    obj = NewTime(temp); // has to be one or the other. using datetime.
			}

			return obj;
		}

		/// <summary>
		///     Converts the value to a type that can be embedded directly in a SQL query, inclusive of any applicable quotes and escaping.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string literal representation of the database value.
		/// </returns>
		public override string ConvertToSqlLiteral( object value )
		{
			string sqlString = ConvertToSqlString( value );

            return "try_convert(datetime, '" + sqlString + "')";
		}

		/// <summary>
		///     Converts the database value from its native type to a string encoding
		///     supported by SQL server, that can be used in queries.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		public override string ConvertToSqlString( object value )
		{
			string text = string.Empty;

			if ( ( value != null ) && ( !( value is DBNull ) ) )
			{
                // Hmm.. it should be standardized what type of value is passed in here.
			    TimeSpan timeSpan;
			    if (value is DateTime)
			        timeSpan = ((DateTime)value).TimeOfDay;
                else
                    timeSpan = (TimeSpan)value;

                text = string.Format(CultureInfo.InvariantCulture, SqlTimeFormatString, timeSpan);
			}

			return text;
		}

		/// <summary>
		///     Converts the database value from its native type to a string encoding.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		public override string ConvertToString( object value )
		{
			string text = string.Empty;
			//TODO:  This is hack code. we need remove later
			bool valueIsEmptyString = false;
			var stringVal = value as string;
			if ( stringVal != null )
			{
				valueIsEmptyString = string.IsNullOrEmpty( stringVal );
			}

			if ( ( value != null ) && ( !( value is DBNull ) && !valueIsEmptyString ) )
			{
				if ( value is TimeSpan )
				{
					var temp = ( TimeSpan ) value;
					text = temp.ToString( "c" );
				}
				else if ( value is DateTime )
				{
					var temp = ( DateTime ) value; // Note: .Net database uses TimeSpan for times
					TimeSpan temp2 = temp - temp.Date;
					text = temp2.ToString( "c" );
				}
			}
			return text;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent DbType.
		/// </summary>
		public override DbType GetDbType( )
		{
			return DbType;
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		public override string GetDisplayName( )
		{
			return DisplayName;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent runtime type.
		/// </summary>
		public override Type GetRunTimeType( )
		{
			return RunTimeType;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		public override SqlDbType GetSqlDbType( )
		{
			return SqlDbType;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		/// <returns></returns>
		public override string GetSqlDbTypeString( )
		{
			return SqlDbTypeString;
		}

        /// <summary>
        /// Create a new DateTime object that represents the specified time.
        /// </summary>
	    public static DateTime NewTime(int hour, int minute, int second)
	    {
            return SqlDateTime.MinValue.Value + new TimeSpan(hour, minute, second);
	    }

        /// <summary>
        /// Create a new DateTime object that represents the specified time.
        /// </summary>
        public static DateTime NewTime(TimeSpan time)
        {
            DateTime tmp = SqlDateTime.MinValue.Value + time;
            return DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
        }
	}
}