// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Services.ImageManager;
using EDC.SoftwarePlatform.WebApi.Controllers.FileUpload;
using EDC.ReadiNow.Core.Cache;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImageManager
{
	/// <summary>
	/// </summary>
	[RoutePrefix( "data/v1/image" )]
	public class ImageController : ApiController
	{
		// The Default cache max age in seconds.
		private const int DefaultCacheMaxAgeSec = 86400;

		#region Public Methods

		/// <summary>
		///     Downloads the image.
		/// </summary>
		/// <param name="imageId">The image id.</param>
		/// <returns></returns>
		[Route( "download/{imageId}" )]
        [HttpGet]
		public HttpResponseMessage GetDownloadImage( string imageId )
		{
            using (Profiler.Measure("ImageController.GetDownloadImage"))
            {
                EntityRef imageIdRef = WebApiHelpers.GetIdWithDashedAlias(imageId);

                var imageInterface = new ImageInterface();
                imageInterface.PreloadImage( imageIdRef );

                var imageFileType = ReadiNow.Model.Entity.Get<ImageFileType>(imageIdRef);

                if (imageFileType == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }                

                string fileName = imageFileType.Name;
                string fileExtension = imageFileType.FileExtension;

                string extension = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension))
                {
                    fileName = string.Format("{0}{1}", fileName, fileExtension);
                }

                Stream stream = imageInterface.GetImageDataStream(imageIdRef);
                if (stream == null)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };                

                response.Content.Headers.ContentType = new MediaTypeHeaderValue(GetImageMediaType(fileExtension));

                // Note: We are not setting the content length because the CompressionHandler will compress 
                // the stream . Specifying the length here will cause the browser to hang as the actual data it
                // receives (as it is compressed) will be less than the specified content length.
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName,
                };

                return response;
            }
		}

		/// <summary>
		///     Gets the image.
		/// </summary>
		/// <param name="imageId">The image id.</param>
		/// <returns></returns>
        [Route( "{imageId}" )]
        [HttpGet]
        public HttpResponseMessage GetImage( string imageId )
        {
            using ( Profiler.Measure( "ImageController.GetImage" ) )
            {
                EntityRef imageIdRef = WebApiHelpers.GetIdWithDashedAlias( imageId );

                var imageInterface = new ImageInterface( );
                imageInterface.PreloadImage( imageIdRef );

                using ( CacheManager.ExpectCacheHits( ) )
                {
                    HttpResponseMessage response = FileController.GetFileForRequest( Request, imageIdRef, GetImageEtag );

                    switch ( response.StatusCode )
                    {
                        case HttpStatusCode.OK:
                            {                                
                                var imageFileType = ReadiNow.Model.Entity.Get<ImageFileType>(imageIdRef);

                                response.Headers.CacheControl = new CacheControlHeaderValue
                                {
                                    MustRevalidate = true,
                                    MaxAge = GetCacheMaxAge( )
                                };
                                response.Content.Headers.ContentType = new MediaTypeHeaderValue( GetImageMediaType(imageFileType.FileExtension ) );
                            }
                            break;
                        case HttpStatusCode.NotModified:
                            response.Headers.CacheControl = new CacheControlHeaderValue
                            {
                                MustRevalidate = true,
                                MaxAge = GetCacheMaxAge( )
                            };
                            break;
                    }

                    return response;
                }
            }
        }

		[Route( "thumbnail/{imageId}" )]
        [HttpGet]
		public HttpResponseMessage GetImageThumbnail( string imageId )
		{
			return GetImageThumbnail( imageId, "console-smallThumbnail", "core-scaleImageProportionally" );
		}

		/// <summary>
		///     Gets the image thumbnail.
		/// </summary>
		/// <param name="imageId">The image id.</param>
		/// <param name="sizeId">The size id.</param>
		/// <param name="scaleId">The scale id.</param>
		/// <returns></returns>
		[Route( "thumbnail/{imageId}/{sizeId}/{scaleId}" )]
        [HttpGet]
		public HttpResponseMessage GetImageThumbnail( string imageId, string sizeId, string scaleId )
		{
            using (Profiler.Measure("ImageController.GetImageThumbnail"))
            {
                EntityRef imageIdRef = WebApiHelpers.GetIdWithDashedAlias(imageId);
                EntityRef sizeIdRef = WebApiHelpers.GetIdWithDashedAlias(sizeId);
                EntityRef scaleIdRef = WebApiHelpers.GetIdWithDashedAlias(scaleId);

                var imageInterface = new ImageInterface();                
                imageInterface.PreloadImageThumbnail( imageIdRef );

                ImageFileType thumbnailImage = imageInterface.GetImageThumbnail( imageIdRef, sizeIdRef, scaleIdRef );

                if (thumbnailImage == null )
                {
                    return new HttpResponseMessage( HttpStatusCode.NotFound );
                }

                string dataHash = thumbnailImage.FileDataHash;
                // Get the entity tag, which is the hash of the image data + dimensions + scaling
                string etag = string.Format( "\"{0}\"", imageInterface.GetImageThumbnailETag(dataHash, sizeIdRef, scaleIdRef ) );

                // If the request contains the same e-tag then return a not modified
                if ( Request.Headers.IfNoneMatch != null &&
                     Request.Headers.IfNoneMatch.Any( et => et.Tag == etag ) )
                {
                    var notModifiedResponse = new HttpResponseMessage( HttpStatusCode.NotModified );
                    notModifiedResponse.Headers.ETag = new EntityTagHeaderValue( etag, false );
                    notModifiedResponse.Headers.CacheControl = new CacheControlHeaderValue
                    {
                        MustRevalidate = true,
                        MaxAge = GetCacheMaxAge( )
                    };
                    return notModifiedResponse;
                }

                Stream imageDataStream;

                try
                {
                    imageDataStream = imageInterface.GetImageDataStream(dataHash);
                }
                catch (FileNotFoundException)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                if (imageDataStream == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                // Return the image
                var response = new HttpResponseMessage( HttpStatusCode.OK )
                {
                    Content = new StreamContent( imageDataStream )
                };
                response.Headers.ETag = new EntityTagHeaderValue( etag, false );
                response.Headers.CacheControl = new CacheControlHeaderValue
                {
                    MustRevalidate = true,
                    MaxAge = GetCacheMaxAge( )
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue( GetImageMediaType(thumbnailImage.FileExtension ) );

                return response;
            }
		}

		#endregion

		#region Non-Public Methods

		/// <summary>
		///     Gets the cache max age.
		/// </summary>
		/// <returns></returns>
		private static TimeSpan GetCacheMaxAge( )
		{
			// Start with the default value
			int maxAgeSec = DefaultCacheMaxAgeSec;
			// Get the value from the configuration file
			string maxAgeSecSettingValue = ConfigurationManager.AppSettings[ "ImageControllerCacheMaxAgeSec" ];

			// Try parse the configuration value if we have one
			if ( maxAgeSecSettingValue != null )
			{
				if ( !int.TryParse( maxAgeSecSettingValue, out maxAgeSec ) )
				{
					maxAgeSec = DefaultCacheMaxAgeSec;
				}
			}

			return TimeSpan.FromSeconds( maxAgeSec );
		}


		private static string GetImageEtag( EntityRef imageRef )
		{
            var imageFileType = ReadiNow.Model.Entity.Get<ImageFileType>(imageRef);

            // Get the entity tag, which is the hash of the image data
            var etag = string.Format( "\"{0}\"", imageFileType.FileDataHash );

			return etag;
		}

		/// <summary>
		///     Gets the type of the image media.
		/// </summary>
		/// <param name="fileExtension">The file extension.</param>
		/// <returns></returns>
		private static string GetImageMediaType( string fileExtension )
		{
            string mediaType = string.Empty;
                        
            if (!string.IsNullOrEmpty(fileExtension))
            {
                if (fileExtension.ToLower().EndsWith("svg"))
                {
                    return "image/svg+xml";
                }

                mediaType = MimeMapping.GetMimeMapping(string.Format("Image{0}", fileExtension));
            }

            // Fall back to unknown binary data
            if (string.IsNullOrWhiteSpace(mediaType))
            {
                mediaType = "application/octet-stream";
            }

            return mediaType;
		}

		#endregion
	}
}