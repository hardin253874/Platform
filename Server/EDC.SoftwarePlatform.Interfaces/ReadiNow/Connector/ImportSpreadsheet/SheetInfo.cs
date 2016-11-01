// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    ///     Metadata result about a single tab/sheet in a spreadsheet file.
    /// </summary>
    public class SheetInfo
    {
        /// <summary>
		///     Unique ID/reference to sheet.
		/// </summary>
		public string SheetId
        {
            get;
            set;
        }

        /// <summary>
		///     Name of the sheet.
		/// </summary>
		public string SheetName
        {
            get;
            set;
        }
	}
}