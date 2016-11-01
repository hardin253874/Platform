// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Core;
using System.Threading.Tasks;
using EDC.ReadiNow.BackgroundTasks;

namespace EDC.ReadiNow.Common.Workflow
{
    /// <summary>
    /// A context that can be used to control how workflows and triggers behave. Contexts are nested and copy their parents values.
    /// </summary>
    public class WorkflowRunContext : IDisposable
    {
        /// <summary>
        ///  The maximum number of times the deferred action loop will run.
        /// </summary>
        const int MaxDeferredLoops = 10;                

        [ThreadStatic]
        static WorkflowRunContext _currentContext;

        static WorkflowRunContext _rootContext = new WorkflowRunContext (true) { RunTriggersInCurrentThread = false, DisableTriggers = false};

        /// <summary>
        /// Get the current context. Return null if there is no context set.
        /// </summary>
        public static WorkflowRunContext Current { get { return _currentContext ?? _rootContext; } }
        
        WorkflowRunContext _parentContext;
        
        IDisposable _profiler;

        /// <summary>
        /// Run any triggered workflows in the current thread
        /// </summary>
        public bool RunTriggersInCurrentThread { get; set; }

        /// <summary>
        /// Stop any triggers running in the current context
        /// </summary>
        public bool DisableTriggers { get; set; }

        /// <summary>
        /// If true and deferred saves and actions will be handled at the end of this context.
        /// </summary>
        private bool HandleDeferred { get; }

        /// <summary>
        /// The depth of the current trigger firing
        /// </summary>
        public int TriggerDepth { get; set; }

        private Queue<WorkflowRun> _deferredRuns = new Queue<WorkflowRun>();
        private Queue<Action> _deferredBeforeActions = new Queue<Action>();
        private Queue<Action> _deferredAfterActions = new Queue<Action>();

        public WorkflowRunContext() : this(false)
        {
        }

        public WorkflowRunContext(bool handleDeferred) : this(Current, handleDeferred)
        {
        }

        public WorkflowRunContext(WorkflowRunContext other) : this(other, false)
        {
        }

        public WorkflowRunContext(WorkflowRunContext other, bool handleDeferred)
        {
            HandleDeferred = handleDeferred;

            _profiler = Profiler.Measure("WorkflowRunContext", false);
            if (other != null)
            {
                RunTriggersInCurrentThread = other.RunTriggersInCurrentThread;
                DisableTriggers = other.DisableTriggers;
                TriggerDepth = other.TriggerDepth;
                _parentContext = _currentContext;
                _currentContext = this;
            }
        }
        
        /// <summary>
        /// Run a action in the background
        /// </summary>
        /// <param name="action"></param>
        public virtual void QueueAction(Action action)
        {
            var contextData = new RequestContextData(RequestContext.GetContext());
            var wfRunContext = WorkflowRunContext.Current;

            Action wrappedAction = ( ) =>
            {
                
                using (CustomContext.SetContext(contextData))
                using (new WorkflowRunContext(wfRunContext))  
                {
                    action();
                }
            };

            ThreadPool.QueueUserWorkItem(StartThread, wrappedAction);
        }


