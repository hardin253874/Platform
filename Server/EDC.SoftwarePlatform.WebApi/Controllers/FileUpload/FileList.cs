// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.FileUpload
{
	/// <summary>
	///     File List class.
	/// </summary>
	[DataContract]
	public class FileList : List<FileListItem>
	{
		// Why is the needed?
	}
}