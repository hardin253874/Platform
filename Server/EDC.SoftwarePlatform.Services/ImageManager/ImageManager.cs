// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using EDC.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Services.FileManager;

namespace EDC.SoftwarePlatform.Services.ImageManager
{
	/// <summary>
	///     Image Manager.
	/// </summary>
	public class ImageManager
	{
        /// <summary>
        /// Set of IDs that aren't images.
        /// </summary>
        private static ConcurrentDictionary<long, object> _invalidImageIds = new ConcurrentDictionary<long, object>( );

		#region Public Methods

		/// <summary>
		///     Creates a new image
		/// </summary>
		/// <param name="imageFileUploadId">The image file upload identifier.</param>
		/// <param name="imageEntityData">The image entity data.</param>
		/// <returns>The requested data.</returns>
		/// <exception cref="System.ArgumentNullException">@imageEntityData</exception>
		/// <exception cref="System.ArgumentException">@imageEntityData</exception>
		public EntityRef CreateImageFromExistingFile( long imageFileUploadId, EntityData imageEntityData )
		{
			if ( imageEntityData == null )
			{
				throw new ArgumentNullException( @"imageEntityData" );
			}

			// Create a new image entity
			var entityInfoService = new EntityInfoService( );
			var imageFile = entityInfoService.CreateEntity( imageEntityData ).Entity.AsWritable<ImageFileType>( );

			int width;
			int height;

			using ( Stream stream = GetImageDataStream( imageFileUploadId ) )
			{
				using ( Image image = Image.FromStream( stream ) )
				{
					width = image.Width;
					height = image.Height;
				}
			}
			var existingImage = Entity.Get<ImageFileType>( imageFileUploadId );

			imageFile.FileDataHash = existingImage.FileDataHash;
		    imageFile.FileExtension = existingImage.FileExtension;
			imageFile.ImageWidth = width;
			imageFile.ImageHeight = height;
			imageFile.Save( );

			return imageFile;
		}

