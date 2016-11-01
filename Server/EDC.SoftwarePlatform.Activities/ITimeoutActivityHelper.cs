// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    public interface ITimeoutActivityHelper
    {
        void CancelTimeout(WorkflowRun workflowRun);
        void ScheduleTimeout(IResumableActivity activityInstance, WorkflowRun workflowRun, decimal timeoutMinutes);
    }
}
