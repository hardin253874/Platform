// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.Builder
{
    [TestFixture]
	[RunWithTransaction]
    public class QueryEngineSecurePagingEvenTests: QueryEnginePagingTestsBase
    {
        [Test]
        [TestCase(0, 100, new[] { 0, 2, 4, 6, 8 })] // Ensure security checks are cached
        [TestCase(0, 1, new[] { 0 })]
        [TestCase(0, 2, new[] { 0, 2 })]
        [TestCase(0, 3, new[] { 0, 2, 4 })]
        [TestCase(0, 4, new[] { 0, 2, 4, 6 })]
        [TestCase(0, 5, new[] { 0, 2, 4, 6, 8 })]
        [TestCase(0, 6, new[] { 0, 2, 4, 6, 8 })]
        [TestCase(1, 1, new[] { 2 })]
        [TestCase(1, 2, new[] { 2, 4 })]
        [TestCase(1, 3, new[] { 2, 4, 6 })]
        [TestCase(1, 4, new[] { 2, 4, 6, 8 })]
        [TestCase(1, 5, new[] { 2, 4, 6, 8 })]
        [TestCase(2, 1, new[] { 4 })]
        [TestCase(2, 2, new[] { 4, 6 })]
        [TestCase(2, 3, new[] { 4, 6, 8 })]
        [TestCase(2, 4, new[] { 4, 6, 8 })]
        [TestCase(3, 1, new[] { 6 })]
        [TestCase(3, 2, new[] { 6, 8 })]
        [TestCase(3, 3, new[] { 6, 8 })]
        [TestCase(4, 1, new[] { 8 })]
        [TestCase(4, 2, new[] { 8 })]
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
                // Set the even numbered entities to have the matching name
                if (i % 2 == 0)
                {
                    result[i].SetField("core:name", matchingName);
                }
                result[i].Save();
            }

            return result;
        }
    }
}
