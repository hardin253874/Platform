// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Services.LongRunningTask
{
	/// <summary>
	///     Defines the long-running task status types.
	/// </summary>
	public enum LongRunningStatus
	{
		/// <summary>
		///     Completed with success.
		/// </summary>
		Success,

		/// <summary>
		///     Not started yet.
		/// </summary>
		Queued,

		/// <summary>
		///     Currently executing.
		/// </summary>
		InProgress,

		/// <summary>
		///     Cancelled by client.
		/// </summary>
		Cancelled,

		/// <summary>
		///     Completed with failure.
		/// </summary>
		Failed
	}
}