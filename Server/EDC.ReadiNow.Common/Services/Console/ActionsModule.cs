// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Services.Console.WorkflowActions;
using EDC.ReadiNow.Cache;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// DI module for actions.
    /// </summary>
    public class ActionsModule : Module
    {
        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(cc => new CachingWorkflowActionsFactory(new WorkflowActionsFactory()))
                .As<IWorkflowActionsFactory>()
                .SingleInstance();

			builder.RegisterType<ActionCache>( )
				.As<ActionCache>( )
				.As<ICacheService>( )
				.SingleInstance( );

		}
    }
}
