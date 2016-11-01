// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ExcelHelpers
    {
        /// <summary>
        /// Get a visible worksheet sheet.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="worksheetName">The name of the worksheet, which Excel ensures is unique within the document.</param>
        /// <returns>The sheet, or null if it could not be found, or was hidden.</returns>
        internal static Worksheet GetWorksheetByName( SpreadsheetDocument document, string worksheetName )
        {
            if ( document == null )
                throw new ArgumentNullException( nameof( document ) );
            if ( string.IsNullOrEmpty( worksheetName ) )
                throw new ArgumentNullException( nameof( worksheetName ) );

            // Sheet - represents the element in Excel's internal table of contents
            // Worksheet - represents the actual content
            IEnumerable<Sheet> sheets = document.WorkbookPart?.Workbook?.GetFirstChild<Sheets>( )?.Elements<Sheet>( );
            if ( sheets == null )
                throw new Exception( "There was a problem with the internal structure of the Excel document." );

            // Note: we are using the Sheet Name as its ID (because Excel enforces unique names within a document, and it will be more convenient for customers)
            Sheet sheet = sheets.FirstOrDefault( s => s.Name == worksheetName );
            if ( sheet == null )
                return null;
            if ( sheet.Id == null || sheet.State != null && sheet.State.Value != SheetStateValues.Visible )
                return null;

            // Find the workbook
            var workbookPart = ( WorksheetPart ) document.WorkbookPart.GetPartById( sheet.Id.Value );
            Worksheet worksheet = workbookPart?.Worksheet;

            return worksheet;
        }


        /// <summary>
        /// Return the text in the specified cell.
        /// </summary>
        /// <param name="document">The document</param>
        /// <param name="cell">The cell</param>
        /// <returns>Its text</returns>
        internal static string GetCellText( SpreadsheetDocument document, Cell cell )
        {
            if ( cell == null )
                return null;
            if ( cell.CellValue == null )
                return null;

            string result = cell.CellValue.Text;

            if ( cell.DataType != null && cell.DataType.Value == CellValues.SharedString )
            {
                int stringIndex = int.Parse( result );
                SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                result = stringTablePart.SharedStringTable.ChildElements [ stringIndex ].InnerText;
            }

            return result;
        }

        /// <summary>
        /// Return the column part of a reference.
        /// </summary>
        /// <param name="cellReference">Cell reference such as A1</param>
        /// <returns>The column portion, such as A</returns>
        internal static string GetColumnPart( string cellReference )
        {
            if ( cellReference == null )
                return null;

            int i = 0;
            while ( !char.IsDigit( cellReference[ i ] ) )
            {
                i++;
                if ( i == cellReference.Length )
                    return cellReference; // assert false, contains no digits
            }

            string columnPart = cellReference.Substring( 0, i );
            return columnPart;
        }

        /// <summary>
        /// Returns true if a row is empty.
        /// </summary>
        /// <param name="document">The document object. (For shared string lookups)</param>
        /// <param name="row">The row to test.</param>
        /// <returns>True if all entries are </returns>
        internal static bool IsRowEmpty( SpreadsheetDocument document, Row row )
        {
            return row.Elements<Cell>( )
                .Select( cell => GetCellText( document, cell ) )
                .All( string.IsNullOrEmpty );
        }
    }
}
