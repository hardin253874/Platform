// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter;
using EDC.ReadiNow.Model.Interfaces;
using EDC.SoftwarePlatform.Activities.Engine;
using EDC.SoftwarePlatform.Activities.Triggers;
using System;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// DI module for actions.
    /// </summary>
    public class ActivitiesModule : Module
    {
        private static Lazy<WorkflowConfiguration> workflowConfig = new Lazy<WorkflowConfiguration>(() => ConfigurationSettings.GetWorkflowConfigurationSection());

        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var settings = workflowConfig.Value.BackgroundTasks;

            builder.Register(cc => new DefaultRunStateFactory())
                .As<IRunStateFactory>()
                .SingleInstance();

            builder.RegisterType<WorkflowRunner>()
                .WithParameter("suspendTimeoutMs", (long) (settings.SuspendTimeoutSeconds) * 1000L)
                .As<IWorkflowRunner>()
                .SingleInstance();

            builder.Register(cc => new TimeoutActivityHelper())
                .As<ITimeoutActivityHelper>()
                .SingleInstance();

            builder.Register(cc =>new CachingWorkflowMetadataFactory( new WorkflowMetadataFactory() ))
                .As<IWorkflowMetadataFactory>()
                .As<ICacheService>()
                .SingleInstance();

            builder.Register( cc=> new WorkflowTriggerHandlerFactory())
                .As<IFilteredTargetHandlerFactory>()
                .SingleInstance();

        }
    }
}
