// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    ///     Metadata result about an entire spreadsheet file. (as opposed to a single tab/sheet)
    /// </summary>
    public class SpreadsheetInfo
	{
		/// <summary>
		///     Filename of the imported File
		/// </summary>
		public string FileName
		{
			get;
			set;
		}

		/// <summary>
		///     Imported File Format
		/// </summary>
		public ImportFormat ImportFileFormat
		{
			get;
			set;
		}

		/// <summary>
		///     Collection of Sheet Info from the Excel File.
		/// </summary>
		public IReadOnlyList<SheetInfo> SheetCollection
		{
			get;
			set;
		}
	}
}