// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.Core
{
    /// <summary>
    /// Extension methods for <see cref="String"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Truncate the string to <paramref name="maxLength"/> characters long.
        /// Similar to <see cref="String.Substring(int, int)"/>, this handles cases
        /// where the string is shorter than maxLength.
        /// </summary>
        /// <param name="value">
        /// The string to truncate. This cannot be null.
        /// </param>
        /// <param name="maxLength">
        /// The number of characters to truncate the string to. This must be positive.
        /// </param>
        /// <returns>
        /// The first <paramref name="maxLength"/> characters of <paramref name="value"/> or
        /// <paramref name="value"/> if it is shorter than <paramref name="maxLength"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="maxLength"/> must be positive.
        /// </exception>
        public static string Truncate(this string value, int maxLength)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (maxLength < 1)
            {
                throw new ArgumentException("Must be positive", "maxLength");
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        /// Break the string into <paramref name="chunkSize"/> length chunks.
        /// </summary>
        /// <param name="value">
        /// The string to split. This cannot be null.
        /// </param>
        /// <param name="chunkSize">
        /// The number of characters to break <paramref name="value"/> into.
        /// This must be positive.
        /// </param>
        /// <returns>
        /// <paramref name="value"/> broken into chunks.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="chunkSize"/> must be positive.
        /// </exception>
        public static ICollection<string> Chunk(this string value, int chunkSize)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (chunkSize < 1)
            {
                throw new ArgumentException("Must be positive", "chunkSize");
            }

            IList<string> result;

            // Based on http://stackoverflow.com/questions/1450774/splitting-a-string-into-chunks-of-a-certain-size

            result = new List<string>();
            if (value.Length != 0)
            {
                for (int i = 0; i < value.Length; i += chunkSize)
                {
                    if (i + chunkSize <= value.Length)
                    {
                        result.Add(value.Substring(i, chunkSize));
                    }
                    else
                    {
                        result.Add(value.Substring(i, value.Length - i));
                    }
                }
            }
            else
            {
                result.Add(string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Replace instances of keys from <see cref="template"/> in <see cref="str"/> with 
        /// the corresponding values from <see cref="template"/>. Keys can optionally be
        /// decorated using <see cref="keyDecorator"/>.
        /// </summary>
        /// <remarks>
        /// A very simplified version of the lodash "template" function (http://devdocs.io/lodash/index#template).
        /// </remarks>
        /// <param name="str">
        /// The string to expand templates. This cannot be null.
        /// </param>
        /// <param name="template">
        /// The template whose keys are substituted for their values in <see cref="str"/>.
        /// </param>
        /// <param name="keyDecorator">
        /// (Optional) Decorate the key, e.g. add trailing or leading characters.
        /// </param>
        /// <returns>
        /// The substituted <see cref="str"/>.
        /// </returns>
        public static string Template(this string str, IDictionary<string, string> template,
            Func<string, string> keyDecorator = null)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (template == null)
            {
                throw new ArgumentNullException("template");
            }
            if (keyDecorator == null)
            {
                keyDecorator = s => s; // s => "<%=" + s + "%>";
            }

            return template.Aggregate(str, (s, kvp) => s.Replace(keyDecorator(kvp.Key), kvp.Value));
        }
    }
}
