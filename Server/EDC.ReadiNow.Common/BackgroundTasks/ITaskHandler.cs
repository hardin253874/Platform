// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.BackgroundTasks
{
    public interface ITaskHandler
    {
        /// <summary>
        /// The key used to fetch the task handler
        /// </summary>
        string TaskHandlerKey { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        void HandleTask(BackgroundTask task);

        /// <summary>
        /// Suspend the given task
        /// </summary>
        IEntity CreateSuspendedTask(BackgroundTask tasks);

        /// <summary>
        /// Resume all the suspended tasks
        /// </summary>
        IEnumerable<BackgroundTask> RestoreSuspendedTasks();
    }
}
