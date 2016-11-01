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
using EDC.ReadiNow.Test.Security.AccessControl;

namespace EDC.ReadiNow.Test.Metadata.Query.Structured
{
    // NOTE: Due to a quirk of the NUnit 2.6.3 runner, using TestCaseSource here prevents NUnit from loading the
    // assembly containing this test. Specifically, the calls to RunAsDefaultTenant.BeforeContext. Interestingly,
    // the tests run fine in the Resharper NUnit test runner, though.

    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class StructuredQueryHelperReferencingTests
    {
        [Test]
        public void Test_GetReferencedFields_NullQuery()
        {
            Assert.That(() => StructuredQueryHelper.GetReferencedFields(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("structuredQuery"));
        }

        [Test]
        public void Test_GetReferencedFields_NullRootEntity()
        {
            Assert.That(() => StructuredQueryHelper.GetReferencedFields(new StructuredQuery()),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        [TestCaseSource("Test_GetReferencedFields_Source")]
        [RunAsDefaultTenant]
        public void Test_GetReferencedFields(Func<StructuredQuery> structuredQueryFactory, string[] expectedResults)
        {
            Assert.That(StructuredQueryHelper.GetReferencedFields(structuredQueryFactory()),
                Is.EquivalentTo(expectedResults.Select(s => new EntityRef(s))).Using(EntityRefComparer.Instance));
        }

        private IEnumerable<TestCaseData> Test_GetReferencedFields_Source()
        {
            // Using a Func<StructuredQuery> rather than a straight StructuredQuery to 
            // avoid issues with NUnit and using a CallContext in a TestCaseSource method.

            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.Entities()), new string[0]);
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithNameA()), new[] {"core:name"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithDescription()), new[] {"core:description"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesOrderByDescription()), new[] {"core:description"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithNameDescription("a", "a")), new[] {"core:name", "core:description"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.EntitiesWithNameAndDescriptionInResults( "a" ) ), new [ ] { "core:name", "core:description" } );
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.AccessRulesWithNamedPermission("read")), new[] {"core:name", "core:accessRuleEnabled"});
            yield return new TestCaseData((Func<StructuredQuery>) (() => TestQueries.ActiveUsersInRole("administrators")), new[] {"core:name", "core:alias"});
        }

        [Test]
        public void Test_GetReferencedRelationships_NullQuery()
        {
            Assert.That(() => StructuredQueryHelper.GetReferencedRelationships(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("structuredQuery"));
        }

        [Test]
        public void Test_GetReferencedRelationships_NullRootEntity()
        {
            Assert.That(() => StructuredQueryHelper.GetReferencedRelationships(new StructuredQuery { RootEntity = null }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        [TestCaseSource("Test_GetReferencedRelationships_Source")]
        [RunAsDefaultTenant]
        public void Test_GetReferencedRelationships(Func<StructuredQuery> structuredQueryFactory, string[] expectedResults)
        {
            Assert.That(StructuredQueryHelper.GetReferencedRelationships(structuredQueryFactory()),
                Is.EquivalentTo(expectedResults.Select(s => new EntityRef(s))).Using(EntityRefComparer.Instance));
        }

        private IEnumerable<TestCaseData> Test_GetReferencedRelationships_Source()
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
