// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EDC.Diagnostics;

namespace EDC.Threading
{
    /// <summary>
    ///     This class defines a named rendezvous point.
    ///     The rendezvous point is reached when this class is disposed.
    ///     At the rendezvous point this class will run any registered actions
    ///     and will wait until the specified timeout has elapsed.
    /// </summary>
    public class RendezvousPoint : IDisposable
    {
        /// <summary>
        ///     The active rendezvous points.
        /// </summary>
        private static readonly ConcurrentDictionary<string, RendezvousPoint> RendezvousPoints = new ConcurrentDictionary<string, RendezvousPoint>();


        /// <summary>
        ///     The id of the rendezvous point.
        /// </summary>
        private readonly string _id;


        /// <summary>
        ///     The timeout in milliseconds that this instance
        ///     will wait at the rendezvous point for any actions to complete.
        /// </summary>
        private readonly int _millisecondsTimeout;


        /// <summary>
        ///     True if disposed, false otherwise.
        /// </summary>
        private bool _disposed;


        /// <summary>
        ///     The actions to run at the rendezvous point.
        /// </summary>
        private ConcurrentQueue<Action> _rendezvousActions = new ConcurrentQueue<Action>();


        /// <summary>
        /// The event log
        /// </summary>
        private readonly IEventLog _eventLog;


        /// <summary>
        /// Initializes a new instance of the <see cref="RendezvousPoint" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="millisecondsTimeout">The maximum amount of time to wait at the rendezvous point. -1 means wait
        /// indefinitely.</param>
        /// <param name="eventLog">The event log.</param>
        /// <exception cref="System.ArgumentNullException">id</exception>
        /// <exception cref="System.ArgumentException">The rendezvousPoint id is already used.;id</exception>
        public RendezvousPoint(string id, int millisecondsTimeout, IEventLog eventLog)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (!RendezvousPoints.TryAdd(id, this))
            {
                throw new ArgumentException(@"The rendezvousPoint id is already used.", "id");
            }            

            _id = id;
            _eventLog = eventLog;
            _millisecondsTimeout = millisecondsTimeout <= 0 ? -1 : millisecondsTimeout;
        }


        #region IDisposable Members


        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion


        /// <summary>
        ///     Finalizes an instance of the <see cref="RendezvousPoint" /> class.
        /// </summary>
        /// <remarks>
        ///     If this is run, the rendezvous point is only removed from the registered rendezvous points. Registered actions are
        ///     NOT run.
        /// </remarks>
        ~RendezvousPoint()
        {
            Dispose(false);
        }


        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Remove from the registered rendezvous points
            RendezvousPoint removedPoint;
            RendezvousPoints.TryRemove(_id, out removedPoint);

            // Dispose is called via Finalizer, so exit here.
            if (!disposing)
            {
                return;
            }

            RunActionsAndWait();
        }


        /// <summary>
        ///     Runs the actions and wait.
        /// </summary>
        private void RunActionsAndWait()
        {
            try
            {
                ConcurrentQueue<Action> rendezvousActions = Interlocked.Exchange(ref _rendezvousActions, new ConcurrentQueue<Action>());                

                // Run the registered actions and wait
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    CancellationToken cancellationToken = cancellationTokenSource.Token;

                    // Run all rendezvous actions asynchronously
                    Task[] tasks = rendezvousActions.Select(action => Task.Run(() => RunAction(action, cancellationToken), cancellationToken)).ToArray();

                    if (!Task.WaitAll(tasks, _millisecondsTimeout, cancellationToken))
                    {
                        cancellationTokenSource.Cancel();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "An error occured waiting for rendezvous point actions to complete. Id:{0}, Error:{1}.";

                if (_eventLog != null)
                {
                    _eventLog.WriteError(msg, _id, ex.ToString());
                }
                else
                {
                    Trace.TraceError(msg, _id, ex);
                }                
            }
        }


        /// <summary>
        ///     Runs the action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private void RunAction(Action action, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                action();
            }
            catch (Exception ex)
            {
                string msg = "An error occured running rendezvous point action. Id:{0}, Error:{1}.";

                if (_eventLog != null)
                {
                    _eventLog.WriteWarning(msg, _id, ex.ToString());
                }
                else
                {
                    Trace.TraceWarning(msg, _id, ex);
                }                
            }
        }


        /// <summary>
        ///     Registers the specified action to be run when the rendezvous point is reached.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     id
        ///     or
        ///     action
        /// </exception>
        public static void RegisterAction(string id, Action action)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            RendezvousPoint rendezvousPoint;

            if (RendezvousPoints.TryGetValue(id, out rendezvousPoint))
            {
                rendezvousPoint._rendezvousActions.Enqueue(action);
            }
        }
    }
}