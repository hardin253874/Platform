// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
            var isDocument = fileType.Is<Document>() || fileType.Is<DocumentRevision>();

            var fileRepository = isDocument ? Factory.DocumentFileRepository : Factory.BinaryFileRepository;

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


        /// <summary>
        ///     Checks that the file extension is in the white-list.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="expectedType">image, xml, or null</param>
        public static bool CheckFileExtensionIsValid( string fileName, string expectedType )
        {
            if ( string.IsNullOrEmpty( fileName ) )
                throw new ArgumentNullException( nameof( fileName ) );

            string checkExt = Path.GetExtension( fileName );
            if ( string.IsNullOrEmpty( checkExt ) )
                return false;

            // Check against blacklist
            // (Since the whitelist is configurable, back it up with an additional black-list check)
            checkExt = checkExt.ToLowerInvariant( );
            if ( BlackListExtensions.Contains( checkExt ) )
            {
                EventLog.Application.WriteWarning( $"File upload blocked by system due to file extension: {fileName}" );
                return false;
            }

            // Check purpose-specific whitelist
            if ( expectedType == "image" )
            {
                bool allowed = ImageWhiteList.Contains( checkExt );
                return allowed;
            }
            if ( expectedType == "import" )
            {
                bool allowed = ImportWhiteList.Contains( checkExt );
                return allowed;
            }
            if ( expectedType == "xml" )
            {
                bool allowed = checkExt == ".xml";
                return allowed;
            }

            // Check tenant document whitelist
            IEnumerable<DocumentType> docTypes = Entity.GetInstancesOfType<DocumentType>( false, "extension, isOfType.id" );

            foreach ( DocumentType docType in docTypes )
            {
                string extensions = docType.Extension;
                if ( string.IsNullOrEmpty( extensions ) )
                    continue;

                string[ ] extList = extensions.Split( ';' );
                foreach ( string ext in extList )
                {
                    string curExt = Path.GetExtension( ext.Trim() );

                    if ( string.Compare( checkExt, curExt, StringComparison.OrdinalIgnoreCase ) == 0 )
                        return true;
                }
            }
            EventLog.Application.WriteWarning( $"File upload blocked by whitelist due to file extension: {fileName}" );
            return false;
        }

        /// <summary>
        /// BlackList of files that cannot be uploaded. Space delimited.
        /// Whitelist processing is also performed, but can be configured by tenant so blacklist is also provided for additional protection.
        /// </summary>
        private static readonly string[ ] BlackListExtensions = ".application .bat .cmd .com .cpl .exe .gadget .hta .inf .jar .js .jse .lnk .msc .msh .msh1 .msh1xml .msh2 .msh2xml .mshxml .msi .msp .pif .ps1 .ps1xml .ps2 .ps2xml .psc1 .psc2 .reg .scf .scr .vb .vbe .vbs .ws .wsc .wsf .wsh".Split( ' ' );

        /// <summary>
        /// Whitelist of acceptable image file formats.
        /// </summary>
        private static readonly string[] ImageWhiteList = ".png .jpg .jpeg .svg".Split( ' ' );

        /// <summary>
        /// Whitelist of acceptable image file formats.
        /// </summary>
        private static readonly string[] ImportWhiteList = ".xlsx .csv .txt".Split( ' ' );
    }
}