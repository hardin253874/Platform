// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Net;
using System.Net.Http;
using System.Web.Http;
using EDC.SoftwarePlatform.Services.LongRunningTask;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;

namespace EDC.SoftwarePlatform.WebApi.Controllers.LongRunning
{
	/// <summary>
	///     A controller for working with long running server tasks.
	/// </summary>
	[RoutePrefix( "data/v1/longRunning" )]
	public class LongRunningController : ApiController
	{
		/// <summary>
		///     Gets the state information about a long running task that is identified by an id or token.
		/// </summary>
		/// <param name="taskId">The identifier string.</param>
		/// <returns>
		///     Information about the task.
		/// </returns>
		[Route( "{taskId}" )]
        [HttpGet]
		public HttpResponseMessage<LongRunningTaskInfo> GetProgress( string taskId )
		{
            if (string.IsNullOrEmpty(taskId))
                throw new WebArgumentNullException("taskId");

            var taskInfo = ParseLongRunningInfo(LongRunningTaskInterface.GetProgress(taskId));
            return new HttpResponseMessage<LongRunningTaskInfo>(taskInfo, HttpStatusCode.OK);
		}

		/// <summary>
		///     Deletes the task.
		/// </summary>
		/// <param name="taskId">The task identifier.</param>
		/// <returns></returns>
		[Route( "" )]
        [HttpDelete]
		public HttpResponseMessage DeleteTask( string taskId )
		{
            if (string.IsNullOrEmpty(taskId))
                throw new WebArgumentNullException("taskId");

            LongRunningTaskInterface.CancelTask(taskId);
			return new HttpResponseMessage( HttpStatusCode.OK );
		}

		/// <summary>
		///     Parses the long running information.
		/// </summary>
		/// <param name="longRunningInfo">The long running information.</param>
		/// <returns></returns>
		private LongRunningTaskInfo ParseLongRunningInfo( LongRunningInfo longRunningInfo )
		{
			return new LongRunningTaskInfo
			{
				TaskId = longRunningInfo.TaskId.ToString( @"N" ),
				Status = longRunningInfo.Status.ToString( ),
				ResultData = longRunningInfo.ResultData,
				ProgressMessage = longRunningInfo.ProgressMessage,
				ErrorMessage = longRunningInfo.ErrorMessage
			};
		}
	}
}