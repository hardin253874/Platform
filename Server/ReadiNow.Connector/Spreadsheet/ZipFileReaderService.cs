// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.IO.Compression;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Wraps another file reader service to support unzipping a .zip file.
    /// </summary>
    internal class ZipFileReaderService : IDataFileReaderService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerReaderService">Inner service</param>
        public ZipFileReaderService( IDataFileReaderService innerReaderService )
        {
            if ( innerReaderService == null )
                throw new ArgumentNullException( nameof( innerReaderService ) );

            InnerReaderService = innerReaderService;
        }


        /// <summary>
        /// The service that reads the single unzipped file.
        /// </summary>
        internal IDataFileReaderService InnerReaderService { get; }

        /// <summary>
        /// Unzip a zip file then use the inner reader to process it.
        /// </summary>
        /// <param name="zipStream">The zip file</param>
        /// <param name="settings">Settings for the inner provider.</param>
        /// <returns>Wrapped data file that can dispose both.</returns>
        public IDataFile OpenDataFile( Stream zipStream, DataFileReaderSettings settings )
        {
            if ( zipStream == null )
                throw new ArgumentNullException( nameof( zipStream ) );
            if ( settings == null )
                throw new ArgumentNullException( nameof( settings ) );

            if ( !zipStream.CanSeek )
                throw new InvalidOperationException( "Stream must be seekable" );

            ZipArchive archive = new ZipArchive( zipStream );
            if ( archive.Entries.Count != 1 )
                throw new FileFormatException( "Zip file must contain exactly one data." );

            Stream dataStream = archive.Entries[ 0 ].Open( );
            IDataFile dataFile = InnerReaderService.OpenDataFile( dataStream, settings );

            // Wrap so both get disposed
            ZipDataFile zipDataFile = new ZipDataFile( dataFile, zipStream );
            return zipDataFile;
        }
    }
}
