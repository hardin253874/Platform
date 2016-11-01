// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    // NOTE: Due to a quirk of the NUnit 2.6.3 runner, using TestCaseSource here prevents NUnit from loading the
    // assembly containing this test. Specifically, the calls to RunAsDefaultTenant.BeforeContext. Interestingly,
    // the tests run fine in the Resharper NUnit test runner, though.

    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class EntityAccessControlCheckerHelperTests
    {
        [Test]
        public void Test_GetFieldTypesReferencedByCondition_NullQuery()
        {
            Assert.That(() => EntityAccessControlCheckerHelper.GetFieldTypesReferencedByCondition(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("structuredQuery"));
        }

        [Test]
        public void Test_GetFieldTypesReferencedByCondition_NullQueryCondition()
        {
            Assert.That(() => EntityAccessControlCheckerHelper.GetFieldTypesReferencedByCondition(new StructuredQuery { Conditions = null }),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("structuredQuery"));
        }

        [Test]
        [TestCaseSource("Test_GetFieldTypesReferencedByCondition_Source")]
        [RunAsDefaultTenant]
        public void Test_GetFieldTypesReferencedByCondition(Func<StructuredQuery> structuredQueryFactory, string[] expectedResults)
        {
            Assert.That(EntityAccessControlCheckerHelper.GetFieldTypesReferencedByCondition(structuredQueryFactory()),
                Is.EquivalentTo(expectedResults.Select(s => new EntityRef(s))).Using(EntityRefComparer.Instance));
        }

        private IEnumerable<TestCaseData> Test_GetFieldTypesReferencedByCondition_Source()
        {
            // Using a Func<StructuredQuery> rather than a straight StructuredQuery to 
            // avoid issues with NUnit and using a CallContext in a TestCaseSource method.

            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.Entities()), new string[0]);
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithNameA()), new[] {"core:name"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithDescription()), new[] {"core:description"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithNameDescription("a", "a")), new[] {"core:name", "core:description"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithNameAndDescriptionInResults("a")), new[] {"core:name"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.AccessRulesWithNamedPermission("read")), new[] {"core:name", "core:accessRuleEnabled"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.ActiveUsersInRole("administrators")), new[] {"core:name", "core:alias"});
        }

        [Test]
        public void Test_GetReferencedRelationshipTypes_NullQuery()
        {
            Assert.That(() => EntityAccessControlCheckerHelper.GetReferencedRelationshipTypes(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("structuredQuery"));
        }

        [Test]
        public void Test_GetReferencedRelationshipTypes_NullRootEntity()
        {
            Assert.That(() => EntityAccessControlCheckerHelper.GetReferencedRelationshipTypes(new StructuredQuery { RootEntity = null }),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("structuredQuery"));
        }

        [Test]
        [TestCaseSource("Test_GetReferencedRelationshipTypes_Source")]
        [RunAsDefaultTenant]
        public void Test_GetReferencedRelationshipTypes(Func<StructuredQuery> structuredQueryFactory, string[] expectedResults)
        {
            Assert.That(EntityAccessControlCheckerHelper.GetReferencedRelationshipTypes(structuredQueryFactory()),
                Is.EquivalentTo(expectedResults.Select(s => new EntityRef(s))).Using(EntityRefComparer.Instance));
        }

        private IEnumerable<TestCaseData> Test_GetReferencedRelationshipTypes_Source()
        {
            // Using a Func<StructuredQuery> rather than a straight StructuredQuery to 
            // avoid issues with NUnit and using a CallContext in a TestCaseSource method.

            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.Entities()), new string[0]);
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.AccessRulesWithNamedPermission("read")), new[] { "core:permissionAccess" });

            // Get multiple relationships form the root entity, including an enum
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.ActiveUsersInRole("administrators")), new[] { "core:userHasRole", "core:accountStatus" });

            // Test nested related resource nodes and reverse relationships
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.TypesSecuredBySubjects("administrators")), new[] { "core:allowAccess", "core:controlAccess" });
        }
    }
}
