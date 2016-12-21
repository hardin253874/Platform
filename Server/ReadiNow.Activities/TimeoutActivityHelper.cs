// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using Quartz;
using EDC.ReadiNow.Core;
using Autofac;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Helper class for setting up timeout conditions.
    /// </summary>
    public class TimeoutActivityHelper : ITimeoutActivityHelper
    {
        public const string TimeoutSchedulingGroup = "Activity Timeout";
        public const string AdditionalContextKey = "AdditionalContext";
            
        public class TimeoutActivityJob : TenantJob
        {
            public const string RunIdKey = "WorkflowRun";

            protected override void Execute(IJobExecutionContext context)
            {
                try
                {
                    var runId =  (string)context.MergedJobDataMap[RunIdKey];


                    var workflowRun = Entity.Get<WorkflowRun>(long.Parse(runId));
                    
                    if (workflowRun != null && workflowRun.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunPaused)
                    {
                        var workflowEvent = new TimeoutEvent();
                        WorkflowRunner.Instance.ResumeWorkflow(workflowRun, workflowEvent);
                    }
                    else
                    {
                        // Ignore sequence numbers that no longer exist. It's probably a stale timeout.
                        EventLog.Application.WriteTrace("Timeout event: Ignoring invalid or stale paused workflow run: {0}", runId);
                    }
                }
                catch (Exception ex)
                {
                    throw new JobExecutionException("Failed to execute timeout.", ex);
                }
            }
        }
        
        public static ITimeoutActivityHelper Instance
        {
            get
            {
                return Factory.Current.Resolve<ITimeoutActivityHelper>();
            }
        }


        public void ScheduleTimeout(IResumableActivity activityInstance, WorkflowRun workflowRun, decimal timeoutMinutes)
        {
            var pointInTime = DateTimeOffset.UtcNow.AddMinutes((double)timeoutMinutes);

            string triggerId = string.Format("Timeout trigger {0}", workflowRun.Id);

               // Define the Job to be scheduled
            var builder = JobBuilder.Create<TimeoutActivityJob>()
                                    .WithIdentity(GetJobId(workflowRun))
                                    .UsingJobData(TimeoutActivityJob.RunIdKey, workflowRun.Id.ToString())
                                    .UsingCurrentTenant()
                                    .RequestRecovery();
           
            var job = builder.Build();

            // Associate a trigger with the Job
            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerId, TimeoutSchedulingGroup)
                .StartAt(pointInTime)
                .Build();

            SchedulingHelper.Instance.ScheduleJob(job, trigger);
        }

        JobKey GetJobId(WorkflowRun workflowRun)
        {
            return new JobKey(workflowRun.Id.ToString(), TimeoutSchedulingGroup);
        }

        public void CancelTimeout(WorkflowRun workflowRun)
        {
            try
            {
                SchedulingHelper.Instance.DeleteJob(GetJobId(workflowRun));
            }
            catch(Exception ex)
            {
                EventLog.Application.WriteError($"Unable to cancel timeout: Job may not have been created. RunId: {workflowRun.Id}  Exception: {ex}");
            }
        }

 }
}
