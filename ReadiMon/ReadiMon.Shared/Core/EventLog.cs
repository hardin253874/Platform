// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Event log.
	/// </summary>
	public class EventLog : IEventLog
	{
		/// <summary>
		///     The logger.
		/// </summary>
		private static readonly Lazy<EventLog> LoggerInstance = new Lazy<EventLog>( ( ) => new EventLog( ), false );

		/// <summary>
		///     Prevents a default instance of the <see cref="EventLog" /> class from being created.
		/// </summary>
		private EventLog( )
		{
			try
			{
				string path = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );

				if ( path != null )
				{
					LogPath = Path.Combine( path, "ReadiMon.log" );
				}
				else
				{
					throw new InvalidDataException( "Unable to determine the current directory." );
				}
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc );
			}
		}

		/// <summary>
		/// Gets or sets the path.
		/// </summary>
		/// <value>
		/// The path.
		/// </value>
		private string LogPath
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		/// <value>
		///     The instance.
		/// </value>
		public static EventLog Instance
		{
			get
			{
				return LoggerInstance.Value;
			}
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteError( string message, params object[ ] args )
		{
			Write( EventType.Error, message, args );
		}

		/// <summary>
		///     Logs the message.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public void WriteException( Exception exception )
		{
			WriteError( exception.ToString( ) );
		}

		/// <summary>
		///     Writes the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteException( Exception exception, string message, params object[ ] args )
		{
			WriteError( message + "\r\n" + exception, args );
		}

		/// <summary>
		///     Logs the message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteInformation( string message, params object[ ] args )
		{
			Write( EventType.Information, message, args );
		}

		/// <summary>
		///     Writes the trace.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteTrace( string message, params object[ ] args )
		{
			Write( EventType.Trace, message, args );
		}

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteWarning( string message, params object[ ] args )
		{
			Write( EventType.Warning, message, args );
		}

		/// <summary>
		///     Writes the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		private void Write( EventType type, string message, params object[ ] args )
		{
			if ( args != null && args.Length > 0 )
			{
				message = string.Format( message, args );
			}

			string typeString = type.ToString( ).PadRight( 15, ' ' );

			string parsedMessage = string.Format( "{0}\t{1}\t{2}\r\n", DateTime.Now, typeString, message );

			byte[ ] bytes = Encoding.UTF8.GetBytes( parsedMessage );

			int retryCount = 0;

			while ( retryCount < 10 )
			{
				try
				{
					using ( var fileStream = File.Open( LogPath, FileMode.Append, FileAccess.Write, FileShare.Read ) )
					{
						fileStream.Write( bytes, 0, bytes.Length );
						fileStream.Flush( );
					}

					break;
				}
				catch ( Exception )
				{
					Thread.Sleep( 500 );
					retryCount++;
				}
			}
		}
	}
}