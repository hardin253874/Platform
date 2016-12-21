// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Threading;
using EDC.IO;
using EDC.Security;
using EDC.Threading;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Provides a base implementation of a file-based event log.
	///     Ensure that instances of this object are
	///     properly disposed to prevent thread leaks.
	/// </summary>
	[Serializable]
	public class FileEventLogWriter : CriticalFinalizerObject, IEventLogWriter, IDisposable
	{		
        private readonly ConcurrentQueue<EventLogEntry> _eventLogEntryQueue;
        private readonly string _filename;
		private readonly string _folder;
        private readonly string _path;
        private readonly string _baseFileName;
        private readonly string _baseExtension;
        private readonly object _lock = new object( );

		/// <summary>
		/// The synchronize mutex
		/// </summary>
		/// <remarks>
		/// Cannot be in a lazy since the owning thread is not deterministic.
		/// Could be primary thread or worker thread depending on which runs first.
		/// </remarks>
		private readonly NamedMutex _syncMutex;

		private readonly ManualResetEvent _shutdownEvent = new ManualResetEvent( false );

		/// <summary>
		///     True if the object has been disposed, false otherwise.
		/// </summary>
		private bool _disposed;	    		
		private Thread _eventLogWorkerThread;
        private readonly int _maxBatchSize = 20; // max number of log entries to write at once
		private int _maxRetention = 30; // 30 days
		private int _maxSize = 1024; // 1024 kilobytes
		private int _rotationCount;
	    private int _maxCount = 100;

		internal event EventHandler<LogWrittenEventArgs> LogWritten;

		/// <summary>
		///     This constructs a FileEventLog object.
		/// </summary>
		/// <param name="folder">The path of the event log folder.</param>
		/// <param name="filename">The name of the default log file.</param>        
		public FileEventLogWriter( string folder, string filename )
		{
			if ( string.IsNullOrEmpty( folder ) )
			{
				throw new ArgumentNullException( nameof( folder ) );
			}

			if ( string.IsNullOrEmpty( filename ) )
			{
				throw new ArgumentNullException( nameof( filename ) );
			}

            TraceEnabled = true;
            InformationEnabled = true;
            WarningEnabled = true;
            ErrorEnabled = true;

            // Initialise the folder and filename            
            _folder = folder;
			_filename = filename;            
            _path = Path.Combine( _folder, _filename );

            _baseFileName = Path.GetFileNameWithoutExtension(_filename);
            _baseExtension = Path.GetExtension(_filename);            
          
            // Derive the name of the mutex from the folder name.            
            // FileEventLog objects with different log folders
            // will have different mutex names.
            var syncMutexName = GetLogMutexName( folder );

            // Lazy mutex factory
            _syncMutex = new NamedMutex( syncMutexName  );

            _eventLogEntryQueue = new ConcurrentQueue<EventLogEntry>();

			// Initialise the thread worker.
			_eventLogWorkerThread = new Thread( WriteEntriesWorker )
			{
				Name = "FileEventLogWorkerThread",
				IsBackground = true
			};

			_eventLogWorkerThread.Start( );
		}

		/// <summary>
		///     Gets the name of the default log file.
		/// </summary>
		public string Filename => _filename;

		/// <summary>
		///     Gets the path of the event log folder
		/// </summary>
		public string Folder => _folder;

		/// <summary>
		///     Gets or sets the maximum number of log file to retain.
		/// </summary>
		public int MaxCount
		{
			get
			{
                return _maxCount;
			}

			set
			{
				int maxCount = value;

				// Check that the property value is between 1 and 10000
				if ( ( maxCount < 1 ) || ( maxCount > 10000 ) )
				{
					throw new ArgumentOutOfRangeException( nameof( value ), @"The specified MaxCount property is out or range." );
				}

                _maxCount = maxCount;
			}
		}

		/// <summary>
		///     Gets the number of days to retain log files.
		/// </summary>
		public int MaxRetention
		{
			get
			{
				return _maxRetention;
			}

			set
			{
				int maxRetention = value;

				// Check that the property value is between 1 day and 10 years
				if ( ( maxRetention <= 0 ) || ( maxRetention > 365 * 10 ) )
				{
					throw new ArgumentOutOfRangeException( nameof( value ), @"The specified MaxRetention property is out or range." );
				}

				_maxRetention = maxRetention;
			}
		}

		/// <summary>
		///     Gets or sets the maximum event log size (in kilobytes) before the log is rotated.
		/// </summary>
		public int MaxSize
		{
			get
			{
				return _maxSize;
			}

			set
			{
				int maxSize = value;

				// Check that the property value is between 1 and 8192 K (1K - 8M)
				if ( ( maxSize < 1 ) || ( maxSize > 8192 ) )
				{
					throw new ArgumentOutOfRangeException( nameof( value ), @"The specified MaxSize property is out or range." );
				}

				_maxSize = maxSize;
			}
		}

		/// <summary>
		///     Gets or sets whether trace messages are logged.
		/// </summary>
		public bool TraceEnabled
		{
            get; set;
		}		

		/// <summary>
		///     Gets or sets whether informational messages are logged.
		/// </summary>
		public bool InformationEnabled
		{
            get; set;
        }		

		/// <summary>
		///     Gets or sets whether warning messages are logged.
		/// </summary>
		public bool WarningEnabled
		{
            get; set;
        }		

		/// <summary>
		///     Gets or sets whether error messages are logged.
		/// </summary>
		public bool ErrorEnabled
		{
            get; set;
        }				

		/// <summary>
        ///     Finalizes an instance of the <see cref="FileEventLogWriter" /> class.
		/// </summary>
		/// <remarks>
		///     Each finalizer gets approximately 2 seconds to execute.
		///     If it has not completed in that time, the process will terminate.
		/// </remarks>
		~FileEventLogWriter( )
		{
			Dispose( false );
		}

		/// <summary>
		///     Flushes this instance.
		/// </summary>
		private void FlushLog( bool disposing )
		{
			/////
			// Any events not flushed after 2 seconds will be lost due to the internal finalizer timeout.
			/////
			try
			{
				Trace.TraceInformation( "Flush starting... " + _eventLogEntryQueue.Count + " entries found." );

				// Get the path of the log file
				string machineName = Environment.MachineName;
				string processName = Process.GetCurrentProcess( ).MainModule.ModuleName;	// Was the wrong process string randonly causing tests to fail.

				// Write the log entry to the event log
                bool acquired;
				using ( _syncMutex.AcquireRelease(out acquired) )
				{
					// Wait to acquire the mutex
					if ( acquired )
					{
						int entriesWritten = 0;

						EventLogEntry logEntry;
						List<EventLogEntry> entries = new List<EventLogEntry>( );
						while ( _eventLogEntryQueue.TryDequeue( out logEntry ) )
						{
							if ( logEntry == null )
							{
								continue;
							}

							logEntry.Id = Guid.NewGuid( );
							logEntry.Machine = machineName;
							logEntry.Process = processName;

							entries.Add( logEntry );
						}

						if ( entries.Count > 0 )
						{
							// Only attempt to open the file if there are entries to be written
							FileStream logStream = FileHelper.TryOpenFile( _path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 10000 );
							try
							{
								if ( logStream != null )
								{
									try
									{
										logStream.Seek( 0, SeekOrigin.End );

										// Encode the log entry to raw bytes
										byte [ ] data = EncodeEntries( entries );

										// Write the log entry
										if ( data != null && data.Length > 0 )
										{
											logStream.Write( data, 0, data.Length );

											entriesWritten = entries.Count;
										}

										logStream.Flush( );
									}
									finally
									{
										logStream.Flush( );
									}
								}
							}
							finally
							{
								logStream?.Dispose( );
							}
						}

						if ( entriesWritten > 0 )
						{
							// Validate the current log file and check if any processing is required
							var details = Validate( LogWritten != null );

							LogWritten?.Invoke( this, new LogWrittenEventArgs( _path, entriesWritten, details.RotateDetails, details.PurgeDetails ) );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				Trace.TraceError( "Failed to flush file event log. {0}", ex );
			}
			finally
			{
			    if (disposing)
			    {
                    Trace.TraceInformation("Flush complete. " + _eventLogEntryQueue.Count + " entries remain.");
                }                
			}
		}

        /// <summary>
        ///     Write an entry to the event log.
        /// </summary>        
        /// <param name="logEntry">
        ///    The log entry.
        /// </param>
        public void WriteEntry(EventLogEntry logEntry)
		{
            if (logEntry == null || !CanWriteEntry(logEntry.Level))
            {
                return;
            }

			// Add the entry to the list
			_eventLogEntryQueue.Enqueue(logEntry);
			    
            lock (_lock)
            {
				// Signal the worker thread that a new entry is available
				Monitor.Pulse(_lock);
            }			    			
		}


        /// <summary>
        ///     Returns true if the specified error level can be written to the log, false otherwise.
        /// </summary>
        /// <param name="level">The error level.</param>
        /// <returns></returns>
        private bool CanWriteEntry(EventLogLevel level)
        {
            bool canWriteEntry = false;

            if ((level == EventLogLevel.Error) && (ErrorEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Warning) && (WarningEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Information) && (InformationEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Trace) && (TraceEnabled))
            {
                canWriteEntry = true;
            }

            return canWriteEntry;
        }


        /// <summary>
        ///     Encodes an entry to a byte array.
        /// </summary>
        /// <param name="logEntries">
        ///     The log entries to encode.
        /// </param>
        private byte[ ] EncodeEntries( IEnumerable<EventLogEntry> logEntries )
		{
			// Build the XML fragment
			var logBuilder = new StringBuilder( );

            foreach ( EventLogEntry logEntry in logEntries )
            {
                try
                {
                    logBuilder.Append( "<entry>" );

                    logBuilder.Append( "<id>" );
                    logBuilder.Append( logEntry.Id.ToString( "B" ) );
                    logBuilder.Append( "</id>" );

                    logBuilder.AppendFormat( "<date timestamp='{0}'>", logEntry.Timestamp.ToString( CultureInfo.InvariantCulture ) );
                    logBuilder.Append( SecurityElement.Escape( logEntry.Date.ToString( "o" ) ) );
                    logBuilder.Append( "</date>" );

                    logBuilder.Append( "<level>" );
                    logBuilder.Append( logEntry.Level.ToString( "g" ) );
                    logBuilder.Append( "</level>" );

                    logBuilder.Append( "<machine>" );
                    logBuilder.Append( SecurityElement.Escape( logEntry.Machine ) );
                    logBuilder.Append( "</machine>" );

                    logBuilder.Append( "<process>" );
                    logBuilder.Append( SecurityElement.Escape( logEntry.Process ) );
                    logBuilder.Append( "</process>" );

                    logBuilder.Append( "<thread>" );
                    logBuilder.Append( logEntry.ThreadId.ToString( CultureInfo.InvariantCulture ) );
                    logBuilder.Append( "</thread>" );

                    logBuilder.Append( "<source>" );
                    logBuilder.Append( SecurityElement.Escape( logEntry.Source ) );
                    logBuilder.Append( "</source>" );

                    logBuilder.Append( "<message>" );
                    logBuilder.Append( SecurityElement.Escape( logEntry.Message ) );
                    logBuilder.Append( "</message>" );

                    logBuilder.Append( "<tenantId>" );
                    logBuilder.Append( logEntry.TenantId.ToString( CultureInfo.InvariantCulture ) );
                    logBuilder.Append( "</tenantId>" );

                    logBuilder.Append( "<tenantName>" );
                    logBuilder.Append( SecurityElement.Escape( logEntry.TenantName ) );
                    logBuilder.Append( "</tenantName>" );

                    logBuilder.Append( "<userName>" );
                    logBuilder.Append( SecurityElement.Escape( logEntry.UserName ) );
                    logBuilder.Append( "</userName>" );

                    logBuilder.Append( "</entry>" );
                }
                catch( Exception exc )
                {
	                Trace.WriteLine( exc.Message );
                }
            }

			// Encode the XML fragment to a byte array
			var encoding = new UTF8Encoding( );
			byte[ ] data = encoding.GetBytes( logBuilder.ToString( ) );

			return data;
		}

		/// <summary>
		///     Gets a unique path based on time and date.
		/// </summary>
		private string GetUniquePath()
		{						
			string newFileName = $"{_baseFileName}_{DateTime.Now:yy-MM-dd_HH-mm-ss}-{Path.GetRandomFileName( )}{_baseExtension}";			
			return Path.Combine(_folder, newFileName );
		}

		/// <summary>
		///		Purges all stale event log files.
		/// </summary>
		/// <param name="generateValidateDetails">if set to <c>true</c> [generate validate details].</param>
		/// <returns></returns>
		public PurgeDetails Purge( bool generateValidateDetails = false )
        {
			// Get the file contents of the log folder
			var directoryInfo = new DirectoryInfo(_folder);

            // Check for any files to purge (based on age). Only include date stamped files with a matching base name
            Dictionary<string, FileInfo> files = directoryInfo.GetFiles().Where(fi => fi.Name.StartsWith(_baseFileName + '_')).ToDictionary(fi => fi.Name);

			List<string> staleFilenames = null;
	        
	        if (files.Count > 0)
            {
	            // Check for any stale event log files
	            List<FileInfo> staleFiles = files.Values.Where( f => string.Compare( f.Name, _filename, StringComparison.OrdinalIgnoreCase ) != 0 && DateTime.Now - f.LastWriteTime >= TimeSpan.FromDays( _maxRetention ) ).ToList( );

				if ( generateValidateDetails && staleFiles.Count > 0 )
				{
					staleFilenames = staleFiles.Select( fi => fi.FullName ).ToList( );
				}

	            // Delete any files and remove from the initial set
                DeleteFiles(staleFiles.ToArray(), files);
            }

	        List<string> overflowFilenames = null;

            // Check for any files to purge (based on count)			
            if (files.Count > MaxCount)
            {
                // Sort the files by last modified date
                List<FileInfo> filesToDelete = files.Values.Where( f => string.Compare( f.Name, _filename, StringComparison.OrdinalIgnoreCase ) != 0 ).OrderBy( f => f.LastWriteTime ).Take( files.Count - MaxCount ).ToList( );

				if ( generateValidateDetails && filesToDelete.Count > 0 )
				{
					overflowFilenames = filesToDelete.Select( fi => fi.FullName ).ToList( );
				}

                DeleteFiles(filesToDelete, null);
            }

			if ( generateValidateDetails )
			{
				return new PurgeDetails( staleFilenames, overflowFilenames );
			}

			return null;
        }

        /// <summary>
        /// Deletes the files.
        /// </summary>
        /// <param name="filesToDelete">The files.</param>
        /// <param name="files">The files dictionary.</param>
	    private void DeleteFiles(IEnumerable<FileInfo> filesToDelete, Dictionary<string, FileInfo> files)
	    {
            if (filesToDelete == null)
            {
                return;
            }

            foreach (FileInfo file in filesToDelete)
            {
                try
                {
					file.Delete();

	                files?.Remove(file.Name);
                }
                catch (Exception exception)
                {
					Trace.TraceError( "Unable to delete the log file. {0}", exception );
				}
            }            
        }

		/// <summary>
		///     Rotates the specified event log file.
		/// </summary>
		/// <param name="path">
		///     A string containing the path of the event log file.
		/// </param>
		private RotateDetails Rotate( string path )
		{
			if ( path == null )
			{
				throw new NullReferenceException( "The specified path parameter is null." );
			}			

			try
			{
				// Attempt to get unique log file name based on date/time
				string newPath = GetUniquePath();

				// Rename the specified event log file
				if ( FileHelper.TryMoveFile( path, newPath, 10000 ) )
				{
					return new RotateDetails( newPath );
				}
			}
			catch ( Exception ex )
			{
				Trace.TraceError( "Unable to rotate the log file. {0}", ex );
			}

			return null;
		}

		/// <summary>
		///     Validate the current log file and check if any processing is required.
		/// </summary>
		private ValidateDetails Validate( bool generateValidateDetails = false )
		{
			try
			{
				RotateDetails rotateDetails = null;
				PurgeDetails purgeDetails = null;

				// Check the path of the log file				
				// Check if the event log file should be rotated
				var fileInfo = new FileInfo( _path );
				if (fileInfo.Exists && fileInfo.Length >= _maxSize * 1024 )
				{
					// Rotate the current event log file
					rotateDetails = Rotate( _path );

					// Purge any stale event log files (every 10 rotations)
					if ( _rotationCount++ % 10 == 0 )
					{
						purgeDetails = Purge( generateValidateDetails );
					}
				}

				if ( generateValidateDetails )
				{
					return new ValidateDetails( rotateDetails, purgeDetails );
				}

				return null;
			}
			catch ( Exception ex )
			{
				Trace.TraceError( "Failed to validate log file. {0}", ex );
			}

			return null;
		}

		/// <summary>
		///     Writes the log entries to the event log.
		/// </summary>
		/// <remarks>This method is the worker method for the event log thread worker.</remarks>
		private void WriteEntriesWorker( )
		{			
            List<EventLogEntry> entriesToWrite = new List<EventLogEntry>( );

			try
			{
				while ( true )
				{
				    lock (_lock)
				    {
				        // Wait for log events to be available
				        while (_eventLogEntryQueue.Count == 0)
				        {
				            Monitor.Wait(_lock);

							if ( _disposed || _shutdownEvent.WaitOne( 0 ) )
							{
								break;
							}
						}
				    }

				    if ( _disposed || _shutdownEvent.WaitOne( 0 ) )
					{
					    break;
					}

					// Get the first log from the list
					EventLogEntry logEntry;
					
					while ( entriesToWrite.Count < _maxBatchSize && _eventLogEntryQueue.TryDequeue( out logEntry ) )
                    {
                        logEntry.Id = Guid.NewGuid( );                        

                        entriesToWrite.Add( logEntry );
					}

					if ( entriesToWrite.Count > 0 )
                    {
					    // Write the log entry to the event log
                        WriteEntriesToFile( entriesToWrite );                        

                        entriesToWrite.Clear( );
					}                            

					
				}
			}
			catch ( ThreadAbortException )
			{
				Thread.ResetAbort( );
                Trace.TraceInformation("FileEventLog.WriteEntries thread aborted. " + _eventLogEntryQueue.Count + " entries remain.");
			}
			catch ( Exception ex )
			{
				Trace.TraceError( "FileEventLog.WriteEntries failed. Error {0}.", ex );
			}
		}

		/// <summary>
		///     Write a log entry to the event log.
		/// </summary>
        /// <param name="logEntries">The log entries to write to the event log.</param>
		private void WriteEntriesToFile( IList<EventLogEntry> logEntries )
		{
			// ReSharper disable EmptyGeneralCatchClause
			try
			{
				// Create a mutex with the specified name.
                bool acquired;
				using ( _syncMutex.AcquireRelease(out acquired) )
				{
					// Wait to acquire the mutex
					if ( acquired )
					{
						// Encode the log entry to raw bytes
						byte[ ] data = EncodeEntries( logEntries );

						// Write the log entry
						if (WriteEntryBytesToFile( data))
						{
							var logWrittenHandler = LogWritten;

							// Validate the current log file and check if any processing is required
							var details = Validate( logWrittenHandler != null );

							logWrittenHandler?.Invoke( this, new LogWrittenEventArgs( _path, logEntries.Count, details.RotateDetails, details.PurgeDetails ) );
						}
					}
				}
			}
			catch
			{
			}
			// ReSharper restore EmptyGeneralCatchClause
		}
             
        /// <summary>
        ///     Write a raw entry to the event log.
        /// </summary>
        /// <param name="data">The data.</param>
        private bool WriteEntryBytesToFile( byte[ ] data )
		{
			try
			{
				if ( data == null || data.Length == 0 )
				{
					// Sanity check
					return false;
				}

				// Attempt to open the file
				FileStream logStream = FileHelper.TryOpenFile( _path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 10000 );
				try
				{
					if ( logStream != null )
					{
						logStream.Seek( 0, SeekOrigin.End );
						logStream.Write( data, 0, data.Length );
						logStream.Flush( );

						return true;
					}
				}
				finally
				{
					logStream?.Dispose( );
				}

				return false;
            }
			catch ( Exception exception )
			{
				Trace.TraceError( "Unable to write an entry to the event log. {0}", exception );
                return false;
			}
		}

		#region Mutex Lock Helper Methods

		/// <summary>
		///     This method gets the name of the mutex based on the log folder.
		/// </summary>
		/// <param name="logFolder">The log folder to get a mutex name for.</param>
		/// <returns>The name of the mutex for this log folder.</returns>
		public static string GetLogMutexName( string logFolder )
		{
			if ( string.IsNullOrEmpty( logFolder ) )
			{
				throw new ArgumentNullException( nameof( logFolder ) );
			}

			return $@"Global\ReadiNowLogMutex_{CryptoHelper.GetMd5Hash( logFolder.ToUpperInvariant( ) )}";
		}

		#endregion

		#region IDisposable Methods

		/// <summary>
		///     Dispose the mutex object.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Dispose the object.
		/// </summary>
		/// <param name="disposing">True if Dispose is called from user code.</param>
		protected virtual void Dispose( bool disposing )
		{
			if ( !_disposed )
			{
				if ( disposing )
				{
					if ( _eventLogWorkerThread != null && _eventLogWorkerThread.IsAlive )
					{
						_shutdownEvent.Set( );

						lock ( _lock )
						{
							Monitor.Pulse( _lock );
						}

						_eventLogWorkerThread.Join( 5000 );

						_shutdownEvent?.Dispose( );
					}

					if ( _eventLogWorkerThread != null && _eventLogWorkerThread.IsAlive )
					{
						_eventLogWorkerThread.Abort( );
						_eventLogWorkerThread = null;
					}
				}

                _disposed = true;

				FlushLog(disposing);

				_syncMutex?.Dispose( );
			}
		}

		#endregion		
	}
}