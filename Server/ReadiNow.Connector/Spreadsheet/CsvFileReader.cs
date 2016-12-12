// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Reads the records out of a CSV file.
    /// </summary>
    class CsvFileReader : IDataFile
    {
        private readonly Stream _csvStream;
        private readonly DataFileReaderSettings _settings;
        private TextFieldParser _parser;
        private TextReader _reader;
        private readonly ImportFormat _importFormat;

        private string[ ] _prevRowFields; // special hack to allow rows to be re-read
        private long _prevRowNumber = -1;
        private bool _returnPrevRowAgain;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="csvStream">The sheet to read.</param>
        /// <param name="settings">Processing settings.</param>
        internal CsvFileReader(Stream csvStream, DataFileReaderSettings settings)
        {
            if (csvStream == null)
                throw new ArgumentNullException("csvStream");
            if (settings == null)
                throw new ArgumentNullException("settings");

            // Strip zip flag (or others)
            _importFormat = settings.ImportFormat & ( ImportFormat.CSV | ImportFormat.Tab );
            if ( _importFormat != ImportFormat.CSV && settings.ImportFormat != ImportFormat.Tab )
                throw new ArgumentException( "settings" );

            _csvStream = csvStream;
            _settings = settings;
            OpenCsvFile( );
        }


        /// <summary>
        /// Read the data records.
        /// </summary>
        /// <returns>Enumeration of record readers.</returns>
        public IEnumerable<IObjectReader> GetObjects()
        {
            AdvanceToLine( _settings.FirstDataRowNumber );

            while ( !_parser.EndOfData )
            {
                CsvObjectReader objectReader;

                try
                {
                    long lineNumber;
                    string [ ] values = ReadFields( out lineNumber );

                    if ( _settings.LastDataRowNumber != null && lineNumber > _settings.LastDataRowNumber )
                        yield break;

                    objectReader = new CsvObjectReader( values, lineNumber, _settings );
                }
                catch ( MalformedLineException )
                {
                    objectReader = null;
                    // TODO: Handle exception somehow
                }
                if ( objectReader != null )
                {
                    yield return objectReader;
                }
            }
        }


        /// <summary>
        /// Return a list of sheets/parts. Or null if not applicable.
        /// </summary>
        /// <returns>Returns null - not applicable to CSV.</returns>
        public IReadOnlyList<SheetInfo> GetSheets( )
        {
            return null;
        }


        /// <summary>
        /// Read the data records.
        /// </summary>
        /// <returns>Enumeration of record readers.</returns>
        public SheetMetadata ReadMetadata()
        {
            string[] values = null;
            SheetMetadata result = new SheetMetadata();

            bool getHeadings = _settings.HeadingRowNumber > 0;

            // Heading row specified
            int startingRow = getHeadings ? _settings.HeadingRowNumber : _settings.FirstDataRowNumber;

            AdvanceToLine( startingRow );

            // Read row
            if ( !_parser.EndOfData )
            {
                long lineNumber;
                values = ReadFields( out lineNumber );
                result.Fields = new List<FieldMetadata>( );
            }

            if ( values == null )
            {
                result.Fields = new List<FieldMetadata>( );
            }

            // Create metadata
            if (values == null)
            {
                result.Fields = new List<FieldMetadata>();
            }
            else
            {
                result.Fields =
                    values.Select((value, index) => new FieldMetadata
                    {
                        Key = ColumnIndexToKey(index),
                        Title = getHeadings ? value : ColumnIndexToKey(index)
                    }).ToList();
            }

            return result;
        }


        /// <summary>
        /// Gets a CSV parser.
        /// </summary>
        /// <returns></returns>
        private void OpenCsvFile( )
        {
            _reader = new StreamReader( _csvStream );

            _parser = new TextFieldParser( _reader )
            {
                TrimWhiteSpace = true,
                TextFieldType = FieldType.Delimited
            };

            if ( _importFormat == ImportFormat.Tab )
                _parser.Delimiters = new [ ] { "\t" };
            if ( _importFormat == ImportFormat.CSV )
                _parser.Delimiters = new[] { "," };
        }

        /// <summary>
        /// Wrap read-fields so that header row can be returned twice.
        /// </summary>
        /// <param name="lineNumber">The line number that was read.</param>
        /// <returns></returns>
        private string[ ] ReadFields( out long lineNumber )
        {
            if ( _returnPrevRowAgain )
            {
                _returnPrevRowAgain = false;
            }
            else
            {
                // This will return the line number, or if there are preceeding blanks then the line number of the first blank after the preceeding line.
                // It's not actually possible to reliably get LineNumber out of TextFieldParser if the data contains blank rows.
                // See https://social.msdn.microsoft.com/Forums/en-US/4aa1ef96-ca6e-44d9-82b5-2a49947cccf6/textfieldparser-cannot-reliably-return-the-linenumber?forum=netfxbcl
                _prevRowNumber = _parser.LineNumber;
                _prevRowFields = _parser.ReadFields( );
            }
            lineNumber = _prevRowNumber;
            return _prevRowFields;
        }


        /// <summary>
        /// Line number to advance to.
        /// </summary>
        /// <param name="lineNumber">Line number</param>
        private void AdvanceToLine( int lineNumber )
        {
            if ( lineNumber == _prevRowNumber )
            {
                _returnPrevRowAgain = true;
                return;
            }
                
            if ( _parser.LineNumber > lineNumber )
                throw new InvalidOperationException( "Reader has already advanced past the required line." );

            // Returns -1 if no more data
            while ( _parser.LineNumber < lineNumber && _parser.LineNumber != -1 )
            {
                _parser.ReadLine( );
            }
        }


        /// <summary>
        /// Converts a column key (one-based-string) to the zero-based column index.
        /// </summary>
        /// <param name="key">Key string</param>
        /// <returns>Zero-based column index, or -1 if the key is not recognizable.</returns>
        public static int ColumnKeyToIndex(string key)
        {
            int columnNumber;
            if (!int.TryParse(key, out columnNumber))
                return -1;
            return columnNumber - 1;
        }


        /// <summary>
        /// Converts an index to its column key.
        /// </summary>
        /// <param name="index">Zero-based column index</param>
        /// <returns>Key string</returns>
        public static string ColumnIndexToKey(int index)
        {
            return (index + 1).ToString();
        }


        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _parser?.Dispose();
            _reader?.Dispose();
            
            _csvStream.Dispose();
        }
    }
}
