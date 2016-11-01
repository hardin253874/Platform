// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	/// <summary>
	///     Defines the Import Format type.
	/// </summary>
	[DataContract]
	public enum ImportFormat
	{
		/// <summary>
		///     The Excel format type.
		/// </summary>
		[EnumMember( Value = "Excel" )]
		Excel,

		/// <summary>
		///     The CSV format type.
		/// </summary>
		[EnumMember( Value = "Csv" )]
		Csv,

        /// <summary>
        ///     The CSV format type.
        /// </summary>
        [EnumMember( Value = "Tab" )]
        Tab
    }
}