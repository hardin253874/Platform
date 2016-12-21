// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EDC.Diagnostics;
using EDC.IO;
using Moq;
using NUnit.Framework;

namespace EDC.Test.IO
{
    [TestFixture]
    public class FileRepositoryTests
    {
        /// <summary>
        ///     The extension for temp files.
        /// </summary>
        private const string TempFileExtension = ".tmp";


        /// <summary>
        ///     The temp directory name.
        /// </summary>
        private const string TempDirectory = "_Temp_";


        /// <summary>
        /// </summary>
        private const string DataFileExtension = ".dat";


        [Test]
        public void CleanupTempFilesNoCleanupTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFileName = "Test.txt";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var sourceFilePath = Path.Combine(sourceDirPath, sourceFileName);
                File.WriteAllText(sourceFilePath, @"Test");

                // Create a repo with a cleanup interval of 1 day
                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                // Put a file into the repository.
                // This should trigger a cleanup
                // and set the last cleanup time
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    repository.Put(stream);
                }

                // Add some files into the temp directory
                var tempDirectoryPath = Path.Combine(repoPath, TempDirectory);

                // None of these should be deleted because the cleanup should not run
                var tempFileToBeDeleted1 = Path.Combine(tempDirectoryPath, "TempFileToDelete1" + TempFileExtension);
                var tempFileToNotBeDeleted1 = Path.Combine(tempDirectoryPath, "TempFileToNotDelete1" + TempFileExtension);
                var tempFileToBeDeleted2 = Path.Combine(tempDirectoryPath, "TempFileToDelete2" + TempFileExtension);
                var tempFileToNotBeDeleted2 = Path.Combine(tempDirectoryPath, "TempFileToNotDelete2" + TempFileExtension);

                File.WriteAllText(tempFileToBeDeleted1, "");
                // Set this to be older than 1 day
                File.SetLastWriteTimeUtc(tempFileToBeDeleted1, DateTime.UtcNow.AddDays(-10));
                File.WriteAllText(tempFileToNotBeDeleted1, "");

                File.WriteAllText(tempFileToBeDeleted2, "");
                // Set this to be older than 1 day
                File.SetLastWriteTimeUtc(tempFileToBeDeleted2, DateTime.UtcNow.AddDays(-10));
                File.WriteAllText(tempFileToNotBeDeleted2, "");

