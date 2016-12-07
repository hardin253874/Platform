// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Services.FileManager;
using EDC.SoftwarePlatform.WebApi.Test.Common;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.FileManager
{
	/// <summary>
	///     Tests for the file manager service
	/// </summary>
	[TestFixture]
	public class FileManagerTests
	{
		//
		// The chunks are multiples of block size which is 512 on NTFS systems.
		//
		private const int Chunkiness = 4 * 512;

		private const string BpSemanticDbModelingDocx = @"BPSemanticDBModeling.docx";
		private const string DespairJpeg = @"despair.jpg";

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

			using ( Stream tinyFileStream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.FileManager.despair.jpg" ) )
			using ( FileStream fs = File.Create( DespairJpeg ) )
			{
				if ( tinyFileStream != null )
					tinyFileStream.CopyTo( fs );
				fs.Flush( true );
			}

			using ( Stream largeFileStream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.FileManager.BPSemanticDBModeling.docx" ) )
			using ( FileStream fs = File.Create( BpSemanticDbModelingDocx ) )
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
			File.Delete( BpSemanticDbModelingDocx );
		}

		private static string FileManagerBeginUpload( byte[ ] chunk, int size, string fileExtension )
		{
			string result = null;

			CommonServiceTestsHelper.CallServiceWithEdcContext<FileManagerService>( ( service, waitHandle ) =>
			{
				result = service.BeginUpload( chunk, size, fileExtension );
				waitHandle.Set( );
			} );

			return result;
		}

		private static void FileManagerUploadChunk( string fileUploadId, byte[ ] chunk, int size, byte[ ] checksum )
		{
			CommonServiceTestsHelper.CallServiceWithEdcContext<FileManagerService>( ( service, waitHandle ) =>
			{
				service.UploadChunk( fileUploadId, chunk, size, checksum );
				waitHandle.Set( );
			} );
		}

		private static void FileManagerCancelUpload( string fileUploadId )
		{
			CommonServiceTestsHelper.CallServiceWithEdcContext<FileManagerService>( ( service, waitHandle ) =>
			{
				service.CancelUpload( fileUploadId );
				waitHandle.Set( );
			} );
		}

		[Test]
		public void Test10TinyFileUpload( )
		{
			string fileExtension = ( new FileInfo( DespairJpeg ) ).Extension;
			// Upload the file in chunks. Everything inside the using block is what the client actually does (except for the service helper stuff...)
			// Open the file and grab a chunk of data to send
			using ( FileStream fs = File.Open( DespairJpeg, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				var filestreamLength = ( int ) fs.Length;
				var chunk = new byte[Chunkiness + 1];
				int bytesRead = fs.Read( chunk, 0, Chunkiness > filestreamLength ? filestreamLength : Chunkiness );

				// Call the web service for the first chunk
				string fileId = FileManagerBeginUpload( chunk, bytesRead, fileExtension );
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
        }

        [TestCase( "image", ".jpg", true )]
        [TestCase( "image", ".jpeg", true )]
        [TestCase( "image", ".png", true )]
        [TestCase( "image", ".docx", false )]
        [TestCase( "image", ".txt", false )]
        [TestCase( "image", ".exe", false )]
        [TestCase( "import", ".csv", true )]
        [TestCase( "import", ".xlsx", true )]
        [TestCase( "import", ".txt", true )]
        [TestCase( "import", ".jpg", false )]
        [TestCase( "import", ".docx", false )]
        [TestCase( "import", ".exe", false )]
        [TestCase( "xml", ".xml", true )]
        [TestCase( "xml", ".csv", false )]
        [TestCase( "xml", ".xlsx", false )]
        [TestCase( "xml", ".txt", false )]
        [TestCase( "xml", ".jpg", false )]
        [TestCase( "xml", ".docx", false )]
        [TestCase( "xml", ".exe", false )]
        [TestCase( null, ".docx", true )]
        [TestCase( null, ".jpg", true )]
        [TestCase( null, ".xlsx", true )]
        [TestCase( null, ".xml", false )]
        [TestCase( null, ".exe", false )]
        [RunAsDefaultTenant]
        public void Test15FileWhitelist( string type, string extension, bool allowed )
        {
            bool actual = FileRepositoryHelper.CheckFileExtensionIsValid( "blah" + extension, type );
            Assert.That( actual, Is.EqualTo( allowed ) );
        }

        [Test]
		public void Test20BigFileUpload( )
		{
			// Upload the file in chunks. Everything inside the using block is what the client actually does (except for the service helper stuff...)
			// Open the file and grab a chunk of data to send
			string fileExtension = ( new FileInfo( BpSemanticDbModelingDocx ) ).Extension;
			using ( FileStream fs = File.Open( BpSemanticDbModelingDocx, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				var filestreamLength = ( int ) fs.Length;
				var chunk = new byte[Chunkiness + 1];
				int bytesRead = fs.Read( chunk, 0, Chunkiness > filestreamLength ? filestreamLength : Chunkiness );

				// Call the web service for the first chunk
				string fileId = FileManagerBeginUpload( chunk, bytesRead, fileExtension );
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
		}

		[Test]
		[ExpectedException( typeof ( ArgumentOutOfRangeException ) )]
		public void Test30CancelFileUpload( )
		{
			// Upload the file in chunks. Everything inside the using block is what the client actually does (except for the service helper stuff...)
			// Open the file and grab a chunk of data to send
			string fileExtension = ( new FileInfo( BpSemanticDbModelingDocx ) ).Extension;
			using ( FileStream fs = File.Open( BpSemanticDbModelingDocx, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				var filestreamLength = ( int ) fs.Length;
				var chunk = new byte[Chunkiness + 1];
				int bytesRead = fs.Read( chunk, 0, Chunkiness > filestreamLength ? filestreamLength : Chunkiness );

				// Call the web service for the first chunk
				string fileId = FileManagerBeginUpload( chunk, bytesRead, fileExtension );
				// Read the rest of the file in chunks and send each chunk up to the web service
				while ( ( bytesRead = fs.Read( chunk, 0, Chunkiness > filestreamLength ? filestreamLength : Chunkiness ) ) == Chunkiness )
				{
					// Upload the chunk to the server while we have full chunks
					FileManagerUploadChunk( fileId, chunk, bytesRead, null );
				}

				// Close the file
				fs.Close( );
				// Sent the finalization checksum for the file to the service.
				// This means that the file is now on the server but not in the database
				FileManagerCancelUpload( fileId );

				// Try and do an upload to our file ID. This should fail
				FileManagerUploadChunk( fileId, chunk, bytesRead, null );
			}
		}

		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void Test30RubishFileUpload( )
		{
			FileManagerBeginUpload( null, 100, string.Empty );
		}
	}
}