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
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.BackgroundTasks
{
    public class RunTriggersHandler : TaskHandler<RunTriggersParams>
    {
        const string HandlerKey = "RunTriggers";

        public RunTriggersHandler() : base(HandlerKey, false)
        {
        }

        protected override EntityType SuspendedTaskType
        {
            get
            {
                return SuspendedTrigger.SuspendedTrigger_Type;
            }
        }

        public static BackgroundTask CreateBackgroundTask(long triggerId, long entityId)
        {
            return CreateBackgroundTask(triggerId, entityId, WorkflowRunContext.Current.TriggerDepth + 1);
        }

        public static BackgroundTask CreateBackgroundTask(long triggerId, long entityId, int triggerDepth)
        {
            var runParams = new RunTriggersParams
            {
                TriggerDepth = triggerDepth,
                TriggerId = triggerId,
                EntityId = entityId
            };
            
            return BackgroundTask.Create(HandlerKey, runParams);
        }


        protected override void HandleTask(RunTriggersParams taskData)
        {
            using (new SecurityBypassContext())
            using (new WorkflowRunContext { TriggerDepth = taskData.TriggerDepth, RunTriggersInCurrentThread = true })
            {
                try
                {
                    var trigger = Entity.Get<WfTriggerUserUpdatesResource>(taskData.TriggerId);

                    if (trigger == null)
                        EventLog.Application.WriteWarning($"RunTriggersHandler.HandleTask: Trigger missing, it may have been deleted. TriggerId: {taskData.TriggerId}");
                    else
                    {
                        var entity = Entity.Get(taskData.EntityId);

                        if (entity == null)
                        {
                            EventLog.Application.WriteWarning($"RunTriggersHandler.HandleTask: Entity missing, it may have been deleted. TriggerId: {taskData.EntityId}");
                        }
                        else
                        {
                            WorkflowTriggerHelper.ActionTrigger(trigger, entity);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError($"RunTriggersHandler.HandleTask: Unexpected exception thrown running trigger. TriggerId: {taskData.TriggerId}, EntityId: {taskData.EntityId}, Exception: {ex}");
                    // exception swallowed
                }
            }
        }

        #region Suspend/Restore


        protected override void AnnotateSuspendedTask(IEntity suspendedTask, RunTriggersParams taskParam)
        {
            var trigger = Entity.Get<WfTrigger>(new EntityRef(taskParam.TriggerId));
            var resource = Entity.Get<Resource>(new EntityRef(taskParam.EntityId));

            if (trigger == null)
                throw new SuspendFailedException($"Trigger no longer available. Trigger Id:{taskParam.TriggerId}");

            if (resource == null)
                throw new SuspendFailedException($"Resource no longer available. Trigger Id:{taskParam.EntityId}");

            suspendedTask.GetRelationships(SuspendedTrigger.StTrigger_Field).Add(trigger);
            suspendedTask.GetRelationships(SuspendedTrigger.StResource_Field).Add(resource);
            suspendedTask.SetField(SuspendedTrigger.StTriggerDepth_Field, taskParam.TriggerDepth);
        }

        protected override RunTriggersParams RestoreTaskData(IEntity suspendedTask)
        {
            var resource = suspendedTask.GetRelationships(SuspendedTrigger.StResource_Field).FirstOrDefault();
            var trigger = suspendedTask.GetRelationships(SuspendedTrigger.StTrigger_Field).FirstOrDefault();
            var triggerDepth = suspendedTask.GetField<int?>(SuspendedTrigger.StTriggerDepth_Field) ?? 0;

            if (trigger == null)
                throw new SuspendFailedException($"Trigger no longer available.");

            if (resource == null)
                throw new SuspendFailedException($"Resource no longer available.");

            return new RunTriggersParams { EntityId = resource.Id, TriggerId = trigger.Id, TriggerDepth = triggerDepth };
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class RunTriggersParams: IWorkflowQueuedEvent
    {
        public int TriggerDepth;
        public long TriggerId;
        public long EntityId;
    }

}
