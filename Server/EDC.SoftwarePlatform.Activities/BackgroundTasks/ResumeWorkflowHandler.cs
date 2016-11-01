// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.BackgroundTasks.Handlers;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using ProtoBuf;
using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities.BackgroundTasks
{
    public class ResumeWorkflowHandler : TaskHandler<ResumeWorkflowParams>
    {
        const string HandlerKey = "ResumeWorkflow";

        public ResumeWorkflowHandler() : base(HandlerKey, false)
        {
        }

        public static BackgroundTask CreateBackgroundTask(WorkflowRun run, IWorkflowQueuedEvent resumeEvent)
        {
            if (resumeEvent == null)
                throw new ArgumentNullException(nameof(resumeEvent));

            var runParams = new ResumeWorkflowParams { WorkflowRunId = run.Id,  /*EventClass = resumeEvent.GetType().FullName,*/ EventState = resumeEvent };
            return BackgroundTask.Create(HandlerKey, runParams);
        }

        protected override void HandleTask(ResumeWorkflowParams taskData)
        {
            using (new SecurityBypassContext())
            {
                if (taskData == null)
                    throw new ArgumentNullException(nameof(taskData));

                IWorkflowEvent resumeEvent = taskData.EventState;

                var run = Entity.Get<WorkflowRun>(taskData.WorkflowRunId);

                try
                {
                    using (new WorkflowRunContext { TriggerDepth = run.TriggerDepth ?? 0, RunTriggersInCurrentThread = true })
                    {
                        WorkflowRunner.Instance.ResumeWorkflow(run, resumeEvent);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError($"ResumeWorkflowHandler.HandleTask: Unexpected exception thrown resuming workflow. RunId: {run?.Id}, Exception: {ex}");
                    // exception swallowed
                }
            }
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class ResumeWorkflowParams: IWorkflowQueuedEvent
    {
        /// <summary>
        /// The workflow run Id
        /// </summary>
        public long WorkflowRunId;

        /// <summary>
        /// the serialised state of the event
        /// </summary>
        public IWorkflowQueuedEvent EventState;

    }

}
