// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class EndToEndTests : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void PassingValuesBetweenActivities( )
		{
			// This test creates a person with an age of ten and a workflow that takes the persons resource id as inputs, reads that persons age and writes it out to the log embedded in a message.
			// Testing:
			//      Mapping workflow input arguments.
			//      Mapping an output argument to a variable.
			//      Using an expression that contains an input parameter.

            var personType = CodeNameResolver.GetTypeByName("AA_Person").As<EntityType>();
		    var ageField = personType.Fields.First(f => f.Name == "Age");

		    var peter = Entity.Create(personType).As<Resource>();
		    peter.Name = "Peter" + DateTime.Now;

		    peter.SetField(ageField, 10);
			peter.Save( );
			ToDelete.Add( peter.Id );

			var workflow = new Workflow
				{
					Name = "Wf" + DateTime.Now
				};

			workflow.AddDefaultExitPoint( );

			var resourceIdArg = new ResourceArgument
				{
					Name = "ResourceId",
                    ConformsToType = personType
				};
			var resourceIdArgAs = resourceIdArg.As<ActivityArgument>( );

			workflow.InputArguments.Add( resourceIdArg.As<ActivityArgument>( ) );
			//workflow.Save( );
			var workflowAs = workflow.As<WfActivity>( );


			
			// log activity
			var log = new LogActivity
				{
					Name = "log" + DateTime.Now
				};
			var logAs = log.As<WfActivity>( );
			workflow.ContainedActivities.Add( logAs );
            workflow.FirstActivity = logAs;

			ActivityTestHelper.AddExpressionToActivityArgument( workflow, logAs, "Message", "'Peters age is ' + ResourceId.Age", false );

			ActivityTestHelper.AddTermination( workflow, logAs );

			workflow.Save( );
			ToDelete.Add( workflow.Id );

			ActivityImplementationBase nextActivity = workflow.Cast<WfActivity>( ).CreateWindowsActivity( );

			var input = new Dictionary<string, object>
				{
					{
						"ResourceId", peter
					}
				};

            var run = (RunWorkflow(workflow, input));


            Assert.AreEqual(WorkflowRunState_Enumeration.WorkflowRunCompleted, run.WorkflowRunStatus_Enum, "The workflow run and completed without error");

			//RunActivity( nextActivity, input );
		}

	}
}