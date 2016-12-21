// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities.Engine.Events
{
    public class ChildWorkflowStartEvent : IWorkflowEvent
    {
        public Workflow WorkflowToStart { get; private set; }
        public WorkflowRun ParentRun { get; private set; }
        public Dictionary<string, object> Inputs { get; private set; }
        public Action RunAction { get; private set; }

        public ChildWorkflowStartEvent(Workflow wfToProxy, Dictionary<string, object> inputs, WorkflowRun parentRun)
        {
            WorkflowToStart = wfToProxy;
            Inputs = inputs;
            ParentRun = parentRun;
        }

        
    }
}
