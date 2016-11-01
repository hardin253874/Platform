// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class FilteringQueryRepositoryTests
    {
        [TestCase( QueryFilter.DoesNotReferToCurrentUser )]
        [TestCase( QueryFilter.RefersToCurrentUser )]
        public void Constructor( QueryFilter queryFilter )
        {
            Mock<IQueryRepository> mockRepository;
            FilteringQueryRepository filteringRepository;

            mockRepository = new Mock<IQueryRepository>( MockBehavior.Strict );
            filteringRepository = new FilteringQueryRepository( mockRepository.Object, queryFilter );

            Assert.That( filteringRepository.Repository, Is.EqualTo( mockRepository.Object ) );
            Assert.That( filteringRepository.QueryFilter, Is.EqualTo( queryFilter ) );
        }

        [Test]
        public void Constructor_NullInner( )
        {
            Assert.Throws<ArgumentNullException>( ( ) => new FilteringQueryRepository( null, QueryFilter.DoesNotReferToCurrentUser ), "queryRepository" );
        }

        [TestCase(QueryFilter.DoesNotReferToCurrentUser, 0, false)]
        [TestCase(QueryFilter.RefersToCurrentUser, 1, false )]
        [TestCase( QueryFilter.DoesNotReferToCurrentUser, 0, true )]
        [TestCase( QueryFilter.RefersToCurrentUser, 1, true )]
        public void TestQueriesGetFiltered(QueryFilter queryFilter, int mockResultToMatch, bool nullParams)
        {
            FilteringQueryRepository filteringRepository;
            Mock<IQueryRepository> mockRepository;
            long subjectId = 1;
            EntityRef permissionId = nullParams ? null : new EntityRef(2);
            long typeId = 3;
            IList<long> securableTypes = nullParams ? null : new List<long> { typeId };
            StructuredQuery queryWithCurrentUser;
            StructuredQuery queryWithoutCurrentUser;
            AccessRuleQuery[] mockResults;
            IEnumerable<AccessRuleQuery> result;

            queryWithoutCurrentUser = TestQueries.Entities( new EntityRef(typeId) );
            queryWithCurrentUser = TestQueries.Entities( new EntityRef(typeId) );
            queryWithCurrentUser.Conditions.Add(new QueryCondition { Operator = ConditionType.CurrentUser } );

            mockResults = new AccessRuleQuery[]
            {
                new AccessRuleQuery(1, 2, 3, queryWithoutCurrentUser, false),
                new AccessRuleQuery(4, 5, 6, queryWithCurrentUser, false)
            };

            mockRepository = new Mock<IQueryRepository>(MockBehavior.Strict);
            mockRepository.Setup( qr => qr.GetQueries( subjectId, permissionId, securableTypes ) ).Returns(mockResults);

            filteringRepository = new FilteringQueryRepository(mockRepository.Object, queryFilter);
            result = filteringRepository.GetQueries( subjectId, permissionId, securableTypes );

            Assert.That( result, Is.EquivalentTo( new[] { mockResults[mockResultToMatch] } ) );

        }
    }
}
