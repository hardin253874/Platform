// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.BackgroundTasks.Handlers;
using EDC.ReadiNow.Configuration;
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
        private static Lazy<WorkflowConfiguration> workflowConfig = new Lazy<WorkflowConfiguration>(() => ConfigurationSettings.GetWorkflowConfigurationSection());



        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var settings = workflowConfig.Value.BackgroundTasks;

            builder.RegisterType<BackgroundTaskManager>()
                .WithParameter("perTenantConcurrency", settings.PerTenantConcurrency)
                .As<IBackgroundTaskManager>()
                .SingleInstance()
                .AutoActivate();


            builder.RegisterType<BackgroundTaskController>()
                .As<IBackgroundTaskController>()
                .SingleInstance()
                .AutoActivate();


            builder.RegisterType<RedisTenantQueueFactory>()
                .WithParameter("queuePrefix", "BackgroundTaskManager")
                .As<ITenantQueueFactory>()
                .SingleInstance();

          

        }
    }
}
