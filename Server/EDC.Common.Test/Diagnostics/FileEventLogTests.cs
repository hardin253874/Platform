// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using EDC.IO;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.Diagnostics.Test
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     This is a test class for the FileEventLog type
	/// </summary>
	[TestFixture]
	public class FileEventLogTests
	{        
        /// <summary>
        ///     Create an event log.
        /// </summary>
        private EventLogDetails CreateEventLog(EventLogOptions options)
		{
			string key = Guid.NewGuid( ).ToString( "B" );

			// Initialize the event log properties
			string folder = Path.GetTempPath( );
			string filename = $"{key}.xml";

			return CreateEventLog( folder, filename, options);
		}

		/// <summary>
		///     Create an event log.
		/// </summary>
		private EventLogDetails CreateEventLog( string folder, string filename, EventLogOptions options)
		{
			var writer = new FileEventLogWriter( folder, filename )
			{
				ErrorEnabled = options.ErrorEnabled,
				WarningEnabled = options.WarningEnabled,
				InformationEnabled = options.InformationEnabled,
				TraceEnabled = options.TraceEnabled
			};

			var eventLog = new EventLog(new List<IEventLogWriter> { writer });

            return new EventLogDetails
            {
                EventLog = eventLog,
                LogWriter = writer
            };
        }

		/// <summary>
		///     Delete the specified event log (and all history).
		/// </summary>
		private void DeleteEventLog(FileEventLogWriter eventLog )
		{
			if ( eventLog != null )
			{
				try
				{
					string path = Path.Combine( eventLog.Folder, eventLog.Filename );
					string key = Path.GetFileNameWithoutExtension( path );
					string pattern = $"{key}*.xml";

					// Get the list of all temporary log files
					string[] files = Directory.GetFiles( eventLog.Folder, pattern );

					// Delete all temporary log files
					foreach ( string file in files )
					{
						try
						{
							File.Delete( file );
						}
// ReSharper disable EmptyGeneralCatchClause
						catch
// ReSharper restore EmptyGeneralCatchClause
						{
							// Do nothing
						}
					}
				}
				finally
				{
					eventLog.Dispose( );
				}
			}
		}


        /// <summary>
        /// Loads the log entries from the main log file.
        /// </summary>
        /// <param name="eventLog">The event log.</param>
        /// <param name="all">if set to <c>true</c> [all].</param>
        /// <returns>
        /// A dictionary of the entries contained within the main log file.
        /// </returns>
		private EventLogEntryDictionary LoadLogEntries(FileEventLogWriter eventLog, bool all = false )
		{
            if (!all)
            {
                // Load the primary log file
                string path = Path.Combine(eventLog.Folder, eventLog.Filename);
                EventLogEntryDictionary logEntries = LoadLogEntries(path);

                return logEntries;
            }
            else
            {
                var logEntries = new EventLogEntryDictionary();

                string path = Path.Combine( eventLog.Folder, eventLog.Filename );
				string key = Path.GetFileNameWithoutExtension( path );
				string pattern = $"{key}*.xml";

				// Get the list of all the log files
				string[] files = Directory.GetFiles( eventLog.Folder, pattern );

                foreach (string file in files)
                {
                    var logEntriesFile = LoadLogEntries(file);

                    foreach (var kvp in logEntriesFile)
                    {
                        logEntries.Add(kvp.Key, kvp.Value);
                    }
                }

                return logEntries;
            }
		}

		/// <summary>
		///     Loads the log entries from the specified log file.
		/// </summary>
		/// <param name="path">
		///     A string containing the pat of the log file to examine.
		/// </param>
		/// <returns>
		///     A dictionary of the entries contained within the specified log file.
		/// </returns>
		private EventLogEntryDictionary LoadLogEntries( string path )
		{
			var logEntries = new EventLogEntryDictionary( );

			if ( File.Exists( path ) )
			{
				// Attempt to open the log file
				FileStream logStream = FileHelper.TryOpenFile( path, FileMode.Open, FileAccess.Read, FileShare.Read, 10000 );
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
									try
									{
										XElement entry = XElement.Load( xr.ReadSubtree( ) );

										XElement xElement = entry.Element( "id" );

										if ( xElement != null )
										{
											var id = new Guid( xElement.Value );
											XElement dateElement = entry.Element( "date" );
											if ( dateElement != null )
											{
												DateTime date = DateTime.Parse( dateElement.Value );
												long timestamp = long.Parse( dateElement.Attribute( "timestamp" ).Value );
												XElement element = entry.Element( "level" );
												if ( element != null )
												{
													var level = ( EventLogLevel ) Enum.Parse( typeof( EventLogLevel ), element.Value, true );
													XElement xElement1 = entry.Element( "machine" );
													if ( xElement1 != null )
													{
														string machine = xElement1.Value;
														XElement element1 = entry.Element( "process" );
														if ( element1 != null )
														{
															string process = element1.Value;
															XElement xElement2 = entry.Element( "thread" );
															if ( xElement2 != null )
															{
																int thread = int.Parse( xElement2.Value );
																XElement element2 = entry.Element( "source" );
																if ( element2 != null )
																{
																	string source = element2.Value;
																	XElement xElement3 = entry.Element( "message" );
																	if ( xElement3 != null )
																	{
																		string message = xElement3.Value;

																		long tenantId = -1;
																		XElement tenantIdElement = entry.Element( "tenantId" );
																		if ( tenantIdElement != null )
																		{
																			if ( !long.TryParse( tenantIdElement.Value, out tenantId ) )
																			{
																				tenantId = -1;
																			}
																		}

																		string tenantName = string.Empty;
																		XElement tenantNameElement = entry.Element( "tenantName" );
																		if ( tenantNameElement != null )
																		{
																			tenantName = tenantNameElement.Value;
																		}

																		string userName = string.Empty;
																		XElement userNameElement = entry.Element( "userName" );
																		if ( userNameElement != null )
																		{
																			userName = userNameElement.Value;
																		}

																		var logEntry = new EventLogEntry( id, date, timestamp, level, machine, process, thread, source, message, tenantId, tenantName, userName );
																		logEntries [ id ] = logEntry;
																	}
																}
															}
														}
													}
												}
											}
										}
									}
									// ReSharper disable EmptyGeneralCatchClause
									catch
									// ReSharper restore EmptyGeneralCatchClause
									{
										// Do nothing
									}
								}
							}
						}
					}
					finally
					{
						logStream.Close( );
					}
				}
			}

			return logEntries;
		}

		/// <summary>
		/// Flushes the event log.
		/// </summary>
		/// <param name="eventLog">The event log.</param>
		private void FlushEventLog( FileEventLogWriter eventLog )
		{
			eventLog?.GetType( ).InvokeMember( "FlushLog", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, eventLog, new object[ ]
			{
				false
			} );
		}

		/// <summary>
		///     Test creating a FileEventLog with invalid parameters.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ConstructorInvalidFilenameParameter( )
		{
			using ( new FileEventLogWriter( "folder", null ) )
			{
			}
		}

		/// <summary>
		///     Test creating a FileEventLog with invalid parameters.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ConstructorInvalidFolderParameter( )
		{
			using ( new FileEventLogWriter( null, "filename" ) )
			{
			}
		}

		/// <summary>
		///     Test creating a FileEventLog with valid parameters.
		/// </summary>
		[Test]
		public void ConstructorValidParameters( )
		{
			using ( var eventLog = new FileEventLogWriter( "folder", "filename" ) )
			{
				Assert.IsNotNull( eventLog, "The event log should not be null" );
			}
		}

		/// <summary>
		///     Test getting a mutex name using an invalid folder GetLogMutexName.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void GetLogMutexNameInvalidFoldername( )
		{
            FileEventLogWriter.GetLogMutexName( null );
		}

		/// <summary>
		///     Tests that WriteError does not write an entry to the event log if error logging is disabled.
		/// </summary>
		[Test]
		public void WriteError_ErrorDisabled_EntryNotPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample error message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { ErrorEnabled = false });
                IEventLog eventLog = eventLogDetails.EventLog;

				eventLog.WriteError( message );

				FlushEventLog( eventLogDetails.LogWriter );

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) );

				// Check that the entry is not present

				Assert.IsFalse( match );
			}
			finally
			{
				if ( eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteError writes an entry to the event log if error logging is enabled.
		/// </summary>
		[Test]
		public void WriteError_ErrorEnabled_EntryPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample error message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { ErrorEnabled = true });
                IEventLog eventLog = eventLogDetails.EventLog;

				using ( ManualResetEvent evt = new ManualResetEvent( false ) )
				{
					EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
					{
						if ( args.EntriesWritten > 0 )
						{
							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
						}
					};

					eventLogDetails.LogWriter.LogWritten += handler;

					// Write an event                
					eventLog.WriteError( message );
					FlushEventLog( eventLogDetails.LogWriter );

					evt.WaitOne( 5000 );

					eventLogDetails.LogWriter.LogWritten += handler;
				}

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				string processName = Process.GetCurrentProcess( ).MainModule.ModuleName;
				int threadId = Thread.CurrentThread.ManagedThreadId;

				// Check that the entry is present
				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) && logEntries[ id ].Process == processName && logEntries[ id ].ThreadId == threadId );

				Assert.IsTrue( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteInformation does not write an entry to the event log if information logging is disabled.
		/// </summary>
		[Test]
		public void WriteInformation_InformationDisabled_EntryNotPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample information message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { InformationEnabled = false });
                IEventLog eventLog = eventLogDetails.EventLog;

				// Write an event                
				eventLog.WriteInformation( message );

				FlushEventLog( eventLogDetails.LogWriter );

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) );

				// Check that the entry is not present

				Assert.IsFalse( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteInformation writes an entry to the event log if information logging is enabled.
		/// </summary>
		[Test]
		public void WriteInformation_InformationEnabled_EntryPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample information message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { InformationEnabled = true });
                IEventLog eventLog = eventLogDetails.EventLog;

				using ( ManualResetEvent evt = new ManualResetEvent( false ) )
				{
					EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
					{
						if ( args.EntriesWritten > 0 )
						{
							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
						}
					};

					eventLogDetails.LogWriter.LogWritten += handler;

					// Write an event                
					eventLog.WriteInformation( message );

					FlushEventLog( eventLogDetails.LogWriter );

					evt.WaitOne( 5000 );

					eventLogDetails.LogWriter.LogWritten += handler;
				}

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				string processName = Process.GetCurrentProcess().MainModule.ModuleName;
				int threadId = Thread.CurrentThread.ManagedThreadId;

				// Check that the entry is present
				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) && logEntries[ id ].Process == processName && logEntries[ id ].ThreadId == threadId );

				Assert.IsTrue( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteTrace writes an entry to the event log if trace logging is enabled.
		/// </summary>
		[Test]
		public void WriteTrace_NoTenantOrUserName( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				DiagnosticsRequestContext.FreeContext( );

				Assert.IsTrue( string.IsNullOrEmpty( DiagnosticsRequestContext.UserName ) );
				Assert.AreEqual( -1, DiagnosticsRequestContext.TenantId );

				string message = $"This is a sample trace message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { TraceEnabled = true });
                IEventLog eventLog = eventLogDetails.EventLog;

				using ( ManualResetEvent evt = new ManualResetEvent( false ) )
				{
					EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
					{
						if ( args.EntriesWritten > 0 )
						{
							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
						}
					};

					eventLogDetails.LogWriter.LogWritten += handler;

					// Write an event                
					eventLog.WriteTrace( message );

					FlushEventLog( eventLogDetails.LogWriter );

					evt.WaitOne( 5000 );

					eventLogDetails.LogWriter.LogWritten += handler;
				}

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				string processName = Process.GetCurrentProcess().MainModule.ModuleName;
				int threadId = Thread.CurrentThread.ManagedThreadId;

				// Check that the entry is present
				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) && logEntries[ id ].Process == processName && logEntries[ id ].ThreadId == threadId && string.IsNullOrEmpty( logEntries[ id ].UserName ) && logEntries[ id ].TenantId == -1 );

				Assert.IsTrue( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}


		/// <summary>
		///     Tests that WriteTrace writes an entry to the event log if trace logging is enabled.
		/// </summary>
		[Test]
		public void WriteTrace_TenantOrUserName( )
		{
			EventLogDetails eventLogDetails = null;

			try
			{
				DiagnosticsRequestContext.SetContext( 12345, "EDC", "UserName" );

				string message = $"This is a sample trace message (Tag: {Guid.NewGuid( ):B}).";

				// Create the event log
				eventLogDetails = CreateEventLog( new EventLogOptions { TraceEnabled = true } );
				IEventLog eventLog = eventLogDetails.EventLog;

				using ( ManualResetEvent evt = new ManualResetEvent( false ) )
				{
					EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
					{
						if ( args.EntriesWritten > 0 )
						{
							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
						}
					};

					eventLogDetails.LogWriter.LogWritten += handler;

					// Write an event                
					eventLog.WriteTrace( message );

					FlushEventLog( eventLogDetails.LogWriter );

					evt.WaitOne( 5000 );

					eventLogDetails.LogWriter.LogWritten += handler;
				}

				// Load the current log entries
				var logEntries = LoadLogEntries( eventLogDetails.LogWriter );

				bool match = false;

				string processName = Process.GetCurrentProcess( ).MainModule.ModuleName;
				int threadId = Thread.CurrentThread.ManagedThreadId;

				// Check that the entry is present
				if ( logEntries != null )
				{
					if ( logEntries.Keys.Any( id => logEntries [ id ].Message.Contains( message ) &&
													logEntries [ id ].Process == processName &&
													logEntries [ id ].ThreadId == threadId &&
													logEntries [ id ].UserName == "UserName" &&
													logEntries [ id ].TenantId == 12345 ) )
					{
						match = true;
					}
				}

				Assert.IsTrue( match );
			}
			finally
			{
				DiagnosticsRequestContext.FreeContext( );

				if ( eventLogDetails != null )
				{
					DeleteEventLog( eventLogDetails.LogWriter );
				}
			}
		}

		/// <summary>
		///     Tests that WriteTrace does not write an entry to the event log if trace logging is disabled.
		/// </summary>
		[Test]
		public void WriteTrace_TraceDisabled_EntryNotPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample trace message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { TraceEnabled = false });
                IEventLog eventLog = eventLogDetails.EventLog;

				// Write an event                
				eventLog.WriteTrace( message );

				FlushEventLog( eventLogDetails.LogWriter );

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) );

				// Check that the entry is not present

				Assert.IsFalse( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteTrace writes an entry to the event log if trace logging is enabled.
		/// </summary>
		[Test]
		public void WriteTrace_TraceEnabled_EntryPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample trace message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { TraceEnabled = true });
                IEventLog eventLog = eventLogDetails.EventLog;

				using ( ManualResetEvent evt = new ManualResetEvent( false ) )
				{
					EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
					{
						if ( args.EntriesWritten > 0 )
						{
							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
						}
					};

					eventLogDetails.LogWriter.LogWritten += handler;

					// Write an event
					eventLog.WriteTrace( message );

					FlushEventLog( eventLogDetails.LogWriter );

					evt.WaitOne( 5000 );

					eventLogDetails.LogWriter.LogWritten += handler;
				}

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				string processName = Process.GetCurrentProcess().MainModule.ModuleName;
				int threadId = Thread.CurrentThread.ManagedThreadId;

				// Check that the entry is present
				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) && logEntries[ id ].Process == processName && logEntries[ id ].ThreadId == threadId );

				Assert.IsTrue( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteTrace writes an entry to the event log if trace logging is enabled.
		/// </summary>
		[Test]
		public void WriteTrace_TraceEnabled_EntryPresent_MultiThreaded( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message1 = $"This is a sample trace message1 (Tag: {Guid.NewGuid( ):B}).";
				string message2 = $"This is a sample trace message2 (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { TraceEnabled = true });
                IEventLog eventLog = eventLogDetails.EventLog;

				using ( var startEvent = new ManualResetEvent( false ) )
				using ( CountdownEvent counter = new CountdownEvent( 20 ) )
				{
					EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
					{
						// ReSharper disable once AccessToDisposedClosure
						counter.Signal( args.EntriesWritten );
					};

					eventLogDetails.LogWriter.LogWritten += handler;

					var t1 = new Thread( ( ) =>
						{
							// ReSharper disable once AccessToDisposedClosure
							startEvent.WaitOne( );

							for ( int i = 0; i < 10; i++ )
							{
								eventLog.WriteTrace( message1 );
							}
						} )
					{
						IsBackground = true
					};
					t1.Start( );

					var t2 = new Thread( ( ) =>
						{
							// ReSharper disable once AccessToDisposedClosure
							startEvent.WaitOne( );

							for ( int i = 0; i < 10; i++ )
							{
								eventLog.WriteTrace( message2 );
							}
						} )
					{
						IsBackground = true
					};
					t2.Start( );

					startEvent.Set( );

					t1.Join( );
					t2.Join( );

					int t1Id = t1.ManagedThreadId;
					int t2Id = t2.ManagedThreadId;


					FlushEventLog( eventLogDetails.LogWriter );

					counter.Wait( 5000 );

					eventLogDetails.LogWriter.LogWritten += handler;

					// Load the current log entries (optionally wait for thread pool to write entries)
					EventLogEntryDictionary logEntries = LoadLogEntries( eventLogDetails.LogWriter );

					string processName = Process.GetCurrentProcess( ).MainModule.ModuleName;

					int t1Count = 0;
					int t2Count = 0;

					// Check that the entries are present
					foreach ( Guid id in logEntries.Keys )
					{
						if ( logEntries [ id ].Message.Contains( message1 ) &&
							 logEntries [ id ].Process == processName &&
							 logEntries [ id ].ThreadId == t1Id )
						{
							t1Count++;
						}
						else if ( logEntries [ id ].Message.Contains( message2 ) &&
								  logEntries [ id ].Process == processName &&
								  logEntries [ id ].ThreadId == t2Id )
						{
							t2Count++;
						}
					}

					Assert.AreEqual( 10, t1Count );
					Assert.AreEqual( 10, t2Count );
				}
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteWarning does not write an entry to the event log if warning logging is disabled.
		/// </summary>
		[Test]
		public void WriteWarning_WarningDisabled_EntryNotPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample warning message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { WarningEnabled = false });
                IEventLog eventLog = eventLogDetails.EventLog;

				// Write an event
				eventLog.WriteWarning( message );

				FlushEventLog( eventLogDetails.LogWriter );

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) );

				// Check that the entry is not present

				Assert.IsFalse( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}

		/// <summary>
		///     Tests that WriteWarning writes an entry to the event log if warning logging is enabled.
		/// </summary>
		[Test]
		public void WriteWarning_WarningEnabled_EntryPresent( )
		{
            EventLogDetails eventLogDetails = null;

		    try
			{
				string message = $"This is a sample warning message (Tag: {Guid.NewGuid( ):B}).";

                // Create the event log
                eventLogDetails = CreateEventLog(new EventLogOptions { WarningEnabled = true });
                IEventLog eventLog = eventLogDetails.EventLog;

				using ( ManualResetEvent evt = new ManualResetEvent( false ) )
				{
					EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
					{
						if ( args.EntriesWritten > 0 )
						{
							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
						}
					};

					eventLogDetails.LogWriter.LogWritten += handler;

					// Write an event                
					eventLog.WriteWarning( message );

					FlushEventLog( eventLogDetails.LogWriter );

					evt.WaitOne( 5000 );

					eventLogDetails.LogWriter.LogWritten += handler;
				}

				// Load the current log entries
				var logEntries = LoadLogEntries(eventLogDetails.LogWriter);

				string processName = Process.GetCurrentProcess().MainModule.ModuleName;
				int threadId = Thread.CurrentThread.ManagedThreadId;

				// Check that the entry is present
				bool match = logEntries != null && logEntries.Keys.Any( id => logEntries[ id ].Message.Contains( message ) && logEntries[ id ].Process == processName && logEntries[ id ].ThreadId == threadId );

				Assert.IsTrue( match );
			}
			finally
			{
				if (eventLogDetails != null )
				{
					DeleteEventLog(eventLogDetails.LogWriter);
				}
			}
		}


        /// <summary>
        /// Test purging log files when the max count has been exceeded.
        /// </summary>
	    [Test]
        public void Purge_MaxCount_Test()
        {
			for ( int ii = 0; ii < 100; ii++ )
			{
				int fileCount = 5;
				int messageCount = 5;

				// Setup unique log folder and file names
				EventLogDetails eventLogDetails = null;
				string folder = Path.Combine( Path.GetTempPath( ), Guid.NewGuid( ).ToString( "B" ) );
				string fileNameOnly = Guid.NewGuid( ).ToString( "B" );
				string filename = $"{fileNameOnly}.xml";

				try
				{

					string message = $"This is a sample warning message (Tag: {Guid.NewGuid( ):B}).";

					message += new string( '.', 1000 ); // make sure it is one message per file

					// Initialize the event log properties                                
					// Create the event log
					Directory.CreateDirectory( folder );
					eventLogDetails = CreateEventLog( folder, filename, new EventLogOptions { WarningEnabled = true } );
					IEventLog eventLog = eventLogDetails.EventLog;

					int expectedFileCount = fileCount;

					using ( AutoResetEvent evt = new AutoResetEvent( false ) )
					{
						int fileCounter = -fileCount;

						EventHandler<LogWrittenEventArgs> logWrittenHandler = ( sender, args ) =>
						{
							if ( args.EntriesWritten > 1 )
							{
								expectedFileCount--;
							}

							if ( !string.IsNullOrEmpty( args.RotateDetails?.NewFilename ) )
							{
								File.SetLastWriteTime( args.RotateDetails.NewFilename, DateTime.Now.AddSeconds( fileCounter++ ) );

							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
							}
						};

						eventLogDetails.LogWriter.MaxCount = fileCount; // Maximum 5 files
						eventLogDetails.LogWriter.MaxSize = 1;
						eventLogDetails.LogWriter.LogWritten += logWrittenHandler;

						// Create 5 event logs, each a minute apart
						for ( var i = 0; i < messageCount; i++ )
						{
							eventLog.WriteWarning( message );
							FlushEventLog( eventLogDetails.LogWriter );
							evt.WaitOne( 1000 );
						}

						eventLogDetails.LogWriter.LogWritten -= logWrittenHandler;
					}

					eventLogDetails.LogWriter.Purge( );

					var oldFileList = Directory.GetFiles( eventLogDetails.LogWriter.Folder, $"{fileNameOnly}_*.xml" );

					Assert.That( oldFileList.Length, Is.EqualTo( expectedFileCount ) );

					var oldestFile = Directory.GetFiles( eventLogDetails.LogWriter.Folder, $"{fileNameOnly}_*.xml" ).OrderBy( f => new FileInfo( f ).LastWriteTime ).First( ); // files are named to be ordered by name

					using ( AutoResetEvent evt = new AutoResetEvent( false ) )
					{
						EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
						{
							if ( args.EntriesWritten > 0 )
							{
							// ReSharper disable once AccessToDisposedClosure
							evt.Set( );
							}
						};

						eventLogDetails.LogWriter.LogWritten += handler;

						// Write one more log that should purge the oldest
						eventLog.WriteWarning( message );

						FlushEventLog( eventLogDetails.LogWriter );

						evt.WaitOne( 5000 );

						eventLogDetails.LogWriter.LogWritten -= handler;
					}

					eventLogDetails.LogWriter.Purge( );   // force a purge

					var newFileList = Directory.GetFiles( eventLogDetails.LogWriter.Folder, $"{fileNameOnly}_*.xml" );

					var lostFiles = oldFileList.Except( newFileList ).ToList( );

					Assert.That( lostFiles.Count, Is.EqualTo( 1 ) );

					Assert.That( lostFiles.First( ), Is.EqualTo( oldestFile ) );
				}
				finally
				{
					if ( eventLogDetails != null )
					{
						DeleteEventLog(eventLogDetails.LogWriter);

						try
						{
							if (Directory.Exists(folder))
							{
							    Directory.Delete(folder, true);
							}
						}
						catch ( Exception exc )
						{
							Trace.WriteLine( exc.Message );
						}
					}
				}
			}
	    }


		/// <summary>
		/// Test purging stale log files.
		/// </summary>
		[Test]
		public void Purge_MaxRetention_Test( )
		{
			// Setup unique log folder and file names
			EventLogDetails eventLogDetails = null;
			string folder = Path.Combine( Path.GetTempPath( ), Guid.NewGuid( ).ToString( "B" ) );
			string fileNameOnly = Guid.NewGuid( ).ToString( "B" );
			string filename = $"{fileNameOnly}.xml";

			try
			{
				string message = $"This is a sample warning message (Tag: {Guid.NewGuid( ):B}).";

				// Initialize the event log properties                                
				// Create the event log
				Directory.CreateDirectory( folder );
				eventLogDetails = CreateEventLog( folder, filename, new EventLogOptions { WarningEnabled = true } );
				IEventLog eventLog = eventLogDetails.EventLog;

				int logEntriesWritten = 0;

				eventLogDetails.LogWriter.MaxRetention = 10;
				eventLogDetails.LogWriter.MaxSize = 1;

				// Create pre existing non-stale files
				var preExistingFileNamesToKeep = new List<string>( );

				for ( int i = 0; i < 5; i++ )
				{
					string fileName = Path.Combine( eventLogDetails.LogWriter.Folder, $"{fileNameOnly}_nonstale_{Guid.NewGuid( ):B}.xml" );
					preExistingFileNamesToKeep.Add( fileName );

					File.WriteAllText( fileName, message );
					File.SetLastWriteTime( fileName, DateTime.Now.AddDays( -5 ) );
				}

				// Create pre existing stale files
				var preExistingFileNamesToDelete = new List<string>( );

				for ( int i = 0; i < 5; i++ )
				{
					string fileName = Path.Combine( eventLogDetails.LogWriter.Folder, $"{fileNameOnly}_stale_{Guid.NewGuid( ):B}.xml" );
					preExistingFileNamesToDelete.Add( fileName );

					File.WriteAllText( fileName, message );
					File.SetLastWriteTime( fileName, DateTime.Now.AddDays( -15 ) );
				}

				using ( AutoResetEvent rotationEvent = new AutoResetEvent( false ) )
				using ( AutoResetEvent writeEvent = new AutoResetEvent( false ) )
				{
					EventHandler<LogWrittenEventArgs> logWrittenHandler = ( sender, args ) =>
					{
						logEntriesWritten += args.EntriesWritten;

						if ( !string.IsNullOrEmpty( args.RotateDetails?.NewFilename ) )
						{
								// ReSharper disable once AccessToDisposedClosure
								rotationEvent.Set( );
						}

						if ( args.EntriesWritten > 0 )
						{
								// ReSharper disable once AccessToDisposedClosure
								writeEvent.Set( );
						}
					};

					eventLogDetails.LogWriter.LogWritten += logWrittenHandler;

					WaitHandle [ ] waitHandles = {
							rotationEvent,
							writeEvent
						};

					// Write enough log entries to cause the log files to exceed it's size and to force a rotation and purge
					while ( true )
					{
						eventLog.WriteWarning( message );

						int handle = WaitHandle.WaitAny( waitHandles );

						if ( handle == 0 )
						{
							break;
						}
					}

					eventLogDetails.LogWriter.LogWritten -= logWrittenHandler;
				}

				// Wait for the thread pool to write entries and to do a rotate and purge
				FlushEventLog( eventLogDetails.LogWriter );

				eventLogDetails.LogWriter.Purge( ); // force a purge

				EventLogEntryDictionary logEntries = LoadLogEntries( eventLogDetails.LogWriter, true );

				// Ensure that new logs are not purged
				Assert.AreEqual( logEntriesWritten, logEntries.Count );

				string processName = Process.GetCurrentProcess( ).MainModule.ModuleName;
				int threadId = Thread.CurrentThread.ManagedThreadId;

				// Ensure that new logs are not purged
				Assert.IsTrue( logEntries.Values.All( le => le.Message == message && le.Process == processName && le.ThreadId == threadId ) );

				// Ensure that all the stale log files are deleted
				Assert.IsTrue( preExistingFileNamesToDelete.All( f => !File.Exists( f ) ) );

				// Ensure non-stale files still exist
				Assert.IsTrue( preExistingFileNamesToKeep.All( File.Exists ) );
			}
			finally
			{
				if ( eventLogDetails != null )
				{
					DeleteEventLog( eventLogDetails.LogWriter );

					try
					{
						if ( Directory.Exists( folder ) )
						{
							Directory.Delete( folder, true );
						}
					}
					catch ( Exception exc )
					{
						Trace.WriteLine( exc.Message );
					}
				}
			}
		}

		private void OnFileChanged(object source, FileSystemEventArgs e)
	    {
	    }

        /// <summary>
        /// Test purging stale log files.
        /// </summary>
        [Test]
        public void Purge_EventLogsIndependent_Test()
        {
            EventLogDetails eventLogDetails1 = null, eventLogDetails2 = null;

            // Setup unique log folder and file names            
            string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("B"));

            string filename1 = "first.xml";
            string filename2 = "second.xml";
            FileSystemWatcher watcher = null;

            try
            {
                // Initialize the event log properties                                
                // Create the event log
                Directory.CreateDirectory(folder);

				// Create a new FileSystemWatcher and set its properties.
				using ( watcher = new FileSystemWatcher
				{
					Path = folder,
					NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName,
					Filter = "*.xml"
				} )
				{

					watcher.Changed += OnFileChanged;
					watcher.Created += OnFileChanged;
					watcher.EnableRaisingEvents = true;

					using ( CountdownEvent evt = new CountdownEvent( 2000 ) )
					{
						// Create the keeper
						eventLogDetails1 = CreateEventLog( folder, filename1, new EventLogOptions { WarningEnabled = true } );
						IEventLog eventLog1 = eventLogDetails1.EventLog;

						eventLogDetails1.LogWriter.MaxRetention = 10;
						eventLogDetails1.LogWriter.MaxSize = 1;
						eventLogDetails1.LogWriter.MaxCount = 2;

						EventHandler<LogWrittenEventArgs> handler = ( sender, args ) =>
						{
							// ReSharper disable once AccessToDisposedClosure
							evt.Signal( args.EntriesWritten );
						};

						eventLogDetails1.LogWriter.LogWritten += handler;

						for ( int i = 0; i < 1000; i++ )
						{
							eventLog1.WriteWarning( "FirstOne" );
						}

						// create the purger
						eventLogDetails2 = CreateEventLog( folder, filename2, new EventLogOptions { WarningEnabled = true } );
						IEventLog eventLog2 = eventLogDetails2.EventLog;

						eventLogDetails2.LogWriter.MaxRetention = 10;
						eventLogDetails2.LogWriter.MaxSize = 1;
						eventLogDetails2.LogWriter.MaxCount = 2;

						eventLogDetails2.LogWriter.LogWritten += handler;

						for ( int i = 0; i < 1000; i++ )
						{
							eventLog2.WriteWarning( "SecondOne" );
						}

						FlushEventLog( eventLogDetails1.LogWriter );
						FlushEventLog( eventLogDetails2.LogWriter );

						evt.Wait( 5000 );

						eventLogDetails2.LogWriter.LogWritten -= handler;
						eventLogDetails1.LogWriter.LogWritten -= handler;
					}

					eventLogDetails1.LogWriter.Purge( );
					eventLogDetails2.LogWriter.Purge( );

					var files = Directory.GetFiles( folder ).Where( f =>
					{
						FileInfo fi = new FileInfo( f );

						return fi.Name.StartsWith( "first_" ) || fi.Name.StartsWith( "second_" );
					} ).ToList( );

					Assert.That( files.Count, Is.EqualTo( 4 ) );

					watcher.Created -= OnFileChanged;
					watcher.Changed -= OnFileChanged;
				}
            }
            finally
            {
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }

                if (eventLogDetails1 != null)
                {
                    DeleteEventLog(eventLogDetails1.LogWriter);
                }
                if (eventLogDetails2 != null)
                {
                    DeleteEventLog(eventLogDetails2.LogWriter);
                }

                try
                {
                    if (Directory.Exists(folder))
                    {
                       Directory.Delete(folder, true);
                    }
                }
                catch ( Exception ex )
                {
	                Trace.WriteLine( ex.Message );
                }
            }
        }  

        private class EventLogDetails
        {
            public IEventLog EventLog { get; set; }
            public FileEventLogWriter LogWriter { get; set; }
        }

        private class EventLogOptions
        {
            public bool ErrorEnabled { get; set; }
            public bool WarningEnabled { get; set; }
            public bool InformationEnabled { get; set; }
            public bool TraceEnabled { get; set; }
        }
    }
}