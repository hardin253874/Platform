// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
	public class WorkflowRunnerTest : TestBase
	{

		[Test]
		[RunAsDefaultTenant]
		public void WorkflowWithErrorsWillNotRun( )
		{
            Workflow workflow = CreateLoggingWorkflow( "{{count(resource(ABC, DEF).GHI)}}");

			workflow.WorkflowHasErrors = true;
			workflow.Save( );

			ToDelete.Add( workflow.Id );

            var run = (RunWorkflow(workflow, null));

            Assert.IsTrue(run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunFailed, "Run Failed as expected");
            Assert.IsTrue(run.ErrorLogEntry.Description.Contains("misconfigured"), "Has expected error message.");

		}
	
	}
}