// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter
{
    /// <summary>
    /// A factory that identifies a handler that is registered for a IFilteredSaveEventHandler.
    /// The factory must be registered via Autofac against the interface.
    /// </summary>
    public interface IFilteredTargetHandlerFactory
    {
        // Get all the filter definitions that currently apply (eg all the workflow triggers)
        IFilteredSaveEventHandler CreateSaveEventHandler();
        EntityType GetHandledType();

    }
}
