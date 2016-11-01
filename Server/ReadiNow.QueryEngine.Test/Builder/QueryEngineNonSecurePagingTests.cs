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
    public class QueryEngineNonSecurePagingTests : QueryEnginePagingTestsBase
    {
        [Test]
        [TestCase(0, 100, new[] { 0, 1, 2, 3, 4  })]
        [TestCase(0, 1, new[] { 0 })]
        [TestCase(0, 2, new[] { 0, 1 })]
        [TestCase(0, 3, new[] { 0, 1, 2 })]
        [TestCase(0, 4, new[] { 0, 1, 2, 3 })]
        [TestCase(0, 5, new[] { 0, 1, 2, 3, 4 })]
        [TestCase(1, 1, new[] { 1 })]
        [TestCase(1, 2, new[] { 1, 2 })]
        [TestCase(1, 3, new[] { 1, 2, 3 })]
        [TestCase(1, 4, new[] { 1, 2, 3, 4 })]
        [TestCase(2, 1, new[] { 2 })]
        [TestCase(2, 2, new[] { 2, 3 })]
        [TestCase(2, 3, new[] { 2, 3, 4 })]
        [TestCase(3, 1, new[] { 3 })]
        [TestCase(3, 2, new[] { 3, 4 })]
        [TestCase(4, 1, new[] { 4 })]
        [TestCase(5, 1, new int[0])]
        public void TestResults(int firstRow, int pageSize, int[] expectedIndexes)
        {
            TestResultInternal(expectedIndexes, new QuerySettings()
            {
                SecureQuery = false,
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
            const int numEntities = 5;
            IEntity[] result;

            result = new IEntity[numEntities];
            for (int i = 0; i < numEntities; i++)
            {
                result[i] = Entity.Create(entityType);
                result[i].Save();
            }

            return result;
        }
    }
}
