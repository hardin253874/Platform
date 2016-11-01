// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Remote
{
    /// <summary>
    /// Describes interactions that may push messages from this instance to another.
    /// </summary>
    public interface IRemoteSender
    {
        /// <summary>
        /// Sends a message remotely that may optionally be durable. The initiating half of <see cref="IRemoteListener.Receive{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of message to serialize and send.</typeparam>
        /// <param name="key">The key used to retrieve the message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="guarantee">If true, the message will be durable and will wait for acknowledgement of receipt.</param>
        void Send<T>(string key, T message, bool guarantee = true);

        /// <summary>
        /// Broadcasts a message remotely to none or more listeners. No guarantees about delivery and if not listening others will
        /// miss the message. The push side of the pub/sub pairing with <see cref="IRemoteListener.Subscribe{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of message to serialize and publish.</typeparam>
        /// <param name="key">The key used to retrieve the message.</param>
        /// <param name="message">The message to send.</param>
        void Publish<T>(string key, T message);

        /// <summary>
        /// Sends a message remotely while anticipating a response in return. Effectively an RPC coupling used 
        /// together with <see cref="IRemoteListener.Respond{T,TResult}"/>.
        /// </summary>
        /// <typeparam name="T">The type of message to serialize and send.</typeparam>
        /// <typeparam name="TResult">The type of message expected in response.</typeparam>
        /// <param name="key">The key used to retrieve the message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="handler">A handler object that may be instantiated to process the return message.</param>
        void Request<T, TResult>(string key, T message, IRemoteResponseHandler<T, TResult> handler);
    }
}
