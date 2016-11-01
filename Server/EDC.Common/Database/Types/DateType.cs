// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.Database.Types
{
	/// <summary>
	///     Date data ranging in value from January 1, 0001 to December 31, 9999 to an accuracy of 1 day.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[XmlType( Namespace = Constants.StructuredQueryNamespace )]
	public class DateType : DatabaseType
	{/// <summary>
     /// The string used in a field's defaultValue to indicate that the current time should be used.
     /// </summary>
        public const string DefaultValueToday = "TODAY";

        /// <summary>
        ///     The format string used to convert a Date type to it's supported SQL string format.
        /// </summary>
        private const string SqlDateFormatString = @"yyyy-MM-dd";

		/// <summary>
		///     The format string for serialising/deserialising a Date type.
		/// </summary>
		public static readonly string DateFormatString = @"yyyy-MM-dd";

		/// <summary>
		///     Returns an enumeration describing the equivalent DbType.
		/// </summary>
		public static DbType DbType
		{
			get
			{
				return DbType.Date;
			}
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		public static string DisplayName
		{
			get
			{
				return "Date";
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent runtime type.
		/// </summary>
		public static Type RunTimeType
		{
			get
			{
				return typeof ( DateTime );
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		public static SqlDbType SqlDbType
		{
			get
			{
				return SqlDbType.Date;
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
                if ( string.Equals( value, DateType.DefaultValueToday, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return DateTime.Today; // server time (should be adjusted for local... but isn't)
                }

                DateTime temp;
				if ( !DateTime.TryParseExact( value, DateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out temp ) )
				{
                    temp = DateTime.Parse( value, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal );
                }
				obj = temp;
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

			return "try_convert(date, '" + sqlString + "')";
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
				var temp = ( DateTime ) value;
				text = temp.ToString( SqlDateFormatString, CultureInfo.InvariantCulture );
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
				var stringVal2 = value as string;

				if ( stringVal2 != null )
				{
					DateTime temp = DateTime.Parse( stringVal2 );
					text = temp.ToString( DateFormatString, CultureInfo.InvariantCulture );
				}
				else
				{
					var temp = ( DateTime ) value;
					text = temp.ToString( DateFormatString, CultureInfo.InvariantCulture );
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
		/// <returns></returns>
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
	}
}