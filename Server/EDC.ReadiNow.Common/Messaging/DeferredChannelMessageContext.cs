// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model.CacheInvalidation;
using System;
using System.Collections.Concurrent;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace EDC.ReadiNow.Messaging
{
    /// <summary>
    /// The deferred channel message context.
    /// </summary>
    public sealed class DeferredChannelMessageContext : IDisposable
    {
        public static readonly string HostContextKeyName = "ReadiNow Deferred Channel Message Context (Host Context)";
        private const string CallContextKeyName = "ReadiNow Deferred Channel Message Context";

        /// <summary>
        /// The context stack for this thread.
        /// This is required because the Dispose call that happens in Global.Application_EndRequest
        /// is on a different thread than the thread which created and registered the context.
        /// This means that attempting to get the context stack in Global.Application_EndRequest
        /// will get the incorrect one.
        /// </summary>
        private ConcurrentStack<DeferredChannelMessageContextEntry> _contextStack;

        private bool _disposed = false;

        /// <summary>
        /// The entry associated with this context.
        /// </summary>
        private DeferredChannelMessageContextEntry _entry;

        /// <summary>
        /// Construct a new message context.
        /// </summary>
        public DeferredChannelMessageContext() : this(ContextType.New)
        {
        }

        /// <summary>
        /// Construct a new message context.
        /// </summary>
        /// <param name="contextType">Whether this is a new or attached context.</param>
        internal DeferredChannelMessageContext(ContextType contextType)
        {
            ContextType = contextType;

            switch (contextType)
            {
                case ContextType.New:
                    _entry = new DeferredChannelMessageContextEntry();

                    // Store an instance of the context stack
                    _contextStack = GetContextStack(true);

                    // Push entry onto the context stack
                    _contextStack.Push(_entry);
                    break;

                case ContextType.Attached:
                    // Attach to a context entry
                    GetContextEntryStack().TryPeek(out _entry);
                    break;

                case ContextType.Detached:
                    _entry = new DeferredChannelMessageContextEntry();
                    break;

                default:
                    throw new ArgumentException("Unknown contextType", "contextType");
            }
        }

        /// <summary>
        /// Gets the context type.
        /// </summary>
        public ContextType ContextType { get; private set; }


        /// <summary>
        /// Gets or sets a value indicating whether [suppress no context warning].
        /// </summary>
        /// <value>
        /// <c>true</c> if [suppress no context warning]; otherwise, <c>false</c>.
        /// </value>
        public static bool SuppressNoContextWarning
        {
            get; set;
        }

        /// <summary>
        /// Get the current (top most) message context if the context exists or a detached context otherwise.
        /// </summary>
        /// <returns>
        /// The current <see cref="DeferredChannelMessageContext"/>.
        /// </returns>
        public static DeferredChannelMessageContext GetContext()
        {
            return new DeferredChannelMessageContext(IsSet() ? ContextType.Attached : ContextType.Detached);
        }

        /// <summary>
        /// Checks if the context has been set in the thread local or host context.
        /// </summary>
        /// <returns>
        /// True if the context has been set, false otherwise.
        /// </returns>
        public static bool IsSet()
        {
            return IsContextStackSet(GetContextStack(false)) || GetDeferredChannelContextFromHostContext() != null;
        }

		/// <summary>
		/// Adds a message to the store assigned to the specified channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channelName">The channel name.</param>
		/// <param name="message">The message</param>
		/// <param name="mergeAction">The merge action.</param>
		/// <exception cref="System.ArgumentNullException">channelName</exception>
        public void AddOrUpdateMessage<T>(string channelName, T message, Action<T, T> mergeAction)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                throw new ArgumentNullException("channelName");
            }

            _entry.AddOrUpdateMessage(channelName, message, mergeAction);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Publishes all the messages held by this entry via their respective channels.
        /// </summary>
        public void FlushMessages()
        {            
            _entry.FlushMessages();
        }

        /// <summary>
        /// Tries to get the message for the specified channel.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="message">The message</param>
        /// <returns>True if the message was found, false otherwise</returns>
        public bool TryGetMessage<T>(string channelName, out T message)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                throw new ArgumentNullException("channelName");
            }

            return _entry.TryGetMessage(channelName, out message);
        }

        /// <summary>
        /// Gets the context entry stack.
        /// </summary>
        /// <returns></returns>
        private static ConcurrentStack<DeferredChannelMessageContextEntry> GetContextEntryStack()
        {
            ConcurrentStack<DeferredChannelMessageContextEntry> contextStack = GetContextStack(false);
            if (IsContextStackSet(contextStack))
            {
                return contextStack;
            }
            else
            {
                // Check host context. This is set in Global.asax.Application_BeginRequest
                DeferredChannelMessageContext context = GetDeferredChannelContextFromHostContext();
                if (context != null)
                {
                    return context._contextStack;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the context stack from the CallContext
        /// </summary>
        /// <param name="createIfNull"></param>
        /// <returns></returns>
        private static ConcurrentStack<DeferredChannelMessageContextEntry> GetContextStack(bool createIfNull)
        {
            ConcurrentStack<DeferredChannelMessageContextEntry> contextStack = CallContext.GetData(CallContextKeyName) as ConcurrentStack<DeferredChannelMessageContextEntry>;
            if (contextStack == null && createIfNull)
            {
                contextStack = new ConcurrentStack<DeferredChannelMessageContextEntry>();
                CallContext.SetData(CallContextKeyName, contextStack);
            }

            return contextStack;
        }

        /// <summary>
        /// Gets the channel context from the host context.
        /// </summary>
        /// <returns></returns>
        private static DeferredChannelMessageContext GetDeferredChannelContextFromHostContext()
        {
            HttpContext httpContext = CallContext.HostContext as HttpContext;

            if (httpContext != null)
            {
                return httpContext.Items[HostContextKeyName] as DeferredChannelMessageContext;
            }

            return null;
        }

        /// <summary>
        /// Checks if the context has been set in the thread local context stack.
        /// </summary>
        /// <returns>
        /// True if the context has been set, false otherwise.
        /// </returns>
        private static bool IsContextStackSet(ConcurrentStack<DeferredChannelMessageContextEntry> contextStack)
        {            
            return contextStack != null && !contextStack.IsEmpty;
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        /// <param name="disposing">
        /// True if called from Dispose, false if called from the finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Only dispose of managed resources if invoked by user code
            if (disposing)
            {
                try
                {
                    if (ContextType == ContextType.New)
                    {
                        PopContextData(_entry);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError("Unable to free cache context data: {0}", ex);
                }

                if (ContextType == ContextType.New)
                {
                    // Only flush messages when the context type is new
                    FlushMessages();
                }                

                _contextStack = null;
            }

            _disposed = true;
        }

		/// <summary>
		/// Pop the context
		/// </summary>
		/// <param name="expectedData">The expected data.</param>
		/// <exception cref="System.InvalidOperationException">Empty context stack</exception>
		/// <exception cref="System.ArgumentException">Popping wrong data;expectedData</exception>
        private void PopContextData(DeferredChannelMessageContextEntry expectedData)
        {
            if (_contextStack == null)
            {
                return;
            }

            DeferredChannelMessageContextEntry top;

            // Sanity checks
            if (!_contextStack.TryPeek(out top))
            {
                throw new InvalidOperationException("Empty context stack");
            }
            if (top != expectedData)
            {
                throw new ArgumentException("Popping wrong data", "expectedData");
            }

            _contextStack.TryPop(out top);

            if (_contextStack.Count <= 0)
            {
                CallContext.FreeNamedDataSlot(CallContextKeyName);
            }
        }
    }
}