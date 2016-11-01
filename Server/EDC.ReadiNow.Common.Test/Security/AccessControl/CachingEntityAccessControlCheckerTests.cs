// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class CachingEntityAccessControlCheckerTests
    {
        [Test]
        public void Test_Ctor()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;

            cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker();
            Assert.That(cachingEntityAccessControlChecker, Has.Property("Cache").Empty);
            Assert.That(cachingEntityAccessControlChecker, Has.Property("CacheInvalidator").Not.Null);
            Assert.That(cachingEntityAccessControlChecker, Has.Property("Checker").Not.Null);
            Assert.That(cachingEntityAccessControlChecker, Has.Property("CacheName").EqualTo("Access control"));
        }

        [Test]
        public void Test_Ctor_NullChecker()
        {
            Assert.That(() => new CachingEntityAccessControlChecker(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityAccessControlChecker"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Test_Ctor_InvalidCacheName(string cacheName)
        {
            Assert.That(() => new CachingEntityAccessControlChecker(new EntityAccessControlChecker(), cacheName),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cacheName"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullEntities()
        {
            Assert.That(
                () =>
                new CachingEntityAccessControlChecker().CheckAccess(null, new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullPermissions()
        {
            Assert.That(
                () =>
                new CachingEntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), null,
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullUser()
        {
            Assert.That(
                () =>
                new CachingEntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), new Collection<EntityRef>(),
                                                             null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        public void Test_Creation_CacheCreated()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;

            cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker();
            Assert.That(cachingEntityAccessControlChecker, Has.Property("Cache").Not.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_SimpleNotCached()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                entities = new EntityRef[] { 1 };
                permissions = new[] {Permissions.Read};
                user = new EntityRef(2);

                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker
                    .Setup(eacc => eacc.CheckAccess(
                        It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                        It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                        It.IsAny<EntityRef>()))
                    .Returns(new Dictionary<long, bool>() {{entities.First().Id, true}});

                cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker(entityAccessControlChecker.Object);

                result = cachingEntityAccessControlChecker.CheckAccess(entities, permissions, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(1));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").EqualTo(true));

                // Ensure that it is cached
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Property("Count").EqualTo(1));
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[0].Id, permissions.Select(x => x.Id))) 
                        .And.Property("Value").True);

                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_SimpleCached()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                entities = new EntityRef[] { 1 };
                permissions = new[] { Permissions.Read };
                user = new EntityRef(2);

                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker
                    .Setup(eacc => eacc.CheckAccess(
                        It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                        It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                        It.IsAny<EntityRef>()))
                    .Returns(new Dictionary<long, bool>() { { entities.First().Id, true } });

                cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker(entityAccessControlChecker.Object);

                // Call twice
                result = cachingEntityAccessControlChecker.CheckAccess(entities, permissions, user);
                result = cachingEntityAccessControlChecker.CheckAccess(entities, permissions, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(1));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").EqualTo(true));

                // Ensure cache contains one entry
                Assert.That(cachingEntityAccessControlChecker.Cache, Has.Count.EqualTo(1));
                Assert.That(cachingEntityAccessControlChecker.Cache,
                            Has.Exactly(1)
                               .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[0].Id, permissions.Select(x => x.Id)))
                               .And.Property("Value").True);

                // Ensure that CheckAccess was only called once for each Setup call.
                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [Explicit("This test is failing on the TC builds but not in dev.")]
        public void Test_CheckAccess_MultiplePermissions()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                entities = new EntityRef[] { 1, 2 };
                permissions = new[] { Permissions.Read, Permissions.Modify };
                user = new EntityRef(100);

                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker
                    .Setup(eacc => eacc.CheckAccess(
                        It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                        It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                        It.IsAny<EntityRef>()))
                    .Returns(new Dictionary<long, bool>() { { entities[0].Id, true }, { entities[1].Id, false } });

                cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker(entityAccessControlChecker.Object);

                result = cachingEntityAccessControlChecker.CheckAccess(entities, permissions, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").EqualTo(true));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[1].Id).And.Property("Value").EqualTo(false));

                // Ensure that it is cached
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[0].Id, permissions.Select(x => x.Id) ))
                        .And.Property("Value").True);
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[1].Id, permissions.Select(x => x.Id) ))
                        .And.Property("Value").False);

                // Second call
                result = cachingEntityAccessControlChecker.CheckAccess(entities, permissions, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").EqualTo(true));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[1].Id).And.Property("Value").EqualTo(false));

                // Ensure that CheckAccess was only called once for each Setup call.
                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_DifferentEntities()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities1;
            EntityRef[] entities2;
            EntityRef[] permissions;
            EntityRef user;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                entities1 = new EntityRef[] { 1, 2 };
                entities2 = new EntityRef[] { 3, 4 };
                permissions = new[] { Permissions.Read };
                user = new EntityRef(100);

                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker
                    .Setup(eacc => eacc.CheckAccess(
                        It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities1, EntityRefComparer.Instance)),
                        It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                        It.IsAny<EntityRef>()))
                    .Returns(new Dictionary<long, bool>() { { entities1[0].Id, true }, { entities1[1].Id, false } });
                entityAccessControlChecker
                    .Setup(eacc => eacc.CheckAccess(
                        It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities2, EntityRefComparer.Instance)),
                        It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                        It.IsAny<EntityRef>()))
                    .Returns(new Dictionary<long, bool>() { { entities2[0].Id, false }, { entities2[1].Id, true } });

                cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker(entityAccessControlChecker.Object);

                result = cachingEntityAccessControlChecker.CheckAccess(entities1, permissions, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities1[0].Id).And.Property("Value").EqualTo(true));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities1[1].Id).And.Property("Value").EqualTo(false));

                // Ensure that it is cached
                Assert.That(cachingEntityAccessControlChecker.Cache, Has.Count.EqualTo(2), "Incorrect count after first check");
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities1[0].Id, permissions.Select(x => x.Id)))
                        .And.Property("Value").True, 
                    "No access to entities1[0]");
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities1[1].Id, permissions.Select(x => x.Id)))
                        .And.Property("Value").False, 
                    "No access to entities1[1]");

                // Second call
                result = cachingEntityAccessControlChecker.CheckAccess(entities2, permissions, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities2[0].Id).And.Property("Value").EqualTo(false));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities2[1].Id).And.Property("Value").EqualTo(true));

                // Ensure that the results of both calls are cached
                Assert.That(cachingEntityAccessControlChecker.Cache, Has.Count.EqualTo(4), "Incorrect count after second check");
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities1[0].Id, permissions.Select(x => x.Id)))
                        .And.Property("Value").True, 
                    "Incorrect result for to entities1[0]");
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities1[1].Id, permissions.Select(x => x.Id)))
                        .And.Property("Value").False,
                    "Incorrect result for to entities1[1]");
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities2[0].Id, permissions.Select(x => x.Id)))
                        .And.Property("Value").False,
                    "Incorrect result for to entities2[0]");
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities2[1].Id, permissions.Select(x => x.Id)))
                        .And.Property("Value").True,
                    "Incorrect result for to entities2[1]");

                // Ensure that CheckAccess was only called once for each Setup call.
                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_DecreasingPermissions()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions1;
            EntityRef[] permissions2;
            EntityRef user;
            IDictionary<long, bool> result;

            using (DatabaseContext.GetContext(true))
            {
                entities = new EntityRef[] { 1, 2 };
                permissions1 = new[] { Permissions.Read, Permissions.Modify };
                permissions2 = new[] { Permissions.Read };
                user = new EntityRef(100);

                // Simulate a case where the user has read access to both entities but not modify.
                entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
                entityAccessControlChecker
                    .Setup(eacc => eacc.CheckAccess(
                        It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                        It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions1, EntityRefComparer.Instance)),
                        It.IsAny<EntityRef>()))
                    .Returns(new Dictionary<long, bool>() { { entities[0].Id, false }, { entities[1].Id, false } });
                entityAccessControlChecker
                    .Setup(eacc => eacc.CheckAccess(
                        It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                        It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions2, EntityRefComparer.Instance)),
                        It.IsAny<EntityRef>()))
                    .Returns(new Dictionary<long, bool>() { { entities[0].Id, true }, { entities[1].Id, true } });

                cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker(entityAccessControlChecker.Object);

                result = cachingEntityAccessControlChecker.CheckAccess(entities, permissions1, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").EqualTo(false));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[1].Id).And.Property("Value").EqualTo(false));

                // Ensure that it is cached
                Assert.That(cachingEntityAccessControlChecker.Cache, 
                    Has.Property("Count").EqualTo(2));
                Assert.That(cachingEntityAccessControlChecker.Cache, 
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[0].Id, permissions1.Select(x => x.Id)))
                        .And.Property("Value").False);
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[1].Id, permissions1.Select(x => x.Id)))
                        .And.Property("Value").False);

                // Second call
                result = cachingEntityAccessControlChecker.CheckAccess(entities, permissions2, user);

                // Check results
                Assert.That(result, Has.Property("Count").EqualTo(2));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").EqualTo(true));
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[1].Id).And.Property("Value").EqualTo(true));

                // Ensure that the results of both calls are cached
                Assert.That(cachingEntityAccessControlChecker.Cache, 
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[0].Id, permissions1.Select(x => x.Id)))
                        .And.Property("Value").False);
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[0].Id, permissions2.Select(x => x.Id)))
                        .And.Property("Value").True);
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[1].Id, permissions1.Select(x => x.Id)))
                        .And.Property("Value").False);
                Assert.That(cachingEntityAccessControlChecker.Cache,
                    Has.Exactly(1)
                        .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entities[1].Id, permissions2.Select(x => x.Id)))
                        .And.Property("Value").True);

                // Ensure that CheckAccess was only called once for each Setup call.
                entityAccessControlChecker.VerifyAll();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Clear()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;

            entities = new EntityRef[] { 1 };
            permissions = new[] { Permissions.Read };
            user = new EntityRef(2);

            entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
            entityAccessControlChecker
                .Setup(eacc => eacc.CheckAccess(
                    It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                    It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                    It.IsAny<EntityRef>()))
                .Returns(new Dictionary<long, bool>() { { entities.First().Id, true } });

            cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker(entityAccessControlChecker.Object);
            cachingEntityAccessControlChecker.CheckAccess(entities, permissions, user);

            // Sanity check
            Assert.That(cachingEntityAccessControlChecker.Cache, Has.Property("Count").Positive, "Not cached");

            cachingEntityAccessControlChecker.Clear();

            Assert.That(cachingEntityAccessControlChecker.Cache, Has.Property("Count").EqualTo(0), "Not cleared");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NoCachingWhenBypassSet()
        {
            CachingEntityAccessControlChecker cachingEntityAccessControlChecker;
            Mock<IEntityAccessControlChecker> entityAccessControlChecker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;

            entities = new EntityRef[] { 1 };
            permissions = new[] { Permissions.Read };
            user = new EntityRef(2);

            entityAccessControlChecker = new Mock<IEntityAccessControlChecker>(MockBehavior.Strict);
            entityAccessControlChecker
                .Setup(eacc => eacc.CheckAccess(
                    It.Is<IList<EntityRef>>(e => e.SequenceEqual(entities, EntityRefComparer.Instance)),
                    It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissions, EntityRefComparer.Instance)),
                    It.IsAny<EntityRef>()))
                .Returns(new Dictionary<long, bool>() { { entities.First().Id, true } });

            cachingEntityAccessControlChecker = new CachingEntityAccessControlChecker(entityAccessControlChecker.Object);

            Assert.That(cachingEntityAccessControlChecker, Has.Property("Cache").Empty,
                "Cache not empty before test");

            using (new SecurityBypassContext())
            {
                cachingEntityAccessControlChecker.CheckAccess(entities, permissions, user);
            }

            Assert.That(cachingEntityAccessControlChecker, Has.Property("Cache").Empty,
                "Cache not empty after test");

            entityAccessControlChecker.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullEntityTypes()
        {
            Assert.That(() => new CachingEntityAccessControlChecker().CheckTypeAccess( null, new EntityRef( ), new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullUser( )
        {
            Assert.That(() => new CachingEntityAccessControlChecker().CheckTypeAccess( new EntityType[0], new EntityRef( ), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullPermission( )
        {
            Assert.That( ( ) => new CachingEntityAccessControlChecker( ).CheckTypeAccess( new EntityType [ 0 ], null, new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "permission" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_SingleUncached( )
        {
            CachingEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            IList<EntityType> entityTypes;
            EntityRef user;
            EntityRef permission;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> canCreate;

            entityTypes = new[] { new EntityType() };
            user = new EntityRef(100);
            permission = new EntityRef( 100 );
            canCreate = c => c.CheckTypeAccess( It.Is<IList<EntityType>>(ets => ets.SequenceEqual(entityTypes)), permission, user );

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockChecker = mockRepository.Create<IEntityAccessControlChecker>();
            mockChecker
                .Setup(canCreate)
                .Returns(entityTypes.ToDictionary(et => et.Id, et => true));

            checker = new CachingEntityAccessControlChecker(mockChecker.Object);
            Assert.That(checker.CheckTypeAccess( entityTypes, permission, user ), Is.EquivalentTo(entityTypes.ToDictionary(et => et.Id, et => true)));

            mockRepository.VerifyAll();
            mockChecker.Verify(canCreate, Times.Once);
            Assert.That(checker.Cache.Count, Is.EqualTo(1), "Incorrect cache count");
            Assert.That(
                checker.Cache,
                Has.Exactly(1)
                   .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entityTypes.First().Id, new[] { permission.Id }))
                   .And.Property("Value").EqualTo(true), "Not cached");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_SingleCached( )
        {
            CachingEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            IList<EntityType> entityTypes;
            EntityRef user;
            EntityRef permission;

            entityTypes = new[] { new EntityType() };
            user = new EntityRef(100);
            permission = new EntityRef( 101 );

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockChecker = mockRepository.Create<IEntityAccessControlChecker>();

            checker = new CachingEntityAccessControlChecker(mockChecker.Object);
            checker.Cache.Add(new UserEntityPermissionTuple(user.Id, entityTypes.First().Id, new[] { permission.Id }), false);
            Assert.That(checker.CheckTypeAccess( entityTypes, permission, user ), Is.EquivalentTo(entityTypes.ToDictionary(et => et.Id, et => false)));

            mockRepository.VerifyAll();
            Assert.That(checker.Cache.Count, Is.EqualTo(1), "Incorrect cache count");
            Assert.That(
                checker.Cache,
                Has.Exactly(1)
                   .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entityTypes.First().Id, new[] { permission.Id }))
                   .And.Property("Value").EqualTo(false), "Not cached");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_MultipleMixed( )
        {
            CachingEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            IList<EntityType> entityTypes;
            EntityRef user;
            EntityRef permission;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> canCreate;

            entityTypes = new[] { new EntityType(), new EntityType(),  };
            user = new EntityRef(100);
            permission = new EntityRef( 101 );
            canCreate = c => c.CheckTypeAccess( It.Is<IList<EntityType>>(ets => ets.SequenceEqual(entityTypes.Skip(1))), permission, user );

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockChecker = mockRepository.Create<IEntityAccessControlChecker>();
            mockChecker
                .Setup(canCreate)
                .Returns(entityTypes.Skip( 1 ).ToDictionary(et => et.Id, et => true));

            checker = new CachingEntityAccessControlChecker(mockChecker.Object);
            checker.Cache.Add(new UserEntityPermissionTuple(user.Id, entityTypes[0].Id, new[] { permission.Id }), false);

            IDictionary<long, bool> result = checker.CheckTypeAccess( entityTypes, permission, user );
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(
                result, 
                Has.Exactly(1)
                   .Property("Key").EqualTo(entityTypes[0].Id)
                   .And.Property("Value").EqualTo(false));
            Assert.That(
                result,
                Has.Exactly(1)
                   .Property("Key").EqualTo(entityTypes[1].Id)
                   .And.Property("Value").EqualTo(true));

            mockRepository.VerifyAll();
            Assert.That(checker.Cache.Count, Is.EqualTo(2), "Incorrect cache count");
            Assert.That(
                checker.Cache,
                Has.Exactly(1)
                   .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entityTypes[0].Id, new[] { permission.Id }))
                   .And.Property("Value").EqualTo(false), "Not cached");
            Assert.That(
                checker.Cache,
                Has.Exactly(1)
                   .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entityTypes[1].Id, new[] { permission.Id }))
                   .And.Property("Value").EqualTo(true), "Not cached");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_SecurityBypass( )
        {
            CachingEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            IList<EntityType> entityTypes;
            EntityRef user;
            EntityRef permission;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> canCreate;
            IDictionary<long, bool> result;

            entityTypes = new[] { new EntityType(), new EntityType() };
            user = new EntityRef(100);
            permission = new EntityRef( 101 );
            canCreate = c => c.CheckTypeAccess( It.Is<IList<EntityType>>(ets => ets.SequenceEqual(entityTypes)), permission, user );

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockChecker = mockRepository.Create<IEntityAccessControlChecker>();
            mockChecker.Setup(canCreate).Returns(entityTypes.ToDictionary(et => et.Id, et => true));

            checker = new CachingEntityAccessControlChecker(mockChecker.Object);
            checker.Cache.Add(new UserEntityPermissionTuple(user.Id, entityTypes[0].Id, new[] { permission.Id }), false);

            using (new SecurityBypassContext())
            {
                result = checker.CheckTypeAccess( entityTypes, permission, user );
            }
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result, Has.All.Property("Value").EqualTo(true));
            Assert.That(result.Keys, Is.EquivalentTo(entityTypes.Select(et => et.Id)));

            mockRepository.VerifyAll();
            mockChecker.Verify(canCreate, Times.Once);
            Assert.That(checker.Cache.Count, Is.EqualTo(1), "Cache contents changed");
            Assert.That(
                checker.Cache,
                Has.Exactly(1)
                   .Property("Key").EqualTo(new UserEntityPermissionTuple(user.Id, entityTypes[0].Id, new[] { permission.Id }))
                   .And.Property("Value").EqualTo(false), "Not cached");
        }
    }
}

