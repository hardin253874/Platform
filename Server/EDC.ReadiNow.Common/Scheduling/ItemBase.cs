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
using EDC.ReadiNow.Security;
using EDC.ReadiNow.IO;

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
                using (DeferredChannelMessageContext deferredMsgContext = new DeferredChannelMessageContext())  // needed to keep redis happy
                {
                    // Set the context to the owner of the scheduled item.
                    var scheduledItem = Entity.Get<ScheduledItem>(jobRef);

                    var owner = scheduledItem.SecurityOwner;

                    if (owner == null)
                    {
                        var message = $"Unable to start scheduled job as import configuration has no owner";
                        Diagnostics.EventLog.Application.WriteError($"StartImportJob.Execute: {message}. {scheduledItem.Id}");
                        throw GenerateJobException(message, scheduledItem);
                    }

                    var identityInfo = new IdentityInfo(owner.Id, owner.Name);
                    var contextData = new RequestContextData(RequestContext.GetContext());
                    contextData.Identity = identityInfo;

                    using (CustomContext.SetContext(contextData))
                    {

                        Execute(jobRef);
                    }
                }                    
            }
            catch (JobExecutionException ex)
            {
                EDC.ReadiNow.Diagnostics.EventLog.Application.WriteTrace("Job execution exception. Ex: {0}", ex.ToString());
                throw;  // The job has already handled the problem and taken action.
            }
            catch (PlatformSecurityException ex)
            {
                Diagnostics.EventLog.Application.WriteError($"Platform security exception thrown to scheduler. This should never occur. Ex: {ex}");
            }
            catch (Exception ex)
            {
                Diagnostics.EventLog.Application.WriteError("Exception thrown to scheduler. This should never occur and should be handled by the scheduled item. Ex: {0}", ex.ToString());
            }

            stopWatch.Stop();
            perfCounters.GetPerformanceCounter<AverageTimer32PerformanceCounter>(WorkflowPerformanceCounters.ScheduleJobDurationCounterName).AddTiming(stopWatch);
        }

        protected JobExecutionException GenerateJobException(string message, IEntity entity)
        {
            string name = entity.GetField<string>(Resource.Name_Field) ?? "[Unnamed]";
            return new JobExecutionException($"'{name}'({entity.Id}) failed. {message}");
        }


        public abstract void Execute(EntityRef scheduledItemRef);
    }
}
