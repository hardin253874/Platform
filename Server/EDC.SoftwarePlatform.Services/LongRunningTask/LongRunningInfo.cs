// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Services.LongRunningTask
{
	/// <summary>
	///     Status information for long-running tasks.
	/// </summary>
	public class LongRunningInfo
	{
		/// <summary>
		///     Any failure error message.
		/// </summary>
		public string ErrorMessage
		{
			get;
			set;
		}

		/// <summary>
		///     Any progress error message.
		/// </summary>
		public string ProgressMessage
		{
			get;
			set;
		}

		/// <summary>
		///     Contains the result data.
		/// </summary>
		public string ResultData
		{
			get;
			set;
		}

		/// <summary>
		///     Current status of the long-running task.
		/// </summary>
		public LongRunningStatus Status
		{
			get;
			set;
		}

		/// <summary>
		///     ID of the long-running task.
		/// </summary>
		public Guid TaskId
		{
			get;
			set;
		}
	}
}