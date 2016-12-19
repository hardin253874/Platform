// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Bugs
{
    [TestFixture]
    public class Bug_23913_workflowClone : TestBase
    {
        [Test]
        [RunAsDefaultTenant, RunWithTransaction]
        //[Ignore("Has not been fixed yet")]
        [Category("WorkflowTests"), Category("Bug")]
        //[Category( "ExtendedTests" )]
        public void Test()
        {
            var wf = Entity
                .Create<Workflow>()
                .AddDefaultExitPoint()
                .AddVariable<StringArgument>("name", "[input].Name")
                .AddInput<ResourceArgument>("input", Person.Person_Type)
                .AddDisplayForm("Display Form", new string[]{"Exit1"}, null, null, "[input]")
                .AddUpdateField("Update Field", UserAccount.Name_Field.As<Resource>(), "[input]", "[input].Name + 'xxx'");

            wf.Name = "UNNUMBERED_resumeMissingInput " + DateTime.Now;

            wf.Save();

            Assert.That(wf.Validate(), Is.Empty);

            var clone = wf.Clone<Workflow>();
            clone.Save();

            Assert.That(clone.Validate(), Is.Empty);

            Assert.That(clone.FirstActivity.Id, Is.Not.EqualTo(wf.FirstActivity.Id));
            Assert.That(clone.FirstActivity.ForwardTransitions.First().Id, Is.Not.EqualTo(wf.FirstActivity.ForwardTransitions.First().Id));

            wf.Delete();

            clone = Entity.Get<Workflow>(clone.Id);

            var validationMessages = clone.Validate();
            Assert.That(validationMessages, Is.Empty);
        }
    }
}
