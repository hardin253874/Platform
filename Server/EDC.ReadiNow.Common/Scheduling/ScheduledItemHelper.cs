// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using Quartz;

namespace EDC.ReadiNow.Scheduling
{
    /// <summary>
    ///     Helper for ScheduledItems interacting with the scheduling system.
    /// </summary>
    public static class ScheduledItemHelper
    {
        public const string ScheduledItemContextKey = "ScheduledItemId";


        private static long GetLongFromJobContext(IJobExecutionContext context, string key)
        {
            return long.Parse((string)context.MergedJobDataMap[key]);
        }

        
        public static EntityRef GetScheduledItemRef(IJobExecutionContext context)
        {
            var triggerOnScheduleId = GetLongFromJobContext(context, ScheduledItemContextKey);     // DB has been set to store context as a string

            return new EntityRef(triggerOnScheduleId);
        }

        public static JobBuilder UsingScheduledItemJobData(this JobBuilder jobBuilder, ScheduledItem scheduledItem)
        {
            return
                jobBuilder.UsingJobData(ScheduledItemHelper.ScheduledItemContextKey, scheduledItem.Id.ToString());
        }
    }
}
