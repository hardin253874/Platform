// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Diagnostics.Tracing;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class LoggingEntityAccessControlCheckerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Creation()
        {
            LoggingEntityAccessControlChecker checker;

            checker = new LoggingEntityAccessControlChecker();
            Assert.That(checker, Has.Property("Trace").Not.Null);
            Assert.That(checker, Has.Property("Checker").Not.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullEntities()
        {
            Assert.That(
                () =>
                new LoggingEntityAccessControlChecker().CheckAccess(null, new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullPermissions()
        {
            Assert.That(
                () =>
                new LoggingEntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), null,
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullUser()
        {
            Assert.That(
                () =>
                new LoggingEntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), new Collection<EntityRef>(),
                                                             null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess()
        {
            LoggingEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            Mock<IEntityAccessControlCheckTrace> mockTrace;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;

            entities = new EntityRef[] { 1 };
            permissions = new EntityRef[] { 2 };
            user = new EntityRef(100);

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockChecker = mockRepository.Create<IEntityAccessControlChecker>();
            mockChecker.Setup(c => c.CheckAccess(
                It.Is<IList<EntityRef>>(x => x.SequenceEqual(entities)),
                It.Is<IList<EntityRef>>(x => x.SequenceEqual(permissions)),
                user)).Returns(() => new Dictionary<long, bool>(){{entities[0].Id, true}});

            mockTrace = mockRepository.Create<IEntityAccessControlCheckTrace>();
            mockTrace.Setup(c => c.TraceCheckAccess(
                It.Is<IDictionary<long, bool>>(x => x.Count() == 1 && x[entities[0].Id]),
                It.Is<IList<EntityRef>>(x => x.SequenceEqual(permissions)),
                user, null, It.IsAny<long>()));

            checker = new LoggingEntityAccessControlChecker(mockChecker.Object, mockTrace.Object);
            checker.CheckAccess(entities, permissions, user);

            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullUser()
        {
            Assert.That(
                () =>
                new LoggingEntityAccessControlChecker().CheckTypeAccess( new EntityType[0], new EntityRef(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullPermission( )
        {
            Assert.That(
                ( ) =>
                new LoggingEntityAccessControlChecker( ).CheckTypeAccess( new EntityType [ 0 ], null, new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "permission" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullEntityTypes( )
        {
            Assert.That(
                () =>
                new LoggingEntityAccessControlChecker().CheckTypeAccess( null, new EntityRef( ), new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess( )
        {
            LoggingEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            Mock<IEntityAccessControlCheckTrace> mockTrace;
            IList<EntityType> entityTypes;
            EntityRef user;
            EntityRef permission;
            const long userId = 100;

            entityTypes = new []{ new EntityType() };
            user = new EntityRef(userId);
            permission = new EntityRef( 101 );

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockChecker = mockRepository.Create<IEntityAccessControlChecker>();
            mockChecker.Setup(c => c.CheckTypeAccess(entityTypes, permission, user ) )
                       .Returns(entityTypes.ToDictionary(et => et.Id, et => true));

            mockTrace = mockRepository.Create<IEntityAccessControlCheckTrace>();
            mockTrace.Setup(c => c.TraceCheckTypeAccess(
                It.Is<IDictionary<long, bool>>(results => results.Count == 1 && results[entityTypes[0].Id]),
                It.Is<EntityRef>( er => EntityRefComparer.Instance.Equals( er, permission ) ),
                It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, user)),
                It.Is<IList<EntityRef>>(ets => ets.SequenceEqual(entityTypes.Select(et => new EntityRef(et)), EntityRefComparer.Instance)),
                It.IsAny<long>()));

            checker = new LoggingEntityAccessControlChecker(mockChecker.Object, mockTrace.Object);
            checker.CheckTypeAccess(entityTypes, permission, user);

            mockRepository.VerifyAll();
        }

    }
}
