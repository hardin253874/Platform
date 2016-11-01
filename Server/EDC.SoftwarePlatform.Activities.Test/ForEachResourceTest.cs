// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    [Category("ExtendedTests")]
    [Category("WorkflowTests")]
	public class ForEachResourceTest : TestBase
	{
        

		/// <summary>
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
        [RunWithTransaction]
		public void SimpleCountingLoop( )
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var wf = new Workflow() { Name = "Simple Counting " + DateTime.Now };

                wf.AddDefaultExitPoint();

                var input = new ResourceListArgument
                {
                    Name = "in"
                };

                wf.InputArguments.Add(input.As<ActivityArgument>());

                var output = new IntegerArgument
                {
                    Name = "out"
                };

                wf.OutputArguments.Add(output.As<ActivityArgument>());

                var employeeType = Entity.Get<EntityType>(new EntityRef("test", "employee"));

                wf.AddVariable<IntegerArgument>("Count")
                  .AddAssignToVar("Assign To Count", "0", "Count")
                  .AddForEach("foreach1", "in", "test:employee")
                  .AddAssignToVar("Assign To Count 2", "[Count] + 1", "Count", "foreach1", "Loop")
                   .AddTransition("Assign To Count 2", "foreach1");

                ActivityTestHelper.AddAssignToVar(wf, "AssignOutput", "Count", "out");

                ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf);

                wf.Save();

                var createdCount = 15;
                var resources = Enumerable.Range(0, createdCount).Select(v => new Person() { Name = v.ToString() }).ToList();
                Entity.Save(resources);

                var wfInput = new Dictionary<string, object>
                {
                    {
                        "in", resources
                    }
                };

                var run = (RunWorkflow(wf, wfInput));

                IDictionary<string, object> outputs = run.GetOutput();

                Assert.AreEqual(1, outputs.Count, "There is one output argument");

                var result = (int)outputs["out"];

                Assert.AreEqual(createdCount, result, "The loop ran the correct number of times");
            }
		}
        
        /// <summary>
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestInputVarNotSet()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var wf = new Workflow();

                wf.AddDefaultExitPoint();

                var forEach1 = new ForEachResource
                {
                    Name = "foreach1" + DateTime.Now
                };
                var forEach1As = forEach1.As<WfActivity>();

                wf.FirstActivity = forEach1As;
                wf.ContainedActivities.Add(forEach1As);

                var loopExitPoint = Entity.Get<ExitPoint>(ForeachImplementation.LoopExitPointAlias);

                ActivityTestHelper.AddTransition(wf, forEach1As, forEach1As, loopExitPoint);
                ActivityTestHelper.AddTermination(wf, forEach1As);
                //ActivityHelper.AddMissingExpressionParametersToWorkflow(wf);

                wf.Save();
                ToDelete.Add(wf.Id);

                ActivityImplementationBase nextActivity = forEach1As.Cast<WfActivity>().CreateWindowsActivity();

                var metaData = new WorkflowMetadata();
                nextActivity.Validate(metaData);

                Assert.IsTrue(metaData.ValidationMessages.Count() == 1, "There is only one validation message");
                Assert.IsTrue(metaData.ValidationMessages.First().StartsWith("Mandatory argument"), "Validation is 'Mandatory argument'");
            }
         }

        [Test]
        [RunAsDefaultTenant]
        [Timeout(60000)]
        [RunWithTransaction]
        public void LoopWithinLoop_bug_17651()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var wf = new Workflow() { Name = "LoopWithinLoop_bug_17651" };
                wf.AddDefaultExitPoint();

                var count = new IntegerArgument() { Name = "count" };
                ActivityTestHelper.AddVariableToWorkflow(wf, count.As<ActivityArgument>());
                wf.AddExpressionToWorkflowVariable("count", "0");

                var foreachX = new ForEachResource() { Name = "ForEachX" + DateTime.Now };
                var foreachXAs = foreachX.As<WfActivity>();
                ActivityTestHelper.AddExpressionToActivityArgument(wf, foreachXAs, "List", "all([Workflow Event])");

                wf.FirstActivity = foreachXAs;
                wf.ContainedActivities.Add(foreachXAs);


                var foreachY = new ForEachResource() { Name = "ForEachY" + DateTime.Now };
                var foreachYAs = foreachY.As<WfActivity>();
                ActivityTestHelper.AddExpressionToActivityArgument(wf, foreachYAs, "List", "all([Event Email Priority Enum])");

                wf.ContainedActivities.Add(foreachYAs);

                ActivityTestHelper.AddTransition(wf, foreachXAs, foreachYAs, "Loop");
                ActivityTestHelper.AddTermination(wf, foreachXAs, "Finished");

                var assign = new AssignToVariable() { Name = "Assign" };
                var assignAs = assign.As<WfActivity>();
                assign.TargetVariable = count.As<ActivityArgument>();
                ActivityTestHelper.AddExpressionToActivityArgument(wf, assignAs, "Value", "count + 1");

                wf.ContainedActivities.Add(assignAs);

                ActivityTestHelper.AddTransition(wf, foreachYAs, assignAs, "Loop");
                ActivityTestHelper.AddTransition(wf, foreachYAs, foreachXAs, "Finished");

                ActivityTestHelper.AddTransition(wf, assignAs, foreachYAs);

                var output = new IntegerArgument() { Name = "out" };
                var outputAs = output.As<ActivityArgument>();

                wf.OutputArguments.Add(outputAs);

                ActivityTestHelper.AddAssignToVar(wf, "Set Output", "count", "out");

                ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf);

                wf.Save();
                ToDelete.Add(wf.Id);

                var run = (RunWorkflow(wf));
                var outputs = run.GetOutput();

                Assert.AreEqual(1, outputs.Count, "There is one output argument");

                var result = (int)outputs["out"];

                var triggeredOnCount = Entity.GetInstancesOfType<WorkflowEventEnum>(false).Count();
                var runStateCount = Entity.GetInstancesOfType<EventEmailPriorityEnum>(false).Count();


                Assert.AreEqual(triggeredOnCount * runStateCount, result, "The nested loops ran the correct number of times.");
            }
        }


        /// <summary>
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [Timeout(60000)]
        [RunWithTransaction]
        public void TestLoopOverNoResultsInExpression()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var wf = new Workflow();

                wf.AddDefaultExitPoint();

                var forEach1 = new ForEachResource
                {
                    Name = "foreach1" + DateTime.Now
                };
                var forEach1As = forEach1.As<WfActivity>();

                ActivityTestHelper.AddExpressionToActivityArgument(wf, forEach1As, "List", "all([Workflow Run State]) where Name='Wibble wobble bob'");  // there shouldn't be any

                wf.FirstActivity = forEach1As;
                wf.ContainedActivities.Add(forEach1As);

                var loopExitPoint = Entity.Get<ExitPoint>(ForeachImplementation.LoopExitPointAlias);

                ActivityTestHelper.AddTransition(wf, forEach1As, forEach1As, loopExitPoint);
                ActivityTestHelper.AddTermination(wf, forEach1As);

                wf.Save();
                ToDelete.Add(wf.Id);

                var run = (RunWorkflow(wf));

                IDictionary<string, object> outputs = run.GetOutput();
            }
        }


        /// <summary>
        /// </summary>
        //[Test]
        [RunAsDefaultTenant]
        [Timeout(20000)]
        [RunWithTransaction]
        public void TestLoopOverNoResultsUsingUnsetListVar()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var wf = new Workflow();

                wf.AddDefaultExitPoint();

                var loopList = new ResourceListArgument() { Name = "loopList" };
                ActivityTestHelper.AddVariableToWorkflow(wf, loopList.As<ActivityArgument>());
                //ActivityHelper.AddExpressionToWorkflowVariable(wf, "list", "0");

                var forEach1 = new ForEachResource
                {
                    Name = "foreach1" + DateTime.Now
                };
                var forEach1As = forEach1.As<WfActivity>();

                ActivityTestHelper.AddExpressionToActivityArgument(wf, forEach1As, "List", "loopList");  // there shouldn't be any

                wf.FirstActivity = forEach1As;
                wf.ContainedActivities.Add(forEach1As);

                var loopExitPoint = Entity.Get<ExitPoint>(ForeachImplementation.LoopExitPointAlias);

                ActivityTestHelper.AddTransition(wf, forEach1As, forEach1As, loopExitPoint);
                ActivityTestHelper.AddTermination(wf, forEach1As);

                wf.Save();
                ToDelete.Add(wf.Id);

                var run = (RunWorkflow(wf));

                IDictionary<string, object> outputs = run.GetOutput();
            }
        }

        /// <summary>
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestLoopOverNoResultsFromGetRelationship()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var myType = new EntityType { Name = "MyType" };
                myType.Save();
                ToDelete.Add(myType.Id);

                var wf = new Workflow { Name = "TestLoopOverNoResultsFromGetRelationship" };

                wf.AddDefaultExitPoint();

                var getResources = new GetResourcesActivity { Name = "GetResources" };
                var getResourcesAs = getResources.As<WfActivity>();

                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, getResourcesAs, "Object", myType);   // this type has no instances

                wf.FirstActivity = getResourcesAs;
                wf.ContainedActivities.Add(getResourcesAs);

                var forEach1 = new ForEachResource
                {
                    Name = "foreach1" + DateTime.Now
                };
                var forEach1As = forEach1.As<WfActivity>();

                ActivityTestHelper.AddExpressionToActivityArgument(wf, forEach1As, "List", "[List]");  // there shouldn't be any

                wf.ContainedActivities.Add(forEach1As);

                ActivityTestHelper.AddTransition(wf, getResourcesAs, forEach1As);
                ActivityTestHelper.AddTransition(wf, forEach1As, forEach1As, "Loop");
                ActivityTestHelper.AddTermination(wf, forEach1As, "Finished");

                ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf);
                wf.Save();
                ToDelete.Add(wf.Id);

                var run = RunWorkflow(wf);

                IDictionary<string, object> outputs = run.GetOutput();
            }
        }

        /// <summary>
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void LogEmployeesAge()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var wf = new Workflow() { Name = "Log Employees Age " + DateTime.Now };

                wf.AddDefaultExitPoint();

                EntityType type = CodeNameResolver.GetTypeByName("AA_Employee").As<EntityType>();

                wf.AddVariable<ResourceListArgument>("list", "all(AA_Employee)", type)
                  .AddForEach("foreach", "list", type)
                  .AddLog("Log", "{{foreach_Record.Age}}", "foreach", "Loop")
                  .AddTransition("Log", "foreach");

                wf.Save();

                var wfInput = new Dictionary<string, object>();

                var run = (RunWorkflow(wf, wfInput));

                Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum,
                                "The workflow completed sucessfully.");
            }
        }
	}

    		
}