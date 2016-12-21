// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using ProtoBuf;

namespace EDC.SoftwarePlatform.Activities.Engine.Events
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class PromptUserTaskCompletedEvent : IWorkflowEvent, IWorkflowQueuedEvent
    {
        public long UserTaskId { get; set; }
    }
}
