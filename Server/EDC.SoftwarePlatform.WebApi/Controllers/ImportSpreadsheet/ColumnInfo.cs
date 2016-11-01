// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	/// <summary>
	///     Mapping column info.
	/// </summary>
	[DataContract]
	public class ColumnInfo
	{
		/// <summary>
		///     Column Id
		/// </summary>
		[DataMember( Name = "id" )]
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     Column Name
		/// </summary>
		[DataMember( Name = "name" )]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Column Type
		/// </summary>
		[DataMember( Name = "type" )]
		public string Type
		{
			get;
			set;
		}

		/// <summary>
		///     Is required column.
		/// </summary>
		[DataMember( Name = "isRequired" )]
		public bool IsRequired
		{
			get;
			set;
		}

		/// <summary>
		///     Target resource table Id.
		/// </summary>
		[DataMember( Name = "targetTableId" )]
		public long TargetTableId
		{
			get;
			set;
		}

		/// <summary>
		///     Resource column name.
		/// </summary>
		[DataMember( Name = "targetResourceColumnId" )]
		public long TargetResourceColumnId
		{
			get;
			set;
		}

		/// <summary>
		///     Spreadsheet column index.
		/// </summary>
		[DataMember( Name = "spreadsheetColumn" )]
		public int SpreadsheetColumn
		{
			get;
			set;
		}

		/// <summary>
		///     Spreadsheet column name.
		/// </summary>
		[DataMember( Name = "spreadsheetColumnName" )]
		public string SpreadsheetColumnName
		{
			get;
			set;
		}

		/// <summary>
		///     Spreadsheet column name.
		/// </summary>
		[DataMember( Name = "isRelationship" )]
		public bool IsRelationship
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the relationship direction.
		/// </summary>
		[DataMember( Name = "relationshipDirection" )]
		public RelationshipDirection RelationshipDirection
		{
			get;
			set;
		}
	}
}