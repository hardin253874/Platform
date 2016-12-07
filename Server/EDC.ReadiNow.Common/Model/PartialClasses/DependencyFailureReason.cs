// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.Model.PartialClasses
{
	/// <summary>
	///     Dependency failure reason
	/// </summary>
	public enum DependencyFailureReason
	{
		/// <summary>
		///     Unknown reason
		/// </summary>
		[EnumMember( Value = "unknown" )]
		Unknown = 0,

		/// <summary>
		///     Missing dependency
		/// </summary>
		[EnumMember( Value = "missing" )]
		Missing = 1,

		/// <summary>
		///     The installed version falls below the minimum supported version
		/// </summary>
		[EnumMember( Value = "belowMinVersion" )]
		BelowMinVersion = 2,

		/// <summary>
		///     The installed version is above the maximum supported version
		/// </summary>
		[EnumMember( Value = "aboveMaxVersion" )]
		AboveMaxVersion = 3,

		/// <summary>
		///     Dependency is not installed
		/// </summary>
		[EnumMember( Value = "notInstalled" )]
		NotInstalled = 4,

		/// <summary>
		///     No upgrade path is available
		/// </summary>
		[EnumMember( Value = "noUpgradePathAvailable" )]
		NoUpgradePathAvailable = 5,

		/// <summary>
		///     An upgrade path is available but conflicts with existing applications
		/// </summary>
		[EnumMember( Value = "incompatibleUpgradePath" )]
		IncompatibleUpgradePath = 6
	}
}