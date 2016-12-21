// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Notify;


namespace EDC.SoftwarePlatform.Activities.Notify
{
    /// <summary>
    /// Autofac dependency injection module for Notifier
    /// </summary>
    public class NotifierModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Asynchronous runner.
            builder.RegisterType<TenantSmsNotifier>()
                .As<INotifier>();
        }
    }
}
