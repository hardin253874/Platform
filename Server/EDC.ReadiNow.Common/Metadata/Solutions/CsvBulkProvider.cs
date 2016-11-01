// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace EDC.ReadiNow.Metadata.Solutions
{
	/// <summary>
	///     CSV Bulk loader.
	/// </summary>
	public class CsvBulkProvider : IDataReader
	{
		/// <summary>
		///     Whether the end-of-file has been read yet.
		/// </summary>
		private bool _eof;

		/// <summary>
		///     Initializes a new instance of the <see cref="CsvBulkProvider" /> class.
		/// </summary>
		/// <param name="sourceNode">The source node.</param>
		/// <param name="metadata">The metadata.</param>
		public CsvBulkProvider( XmlNode sourceNode, BulkProviderMetadata metadata )
		{
			SourceNode = sourceNode;
			Metadata = metadata;
			Reader = new StringReader( SourceNode.InnerText );
		}

		/// <summary>
		///     Gets or sets the current line.
		/// </summary>
		/// <value>
		///     The current line.
		/// </value>
		private string[] CurrentLine
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the metadata.
		/// </summary>
		/// <value>
		///     The metadata.
		/// </value>
		private BulkProviderMetadata Metadata
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the reader.
		/// </summary>
		/// <value>
		///     The reader.
		/// </value>
		private StringReader Reader
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the source node.
		/// </summary>
		/// <value>
		///     The source node.
		/// </value>
		private XmlNode SourceNode
		{
			get;
			set;
		}


		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( Reader != null )
			{
				Reader.Dispose( );
				Reader = null;
			}
		}

		/// <summary>
		///     Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		public string GetName( int i )
		{
			return Metadata.Columns[ i ].ColumnName;
		}

		/// <summary>
		///     Gets the data type information for the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The data type information for the specified field.
		/// </returns>
		public string GetDataTypeName( int i )
		{
			return Metadata.Columns[ i ].DataType.Name;
		}

		/// <summary>
		///     Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from
		///     <see
		///         cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />
		///     .
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from
		///     <see
		///         cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />
		///     .
		/// </returns>
		public Type GetFieldType( int i )
		{
			return Metadata.Columns[ i ].DataType;
		}

		/// <summary>
		///     Return the value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Object" /> which will contain the field value upon return.
		/// </returns>
		public object GetValue( int i )
		{
			return Cast( CurrentLine[ i ], i );
		}

		/// <summary>
		///     Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <param name="values">
		///     An array of <see cref="T:System.Object" /> to copy the attribute fields into.
		/// </param>
		/// <returns>
		///     The number of instances of <see cref="T:System.Object" /> in the array.
		/// </returns>
		public int GetValues( object[] values )
		{
			return CurrentLine.Length;
		}

		/// <summary>
		///     Return the index of the named field.
		/// </summary>
		/// <param name="name">The name of the field to find.</param>
		/// <returns>
		///     The index of the named field.
		/// </returns>
		public int GetOrdinal( string name )
		{
			for ( int i = 0; i < Metadata.Columns.Count; i++ )
			{
				if ( Metadata.Columns[ i ].ColumnName == name )
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		///     Gets the value of the specified column as a Boolean.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The value of the column.
		/// </returns>
		public bool GetBoolean( int i )
		{
			return bool.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the 8-bit unsigned integer value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The 8-bit unsigned integer value of the specified column.
		/// </returns>
		public byte GetByte( int i )
		{
			return byte.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
		/// <param name="bufferoffset">
		///     The index for <paramref name="buffer" /> to start the read operation.
		/// </param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>
		///     The actual number of bytes read.
		/// </returns>
		public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length )
		{
			byte[] bytes = Encoding.UTF8.GetBytes( CurrentLine[ i ] );

			Array.Copy( bytes, 0, buffer, bufferoffset, length );

			return bytes.LongLength;
		}

		/// <summary>
		///     Gets the character value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <returns>
		///     The character value of the specified column.
		/// </returns>
		public char GetChar( int i )
		{
			return char.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldoffset">The index within the row from which to start the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
		/// <param name="bufferoffset">
		///     The index for <paramref name="buffer" /> to start the read operation.
		/// </param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>
		///     The actual number of characters read.
		/// </returns>
		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length )
		{
			byte[] bytes = Encoding.UTF8.GetBytes( CurrentLine[ i ] );
			char[] chars = Encoding.UTF8.GetChars( bytes );

			Array.Copy( chars, 0, buffer, bufferoffset, length );

			return chars.LongLength;
		}

		/// <summary>
		///     Returns the GUID value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The GUID value of the specified field.
		/// </returns>
		public Guid GetGuid( int i )
		{
			return Guid.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the 16-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 16-bit signed integer value of the specified field.
		/// </returns>
		public short GetInt16( int i )
		{
			return Int16.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the 32-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 32-bit signed integer value of the specified field.
		/// </returns>
		public int GetInt32( int i )
		{
			return Int32.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the 64-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The 64-bit signed integer value of the specified field.
		/// </returns>
		public long GetInt64( int i )
		{
			return Int64.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the single-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The single-precision floating point number of the specified field.
		/// </returns>
		public float GetFloat( int i )
		{
			return float.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the double-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The double-precision floating point number of the specified field.
		/// </returns>
		public double GetDouble( int i )
		{
			return double.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the string value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The string value of the specified field.
		/// </returns>
		public string GetString( int i )
		{
			return CurrentLine[ i ];
		}

		/// <summary>
		///     Gets the fixed-position numeric value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The fixed-position numeric value of the specified field.
		/// </returns>
		public decimal GetDecimal( int i )
		{
			return decimal.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Gets the date and time data value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The date and time data value of the specified field.
		/// </returns>
		public DateTime GetDateTime( int i )
		{
			return DateTime.Parse( CurrentLine[ i ] );
		}

		/// <summary>
		///     Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     The <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
		/// </returns>
		public IDataReader GetData( int i )
		{
			if ( i == 0 )
			{
				return this;
			}

			return null;
		}

		/// <summary>
		///     Return whether the specified field is set to null.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		///     true if the specified field is set to null; otherwise, false.
		/// </returns>
		public bool IsDBNull( int i )
		{
			return Metadata.Columns[ i ].AllowDBNull;
		}

		/// <summary>
		///     Gets the number of columns in the current row.
		/// </summary>
		/// <returns>When not positioned in a valid recordset, 0; otherwise, the number of columns in the current record. The default is -1.</returns>
		public int FieldCount
		{
			get
			{
				return Metadata.Columns.Count;
			}
		}

		/// <summary>
		///     Gets the column located at the specified index.
		/// </summary>
		/// <param name="i">The attribute.</param>
		/// <returns></returns>
		object IDataRecord.this[ int i ]
		{
			get
			{
				return GetValue( i );
			}
		}

		/// <summary>
		///     Gets the column with the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		object IDataRecord.this[ string name ]
		{
			get
			{
				int ordinal = GetOrdinal( name );

				if ( ordinal >= 0 )
				{
					return GetValue( ordinal );
				}

				return null;
			}
		}

		/// <summary>
		///     Closes the <see cref="T:System.Data.IDataReader" /> Object.
		/// </summary>
		public void Close( )
		{
			Reader.Dispose( );
			Reader = null;
		}

		/// <summary>
		///     Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the
		///     <see
		///         cref="T:System.Data.IDataReader" />
		///     .
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Data.DataTable" /> that describes the column metadata.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public DataTable GetSchemaTable( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Advances the data reader to the next result, when reading the results of batch SQL statements.
		/// </summary>
		/// <returns>
		///     true if there are more rows; otherwise, false.
		/// </returns>
		public bool NextResult( )
		{
			return false;
		}

		/// <summary>
		///     Advances the <see cref="T:System.Data.IDataReader" /> to the next record.
		/// </summary>
		/// <returns>
		///     true if there are more rows; otherwise, false.
		/// </returns>
		public bool Read( )
		{
			string currentLine = Reader.ReadLine( );

			_eof = currentLine == null;

			if ( !_eof )
			{
				CurrentLine = SplitCsvLine( currentLine );
			}

			return !_eof;
		}

		/// <summary>
		///     Gets a value indicating the depth of nesting for the current row.
		/// </summary>
		/// <returns>The level of nesting.</returns>
		public int Depth
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the data reader is closed.
		/// </summary>
		/// <returns>true if the data reader is closed; otherwise, false.</returns>
		public bool IsClosed
		{
			get
			{
				return _eof;
			}
		}

		/// <summary>
		///     Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
		/// </summary>
		/// <returns>The number of rows changed, inserted, or deleted; 0 if no rows were affected or the statement failed; and -1 for SELECT statements.</returns>
		public int RecordsAffected
		{
			get
			{
				return -1;
			}
		}

		/// <summary>
		///     Casts the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="columnOrdinal">The column ordinal.</param>
		/// <returns></returns>
		private object Cast( string input, int columnOrdinal )
		{
			return Convert.ChangeType( input, Metadata.Columns[ columnOrdinal ].DataType );
		}

		/// <summary>
		///     Splits the CSV line.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <returns></returns>
		private static string[] SplitCsvLine( string line )
		{
			return ( from Match m in Regex.Matches( line, @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)", RegexOptions.ExplicitCapture )
			         select m.Groups[ 1 ].Value ).ToArray( );
		}
	}
}