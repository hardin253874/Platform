// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	/// <summary>
	///     Sheet import info object.
	/// </summary>
	[DataContract]
	public class SheetImportInfo
	{
		/// <summary>
		///     Id of the sheet.
		/// </summary>
		[DataMember( Name = "sheetId" )]
		public int SheetId
		{
			get;
			set;
		}

		/// <summary>
		///     Relational id of the sheet.
		/// </summary>
		[DataMember( Name = "id" )]
		public string Id
		{
			get;
			set;
		}

		/// <summary>
		///     Import task id.
		/// </summary>
		[DataMember( Name = "importTaskId" )]
		public string ImportTaskId
		{
			get;
			set;
		}

		/// <summary>
		///     File upload id.
		/// </summary>
		[DataMember( Name = "fileUploadId" )]
		public string FileUploadId
		{
			get;
			set;
		}

		/// <summary>
		///     Import file format.
		/// </summary>
		[DataMember( Name = "importFileFormat" )]
		public ImportFormat ImportFileFormat
		{
			get;
			set;
		}

		/// <summary>
		///     Row number contains header columns.
		/// </summary>
		[DataMember( Name = "headerRowNumber" )]
		public int HeaderRowNumber
		{
			get;
			set;
		}

		/// <summary>
		///     Row number to start reading data.
		/// </summary>
		[DataMember( Name = "dataRowNumber" )]
		public int DataRowNumber
		{
			get;
			set;
		}

		/// <summary>
		///     Resource Definition Id.
		/// </summary>
		[DataMember( Name = "resourceDefinitionId" )]
		public long ResourceDefinitionId
		{
			get;
			set;
		}

		/// <summary>
		///     Column Mapping info from resource definition to spreadsheet.
		/// </summary>
		[DataMember( Name = "columnMappingInfo" )]
		public List<ColumnInfo> ColumnMappingInfo
		{
			get;
			set;
		}

		/// <summary>
		///     Gets timezone of the client importing the data.
		///     Use the display name of time timezone in culture en-US.
		/// </summary>
		[DataMember( Name = "timeZoneName" )]
		public string TimeZoneName
		{
			get;
			set;
		}

		/// <summary>
		///     If sets true then overwrites existing data if the entity key is in place.
		/// </summary>
		[DataMember ( Name = "overWriteExistingData") ]
        public bool OverWriteExistingData
		{
			get;
			set;
		}
	}
}