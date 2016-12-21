// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using FluentAssertions;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using Autofac;
using EDC.SoftwarePlatform.Activities.Test.Mocks;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class DisplayFormTest : TestBase
	{
		[TestFixtureSetUp]
		public void SetUp( )
		{
			// start the scheduler outside the tests to lower timeout risk
			var scheduler = SchedulingHelper.Instance;
			if ( !scheduler.IsStarted && !scheduler.InStandbyMode )
			{
				scheduler.Standby( );
			}
		}

		private static DateTime TimeIt( string message, DateTime start )
		{
			DateTime end = DateTime.Now;
			Console.WriteLine( message + ( end - start ) );
			return end;
		}

        [Test]
        [RunAsDefaultTenant]
        [TestCase("Option 1", "First Exit", null,  -1.0, false, false)]
        [TestCase("Option 2", "Second Exit", null, -1.0, false, false)]
        [TestCase("Option 1", "First Exit", "highPriority", 50.0, true, false)]
        [TestCase("Option 2", "Second Exit", "highPriority", 50.0, false, true)]
        public void TestDisplayFormSelection(string selectedButton, string expectedExitPointName, string priorityName, double percentageCompleteFloat, bool waitForNext, bool keepTask)
        {
            WorkflowRun run;
            DateTime start;
            DisplayFormUserTask userTask;

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var priority = priorityName != null ? new EntityRef("core", priorityName) : null;
                Decimal? percentageComplete = percentageCompleteFloat == -1 ? null : (decimal?)percentageCompleteFloat;

                start = DateTime.Now;

                Workflow approvalWf = CreateDisplayFormTestWf(priority, percentageComplete, waitForNext, null, keepTask);

                start = TimeIt("Created: ", start);

                using (new WorkflowRunContext(true))
                {
                    run = RunWorkflow(approvalWf);
                }   

                start = TimeIt("Run: ", start);

                Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunPaused, run.WorkflowRunStatus_Enum, "Expected workflow to be paused.");
                Assert.IsNotNull(run.PendingActivity, "Expected Pending activity to be set.");

                var userTasks = run.TaskWithinWorkflowRun;

                Assert.IsTrue(userTasks.Count == 1, "There should be one user task for the workflow run.");

                userTask = userTasks.First().AsWritable<DisplayFormUserTask>();

                Assert.AreEqual(2, userTask.AvailableTransitions.Count, "There should be two available exits to use.");
                Assert.AreEqual(run.Id, userTask.WorkflowRunForTask.Id, "The workflow run is assigned");

                if (priority == null)
                {
                    Assert.AreEqual("core:normalPriority", userTask.TaskPriority.Alias, "Priority defaults to normal");
                }
                else
                {
                    Assert.AreEqual(Entity.Get(priority).Id, userTask.TaskPriority.Id, "Priority correctly set on task");
                }

                Assert.AreEqual(percentageComplete, userTask.PercentageCompleted, "Completed correctly set on task");
                Assert.AreEqual(waitForNext, userTask.WaitForNextTask, "Completed correctly set on task");

                // Complete the task
                userTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                userTask.UserTaskCompletedOn = DateTime.Now - TimeSpan.FromDays(2); // set a ficticious completed time
                userTask.UserResponse = userTask.AvailableTransitions.First(e => e.Name == selectedButton);
                userTask.TaskComment = "Comment";
                userTask.Save();
            }
            Assert.IsTrue(WaitForWorkflowToStop(run, 60 * 1000, 1000), "Workflow run should have completed.");

            run = Entity.Get<WorkflowRun>(run);

            TimeIt("Completed: ", start);

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum);

            var exitPoint = run.GetExitPoint();

            Assert.AreEqual(expectedExitPointName, exitPoint.Name, "Ensure response was followed");

            var updatedUserTask = Entity.Get<BaseUserTask>(userTask.Id);

            if (keepTask)
            {
                Assert.IsNotNull(updatedUserTask.UserTaskCompletedOn, "Completed on has been set");
                Assert.IsTrue(DateTime.UtcNow - updatedUserTask.UserTaskCompletedOn < TimeSpan.FromMinutes(5),
                    "The user task completed time ignored the user submitted time.");

                var output = run.GetOutput();
                Assert.That(output, Is.Not.Empty);
                var returnedUserTaskId = (IEntity)output["UserTaskOutput"];

                Assert.That(userTask.Id, Is.EqualTo(returnedUserTaskId.Id));
                Assert.That(userTask.LogEntryForUserAction, Is.Not.Null);
                Assert.That(userTask.LogEntryForUserAction.WuleUserComment, Is.EqualTo("Comment"));
                Assert.That(userTask.LogEntryForUserAction.WuleCompletedDate, Is.EqualTo(updatedUserTask.UserTaskCompletedOn));
                Assert.That(userTask.LogEntryForUserAction.WuleDueDate, Is.EqualTo(updatedUserTask.UserTaskDueOn));
                Assert.That(userTask.LogEntryForUserAction.ActingPersonReferencedInLog, Is.Not.Null);
                Assert.That(userTask.LogEntryForUserAction.ActingPersonReferencedInLog.Id, Is.EqualTo(updatedUserTask.AssignedToUser.Id));
                Assert.That(userTask.LogEntryForUserAction.ActingPersonName, Is.StringStarting("Dummy"));
                Assert.That(userTask.LogEntryForUserAction.WuleActionSummary, Is.StringEnding("action summary"));
                Assert.That(userTask.LogEntryForUserAction.ObjectReferencedInLog, Is.Not.Null);
                Assert.That(userTask.LogEntryForUserAction.ObjectReferencedInLog.Name, Is.StringContaining("Sri Korada"));
                
            }
            else
            {
                Assert.That(updatedUserTask, Is.Null, "The user task has been deleted");
            }

		}
        
		protected Workflow CreateDisplayFormTestWf( EntityRef priority, decimal? percentageComplete, bool? waitForNext, decimal? timeoutSeconds = null, bool? keepTask = null )
		{

            // moved this out to help with deadlocking issues when running the test.

            Person dummyPerson = null;
            UserAccount dummyAccount = null;

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                dummyPerson = Entity.Create<Person>();
                var tag = Rand.Next().ToString();
                dummyPerson.FirstName = "Dummy" + tag;
                dummyPerson.LastName = "Dummy" + tag;
                dummyPerson.Name = dummyPerson.FirstName + ' ' + dummyPerson.LastName;
                dummyPerson.Save();

                dummyAccount = Entity.Create<UserAccount>();
                dummyAccount.Name = "Dummy" + DateTime.Now + Rand.Next();
                dummyAccount.AccountHolder = dummyPerson;
                dummyAccount.Save();

            }

            var displayFormWf = Entity.Create<Workflow>();
            displayFormWf.Name = "Test approval Wf";

            displayFormWf
                .AddWfExitPoint("First Exit", true)
                .AddWfExitPoint("Second Exit")
                .AddWfExitPoint("Time-out")
                .AddLog("log", "Test");


            var displayForm = Entity.Create<DisplayFormActivity>();
            displayForm.Name = "displayForm";
            var displayFormAs = displayForm.Cast<WfActivity>();
            var withExits = displayForm.Cast<EntityWithArgsAndExits>();

            // add some exits
            foreach (var name in new[]
            {
            "Option 1",
            "Option 2"
        })
            {
                var exitPoint = Entity.Create<ExitPoint>();
                exitPoint.Name = name;
                exitPoint.ExitPointActionSummary = name + " action summary";
                exitPoint.IsDefaultExitPoint = name == "Option 1";
                withExits.ExitPoints.Add(exitPoint);
            }

            displayFormWf.AddActivity(displayFormAs, dontTerminate: true);

            var timeOutExitPoint = Entity.Get<ExitPoint>("core:displayFormTimeout");

            displayFormWf
                .AddEntityExpressionToInputArgument(displayFormAs, "Form", new EntityRef("test", "personForm"))
                .AddEntityExpressionToInputArgument(displayFormAs, "Record", new EntityRef("test", "aaAaSriKorada"))
                .AddEntityExpressionToInputArgument(displayFormAs, "For Person", dummyPerson);

            if (timeoutSeconds != null)
            {
                var days = (double)timeoutSeconds / 60.0 / 60.0 / 24.0;
                ActivityTestHelper.AddExpressionToActivityArgument(displayFormWf, displayFormAs, "Due in (Days)",
                    days.ToString(CultureInfo.InvariantCulture));
            }
            if (priority != null)
                displayFormWf.AddEntityExpressionToInputArgument(displayFormAs, "Priority", priority);

            if (percentageComplete != null)
                displayFormWf.AddExpressionToActivityArgument(displayFormAs, "% Completed", percentageComplete.ToString());

            if (waitForNext != null)
                displayFormWf.AddExpressionToActivityArgument(displayFormAs, "Wait for next form", waitForNext.ToString());

            if (keepTask != null)
                displayFormWf.AddExpressionToActivityArgument(displayFormAs, "Record history", keepTask.ToString());

            displayFormWf
                .AddOutput<ResourceArgument>("UserTaskOutput")
                .AddAssignToVar("AssignOutput", "[displayForm_Completed task]", "UserTaskOutput", "displayForm", "Option 2")
                .AddTermination("displayForm", "Option 1", "First Exit")
                .AddTermination("AssignOutput", null, "Second Exit")
                .AddTermination("displayForm", timeOutExitPoint.Name, "Time-out");


            //ActivityTestHelper.AddTermination(displayFormWf, displayFormAs, "Option 2", "Second Exit");
            //ActivityTestHelper.AddTermination(displayFormWf, displayFormAs, timeOutExitPoint.Name, "Time-out");

            displayFormWf.Save();
            ToDelete.Add(displayFormWf.Id);
            ToDelete.Add(dummyAccount.Id);
            ToDelete.Add(dummyAccount.AccountHolder.Id);
            return displayFormWf;
		}

        
        [Test]
		[RunAsDefaultTenant]
		public void TestTimeout( )
		{
            var dummyTimeoutHelper = new DummyTimeoutHelper();

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => dummyTimeoutHelper).As<ITimeoutActivityHelper>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                
                Workflow displayFormWf = CreateDisplayFormTestWf(null, null, null, 5);

                WorkflowRun run = RunWorkflow(displayFormWf);

                dummyTimeoutHelper.Timeout();

                run = Entity.Get<WorkflowRun>(run);

                Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum);

                var exitPoint = run.GetExitPoint();

                Assert.AreEqual("Time-out", exitPoint.Name, "Workflow exited at a time-out");
                // Need to figure out how to test the exit point.
            }
		}

        [Test]
        [RunAsDefaultTenant]
        public void TestDisplayFormDeleteInputParameter_Bug_25116()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                IEntity driver = null;
                Workflow wf = null;

                try
                {
                    WorkflowRun run, run2;


                    #region Arrange
                    var driverType = new EntityRef("test", "driver");

                    driver = Entity.Create(driverType);
                    driver.SetField("core:name", "Test Delete Driver");
                    driver.Save();

                    wf = Entity.Create<Workflow>();
                    wf.Name = "TEST WF";
                    wf.AddDefaultExitPoint();
                    wf.AddInput<ResourceArgument>("Input", Entity.Get<EntityType>(driverType));
                    wf.InputArgumentForAction = wf.InputArguments.First();

                    var i = Entity.Create<WfExpression>();
                    i.ArgumentToPopulate = wf.InputArgumentForAction;
                    wf.ExpressionMap = new EntityCollection<WfExpression> { i };
                    wf.InputArgumentForAction.PopulatedByExpression = new EntityCollection<WfExpression> { i };

                    var uAExitA = Entity.Create<ExitPoint>();
                    uAExitA.Name = "Exit A";
                    uAExitA.ExitPointActionSummary = "Exit A action summary";
                    uAExitA.IsDefaultExitPoint = true;
                    uAExitA.Save();

                    var userActionA = Entity.Create<DisplayFormActivity>();
                    userActionA.Name = "A";
                    userActionA.ExitPoints.Add(uAExitA);

                    var uA = userActionA.As<WfActivity>();
                    wf.AddActivity(uA);

                    ActivityTestHelper.AddExpressionToActivityArgument(wf, uA, "Record", "[Input]");
                    ActivityTestHelper.AddExpressionToActivityArgument(wf, uA, "Wait for next form", "true");
                    ActivityTestHelper.AddExpressionToActivityArgument(wf, uA, "Hide comment field", "true");
                    ActivityTestHelper.AddExpressionToActivityArgument(wf, uA, "Record history", "false");
                    ActivityTestHelper.AddEntityExpression(wf, uA, uA.GetInputArgument("For Person"), new EntityRef("core", "administratorPerson"));
                    ActivityTestHelper.AddEntityExpression(wf, uA, uA.GetInputArgument("Form"), new EntityRef("test", "personForm"));
                    uA.Save();

                    var delete = Entity.Create<DeleteActivity>();
                    delete.Name = "Delete";

                    var d = delete.As<WfActivity>();
                    wf.AddActivity(d);

                    ActivityTestHelper.AddExpressionToActivityArgument(wf, d, "Record", "[Input]");
                    d.Save();

                    var uBExitC = Entity.Create<ExitPoint>();
                    uBExitC.Name = "Exit C";
                    uBExitC.ExitPointActionSummary = "Exit C action summary";
                    uBExitC.IsDefaultExitPoint = true;
                    uBExitC.Save();

                    var userActionB = Entity.Create<DisplayFormActivity>();
                    userActionB.Name = "B";
                    userActionB.ExitPoints.Add(uBExitC);

                    var uB = userActionB.As<WfActivity>();
                    wf.AddActivity(uB);

                    ActivityTestHelper.AddEntityExpression(wf, uB, uB.GetInputArgument("For Person"), new EntityRef("core", "administratorPerson"));
                    uB.Save();

                    ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf);

                    wf.Save();
                    #endregion

                    // Act
                    var wfInput = new Dictionary<string, object> { { "Input", driver } };

                    using (new WorkflowRunContext(true))
                    {
                        run = RunWorkflow(wf, wfInput);
                    }
                    run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunPaused);
                    run.PendingActivity.Should().NotBeNull();
                    run.PendingActivity.Name.Should().Be("A");
                    run.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

                    var userTaskA = run.TaskWithinWorkflowRun.First().AsWritable<DisplayFormUserTask>();
                    userTaskA.AvailableTransitions.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);
                    userTaskA.WorkflowRunForTask.Id.Should().Be(run.Id);

                    var resumeEvent = new UserCompletesTaskEvent
                    {
                        CompletionStateId = userTaskA.AvailableTransitions.First(t => t.Name == "Exit A").Id,
                        UserTaskId = run.TaskWithinWorkflowRun.First().Id
                    };

                    using (new WorkflowRunContext(true))
                    {
                        run2 = WorkflowRunner.Instance.ResumeWorkflow(run, resumeEvent);
                    }

                    // Assert
                    run2.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunPaused);
                    run2.PendingActivity.Should().NotBeNull();
                    run2.PendingActivity.Name.Should().Be("B");
                    run2.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

                    var deletedDriver = Entity.Exists(new EntityRef(driver.Id));
                    deletedDriver.Should().BeFalse();
                }
                finally
                {
                    if (wf != null)
                        ToDelete.Add(wf.Id);
                    if (driver != null && Entity.Exists(new EntityRef(driver.Id)))
                        ToDelete.Add(driver.Id);
                }
            }
        }
	}
}