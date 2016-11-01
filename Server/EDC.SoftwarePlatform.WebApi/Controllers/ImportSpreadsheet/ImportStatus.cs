// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	/// <summary>
	///     Defines the Import status types.
	/// </summary>
	[DataContract]
	public enum ImportStatus
	{
		/// <summary>
		///     success import Status.
		/// </summary>
		[EnumMember( Value = "success" )]
		Success,

		/// <summary>
		///     In Progress Import Status.
		/// </summary>
		[EnumMember( Value = "inProgress" )]
		InProgress,

		/// <summary>
		///     Cancelled Import Status.
		/// </summary>
		[EnumMember( Value = "cancelled" )]
		Cancelled,

		/// <summary>
		///     Failed Import Status
		/// </summary>
		[EnumMember( Value = "failed" )]
		Failed
	}
}