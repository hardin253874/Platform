// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Extensions to the System.String class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		///     Removes the spaces.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>The source string with spaces removed.</returns>
		/// <remarks>
		///     For example:
		///     "Test String" -> "TestString"
		/// </remarks>
		public static string RemoveSpaces( this string source )
		{
			if ( string.IsNullOrEmpty( source ) )
			{
				return source;
			}

			return source.Replace( " ", string.Empty );
		}

		/// <summary>
		///     Converts the source string into camel case.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>The source string in camel case.</returns>
		/// <remarks>
		///     For example:
		///     TestString -> testString
		/// </remarks>
		[SuppressMessage( "Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase" )]
		public static string ToCamelCase( this string source )
		{
			if ( string.IsNullOrEmpty( source ) )
			{
				return source;
			}

			return source.Substring( 0, 1 ).ToLower( CultureInfo.InvariantCulture ) + source.Substring( 1 );
		}

		/// <summary>
		///     Converts the source string into Pascal case.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>The source string in Pascal case.</returns>
		/// <remarks>
		///     For example:
		///     testString -> TestString
		/// </remarks>
		public static string ToPascalCase( this string source )
		{
			if ( string.IsNullOrEmpty( source ) )
			{
				return source;
			}

			return source.Substring( 0, 1 ).ToUpper( CultureInfo.InvariantCulture ) + source.Substring( 1 );
		}


        /// <summary>
        /// Normalizes a string into the preferred storage format.
        /// </summary>
        /// <remarks>
        /// Converts empty strings to null.
        /// Converts \r to \n, converts \r\n to \n.
        /// </remarks>
        public static string NormalizeForDatabase( this string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return null;
            }

            string normalized = NewlineRegex.Value.Replace( value, "\n" );
            return normalized;
        }

        private static Lazy<Regex> NewlineRegex = new Lazy<Regex>(
            ( ) => new Regex( "\r\n|\n\r|\r" ),
            System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );



        /// <summary>
        /// Normalizes a string for a database field that only accepts a single line.
        /// Joins multiple lines together.
        /// </summary>
        public static string NormalizeForSingleLine( string value )
        {
            string normalized = NormalizeForDatabase( value );

            if ( normalized == null )
                return null;

            string singleLine = StringHelpers.ToSingleLine( normalized );
            return singleLine;
        }

		/// <summary>
		/// Splits the specified separator.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="separator">The separator.</param>
		/// <param name="options">The options.</param>
		/// <returns></returns>
		public static string [ ] Split( this string value, char separator, StringSplitOptions options )
		{
			return value.Split( new[ ]
			{
				separator
			}, options );
		}
	}
}