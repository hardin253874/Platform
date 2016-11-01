// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class ResourceListArgumentTest : TestBase
	{
		/// <summary>
		///     Ensure that a resource list can be passed into a workflow and come out with the same contents.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void InOutWorkflow( )
		{
            var wf = new Workflow();
            wf
                .AddInput<ResourceListArgument>("in")
                .AddOutput<ResourceListArgument>("out")
                .AddDefaultExitPoint()
                .AddAssignToVar("Set output", "in", "out")
                .Save( );

            var bob = new Person() { Name = "bob" };
            var john = new Person() { Name = "john" };

            bob.Save();
		    john.Save();
		    ToDelete.Add(bob.Id);
            ToDelete.Add(john.Id);

			var wfInput = new Dictionary<string, object>
				{
					{
						"in", new List<IEntity>
							{
								bob,
								john
							}
					}
				};

            var run = (RunWorkflow(wf, wfInput));

			IDictionary<string, object> outputs = run.GetOutput();

			Assert.AreEqual( 1, outputs.Count, "There is one output argument" );

            var result = (IEnumerable<IEntity>)outputs["out"];

            var sIn = string.Join(",", ((IEnumerable<IEntity>)wfInput["in"]).Select(e => e.Id));
            var sResult = string.Join(",", result.Select(e => e.Id));


            Assert.AreEqual( sIn, sResult, "Output is the same as input.");

			Assert.AreEqual( 2, result.Count(), "There are two results" );
		}
	}
}