// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.Remote;

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// DI module for the message queuing component.
    /// </summary>
    public class MessageQueueModule : Module
    {
        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessageQueue>().As<IMessageQueue>();
            builder.RegisterType<MessageQueueListener>().As<IRemoteListener>();
            builder.RegisterType<MessageQueueSender>().As<IRemoteSender>();
        }
    }
}
