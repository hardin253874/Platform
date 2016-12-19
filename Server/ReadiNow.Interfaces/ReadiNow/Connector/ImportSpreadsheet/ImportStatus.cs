// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.ImportSpreadsheet
{
	/// <summary>
	///     Defines the Import status types.
	/// </summary>
	public enum ImportStatus
	{
		/// <summary>
		///     success import Status.
		/// </summary>
		Success,

		/// <summary>
		///     In Progress Import Status.
		/// </summary>
		InProgress,

		/// <summary>
		///     Cancelled Import Status.
		/// </summary>
		Cancelled,

		/// <summary>
		///     Failed Import Status
		/// </summary>
		Failed
	}
}