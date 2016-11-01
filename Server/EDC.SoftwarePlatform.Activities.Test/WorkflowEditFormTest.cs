// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EditForm;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Common.Workflow;
//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    //[Ignore("Failing on first run in test environment.")]

	public class WorkflowEditformTest : TestBase
	{
        private static EntityType GetEntityType(string name)
        {
            return CodeNameResolver.GetTypeByName(name).As<EntityType>();
        }

        private static Field GetPersonField(string name)
        {
            var type = GetEntityType("AA_Person");
            var field = type.Fields.First(f => f.Name == name);
            Assert.That(field, Is.Not.Null, name + "field not found");
            return field;
        }

        private static Relationship GetTitleRel()
        {
            var type = GetEntityType("AA_Person");
            var rel = type.Relationships.First(r => r.Name == "Title");
            Assert.That(rel, Is.Not.Null, "Title relationship not found");
            return rel;
        }

	    private static Workflow CreateTestWorkflow( string name, string value )
		{
            var ageField = GetPersonField("Age");

			var wf = new Workflow
				{
                    Name = name
				};

            wf.AddDefaultExitPoint()
               .AddInput<ResourceArgument>("ResourceId")
               .AddUpdateField("Update Field", ageField.As<Resource>(), "ResourceId", value);

		    wf.InputArgumentForAction = wf.InputArguments.First();
            Assert.That(wf.InputArgumentForAction, Is.Not.Null, "InputArgumentForAction null");

			return wf;
		}

		private static EntityRef CreateResource( EntityRef typeRef )
		{
            using (new WorkflowRunContext(true))
            {
                var person = Entity.Create(typeRef).Cast<Resource>();

                person.Name = "FirstName";
                //person.LastName = "LastName";

                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    person.Save();

                    ctx.CommitTransaction();
                }

                return person;
            }
		}

		private static void UpdateEmployeeField( EntityRef employeeRef, Field field, object value )
		{
            using (new WorkflowRunContext(true))
            {
                var employee = Entity.Get(employeeRef, true);
                employee.SetField(field, value);

                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    employee.Save();

                    ctx.CommitTransaction();
                }
            }       
		}


		private static void UpdateEmployeeRelationship( EntityRef employeeRef, Relationship relToUpdate )
		{
            using (new WorkflowRunContext(true))
            {
                var employee = Entity.Get(employeeRef, true);
                var rels = employee.GetRelationships(relToUpdate);
                var otherThing = relToUpdate.ToType.InstancesOfType.First(); ;
                rels.Add(otherThing);
                employee.SetRelationships(relToUpdate, rels);

                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    employee.Save();

                    ctx.CommitTransaction();
                }
            }
		}

        /// <summary>
        /// Wait until the test returns true.
        /// </summary>
        private bool WaitUntil(Func<bool> test, int count, int pauseMs)
	    {
	        for (int i = 0; i < count; i++)
	        {
                Thread.Sleep(pauseMs);
	            if (test()) return true;
	        }

	        return false;
	    }
        

		[Test]
		[RunAsDefaultTenant]
        //[Ignore("Failing on first run in test environment.")]
		public void TestWorkflowRunOnCreate( )
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var ageField = GetPersonField("Age");
                var myType = CreateMyType("Type_TestWorkflowRunOnUpdate");

                Workflow wf = CreateTestWorkflow("TestWorkflowRunOnCreate" + DateTime.Now, "995");

                var trigger = new WfTriggerUserUpdatesResource
                {
                    WorkflowToRun = wf,
                    TriggeringCondition_Enum = TriggeredOnEnum_Enumeration.TriggeredOnEnumCreate,
                    TriggerEnabled = true
                };
                trigger.TriggeredOnType = myType;

                trigger.Save();
                ToDelete.Add(trigger.Id);
                ToDelete.Add(wf.Id);

                // Ensure an employee triggers the workflow
                EntityRef  employeeRef = CreateResource(myType);

                var savedEmployee = Entity.Get(employeeRef);
                var age1 = savedEmployee.GetField<int?>(ageField);

                Assert.That(age1, Is.EqualTo(995), "Ensure Age has not been set by workflow");

                EntityRef personRef = CreateResource(Person.Person_Type);

                var savedPerson = Entity.Get(personRef);
                var age2 = savedPerson.GetField<int?>(ageField);
                Assert.That(age2, Is.Null, "Ensure Age has not been set by workflow");
            }
		}

		[Test]
		[RunAsDefaultTenant]
       // [Ignore("Failing on first run in test environment.")]
        public void TestWorkflowRunOnRelationshipUpdate()
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var ageField = GetPersonField("Age");
                var titleRel = GetTitleRel();

                var myType = CreateMyType("Type_TestWorkflowRunOnUpdate");

                Workflow wf = CreateTestWorkflow("TestWorkflowRunOnRelationshipUpdate " + DateTime.Now, "990");

                var updateTrigger = new WfTriggerUserUpdatesResource
                {
                    WorkflowToRun = wf,
                    // ReSharper disable SpecifyACultureInStringConversionExplicitly
                    Name = "Test" + DateTime.Now.ToString(),
                    // ReSharper restore SpecifyACultureInStringConversionExplicitly
                    TriggeringCondition_Enum = TriggeredOnEnum_Enumeration.TriggeredOnEnumUpdate,
                    TriggerEnabled = true
                };


                updateTrigger.UpdatedRelationshipsToTriggerOn.Add(titleRel);
                updateTrigger.TriggeredOnType = myType;

                updateTrigger.Save();
                ToDelete.Add(updateTrigger.Id);
                ToDelete.Add(wf.Id);

                EntityRef employee = CreateResource(myType);

                var savedEmployee = Entity.Get(employee, true);
                var age = savedEmployee.GetField<int?>(ageField);
                Assert.AreEqual(null, age, "Age has been not been set by workflow");

                UpdateEmployeeRelationship(savedEmployee.Id, titleRel);

                savedEmployee = Entity.Get(employee);
                var age1 = savedEmployee.GetField<int?>(ageField);
                Assert.That(age1, Is.EqualTo(990), "The update has not occurred and has not triggered the workflow.");
            }
		}


        [Test]
        [RunAsDefaultTenant]
        // [Ignore("Failing on first run in test environment.")]
        public void TestWorkflowRunOnReverseRelationshipUpdate()
        {
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var ageField = GetPersonField("Age");
                var titleRel = GetTitleRel();

                var myType = CreateMyType("Type_TestWorkflowRunOnUpdate");

                Workflow wf = CreateTestWorkflow("TestWorkflowRunOnRelationshipUpdate " + DateTime.Now, "990");

                var updateTrigger = new WfTriggerUserUpdatesResource
                {
                    WorkflowToRun = wf,
                    // ReSharper disable SpecifyACultureInStringConversionExplicitly
                    Name = "Test" + DateTime.Now.ToString(),
                    // ReSharper restore SpecifyACultureInStringConversionExplicitly
                    TriggeringCondition_Enum = TriggeredOnEnum_Enumeration.TriggeredOnEnumUpdate,
                    TriggerEnabled = true
                };

                var task = new DisplayFormUserTask();

                var relToUpdate = Entity.Get<Relationship>("core:recordToPresent");
                updateTrigger.UpdatedRelationshipsToTriggerOn.Add(relToUpdate);         // a reverse relationship
                updateTrigger.TriggeredOnType = myType;

                updateTrigger.Save();
                ToDelete.Add(updateTrigger.Id);
                ToDelete.Add(wf.Id);

                EntityRef employee = CreateResource(myType);

                var savedEmployee = Entity.Get(employee);
                var age = savedEmployee.GetField<int?>(ageField);
                Assert.AreEqual(null, age, "Age has been not been set by workflow");

                var rels = savedEmployee.GetRelationships(relToUpdate);

                rels.Add(task);

                using (new WorkflowRunContext(true))
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    savedEmployee.Save();

                    ctx.CommitTransaction();
                }

                savedEmployee = Entity.Get(employee);
                var age1 = savedEmployee.GetField<int?>(ageField);
                Assert.That(age1, Is.EqualTo(990), "The update has not occurred and has not triggered the workflow.");
            }
        }


        EntityType CreateMyType(string name)
        {
            var myType = new EntityType() { Name = name };
            var empType = GetEntityType("AA_Employee");
            myType.Inherits.Add(empType);
            myType.Save();
            ToDelete.Add(myType.Id);
            return myType;
        }


		[Test]
		[RunAsDefaultTenant]
        public void TestWorkflowRunOnUpdate()
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var emailField = GetPersonField("Email Address");
                var ageField = GetPersonField("Age");
                var fnField = GetPersonField("First Name");
                var lnField = GetPersonField("Last Name");

                var myType = CreateMyType("Type_TestWorkflowRunOnUpdate");

                Workflow wf = CreateTestWorkflow("TestWorkflowRunOnUpdate" + DateTime.Now, "991");

                var updateTrigger = new WfTriggerUserUpdatesResource
                {
                    WorkflowToRun = wf,
                    // ReSharper disable SpecifyACultureInStringConversionExplicitly
                    Name = "Test" + DateTime.Now.ToString(),
                    // ReSharper restore SpecifyACultureInStringConversionExplicitly
                    TriggeringCondition_Enum = TriggeredOnEnum_Enumeration.TriggeredOnEnumUpdate,
                    TriggerEnabled = true
                };

                updateTrigger.UpdatedFieldsToTriggerOn.Add(emailField.As<Field>());
                updateTrigger.TriggeredOnType = myType;

                updateTrigger.Save();
                ToDelete.Add(updateTrigger.Id);
                ToDelete.Add(wf.Id);

                // Ensure an employee triggers the workflow
                var employee = Entity.Create(myType).Cast<Resource>();
                employee.SetField(fnField, "Test Firstname" + DateTime.Now);
                employee.SetField(lnField, "Test Lastname" + DateTime.Now);
                employee.SetField(ageField, 1);

                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    employee.Save();

                    ctx.CommitTransaction();
                }

                ToDelete.Add(employee.Id);

                //Thread.Sleep(2000); // Give the workflow time to finish if it started

                var savedEmployee = Entity.Get(employee);
                var age = savedEmployee.GetField<int?>(ageField);

                Assert.AreEqual(1, age, "Age has been not been set by workflow");

                UpdateEmployeeField(savedEmployee.Id, ageField, 10);

                //Thread.Sleep(2000); // Give the workflow time to finish if it started

                savedEmployee = Entity.Get(savedEmployee.Id);
                age = savedEmployee.GetField<int?>(ageField);
                Assert.AreEqual(10, age, "The update has occurred but has not triggered the workflow.");

                UpdateEmployeeField(savedEmployee.Id, emailField, "test@edc.com");

                savedEmployee = Entity.Get(employee);
                age = savedEmployee.GetField<int?>(ageField);
                Assert.That(age, Is.EqualTo(991), "The update has occurred and has triggered the workflow.");
            }
		}
       


        [Test]
        [RunAsDefaultTenant]
        [Explicit]               
        public void TestForceDeadlockOnWorkflowRunOnCreate()
        {
            var ageField = GetPersonField("Age");
            Workflow wf = CreateTestWorkflow("TestForceDeadlockOnWorkflowRunOnCreate" + DateTime.Now, "999");
            var empType = new EntityType();
            empType.Name = "MyType" + DateTime.Now.Ticks;
            empType.Inherits.Add(GetEntityType("AA_Employee"));
            empType.Save();
            ToDelete.Add(empType.Id);
            

            var trigger = new WfTriggerUserUpdatesResource
            {
                WorkflowToRun = wf,
                TriggeringCondition_Enum = TriggeredOnEnum_Enumeration.TriggeredOnEnumCreate,
                TriggerEnabled = true
            };
            trigger.TriggeredOnType = empType;

            trigger.Save();
            ToDelete.Add(trigger.Id);
            ToDelete.Add(wf.Id);

            EntityRef employeeRef = null;

            for (var i = 0; i < 20; i++)
            {
                // Ensure an employee triggers the workflow
                employeeRef = CreateResource(empType);
                ToDelete.Add(employeeRef.Id);
            }

            if (!WaitUntil(() =>
            {
                var savedEmployee = Entity.Get(employeeRef);
                var age = savedEmployee.GetField<int?>(ageField);
                return age == 999;
            }, 50, 200))
            {
                Assert.Fail("Age has been set by workflow");
            }

        }

	}
}