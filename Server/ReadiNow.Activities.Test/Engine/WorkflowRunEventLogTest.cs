// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
	[TestFixture]
	public class WorkflowRunEventLogTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
        [Ignore("This test needs to be rewritten")]
		public void EnsureBadExpressionGeneratesEventLog( )
		{
            Workflow wf = CreateLoggingWorkflow("{{EnsureBadExpressionGeneratesEventLog - This is an expected error in the log}}");

			wf.Save( );
			ToDelete.Add( wf.Id );

			ActivityImplementationBase nextActivity = wf.Cast<WfActivity>( ).CreateWindowsActivity( );

			var input = new Dictionary<string, object>( );

            var run = RunWorkflow(wf);

            Assert.IsNotNull(run.ErrorLogEntry, "A log entry exists");
        }
	}
}