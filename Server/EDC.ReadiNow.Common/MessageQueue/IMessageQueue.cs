// Copyright 2011-2016 Global Software Innovation Pty Ltd

using RabbitMQ.Client;

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// Allows access to a message queue channel through RabbitMQ's <see cref="IModel"/> interface.
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        /// Creates and connects to a channel allowing messages to be sent and received remotely via the queue.
        /// </summary>
        /// <returns>The instance of the channel object.</returns>
        IModel Connect();
    }
}
