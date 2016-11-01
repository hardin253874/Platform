// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    public interface IRunStateFactory
    {
        IRunState CreateRunState(WorkflowMetadata metaData, WorkflowRun workflowRun);
    }
}
