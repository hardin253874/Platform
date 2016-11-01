// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Service for reading a CSV file.
    /// </summary>
    internal class CsvFileReaderService : IDataFileReaderService
    {
        public IDataFile OpenDataFile(Stream sheetFile, DataFileReaderSettings settings)
        {
            if (sheetFile == null)
                throw new ArgumentNullException("sheetFile");
            if (settings == null)
                throw new ArgumentNullException("settings");

            return new CsvFileReader(sheetFile, settings);
        }
    }
}
