// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging.Redis
{
    /// <summary>
    /// A task manager that uses the redis memory store
    /// </summary>
    public class TaskManager: ITaskManager
    {
        /// <summary>
        /// The default length of time that a task will be kept for.
        /// </summary>
        public readonly TimeSpan DefaultTaskExpiry = TimeSpan.FromMinutes(10);

        const string TaskManagerPerfix = "ReadiNowTaskManager";
        const string Started = "TaskStarted";
        const string Completed = "TaskCompleted";
        const string Cancelled = "TaskCancelled";

        IMemoryStore _memoryStore;
        string _taskGroupName;
        TimeSpan _taskExpiry;

        // TODO: Handle task expiration

		/// <summary>
		/// Create a Redis backed task manager
		/// </summary>
		/// <param name="memoryManager">The memory manager to use</param>
		/// <param name="taskGroupName">The name of the task group. This is used namespace different groups of tasks.</param>
		/// <param name="taskExpiry">The task expiry.</param>
        public TaskManager(IDistributedMemoryManager memoryManager, string taskGroupName, TimeSpan? taskExpiry = null) 
        {
            if (!memoryManager.IsConnected)
                memoryManager.Connect();

            _memoryStore = memoryManager.GetMemoryStore();
            _taskGroupName = taskGroupName;
            _taskExpiry = taskExpiry ?? DefaultTaskExpiry;
        }

        public void RegisterStart(string taskId)
        {
            Register(taskId, Started, null);
        }

        public void RegisterComplete(string taskId, string result = null)
        {
            Register(taskId, Completed, result);
        }

        public void RegisterCancelled(string taskId)
        {
            Register(taskId, Cancelled, null);
        }


        void Register(string taskId, string state, string additionalInfo)
        {
            string value = state;
                
            _memoryStore.StringSet(GetKey(taskId), state, _taskExpiry);
            _memoryStore.StringSet(GetResultKey(taskId), additionalInfo, _taskExpiry);
        }

        public bool HasStarted(string taskId)
        {
            var state = GetState(taskId);
            return !string.IsNullOrEmpty(state);
        }

        public bool HasCompleted(string taskId)
        {
            var state = GetState(taskId);
            return state == Completed || state == Cancelled;
        }

        public bool HasCancelled(string taskId)
        {
            var state = GetState(taskId);
            return state == Cancelled;
        }

        string GetState(string taskId)
        {
			RedisValue value;
            _memoryStore.TryGetString(GetKey(taskId), out value);
            return value;
        }

        string GetInfoState(string taskId)
        {
            RedisValue value;
            _memoryStore.TryGetString(GetResultKey(taskId), out value);
            return value;
        }

        public void Clear(string taskId)
        {
            _memoryStore.KeyDelete(GetKey(taskId));
            _memoryStore.KeyDelete(GetResultKey(taskId));
        }
        
        string GetKey(string taskId)
        {
            return TaskManagerPerfix + ':' + _taskGroupName + ':' + taskId;
        }

        string GetResultKey(string taskId)
        {
            return GetKey(taskId) + ":info";
        }

        public void SetResult(string taskId, string result)
        {
            var state = GetState(taskId);

            if (String.IsNullOrEmpty(state))
                throw new ArgumentException(nameof(taskId));

            Register(taskId, state, result);
        }

        public string GetResult(string taskId)
        {
            var state = GetInfoState(taskId);
            return state;
        }

        /// <summary>
        /// Get a context block that can wrap some code to ensure that the start and stop are always called.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public ITaskManagerContext GetContextBlock(string taskId)
        {
            return new TaskManagerContext(this, taskId);
        }

        /// <summary>
        /// Used to wrap a block of code to ensure the task is started and completed.
        /// </summary>
        public class TaskManagerContext : ITaskManagerContext
        {
            ITaskManager _manager;
            string _taskId;

            public TaskManagerContext(ITaskManager manager, string taskId)
            {
                _manager = manager;
                _taskId = taskId;
                _manager.RegisterStart(_taskId);
            }

            void IDisposable.Dispose()
            {
                if (!_manager.HasCompleted(_taskId))
                {
                    _manager.RegisterComplete(_taskId);
                }
            }
        }
    }
}
