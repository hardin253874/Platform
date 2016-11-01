// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter.EventHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter
{
    /// <summary>
    /// DI module for ResourceTriggerFilters.
    /// </summary>
    public class ResourceTriggerFilterModule : Module
    {
        /// <summary>
        /// Loads the registrations.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(cc => new ResourceTriggerFilterPolicyCache(cc.Resolve<IEnumerable<IFilteredTargetHandlerFactory>>()))
                .As<IResourceTriggerFilterPolicyCache>()
                .SingleInstance();

            builder.Register(cc => new RecordChangeAuditHandlerFactory())
                .As<IFilteredTargetHandlerFactory>()
                .SingleInstance();


        }
    }
}
