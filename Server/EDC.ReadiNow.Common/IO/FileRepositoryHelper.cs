// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Linq;
using EDC.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.IO
{
    /// <summary>
    /// File repository helper class.
    /// </summary>
    public static class FileRepositoryHelper
    {
        /// <summary>
        /// Returns true if the file with the given token exists in the given repository, false otherwise.
        /// </summary>
        /// <param name="repository">The file repository.</param>
        /// <param name="token">The file token.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public static bool DoesFileExist(IFileRepository repository, string token)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(@"token");
            }

            bool exists;

            try
            {
                using (repository.Get(token))
                {
                    exists = true;
                }
            }
            catch (FileNotFoundException)
            {
                exists = false;
            }

            return exists;
        }


        /// <summary>
        ///     Gets the file data stream for the given entity.
        /// </summary>
        /// <param name="fileRef">The image id.</param>
        /// <returns>The file data stream.</returns>
        public static Stream GetFileDataStreamForEntity(EntityRef fileRef)
        {
            if (fileRef == null)
            {
                throw new ArgumentNullException("fileRef");
            }

            var fileType = Entity.Get<FileType>(fileRef);

			var fileRepository = fileType.Is<Document>( ) ? Factory.DocumentFileRepository : Factory.BinaryFileRepository;

            return GetFileDataStreamForToken(fileRepository, fileType.FileDataHash);
        }


        /// <summary>
        /// Gets the file data stream for the given token from the given repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="token">The token.</param>
        /// <returns>The file data stream.</returns>
        public static Stream GetFileDataStreamForToken(IFileRepository repository, string token)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(@"token");
            }            

            try
            {
                MemoryStream stream;

                using (var fileStream = repository.Get(token))
                {
                    stream = new MemoryStream();
                    fileStream.CopyTo(stream);
                    stream.Position = 0;
                }

                return stream;
            }
            catch (FileNotFoundException ex)
            {
                EventLog.Application.WriteTrace("An error occurred getting file with token {0} from the file repository. Error {1}.", token, ex);
                return null;

            }                        
        }


        /// <summary>
        /// Gets the file data stream for the given token from the temporary repository.
        /// </summary>        
        /// <param name="token">The token.</param>
        /// <returns>The file data stream.</returns>
        public static Stream GetTemporaryFileDataStream(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(@"token");
            }

            return GetFileDataStreamForToken(Factory.TemporaryFileRepository, token);
        }


        /// <summary>
        /// Adds the specified file to the temporary file repository.
        /// </summary>
        /// <param name="stream">The file stream to add.</param>
        /// <returns>The token that refers to the added file.</returns>
        public static string AddTemporaryFile(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(@"stream");
            }

            return Factory.TemporaryFileRepository.Put(stream);
        }


        /// <summary>
        ///     Gets the type of the stream content.
        /// </summary>
        /// <param name="fileRef">The file reference.</param>
        /// <returns>System.String.</returns>
        public static FileDetails GetStreamContentType(EntityRef fileRef)
        {
            if (fileRef == null)
            {
                throw new ArgumentNullException("fileRef");
            }

            var fileType = Entity.Get<FileType>(fileRef);
            if (fileType == null || fileType.FileExtension == null)
            {
                return null;
            }

            var dbFileType = new FileDetails
            {
                Filename = string.Format("{0}.{1}", fileType.Name, fileType.FileExtension.Split('.').Last())
            };

            var documentType = fileType.DocumentFileType;
            if (documentType == null || documentType.MimeType == null)
            {
                return dbFileType;
            }

            dbFileType.ContentType = documentType.MimeType;
            return dbFileType;
        }
    }
}