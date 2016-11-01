// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Processing context.
	/// </summary>
	public class ProcessingContext : IProcessingContext
	{
		/// <summary>
		/// Whether the last message was a progress message.
		/// </summary>
		private bool _lastMessageWasProgress;

		/// <summary>
		///     Initializes a new instance of the <see cref="ProcessingContext" /> class.
		/// </summary>
		public ProcessingContext( )
		{
			Report = new StatisticsReport( WriteError, WriteWarning, WriteInfo );
		}

		/// <summary>
		///     Writes the info.
		/// </summary>
		/// <param name="message">The message.</param>
		public void WriteInfo( string message )
		{
			if ( _lastMessageWasProgress )
			{
				Console.WriteLine( );
				_lastMessageWasProgress = false;
			}

			if ( !Console.IsOutputRedirected )
			{
				Console.CursorLeft = 0;
			}

			Console.WriteLine( message );
		}

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		public void WriteWarning( string message )
		{
			if ( _lastMessageWasProgress )
			{
				Console.WriteLine( );
				_lastMessageWasProgress = false;
			}

			if ( !Console.IsOutputRedirected )
			{
				Console.CursorLeft = 0;
			}

			Console.WriteLine( @"WARNING: " + message );
		}

		/// <summary>
		///     Gets the report.
		/// </summary>
		/// <value>
		///     The report.
		/// </value>
		public StatisticsReport Report
		{
			get;
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		public void WriteError( string message )
		{
			if ( _lastMessageWasProgress )
			{
				Console.WriteLine( );
				_lastMessageWasProgress = false;
			}

			if ( !Console.IsOutputRedirected )
			{
				Console.CursorLeft = 0;
			}

			Console.WriteLine( @"ERROR: " + message );
		}

		/// <summary>
		///     Writes the progress.
		/// </summary>
		/// <param name="message">The message.</param>
		public void WriteProgress( string message )
		{
			if ( !Console.IsOutputRedirected )
			{
				Console.CursorLeft = 0;
				Console.Write( message.PadRight( 79, ' ' ) );
			}
			else
			{
				Console.WriteLine( message );
			}

			_lastMessageWasProgress = true;
		}
	}
}