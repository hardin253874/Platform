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
    ///     Date and time data ranging in value from January 1, 1753 to December 31, 9999 to an accuracy of 3.33 milliseconds.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace, IsReference = true)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class DateTimeType : DatabaseType
    {
        /// <summary>
        /// The string used in a field's defaultValue to indicate that the current time should be used.
        /// </summary>
        public const string DefaultValueNow = "NOW";

        /// <summary>
        ///     The format string for serialising/deserialising a DateTime type.
        /// </summary>
        public const string DateTimeFormatString = @"yyyy-MM-dd HH\:mm\:ss";
        public const string ModifiedISODateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'fffK";    // ISO 8601 accurate to millisecond with delimeter removed (as permitted by the standard)

        /// <summary>
        ///     The format string used to convert a DateTime type to it's supported SQL string format.
        /// </summary>
        private const string SqlDateTimeFormatString = @"yyyy-MM-dd HH\:mm\:ss";

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
                return DbType.DateTime2;
            }
        }

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        public static string DisplayName
        {
            get
            {
                return "DateTime";
            }
        }

        /// <summary>
        ///     Returns an enumeration describing the equivalent runtime type.
        /// </summary>
        public static Type RunTimeType
        {
            get
            {
                return typeof(DateTime);
            }
        }

        /// <summary>
        ///     Returns an enumeration describing the equivalent SqlDbType.
        /// </summary>
        public static SqlDbType SqlDbType
        {
            get
            {
                return SqlDbType.DateTime2;
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
                return Enum.Format(typeof(SqlDbType), SqlDbType, "g").ToLower();
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
        public override object ConvertFromString(string value)
        {
            object obj = null;

            if (!String.IsNullOrEmpty(value))
            {
                if ( string.Equals( value, DateType.DefaultValueToday, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    // TODO: Make this work correctly for timezones
                    // hack untill this get fixed.
                    return DateTime.UtcNow; //return DateTime.Today;
                }

                if ( string.Equals( value, DefaultValueNow, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return DateTime.UtcNow;
                }

                DateTime temp;
                if (!DateTime.TryParseExact(value, DateTimeFormatString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out temp))
                {
                    temp = DateTime.Parse(value, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal );
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
        public override string ConvertToSqlLiteral(object value)
        {
            string sqlString = ConvertToSqlString(value);

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
        public override string ConvertToSqlString(object value)
        {
            string text = string.Empty;

            if ((value != null) && (!(value is DBNull)))
            {
                var temp = (DateTime)value;
                text = temp.ToString(SqlDateTimeFormatString, CultureInfo.InvariantCulture);
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
        public override string ConvertToString(object value)
        {
            return ConvertToString(value, DateTimeFormatString, false);
        }


        /// <summary>
        ///     Converts the database value from its native type to a string encoding. If the timezone is unspecified, assume it is UTC
        /// </summary>
        /// <param name="value">
        ///     An object representing the database value.
        /// </param>
        /// <param name="formatString">The formatting for the date time</param>
        /// <param name="unspecifiedAsUtc">If true, treat unspecified Kind date times as UTC values</param>

        /// <returns>
        ///     A string containing the string representation of the database value.
        /// </returns>
        public string ConvertToString(object value, string formatString, bool unspecifiedAsUtc)
        {
            string text = string.Empty;
            //TODO:  This is hack code. we need remove later
            bool valueIsEmptyString = false;
            var stringVal = value as string;

            if (stringVal != null)
            {
                valueIsEmptyString = string.IsNullOrEmpty(stringVal);
            }

            if ((value != null) && (!(value is DBNull) && !valueIsEmptyString))
            {
                var stringVal2 = value as string;
                DateTime temp;

                if (stringVal2 != null)
                {
                    temp = DateTime.Parse(stringVal2);
                }
                else
                {
                    temp = (DateTime)value;
                }

                //
                // Deal with the Kind being unspecified. Assume it is UTC, then covert it to local time. (This makes it easer for a user to read.)
                if (unspecifiedAsUtc && temp.Kind == DateTimeKind.Unspecified)
                {
                    temp = new DateTime(temp.Ticks, DateTimeKind.Utc).ToLocalTime();
                }
                text = temp.ToString(formatString, CultureInfo.InvariantCulture);

            }
            return text;
        }



        /// <summary>
        ///     Returns an enumeration describing the equivalent DbType.
        /// </summary>
        public override DbType GetDbType()
        {
            return DbType;
        }

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        public override string GetDisplayName()
        {
            return DisplayName;
        }

        /// <summary>
        ///     Returns an enumeration describing the equivalent runtime type.
        /// </summary>
        public override Type GetRunTimeType()
        {
            return RunTimeType;
        }

        /// <summary>
        ///     Returns an enumeration describing the equivalent SqlDbType.
        /// </summary>
        public override SqlDbType GetSqlDbType()
        {
            return SqlDbType;
        }

        /// <summary>
        ///     Returns an enumeration describing the equivalent SqlDbType.
        /// </summary>
        /// <returns></returns>
        public override string GetSqlDbTypeString()
        {
            return SqlDbTypeString;
        }
    }
}