// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.Remote
{
    /// <summary>
    /// Describes an interface for listening for messages arriving from a remote instance.
    /// </summary>
    public interface IRemoteListener
    {
        /// <summary>
        /// Listens for messages that have been sent remotely via <see cref="IRemoteSender.Send{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the message expected.</typeparam>
        /// <param name="key">The key that the message was sent with.</param>
        /// <param name="handler">A handler function that can act on any messages received while listening.</param>
        /// <param name="guarantee">Must match the argument that the message was sent with.</param>
        void Receive<T>(string key, Action<T> handler, bool guarantee = true);

        /// <summary>
        /// Subscribes to messages that have been broadcast via <see cref="IRemoteSender.Publish{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the message expected.</typeparam>
        /// <param name="key">The key that the message was published with.</param>
        /// <param name="handler">A handler function that can act on any messages received while listening.</param>
        void Subscribe<T>(string key, Action<T> handler);

        /// <summary>
        /// Waits for messages sent with <see cref="IRemoteSender.Request{T,TResult}"/> and provides a response.
        /// </summary>
        /// <typeparam name="T">The type of the message expected.</typeparam>
        /// <typeparam name="TResult">The type of the response message to provide.</typeparam>
        /// <param name="key">The key that the message was sent with.</param>
        /// <param name="handler">A handler function that can responsd to any messages received while listening.</param>
        void Respond<T, TResult>(string key, Func<T, TResult> handler);

        /// <summary>
        /// Stops listening.
        /// </summary>
        void Stop();
    }
}
