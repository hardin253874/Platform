// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model.Interfaces;
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.Test.Mocks
{
    public class DummyWorkflowRunner : IWorkflowRunner
    {
        public WorkflowRun ResumeWorkflow(WorkflowRun run, IWorkflowEvent resumeEvent)
        {
            throw new NotImplementedException();
        }

        public string ResumeWorkflowAsync(WorkflowRun workflowRun, IWorkflowEvent resumeEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowRun RunWorkflow(WorkflowStartEvent startEvent)
        {
            throw new NotImplementedException();
        }


        public Func<WorkflowStartEvent, string> StartWorkflowAsyncFn;

        public string RunWorkflowAsync(WorkflowStartEvent startEvent)
        {
            return StartWorkflowAsyncFn(startEvent);
        }
    }
}
