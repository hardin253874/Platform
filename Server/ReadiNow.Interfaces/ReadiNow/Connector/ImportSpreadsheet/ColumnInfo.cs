// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;

namespace ReadiNow.Connector.ImportSpreadsheet
{
	/// <summary>
	///     Mapping column info.
	/// </summary>
	public class ColumnInfo
	{
		/// <summary>
		///     Column Id
		/// </summary>
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     Spreadsheet column name.
		/// </summary>
		public bool IsRelationship
		{
			get;
			set;
		}

		/// <summary>
		///     Is required column.
		/// </summary>
		public bool IsRequired
		{
			get;
			set;
		}

		/// <summary>
		///     Column Name
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the relationship direction.
		/// </summary>
		public Direction RelationshipDirection
		{
			get;
			set;
		}

		/// <summary>
		///     Spreadsheet column index.
		/// </summary>
		public int SpreadsheetColumn
		{
			get;
			set;
		}

		/// <summary>
		///     Spreadsheet column name.
		/// </summary>
		public string SpreadsheetColumnName
		{
			get;
			set;
		}

		/// <summary>
		///     Resource column name.
		/// </summary>
		public long TargetResourceColumnId
		{
			get;
			set;
		}

		/// <summary>
		///     Target resource table Id.
		/// </summary>
		public long TargetTableId
		{
			get;
			set;
		}

		/// <summary>
		///     Column Type
		/// </summary>
		public string Type
		{
			get;
			set;
		}
	}
}