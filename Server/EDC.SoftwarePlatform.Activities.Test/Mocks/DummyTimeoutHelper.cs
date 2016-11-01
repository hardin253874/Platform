// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;

namespace EDC.SoftwarePlatform.Activities.Test.Mocks
{
    public class DummyTimeoutHelper : ITimeoutActivityHelper
    {
        Action timeoutAction;

        public void CancelTimeout(WorkflowRun workflowRun)
        {
           timeoutAction = null;
        }

        public void ScheduleTimeout(IResumableActivity activityInstance, WorkflowRun workflowRun, decimal timeoutMinutes)
        {
            timeoutAction = () =>
            {
                var workflowEvent = new TimeoutEvent();
                WorkflowRunner.Instance.ResumeWorkflow(workflowRun, workflowEvent);
            };
        }

        public void Timeout()
        {
            timeoutAction();
        }

    }
}
