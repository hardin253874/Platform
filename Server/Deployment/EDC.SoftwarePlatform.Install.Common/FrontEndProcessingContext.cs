// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     The front end processing context class.
	/// </summary>
	/// <seealso cref="IProcessingContext" />
	public class FrontEndProcessingContext : IProcessingContext
	{
		/// <summary>
		///     The buffer
		/// </summary>
		private readonly List<string> _buffer = new List<string>( );

		/// <summary>
		///     The buffer lines
		/// </summary>
		private readonly List<string> _bufferLines = new List<string>
		{
			"Initializing...",
			"Copying metadata..."
		};

		/// <summary>
		///     The log callback
		/// </summary>
		private readonly Action<string> _logCallback;

		/// <summary>
		///     The buffered flag.
		/// </summary>
		private bool _buffered;

		/// <summary>
		///     Initializes a new instance of the <see cref="FrontEndProcessingContext" /> class.
		/// </summary>
		/// <param name="logCallback">The log callback.</param>
		public FrontEndProcessingContext( Action<string> logCallback )
		{
			if ( logCallback == null )
			{
				throw new ArgumentNullException( nameof( logCallback ) );
			}

			_logCallback = logCallback;

			Report = new StatisticsReport( );
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
		/// <exception cref="NotImplementedException"></exception>
		public void WriteError( string message )
		{
			_logCallback( $"ERROR: {message}" );
		}

		/// <summary>
		///     Writes the info.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="NotImplementedException"></exception>
		public void WriteInfo( string message )
		{
			if ( _bufferLines.Contains( message ) )
			{
				_buffer.Add( message );
				_buffered = true;
				return;
			}

			if ( _buffered )
			{
				foreach ( string line in _buffer )
				{
					_logCallback( line );
				}

				_buffer.Clear( );
				_buffered = false;
			}

			_logCallback( message );
		}

		/// <summary>
		///     Writes the progress.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteProgress( string message )
		{
		}

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteWarning( string message )
		{
			_logCallback( $"WARNING: {message}" );
		}
	}
}