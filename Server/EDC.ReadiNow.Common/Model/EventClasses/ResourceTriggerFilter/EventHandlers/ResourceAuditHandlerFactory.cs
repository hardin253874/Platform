// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter.EventHandlers
{
    /// <summary>
    /// Handler factory for dealing with resource change audits 
    /// </summary>
    public class RecordChangeAuditHandlerFactory : IFilteredTargetHandlerFactory
    {
        public IFilteredSaveEventHandler CreateSaveEventHandler()
        {
            return new RecordChangeAuditEventHandler();
        }

        public EntityType GetHandledType()
        {
            return RecordChangeAuditPolicy.RecordChangeAuditPolicy_Type;
        }
    }
}
