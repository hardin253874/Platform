// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Message Helper class.
	/// </summary>
	public static class MessageHelper
	{
		/// <summary>
		///     Combines the string.
		/// </summary>
		/// <param name="separator">The separator.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public static string CombineString( char separator, params string[ ] values )
		{
			if ( values == null )
			{
				return null;
			}

			return string.Join( separator.ToString( CultureInfo.InvariantCulture ), values.Select( i => ToUnicode( i, separator ) ).ToArray( ) );
		}

		/// <summary>
		///     Replace the specified Unicode character with the ASCII representation.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="characters">The characters.</param>
		/// <returns></returns>
		public static string FromUnicode( string input, params char[ ] characters )
		{
			if ( string.IsNullOrEmpty( input ) )
			{
				return input;
			}

			return characters.Aggregate( input, ( current, c ) => current.Replace( string.Format( @"\u{0:x4}", ( int ) c ), c.ToString( CultureInfo.InvariantCulture ) ) );
		}

		/// <summary>
		///     Splits the string.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="separator">The separator.</param>
		/// <returns></returns>
		public static List<string> SplitString( string input, char separator )
		{
			if ( string.IsNullOrEmpty( input ) )
			{
				return new List<string>( );
			}

			return input.Split( separator ).Select( i => FromUnicode( i, separator ) ).ToList( );
		}

		/// <summary>
		///     Replaces the specified character with the equivalent Unicode character.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="characters">The characters.</param>
		/// <returns></returns>
		public static string ToUnicode( string input, params char[ ] characters )
		{
			if ( string.IsNullOrEmpty( input ) )
			{
				return input;
			}

			return characters.Aggregate( input, ( current, c ) => current.Replace( c.ToString( CultureInfo.InvariantCulture ), string.Format( @"\u{0:x4}", ( int ) c ) ) );
		}
	}
}