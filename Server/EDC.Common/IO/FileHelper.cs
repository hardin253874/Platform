// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Threading;

namespace EDC.IO
{
	/// <summary>
	///     Provides helper methods for interacting with databases.
	/// </summary>
	public static class FileHelper
	{
		/// <summary>
		///     Attempts to move the specified file to a new location.
		/// </summary>
		/// <param name="sourcePath">A string containing the name of the file to move.</param>
		/// <param name="destinationPath">A string containing the new path for the file.</param>
		/// <param name="timeout">The timeout.</param>
		/// <returns>
		///     true if the file could be moved; otherwise false.
		/// </returns>
		public static bool TryMoveFile( string sourcePath, string destinationPath, int timeout )
		{
			bool success = false;
			DateTime startTime = DateTime.Now;

			AutoResetEvent activityEvent = null;
			FileSystemWatcher monitor = null;

			while ( true )
			{
				try
				{
					// Attempt to move the file
					File.Move( sourcePath, destinationPath );
					success = true;
				}
				catch ( IOException )
				{
					// Do nothing
				}

				// Check if the move operation failed
				if ( !success )
				{
					// Check if the timeout has been exceeded
					int elapsedTime = Convert.ToInt32( ( DateTime.Now - startTime ).TotalMilliseconds );
					if ( elapsedTime <= timeout )
					{
						if ( activityEvent == null )
						{
							activityEvent = new AutoResetEvent( false );
						}

						if ( monitor == null )
						{
							string directoryName = Path.GetDirectoryName( sourcePath );

							if ( directoryName != null )
							{
								monitor = new FileSystemWatcher( directoryName )
									{
										EnableRaisingEvents = true
									};

								// Wait until there is some activity on the file in question
								AutoResetEvent evt = activityEvent;

								monitor.Changed += ( sender, args ) =>
									{
										// Check if the activity is related to the file in question
										if ( String.Compare( Path.GetFullPath( args.FullPath ), Path.GetFullPath( sourcePath ), StringComparison.OrdinalIgnoreCase ) == 0 )
										{
											evt.Set( );
										}
									};
							}
						}

						// Wait until there is some activity on the file in question (or the timeout is exceeded)
						if ( !activityEvent.WaitOne( timeout - elapsedTime ) )
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			// Check if the file was moved; else attempt to move the file one last time
			if ( !success )
			{
				try
				{
					// Attempt to move the file (one last time)
					File.Move( sourcePath, destinationPath );
					success = true;
				}
				catch ( IOException )
				{
					// Do nothing
				}
			}

			return success;
		}

		/// <summary>
		///     Attempts to open a FileStream on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
		/// </summary>
		/// <param name="path">
		///     A string containing the path of the file to open.
		/// </param>
		/// <param name="mode">
		///     An enumeration that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.
		/// </param>
		/// <param name="access">
		///     An enumeration that specifies the operations that can be performed on the file.
		/// </param>
		/// <param name="share">
		///     An enumeration that specifies the type of access other threads have to the file.
		/// </param>
		/// <param name="timeout">
		///     The number of milliseconds to try to open the file if locked.
		/// </param>
		/// <returns>
		///     A stream on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
		/// </returns>
		public static FileStream TryOpenFile( string path, FileMode mode, FileAccess access, FileShare share, int timeout )
		{
			FileStream stream = null;
			DateTime startTime = DateTime.Now;

			AutoResetEvent activityEvent = null;
			FileSystemWatcher monitor = null;

			while ( true )
			{
				try
				{
					// Attempt to open the file
					stream = File.Open( path, mode, access, share );
				}
				catch ( IOException )
				{
					// Do nothing
				}

				// Check if the open operation failed
				if ( stream == null )
				{
					// Check if the timeout has been exceeded
					int elapsedTime = Convert.ToInt32( ( DateTime.Now - startTime ).TotalMilliseconds );
					if ( elapsedTime <= timeout )
					{
						if ( activityEvent == null )
						{
							activityEvent = new AutoResetEvent( false );
						}

						if ( monitor == null )
						{
							string directoryName = Path.GetDirectoryName( path );

							if ( directoryName != null )
							{
								monitor = new FileSystemWatcher( directoryName )
									{
										EnableRaisingEvents = true
									};

								// Wait until there is some activity on the file in question
								AutoResetEvent evt = activityEvent;
								monitor.Changed += ( sender, args ) =>
									{
										// Check if the activity is related to the file in question
										if ( String.Compare( Path.GetFullPath( args.FullPath ), Path.GetFullPath( path ), StringComparison.OrdinalIgnoreCase ) == 0 )
										{
											evt.Set( );
										}
									};
							}
						}

						// Wait until there is some activity on the file in question (or the timeout is exceeded)
						if ( !activityEvent.WaitOne( timeout - elapsedTime ) )
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			// Check if the file was opened; else attempt to open the file one last time)
			if ( stream == null )
			{
				try
				{
					// Attempt to open the file (one last time)
					stream = File.Open( path, mode, access, share );
				}
				catch ( IOException )
				{
					// Do nothing
				}
			}

			return stream;
		}


        /// <summary>
        ///     Get the extension for a file, but if the file proves problematic, return an empty extension.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string GetFileExtension(string fileName)
        {
            // safely pull out an extension
            try
            {
                return Path.GetExtension(fileName);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}