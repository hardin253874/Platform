// Copyright 2011-2016 Global Software Innovation Pty Ltd

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
		Unknown = 0,

		/// <summary>
		///     Missing dependency
		/// </summary>
		Missing = 1,

		/// <summary>
		///     The installed version falls below the minimum supported version
		/// </summary>
		BelowMinVersion = 2,

		/// <summary>
		///     The installed version is above the maximum supported version
		/// </summary>
		AboveMaxVersion = 3,

		/// <summary>
		///		Dependency is not installed
		/// </summary>
		NotInstalled = 4,

		/// <summary>
		/// No upgrade path is available
		/// </summary>
		NoUpgradePathAvailable = 5,

		/// <summary>
		/// An upgrade path is available but conflicts with existing applications
		/// </summary>
		IncompatibleUpgradePath = 6
	}
}