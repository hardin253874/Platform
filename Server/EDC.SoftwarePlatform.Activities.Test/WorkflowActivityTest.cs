// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class WorkflowActivityTest : TestBase
	{
		[SetUp]
		public void TestInitialize( )
		{
			using ( new AdministratorContext( ) )
			{
				_alternateExitPoint = new ExitPoint( );
				_alternateExitPoint.Save( );
				ToDelete.Add( _alternateExitPoint.Id );
			}
		}

		private static ExitPoint CreateDefaultExitPoint()
		{
            var result = Entity.Create<ExitPoint>();
            result.Name = "GeneratedDefaultExitPoint";
            result.IsDefaultExitPoint = true;
            return result;
		}

		private static ExitPoint _alternateExitPoint;


		private class RunActionWithNameActivity : ActivityImplementationBase, IRunNowActivity
		{
			public static Action<long> Action;

			void IRunNowActivity.OnRunNow( IRunState context, ActivityInputs inputs )
			{
				Action( ActivityInstance.Id );
			}
		}

		[Test]
        [RunAsDefaultTenant]
		public void TestRun( )
		{
			var wf = new Workflow
				{
					Name = "Wf"
				};

			wf.AddDefaultExitPoint( );

			var l1 = new LogActivity
				{
					Name = "l1"
				}.Cast<WfActivity>( );
			var l2 = new LogActivity
				{
					Name = "12"
				}.Cast<WfActivity>( );
			// wf.FirstActivity = l1;
			//wf.ContainedActivities.Add(l1);
			wf.ContainedActivities.Add( l2 );

			ActivityTestHelper.AddFirstActivityWithMapping( wf, l1, null );
			ActivityTestHelper.AddExpressionToActivityArgument( wf, l1, "Message", "'Message 1'", false );

			ActivityTestHelper.AddTransition( wf, l1, l2 );
			ActivityTestHelper.AddExpressionToActivityArgument( wf, l2, "Message", "'Message 2'", false );

            ActivityTestHelper.AddTermination(wf, l2, l2.GetDefaultExitPoint(), CreateDefaultExitPoint());

			wf.Save( );
			ActivityImplementationBase nextActivity = wf.Cast<WfActivity>( ).CreateWindowsActivity( );

			l1.Save( );
			l2.Save( );

			ToDelete.Add( wf.Id );
			ToDelete.Add( l1.Id );
			ToDelete.Add( l2.Id );

            var run = (RunWorkflow(wf));
            
            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "Finished without errors");

			// run using service
		}

		[Test]
        [RunAsDefaultTenant]
		public void TestTwoActivitiesRunInCorrectOrder( )
		{
			// Check that the activities are running in the correct order.
			var runLog = new List<long>( );

			RunActionWithNameActivity.Action = runLog.Add;

			ActivityType testType = CreateClassBackedType( typeof ( RunActionWithNameActivity ) );

			var wf = new Workflow
				{
					Name = "Wf"
				};

			wf.AddDefaultExitPoint( );

			var l1 = new Entity( testType.Id ).Cast<WfActivity>( );
			var l2 = new Entity( testType.Id ).Cast<WfActivity>( );

            l1.Name = "l1";
            l2.Name = "l2";
            ActivityTestHelper.AddFirstActivityWithMapping( wf, l1, null );

			wf.ContainedActivities.Add( l2 );
			ActivityTestHelper.AddTransition( wf, l1, l2 );

			ActivityTestHelper.AddTermination( wf, l2 );

			wf.Save( );
			ToDelete.Add( wf.Id );

            var run = (RunWorkflow(wf));

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "Finished without errors");


			var expectedOrder = new List<long>
				{
					l1.Id,
					l2.Id
				};
			Assert.AreEqual( expectedOrder, runLog );
		}

	}
}