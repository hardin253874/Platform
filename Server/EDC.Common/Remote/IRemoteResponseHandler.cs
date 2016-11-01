// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Remote
{
    /// <summary>
    /// Describes a type that may participate in RPC behavior initiated by <see cref="IRemoteSender.Request{T,TResult}"/>. The
    /// type is instantiated by the Requester once a response to the original message has been received.
    /// </summary>
    /// <typeparam name="T">The request message type.</typeparam>
    /// <typeparam name="TResult">The response message type.</typeparam>
    public interface IRemoteResponseHandler<in T, in TResult>
    {
        /// <summary>
        /// Performs some function based on the response received for a given request.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="response">The response message.</param>
        void Process(T request, TResult response);
    }
}
