// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.Builder
{
    [TestFixture]
	[RunWithTransaction]
    public class QueryEngineSecurePagingOddTests: QueryEnginePagingTestsBase
    {
        [Test]
        [TestCase(0, 100, new[] { 1, 3, 5, 7, 9 })]  // Ensure security checks are cached
        [TestCase(0, 1, new[] { 1 })]
        [TestCase(0, 2, new[] { 1, 3 })]
        [TestCase(0, 3, new[] { 1, 3, 5 })]
        [TestCase(0, 4, new[] { 1, 3, 5, 7 })]
        [TestCase(0, 5, new[] { 1, 3, 5, 7, 9 })]
        [TestCase(0, 6, new[] { 1, 3, 5, 7, 9 })]
        [TestCase(1, 1, new[] { 3 })]
        [TestCase(1, 2, new[] { 3, 5 })]
        [TestCase(1, 3, new[] { 3, 5, 7 })]
        [TestCase(1, 4, new[] { 3, 5, 7, 9 })]
        [TestCase(1, 5, new[] { 3, 5, 7, 9 })]
        [TestCase(2, 1, new[] { 5 })]
        [TestCase(2, 2, new[] { 5, 7 })]
        [TestCase(2, 3, new[] { 5, 7, 9 })]
        [TestCase(2, 4, new[] { 5, 7, 9 })]
        [TestCase(3, 1, new[] { 7 })]
        [TestCase(3, 2, new[] { 7, 9 })]
        [TestCase(3, 3, new[] { 7, 9 })]
        [TestCase(4, 1, new[] { 9 })]
        [TestCase(4, 2, new[] { 9 })]
        public void TestAccess(int firstRow, int pageSize, int[] expectedIndexes)
        {
            TestResultInternal(expectedIndexes, new QuerySettings()
            {
                SecureQuery = true,
                SupportPaging = true,
                FirstRow = firstRow,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// Create entities used for testing.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="matchingName"></param>
        protected override IEntity[] CreateEntities(EntityType entityType, string matchingName)
        {
            const int numEntities = 10;
            IEntity[] result;

            result = new IEntity[numEntities];
            for (int i = 0; i < numEntities; i++)
            {
                result[i] = Entity.Create(entityType);
                // Set the odd numbered entities to have the matching name
                if (i % 2 == 1)
                {
                    result[i].SetField("core:name", matchingName);
                }
                result[i].Save();
            }

            return result;
        }
    }
}
