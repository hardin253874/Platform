// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Common.Workflow
{
    public class WorkflowRunDeferred : WorkflowRun
    {
        public long TriggeringUserId { get; private set; }
        public List<Action> DeferredActions = new List<Action>();

        public WorkflowRunDeferred(Model.Workflow wf, WfTrigger trigger = null)
        {
            WorkflowBeingRun = wf;

            if (trigger != null)
                TriggerForRun = trigger;

            TaskId = Guid.NewGuid().ToString("N");
            
            CreatedDate = DateTime.UtcNow;
            WorkflowRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunQueued;
            RunStepCounter = 0;

            var ctx = RequestContext.GetContext();
            if (ctx != null)
            {
                TriggerDepth = WorkflowRunContext.Current.TriggerDepth;
                TriggeringUserId = ctx.Identity.Id;
                TriggeringUser = GetTriggeringUser();
            }
        }

        public void Sync()
        {
            using (Profiler.Measure("WorkflowRunDeferred.Sync"))
            {
                Name = WorkflowBeingRun.Name;
                Description = string.Empty;

                GetTriggeringUser();
            }
        }

        public UserAccount GetTriggeringUser()
        {
            if (TriggeringUser != null)
                return TriggeringUser;

            using (Profiler.Measure("WorkflowRunDeferred.GetTriggeringUser"))
            {
                using (new SecurityBypassContext())
                {
                    var trigger = TriggerForRun;

                    if (trigger != null && trigger.Is<WfTriggerOnSchedule>())
                    {
                        TriggeringUser = trigger.SecurityOwner;
                    }
                    else
                    {
                        if (TriggeringUserId == 0)
                        {
                            throw new ArgumentException("Attempted to run a workflow without an appropriate security context. Should the workflow be set to 'Run as owner'?");
                        }

                        TriggeringUser = Model.Entity.Get<UserAccount>(TriggeringUserId);
                    }
                }

                return TriggeringUser;
            }
        }
     
    }
}
