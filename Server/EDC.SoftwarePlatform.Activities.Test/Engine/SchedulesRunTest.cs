// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.Scheduling;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    /// <summary>
    /// These test create and wait for schedues to complete.
    /// 
    /// This tests that the scheduled workflows will run. The testign that the various schedules work is in another test.
    /// This test may take up to 30 seconds to run as it may need to wait for the scheduler to spool up.
    /// </summary>
    [TestFixture]
    
    public class SchedulesRunTest : TestBase
    {
        private const int secondsBetweenChecks = 1;
        private const int maximumChecks = 60;

        [TestFixtureSetUp]
        public void SetUp()
        {
            // start the scheduler outside the tests to lower timeout risk
            var scheduler = SchedulingHelper.Instance;
            Assert.IsTrue(scheduler.InStandbyMode);
        }

        private static Workflow WaitForWfRun(WfTriggerOnSchedule tos)
        {
            long relTypeId = new EntityRef( "core:workflowBeingRun" ).Id;

            Workflow workflow = null;

            // wait up to a minute checking if it starts.
            for (int i = 0; i < maximumChecks; i++)
            {
                Thread.Sleep(secondsBetweenChecks*1000);

                // refetch the workflow to see if it tried to run
                workflow = Entity.Get<Workflow>(tos.WorkflowToRun);

                // manually clear entity-relationship-cache, because 'RunningInstances' is one of the special relationships that
                // we've set to suppress invalidations for performance reasons. See Entity.SaveInvalidate.
                EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( workflow.Id, Direction.Reverse, relTypeId ), true );

				if ( workflow.RunningInstances.Count > 0 )
                {
                    Console.WriteLine("WaitForWfRun: waiting time " + i * secondsBetweenChecks);
                    break;
                }
            }

            return workflow;
        }

        [Test]
        //[Ignore("Test is not reliable, the workflow runs but the cache invalidation between the scheduler and the w3wp is a little unreliable")]
        [Description("Ensure that a schedule runs when expected.")]
        [RunAsDefaultTenant]
        public void EnsureAScheduledRunOnceRuns()
        {
            var schedule = new ScheduleOneOff { ScheduleSpecificTime = DateTime.UtcNow.AddMilliseconds(5000) };

            schedule.Save();

            var tos = new WfTriggerOnSchedule
            {
                Name = "EnsureAScheduledRunOnceRuns_Job " + DateTime.Now ,
                TriggerEnabled = true,
                WorkflowToRun = CreateLoggingWorkflow("EnsureAScheduledRunOnceRuns")
            };
            
            tos.ScheduleForTrigger.Add(schedule.As<Schedule>());
            tos.Save();

            ToDelete.Add(tos.Id);
            ToDelete.Add(tos.WorkflowToRun.Id);
            ToDelete.Add(tos.ScheduleForTrigger.First().Id);

            var workflow = WaitForWfRun(tos);

            Assert.AreEqual(1, workflow.RunningInstances.Count(), "Ensure the workflow actually ran once");
        }

        [Test]
        //[Ignore("Test is not reliable, the workflows runs but the cache invalidation between the scheduler and the w3wp is a little unreliable")]
        [Description("Ensure that two runs on the one trigger work.")]
        [RunAsDefaultTenant]
        public void TwoTriggersTest_22065()
        {
            var schedule = new ScheduleOneOff
            {
                ScheduleSpecificTime = DateTime.UtcNow.AddMilliseconds(500),

            };

            schedule.Save();

            var tos1 = new WfTriggerOnSchedule
            {
                TriggerEnabled = true,
                WorkflowToRun = CreateLoggingWorkflow("TwoTriggersTest_22065-1")
            };
            tos1.ScheduleForTrigger.Add(schedule.As<Schedule>());

            tos1.Save();

            ToDelete.Add(tos1.Id);
            ToDelete.Add(tos1.WorkflowToRun.Id);
            ToDelete.Add(tos1.ScheduleForTrigger.First().Id);

            var tos2 = new WfTriggerOnSchedule
            {
                TriggerEnabled = true,
                WorkflowToRun = CreateLoggingWorkflow("TwoTriggersTest_22065-2")
            };
            tos2.ScheduleForTrigger.Add(schedule.As<Schedule>());

            tos2.Save();

            ToDelete.Add(tos2.Id);
            ToDelete.Add(tos2.WorkflowToRun.Id);
            ToDelete.Add(tos2.ScheduleForTrigger.First().Id);

            var workflow1 = WaitForWfRun(tos1);
            var workflow2 = WaitForWfRun(tos2);

            Assert.AreEqual(1, workflow1.RunningInstances.Count(), "Ensure the workflow1 actually ran once");
            Assert.AreEqual(1, workflow2.RunningInstances.Count(), "Ensure the workflow2 actually ran once");
        }
    }
}