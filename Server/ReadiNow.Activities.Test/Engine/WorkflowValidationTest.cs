// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
    public class WorkflowValidationTest : TestBase
    {
       
        private Workflow CreateTestWf()
        {
            var person = CodeNameResolver.GetTypeByName("AA_Person").As<EntityType>();
            var ageField = person.Fields.First(f => f.Name == "Age");

            var wf = new Workflow
                {
                    Name = "Test"
                };

            wf.AddDefaultExitPoint()
              .AddInput<ResourceArgument>("ResourceId")
              .AddUpdateField("Update Field", ageField.As<Resource>(), "ResourceId", "13");

            wf.InputArgumentForAction = wf.InputArguments.First();

            wf.Save();
            ToDelete.Add(wf.Id);
            return wf;
        }

        [Test]
        [RunAsDefaultTenant]
        public void NoErrorsTest()
        {
			var wf = CreateTestWf();

            var messages = wf.Validate();
            Assert.AreEqual(0, messages.Count(), "There ahould be no errors.");

        }



        [Test]
        [RunAsDefaultTenant]
        public void EnsureMandatoryArgsTested_Test()
        {
            var wf = CreateTestWf();
            
            // remove the mandatory input
            Entity.Delete(wf.FirstActivity.ExpressionMap.Select(e => e.Id));
            wf.Save();
            
            var messages = wf.Validate().ToList();
            Assert.AreEqual(1, messages.Count(), "There should be one error.");
            Assert.IsTrue(messages.First().ToLower().Contains("mandatory"), "The message should contain the word 'mandatory'.");
        }
        
      

    }
}