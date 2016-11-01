// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	/// <summary>
	///     Metadata result about an entire spreadsheet file. (as opposed to a single tab/sheet)
	/// </summary>
	[DataContract]
	public class SpreadsheetInfo
	{
		/// <summary>
		///     Gets or sets the error message.
		/// </summary>
		/// <value>
		///     The error message.
		/// </value>
		[DataMember( Name = "error", EmitDefaultValue = true, IsRequired = false )]
		public string ErrorMessage
		{
			get;
			set;
		}

		/// <summary>
		///     Filename of the imported File
		/// </summary>
		[DataMember( Name = "fileName" )]
		public string FileName
		{
			get;
			set;
		}

		/// <summary>
		///     Imported File Format
		/// </summary>
		[DataMember( Name = "importFileFormat" )]
		public ImportFormat ImportFileFormat
		{
			get;
			set;
		}

		/// <summary>
		///     Collection of Sheet info from the Excel File.
		/// </summary>
		[DataMember( Name = "sheetCollection" )]
		public List<SheetInfo> SheetCollection
		{
			get;
			set;
        }

        /// <summary>
		///     ID of the initial sheet
		/// </summary>
		[DataMember( Name = "initialSheetId" )]
        public string InitialSheetId
        {
            get;
            set;
        }

        /// <summary>
		///     Sample Data Table for the sheet.
		/// </summary>
		[DataMember( Name = "initialSampleTable" )]
        public SampleTable InitialSampleTable
        {
            get;
            set;
        }
    }
}