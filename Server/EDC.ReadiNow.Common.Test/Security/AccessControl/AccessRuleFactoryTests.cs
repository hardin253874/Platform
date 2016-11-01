// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class AccessRuleFactoryTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_NullSubject()
        {
            using (DatabaseContext.GetContext(true))
            {
                Assert.That(() => new AccessRuleFactory().AddAllowReadQuery(null, Entity.Create<SecurableEntity>(), new Report()),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("subject"));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_NullSecurableEntity()
        {
            using (DatabaseContext.GetContext(true))
            {
                Assert.That(() => new AccessRuleFactory().AddAllowReadQuery(Entity.Create<Subject>(), null, new Report()),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("securableEntity"));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_NullReport()
        {
            using (DatabaseContext.GetContext(true))
            {
                Assert.That(() => new AccessRuleFactory().AddAllowReadQuery(Entity.Create<Subject>(), Entity.Create<SecurableEntity>(), null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("report"));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowCreate_BasicAllowCreate()
        {
            EntityType securableEntityType;
            Subject subject;
            IEntity entity;

            securableEntityType = Entity.Create<EntityType>();
            securableEntityType.Inherits.Add(Entity.Get<EntityType>("core:resource"));
            securableEntityType.Save();

            subject = Entity.Create<UserAccount>().As<Subject>();
            subject.Save();

            new AccessRuleFactory().AddAllowCreate(subject, securableEntityType.As<SecurableEntity>());

            entity = Entity.Create(securableEntityType);
            Assert.That(new EntityAccessControlChecker().CheckAccess(new[] { new EntityRef(entity) },
                                                                     new []{ Permissions.Create}, subject),
                            Has.Exactly(1).Property("Key").EqualTo(entity.Id).And.Property("Value").True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_BasicAllowRead()
        {
            Do_AddAllowXXXQuery_AllowSingleXXX(Permissions.Read);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowModifyQuery_BasicAllowModify()
        {
            Do_AddAllowXXXQuery_AllowSingleXXX(Permissions.Modify);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowDeleteQuery_BasicAllowDelete()
        {
            Do_AddAllowXXXQuery_AllowSingleXXX(Permissions.Delete);
        }

        private void Do_AddAllowXXXQuery_AllowSingleXXX(EntityRef permission)
        {
            EntityType securableEntityType;
            IEntity entity;
            Subject subject;
            List<EntityRef> permissions;

            using (DatabaseContext.GetContext(true))
            {
                permissions = new List<EntityRef>() { permission };

                securableEntityType = Entity.Create<EntityType>();
                securableEntityType.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType.Save();
                entity = Entity.Create(new EntityRef(securableEntityType.Id));
                entity.SetField("core:name", "Entity 1");
                entity.Save();
                subject = Entity.Create<UserAccount>().As<Subject>();
                subject.Save();

                new AccessRuleFactory().AddAllowByQuery(subject, securableEntityType.As<SecurableEntity>(), permissions, 
                    TestQueries.Entities().ToReport());

                Assert.That(new EntityAccessControlChecker().CheckAccess(new[] { new EntityRef(entity.Id) },
                        permissions, subject),
                    Has.Exactly(1).Property("Key").EqualTo(entity.Id).And.Property("Value").True);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_BasicAllowReadAddTwice()
        {
            Do_AddAllowXXXQuery_AllowSingleXXXAddTwice(Permissions.Read );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowModifyQuery_BasicAllowModifyAddTwice()
        {
            Do_AddAllowXXXQuery_AllowSingleXXXAddTwice(Permissions.Modify);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowDeleteQuery_BasicAllowDeleteAddTwice()
        {
            Do_AddAllowXXXQuery_AllowSingleXXXAddTwice(Permissions.Delete);
        }

        private void Do_AddAllowXXXQuery_AllowSingleXXXAddTwice(EntityRef permission)
        {
            EntityType securableEntityType;
            IEntity entity;
            Subject subject;
            List<EntityRef> permissions;

            using (DatabaseContext.GetContext(true))
            {
                permissions = new List<EntityRef>() { permission };

                securableEntityType = Entity.Create<EntityType>();
                securableEntityType.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType.Save();
                entity = Entity.Create(new EntityRef(securableEntityType.Id));
                entity.SetField("core:name", "Entity 1");
                entity.Save();
                subject = Entity.Create<UserAccount>().As<Subject>();
                subject.Save();

                new AccessRuleFactory().AddAllowByQuery(subject, securableEntityType.As<SecurableEntity>(), permissions, TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllowByQuery(subject, securableEntityType.As<SecurableEntity>(), permissions, TestQueries.Entities().ToReport());

                Assert.That(new EntityAccessControlChecker().CheckAccess(new[] { new EntityRef(entity.Id) }, permissions, subject),
                    Has.Exactly(1).Property("Key").EqualTo(entity.Id).And.Property("Value").True);
                Assert.That(subject.AllowAccess.Select(x => x.ControlAccess), 
                    Has.Exactly(2).Property("Id").EqualTo(securableEntityType.Id));
                Assert.That(subject.AllowAccess.Where(x => x.ControlAccess.Id == securableEntityType.Id).SelectMany(x => x.PermissionAccess),
                    Has.Exactly(2).Property("Id").EqualTo(permission.Id));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowCreate_AllowMultipleCreate()
        {
            EntityType[] securableEntityTypes;
            const int numTypes = 5;
            Subject subject;
            IDictionary<long, bool> result;
            IEntity[] entities;

            subject = Entity.Create<UserAccount>().As<Subject>();
            subject.Save();

            securableEntityTypes = new EntityType[numTypes];
            for (int i = 0; i < numTypes; i++)
            {
                securableEntityTypes[i] = Entity.Create<EntityType>();
                securableEntityTypes[i].Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityTypes[i].Save();

                new AccessRuleFactory().AddAllowCreate(subject, securableEntityTypes[i].As<SecurableEntity>());
            }

            entities = new IEntity[numTypes];
            for (int i = 0; i < numTypes; i++)
            {
                entities[i] = Entity.Create(securableEntityTypes[i]);
            }

            result = new EntityAccessControlChecker().CheckAccess(entities.Select(x => new EntityRef(x)).ToList(),
                                                                new[] { Permissions.Create }, subject);

            Assert.That(result, Has.Count.EqualTo(numTypes));
            Assert.That(result.Keys, Is.EquivalentTo(entities.Select(x => x.Id)));
            Assert.That(result.Values, Has.All.True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_AllowMultipleRead()
        {
            Do_AddAllowXXXQuery_AllowMultiple(
                (s, se) => new AccessRuleFactory().AddAllowReadQuery(s, se, TestQueries.Entities().ToReport()),
                new[] { Permissions.Read });
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowModifyQuery_AllowMultipleModify()
        {
            Do_AddAllowXXXQuery_AllowMultiple(
                (s, se) => new AccessRuleFactory().AddAllowModifyQuery(s, se, TestQueries.Entities().ToReport()),
                new[] { Permissions.Modify });
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowDeleteQuery_AllowMultipleDelete()
        {
            Do_AddAllowXXXQuery_AllowMultiple(
                (s, se) => new AccessRuleFactory().AddAllowDeleteQuery(s, se, TestQueries.Entities().ToReport()),
                new[] { Permissions.Delete });
        }

        private void Do_AddAllowXXXQuery_AllowMultiple(Action<Subject, SecurableEntity> addAccess, ICollection<EntityRef> operations)
        {
            EntityType securableEntityType;
            const int numEntities = 5;
            Entity[] entities;
            Subject subject;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType = Entity.Create<EntityType>();
                securableEntityType.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType.Save();
                entities = new Entity[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = Entity.Create(new EntityRef(securableEntityType.Id)).As<Entity>();
                    entities[i].Save();
                }
                subject = Entity.Create<UserAccount>().As<Subject>();
                subject.Save();

                addAccess(subject, securableEntityType.As<SecurableEntity>());

                result = new EntityAccessControlChecker().CheckAccess(entities.Select(x => (EntityRef) x).ToList(),
                                                                      operations.ToList(), subject);
                for (int i = 0; i < numEntities; i++)
                {
                    Assert.That(result,
                        Has.Exactly(1).Property("Key").EqualTo(entities[i].Id).And.Property("Value").True);
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_BasicAllowReadToOneTypeOnly()
        {
            using (DatabaseContext.GetContext(true))
            {
                EntityType securableEntityType1;
                EntityType securableEntityType2;
                IEntity entity1;
                IEntity entity2;
                Subject subject;
                Report report;
                IDictionary<long, bool> result;

                securableEntityType1 = Entity.Create<EntityType>();
                securableEntityType1.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType1.Save();
                securableEntityType2 = Entity.Create<EntityType>();
                securableEntityType2.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType2.Save();
                entity1 = Entity.Create(new EntityRef(securableEntityType1.Id));
                entity1.Save();
                entity2 = Entity.Create(new EntityRef(securableEntityType2.Id));
                entity2.Save();
                subject = Entity.Create<UserAccount>().As<Subject>();
                subject.Save();
                report = TestQueries.Entities().ToReport();

                new AccessRuleFactory().AddAllowReadQuery(subject, securableEntityType1.As<SecurableEntity>(), report);

                result = new EntityAccessControlChecker().CheckAccess(
                                new[] { new EntityRef(entity1.Id), new EntityRef(entity2.Id) },
                                new[] { Permissions.Read },
                                subject);
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").True);
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity2.Id).And.Property("Value").False);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddAllowReadQuery_NameQuery()
        {
            using (DatabaseContext.GetContext(true))
            {
                EntityType securableEntityType1;
                IEntity entity1A;
                IEntity entity1Z;
                Subject subject;
                Report report;
                IDictionary<long, bool> result;
                    
                securableEntityType1 = Entity.Create<EntityType>();
                securableEntityType1.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType1.Save();
                entity1A = Entity.Create(new EntityRef(securableEntityType1.Id));
                entity1A.SetField("core:name", "A");
                entity1A.Save();
                entity1Z = Entity.Create(new EntityRef(securableEntityType1.Id));
                entity1Z.SetField("core:name", "Z");
                entity1Z.Save();
                subject = Entity.Create<UserAccount>().As<Subject>();
                subject.Save();
                report = TestQueries.EntitiesWithNameA().ToReport();

                new AccessRuleFactory().AddAllowReadQuery(subject, securableEntityType1.As<SecurableEntity>(), report);

                result = new EntityAccessControlChecker().CheckAccess(
                                new[] { new EntityRef(entity1A.Id), new EntityRef(entity1Z.Id) },
                                new[] { Permissions.Read }, 
                                subject);
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity1A.Id).And.Property("Value").True);
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity1Z.Id).And.Property("Value").False);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddMultiplePermissions()
        {
            EntityType securableEntityType1;
            EntityType securableEntityType2;
            Entity entity1;
            Entity entity2;
            Subject subject;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType1 = Entity.Create<EntityType>();
                securableEntityType1.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType1.Save();
                securableEntityType2 = Entity.Create<EntityType>();
                securableEntityType2.Inherits.Add(Entity.Get<EntityType>("core:resource"));
                securableEntityType2.Save();
                entity1 = Entity.Create(securableEntityType1).As<Entity>();
                entity1.SetField("core:alias", "entity1__test");
                entity1.Save();
                entity2 = Entity.Create(securableEntityType2).As<Entity>();
                entity2.SetField("core:alias", "entity2__test");
                entity2.Save();
                subject = Entity.Create<UserAccount>().As<Subject>();
                subject.Save();

                new AccessRuleFactory().AddAllowReadQuery(subject, securableEntityType1.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllowReadQuery(subject, securableEntityType2.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllowModifyQuery(subject, securableEntityType2.As<SecurableEntity>(), TestQueries.Entities().ToReport());

                result = new EntityAccessControlChecker().CheckAccess(new EntityRef[] { entity1, entity2 },
                                                                      new[] { Permissions.Read }, subject);
                Assert.That(result,
                    Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").True, "Allow read");
                Assert.That(result,
                    Has.Exactly(1).Property("Key").EqualTo(entity2.Id).And.Property("Value").True, "Allow read");

                result = new EntityAccessControlChecker().CheckAccess(new EntityRef[] { entity1, entity2 },
                                                                      new[] { Permissions.Modify }, subject);
                Assert.That(result,
                    Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").False, "Allow modify");
                Assert.That(result,
                    Has.Exactly(1).Property("Key").EqualTo(entity2.Id).And.Property("Value").True, "Allow modify");

                result = new EntityAccessControlChecker().CheckAccess(new EntityRef[] { entity1, entity2 },
                                                                      new[] { Permissions.Read, Permissions.Modify }, subject);
                Assert.That(result,
                    Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").False, "Allow read and modify");
                Assert.That(result,
                    Has.Exactly(1).Property("Key").EqualTo(entity2.Id).And.Property("Value").True, "Allow read and modify");
            }
        }
    }
}
