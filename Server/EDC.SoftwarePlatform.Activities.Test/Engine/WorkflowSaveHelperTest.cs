// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
	[TestFixture]
	public class WorkflowSaveHelperTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void EnsureWorkflowCanSaveTwice( )
		{
            Workflow wf = CreateLoggingWorkflow("EnsureWorkflowCanSaveTwice");

			wf.Save( );
			ToDelete.Add( wf.Id );

		    var wf2 = Entity.Get<Workflow>(wf.Id, true);
		    wf2.Save();

            var wf3 = Entity.Get<Workflow>(wf.Id, true);

		    ActivityTestHelper.AddVariableToWorkflow(wf3, (new StringArgument() {Name="bob"}).As<ActivityArgument>());
            ActivityTestHelper.AddMissingExpressionParametersToWorkflow(wf3);
            wf3.Save();
        }
	}
}