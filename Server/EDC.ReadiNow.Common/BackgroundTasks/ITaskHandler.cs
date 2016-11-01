// Copyright 2011-2016 Global Software Innovation Pty Ltd

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
    }
}
