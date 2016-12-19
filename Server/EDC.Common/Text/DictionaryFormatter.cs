// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Globalization;

namespace EDC.Text
{
    /// <summary>
    /// This class represents a formatter that can be used to render format strings using a dictionary as a parameter.
    /// </summary>
    public class DictionaryFormatter : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// Returns an object that provides formatting services for the specified type.
        /// </summary>
        /// <param name="formatType">An object that specifies the type of format object to return.</param>
        /// <returns>
        /// An instance of the object specified by <paramref name="formatType" />, if the <see cref="T:System.IFormatProvider" /> implementation can supply that type of object; otherwise, null.
        /// </returns>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }


        /// <summary>
        /// Converts the value of a specified object to an equivalent string representation using specified format and culture-specific formatting information.
        /// </summary>
        /// <param name="format">A format string containing formatting specifications.</param>
        /// <param name="arg">An object to format.</param>
        /// <param name="formatProvider">An object that supplies format information about the current instance.</param>
        /// <returns>
        /// The string representation of the value of <paramref name="arg" />, formatted as specified by <paramref name="format" /> and <paramref name="formatProvider" />.
        /// </returns>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                return string.Empty;
            }

            var dictionary = arg as IDictionary;

            if (dictionary != null)
            {
                if (string.IsNullOrEmpty(format))
                {
                    return string.Empty;
                }

                // The argument is a dictionary

                // Assume first part of format string is
                // the key. Second part is optional format string
                string[] formatParts = format.Split(',');

                string key = formatParts[0];
                string stdFormat = string.Empty;                

                if (formatParts.Length > 1)
                {
                    stdFormat = formatParts[1];
                }                

                // Get the value
                if (!dictionary.Contains(key))
                {
                    return string.Empty;
                }

                object value = dictionary[key];

                if (value == null)                
                {
                    return string.Empty;
                }

                // There is no format string
                if (string.IsNullOrEmpty(stdFormat))
                {
                    return value.ToString();
                }

                // Apply format string
                return string.Format(string.Format(@"{{0:{0}}}", stdFormat), value);
            }

            var formattable = arg as IFormattable;

            if (formattable != null)
            {
                return formattable.ToString(format, CultureInfo.CurrentCulture);
            }

            return arg.ToString();
        }
    }
}
