// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace PlatformConfigure
{
	/// <summary>
	///     The console logger class.
	/// </summary>
	internal static class ConsoleLogger
	{
		/// <summary>
		///     Writes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		internal static void Write( string value, ConsoleColor color = ConsoleColor.Gray )
		{
			Write( value, color, null );
		}

		/// <summary>
		///     Writes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="args">The arguments.</param>
		internal static void Write( string value, params object[ ] args )
		{
			Write( value, ConsoleColor.Gray, args );
		}

		/// <summary>
		///     Writes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <param name="args">The arguments.</param>
		internal static void Write( string value, ConsoleColor color = ConsoleColor.Gray, params object[ ] args )
		{
			Console.ForegroundColor = color;

			if ( value != null )
			{
				if ( args == null )
				{
					Console.Write( value );
				}
				else
				{
					Console.Write( value, args );
				}
			}

			Console.ResetColor( );
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="value">The value.</param>
		internal static void WriteError( string value )
		{
			WriteError( value, null );
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="args">The arguments.</param>
		internal static void WriteError( string value, params object[ ] args )
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;

			if ( value == null )
			{
				Console.Error.WriteLine( );
			}
			else
			{
				if ( args == null )
				{
					Console.Error.WriteLine( value );
				}
				else
				{
					Console.Error.WriteLine( value, args );
				}
			}

			Console.ResetColor( );
		}

		/// <summary>
		///     Writes the heading to console out.
		/// </summary>
		/// <param name="heading">The heading.</param>
		/// <param name="color">The color.</param>
		internal static void WriteHeading( string heading, ConsoleColor color = ConsoleColor.Gray )
		{
			if ( string.IsNullOrEmpty( heading ) )
			{
				return;
			}

			Console.ForegroundColor = color;
			Console.WriteLine( );
			Console.WriteLine( heading );
			Console.WriteLine( new string( '-', heading.Length ) );
			Console.ResetColor( );
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		internal static void WriteLine( )
		{
			WriteLine( null, ConsoleColor.Gray, null );
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="args">The arguments.</param>
		internal static void WriteLine( string value, params object[ ] args )
		{
			WriteLine( value, ConsoleColor.Gray, args );
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		internal static void WriteLine( string value, ConsoleColor color = ConsoleColor.Gray )
		{
			WriteLine( value, color, null );
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <param name="args">The arguments.</param>
		internal static void WriteLine( string value, ConsoleColor color = ConsoleColor.Gray, params object[ ] args )
		{
			Console.ForegroundColor = color;

			if ( value == null )
			{
				Console.WriteLine( );
			}
			else
			{
				if ( args == null )
				{
					Console.WriteLine( value );
				}
				else
				{
					Console.WriteLine( value, args );
				}
			}

			Console.ResetColor( );
		}
	}
}