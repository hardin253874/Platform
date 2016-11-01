// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Text utility.
	/// </summary>
	public static class TextUtil
	{
		/// <summary>
		///     Normalizes line endings, converting "\r" into "\r\n" and "\n" into "\r\n".
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="ctx">The CTX.</param>
		/// <returns></returns>
		public static TextReader Normalize( string s, ISerializationContext ctx )
		{
			// Replace \r and \n with \r\n.
			s = Regex.Replace( s, @"((\r(?=[^\n]))|((?<=[^\r])\n))", "\r\n" );

			var settings = ctx.GetService( typeof ( ISerializationSettings ) ) as ISerializationSettings;
			if ( settings == null || !settings.EnsureAccurateLineNumbers )
			{
				s = RemoveEmptyLines( UnwrapLines( s ) );
			}

			return new StringReader( s );
		}

		/// <summary>
		///     Normalizes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <param name="ctx">The CTX.</param>
		/// <returns></returns>
		public static TextReader Normalize( TextReader tr, ISerializationContext ctx )
		{
			string s = tr.ReadToEnd( );
			TextReader reader = Normalize( s, ctx );
			tr.Close( );

			return reader;
		}

		/// <summary>
		///     Removes blank lines from a string with normalized (\r\n)
		///     line endings.
		///     NOTE: this method makes the line/column numbers output from
		///     antlr incorrect.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string RemoveEmptyLines( string s )
		{
			int len = -1;
			while ( len != s.Length )
			{
				s = s.Replace( "\r\n\r\n", "\r\n" );
				len = s.Length;
			}
			return s;
		}

		/// <summary>
		///     Unwraps lines from the RFC 2445 "line folding" technique.
		///     NOTE: this method makes the line/column numbers output from
		///     antlr incorrect.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string UnwrapLines( string s )
		{
			return Regex.Replace( s, @"(\r\n[ \t])", string.Empty );
		}

		/// <summary>
		///     Unwraps the lines.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public static TextReader UnwrapLines( TextReader tr )
		{
			return new StringReader( UnwrapLines( tr.ReadToEnd( ) ) );
		}

		/// <summary>
		///     Wraps the lines.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string WrapLines( string value )
		{
			// NOTE: Made this method more efficient by removing
			// the use of strings, and only using StringBuilders.
			// Also, the "while" loop was removed, and StringBuilder
			// modifications are kept at a minimum.
			var result = new StringBuilder( );
			var current = new StringBuilder( value );

			// Wrap lines at 75 characters, per RFC 2445 "folding" technique
			int i = 0;
			if ( current.Length > 75 )
			{
				result.Append( current.ToString( 0, 75 ) + "\r\n " );
				for ( i = 75; i < current.Length - 74; i += 74 )
				{
					result.Append( current.ToString( i, 74 ) + "\r\n " );
				}
			}
			result.Append( current.ToString( i, current.Length - i ) );
			result.Append( "\r\n" );

			return result.ToString( );
		}
	}
}