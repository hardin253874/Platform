// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.FileManager
{
	/// <summary>
	///     FileItem class.
	/// </summary>
	[DataContract]
	public class FileItem
	{
		/// <summary>
		///     Gets or sets the name of the file.
		/// </summary>
		/// <value>
		///     The name of the file.
		/// </value>
		[DataMember( Name = "fileName", EmitDefaultValue = true, IsRequired = true )]
		public string FileName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the token.
		/// </summary>
		/// <value>
		///     The token.
		/// </value>
		[DataMember( Name = "token", EmitDefaultValue = true, IsRequired = true )]
		public string Token
		{
			get;
			set;
		}
	}
}