// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
    /// <summary>
    ///     Metadata result about a single tab/sheet in a spreadsheet file.
    /// </summary>
    [DataContract]
	public class SheetInfo
	{
		/// <summary>
		///     Id of the sheet.
		/// </summary>
		[DataMember( Name = "sheetId" )]
		public string SheetId
		{
			get;
			set;
		}

		/// <summary>
		///     Name of the sheet.
		/// </summary>
		[DataMember( Name = "sheetName" )]
		public string SheetName
		{
			get;
			set;
		}
	}
}