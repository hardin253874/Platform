// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.Diagnostics;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using EDC.ReadiNow.Test.Model.CacheInvalidation;
using Moq;
using NUnit.Framework;
using ConfigurationSettings = EDC.ReadiNow.Configuration.ConfigurationSettings;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class EntityAccessControlServiceTests
    {
        [Test]
        public void Test_Creation()
        {
            EntityAccessControlService entityAccessControlService;
            IEntityAccessControlChecker entityAccessControlChecker;

            entityAccessControlChecker = new EntityAccessControlChecker();
            entityAccessControlService = new EntityAccessControlService(entityAccessControlChecker);
            Assert.That(entityAccessControlService,
                Has.Property("EntityAccessControlChecker").EqualTo(entityAccessControlChecker));
            Assert.That(entityAccessControlService,
                Has.Property("TraceLevelFactory").EqualTo((Func<int>)entityAccessControlService.GetTraceLevelSetting));
            Assert.That(entityAccessControlService,
                Has.Property("TraceLevel").Not.Null);
        }

        [Test]
        public void Test_Creation_NullEntityAccessControlService()
        {
            Assert.That(() => new EntityAccessControlService(null), 
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityAccessControlChecker"));
        }

        [Test]
        public void Test_Check_SingleEntity_NullEntity()
        {
            Assert.That(() => new EntityAccessControlService().Check((EntityRef) null, new EntityRef[0]),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entity"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Check_SingleEntity_NullPermissions()
        {
            Assert.That(() => new EntityAccessControlService().Check(new EntityRef(1), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        public void Test_Check_SingleEntity_NoRequestContext()
        {
            Assert.That(() => new EntityAccessControlService().Check(new EntityRef(1), new [] { Permissions.Read }),
                Throws.InvalidOperationException);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Check_SingleEntity()
        {
            EntityAccessControlService entityAccessControlService;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            Resource entity;
            EntityRef[] permissions;

            using (DatabaseContext.GetContext(true))
            {
                entity = Entity.Create<Resource>();
                permissions = new[] { Permissions.Read };

                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker.Setup(eacc => eacc.CheckAccess(
                                                It.Is<IList<EntityRef>>(e => e.SequenceEqual(new EntityRef[] { entity }, EntityRefComparer.Instance)),
                                                It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                                                It.IsAny<EntityRef>()))
                                          .Returns(new Dictionary<long, bool>() { { entity.Id, true } });

                entityAccessControlService = new EntityAccessControlService(entityAccessControlChecker.Object);

                Assert.That(entityAccessControlService.Check(entity, permissions), Is.True);

                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        public void Test_Check_MultipleEntities_NullEntity()
        {
            Assert.That(() => new EntityAccessControlService().Check((EntityRef[])null, new EntityRef[0]),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        public void Test_Check_MultipleEntities_EntitiesContainsNull()
        {
            Assert.That(() => new EntityAccessControlService().Check(new EntityRef[] { null }, new EntityRef[] { null }),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Check_MultipleEntities_NullPermissions()
        {
            Assert.That(() => new EntityAccessControlService().Check(new EntityRef[]{ 1 }, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        public void Test_Check_MultipleEntities_PermissionsContainsNull()
        {
            Assert.That(() => new EntityAccessControlService().Check(new EntityRef[] { 1 }, new EntityRef[] { null }),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        public void Test_Check_MultipleEntities_NoRequestContext()
        {
            Assert.That(() => new EntityAccessControlService().Check(new EntityRef[] { 1 }, new[] { Permissions.Read }),
                Throws.InvalidOperationException);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Check_MultipleEntities()
        {
            EntityAccessControlService entityAccessControlService;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                entities = new EntityRef[] { 1 };
                permissions = new[] { Permissions.Read };

                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker.Setup(eacc => eacc.CheckAccess(
                                                It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                                                It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                                                It.IsAny<EntityRef>()))
                                          .Returns(new Dictionary<long, bool>() { { entities[0].Id, true } });

                entityAccessControlService = new EntityAccessControlService(entityAccessControlChecker.Object);

                result = entityAccessControlService.Check(entities, permissions);
                Assert.That(result, Has.Property("Count").EqualTo(1));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").EqualTo(true));

                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        public void Test_Demand_NullEntity()
        {
            Assert.That(() => new EntityAccessControlService().Demand((EntityRef[])null, new EntityRef[0]),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Demand_NullPermissions()
        {
            Assert.That(() => new EntityAccessControlService().Demand(new EntityRef[] { 1 }, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        public void Test_Demand_NoRequestContext()
        {
            Assert.That(() => new EntityAccessControlService().Demand(new EntityRef[] { 1 }, new[] { Permissions.Read }),
                Throws.InvalidOperationException);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Demand_Success()
        {
            EntityAccessControlService entityAccessControlService;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;

            using (DatabaseContext.GetContext(true))
            {
                entities = new EntityRef[] { 1 };
                permissions = new[] { Permissions.Read };

                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker.Setup(eacc => eacc.CheckAccess(
                                                It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                                                It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                                                It.IsAny<EntityRef>()))
                                          .Returns(new Dictionary<long, bool>() { { entities[0].Id, true } });

                entityAccessControlService = new EntityAccessControlService(entityAccessControlChecker.Object);

                Assert.That(() => entityAccessControlService.Demand(entities, permissions), Throws.Nothing);

                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Demand_Failure()
        {
            EntityAccessControlService entityAccessControlService;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;

            entities = new EntityRef[] { 1 };
            permissions = new[] { Permissions.Read };

            entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
            entityAccessControlChecker.Setup(eacc => eacc.CheckAccess(
                                            It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                                            It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                                            It.IsAny<EntityRef>()))
                                        .Returns(new Dictionary<long, bool>() { { entities[0].Id, false } });

            entityAccessControlService = new EntityAccessControlService(entityAccessControlChecker.Object);

            Assert.That(() => entityAccessControlService.Demand(entities, permissions), 
                Throws.TypeOf<PlatformSecurityException>());

            entityAccessControlChecker.VerifyAll();
        }

        [Test]
        public void Test_CanCreate_NullEntityType()
        {
            Assert.That(() => new EntityAccessControlService().CanCreate((EntityType) null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityType"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CanCreate_NullEntityTypes()
        {
            Assert.That(() => new EntityAccessControlService().CanCreate((IList<EntityType>) null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CanCreateEntityType()
        {
            EntityAccessControlService entityAccessControlService;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityType entityType;

            entityType = Entity.Get<EntityType>("core:person");
            Assert.That(entityType, Is.Not.Null, "Person type not found");

            entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
            entityAccessControlChecker.Setup(eacc => eacc.CheckTypeAccess(
                                            It.Is<IList<EntityType>>(ets => ets.SequenceEqual(new [] { entityType }, new EntityEqualityComparer())),
                                            It.Is<EntityRef>( perm => perm.Id == Permissions.Create.Id ),
                                            It.IsAny<EntityRef>()))
                                        .Returns<IList<EntityType>, EntityRef, EntityRef>(
                                            (ets, perm, user) => ets.ToDictionary(et => et.Id, et => true));

            entityAccessControlService = new EntityAccessControlService(entityAccessControlChecker.Object);

            Assert.That(entityAccessControlService.CanCreate(entityType), Is.True);

            entityAccessControlChecker.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CanCreateEntityTypes()
        {
            EntityAccessControlService entityAccessControlService;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityType entityType;

            entityType = Entity.Get<EntityType>("core:person");
            Assert.That(entityType, Is.Not.Null, "Person type not found");

            entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
            entityAccessControlChecker.Setup(eacc => eacc.CheckTypeAccess(
                                            It.Is<IList<EntityType>>(ets => ets.SequenceEqual(new [] { entityType }, new EntityEqualityComparer())),
                                            It.Is<EntityRef>( perm => perm.Id == Permissions.Create.Id ),
                                            It.IsAny<EntityRef>()))
                                        .Returns<IList<EntityType>, EntityRef, EntityRef>(
                                            (ets, perm, user) => new Dictionary<long, bool>{ { entityType.Id, true }});

            entityAccessControlService = new EntityAccessControlService(entityAccessControlChecker.Object);

            Assert.That(entityAccessControlService.CanCreate(new [] { entityType }), 
                Is.EquivalentTo(new [] { new KeyValuePair<long, bool>(entityType.Id, true) }));

            entityAccessControlChecker.VerifyAll();
        }


        [Test]
        public void Test_GetTraceCacheInvalidationSetting()
        {
            int oldSetting;
            int newSetting;
            EntityAccessControlService entityAccessControlService;

            oldSetting = ConfigurationSettings.GetServerConfigurationSection().Security.Trace;
            try
            {
                newSetting = oldSetting + 1;
                ConfigurationSettings.GetServerConfigurationSection().Security.Trace = newSetting;

                entityAccessControlService = new EntityAccessControlService();

                Assert.That(entityAccessControlService.GetTraceLevelSetting(), Is.EqualTo(newSetting));
            }
            finally 
            {
                ConfigurationSettings.GetServerConfigurationSection().Security.Trace = oldSetting;
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(SecurityTraceLevel.None)]
        [TestCase(SecurityTraceLevel.DenyBasic)]
        [TestCase(SecurityTraceLevel.DenyVerbose)]
        [TestCase(SecurityTraceLevel.AllBasic)]
        [TestCase(SecurityTraceLevel.AllVerbose)]
        [FailOnEvent]
        public void Test_TraceCacheInvalidations_Check(SecurityTraceLevel traceLevel)
        {
            IList<IEntity> testEntities;
            IEntity testEntity1;
            IEntity testEntity2;
            EntityAccessControlService entityAccessControlService;
            UserAccount userAccount;
            int expectedOccurrences;

            userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            testEntity1 = Entity.Create<Folder>();
            testEntity1.Save();

            testEntity2 = Entity.Create<Folder>();
            testEntity2.Save();

            testEntities = new[] {testEntity1, testEntity2};

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int) traceLevel);

            using (new SetUser(userAccount))
            {
                entityAccessControlService.Check(
                    testEntities.Select(e => new EntityRef(e)).ToList(),
                    new[] {Permissions.Read});
            }

            expectedOccurrences = -1;
            switch (traceLevel)
            {
                case SecurityTraceLevel.DenyVerbose:
                case SecurityTraceLevel.DenyBasic:
                case SecurityTraceLevel.AllVerbose:
                case SecurityTraceLevel.AllBasic:
                    expectedOccurrences = 1;
                    break;
                case SecurityTraceLevel.None:
                    expectedOccurrences = 0;
                    break;
                default:
                    Assert.Fail("Unknown security trace level.");
                    break;
            }

            IList<LogActivityLogEntry> activityLogEntries;
            activityLogEntries = Entity.GetInstancesOfType<LogActivityLogEntry>().ToList();
            Assert.That(activityLogEntries,
                Has.Exactly(expectedOccurrences)
                    .Property("LogEntrySeverity_Enum").EqualTo(LogSeverityEnum_Enumeration.InformationSeverity).And
                    .Property("LogEventTime").EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(10)).And
                    .Property("Description").StartsWith(
                        string.Format(
                            "Access control check: Does user '{0}' have '{1}' access to entity(ies) '{2}'",
                            userAccount.Name, Permissions.Read.Alias,
                            string.Join(", ", testEntities.Select(x => x.Id)))));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(SecurityTraceLevel.None)]
        [TestCase(SecurityTraceLevel.DenyBasic)]
        [TestCase(SecurityTraceLevel.DenyVerbose)]
        [TestCase(SecurityTraceLevel.AllBasic)]
        [TestCase(SecurityTraceLevel.AllVerbose)]
        [FailOnEvent]
        public void Test_TraceCacheInvalidations_CanCreate(SecurityTraceLevel traceLevel)
        {
            EntityType entityType;
            EntityAccessControlService entityAccessControlService;
            UserAccount userAccount;
            int expectedOccurrences;

            userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            entityType = new EntityType();
            entityType.Save();

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int)traceLevel);

            using (new SetUser(userAccount))
            {
                entityAccessControlService.CanCreate(entityType);
            }

            expectedOccurrences = -1;
            switch (traceLevel)
            {
                case SecurityTraceLevel.DenyVerbose:
                case SecurityTraceLevel.DenyBasic:
                case SecurityTraceLevel.AllVerbose:
                case SecurityTraceLevel.AllBasic:
                    expectedOccurrences = 1;
                    break;
                case SecurityTraceLevel.None:
                    expectedOccurrences = 0;
                    break;
                default:
                    Assert.Fail("Unknown security trace level.");
                    break;
            }

            IList<LogActivityLogEntry> activityLogEntries;
            activityLogEntries = Entity.GetInstancesOfType<LogActivityLogEntry>().ToList();
            Assert.That(activityLogEntries,
                Has.Exactly(expectedOccurrences)
                    .Property("LogEntrySeverity_Enum").EqualTo(LogSeverityEnum_Enumeration.InformationSeverity).And
                    .Property("LogEventTime").EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(10)).And
                    .Property("Description").StartsWith(
                        string.Format(
                            "Access control check: Does user '{0}' have '{1}' access to entity(ies) '{2}'",
                            userAccount.Name, Permissions.Create.Alias, entityType.Id)));
        }

        [Test]
        public void Test_Caches()
        {
            IList<ICacheService> caches;

            caches = Factory.Current.Resolve<IList<ICacheService>>( );
            Assert.That(caches, Has.Exactly(2).TypeOf<CachingEntityAccessControlChecker>());
            Assert.That(caches, Has.Exactly(1).TypeOf<CachingUserRoleRepository>());
            Assert.That(caches, Has.Exactly(2).TypeOf<CachingQueryRepository>());
            Assert.That(caches, Has.Exactly(1).TypeOf<CachingEntityMemberRequestFactory>());
        }

        [Test]
        public void Test_GetBehavior_Null()
        {
            EntityAccessControlService entityAccessControlService;

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int) SecurityTraceLevel.None);            

            Assert.That(
                () => entityAccessControlService.GetBehavior(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityIds"));
        }

        [Test]
        public void Test_GetBehavior_RequestContextNotSet()
        {
            EntityAccessControlService entityAccessControlService;

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int)SecurityTraceLevel.None);

            Assert.That(
                () => entityAccessControlService.GetBehavior(new long[0]),
                Throws.InvalidOperationException);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetBehavior_Empty()
        {
            EntityAccessControlService entityAccessControlService;

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int)SecurityTraceLevel.None);

            Assert.That(
                () => entityAccessControlService.GetBehavior(new long[0]),
                Is.EqualTo(MessageContextBehavior.New));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetBehavior_ForceTrace_Overlap()
        {
            EntityAccessControlService entityAccessControlService;

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int) SecurityTraceLevel.None);

            using (new ForceSecurityTraceContext(1))
            {
                Assert.That(
                    () => entityAccessControlService.GetBehavior(new long[] {1}),
                    Is.EqualTo(MessageContextBehavior.New | MessageContextBehavior.Capturing));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetBehavior_ForceTrace_NoOverlap()
        {
            EntityAccessControlService entityAccessControlService;
            const long entityId = 1;

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int)SecurityTraceLevel.None);

            using (new ForceSecurityTraceContext(entityId))
            {
                Assert.That(
                    () => entityAccessControlService.GetBehavior(new long[] { entityId + 1 }),
                    Is.EqualTo(MessageContextBehavior.New ));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetBehavior_Inspect_Overlap()
        {
            EntityAccessControlService entityAccessControlService;
            EventLogSettings eventLogSettings;
            IEntity testEntity;

            testEntity = Entity.Get<Resource>("core:name");

            eventLogSettings = Entity.Get<EventLogSettings>(ForceSecurityTraceContext.EventLogSettingsAlias, true);
            eventLogSettings.InspectSecurityChecksOnResource.Add(Entity.Get<Resource>(testEntity.Id));
            eventLogSettings.Save();

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int)SecurityTraceLevel.None);

            Assert.That(
                () => entityAccessControlService.GetBehavior(new [] { testEntity.Id }),
                Is.EqualTo(MessageContextBehavior.New | MessageContextBehavior.Capturing));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetBehavior_Inspect_NoOverlap()
        {
            EntityAccessControlService entityAccessControlService;
            EventLogSettings eventLogSettings;
            IEntity testEntity;

            testEntity = Entity.Get<Resource>("core:name");

            eventLogSettings = Entity.Get<EventLogSettings>(ForceSecurityTraceContext.EventLogSettingsAlias, true);
            eventLogSettings.InspectSecurityChecksOnResource.Add(Entity.Get<Resource>("core:name"));
            eventLogSettings.Save();

            Thread.Sleep( new TimeSpan( ForceSecurityTraceContext.TicksToWaitBeforeRefreshing ) );

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int)SecurityTraceLevel.None);

            Assert.That(
                () => entityAccessControlService.GetBehavior(new [] { testEntity.Id + 1 }),
                Is.EqualTo(MessageContextBehavior.New));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetBehavior_SecurityBypass()
        {
            EntityAccessControlService entityAccessControlService;
            EventLogSettings eventLogSettings;
            IEntity testEntity;

            testEntity = Entity.Get<Resource>("core:name");

            eventLogSettings = Entity.Get<EventLogSettings>(ForceSecurityTraceContext.EventLogSettingsAlias, true);
            eventLogSettings.InspectSecurityChecksOnResource.Add(Entity.Get<Resource>("core:name"));
            eventLogSettings.Save();

            entityAccessControlService = new EntityAccessControlService(
                new EntityAccessControlChecker(), () => (int)SecurityTraceLevel.None);

            using (new SecurityBypassContext())
            {
                Assert.That(
                    () => entityAccessControlService.GetBehavior(new[] {testEntity.Id }),
                    Is.EqualTo(MessageContextBehavior.New));
            }
        }
    }
}
