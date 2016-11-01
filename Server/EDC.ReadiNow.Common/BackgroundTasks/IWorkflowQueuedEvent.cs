// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.BackgroundTasks
{
    /// <summary>
    /// An event that can be added to the background task queue. This marker is used to identify all the events that need to be able to be serialized
    /// </summary>
    public interface IWorkflowQueuedEvent:  IWorkflowEvent
    {
    }

}
