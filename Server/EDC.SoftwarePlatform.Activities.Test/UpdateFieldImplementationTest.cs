// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using EDC.ReadiNow.Database;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    public class UpdateFieldImplementationTest : TestBase
    {
        [Test]
        [RunAsDefaultTenant]
        public void UpdateField()
        {
            var employeeType = CodeNameResolver.GetTypeByName("AA_Person").As<EntityType>();
            var ageField = employeeType.Fields.First(f => f.Name == "Age");
            var bob = new Entity(employeeType);
            bob.Save();
            ToDelete.Add(bob.Id);

            var updateActivity = new UpdateFieldActivity();

            updateActivity.InputArguments.Add(new ResourceArgument {Name = "1"}.Cast<ActivityArgument>());
            updateActivity.InputArguments.Add(new IntegerArgument { Name = "1_value" }.Cast<ActivityArgument>());

            updateActivity.Save();
            ToDelete.Add(updateActivity.Id);

            var updateActionAs = updateActivity.As<WfActivity>();

            ActivityImplementationBase nextActivity = updateActionAs.CreateWindowsActivity();

            var args = new Dictionary<string, object>
                {
                    {
                        "Record", bob
                    },
                    {
                        "1_value", 32
                    },
                    {
                        "1", (new EntityRef(ageField.Id)).Entity
                    }
                };

            RunActivity(nextActivity, args);

            Entity.Get(bob);
            var age = (int?)bob.GetField(ageField);

            Assert.AreEqual(32, age);
        }
    
    

        [Test]
        [RunAsDefaultTenant]
        public void UpdateRel()
        {
            var employeeType = Entity.Get<EntityType>("test:employee");
            var managerType = Entity.Get<EntityType>("test:manager");
            var reportsToRel = Entity.Get<Relationship>("test:reportsTo");

            var bob = new Entity(employeeType);
            var bobManager = new Entity(employeeType);
            bob.Save();
            bobManager.Save();
            ToDelete.Add(bob.Id);
            ToDelete.Add(bobManager.Id);

            var updateActivity = new UpdateFieldActivity();

            updateActivity.InputArguments.Add(new ResourceArgument {Name = "1"}.Cast<ActivityArgument>());
            updateActivity.InputArguments.Add(new ResourceArgument { Name = "1_value_" }.Cast<ActivityArgument>());
            updateActivity.InputArguments.Add(new BoolArgument { Name = "1_reverse" }.Cast<ActivityArgument>());
            updateActivity.InputArguments.Add(new BoolArgument { Name = "1_replace" }.Cast<ActivityArgument>());

            updateActivity.Save();
            ToDelete.Add(updateActivity.Id);

            var updateActionAs = updateActivity.As<WfActivity>();

            ActivityImplementationBase nextActivity = updateActionAs.CreateWindowsActivity();

            var args = new Dictionary<string, object>
                {
                    {
                        "Record", bob
                    },
                    {
                        "1_value_", bobManager
                    },
                    {
                        "1", reportsToRel
                    },
                    {
                        "1_reverse", false
                    }
                };

            RunActivity(nextActivity, args);

            var bob2 = Entity.Get(bob);
            var reportsTo = bob2.GetRelationships(reportsToRel);

            Assert.AreEqual(1, reportsTo.Count(), "Relationship set");
            Assert.AreEqual(bobManager.Id, reportsTo.First().Entity.Id, "Manager is correct");
        }


        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void PerformUpdate()
        {
            var reportsToRef = Entity.Get<Relationship>(new EntityRef("test:reportsTo"));
            var personType = Entity.Get<Definition>(new EntityRef("test:employee"));

            var mgr = Entity.Create(new EntityRef("test:manager"));

            Action<IEntity> updateAction = (wfr) => wfr.SetRelationships(reportsToRef, new EntityRelationshipCollection<IEntity> { mgr });

            var emp = Entity.Create(new EntityRef("test:employee"));
            emp.Save();

            UpdateFieldImplementation.PerformUpdate(emp, updateAction);
        }

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(UpdateFieldImplementation.UpdateInvalidReferenceException))]
        public void PerformUpdateWithRefToInvalidResource()
        {
            using (var ctx = DatabaseContext.GetContext(true))
            {
                var mgr1 = Entity.Create(new EntityRef("test:manager"));
                mgr1.Save();
                Entity.Delete(mgr1.Id);

                var reportsToRef = Entity.Get<Relationship>(new EntityRef("test:reportsTo"));
                var personType = Entity.Get<Definition>(new EntityRef("test:employee"));

                Action<IEntity> updateAction = (wfr) => wfr.SetRelationships(reportsToRef, new EntityRelationshipCollection<IEntity> { mgr1 });

                var emp = Entity.Create(new EntityRef("test:employee"));
                emp.Save();

                UpdateFieldImplementation.PerformUpdate(emp, updateAction);
            }

        }
    }

}