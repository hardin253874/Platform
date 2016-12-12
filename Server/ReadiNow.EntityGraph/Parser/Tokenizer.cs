// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ReadiNow.EntityGraph.Parser
{
	/// <summary>
	///     Produces a stream of tokens for the RequestParser.
	/// </summary>
	/// <remarks>
	///     Tokenizer is initially at start position. Call MoveNext() before reading first token.
	///     Call 'Current' to access current token, or MoveNext() to advance.
	///     Does not currently support rewind.
	/// </remarks>
	internal class Tokenizer
	{
		/// <summary>
		///     Valid single-character punctuation characters  (note: colon is part of identifier)
		/// </summary>
		private const string PuncList = "{}.*,#-?@=";

		/// <summary>
		///     Buffer for building identifiers
		/// </summary>
		private readonly StringBuilder _buffer = new StringBuilder( );

		/// <summary>
		///     The input text being tokenized.
		/// </summary>
		private readonly string _text;

		/// <summary>
		///     Current line number in input text
		/// </summary>
		private int _line = 1;

		/// <summary>
		///     Current character position within current line
		/// </summary>
		private int _linePos = 1;

		/// <summary>
		///     Current character position in input text
		/// </summary>
		private int _pos;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="text">Input text to be tokenized.</param>
		public Tokenizer( string text )
		{
			if ( string.IsNullOrEmpty( text ) )
			{
				throw new ArgumentNullException( "text" );
			}

			_text = text;
			Current = new Token( TokenType.AtStart, null );
		}

		/// <summary>
		///     The current token.
		/// </summary>
		public Token Current
		{
			get;
            private set;
		}

		/// <summary>
		///     Advance to the next token.
		/// </summary>
		public void MoveNext( )
		{
			if ( Current.Type == TokenType.AtEnd )
			{
				throw new InvalidOperationException( "Cannot move past end." );
			}

			var state = State.Ready;

			while ( state != State.Done )
			{
				// Get character
				char ch = _pos >= _text.Length ? '\0' : _text[ _pos ];

				switch ( state )
				{
					case State.Ready:
						if ( _pos >= _text.Length )
						{
							Current = new Token( TokenType.AtEnd, null );
							return;
						}
						if ( PuncList.Contains( ch ) )
						{
							Current = new Token( TokenType.Punc, ch.ToString( CultureInfo.InvariantCulture ) );
							state = State.Done;
						}
						else if ( ch == '/' && _text[ _pos + 1 ] == '/' )
						{
							state = State.SingleComment;
						}
						else if ( ch == '/' && _text[ _pos + 1 ] == '*' )
						{
							state = State.MultiComment;
						}
						else if ( char.IsLetterOrDigit( ch ) || ch == ':' )
						{
							_buffer.Clear( );
							_buffer.Append( ch );
							state = State.Identifier;
						}
						else if ( char.IsWhiteSpace( ch ) )
						{
							if ( ch == '\n' )
							{
								_linePos = 0;
								_line++;
							}
						}
						else
						{
							RaiseError( "Unexpected character " + ch );
						}
						_pos++;
						_linePos++;
						break;

					case State.SingleComment:
						if ( ch == '\n' )
						{
							state = State.Ready;
							_linePos = 0;
							_line++;
						}
						_pos++;
						_linePos++;
						break;

					case State.MultiComment:
						if ( ch == '*' && _text[ _pos + 1 ] == '/' )
						{
							_pos++;
							_linePos++;
							state = State.Ready;
						}
						_pos++;
						_linePos++;
						break;

					case State.Identifier:
						if ( char.IsLetterOrDigit( ch ) || ch == ':' )
						{
							_buffer.Append( ch );
							_pos++;
							_linePos++;
						}
						else
						{
							Current = new Token( TokenType.Identifier, _buffer.ToString( ) );
							state = State.Done;
						}
						break;
				}
			}
		}

		/// <summary>
		///     Call to throw an exception that includes the current line number and character position.
		/// </summary>
		/// <param name="message">Message to appear within the exception.</param>
		internal void RaiseError( string message )
		{
		    throw new TokenizerException( message + string.Format( " at {0} line {1}.\nQuery:\n", _linePos, _line ) );
		}


	}
}