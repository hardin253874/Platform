// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Wraps an IDataFile so that a zip stream can be disposed along with it.
    /// </summary>
    class ZipDataFile : IDataFile
    {
        private readonly IDataFile _innerDataFile;
        private readonly Stream _zipStream;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerDataFile">The inner data file.</param>
        /// <param name="zipStream">The zip stream.</param>
        public ZipDataFile( IDataFile innerDataFile, Stream zipStream )
        {
            if ( innerDataFile == null )
                throw new ArgumentNullException( nameof( innerDataFile ) );
            if ( zipStream == null )
                throw new ArgumentNullException( nameof( zipStream ) );
            _innerDataFile = innerDataFile;
            _zipStream = zipStream;
        }

        /// <summary>
        /// Dispose zip stream and wrapped file.
        /// </summary>
        public void Dispose( )
        {
            _innerDataFile.Dispose( );
            _zipStream.Dispose( );
        }

        /// <summary>
        /// Wrap GetObjects.
        /// </summary>
        public IEnumerable<IObjectReader> GetObjects( )
        {
            return _innerDataFile.GetObjects( );
        }

        /// <summary>
        /// Wrap GetSheets.
        /// </summary>
        public IReadOnlyList<SheetInfo> GetSheets( )
        {
            return _innerDataFile.GetSheets( );
        }

        /// <summary>
        /// Wrap ReadMetadata.
        /// </summary>
        public SheetMetadata ReadMetadata( )
        {
            return _innerDataFile.ReadMetadata( );
        }
    }
}
