// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;
using System;
using EDC.ReadiNow.Database;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    public class CreateActivityTest : TestBase
    {
        [Test]
        [RunAsDefaultTenant]
        public void CreateActivityRun()
        {
            var personType = CodeNameResolver.GetTypeByName("AA_Person").As<EntityType>();

            const string createdRecordArgKey = "Record";

            var createActivity = new CreateActivity();
            createActivity.Save();
            ToDelete.Add(createActivity.Id);

            var nextActivity = (CreateImplementation)createActivity.As<WfActivity>().CreateWindowsActivity();

            var inputs = new Dictionary<string, object>
				{
					{
						"Object", personType
					}
				};

            IDictionary<string, object> result = RunActivity(nextActivity, inputs);

            Assert.AreEqual(2, result.Count, "There are two results, and exit point + a value");

            Assert.IsTrue(result.ContainsKey(createdRecordArgKey));

            var resourceId = (IEntity)result[createdRecordArgKey];

            ToDelete.Add(resourceId.Id);

            var person = Entity.Get(resourceId.Id);

            Assert.IsNotNull(person, "We have a person");
        }

       


        Entity CreateEmployee(Entity oldMgr, Relationship reportsTo)
        {
            var aaEmployee = CodeNameResolver.GetTypeByName("AA_Employee").As<EntityType>();
            
            var employee = new Entity(aaEmployee);
            employee.Save();
            var rel = employee.GetRelationships(reportsTo);
            rel.Add(oldMgr);
            employee.Save();
            ToDelete.Add(employee.Id);

            return employee;
        }

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(WorkflowRunException))]
        //[Ignore("This test is the same as the failing intg workflow tests '610' and '620' due to the enforcement of cardinality. The problem is that the assignment of a relationship is not clearing the old relationship if  the cardinality does not permit it.")]
        public void ReassignReportDuringManagerCreate()
        {
            var aaManager = CodeNameResolver.GetTypeByName("AA_Manager").As<EntityType>();
            var reportsTo = Entity.Get<Relationship>(new EntityRef("test:reportsTo"));
            
            var oldMgr = new Entity(aaManager);
            oldMgr.Save();
            ToDelete.Add(oldMgr.Id);

           var employee1 = CreateEmployee(oldMgr, reportsTo);
           var employee2 = CreateEmployee(oldMgr, reportsTo);

            var createActivity = new CreateActivity();
            createActivity.InputArguments.Add(new ResourceArgument { Name = "1" }.Cast<ActivityArgument>());
            createActivity.InputArguments.Add(new ResourceArgument { Name = "1_value_" }.Cast<ActivityArgument>());
            createActivity.InputArguments.Add(new BoolArgument { Name = "1_reverse" }.Cast<ActivityArgument>());
            createActivity.InputArguments.Add(new BoolArgument { Name = "1_replace" }.Cast<ActivityArgument>());

            createActivity.Save();
            ToDelete.Add(createActivity.Id);

            var nextActivity = (CreateImplementation)createActivity.As<WfActivity>().CreateWindowsActivity();


            var inputs = new Dictionary<string, object>
				{
					{
						"Object", aaManager
					},
                    {
                        "1_value_", new List<Entity>() {employee1, employee2}
                    },
                    {
                        "1", reportsTo
                    },
                    {
                        "1_reverse", true
                    },
                                        {
                        "1_replace", true
                    }
				};


            RunActivity(nextActivity, inputs);

        }

        [Test]
        [RunAsDefaultTenant]
        public void Bug_25770_AssigningWithNull()
        {
            var aaManager = CodeNameResolver.GetTypeByName("AA_Manager").As<EntityType>();
            var reportsTo = Entity.Get<Relationship>(new EntityRef("test:reportsTo"));

          
            var createActivity = new CreateActivity();
            createActivity.InputArguments.Add(new ResourceArgument { Name = "1" }.Cast<ActivityArgument>());
            createActivity.InputArguments.Add(new ResourceArgument { Name = "1_value_" }.Cast<ActivityArgument>());
            createActivity.InputArguments.Add(new BoolArgument { Name = "1_reverse" }.Cast<ActivityArgument>());
            createActivity.InputArguments.Add(new BoolArgument { Name = "1_replace" }.Cast<ActivityArgument>());

            createActivity.Save();
            ToDelete.Add(createActivity.Id);

            var nextActivity = (CreateImplementation)createActivity.As<WfActivity>().CreateWindowsActivity();


            var inputs = new Dictionary<string, object>
				{
					{
						"Object", aaManager
					},
                    {
                        "1_value_", new List<Entity>() {null}           // NULL (This was a side effect of a problem in the expression engine)
                    },
                    {
                        "1", reportsTo
                    },
                    {
                        "1_reverse", true
                    },
                                        {
                        "1_replace", true
                    }
				};


            RunActivity(nextActivity, inputs);
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void PerformCreate()
        {
            var reportsToRef = Entity.Get<Relationship>(new EntityRef("test:reportsTo"));
            var personType = Entity.Get<Definition>(new EntityRef("test:employee"));

            var mgr = Entity.Create(new EntityRef("test:manager"));

            Action<IEntity> updateAction = (wfr) => wfr.SetRelationships(reportsToRef, new EntityRelationshipCollection<IEntity> { mgr });

            CreateImplementation.PerformCreate(personType, updateAction);
        }

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(CreateImplementation.CreateInvalidReferenceException))]
        public void PerformCreateWithRefToInvalidResource()
        {
            using (var ctx = DatabaseContext.GetContext(true))
            {
                var mgr1 = Entity.Create(new EntityRef("test:manager"));
                mgr1.Save();
                Entity.Delete(mgr1.Id);

                var reportsToRef = Entity.Get<Relationship>(new EntityRef("test:reportsTo"));
                var personType = Entity.Get<Definition>(new EntityRef("test:employee"));

                Action<IEntity> updateAction = (wfr) => wfr.SetRelationships(reportsToRef, new EntityRelationshipCollection<IEntity> { mgr1 });

                CreateImplementation.PerformCreate(personType, updateAction);
            }

        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void PerformCreateWithDefault()
        {
            var myField = new StringField { Name = "myStringField", DefaultValue = "bob" }.As<Field>();
            var myType = new Definition { Name = "MyType", Fields = { myField } };
            myType.Save();


            var mgr = Entity.Create(new EntityRef("test:manager"));

            IEntity newEntity = null;

            // Make sure the defaults are getting set
            Action<IEntity> updateAction = (wfr) => { newEntity = wfr; };

            CreateImplementation.PerformCreate(myType, updateAction);

            Assert.That(newEntity.GetField<string>(myField), Is.EqualTo("bob"));

            // Make sure that the default can be overridden
            Action<IEntity> overrideUpdateAction = (wfr) => { newEntity = wfr; wfr.SetField(myField, "jane"); };

            CreateImplementation.PerformCreate(myType, overrideUpdateAction);

            Assert.That(newEntity.GetField<string>(myField), Is.EqualTo("jane"));
        }
    }
}