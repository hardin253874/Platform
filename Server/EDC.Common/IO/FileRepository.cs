// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EDC.Diagnostics;

namespace EDC.IO
{
    /// <summary>
    ///     Implements a file repository.
    /// </summary>
    public class FileRepository : IFileRepository
    {
        /// <summary>
        ///     The extension for the actual data files.
        /// </summary>
        private const string DataFileExtension = ".dat";


        /// <summary>
        ///     The extension for temp files.
        /// </summary>
        private const string TempFileExtension = ".tmp";


        /// <summary>
        ///     The temp directory name.
        /// </summary>
        private const string TempDirectory = "_Temp_";


        /// <summary>
        ///     The maximum retry count for failed IO operations.
        /// </summary>
        private const int MaxRetryCount = 5;


        /// <summary>
        ///     The frequency at which to cleanup the repository.
        /// </summary>
        private readonly TimeSpan _cleanupInterval;


        /// <summary>
        ///     The event log.
        /// </summary>
        private readonly IEventLog _eventLog;


        /// <summary>
        ///     The file repository path.
        /// </summary>
        private readonly string _repositoryPath;


        /// <summary>
        ///     The retention period for the repository.
        /// </summary>
        private readonly TimeSpan _retentionPeriod;


        /// <summary>
        ///     The syncroot for this class.
        /// </summary>
        private readonly object _syncRoot = new object();


        /// <summary>
        ///     The path to the temp directory.
        /// </summary>
        private readonly string _tempDirectoryPath;


        /// <summary>
        ///     The token provider.
        /// </summary>
        private readonly IFileTokenProvider _tokenProvider;


        /// <summary>
        ///     The time the repository was last cleaned.
        /// </summary>
        private DateTime _lastCleanupTimeUtc = DateTime.MinValue;

		/// <summary>
		/// Occurs after files have been removed.
		/// </summary>
		public event EventHandler<FilesRemovedEventArgs> FilesRemoved;

