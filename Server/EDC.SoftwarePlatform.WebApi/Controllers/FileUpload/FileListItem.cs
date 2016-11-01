// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.FileUpload
{
	/// <summary>
	///     File List Item
	/// </summary>
	[DataContract]
	public class FileListItem
	{
		/// <summary>
		///     Gets or sets the name of the file.
		/// </summary>
		/// <value>
		///     The name of the file.
		/// </value>
		[DataMember( Name = "fileName" )]
		public string FileName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the hash.
		/// </summary>
		/// <value>
		///     The hash.
		/// </value>
		[DataMember( Name = "hash" )]
		public string Hash
		{
			get;
			set;
		}
	}
}