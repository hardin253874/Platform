// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace EDC.Database
{
	/// <summary>
	///     Class for dynamically constructing indented SQL code.
	/// </summary>
	/// <remarks>
	///     The design of this class is to generally add new-line characters at the start of each new line, rather than at the
	///     conclusion of each line. This is a more natural approach for producing T-SQL, where additional content is often later
	///     appended to a line.
	/// </remarks>
	public class SqlBuilder
	{
		private readonly TextWriter _writer;
		private bool _firstLine = true;
		private int _indent;
		private string _indentText = string.Empty;

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlBuilder" /> class with a <see cref="StringWriter" />.
		/// </summary>
		public SqlBuilder( )
		{
			_writer = new StringWriter( );
		}


		/// <summary>
		///     Gets the current indent
		/// </summary>
		public int CurrentIndent
		{
			get
			{
				return _indent;
			}
		}


		/// <summary>
		///     Appends the specified content.
		/// </summary>
		/// <param name="content">The content to write.</param>
		public void Append( string content )
		{
			_firstLine = false;
			_writer.Write( content );
		}


		/// <summary>
		///     Appends the specified content.
		/// </summary>
		/// <param name="format">A formatting string of content to write.</param>
		/// <param name="arguments">Arguments for string formatting.</param>
		// ReSharper disable MethodOverloadWithOptionalParameter
		public void Append( string format, params object[] arguments )
		{
			_firstLine = false;
			_writer.Write( format, arguments );
		}

		// ReSharper restore MethodOverloadWithOptionalParameter

		/// <summary>
		///     Terminates the current line. Then writes the content on a new intended line (but does not terminate it).
		/// </summary>
		/// <param name="content">The content to write.</param>
		public void AppendOnNewLine( string content )
		{
			if ( !_firstLine )
			{
				WriteNewLine( );
			}
			_firstLine = false;

			_writer.Write( content );
		}

		/// <summary>
		///     Terminates the current line. Then writes the content on a new intended line (but does not terminate it).
		/// </summary>
		/// <param name="format">A formatting string of content to write.</param>
		/// <param name="arguments">Arguments for string formatting.</param>
		// ReSharper disable MethodOverloadWithOptionalParameter
		public void AppendOnNewLine( string format, params object[] arguments )
		{
			if ( !_firstLine )
			{
				WriteNewLine( );
			}
			_firstLine = false;

			_writer.Write( format, arguments );
		}

		// ReSharper restore MethodOverloadWithOptionalParameter

		/// <summary>
		///     Formats a date literal.
		/// </summary>
		public static string DateLiteral( DateTime date )
		{
			return string.Format( "'{0:yyyy-mm-dd}'", date );
		}


		/// <summary>
		///     Removes one layer of indenting.
		/// </summary>
		public void EndIndent( )
		{
			--_indent;
			if ( _indent < 0 )
			{
				_indent = 0; // assert false
			}
			_indentText = new string( ' ', _indent * 4 );
		}


		/// <summary>
		///     Creates a SQL block comment from some text.
		///     Adds the begin and end comment characters.
		///     Disables any character combinations in the comment that would prematurely break out of the comment.
		/// </summary>
		/// <param name="commentText">The text, without comment characters.</param>
		/// <returns>Escaped comment, with block comment characters.</returns>
		public static string EscapeBlockComment( string commentText )
		{
			if ( commentText == null )
			{
				return string.Empty;
			}

			var sb = new StringBuilder( );
			sb.Append( "/*" );
			char prev = '*';
			foreach ( char ch in commentText )
			{
				if ( prev == '*' && ch == '/' || prev == '/' && ch == '*' )
				{
					sb.Append( ' ' );
				}
				sb.Append( ch );
				prev = ch;
			}
			sb.Append( "*/" );
			return sb.ToString( );
		}

		/// <summary>
		///     Escapes a SQL identifier.
		/// </summary>
		/// <param name="name">The identifier name.</param>
		/// <returns>An escaped version, including square brackets if necessary.</returns>
		public static string EscapeSqlIdentifier( string name )
		{
            if ( string.IsNullOrEmpty( name ) )
                throw new ArgumentNullException( nameof( name ) );

            if (name.StartsWith("dbo."))
                return "[dbo].[" + name.Substring(4).Replace( "]", "]]" ) + "]";
            else
    			return "[" + name.Replace( "]", "]]" ) + "]";
		}

		/// <summary>
		///     Embeds T-SQL escape sequences so that a value can be used as a string literal.
		/// </summary>
		/// <param name="value">The string to be escaped.</param>
		/// <returns>An escaped string. Does not include surrounding quotes.</returns>
		public static string EscapeStringLiteral( string value )
		{
			return value.Replace( "'", "''" );
		}

        /// <summary>
        ///     Allows a limited set of characters that can be used in a comment.
        /// </summary>
        /// <param name="value">The string to be validated.</param>
        /// <returns>A sanitized string.</returns>
        public static string SanitizeForComment(string value)
        {
            if (value == null)
                return null;

            StringBuilder sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (char.IsLetterOrDigit(ch) || "-, .".IndexOf(ch) >= 0)
                    sb.Append(ch);
            }
            return sb.ToString();
        }

		/// <summary>
		///     Indents subsequent lines.
		/// </summary>
		public void Indent( )
		{
            if (_indent > 50)
                throw new InvalidOperationException("Nesting is too deep.");

			++_indent;
			_indentText = new string( ' ', _indent * 4 );
		}

		/// <summary>
		///     Terminates the current line.
		/// </summary>
		public void StartNewLine( )
		{
			if ( !_firstLine )
			{
				WriteNewLine( );
			}
			_firstLine = false;
		}

		/// <summary>
		///     Returns a string representation of the generated SQL.
		/// </summary>
		/// <remarks>
		///     This calls <see cref="TextWriter.ToString" /> on the underlying text writer.
		///     If a custom TextWriter has been provided then this may not yield the expected results.
		/// </remarks>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return _writer.ToString( );
		}

		/// <summary>
		///     Renders new-line characters and indentation for the following line.
		/// </summary>
		private void WriteNewLine( )
		{
			_writer.WriteLine( );
			_writer.Write( _indentText );
		}




        /// <summary>
        /// Builds a 'like' clause, handling any escape sequences, for use as a SqlParameter
        /// </summary>
        /// <param name="searchFor">The term to search for.</param>
        /// <param name="prefix">Any character to prefix unescaped (e.g. %)</param>
        /// <param name="suffix">Any character to prefix suffix (e.g. %)</param>
        /// <returns>prefix/searchFor/suffix</returns>
        public static string BuildSafeLikeParameter( string searchFor, string prefix, string suffix )
        {
            if ( searchFor == null )
                throw new ArgumentNullException( nameof( searchFor ) );

            string likeEscaped = searchFor;

            var blacklist = new [ ]
				{
					'[', '_', '%'
				}; // apostrophe handled specially
            if ( likeEscaped.IndexOfAny( blacklist ) != -1 )
            {
                likeEscaped = blacklist.Aggregate( likeEscaped, ( current, ch ) => current.Replace( ch.ToString( CultureInfo.InvariantCulture ), "[" + ch + "]" ) );
            }

            string result = string.Concat( prefix, likeEscaped, suffix );
            return result;
        }


        /// <summary>
        /// Builds a 'like' clause, handling any escape sequences, for use being injected into a SQL statement.
        /// </summary>
        /// <param name="searchFor">The term to search for.</param>
        /// <param name="prefix">Any character to prefix unescaped (e.g. %)</param>
        /// <param name="suffix">Any character to prefix suffix (e.g. %)</param>
        /// <returns>like 'prefix/searchFor/suffix' escape '_'</returns>
        public static string BuildSafeLikeStatement( string searchFor, string prefix, string suffix )
        {
            string param = BuildSafeLikeParameter( searchFor, prefix, suffix );
            string result = string.Concat( " like '", EscapeStringLiteral( param ), "'" );
            return result;
        }
	}
}