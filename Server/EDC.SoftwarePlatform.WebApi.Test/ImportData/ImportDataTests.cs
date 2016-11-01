// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using EDC.SoftwarePlatform.Services.FileManager;
using EDC.SoftwarePlatform.Services.ImportData;
using EDC.SoftwarePlatform.WebApi.Test.Common;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.ImportData
{
	/// <summary>
	///     Summary description for ImportDataTests
	/// </summary>
	[TestFixture]
	public class ImportDataTests
	{
		//
		// The chunks are multiples of block size which is 512 on NTFS systems.
		//
		private const int Chunkiness = 4 * 512;

		private const string DespairJpeg = @"despair.jpg";
		private const string MediocrityJpeg = @"mediocrity.jpg";

		/// <summary>
		///     Gets or sets the test context which provides
		///     information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext
		{
			get;
			set;
		}

		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [TestFixtureSetUp]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [TestFixtureTearDown]
		// public static void MyClassCleanup() { }
		//
		[TestFixtureSetUp]
		public static void TestClassInitialize( )
		{
			Assembly assembly = Assembly.GetExecutingAssembly( );

			using ( Stream tinyFileStream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.ImportData.despair.jpg" ) )
			using ( FileStream fs = File.Create( DespairJpeg ) )
			{
				if ( tinyFileStream != null )
					tinyFileStream.CopyTo( fs );
				fs.Flush( true );
			}

			using ( Stream largeFileStream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.ImportData.mediocrity.jpg" ) )
			using ( FileStream fs = File.Create( MediocrityJpeg ) )
			{
				if ( largeFileStream != null )
					largeFileStream.CopyTo( fs );
				fs.Flush( true );
			}
		}

		[TestFixtureTearDown]
		public static void TestClassCleanup( )
		{
			// Delete all files used for the test
			File.Delete( DespairJpeg );
			File.Delete( MediocrityJpeg );
		}

		private string UploadFileToServer( string filename )
		{
			string fileId;
			// Upload the file in chunks. Everything inside the using block is what the client actually does (except for the service helper stuff...)
			// Open the file and grab a chunk of data to send
			string fileExtension = ( new FileInfo( filename ) ).Extension;
			using ( FileStream fs = File.Open( filename, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				var filestreamLength = ( int ) fs.Length;
				var chunk = new byte[Chunkiness + 1];
				int bytesRead = fs.Read( chunk, 0, Chunkiness > filestreamLength ? filestreamLength : Chunkiness );

				// Call the web service for the first chunk
				fileId = FileManagerBeginUpload( chunk, bytesRead, fileExtension );
				// Read the rest of the file in chunks and send each chunk up to the web service
				while ( ( bytesRead = fs.Read( chunk, 0, Chunkiness > filestreamLength ? filestreamLength : Chunkiness ) ) == Chunkiness )
				{
					// Upload the chunk to the server while we have full chunks
					FileManagerUploadChunk( fileId, chunk, bytesRead, null );
				}

				// Generate the checksum for the service
				fs.Seek( 0, SeekOrigin.Begin );
				SHA1 hash = new SHA1CryptoServiceProvider( );
				byte[ ] ourChecksum = hash.ComputeHash( fs );

				// Close the file
				fs.Close( );
				// Sent the finalization checksum for the file to the service.
				// This means that the file is now on the server but not in the database
				FileManagerUploadChunk( fileId, chunk, bytesRead, ourChecksum );
			}
			return fileId;
		}

		public static string FileManagerBeginUpload( byte[ ] chunk, int size, string fileExtension )
		{
			string result = null;

			CommonServiceTestsHelper.CallServiceWithEdcContext<FileManagerService>( ( service, waitHandle ) =>
			{
				result = service.BeginUpload( chunk, size, fileExtension );
				waitHandle.Set( );
			} );

			return result;
		}

		public static void FileManagerUploadChunk( string fileUploadId, byte[ ] chunk, int size, byte[ ] checksum )
		{
			CommonServiceTestsHelper.CallServiceWithEdcContext<FileManagerService>( ( service, waitHandle ) =>
			{
				service.UploadChunk( fileUploadId, chunk, size, checksum );
				waitHandle.Set( );
			} );
		}

		public static string ImportDataInsertToRepository( string fileUploadId )
		{
		    string token = string.Empty;

			CommonServiceTestsHelper.CallServiceWithEdcContext<ImportDataService>( ( service, waitHandle ) =>
			{
                token = service.InsertFileToRepository( fileUploadId );
				waitHandle.Set( );
			} );

		    return token;

		}

		public static Stream ImportDataGetStreamFromRepository( string fileUploadId )
		{
			Stream result = null;

			CommonServiceTestsHelper.CallServiceWithEdcContext<ImportDataService>( ( service, waitHandle ) =>
			{
				result = service.GetFileFromRepository( fileUploadId );
				waitHandle.Set( );
			} );

			return result;
		}

		public static void ImportDataDeleteFileFromRepository( string fileUploadId )
		{
			CommonServiceTestsHelper.CallServiceWithEdcContext<ImportDataService>( ( service, waitHandle ) =>
			{
				service.DeleteFileFromRepository( fileUploadId );
				waitHandle.Set( );
			} );
		}

		[Test]
		public void Test10FileImportToDatabase( )
		{
			string fileId = UploadFileToServer( DespairJpeg );

            ImportDataInsertToRepository( fileId );
		}

		[Test]
		public void Test20FileFromDatabaseToStream( )
		{
			string fileId = UploadFileToServer( DespairJpeg );

			string token = ImportDataInsertToRepository( fileId );
			Stream streamFromDatabase = ImportDataGetStreamFromRepository( token );
			// Generate hashes for both the file and the returned file stream to see if they match
			SHA1 hash = new SHA1CryptoServiceProvider( );
			byte[ ] databaseFilestreamChecksum = hash.ComputeHash( streamFromDatabase );

			byte[ ] physicalFileChecksum;
			using ( FileStream streamFromFile = File.Open( DespairJpeg, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				physicalFileChecksum = hash.ComputeHash( streamFromFile );
			}
			Assert.IsTrue( databaseFilestreamChecksum.SequenceEqual( physicalFileChecksum ) );
		}

		[Test]
		public void Test30DeleteFileFromDatabase( )
		{
			string fileId = UploadFileToServer( DespairJpeg );

			string token = ImportDataInsertToRepository( fileId );
			Stream streamFromDatabaseBefore = ImportDataGetStreamFromRepository(token);
			Assert.IsFalse( streamFromDatabaseBefore == null );
            ImportDataDeleteFileFromRepository(token);
			Stream streamFromDatabaseAfter = ImportDataGetStreamFromRepository(token);
			Assert.IsTrue( streamFromDatabaseAfter == null );
		}

		[Test]
		public void Test40MultipleFileLoadToDatabase( )
		{
			// 1st file
			string despairFileId = UploadFileToServer( DespairJpeg );

			string tokenDespair = ImportDataInsertToRepository( despairFileId );
			// 2nd file
			string mediocrityFileId = UploadFileToServer( MediocrityJpeg );
			string tokenMediocrity = ImportDataInsertToRepository( mediocrityFileId );
			// Get the both back
			Stream despairStreamFromDatabase = ImportDataGetStreamFromRepository(tokenDespair);
			Assert.IsFalse( despairStreamFromDatabase == null );
			Stream mediocrityStreamFromDatabase = ImportDataGetStreamFromRepository(tokenMediocrity);
			Assert.IsFalse( mediocrityStreamFromDatabase == null );
			// Test against the original files
			SHA1 hash = new SHA1CryptoServiceProvider( );
			byte[ ] despairFilestreamChecksum = hash.ComputeHash( despairStreamFromDatabase );
			byte[ ] despairFileChecksum;
			using ( FileStream streamFromFile = File.Open( DespairJpeg, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				despairFileChecksum = hash.ComputeHash( streamFromFile );
			}
			Assert.IsTrue( despairFilestreamChecksum.SequenceEqual( despairFileChecksum ) );

			byte[ ] mediocrityFilestreamChecksum = hash.ComputeHash( mediocrityStreamFromDatabase );
			byte[ ] mediocrityFileChecksum;
			using ( FileStream streamFromFile = File.Open( MediocrityJpeg, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				mediocrityFileChecksum = hash.ComputeHash( streamFromFile );
			}
			Assert.IsTrue( mediocrityFilestreamChecksum.SequenceEqual( mediocrityFileChecksum ) );
			// Test they are different streams from the DB
			Assert.IsFalse( despairFilestreamChecksum.SequenceEqual( mediocrityFilestreamChecksum ) );
            // Delete the files from the database
            ImportDataDeleteFileFromRepository(tokenDespair);
            ImportDataDeleteFileFromRepository(tokenMediocrity);

			// Check they are goneski
			Stream despairStreamAfterDelete = ImportDataGetStreamFromRepository(tokenDespair);
			Assert.IsTrue( despairStreamAfterDelete == null );
			Stream mediocrityStreamAfterDelete = ImportDataGetStreamFromRepository(tokenMediocrity);
			Assert.IsTrue( mediocrityStreamAfterDelete == null );
		}
	}
}