// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Messaging.Redis
{
    /// <summary>
    /// Given a ListeningQueue, the actioner will repeatedly Dequeue and run an action.
    /// </summary>
    public class QueueActioner<T>: IDisposable
    {
        const string ActionLoopTheadPrefix = "Actioner Loop";

        const int DisposeStopWait = 100;                // How long to wait during a dispose operation for the thread to stop.
        const int TaskStopWait = 300 * 1000;                   // How long to wait during a stop for the tasks to complete.
        const int TaskReportingTime = 60 * 1000;        // Interval between reporting

        private object _syncRoot = new object();        // for locking
        private Action<T> Action { get; }

        private Thread LoopingThread { get; set; }      // The thread that runs the action. This is alive so long as the Actioner is running.

        private AutoResetEvent WaitForQueueEvent { get; set; }  // Used to block the LoopingThread if there is nothing to process.
        private AutoResetEvent WaitForTaskEvent { get; set; }  // Used to block the LoopingThread if there is nothing to process.

        private  ISet<Task> RunningTasks { get; set; }  // The current running task tokens
        private CancellationTokenSource CancelTokenSrc { get; set; }  // The current running task tokens

        /// <summary>
        /// The state the actioner is in.
        /// </summary>
        public ActionerState State { get; private set; }

        /// <summary>
        /// The queue the actioner is working on
        /// </summary>
        public IQueue<T> Queue { get; }

        /// <summary>
        /// the maximum amount of concurrency that this actioner supports
        /// </summary>
        public int MaxConcurrency { get; }

        /// <summary>
        /// The number of running tasks
        /// </summary>
        public int RunningTaskCount { get { return RunningTasks.Count(); } }

        /// <summary>
        /// Create an actioner for the given queue
        /// </summary>
        /// <param name="queue">The queue to run the action on</param>
        /// <param name="action">The action to perform</param>
        /// <param name="maxThreads">the maximum number of threads to run with</param>
        public QueueActioner(IListeningQueue<T> queue, Action<T> action, int maxConcurrency)
        {
            Queue = queue;
            Action = action;
            MaxConcurrency = maxConcurrency;

            State = ActionerState.Stopped;

            WaitForQueueEvent = new AutoResetEvent(false);
            WaitForTaskEvent = new AutoResetEvent(false);
            RunningTasks = new HashSet<Task>();

            queue.EnqueuedReceived += Queue_EnqueuedReceived;
        }

        /// <summary>
        /// Called when an enqueue event is received. This toggles the WaitEvent allowing further entries to be processed.
        /// </summary>
        private void Queue_EnqueuedReceived(object sender, EventArgs e)
        {
            if (!(State == ActionerState.Stopped || State == ActionerState.Stopping))
                WaitForQueueEvent.Set();
        }

        /// <summary>
        /// Start the actioner. It will begin processing the entries until stopped.
        /// </summary>
        public void Start()
        {
            CancelTokenSrc = new CancellationTokenSource();

            if (State == ActionerState.Stopping)
                throw new ActionerStartingWhileStoppingException(Queue.Name ?? "[Unnamed]");

            if (State == ActionerState.Running)
                return;

            if (LoopingThread != null)
                throw new ApplicationException("Attempted to start a loop while another was still running. This should never happen.");

            State = ActionerState.Running;

            LoopingThread = new Thread(ActionLoop) { Name = $"{ActionLoopTheadPrefix} {Queue.Name}" };
            LoopingThread.Start();
        }



        /// <summary>
        /// Stop the Actioner
        /// </summary>
        /// <param name="timeoutMs">How long to wait in ms for the actioner to complete, -1 to wait forever</param>
        /// <returns>true if thread was or is stopped.</returns>
        public bool Stop(int timeoutMs = -1)
        {
            if (State != ActionerState.Stopped)
            {

                lock (_syncRoot)
                {
                    if (State == ActionerState.Running || State == ActionerState.WaitingForQueue || State == ActionerState.WaitingForTask)
                    {
                        State = ActionerState.Stopping;
                    }
                }


                // cancel running tasks
                if (CancelTokenSrc != null)
                {
                    CancelTokenSrc.Cancel();
                    CancelTokenSrc.Dispose();
                    CancelTokenSrc = null;
                }
            }

            // Get rid of the looping thread
            if (LoopingThread != null)
            {
                WaitForQueueEvent.Set();
                WaitForTaskEvent.Set();

                if (LoopingThread.Join(timeoutMs))
                {
                    LoopingThread = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }



        /// <summary>
        /// The loop that pulls elements from the Queue and acts upon them.
        /// </summary>
        private void ActionLoop()
        {
            var lastReport = DateTime.UtcNow;

            while (true)
            {
                if (DateTime.UtcNow - lastReport > TimeSpan.FromMilliseconds(TaskReportingTime))
                {
                    lastReport = DateTime.UtcNow;
                    Report();
                }

                try
                {
                    switch (State)
                    {
                        case ActionerState.Running:

                            if (RunningTasks.Count() < MaxConcurrency)
                            {
                                T next = Queue.Dequeue();

                                if (next != null)
                                {
                                    QueueAction(next);
                                }
                                else
                                {
                                    SetStateIfNotStopping(ActionerState.WaitingForQueue);
                                }
                            }
                            else
                            {
                                SetStateIfNotStopping(ActionerState.WaitingForTask);
                            }
                            break;

                        case ActionerState.WaitingForQueue:
                            var timeout = WaitForQueueEvent.WaitOne(TaskReportingTime);

                            SetStateIfNotStopping(ActionerState.Running);
                            break;

                        case ActionerState.WaitingForTask:
                            WaitForTaskEvent.WaitOne(TaskReportingTime);
                            SetStateIfNotStopping(ActionerState.Running);
                            break;

                        case ActionerState.Stopping:
                            if (!Task.WaitAll(RunningTasks.ToArray(), TaskStopWait))
                            {
                                EventLog.Application.WriteError($"Failed to stop all the running tasks when shutting down QueueActioner.");
                            }
                            State = ActionerState.Stopped;
                            Report();
                            return;

                        case ActionerState.Stopped:
                            EventLog.Application.WriteError($"A Stopped thread should not be processing.");
                            return;
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError($"Unhandled exception in QueueAction loop. Exception: {ex}");
                    // exception not rethrown
                }
            }


        }

        void Report()
        {
            EventLog.Application.WriteTrace($"QueueActioner Running Tasks: {Queue.Name}, {RunningTasks.Count}");
        }


        void SetStateIfNotStopping(ActionerState newState)
        {
            lock (_syncRoot)
            {
                if (State != ActionerState.Stopping && State != ActionerState.Stopped)
                    State = newState;
            }
        }

        void QueueAction(T next)
        {
            Task task = null;

            var ct = CancelTokenSrc.Token;

            task = Task.Factory.StartNew(() =>
            {
                // If we are cancelled before we start, push the job back on the queue so it can be dealt with by another runner.
                if (ct.IsCancellationRequested)
                {
                    Queue.Enqueue(next);
                    return;
                }

                SafeAction(next);

                lock(_syncRoot) RunningTasks.Remove(task);

                WaitForTaskEvent.Set();
            });

            lock (_syncRoot) RunningTasks.Add(task);
        }

        void SafeAction(T next)
        {
            try
            {
                Action(next);
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError($"Uncaught exception running a queue action. This should never occur. Exception: {ex}");
                // Exception not thrown to prevent the thread from falling over
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop(DisposeStopWait);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    /// <summary>
    /// Thrown if there is an attempt to start an actioner in the process of stopping.
    /// </summary>
    public class ActionerStartingWhileStoppingException : Exception
    {
        public ActionerStartingWhileStoppingException(string queueName) : base($"Attempted to start a stopping actioner: {queueName}")
        {
        }
    }

    public enum ActionerState { Stopped, Running, WaitingForQueue, WaitingForTask, Stopping };

}
