// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ReadiMon.Shared.Controls
{
	/// <summary>
	///     Avalon Edit extensions.
	/// </summary>
	public static class AvalonEditExtensions
	{
		/// <summary>
		///     Adds the custom highlighting.
		/// </summary>
		/// <param name="textEditor">The text editor.</param>
		/// <param name="xshdStream">The XSHD stream.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Could not find embedded resource</exception>
		public static IHighlightingDefinition AddCustomHighlighting( this TextEditor textEditor, Stream xshdStream )
		{
			if ( xshdStream == null )
			{
				throw new InvalidOperationException( "Could not find embedded resource" );
			}

			IHighlightingDefinition customHighlighting;

			/////
			// Load our custom highlighting definition
			/////
			using ( XmlReader reader = new XmlTextReader( xshdStream ) )
			{
				customHighlighting = HighlightingLoader.Load( reader, HighlightingManager.Instance );
			}

			/////
			// And register it in the HighlightingManager
			/////
			HighlightingManager.Instance.RegisterHighlighting( "Custom Highlighting", null, customHighlighting );

			return customHighlighting;
		}

		/// <summary>
		///     Adds the custom highlighting.
		/// </summary>
		/// <param name="textEditor">The text editor.</param>
		/// <param name="xshdStream">The XSHD stream.</param>
		/// <param name="extensions">The extensions.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Could not find embedded resource</exception>
		public static IHighlightingDefinition AddCustomHighlighting( this TextEditor textEditor, Stream xshdStream, string[ ] extensions )
		{
			if ( xshdStream == null )
			{
				throw new InvalidOperationException( "Could not find embedded resource" );
			}

			IHighlightingDefinition customHighlighting;

			// Load our custom highlighting definition
			using ( XmlReader reader = new XmlTextReader( xshdStream ) )
			{
				customHighlighting = HighlightingLoader.Load( reader, HighlightingManager.Instance );
			}

			// And register it in the HighlightingManager
			HighlightingManager.Instance.RegisterHighlighting( "Custom Highlighting", extensions, customHighlighting );

			return customHighlighting;
		}

		/// <summary>
		///     Gets the word before dot.
		/// </summary>
		/// <param name="textEditor">The text editor.</param>
		/// <returns></returns>
		public static string GetWordBeforeDot( this TextEditor textEditor )
		{
			var wordBeforeDot = string.Empty;

			var caretPosition = textEditor.CaretOffset - 2;

			var lineOffset = textEditor.Document.GetOffset( textEditor.Document.GetLocation( caretPosition ) );

			string text = textEditor.Document.GetText( lineOffset, 1 );

			/////
			// Get text backward of the mouse position, until the first space
			/////
			while ( !string.IsNullOrWhiteSpace( text ) && String.Compare( text, ".", StringComparison.Ordinal ) > 0 )
			{
				wordBeforeDot = text + wordBeforeDot;

				if ( caretPosition == 0 )
					break;

				lineOffset = textEditor.Document.GetOffset( textEditor.Document.GetLocation( --caretPosition ) );

				text = textEditor.Document.GetText( lineOffset, 1 );
			}

			return wordBeforeDot;
		}

		/// <summary>
		///     Gets the word under mouse.
		/// </summary>
		/// <param name="document">The document.</param>
		/// <param name="position">The position.</param>
		/// <returns></returns>
		public static string GetWordUnderMouse( this TextDocument document, TextViewPosition position )
		{
			string wordHovered = string.Empty;

			var line = position.Line;
			var column = position.Column;

			var offset = document.GetOffset( line, column );
			if ( offset >= document.TextLength )
				offset--;

			if ( offset < 0 )
			{
				return string.Empty;
			}

			var textAtOffset = document.GetText( offset, 1 );

			/////
			// Get text backward of the mouse position, until the first space
			/////
			while ( !textAtOffset.IsWordBreakCharacter( ) )
			{
				wordHovered = textAtOffset + wordHovered;

				offset--;

				if ( offset < 0 )
					break;

				textAtOffset = document.GetText( offset, 1 );
			}

			/////
			// Get text forward the mouse position, until the first space
			/////
			offset = document.GetOffset( line, column );
			if ( offset < document.TextLength - 1 )
			{
				offset++;

				textAtOffset = document.GetText( offset, 1 );

				while ( !textAtOffset.IsWordBreakCharacter( ) )
				{
					wordHovered = wordHovered + textAtOffset;

					offset++;

					if ( offset >= document.TextLength )
						break;

					textAtOffset = document.GetText( offset, 1 );
				}
			}

			return wordHovered;
		}

		/// <summary>
		///     Determines whether [is word break character].
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		private static bool IsWordBreakCharacter( this string input )
		{
			if ( string.IsNullOrEmpty( input ) )
			{
				return true;
			}

			var regEx = new Regex( "([^\\w-]|_)+" );

			return regEx.IsMatch( input );
		}
	}
}