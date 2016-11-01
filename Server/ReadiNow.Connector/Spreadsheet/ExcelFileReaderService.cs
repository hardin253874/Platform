// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using RN = ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Service for reading a CSV file.
    /// </summary>
    internal class ExcelFileReaderService : IDataFileReaderService
    {
        public IDataFile OpenDataFile(Stream excelSpreadsheet, DataFileReaderSettings settings)
        {
            if (excelSpreadsheet == null)
                throw new ArgumentNullException("excelSpreadsheet");
            if (settings == null)
                throw new ArgumentNullException("settings");

            try
            {
                return new ExcelFileReader( excelSpreadsheet, settings );
            }
            catch ( System.IO.FileFormatException ex )
            {
                throw new RN.FileFormatException( ex.Message );
            }
        }
    }
}
