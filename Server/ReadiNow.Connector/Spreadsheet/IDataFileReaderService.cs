// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using ReadiNow.Connector.ImportSpreadsheet;
using RN = ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Interface for a service that can open data file such as spreadsheets.
    /// </summary>
    public interface IDataFileReaderService
    {
        /// <summary>
        /// Open a data file, such as a spreadsheet, for reading.
        /// </summary>
        /// <param name="dataFile"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="RN.FileFormatException">Thrown if file is invalid.</exception>
        IDataFile OpenDataFile( Stream dataFile, DataFileReaderSettings settings );
    }


    /// <summary>
    /// Settings for reading sheet data
    /// </summary>
    public class DataFileReaderSettings
    {
        public DataFileReaderSettings( )
        {
            ImportFormat = ImportFormat.CSV;
            HeadingRowNumber = 1;
            FirstDataRowNumber = 2;
            LastDataRowNumber = null;
        }

        /// <summary>
        /// Number of row that contains headings.
        /// </summary>
        /// <remarks>
        /// First row is 1; or set to 0 if no heading row.
        /// </remarks>
        public int HeadingRowNumber { get; set; }

        /// <summary>
        /// Number of row that contains first data.
        /// </summary>
        /// <remarks>
        /// First row is 1.
        /// </remarks>
        public int FirstDataRowNumber { get; set; }

        /// <summary>
        /// Last row to be included. Or null to read to end.
        /// </summary>
        /// <remarks>
        /// First row is 1.
        /// </remarks>
        public int? LastDataRowNumber { get; set; }

        /// <summary>
        /// Name of worksheet to import, if applicable.
        /// </summary>
        public string SheetId { get; set; }

        /// <summary>
        /// File format being imported.
        /// </summary>
        public ImportFormat ImportFormat { get; set; }

        /// <summary>
        /// Time Zone Info to use when importing 
        /// </summary>
        public TimeZoneInfo TimeZoneInfo { get; set; }
    }
}
