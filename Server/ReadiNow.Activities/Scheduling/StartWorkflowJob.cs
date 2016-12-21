// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using Quartz;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities.Scheduling
{
    /// <summary>
    /// A Scheduled job that runs a workflow
    /// </summary>
    public class StartWorkflowJob: ItemBase
    {
        protected override bool RunAsOwner
        {
            get
            {
                return true;            // Runs as the user
            }
        }


        public override void Execute(EntityRef scheduledItemRef)
        {
            
            var trigger = Entity.Get<WfTrigger>(scheduledItemRef);

            if (trigger == null)
            {
                throw new JobExecutionException(string.Format("Failed to start workflow, trigger does not exist. Trigger={0}", scheduledItemRef));
            }

            if (trigger.WorkflowToRun == null)
            {
                throw new JobExecutionException(string.Format("Failed to start workflow, no workflow defined. Trigger={0}({1})", trigger.Name ?? "[Unnamed]", trigger.Id));
            }

            try
            {
                using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
                {
                    WorkflowRunner.Instance.RunWorkflow(new WorkflowStartEvent(trigger));
                }
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("StartWorkflowJob.Execute: Unexpected exception thrown: {0}", ex);               // This should be unnecessary, but some exceptions are being lost.
                throw new JobExecutionException(string.Format("Unexpected exception when triggering scheduled workflow. Trigger={0}({1})", trigger.Name ?? "[Unnamed]", trigger.Id), ex, false);
            }
        }
    }
}
