// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Monitoring;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Monitoring.Workflow;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities
{

    public static class WorkflowTriggerHelper
    {
        private static ISingleInstancePerformanceCounterCategory perfCounters = new SingleInstancePerformanceCounterCategory(WorkflowPerformanceCounters.CategoryName);

        // Get the config once. This eliminated the risk of getting a "configration changed by another process" error.
        private static Lazy<WorkflowConfiguration> workflowConfig = new Lazy<WorkflowConfiguration>(() => ConfigurationSettings.GetWorkflowConfigurationSection());

        /// <summary>
        /// The maximum trigger depth 
        /// </summary>
        public static int MaxTriggerDepth 
        { 
            get
            {
                return workflowConfig.Value.Triggers.MaxDepth;
            } 
        }

        /// <summary>
        /// The maximum amount of time a workflow will run 
        /// </summary>
        public static int MaxRunTimeSeconds
        {
            get
            {
                return workflowConfig.Value.Triggers.MaxRunTimeSeconds;
            }
        } 


        public static void ActionTrigger(this WfTriggerUserUpdatesResource trigger, IEntity entity)
        {
            perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(WorkflowPerformanceCounters.TriggerRateCounterName).Increment();

            RunWorkflow(trigger, entity.Id);
        }

        private static void RunWorkflow(WfTriggerUserUpdatesResource trigger, EntityRef updatedEntity)
        {
            var args = new Dictionary<string, object>();

            var actionArg = trigger.WorkflowToRun.InputArgumentForAction;

            if (actionArg != null)
                args.Add(trigger.WorkflowToRun.InputArgumentForAction.Name, updatedEntity.Entity);

            WorkflowRunner.Instance.RunWorkflow(new WorkflowStartEvent(trigger.Cast<WfTrigger>()) { Arguments = args });
        }
    }
}
