// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace EDC.ReadiNow.Common.ConfigParser.Logging
{
	/// <summary>
	/// Static Event Log class for recording messages into the Windows Application EventLog.
	/// </summary>
	public static class EventLog
	{
		/// <summary>
		/// Writes the information into the Windows Application event log.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		public static void WriteInformation( string message, string source )
		{
			WriteEntry( message, source, EventLogEntryType.Information, null );
		}

		/// <summary>
		/// Writes the information into the Windows Application event log.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		/// <param name="args">The arguments.</param>
		public static void WriteInformation( string message, string source, params object [ ] args )
		{
			WriteEntry( message, source, EventLogEntryType.Information, args );
		}

		/// <summary>
		/// Writes the warning into the Windows Application event log.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		public static void WriteWarning( string message, string source )
		{
			WriteEntry( message, source, EventLogEntryType.Warning, null );
		}

		/// <summary>
		/// Writes the warning into the Windows Application event log.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		/// <param name="args">The arguments.</param>
		public static void WriteWarning( string message, string source, params object [ ] args )
		{
			WriteEntry( message, source, EventLogEntryType.Warning, args );
		}

		/// <summary>
		/// Writes the error into the Windows Application event log.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		public static void WriteError( string message, string source )
		{
			WriteError( message, source, null );
		}

		/// <summary>
		/// Writes the error into the Windows Application event log.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		/// <param name="args">The message arguments.</param>
		public static void WriteError( string message, string source, params object[ ] args )
		{
			WriteEntry( message, source, EventLogEntryType.Error, args );
		}

		/// <summary>
		/// Writes the error into the Windows Application event log.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		/// <param name="type">The type.</param>
		/// <param name="args">The message arguments.</param>
		private static void WriteEntry( string message, string source, EventLogEntryType type, params object [ ] args )
		{
			if ( string.IsNullOrEmpty( message ) )
			{
				return;
			}

			if ( string.IsNullOrEmpty( source ) )
			{
				throw new ArgumentNullException( "source" );
			}

			/////
			// Use the 'Application' log.
			/////
			string log = "Application";

			if ( !System.Diagnostics.EventLog.SourceExists( source ) )
			{
				System.Diagnostics.EventLog.CreateEventSource( source, log );
			}

			if ( args == null )
			{
				System.Diagnostics.EventLog.WriteEntry( source, message, type );
			}
			else
			{
				System.Diagnostics.EventLog.WriteEntry( source, string.Format( CultureInfo.CurrentCulture, message, args ), type );
			}
		}
	}
}
