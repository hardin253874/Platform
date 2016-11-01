// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Applications
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class CompileWorkflowsInAppliocation
    {
   
        [Test]
        [Category("AppTests")]
        [Explicit]
        public void CompileWorkflows()
        {
            var wfs = Entity.GetInstancesOfType<Workflow>();

            var errors = new List<string>();

            Console.Write("Compiling");

            foreach (var wf in wfs)
            {
                Console.Write(".");
                var metadata = new WorkflowMetadata(wf);

                if (metadata.HasViolations)
                {
                    errors.Add(wf.Name);
                    Console.WriteLine("\nErrors in '{0}':{1}\n", wf.Name, string.Join("\n\t", metadata.ValidationMessages));
                }
            }

            Assert.That(errors, Is.Empty, "All workflows should be valid.");
        }
    }
}
