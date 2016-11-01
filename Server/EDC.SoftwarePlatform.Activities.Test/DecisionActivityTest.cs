// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;

//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class DecisionActivityTest : TestBase
	{
        [Test]
        [RunAsDefaultTenant]
        public void DecisionTest()
        {
            var wf = new Workflow
            {
                Name = "ConnectingParameters Wf"
            };
            var wfAs = wf.Cast<WfActivity>();

            wf.InputArguments.Add((new IntegerArgument
            {
                Name = "wfInInt"
            }).Cast<ActivityArgument>());

            var yesExit = new ExitPoint
            {
                IsDefaultExitPoint = true,
                Name = "Yes"
            };
            var noExit = new ExitPoint
            {
                Name = "No"
            };

            wf.ExitPoints.Add(yesExit);
            wf.ExitPoints.Add(noExit);

            var decision1 = new DecisionActivity
            {
                Name = "Decision1"
            };
            var decision1As = decision1.Cast<WfActivity>();


            ActivityTestHelper.AddFirstActivityWithMapping(
                wf,
                decision1As,
                null);

            string wfInIntSubString = ActivityTestHelper.CreateArgumentInstance(wf, wfAs, wfAs.GetInputArgument("wfInInt"));
            ActivityTestHelper.AddExpressionToActivityArgument(wf, decision1As, "DecisionArgument", wfInIntSubString + " > 5", false);

            ActivityTestHelper.AddTermination(
                wf,
                decision1As,
                Entity.Get<ExitPoint>(new EntityRef("core", "decisionActivityYesExitPoint")),
                yesExit);

            ActivityTestHelper.AddTermination(
                wf,
                decision1As,
                Entity.Get<ExitPoint>(new EntityRef("core", "decisionActivityNoExitPoint")),
                noExit);

            ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf);

            wf.Save();
            ToDelete.Add(wf.Id);
            ToDelete.Add(decision1.Id);

            var input = new Dictionary<string, object>
            {
                {
                    "wfInInt", 10
                }
            };

            var run = RunWorkflow(wf, input);

            Assert.AreEqual(run.GetExitPoint().Id, yesExit.Id, "Input of '10' should result in a 'yes' decision");

            input = new Dictionary<string, object>
            {
                {
                    "wfInInt", 1
                }
            };

            run = RunWorkflow(wf, input);

            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "Finished without errors");

            Assert.AreEqual(noExit.Id, run.GetExitPoint().Id, "Input of '1' should results in a 'no' decision");
        }
	}
}