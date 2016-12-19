// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Threading;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using ReadiNow.Core;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Core.AsyncRunner
{
    /// <summary>
    /// Starts an async task.
    /// </summary>
    public class AsyncRunner : IAsyncRunner
    {
        /// <summary>
        /// Start running a task asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="settings"></param>
        public void Start( Action action, AsyncRunnerSettings settings = null )
        {
            var context = RequestContext.GetContext( );
            var contextData = new RequestContextData( context );

            Action wrappedAction = ( ) =>
            {
                using ( CustomContext.SetContext( contextData ) )
                {
                    action( );
                }
            };

            ThreadPool.QueueUserWorkItem( StartThread, wrappedAction );
        }

        /// <summary>
        /// Entry point to the thread.
        /// </summary>
        /// <param name="o">
        /// The <see cref="Action"/> to run. This cannot be null.
        /// </param>
        private static void StartThread( object o )
        {
            try
            {
                Action action = ( Action ) o;

                using (new DeferredChannelMessageContext())
                using ( DatabaseContext.GetContext( ) )
                {
                    action( );
                }
            }
            catch ( Exception ex )
            {
                EventLog.Application.WriteError(
                    "WorkflowRunContext.StartThread: Unexpected exception thrown: {0}",
                    ex );
            }
        }
    }
}
