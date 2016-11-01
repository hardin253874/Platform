// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Activities.Engine;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using Autofac;
using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using EDC.ReadiNow.BackgroundTasks;

namespace EDC.SoftwarePlatform.Activities.Triggers
{
    /// <summary>
    /// Handler for workflow triggers
    /// </summary>
    public class WorkflowTriggerHandler : IFilteredSaveEventHandler
    {

        bool IFilteredSaveEventHandler.OnBeforeSave(ResourceTriggerFilterDef policy, IEntity changedEntity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
        {
            return false;
        }

        void IFilteredSaveEventHandler.OnAfterSave(ResourceTriggerFilterDef policy, IEntity entity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
        {
            TestAndTrigger(policy, entity, isNew);
        }

        

        bool IFilteredSaveEventHandler.OnBeforeReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity, bool isNew)
        {
            return false;
        }

        bool IFilteredSaveEventHandler.OnBeforeReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity)
        {
            return false;

        }


        void IFilteredSaveEventHandler.OnAfterReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity, bool isNew)
        {
            TestAndTrigger(policy, policyEntity, isNew);
        }

        void IFilteredSaveEventHandler.OnAfterReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity)
        {
            TestAndTrigger(policy, policyEntity, false);
        }


        bool IFilteredSaveEventHandler.OnBeforeDelete(ResourceTriggerFilterDef policy, IEntity entity)
        {
            return false;
        }

        void IFilteredSaveEventHandler.OnAfterDelete(ResourceTriggerFilterDef policy, IEntity entity)
        {
            // Do nothing
        }

        /// <summary>
        /// Confirm that the workflow should trigger, if so trigger it
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="entity"></param>
        /// <param name="isNew"></param>
        private void TestAndTrigger(ResourceTriggerFilterDef policy, IEntity entity, bool isNew)
        {
            using (new SecurityBypassContext())
            {
                if (WorkflowRunContext.Current.DisableTriggers)
                    return;

                var updateTrigger = policy.Cast<WfTriggerUserUpdatesResource>();

                var triggeredOnCreate = updateTrigger.TriggeringCondition_Enum == TriggeredOnEnum_Enumeration.TriggeredOnEnumCreate;
                var triggeredOnUpdate = updateTrigger.TriggeringCondition_Enum == TriggeredOnEnum_Enumeration.TriggeredOnEnumUpdate;
                var triggeredOnEither = updateTrigger.TriggeringCondition_Enum == TriggeredOnEnum_Enumeration.TriggeredOnEnumCreateUpdate;


                if (triggeredOnEither || (isNew && triggeredOnCreate) || (!isNew && triggeredOnUpdate))
                {
                    if (Factory.FeatureSwitch.Get("longRunningWorkflow"))
                    {
                        Trigger_New(updateTrigger, entity, policy.RtfdRunInForeground == true);
                    }
                    else
                    {
                        Trigger_Old(updateTrigger, entity, policy.RtfdRunInForeground == true);
                    }
                }
            }
        }

        private void Trigger_New(WfTriggerUserUpdatesResource updateTrigger, IEntity entity, bool runInForeground)
        {
            var bgTask = RunTriggersHandler.CreateBackgroundTask(updateTrigger.Id, entity.Id);


            if (WorkflowRunContext.Current.RunTriggersInCurrentThread || runInForeground)
            {
                WorkflowRunContext.Current.DeferAction(() => Factory.BackgroundTaskManager.ExecuteImmediately(bgTask));
            }
            else
            {
                using (var ctx = DatabaseContext.GetContext())
                {
                    ctx.AddPostDisposeAction(() => Factory.BackgroundTaskManager.EnqueueTask(bgTask));
                }
            }
        }

        private void Trigger_Old(WfTriggerUserUpdatesResource updateTrigger, IEntity entity, bool runInForeground)
        { 
            Action action = () =>
            {
                try
                {
                    using (new SecurityBypassContext())
                    {
                        updateTrigger.ActionTrigger(entity);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError("Workflow trigger '{0}' on entity '{1}' failed: {2}",
                        new EntityRef(updateTrigger), new EntityRef(entity), ex);
                }
            };

            action = WrapActionForTriggerDepth(action);

            if (WorkflowRunContext.Current.RunTriggersInCurrentThread || runInForeground)
            {
                WorkflowRunContext.Current.DeferAction(() => action());
            }
            else
            {
                var wrappedAction = WrapActionForThread(action);

                using (var ctx = DatabaseContext.GetContext())
                {
                    ctx.AddPostDisposeAction(() =>                // make sure the second thread does not start till the current transaction has completed.
                    WorkflowRunContext.Current.QueueAction(wrappedAction));
                }
            }
        }


        /// <summary>
        /// Wrap an action so it can be run in a separate thread.
        /// </summary>
        protected Action WrapActionForThread(Action act)
        {
            var originalRunContext = WorkflowRunContext.Current;

            return () =>
            {
                using (EntryPointContext.AppendEntryPoint("WorkflowTrigger"))
                using (new WorkflowRunContext { TriggerDepth = originalRunContext.TriggerDepth, RunTriggersInCurrentThread = true }) // Note that the context is coppied across from the original thread in the WorkflowRunContext DefaultQueueTask
                {

                    EventLog.Application.WriteTrace("Starting workflow thread at depth {0}", originalRunContext.TriggerDepth);

                    act();
                }
            };
        }

        /// <summary>
        /// Wrap an action To keep track of the trigger depth
        /// </summary>
        Action WrapActionForTriggerDepth(Action act)
        {
            var triggerDepth = WorkflowRunContext.Current.TriggerDepth;

            return () =>
            {
                using (new WorkflowRunContext { TriggerDepth = triggerDepth + 1 }) // Note that the context is coppied across from the original thread in the WorkflowRunContext DefaultQueueTask
                {
                    act();
                }
            };
        }

    }


    /// <summary>
    /// Factory for generating trigger handlers for workflows.
    /// </summary>
    public class WorkflowTriggerHandlerFactory : IFilteredTargetHandlerFactory 
    {

        public IFilteredSaveEventHandler CreateSaveEventHandler()
        {
            return new WorkflowTriggerHandler();
        }

        public EntityType GetHandledType()
        {
            return WfTriggerUserUpdatesResource.WfTriggerUserUpdatesResource_Type.Cast<EntityType>();
        }
    }


    
}
