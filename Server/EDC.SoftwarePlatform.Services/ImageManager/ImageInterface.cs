// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Text;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.Security;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Services.ImageManager
{
	/// <summary>
	/// </summary>
	public class ImageInterface
	{
		#region Public Methods

		/// <summary>
		///     Gets the image data stream.
		/// </summary>
		/// <param name="imageId">The image id.</param>
		/// <returns></returns>
		public Stream GetImageDataStream( EntityRef imageId )
		{
			return FileRepositoryHelper.GetFileDataStreamForEntity( imageId );
		}

		/// <summary>
		///     Gets the image data stream.
		/// </summary>
		/// <param name="imageFileDataHash">The image file data hash.</param>
		/// <returns></returns>
		public Stream GetImageDataStream( string imageFileDataHash )
		{
			return FileRepositoryHelper.GetFileDataStreamForToken(Factory.BinaryFileRepository, imageFileDataHash );
		}		

		/// <summary>
		///     Gets the image thumbnail.
		/// </summary>
		/// <param name="imageId">The image id.</param>
		/// <param name="sizeId">The size id.</param>
		/// <param name="scaleId">The scale id.</param>
		/// <returns></returns>
		public ImageFileType GetImageThumbnail( EntityRef imageId, EntityRef sizeId, EntityRef scaleId )
		{
			if ( imageId == null )
			{
				throw new ArgumentNullException( "imageId" );
			}

			if ( sizeId == null )
			{
				throw new ArgumentNullException( "sizeId" );
			}

			if ( scaleId == null )
			{
				throw new ArgumentNullException( "scaleId" );
			}

		    try
		    {
		        var imageManagerService = new ImageManager();
		        return imageManagerService.GetImageThumbnail(imageId, sizeId, scaleId);
		    }
		    catch (ArgumentException)
		    {
		        return null;
		    }		   
		}

		/// <summary>
		///     Gets the image thumbnail etag.
		/// </summary>
		/// <param name="imageFileDataHash">The data hash.</param>
		/// <param name="sizeId">The size id.</param>
		/// <param name="scaleId">The scale id.</param>
		/// <returns></returns>
		public string GetImageThumbnailETag( string imageFileDataHash, EntityRef sizeId, EntityRef scaleId )
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

            int thumbnailWidth;
            int thumbnailHeight;
		    using (new SecurityBypassContext())
		    {
                // not a security concern.
                var thumbnailSizeEnum = Entity.Get<ThumbnailSizeEnum>(sizeId);

                thumbnailWidth = thumbnailSizeEnum.ThumbnailWidth ?? -1;
                thumbnailHeight = thumbnailSizeEnum.ThumbnailHeight ?? -1;		        
		    }

			var hashInput = new StringBuilder( );
			hashInput.AppendFormat( "DataHash:{0}", imageFileDataHash );
			hashInput.AppendFormat( "Width:{0}", thumbnailWidth );
			hashInput.AppendFormat( "Height:{0}", thumbnailHeight );
			hashInput.AppendFormat( "Scale:{0}", scaleId.Id );

			return CryptoHelper.GetSha1Hash( hashInput.ToString( ) );
		}

        /// <summary>
        /// Preload all entity data to load an image.
        /// </summary>
        /// <param name="imageId"></param>
        public void PreloadImage( EntityRef imageId )
        {
            string query = "isOfType.id, name, fileDataHash, fileExtension, flags.id, documentFileType.id";
            var request = new EntityRequest( imageId, query, "Preload image" );
            BulkPreloader.Preload( request );
        }

        /// <summary>
        /// Preload all entity data to load an image.
        /// </summary>
        /// <param name="imageId"></param>
        public void PreloadImageThumbnail( EntityRef imageId )
        {
            string query = "isOfType.id, name, fileDataHash, fileExtension, flags.id, documentFileType.id, imageHasThumbnails.{ isOfType.id, thumbnailScaling.id, imageWidth, imageHeight, imageBackgroundColor }";
            var request = new EntityRequest( imageId, query, "Preload image" );
            BulkPreloader.Preload( request );
        }

		#endregion
	}
}