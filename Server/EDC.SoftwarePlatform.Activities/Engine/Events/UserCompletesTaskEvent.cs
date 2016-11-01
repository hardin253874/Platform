// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using ProtoBuf;
using EDC.ReadiNow.BackgroundTasks;

namespace EDC.SoftwarePlatform.Activities.Engine.Events
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class UserCompletesTaskEvent: IWorkflowEvent, IWorkflowUserTransitionEvent, IWorkflowQueuedEvent
    {
        //[ProtoIgnore]
        //public TransitionStart CompletionState { get { return CompletionStateId != 0 ? Entity.Get<TransitionStart>(CompletionStateId) : null; } }

        public long CompletionStateId { get; set; }

        public long UserTaskId { get; set; }
    }
}
