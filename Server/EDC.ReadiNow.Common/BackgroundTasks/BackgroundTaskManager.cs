// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
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

        Lazy<Dictionary<long, QueueActioner<BackgroundTask>>> _tenantActioners;
        private Dictionary<long, QueueActioner<BackgroundTask>> TenantActioners { get { return _tenantActioners.Value; } }

        private Dictionary<string, ITaskHandler> TaskHandlers { get; }

        /// <summary>
        /// Is this task manager active. Will it process queues?
        /// </summary>
        public bool IsActive { get; set; }


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

            if (handlers == null)
            {
                handlers = Core.Factory.Current.Resolve<IEnumerable<ITaskHandler>>();
            }

            TaskHandlers = handlers.ToDictionary(h => h.TaskHandlerKey, h => h);

            _tenantActioners = new Lazy<Dictionary<long, QueueActioner<BackgroundTask>>>(CreateActioners, false);
        }

        /// <summary>
        /// Start the task manager
        /// </summary>
        public void Start()
        {
            if (!IsActive)
                throw new ApplicationException("You cannot start an inactive task manager");

            if (IsActive)
            {

                foreach (var actioner in TenantActioners.Values)
                {
                    actioner.Start();       // actioners are thread safe
                }

                EventLog.Application.WriteInformation($"BackgroundTaskManager started");
            }
            else
            {
                EventLog.Application.WriteInformation($"BackgroundTaskManager started");
            }
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
        private Dictionary<long, QueueActioner<BackgroundTask>> CreateActioners()
        {
            var result = new Dictionary<long, QueueActioner<BackgroundTask>>();

            List<long> tenantIds;

            using (new GlobalAdministratorContext())
            {
                tenantIds = TenantHelper.GetAll().Select(t => t.Id).ToList();
            }

            foreach (var tenantId in tenantIds)
            {
                var queue = _tenantQueueFactory.Create(tenantId);

                result.Add(tenantId, new QueueActioner<BackgroundTask>(queue, ProcessTask, _perTenantConcurrency));
            }

            return result;
        }



        /// <summary>
        /// Add a tenant to manage the background tasks of
        /// </summary>
        /// <param name="tenantId"></param>
        public void AddTenant(long tenantId)
        {
            lock (_sync)
            {
                if (!TenantActioners.Keys.Contains(tenantId))
                {
                    var queue = _tenantQueueFactory.Create(tenantId);

                    TenantActioners.Add(tenantId, new QueueActioner<BackgroundTask>(queue, ProcessTask, _perTenantConcurrency));
                }
            }
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
            HandleTask(task, handler =>
            {
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
            });
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

        /// <summary>
        /// Suspend all queued tasks
        /// </summary>
        public void SuspendAllTasks()
        {
            foreach (var entry in TenantActioners)
            {
                var actioner = entry.Value;

                using (new TenantAdministratorContext(entry.Key))
                {
                    if (actioner.State != ActionerState.Stopped)
                        throw new ApplicationException("Cannot suspend a running actioner");

                    var queue = actioner.Queue;

                    var tasks = new List<BackgroundTask>();

                    while (true)
                    {
                        var task = queue.Dequeue();

                        if (task == null)
                            break;

                        tasks.Add(task);
                    }

                    var toSave = new List<IEntity>();

                    foreach (var group in tasks.GroupBy(t => t.HandlerKey))
                    {
                        if (group.Any())
                        {
                            HandleTask(group.Key, (handler) =>
                            {
                                toSave.AddRange(group.Select(task => handler.CreateSuspendedTask(task)));
                            });
                        }
                    }

                    EventLog.Application.WriteInformation($"Saving {toSave.Count} suspended tasks.");


                    if (toSave.Any())
                        Entity.Save(toSave);
                }
            }
        }



        private void HandleTask(BackgroundTask task, Action<ITaskHandler> act)
        {
            if (task == null)
            {
                EventLog.Application.WriteError($"Tried to process a null task. Dropping task.");
            }

            HandleTask(task.HandlerKey, act);
        }

        private void HandleTask(string handlerKey, Action<ITaskHandler> act)
        {
            ITaskHandler handler;
            if (!TaskHandlers.TryGetValue(handlerKey, out handler))
            {
                EventLog.Application.WriteError($"Tried to process task without registered handler. Dropping task. Handler: {handlerKey}");

                return;
            }

            act(handler);
        }


        /// <summary>
        /// Suspend all queued tasks
        /// </summary>
        public void RestoreAllTasks()
        {
            int restoredCount = 0;
            foreach (var tenant in TenantHelper.GetAll())
            {
                using (new TenantAdministratorContext(tenant.Id))
                {
                    foreach (var handler in TaskHandlers.Values)
                    {

                        var tasks = handler.RestoreSuspendedTasks();

                        foreach (var task in tasks)
                        {
                            EnqueueTask(task);
                            restoredCount++;
                        }
                    }
                }
            }

            EventLog.Application.WriteInformation($"Restoring {restoredCount} suspended tasks.");

        }

        public void GenerateReport(StringBuilder reportBuilder)
        {
            foreach (var ta in TenantActioners)
            {
                reportBuilder.AppendLine($"   Tenant: {TenantHelper.GetTenantName(ta.Key)}, State: {ta.Value.State}, Running {ta.Value.RunningTaskCount}/{ta.Value.MaxConcurrency}");
            }
        }


        public class UnknownTenantException : Exception
        {
            public UnknownTenantException(long tenantId) : base($"Attempted to add a task for an unknown tenant: {tenantId}")
            {
            }
        }


    }
}
