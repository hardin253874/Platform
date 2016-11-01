// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using EDC.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.EventClasses
{
    class FileTypeEventTarget : IEntityEventSave
    {
        private class StreamProperties
        {            
            /// <summary>
            /// The size of the stream.
            /// </summary>
            public long FileSize;

            /// <summary>
            /// The width of the image if the stream represents an image.            
            /// </summary>
            public int ImageWidth;

            /// <summary>
            /// The height of the image if the stream represents an image.            
            /// </summary>
            public int ImageHeight;
        }


        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // I wish interfaces in C# had an optional modifier!
        }

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (FileType fileType in from entity in entities where entity.Is<FileType>() select entity.AsWritable<FileType>())
            {
                string dataHash = fileType.FileDataHash;
                if (string.IsNullOrWhiteSpace(dataHash))
                {
                    continue;
                }
                
                bool isImage = fileType.Is<ImageFileType>();                

                // Check to see if we have a relationship to a document type and if so then perform the migration into the
                // indexed table otherwise insert the data into the non-indexed table.
                IFileRepository fileRepository = isImage ? Factory.BinaryFileRepository : Factory.DocumentFileRepository;

                if (FileRepositoryHelper.DoesFileExist(fileRepository, dataHash))
                {
                    // File aleady exists
                    continue;
                }

                if (!FileRepositoryHelper.DoesFileExist(Factory.TemporaryFileRepository, dataHash))
                {
                    // File was not found
                    continue;
                }

                StreamProperties streamProperties;
                string token;

                try
                {
                    using (var stream = Factory.TemporaryFileRepository.Get(dataHash))
                    {
                        // Copy the file to its correct store
                        streamProperties = GetPropertiesFromStream(stream, isImage);
                        stream.Position = 0;

                        token = fileRepository.Put(stream);
                    }
                }
                catch (FileNotFoundException)
                {
                    // File was not found
                    continue;
                }                                
                
                Factory.TemporaryFileRepository.Delete(dataHash);

                if (isImage)
                {
                    // Set image properties
                    fileType.SetField(ImageFileType.ImageHeight_Field, streamProperties.ImageHeight);
                    fileType.SetField(ImageFileType.ImageWidth_Field, streamProperties.ImageWidth);
                }
                else
                {
                    Document document = fileType.As<Document>();

                    // Verify that we have the correct çurrent revision', as the current revision should have been 
                    // given the same temporary dataHash as the document. (This is done in DocumentEventTarget)
                    if (document?.CurrentDocumentRevision?.FileDataHash == dataHash)
                    {
                        document.CurrentDocumentRevision.FileDataHash = token;
                    }
                }
                
                // Apply the new file hash.
                fileType.FileDataHash = token;                
                fileType.Size = Convert.ToInt32(streamProperties.FileSize);
            }

            return false;
        }              

        private StreamProperties GetPropertiesFromStream(Stream stream, bool isImage)
        {
            StreamProperties streamProperties = new StreamProperties {FileSize = 0, ImageHeight = 0, ImageWidth = 0};

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                streamProperties.FileSize = memoryStream.Length;

                if (isImage)
                {
                    try
                    {
                        using (Image image = Image.FromStream(memoryStream))
                        {
                            streamProperties.ImageWidth = image.Width;
                            streamProperties.ImageHeight = image.Height;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.Application.WriteWarning("Failed to get image from stream. Image entity width and height will not be set. Exception {0}.", ex.ToString());

                        streamProperties.ImageWidth = 0;
                        streamProperties.ImageHeight = 0;
                    }
                }
                
                return streamProperties;
            }                                                                     
        }        
    }
}
