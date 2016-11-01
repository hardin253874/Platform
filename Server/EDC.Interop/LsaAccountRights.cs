// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Interop
{
	/// <summary>
	///     Local Security Authority.
	/// </summary>
	public static partial class Lsa
	{
		/// <summary>
		///     Required for an account to log on using the batch logon type.
		/// </summary>
		public const string BatchLogon = "SeBatchLogonRight";

		/// <summary>
		///     Explicitly denies an account the right to log on using the batch logon type.
		/// </summary>
		public const string DenyBatchLogon = "SeDenyBatchLogonRight";

		/// <summary>
		///     Explicitly denies an account the right to log on using the interactive logon type.
		/// </summary>
		public const string DenyInteractiveLogon = "SeDenyInteractiveLogonRight";

		/// <summary>
		///     Explicitly denies an account the right to log on using the network logon type.
		/// </summary>
		public const string DenyNetworkLogon = "SeDenyNetworkLogonRight";

		/// <summary>
		///     Explicitly denies an account the right to log on remotely using the interactive logon type.
		/// </summary>
		public const string DenyRemoteInteractiveLogon = "SeDenyRemoteInteractiveLogonRight";

		/// <summary>
		///     Explicitly denies an account the right to log on using the service logon type.
		/// </summary>
		public const string DenyServiceLogon = "SeDenyServiceLogonRight";

		/// <summary>
		///     Required for an account to log on using the interactive logon type.
		/// </summary>
		public const string InteractiveLogon = "SeInteractiveLogonRight";

		/// <summary>
		///     Required for an account to log on using the network logon type.
		/// </summary>
		public const string NetyworkLogon = "SeNetworkLogonRight";

		/// <summary>
		///     Required for an account to log on remotely using the interactive logon type.
		/// </summary>
		public const string RemotenteractiveLogon = "SeRemoteInteractiveLogonRight";

		/// <summary>
		///     Required for an account to log on using the service logon type.
		/// </summary>
		public const string ServiceLogon = "SeServiceLogonRight";
	}
}