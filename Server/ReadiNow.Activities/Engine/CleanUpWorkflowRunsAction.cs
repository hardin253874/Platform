// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging.Mail;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Messaging.Mail;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;

namespace EDC.SoftwarePlatform.Activities.Engine
{
    public class CleanUpWorkflowRunsAction : ItemBase
    {
        protected override bool RunAsOwner
        {
            get
            {
                return false;
            }
        }


        internal void RemoveOldWorkflowVersions()
        {
            var toDelete = new List<IEntity>();

            //todo: change to use resource query
            var workflows = Entity.GetInstancesOfType<Workflow>(false); // NOTE: Can't use a prefetch hint because verion 2.x will blow up if there are more thatn 500 instances. 

            var superceededWfs = workflows.Where(wf => wf.WfNewerVersion != null);
			toDelete.AddRange( superceededWfs.Where( wf => wf.RunningInstances.Count <= 0 ) );

			if ( toDelete.Count > 0 )
            {
                EventLog.Application.WriteInformation("Deleting {0} superceeded workflows with no running instances.", toDelete.Count);
                Entity.Delete(toDelete.Select(r => r.Id));
            }
        }

        bool ShouldDelete(WorkflowRun run, DateTime deleteOlderThan)
        {
            DateTime runCompletedAt = run.RunCompletedAt ?? DateTime.MinValue;
            return
                runCompletedAt < deleteOlderThan &&
                (
                    run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunFailed ||
                    run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted
                );
        }

        internal void RemoveOldWorkflowRuns(DateTime deleteOlderThan)
        {
            var toDelete = new List<IEntity>();



            var runs = Entity.GetInstancesOfType<WorkflowRun>(false); // NOTE: Can't use a prefetch hint because verion 2.x will blow up if there are more thatn 500 instances. 
            toDelete.AddRange(runs.Where(r => ShouldDelete(r, deleteOlderThan)));

			if ( toDelete.Count > 0 )
            {
                EventLog.Application.WriteInformation("Deleting {0} old workflow runs", toDelete.Count);
                Entity.Delete(toDelete.Select(r => r.Id));
            }

        }

        internal void RemoveUnreferencedWorkflowTraces()
        {
            var toDelete = BulkRequestRunner.FetchFilteredEntities("runTraceLogEntry", "[Workflow run] is null");

            if (toDelete.Count() > 0)
            {
                EventLog.Application.WriteInformation("Deleting {0} old workflow runs trace log entries", toDelete.Count());
                Entity.Delete(toDelete.Select(r => r.Id));
            }

        }

        /// <summary>
        /// Clean up all the stale workflow runs in the system.
        /// </summary>
        /// <param name="scheduledItemRef"></param>
        public override void Execute(EntityRef scheduledItemRef)
        {
            var daysToKeepRuns = ConfigurationSettings.GetWorkflowConfigurationSection().Triggers.KeepCompletedRunDays;
            DateTime deleteAfter = DateTime.UtcNow.AddDays( - daysToKeepRuns);

            var tenant = RequestContext.GetContext().Tenant;
            var tenantId = tenant != null ? tenant.Id : 0;
            var tenantName = tenant != null ? tenant.Name : "Zero Tenant";

            EventLog.Application.WriteInformation("Cleaning workflow runs for tenant '{0}' ({1})", tenantName, tenantId);

            RemoveOldWorkflowRuns(deleteAfter);
            RemoveOldWorkflowVersions();
            RemoveUnreferencedWorkflowTraces();
        }
    }
}