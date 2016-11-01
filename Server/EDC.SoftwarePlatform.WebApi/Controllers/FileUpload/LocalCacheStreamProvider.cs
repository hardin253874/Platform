// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using EDC.SoftwarePlatform.Services.FileManager;

namespace EDC.SoftwarePlatform.WebApi.Controllers.FileUpload
{
	/// <summary>
	///     This multi-part stream provider stores the files in the local upload directory.
	///     It also keeps a map of remote to local filenames along with file hash.
	/// </summary>
	public class LocalCacheStreamProvider : MultipartFormDataStreamProvider
	{
		/// <summary>
		///     The remote to local file name map
		/// </summary>
		public Dictionary<string, string> RemoteToLocalFileNameMap = new Dictionary<string, string>( );


		/// <summary>
		///     Initializes a new instance of the <see cref="LocalCacheStreamProvider" /> class.
		/// </summary>
		public LocalCacheStreamProvider( )
			: base( FileManagerService.UploadDirectory )
		{
		}


		/// <summary>
		///     Gets the name of the local file which will be combined with the root path to create an absolute file name where the
		///     contents of the current MIME body part will be stored.
		/// </summary>
		/// <param name="headers">The headers for the current MIME body part.</param>
		/// <returns>
		///     A relative filename with no path component.
		/// </returns>
		public override string GetLocalFileName( HttpContentHeaders headers )
		{
			string id = Guid.NewGuid( ).ToString( @"N" );
			string fileName = headers.ContentDisposition.FileName.Trim( '"' );

			string localFileName = FileManagerService.GetFilenameFromFileId( id );
			RemoteToLocalFileNameMap.Add( fileName, localFileName );

			return localFileName;
		}
	}
}