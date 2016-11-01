// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Messaging
{
    /// <summary>
    /// Used to manage long running tasks across the system
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Register the start of a task
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        void RegisterStart(string taskId);

        /// <summary>
        /// Register the completion of a task
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        void RegisterComplete(string taskId, string result = null);

        /// <summary>
        /// Has the given task started
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>true if it has</returns>
        bool HasStarted(string taskId);

        /// <summary>
        /// Has the given task completed
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>true if it has</returns>
        bool HasCompleted(string taskId);

        /// <summary>
        /// Gets the stored result of the task.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        string GetResult(string taskId);

        /// <summary>
        /// Remove all information about the task
        /// </summary>
        /// <param name="taskId"></param>
        void Clear(string taskId);

        /// <summary>
        /// Used to wrap a block of code to ensure the task is started and completed.
        /// </summary>
        ITaskManagerContext GetContextBlock(string taskId);

    }

    public interface ITaskManagerContext: IDisposable
    {

    }

}
