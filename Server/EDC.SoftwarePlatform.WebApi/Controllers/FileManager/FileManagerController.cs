// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApi.Controllers.FileManager
{
	/// <summary>
	///     File Manager controller class.
	/// </summary>
	[RoutePrefix( "data/v3/file" )]
	public class FileManagerController : ApiController
	{
		/// <summary>
		///     Puts the transitory file.
		/// </summary>
		/// <returns></returns>
		[Route( "" )]
        [HttpPut]
		public async Task<HttpResponseMessage<List<FileItem>>> PutTransitoryFile( )
		{
			var response = new HttpResponseMessage<List<FileItem>>( new List<FileItem>( ) );
			return await Task.FromResult( response );
		}

		/// <summary>
		///     Gets the file for token.
		/// </summary>
		/// <param name="fileId">The file identifier.</param>
		/// <returns></returns>
		[Route( "{fileToken}" )]
        [HttpGet]
		public HttpResponseMessage GetFileForToken( string fileId )
		{
			return new HttpResponseMessage( );
		}
	}
}