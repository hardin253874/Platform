// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Services.LongRunningTask
{
    public class LongRunningTaskInterface
    {
        /// <summary>
        /// Gets the progress of the long running task.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <returns>LongRunningInfo.</returns>
        /// <exception cref="System.ArgumentException">@Invalid task identifier</exception>
        public static LongRunningInfo GetProgress(string taskId)
        {
            Guid taskIdentifier;
            if (!Guid.TryParse(taskId, out taskIdentifier))
            {
                throw new ArgumentException(@"Invalid task identifier", taskId);
            }
            return LongRunningHelper.GetTaskInfo(taskIdentifier, true);
        }

        /// <summary>
        /// Cancels the task if it is still running or queued.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <exception cref="System.ArgumentException">@Invalid task identifier</exception>
        public static void CancelTask(string taskId)
        {
            Guid taskIdentifier;
            if (!Guid.TryParse(taskId, out taskIdentifier))
            {
                throw new ArgumentException(@"Invalid task identifier", taskId);
            }
            LongRunningInfo info = LongRunningHelper.GetTaskInfo(taskIdentifier, false);
            if (info.Status == LongRunningStatus.InProgress || info.Status == LongRunningStatus.Queued)
            {
                LongRunningHelper.UpdateStatus(taskIdentifier, LongRunningStatus.Cancelled);
            }
        }

    }
}
