// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Text;

namespace EDC.Database
{
	/// <summary>
	///     Accumulates multiple insert values, and executes them together.
	///     Note: rows are all inserted as part of a single statement.
	/// </summary>
	public class BulkInserter : IDisposable
	{
		// E.g.
		// insert into Table ( Column1, Column2 ) VALUES
		// ( Value1, Value2 ), ( Value1, Value2 )

		private readonly IDbCommand _cmd;
		private readonly string _firstHalf;
		private readonly string _insertClause;
		private readonly StringBuilder _sb = new StringBuilder( );
		private readonly string _secondHalf;
		private bool _anyRows;
		private bool _insertedFirstPart;
		private int _rowsAffected;

		/// <summary>
		/// Gets or sets the row count action.
		/// </summary>
		/// <value>
		/// The row count action.
		/// </value>
		public Action<int> RowCountAction
		{
			get;
			set;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="BulkInserter" /> class.
		/// </summary>
		/// <param name="cmd">An uninitialized command.</param>
		/// <param name="insertIntoTableColumns">E.g.: insert into Table (Column1, Column2) values %ROWS%</param>
		public BulkInserter( IDbCommand cmd, string insertIntoTableColumns )
		{
			_cmd = cmd;
			_insertClause = insertIntoTableColumns;

			string[] parts = _insertClause.Split( new[]
				{
					"%ROWS%"
				}, StringSplitOptions.None );
			if ( parts.Length != 2 )
			{
				throw new ArgumentException( @"Insert statement must contain %ROWS% exactly once", "insertIntoTableColumns" );
			}

			_firstHalf = parts[ 0 ];
			_secondHalf = parts[ 1 ];

			FlushThreshold = 10000;
		}

		/// <summary>
		///     Once the buffer exceeds this length, it is executed.
		/// </summary>
		public int FlushThreshold
		{
			get;
			set;
		}

		/// <summary>
		///     Executes any remaining queries in the buffer.
		/// </summary>
		public void Dispose( )
		{
			Flush( );
		}

		/// <summary>
		///     Queues an insert. Executes all queued data once the buffer is full.
		/// </summary>
		/// <param name="valuesString">E.g.:  'Value1', 2, 3</param>
		public void QueueInsert( string valuesString )
		{
			// Append query
			if ( _anyRows )
			{
				_sb.Append( ", \r\n (" );
			}
			else
			{
				_sb.Append( _firstHalf + "\r\n(" );
				_insertedFirstPart = true;
			}
			_anyRows = true;
			_sb.Append( valuesString );
			_sb.Append( ")" );

			// Execute if buffer is full
			if ( _sb.Length > FlushThreshold )
			{
				Flush( );
			}
		}

		/// <summary>
		///     Executes the contents of the buffer, and resets the buffer.
		/// </summary>
		private void Flush( )
		{
			if ( _anyRows )
			{
				if ( _insertedFirstPart )
				{
					_sb.Append( _secondHalf );
				}

				_cmd.CommandText = _sb.ToString( );
				_sb.Clear( );
				_anyRows = false;
				_rowsAffected += _cmd.ExecuteNonQuery( );
			}

			if ( RowCountAction != null )
			{
				RowCountAction( _rowsAffected );
			}
		}
	}
}