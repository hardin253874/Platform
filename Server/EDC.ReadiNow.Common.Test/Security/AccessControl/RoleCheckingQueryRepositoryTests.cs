// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class RoleCheckingQueryRepositoryTests
    {
        [Test]
        public void Test_Creation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var mockQueryRepository = mockRepository.Create<IQueryRepository>();
            var mockRoleRepository = mockRepository.Create<IUserRoleRepository>();
            Assert.IsNotNull(new RoleCheckingQueryRepository(mockQueryRepository.Object, mockRoleRepository.Object));

            mockRepository.VerifyAll();            
        }


        [Test]
        public void Test_CreationDefaultConstructor()
        {
            Assert.IsNotNull(new RoleCheckingQueryRepository());            
        }


        [Test]
        public void Test_Creation_NullQueryRepository()
        {
            Assert.That(() => new RoleCheckingQueryRepository(null, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("queryRepository"));
        }


        [Test]
        public void Test_Creation_NullRoleRepository()
        {
            var mockRepository = new Mock<IQueryRepository>(MockBehavior.Strict);

            Assert.That(() => new RoleCheckingQueryRepository(mockRepository.Object, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("roleRepository"));
        }


        [Test]
        public void Test_GetQueries()
        {                        
            const long testSubjectId = 1;
            var testPermission = new EntityRef(2);
            long[] entities = {3, 4};
            // The roles that the test subject is in
            ISet<long> expectedRolesResult = new SortedSet<long> {1, 2, 3};
            // The queries that apply to each role
            IDictionary<long, IEnumerable<AccessRuleQuery>> expectedQueriesDictResult = new SortedDictionary<long, IEnumerable<AccessRuleQuery>>
            {
                {
                    1, new AccessRuleQuery[2]
                },
                {
                    2, new AccessRuleQuery[1]
                },
                {
                    3, new AccessRuleQuery[1]
                }
            };

            var expectedQueriesResult = new List<AccessRuleQuery>();
            foreach (var v in expectedQueriesDictResult.Values)
            {
                expectedQueriesResult.AddRange( v );
            }

            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockQueryRepository = mockRepository.Create<IQueryRepository>();
            // Setup 3 calls to GetQueries, 1 for each role 
            mockQueryRepository.Setup(mr => mr.GetQueries(1, testPermission, entities))
                .Returns(() => expectedQueriesDictResult[1]);
            mockQueryRepository.Setup(mr => mr.GetQueries(2, testPermission, entities))
                .Returns(() => expectedQueriesDictResult[2]);
            mockQueryRepository.Setup(mr => mr.GetQueries(3, testPermission, entities))
                .Returns(() => expectedQueriesDictResult[3]);

            var mockRoleRepository = mockRepository.Create<IUserRoleRepository>();
            mockRoleRepository.Setup(mr => mr.GetUserRoles(testSubjectId))
                .Returns(() => expectedRolesResult);

            var repository = new RoleCheckingQueryRepository(mockQueryRepository.Object, mockRoleRepository.Object);

            Assert.That(repository.GetQueries(1, testPermission, entities), Is.EqualTo(expectedQueriesResult));

            mockRepository.VerifyAll();            
        }
    }
}