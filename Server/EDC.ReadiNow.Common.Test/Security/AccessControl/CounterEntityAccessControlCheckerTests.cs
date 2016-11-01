// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Monitoring;
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
    public class CounterEntityAccessControlCheckerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Creation()
        {
            CounterEntityAccessControlChecker checker;

            checker = new CounterEntityAccessControlChecker();
            Assert.That(checker, Has.Property("Checker").Not.Null);
            Assert.That(checker, Has.Property("AccessControlCounters").Not.Null);
            Assert.That(checker, Has.Property("AccessControlPermissionCounters").Not.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullEntities()
        {
            Assert.That(
                () =>
                new CounterEntityAccessControlChecker().CheckAccess(null, new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullPermissions()
        {
            Assert.That(
                () =>
                new CounterEntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), null,
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullUser()
        {
            Assert.That(
                () =>
                new CounterEntityAccessControlChecker().CheckAccess(new Collection<EntityRef>(), new Collection<EntityRef>(),
                                                             null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess()
        {
            CounterEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            Mock<ISingleInstancePerformanceCounterCategory> mockSingleInstancePerformanceCounterCategory;
            Mock<IMultiInstancePerformanceCounterCategory> mockMultiInstancePerformanceCounterCategory;
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

            mockSingleInstancePerformanceCounterCategory = mockRepository.Create<ISingleInstancePerformanceCounterCategory>();
            mockSingleInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(It.IsAny<string>()))
                .Returns(mockRepository.Create<RatePerSecond32PerformanceCounter>(MockBehavior.Loose).Object);
            mockSingleInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(It.IsAny<string>()))
                .Returns(mockRepository.Create<NumberOfItems64PerformanceCounter>(MockBehavior.Loose).Object);
            mockSingleInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<AverageTimer32PerformanceCounter>(It.IsAny<string>()))
                .Returns(mockRepository.Create<AverageTimer32PerformanceCounter>(MockBehavior.Loose).Object);

            mockMultiInstancePerformanceCounterCategory = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            mockMultiInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockRepository.Create<RatePerSecond32PerformanceCounter>(MockBehavior.Loose).Object);
            mockMultiInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockRepository.Create<NumberOfItems64PerformanceCounter>(MockBehavior.Loose).Object);

            checker = new CounterEntityAccessControlChecker(mockChecker.Object, 
                mockSingleInstancePerformanceCounterCategory.Object,
                mockMultiInstancePerformanceCounterCategory.Object);
            checker.CheckAccess(entities, permissions, user);

            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullUser( )
        {
            Assert.That(
                ( ) =>
                new CounterEntityAccessControlChecker( ).CheckTypeAccess( new EntityType [ 0 ], new EntityRef( ), null ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "user" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_Permission()
        {
            Assert.That(
                () =>
                new CounterEntityAccessControlChecker().CheckTypeAccess( new EntityType[0], null, new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permission"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullEntityTypes( )
        {
            Assert.That(
                () =>
                new CounterEntityAccessControlChecker().CheckTypeAccess( null, new EntityRef( ), new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess( )
        {
            CounterEntityAccessControlChecker checker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> mockChecker;
            Mock<ISingleInstancePerformanceCounterCategory> mockSingleInstancePerformanceCounterCategory;
            Mock<IMultiInstancePerformanceCounterCategory> mockMultiInstancePerformanceCounterCategory;
            IList<EntityType> entityTypes;
            EntityRef user;
            EntityRef permission;

            entityTypes = new []{ new EntityType() };
            user = new EntityRef(100);
            permission = new EntityRef( 101 );

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockChecker = mockRepository.Create<IEntityAccessControlChecker>();
            mockChecker
                .Setup(c => c.CheckTypeAccess( It.Is<IList<EntityType>>(ets => ets.SequenceEqual(entityTypes)), permission, user ) )
                .Returns(entityTypes.ToDictionary(et => et.Id, et => true));

            mockSingleInstancePerformanceCounterCategory = mockRepository.Create<ISingleInstancePerformanceCounterCategory>();
            mockSingleInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(It.IsAny<string>()))
                .Returns(mockRepository.Create<RatePerSecond32PerformanceCounter>(MockBehavior.Loose).Object);
            mockSingleInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(It.IsAny<string>()))
                .Returns(mockRepository.Create<NumberOfItems64PerformanceCounter>(MockBehavior.Loose).Object);
            mockSingleInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<AverageTimer32PerformanceCounter>(It.IsAny<string>()))
                .Returns(mockRepository.Create<AverageTimer32PerformanceCounter>(MockBehavior.Loose).Object);

            mockMultiInstancePerformanceCounterCategory = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            mockMultiInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockRepository.Create<RatePerSecond32PerformanceCounter>(MockBehavior.Loose).Object);
            mockMultiInstancePerformanceCounterCategory
                .Setup(c => c.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockRepository.Create<NumberOfItems64PerformanceCounter>(MockBehavior.Loose).Object);

            checker = new CounterEntityAccessControlChecker(mockChecker.Object,
                mockSingleInstancePerformanceCounterCategory.Object,
                mockMultiInstancePerformanceCounterCategory.Object);
            checker.CheckTypeAccess(entityTypes, permission, user );

            mockRepository.VerifyAll();
        }
    }
}
