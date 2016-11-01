// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.BackgroundTasks.Handlers;
using ProtoBuf.Meta;
using System;
using System.Linq;

namespace EDC.ReadiNow.BackgroundTasks
{
    /// <summary>
    /// DI module for BackgroundTasks.
    /// </summary>
    public class BackgroundTasksModule : Module
    {



        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BackgroundTaskManager>()
                .As<IBackgroundTaskManager>()
                .SingleInstance();

            builder.RegisterType<RedisTenantQueueFactory>()
                .WithParameter("queuePrefix", "BackgroundTaskManager")
                .As<ITenantQueueFactory>()
                .SingleInstance();

          

        }
    }
}
