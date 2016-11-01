// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.SoftwarePlatform.Activities.Test.Triggers;
using NUnit.Framework;
using System;
using System.Linq;
using FluentAssertions;

namespace EDC.SoftwarePlatform.Activities.Test.Bugs
{
    [TestFixture]
    public class Bug_24028_DianasSecurityProblem : TriggerTestBase
    {
        [Test]
        [RunAsDefaultTenant]
        public void SimplifiedTest()
        {
            // Set up security so we have a user that can save stuff
            var myUser = Entity.Create<UserAccount>();
            myUser.Name = "Bob" + DateTime.Now;
            myUser.Save();

            new AccessRuleFactory().AddAllowByQuery(
            myUser.As<Subject>(),
            Resource.Resource_Type.As<SecurableEntity>(),
            new EntityRef("core:modify").ToEnumerable(),
            TestQueries.EntitiesWithName("Creatable").ToReport());


            // Create a person
            var p = Entity.Create<Person>();
            p.Name = "Creatable";

            p.Save();


            // THIS FAILS
            using (new SetUser(myUser))
            {
                var pw = p.AsWritable<Person>();
                pw.Name = "bob";
                pw.Save();
            }

            // THIS WORKS
            //var pw = p2.AsWritable<Person>();
            //pw.Name = "bob";
            //pw.Save();
        }

        [Test]
        [RunAsDefaultTenant]
        [Category("WorkflowTests"), Category("Bug"), Category("ExtendedTests")]
        public void TestWithWorkflowContext()
        {
            using (new WorkflowRunContext {RunTriggersInCurrentThread = true})
            {
                Workflow myWorkflow = null;
                UserAccount myUser = null;
                Person myPerson = null;
                DisplayFormUserTask task1 = null;
                DisplayFormUserTask task2 = null;

                try
                {
                    myWorkflow = Entity.Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddDisplayForm("Display Form 1", new string[] { "Exit1" }, null, null, null)
                        .AddLog("First Log", "Log 1")
                        .AddDisplayForm("Display Form 2", new string[] { "Exit1" }, null, null, null)
                        .AddLog("Seond Log", "Log 2");

                    myWorkflow.Name = "UNNUMBERED_DianasSecurityProblem " + DateTime.Now;
                    myWorkflow.WorkflowRunAsOwner = true;
                    myWorkflow.Save();
                    ToDelete.Add(myWorkflow.Id);

                    var myType = CreateType("CreateTriggerAddsAndRemovesTypeHook_type", UserResource.UserResource_Type);
                    var myTrigger = CreateTrigger("CreateTriggerAddsAndRemovesTypeHook_trigger", myType, myWorkflow);

                    myType.AsWritable();
                    myType.Name = "Creatable";
                    myType.IsOfType.Add(Resource.Resource_Type);
                    myType.Save();

                    myPerson = Entity.Create<Person>();
                    myPerson.FirstName = "John";
                    myPerson.LastName = "Bob";
                    myUser = Entity.Create<UserAccount>();
                    myUser.Name = "Bob" + DateTime.Now;
                    myUser.AccountHolder = myPerson;
                    myUser.Save();
                    ToDelete.Add(myUser.Id);

                    new AccessRuleFactory().AddAllowByQuery(myUser.As<Subject>(),
                        Resource.Resource_Type.As<SecurableEntity>(),
                        new EntityRef("core:create").ToEnumerable(),
                        TestQueries.EntitiesWithName("Creatable").ToReport());

                    using (new SetUser(myUser))
                    using (new WorkflowRunContext(true))
                    {
                        var e = Entity.Create(myType).As<Resource>();
                        e.Name = "MyName";
                        e.CreatedDate = DateTime.Now;
                        e.CreatedBy = myUser;
                        e.SecurityOwner = myUser;
                        e.Save();
                        ToDelete.Add(e.Id);
                    }

                    Workflow wf;
                    WorkflowRun run1;

                    wf = Entity.Get<Workflow>(myWorkflow.Id);

                    run1 = wf.RunningInstances.First();
                    Assert.That(run1, Is.Not.Null);

                    task1 = run1.TaskWithinWorkflowRun.Select(t => t.As<DisplayFormUserTask>()).FirstOrDefault(t => t != null);
                    Assert.That(task1, Is.Not.Null);

                    task1.AssignedToUser.Should().NotBeNull();
                    task1.AssignedToUser.Id.Should().Be(myPerson.Id);

                    using (new SetUser(myUser))
                    using (new WorkflowRunContext(true))
                    {
                        var writableTask = task1.AsWritable<DisplayFormUserTask>();
                        writableTask.UserResponse = task1.AvailableTransitions.First(t => t.Name == "Exit1");
                        writableTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                        writableTask.Save();
                    }

                    Assert.IsTrue(WaitForWorkflowToStop(run1), "Workflow run should have completed.");

                    wf = Entity.Get<Workflow>(wf.Id);

                    var run2 = wf.RunningInstances.First();
                    Assert.That(run2, Is.Not.Null);

                    task2 = run2.TaskWithinWorkflowRun.First(t => t.Id != task1.Id).As<DisplayFormUserTask>();
                    Assert.That(task2, Is.Not.Null);

                    using (new SetUser(myUser))
                    using (new WorkflowRunContext(true))
                    {
                        var writableTask = task2.AsWritable<DisplayFormUserTask>();
                        writableTask.UserResponse = writableTask.AvailableTransitions.First(t => t.Name == "Exit1");
                        writableTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                        writableTask.Save();
                    }

                    Assert.IsTrue(WaitForWorkflowToStop(run2), "Workflow run should have completed.");
                }
                finally
                {
                    if (task1 != null)
                        ToDelete.Add(task1.Id);
                    if (task2 != null)
                        ToDelete.Add(task2.Id);
                    if (myWorkflow != null)
                        ToDelete.Add(myWorkflow.Id);
                    if (myUser != null)
                        ToDelete.Add(myUser.Id);
                    if (myPerson != null)
                        ToDelete.Add(myPerson.Id);
                }
            }
        }
    }
}
