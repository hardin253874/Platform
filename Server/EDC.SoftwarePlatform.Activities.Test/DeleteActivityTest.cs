// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class DeleteActivityTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void DeleteActivityRun( )
		{
			var person = new Person( );
			person.Save( );
			ToDelete.Add( person.Id );

			var deleteActivity = new DeleteActivity( );
			deleteActivity.Save( );
			ToDelete.Add( deleteActivity.Id );

			var nextActivity = ( DeleteImplementation ) deleteActivity.As<WfActivity>( ).CreateWindowsActivity( );

			var inputs = new Dictionary<string, object>
				{
					{
						"Record", person
					}
				};

			RunActivity( nextActivity, inputs );

            Assert.That(Entity.Exists(person.Id), Is.False);
		}

	    [Test]
	    [RunAsDefaultTenant]
	    public void DeleteActivityRunInWf()
	    {
            var wf = new Workflow
            {
                Name = "DeleteActivityRunInWf"
            };

            wf.AddDefaultExitPoint()
               .AddInput<ResourceArgument>("ResourceId")
               .AddDelete("Delete", "ResourceId");

            wf.InputArgumentForAction = wf.InputArguments.First();

	        wf.Save();
	        ToDelete.Add(wf.Id);

	        var person = new Person();
            person.Save();

	        var personId = person.Id;

	        var args = new Dictionary<string, object>() {{"ResourceId", person}};
	        
            var run = (RunWorkflow(wf, args));

            Assert.IsTrue(run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted, "Run Completed");
            
            Assert.IsNull(Entity.Get(personId), "Ensure person is deleted");
	    }
	}
}