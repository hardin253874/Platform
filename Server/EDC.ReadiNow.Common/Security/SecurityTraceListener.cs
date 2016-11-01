// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Security trace listener.
	/// </summary>
	public class SecurityTraceListener : TraceListener
	{
		/// <summary>
		///     Security Cache header.
		/// </summary>
		private const string SecurityCacheHeader = "SC:";

		/// <summary>
		///     Default trace listener.
		/// </summary>
		private readonly DefaultTraceListener _defaultListener = new DefaultTraceListener( );

		/// <summary>
		///     Messages are available.
		/// </summary>
		private readonly AutoResetEvent _messageAvailable = new AutoResetEvent( false );

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _messageQueueLock = new object( );

		/// <summary>
		///     Message queue.
		/// </summary>
		private readonly Queue<Tuple<string[], long>> _messages = new Queue<Tuple<string[], long>>( );

		/// <summary>
		///     Name map.
		/// </summary>
		private readonly Dictionary<long, string> _nameMap = new Dictionary<long, string>( );

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _nameMapLock = new object( );

		/// <summary>
		///     Trace level.
		/// </summary>
		private readonly SecurityTraceLevel _traceLevel = SecurityTraceLevel.DenyVerbose;

		/// <summary>
		///     Worker thread.
		/// </summary>
		private readonly Thread _workerThread;

		/// <summary>
		///     Initializes a new instance of the <see cref="SecurityTraceListener" /> class.
		/// </summary>
		/// <param name="traceLevel">The trace level.</param>
		public SecurityTraceListener( SecurityTraceLevel traceLevel )
		{
			_traceLevel = traceLevel;

			/////
			// Remove the default listener and replace it with this one.
			/////
			Trace.Listeners.Add( this );
			Trace.Listeners.Remove( "Default" );

			/////
			// Single threaded to ensure ordered output.
			/////
			_workerThread = new Thread( ProcessMessageQueue )
				{
					IsBackground = true,
					Name = "SecurityTraceListener"
				};

			_workerThread.Start( );
		}

		/// <summary>
		///     When overridden in a derived class, writes the specified message to the listener you create in the derived class.
		/// </summary>
		/// <param name="message">A message to write.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public override void Write( string message )
		{
			ProcessMessage( message, m => _defaultListener.Write( m ) );
		}

		/// <summary>
		///     When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
		/// </summary>
		/// <param name="message">A message to write.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public override void WriteLine( string message )
		{
			ProcessMessage( message, m => _defaultListener.WriteLine( m ) );
		}

		/// <summary>
		/// Gets the map value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private string GetMapValue( string value )
		{
			long id;

			if ( long.TryParse( value, out id ) )
			{
				lock ( _nameMapLock )
				{
					string mapValue;

					if ( _nameMap.TryGetValue( id, out mapValue ) )
					{
						return mapValue;
					}
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// Preloads the strings.
		/// </summary>
		/// <param name="stringIds">The string ids.</param>
		/// <param name="tenantId">The tenant id.</param>
		/// <param name="command">The command.</param>
		/// <param name="parameter">The parameter.</param>
		private void PreloadStrings( IEnumerable<string> stringIds, long tenantId, IDbCommand command, IDbDataParameter parameter )
		{
			var unknownIds = new List<string>( );

			foreach ( string stringId in stringIds )
			{
				long id;

				if ( long.TryParse( stringId, out id ) )
				{
					lock ( _nameMapLock )
					{
						if ( !_nameMap.ContainsKey( id ) )
						{
							unknownIds.Add( stringId );
						}
					}
				}
			}

			/////
			// Load the unknown strings.
			/////
			if ( unknownIds.Count > 0 )
			{
				command.CommandText = string.Format( @"-- SecurityTraceListener
                    SELECT d.EntityId, d.Data
                    FROM dbo.Data_NVarChar d WITH (NOLOCK)
                    LEFT JOIN dbo.Data_Alias a WITH (NOLOCK) ON d.TenantId = a.TenantId AND d.FieldId = a.EntityId
                    WHERE a.Namespace = 'core' AND a.Data = 'name' AND a.TenantId = @tenantId AND d.EntityId IN ({0})", string.Join( ",", unknownIds.ToArray( ) ) );
				parameter.Value = tenantId;

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					lock ( _nameMapLock )
					{
						if ( reader != null )
						{
							/////
							// Place the returned data into a map.
							/////
							while ( reader.Read( ) )
							{
								_nameMap[ reader.GetInt64( 0 ) ] = reader.GetString( 1 );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Processes the message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="defaultAction">The default action.</param>
		private void ProcessMessage( string message, Action<string> defaultAction )
		{
			if ( message.StartsWith( SecurityCacheHeader ) )
			{
				message = message.Replace( SecurityCacheHeader, string.Empty );

				string[] messageComponents = message.Split( '|' );

				switch ( _traceLevel )
				{
					case SecurityTraceLevel.None:
						/////
						// Tracing is turned off.
						/////
						return;

					case SecurityTraceLevel.DenyBasic:
						if ( messageComponents[ 0 ] == "DENIED" )
						{
							defaultAction( message );
						}

						break;

					case SecurityTraceLevel.AllBasic:
						defaultAction( message );

						break;

					case SecurityTraceLevel.DenyVerbose:
						if ( messageComponents[ 0 ] == "DENIED" )
						{
							lock ( _messageQueueLock )
							{
								_messages.Enqueue( new Tuple<string[], long>( messageComponents, RequestContext.TenantId ) );
							}

							/////
							// Raise the messages available event.
							/////
							_messageAvailable.Set( );
						}

						break;

					case SecurityTraceLevel.AllVerbose:

						lock ( _messageQueueLock )
						{
							_messages.Enqueue( new Tuple<string[], long>( messageComponents, RequestContext.TenantId ) );
						}

						/////
						// Raise the messages available event.
						/////
						_messageAvailable.Set( );

						break;
				}
			}
			else
			{
				/////
				// Pass the Write messages through untouched.
				/////
				defaultAction( message );
			}
		}

		/// <summary>
		///     Processes the message queue.
		/// </summary>
		private void ProcessMessageQueue( )
		{
		    try
		    {
		        using (DatabaseContext databaseContext = DatabaseContext.GetContext())
		        {
		            using (IDbCommand command = databaseContext.CreateCommand())
		            {
		                IDbDataParameter parameter = databaseContext.AddParameter(command, "@tenantId", DbType.Int64);

		                while (true)
		                {		                    
		                    _messageAvailable.WaitOne();

		                    while (_messages.Count > 0)
		                    {
		                        try
		                        {
                                    Tuple<string[], long> data;

                                    lock (_messageQueueLock)
                                    {
                                        data = _messages.Dequeue();
                                    }

                                    string[] parts = data.Item1;

                                    PreloadStrings(parts.Skip(1).ToArray(), data.Item2, command, parameter);

                                    /////
                                    // Format the output.
                                    /////
                                    switch (parts[0])
                                    {
                                        case "GRANTED":
                                            _defaultListener.WriteLine(string.Format("Entity '{0} ({1})' GRANTED '{2} ({3})' due to statement between Role '{4} ({5})' and Group '{6} ({7})'.", GetMapValue(parts[1]), parts[1], GetMapValue(parts[2]), parts[2], GetMapValue(parts[3]), parts[3], GetMapValue(parts[4]), parts[4]));
                                            break;

                                        case "DENIED":
                                            _defaultListener.WriteLine(string.Format("Entity '{0} ({1})' DENIED '{2} ({3})' due to statement between Role '{4} ({5})' and Group '{6} ({7})'.", GetMapValue(parts[1]), parts[1], GetMapValue(parts[2]), parts[2], GetMapValue(parts[3]), parts[3], GetMapValue(parts[4]), parts[4]));
                                            break;

                                        case "NOTGRANTED":
                                            _defaultListener.WriteLine(string.Format("Entity '{0} ({1})' NOT GRANTED '{2} ({3})' due to no applicable security statement.", GetMapValue(parts[1]), parts[1], GetMapValue(parts[2]), parts[2]));
                                            break;
                                    }
		                        }
                                catch (Exception ex)
                                {
                                    Diagnostics.EventLog.Application.WriteError("A failure occurred writing a message to the security trace listener. Error {0}.", ex);
                                }
		                    }		                    
		                }
		            }
		        }
		    }
		    catch (Exception ex)
		    {
                Diagnostics.EventLog.Application.WriteError("An unexpected error occurred in the security trace listener worker thread. No more security trace messages will be logged. Error {0}.", ex);
		    }
		}
	}
}