// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class SystemAccessRuleQueryRepositoryTests
    {
        public IEnumerable<TestCaseData> GetRuleRequests()
        {
            return new List<TestCaseData>
            {
                new TestCaseData(100, "core:read", new long[] {102}, new long[] {100}),
                new TestCaseData(100, "core:read", new long[] {102, 402}, new long[] {100}),
                new TestCaseData(200, "core:modify", new long[] {202}, new long[] {200}),
                new TestCaseData(300, "core:delete", new long[] {302}, new long[] {300}),
                new TestCaseData(400, "core:read", new long[] {402}, new long[] {400}),
                new TestCaseData(400, "core:modify", new long[] {502}, new long[] {500}),
                new TestCaseData(400, "core:delete", new long[] {602}, new long[] {600}),
                new TestCaseData(400, "core:read", new long[] {402, 702, 802}, new long[] {400, 700, 800}),
                new TestCaseData(400, "core:read", new long[] {102}, new long[0]),
                new TestCaseData(500, "core:read", new long[] {102}, new long[0])
            };
        }

        [Test]
        public void ConstructorTests()
        {
            Assert.That(
                () =>
                    new SystemAccessRuleQueryRepository(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("systemQueryFactory"));

            Assert.DoesNotThrow(
                () =>
                {
                   new SystemAccessRuleQueryRepository(new SystemAccessRuleQueryFactory());                    
                });
        }


        [Test]
        [TestCaseSource(nameof(GetRuleRequests))]
        public void GetApplicableRulesTest(long subjectId, string permission, long[] entityTypeIds, long[] expectedAccessRuleIds)
        {
            IDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>> rules = new Dictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>
            {
                {new SubjectPermissionTuple(100, Permissions.Read.Id), new List<AccessRuleQuery> {new AccessRuleQuery(100, 101, 102, new StructuredQuery(), false)}},
                {new SubjectPermissionTuple(200, Permissions.Modify.Id), new List<AccessRuleQuery> {new AccessRuleQuery(200, 201, 202, new StructuredQuery(), false)}},
                {new SubjectPermissionTuple(300, Permissions.Delete.Id), new List<AccessRuleQuery> {new AccessRuleQuery(300, 301, 302, new StructuredQuery(), false)}},
                {
                    new SubjectPermissionTuple(400, Permissions.Read.Id), new List<AccessRuleQuery>
                    {
                        new AccessRuleQuery(400, 401, 402, new StructuredQuery(), false),
                        new AccessRuleQuery(700, 701, 702, new StructuredQuery(), false),
                        new AccessRuleQuery(800, 801, 802, new StructuredQuery(), false)
                    }
                },
                {new SubjectPermissionTuple(400, Permissions.Modify.Id), new List<AccessRuleQuery> {new AccessRuleQuery(500, 501, 502, new StructuredQuery(), false)}},
                {new SubjectPermissionTuple(400, Permissions.Delete.Id), new List<AccessRuleQuery> {new AccessRuleQuery(600, 601, 602, new StructuredQuery(), false)}}
            };

            var mockRepo = new MockRepository(MockBehavior.Strict);
            var accessQueryFactoryMock = mockRepo.Create<IAccessRuleQueryFactory>();
            accessQueryFactoryMock.Setup(f => f.GetQueries()).Returns(rules);

            var sysRepo = new SystemAccessRuleQueryRepository(accessQueryFactoryMock.Object);

            var accessRules = sysRepo.GetQueries(subjectId, new EntityRef(permission), entityTypeIds).ToList();
            Assert.AreEqual(expectedAccessRuleIds.Length, accessRules.Count, "The number of access rules is invalid");

            if (expectedAccessRuleIds.Length > 0)
            {
                var accessRuleIds = accessRules.Select(ar => ar.AccessRuleId).ToSet();
                var expectedSet = new HashSet<long>(expectedAccessRuleIds);

                Assert.IsTrue(accessRuleIds.SetEquals(expectedSet), "The access rules returned is invalid.");
            }

            mockRepo.VerifyAll();
        }

        [Test]
        public void NoApplicableRules()
        {
            IDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>> rules = new Dictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>();
            var mockRepo = new MockRepository(MockBehavior.Strict);
            var accessQueryFactoryMock = mockRepo.Create<IAccessRuleQueryFactory>();
            accessQueryFactoryMock.Setup(f => f.GetQueries()).Returns(rules);

            var sysRepo = new SystemAccessRuleQueryRepository(accessQueryFactoryMock.Object);

            var queries = sysRepo.GetQueries(100, Permissions.Read, new List<long> {200, 300});

            Assert.AreEqual(0, queries.Count(), "The count of queries is invalid.");

            mockRepo.VerifyAll();
        }
    }
}