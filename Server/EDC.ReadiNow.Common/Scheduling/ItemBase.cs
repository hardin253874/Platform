// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Monitoring;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Monitoring;
using EDC.ReadiNow.Monitoring.Workflow;
using Quartz;
using System;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Scheduling
{
    /// <summary>
    ///     Optional base class for IScheduledItems. This class will extract the ScheduledItemRef from the context. It will also catch and re-throw any exceptions as JobExecutionExceptions.
    /// </summary>
    [DisallowConcurrentExecution]
    public abstract class ItemBase : TenantJob, IScheduledItemJob
    {
        private static ISingleInstancePerformanceCounterCategory perfCounters = new SingleInstancePerformanceCounterCategory(WorkflowPerformanceCounters.CategoryName);

        protected override void Execute(IJobExecutionContext jobContext)
        {

            var stopWatch = new Stopwatch();
            perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(WorkflowPerformanceCounters.ScheduleFireRateCounterName).Increment();
            stopWatch.Start();

            var jobRef = ScheduledItemHelper.GetScheduledItemRef(jobContext);
            var jobName = Entity.GetName(jobRef.Id) ?? "[Unnamed]";

            EDC.ReadiNow.Diagnostics.EventLog.Application.WriteTrace("Starting job '{0}'({1})", jobName, jobRef.Id);

            try
            {
                using (DeferredChannelMessageContext deferredMsgContext = new DeferredChannelMessageContext())
                {
                    Execute(jobRef);
                }                    
            }
            catch (JobExecutionException ex)
            {
                EDC.ReadiNow.Diagnostics.EventLog.Application.WriteTrace("Job execution exception. Ex: {0}", ex.ToString());
                throw;  // The job has already handled the problem and taken action.
            }
            catch (Exception ex)
            {
                EDC.ReadiNow.Diagnostics.EventLog.Application.WriteError("Exception thrown to scheduler. This should never occur and should be handled by the scheduled item. Ex: {0}", ex.ToString());
            }

            stopWatch.Stop();
            perfCounters.GetPerformanceCounter<AverageTimer32PerformanceCounter>(WorkflowPerformanceCounters.ScheduleJobDurationCounterName).AddTiming(stopWatch);
        }

        public abstract void Execute(EntityRef scheduledItemRef);
    }
}
