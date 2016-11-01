// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Threading;
using EDC.ReadiNow.Diagnostics;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     The file logger class.
	/// </summary>
	/// <seealso cref="IDisposable" />
	internal class FileLogger : IDisposable
	{
		/// <summary>
		///     The _stream writer
		/// </summary>
		private readonly StreamWriter _streamWriter;

		/// <summary>
		///     The disposed flag.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="FileLogger" /> class.
		/// </summary>
		/// <param name="logFilePath">The log file path.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public FileLogger( string logFilePath )
		{
			if ( string.IsNullOrEmpty( logFilePath ) )
			{
				throw new ArgumentNullException( nameof( logFilePath ) );
			}

			_streamWriter = new StreamWriter( logFilePath, true )
			{
				AutoFlush = true
			};
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( !_disposed )
			{
				_streamWriter?.Flush( );
				_streamWriter?.Close( );
				_streamWriter?.Dispose( );
				_disposed = true;

				/////
				// Time for the file to flush
				/////
				Thread.Sleep( 500 );
			}
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		internal void WriteLine( )
		{
			WriteLine( null );
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="severity">The severity.</param>
		internal void WriteLine( string value, Severity severity = Severity.Info )
		{
			WriteLine( value, severity, null );
		}

		/// <summary>
		///     Writes the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="severity">The severity.</param>
		/// <param name="args">The arguments.</param>
		internal void WriteLine( string value, Severity severity = Severity.Info, params object[ ] args )
		{
			if ( value == null )
			{
				_streamWriter?.WriteLine( );
			}
			else
			{
				if ( args == null )
				{
					_streamWriter?.WriteLine( value );
				}
				else
				{
					_streamWriter?.WriteLine( value, args );
				}
			}
		}
	}
}