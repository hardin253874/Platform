// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// An event that fires for a CAST activity that has received a response in turn for a remote request made.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class CastActivityResponseEvent<TRequest, TResponse> : IWorkflowEvent, ICastActivityResponseEvent
        where TRequest : CastRequest
        where TResponse : CastResponse
    {
        /// <summary>
        /// Constructs the event from the request and response objects.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="response">The response object.</param>
        public CastActivityResponseEvent(TRequest request, TResponse response)
        {
            Request = request;
            Response = response;
        }

        /// <summary>
        /// The original request object.
        /// </summary>
        public TRequest Request { get; private set; }

        /// <summary>
        /// The response object that triggered the event.
        /// </summary>
        public TResponse Response { get; private set; }

    }
}
