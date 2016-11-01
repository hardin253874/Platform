// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;

namespace EDC.SoftwarePlatform.Activities.Test
{
	public class WorkflowVariableTests : TestBase
	{
        //[Test]
        //[RunAsDefaultTenant]
        //public void EmptyResourceVarTest( )
        //{
        //    Workflow workflow = CreateLoggingWorkflow("Empty resource {{TestResourceArg}}");


        //    var resArg = new ResourceArgument
        //        {
        //            Name = "TestResourceArg"
        //        };

        //    ActivityHelper.AddVariableToWorkflow( workflow, resArg.Cast<ActivityArgument>( ) );

        //    workflow.Save( );

        //    ToDelete.Add( workflow.Id );

        //    var run = WorkflowRunner.Instance.RunWorkflow(workflow, null);
        //    Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunFailed, run.WorkflowRunStatus_Enum , "Run Exprected to fail");

        //}

		[Test]
		[RunAsDefaultTenant]
        [RunWithTransaction]
		public void ResourceVarWithEntityExpressionTest( )
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                Workflow workflow = CreateLoggingWorkflow("The workflow is {{TestResourceArg}}");


                var resArg = new ResourceArgument
                {
                    Name = "TestResourceArg"
                };

                ActivityTestHelper.AddVariableToWorkflow(workflow, resArg.Cast<ActivityArgument>());

                workflow.AddEntityExpressionToVariable("TestResourceArg", workflow);

                workflow.Save();

                ToDelete.Add(workflow.Id);

                var run = (RunWorkflow(workflow));

                Assert.IsTrue(run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted, "Run Completed");
            }
		}

		[Test]
		[RunAsDefaultTenant]
        [RunWithTransaction]
		public void SetResourceVariableBug( )
		{
            // This is a bug were the SetVariable activity when setting a resource variable evaluates it as an objectVariable
            // because the evaluateExpression uses the target to determine type which in this case is the inputArgument, not the target of the
            // set.

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var workflow = new Workflow
                {
                    Name = "Wf" + DateTime.Now
                };

                workflow
                    .AddDefaultExitPoint()
                    .AddVariable<ResourceArgument>("originArg")
                    .AddVariable<ResourceArgument>("targetArg")
                    .AddOutput<StringArgument>("v1Out")
                    .AddAssignToVar("Assign", "[originArg]", "targetArg")
                    .AddAssignToVar("Out 1", "'output is ' + targetArg", "v1Out")
                    .AddEntityExpressionToVariable("originArg", new EntityRef("core:person"))
                    .Save();


                var run = (RunWorkflow(workflow));

                Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted));

                IDictionary<string, object> outputs = run.GetOutput();

                Assert.IsTrue(outputs.ContainsKey("v1Out"), "Output has the result.");
                Assert.AreEqual("output is Person", outputs["v1Out"], "Ensure the variable was updated and sent to the output");
            }
		}

		[Test]
		[RunAsDefaultTenant]
        [RunWithTransaction]
		public void SetVariableTest( )
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var workflow = new Workflow
                {
                    Name = "Wf" + DateTime.Now
                };

                workflow.AddDefaultExitPoint();

                var v1 = Entity.Create<IntegerArgument>().As<ActivityArgument>();
                v1.Name = "v1";
                ActivityTestHelper.AddVariableToWorkflow(workflow, v1);

                var v2 = Entity.Create<ResourceArgument>().As<ActivityArgument>();
                v2.Name = "v2";
                ActivityTestHelper.AddVariableToWorkflow(workflow, v2);

                var v3 = Entity.Create<ResourceListArgument>().As<ActivityArgument>();
                v3.Name = "v3";
                ActivityTestHelper.AddVariableToWorkflow(workflow, v3);

                // set starting values
                workflow.AddExpressionToWorkflowVariable("v1", "111", false);
                workflow.AddEntityExpressionToVariable("v2", new EntityRef("core:person"));
                //ActivityHelper.AddExpressionToWorkflowVariable(workflow, "v3", "[Resource Type]", false);

                // assign 222 to v1
                var setV1 = Entity.Create<AssignToVariable>();
                var setV1As = setV1.Cast<WfActivity>();
                setV1.TargetVariable = v1;
                ActivityTestHelper.AddExpressionToActivityArgument(workflow, setV1As, "Value", "222", false);
                workflow.FirstActivity = setV1As;
                workflow.ContainedActivities.Add(setV1As);
                ActivityTestHelper.AddTermination(workflow, setV1As);
                // output
                var v1Out = Entity.Create<StringArgument>();
                var v1OutAs = v1Out.Cast<ActivityArgument>();
                v1Out.Name = "v1Out";

                workflow.OutputArguments.Add(v1OutAs);

                ActivityTestHelper.AddAssignToVar(workflow, "set out", "'output is ' + v1", "v1Out");
                ActivityTestHelper.AddMissingExpressionParametersToWorkflow(workflow);

                workflow.Save();

                ToDelete.Add(workflow.Id);

                var run = (RunWorkflow(workflow));

                Assert.IsTrue(run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted, "Run Completed");


                IDictionary<string, object> outputs = run.GetOutput();

                Assert.IsTrue(outputs.ContainsKey("v1Out"), "Output has the result.");
                Assert.AreEqual("output is 222", outputs["v1Out"], "Ensure the variable was updated and sent to the output");
            }

		}
	}
}