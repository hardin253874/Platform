// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter;
using EDC.ReadiNow.Model.Interfaces;
using EDC.SoftwarePlatform.Activities.Engine;
using EDC.SoftwarePlatform.Activities.Triggers;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// DI module for actions.
    /// </summary>
    public class ActivitiesModule : Module
    {
        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(cc => new DefaultRunStateFactory())
                .As<IRunStateFactory>()
                .SingleInstance();

            builder.RegisterType<WorkflowRunner>()
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
