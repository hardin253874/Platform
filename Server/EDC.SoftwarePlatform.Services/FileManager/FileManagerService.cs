// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using EDC.ReadiNow.Configuration;

namespace EDC.SoftwarePlatform.Services.FileManager
{
	public class FileManagerService
	{
		// This needs to be put into configuration somehow...by web.config or somewhere in the database!!
		public static readonly string UploadDirectory = UploadDirectorySettings.Current.Path;

		#region Implementation of IFileManager

		/// <summary>
		///     Begins the upload of a file.
		/// </summary>
		/// <param name="chunk">The binary chunk of data that is at the start of the file.</param>
		/// <param name="size">The size of the binary chunk.</param>
		/// <param name="fileExtension">The file extension.</param>
		/// <returns>A GUID that uniquely identifies the start of the file.</returns>
		public string BeginUpload( byte[ ] chunk, int size, string fileExtension )
		{
			string fileUploadId = Guid.NewGuid( ).ToString( @"N" );
			string filename = GetFilenameFromFileId( fileUploadId );
			FileStream fs = File.Open( filename, FileMode.CreateNew, FileAccess.ReadWrite );
			fs.Write( chunk, 0, size );
			fs.Flush( true );
			fs.Close( );
			return fileUploadId;
		}

		/// <summary>
		///     Cancels the upload.
		/// </summary>
		/// <param name="fileUploadId">The file upload id that was returned from a previous call to <see cref="BeginUpload" />.</param>
		/// <remarks>
		///     This will delete the existing file on the server and invalidate the ID from being used again.
		/// </remarks>
		public void CancelUpload( string fileUploadId )
		{
			string filename = GetFilenameFromFileId( fileUploadId );
			if ( !File.Exists( filename ) )
			{
				throw new ArgumentOutOfRangeException( "fileUploadId" );
			}
			if ( File.Exists( filename ) )
			{
				// Delete the file
				File.Delete( filename );
			}
		}

		/// <summary>
		///     Gets the file details for a specific uploaded file.
		/// </summary>
		/// <param name="fileUploadId">The file upload id for the file to be queried.</param>
		/// <returns>
		///     The basic details for the file.
		/// </returns>
		/// <remarks></remarks>
		public FileDetail GetFileDetails( string fileUploadId )
		{
			return GetFileDetails_Static( fileUploadId );
		}

		/// <summary>
		///     Gets the file details for a specific uploaded file.
		/// </summary>
		/// <param name="fileUploadId">The file upload id for the file to be queried.</param>
		/// <returns>
		///     The basic details for the file.
		/// </returns>
		/// <remarks></remarks>
		public static FileDetail GetFileDetails_Static( string fileUploadId )
		{
			var fileDetail = new FileDetail( );
			string filenameFromId = GetFilenameFromFileId( fileUploadId );
			if ( File.Exists( filenameFromId ) )
			{
				fileDetail.Exists = true;
				var fileInfo = new FileInfo( filenameFromId );
				fileDetail.Name = fileInfo.Name;
				fileDetail.Path = fileInfo.DirectoryName;
				fileDetail.FullName = fileInfo.FullName;
				fileDetail.Size = Convert.ToInt32( fileInfo.Length );
			}
			else
			{
				fileDetail.Exists = false;
				fileDetail.Name = string.Empty;
				fileDetail.Path = string.Empty;
				fileDetail.FullName = string.Empty;
				fileDetail.Size = 0;
			}
			return fileDetail;
		}

		/// <summary>
		///     Appends a chunk of data to the end of an existing file part that is being uploaded.
		/// </summary>
		/// <param name="fileUploadId">The file upload id that was returned from a previous call to <see cref="BeginUpload" />.</param>
		/// <param name="chunk">The binary chunk of data that is at the start of the file.</param>
		/// <param name="size">The size of the binary chunk.</param>
		/// <param name="checksum">The SHA1 checksum calculated on the client for this file.</param>
		/// <remarks>
		///     If the checksum is null, no file validation is performed.
		/// </remarks>
		public void UploadChunk( string fileUploadId, byte[ ] chunk, int size, byte[ ] checksum )
		{
			string filename = GetFilenameFromFileId( fileUploadId );
			if ( !File.Exists( filename ) )
			{
				throw new ArgumentOutOfRangeException( "fileUploadId" );
			}
			using ( FileStream fs = File.Open( filename, FileMode.Open, FileAccess.ReadWrite ) )
			{
				if ( chunk != null && size > 0 )
				{
					fs.Seek( 0, SeekOrigin.End );
					fs.Write( chunk, 0, size );
					fs.Flush( true );
				}
				if ( checksum == null )
				{
					fs.Close( );
					return;
				}

				// Has checksum, must be the last chunk. Validate that we have it right
				SHA1 hash = new SHA1CryptoServiceProvider( );
				fs.Seek( 0, SeekOrigin.Begin );
				byte[ ] ourChecksum = hash.ComputeHash( fs );
				if ( !ourChecksum.SequenceEqual( checksum ) )
				{
					throw new Exception( "Checksum validation failed" );
				}
				fs.Close( );
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		///     Gets the filename from upload id.
		/// </summary>
		/// <param name="fileUploadId">The file upload id.</param>
		/// <returns>The full file path for the uploading document.</returns>
		/// <remarks>
		///     TODO - Requires a fast way of getting the base path rather than going and grabbing it from the DB as with
		///     other configuration items
		/// </remarks>
		public static string GetBaseFilenameFromFileId( Guid fileUploadId )
		{
			return string.Format( @"{0}.file", fileUploadId.ToString( @"N" ) );
		}

		/// <summary>
		///     Gets the filename from upload id.
		/// </summary>
		/// <param name="fileUploadId">The file upload id.</param>
		/// <returns>The full file path for the uploading document.</returns>
		/// <remarks>
		///     TODO - Requires a fast way of getting the base path rather than going and grabbing it from the DB as with
		///     other configuration items
		/// </remarks>
		public static string GetFilenameFromFileId( string fileUploadId )
		{
			Guid fileGuid;
			if ( Guid.TryParse( fileUploadId, out fileGuid ) )
			{
				return string.Format( @"{0}\{1}", UploadDirectory, GetBaseFilenameFromFileId( fileGuid ) );
			}
			return string.Empty;
		}

		#endregion
	}
}