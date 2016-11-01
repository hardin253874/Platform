// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.Builder
{
    public abstract class QueryEnginePagingTestsBase
    {
        public static readonly string MatchingName = "A";

        public StructuredQuery Query { get; protected set; }
        public UserAccount UserAccount { get; protected set; }
        public DatabaseContext DatabaseContext { get; protected set; }
        public IEntity[] Entities { get; protected set; }
        public RunAsDefaultTenant RunAsDefaultTenant { get; protected set; }

        [TestFixtureSetUp]
        public virtual void TestFixtureSetup()
        {
            EntityType entityType;

            try
            {
                DatabaseContext = DatabaseContext.GetContext(true);

                RunAsDefaultTenant = new RunAsDefaultTenant();
                RunAsDefaultTenant.BeforeTest(null);

                entityType = EDC.ReadiNow.Model.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                Entities = CreateEntities(entityType, MatchingName);

                // Ensure the results are ordered
                Query = TestQueries.Entities(entityType);
                Query.OrderBy.Add(new OrderByItem()
                {
                    Expression = new ColumnReference()
                    {
                        ColumnId = Query.SelectColumns[0].ColumnId
                    },
                    Direction = OrderByDirection.Ascending
                });

                UserAccount = new UserAccount();
                UserAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(UserAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA(entityType).ToReport());
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("Setup failed: " + ex);
                if (DatabaseContext != null)
                {
                    DatabaseContext.Dispose();
                }
                throw;
            }
        }

        protected void TestResultInternal(int[] expectedIndexes, QuerySettings querySettings)
        {
            QueryResult queryResult;

            using (new SetUser(UserAccount))
            {
                queryResult = Factory.QueryRunner.ExecuteQuery( Query, querySettings );
            }

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(expectedIndexes.Select(x => Entities[x].Id)));            
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            // Return to normality
            RunAsDefaultTenant.AfterTest(null);

            // Rollback the transaction
            DatabaseContext.Dispose();
        }

        /// <summary>
        /// Create entities used for testing.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="matchingName"></param>
        protected abstract IEntity[] CreateEntities(EntityType entityType, string matchingName);
    }
}
