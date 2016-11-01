// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using EDC.Diagnostics;
using EDC.IO;
using EDC.ReadiNow.IO;
using EDC.Threading;

namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     Provides helper methods for interacting with the application file event log.
	/// </summary>
	public static class EventLogHelper
	{
		/// <summary>
		///     Gets all log entries from the active log file.
		/// </summary>
		/// <returns>
		///     A collection of the entries contained within the active log file.
		/// </returns>
		public static EventLogEntryCollection GetEntries( )
		{
			return LoadActiveLog( );
		}

		/// <summary>
		///     Gets all log entries from the selected log file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path of the log file to examine.
		/// </param>
		/// <returns>
		///     A collection of the entries contained within the active log file.
		/// </returns>
		public static EventLogEntryCollection GetEntries( string path )
		{
			return LoadLog( path );
		}

		/// <summary>
		///     Gets the specified log entries from the active log file.
		/// </summary>
		/// <param name="origin">
		///     An enumeration indicating where the query operation starts.
		/// </param>
		/// <param name="direction">
		///     An enumeration indicating the direction of the query task.
		/// </param>
		/// <param name="count">
		///     The number of event log entries to retrieve.
		/// </param>
		/// <returns>
		///     A collection of the entries contained within the active log file.
		/// </returns>
		public static EventLogEntryCollection GetEntries( EventLogOrigin origin, EventLogDirection direction, int count )
		{
			var entries = new EventLogEntryCollection( );

			// Load the active log entries
			EventLogEntryCollection activeEntries = LoadActiveLog( );

			// Check for valid query options
			if ( ( origin == EventLogOrigin.First ) && ( direction == EventLogDirection.FirstToLast ) )
			{
				count = ( activeEntries.Count < count ) ? activeEntries.Count : count;

				// Copy the specified log entries (oldest to newest)
				for ( int x = 0; x < count; ++x )
				{
				    if (x >= 0 && x < activeEntries.Count) //assert true .. but something is failing intermittently
				    {
				        entries.Add(activeEntries[x]);
				    }
				}
			}
			else if ( ( origin == EventLogOrigin.Last ) && ( direction == EventLogDirection.LastToFirst ) )
			{
				count = ( activeEntries.Count < count ) ? activeEntries.Count : count;

				// Copy the specified log entries (newest to oldest)
                for (int x = count - 1; x >= 0; --x)
                {
                    if (x >= 0 && x < activeEntries.Count) //assert true .. but something is failing intermittently
                    {
                        entries.Add(activeEntries[x]);
                    }
                }
			}

			return entries;
		}

		/// <summary>
		///     Gets the specified log entries from the active log file.
		/// </summary>
		/// <param name="id">
		///     An id identifying the log entry where the query operation starts.
		/// </param>
		/// <param name="direction">
		///     An enumeration indicating the direction of the query task.
		/// </param>
		/// <param name="count">
		///     The number of event log entries to retrieve.
		/// </param>
		/// <returns>
		///     A collection of the entries contained within the active log file.
		/// </returns>
		public static EventLogEntryCollection GetEntries( Guid id, EventLogDirection direction, int count )
		{
			var entries = new EventLogEntryCollection( );

			// Check for valid query options
			if ( id != Guid.Empty )
			{
				// Load the active log entries
				EventLogEntryCollection activeEntries = LoadActiveLog( );

				// Check if the specified entry exists (using LINQ)
				int index = activeEntries.FindIndex( entry => entry.Id == id );
				if ( index >= 0 )
				{
					if ( direction == EventLogDirection.FirstToLast )
					{
						// Move the index forward by one position (i.e. next position)
						index = index + 1;

						// Determine the number of log entries to retreive
						count = ( activeEntries.Count - index < count ) ? activeEntries.Count - index : count;

						// Copy the specified log entries (oldest to newest)
						for ( int x = 0; x < count; ++x )
						{
						    var i = index + x;
                            if (i >= 0 && i < activeEntries.Count)  //assert true .. but something is failing intermittently
						    {
                                entries.Add(activeEntries[i]);
						    }
						}
					}
					else if ( direction == EventLogDirection.LastToFirst )
					{
						// ToDo: Implement
					}
				}
			}

			return entries;
		}

		/// <summary>
		///     Loads the log entries from the active log file.
		/// </summary>
		/// <returns>
		///     A collection of the entries contained within the active log file.
		/// </returns>
		private static EventLogEntryCollection LoadActiveLog( )
		{
			// Load the active log file
			string path = Path.Combine( SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Log ), "Log.xml" );
			EventLogEntryCollection eventLogEntries = LoadLog( path );

			return eventLogEntries;
		}

		/// <summary>
		///     Loads the log entries from the specified log file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path of the log file to examine.
		/// </param>
		/// <returns>
		///     A dictionary of the entries contained within the specified log file.
		/// </returns>
		private static EventLogEntryCollection LoadLog( string path )
		{
			var eventLogEntries = new EventLogEntryCollection( );

			// Check that the log file exists
			if ( File.Exists( path ) )
			{
				// Create a mutex for this folder.
				string mutexName = FileEventLogWriter.GetLogMutexName( path );

				using ( var logMutex = new NamedMutex( mutexName ) )
				{
					if ( logMutex.Acquire( ) )
					{
						eventLogEntries = LoadLog_Impl( path );
					}
				}
			}

			return eventLogEntries;
		}

		/// <summary>
		///     Loads the log entries from the specified log file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path of the log file to examine.
		/// </param>
		/// <returns>
		///     A dictionary of the entries contained within the specified log file.
		/// </returns>
		private static EventLogEntryCollection LoadLog_Impl( string path )
		{
			var eventLogEntries = new EventLogEntryCollection( );

			// Attempt to open the log file
			FileStream logStream = FileHelper.TryOpenFile( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 10000 );
			if ( logStream != null )
			{
				try
				{
					var xrs = new XmlReaderSettings
						{
							ConformanceLevel = ConformanceLevel.Fragment
						};

					using ( XmlReader xr = XmlReader.Create( logStream, xrs ) )
					{
						while ( xr.Read( ) )
						{
							if ( xr.NodeType == XmlNodeType.Element &&
							     xr.Name == "entry" )
							{
								// ReSharper disable EmptyGeneralCatchClause
								try
								{
									XElement entry = XElement.Load( xr.ReadSubtree( ) );

									Guid id;
									DateTime date;
									long timestamp;
									EventLogLevel level;
									string machine;
									string process;
									int thread;
									string source;
									string message;
								    long tenantId = -1;
								    string userName = string.Empty;
								    string tenantName = string.Empty;

									XElement element = entry.Element( "id" );

									if ( element != null )
									{
										id = new Guid( element.Value );
									}
									else
									{
										continue;
									}

									XElement dateElement = entry.Element( "date" );

									if ( dateElement != null )
									{
										date = DateTime.Parse( dateElement.Value );

										timestamp = long.Parse( dateElement.Attribute( "timestamp" ).Value );
									}
									else
									{
										continue;
									}

									XElement levelElement = entry.Element( "level" );

									if ( levelElement != null )
									{
										level = ( EventLogLevel ) Enum.Parse( typeof ( EventLogLevel ), levelElement.Value, true );
									}
									else
									{
										continue;
									}

									XElement machineElement = entry.Element( "machine" );

									if ( machineElement != null )
									{
										machine = machineElement.Value;
									}
									else
									{
										continue;
									}

									XElement processElement = entry.Element( "process" );

									if ( processElement != null )
									{
										process = processElement.Value;
									}
									else
									{
										continue;
									}

									XElement threadElement = entry.Element( "thread" );

									if ( threadElement != null )
									{
										thread = int.Parse( threadElement.Value );
									}
									else
									{
										continue;
									}

									XElement sourceElement = entry.Element( "source" );

									if ( sourceElement != null )
									{
										source = sourceElement.Value;
									}
									else
									{
										continue;
									}

									XElement messageElement = entry.Element( "message" );

									if ( messageElement != null )
									{
										message = messageElement.Value;
									}
									else
									{
										continue;
									}

                                    XElement tenantIdElement = entry.Element("tenantId");

                                    if (tenantIdElement != null)
                                    {
                                        if (!long.TryParse(tenantIdElement.Value, out tenantId))
                                        {
                                            tenantId = -1;
                                        }                                        
                                    }

                                    XElement tenantNameElement = entry.Element("tenantName");

                                    if (tenantNameElement != null)
                                    {
                                        tenantName = tenantNameElement.Value;
                                    }

                                    XElement userNameElement = entry.Element("userName");

                                    if (userNameElement != null)
                                    {
                                        userName = userNameElement.Value;
                                    }

                                    var eventLogEntry = new EventLogEntry(id, date, timestamp, level, machine, process, thread, source, message, tenantId, tenantName, userName, path);
									eventLogEntries.Add( eventLogEntry );
								}
								catch
								{
									// Do nothing
								}
								// ReSharper restore EmptyGeneralCatchClause
							}
						}
					}
				}
				finally
				{
					logStream.Close( );
				}
			}

			return eventLogEntries;
		}
	}
}