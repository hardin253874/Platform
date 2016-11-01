// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    public class PromptUserImplementationTest : TestBase
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            // start the scheduler outside the tests to lower timeout risk
            var scheduler = SchedulingHelper.Instance;
            if (!scheduler.IsStarted && !scheduler.InStandbyMode)
            {
                scheduler.Standby();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestDisplayFormResumeWithPromptActivity_Bug_27060()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                Workflow wf = null;
                DisplayFormUserTask task = null;
                PromptUserTask p = null;

                try
                {
                    WorkflowRun run, run2, run3;


                    // Arrange
                    var cheese = new EntityRef("test", "cheese");
                    var cheeses = Entity.GetInstancesOfType(cheese);

                    //
                    // IMPORTANT: the point is to NOT set a value on the variable for this test!
                    //            the run state serialization that occurs when pausing must realize that the variable is used
                    //            subsequently.
                    //
                    wf = Entity.Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddVariable<ResourceArgument>("myVar", null, Entity.Get<EntityType>(new EntityRef("test", "employee")))
                        //.AddEntityExpressionToVariable("myVar", new EntityRef("test", "aaJudeJacobs"))
                        .AddInput<ResourceArgument>("myInput", Entity.Get<EntityType>(cheese))
                        .AddDisplayForm("Display Form", new[] { "Exit1", "Exit2" }, recordExpression: "[myInput]")
                        .AddPromptUser("Prompt User")
                        .AddLog("Log", "MyVar: {{myVar}}");

                    wf.Name = "!TestDisplayFormResumeWithVariables_Bug_23903_" + DateTime.Now;

                    wf.Save();

                    // Act
                    var wfInput = new Dictionary<string, object> { { "myInput", cheeses.First() } };

                    using (new WorkflowRunContext(true))
                    {
                        run = RunWorkflow(wf, wfInput);
                    }

                    run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunPaused);
                    run.PendingActivity.Should().NotBeNull();
                    run.PendingActivity.Name.Should().Be("Display Form");
                    run.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

                    task = run.TaskWithinWorkflowRun.First().AsWritable<DisplayFormUserTask>();
                    task.Should().NotBeNull();

                    run.StateInfo.Count.Should().BeGreaterOrEqualTo(2);
                    run.StateInfo.Select(si => si.StateInfoArgument.Name).Should().NotContain("myVar");
                    run.StateInfo.Select(si => si.StateInfoArgument.Name).Should().Contain("myInput");

                    using (new WorkflowRunContext(true))
                    {
                        run2 = WorkflowRunner.Instance.ResumeWorkflow(run, new UserCompletesTaskEvent
                        {
                            CompletionStateId = task.AvailableTransitions.First(t => t.Name == "Exit1").Id,
                            UserTaskId= task.Id
                        });
                    }

                    run2.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunPaused);
                    run2.PendingActivity.Should().NotBeNull();
                    run2.PendingActivity.Name.Should().Be("Prompt User");
                    run2.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

                    p = run2.TaskWithinWorkflowRun.First().AsWritable<PromptUserTask>();
                    p.Should().NotBeNull();

                    p.PromptForTaskStateInfo.Should().NotBeNull().And.HaveCount(2);
                    p.PromptForTaskStateInfo.Select(si => si.StateInfoArgument.Name).Should().Contain("myVar");
                    p.PromptForTaskStateInfo.Select(si => si.StateInfoArgument.Name).Should().Contain("myInput");

                    run2.StateInfo.Count.Should().BeGreaterOrEqualTo(3);
                    run2.StateInfo.Select(si => si.StateInfoArgument.Name).Should().NotContain("myVar");
                    run2.StateInfo.Select(si => si.StateInfoArgument.Name).Should().Contain("myInput");

                    var value = p.PromptForTaskStateInfo.First(si => si.StateInfoArgument.Name == "myVar");
                    var param = value.StateInfoValue.AsWritable<ResourceArgument>();
                    param.ResourceParameterValue = Entity.Get<Resource>(new EntityRef("test", "aaJudeJacobs"));
                    param.Save();

                    var param2 = Entity.Get<ResourceArgument>(param);
                    param2.ResourceParameterValue.Should().NotBeNull();
                    param2.ResourceParameterValue.Name.Should().Be("Jude Jacobs");

                    using (new WorkflowRunContext(true))
                    {
                        run3 = WorkflowRunner.Instance.ResumeWorkflow(run2, new PromptUserTaskCompletedEvent
                        {
                            UserTaskId = p.Id
                        });
                    }

                    // Assert
                    run3.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunCompleted);
                    run3.RunLog.Should().NotBeNull().And.NotBeEmpty();
                    run3.RunLog.Where(l => l.Description == "MyVar: Jude Jacobs").Should().NotBeEmpty();
                }
                finally
                {
                    if (p != null)
                        ToDelete.Add(p.Id);
                    if (task != null)
                        ToDelete.Add(task.Id);
                    if (wf != null)
                        ToDelete.Add(wf.Id);
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestPromptActivityLowSecurityUser_Bug_27134()
        {

            Workflow wf = null;
            PromptUserTask p = null;

            try
            {
                // Arrange
                var dummyPerson = Entity.Create<Person>();
                dummyPerson.Name = "Dummy Person" + DateTime.Now + Rand.Next();
                dummyPerson.FirstName = "Dummy";
                dummyPerson.LastName = "Dummy";
                dummyPerson.Save();

                var dummyAccount = Entity.Create<UserAccount>();
                dummyAccount.Name = "Dummy" + DateTime.Now + Rand.Next();
                dummyAccount.AccountHolder = dummyPerson;
                dummyAccount.Save();

                wf = Entity.Create<Workflow>();
                wf.Name = "Test User Input Workflow" + DateTime.Now + Rand.Next();
                //wf.WorkflowRunAsOwner = true;
                //wf.SecurityOwner = dummyAccount;
                wf.AddWfExitPoint("Test User Input Workflow Exit Point", true);
                wf.AddInput<ResourceArgument>("input", Resource.Resource_Type);

                var act = Entity.Create<PromptUserActivity>();
                act.Name = "Test User Input" + DateTime.Now + Rand.Next();
                act.PromptForArguments.Add(new ActivityPrompt { Name = "input", ActivityPromptArgument = wf.InputArguments.First() });
                var actAs = act.Cast<WfActivity>();

                wf.AddActivity(actAs);
                wf.AddEntityExpressionToInputArgument(actAs, "For Person", dummyPerson);
                wf.Save();

                // Act
                WorkflowRun run;
                using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
                {
                    run = RunWorkflow(wf);
                }

                // Assert
                run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunPaused);
                run.PendingActivity.Should().NotBeNull();
                run.PendingActivity.Name.Should().Be(act.Name);
                run.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

                var task = run.TaskWithinWorkflowRun.FirstOrDefault();
                task.Should().NotBeNull();

                using (new SetUser(dummyAccount))
                {
                    p = Entity.Get<PromptUserTask>(task.Id);
                    p.Should().NotBeNull();

                    p.PromptForTaskArguments.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

                    var arg = p.PromptForTaskArguments.First();
                    arg.ActivityPromptArgument.Should().NotBeNull();
                    arg.ActivityPromptArgument.Name.Should().Be("input");
                }
            }
            finally
            {
                if (p != null)
                    ToDelete.Add(p.Id);
                if (wf != null)
                    ToDelete.Add(wf.Id);
            }
        }
    }
}
