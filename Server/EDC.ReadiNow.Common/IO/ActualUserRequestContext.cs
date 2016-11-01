// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Remoting.Messaging;

namespace EDC.ReadiNow.IO
{
    /// <summary>
    /// Stores the request context in a new slot if it refers to an actual user.
    /// An actual user is one with a non-zero id. i.e. not the system admin or tenant admin.
    /// </summary>
    internal class ActualUserRequestContext
    {
        private const string ActualUserContextKey = "ReadiNow Actual User Request Context";


        /// <summary>
        ///     Sets the context only if it refers to an actual user.
        /// </summary>
        /// <param name="contextData">The context data.</param>
        internal static void SetContext(RequestContextData contextData)
        {            
            if (contextData != null &&
                contextData.Identity != null &&
                contextData.Identity.Id > 0)
            {
                // Set the context if it refers to an actual user
                CallContext.SetData(ActualUserContextKey, contextData);
            }            
        }


        /// <summary>
        ///     Gets the context.
        /// </summary>
        /// <returns></returns>
        internal static RequestContext GetContext()
        {
            var contextData = (RequestContextData) CallContext.GetData(ActualUserContextKey);
            return new RequestContext(contextData);            
        }


        /// <summary>
        ///     Frees the context.
        /// </summary>
        internal static void FreeContext()
        {
            CallContext.FreeNamedDataSlot(ActualUserContextKey);
        }
    }
}