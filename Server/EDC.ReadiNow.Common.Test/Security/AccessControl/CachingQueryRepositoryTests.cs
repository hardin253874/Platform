// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class CachingQueryRepositoryTests
    {
        [Test]
        public void Test_Creation_NullRepository()
        {
            Assert.That(() => new CachingQueryRepository(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("queryRepository"));
        }

        [Test]
        public void Test_Creation()
        {
            CachingQueryRepository repository;
            Mock<IQueryRepository> mockRepository;

            mockRepository = new Mock<IQueryRepository>(MockBehavior.Strict);

            repository = new CachingQueryRepository(mockRepository.Object);

            mockRepository.VerifyAll();
            Assert.That(repository, Has.Property("Cache").Count.EqualTo(0));
            Assert.That(repository, Has.Property("Repository").EqualTo(mockRepository.Object));
        }

        [TestCase( false, false )]
        [TestCase( true, false )]
        [TestCase( false, true )]
        public void Test_GetQueries(bool nullPerm, bool nullTypes)
        {
            CachingQueryRepository repository;
            Mock<IQueryRepository> mockRepository;
            const long testSubjectId = 1;
            EntityRef testPermission = nullPerm ? null : new EntityRef(2);
            long testPermissionId = nullPerm ? -1 : testPermission.Id;
            long[] entities = nullTypes ? null : new long[]{3, 4};
            IEnumerable<AccessRuleQuery> expectedResult = new AccessRuleQuery[0];
            
            mockRepository = new Mock<IQueryRepository>(MockBehavior.Strict);
            mockRepository.Setup(mr => mr.GetQueries(testSubjectId, testPermission, entities))
                          .Returns(() => expectedResult);

            repository = new CachingQueryRepository(mockRepository.Object);

            Assert.That(repository.GetQueries(1, testPermission, entities),
                Is.EqualTo(expectedResult));
            Assert.That(repository.Cache,
                Has.Exactly(1).Property("Key").EqualTo(new SubjectPermissionTypesTuple(testSubjectId, testPermissionId, entities))
                              .And.Property("Value").EqualTo(expectedResult));
            Assert.That(repository.GetQueries(1, testPermission, entities),
                Is.EqualTo(expectedResult));
            Assert.That(repository.Cache,
                Has.Exactly(1).Property("Key").EqualTo(new SubjectPermissionTypesTuple(testSubjectId, testPermissionId, entities))
                              .And.Property("Value").EqualTo(expectedResult));

            mockRepository.VerifyAll();            
        }

        [Test]
        public void Test_Clear()
        {
            CachingQueryRepository repository;
            Mock<IQueryRepository> mockRepository;
            const long testSubjectId = 1;
            EntityRef testPermission = new EntityRef(2);
            long[] entities = new long[]{3, 4};
            IEnumerable<AccessRuleQuery> expectedResult = new []{ new AccessRuleQuery(1, 1, 1, new StructuredQuery(), false) };

            mockRepository = new Mock<IQueryRepository>(MockBehavior.Strict);
            mockRepository.Setup(mr => mr.GetQueries(testSubjectId, testPermission, entities))
                          .Returns(() => expectedResult);

            repository = new CachingQueryRepository(mockRepository.Object);
            repository.GetQueries(1, testPermission, entities);

            // Sanity check
            Assert.That(repository.Cache, Has.Property("Count").Positive, "Not cached");

            repository.Clear();

            Assert.That(repository.Cache, Has.Property("Count").EqualTo(0), "Not cleared");
        }
    }
}
