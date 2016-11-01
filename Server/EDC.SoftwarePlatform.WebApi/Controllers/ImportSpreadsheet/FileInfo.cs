// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	/// <summary>
	///     Uploaded File object.
	/// </summary>
	[DataContract]
	public class FileInfo
	{
		/// <summary>
		///     Name of the uploaded file.
		/// </summary>
		[DataMember( Name = "fileName" )]
		public string FileName
		{
			get;
			set;
		}

		/// <summary>
		///     Data hash of the uploaded file.
		/// </summary>
		[DataMember( Name = "fileId" )]
		public string FileId
		{
			get;
			set;
		}

		/// <summary>
		///     File format of the imported file (Excel/CSV)
		/// </summary>
		[DataMember( Name = "fileFormat" )]
		public string FileFormat
		{
			get;
			set;
		}
	}
}