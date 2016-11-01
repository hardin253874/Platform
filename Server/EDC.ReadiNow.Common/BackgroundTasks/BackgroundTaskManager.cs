// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;
using EDC.ReadiNow.Metadata.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.BackgroundTasks
{
    /// <summary>
    /// Manages the background tasks that are running in the system
    /// </summary>
    public class BackgroundTaskManager : IBackgroundTaskManager
    {
        const string QueuePrefix = "BackgroundTaskManager";
        readonly object _sync = new object();

        readonly int _waitForStopTimeMs;
        readonly int _perTenantConcurrency;
        readonly ITenantQueueFactory _tenantQueueFactory;

        private Dictionary<long, QueueActioner<BackgroundTask>> TenantActioners { get; } 

        private Dictionary<string, ITaskHandler> TaskHandlers { get; } 
             

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantQueueFactory">The factory used to generate the tenant queues</param>
        /// <param name="waitForStopTimeMs">The length of time to wait for all queues to stop before terminating them</param>
        public BackgroundTaskManager(ITenantQueueFactory tenantQueueFactory, int perTenantConcurrency = 20, int waitForStopTimeMs = 30000, IEnumerable<ITaskHandler> handlers = null)
        {
            _tenantQueueFactory = tenantQueueFactory;
            _waitForStopTimeMs = waitForStopTimeMs;
            _perTenantConcurrency = perTenantConcurrency;
            TenantActioners = new Dictionary<long, QueueActioner<BackgroundTask>>();

            if (handlers == null)
            {
                handlers = Core.Factory.Current.Resolve<IEnumerable<ITaskHandler>>();
            }

            TaskHandlers = handlers.ToDictionary(h => h.TaskHandlerKey, h => h);

            SyncQueues();
        }


        /// <summary>
        /// Start the task manager
        /// </summary>
        public void Start()
        {
            SyncQueues();

            foreach (var actioner in TenantActioners.Values)
            {
                actioner.Start();       // actioners are thread safe
            }

            EventLog.Application.WriteInformation($"BackgroundTaskManager started");
        }

        /// <summary>
        /// Stop the task manager
        /// </summary>
        public void Stop()
        {
            var actioners = TenantActioners.Values;

            // Tell them all to stop.
            foreach (var actioner in actioners)
            {
                actioner.Stop(0);      
            }

            // Wait for them to stop
            foreach (var actioner in actioners)
            {
                actioner.Stop(); 
            }
            EventLog.Application.WriteInformation($"BackgroundTaskManager stopped");
        }


        /// <summary>
        /// Make sure a queue exusts for every tenant and have the correct concurrency
        /// </summary>
        private void SyncQueues()
        {
            lock (_sync)
            {
                List<long> tenantIds;

                using (new GlobalAdministratorContext())
                {
                    tenantIds = TenantHelper.GetAll().Select(t => t.Id).ToList();
                }

                var newTenants = tenantIds.Except(TenantActioners.Keys);

                var redisManager = new RedisManager();
                redisManager.Connect();

                foreach (var tenantId in newTenants)
                {
                    var queue = _tenantQueueFactory.Create(tenantId);
                    TenantActioners.Add(tenantId, new QueueActioner<BackgroundTask>(queue, ProcessTask, _perTenantConcurrency));
                }
                
            }
        }

        /// <summary>
        /// Add a tenant to manage the background tasks of
        /// </summary>
        /// <param name="tenantId"></param>
        public void AddTenant(long tenantId)
        {
            SyncQueues();
        }

        public void EnqueueTask(BackgroundTask task)
        {
            EnqueueTask(RequestContext.TenantId, task);
        }

        public void EnqueueTask(long tenantId, BackgroundTask task)
        { 
            QueueActioner<BackgroundTask> actioner;

            if (!TenantActioners.TryGetValue(tenantId, out actioner))
                throw new UnknownTenantException(tenantId);

            actioner.Queue.Enqueue(task);
        }

        /// <summary>
        /// Run the provided task in the current thread immediately
        /// </summary>
        /// <param name="task">The task to run</param>
        public void ExecuteImmediately(BackgroundTask task)
        {
            ProcessTask(task);
        }

        /// <summary>
        /// Empty all the tasks out of current tenants Queue
        /// </summary>
        /// <returns>The tasks</returns>
        public IEnumerable<BackgroundTask> EmptyQueue()
        {
            return EmptyQueue(RequestContext.TenantId);
        }

        /// <summary>
        /// Empty all the tasks out of the given tenants queue
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns>The tasks</returns>
        public IEnumerable<BackgroundTask> EmptyQueue(long tenantId)
        {
            var result = new List<BackgroundTask>();

            QueueActioner<BackgroundTask> actioner;

            if (!TenantActioners.TryGetValue(tenantId, out actioner))
                throw new UnknownTenantException(tenantId);

            BackgroundTask next = null;

            do
            {
                next = actioner.Queue.Dequeue();

                if (next != null)
                    result.Add(next);
            } while (next != null);

            return result;
        }



        private void ProcessTask(BackgroundTask task)
        {
            if (task == null)
            {
                EventLog.Application.WriteError($"Tried to process a null task. Dropping task.");
            }


            ITaskHandler handler;
            if (!TaskHandlers.TryGetValue(task.HandlerKey, out handler))
            {
                EventLog.Application.WriteError($"Tried to process task without registered handler. Dropping task. Handler: {task.HandlerKey}");

                return;
            }

            using (EntryPointContext.SetEntryPoint("BackgroundTask"))
            {
                ProcessMonitorWriter.Instance.Write("BackgroundTask");
                using (DeferredChannelMessageContext deferredMsgContext = new DeferredChannelMessageContext())
                {
                    var contextData = task.Context;
                    using (CustomContext.SetContext(contextData))
                    {
                        handler.HandleTask(task);
                    }
                }
            }
        }

        /// <summary>
        /// Get the queue name and lengths
        /// </summary>
        /// <returns></returns>
        public IEnumerable<QueueLengthEntry> QueueLengths()
        {
            return TenantActioners.Select(entry =>
                new QueueLengthEntry
                {
                    QueueName = entry.Value.Queue.Name,
                    TenantName = TenantHelper.GetTenantName(entry.Key),
                    TenantId = entry.Key,
                    Length = entry.Value.Queue.Length
                }
            );
        }

        public class UnknownTenantException : Exception
        {
            public UnknownTenantException(long tenantId) : base($"Attempted to add a task for an unknown tenant: {tenantId}")
            {
            }
        }


    }
}
