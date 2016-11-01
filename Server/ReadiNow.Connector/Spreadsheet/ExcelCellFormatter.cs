// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Converts an Excel cell to a formatted string that hopefully has the same textual represententation as in Excel.
    /// Used when extracting a numeric/date/etc column for importing into a string field. 
    /// </summary>
    internal class ExcelCellFormatter
    {
        static readonly CultureInfo DateTimeCulture = CultureInfo.GetCultureInfo( "en-AU" );

        // Same as for CSV, except include the exponent, because Excel sometimes uses it when representing numbers.
        const NumberStyles ExcelDecimalStyle =
                NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowThousands | NumberStyles.AllowExponent;

        private static readonly Regex DateFormatCodeRegex = new Regex( "dd|mm|yy|h|H|M" );

        // Remove color instructions like [Red] and [Blue]
        // If _ or * is encountered, remove it and the following character
        private static readonly Regex RemoveFormatCodeCruftRegex = new Regex( "\\[^]*]|[_*]." );

        internal static string CellToString(Cell cell, SpreadsheetDocument document)
        {
            if (cell == null)
                throw new ArgumentNullException("cell");
            if (document == null)
                throw new ArgumentNullException("document");

            if (cell.CellValue == null)
                return null;

            // Find the cell format
            CellFormat cellFormat = null;
            if (cell.StyleIndex != null)
            {
                cellFormat = (CellFormat)document.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ElementAt((int)cell.StyleIndex.Value);
            }

            // Convert cell based on data type
            if (cell.DataType != null)
            {
                switch (cell.DataType.Value)
                {
                    case CellValues.SharedString:
                        SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                        return stringTablePart.SharedStringTable.ChildElements[int.Parse(cell.CellValue.Text)].InnerText;
                    case CellValues.Boolean:
                        return cell.CellValue.Text == "1" ? "TRUE" : "FALSE";
                    case CellValues.Date:
                        return DateCellToString(cell, cellFormat);
                    case CellValues.Number:
                        return NumericCellToString(cell, cellFormat, document );
                    case CellValues.InlineString:
                        if (cell.HasChildren)
                            return cell.ChildElements[0].InnerText;
                        return string.Empty;
                }
            }

            // Else convert string based on style information
            if (cellFormat != null)
            {
                uint formatId = cellFormat.NumberFormatId;

                // Handle numeric styles
                if (formatId >= 0 && formatId <= 13 || formatId >= 37 && formatId <= 44 )
                {
                    return NumericCellToString(cell, cellFormat, document );
                }

                // Handle date styles
                if (formatId >= 14 && formatId <= 22)
                {
                    return DateCellToString(cell, cellFormat );
                }

                // Handle custom styles
                // Formats over 163 represent custom formats. Might be date or numeric.
                if (formatId > 163)
                {
                    return CustomCellToString(cell, cellFormat, document);
                }
            }

            // Finally, a cell with no formatting information may be a string, or it may be a floating point number that needs rounding.
            string cellText = cell.CellValue.Text;
            decimal dRes;
            if ( !TryParseDecimal( cellText, out dRes ) )
                return cellText;
            return dRes.ToString( "0.#########", CultureInfo.InvariantCulture );   // format string to show exactly whatever decimal digits are available
        }


        /// <summary>
        /// Convert a general field that has a numeric style applied to it to a string.
        /// </summary>
        /// <param name="cell">The cell</param>
        /// <param name="cellFormat">The cell format. Or null.</param>
        /// <param name="document">The document</param>
        /// <returns></returns>
        private static string NumericCellToString(Cell cell, CellFormat cellFormat, SpreadsheetDocument document )
        {
            // Find a numeric format pattern that matches the cell style
            string formatPattern = null;
            if (cellFormat != null && cellFormat.NumberFormatId.HasValue)
            {
                formatPattern = GetNumberFormatPattern(cellFormat.NumberFormatId.Value);
            }

            if ( formatPattern != null )
            {
                return CustomNumericCellToString( cell.CellValue.Text, formatPattern );
            }
            return CustomCellToString( cell, cellFormat, document );
        }


        /// <summary>
        /// Convert a general field that has a numeric style applied to it to a string.
        /// </summary>
        /// <param name="cell">The cell</param>
        /// <param name="cellFormat">The cell format</param>
        /// <returns></returns>
        private static string DateCellToString(Cell cell, CellFormat cellFormat)
        {
            // Find a numeric format pattern that matches the cell style
            string formatPattern = null;
            if ( cellFormat != null && cellFormat.NumberFormatId.HasValue )
            {
                formatPattern = GetNumberFormatPattern( cellFormat.NumberFormatId.Value );
            }

            if ( formatPattern != null )
            {
                return CustomDateCellToString( cell.CellValue.Text, formatPattern );
            }
            return cell.CellValue.Text;
        }


        /// <summary>
        /// Convert a custom formatted cell to a string.
        /// </summary>
        /// <param name="cell">The cell</param>
        /// <param name="cellFormat">The cell format.</param>
        /// <param name="document">The document</param>
        /// <returns></returns>
        private static string CustomCellToString( Cell cell, CellFormat cellFormat, SpreadsheetDocument document )
        {
            string cellText = cell.CellValue.Text;

            // Get format code
            if ( document.WorkbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats == null )
                return cellText;

            string formatCode =
                ( from i in document.WorkbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats.Elements<NumberingFormat>( )
                    where i.NumberFormatId.Value == cellFormat.NumberFormatId.Value
                    select i ).FirstOrDefault( )?.FormatCode;

            if ( string.IsNullOrEmpty( formatCode ) || formatCode == "General" )
                return cellText;

            // Detect date/time format codes
            if ( DateFormatCodeRegex.IsMatch( formatCode ) )
            {
                // Assume its a date.
                return CustomDateCellToString( cellText, formatCode );
            }

            // Attempt to treat as numeric
            //return NumericCellToString(cell, cellFormat);

            // Format the string using that style
            // Format the string using that style
            return CustomNumericCellToString( cellText, formatCode );
        }


        /// <summary>
        /// Convert a custom date cell to a string.
        /// </summary>
        /// <param name="cellText">The cell text</param>
        /// <param name="formatCode">The custom format code from Excel.</param>
        /// <returns>The formatted string</returns>
        private static string CustomDateCellToString( string cellText, string formatCode )
        {
            // Month versus minutes
            // If "m" or "mm" code is used immediately after the "h" or "hh" code(for hours) or immediately before the "ss" code(for seconds),
            // the application shall display minutes instead of the month.
            // https://msdn.microsoft.com/en-us/library/documentformat.openxml.spreadsheet.numberingformats(v=office.14).aspx
            bool hasDatePart = formatCode == null || formatCode.Contains( 'd' ) || formatCode.Contains( 'y' );
            bool hasTimePart = formatCode == null || formatCode.Contains( 'h' ) || formatCode.Contains( 'H' ) || formatCode.Contains( "m:ss" );
            if ( !hasDatePart && !hasTimePart )
            {
                hasDatePart = formatCode.Contains( 'm' );
            }

            var d = DateTime.FromOADate( Convert.ToDouble( cellText ) );

            if ( !hasDatePart )
                return d.ToString( "T", DateTimeCulture );  // long time pattern (for now)
            if ( !hasTimePart )
                return d.ToString( "d", DateTimeCulture );  // short date pattern (for now)
            else
                return d.ToString( "G", DateTimeCulture );  // general pattern (for now)
        }


        /// <summary>
        /// Convert a custom numeric cell to a string.
        /// </summary>
        /// <param name="cellText">The cell text</param>
        /// <param name="formatCode">The custom format code from Excel.</param>
        /// <returns>The formatted string</returns>
        private static string CustomNumericCellToString( string cellText, string formatCode )
        {
            decimal d;
            if ( !TryParseDecimal( cellText, out d ) )
                return cellText;

            // Decide which part of format code to use
            // Up to four sections of format codes can be specified. The format codes, separated by semicolons, define the formats for
            // positive numbers, negative numbers, zero values, and text, in that order. If only two sections are specified, the first
            // is used for positive numbers and zeros, and the second is used for negative numbers. If only one section is specified, it is used for all numbers.
            // https://msdn.microsoft.com/en-us/library/documentformat.openxml.spreadsheet.numberingformats.aspx
            string[ ] formatParts = formatCode.Split( ';' );
            string formatPart;
            if ( formatParts.Length >= 3 )
            {
                if ( d == 0 )
                    formatPart = formatParts[ 2 ];
                else if ( d < 0 )
                    formatPart = formatParts[ 1 ];
                else
                    formatPart = formatParts[ 0 ];
            }
            else if ( formatParts.Length == 2 )
            {
                if ( d < 0 )
                    formatPart = formatParts[ 1 ];
                else
                    formatPart = formatParts[ 0 ];
            }
            else
                formatPart = formatParts[ 0 ];

            // Interpret formatCode
            // http://www.exceltactics.com/definitive-guide-custom-number-formats-excel/#Asterisk
            if ( formatPart.Contains( '/' ) )
                formatPart = "0.000";   // we can't handle fractions

            // Remove color instructions like [Red] and [Blue], as well as whitespace instructions
            if ( formatPart.Contains( '[' ) || formatPart.Contains( '_' ) || formatPart.Contains( '*' ) )
                formatPart = RemoveFormatCodeCruftRegex.Replace( formatPart, "" );

            // In Excel, both ? and # are used to represent placeholders that are not shown if not present
            // The difference being that ? substitutes a space to give alignment. C# has no equivalent to ?
            formatPart = formatPart.Replace( '?', '#' );

            // In Excel, trailing commas can be used to automatically remove groups of thousands
            while ( formatPart.EndsWith( ",", StringComparison.InvariantCulture ) )
            {
                formatPart = formatPart.Substring( 0, formatPart.Length - 1 );
                d = d / 1000;
            }
            if ( formatPart.Contains( '-' ) )
            {
                d = Math.Abs( d );
            }

            // Trust that there are mercifully enough similarities between .Net and Excel for it to work in cases that people care about.
            string result = d.ToString( formatPart, CultureInfo.InvariantCulture );
            return result;
        }


        /// <summary>
        /// Map of Excel numeric format IDs to their equivelent C# format patterns.
        /// </summary>
        /// <param name="numberFormatId">Excel numeric format ID</param>
        /// <returns>C# numeric format pattern</returns>
        private static string GetNumberFormatPattern(uint numberFormatId)
        {
            // http://www.exceltactics.com/definitive-guide-custom-number-formats-excel/#Asterisk
            // http://stackoverflow.com/questions/4730152/what-indicates-an-office-open-xml-cell-contains-a-date-time-value

            switch ( numberFormatId)
            {
                case 1:
                    return "0";
                case 2:
                    return "0.00";
                case 3:
                    return "#,##0";
                case 4:
                    return "#,##0.00";
                case 8:
                    return "'$'#,##0.00;'-$'#,##0.00"; //"$#,##0.00;[Red]-$#,##0.00";
                case 9:
                    return "0%";
                case 10:
                    return "0.00%";
                case 11:
                    return "0.00E+00";
                case 12:
                    return "0.000"; //"#?/?";    // can't handle fractions
                case 13:
                    return "0.000"; //"#??/??";  // can't handle fractions
                case 14:
                    return "d/M/yyyy";
                case 15:
                    return "d-MMM-yy";
                case 16:
                    return "d-MMM";
                case 17:
                    return "MMM-yy";
                case 18:
                    return "h:mm tt";
                case 19:
                    return "h:mm:ss tt";
                case 20:
                    return "H:mm";
                case 21:
                    return "H:mm:ss";
                case 22:
                    return "d/M/yyyy H:mm";
                case 37:
                    return "#,##0;(#,##0)";
                case 38:
                    return "#,##0";  //"#,##0;[Red](#,##0)";
                case 39:
                    return "#,##0.00;(#,##0.00)";
                case 40:
                    return "#,##0.00;[Red](#,##0.00)";
                case 44:
                    return "'$'#,##0.00;'-$'#,##0.00"; // close enough for currency // '_("$"* #,##0.00_);_("$"* \(#,##0.00\);_("$"* "-"??_);_(@_)';
                case 45:
                    return "mm:ss";
                case 46:
                    return "[h]:mm:ss";
                case 47:
                    return "mmss.0";
                case 48:
                    return "##0.0E+0";
                case 49:
                    return "@";
                default:
                    return null;
            }
        }


        public static bool TryParseInt( string value, out int result )
        {
            decimal dResult;
            if ( !TryParseDecimal( value, out dResult ) )
            {
                result = 0;
                return false;
            }
            decimal dRounded = Math.Round( dResult, MidpointRounding.AwayFromZero );
            result = ( int ) dRounded;
            return true;
        }

        public static bool TryParseDecimal( string value, out decimal result )
        {
            if ( decimal.TryParse( value, ExcelDecimalStyle, CultureInfo.InvariantCulture, out result ) )
            {
                // Excel stores values in floating point format, which means the stored value is not exactly the entered values.
                // E.g. 0.50125 gets stored in the file itself as 0.50124999999999997
                // Round to 10dp should be sufficient to always get enough precision for ReadiNow, but also always drop any rounding errors.
                // Note: while this is the CSV parser, the Excel parser calls into it, and in principle someone could have the same issue with CSV too.
                result = Math.Round( result, 10 );

                return true;
            }
            return false;
        }

    }
}
