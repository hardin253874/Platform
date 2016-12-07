// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using EDC.Exceptions;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using ERM = EDC.ReadiNow.Model;
using System.Text.RegularExpressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.WebApi.Controllers.FileUpload
{
	/// <summary>
	///     file upload and download controller
	/// </summary>
	[RoutePrefix( "data/v2/file" )]
	public class FileController : ApiController
	{
        /// <summary>
        /// Regex for matching IOS mobile devices.
        /// </summary>
        private static readonly Regex IosMobileRegex = new Regex("iPad|iPhone|iPod", RegexOptions.IgnoreCase | RegexOptions.Multiline);


        //TODO: look at http://www.strathweb.com/2012/09/dealing-with-large-files-in-asp-net-web-api/
        // and get the max file size sorted out.       


		/// <summary>
		///     Posts the form data.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Web.Http.HttpResponseException">
		/// </exception>
		[Route( "" )]
        [HttpPost]
		public async Task<HttpResponseMessage<FileList>> PostFormData( )
		{
			//TODO: store the original filename along with the file.

			if ( !Request.Content.IsMimeMultipartContent( ) )
			{
				throw new HttpResponseException( HttpStatusCode.UnsupportedMediaType );
			}

			var provider = new LocalCacheStreamProvider( );

		    try
		    {
                await Request.Content.ReadAsMultipartAsync(provider);
		    }
		    catch (Exception ex)
		    {
                throw new WebArgumentException(string.Format("Invalid file upload request. Request: {0}", Request), ex);
		    }

			var idMap = new FileList( );

			try
			{
			    string expectedType = Request.RequestUri.ParseQueryString( )[ "type" ];

				foreach ( var entry in provider.RemoteToLocalFileNameMap )
				{
					string remoteFileName = entry.Key;
					string localFileName = entry.Value;						

					string token;

				    if ( !FileRepositoryHelper.CheckFileExtensionIsValid( remoteFileName, expectedType ) )
				        throw new PlatformSecurityException( "Disallowed file type." );

					using ( var source = new FileStream( localFileName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
					{
						token = FileRepositoryHelper.AddTemporaryFile(source);                            
					}
					idMap.Add( new FileListItem
					{
						FileName = remoteFileName, Hash = token
                    } );
				}                
			}
			finally
			{
				// clean up the local files
				foreach ( var entry in provider.RemoteToLocalFileNameMap )
				{
					File.Delete( entry.Value );
				}
			}


			return new HttpResponseMessage<FileList>( idMap );
		}

		/// <summary>
		///     Gets the file by identifier.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <returns></returns>
		[Route( "{entityId:long}" )]
        [HttpGet]
		public HttpResponseMessage GetFileById( long entityId )
		{
			return GetFileForRequest( Request, entityId, GetFileEtag );
		}

		/// <summary>
		///     Gets the file by token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns></returns>
		[Route( "{token}" )]
        [HttpGet]
		public HttpResponseMessage GetFileByToken( string token )
		{
			var response = new HttpResponseMessage( HttpStatusCode.OK )
			{
				Content = new StreamContent( FileRepositoryHelper.GetTemporaryFileDataStream( token ) )
			};
			return response;
		}

		/// <summary>
		///     Gets the file.
		/// </summary>
		/// <param name="fileId">The file identifier.</param>
		/// <returns></returns>
		[Route( "{fileId}/download" )]
        [HttpGet]
		public HttpResponseMessage GetFile( string fileId = null )
		{
			return GetFileForRequest( Request, fileId, GetFileEtag );
		}


		/// <summary>
		///     Gets the file e-tag.
		/// </summary>
		/// <param name="fileRef">The file reference.</param>
		/// <returns></returns>
		private string GetFileEtag( ERM.EntityRef fileRef )
		{
			var file = ERM.Entity.Get<ERM.FileType>( fileRef, ERM.FileType.FileDataHash_Field );

			// Get the entity tag, which is the hash of the image data
			string etag = string.Format( "\"{0}\"", file.FileDataHash );

			return etag;
		}

		/// <summary>
		///     Get a file for a request. If the
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="imageIdRef">The image identifier reference.</param>
		/// <param name="generateEtagFn">The generate e-tag function.</param>
		/// <returns>
		///     The file response or a NotModified response if the e-tag matches.
		/// </returns>
		public static HttpResponseMessage GetFileForRequest( HttpRequestMessage request, ERM.EntityRef imageIdRef, Func<ERM.EntityRef, string> generateEtagFn )
		{
            string etag = generateEtagFn(imageIdRef);

            // If the request contains the same e-tag then return a not modified
            if (request.Headers.IfNoneMatch != null &&
                 request.Headers.IfNoneMatch.Any(et => et.Tag == etag))
            {
                var notModifiedResponse = new HttpResponseMessage(HttpStatusCode.NotModified);
                notModifiedResponse.Headers.ETag = new EntityTagHeaderValue(etag, false);
                return notModifiedResponse;
            }

            // Return the image
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(FileRepositoryHelper.GetFileDataStreamForEntity(imageIdRef))
            };
            response.Headers.ETag = new EntityTagHeaderValue(etag, false);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MustRevalidate = true,
                MaxAge = new TimeSpan(0)
            };

            // Yuck. You gotta do what you gotta do.
            bool isIosDevice = false;
            if (request.Headers.UserAgent != null)
            {                
                isIosDevice = request.Headers.UserAgent.Any(ua => IosMobileRegex.IsMatch(ua.ToString()));
            }            

            string contentType = string.Empty;

            // Note: We are not setting the content length because the CompressionHandler will compress the stream.
            // Specifying the length here will cause the browser to hang as the actual data it
            // receives (as it is compressed) will be less than the specified content length.
            FileDetails dbFileType = FileRepositoryHelper.GetStreamContentType(imageIdRef);
            if (dbFileType != null)
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = dbFileType.Filename
                };

                // Found that specifying specific mime types on IOS mobile
                // causes safari to raise errors.
                // However, found that on Android the download
                // manager requires the mime type so that the downloaded
                // file can be opened by tapping.
                if (!isIosDevice)
                {
                    if (!string.IsNullOrWhiteSpace(dbFileType.ContentType))
                    {
                        contentType = dbFileType.ContentType;
                    }
                    else
                    {
                        // Attempt to get a mime type from the file name
                        if (!string.IsNullOrWhiteSpace(dbFileType.Filename))
                        {
                            contentType = MimeMapping.GetMimeMapping(dbFileType.Filename);
                        }
                    }
                }                
            }            
            
            // If we don't have a content type, fallback to the generic one.            
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = "application/octet-stream";
            }

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            return response;
		}
	}
}