// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Services.Console.WorkflowActions;
using EDC.ReadiNow.Cache;
using ReadiNow.ExportData;

namespace EDC.SoftwarePlatform.Services.ExportData
{
    /// <summary>
    /// DI module for actions.
    /// </summary>
    public class ExportDataModule : Module
    {
        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExportDataInterface>()
                .As<IExportDataInterface>()
                .SingleInstance();
        }
    }
}
