// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;

namespace ReadiNow.Connector.Payload
{
    /// <summary>
    /// Functions for parsing values out of JSON dates.
    /// </summary>
    internal static class JsonValueParser
    {
        /// <summary>
        /// Parse a date.
        /// </summary>
        /// <param name="value">The text</param>
        /// <param name="result">True if successful, otherwise false.</param>
        /// <returns>The date</returns>
        public static bool TryParseDate(string value, out DateTime result)
        {
            if (value.Length == 5)
                value = value + ":00";

            if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
                return true;
            return false;
        }

        /// <summary>
        /// Parse a time.
        /// </summary>
        /// <param name="value">The text</param>
        /// <param name="result">True if successful, otherwise false.</param>
        /// <returns>The time</returns>
        public static bool TryParseTime(string value, out DateTime result)
        {
            if (value.Length == 5)
                value = value + ":00";

            if (DateTime.TryParseExact("1753-01-01T" + value + ".0000000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
                return true;
            return false;
        }

        /// <summary>
        /// Parse a date-time.
        /// </summary>
        /// <param name="value">The text</param>
        /// <param name="result">True if successful, otherwise false.</param>
        /// <returns>The date-time</returns>
        public static bool TryParseDateTime(string value, out DateTime result)
        {
            if (value.EndsWith("Z"))
                value = value.Substring(0, value.Length - 1);
            if (value.Length == 10)
                value = value + "T00:00:00.0000000Z";
            else if (value.Length == 16)
                value = value + ":00.0000000Z";
            else if (value.Length == 19)
                value = value + ".0000000Z";

            if (DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
                return true;
            return false;
        }
    }
}
