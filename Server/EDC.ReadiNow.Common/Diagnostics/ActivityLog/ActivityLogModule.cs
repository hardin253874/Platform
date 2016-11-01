// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace EDC.ReadiNow.Diagnostics.ActivityLog
{
    /// <summary>
    /// Module for activity log classes.
    /// </summary>
    public class ActivityLogModule: Module
    {
        /// <summary>
        /// Register any published types.
        /// </summary>
        /// <param name="builder">
        /// The builder to use.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new ActivityLogWriter(new RateLimitedPurger(new ActivityLogPurger())))
                   .As<IActivityLogWriter>()
                   .As<ActivityLogWriter>()
                   .SingleInstance();
        }
    }
}
