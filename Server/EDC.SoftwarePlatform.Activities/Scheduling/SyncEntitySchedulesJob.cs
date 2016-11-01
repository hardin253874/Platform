// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Diagnostics;
using Quartz;

namespace EDC.SoftwarePlatform.Activities.Scheduling
{
    /// <summary>
    ///  This job syncronizes schedules defined in resource model with the quartz scheduler.
    /// </summary>
    public class SyncEntitySchedulesJob: IJob
    {

        public void Execute(IJobExecutionContext context)
        {
            EventLog.Application.WriteInformation("Starting SyncEdcSchedulesJob");


            EventLog.Application.WriteInformation("Starting SyncEdcSchedulesJob");
        }
    }
}