		/// <summary>
		/// Occurs when a cleanup task has completed.
		/// </summary>
		public event EventHandler CleanupComplete;


		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="name">The name of the repository.</param>
		/// <param name="path">The path of the repository,</param>
		/// <param name="tokenProvider">The token provider.</param>
		/// <param name="eventLog">The event log.</param>
		/// <param name="retentionPeriod">The retention period for the repository</param>
		public FileRepository(string name, string path, IFileTokenProvider tokenProvider, IEventLog eventLog, TimeSpan retentionPeriod) : this(name, path, tokenProvider, eventLog, retentionPeriod, TimeSpan.FromHours(12))
        {
        }


        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="name">The name of the repository.</param>
        /// <param name="path">The path of the repository,</param>
        /// <param name="tokenProvider">The token provider.</param>
        /// <param name="eventLog">The event log.</param>
        /// <param name="cleanupInterval">Cleanup interval for the repository.</param>
        /// <param name="retentionPeriod">The retention period for the repository</param>
        public FileRepository(string name, string path, IFileTokenProvider tokenProvider, IEventLog eventLog, TimeSpan retentionPeriod, TimeSpan cleanupInterval)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof( name ) );
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException( nameof( path ) );
            }

            if (tokenProvider == null)
            {
                throw new ArgumentNullException( nameof( tokenProvider ) );
            }

            if (eventLog == null)
            {
                throw new ArgumentNullException( nameof( eventLog ) );
            }

            if (retentionPeriod.TotalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException( nameof( retentionPeriod ) );
            }

            if (cleanupInterval.TotalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException( nameof( cleanupInterval ) );
            }

            // Try and create the file repository path if it does not exist                        
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            eventLog.WriteInformation("Creating file repository {0} at {1}.", name, path);

            Name = name;

            _repositoryPath = path.TrimEnd(Path.DirectorySeparatorChar);
            _tokenProvider = tokenProvider;
            _eventLog = eventLog;
            _cleanupInterval = cleanupInterval;
            _retentionPeriod = retentionPeriod;

            // Get the temp directory path
            _tempDirectoryPath = Path.Combine(path, TempDirectory);
            Directory.CreateDirectory(_tempDirectoryPath);
        }


        /// <summary>
        ///     The file repository name.
        /// </summary>
        public string Name { get; set; }


        #region IFileRepository Members


        /// <summary>
        ///     Get a file with the given token from the repository.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Stream Get(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException( nameof( token ) );
            }

            var directoryPath = GetDataDirectoryPath(token);
            var dataFilePath = GetDataFilePath(token, directoryPath);

            if (!File.Exists(dataFilePath))
            {
                throw new FileNotFoundException( $@"The file with token {token} does not exist in repository {Name}." );
            }

            return File.OpenRead(dataFilePath);
        }


        /// <summary>
        ///     Deletes a file with the given token from the repository.
        /// </summary>
        /// <param name="token"></param>
        public void Delete(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException( nameof( token ) );
            }

            var directoryPath = GetDataDirectoryPath(token);
            var dataFilePath = GetDataFilePath(token, directoryPath);

            DeleteFile(dataFilePath, false);

            DeleteEmptyDirectoriesUpToRepositoryRoot(new DirectoryInfo(directoryPath));
        }


        /// <summary>
        ///     Puts a file into the repository.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public string Put(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException( nameof( stream ) );
            }

            RunFileCleanup();

            var token = ComputeToken(stream);

            var directoryPath = GetDataDirectoryPath(token);
            var dataFilePath = GetDataFilePath(token, directoryPath);

            if (File.Exists(dataFilePath))
            {
                // The file already exists, we return.
                return token;
            }

            Write(stream, directoryPath, dataFilePath);

            return token;
        }


        /// <summary>
        ///     Gets all the tokens in the repository.
        /// </summary>
        /// <returns>All the tokens in the repository.</returns>
        public IEnumerable<string> GetTokens()
        {
            if (!Directory.Exists(_repositoryPath))
            {
                yield break;
            }

            var directoryInfo = new DirectoryInfo(_repositoryPath);

            foreach (var fileInfo in directoryInfo.EnumerateFiles("*" + DataFileExtension, SearchOption.AllDirectories))
            {
                var token = Path.GetFileNameWithoutExtension(fileInfo.Name);
                yield return token.ToUpperInvariant();
            }
        }


        #endregion


        /// <summary>
        ///     Runs the file cleanup as a background task.
        /// </summary>
        private void RunFileCleanup()
        {
            try
            {
                if (!ShouldRunFileCleanup())
                {
                    return;
                }

                lock (_syncRoot)
                {
                    if (!ShouldRunFileCleanup())
                    {
                        return;
                    }

                    _lastCleanupTimeUtc = DateTime.UtcNow;
                }

                Task.Run(() =>
                {
                    _eventLog.WriteInformation("Cleanup for file repository {0} starting", Name);

	                var oldFiles = CleanupOldTempFiles();
	                var staleFiles = CleanupFilesExceedingRetentionPeriod();

					var filesRemoved = FilesRemoved;

					if ( filesRemoved != null && ( ( oldFiles != null && oldFiles.Count > 0 ) || ( staleFiles != null && staleFiles.Count > 0 ) ) )
					{
						FilesRemovedEventArgs args = new FilesRemovedEventArgs( oldFiles, staleFiles );

						filesRemoved( this, args );
					}

	                _eventLog.WriteInformation("Cleanup for file repository {0} completed.", Name);
                }).ContinueWith( t =>
                {
	                var cleanupComplete = CleanupComplete;

	                cleanupComplete?.Invoke( this, new EventArgs( ) );
                });
            }
            catch (Exception exception)
            {
                _eventLog.WriteError("An error occured running the temp file cleanup for file repository {0}. Error {1}.", Name, exception);
            }
        }


        /// <summary>
        ///     Returns true if a cleanup needs to be run, false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool ShouldRunFileCleanup()
        {
            return (DateTime.UtcNow - _lastCleanupTimeUtc >= _cleanupInterval) || (_lastCleanupTimeUtc == DateTime.MinValue);
        }


        /// <summary>
        ///     Cleans up files older than retention period.
        /// </summary>
        private List<string> CleanupFilesExceedingRetentionPeriod()
        {
            if (_retentionPeriod == TimeSpan.MaxValue || _retentionPeriod.TotalMilliseconds <= 0)
            {
                return null;
            }

            if (!Directory.Exists(_repositoryPath))
            {
                return null;
            }

            var directoryInfo = new DirectoryInfo(_repositoryPath);

			List<string> results = new List<string>( );

			foreach (var fileInfo in directoryInfo.EnumerateFiles("*" + DataFileExtension, SearchOption.AllDirectories))
            {
                try
                {
                    // Delete any files older than retention period                    
                    if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < _retentionPeriod)
                    {
                        continue;
                    }

	                results.Add( fileInfo.FullName );

                    fileInfo.Delete();

                    DeleteEmptyDirectoriesUpToRepositoryRoot(fileInfo.Directory);
                }
                catch
                {
                    // Ignore
                }
            }

	        return results;
        }


        /// <summary>
        ///     Cleans up old temp files.
        /// </summary>
        private List<string> CleanupOldTempFiles()
        {
            if (!Directory.Exists(_tempDirectoryPath))
            {
                return null;
            }

            var tempDirInfo = new DirectoryInfo(_tempDirectoryPath);

	        List<string> results = new List<string>( );

            foreach (var fileInfo in tempDirInfo.EnumerateFiles("*" + TempFileExtension))
            {
                try
                {
                    // Delete any files older than 1 day
                    // The presence of a temp file indicates a problem occurred moving the file
                    // during a put.
                    if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < TimeSpan.FromDays(1))
                    {
                        continue;
                    }

	                results.Add( fileInfo.FullName );
	                
                    fileInfo.Delete();
                }
                catch
                {
                    // Ignore
                }
            }

	        return results;
        }


        /// <summary>
        ///     Deletes the specified file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="suppressErrors"></param>
        private void DeleteFile(string filePath, bool suppressErrors)
        {
            var retryCount = 0;

            while (retryCount < MaxRetryCount)
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        return;
                    }

                    File.Delete(filePath);

                    _eventLog.WriteTrace("Deleted file {0} from repository {1}.", filePath, Name);

                    break;
                }
                catch (Exception exception)
                {
                    retryCount++;

                    if (retryCount >= MaxRetryCount)
                    {
                        if (suppressErrors)
                        {
                            _eventLog.WriteTrace("An error occured deleting file {0} from file repository at {1}. Error {2}.", filePath, Name, exception);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }


        /// <summary>
        ///     Computes the token for the specified stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string ComputeToken(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException( nameof( stream ) );
            }

            return _tokenProvider.ComputeToken(stream);
        }


        /// <summary>
        ///     Tries to create the specified directory.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="retryCount"></param>
        private bool TryCreateDirectory(string path, ref int retryCount)
        {
            try
            {
                // Ensure data directory exists
                Directory.CreateDirectory(path);
                return true;
            }
            catch (IOException)
            {
                retryCount++;

                if (retryCount >= MaxRetryCount)
                {
                    throw;
                }

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                retryCount++;

                if (retryCount >= MaxRetryCount)
                {
                    throw;
                }

                return false;
            }
        }


        /// <summary>
        ///     Tries to move the specified file.
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <param name="retryCount"></param>
        private bool TryMoveFile(string sourceFileName, string destFileName, ref int retryCount)
        {
            try
            {                
                File.Move(sourceFileName, destFileName);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                retryCount++;

                if (retryCount >= MaxRetryCount)
                {
                    throw;
                }

                return false;
            }
            catch (DirectoryNotFoundException)
            {
                // Target directory doesn't exist try again            
                retryCount++;

                if (retryCount >= MaxRetryCount)
                {
                    throw;
                }

                return false;
            }
            catch (IOException)
            {
                // Ignore. The file already exists                
                return true;
            }
        }


        /// <summary>
        ///     Writes the given source file to the given destination.
        /// </summary>
        /// <param name="sourceFileStream"></param>
        /// <param name="destinationDirectoryPath"></param>
        /// <param name="destinationFilePath"></param>
        private void Write(Stream sourceFileStream, string destinationDirectoryPath, string destinationFilePath)
        {
            // Ensure temp directory exists
            Directory.CreateDirectory(_tempDirectoryPath);

            var tempDataFilePath = GetTempDataFilePath();

            try
            {
                if (sourceFileStream.CanSeek)
                {
                    sourceFileStream.Position = 0;
                }

                // Write the file to the temp file                
                using (var tempFileStream = File.Open(tempDataFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    sourceFileStream.CopyTo(tempFileStream);
                }

                // Now that changes are on disk move the file as an atomic operation
                if (!File.Exists(destinationFilePath))
                {
                    var retryCount = 0;

                    while (retryCount < MaxRetryCount)
                    {
                        // Ensure data directory exists
                        if (!TryCreateDirectory(destinationDirectoryPath, ref retryCount))
                        {
                            continue;
                        }

                        if (TryMoveFile(tempDataFilePath, destinationFilePath, ref retryCount))
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                // Delete the temp file
                DeleteFile(tempDataFilePath, true);
            }
        }


        /// <summary>
        ///     Get the path to the data file.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private string GetDataFilePath(string token, string directoryPath)
        {
            return Path.Combine(directoryPath, $"{token}{DataFileExtension}" );
        }


        /// <summary>
        ///     Get the path to a temp file.
        /// </summary>
        /// <returns></returns>
        private string GetTempDataFilePath()
        {
            return Path.Combine(_tempDirectoryPath, $"{Guid.NewGuid( ):N}{TempFileExtension}" );
        }


        /// <summary>
        ///     Get the directory for a given data file.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private string GetDataDirectoryPath(string token)
        {
            const int subDirectoryPathLength = 8;
            const int subDirectoryCount = 2;

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException( nameof( token ) );
            }

            var start = 0;
            var countSubDirectories = 0;
            var paths = new List<string> {_repositoryPath};

            while (start < token.Length &&
                   countSubDirectories < subDirectoryCount)
            {
                var length = subDirectoryPathLength;
                if (token.Length - start < subDirectoryPathLength)
                {
                    length = token.Length - start;
                }
                paths.Add(token.Substring(start, length));

                start += length;
                countSubDirectories++;
            }

            return Path.Combine(paths.ToArray());
        }


        /// <summary>
        ///     Delete empty directories up to the repository root.
        /// </summary>
        /// <param name="directoryInfo"></param>
        private void DeleteEmptyDirectoriesUpToRepositoryRoot(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                return;
            }

            // Sanity check
            var directoryInfoFullPath = directoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar);
            var rootRepoPath = _repositoryPath.TrimEnd(Path.DirectorySeparatorChar);

            if (string.Compare(rootRepoPath, directoryInfoFullPath, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Stop at the root of the repository
                return;
            }

            if (!directoryInfo.Exists || directoryInfo.EnumerateFileSystemInfos().Any()) return;

            try
            {
                directoryInfo.Delete();
                DeleteEmptyDirectoriesUpToRepositoryRoot(directoryInfo.Parent);
            }
            catch (IOException)
            {
                // Ignore. Directory not empty                    
            }
            catch (Exception ex)
            {
                _eventLog.WriteError("An error occurred deleting the directory {0}. Exception {1}.", directoryInfo.FullName, ex);
            }
        }
    }
}