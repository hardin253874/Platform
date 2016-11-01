// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using Quartz;
using Quartz.Impl;
using EDC.ReadiNow.Diagnostics;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System.Collections.Specialized;

namespace EDC.SoftwarePlatform.Activities.Scheduling
{
    public static class SchedulingSyncHelper
    {
      /// <summary>
        /// A job ID has the id of the triggerOnSchedule and a groupname corresponding to the tenantId
        /// </summary>
        public static Quartz.JobKey GetJobId(IEntity scheduledItem)
        {
            if (!((IEntity)scheduledItem).HasId)
                throw new ApplicationException("Attempted to schedule or unschedule a triggerOnSchedule which has not been saved. This should never occur.");

            return GetJobKey(scheduledItem.Id);
        }


        public static JobKey GetJobKey(long entityId)
        {
            return new JobKey(string.Format("{0}:{1}", RequestContext.TenantId, entityId), GetGroupId());
        }

        /// <summary>
        /// Get the ID string used to identify all the jobs and triggers for the Tenant
        /// </summary>
        /// <returns></returns>
        public static string GetGroupId()
        {
            return RequestContext.TenantId.ToString();
        }

        /// <summary>
        /// A job ID has the id of the triggerOnSchedule and a groupname corresponding to the tenantId
        /// </summary>
        public static Quartz.TriggerKey GetTriggerKey(IEntity job, IEntity triggerOnSchedule)
        {
            return new TriggerKey(string.Format("{0}:{1}:{2}", RequestContext.TenantId, job.Id, triggerOnSchedule.Id), GetGroupId());
        }

        /// <summary>
        /// Delete the corresponding  trigger on schedule, recreating the triggers.
        /// </summary>
        /// <param name="triggerOnSchedule"></param>
        public static void DeleteScheduledJob(long triggerOnScheduleId, IScheduler sch)
        {
            var jobKey = GetJobKey(triggerOnScheduleId);
            var triggerKeys = sch.GetTriggersOfJob(jobKey);
            sch.UnscheduleJobs(triggerKeys.Select(t => t.Key).ToList());

            try
            {
                sch.DeleteJob(jobKey);
            }
            catch (JobPersistenceException)
            {
                // a known issue which has been fixed in a pre-release of a later version of Quartz.Net:
                // fixes #93 DailyTimeIntervalTriggerPersistenceDelegate does not handle…
                // https://github.com/quartznet/quartznet/commit/1fd39d2e4b73b4d7af0d16755fe59ebc3353ae2d                
            }
        }

        /// <summary>
        /// Update the corresponding scheduling job for a a trigger on schedule, recreating the triggers.
        /// </summary>
        /// <param name="scheduledItem"></param>
        public static void UpdateScheduledJob(ScheduledItem scheduledItem, IScheduler scheduler)
        {
            var deleteJob = false;

            if (scheduledItem.TriggerEnabled ?? true )
            {
                var action = scheduledItem.IsOfType.First(t=>t.Is<ScheduleAction>());
                var jobClass = action.As<ScheduleAction>().OnScheduleFire.GetClass();

                var job =
                    JobBuilder.Create(jobClass)
                              .WithIdentity(GetJobId(scheduledItem))
                              .WithDescription(scheduledItem.Name)
                              .RequestRecovery(true)
                              .UsingCurrentTenant()
                              .UsingScheduledItemJobData(scheduledItem)
                              .Build();

                var oneOffJobs =   CreateOneOffJobs(scheduledItem);
                var dailyRepeats = CreateDailyRepeats(scheduledItem);
                var cronJobs =     CreateCron(scheduledItem);

                var allTriggers =
                    new Quartz.Collection.HashSet<ITrigger>(oneOffJobs.Union(dailyRepeats).Union(cronJobs));

                if (allTriggers.Count > 0)
                {
                    scheduler.ScheduleJob(job, allTriggers, true); // replace any existing jobs.
                }
                else
                {
                    deleteJob = true;
                }
            }
            else
            {
                deleteJob = true;
            }

            if (deleteJob)
                DeleteScheduledJob(scheduledItem.Id, SchedulingHelper.Instance);
        }


