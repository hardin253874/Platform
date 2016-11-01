// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ExportData
{
	/// <summary>
	///     Export data Info object
	/// </summary>
	[DataContract]
	public class ExportDataInfo
	{
		/// <summary>
		///     Gets and sets the FileHash.
		/// </summary>
		[DataMember( Name = "fileHash", EmitDefaultValue = true, IsRequired = true )]
		public string FileHash
		{
			get;
			set;
		}

		/// <summary>
		///     Gets and sets the ResponseMessage.
		/// </summary>
		[DataMember( Name = "responseMessage", EmitDefaultValue = true )]
		public string ResponseMessage
		{
			get;
			set;
		}
	}
}