// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EDC.Database.Types;
using EDC.ReadiNow.Utc;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Reader for extracting data from a single Excel row, where the caller knows what type of data they intend to extract.
    /// </summary>
    class ExcelObjectReader : IObjectReader
    {
        private readonly Row _row;
        private readonly SpreadsheetDocument _document;
        private readonly uint _rowIndex;
        private readonly DataFileReaderSettings _settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="excelRow">The OpenXml Excel row.</param>
        /// <param name="document"></param>
        /// <param name="settings">General settings.</param>
        internal ExcelObjectReader(Row excelRow, SpreadsheetDocument document, DataFileReaderSettings settings )
        {
            if (excelRow == null)
                throw new ArgumentNullException( nameof( excelRow ) );
            if (!excelRow.RowIndex.HasValue)
                throw new ArgumentException("RowIndex is not set", nameof( excelRow ) );
            if (document == null)
                throw new ArgumentNullException( nameof( document ) );
            if ( settings == null )
                throw new ArgumentNullException( nameof( settings ) );

            _row = excelRow;
            _rowIndex = excelRow.RowIndex.Value;
            _document = document;
            _settings = settings;
        }

        /// <summary>
        /// Locate a single cell by column reference.
        /// </summary>
        /// <param name="key">The column reference. An Excel alphabetical column reference.</param>
        /// <returns>The text data.</returns>
        private Cell GetCell(string key)
        {
            string cellKey = key + _rowIndex.ToString(CultureInfo.InvariantCulture);
            return _row.Elements<Cell>().FirstOrDefault(c => StringComparer.InvariantCultureIgnoreCase.Equals(c.CellReference?.Value, cellKey) );
        }

        /// <summary>
        /// Locate a single cell by column reference and return its text content.
        /// </summary>
        /// <param name="key">The column reference. An Excel alphabetical column reference.</param>
        /// <returns>The text data.</returns>
        private string GetCellText(string key)
        {
            Cell cell = GetCell(key);
            return ExcelHelpers.GetCellText(_document, cell);
        }

        /// <summary>
        /// Helper to split a list of values stored in a single field cell.
        /// </summary>
        /// <param name="value">The text.</param>
        /// <returns>The split text.</returns>
        internal static string[] GetList(string value)
        {
            return value
                .Split( new [ ] { ';' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( s => s.Trim( ) )
                .Where( s => s.Length > 0 )
                .ToArray( );
        }

        /// <summary>
        /// Returns a list of keys that can be used to access fields.
        /// </summary>
        /// <returns>A set containing the numbers zero to field-1 as strings.</returns>
        public ISet<string> GetKeys()
        {
            int chop = _rowIndex.ToString(CultureInfo.InvariantCulture).Length;
            return new HashSet<string>(
                _row.Elements<Cell>().Where(cell => cell.CellReference != null ).Select(
                    c => c.CellReference.Value.Substring( 0, c.CellReference.Value.Length - chop )
                ));
        }

        /// <summary>
        /// Determine if the key is known.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            Cell cell = GetCell(key);
            return cell != null;
        }

        /// <summary>
        /// Reads a field, as an integer.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public int? GetInt(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            int result;
            if (!ExcelCellFormatter.TryParseInt(value, out result))
                throw new InvalidCastException();
            return result;
        }

        /// <summary>
        /// Reads a field, as a decimal.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public decimal? GetDecimal(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            decimal result;
            if (!ExcelCellFormatter.TryParseDecimal(value, out result))
                throw new InvalidCastException();

            return result;
        }

        /// <summary>
        /// Reads a field, as a date.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public DateTime? GetDate(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            DateTime dateTime = DateTime.FromOADate(Convert.ToDouble(value));
            DateTime dateOnly = dateTime.Date;
            DateTime date = DateTime.SpecifyKind( dateOnly, DateTimeKind.Utc);

            return date;
        }

        /// <summary>
        /// Reads a field, as a time.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public DateTime? GetTime(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            DateTime dateTime = DateTime.FromOADate(Convert.ToDouble(value));
            DateTime time = TimeType.NewTime(dateTime.TimeOfDay);

            return time;
        }

        /// <summary>
        /// Reads a field, as a date-time.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public DateTime? GetDateTime(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            DateTime dateTime = DateTime.FromOADate(Convert.ToDouble(value));
            DateTime result;

            // If a timezone was provided with the import, assume the value is specified in that. Otherwise, assume it is UTC.
            if (_settings.TimeZoneInfo != null )
                result = TimeZoneHelper.ConvertToUtcTZ( dateTime, _settings.TimeZoneInfo );
            else
                result = DateTime.SpecifyKind( dateTime, DateTimeKind.Utc );

            return result;
        }

        /// <summary>
        /// Reads a field, as a string.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public string GetString(string key)
        {
            Cell cell = GetCell(key);
            if ( cell == null )
                return null;

            string result = ExcelCellFormatter.CellToString(cell, _document);

	        result = result?.Trim( );

	        return result;
        }

        /// <summary>
        /// Reads a field, as a boolean.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public bool? GetBoolean(string key)
        {
            string text = GetCellText(key);
            if ( string.IsNullOrEmpty( text ) )
                return false; // treat null as false

            bool result;
            if (!CsvValueParser.TryParseBool(text, out result))
                throw new InvalidCastException();
            return result;
        }

        /// <summary>
        /// Reads a field, as a list of integers.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public IReadOnlyList<int> GetIntList(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            string[] parts = GetList(value);

            List<int> res = new List<int>(parts.Length);
            foreach (string part in parts)
            {
                int entry;
                if (CsvValueParser.TryParseInt(part, out entry))
                    res.Add(entry);
                else
                    throw new InvalidCastException();
            }
            return res;
        }

        /// <summary>
        /// Reads a field, as a list of decimals.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public IReadOnlyList<decimal> GetDecimalList(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            string[] parts = GetList(value);

            List<decimal> res = new List<decimal>(parts.Length);
            foreach (string part in parts)
            {
                decimal entry;
                if (CsvValueParser.TryParseDecimal(part, out entry))
                    res.Add(entry);
                else
                    throw new InvalidCastException();
            }
            return res;
        }

        /// <summary>
        /// Reads a field, as a list of strings.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public IReadOnlyList<string> GetStringList(string key)
        {
            string value = GetCellText(key);
            if ( string.IsNullOrEmpty( value ) )
                return null;

            string[] parts = GetList(value);
            if (parts.Length == 0)
                return null;

            return parts;
        }

        /// <summary>
        /// Reads a field, as a child record.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IObjectReader GetObject(string key)
        {
            throw new NotSupportedException("CSV object reader does not support nested objects");
        }

        /// <summary>
        /// Reads a field, as a list of child records.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IReadOnlyList<IObjectReader> GetObjectList(string key)
        {
            throw new NotSupportedException("CSV object reader does not support nested objects");
        }

        /// <summary>
        /// Get some sort of reference of where the object came from.
        /// </summary>
        /// <returns>The row number.</returns>
        public string GetLocation()
        {
            return "Row " + _rowIndex.ToString(CultureInfo.InvariantCulture);
        }
    }
}
