// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Reader for extracting data from a single CSV row.
    /// </summary>
    class CsvObjectReader : IObjectReader
    {
        private readonly string[ ] _fields;
        private readonly long _lineNumber;
        private DataFileReaderSettings _settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fields">The field data.</param>
        /// <param name="lineNumber">The line number, for reporting purposes.</param>
        /// <param name="settings">General settings</param>
        internal CsvObjectReader( string[ ] fields, long lineNumber, DataFileReaderSettings settings )
        {
            if ( fields == null )
                throw new ArgumentNullException( nameof( fields ) );
            if ( settings == null )
                throw new ArgumentNullException( nameof( settings ) );
            _fields = fields;
            _lineNumber = lineNumber;
            _settings = settings;
        }

        /// <summary>
        /// Extracts a single field value by name.
        /// </summary>
        /// <param name="key">The column reference. A number, starting from 0.</param>
        /// <returns>The text data.</returns>
        private string GetValue( string key )
        {
            int columnNumber = CsvFileReader.ColumnKeyToIndex( key );
            if ( columnNumber == -1 )
                throw new Exception( "Unknown column" );

            if ( columnNumber < 0 )
                throw new Exception( "Unknown column" );
            if ( columnNumber >= _fields.Length )
                return string.Empty;
            return _fields[ columnNumber ];
        }

        /// <summary>
        /// Helper to split a list of values stored in a single field cell.
        /// </summary>
        /// <param name="value">The text.</param>
        /// <returns>The split text.</returns>
        internal static string[ ] GetList( string value )
        {
            if ( value == null )
                throw new ArgumentNullException( "value" );

            return value
                .Split( new[ ]
                {
                    ';'
                }, StringSplitOptions.RemoveEmptyEntries )
                .Select( s => s.Trim( ) )
                .Where( s => s.Length > 0 )
                .ToArray( );
        }

        /// <summary>
        /// Returns a list of keys that can be used to access fields.
        /// </summary>
        /// <returns>A set containing the numbers zero to field-1 as strings.</returns>
        public ISet<string> GetKeys( )
        {
            return new HashSet<string>(
                Enumerable.Range( 0, _fields.Length )
                    .Select( CsvFileReader.ColumnIndexToKey )
                );
        }

        /// <summary>
        /// Determine if the key is known.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey( string key )
        {
            int columnNumber = CsvFileReader.ColumnKeyToIndex( key );
            if ( columnNumber == -1 )
                return false;

            return columnNumber >= 0 && columnNumber < _fields.Length;
        }

        /// <summary>
        /// Reads a field, as an integer.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public int? GetInt( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            int result;
            if ( !CsvValueParser.TryParseInt( value, out result ) )
                throw new InvalidCastException( );
            return result;
        }

        /// <summary>
        /// Reads a field, as a decimal.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public decimal? GetDecimal( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            decimal result;
            if ( !CsvValueParser.TryParseDecimal( value, out result ) )
                throw new InvalidCastException( );
            return result;
        }

        /// <summary>
        /// Reads a field, as a date.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public DateTime? GetDate( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            DateTime result;
            if ( !CsvValueParser.TryParseDate( value, out result ) )
                throw new InvalidCastException( );
            return result;
        }

        /// <summary>
        /// Reads a field, as a time.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public DateTime? GetTime( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            DateTime result;
            if ( !CsvValueParser.TryParseTime( value, out result ) )
                throw new InvalidCastException( );
            return result;
        }

        /// <summary>
        /// Reads a field, as a date-time.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public DateTime? GetDateTime( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            DateTime result;
            if ( !CsvValueParser.TryParseDateTime( value, _settings.TimeZoneInfo, out result ) )
                throw new InvalidCastException( );
            return result;
        }

        /// <summary>
        /// Reads a field, as a string.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public string GetString( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;
            return value;
        }

        /// <summary>
        /// Reads a field, as a boolean.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public bool? GetBoolean( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return false; // treat null as false

            bool result;
            if ( !CsvValueParser.TryParseBool( value, out result ) )
                throw new InvalidCastException( );
            return result;
        }

        /// <summary>
        /// Reads a field, as a list of integers.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public IReadOnlyList<int> GetIntList( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            string[ ] parts = GetList( value );

            List<int> res = new List<int>( parts.Length );
            foreach ( string part in parts )
            {
                int entry;
                if ( CsvValueParser.TryParseInt( part, out entry ) )
                    res.Add( entry );
                else
                    throw new InvalidCastException( );
            }
            return res;
        }

        /// <summary>
        /// Reads a field, as a list of decimals.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public IReadOnlyList<decimal> GetDecimalList( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            string[ ] parts = GetList( value );

            List<decimal> res = new List<decimal>( parts.Length );
            foreach ( string part in parts )
            {
                decimal entry;
                if ( CsvValueParser.TryParseDecimal( part, out entry ) )
                    res.Add( entry );
                else
                    throw new InvalidCastException( );
            }
            return res;
        }

        /// <summary>
        /// Reads a field, as a list of strings.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public IReadOnlyList<string> GetStringList( string key )
        {
            string value = GetValue( key );
            if ( string.IsNullOrEmpty( value ) )
                return null;

            string[ ] parts = GetList( value );
            return parts;
        }

        /// <summary>
        /// Reads a field, as a child record.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IObjectReader GetObject( string key )
        {
            throw new NotSupportedException( "CSV object reader does not support nested objects" );
        }

        /// <summary>
        /// Reads a field, as a list of child records.
        /// </summary>
        /// <param name="key">The column</param>
        /// <returns>The value</returns>
        /// <exception cref="NotSupportedException"></exception>
        public IReadOnlyList<IObjectReader> GetObjectList( string key )
        {
            throw new NotSupportedException( "CSV object reader does not support nested objects" );
        }

        /// <summary>
        /// Get some sort of reference of where the object came from.
        /// </summary>
        /// <returns>The row number.</returns>
        public string GetLocation( )
        {
            return "Line " + _lineNumber.ToString( CultureInfo.InvariantCulture );
        }
    }
}
