// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.Data
{
	/// <summary>
	///     Defines a single table of in-memory data.
	/// </summary>
	[DataContract]
	public class DbDataTable
	{
		private List<DbDataColumn> _columns = new List<DbDataColumn>( );
		private Guid _id = Guid.Empty;
		private string _name = string.Empty;
		private List<DbDataRow> _rows = new List<DbDataRow>( );
		private string _tableName = string.Empty;

		/// <summary>
		///     Initializes a new instance of the DbDataTable class.
		/// </summary>
		public DbDataTable( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the DbDataTable class.
		/// </summary>
		public DbDataTable( string tableName )
		{
			_tableName = tableName;
		}

		/// <summary>
		///     Initializes a new instance of the DbDataTable class.
		/// </summary>
		public DbDataTable( Guid id, string tableName )
		{
			_id = id;
			_tableName = tableName;
		}

		/// <summary>
		///     Initializes a new instance of the DbDataTable class.
		/// </summary>
		public DbDataTable( Guid id, string tableName, string name, List<DbDataColumn> columns, List<DbDataRow> rows )
		{
			_id = id;
			_tableName = tableName;
			_name = name;
			_columns = columns;
			_rows = rows;
		}

		/// <summary>
		///     Gets or sets the columns contained within the data table.
		/// </summary>
		[DataMember]
		public List<DbDataColumn> Columns
		{
			get
			{
				return _columns;
			}

			set
			{
				_columns = value;
			}
		}

		/// <summary>
		///     Gets or sets the ID associated with the table.
		/// </summary>
		[DataMember]
		public Guid Id
		{
			get
			{
				return _id;
			}

			set
			{
				_id = value;
			}
		}

		/// <summary>
		///     Gets or sets the display name of the table.
		/// </summary>
		[DataMember]
		public string Name
		{
			get
			{
				return _name;
			}

			set
			{
				_name = value;
			}
		}

		/// <summary>
		///     Gets or sets the rows of data contained within the data table.
		/// </summary>
		[DataMember]
		public List<DbDataRow> Rows
		{
			get
			{
				return _rows;
			}

			set
			{
				_rows = value;
			}
		}

		/// <summary>
		///     Gets or sets the internal name of the table.
		/// </summary>
		[DataMember]
		public string TableName
		{
			get
			{
				return _tableName;
			}

			set
			{
				_tableName = value;
			}
		}
	}
}