                // Put a file into the repository.
                // This should trigger a cleanup
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    repository.Put(stream);
                }

                var success = File.Exists(tempFileToBeDeleted1) &&
                            File.Exists(tempFileToBeDeleted2) &&
                            File.Exists(tempFileToNotBeDeleted1) &&
                            File.Exists(tempFileToNotBeDeleted2);
                    
                Assert.IsTrue(success, "The repository cleanup failed");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void CleanupTempFilesTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFileName = "Test.txt";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var sourceFilePath = Path.Combine(sourceDirPath, sourceFileName);
                File.WriteAllText(sourceFilePath, @"Test");

                // Create a repo with a cleanup interval of 50 milliseconds
                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromMilliseconds(50));

                // Add some files into the temp directory
                var tempDirectoryPath = Path.Combine(repoPath, TempDirectory);

                var tempFileToBeDeleted1 = Path.Combine(tempDirectoryPath, "TempFileToDelete1" + TempFileExtension);
                var tempFileToNotBeDeleted1 = Path.Combine(tempDirectoryPath, "TempFileToNotDelete1" + TempFileExtension);
                var tempFileToBeDeleted2 = Path.Combine(tempDirectoryPath, "TempFileToDelete2" + TempFileExtension);
                var tempFileToNotBeDeleted2 = Path.Combine(tempDirectoryPath, "TempFileToNotDelete2" + TempFileExtension);

                File.WriteAllText(tempFileToBeDeleted1, "");
                // Set this to be older than 1 day
                File.SetLastWriteTimeUtc(tempFileToBeDeleted1, DateTime.UtcNow.AddDays(-10));
                File.WriteAllText(tempFileToNotBeDeleted1, "");

                File.WriteAllText(tempFileToBeDeleted2, "");
                // Set this to be older than 1 day
                File.SetLastWriteTimeUtc(tempFileToBeDeleted2, DateTime.UtcNow.AddDays(-10));
                File.WriteAllText(tempFileToNotBeDeleted2, "");

                // Put a file into the repository.
                // This should trigger a cleanup
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    repository.Put(stream);
                }                

                var start = DateTime.Now;
                var success = false;

                while (!success)
                {
                    // Check that the expected temp files have been deleted
                    success = !File.Exists(tempFileToBeDeleted1) &&
                              !File.Exists(tempFileToBeDeleted2) &&
                              File.Exists(tempFileToNotBeDeleted1) &&
                              File.Exists(tempFileToNotBeDeleted2);

                    var diff = DateTime.Now - start;
                    if (diff.TotalSeconds > 10)
                    {
                        // Give the cleanup up to 30 seconds to complete
                        break;
                    }
                }

                Assert.IsTrue(success, "The repository cleanup failed");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }

        [Test]
        public void CleanupExpiredFilesTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFileName = "Test{0}.txt";            

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                for (int i = 0; i < 5; i++)
                {
                    var sourceFilePath = Path.Combine(sourceDirPath, string.Format(sourceFileName, i));
                    File.WriteAllText(sourceFilePath, @"Test" + i);
                }

				using ( AutoResetEvent evt = new AutoResetEvent( false ) )
				{
					// Create a repo with a retention period of 100 milliseconds and a cleanup interval of 50 milliseconds
					var repository = CreateTestRepository( repoPath, true, TimeSpan.FromMilliseconds( 100 ), TimeSpan.FromMilliseconds( 5 ) );

					EventHandler<FilesRemovedEventArgs> filesRemovedHandler = ( sender, args ) =>
					{
						// ReSharper disable once AccessToDisposedClosure
						evt.Set( );
					};

					repository.FilesRemoved += filesRemovedHandler;

					Dictionary<int, string> tokensMap = new Dictionary<int, string>( );

					for ( int i = 0; i < 3; i++ )
					{
						// Put 3 files into the repository.                    
						using ( var stream = File.OpenRead( Path.Combine( sourceDirPath, string.Format( sourceFileName, i ) ) ) )
						{
							tokensMap [ i ] = repository.Put( stream );
						}
					}

					Task.Delay( 150 ).Wait( );

					using ( var stream = File.OpenRead( Path.Combine( sourceDirPath, string.Format( sourceFileName, 3 ) ) ) )
					{
						repository.Put( stream );
					}

					evt.WaitOne( 100 );

					// At this point token3 should only exist
					var tokensSet = new HashSet<string>( repository.GetTokens( ).ToList( ) );

					foreach ( var token in tokensMap.Values )
					{
						Assert.IsFalse( tokensSet.Contains( token ), "The token {0} should no longer exist", token );
					}

					// Put one more file to trigger a cleanup
					using ( var stream = File.OpenRead( Path.Combine( sourceDirPath, string.Format( sourceFileName, 4 ) ) ) )
					{
						repository.Put( stream );
					}

					// Wait for the cleanup to happen
					evt.WaitOne( 100 );

					// At this point tokens1 - 3 should not exist
					tokensSet = new HashSet<string>( repository.GetTokens( ).ToList( ) );

					foreach ( var token in tokensMap.Values )
					{
						Assert.IsFalse( tokensSet.Contains( token ), "The token {0} should not exist", token );
					}

					repository.FilesRemoved -= filesRemovedHandler;
				}
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void DeleteFailsTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFilePath = Path.Combine(sourceDirPath, "Test1.txt");
            var contents = "This is a test !";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                File.WriteAllText(sourceFilePath, contents);

                string token;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token = repository.Put(stream);
                }

                Assert.IsNotNullOrEmpty(token, "The token is invalid.");

                var repoFilePath = FindFilePathByName(repoPath, token + DataFileExtension);

                // Open the file for writing so that it is locked
                using (File.OpenWrite(repoFilePath))
                {
                    Assert.Throws<IOException>(() => repository.Delete(token), "Deleting the file should fail.");
                }

                Assert.AreEqual(1, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void DeleteInvalidTokenTest()
        {
            var repoPath = GetTempDirectoryPath();
            FileRepository repository;

            try
            {
                Directory.CreateDirectory(repoPath);
                repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                Assert.Throws<ArgumentNullException>(() => repository.Delete(null));
            }
            finally
            {
                DeleteDirectory(repoPath);
            }
        }


        [Test]
        public void DeleteUnknownTokenTest()
        {
            var repoPath = GetTempDirectoryPath();
            FileRepository repository;

            try
            {
                Directory.CreateDirectory(repoPath);
                repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                Assert.DoesNotThrow(() => repository.Delete("XXX"));
            }
            finally
            {
                DeleteDirectory(repoPath);
            }
        }


        [Test]
        public void GetInvalidTokenTest()
        {
            var repoPath = GetTempDirectoryPath();
            FileRepository repository;

            try
            {
                Directory.CreateDirectory(repoPath);
                repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                Assert.Throws<ArgumentNullException>(() => repository.Get(null));
                Assert.Throws<FileNotFoundException>(() => repository.Get("X"));
            }
            finally
            {
                DeleteDirectory(repoPath);
            }
        }


        [Test]
        public void GetMissingFileTest()
        {
            var repoPath = GetTempDirectoryPath();
            FileRepository repository;

            try
            {
                Directory.CreateDirectory(repoPath);
                repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                Assert.Throws<FileNotFoundException>(() => repository.Get("0000000000111111111122222222223333333333444444444455555555556666"));
            }
            finally
            {
                DeleteDirectory(repoPath);
            }
        }


        [Test]
        public void InvalidConstructorParamTest()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var eventLogMock = mockRepo.Create<IEventLog>();
            var repoPath = GetTempDirectoryPath();
            var tokenProvider = new RandomFileTokenProvider();

            try
            {
                Assert.That(() => new FileRepository(null, repoPath, tokenProvider, eventLogMock.Object, TimeSpan.MaxValue),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));

                Assert.That(() => new FileRepository("TestRepo", null, tokenProvider, eventLogMock.Object, TimeSpan.MaxValue),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("path"));

                Assert.That(() => new FileRepository("TestRepo", repoPath, null, eventLogMock.Object, TimeSpan.MaxValue),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("tokenProvider"));

                Assert.That(() => new FileRepository("TestRepo", @"XYZ:\?", tokenProvider, eventLogMock.Object, TimeSpan.MaxValue),
                    Throws.TypeOf<ArgumentException>());

                Assert.That(() => new FileRepository("TestRepo", repoPath, tokenProvider, null, TimeSpan.MaxValue),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("eventLog"));

                Assert.That(() => new FileRepository("TestRepo", repoPath, tokenProvider, eventLogMock.Object, new TimeSpan()),
                    Throws.TypeOf<ArgumentOutOfRangeException>().And.Property("ParamName").EqualTo("retentionPeriod"));

                Assert.That(() => new FileRepository("TestRepo", repoPath, tokenProvider, eventLogMock.Object, TimeSpan.MaxValue, new TimeSpan()),
                    Throws.TypeOf<ArgumentOutOfRangeException>().And.Property("ParamName").EqualTo("cleanupInterval"));
            }
            finally
            {
                DeleteDirectory(repoPath);
            }
        }


        [Test]
        public void MultipleDeletesTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFilePath = Path.Combine(sourceDirPath, "Test1.txt");
            var contents = "This is a test !";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                File.WriteAllText(sourceFilePath, contents);

                string token;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token = repository.Put(stream);
                }

                Assert.IsNotNullOrEmpty(token, "The token is invalid.");

                Assert.AreEqual(1, CountFiles(repoPath, "*" + DataFileExtension), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");

                repository.Delete(token);
                repository.Delete(token);
                repository.Delete(token);
                repository.Delete(token);

                Assert.AreEqual(0, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void MultiThreadedDuplicatePutTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFileName = "Test.txt";
            var tasks = new List<Task>();
            var tokens = new ConcurrentDictionary<string, string>();

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var sourceFilePath = Path.Combine(sourceDirPath, sourceFileName);
                File.WriteAllText(sourceFilePath, @"Test Data");
                var startEvent = new ManualResetEvent(false);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                var taskFactory = new TaskFactory();

                for (var i = 0; i < 10; i++)
                {
                    var task = taskFactory.StartNew(() =>
                    {
                        startEvent.WaitOne();

                        string token;
                        using (var stream = File.OpenRead(sourceFilePath))
                        {
                            token = repository.Put(stream);
                        }

                        tokens[token] = token;
                        Assert.IsNotNullOrEmpty(token, "The token is invalid.");
                    });

                    tasks.Add(task);
                }

                startEvent.Set();

                // Wait for all tasks to complete
                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(1, tokens.Count, "The token count is invalid");
                Assert.AreEqual(1, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");

                using (var stream = repository.Get(tokens.First().Key))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var repoContents = sr.ReadToEnd();
                        Assert.AreEqual(File.ReadAllText(sourceFilePath), repoContents, "The file's contents are not valid.");
                    }
                }
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void MultiThreadedPutGetDeleteTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFileName = "Test.txt";
            var tasks = new List<Task>();
            var tokens = new ConcurrentDictionary<string, string>();

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var sourceFilePath = Path.Combine(sourceDirPath, sourceFileName);
                File.WriteAllText(sourceFilePath, @"Test Data");

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                string tokenGlobal;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    tokenGlobal = repository.Put(stream);
                }
                repository.Delete(tokenGlobal);

                var taskFactory = new TaskFactory();
                var startEvent = new ManualResetEvent(false);

                // Deleters
                for (var t = 0; t < 10; t++)
                {
                    var task = taskFactory.StartNew(() =>
                    {
                        startEvent.WaitOne();

                        for (var i = 0; i < 100; i++)
                        {
                            try
                            {
                                repository.Delete(tokenGlobal);
                            }
                            catch (Exception)
                            {
                                // Ignore delete errors
                            }
                        }
                    });

                    tasks.Add(task);
                }

                // Writers
                for (var t = 0; t < 10; t++)
                {
                    var task = taskFactory.StartNew(() =>
                    {
                        startEvent.WaitOne();

                        for (var i = 0; i < 100; i++)
                        {
                            using (var stream = File.OpenRead(sourceFilePath))
                            {
                                try
                                {
                                    var token = repository.Put(stream);
                                    tokens[token] = token;

                                    Assert.IsNotNullOrEmpty(token, "The token is invalid.");
                                }
                                catch
                                {
                                    // Ignore    
                                }                                
                            }
                        }
                    });

                    tasks.Add(task);
                }

                // Getters
                for (var t = 0; t < 10; t++)
                {
                    var task = taskFactory.StartNew(() =>
                    {
                        startEvent.WaitOne();

                        for (var i = 0; i < 100; i++)
                        {
                            try
                            {
                                using (var stream = repository.Get(tokenGlobal))
                                {
                                    using (var sr = new StreamReader(stream))
                                    {
                                        var repoContents = sr.ReadToEnd();
                                        Assert.AreEqual(File.ReadAllText(sourceFilePath), repoContents, "The file's contents are not valid.");
                                    }
                                }
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                    });

                    tasks.Add(task);
                }

                startEvent.Set();

                // Wait for all tasks to complete
                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(1, tokens.Count, "The token count is invalid");
                var countFiles = CountFiles(repoPath, GetDataFilePattern());
                Assert.IsTrue(countFiles == 0 || countFiles == 1, "The number of data files is invalid");
                if (countFiles == 1)
                {
                    using (var stream = repository.Get(tokens.First().Key))
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            var repoContents = sr.ReadToEnd();
                            Assert.AreEqual(File.ReadAllText(sourceFilePath), repoContents, "The file's contents are not valid.");
                        }
                    }
                }
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void MultiThreadedUniquePutTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var contentTemplate = "This is a test. {0}.";
            var sourceFileNameTemplate = "Test{0}.txt";
            var sourceFileToToken = new ConcurrentDictionary<string, string>();
            var tasks = new List<Task>();

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                var taskFactory = new TaskFactory();

                for (var i = 0; i < 10; i++)
                {
                    var id = i;
                    var task = taskFactory.StartNew(() =>
                    {
                        var contents = string.Format(contentTemplate, id);
                        var sourceFilePath = Path.Combine(sourceDirPath, string.Format(sourceFileNameTemplate, id));
                        File.WriteAllText(sourceFilePath, contents);

                        string token;
                        using (var stream = File.OpenRead(sourceFilePath))
                        {
                            token = repository.Put(stream);
                        }

                        Assert.IsNotNullOrEmpty(token, "The token is invalid.");

                        sourceFileToToken[sourceFilePath] = token;
                    });

                    tasks.Add(task);
                }

                // Wait for all tasks to complete
                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(10, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");

                foreach (var kvp in sourceFileToToken)
                {
                    using (var stream = repository.Get(kvp.Value))
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            var repoContents = sr.ReadToEnd();
                            Assert.AreEqual(File.ReadAllText(kvp.Key), repoContents, "The file's contents are not valid.");
                        }
                    }

                    repository.Delete(kvp.Value);
                }

                Assert.AreEqual(0, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void PutInvalidStreamTest()
        {
            var repoPath = GetTempDirectoryPath();
            FileRepository repository;

            try
            {
                Directory.CreateDirectory(repoPath);
                repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                Assert.Throws<ArgumentNullException>(() => repository.Put(null));
            }
            finally
            {
                DeleteDirectory(repoPath);
            }
        }


        [Test]
        public void SingleThreadedMultiPutGetTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFilePath = Path.Combine(sourceDirPath, "Test1.txt");
            var contents = "This is a test !";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                File.WriteAllText(sourceFilePath, contents);

                string token1;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token1 = repository.Put(stream);
                }

                string token2;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token2 = repository.Put(stream);
                }

                Assert.IsNotNullOrEmpty(token1, "The token1 is invalid.");
                Assert.IsNotNullOrEmpty(token2, "The token2 is invalid.");
                Assert.AreEqual(token1, token2, "The tokens should match");

                Assert.AreEqual(1, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");

                using (var stream = repository.Get(token1))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var repoContents = sr.ReadToEnd();
                        Assert.AreEqual(contents, repoContents, "The file's contents are not valid.");
                    }
                }
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void SingleThreadedPutGetDeleteTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFilePath = Path.Combine(sourceDirPath, "Test1.txt");
            var contents = "This is a test !";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                File.WriteAllText(sourceFilePath, contents);

                string token;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token = repository.Put(stream);
                }

                Assert.IsNotNullOrEmpty(token, "The token is invalid.");

                Assert.AreEqual(1, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");

                using (var stream = repository.Get(token))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var repoContents = sr.ReadToEnd();
                        Assert.AreEqual(contents, repoContents, "The file's contents are not valid.");
                    }
                }

                repository.Delete(token);

                Assert.AreEqual(0, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void SingleThreadedPutGetDeleteDuplicatesTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFilePath = Path.Combine(sourceDirPath, "Test1.txt");
            var contents = "This is a test !";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, true, TimeSpan.MaxValue, TimeSpan.FromDays(1));
                File.WriteAllText(sourceFilePath, contents);

                string token1;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token1 = repository.Put(stream);
                }

                string token2;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token2 = repository.Put(stream);
                }

                Assert.IsNotNullOrEmpty(token1, "The token is invalid.");
                Assert.IsNotNullOrEmpty(token2, "The token is invalid.");
                Assert.AreNotEqual(token1, token2, "The tokens should not match");

                Assert.AreEqual(2, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");

                using (var stream = repository.Get(token1))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var repoContents = sr.ReadToEnd();
                        Assert.AreEqual(contents, repoContents, "The file's contents are not valid.");
                    }
                }

                using (var stream = repository.Get(token2))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var repoContents = sr.ReadToEnd();
                        Assert.AreEqual(contents, repoContents, "The file's contents are not valid.");
                    }
                }

                repository.Delete(token1);
                repository.Delete(token2);

                Assert.AreEqual(0, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void SingleThreadedPutGetTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFilePath = Path.Combine(sourceDirPath, "Test1.txt");
            var contents = "This is a test !";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                File.WriteAllText(sourceFilePath, contents);

                string token;
                using (var stream = File.OpenRead(sourceFilePath))
                {
                    token = repository.Put(stream);
                }

                Assert.IsNotNullOrEmpty(token, "The token is invalid.");

                Assert.AreEqual(1, CountFiles(repoPath, GetDataFilePattern()), "The number of data files is invalid");
                Assert.AreEqual(0, CountFiles(repoPath, GetTempFilePattern()), "The number of temp files is invalid");

                using (var stream = repository.Get(token))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var repoContents = sr.ReadToEnd();
                        Assert.AreEqual(contents, repoContents, "The file's contents are not valid.");
                    }
                }
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void GetTokensTest()
        {
            var repoPath = GetTempDirectoryPath();
            var sourceDirPath = GetTempDirectoryPath();
            var sourceFilePath1 = Path.Combine(sourceDirPath, "Test1.txt");
            var sourceFilePath2 = Path.Combine(sourceDirPath, "Test2.txt");

            var contents1 = "This is a test1 !";
            var contents2 = "This is a test2 !";

            try
            {
                Directory.CreateDirectory(repoPath);
                Directory.CreateDirectory(sourceDirPath);

                var repository = CreateTestRepository(repoPath, false, TimeSpan.MaxValue, TimeSpan.FromDays(1));

                var tokens = repository.GetTokens().ToList();

                Assert.AreEqual(0, tokens.Count, "The number of tokens should be 0");

                File.WriteAllText(sourceFilePath1, contents1);
                File.WriteAllText(sourceFilePath2, contents2);

                string token1;
                using (var stream = File.OpenRead(sourceFilePath1))
                {
                    token1 = repository.Put(stream);
                }

                string token2;
                using (var stream = File.OpenRead(sourceFilePath2))
                {
                    token2 = repository.Put(stream);
                }

                tokens = repository.GetTokens().ToList();

                Assert.AreEqual(2, tokens.Count, "The number of tokens should be 2");
                Assert.IsTrue(tokens.Contains(token1), "token1 was not found");
                Assert.IsTrue(tokens.Contains(token2), "token2 was not found");
            }
            finally
            {
                DeleteDirectory(repoPath);
                DeleteDirectory(sourceDirPath);
            }
        }


        [Test]
        public void ValidConstructorParamTest()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var eventLogMock = mockRepo.Create<IEventLog>();

            // ReSharper disable once ObjectCreationAsStatement
            Assert.DoesNotThrow(() => new FileRepository("TestRepo", @"C:\", new RandomFileTokenProvider(), eventLogMock.Object, TimeSpan.FromDays(1)));
            Assert.DoesNotThrow(() => new FileRepository("TestRepo", @"C:\", new RandomFileTokenProvider(), eventLogMock.Object, TimeSpan.MaxValue));
            // ReSharper restore once ObjectCreationAsStatement
        }


        #region Helper Methods


        /// <summary>
        ///     Gets a count of the files in all directories and subdirectories.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        private int CountFiles(string directory, string searchPattern)
        {
            var directoryInfo = new DirectoryInfo(directory);

            var count = directoryInfo.GetFiles(searchPattern, SearchOption.AllDirectories).Length;

            return count;
        }


        private string FindFilePathByName(string directory, string fileName)
        {
            var directoryInfo = new DirectoryInfo(directory);

            var files = directoryInfo.GetFiles(fileName);
            switch (files.Length)
            {
                case 1:
                    // Found the file return it.
                    return files[0].FullName;
                case 0:
                    var subDirectories = directoryInfo.GetDirectories();
                    foreach (var foundFileName in subDirectories.Select(subDirectory => FindFilePathByName(subDirectory.FullName, fileName)).Where(foundFileName => foundFileName != null))
                    {
                        return foundFileName;
                    }
                    break;
                default:
                    return null;
            }
            return null;
        }


        private string GetTempDirectoryPath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }


        private void DeleteDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch
            {
                // Ignore
            }
        }


        private FileRepository CreateTestRepository(string tempRepoPath, bool allowDuplicates, TimeSpan retentionPeriod, TimeSpan cleanupInterval)
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var eventLogMock = mockRepo.Create<IEventLog>();

            IFileTokenProvider tokenProvider;
            if (allowDuplicates)
            {
                tokenProvider = new RandomFileTokenProvider();
            }
            else
            {
                tokenProvider = new Sha256FileTokenProvider();
            }

            return new FileRepository("TestRepo", tempRepoPath, tokenProvider, eventLogMock.Object, retentionPeriod, cleanupInterval);
        }


        private string GetTempFilePattern()
        {
            return "*" + TempFileExtension;
        }


        private string GetDataFilePattern()
        {
            return "*" + DataFileExtension;
        }


        #endregion
    }
}