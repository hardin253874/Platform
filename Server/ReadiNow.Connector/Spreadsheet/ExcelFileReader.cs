// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Reads the records out of a CSV file.
    /// </summary>
    class ExcelFileReader : IDataFile
    {
        private readonly Stream _stream;
        private readonly DataFileReaderSettings _settings;
        private SpreadsheetDocument _document;
        private readonly string _sheetId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="excelFile">The sheet to read.</param>
        /// <param name="settings">Processing settings.</param>
        internal ExcelFileReader( Stream excelFile, DataFileReaderSettings settings )
        {
            if ( excelFile == null )
                throw new ArgumentNullException( "excelFile" );
            if ( settings == null )
                throw new ArgumentNullException( "settings" );

            _stream = excelFile;
            _settings = settings;
            _document = SpreadsheetDocument.Open( _stream, false );
            _sheetId = _settings.SheetId;

            if ( _sheetId == null )
            {
                SheetInfo firstSheet = GetSheets( )?.FirstOrDefault( );
                if ( firstSheet != null )
                    _sheetId = firstSheet.SheetId;
            }
        }

        /// <summary>
        /// Read the data records.
        /// </summary>
        /// <returns>Enumeration of record readers.</returns>
        public IEnumerable<IObjectReader> GetObjects( )
        {
            // Skip non-data lines
            var rows = GetRows( _settings.FirstDataRowNumber, _settings.LastDataRowNumber );

            return rows.Select( GetReaderForRow );
        }

        /// <summary>
        /// Return a list of sheets/parts. Or null if not applicable.
        /// </summary>
        /// <returns>List of sheets.</returns>
        public IReadOnlyList<SheetInfo> GetSheets( )
        {
            var result = new List<SheetInfo>( );

            IEnumerable<Sheet> sheets = _document.WorkbookPart.Workbook.GetFirstChild<Sheets>( ).Elements<Sheet>( );

            foreach ( Sheet sheet in sheets )
            {
                string sheetId = sheet.Id?.Value;
                if ( !IsSheetValid( sheetId ) )
                    continue;

                var sheetInfo = new SheetInfo
                {
                    SheetName = sheet.Name,
                    SheetId = sheet.Name            // just use the names to refer to the sheets, because Excel enforces unique names within a document anyway
                };
                result.Add( sheetInfo );
            }
            return result;
        }


        /// <summary>
        ///     Check whether the sheet has any data on it.
        /// </summary>
        private bool IsSheetValid( string sheetId )
        {
            if ( sheetId == null )
                return false;
            var worksheetPart = ( WorksheetPart ) _document.WorkbookPart.GetPartById( sheetId );
            Worksheet workSheet = worksheetPart.Worksheet;

            var sheetData = workSheet.GetFirstChild<SheetData>( );

            IEnumerable<Row> rows = sheetData.Descendants<Row>( );
            // A sheet is valid for import if it contains any data.
            bool result = rows.Any( );
            return result;
        }


        /// <summary>
        /// Read the data records.
        /// </summary>
        /// <remarks>
        /// Reads the heading row, if specified, and also inspects several data rows to see what columns we can find.
        /// </remarks>
        /// <returns>Enumeration of record readers.</returns>
        public SheetMetadata ReadMetadata( )
        {
            const int dataRowsToInspect = 10;

            // Dictionary of columns and their headings
            Dictionary<string, string> columns = new Dictionary<string, string>( );

            // Heading cells
            bool getHeadings = _settings.HeadingRowNumber > 0;

            if ( getHeadings )
            {
                Row headingRow = GetRow( _settings.HeadingRowNumber );
                if ( headingRow != null )
                {
                    IEnumerable<Cell> headingCells = headingRow.Elements<Cell>( );

                    foreach ( Cell headingCell in headingCells )
                    {
                        string column = ExcelHelpers.GetColumnPart( headingCell.CellReference?.Value );
                        string title = ExcelHelpers.GetCellText( _document, headingCell );
                        if ( string.IsNullOrEmpty( title ) )
                            continue;
                        columns[ column ] = title;
                    }
                }
            }

            // Then inspect several rows of data cells - to pick up any loose data values that don't have a heading
            // (but just use the cell reference as the title)

            IEnumerable<Row> rows = GetRows( _settings.FirstDataRowNumber, _settings.LastDataRowNumber ).Take( dataRowsToInspect );
            foreach ( Row row in rows )
            {
                IEnumerable<Cell> cells = row.Elements<Cell>( );

                foreach ( Cell cell in cells )
                {
                    string column = ExcelHelpers.GetColumnPart( cell.CellReference?.Value );
                    if ( !columns.ContainsKey( column ) )
                    {
                        string content = ExcelHelpers.GetCellText( _document, cell );
                        if ( string.IsNullOrEmpty( content ) )
                            continue;
                        columns [ column ] = column;
                    }
                }
            }

            // Build result
            SheetMetadata result = new SheetMetadata( );
            result.Fields = columns.Keys
                .OrderBy( colRef => colRef.Length ).ThenBy( colRef => colRef ) // make sure that 'Z' appears before 'AA', etc.
                .Select( colRef => new FieldMetadata
                {
                    Key = colRef,
                    Title = columns[ colRef ]?.Trim()
                } )
                .ToList( );

            return result;
        }


        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose( )
        {
            if ( _document != null )
            {
                _document.Dispose( );
                _document = null;
            }
            _stream.Dispose( );
        }

        private IEnumerable<Row> GetRows( int firstRowIndex, int? lastRowIndex )
        {
            if ( _sheetId == null )
                return Enumerable.Empty<Row>( );

            // Note: we are using the worksheet name as a unique ID, because Excel ensures it is unique within the document anyway.
            string nameToFind = _sheetId;
            Worksheet workSheet = ExcelHelpers.GetWorksheetByName( _document, nameToFind );
            SheetData sheetData = workSheet?.GetFirstChild<SheetData>( );

            IEnumerable<Row> rows = sheetData?.Descendants<Row>( )
                ?.Where( r => r.RowIndex?.Value >= firstRowIndex )
                .TakeWhile( r => lastRowIndex == null || r.RowIndex?.Value <= lastRowIndex )
                .Where( r => !ExcelHelpers.IsRowEmpty( _document, r ) );

            if ( rows == null )
                return Enumerable.Empty<Row>( );
            return rows;
        }


        private Row GetRow( int rowIndex )
        {
            Row row = GetRows( rowIndex, null ).FirstOrDefault( );
            if ( row == null || row.RowIndex != rowIndex )
                return null;
            return row;
        }


        /// <summary>
        /// Creates a reader for an Excel row.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private IObjectReader GetReaderForRow( Row row )
        {
            return new ExcelObjectReader( row, _document, _settings );
        }

    }
}