        /// <summary>
        /// Entry point to the thread running background workflows. Used by monitoring software.
        /// </summary>
        /// <param name="o">
        /// The <see cref="Action"/> to run. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="o"/> cannot be null and must be an <see cref="Action"/>.
        /// </exception>
        private static void StartThread(object o)
        {
            using (EntryPointContext.SetEntryPoint("WorkflowThread"))
            {
                ProcessMonitorWriter.Instance.Write("WorkflowThread");

                try
                {
                    Action action;

                    action = o as Action;
                    if (action == null)
                    {
                        throw new ArgumentException("Null or not an Action", "o");
                    }

                    using (DeferredChannelMessageContext deferredMsgContext = new DeferredChannelMessageContext())
                    using (DatabaseContext.GetContext())
                    {
                        action();
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError("WorkflowRunContext.StartThread: Unexpected exception thrown: {0}", ex);
                }
            }
        }

        public virtual void Dispose()
        {
            try
            {
                using (Profiler.Measure("WorkflowRunContext.Dispose"))
                {
                    // deal with deferred stuff
                    if (TriggerDepth == 0 || _parentContext == null || HandleDeferred)
                    {
                        HandleDeferredStuff();
                    }


                    // Pop
                    _currentContext = _parentContext;
                }
            }
            finally
            {
                if (_profiler != null)
                    _profiler.Dispose();
            }
        }

        public void DeferSave(WorkflowRun run)
        {
            if (this == _rootContext)
                throw new ArgumentException("run");

            if (HandleDeferred || _parentContext == null)
            {
                _deferredRuns.Enqueue(run);
            }
            else
            {
                _parentContext.DeferSave(run);
            }
        }


        /// <summary>
        /// A deferred action will not occur until after the current context completes or until we hit a handleDefered.
        /// </summary>
        /// <param name="action"></param>
        public void DeferAction(Action action, bool runBeforeSave = false)
        {
            if (this == _rootContext)
                throw new ArgumentException("action");
            
            if (HandleDeferred || _parentContext == null)
            {
                var queue = runBeforeSave ? _deferredBeforeActions : _deferredAfterActions;

                queue.Enqueue(action);
            }
            else
            {
                _parentContext.DeferAction(action, runBeforeSave);
            }
        }

        /// <summary>
        /// Deal with any deferred saves ans actions. Keep going until there is nothing more to handle.
        /// </summary>
        private void HandleDeferredStuff()
        {
            int loopCount = 0;

            do
            {

                if (_deferredBeforeActions.Any())
                    HandleDeferredActions(_deferredBeforeActions);

                // saves occur before actions to ensure that Ids are allocated - needed for logging.
                if (_deferredRuns.Any())
                    HandleDeferredSaves(_deferredRuns);

                if (_deferredAfterActions.Any())
                    HandleDeferredActions(_deferredAfterActions);

                if (loopCount++ > MaxDeferredLoops)
                {
                    EventLog.Application.WriteError("WorkflowRunContext.HandleDeferredStuff: Exceeded maximum number of deferred actions loops. This should never occur.");
                    throw new ApplicationException("Exceeded maximum number of deferred actions loops. This should never occur.");
                }
            }
            while (AnyDeferredActivity());
        }

        private bool AnyDeferredActivity()
        {
            return _deferredRuns.Any() || _deferredBeforeActions.Any() || _deferredAfterActions.Any();
        }

        private static void HandleDeferredSaves(Queue<WorkflowRun> runs)
        {
            var asDeferred = runs.WhereType<WorkflowRunDeferred>().ToList();

            using (new SecurityBypassContext())
            {
                DatabaseContext.RunWithRetry(() => Entity.Save(runs));
            }

             
            foreach (var deferredRun in asDeferred)
            {
                if (deferredRun.DeferredActions != null && deferredRun.DeferredActions.Count > 0)
                {
                    deferredRun.DeferredActions.ForEach(a =>
                    {
                        try
                        {
                            a.Invoke();
                        }
                        catch (Exception e)
                        {
                            EventLog.Application.WriteError("WorkflowRunContext.HandleDeferredRuns: Unexpected exception thrown: {0}", e);
                        }
                    });

                    deferredRun.DeferredActions.Clear();
                }
            }

            runs.Clear();

        }

        /// <summary>
        /// Process any deferred actions. 
        /// </summary>
        private static void HandleDeferredActions(Queue<Action> deferredActions)
        {
            while (deferredActions.Any())
            {
                var act = deferredActions.Dequeue();

                try
                {
                    act();
                }
                catch (Exception e)
                {
                    EventLog.Application.WriteError("WorkflowRunContext.HandleDeferredActions: Unexpected exception thrown: {0}", e);
                }
            }
        }
    }
}
