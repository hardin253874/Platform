// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.Test.Bugs
{
    [TestFixture]
    public class Bug_23903_resumeMissingInput : TestBase
    {
        [Test]
        [RunAsDefaultTenant]
        [Category("WorkflowTests"), Category("Bug"), Category("ExtendedTests")]
        // NOTE: Cannot be run in transaction
        public void Test()
        {
            var wf = Entity
                .Create<Workflow>()
                .AddDefaultExitPoint()
                .AddVariable<StringArgument>("name", "[input].Name")
                .AddInput<ResourceArgument>("input", Person.Person_Type)
                .AddDisplayForm("Display Form", new string[] { "Exit1" }, null, null, "[input]")
                .AddUpdateField("Update Field", Person.Name_Field.As<Resource>(), "[input]", "[input].Name + 'xxx'");

            wf.Name = "UNNUMBERED_resumeMissingInput " + DateTime.Now;

            wf.Save();
            ToDelete.Add(wf.Id);

            var newPerson = Entity.Create<Person>();
            newPerson.Name = "newUser " + DateTime.Now;
            newPerson.Save();
            ToDelete.Add(newPerson.Id);

            var inputs = new Dictionary<string, object> { { "input", newPerson } };

            var run = TestBase.RunWorkflow(wf, inputs, null, true);
                
            Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunPaused));

            var userTasks = run.TaskWithinWorkflowRun;

            Assert.IsTrue(userTasks.Count == 1, "There should be one user task for the workflow run.");

            var userTask = userTasks.First().AsWritable<DisplayFormUserTask>();

            // Complete the task
            userTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            userTask.UserResponse = userTask.AvailableTransitions.First();
            userTask.TaskComment = "Comment";
            userTask.Save();

            Assert.IsTrue(WaitForWorkflowToStop(run, 30 * 1000), "Workflow run should have completed.");

            run = Entity.Get<WorkflowRun>(run);

            Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted), "The run completed");
        }
    }
}
