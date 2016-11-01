// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System.Linq;
//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test.WorkflowValidationTests
{
	[TestFixture]
	public class WorkflowValidationTest : TestBase
	{


		[Test]
		[RunAsDefaultTenant]
		public void OnlyOneExpressionPerArgument( )
		{
			var wf = new Workflow
				{
					Name = "wf" + DateTime.Now
				};
			var logging = ( new LogActivity
				{
					Name = "log" + DateTime.Now
				} ).As<WfActivity>( );
			wf.FirstActivity = logging;
			wf.ContainedActivities.Add( logging );

			ActivityTestHelper.AddExpressionToActivityArgument( wf, logging, "Message", "'First expression'", false );
			ActivityTestHelper.AddExpressionToActivityArgument( wf, logging, "Message", "'Second expression'", false );

			wf.Save( );
			ToDelete.Add( wf.Id );

            var messages = wf.Validate();
            Assert.Greater(messages.Count(), 0, "There are error messages");
            Assert.IsTrue(messages.Any(m=>m.Contains("An expression has more than one argument to populate.")));
		}

		[Test]
		[RunAsDefaultTenant]
		public void WorkflowMustHaveFirstActivityWiredUp( )
		{
			var wf = new Workflow
				{
					Name = "wf" + DateTime.Now
				};
			var logging = ( new LogActivity
				{
					Name = "log" + DateTime.Now
				} ).As<WfActivity>( );
			wf.ContainedActivities.Add( logging );

			wf.Save( );
			ToDelete.Add( wf.Id );

            var messages = wf.Validate();
            Assert.Greater(messages.Count(), 0, "There are error messages");
            Assert.IsTrue(messages.Any(m=>m.Contains("first activity")));

		}

		[Test]
		[RunAsDefaultTenant]
		public void WorkflowVariablesAndArgumentsCannotOverlap( )
		{
			var wf = new Workflow
				{
					Name = "wf" + DateTime.Now
				};
			wf.InputArguments.Add( ( new IntegerArgument
				{
					Name = "Clashing"
				} ).As<ActivityArgument>( ) );
			wf.Variables.Add( ( new IntegerArgument
				{
					Name = "Clashing"
				} ).As<ActivityArgument>( ) );

			var logging = ( new LogActivity
				{
					Name = "log" + DateTime.Now
				} ).As<WfActivity>( );
			wf.ContainedActivities.Add( logging );
			wf.FirstActivity = logging;

			wf.Save( );
			ToDelete.Add( wf.Id );


            var messages = wf.Validate();
            Assert.Greater(messages.Count(), 0, "There are error messages");
            Assert.IsTrue(messages.Any(m => m.Contains("clashed")));

		}

        [Test]
        [RunAsDefaultTenant]
        public void AllActivitiesUniqueName()
        {
            var wf = new Workflow
            {
                Name = "wf" + DateTime.Now
            };

            wf
                .AddDefaultExitPoint()
                .AddLog("Log", "Message")
                .AddLog("Log", "Message");


            wf.Save();
            ToDelete.Add(wf.Id);

            var messages = wf.Validate();
            Assert.That(messages.Count(), Is.EqualTo(1));
            Assert.That(messages.First(), Is.EqualTo("All activities in a workflow must have a unique name: Log"));
        }
    }
}