// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.BackgroundTasks;


namespace EDC.SoftwarePlatform.Activities.BackgroundTasks
{
    /// <summary>
    /// Autofac dependency injection module for background tasks
    /// </summary>
    public class BackgroundTasksModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RunTriggersHandler>()
                .As<ITaskHandler>()
                .SingleInstance();

            builder.RegisterType<ResumeWorkflowHandler>()
                .As<ITaskHandler>()
                .SingleInstance();
        }
    }
}
