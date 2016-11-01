// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class SecurityQueryCacheInvalidationTests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase(InvalidationCause.Save)]
        [TestCase(InvalidationCause.Delete)]
        public void Test_AccessRuleChange_SingleEntitySinglePermission(InvalidationCause cause)
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            AccessRule accessRule;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount = new UserAccount();
                userAccount.Name = Guid.NewGuid().ToString();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity = Entity.Create(entityType);
                entity.Save();

                accessRule = new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(), TestQueries.Entities().ToReport());

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount))
            {
                Assert.That(() => Entity.Get(entity.Id), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType.Id),
                    "Entry not added to cache");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                if (cause == InvalidationCause.Save)
                {
                    accessRule.Save();
                }
                else if (cause == InvalidationCause.Delete)
                {
                    accessRule.Delete();
                }
                else
                {
                    Assert.Fail("Unknown invalidation cause");
                }
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id),
                    "Entry not removed from cache");

            //Trace.WriteLine(string.Empty);
            //Trace.WriteLine("Entities for type " + entityType.Id);
            //TraceEntries(kvp => kvp.Key.EntityTypes.Contains(entityType.Id), cachingQueryRepository);

            //Trace.WriteLine(string.Empty);
            //Trace.WriteLine("Entities for administrators");
            //TraceEntries(kvp => kvp.Key.SubjectId == Entity.Get<Role>("core:administratorRole").Id, cachingQueryRepository);

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(InvalidationCause.Save)]
        [TestCase(InvalidationCause.Delete)]
        public void Test_AccessRuleChange_SingleEntityMultiplePermission(InvalidationCause cause)
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            AccessRule accessRule;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount = new UserAccount();
                userAccount.Name = Guid.NewGuid().ToString();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity = Entity.Create(entityType);
                entity.Save();

                accessRule = new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllowModifyQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount))
            {
                Assert.That(() => Entity.Get(entity.Id), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType.Id),
                    "Read entry not added to cache");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Modify.Id),
                    "Modify entry added to cache");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                if (cause == InvalidationCause.Save)
                {
                    accessRule.Save();
                }
                else if (cause == InvalidationCause.Delete)
                {
                    accessRule.Delete();
                }
                else
                {
                    Assert.Fail("Unknown invalidation cause");
                }

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id),
                    "Entry not removed from cache");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(InvalidationCause.Save)]
        [TestCase(InvalidationCause.Delete)]
        public void Test_SubjectDeletion_SingleEntitySinglePermission(InvalidationCause cause)
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            CachingQueryRepository cachingQueryRepository;
            long userAccountId;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount = new UserAccount();
                userAccount.Name = Guid.NewGuid().ToString();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity = Entity.Create(entityType);
                entity.Save();
                ctx.CommitTransaction();
            }

            new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                entityType.As<SecurableEntity>(), TestQueries.Entities().ToReport());

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount))
            {
                Assert.That(() => Entity.Get(entity.Id), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType.Id),
                    "Entry not added to cache");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccountId = userAccount.Id;
                if (cause == InvalidationCause.Save)
                {
                    userAccount.Save();
                }
                else if (cause == InvalidationCause.Delete)
                {
                    userAccount.Delete();
                }
                else
                {
                    Assert.Fail("Unknown invalidation cause");
                }
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccountId),
                    "Entry not removed from cache");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_SubjectAllowAccessChange_SwitchUserAccessRule()
        {
            UserAccount userAccount;
            EntityType entityType1;
            EntityType entityType2;
            IEntity entity1;
            IEntity entity2;
            AccessRule accessRule1;
            AccessRule accessRule2;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount = new UserAccount();
                userAccount.Name = Guid.NewGuid().ToString();
                userAccount.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entityType2 = new EntityType();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                entity2 = Entity.Create(entityType2);
                entity2.Save();

                accessRule1 = new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                    entityType1.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                accessRule2 = new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                    entityType2.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount))
            {
                Assert.That(() => Entity.Get(entity1.Id), Throws.Nothing);
                Assert.That(() => Entity.Get(entity2.Id), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount.AllowAccess.Add(accessRule2);
                userAccount.AllowAccess.Remove(accessRule1);
                userAccount.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id),
                    "Entry not removed from cache");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_SubjectAllowAccessChange_MoveAccessRuleFromUserToUser()
        {
            UserAccount userAccount1;
            UserAccount userAccount2;
            EntityType entityType1;
            EntityType entityType2;
            IEntity entity1;
            IEntity entity2;
            AccessRule accessRule1;
            AccessRule accessRule2;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                userAccount2 = new UserAccount();
                userAccount2.Name = Guid.NewGuid().ToString();
                userAccount2.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entityType2 = new EntityType();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                entity2 = Entity.Create(entityType2);
                entity2.Save();

                accessRule1 = new AccessRuleFactory().AddAllowReadQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                accessRule2 = new AccessRuleFactory().AddAllowReadQuery(userAccount2.As<Subject>(),
                    entityType2.As<SecurableEntity>(), TestQueries.Entities().ToReport());

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1), SecurityOption.DemandAll), Throws.Nothing);
            }
            using (new SetUser(userAccount2))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity2), SecurityOption.DemandAll), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount2.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType2.Id),
                    "Read entry not added to cache for user 2/entity type 2");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1.AllowAccess.Remove(accessRule1);
                userAccount1.Save();
                userAccount2.AllowAccess.Remove(accessRule2);
                userAccount2.AllowAccess.Add(accessRule1);
                userAccount2.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount2.Id),
                    "Entries not removed from cache for user 2");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_SubjectAllowAccessChange_MoveUserFromAccessRuleToAccessRule()
        {
            UserAccount userAccount1;
            UserAccount userAccount2;
            EntityType entityType1;
            EntityType entityType2;
            IEntity entity1;
            IEntity entity2;
            AccessRule accessRule1;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                userAccount2 = new UserAccount();
                userAccount2.Name = Guid.NewGuid().ToString();
                userAccount2.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entityType2 = new EntityType();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                entity2 = Entity.Create(entityType2);
                entity2.Save();

                accessRule1 = new AccessRuleFactory().AddAllowReadQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllowReadQuery(userAccount2.As<Subject>(),
                    entityType2.As<SecurableEntity>(), TestQueries.Entities().ToReport());

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }
            using (new SetUser(userAccount2))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity2)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount2.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType2.Id),
                    "Read entry not added to cache for user 2/entity type 2");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule1.AllowAccessBy = userAccount2.As<Subject>();
                accessRule1.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount2.Id),
                    "Entries not removed from cache for user 2");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ReportChange_ChangeAuthReport()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            IEntity entity1;
            AccessRule accessRule1;
            Report report1;
            Report report2;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                report2 = TestQueries.EntitiesWithNameA().ToReport();
                report2.Save();

                accessRule1 = new AccessRuleFactory().AddAllowReadQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), report1);
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule1.AccessRuleReport = report2;
                accessRule1.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ReportChange_ChangeReportAuth()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            IEntity entity1;
            AccessRule accessRule1;
            Report report1;
            Report report2;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                report2 = TestQueries.EntitiesWithNameA().ToReport();
                report2.Save();

                accessRule1 = new AccessRuleFactory().AddAllowReadQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), report1);

                ctx.CommitTransaction();
            }


            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule1.AccessRuleReport = null;
                accessRule1.Save();

                report2.ReportForAccessRule = accessRule1;
                report2.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_PermissionChange_AddAuthPermission()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            IEntity entity1;
            AccessRule accessRule1;
            Report report1;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();
            cachingQueryRepository.Cache.Clear();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                accessRule1 = new AccessRuleFactory().AddAllowReadQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), report1);

                ctx.CommitTransaction();
            }

            // Sanity check - ensure admins have at least one entry in the cache beforehand.
            Entity.Get( "test:af01" );
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "No initial entries");

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule1.PermissionAccess.Add(Permissions.Modify.Entity.As<Permission>());
                accessRule1.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");

            // Ensure the whole cache has been invalidated
            // Unfortunately, this currently appears to clear the entire cache
            //Assert.That(cachingQueryRepository.Cache,
            //    Has.Some
            //       .Property("Key").Property("SubjectId").EqualTo(Entity.Get<Role>("core:administratorRole").Id),
            //       "Removed all entries for administrators role");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_PermissionChange_RemoveAuthPermission()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            IEntity entity1;
            AccessRule accessRule1;
            Report report1;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();
            cachingQueryRepository.Cache.Clear();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                accessRule1 = new AccessRuleFactory().AddAllowByQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), new[] { Permissions.Read, Permissions.Modify }, report1);

                ctx.CommitTransaction();
            }

            // Sanity check - ensure admins have at least one entry in the cache beforehand.
            Entity.Get( "test:af01" );
            Assert.That( cachingQueryRepository.Cache, Is.Not.Empty, "No initial entries" );

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule1.PermissionAccess.Remove(Permissions.Modify.Entity.As<Permission>());
                accessRule1.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");

            // Ensure the whole cache has not been invalidated
            // Unfortunately, this currently appears to clear the entire cache
            //Assert.That(cachingQueryRepository.Cache,
            //    Has.Some
            //       .Property("Key").Property("SubjectId").EqualTo(Entity.Get<Role>("core:administratorRole").Id),
            //       "Removed all entries for administrators role");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_PermissionChange_ChangePermissionAuth()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            IEntity entity1;
            AccessRule accessRule1;
            Report report1;
            Permission modifyPermission;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();
            cachingQueryRepository.Cache.Clear();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                accessRule1 = new AccessRuleFactory().AddAllowByQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), new[] { Permissions.Read }, report1);

                ctx.CommitTransaction();
            }

            // Sanity check - ensure admins have at least one entry in the cache beforehand.
            Entity.Get( "test:af01" );
            Assert.That( cachingQueryRepository.Cache, Is.Not.Empty, "No initial entries" );

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                modifyPermission = Entity.Get<Permission>(Permissions.Modify, true);
                modifyPermission.PermissionAccessBy.Add(accessRule1);
                modifyPermission.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");

            // Ensure the whole cache has been invalidated
            // Unfortunately, this currently appears to clear the entire cache
            //Assert.That(cachingQueryRepository.Cache,
            //    Has.Some
            //       .Property("Key").Property("SubjectId").EqualTo(Entity.Get<Role>("core:administratorRole").Id),
            //       "Removed read entries for administrators role");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_SecurableEntityChange_ChangeAuthType()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            EntityType entityType2;
            IEntity entity1;
            AccessRule accessRule;
            Report report1;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                entityType2 = new EntityType();
                entityType2.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                accessRule = new AccessRuleFactory().AddAllowByQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), new[] { Permissions.Read }, report1);

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule.ControlAccess = entityType2.As<SecurableEntity>();
                accessRule.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_SecurableEntityChange_ChangeTypeAuth()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            IEntity entity1;
            AccessRule accessRule;
            Report report1;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                accessRule = new AccessRuleFactory().AddAllowByQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), new[] { Permissions.Read }, report1);

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entityType1.ControlAccessBy.Remove(accessRule);
                entityType1.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entries not removed from cache for user 1");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_MultiTypeInvalidation()
        {
            UserAccount userAccount1;
            EntityType entityType1;
            EntityType entityType2;
            IEntity entity1;
            IEntity entity2;
            AccessRule accessRule1;
            AccessRule accessRule2;
            Report report1;
            Report report2;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                entityType2 = new EntityType();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();

                entity2 = Entity.Create(entityType2);
                entity2.Save();

                report2 = TestQueries.Entities().ToReport();
                report2.Save();

                accessRule1 = new AccessRuleFactory().AddAllowByQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), new[] { Permissions.Read }, report1);
                accessRule2 = new AccessRuleFactory().AddAllowByQuery(userAccount1.As<Subject>(),
                    entityType2.As<SecurableEntity>(), new[] { Permissions.Read, Permissions.Modify }, report2);

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity2)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType2.Id),
                    "Read entry not added to cache for user 1/entity type 2");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule1.AccessRuleEnabled = false;
                accessRule1.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Entries not removed from cache for user 1, entity type 1");

            // Ideally, these should not be invalidated but there is not much we can do about it
            // at this time.
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0) // ideally, should be 1
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType2.Id),
                    "Read entry removed from cache for user 1, entity type 2");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0) // ideally, should be 1
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Modify.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType2.Id),
                    "Modify entry from cache for user 1, entity type 2");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_PartialInvalidation()
        {
            UserAccount userAccount1;
            UserAccount userAccount2;
            EntityType entityType1;
            EntityType entityType2;
            IEntity entity1;
            IEntity entity2;
            AccessRule accessRule1;
            AccessRule accessRule2;
            Report report1;
            Report report2;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                userAccount1 = new UserAccount();
                userAccount1.Name = Guid.NewGuid().ToString();
                userAccount1.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entity1 = Entity.Create(entityType1);
                entity1.Save();

                report1 = TestQueries.Entities().ToReport();
                report1.Save();

                userAccount2 = new UserAccount();
                userAccount2.Name = Guid.NewGuid().ToString();
                userAccount2.Save();

                entityType2 = new EntityType();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();

                entity2 = Entity.Create(entityType2);
                entity2.Save();

                report2 = TestQueries.Entities().ToReport();
                report2.Save();

                accessRule1 = new AccessRuleFactory().AddAllowByQuery(userAccount1.As<Subject>(),
                    entityType1.As<SecurableEntity>(), new[] { Permissions.Read }, report1);
                accessRule2 = new AccessRuleFactory().AddAllowByQuery(userAccount2.As<Subject>(),
                    entityType2.As<SecurableEntity>(), new[] { Permissions.Read }, report2);

                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount1))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)), Throws.Nothing);
            }
            using(new SetUser(userAccount2))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity2)), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Read entry not added to cache for user 1/entity type 1");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount2.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permissions.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType2.Id),
                    "Read entry not added to cache for user 2/entity type 2");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule1.AccessRuleEnabled = false;
                accessRule1.Save();
                ctx.CommitTransaction();
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount1.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType1.Id),
                    "Entries not removed from cache for user 1, entity type 1");
            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount2.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType2.Id),
                    "Entries not removed from cache for user 2, entity type 2");

            // Ensure the whole cache has not been invalidated
            Assert.That(cachingQueryRepository.Cache, Is.Not.Empty, "Removed all entries");
        }
        
        private void TraceEntries(Func<KeyValuePair<SubjectPermissionTypesTuple, IEnumerable<StructuredQuery>>, bool> predicate, 
            CachingQueryRepository cachingQueryRepository)
        {
            foreach (KeyValuePair<SubjectPermissionTypesTuple, IEnumerable<StructuredQuery>> keyValuePair in
                cachingQueryRepository.Cache
                .Select( kvp => new KeyValuePair<SubjectPermissionTypesTuple, IEnumerable<StructuredQuery>>( kvp.Key, kvp.Value.Select( q => q.Query ) ) )
                .Where( predicate ) )
            {
                Debug.WriteLine(string.Format("Subject {0}, Permission {1}, Entities: {2} : {3} queries", 
                    Entity.Get<Subject>(keyValuePair.Key.SubjectId).Name,
                    Entity.Get<Permission>(keyValuePair.Key.PermissionId).Name,
                    string.Join(",", keyValuePair.Key.EntityTypes.Select(e => Entity.Get<Resource>(e).Name)), 
                    keyValuePair.Value.Count()));
            }            
        }

        private void TraceIds(IDictionary<string, IEntity> map)
        {
            foreach (KeyValuePair<string, IEntity> kvp in map)
            {
                Trace.WriteLine(string.Format("{0}: {1}", kvp.Key, kvp.Value.Id));
            }
        }

        private void TraceCacheInvalidationEntries(string name, IEntity entity, CachingQueryRepository cachingQueryRepository)
        {
            Trace.WriteLine(string.Format("Cache invalidation entries for {0} ({1}): {2}", name, entity.Id,
                string.Join(",", ((SecurityQueryCacheInvalidator)cachingQueryRepository.CacheInvalidator).EntityToCacheKey.GetValues(entity.Id))));
        }
    }
}
