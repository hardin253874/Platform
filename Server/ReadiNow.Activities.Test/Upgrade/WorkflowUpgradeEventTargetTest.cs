// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Common.Workflow;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Activities.Triggers;
using EDC.ReadiNow.Model.EventClasses;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.SoftwarePlatform.Activities.Engine.Upgrade;



namespace EDC.SoftwarePlatform.Activities.Test.Upgrade
{
    /// <summary>
    /// Test to ensure that creating, deleting and modifying triggers adds the appropriate hooks to the types.
    /// </summary>
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class WorkflowUpgradeEventTargetTest: TestBase
    {
        [TestCase(null, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 3)]
        public void VersionNumberWhenUpgraded(int? versionNumber, int expectedVersionNumber)
        {
            var workflow = CreateWorkflow();
            workflow.WorkflowVersion = versionNumber;
            workflow.Save();
            UpgradeWithUpdate(workflow);
            Assert.AreEqual(expectedVersionNumber, workflow.WorkflowVersion);
        }



        [Test]
        public void NoPausedInstancesWithNoReplacement()
        {
            var workflow = CreateWorkflow();

            workflow.Save();

            var beforeCount = Entity.GetInstancesOfType<Workflow>().Count();
            
            Upgrade(workflow);

            var afterCount = Entity.GetInstancesOfType<Workflow>().Count();

            Assert.IsNull(workflow.WfOlderVersion, "We don't have an older version");
            Assert.IsNull(workflow.WfNewerVersion, "We don't have a newer version");
            Assert.AreEqual(beforeCount, afterCount, "No workflows created");
        }

        [Test]
        public void NoPausedInstancesWithReplacement()
        {
            var workflow = CreateWorkflow();

            workflow.Save();

            var beforeCount = Entity.GetInstancesOfType<Workflow>().Count();

            UpgradeWithUpdate(workflow);


            var afterCount = Entity.GetInstancesOfType<Workflow>().Count();

            Assert.IsNull(workflow.WfOlderVersion, "We don't have an older version");
            Assert.IsNull(workflow.WfNewerVersion, "We don't have a newer version");
            Assert.AreEqual(beforeCount, afterCount, "No workflows created");
        }



        [Test]
        public void HavePausedInstancesWithNoReplacement()
        {
            var workflow = CreatePausedWorkflow();
             
            workflow.Save();

            var beforeCount = Entity.GetInstancesOfType<Workflow>().Count();

            Upgrade(workflow);

            var afterCount = Entity.GetInstancesOfType<Workflow>().Count();

            Assert.IsNull(workflow.WfOlderVersion, "We don't have an older version");
            Assert.IsNull(workflow.WfNewerVersion, "We don't have a newer version");
            Assert.AreEqual(beforeCount, afterCount, "No workflows created");
        }

        [Test]
        public void HavePausedInstancesWithReplacement()
        {
            var workflow = CreatePausedWorkflow();

            workflow.Save();

            var beforeCount = Entity.GetInstancesOfType<Workflow>().Count();

            UpgradeWithUpdate(workflow);

            var afterCount = Entity.GetInstancesOfType<Workflow>().Count();

            workflow = Entity.Get<Workflow>(workflow.Id);

            Assert.IsNotNull(workflow.WfOlderVersion, "We do have an older verison");
            Assert.IsNull(workflow.WfNewerVersion, "We don't have a newer version");
            Assert.AreEqual(beforeCount + 1, afterCount, "1 workflows created");
            Assert.IsEmpty(workflow.RunningInstances, "New workflow has no running instances");
            Assert.IsNotEmpty(workflow.WfOlderVersion.RunningInstances, "Old workflow has running instances");
            Assert.AreEqual(1, workflow.WfOlderVersion.RunningInstances.Count, "Old workflow has 1 running instance");
        }


        [Test]
        [ExpectedException("System.ArgumentException")]
        public void YouCantRunAOldVersion()
        {
            var workflow = CreatePausedWorkflow();
            workflow.Save();
            UpgradeWithUpdate(workflow);

            Assert.IsNotNull(workflow.WfOlderVersion, "There is an older version");
            TestBase.RunWorkflow(workflow.WfOlderVersion);

        }


        [Test]
        //[RunAsDefaultTenant]
        //[Ignore("This workflow is not working due to clone not correctly cloning the first activity relationship.")]
        public void ContinueTheOldWorkflow()
        {
            var workflow = CreateWorkflowWithUserAction();
            workflow.Save();

            var input = new Dictionary<string, object> {{"input", "my input"}};

            WorkflowRun run;
            using (new WorkflowRunContext {RunTriggersInCurrentThread = true})
            {
                run = TestBase.RunWorkflow(workflow, input);
            }

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunPaused, run.WorkflowRunStatus_Enum, "The workflow is paused");

            UpgradeWithUpdate(workflow);

            var completionState = run.WorkflowBeingRun.FirstActivity.ForwardTransitions.First();
                
            // break the workflow
            workflow.Delete();

            var resumeEvent = new UserCompletesTaskEvent
            {
                CompletionStateId = completionState.Id,
                UserTaskId = run.TaskWithinWorkflowRun.First().Id
            };
                        
            run = WorkflowRunner.Instance.ResumeWorkflow(run, resumeEvent);

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "The workflow completed");
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

            var run = Entity.Create<WorkflowRun>();
            
            run.WorkflowRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunPaused;
            workflow.RunningInstances.Add(run);

            return workflow;
        }

        Workflow CreateWorkflowWithUserAction()
        {
            var solution = new Solution();
            var workflow = new Workflow {Name = "WorkflowUpgradeEventTargetTest"};
            workflow.InSolution = solution;
            workflow
                .AddDefaultExitPoint()
                .AddInput<StringArgument>("input")
                .AddDisplayForm("Display Form", new string[] { "Exit1" }, null, null)
                .AddLog("Log", "'Input: {{input}}");
            

            return workflow;
        }

        void Upgrade(Workflow workflow)
        {
            var target = new WorkflowUpgradeEventTarget();
            var state = new Dictionary<string, object>();

            target.OnBeforeUpgrade(workflow.InSolution.ToEnumerable( ), state);
            target.OnAfterUpgrade(workflow.InSolution.ToEnumerable( ), state);
        }

        void UpgradeWithUpdate(Workflow workflow)
        {
            var target = new WorkflowUpgradeEventTarget();
            var state = new Dictionary<string, object>();

            target.OnBeforeUpgrade(workflow.InSolution.ToEnumerable( ), state);
            UpdateWorkflowVersionHash(workflow);
            target.OnAfterUpgrade(workflow.InSolution.ToEnumerable( ), state);
        }

        void UpdateWorkflowVersionHash(Workflow wf)
        {
            wf.WorkflowUpdateHash = new Random().Next().ToString();
            wf.Save();
        }
    }
}
