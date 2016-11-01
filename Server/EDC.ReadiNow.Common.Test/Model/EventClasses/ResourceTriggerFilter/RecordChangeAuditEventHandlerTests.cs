// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EDC.ReadiNow.Test.Model.EventClasses.ResourceTriggerFilter
{
	[TestFixture]
    [RunAsDefaultTenant]
    public class RecordChangeAuditEventHandlerTests
    {

        [Test]
        public void LogCreate()
        {
            RecordChangeAuditPolicy policy = null;
            Person person = null;
             
            try
            {

                policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreate", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Person.Name_Field.Cast<Field>());
                policy.Save();

                var log = RunAndLog(() =>
                {
                    person = Entity.Create<Person>();
                    person.Name = "bob";
                    person.Save();
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();

                Assert.That(logEntry, Is.Not.Null);

                var resource = logEntry.ObjectReferencedInLog;
                Assert.That(resource, Is.Not.Null);
                Assert.That(resource.Id, Is.EqualTo(person.Id));

                Assert.That(logEntry.Name, Is.EqualTo("System Administrator created 'bob'"));
                Assert.That(logEntry.Description, Is.EqualTo("[Name] set to 'bob'"));
            }
            finally
            {
                if (policy != null)
                    policy.Delete();

                if (person != null)
                    person.Delete();
            }
        }

        [Test]
        public void LogCreateNoFields()
        {
            RecordChangeAuditPolicy policy = null;
            Person person = null;

            try
            {

                policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreateNoFields", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.Save();

                var log = RunAndLog(() =>
                {
                    person = Entity.Create<Person>();
                    person.Name = "bob";
                    person.Save();
                });

                Assert.That(log.Count, Is.EqualTo(0));
            }
            finally
            {
                if (policy != null)
                    policy.Delete();

                if (person != null)
                    person.Delete();
            }
        }


        [Test]
        public void LogCreateWithMultipleFields()
        {
            RecordChangeAuditPolicy policy = null;
            Person person = null;

            try
            {

                policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreateWithMultipleFields", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Person.Name_Field.Cast<Field>());
                policy.UpdatedFieldsToTriggerOn.Add(Person.Description_Field.Cast<Field>());
                policy.Save();

                var log = RunAndLog(() =>
                {
                    person = Entity.Create<Person>();
                    person.Name = "bob";
                    person.Description = "myDescription";
                    person.Save();
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();

                Assert.That(logEntry, Is.Not.Null);

                var resource = logEntry.ObjectReferencedInLog;
                Assert.That(resource, Is.Not.Null);
                Assert.That(resource.Id, Is.EqualTo(person.Id));

                Assert.That(logEntry.Name, Is.EqualTo("System Administrator created 'bob'"));
                Assert.That(logEntry.Description, Is.EqualTo("[Description] set to 'myDescription'\r\n[Name] set to 'bob'"));
            }
            finally
            {
                if (policy != null)
                    policy.Delete();

                if (person != null)
                    person.Delete();
            }
        }

        [Test]
        public void LogUpdate()
        {
            RecordChangeAuditPolicy policy = null;
            Person person = null;

            try
            {

                policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogUpdate", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Person.Name_Field.Cast<Field>());
                policy.Save();

                person = Entity.Create<Person>();
                person.Name = "bob2";
                person.Save();

                var log = RunAndLog(() =>
                {
                    person = person.AsWritable<Person>();
                    person.Name = "jane";
                    person.Save();
                });
                 
                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();

                Assert.That(logEntry, Is.Not.Null);
                Assert.That(logEntry.LogEventTime, Is.LessThanOrEqualTo(DateTime.UtcNow));
                Assert.That(logEntry.LogEventTime, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));

                var resource = logEntry.ObjectReferencedInLog;
                Assert.That(resource, Is.Not.Null);
                Assert.That(resource.Id, Is.EqualTo(person.Id));

                Assert.That(logEntry.Name, Is.EqualTo("System Administrator updated 'bob2'"));
                Assert.That(logEntry.Description, Is.StringContaining("bob2"));
                Assert.That(logEntry.Description, Is.StringContaining("jane"));
            }
            finally
            {
                if (policy != null)
                    policy.Delete();

                if (person != null)
                    person.Delete();
            }
        }


        [Test]
        public void ForwardLookup()
        {
            TestRelationship( CardinalityEnum_Enumeration.ManyToOne,
                (policy, rType1, rType2) =>
                {
                    policy.TriggeredOnType = rType1;
                },
                (r1, r2, rel) =>
                {
                    r1 = r1.AsWritable<Resource>();
                    var rels = r1.GetRelationships<Resource>(rel);
                    rels.Add(r2);
                    r1.Save();
                },
                "[MyRel] set to 'r2'");
        }

        [Test]
        public void ReverseLookup()
        {
            TestRelationship( CardinalityEnum_Enumeration.OneToMany,
                (policy, rType1, rType2) =>
                {
                    policy.TriggeredOnType = rType2;
                },
                (r1, r2, rel) =>
                {
                    r2 = r2.AsWritable<Resource>();
                    var rels = r2.GetRelationships<Resource>(rel, Direction.Reverse);
                    rels.Add(r1);
                    r2.Save();
                },
                "[MyRel] set to 'r1'");
        }

        [Test]
        public void ForwardRel()
        {
            TestRelationship(CardinalityEnum_Enumeration.ManyToMany,
                (policy, rType1, rType2) =>
                {
                    policy.TriggeredOnType = rType1;
                },
                (r1, r2, rel) =>
                {
                    r1 = r1.AsWritable<Resource>();
                    var rels = r1.GetRelationships<Resource>(rel);
                    rels.Add(r2);
                    r1.Save();
                },
                "[MyRel] added 'r2'");
        }

        [Test]
        public void ReverseRel()
        {
            TestRelationship(CardinalityEnum_Enumeration.ManyToMany,
                (policy, rType1, rType2) =>
                {
                    policy.TriggeredOnType = rType2;
                },
                (r1, r2, rel) =>
                {
                    r2 = r2.AsWritable<Resource>();
                    var rels = r2.GetRelationships<Resource>(rel, Direction.Reverse);
                    rels.Add(r1);
                    r2.Save();
                },
                "[MyRel] added 'r1'");
        }

        
        [Test]
        public void ChoiceField()
        {
                Func<EntityType, EntityType, Relationship> createRelFn = (rType1, rType2) =>
                {
                    var rel = new Relationship() { Name = "MyRel" };
                    rel.FromType = rType1;
                    rel.ToType = rType2;

                    rel.RelType_Enum = RelTypeEnum_Enumeration.RelChoiceField;
                    rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;

                    return rel;
                };

                TestRelationship(
                    createRelFn,
                    (policy, rType1, rType2) =>
                    {
                        policy.TriggeredOnType = rType1;
                    },
                    (r1, r2, rel) =>
                    {
                        r1 = r1.AsWritable<Resource>();
                        var rels = r1.GetRelationships<Resource>(rel, Direction.Forward);
                        rels.Add(r2);
                        r1.Save();
                    },
                    "[MyRel] set to 'r2'");
        }

        [Test]
        public void Delete()
        {
            var toDelete = new List<long>();

            try
            {
                var myType = new EntityType { Name = "myType" };
                myType.Inherits.Add(Resource.Resource_Type);
                myType.Save();
                toDelete.Add(myType.Id);

                var instance = Entity.Create(myType).As<Resource>();
                instance.Name = "instance";
                instance.Save();
                toDelete.Add(instance.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests Delete", TriggeredOnType = myType, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Person.Name_Field.Cast<Field>());
                policy.Save();
                toDelete.Add(policy.Id);

                var log = RunAndLog(() =>
                {
                    Entity.Delete(instance.Id);
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();

                Assert.That(logEntry.Name, Is.EqualTo("System Administrator deleted 'instance'"));
            }
            finally
            {
                Entity.Delete(toDelete);
            }

}


        [Test]
        [Ignore("This is not working yet.")]
        public void CascadeDeleteOfRelated()
        {
            var toDelete = new List<long>();

            try
            {
                var parentType = new EntityType { Name = "dummy parent" };
                parentType.Save();
                toDelete.Add(parentType.Id);

                var childType = new EntityType { Name = "dummy child" };
                childType.Save();
                toDelete.Add(childType.Id);

                var rel = new Relationship { Name = "test rel",  FromType = parentType, ToType = childType, CascadeDelete = true, Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne };
                rel.Save();
                toDelete.Add(rel.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests CascadeDeleteOfRelated", TriggeredOnType = childType, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Person.Name_Field.Cast<Field>());
                policy.Save();
                toDelete.Add(policy.Id);

                var child = Entity.Create(childType);
                child.Save();
                toDelete.Add(child.Id);

                var parent = Entity.Create(parentType);
                parent.GetRelationships(rel).Add(child);
                parent.Save();
                toDelete.Add(parent.Id);

                var log = RunAndLog(() =>
                {
                    Entity.Delete(parent.Id);
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();

                Assert.That(logEntry.Name, Is.EqualTo("System Administrator deleted 'dummy child'"));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        [Test]
        public void RegularUserMakesAChangeIsLogged()
        {
            var toDelete = new List<long>();

            try
            {
                var userAccount = Entity.Create<UserAccount>();
                userAccount.Name = "Test user " + Guid.NewGuid().ToString();
                userAccount.Save();
                toDelete.Add(userAccount.Id);

                new AccessRuleFactory()
                    .AddAllowByQuery(
                        userAccount.As<Subject>(),
                        UserResource.UserResource_Type.As<SecurableEntity>(),
                        new EntityRef[] { new EntityRef("core:create") },
                        TestQueries.EntitiesWithName("Creatable").ToReport());

                new AccessRuleFactory()
                   .AddAllowByQuery(
                        userAccount.As<Subject>(),
                        UserResource.UserResource_Type.As<SecurableEntity>(),
                        new EntityRef[] { new EntityRef("core:modify") },
                        TestQueries.EntitiesWithName("bob").ToReport());

                var entityType = new EntityType { Name = "Creatable" };
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();
                toDelete.Add(entityType.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests RegularUserMakesAChangeIsLogged", TriggeredOnType = entityType, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Resource.Description_Field.Cast<Field>());
                policy.Save();
                toDelete.Add(policy.Id);

                var log = RunAndLog(() =>
                {
                    using (new SetUser(userAccount))
                    {
                        var entity = Entity.Create(entityType);
                        entity.SetField(Resource.Name_Field.As<Field>(), "bob");
                        entity.SetField(Resource.Description_Field.As<Field>(), "first");
                        entity.Save();
                        toDelete.Add(entity.Id);

                        entity = entity.AsWritable();
                        entity.SetField(Resource.Description_Field.As<Field>(), "second");
                        entity.Save();
                    }
                });


                Assert.That(log.Count, Is.EqualTo(2));
                Assert.That(log[0].Description, Is.EqualTo("[Description] set to 'first'"));
                Assert.That(log[1].Description, Is.EqualTo("[Description] changed from 'first' -> 'second'"));
            }
            finally
            {
                Entity.Delete(toDelete);
            }

        }

        [Test]
        public void FieldNameChangeDoesNotBreakExistingPolicy()
        {
            var toDelete = new List<long>();

            try
            {
                var newField = new StringField { Name = "test field" }.As<Field>();
                newField.Save();
                toDelete.Add(newField.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreate", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(newField);
                policy.Save();
                toDelete.Add(policy.Id);

                newField = newField.AsWritable<Field>();
                newField.Name = "Renamed field";
                newField.Save();

                var log = RunAndLog(() =>
                {
                    var person = Entity.Create<Person>();
                    person.SetField(newField, "bob");
                    person.Save();
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();
                Assert.That(logEntry.Description, Is.EqualTo("[Renamed field] set to 'bob'"));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        [Test]
        public void RelNameChangeDoesNotBreakExistingPolicy()
        {
            var toDelete = new List<long>();

            try
            {
                var parentType = new EntityType { Name = "dummy parent type" };
                parentType.Save();
                toDelete.Add(parentType.Id);

                var childType = new EntityType { Name = "dummy child type" };
                childType.Save();
                toDelete.Add(childType.Id);

                var rel = new Relationship { Name = "test rel", FromType = parentType, ToType = childType, Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne };
                rel.Save();
                toDelete.Add(rel.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests CascadeDeleteOfRelated", TriggeredOnType = parentType, TriggerEnabled = true };
                policy.UpdatedRelationshipsToTriggerOn.Add(rel);
                policy.Save();
                toDelete.Add(policy.Id);

                rel = rel.AsWritable<Relationship>();
                rel.Name = "renamed rel";
                rel.Save();

                var child = Entity.Create(childType);
                child.SetField(Resource.Name_Field, "child");
                child.Save();
                toDelete.Add(child.Id);

                var parent = Entity.Create(parentType);
                parent.Save();
                toDelete.Add(parent.Id);

                var log = RunAndLog(() =>
                {
                    parent = parent.AsWritable();
                    parent.GetRelationships(rel).Add(child);
                    parent.Save();
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();
                Assert.That(logEntry.Description, Is.EqualTo("[renamed rel] set to 'child'"));

            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        [Test]
        public void DeleteAFieldReferedToByAPolicy()
        {
            var toDelete = new List<long>();

            try
            {
                var newField = new StringField { Name = "test field" }.As<Field>();
                newField.Save();
                toDelete.Add(newField.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreate", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(newField);
                policy.UpdatedFieldsToTriggerOn.Add(Resource.Name_Field.As<Field>());
                policy.Save();
                toDelete.Add(policy.Id);

                newField.Delete();

                var log = RunAndLog(() =>
                {
                    var person = Entity.Create<Person>();
                    person.Name = "bob";
                    person.Save();
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();
                Assert.That(logEntry.Description, Is.EqualTo("[Name] set to 'bob'"));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }


        [Test]
        [Explicit]
        [Ignore("Need to confirm requirment")]
        public void TwoPoliciesOnOneObjectGeneratesOneLogEntry()
        {
            var toDelete = new List<long>();

            try
            {
                var policy1 = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreate1", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy1.UpdatedFieldsToTriggerOn.Add(Resource.Name_Field.As<Field>());
                policy1.Save();
                toDelete.Add(policy1.Id);

                var policy2 = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreate2", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy2.UpdatedFieldsToTriggerOn.Add(Resource.Description_Field.As<Field>());
                policy2.Save();
                toDelete.Add(policy2.Id);

                var log = RunAndLog(() =>
                {
                    var person = Entity.Create<Person>();
                    person.Name = "bob";
                    person.Description = "bob description";
                    person.Save();
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();
                Assert.That(logEntry.Description, Is.EqualTo("[Description] set to 'bob description'\r\n[Name] set to 'bob'"));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }


        [Explicit]
        [Test]
        public void TwoPoliciesOnOneObjectGeneratesOneLogEntry_OneInheritsFromOther()
        {
            Assert.Fail();
        }

        [Test]
        public void NameChangeReportsUsingOldName()
        {
            var toDelete = new List<long>();

            try
            {
                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreate1", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Resource.Name_Field.As<Field>());
                policy.Save();
                toDelete.Add(policy.Id);

                var person = Entity.Create<Person>();
                person.Name = "bob";
                person.Save();

                var log = RunAndLog(() =>
                {
                    person = person.AsWritable<Person>();
                    person.Name = "jane";
                    person.Save();

                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();
                Assert.That(logEntry.Name, Is.EqualTo("System Administrator updated 'bob'"));
                Assert.That(logEntry.Description, Is.EqualTo("[Name] changed from 'bob' -> 'jane'"));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }



        [TestCase(null, "System Administrator updated 'bob'")]
        [TestCase("Secondary", "System Administrator(Secondary) updated 'bob'")]
        [TestCase("System Administrator", "System Administrator updated 'bob'")]
        public void LogEntryContainsSecondaryName( string secondaryName, string expected)
        {
            var toDelete = new List<long>();

            try
            {
                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests LogCreate1", TriggeredOnType = Person.Person_Type, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(Resource.Name_Field.As<Field>());
                policy.Save();
                toDelete.Add(policy.Id);

                var person = Entity.Create<Person>();
                person.Name = "bob";
                person.Save();


                var log = RunAndLog(() =>
                {
                    var contextData = new RequestContextData(RequestContext.GetContext());

                    if (secondaryName != null)
                        contextData.SecondaryIdentity = new IdentityInfo(999, secondaryName);

                    RequestContext.SetContext(contextData);

                    person = person.AsWritable<Person>();
                    person.Name = "jane";
                    person.Save();

                });

                Assert.That(log.Count, Is.EqualTo(1));
                Assert.That(log[0].Name, Is.EqualTo(expected));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }


        void TestRelationship(CardinalityEnum_Enumeration cardinality, Action<RecordChangeAuditPolicy, EntityType, EntityType> setPolicyAction, Action<Resource, Resource, Relationship> updateAction, string expected)
        {
            Func<EntityType, EntityType, Relationship> createRelFn = (rType1, rType2) =>
              {
                  var rel = new Relationship() { Name = "MyRel" };
                  rel.FromType = rType1;
                  rel.ToType = rType2;

                  rel.RelType_Enum = RelTypeEnum_Enumeration.RelCustom;
                  rel.Cardinality_Enum = cardinality;

                  return rel;
              };

            TestRelationship(createRelFn, setPolicyAction, updateAction, expected);
        }

        void TestRelationship(Func<EntityType, EntityType, Relationship> createRelFn, Action<RecordChangeAuditPolicy, EntityType, EntityType> setPolicyAction, Action<Resource, Resource, Relationship> updateAction, string expected)
        {

            var toDelete = new List<long>();

            try
            {
                var rType1 = new EntityType() { Name = "rType1" };
                rType1.Inherits.Add(Resource.Resource_Type);
                rType1.Save();
                toDelete.Add(rType1.Id);

                var rType2 = new EntityType() { Name = "rType2" };
                rType2.Inherits.Add(Resource.Resource_Type);
                rType2.Save();
                toDelete.Add(rType2.Id);

                var rel = createRelFn(rType1, rType2);

                rel.Save();
                toDelete.Add(rel.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests ForwardLookup", TriggerEnabled = true };
                policy.UpdatedRelationshipsToTriggerOn.Add(rel);
                setPolicyAction(policy, rType1, rType2);
                policy.Save();
                toDelete.Add(policy.Id);

                var r1 = Entity.Create(rType1).As<Resource>();
                r1.Name = "r1";
                r1.Save();

                var r2 = Entity.Create(rType2).As<Resource>();
                r2.Name = "r2";
                r2.Save();

                var log = RunAndLog(() =>
                {
                    updateAction(r1, r2, rel);
                });

                Assert.That(log.Count, Is.EqualTo(1));

                var logEntry = log[0].As<RecordChangeLogEntry>();

                Assert.That(logEntry, Is.Not.Null);

                Assert.That(logEntry.Description, Is.StringContaining(expected));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }





        public IEnumerable<TestCaseData> TestIgnoredFieldsSource()
        {
            yield return new TestCaseData("core:createdDate", (Func<object>)(() => DateTime.UtcNow));
            yield return new TestCaseData("core:modifiedDate", (Func<object>)(() => DateTime.UtcNow));
            yield return new TestCaseData("core:createdBy", (Func<object>)(() => Entity.Get<UserAccount>(new EntityRef("core:administratorUserAccount"))));
            yield return new TestCaseData("core:lastModifiedBy", (Func<object>)(() => Entity.Get<UserAccount>(new EntityRef("core:administratorUserAccount"))));
        }


        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource("TestIgnoredFieldsSource")]
        public void TestIgnoredField_R_2_2_8(string fieldRef, Func<object> getNewValue )
        {
            Action<IEntity> updateAction;
            var field = Entity.Get<Field>(new EntityRef(fieldRef));
            if (field != null)
            {
                updateAction = (e) => e.SetField(field, getNewValue());
            }
            else
            {
                var rel = Entity.Get<Relationship>(new EntityRef(fieldRef));
                updateAction = (e) => e.GetRelationships(rel).Add((IEntity) getNewValue());
            }

            var toDelete = new List<long>();

            try
            {
                var myType = Entity.Create<EntityType>();
                myType.Name = "ResourceAuditEventHandlerTests TestIgnoredField";
                myType.Save();
                toDelete.Add(myType.Id);

                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests TestIgnoredField", TriggeredOnType = myType, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(field);
                policy.Save();

                var log = RunAndLog(() =>
                {
                    var entity = Entity.Create(myType);
                    updateAction(entity);
                    entity.Save();
                    toDelete.Add(entity.Id);
                });

                Assert.That(log.Count, Is.EqualTo(0));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }

        public IEnumerable<TestCaseData> TestIgnoredFieldTypesSource()
        {
            yield return new TestCaseData("autoNumberField", (Func<Field>)(() => (new AutoNumberField { Name = "ResourceAuditEventHandlerTests AutoNumber", AutoNumberSeed =  0 } ).As<Field>()));
            yield return new TestCaseData("calculatedField", (Func<Field>)(() => (new StringField { Name = "ResourceAuditEventHandlerTests Calc", IsCalculatedField = true } ).As<Field>()));
        }

        [Test]
        [TestCaseSource("TestIgnoredFieldTypesSource")]
        [RunAsDefaultTenant]
        public void TestIgnoredFieldTypes_R_2_2_8(string fieldType, Func<Field> createField)
        {
            var toDelete = new List<long>();

            try
            {
                var myField = createField();
                myField.Save();

                var myType = Entity.Create<EntityType>();
                myType.Name = "ResourceAuditEventHandlerTests TestIgnoredField";
                myType.Fields.Add(myField.As<Field>());
                myType.Save();
                toDelete.Add(myType.Id);



                var policy = new RecordChangeAuditPolicy() { Name = "TEST ResourceAuditEventHandlerTests TestIgnoredField", TriggeredOnType = myType, TriggerEnabled = true };
                policy.UpdatedFieldsToTriggerOn.Add(myField.As<Field>());
                policy.Save();

                var log = RunAndLog(() =>
                {
                    var entity = Entity.Create(myType);
                    entity.Save();      // This should update the autonumber field
                    toDelete.Add(entity.Id);
                });

                Assert.That(log.Count, Is.EqualTo(0));
            }
            finally
            {
                Entity.Delete(toDelete);
            }
        }


        public List<TenantLogEntry> RunAndLog(Action act)
        {
            Mock<IActivityLogWriter> mockLogWriter;
            List<TenantLogEntry> log;

            var mockRepository = new MockRepository(MockBehavior.Strict);

            CreateMockedLog(mockRepository, out mockLogWriter, out log);

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockLogWriter.Object).As<IActivityLogWriter>();

            }))
            using (Factory.SetCurrentScope(scope))
            {
                using (var dbContext = DatabaseContext.GetContext(requireTransaction:true, preventPostSaveActionsPropagating:true))
                {
                    act();

                    Assert.That(log.Count, Is.EqualTo(0), "No log messages until the transaction commits");

                    dbContext.CommitTransaction();
                }

            }

            return log;
        }


        void CreateMockedLog(MockRepository mockRepo, out Mock<IActivityLogWriter> mockLogWriter, out List<TenantLogEntry> loggedMessages)
        {
            var actualLogWriter = Factory.ActivityLogWriter;
            mockLogWriter = mockRepo.Create<IActivityLogWriter>(MockBehavior.Loose);

            var log = new List<TenantLogEntry>();

            Expression<Action<IActivityLogWriter>> logAction = (lw) => lw.WriteLogEntry(It.IsAny<TenantLogEntry>());

            mockLogWriter.Setup(logAction).Callback<TenantLogEntry>(tle =>
            {
                log.Add(tle);
                //actualLogWriter.WriteLogEntry(tle);
                });
            loggedMessages = log;
        }
    }
}