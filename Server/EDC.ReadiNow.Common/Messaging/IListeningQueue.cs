// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Messaging
{
    /// <summary>
    /// A distributed queue that also listens and raises an event when an enqueue occurs.
    /// </summary>
    public interface IListeningQueue<T> : IQueue<T>, IDisposable
    {
        /// <summary>
        /// Event which is raised when something has been added to the queue
        /// </summary>
        event EventHandler EnqueuedReceived;
    }
}
