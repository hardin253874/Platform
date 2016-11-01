// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Messaging.Redis;

namespace EDC.ReadiNow.Messaging
{
    /// <summary>
    /// DI module for messaging, including Redis..
    /// </summary>
    public class MessagingModule : Module
    {
        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(cc =>
                {
                    RedisManager redisManager = new RedisManager();
                    redisManager.Connect();
                    return redisManager;
                })
                .As<IDistributedMemoryManager>()
                .SingleInstance();

            builder.RegisterType<TaskManager>().As<ITaskManager>().SingleInstance();
        }
    }
}