        private static IEnumerable<ITrigger> CreateOneOffJobs(ScheduledItem scheduledItem)
        {
            var oneOffs = scheduledItem.ScheduleForTrigger.Select(sch => sch.As<ScheduleOneOff>())
                                         .Where(sch => sch != null);

            var result = new List<ITrigger>();

            foreach (var oneOff in oneOffs)
            {
                if (oneOff.ScheduleSpecificTime != null)
                {
                    // The date time is stored in the DB as UTC so we must ensure that the offset is created accordingly
                    var startAt = new DateTimeOffset((DateTime) oneOff.ScheduleSpecificTime, TimeSpan.Zero);
                    var qtrigger = TriggerBuilder.Create()
                                                     .WithIdentity(GetTriggerKey(scheduledItem, oneOff))
                                                     .StartAt(startAt)
                                                     .Build();
                    result.Add(qtrigger);
                }
            }

            return result;
        }


        private static IEnumerable<ITrigger> CreateDailyRepeats(ScheduledItem scheduledItem)
        {
             var dailyRepeats = scheduledItem.ScheduleForTrigger.Select(sch => sch.As<ScheduleDailyRepeat>())
                                         .Where(sch => sch != null);

            var result = new Quartz.Collection.HashSet<ITrigger>();

            foreach (var dailyRepeat in dailyRepeats)
            {
                if (dailyRepeat.SdrTimeOfDay != null)
                {
                    // TODO: This should not be converted to local time but to the TZ of the original creator. We currently do not store that infomation.
                    var sdrTimeOfDay = ((DateTime) dailyRepeat.SdrTimeOfDay);
                    var timeOfDay = new TimeOfDay(sdrTimeOfDay.Hour, sdrTimeOfDay.Minute, sdrTimeOfDay.Second);
                    var timeZone = TimeZoneHelper.GetTimeZoneInfo(dailyRepeat.SdrTimeZone);
                    var daysOfWeek = dailyRepeat.GetRelationships("core:sdrDayOfWeek").Entities.Select(dayEnumVal => (int) dayEnumVal.GetField("core:enumOrder"));

                    if (!daysOfWeek.Any())
                        daysOfWeek = Enumerable.Range(0, 7);

                    
                    var dow = daysOfWeek.Select( i => (DayOfWeek) i).ToArray();

                    var qtrigger = TriggerBuilder.Create()
                                                 .WithIdentity(GetTriggerKey(scheduledItem, dailyRepeat))
                                                 .WithDailyTimeIntervalSchedule(
                                                    x => x
                                                        .OnDaysOfTheWeek(dow)
                                                        .StartingDailyAt(timeOfDay)
                                                        .WithInterval(24, IntervalUnit.Hour).InTimeZone(timeZone))
                                                 .Build();

                    result.Add(qtrigger);
                }
            }

            return result;
        }

      
        private static IEnumerable<ITrigger> CreateCron(ScheduledItem scheduledItem)
        {
            var crons = scheduledItem.ScheduleForTrigger.Select(sch => sch.As<ScheduleCron>())
                                         .Where(sch => sch != null);
            var result = new List<ITrigger>();

            foreach (var cron in crons)
            {
                if (cron.CronDefinition != null)
                {
                    // The date time is stored in the DB as UTC so we must ensure that the offset is created accordingly
                   var qtrigger = TriggerBuilder.Create()
                                                     .WithIdentity(GetTriggerKey(scheduledItem, cron))
                                                     .WithCronSchedule(cron.CronDefinition)
                                                     .Build();
                    result.Add(qtrigger);
                }
            }

            return result;
        }

        public static void RecreateJobsForTenant(long tenantId, IScheduler scheduler)
        {
            SchedulingHelper.PauseAndDeleteJobsForTenant(tenantId, SchedulingHelper.Instance);

            var allScheduledItems = Entity.GetInstancesOfType<ScheduledItem>();

            EventLog.Application.WriteInformation(
                string.Format("Creating Jobs for {0} ScheduleItems", allScheduledItems.Count()));

            foreach (var scheduledItem in allScheduledItems)
            {
                UpdateScheduledJob(scheduledItem, scheduler);
            }
        }
       
    }
}