		/// <summary>
		///     Creates a new image
		/// </summary>
		/// <param name="imageFileUploadId">The image file upload id.</param>
		/// <param name="imageEntityData">The image entity data.</param>
		/// <param name="fileExtension">The file extension.</param>
		/// <returns>
		///     The requested data.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public EntityRef CreateImageFromUploadedFile( string imageFileUploadId, EntityData imageEntityData, string fileExtension )
		{
			if ( string.IsNullOrEmpty( imageFileUploadId ) )
			{
				throw new ArgumentException( @"The imageFileUploadId is empty.", "imageFileUploadId" );
			}

			if ( imageEntityData == null )
			{
				throw new ArgumentNullException( @"imageEntityData" );
			}

			if ( string.IsNullOrEmpty( fileExtension ) )
			{
				throw new ArgumentNullException( @"fileExtension" );
			}

			ImageFileType imageFile;
			string filePath = string.Empty;

			try
			{
				// Create a new image entity
				var entityInfoService = new EntityInfoService( );
				imageFile = entityInfoService.CreateEntity( imageEntityData ).Entity.AsWritable<ImageFileType>( );

				var fileManagerService = new FileManagerService( );
				FileDetail fileDetails = fileManagerService.GetFileDetails( imageFileUploadId );

				filePath = fileDetails.FullName;

				int width;
				int height;

				using ( Image image = Image.FromFile( fileDetails.FullName ) )
				{
					width = image.Width;
					height = image.Height;
				}

			    string token;
				using ( var source = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
				{
                    token = FileRepositoryHelper.AddTemporaryFile(source);
				}

				imageFile.FileDataHash = token;
			    imageFile.FileExtension = FileHelper.GetFileExtension(filePath);
				imageFile.ImageWidth = width;
				imageFile.ImageHeight = height;
				imageFile.Save( );
			}
			finally
			{
				// Delete the underlying temp file
				if ( !string.IsNullOrEmpty( filePath ) )
				{
					File.Delete( filePath );
				}
			}

			return imageFile;
		}

		/// <summary>
		///     Determines if the image already exists.
		/// </summary>
		/// <param name="imageId">The image identifier.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		public bool DoesImageFileExist( long imageId )
		{
			var imageFile = Entity.Get<ImageFileType>( imageId );

			if ( imageFile == null || string.IsNullOrWhiteSpace(imageFile.FileDataHash))
			{
				return false;
			}

		    return FileRepositoryHelper.DoesFileExist(Factory.BinaryFileRepository, imageFile.FileDataHash);   
		}

		/// <summary>
		///     Gets the binary file stream.
		/// </summary>
		/// <param name="imageId">The image identifier.</param>
		/// <returns>Stream.</returns>
		/// <exception cref="System.ArgumentNullException">@imageFileDataHash</exception>
		public Stream GetImageDataStream( long imageId )
		{
			return FileRepositoryHelper.GetFileDataStreamForEntity( imageId );
		}        	

		/// <summary>
		///     Gets the image thumbnail.
		/// </summary>
		/// <param name="imageId">The image id.</param>
		/// <param name="sizeId">The size id.</param>
		/// <param name="scaleId">The scale id.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">The thumbnail size has invalid dimensions specified.</exception>
		public ImageFileType GetImageThumbnail( EntityRef imageId, EntityRef sizeId, EntityRef scaleId )
		{
            int thumbnailWidth;
            int thumbnailHeight;
            ImageScaleEnum imageScaleEnum;
            ThumbnailSizeEnum thumbnailSizeEnum;

            // Quickly fail on invalid IDs
            if ( _invalidImageIds.ContainsKey( imageId.Id ) )
            {
                throw new ArgumentException( "The image entity was not found." );
            }

            // We don't really need to secure thumbnail sizes and image scale in this context.
            using (new SecurityBypassContext())
            {
                // Get the size and scale entities
                thumbnailSizeEnum = Entity.Get<ThumbnailSizeEnum>(sizeId);
                imageScaleEnum = Entity.Get<ImageScaleEnum>(scaleId);

                if (thumbnailSizeEnum == null)
                    throw new ArgumentNullException("Invalid thumbnail size");

                if (imageScaleEnum == null)
                    throw new ArgumentNullException("Invalid image scale");

                // Get the width and height from the thumbnail size enum 
                thumbnailWidth = thumbnailSizeEnum.ThumbnailWidth ?? -1;
                thumbnailHeight = thumbnailSizeEnum.ThumbnailHeight ?? -1;
            }

            if (thumbnailHeight <= 0 || thumbnailWidth <= 0)
            {
                throw new ArgumentNullException("The thumbnail size has invalid dimensions specified.");
            }

            // Get the requested image
            var originalImageEntity = Entity.Get<ImageFileType>( imageId );

            // Handle missing images
			if ( originalImageEntity == null )
			{
                // Verify if an admin can access it.
                using ( new SecurityBypassContext( ) )
                {
                    var unsecuredEntity = Entity.Get<ImageFileType>( imageId );
                    if ( unsecuredEntity == null )
                    {
                        _invalidImageIds [ imageId.Id ] = null;
                    }
                }
				throw new ArgumentException( "The image entity was not found." );
			}

			// Allow user access to thumbnails if they have access to the image
			using ( new SecurityBypassContext( ) )
			{                
                // SVG can scale themselves fine as they are vector based. No need for thumbs.
                if (!string.IsNullOrEmpty(originalImageEntity.FileExtension) &&
                    originalImageEntity.FileExtension.ToLower().EndsWith(".svg"))
                {
                    return originalImageEntity;
                }

                ThumbnailFileType thumbnail;

                // Create a thumbnail create key for this combination of image, size and scale.   
                ThumbnailCreationKey createKey = new ThumbnailCreationKey(originalImageEntity.Id, thumbnailSizeEnum.Id, imageScaleEnum.Id);
                ThumbnailCreationValue createValue = null;

                try
                {                                        
                    // Get or add a create value
                    createValue = _activeThumbnailCreations.GetOrAdd(createKey, k => new ThumbnailCreationValue());

                    // Increment the ref count
                    createValue.IncrementRefCount();

                    // Lock on this syncroot.
                    // Note: we are locking on this create value. This will only block parallel requests for the same image, size and scale combinations.                    
                    lock (createValue.SyncRoot)
                    {
                        // Find the first thumbnail with a matching size and scale
                        thumbnail = originalImageEntity.ImageHasThumbnails.FirstOrDefault(tt =>
                        {
                            int height = tt.ImageHeight ?? -1;
                            int width = tt.ImageWidth ?? -1;

                            if (height == -1 ||
                                 width == -1 ||
                                 width != thumbnailWidth ||
                                 height != thumbnailHeight)
                            {
                                // The thumbnail width does not match.
                                return false;
                            }

                            long thumbnailScalingId = tt.ThumbnailScaling != null ? tt.ThumbnailScaling.Id : -1;

                            if (thumbnailScalingId == -1 || thumbnailScalingId != imageScaleEnum.Id)
                            {
                                // The thumbnail scale does not match.
                                return false;
                            }

                            return true;
                        });

                        if (thumbnail == null || (string.IsNullOrEmpty(thumbnail.FileDataHash)))
                        {
                            using (Stream thumbnailStream = CreateThumbnailImage(originalImageEntity, thumbnailWidth, thumbnailHeight, imageScaleEnum.Alias))
                            {                                
                                // Create the thumbnail entity and add the file to the database
                                ThumbnailFileType thumbnailFileType = CreateThumbnailImageEntity(thumbnailStream,
                                    originalImageEntity, originalImageEntity.FileExtension,
                                    thumbnailWidth, thumbnailHeight, imageScaleEnum);                                

                                return thumbnailFileType.As<ImageFileType>();
                            }
                        }
                    }
                }
                finally
                {
                    // Decrement the ref count and remove from the active creations dictionary.
                    if (createValue != null &&
                        createValue.DecrementRefCount() <= 0)
                    {
                        ThumbnailCreationValue removedValue;
                        _activeThumbnailCreations.TryRemove(createKey, out removedValue);                        
                    }                                                            
                }

				// Found a thumbnail. Return it.                                
				return thumbnail.As<ImageFileType>();
			}
		}

		#endregion

		#region Non-Public Methods

		/// <summary>
		///     Creates the thumbnail image.
		/// </summary>
		/// <param name="originalImageEntity">The original image entity.</param>
		/// <param name="thumbnailWidth">Width of the thumbnail.</param>
		/// <param name="thumbnailHeight">Height of the thumbnail.</param>
		/// <param name="scaleEnumAlias">The scale enum alias.</param>
		/// <returns></returns>
		private Stream CreateThumbnailImage( ImageFileType originalImageEntity, int thumbnailWidth, int thumbnailHeight, string scaleEnumAlias )
		{
			// No thumbnail was found.
			// Need to create one                                   
			var thumbnailImageMemoryStream = new MemoryStream( );

			// Get the original image from the database
			using ( Stream imageDataStream = GetImageDataStream( originalImageEntity.Id ) )
			{
				using ( Image originalImage = Image.FromStream( imageDataStream ) )
				{
					using ( Image thumbnailImage = new Bitmap( thumbnailWidth, thumbnailHeight ) )
					{
						using ( Graphics thumbnailGraphics = Graphics.FromImage( thumbnailImage ) )
						{
							switch ( scaleEnumAlias )
							{
								case "core:scaleImageToFit":
									thumbnailGraphics.CompositingQuality = CompositingQuality.HighQuality;
									thumbnailGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
									thumbnailGraphics.SmoothingMode = SmoothingMode.HighQuality;
									thumbnailGraphics.DrawImage( originalImage, new Rectangle( 0, 0, thumbnailWidth, thumbnailHeight ) );
									break;

								case "core:scaleImageProportionally":
									// Get the smaller of the ratio between height and width
									double aspectRatioWidth = thumbnailWidth / ( double ) originalImage.Width;
									double aspectRatioHeight = thumbnailHeight / ( double ) originalImage.Height;
									double aspectRatio = aspectRatioWidth < aspectRatioHeight ? aspectRatioWidth : aspectRatioHeight;

									// Calculate the target rectangle
									var targetRectangle = new Rectangle
									{
										X = ( int ) ( ( thumbnailWidth - ( originalImage.Width * aspectRatio ) ) / 2 ),
										Y = ( int ) ( ( thumbnailHeight - ( originalImage.Height * aspectRatio ) ) / 2 ),
										Width = ( int ) ( originalImage.Width * aspectRatio ),
										Height = ( int ) ( originalImage.Height * aspectRatio )
									};

									// Calculate the source rectangle
									var sourceRectangle = new Rectangle
									{
										X = 0,
										Y = 0,
										Width = originalImage.Width,
										Height = originalImage.Height
									};

									thumbnailGraphics.Clear( Color.Transparent );
									thumbnailGraphics.CompositingQuality = CompositingQuality.HighQuality;
									thumbnailGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
									thumbnailGraphics.SmoothingMode = SmoothingMode.HighQuality;
									thumbnailGraphics.DrawImage( originalImage, targetRectangle, sourceRectangle, GraphicsUnit.Pixel );
									break;

								case "core:cropImage":
									thumbnailGraphics.DrawImageUnscaled( originalImage, new Rectangle( 0, 0, thumbnailWidth, thumbnailHeight ) );
									break;
							}
						}

						// Save the thumbnail as the same format as the original
						thumbnailImage.Save( thumbnailImageMemoryStream, originalImage.RawFormat );
					}
				}
			}

			return thumbnailImageMemoryStream;
		}

		/// <summary>
		///     Creates the thumbnail image entity.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="originalImageEntity">The original image entity.</param>
		/// <param name="fileExtension">The file extension.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="scaleEnum">The scale enum.</param>
		/// <returns></returns>
		private ThumbnailFileType CreateThumbnailImageEntity( Stream stream, ImageFileType originalImageEntity, string fileExtension,
			int width, int height, ImageScaleEnum scaleEnum )
		{
			using ( DatabaseContext context = DatabaseContext.GetContext( true ) )
			{
				stream.Position = 0;

			    string dataHash = FileRepositoryHelper.AddTemporaryFile(stream);				

				var thumbnailEntity = new ThumbnailFileType( );

				string thumbnailName = originalImageEntity.Name;
				if ( string.IsNullOrEmpty( thumbnailName ) )
				{
					thumbnailName = "Thumbnail";
				}
				else
				{
					thumbnailName += " Thumbnail";
				}

				thumbnailEntity.Name = thumbnailName;
				thumbnailEntity.FileDataHash = dataHash;
			    thumbnailEntity.FileExtension = fileExtension;
				thumbnailEntity.ImageWidth = width;
				thumbnailEntity.ImageHeight = height;
				thumbnailEntity.ThumbnailScaling = scaleEnum;
				thumbnailEntity.IsThumbnailForImage = originalImageEntity;
				thumbnailEntity.Save( );

				context.CommitTransaction( );

				return thumbnailEntity;
			}
		}

        #endregion

        #region Active Thumbnail Creation Dictionary

        /// <summary>
        /// Key into the active thumbnail creation dictionary.
        /// </summary>
        private class ThumbnailCreationKey : Tuple<long, long, long>
        {
            /// <summary>
            /// Constructs a thumbnail creation key.
            /// </summary>
            /// <param name="imageId">The image id.</param>
            /// <param name="sizeId">The size id.</param>
            /// <param name="scaleId">The scale id.</param>
            public ThumbnailCreationKey(long imageId, long sizeId, long scaleId) : base(imageId, sizeId, scaleId)
            {
            }
        }


        /// <summary>
        /// Thumbnail creation value.
        /// </summary>
        private class ThumbnailCreationValue
        {
            /// <summary>
            /// Constructs a thumbnail creation value.
            /// </summary>            
            public ThumbnailCreationValue()
            {
                SyncRoot = new object();
                _refCount = 0;
            }

            /// <summary>
            /// Increments the ref count for this object
            /// and returns the incremented value.
            /// </summary>
            /// <returns></returns>
            public int IncrementRefCount()
            {
                return Interlocked.Increment(ref _refCount);
            }

            /// <summary>
            /// Decrements the ref count for this object
            /// and returns the decremented value.
            /// </summary>
            /// <returns></returns>
            public int DecrementRefCount()
            {
                return Interlocked.Decrement(ref _refCount);
            }

            /// <summary>
            /// The syncroot for this object.
            /// </summary>
            public object SyncRoot { get; private set; }

            /// <summary>
            /// The reference count;
            /// </summary>
            private int _refCount;   
        }

        /// <summary>
        /// Used to track active thumbnail creations.
        /// When there are multiple create requests for the same thumbnail this dictionary is used to make non-active requests wait for the active one before continuing.
        /// Note: This will prevent parallel creates in the same process. Does not handle parallel creates across multiple front ends.
        /// Note: Parallel creates across multiple front ends should be rare. To implement this edge cause would involve implementing some sort of database lock which would affect all requests,
        /// so we are not doing this now.
        /// </summary>
        private static ConcurrentDictionary<ThumbnailCreationKey, ThumbnailCreationValue> _activeThumbnailCreations = new ConcurrentDictionary<ThumbnailCreationKey, ThumbnailCreationValue>();

        #endregion
    }
}