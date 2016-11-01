// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.LongRunning
{
	[DataContract]
	public class LongRunningTaskInfo
	{
		/// <summary>
		///     ID of the long-running task.
		/// </summary>
		[DataMember( Name = "taskid", EmitDefaultValue = false, IsRequired = true )]
		public string TaskId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the task identifier value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTaskId( )
	    {
			return TaskId != null;
	    }

		/// <summary>
		///     Current status of the long-running task.
		/// </summary>
		[DataMember( Name = "status", EmitDefaultValue = false, IsRequired = false )]
		public string Status
		{
			get;
			set;
		}

		/// <summary>
		///		Should the status value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeStatus( )
	    {
			return Status != null;
	    }

		/// <summary>
		///     Contains the result data.
		/// </summary>
		[DataMember( Name = "result", EmitDefaultValue = false, IsRequired = false )]
		public string ResultData
		{
			get;
			set;
		}

		/// <summary>
		///		Should the result data value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeResultData( )
	    {
			return ResultData != null;
	    }

		/// <summary>
		///     Progress message (if any)
		/// </summary>
		[DataMember( Name = "progress", EmitDefaultValue = false, IsRequired = false )]
		public string ProgressMessage
		{
			get;
			set;
		}

		/// <summary>
		///		Should the progress message value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeProgressMessage( )
	    {
			return ProgressMessage != null;
	    }

		/// <summary>
		///     Any failure error message.
		/// </summary>
		[DataMember( Name = "errormsg", EmitDefaultValue = false, IsRequired = false )]
		public string ErrorMessage
		{
			get;
			set;
		}

		/// <summary>
		///		Should the error message value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeErrorMessage( )
	    {
			return ErrorMessage != null;
	    }
	}
}