// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.SoftwarePlatform.Migration.Contract.Statistics.Failure
{
	/// <summary>
	///     Xml Translation Failure reason.
	/// </summary>
	public enum XmlFailureReason
	{
		/// <summary>
		///     The reason for the failure was an invalid source node.
		/// </summary>
		InvalidSource,

		/// <summary>
		///     The reason for the failure was an unknown local id.
		/// </summary>
		UnknownLocalId,

		/// <summary>
		///     The reason for the failure was an unknown local alias.
		/// </summary>
		UnknownLocalAlias,

		/// <summary>
		///		The reason for the failure was an unknown upgrade id.
		/// </summary>
		UnknownUpgradeId
	}
}