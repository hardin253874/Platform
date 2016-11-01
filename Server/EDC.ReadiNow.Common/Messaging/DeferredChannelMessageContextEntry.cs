// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EDC.ReadiNow.Messaging
{
    /// <summary>
    /// The deferred channel message context entry.
    /// </summary>
    public class DeferredChannelMessageContextEntry
    {
        /// <summary>
        /// Dictionary of type specific FlushChannelMessages actions.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Action<IDistributedMemoryManager, string, DeferredChannelMessage>> TypeSpecificFlushActionDictionary = new ConcurrentDictionary<Type, Action<IDistributedMemoryManager, string, DeferredChannelMessage>>();

        /// <summary>
        /// The channel message store. This stores the deferred message per channel.
        /// The outer dictionary is keyed off the channel name.
        /// </summary>
        private ConcurrentDictionary<string, DeferredChannelMessage> _deferredChannelMessageStore = new ConcurrentDictionary<string, DeferredChannelMessage>();

        /// <summary>
        /// The distributed memory manager.
        /// </summary>
        private readonly IDistributedMemoryManager _distributedMemoryManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public DeferredChannelMessageContextEntry() : this(Entity.DistributedMemoryManager)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeferredChannelMessageContextEntry(IDistributedMemoryManager distributedMemoryManager)
        {
            if (distributedMemoryManager == null)
            {
                throw new ArgumentNullException("distributedMemoryManager");
            }

            _distributedMemoryManager = distributedMemoryManager;
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

            if (Equals(message, default(T)))
            {
                return;
            }

            _deferredChannelMessageStore.AddOrUpdate(channelName, new DeferredChannelMessage(typeof(T), message), (k, existingMessage) =>
            {
                // The message already exists so update it.
                if (mergeAction == null)
                {
                    throw new InvalidOperationException(string.Format("An existing message is already queued against channel {0} with no mergeAction specified.", channelName));
                }
                if (existingMessage != null && existingMessage.Message != null && !(existingMessage.Message is T))
                {
                    throw new InvalidOperationException(string.Format("An existing message of type '{0}' cannot be merged with type '{1}' for channel '{2}'.", existingMessage.Message.GetType(), typeof(T), channelName));
                }

                mergeAction((T)existingMessage.Message, message);

                return existingMessage;
            });
        }

        /// <summary>
        /// Publishes all the messages held by this entry via their respective channels.
        /// </summary>
        public void FlushMessages()
        {
            ConcurrentDictionary<string, DeferredChannelMessage> channelMessageStore = _deferredChannelMessageStore;

            _deferredChannelMessageStore = new ConcurrentDictionary<string, DeferredChannelMessage>();

            foreach (KeyValuePair<string, DeferredChannelMessage> kvp in channelMessageStore)
            {
                string channelName = kvp.Key;
                DeferredChannelMessage message = kvp.Value;

                Action<IDistributedMemoryManager, string, DeferredChannelMessage> flushChannelAction = TypeSpecificFlushActionDictionary.GetOrAdd(message.MessageType, GetTypeSpecificFlushAction);

                flushChannelAction(_distributedMemoryManager, channelName, message);
            }
        }

        /// <summary>
        /// Tries to get the message for the specified channel. Mainly used for testing.
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

            message = default(T);
            DeferredChannelMessage channelMessage;

            if (!_deferredChannelMessageStore.TryGetValue(channelName, out channelMessage))
            {
                return false;
            }

            message = (T)channelMessage.Message;

            return true;
        }

		/// <summary>
		/// Flushes the messages via the specified channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="distributedMemoryManager">The distributed memory manager.</param>
		/// <param name="channelName">The channel name.</param>
		/// <param name="channelMessage">The channel message.</param>
	    [UsedImplicitly]
	    private static void FlushChannelMessages<T>(IDistributedMemoryManager distributedMemoryManager, string channelName, DeferredChannelMessage channelMessage)
        {
            if (channelMessage == null || channelMessage.Message == null)
            {
                return;
            }

            using (IChannel<T> channel = distributedMemoryManager.GetChannel<T>(channelName))
			using ( new TenantAdministratorContext( channelMessage.TenantId ) )
            {
                channel.Publish((T)channelMessage.Message, PublishMethod.Immediate, PublishOptions.FireAndForget );
            }
        }

        /// <summary>
        /// Gets the type specific flush channel message method.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <returns>An strongly typed action for flushing messages on a channel.</returns>
        private static Action<IDistributedMemoryManager, string, DeferredChannelMessage> GetTypeSpecificFlushAction(Type messageType)
        {
            ParameterExpression memManagerParam = Expression.Parameter(typeof(IDistributedMemoryManager), "memManager");
            ParameterExpression channelParam = Expression.Parameter(typeof(string), "channel");
            ParameterExpression messageParam = Expression.Parameter(typeof(DeferredChannelMessage), "message");

            MethodCallExpression callExpression = Expression.Call(typeof(DeferredChannelMessageContextEntry), "FlushChannelMessages",
                new[]
                {
                    messageType
                },
                new Expression[]
                {
                    memManagerParam,
                    channelParam,
                    messageParam
                });            

            LambdaExpression lambda = Expression.Lambda(callExpression, memManagerParam, channelParam, messageParam);

            var typedExpression = (Expression<Action<IDistributedMemoryManager, string, DeferredChannelMessage>>)lambda;

            return typedExpression.Compile();
        }

        /// <summary>
        /// Deferred channel messages object.
        /// </summary>
        private class DeferredChannelMessage
        {
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="messageType">The type of the message.</param>
			/// <param name="message">The message.</param>
			/// <exception cref="System.ArgumentNullException">messageType</exception>
            public DeferredChannelMessage(Type messageType, object message)
            {
                if (messageType == null)
                {
                    throw new ArgumentNullException("messageType");
                }

                MessageType = messageType;
				TenantId = RequestContext.IsSet ? RequestContext.TenantId : 0;
                Message = message;
            }

            /// <summary>
            /// The message.
            /// </summary>
            public object Message { get; private set; }

            /// <summary>
            /// The message type.
            /// </summary>
            public Type MessageType { get; private set; }

			/// <summary>
			/// Gets or sets the tenant identifier.
			/// </summary>
			/// <value>
			/// The tenant identifier.
			/// </value>
	        public long TenantId
	        {
		        get;
		        private set;
	        }
        }
    }
}