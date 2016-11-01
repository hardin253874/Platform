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
	[TestFixture]
	public class SchedulingSyncHelperTest : TestBase
	{
        [TestFixtureSetUp]
        public void SetUp()
        {
            // start the scheduler outside the tests to lower timeout risk
            var scheduler = SchedulingHelper.Instance;
        }


		[Test]
		[Description( "Ensure that a disabled triggerOnschedule does not get scheduled in Quarts." )]
		[RunAsDefaultTenant]
		public void CreateDisabledScheduleTest( )
		{
			var tos = new WfTriggerOnSchedule
				{
					WorkflowToRun = CreateLoggingWorkflow()
				};
			tos.ScheduleForTrigger.Add( new ScheduleDailyRepeat
				{
					SdrTimeOfDay = SqlDateTime.MinValue.Value.AddHours( 23 ).ToUniversalTime()
				}.As<Schedule>( ) );
			tos.TriggerEnabled = false;
			tos.Save( );

			ToDelete.Add( tos.Id );
			ToDelete.Add( tos.WorkflowToRun.Id );
			ToDelete.Add( tos.ScheduleForTrigger.First( ).Id );

			Assert.AreEqual( null, SchedulingHelper.Instance.GetJobDetail( SchedulingSyncHelper.GetJobId( tos ) ), "No job should be scheduled." );
		}

		[Test]
		[Description( "Ensure that a schedule with a workflow and trigger does get scheduled when created and unscheduled when deleted." )]
		[RunAsDefaultTenant]
		public void CreateScheduleThenDelete( )
		{
			var tos = new WfTriggerOnSchedule
				{
					WorkflowToRun = CreateLoggingWorkflow()
				};
			tos.ScheduleForTrigger.Add( new ScheduleDailyRepeat
				{
					SdrTimeOfDay = SqlDateTime.MinValue.Value.AddHours( 23 ).ToUniversalTime()
				}.As<Schedule>( ) );

			tos.Save( );

			ToDelete.Add( tos.Id );
			ToDelete.Add( tos.WorkflowToRun.Id );
			ToDelete.Add( tos.ScheduleForTrigger.First( ).Id );

			Assert.IsNotNull( SchedulingHelper.Instance.GetJobDetail( SchedulingSyncHelper.GetJobId( tos ) ), "Ensure the job was scheduled" );

			Entity.Delete( tos );

			Assert.IsNull( SchedulingHelper.Instance.GetJobDetail( SchedulingSyncHelper.GetJobId( tos ) ), "Ensure the job was unscheduled after delete." );
		}

		[Test]
		[Description( "Ensure that a schedule with no trigger does not get scheduled in Quartz." )]
		[RunAsDefaultTenant]
		public void CreateScheduleWithNoTriggerTest( )
		{
			var tos = new WfTriggerOnSchedule
				{
					WorkflowToRun = CreateLoggingWorkflow()
				};

			tos.Save( );

			ToDelete.Add( tos.Id );
			ToDelete.Add( tos.WorkflowToRun.Id );

			Assert.AreEqual( null, SchedulingHelper.Instance.GetJobDetail( SchedulingSyncHelper.GetJobId( tos ) ), "No job should be scheduled." );
		}


        [Test]
        [Description("Ensure recreating jobs works.")]
        [RunAsDefaultTenant]
        public void TestRecreateJobs()
        {
            SchedulingSyncHelper.RecreateJobsForTenant(RequestContext.TenantId, SchedulingHelper.Instance);
        }
	}
}