// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Diagnostics;
using EDC.Monitoring;

namespace EDC.ReadiNow.Monitoring.Workflow
{
    /// <summary>
    /// Performance counters used for workflows.
    /// </summary>
    public class WorkflowPerformanceCounters
    {
        /// <summary>
        /// Access control counters.
        /// </summary>
        public static readonly string CategoryName = PerformanceCounterConstants.SoftwarePlatformCategoryPrefix + "Workflow";
        private static readonly string CategoryHelp = "Software platform workflow counters.";

        /// <summary>
        /// Average run duration.
        /// </summary>
        public static readonly string QueueDurationCounterName = "Queue Duration";
        private static readonly string QueueDurationCounterHelp = "Average duration a workflow queues until started.";

        /// <summary>
        /// Average run duration.
        /// </summary>
        public static readonly string RunDurationCounterName = "Run Duration";
        private static readonly string RunDurationCounterHelp = "Average duration of workflow runs until paused or completed";

        /// <summary>
        /// Run rate.
        /// </summary>
        public static readonly string RunRateCounterName = "Run Rate";
        private static readonly string RunRateCounterHelp = "Number of workflow runs per second";

        /// <summary>
        ///Run rate.
        /// </summary>
        public static readonly string RunCountCounterName = "Run Count";
        private static readonly string RunCountCounterHelp = "Total number of workflow runs";

        /// <summary>
        /// Trgger rate.
        /// </summary>
        public static readonly string TriggerRateCounterName = "Trigger Rate";
        private static readonly string TriggerRateCounterHelp = "Number of workflows triggered per second by entity creates of updates";

        /// <summary>
        /// Schedule rate.
        /// </summary>
        public static readonly string ScheduleFireRateCounterName = "Scheduler fire Rate";
        private static readonly string ScheduleFireRateCounterHelp = "Number of schedule events triggered per second";

        /// <summary>
        /// Average run duration.
        /// </summary>
        public static readonly string ScheduleJobDurationCounterName = "Schedule job Duration";
        private static readonly string ScheduleJobDurationCounterHelp = "Average duration of scheduler job";

        /// <summary>
        /// Create a new <see cref="WorkflowPerformanceCounters"/>.
        /// </summary>
        public WorkflowPerformanceCounters()
        {
           // Do nothing
        }

        /// <summary>
        /// Create the performance counters if they have not already been registered.
        /// Note the calling user must be a member of the local administrators group.
        /// </summary>
        public void CreateCategory()
        {
            new PerformanceCounterCategoryFactory()
                .AddAverageTimer32(QueueDurationCounterName, QueueDurationCounterHelp)
                .AddAverageTimer32(RunDurationCounterName, RunDurationCounterHelp)
                .AddRatePerSecond32(RunRateCounterName, RunRateCounterHelp)
                .AddNumberOfItems64(RunCountCounterName, RunCountCounterHelp)
                .AddRatePerSecond32(TriggerRateCounterName, TriggerRateCounterHelp)
                .AddRatePerSecond32(ScheduleFireRateCounterName, ScheduleFireRateCounterHelp)
                .AddAverageTimer32(ScheduleJobDurationCounterName, ScheduleJobDurationCounterHelp)
                .CreateCategory(CategoryName, CategoryHelp, PerformanceCounterCategoryType.SingleInstance);
        }
    }
}
