// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using EDC.ReadiNow.IO;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Handles aspects of the file upload that relate to initial examination of the sheet.
    /// </summary>
    public class SpreadsheetInspector : ISpreadsheetInspector
    {
        private readonly Func<ImportFormat, IDataFileReaderService> _readerActivator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="readerActivator">Injected function that can activate the correct type of reader.</param>
        public SpreadsheetInspector( Func<ImportFormat, IDataFileReaderService> readerActivator )
        {
            if ( readerActivator == null )
                throw new ArgumentNullException( nameof( readerActivator ) );

            _readerActivator = readerActivator;
        }


        /// <summary>
        ///     Get spreadsheet Information from the imported file.
        /// </summary>
        /// <param name="fileUploadId">
        ///     File upload Id.
        /// </param>
        /// <param name="fileFormat">
        ///     Imported file fileFormat ( Excel or CSV)
        /// </param>
        /// <returns>Spreadsheet info
        /// </returns>
        public SpreadsheetInfo GetSpreadsheetInfo( string fileUploadId, ImportFormat fileFormat )
        {
            // Get service
            IDataFileReaderService service = _readerActivator( fileFormat );

            // Load info about sheets
            IReadOnlyList<SheetInfo> sheets;
            using ( Stream stream = FileRepositoryHelper.GetTemporaryFileDataStream( fileUploadId ) )
            {
                // Settings
                DataFileReaderSettings settings = new DataFileReaderSettings
                {
                    ImportFormat = fileFormat
                };

                IDataFile dataFile = service.OpenDataFile( stream, settings );
                sheets = dataFile.GetSheets( );
            }

            var spreadsheetInfo = new SpreadsheetInfo
            {
                ImportFileFormat = fileFormat,
                SheetCollection = sheets
            };
            return spreadsheetInfo;
        }


        /// <summary>
        ///     Get sheet info from the imported spreadsheet.
        /// </summary>
        /// <param name="fileUploadId">File upload Id.</param>
        /// <param name="sheetId">Selected sheet info.</param>
        /// <param name="headerRowNo">Header row no.</param>
        /// <param name="dataRowNo">Data row number to start reading data.</param>
        /// <param name="lastRowNo">Optional last row number to read.</param>
        /// <param name="fileFormat">Imported file fileFormat ( Excel or CSV)</param>
        /// <returns>
        ///     Sheet Info.
        /// </returns>
        public SampleTable GetSampleTable( string fileUploadId, ImportFormat fileFormat, string sheetId, int headerRowNo, int dataRowNo, int? lastRowNo )
        {
            // Get service
            IDataFileReaderService service = _readerActivator( fileFormat );

            // Settings
            DataFileReaderSettings settings = new DataFileReaderSettings
            {
                ImportFormat = fileFormat,
                HeadingRowNumber = headerRowNo,
                FirstDataRowNumber = dataRowNo,
                LastDataRowNumber = lastRowNo,
                SheetId = sheetId
            };

            // Open stream
            using ( Stream stream = FileRepositoryHelper.GetTemporaryFileDataStream( fileUploadId ) )
            {
                // Build sample
                SampleDataCreator creator = new SampleDataCreator( );
                SampleTable sampleTable = creator.CreateSample( stream, settings, service );

                // Trim titles
                foreach ( var col in sampleTable.Columns )
                    col.Name = col.Name?.Trim( ) ?? "";

                return sampleTable;
            }
        }
    }
}
