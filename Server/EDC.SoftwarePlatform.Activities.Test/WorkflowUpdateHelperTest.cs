// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.Exceptions;
using EDC.ReadiNow.Common.Workflow;

namespace EDC.SoftwarePlatform.Activities.Test
{
    /// <summary>
    /// Test to ensure that creating, deleting and modifying triggers adds the appropriate hooks to the types.
    /// </summary>
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class WorkflowUpdateHelperTest : TestBase
    {
        [Test]
        [ExpectedException(typeof(WebArgumentException))]
        public void NoUpdatingOldVersion()
        {
            var wfNew = new Workflow();
            wfNew.WfOlderVersion = new Workflow();
            wfNew.Save();

            WorkflowUpdateHelper.Update(wfNew.WfOlderVersion.Id, () => { });
        }

        [TestCase(null, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 3)]
        public void VersionNumber(int? versionNumber, int expectedVersionNumber)
        {
            var workflow = CreateWorkflow();
            workflow.WorkflowVersion = versionNumber;
            WorkflowUpdateHelper.Update(workflow.Id, () => { });
            Assert.AreEqual(expectedVersionNumber, workflow.WorkflowVersion);
        }

        [Test]
        public void UpdateHashUpdated()
        {
            var workflow = base.CreateLoggingWorkflow();
            workflow.Save();
            var hashBefore = workflow.WorkflowUpdateHash;

            WorkflowUpdateHelper.Update(workflow.Id, () => { });

            var hashAfter = workflow.WorkflowUpdateHash;

            Assert.AreNotEqual(hashBefore, hashAfter, "Hash changed");

            WorkflowUpdateHelper.Update(workflow.Id, () => { });
            var hashAfterAfter = workflow.WorkflowUpdateHash;

            Assert.AreNotEqual(hashAfter, hashAfterAfter, "Hash changed");
        }


        [Test]
        public void CloneRemovedFromSolution()
        {
            var workflow = CreatePausedWorkflow();
            workflow.InSolution = Entity.Get<Solution>(new EntityRef("testSolution"));
            workflow.Save();

            WorkflowUpdateHelper.Update(workflow.Id, () => { });

            workflow = Entity.Get<Workflow>(workflow.Id);
            Assert.That(workflow.WfOlderVersion, Is.Not.Null);
            Assert.That(workflow.WfOlderVersion.InSolution, Is.Null);
        }

        [Test]
        public void NoPausedInstances()
        {
            var workflow = CreateWorkflow();
            workflow.Save();

            var beforeCount = Entity.GetInstancesOfType<Workflow>().Count();

            Action updateAct = () =>
                {
                    var wf = Entity.Get<Workflow>(workflow.Id, true);
                    wf.Name += "_";
                    wf.Save();
                };

            WorkflowUpdateHelper.Update(workflow.Id, updateAct);

            var afterCount = Entity.GetInstancesOfType<Workflow>().Count();

            Assert.IsNull(workflow.WfOlderVersion, "We don't have an older version");
            Assert.IsNull(workflow.WfNewerVersion, "We don't have a newer version");
            Assert.AreEqual(beforeCount, afterCount, "No workflows created");
            Assert.AreEqual(2, workflow.WorkflowVersion, "Version updated");
        }
        
        [Test]
        public void PausedInstances()
        {
            var workflow = CreatePausedWorkflow();
            workflow.Save();

            var beforeCount = Entity.GetInstancesOfType<Workflow>().Count();

            Action updateAct = () =>
            {
                var wf = Entity.Get<Workflow>(workflow.Id, true);
                wf.Name += "_";
                wf.Save();
            };

            WorkflowUpdateHelper.Update(workflow.Id, updateAct);

            var afterCount = Entity.GetInstancesOfType<Workflow>().Count();

            Assert.IsNotNull(workflow.WfOlderVersion, "We have an older version");
            Assert.IsNull(workflow.WfNewerVersion, "We don't have a newer version");
            Assert.AreEqual(beforeCount + 1, afterCount, "One workflows created");
        }

        [Test]
        public void EditPausedWorkflowUnderHobbledSecurityAccount_Bug_25246()
        {
            Workflow wf = null, wfClone = null;

            try
            {
                #region Arrange
                var dogBreedType = Entity.GetByName<EntityType>("AA_DogBreeds").FirstOrDefault();
                dogBreedType.Should().NotBeNull("Couldn't locate the type for dog breeds.");

                var poodle = Entity.GetByName("Poodle").FirstOrDefault();
                poodle.Should().NotBeNull("Couldn't locate poodle.");

                wf = Entity.Create<Workflow>();
                wf.Name = "Pet the poodle.";
                wf.AddDefaultExitPoint();
                wf.AddInput<ResourceArgument>("Input", dogBreedType);
                wf.InputArgumentForAction = wf.InputArguments.First();

                var i = Entity.Create<WfExpression>();
                i.ArgumentToPopulate = wf.InputArgumentForAction;
                wf.ExpressionMap = new EntityCollection<WfExpression> { i };
                wf.InputArgumentForAction.PopulatedByExpression = new EntityCollection<WfExpression> { i };

                var exitPoint = Entity.Create<ExitPoint>();
                exitPoint.Name = "Pet";
                exitPoint.IsDefaultExitPoint = true;
                exitPoint.Save();

                var userAction = Entity.Create<DisplayFormActivity>();
                userAction.Name = "Do Action";
                userAction.ExitPoints.Add(exitPoint);

                var act = userAction.As<WfActivity>();
                wf.AddActivity(act);

                ActivityTestHelper.AddExpressionToActivityArgument(wf, act, "Record", "[Input]");
                ActivityTestHelper.AddEntityExpression(wf, act, act.GetInputArgument("For Person"), new EntityRef("core", "administratorPerson"));
                act.Save();

                ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf);

                wf.Save();
                #endregion

                // Act
                using (new SetUser(Entity.Get<UserAccount>("core:administratorUserAccount")))
                {
                    // Hobbled.

                    var wfInput = new Dictionary<string, object> { { "Input", poodle } };

                    var run = RunWorkflow(wf, wfInput);
                        
                    run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunPaused);
                    run.PendingActivity.Should().NotBeNull();
                    run.PendingActivity.Name.Should().Be("Do Action");
                    run.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);
                        
                    run.WorkflowBeingRun.Should().NotBeNull();
                    run.WorkflowBeingRun.FirstActivity.Should().NotBeNull();
                    run.WorkflowBeingRun.RunningInstances.Should().NotBeNull();
                    run.WorkflowBeingRun.RunningInstances.Count.Should().Be(1);

                    // Edit
                    WorkflowUpdateHelper.Update(wf.Id, () =>
                    {
                        var wfEdit = Entity.Get<Workflow>(wf.Id, true).AsWritable<Workflow>();
                        wfEdit.Description = "What?";

                        var actEdit = wfEdit.FirstActivity.AsWritable<WfActivity>();
                        actEdit.Name += "2";
                        actEdit.Save();

                        wfEdit.Save();
                    });

                    // Assert
                    var wfOriginal = Entity.Get<Workflow>(wf.Id);
                    wfOriginal.Should().NotBeNull();
                    wfOriginal.Description.Should().Be("What?");
                    wfOriginal.FirstActivity.Should().NotBeNull();
                    wfOriginal.FirstActivity.Name.Should().Be("Do Action2"); // i guess?
                    wfOriginal.RunningInstances.Should().NotBeNull().And.BeEmpty();
                    wfOriginal.WfOlderVersion.Should().NotBeNull();
                    wfOriginal.WfNewerVersion.Should().BeNull();

                    wfClone = wfOriginal.WfOlderVersion;
                    wfClone.Description.Should().BeNullOrEmpty();
                    wfClone.FirstActivity.Should().NotBeNull();
                    wfClone.FirstActivity.Name.Should().Be("Do Action");
                    wfClone.RunningInstances.Should().NotBeNull();
                    wfClone.RunningInstances.Count.Should().Be(1);
                }
            }
            finally
            {
                if (wf != null)
                {
                    ToDelete.Add(wf.Id);
                }
                if (wfClone != null)
                {
                    ToDelete.Add(wfClone.Id);
                }
            }
        }
        
        [Test]
        //[Ignore("This test will fail until the clone activity is fixed")]
        public void ContinueTheOldWorkflow()
        {
            var workflow = CreateWorkflowWithUserAction();

            workflow.Save();

            var input = new Dictionary<string, object> {{"input", "my input"}};

            var run = RunWorkflow(workflow, input);

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunPaused, run.WorkflowRunStatus_Enum, "The workflow is paused");

            WorkflowUpdateHelper.Update(workflow.Id, () => { });

            var resumeEvent = new UserCompletesTaskEvent()
            {
                CompletionStateId = run.WorkflowBeingRun.FirstActivity.ForwardTransitions.First().Id,
                UserTaskId = run.TaskWithinWorkflowRun.First().Id
            };
            
            // break the workflow
            workflow.Delete();

            run = WorkflowRunner.Instance.ResumeWorkflow(run, resumeEvent);

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "The workflow completed");
        }

        [Test]
        public void CheckPromptTaskAfterUpgrade()
        {
            var workflow = CreateWorkflowWithUserPrompt();

            workflow.Save();

            var input = new Dictionary<string, object> { { "input", "my input" } };

            var run = RunWorkflow(workflow, input);

            run.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

            var task = run.TaskWithinWorkflowRun.First().As<PromptUserTask>();
            task.Should().NotBeNull();
            task.PromptForTaskArguments.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunPaused, run.WorkflowRunStatus_Enum, "The workflow is paused");

            WorkflowUpdateHelper.Update(workflow.Id, () => { });

            var run2 = RunWorkflow(workflow, input);
            run2.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

            var task1 = Entity.Get<PromptUserTask>(task.Id);
            task1.Should().NotBeNull();
            task1.PromptForTaskArguments.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);
        }

        [Test]
        public void Bug25996_OldVersioOnReport()
        {
            var workflow = CreateWorkflow();
            CreatePausedRun(workflow);
            workflow.Save();

            WorkflowUpdateHelper.Update(workflow.Id, () => { });

            CreatePausedRun(workflow).Save(); ;

            WorkflowUpdateHelper.Update(workflow.Id, () => { });
            
            workflow = Entity.Get<Workflow>(workflow.Id);
            Assert.That(workflow.WfOlderVersion, Is.Not.Null);
            Assert.That(workflow.WfOlderVersion.WfOlderVersion, Is.Not.Null);
        }

        Workflow CreateWorkflow()
        {
            var solution = new Solution();
            var workflow = base.CreateLoggingWorkflow();
	        workflow.InSolution = solution;
            return workflow;
        }

        Workflow CreatePausedWorkflow()
        {
            var workflow = CreateWorkflow();

            CreatePausedRun(workflow);
                      
            return workflow;
        }

        WorkflowRun CreatePausedRun(Workflow workflow)
        {
            var run = Entity.Create<WorkflowRun>();
            run.WorkflowRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunPaused;
            run.WorkflowBeingRun = workflow;
            workflow.RunningInstances.Add(run);

            return run;
        }

        Workflow CreateWorkflowWithUserAction()
        {
            var solution = new Solution();
            var workflow = new Workflow { Name = "WorkflowUpdateHelperTest" };
            workflow.InSolution = solution;
            workflow
                .AddDefaultExitPoint()
                .AddInput<StringArgument>("input")
                .AddDisplayForm("Display Form", new string[] { "Exit1" }, null, null)
                .AddLog("Log", "'Input: {{input}}");


            return workflow;
        }

        Workflow CreateWorkflowWithUserPrompt()
        {
            var solution = new Solution();
            var workflow = new Workflow { Name = "WorkflowUpdateHelperTestPrompt" };
            workflow.InSolution = solution;
            workflow
                .AddDefaultExitPoint()
                .AddInput<StringArgument>("input")
                .AddPromptUser("Prompt user")
                .AddLog("Log", "'Input: {{input}}");

            return workflow;
        }
    }
}