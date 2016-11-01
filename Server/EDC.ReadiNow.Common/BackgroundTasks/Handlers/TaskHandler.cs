// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging.Redis;
using ProtoBuf.Meta;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.BackgroundTasks.Handlers
{
    /// <summary>
    /// A base task handler that deals with the serialization and deserialization of the task information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TaskHandler<T>: ITaskHandler where T: IWorkflowQueuedEvent, new()
    {
        /// <summary>
        /// The key used to fetch the task handler
        /// </summary>
        public string TaskHandlerKey { get; }

        public bool CompressData { get; }



        protected TaskHandler(string handlerKey, bool compressData)
        {
            TaskHandlerKey = handlerKey;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void HandleTask(BackgroundTask task)
        {
            var value = task.GetData<T>();

            HandleTask(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        protected abstract void HandleTask(T taskData);


        /// <summary>
        /// Convert a T to a background task
        /// </summary>
        public BackgroundTask ToBackgroundTask(T taskData)
        {
            return BackgroundTask.Create(TaskHandlerKey, taskData);
        }


    }
}
