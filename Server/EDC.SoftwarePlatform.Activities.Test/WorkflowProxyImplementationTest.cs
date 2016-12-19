// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using FluentAssertions;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    //[Category("ExtendedTests")]
    [Category("WorkflowTests")]
	public class WorkflowProxyImplementationTest : TestBase
	{
        private static EntityType GetEntityType(string name)
        {
            return CodeNameResolver.GetTypeByName(name).As<EntityType>();
        }

        private static Field GetPersonField(string name)
        {
            var type = GetEntityType("AA_Person");
            var field = type.Fields.First(f => f.Name == name);
            return field;
        }

		private Workflow CreateInnerWf( )
		{
            var personType = GetEntityType("AA_Person");

            // Inner Workflow
            var innerWf = new Workflow
				{
					Name = "WorkflowNestedTest, Inner Wf, " + DateTime.Now
				};

            innerWf
                .AddDefaultExitPoint()
                .AddInput<ResourceArgument>("Resource", personType)
                .AddOutput<IntegerArgument>("Age")
                .AddLog("Log age", "{{Resource}} persons age is {{Resource.Age}}")
                .AddAssignToVar("Assign", "Resource.Age", "Age");

			return innerWf;
		}

		[Test]
		[RunAsDefaultTenant]
        [Timeout(100 * 1000)]
        [RunWithTransaction]
        public void ComplexNestedWorkflow( )
		{
            var personType = GetEntityType("AA_Person");
            var ageField = GetPersonField("Age");
            var bob = Entity.Create(personType).As<Resource>();
            bob.Name = "Test bob";

            bob.SetField(ageField, 13);
            bob.Save();
            ToDelete.Add(bob.Id);

            Workflow innerWf = CreateInnerWf();

            innerWf.Save();
            ToDelete.Add(innerWf.Id);


            var wf = new Workflow
            {
                Name = "Outer workflow" + DateTime.Now
            };

            var wfAs = wf.Cast<WfActivity>();

            wf
                .AddDefaultExitPoint()
                .AddInput<ResourceArgument>("OuterResource", personType)
                .AddOutput<IntegerArgument>("OuterAge")
                .AddWorkflowProxy("Proxy", innerWf)
                .AddExpressionToArgument("Proxy", "Resource", "OuterResource")
                .AddAssignToVar("AssignToOutput", "[Proxy_Age]", "OuterAge");

            wf.Save();
            ToDelete.Add(wf.Id);
                
            var inputs = new Dictionary<string, object>
            {
                {
                    "OuterResource", bob
                }
            };

            WorkflowRun run;
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                run = RunWorkflow(wf, inputs);
            }
            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "Finished without errors");

            Assert.AreEqual(13, run.GetOutput()["OuterAge"], "Age has been retrieved");
		}

		[Test]
		[RunAsDefaultTenant]
        [RunWithTransaction]
        public void ComplexNestedWorkflow_TestInner( )
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var personType = GetEntityType("AA_Person");
                var ageField = GetPersonField("Age");
                var bob = Entity.Create(personType).As<Resource>();
                bob.Name = "Test bob";

                bob.SetField(ageField, 33);
                bob.Save();
                ToDelete.Add(bob.Id);

                Workflow innerWf = CreateInnerWf();

                innerWf.Save();
                ToDelete.Add(innerWf.Id);

                var inputs = new Dictionary<string, object>
                {
                    {
                        "Resource", bob
                    }
                };

                var run = (RunWorkflow(innerWf, inputs));

                Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "Finished without errors");

                Assert.AreEqual(33, run.GetOutput()["Age"], "Age has been retrieved");
            }
		}

        [Test]
        [Timeout(180 * 1000)]
        [RunAsDefaultTenant]
        public void LowAccessUserPausedPriorNestedWorkflow_bug_27863()
	    {
            using (new WorkflowRunContext {RunTriggersInCurrentThread = true})
            {
                Workflow myParentWorkflow = null;
                Workflow myChildWorkflow = null;
                UserAccount myUser = null;
                Person myPerson = null;
                PromptUserTask userInputTask = null;

                try
                {
                    myChildWorkflow = Entity.Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddInput<BoolArgument>("InputChild")
                        .AddLog("Child Log", "Child Log");

                    myChildWorkflow.Name = "Child Workflow 27863 " + DateTime.Now;
                    myChildWorkflow.WorkflowRunAsOwner = true;
                    myChildWorkflow.Save();

                    myParentWorkflow = Entity.Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddInput<BoolArgument>("InputParent")
                        .AddPromptUser("User Input")
                        .AddWorkflowProxy("Child Workflow", myChildWorkflow)
                        .AddLog("Parent Log", "Parent Log");

                    var childWorkflowActivity = myParentWorkflow.ContainedActivities.FirstOrDefault(a => a.Name == "Child Workflow");

                    ActivityTestHelper.AddExpressionToActivityArgument(myParentWorkflow, childWorkflowActivity, "InputChild", "[InputParent]");

                    myParentWorkflow.Name = "Parent Workflow 27863 " + DateTime.Now;
                    myParentWorkflow.WorkflowRunAsOwner = true;
                    myParentWorkflow.Save();

                    myPerson = Entity.Create<Person>();
                    myPerson.FirstName = "Billy";
                    myPerson.LastName = "Bob";
                    myUser = Entity.Create<UserAccount>();
                    myUser.Name = "bb" + DateTime.Now;
                    myUser.AccountHolder = myPerson;
                    myUser.Save();

                    new AccessRuleFactory().AddAllowByQuery(myUser.As<Subject>(),
                        Workflow.Workflow_Type.As<SecurableEntity>(),
                        new EntityRef("core:read").ToEnumerable(),
                        TestQueries.Entities(new EntityRef("core:workflow")).ToReport());

                    WorkflowRun run;

                    using (new SetUser(myUser))
                    using (new WorkflowRunContext(true) { RunTriggersInCurrentThread = true })
                    {
                        run = RunWorkflow(myParentWorkflow);
                    }

                    run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunPaused);
                    run.PendingActivity.Should().NotBeNull();
                    run.PendingActivity.Name.Should().Be("User Input");
                    run.TaskWithinWorkflowRun.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);

                    userInputTask = run.TaskWithinWorkflowRun.First().AsWritable<PromptUserTask>();
                    userInputTask.Should().NotBeNull();

                    userInputTask.PromptForTaskStateInfo.Should().NotBeNull().And.HaveCount(1);
                    userInputTask.PromptForTaskStateInfo.Select(si => si.StateInfoArgument.Name).Should().Contain("InputParent");

                    var value = userInputTask.PromptForTaskStateInfo.First(si => si.StateInfoArgument.Name == "InputParent");
                    var param = value.StateInfoValue.AsWritable<BoolArgument>();
                    param.BoolParameterValue = true;
                    param.Save();

                    userInputTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                    userInputTask.UserTaskIsComplete = true;

                    using (new SetUser(myUser))
                    using (new WorkflowRunContext(true) { RunTriggersInCurrentThread = true })
                    {
                        userInputTask.Save();
                    }

                    var wf = Entity.Get<Workflow>(myParentWorkflow.Id, Workflow.RunningInstances_Field);

                    wf.RunningInstances.Count().Should().Be(1);
                    var runResume = wf.RunningInstances.First();

                    Assert.IsTrue(WaitForWorkflowToStop(runResume), "Workflow run should have completed.");

                    runResume = Entity.Get<WorkflowRun>(runResume.Id, WorkflowRun.WorkflowRunStatus_Field);
                    runResume.Should().NotBeNull();
                    runResume.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunCompleted);
                    runResume.RunLog.Should().NotBeNull().And.NotBeEmpty();
                }
                finally
                {
                    if (userInputTask != null)
                        ToDelete.Add(userInputTask.Id);
                    if (myParentWorkflow != null)
                        ToDelete.Add(myParentWorkflow.Id);
                    if (myChildWorkflow != null)
                        ToDelete.Add(myChildWorkflow.Id);
                    if (myUser != null)
                        ToDelete.Add(myUser.Id);
                    if (myPerson != null)
                        ToDelete.Add(myPerson.Id);
                }
            }
        }

	    [Test]
	    [RunAsDefaultTenant]
        [RunWithTransaction]
        public void ProxyCallingItself_bug_17649()
	    {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                // Inner Workflow
                var wf = new Workflow
                {
                    Name = "ProxyCallingItself_bug_17649 " + DateTime.Now
                };

                var isInnerArg = new BoolArgument()
                {
                    Name = "Is Inner"
                }.As<ActivityArgument>();
                wf.InputArguments.Add(isInnerArg);

                wf.AddDefaultExitPoint();

                var decision = new DecisionActivity() { Name = "Is Inner" };
                var decisionAs = decision.As<WfActivity>();
                ActivityTestHelper.AddExpressionToActivityArgument(wf, decisionAs, "DecisionArgument", "[Is Inner]", false);

                wf.FirstActivity = decisionAs;
                wf.ContainedActivities.Add(decisionAs);

                WorkflowProxy innerProxy = ActivityTestHelper.CreateWorkflowProxy(wf);
                var innerProxyAs = innerProxy.As<WfActivity>();

                wf.ContainedActivities.Add(innerProxyAs);

                ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf);

                ActivityTestHelper.AddExpressionToActivityArgument(wf, innerProxyAs, "Is Inner", "true", false);

                ActivityTestHelper.AddTransition(wf, decisionAs, innerProxyAs, "No");
                ActivityTestHelper.AddTermination(wf, innerProxyAs);
                ActivityTestHelper.AddTermination(wf, decisionAs, "Yes");

                wf.Save();
                ToDelete.Add(wf.Id);

                wf.Validate();


                var input = new Dictionary<string, object>() { { "Is Inner", true } };
                var run = (RunWorkflow(wf, input));

                Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "Finished without errors");
            }
	    }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestMultipleNestedWorkflowActivities_Bug_24928()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                Workflow wfA = null, wfB = null, wfC = null, wfD = null;

                #region Arrange
                var exitA = Entity.Create<ExitPoint>();
                exitA.Name = "TEST Exit A";
                exitA.IsDefaultExitPoint = true;
                exitA.Save();

                var exitB = Entity.Create<ExitPoint>();
                exitB.Name = "TEST Exit B";
                exitB.IsDefaultExitPoint = true;
                exitB.Save();

                var exitC = Entity.Create<ExitPoint>();
                exitC.Name = "TEST Exit C";
                exitC.IsDefaultExitPoint = true;
                exitC.Save();

                var exitD = Entity.Create<ExitPoint>();
                exitD.Name = "TEST Exit D";
                exitD.IsDefaultExitPoint = true;
                exitD.Save();

                wfA = Entity.Create<Workflow>();
                wfA.Name = "TEST WF A";
                wfA.ExitPoints.Add(exitA);

                var logA = Entity.Create<LogActivity>().As<WfActivity>();
                logA.Name = "TEST Log A";
                wfA.AddActivity(logA);

                ActivityTestHelper.AddExpressionToActivityArgument(wfA, logA, "Message", "'WF A'");
                wfA.Save();

                wfB = Entity.Create<Workflow>();
                wfB.Name = "TEST WF B";
                wfB.ExitPoints.Add(exitB);

                var logB = Entity.Create<LogActivity>().As<WfActivity>();
                logB.Name = "TEST Log B";
                wfB.AddActivity(logB);

                ActivityTestHelper.AddExpressionToActivityArgument(wfB, logB, "Message", "'WF B'");
                wfB.Save();

                wfC = Entity.Create<Workflow>();
                wfC.Name = "TEST WF C";
                wfC.ExitPoints.Add(exitC);

                var proxyA = Entity.Create<WorkflowProxy>();
                proxyA.Name = "TEST Run WF A";
                proxyA.WorkflowToProxy = wfA;
                var exitProxyA = Entity.Create<ExitPoint>();
                exitProxyA.Name = "TEST Exit A";
                exitProxyA.IsDefaultExitPoint = true;
                proxyA.ExitPoints.Add(exitProxyA);
                wfC.AddActivity(proxyA.As<WfActivity>());

                var logCa = Entity.Create<LogActivity>().As<WfActivity>();
                logCa.Name = "TEST Log C A";
                wfC.AddActivity(logCa);

                ActivityTestHelper.AddExpressionToActivityArgument(wfC, logCa, "Message", "'WF C A'");

                var proxyB = Entity.Create<WorkflowProxy>();
                proxyB.Name = "TEST Run WF B";
                proxyB.WorkflowToProxy = wfB;
                var exitProxyB = Entity.Create<ExitPoint>();
                exitProxyB.Name = "TEST Exit B";
                exitProxyB.IsDefaultExitPoint = true;
                proxyB.ExitPoints.Add(exitProxyB);
                wfC.AddActivity(proxyB.As<WfActivity>());

                var logCb = Entity.Create<LogActivity>().As<WfActivity>();
                logCb.Name = "TEST Log C B";
                wfC.AddActivity(logCb);

                ActivityTestHelper.AddExpressionToActivityArgument(wfC, logCb, "Message", "'WF C B'");
                wfC.Save();

                wfD = Entity.Create<Workflow>();
                wfD.Name = "TEST WF D";
                wfD.ExitPoints.Add(exitD);

                var proxyA2 = Entity.Create<WorkflowProxy>();
                proxyA2.Name = "TEST Run WF A";
                proxyA2.WorkflowToProxy = wfA;
                var exitProxyA2 = Entity.Create<ExitPoint>();
                exitProxyA2.Name = "TEST Exit A";
                exitProxyA2.IsDefaultExitPoint = true;
                proxyA2.ExitPoints.Add(exitProxyA2);
                wfD.AddActivity(proxyA2.As<WfActivity>());

                var logDa = Entity.Create<LogActivity>().As<WfActivity>();
                logDa.Name = "TEST Log D A";
                wfD.AddActivity(logDa);

                ActivityTestHelper.AddExpressionToActivityArgument(wfD, logDa, "Message", "'WF D A'");

                var proxyB2 = Entity.Create<WorkflowProxy>();
                proxyB2.Name = "TEST Run WF B";
                proxyB2.WorkflowToProxy = wfB;
                var exitProxyB2 = Entity.Create<ExitPoint>();
                exitProxyB2.Name = "TEST Exit B";
                exitProxyB2.IsDefaultExitPoint = true;
                proxyB2.ExitPoints.Add(exitProxyB2);
                wfD.AddActivity(proxyB2.As<WfActivity>());

                var logDb = Entity.Create<LogActivity>().As<WfActivity>();
                logDb.Name = "TEST Log D B";
                wfD.AddActivity(logDb);

                ActivityTestHelper.AddExpressionToActivityArgument(wfD, logDb, "Message", "'WF D B'");

                var proxyC = Entity.Create<WorkflowProxy>();
                proxyC.Name = "TEST Run D C";
                proxyC.WorkflowToProxy = wfC;
                var exitProxyC = Entity.Create<ExitPoint>();
                exitProxyC.Name = "TEST Exit C";
                exitProxyC.IsDefaultExitPoint = true;
                proxyC.ExitPoints.Add(exitProxyC);
                wfD.AddActivity(proxyC.As<WfActivity>());

                var logDc = Entity.Create<LogActivity>().As<WfActivity>();
                logDc.Name = "TEST Log D C";
                wfD.AddActivity(logDc);

                ActivityTestHelper.AddExpressionToActivityArgument(wfD, logDc, "Message", "'WF D C'");
                wfD.Save();
                #endregion

                // Act
                WorkflowRun run;
                using (new WorkflowRunContext(true))
                {
                    run = RunWorkflow(wfD);
                }

                // Assert
                run.Should().NotBeNull();
                run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunCompleted);
                run.Id.Should().Be(run.Id);
                run.RunStepCounter.Should().Be(7);
            }
        }

        [Test]
        [Timeout(30000)]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void RecursiveWorkflowLimit_Bug_27649()
        {
            var wf = new Workflow
            {
                Name = "RecursiveWorkflowLimit_Bug_27649" + DateTime.Now
            };

            wf.Save();
            wf = wf.AsWritable<Workflow>();

            wf
                .AddDefaultExitPoint()
                .AddLog("log before", "RecursiveWorkflowLimit_Bug_27649 before")
                .AddWorkflowProxy("call to self", wf)
                .AddLog("log after", "RecursiveWorkflowLimit_Bug_27649 after");

            wf.Save();

            WorkflowRun run;

            using (new WorkflowRunContext())
            {
                run = RunWorkflow(wf);
            }

            run.Should().NotBeNull();
            run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunFailed);
            Assert.That(run.ErrorLogEntry.Name, Contains.Substring("Inner workflow"));
        }

        [Test]
        [Timeout(30000)]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void DoubleResume_Bug_27724()
        {
            var wf0 = CreateLoggingWorkflow("DoubleResume_Bug_27724 Bottom ");
            wf0.Save();

            var wf1 = new Workflow
            {
                Name = "DoubleResume_Bug_27724 Middle " + DateTime.Now
            };

            wf1
                .AddDefaultExitPoint()
                .AddWorkflowProxy("call to bottom", wf0);

            wf1.Save();

            var wf2 = new Workflow
            {
                Name = "DoubleResume_Bug_27724 Top " + DateTime.Now
            };

            wf2
                .AddDefaultExitPoint()
                .AddWorkflowProxy("call to wf1", wf1);

            wf2.Save();

            WorkflowRun run;

            using (new WorkflowRunContext())
            {
                run = RunWorkflow(wf2);
            }

            run.Should().NotBeNull();
            run.WorkflowRunStatus_Enum.Should().Be(WorkflowRunState_Enumeration.WorkflowRunCompleted);

            var childRuns = run.ChildRuns;

            Assert.That(childRuns.Count(), Is.EqualTo(1));
            Assert.That(childRuns.First().WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));

            var childChildRuns = childRuns[0].ChildRuns;

            Assert.That(childChildRuns.Count(), Is.EqualTo(1));
            Assert.That(childChildRuns.First().WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));

        }
    }
}