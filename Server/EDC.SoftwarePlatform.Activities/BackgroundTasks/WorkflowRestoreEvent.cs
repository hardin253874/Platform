// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Model.Interfaces;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.BackgroundTasks
{
    /// <summary>
    /// Event used to indicate a workflow is being restored from a suspended state.
    /// </summary>
    [ProtoContract]
    public class WorkflowRestoreEvent: IWorkflowQueuedEvent
    {
    }
}
