// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.Database;
using EDC.Database.Types;
using EDC.Diagnostics;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using EDC.Collections.Generic;
using Quartz.Util;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class EntityAccessControlCheckerTests
    {
        [Test]
        public void Test_Creation()
        {
            EntityAccessControlChecker entityAccessControlChecker;

            entityAccessControlChecker = null;
            Assert.That(() => entityAccessControlChecker = new EntityAccessControlChecker(), Throws.Nothing);
            Assert.That(entityAccessControlChecker, Has.Property("RoleRepository").Not.Null);
            Assert.That(entityAccessControlChecker, Has.Property("QueryRepository").Not.Null);
            Assert.That(entityAccessControlChecker, Has.Property("EntityTypeRepository").Not.Null);
        }

        [Test]
        public void Test_Creation_NullRoleRepo()
        {
            Assert.That(() => new EntityAccessControlChecker(null, new QueryRepository(), new EntityTypeRepository()),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("roleRepository"));
        }

        [Test]
        public void Test_Creation_NullQueryRepo()
        {
            Assert.That(() => new EntityAccessControlChecker(new UserRoleRepository(), null, new EntityTypeRepository()),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("queryRepository"));
        }

        [Test]
        public void Test_Creation_NullEntityTypeRepo()
        {
            Assert.That(() => new EntityAccessControlChecker(new UserRoleRepository(), new QueryRepository(), null),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypeRepository"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullEntities()
        {
            Assert.That(
                () =>
                new EntityAccessControlChecker().CheckAccess(null, new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_EntitiesContainsNull()
        {
            Assert.That(
                () =>
                new EntityAccessControlChecker().CheckAccess(new Collection<EntityRef>() {null},
                                                             new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullPermissions()
        {
            Assert.That(
                () =>
                new EntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), null,
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullUser()
        {
            Assert.That(
                () =>
                new EntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), new Collection<EntityRef>(),
                                                             null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NoEntities()
        {
            MockRepository mockRepository;
            EntityAccessControlChecker entityAccessControlChecker;
            Mock<IUserRoleRepository> roleRepository;
            Mock<IQueryRepository> queryRepository;
            Mock<IEntityTypeRepository> entityTypeRepository;
            IDictionary<long, bool> result;
            UserAccount userAccount;

            userAccount = Entity.Get<UserAccount>(RequestContext.GetContext().Identity.Id);

            mockRepository = new MockRepository(MockBehavior.Strict);

            roleRepository = mockRepository.Create<IUserRoleRepository>();
            queryRepository = mockRepository.Create<IQueryRepository>();
            entityTypeRepository = mockRepository.Create<IEntityTypeRepository>();
            entityAccessControlChecker = new EntityAccessControlChecker(roleRepository.Object,
                                                                        queryRepository.Object,
                                                                        entityTypeRepository.Object);
            result = entityAccessControlChecker.CheckAccess(new Collection<EntityRef>(),
                                                            new[] { Permissions.Read },
                                                            userAccount);

            mockRepository.VerifyAll();

            Assert.That(result, Is.Empty);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NoPermissions()
        {
            MockRepository mockRepository;
            EntityAccessControlChecker entityAccessControlChecker;
            Mock<IUserRoleRepository> roleRepository;
            Mock<IQueryRepository> queryRepository;
            Mock<IEntityTypeRepository> entityTypeRepository;
            IDictionary<long, bool> result;
            EntityRef testEntity;
            UserAccount userAccount;

            userAccount = Entity.Get<UserAccount>(RequestContext.GetContext().Identity.Id);

            testEntity = new EntityRef(1);

            mockRepository = new MockRepository(MockBehavior.Strict);

            roleRepository = mockRepository.Create<IUserRoleRepository>();
            roleRepository.Setup(rr => rr.GetUserRoles(userAccount.Id)).Returns(() => new HashSet<long>());

            queryRepository = mockRepository.Create<IQueryRepository>();

            entityTypeRepository = mockRepository.Create<IEntityTypeRepository>();

            entityAccessControlChecker = new EntityAccessControlChecker(roleRepository.Object,
                                                                        queryRepository.Object,
                                                                        entityTypeRepository.Object);
            result = entityAccessControlChecker.CheckAccess(new[] {testEntity}, new Collection<EntityRef>(),
                                                            userAccount);

            mockRepository.VerifyAll();

            Assert.That(result,
                        Has.Exactly(1).Property("Key").EqualTo(testEntity.Id).And.Property("Value").EqualTo(false));
            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public void Test_SetAll_NullEntities()
        {
            Assert.That(() => new EntityAccessControlChecker().SetAll(null, false),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        public void Test_SetAll_EmptyEntities()
        {
            Assert.That(new EntityAccessControlChecker().SetAll(new long[0], false), Is.Empty);
        }

        [Test]
        public void Test_SetAll_Entities()
        {
            long[] entities = { 1, 2 };
            IDictionary<long, bool> result;

            result = new EntityAccessControlChecker().SetAll(entities, false);

            Assert.That(result.Keys, Is.EquivalentTo(entities));
            Assert.That(result.Values, Has.All.False);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_UserNoRelationships()
        {
            UserAccount userAccount;
            IEntity folder;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                userAccount = Entity.Create<UserAccount>();
                userAccount.Save();

                folder = Entity.Create<Folder>();
                folder.Save();

                result = new EntityAccessControlChecker().CheckAccess(new[] {new EntityRef(folder.Id)},
                                                                      new[] {Permissions.Read},
                                                                      new EntityRef(userAccount.Id));

                Assert.That(result[folder.Id], Is.False);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_AccessRuleOnly()
        {
            UserAccount userAccount;
            SecurableEntity securableEntity;
            IDictionary<long, bool> result;
            AccessRule accessRule;

            // Test:
            //
            //                                              Instance
            //                                                 ^
            //                                                 |
            //                                                 |
            //  UserAccount ------- AccessByQuery -----> SecurableEntity
            //                   
            //   (no PermissionAccess relationship to Create)
            //                   

            using (DatabaseContext.GetContext(true))
            {
                securableEntity = Entity.Create<SecurableEntity>();
                securableEntity.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.ControlAccess = securableEntity;
                accessRule.Save();

                userAccount = Entity.Create<UserAccount>();
                userAccount.AllowAccess.Add(accessRule);
                userAccount.Save();

                result = new EntityAccessControlChecker().CheckAccess(new[] { new EntityRef(securableEntity.Id) },
                                                                      new[] { Permissions.Create },
                                                                      new EntityRef(userAccount.Id));

                Assert.That(result[securableEntity.Id], Is.False);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_AccessByQueryOnly()
        {
            UserAccount userAccount;
            SecurableEntity securableEntity;
            IDictionary<long, bool> result;
            AccessRule accessRule;
            Report authReport;

            // Test:
            //
            //                                      Folder Instance
            //                                          ^
            //                                          |
            //                                          |
            //  UserAccount ----> AccessRule -----> FolderType
            //                   
            //             (no PermissionAccess relationship)
            //                   

            using (DatabaseContext.GetContext(true))
            {
                securableEntity = Entity.Create<SecurableEntity>();
                securableEntity.Save();

                authReport = Entity.Create<Report>();
                authReport.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.AccessRuleReport = authReport;
                accessRule.ControlAccess = securableEntity;
                // Do not set accessRule.PermissionAccess
                accessRule.Save();

                userAccount = Entity.Create<UserAccount>();
                userAccount.AllowAccess.Add(accessRule.As<AccessRule>());
                userAccount.Save();

                result = new EntityAccessControlChecker().CheckAccess(new[] {new EntityRef(securableEntity.Id)},
                                                                      new[] { Permissions.Read },
                                                                      new EntityRef(userAccount.Id));

                Assert.That(result[securableEntity.Id], Is.False);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_SecurityBypassContext()
        {
            EntityRef[] entityIds;
            IDictionary<long, bool> result;
            UserAccount userAccount;
            EntityAccessControlChecker entityAccessControlChecker;
            MockRepository mockRepository;

            entityIds = new EntityRef[] { 1, 2, 3 };

            mockRepository = new MockRepository(MockBehavior.Strict);

            entityAccessControlChecker = new EntityAccessControlChecker(
                mockRepository.Create<IUserRoleRepository>().Object, 
                mockRepository.Create<IQueryRepository>().Object, 
                mockRepository.Create<IEntityTypeRepository>().Object
            );

            userAccount = Entity.Create<UserAccount>();
            userAccount.Save();

            using (new SecurityBypassContext())
            {
                result = entityAccessControlChecker.CheckAccess(entityIds,
                                                                new[] { Permissions.Read },
                                                                userAccount);
            }

            Assert.That(result, Has.Count.EqualTo(entityIds.Count()));
            Assert.That(result, Has.All.Property("Value").True);

            mockRepository.VerifyAll();
        }

        [Test]
        public void Test_CheckAccessControlByQuery_NullPermission()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByQuery(0, null, 0, new EntityRef[0], new HashSet<long>(), new Dictionary<long, ISet<long>>(), new Dictionary<long, bool>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permission"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByQuery_NullResult()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByQuery(0, Permissions.Read, 0, new EntityRef[0], new HashSet<long>(), new Dictionary<long, ISet<long>>(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("result"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByQuery_NullEntities()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByQuery(0, Permissions.Read, 0, null, new HashSet<long>(), new Dictionary<long, ISet<long>>(), new Dictionary<long, bool>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByQuery_NullAllEntities()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByQuery(0, Permissions.Read, 0, new EntityRef[0], null, new Dictionary<long, ISet<long>>(), new Dictionary<long, bool>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("allEntities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByQuery_NullQueryResultsCache()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByQuery(0, Permissions.Read, 0, new EntityRef[0], new HashSet<long>(), null, new Dictionary<long, bool>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("queryResultsCache"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByQuery_EntitiesContainsNull()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByQuery(0, Permissions.Read, 0, new EntityRef[] { null }, new HashSet<long>(), new Dictionary<long, ISet<long>>(), new Dictionary<long, bool>()),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("entities"));
        }
        [Test]
        public void Test_CheckAccessControlByRelationship_NullPermission()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByRelationship(0, null, 0, new EntityRef[0], new Dictionary<long, bool>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permission"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByRelationship_NullEntities()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByRelationship(0, Permissions.Create, 0, null, new Dictionary<long, bool>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByRelationship_EntitiesContainsNull()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByRelationship(0, Permissions.Create, 0, new EntityRef[] { null }, new Dictionary<long, bool>()),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByRelationship_NullSecurableResult()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAccessControlByRelationship(0, Permissions.Create, 0, new EntityRef[0], null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("result"));
        }

        [Test]
        public void Test_CollateAccess_NullEntities()
        {
            Assert.That(() => new EntityAccessControlChecker().CollateAccess(null, new Dictionary<long, IDictionary<long, bool>>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        public void Test_CollateAccess_EntitiesContainsNull()
        {
            Assert.That(() => new EntityAccessControlChecker().CollateAccess(new EntityRef[] { null }, new Dictionary<long, IDictionary<long, bool>>()),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        public void Test_CollateAccess_NullPermisssionToAccess()
        {
            Assert.That(() => new EntityAccessControlChecker().CollateAccess(new EntityRef[0], null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissionToAccess"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CollateAccess_EmptyEntities()
        {
            IDictionary<long, bool> result;
            Dictionary<EntityRef, IDictionary<long, bool>> operationToAccess;
            EntityRef allowRead;

            allowRead = Permissions.Read;
            operationToAccess = new Dictionary<EntityRef, IDictionary<long, bool>>();
            operationToAccess[allowRead] = new Dictionary<long, bool>();
            operationToAccess[allowRead][1] = false;

            result = new EntityAccessControlChecker().CollateAccess(new EntityRef[0], new Dictionary<long, IDictionary<long, bool>>());
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Test_CollateAccess_EmptyOperationsToAccess()
        {
            IDictionary<long, bool> result;
            result = new EntityAccessControlChecker().CollateAccess(new EntityRef[] { 1 }, new Dictionary<long, IDictionary<long, bool>>());
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(1).And.Property("Value").False);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CollateAccess_SingleEntityAllowed()
        {
            IDictionary<long, bool> result;
            Dictionary<long, IDictionary<long, bool>> operationToAccess;
            EntityRef readPermission;
            const int entityId = 1;

            readPermission = Permissions.Read;
            operationToAccess = new Dictionary<long, IDictionary<long, bool>>();
            operationToAccess[readPermission.Id] = new Dictionary<long, bool>();
            operationToAccess[readPermission.Id][entityId] = true;

            result = new EntityAccessControlChecker().CollateAccess(new[] { new EntityRef(entityId) }, operationToAccess);
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entityId).And.Property("Value").True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CollateAccess_SingleEntityNotAllowed()
        {
            IDictionary<long, bool> result;
            Dictionary<long, IDictionary<long, bool>> operationToAccess;
            EntityRef readPermission;
            const int entityId = 1;

            readPermission = Permissions.Read;
            operationToAccess = new Dictionary<long, IDictionary<long, bool>>();
            operationToAccess[readPermission.Id] = new Dictionary<long, bool>();
            operationToAccess[readPermission.Id][entityId] = false;

            result = new EntityAccessControlChecker().CollateAccess(new[] { new EntityRef(entityId) }, new Dictionary<long, IDictionary<long, bool>>());
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entityId).And.Property("Value").False);
        }

        [Test]
        [TestCase(false, false, false, false, false, false)]
        [TestCase(true,  true,  true,  true,  true,  true)]
        [TestCase(true,  false, true,  true,  true,  false)]
        [TestCase(true,  true,  true,  false, true,  false)]
        [TestCase(true,  false, true,  false, true,  false)]
        [RunAsDefaultTenant]
        public void Test_CollateAccess_TwoEntitiesTwoOperations(bool entity1Read, bool entity2Read, bool entity1Modify, bool entity2Write, 
            bool expectedEntity1CombinedAccess, bool expectedEntity2CombinedAccess)
        {
            IDictionary<long, bool> result;
            Dictionary<long, IDictionary<long, bool>> operationToAccess;
            EntityRef readPermission;
            EntityRef modifyPermission;
            const int entity1Id = 1;
            const int entity2Id = 2;

            readPermission = Permissions.Read;
            modifyPermission = Permissions.Modify;

            operationToAccess = new Dictionary<long, IDictionary<long, bool>>();
            operationToAccess[readPermission.Id] = new Dictionary<long, bool>();
            operationToAccess[readPermission.Id][entity1Id] = entity1Read;
            operationToAccess[readPermission.Id][entity2Id] = entity2Read;
            operationToAccess[modifyPermission.Id] = new Dictionary<long, bool>();
            operationToAccess[modifyPermission.Id][entity1Id] = entity1Modify;
            operationToAccess[modifyPermission.Id][entity2Id] = entity2Write;

            result = new EntityAccessControlChecker().CollateAccess(new[] { new EntityRef(entity1Id), new EntityRef(entity2Id) }, operationToAccess);
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity1Id).And.Property("Value").EqualTo(expectedEntity1CombinedAccess));
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity2Id).And.Property("Value").EqualTo(expectedEntity2CombinedAccess));
        }

        private StructuredQuery AllEntities
        {
            get
            {
                Guid resourceGuid = Guid.NewGuid();
                StructuredQuery structuredQuery = new StructuredQuery
                    {
                        RootEntity = new ResourceEntity
                            {
                                EntityTypeId = new EntityRef("core:resource"),
                                NodeId = resourceGuid
                            },
                        SelectColumns = new List<SelectColumn>()
                    };

                structuredQuery.SelectColumns.Add(new SelectColumn
                    {
                        Expression = new IdExpression() {NodeId = resourceGuid}
                    });
                return structuredQuery;
            }
        }

        [Test]
        public void Test_AllowAccessToTemporaryIds_Null()
        {
            Assert.That(() => new EntityAccessControlChecker().AllowAccessToTemporaryIds(null),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("mapping"));
        }

        [Test]
        public void Test_AllowAccessToTemporaryIds()
        {
            const long nonTemporaryId = 1;
            long temporaryId = EntityTemporaryIdAllocator.AcquireId();
            Dictionary<long, bool> mapping;

            try
            {
                mapping = new Dictionary<long, bool>
                    {
                        { temporaryId, false},
                        { nonTemporaryId, false}
                    };

                new EntityAccessControlChecker().AllowAccessToTemporaryIds(mapping);

                Assert.That(mapping, Has.Property("Count").EqualTo(2));
                Assert.That(mapping, Has.Exactly(1).Property("Key").EqualTo(temporaryId).And.Property("Value").True);
                Assert.That(mapping, Has.Exactly(1).Property("Key").EqualTo(nonTemporaryId).And.Property("Value").False);
            }
            finally
            {
                EntityTemporaryIdAllocator.RelinquishId(temporaryId);
            }
        }

        [Test]
        public void Test_AllowAccessToInvalidIds_NullMapping()
        {
            IDictionary<long, ISet<EntityRef>> entityTypes = new Dictionary<long, ISet<EntityRef>>();

            Assert.That(() => new EntityAccessControlChecker().AllowAccessToTypelessIds(null, entityTypes),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("mapping"));
        }

        [Test]
        public void Test_AllowAccessToInvalidIds_EmptyMapping()
        {
            IDictionary<long, ISet<EntityRef>> entityTypes;
            Dictionary<long, bool> mapping;
            IDictionary<long, bool> result;

            mapping = new Dictionary<long, bool>();
            entityTypes = new Dictionary<long, ISet<EntityRef>>();
            entityTypes.Add( 1, new HashSet<EntityRef> { new EntityRef(1)});

            result = new EntityAccessControlChecker().AllowAccessToTypelessIds(mapping, entityTypes);
            Assert.That(result, Has.Count.EqualTo(mapping.Count));
        }

        [Test]
        public void Test_AllowAccessToInvalidIds_NoTypelessEntities()
        {
            IDictionary<long, ISet<EntityRef>> entityTypes;
            Dictionary<long, bool> mapping;
            IDictionary<long, bool> result;

            mapping = new Dictionary<long, bool>();
            mapping[1] = false;
            entityTypes = new Dictionary<long, ISet<EntityRef>>();

            result = new EntityAccessControlChecker().AllowAccessToTypelessIds(mapping, entityTypes);
            Assert.That(result, Has.Count.EqualTo(mapping.Count));
            Assert.That(result[1], Is.False);
        }

        [Test]
        public void Test_AllowAccessToInvalidIds_SingleTypelessEntity()
        {
            IDictionary<long, ISet<EntityRef>> entityTypes;
            Dictionary<long, bool> mapping;
            IDictionary<long, bool> result;
            const long testEntityId = 1;

            mapping = new Dictionary<long, bool>();
            mapping[testEntityId] = false;
            entityTypes = new Dictionary<long, ISet<EntityRef>>();
            entityTypes.Add(EntityTypeRepository.TypelessId, new HashSet<EntityRef> { new EntityRef(testEntityId) });

            result = new EntityAccessControlChecker().AllowAccessToTypelessIds(mapping, entityTypes);
            Assert.That(result, Has.Count.EqualTo(mapping.Count));
            Assert.That(result[testEntityId], Is.True);
        }

        [Test]
        public void Test_AllowAccessToInvalidIds_OneTypelessOneTypedEntity()
        {
            IDictionary<long, ISet<EntityRef>> entityTypes;
            Dictionary<long, bool> mapping;
            IDictionary<long, bool> result;
            const long typelessEntityId = 1;
            const long typedEntityId = 2;

            mapping = new Dictionary<long, bool>();
            mapping[typelessEntityId] = false;
            mapping[typedEntityId] = false;
            entityTypes = new Dictionary<long, ISet<EntityRef>>();
            entityTypes.Add(EntityTypeRepository.TypelessId, new HashSet<EntityRef> { new EntityRef(typelessEntityId) });

            result = new EntityAccessControlChecker().AllowAccessToTypelessIds(mapping, entityTypes);
            Assert.That(result, Has.Count.EqualTo(mapping.Count));
            Assert.That(result[typelessEntityId], Is.True);
            Assert.That(result[typedEntityId], Is.False);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_Mocked_TypelessEntity()
        {
            MockRepository mockRepository;
            EntityAccessControlChecker entityAccessControlChecker;
            Mock<IUserRoleRepository> roleRepository;
            Mock<IQueryRepository> queryRepository;
            Mock<IEntityTypeRepository> entityTypeRepository;
            IDictionary<long, bool> result;
            UserAccount userAccount;
            EntityRef[] entitiesToTest;
            long testId = EntityId.Max;

            userAccount = Entity.Get<UserAccount>(RequestContext.GetContext().Identity.Id);

            mockRepository = new MockRepository(MockBehavior.Strict);

            roleRepository = mockRepository.Create<IUserRoleRepository>();
            roleRepository.Setup(rr => rr.GetUserRoles(userAccount.Id)).Returns(() => new HashSet<long>());

            queryRepository = mockRepository.Create<IQueryRepository>();            

            entitiesToTest = new [] {new EntityRef(testId)};

            entityTypeRepository = mockRepository.Create<IEntityTypeRepository>();
            entityTypeRepository.Setup(etr => etr.GetEntityTypes(entitiesToTest))
                                .Returns(() => new Dictionary<long, ISet<EntityRef>>
                                    {
                                        { EntityTypeRepository.TypelessId, new HashSet<EntityRef> { new EntityRef(testId)} }
                                    });

            entityAccessControlChecker = new EntityAccessControlChecker(roleRepository.Object,
                                                                        queryRepository.Object,
                                                                        entityTypeRepository.Object);
            result = entityAccessControlChecker.CheckAccess(entitiesToTest,
                                                            new[] { Permissions.Read },
                                                            userAccount);

            mockRepository.VerifyAll();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[testId], Is.True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_TypelessEntity()
        {
            IDictionary<long, bool> result;
            long testId = EntityId.Max;

            result = new EntityAccessControlChecker().CheckAccess(new[] { new EntityRef(testId) },
                                                            new[] { Permissions.Read },
                                                            Entity.Get<UserAccount>(RequestContext.GetContext().Identity.Id));

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[testId], Is.True);
        }

        [Test]
        public void Test_SkipCheck_NullUser()
        {
            Assert.That(() => EntityAccessControlChecker.SkipCheck(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        public void Test_SkipCheck_GlobalUser()
        {
            EntityRef user;
            user = new EntityRef(0); // 0 = Global tenant

            Assert.That(EntityAccessControlChecker.SkipCheck(user), Is.True);
        }

        [Test]
        public void Test_SkipCheck_Bypass()
        {
            EntityRef user;
            user = new EntityRef(1);

            using (new SecurityBypassContext())
            {
                Assert.That(EntityAccessControlChecker.SkipCheck(user), Is.True);
            }
        }

        [Test]
        public void Test_SkipCheck_Fail()
        {
            EntityRef user;
            user = new EntityRef(1);

            Assert.That(EntityAccessControlChecker.SkipCheck(user), Is.False);
        }

        [Test]
        public void Test_CheckTypeAccess_NullEntityTypes()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckTypeAccess( (IList<EntityType>) null, new EntityRef(), new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullUser( )
        {
            Assert.That( ( ) => new EntityAccessControlChecker( ).CheckTypeAccess( new EntityType [ 0 ], new EntityRef( ), null ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "user" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullPermission( )
        {
            Assert.That(() => new EntityAccessControlChecker().CheckTypeAccess( new EntityType[0], null, new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permission"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CheckTypeAccess_NoAccess( )
        {
            IList<EntityType> entityTypes;
            UserAccount userAccount;
            const int numEntities = 2;

            entityTypes = new List<EntityType>();
            for (int i = 0; i < numEntities; i++)
            {
                EntityType entityType;

                entityType = new EntityType();
                entityType.Save();

                entityTypes.Add(entityType);
            }

            userAccount = new UserAccount();
            userAccount.Save();

            Assert.That(new EntityAccessControlChecker().CheckTypeAccess( entityTypes, Permissions.Create, userAccount),
                Is.EquivalentTo(entityTypes.ToDictionary(et => et.Id, et => false)));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CheckTypeAccess_WithAccess( )
        {
            IList<EntityType> entityTypes;
            UserAccount userAccount;
            const int numEntities = 2;

            entityTypes = new List<EntityType>();
            for (int i = 0; i < numEntities; i++)
            {
                EntityType entityType;

                entityType = new EntityType();
                entityType.Save();

                entityTypes.Add(entityType);
            }

            userAccount = new UserAccount();
            userAccount.Save();

            Assert.That(new EntityAccessControlChecker().CheckTypeAccess( entityTypes, Permissions.Create, userAccount),
                Is.EquivalentTo(entityTypes.ToDictionary(et => et.Id, et => false)));

            new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(), 
                entityTypes[0].As<SecurableEntity>());
            new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(),
                entityTypes[1].As<SecurableEntity>());

            Assert.That(new EntityAccessControlChecker().CheckTypeAccess( entityTypes, Permissions.Create, userAccount ),
                Is.EquivalentTo(entityTypes.ToDictionary(et => et.Id, et => true)));
        }


        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CheckTypeAccess_WithSomeAccess( )
        {
            IList<EntityType> entityTypes;
            UserAccount userAccount;
            const int numEntities = 2;

            entityTypes = new List<EntityType>();
            for (int i = 0; i < numEntities; i++)
            {
                EntityType entityType;

                entityType = new EntityType();
                entityType.Save();

                entityTypes.Add(entityType);
            }

            userAccount = new UserAccount();
            userAccount.Save();

            Assert.That(new EntityAccessControlChecker().CheckTypeAccess( entityTypes, Permissions.Create, userAccount),
                Is.EquivalentTo(entityTypes.ToDictionary(et => et.Id, et => false)));

            // Only grant access to the first element
            new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(),
                entityTypes[0].As<SecurableEntity>());

            Assert.That(new EntityAccessControlChecker().CheckTypeAccess( entityTypes, Permissions.Create, userAccount ),
                Is.EquivalentTo(entityTypes.ToDictionary(et => et.Id, et => et.Id == entityTypes.First().Id)));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CanCreate_InheritedTypes()
        {
            EntityType baseEntityType;
            EntityType derivedEntityType;
            UserAccount userAccount;

            derivedEntityType = new EntityType();            
            derivedEntityType.Name = "Derived Type " + Guid.NewGuid();
            derivedEntityType.Save();

            baseEntityType = new EntityType();            
            baseEntityType.Name = "Base Type " + Guid.NewGuid();
            baseEntityType.DerivedTypes.Add(derivedEntityType);
            baseEntityType.Inherits.Add(UserResource.UserResource_Type);
            baseEntityType.Save();

            derivedEntityType = Entity.Get<EntityType>(derivedEntityType.Id);

            Assert.That(derivedEntityType.GetAncestorsAndSelf(),
                Is.EquivalentTo(new [] { baseEntityType, derivedEntityType, UserResource.UserResource_Type, Entity.Get<EntityType>("core:resource") })
                  .Using(new EntityIdEqualityComparer<EntityType>()));
            Assert.That(derivedEntityType.IsDerivedFrom(baseEntityType),
                Is.True, "Not derived form base type");

            userAccount = new UserAccount();
            userAccount.Name = "Test User Account " + Guid.NewGuid();
            userAccount.Save();

            new AccessRuleFactory().AddAllowCreate(
                userAccount.As<Subject>(),
                baseEntityType.As<SecurableEntity>());

            using (new SetUser(userAccount))
            {
                Assert.That(
                    Factory.EntityAccessControlService.CanCreate(baseEntityType),
                    "Cannot create base type");
                Assert.That(
                    Factory.EntityAccessControlService.CanCreate(derivedEntityType),
                    "Cannot create derived type");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void CheckAutomaticAccess_NullPermission()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAutomaticAccess(
                    null, new KeyValuePair<long, ISet<EntityRef>>(), new Dictionary<long, IDictionary<long, bool>>() ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permission"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void CheckAutomaticAccess_NullPermissionToAccess()
        {
            Assert.That(() => new EntityAccessControlChecker().CheckAutomaticAccess(
                    Permissions.Read, new KeyValuePair<long, ISet<EntityRef>>(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissionToAccess"));
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("core:read", "core:report", true)] // Type
        [TestCase("core:read", "core:targetVariable", true)] // Relationship
        [TestCase("core:read", "core:name", true)] // Field
        [TestCase("core:read", "core:tenantAuditLogSettingsInstance", false)] // Neither
        [TestCase("core:read", "core:inLogActivityMessage", false)] // Neither
        [TestCase("core:modify", "core:report", false)] // Wrong permission
        [TestCase("core:delete", "core:name", false)]
        public void CheckAutomaticAccess_GrantType(string permissionEntityRef, string testEntityRef, bool expectedResult)
        {
            Permission permission;
            Dictionary<long, IDictionary<long, bool>> permissionToAccess;
            IDictionary<long, ISet<EntityRef>> entityTypes;
            EntityRef testType;
            EntityAccessControlChecker entityAccessControlChecker;

            permission = Entity.Get<Permission>(permissionEntityRef);
            Assert.That(permission, Is.Not.Null, "Unknown permission.");

            permissionToAccess = new Dictionary<long, IDictionary<long, bool>>();
            permissionToAccess[permission.Id] = new Dictionary<long, bool>();

            testType = new EntityRef(testEntityRef);
            entityTypes = new EntityTypeRepository().GetEntityTypes(new [] { testType });

            entityAccessControlChecker = new EntityAccessControlChecker();
            foreach (KeyValuePair<long, ISet<EntityRef>> entityType in entityTypes)
            {
               entityAccessControlChecker.CheckAutomaticAccess(
                    permission, entityType, permissionToAccess);
            }

            if (expectedResult)
            {
                Assert.That(
                    permissionToAccess[permission.Id],
                    Has.Some.Property("Key").EqualTo(testType.Id)
                        .And.Property("Value").EqualTo(expectedResult));
            }
            else
            {
                Assert.That(
                    permissionToAccess[permission.Id],
                    Has.None.Property("Key").EqualTo(testType.Id));
            }

        }
    }
}
