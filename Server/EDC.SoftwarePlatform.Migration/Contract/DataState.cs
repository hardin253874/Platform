// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Flag to indicate the state of the data in the export/import database.
	///     States represent how the data in a tenant (at time of export) compares to the global tenant at the time of export.
	/// </summary>
	[Flags]
	[DataContract]
	public enum DataState
	{
		/// <summary>
		///     No change: the row appears in both the tenant and global tenant.
		/// </summary>
		[EnumMember( Value = "unchanged" )]
		Unchanged = 1,

		/// <summary>
		///     The row is in the tenant, but not the global tenant.
		/// </summary>
		[EnumMember( Value = "added" )]
		Added = 2,

		/// <summary>
		///     The row is in the global tenant, but missing from the tenant.
		/// </summary>
		[EnumMember( Value = "removed" )]
		Removed = 4,

		/// <summary>
		///     The row appears in both the tenant and global tenant, but data is different.
		/// </summary>
		[EnumMember( Value = "changed" )]
		Changed = 8,

		/// <summary>
		///     All states
		/// </summary>
		[EnumMember( Value = "all" )]
		All = 15
	}
}