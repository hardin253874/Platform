// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class SwitchActivityTest : TestBase
	{
		private void TestResult( Workflow wf, string inputString, ExitPoint expectedExit )
		{
			var input = new Dictionary<string, object>
				{
					{
						"wfInString", inputString
					}
				};

            var run = (RunWorkflow(wf, input, null, true));

            var exitPoint = run.GetExitPoint();

			Assert.AreEqual(  exitPoint.Id, expectedExit.Id, string.Format( "Ensure input of '{0}' should result in a '{1}' decision", inputString, expectedExit.Name ) );
		}

		[Test]
		[RunAsDefaultTenant]
		public void SwitchTest( )
		{
			var wf = new Workflow
				{
					Name = "ConnectingParameters Wf"
				};
			var wfAs = wf.Cast<WfActivity>( );

			wf.InputArguments.Add( ( new StringArgument
				{
					Name = "wfInString"
				} ).Cast<ActivityArgument>( ) );

			var wfYesExit = new ExitPoint
				{
					IsDefaultExitPoint = true,
					Name = "Wf Yes"
				};
			var wfNoExit = new ExitPoint
				{
					Name = "Wf No"
				};
			var wfMaybeExit = new ExitPoint
				{
					Name = "Wf Maybe"
				};
			var wfDontKnowExit = new ExitPoint
				{
					Name = "Wf Dont know"
				};

			wf.ExitPoints.Add( wfYesExit );
			wf.ExitPoints.Add( wfNoExit );
			wf.ExitPoints.Add( wfMaybeExit );
			wf.ExitPoints.Add( wfDontKnowExit );

			var switch1 = new SwitchActivity
				{
					Name = "Switch1"
				};
			var switch1As = switch1.Cast<WfActivity>( );

			ActivityTestHelper.AddFirstActivityWithMapping(
				wf,
				switch1As,
				null );

			ActivityTestHelper.CreateArgumentInstance( wf, wfAs, wfAs.GetInputArgument( "wfInString" ) );
			ActivityTestHelper.AddExpressionToActivityArgument( wf, switch1As, "Value to Switch On", "wfInString", false );

			var switchYesExit = new ExitPoint
				{
					IsDefaultExitPoint = true,
					Name = "Yes"
				};
			var switchNoExit = new ExitPoint
				{
					Name = "No"
				};
			var switchMaybeExit = new ExitPoint
				{
					Name = "Maybe"
				};

			switch1.ExitPoints.Add( switchYesExit );
			switch1.ExitPoints.Add( switchNoExit );
			switch1.ExitPoints.Add( switchMaybeExit );

			ActivityTestHelper.AddTermination(
                wf,
                switch1As,
                switchYesExit,
                wfYesExit);

			ActivityTestHelper.AddTermination(
                wf,
                switch1As,
                switchNoExit,
                wfNoExit);

			ActivityTestHelper.AddTermination(
                wf,
                switch1As,
                switchMaybeExit,
                wfMaybeExit);

			var otherwiseExit = Entity.Get<ExitPoint>( new EntityRef( "core", "switchActivityOtherwiseExitPoint" ) );
			ActivityTestHelper.AddTermination(
                wf,
                switch1As,
                otherwiseExit,
                wfDontKnowExit);

			ActivityTestHelper.AddMissingExpressionParametersToWorkflow( wf );

			wf.Save( );
			ToDelete.Add( wf.Id );
			ToDelete.Add( switch1.Id );

            TestResult(wf, "Yes", wfYesExit);
            TestResult(wf, "No", wfNoExit);
            TestResult(wf, "Maybe", wfMaybeExit);
            TestResult(wf, "Something else", wfDontKnowExit);
		}
	}
}