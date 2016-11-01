// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using EDC.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Test;
using EDC.Security;
using EDC.SoftwarePlatform.Services.FileManager;
using EDC.SoftwarePlatform.WebApi.Test.Common;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.ImageManager
{
	/// <summary>
	/// </summary>
	[TestFixture]
	public class ImageControllerTests
	{
		/// <summary>
		///     The chunkiness
		/// </summary>
		private const int Chunkiness = 4 * 512;

		/// <summary>
		///     The image1 JPG
		/// </summary>
		private const string Image1Jpg = @"Image1.jpg";

		/// <summary>
		///     Tests the class initialize.
		/// </summary>
		[TestFixtureSetUp]
		public static void TestClassInitialize( )
		{
			Assembly assembly = Assembly.GetExecutingAssembly( );
			using ( Stream image1Stream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.ImageManager.Image1.jpg" )
				)
			{
				using ( FileStream fs = File.Create( Image1Jpg ) )
				{
					if ( image1Stream != null )
					{
						image1Stream.CopyTo( fs );
					}

					fs.Flush( true );
				}
			}
		}

		/// <summary>
		///     Teardown
		/// </summary>
		[TestFixtureTearDown]
		public static void TestClassCleanup( )
		{
			// Delete all files used for the test
			File.Delete( Image1Jpg );
		}

		/// <summary>
		///     Creates the image1.
		/// </summary>
		/// <returns></returns>
		private PhotoFileType CreateImage1( )
		{
			// Upload file
			string image1Id = UploadFileToServer( Image1Jpg );

			// Define a new image entity
			var image1EntityData = new EntityData
			{
				Fields = new List<FieldData>( ),
				TypeIds = new EntityRef( "core", "photoFileType" ).ToEnumerable( ).ToList( )
			};
			image1EntityData.Fields.Add( new FieldData
			{
				FieldId = new EntityRef( "name" ),
				Value = new TypedValue( "Image1" )
			} );

			// Create the image entity and add the file to the file stream binary table
			var imageManager = new Services.ImageManager.ImageManager( );
			EntityRef image1EntityRef = imageManager.CreateImageFromUploadedFile( image1Id, image1EntityData, ".jpg" );

			// Verify that the entity exists
			var image1Entity = Entity.Get<PhotoFileType>( image1EntityRef, true );

			Assert.IsNotNull( image1Entity, "The image should not be null." );
			Assert.AreEqual( 1600, image1Entity.ImageWidth, "The image width is invalid." );
			Assert.AreEqual( 1200, image1Entity.ImageHeight, "The image height is invalid." );

			Assert.IsTrue( imageManager.DoesImageFileExist( image1Entity.Id ), "The binary files does not exist." );

			return image1Entity;
		}

		/// <summary>
		///     Uploads the file to server.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
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

		/// <summary>
		///     Files the manager begin upload.
		/// </summary>
		/// <param name="chunk">The chunk.</param>
		/// <param name="size">The size.</param>
		/// <param name="fileExtension"></param>
		/// <returns></returns>
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

		/// <summary>
		///     Files the manager upload chunk.
		/// </summary>
		/// <param name="fileUploadId">The file upload id.</param>
		/// <param name="chunk">The chunk.</param>
		/// <param name="size">The size.</param>
		/// <param name="checksum">The checksum.</param>
		private static void FileManagerUploadChunk( string fileUploadId, byte[ ] chunk, int size, byte[ ] checksum )
		{
			CommonServiceTestsHelper.CallServiceWithEdcContext<FileManagerService>( ( service, waitHandle ) =>
			{
				service.UploadChunk( fileUploadId, chunk, size, checksum );
				waitHandle.Set( );
			} );
		}

		/// <summary>
		///     Gets the image thumbnail E tag.
		/// </summary>
		/// <param name="imageFileDataHash">The image file data hash.</param>
		/// <param name="sizeId">The size id.</param>
		/// <param name="scaleId">The scale id.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		///     imageFileDataHash
		///     or
		///     sizeId
		///     or
		///     scaleId
		/// </exception>
		private string GetImageThumbnailETag( string imageFileDataHash, EntityRef sizeId, EntityRef scaleId )
		{
			if ( string.IsNullOrEmpty( imageFileDataHash ) )
			{
				throw new ArgumentNullException( "imageFileDataHash" );
			}

			if ( sizeId == null )
			{
				throw new ArgumentNullException( "sizeId" );
			}

			if ( scaleId == null )
			{
				throw new ArgumentNullException( "scaleId" );
			}

			var thumbnailSizeEnum = Entity.Get<ThumbnailSizeEnum>( sizeId );

			int thumbnailWidth = thumbnailSizeEnum.ThumbnailWidth ?? -1;
			int thumbnailHeight = thumbnailSizeEnum.ThumbnailHeight ?? -1;

			var hashInput = new StringBuilder( );
			hashInput.AppendFormat( "DataHash:{0}", imageFileDataHash );
			hashInput.AppendFormat( "Width:{0}", thumbnailWidth );
			hashInput.AppendFormat( "Height:{0}", thumbnailHeight );
			hashInput.AppendFormat( "Scale:{0}", scaleId.Id );

			return CryptoHelper.GetSha1Hash( hashInput.ToString( ) );
		}

		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetImage( )
		{
			PhotoFileType image1Entity = null;

			try
			{
				// Create the image
				image1Entity = CreateImage1( );

				using ( var request = new PlatformHttpRequest( string.Format( @"data/v1/image/{0}", image1Entity.Id ) ) )
				{
					HttpWebResponse response = request.GetResponse( );

					// check that it worked (200)
					Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );

					using ( var ms = new MemoryStream( ) )
					using ( var originalImage = new MemoryStream( ) )
					{
						using ( Stream dataStream = response.GetResponseStream( ) )
						{
							Assert.IsNotNull( dataStream );

							dataStream.CopyTo( ms );
						}

						// Get the actual image as a memory stream
						using ( FileStream fs = File.Open( Image1Jpg, FileMode.Open, FileAccess.Read, FileShare.Read ) )
						{
							fs.CopyTo( originalImage );
						}

                        Sha256FileTokenProvider tokenProvider = new Sha256FileTokenProvider();

                        // Compare the values
                        string streamHash = tokenProvider.ComputeToken( ms );
						string fileHash = tokenProvider.ComputeToken( originalImage );

						string eTagValue = response.Headers.Get( "ETag" );

						Assert.AreEqual( fileHash, streamHash, "The file hash is invalid" );                        					    

						Assert.AreEqual( "\"" + fileHash + "\"", eTagValue, "The ETag value is invalid" );
					}
				}
			}
			finally
			{
				if ( image1Entity != null )
				{
					image1Entity.Delete( );
				}
			}
		}

		/// <summary>
		///     Tests the get image thumbnail
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetImageThumbnail( )
		{
			PhotoFileType image1Entity = null;

			try
			{
				// Create the image
				image1Entity = CreateImage1( );

				var thumbnailSize = Entity.Get<ThumbnailSizeEnum>( new EntityRef( "console", "smallThumbnail" ) );

				// Get the thumbnail
				using (
					var request =
						new PlatformHttpRequest( string.Format( @"data/v1/image/thumbnail/{0}/console-smallThumbnail/core-cropImage", image1Entity.Id ) ) )
				{
					HttpWebResponse response = request.GetResponse( );

					// check that it worked (200)
					Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );

					using ( var ms = new MemoryStream( ) )
					{
						using ( Stream dataStream = response.GetResponseStream( ) )
						{
							Assert.IsNotNull( dataStream );

							dataStream.CopyTo( ms );
						}

						using ( Image image = Image.FromStream( ms ) )
						{
							Assert.AreEqual( image.Width, thumbnailSize.ThumbnailWidth, "The image width is invalid" );
							Assert.AreEqual( image.Height, thumbnailSize.ThumbnailHeight, "The image height is invalid" );
						}

                        Sha256FileTokenProvider tokenProvider = new Sha256FileTokenProvider();

                        string streamHash = tokenProvider.ComputeToken( ms );
					    string fileHash;

                        using (FileStream fs = File.Open(Image1Jpg, FileMode.Open, FileAccess.Read, FileShare.Read))
					    {
                            fileHash = tokenProvider.ComputeToken(fs);
                        }                            

						Assert.AreNotEqual( fileHash, streamHash, "The file hash should not match" );

						string eTagValue = response.Headers.Get( "ETag" );
						string expectedETag = GetImageThumbnailETag( fileHash, new EntityRef( "console", "smallThumbnail" ),
							new EntityRef( "core", "cropImage" ) );
						Assert.AreNotEqual( "\"" + expectedETag + "\"", eTagValue, "The ETag is invalid" );
					}
				}
			}
			finally
			{
				if ( image1Entity != null )
				{
					image1Entity.Delete( );
				}
			}
		}
	}
}