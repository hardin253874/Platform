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

namespace EDC.SoftwarePlatform.Activities.BackgroundTasks
{
    public class RunTriggersHandler : TaskHandler<RunTriggersParams>
    {
        const string HandlerKey = "RunTriggers";

        public RunTriggersHandler() : base(HandlerKey, false)
        {
        }

        public static BackgroundTask CreateBackgroundTask(long triggerId, long entityId)
        {
            var entry = new TriggerEntry { TriggerId = triggerId, EntityId = entityId };
            var runParams = new RunTriggersParams { TriggerDepth = WorkflowRunContext.Current.TriggerDepth + 1, TriggerList = new List<TriggerEntry> { entry } };
            return BackgroundTask.Create(HandlerKey, runParams);
        }

        protected override void HandleTask(RunTriggersParams taskData)
        {
            using (new SecurityBypassContext())
            using (new WorkflowRunContext { TriggerDepth = taskData.TriggerDepth, RunTriggersInCurrentThread = true })
            {
                foreach (var triggerEntry in taskData.TriggerList)
                {
                    try
                    {
                        var trigger = Entity.Get<WfTriggerUserUpdatesResource>(triggerEntry.TriggerId);

                        if (trigger == null)
                            EventLog.Application.WriteWarning($"RunTriggersHandler.HandleTask: Trigger missing, it may have been deleted. TriggerId: {triggerEntry.TriggerId}");
                        else
                        {
                            var entity = Entity.Get(triggerEntry.EntityId);

                            if (entity == null)
                            {
                                EventLog.Application.WriteWarning($"RunTriggersHandler.HandleTask: Entity missing, it may have been deleted. TriggerId: {triggerEntry.EntityId}");
                            }
                            else
                            {
                                WorkflowTriggerHelper.ActionTrigger(trigger, entity);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.Application.WriteError($"RunTriggersHandler.HandleTask: Unexpected exception thrown running trigger. TriggerId: {triggerEntry.TriggerId}, EntityId: {triggerEntry.EntityId}, Exception: {ex}");
                        // exception swallowed
                    }
                }
            }
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class RunTriggersParams: IWorkflowQueuedEvent
    {
        public int TriggerDepth;

        /// <summary>
        /// The set of triggers to run
        /// </summary>
        public List<TriggerEntry> TriggerList;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class TriggerEntry
    {
        public long TriggerId;

        public long EntityId;
    }
}
