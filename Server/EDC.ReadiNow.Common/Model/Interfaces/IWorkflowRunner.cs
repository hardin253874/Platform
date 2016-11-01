// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.Interfaces
{
    public interface IWorkflowRunner
    {
        WorkflowRun ResumeWorkflow(WorkflowRun run, IWorkflowEvent resumeEvent);
        string ResumeWorkflowAsync(WorkflowRun workflowRun, IWorkflowEvent resumeEvent);
        WorkflowRun RunWorkflow(WorkflowStartEvent startEvent);
        string RunWorkflowAsync(WorkflowStartEvent startEvent);
    }

}
