// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Annotations;

namespace ReadiNow.Core
{
    /// <summary>
    /// Interface for service that starts running code asynchronously.
    /// </summary>
    /// <remarks>
    /// This is for tasks that are intended to end - not for long running background tasks.
    /// </remarks>
    public interface IAsyncRunner
    {
        /// <summary>
        /// Start running a task asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="settings"></param>
        void Start( [NotNull] Action action, [CanBeNull] AsyncRunnerSettings settings = null );
    }

    /// <summary>
    /// Settings for starting async tasks.
    /// </summary>
    public class AsyncRunnerSettings
    {
        
    }
}
