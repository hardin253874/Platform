// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using EntityModel = EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Test;
using ReadiNow.QueryEngine.ReportConverter;

namespace ReadiNow.QueryEngine.Test.Builder
{
    [TestFixture]
	[RunWithTransaction]
    public class QueryEngineSecurityTests
    {
        private IQueryRunner QueryEngine
        {
            get { return Factory.QueryRunner; }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NonPaging_NoAccess()
        {
            StructuredQuery query;
            EntityType entityType;
            IEntity[] entities;
            const int numEntities = 3;
            UserAccount userAccount;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entities = new IEntity[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();
            }

            using (new SetUser(userAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(0));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NonPaging_AccessAll()
        {
            StructuredQuery query;
            EntityType entityType;
            IEntity[] entities;
            const int numEntities = 3;
            UserAccount userAccount;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entities = new IEntity[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);                    
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.Entities(entityType).ToReport());
            }

            using (new SetUser(userAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities));
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Test_NonPaging_AccessOne(int matchingIndex)
        {
            StructuredQuery query;
            EntityType entityType;
            IEntity[] entities;
            const int numEntities = 3;
            UserAccount userAccount;
            QueryResult queryResult;
            const string matchingName = "A";

            using (new SecurityBypassContext())
            {
                entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entities = new IEntity[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    if (i == matchingIndex)
                    {
                        entities[i].SetField("core:name", matchingName);
                    }
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA(entityType).ToReport());
            }

            using (new SetUser(userAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(1));
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(entities.Where(x => x.GetField("core:name") != null && ((string)x.GetField("core:name")) == matchingName).Select(x => x.Id)));
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo((from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0]).Distinct()), "Duplicates found");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Test_NonPaging_AccessSome(int nonMatchingIndex)
        {
            StructuredQuery query;
            EntityType entityType;
            IEntity[] entities;
            const int numEntities = 3;
            UserAccount userAccount;
            QueryResult queryResult;
            const string matchingName = "A";

            using (new SecurityBypassContext())
            {
                entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entities = new IEntity[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    if (i != nonMatchingIndex)
                    {
                        entities[i].SetField("core:name", matchingName);
                    }
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA(entityType).ToReport());
            }

            using (new SetUser(userAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities - 1));
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(entities.Where(x => x.GetField("core:name") != null && ((string)x.GetField("core:name")) == matchingName).Select(x => x.Id)));
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo((from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0]).Distinct()), "Duplicates found");
        }

        /// <remarks>
        /// Very long running test. Details are written to Trace rather than assertions thrown.
        /// </remarks>
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void Test_AllReports()
        {
            IEnumerable<Report> allReports;
            StructuredQuery query;
            Stopwatch stopwatch;

            using (new SecurityBypassContext())
            {
                allReports = EntityModel.Entity.GetInstancesOfType<Report>();
                Trace.WriteLine(string.Format("Checking all {0} reports ...", allReports.Count()));
                stopwatch = Stopwatch.StartNew();
                foreach (Report report in allReports)
                {
                    try
                    {
                        query = ReportToQueryConverter.Instance.Convert( report );
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine(string.Format("Report {0} conversion failed. Ignoring.", new EntityRef(report)));
                        query = null;
                    }

                    if (query != null)
                    {
                        try
                        {
                            QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(string.Format("Report {0} execution failed: {1}", new EntityRef(report), ex));
                        }
                    }
                }
            }
            stopwatch.Stop();
            Trace.WriteLine(string.Format("Took {0}.", stopwatch.Elapsed));
        }


        private class RelatedEntityAccessSecurityTestData
        {
            public RelatedEntityAccessSecurityTestData()
            {
                RelationshipsToFollow = new List<Relationship>();
                EntityTypes = new List<EntityType>();
                EntityInstances = new Dictionary<long, List<IEntity>>();
            }

            public List<Relationship> RelationshipsToFollow { get; private set; }
            public List<EntityType> EntityTypes { get; private set; }
            public StructuredQuery Query { get; set; }
            public Dictionary<long, List<IEntity>> EntityInstances { get; private set; }
            public UserAccount UserAccount { get; set; }
        }


        /// <summary>
        /// Root -> RelType 0 -> RelType 1 -> RelType 2
        /// </summary>
        /// <returns></returns>
        private RelatedEntityAccessSecurityTestData SetupRelatedEntityAccessSecurityTest(int numEntities, bool relationshipsAreForward = true, int relationshipDepth = 3)
        {
            var testData = new RelatedEntityAccessSecurityTestData();

            // Create entity types and relationships
            var entityType = EntityModel.Entity.Create<EntityType>();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Name = "RootType";
            entityType.Save();

            testData.EntityTypes.Add(entityType);
            testData.EntityInstances[entityType.Id] = new List<IEntity>();

            EntityType fromType = entityType;

            for ( int i = 0; i < relationshipDepth; i++ )
            {
                var relatedEntityType = EntityModel.Entity.Create<EntityType>();
                relatedEntityType.Inherits.Add(UserResource.UserResource_Type);
                relatedEntityType.Name = "RelType" + i;
                relatedEntityType.Save();

                testData.EntityInstances[relatedEntityType.Id] = new List<IEntity>();

                testData.EntityTypes.Add(relatedEntityType);

                // Create a relationship between the entity types
                Relationship relationship = new Relationship
                {
                    FromType = relationshipsAreForward ? fromType : relatedEntityType,
                    ToType = relationshipsAreForward ? relatedEntityType : fromType,
                    Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne
                };
                relationship.Save();

                testData.RelationshipsToFollow.Add(relationship);

                fromType = relatedEntityType;
            }

            // Create X instances of each entity type
            for (int i = 0; i < numEntities; i++)
            {
                foreach (EntityType et in testData.EntityTypes)
                {
                    var entity = EntityModel.Entity.Create(et);
                    if (i != 0)
                    {
                        entity.SetField("core:name", et.Name);
                    }
                    entity.SetField("core:description", et.Name);
                    entity.Save();

                    testData.EntityInstances[et.Id].Add(entity);
                }
            }

            // Setup the relationships between the entities
            foreach (Relationship relationship in testData.RelationshipsToFollow)
            {
                for (int i = 0; i < numEntities; i++)
                {
                    var fromEntityInstance = testData.EntityInstances[relationship.FromType.Id][i];
                    var toEntityInstance = testData.EntityInstances[relationship.ToType.Id][i];

                    var relationships = fromEntityInstance.GetRelationships(relationship, Direction.Forward);
                    relationships.Add(toEntityInstance);
                    fromEntityInstance.SetRelationships(relationship, relationships, Direction.Forward);

                    fromEntityInstance.Save();
                }
            }            

            testData.UserAccount = new UserAccount();
            testData.UserAccount.Save();

            return testData;
        }


        /// <summary>
        /// BaseType -> DerivedType
        /// </summary>
        /// <returns></returns>
        private RelatedEntityAccessSecurityTestData SetupInheritedEntityAccessSecurityTest(int numEntities)
        {
            var testData = new RelatedEntityAccessSecurityTestData();

            // Create entity types
            var entityType = EntityModel.Entity.Create<EntityType>();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Name = "BaseType";
            entityType.Save();

            testData.EntityTypes.Add(entityType);
            testData.EntityInstances[entityType.Id] = new List<IEntity>();

            var derivedEntityType = EntityModel.Entity.Create<EntityType>();
            derivedEntityType.Name = "DerivedType";
            derivedEntityType.Inherits.Add(entityType);
            derivedEntityType.Save();

            testData.EntityTypes.Add(derivedEntityType);
            testData.EntityInstances[derivedEntityType.Id] = new List<IEntity>();
           
            // Create X instances of each entity type
            for (int i = 0; i < numEntities; i++)
            {
                foreach (EntityType et in testData.EntityTypes)
                {
                    var entity = EntityModel.Entity.Create(et);
                    if (i != 0)
                    {
                        entity.SetField("core:name", et.Name);
                    }
                    entity.Save();

                    testData.EntityInstances[et.Id].Add(entity);
                }
            }            

            testData.UserAccount = new UserAccount();
            testData.UserAccount.Save();

            return testData;
        }


        /// <summary>
        /// BaseType -> DerivedType1 -> DerivedType2
        /// </summary>
        /// <returns></returns>
        private RelatedEntityAccessSecurityTestData SetupInheritedEntityAccessSecurityTestMultipleDerivedTypes(int numEntities)
        {
            var testData = new RelatedEntityAccessSecurityTestData();

            // Create entity types
            var entityType = EntityModel.Entity.Create<EntityType>();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Name = "BaseType";
            entityType.Save();

            testData.EntityTypes.Add(entityType);
            testData.EntityInstances[entityType.Id] = new List<IEntity>();

            var derivedEntityType1 = EntityModel.Entity.Create<EntityType>();
            derivedEntityType1.Name = "DerivedType1";
            derivedEntityType1.Inherits.Add(entityType);
            derivedEntityType1.Save();

            testData.EntityTypes.Add(derivedEntityType1);
            testData.EntityInstances[derivedEntityType1.Id] = new List<IEntity>();

            var derivedEntityType2 = EntityModel.Entity.Create<EntityType>();
            derivedEntityType2.Name = "DerivedType2";
            derivedEntityType2.Inherits.Add(derivedEntityType1);
            derivedEntityType2.Save();

            testData.EntityTypes.Add(derivedEntityType2);
            testData.EntityInstances[derivedEntityType2.Id] = new List<IEntity>();

            // Create a relationship between DerivedType1 and DerivedType1
            Relationship relationship = new Relationship
            {
                FromType = derivedEntityType1,
                ToType = derivedEntityType1,
                Cardinality_Enum = CardinalityEnum_Enumeration.OneToMany                
            };
            relationship.Save();

            testData.RelationshipsToFollow.Add(relationship);

            // Create X instances of each entity type
            for (int i = 0; i < numEntities; i++)
            {
                foreach (EntityType et in testData.EntityTypes)
                {
                    var entity = EntityModel.Entity.Create(et);
                    if (i == 0)
                    {
                        entity.SetField("core:name", "A");
                    }
                    entity.Save();

                    testData.EntityInstances[et.Id].Add(entity);
                }
            }

            // Setup relationships between derived type 2 instance with name A and derived type 1 instances            
            var fromEntityInstance = testData.EntityInstances[derivedEntityType2.Id][0];
            var toEntityInstances = testData.EntityInstances[derivedEntityType1.Id];

            var relationships = fromEntityInstance.GetRelationships(relationship, Direction.Forward);
            foreach (var toInstance in toEntityInstances)
            {
                relationships.Add(toInstance);    
            }            
            fromEntityInstance.SetRelationships(relationship, relationships, Direction.Forward);
            fromEntityInstance.Save();                

            testData.UserAccount = new UserAccount();
            testData.UserAccount.Save();

            return testData;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private RelatedEntityAccessSecurityTestData SetupModifiedSchemaAccessSecurityTest(int numEntities)
        {
            var testData = new RelatedEntityAccessSecurityTestData();

			var field = new StringField
			{
				Name = "Test String Field",
				IsRequired = false
			};
			field.Save( );

			// Create entity type
			var entityType = EntityModel.Entity.Create<EntityType>( );
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Fields.Add( field.As<Field>( ) );
            entityType.Name = "Test Type";
            entityType.Save();

            testData.EntityTypes.Add(entityType);
            testData.EntityInstances[entityType.Id] = new List<IEntity>();            

            // Create X instances of each entity type
            for (int i = 0; i < numEntities; i++)
            {
                var entity = EntityModel.Entity.Create(entityType);
                entity.SetField("core:name", "A");
                entity.SetField(field.Id, "A");                
                entity.Save();

                testData.EntityInstances[entityType.Id].Add(entity);                
            }

            testData.UserAccount = new UserAccount();
            testData.UserAccount.Save();

            return testData;
        }


        /// <summary>
        /// Adds the read access rule.
        /// </summary>
        /// <param name="testData">The test data.</param>
        /// <param name="entityTypeIndex">Index of the entity type.</param>
        /// <param name="name">The name.</param>
        private void AddReadAccessRule(RelatedEntityAccessSecurityTestData testData, int entityTypeIndex, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = testData.EntityTypes[entityTypeIndex].Name;
            }

            new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<Subject>(), testData.EntityTypes[entityTypeIndex].As<SecurableEntity>(),
                    TestQueries.EntitiesWithName(testData.EntityTypes[entityTypeIndex], name).ToReport());
        }


        /// <summary>
        /// Checks the query result.
        /// </summary>
        /// <param name="queryResult">The query result.</param>
        /// <param name="testData">The test data.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="entityTypeIndex">Index of the entity type.</param>
        private void CheckQueryResult(QueryResult queryResult, RelatedEntityAccessSecurityTestData testData, int columnIndex, int entityTypeIndex)
        {
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[columnIndex],
                Is.EquivalentTo(testData.EntityInstances[testData.EntityTypes[entityTypeIndex].Id].Where(x => x.GetField("core:name") != null && ((string)x.GetField("core:name")) == testData.EntityTypes[entityTypeIndex].Name).Select(x => x.Id)));
        }


        [Test]
        [RunAsDefaultTenant]
        [TestCase(false, false, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, true)]
        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        public void Test_RelatedEntity_AccessSecurity(bool rel1Access, bool rel2Access, bool rel3Access)
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities);
                testData.Query = TestQueries.Entities(testData.EntityTypes[0], testData.RelationshipsToFollow, true);

                // Allow read only to the root entity
                AddReadAccessRule(testData, 0);

                // Create rules for any accessible relationships
                if (rel1Access)
                {
                    AddReadAccessRule(testData, 1);
                }
                if (rel2Access)
                {
                    AddReadAccessRule(testData, 2);
                }
                if (rel3Access)
                {
                    AddReadAccessRule(testData, 3);
                }
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities - 1));

            // Check root column
            CheckQueryResult(queryResult, testData, 0, 0);

            var dbNullRows = Enumerable.Repeat(DBNull.Value, numEntities - 1).ToList();

            if (rel1Access)
            {
                CheckQueryResult(queryResult, testData, 1, 1);
            }
            else
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[1], Is.EquivalentTo(dbNullRows));
            }

            // Results for rel2 should only be available if rel1 is available
            if (rel1Access && rel2Access)
            {
                CheckQueryResult(queryResult, testData, 2, 2);
            }
            else
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[2], Is.EquivalentTo(dbNullRows));
            }

            // Results for rel3 should only be available if rel1 and rel2 is available
            if (rel1Access && rel2Access && rel3Access)
            {
                CheckQueryResult(queryResult, testData, 3, 3);
            }
            else
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[3], Is.EquivalentTo(dbNullRows));
            }
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_MultipleApplicableAccessRules()
        {
            const int numEntities = 4;
            QueryResult queryResult;
            const string matchingNameA = "A";
            const string matchingNameB = "B";
            UserAccount userAccount;
            StructuredQuery query;
            var entities = new IEntity[numEntities];

            using (new SecurityBypassContext())
            {
                var entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    entities[i].SetField("core:name", i % 2 == 0 ? matchingNameA : matchingNameB);
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithName(entityType, matchingNameA).ToReport());
                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithName(entityType, matchingNameB).ToReport());
            }

            using (new SetUser(userAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities));
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(entities.Where(x => x.GetField("core:name") != null && ((((string)x.GetField("core:name")) == matchingNameA) || (((string)x.GetField("core:name")) == matchingNameB))).Select(x => x.Id)));
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo((from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0]).Distinct()), "Duplicates found");
        }

        private void SetRelationshipSecuresFlags(Relationship relationship, bool isForward, bool secures)
        {
            relationship.SecuresTo = isForward ? secures : !secures;
            relationship.SecuresFrom = isForward ? !secures : secures;
            relationship.Save();
        }

        /// <summary>
        /// Test that related entities show up in a query just using the secures flags.
        /// The model looks like this Root -&gt; R1 -&gt; R2 -&gt; R3.
        /// The query is rooted from Root.
        /// An access rule exists for Root and R1,R2 and R3 are secured using the secures flags.
        /// </summary>
        /// <param name="rel1Secures">if set to <c>true</c> set the secures flag for the first related entity.</param>
        /// <param name="rel2Secures">if set to <c>true</c> set the secures flag for the second related entity.</param>
        /// <param name="rel3Secures">if set to <c>true</c> set the secures flag for the third related entity.</param>
        /// <param name="relationshipsAreForward">if set to <c>true</c> the relationships are forward.</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(false, false, false, true)]
        [TestCase(false, false, true, true)]
        [TestCase(false, true, false, true)]
        [TestCase(true, false, false, true)]
        [TestCase(false, true, true, true)]
        [TestCase(true, true, false, true)]
        [TestCase(true, false, true, true)]
        [TestCase(true, true, true, true)]
        [TestCase(false, false, false, false)]
        [TestCase(false, false, true, false)]
        [TestCase(false, true, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, true, false)]
        [TestCase(true, true, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, true, false)]
        public void Test_RelatedEntity_NoAccessRules_SecuresFlags(bool rel1Secures, bool rel2Secures, bool rel3Secures, bool relationshipsAreForward)
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities, relationshipsAreForward);
                // Create a report of Root
                testData.Query = TestQueries.Entities(testData.EntityTypes[0], testData.RelationshipsToFollow, relationshipsAreForward);

                // Allow read only to the root entity only
                AddReadAccessRule(testData, 0);

                // Specify the secures flag only for each of the related entities
                if (rel1Secures)
                {
                    SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], relationshipsAreForward, true);
                }
                if (rel2Secures)
                {
                    SetRelationshipSecuresFlags(testData.RelationshipsToFollow[1], relationshipsAreForward, true);
                }
                if (rel3Secures)
                {
                    SetRelationshipSecuresFlags(testData.RelationshipsToFollow[2], relationshipsAreForward, true);
                }
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities - 1));

            // Check root column
            CheckQueryResult(queryResult, testData, 0, 0);

            var dbNullRows = Enumerable.Repeat(DBNull.Value, numEntities - 1).ToList();

            if (rel1Secures)
            {
                CheckQueryResult(queryResult, testData, 1, 1);
            }
            else
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[1], Is.EquivalentTo(dbNullRows));
            }

            // Results for rel2 should only be available if rel1 is available
            if (rel1Secures && rel2Secures)
            {
                CheckQueryResult(queryResult, testData, 2, 2);
            }
            else
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[2], Is.EquivalentTo(dbNullRows));
            }

            // Results for rel3 should only be available if rel1 and rel2 is available
            if (rel1Secures && rel2Secures && rel3Secures)
            {
                CheckQueryResult(queryResult, testData, 3, 3);
            }
            else
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[3], Is.EquivalentTo(dbNullRows));
            }
        }


        /// <summary>
        /// Test that related entities show up in a query just using the secures flags.
        /// The model looks like this Root -&gt; R1 -&gt; R2 -&gt; R3.
        /// The query is rooted from R1.
        /// An access rule exists for Root and R1, R2 and R3 are secured using the secures flags.
        /// </summary>
        /// <param name="rel1Secures">if set to <c>true</c> [rel1 secures].</param>
        /// <param name="rel2Secures">if set to <c>true</c> [rel2 secures].</param>
        /// <param name="rel3Secures">if set to <c>true</c> [rel3 secures].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(false, false, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, true)]
        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]        
        public void Test_RelatedEntity_NoAccessRules_SecuresFlags_SecuringEntities(bool rel1Secures, bool rel2Secures, bool rel3Secures)
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities);
                // Create a report of the R1, R2, R3
                testData.Query = TestQueries.Entities(testData.EntityTypes[1], testData.RelationshipsToFollow.Skip(1));

                // Create an access rule for the root entities
                AddReadAccessRule(testData, 0);                                

                // Specify the secures flag only for each of the related entities
                if (rel1Secures)
                {
                    SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], true, true);
                }
                if (rel2Secures)
                {
                    SetRelationshipSecuresFlags(testData.RelationshipsToFollow[1], true, true);
                }
                if (rel3Secures)
                {
                    SetRelationshipSecuresFlags(testData.RelationshipsToFollow[2], true, true);
                }
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            var dbNullRows = Enumerable.Repeat(DBNull.Value, numEntities - 1).ToList();

            Assert.That(queryResult.DataTable, rel1Secures ? Has.Property("Rows").Count.EqualTo(numEntities - 1) : Has.Property("Rows").Count.EqualTo(0));

            if (rel1Secures)
            {                
                // Check R1 column returns valid data.
                // R1 is secured by Root
                CheckQueryResult(queryResult, testData, 0, 1);                
            }
            else
            {
                Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(0));                
            }

            if (rel1Secures && rel2Secures)
            {
                // Check R2 column returns valid data.
                // R2 is secured by R1
                CheckQueryResult(queryResult, testData, 1, 2);                
            }

            if (rel1Secures && !rel2Secures)
            {
                // R1 is secured by R2 is not secured so it should be null
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[1], Is.EquivalentTo(dbNullRows));
            }
            
            if (rel1Secures && rel2Secures && rel3Secures)
            {
                // Check R3 column returns valid data.
                // R3 is secured by R2
                CheckQueryResult(queryResult, testData, 2, 3);                
            }

            if (rel1Secures && rel2Secures && !rel3Secures)
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[2], Is.EquivalentTo(dbNullRows));
            }            
        }


        /// <summary>
        /// Test that related entities show up in a query using the secures flags and access rules.
        /// The model looks like this Root -&gt; R1 -&gt; R2 -&gt; R3.
        /// The query is rooted from Root.
        /// An access rule exists for Root and R1. R2 and R3 are secured using the secures flags.
        /// </summary>
        /// <param name="rel1Secures">if set to <c>true</c> [rel1 secures].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_RelatedEntity_AccessRule_And_SecuresFlags(bool rel1Secures)
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities);
                // Create a report of Root
                testData.Query = TestQueries.Entities(testData.EntityTypes[0], testData.RelationshipsToFollow);

                // Allow read to the root entity
                AddReadAccessRule(testData, 0);

                // Add an access rule that does not give access to any of R1
                AddReadAccessRule(testData, 1, "XXX");

                // Specify the secures flag for R1
                if (rel1Secures)
                {
                    SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], true, true);
                }               
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities - 1));

            // Check root column
            CheckQueryResult(queryResult, testData, 0, 0);

            var dbNullRows = Enumerable.Repeat(DBNull.Value, numEntities - 1).ToList();

            if (rel1Secures)
            {
                CheckQueryResult(queryResult, testData, 1, 1);
            }
            else
            {
                Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[1], Is.EquivalentTo(dbNullRows));
            }
        }




        /// <summary>
        /// Test that related entities show up in a query using the secures flags and access rules.
        /// The model looks like this Root -&gt; R1 -&gt; R2 -&gt; R3.
        /// The query is rooted from Root.
        /// An access rule exists for Root and R1. R2 and R3 are secured using the secures flags.
        /// </summary>
        /// <param name="rel1Secures">if set to <c>true</c> [rel1 secures].</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase( true, false, true, true )]
        [TestCase( true, true, false, false )]
        [TestCase( true, true, true, false )]
        [TestCase( false, true, false, true )]
        [TestCase( false, false, true, false )]
        [TestCase( false, true, true, false )]
        public void Test_RelatedEntity_AccessRule_And_SecuresFlags_Ensure_No_Unnecessary_Checks( bool forward, bool securesFrom, bool securesTo, bool securityChecksShouldBeImplicit )
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryBuild queryResult;

            using ( new SecurityBypassContext( ) )
            {
                // Explicit type
                var explitType = EntityModel.Entity.Create<EntityType>( );
                explitType.Inherits.Add(UserResource.UserResource_Type);
                explitType.Name = "Explicit";
                explitType.Save( );

                // Implicit type
                var implicitType = EntityModel.Entity.Create<EntityType>( );
                implicitType.Inherits.Add(UserResource.UserResource_Type);
                implicitType.Name = "Implicit";
                implicitType.Save( );

                // Relationship
                var relationship = EntityModel.Entity.Create<Relationship>( );
                relationship.Name = "TestRel";
                relationship.FromType = forward ? explitType : implicitType;
                relationship.ToType = forward ? implicitType : explitType;
                relationship.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                relationship.SecuresTo = securesTo;
                relationship.SecuresFrom = securesFrom;
                relationship.Save( );

                // Root -> R1
                testData = SetupRelatedEntityAccessSecurityTest( numEntities, true, 1 );

                // Create a report of Root
                testData.Query = TestQueries.Entities( explitType, relationship.ToEnumerable() );

                // Allow read to the root entity
                AddReadAccessRule( testData, 0 );
            }

            // Build the query
            using ( new SetUser( testData.UserAccount ) )
            {
                var settings = new QuerySettings( ) { SecureQuery = true, DebugMode = true };
                queryResult = Factory.NonCachedQuerySqlBuilder.BuildSql( testData.Query, settings );
            }
            string sql = queryResult.Sql;

            if ( securityChecksShouldBeImplicit )
            {
                Assert.That( sql, Is.Not.StringContaining( "with" ) );
                Assert.That( sql, Is.Not.StringContaining( "visIds" ) );
            }
            else
            {
                Assert.That( sql, Is.StringContaining( "with" ) );
                Assert.That( sql, Is.StringContaining( "visIds" ) );
            }
        }


        [Test]
        [RunAsDefaultTenant]        
        public void Test_BaseTypeReport_DerivedTypeAccessRule_AccessSecurity()
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // BaseType -> Derived Type
                testData = SetupInheritedEntityAccessSecurityTest(numEntities);

                // Create report on base type
                testData.Query = TestQueries.Entities(testData.EntityTypes[0]);

                // Allow read only to the derived type                
                new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<Subject>(), testData.EntityTypes[1].As<SecurableEntity>(),
                    TestQueries.Entities(testData.EntityTypes[1]).ToReport());
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities));

            // Check result
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(testData.EntityInstances[testData.EntityTypes[1].Id].Select(e => e.Id)));                        
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_DerivedTypeReport_BaseTypeAccessRule_AccessSecurity()
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // BaseType -> Derived Type
                testData = SetupInheritedEntityAccessSecurityTest(numEntities);

                // Create report on derived type
                testData.Query = TestQueries.Entities(testData.EntityTypes[1]);

                // Allow read only to the base type
                new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<Subject>(), testData.EntityTypes[0].As<SecurableEntity>(),
                    TestQueries.Entities(testData.EntityTypes[0]).ToReport());
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            // Has access to derived
            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities));

            // Check result            
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(testData.EntityInstances[testData.EntityTypes[1].Id].Select(e => e.Id)));
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_BaseTypeReport_BaseTypeAccessRule_AccessSecurity()
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // BaseType -> Derived Type
                testData = SetupInheritedEntityAccessSecurityTest(numEntities);

                // Create report on base type
                testData.Query = TestQueries.Entities(testData.EntityTypes[0]);

                // Allow read only to the base type
                new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<Subject>(), testData.EntityTypes[0].As<SecurableEntity>(),
                    TestQueries.Entities(testData.EntityTypes[0]).ToReport());
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            // Has access to derived and base
            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities * 2));

            // Check result          
            SortedSet<long> ids = new SortedSet<long>();
            ids.UnionWith(testData.EntityInstances[testData.EntityTypes[0].Id].Select(e => e.Id));
            ids.UnionWith(testData.EntityInstances[testData.EntityTypes[1].Id].Select(e => e.Id));

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(ids));
        }

        /// <summary>
        /// Test that running an access rule whose report schema has been modified still runs
        /// and gives no access.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Test_Modify_Schema_AccessSecurity()
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {                
                testData = SetupModifiedSchemaAccessSecurityTest(numEntities);

                // Create report on type
                testData.Query = TestQueries.Entities(testData.EntityTypes[0]);

                ResourceEntity rootEntity = new ResourceEntity
                {
                    EntityTypeId = testData.EntityTypes[0],
                    ExactType = false,
                    NodeId = Guid.NewGuid(),
                    RelatedEntities = new List<SQ.Entity>()
                };

                Field testField = testData.EntityTypes[0].Fields.FirstOrDefault(f => f.Name == "Test String Field");

                StructuredQuery structuredQuery = new StructuredQuery
                {
                    RootEntity = rootEntity,
                    SelectColumns = new List<SelectColumn>()
                };
                structuredQuery.Conditions.Add(new QueryCondition
                {
                    Expression = new ResourceDataColumn(rootEntity, new EntityRef(testField)),
                    Operator = ConditionType.Equal,
                    Argument = new TypedValue("A")
                });
                structuredQuery.Conditions.Add(new QueryCondition
                {
                    Expression = new ResourceDataColumn(rootEntity, new EntityRef("core:name")),
                    Operator = ConditionType.Equal,
                    Argument = new TypedValue("A")
                });
                structuredQuery.SelectColumns.Add(new SelectColumn
                {
                    Expression = new SQ.IdExpression {NodeId = rootEntity.NodeId}
                });

                Report report = structuredQuery.ToReport();
                report.Name = "Test Report";
                report.Save();                                

                // Allow read only to the type
                new AccessRuleFactory().AddAllowReadQuery(
                    testData.UserAccount.As<Subject>(), 
                    testData.EntityTypes[0].As<SecurableEntity>(),
                    report);

                // Modify the schema to make the access rule report invalid
                testData.EntityTypes[0].Fields.Remove(testField);
                testData.EntityTypes[0].Save();

                EntityModel.Entity.Delete(testField);                
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {                
                // The access rule report is now invalid and should generate SQL that gives no access
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
                Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(0));                
            }                        
        }


        /// <summary>
        /// Test for bug 24755 Security: user is missing some access on a parent object when access on child object is gained via security flags
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Test_BaseTypeReport_DerivedTypeSecuresFlags()
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                // BaseType -> Derived Type1 -> Derived Type2
                testData = SetupInheritedEntityAccessSecurityTestMultipleDerivedTypes(numEntities);

                // Create report on base type
                testData.Query = TestQueries.Entities(testData.EntityTypes[0]);

                // Allow read only to the derived type 2 with name A
                new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<Subject>(), testData.EntityTypes[2].As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA(testData.EntityTypes[2]).ToReport());

                ctx.CommitTransaction();
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            // Should only have access to 1 row
            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(1));

            // Check result          
            var ids = new SortedSet<long> { testData.EntityInstances[testData.EntityTypes[2].Id].Where(e => (string)e.GetField("core:name") == "A").Select(e => e.Id).FirstOrDefault()};

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(ids));

            // Enable relationship secures flag
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                testData.RelationshipsToFollow[0].SecuresTo = true;
                testData.RelationshipsToFollow[0].Save();
                ctx.CommitTransaction();
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            // Should now have access to derived type 2 instance plus two instances of derived type 1 that
            // are secured by derived type 2
            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities + 1));

            // Check result          
            // The derived type 2 instance
            // The derived type 1 instances
            ids = new SortedSet<long> { testData.EntityInstances[testData.EntityTypes[2].Id].Where(e => (string)e.GetField("core:name") == "A").Select(e => e.Id).FirstOrDefault() };
            ids.UnionWith(testData.EntityInstances[testData.EntityTypes[1].Id].Select(e => e.Id));

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(ids));
        }


        /// <summary>
        /// Test that related entities show up in a query using the secures flags and faux relationships
        /// </summary>
        [Test]
        [RunAsDefaultTenant]        
        public void Test_RelatedEntity_AccessRule_And_SecuresFlags_FauxRelationships()
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;
            QuerySettings querySettings;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities);
                // Create a report of Root
                testData.Query = TestQueries.Entities(testData.EntityTypes[0], testData.RelationshipsToFollow);

                // Allow read to the R1 entity
                AddReadAccessRule(testData, 1);
            
                // Set secures flag from R1 to Root
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], false, true);                

                // Add faux relationships for root type
                var relatedResource = testData.Query.RootEntity.RelatedEntities.Where(re =>
                {
                    var rr = re as RelatedResource;
                    return rr != null && rr.EntityTypeId.Id == testData.EntityTypes[1].Id;
                }).FirstOrDefault() as RelatedResource;

                relatedResource.FauxRelationships = new FauxRelationships();

                long rootTypeId = testData.EntityTypes [ 0 ].Id;
                long r1TypeId = testData.EntityTypes [ 1 ].Id;

                relatedResource.FauxRelationships.HasTargetResource = true;
                relatedResource.FauxRelationships.HasIncludedResources = true;

                querySettings = new QuerySettings( ) { SecureQuery = true };
                querySettings.TargetResource = testData.EntityInstances [ r1TypeId ] [ 1 ].Id;
                querySettings.IncludeResources = testData.EntityInstances [ rootTypeId ] [ 1 ].Id.ToEnumerable( ).ToList( );
            }

            // Execute the query
            using ( new SetUser( testData.UserAccount ) )
            {
                queryResult = QueryEngine.ExecuteQuery( testData.Query, querySettings );
            }

            Assert.That( queryResult.DataTable, Has.Property( "Rows" ).Count.EqualTo( numEntities - 1 ) );

            CheckQueryResult( queryResult, testData, 0, 0 );
        }



        /// <summary>        
        /// </summary>
        /// <returns></returns>
        private RelatedEntityAccessSecurityTestData SetupRelatedEntityCyclicRelationshipTest(int numEntities)
        {
            var testData = new RelatedEntityAccessSecurityTestData();

            // Create entity types and relationships
            var entityType = EntityModel.Entity.Create<EntityType>();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Name = "RootType";
            entityType.Save();

            testData.EntityTypes.Add(entityType);
            testData.EntityInstances[entityType.Id] = new List<IEntity>();            

            // Create a relationship between the entity types
            Relationship relationship = new Relationship
            {
                FromType = entityType,
                ToType = entityType,
                Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne
            };
            relationship.Save();

            testData.RelationshipsToFollow.Add(relationship);                        
           
            // Create X instances of each entity type
            for (int i = 0; i < numEntities; i++)
            {
                foreach (EntityType et in testData.EntityTypes)
                {
                    var entity = EntityModel.Entity.Create(et);                    
                    entity.SetField("core:name", et.Name + i);                    
                    entity.Save();

                    testData.EntityInstances[et.Id].Add(entity);
                }
            }

            // Setup the relationships between the entities                          
            // Root1 -> Root0
            var fromEntityInstance = testData.EntityInstances[relationship.FromType.Id][1];
            var toEntityInstance = testData.EntityInstances[relationship.ToType.Id][0];

            var relationships = fromEntityInstance.GetRelationships(relationship, Direction.Forward);
            relationships.Add(toEntityInstance);
            fromEntityInstance.SetRelationships(relationship, relationships, Direction.Forward);

            fromEntityInstance.Save();            

            testData.UserAccount = new UserAccount();
            testData.UserAccount.Save();

            return testData;
        }


        /// <summary>
        /// Test that related entities show up in a query using the secures flags and a cyclic relationship i.e. to/from type are the same
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Test_RelatedEntity_AccessRule_And_SecuresFlags_CyclicRelationship()
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                // Root1 -> Root0
                // Root2
                testData = SetupRelatedEntityCyclicRelationshipTest(numEntities);
                // Create a report of Root following relationship in reverse
                testData.Query = TestQueries.Entities(testData.EntityTypes[0], testData.RelationshipsToFollow, false);

                // Allow read to root 0
                AddReadAccessRule(testData, 0, testData.EntityTypes[0].Name + "0");

                // Set secures from flag on relationship to give access to root1
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], false, true);                
            }

            // Execute the query
            using (new SetUser(testData.UserAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
            }

            // The query should return Root0 and Root1
            // Root0 is available via access rule and Root1 is available via secures flag
            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(2));

            var ids = new SortedSet<long> {testData.EntityInstances[testData.EntityTypes[0].Id][0].Id, testData.EntityInstances[testData.EntityTypes[0].Id][1].Id};

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(ids));            
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_SecurityCte_NoTypeCheck_Optimisation_ExactType( )
        {
            // Ensure that if a CTE is of exactly the same type as the parent node, it has no type check of its own.

            StructuredQuery query;
            EntityType entityType;
            IEntity instance;
            UserAccount userAccount;
            QueryResult queryResult;
            const string matchingName = "A";

            using ( new SecurityBypassContext( ) )
            {
                entityType = EntityModel.Entity.Create<EntityType>( );
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save( );

                query = TestQueries.Entities( entityType );

                userAccount = new UserAccount( );
                userAccount.Save( );

                instance = EntityModel.Entity.Create( entityType );
                instance.SetField( "core:name", matchingName );
                instance.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery(
                    userAccount.As<Subject>( ),
                    entityType.As<SecurableEntity>( ),
                    TestQueries.EntitiesWithNameA( entityType ).ToReport( ) );
            }

            using ( new SetUser( userAccount ) )
            {
                queryResult = QueryEngine.ExecuteQuery( query, new QuerySettings( ) { SecureQuery = true } );
            }

            // assert that type check does not appear prior to the main query .. i.e. not in security CTEs
            int mainQuery = queryResult.Sql.IndexOf( "Main query" );
            int typeCheck = queryResult.Sql.IndexOf( "/*isOfType*/" );
            Assert.That( typeCheck, Is.GreaterThan( mainQuery ) );
            Assert.That( queryResult.DataTable, Has.Property( "Rows" ).Count.EqualTo( 1 ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_SecurityCte_NoTypeCheck_Optimisation_ParentType( )
        {
            // Ensure that if a CTE is of exactly the same type as the parent node, it has no type check of its own.

            StructuredQuery query;
            EntityType entityType;
            EntityType parentType;
            IEntity instance;
            IEntity parentInstance;
            UserAccount userAccount;
            QueryResult queryResult;
            const string matchingName = "A";

            using ( new SecurityBypassContext( ) )
            {
                parentType = EntityModel.Entity.Create<EntityType>( );
                parentType.Inherits.Add(UserResource.UserResource_Type);
                parentType.Save( );
                entityType = EntityModel.Entity.Create<EntityType>( );                
                entityType.Inherits.Add( parentType );
                entityType.Save( );

                query = TestQueries.Entities( entityType );

                userAccount = new UserAccount( );
                userAccount.Save( );

                instance = EntityModel.Entity.Create( entityType );
                instance.SetField( "core:name", matchingName );
                instance.Save( );
                parentInstance = EntityModel.Entity.Create( parentType );
                parentInstance.SetField( "core:name", matchingName );
                parentInstance.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery(
                    userAccount.As<Subject>( ),
                    entityType.As<SecurableEntity>( ),
                    TestQueries.EntitiesWithNameA( parentType ).ToReport( ) );
            }

            using ( new SetUser( userAccount ) )
            {
                queryResult = QueryEngine.ExecuteQuery( query, new QuerySettings( ) { SecureQuery = true } );
            }

            // assert that type check does not appear prior to the main query .. i.e. not in security CTEs
            int mainQuery = queryResult.Sql.IndexOf( "Main query" );
            int typeCheck = queryResult.Sql.IndexOf( "/*isOfType*/" );
            Assert.That( typeCheck, Is.GreaterThan( mainQuery ) );
            Assert.That( queryResult.DataTable, Has.Property( "Rows" ).Count.EqualTo( 1 ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_SecurityCte_NoTypeCheck_Optimisation_ChildType( )
        {
            // Ensure that if a CTE is of exactly the same type as the parent node, it has no type check of its own.

            StructuredQuery query;
            EntityType entityType;
            EntityType childType;
            IEntity instance;
            IEntity childInstance;
            UserAccount userAccount;
            QueryResult queryResult;
            const string matchingName = "A";

            using ( new SecurityBypassContext( ) )
            {
                entityType = EntityModel.Entity.Create<EntityType>( );
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save( );
                childType = EntityModel.Entity.Create<EntityType>( );
                childType.Inherits.Add( entityType );
                childType.Save( );

                query = TestQueries.Entities( entityType );

                userAccount = new UserAccount( );
                userAccount.Save( );

                instance = EntityModel.Entity.Create( entityType );
                instance.SetField( "core:name", matchingName );
                instance.Save( );
                childInstance = EntityModel.Entity.Create( childType );
                childInstance.SetField( "core:name", matchingName );
                childInstance.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery(
                    userAccount.As<Subject>( ),
                    childType.As<SecurableEntity>( ),
                    TestQueries.EntitiesWithNameA( childType ).ToReport( ) );
            }

            using ( new SetUser( userAccount ) )
            {
                queryResult = QueryEngine.ExecuteQuery( query, new QuerySettings( ) { SecureQuery = true } );
            }

            // assert that type check DOES appear prior to the main query .. i.e. in security CTEs
            int mainQuery = queryResult.Sql.IndexOf( "Main query" );
            int typeCheck = queryResult.Sql.IndexOf( "/*isOfType*/" );
            Assert.That( typeCheck, Is.LessThan( mainQuery ) );
            Assert.That( queryResult.DataTable, Has.Property( "Rows" ).Count.EqualTo( 1 ) );
        }



        [Test]
        [RunAsDefaultTenant]
        public void Test_SecurityCte_NoTypeCheck_Optimisation_ExactAndChildType( )
        {
            // Ensure that if a CTE is of exactly the same type as the parent node, it has no type check of its own.

            StructuredQuery query;
            EntityType entityType;
            EntityType childType;
            Relationship relationship;
            UserAccount userAccount;
            QueryResult queryResult;

            using ( new SecurityBypassContext( ) )
            {
                entityType = EntityModel.Entity.Create<EntityType>( );
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save( );
                childType = EntityModel.Entity.Create<EntityType>( );
                childType.Inherits.Add( entityType );
                childType.Save( );
                relationship = EntityModel.Entity.Create<Relationship>( );
                relationship.FromType = childType;
                relationship.ToType = entityType;
                relationship.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                relationship.Save( );

                query = TestQueries.Entities( childType, new [ ] { relationship } );

                userAccount = new UserAccount( );
                userAccount.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery(
                    userAccount.As<Subject>( ),
                    childType.As<SecurableEntity>( ),
                    TestQueries.EntitiesWithNameA( childType ).ToReport( ) );
            }

            using ( new SetUser( userAccount ) )
            {
                queryResult = QueryEngine.ExecuteQuery( query, new QuerySettings( ) { SecureQuery = true } );
            }

            // assert that type check DOES appear prior to the main query .. i.e. in security CTEs
            int mainQuery = queryResult.Sql.IndexOf( "Main query" );
            int typeCheck = queryResult.Sql.IndexOf( "/*isOfType*/" );
            Assert.That( typeCheck, Is.LessThan( mainQuery ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_DownCast_AccessToDerivedOnly_Bug25606( )
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;
            EntityType baseType;
            EntityType derivedType;

            using ( new SecurityBypassContext( ) )
            {
                // BaseType -> Derived Type
                testData = SetupInheritedEntityAccessSecurityTest( numEntities );
                baseType = testData.EntityTypes [ 0 ];
                derivedType = testData.EntityTypes [ 1 ];

                // Create report on base type
                var sq = TestQueries.Entities( testData.EntityTypes [ 0 ] );
                sq.RootEntity.RelatedEntities.Add( new DownCastResource { EntityTypeId = new EntityRef( derivedType.Id ) } );
                sq.SelectColumns.Add( new SelectColumn
                {
                    Expression = new EDC.ReadiNow.Metadata.Query.Structured.IdExpression( ) { NodeId = sq.RootEntity.RelatedEntities[0].NodeId }
                } );
                testData.Query = sq;

                // Allow read only to the derived type
                new AccessRuleFactory( ).AddAllowReadQuery( testData.UserAccount.As<Subject>( ), derivedType.As<SecurableEntity>( ),
                    TestQueries.Entities( derivedType ).ToReport( ) );
            }

            // Execute the query
            using ( new SetUser( testData.UserAccount ) )
            {
                queryResult = QueryEngine.ExecuteQuery( testData.Query, new QuerySettings( ) { SecureQuery = true } );
            }

            // Has access to derived only
            Assert.That( queryResult.DataTable, Has.Property( "Rows" ).Count.EqualTo( numEntities * 1 ) );

            // Check result          
            SortedSet<long> ids = new SortedSet<long>( );
            ids.UnionWith( testData.EntityInstances [ derivedType.Id ].Select( e => e.Id ) );

            Assert.That( from DataRow dataRow in queryResult.DataTable.Rows select dataRow [ 0 ],
                Is.EquivalentTo( ids ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_Related_ResourceType( )
        {
            const int numEntities = 2;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;

            using ( new SecurityBypassContext( ) )
            {
                Relationship isOfTypeRel = EDC.ReadiNow.Model.Entity.Get<Relationship>( WellKnownAliases.CurrentTenant.IsOfType );

                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest( numEntities );
                testData.RelationshipsToFollow.Clear( );
                testData.RelationshipsToFollow.Add( isOfTypeRel );
                testData.Query = TestQueries.Entities( testData.EntityTypes [ 0 ], testData.RelationshipsToFollow, true );

                // Allow read only to the root entity
                AddReadAccessRule( testData, 0 );
            }

            // Execute the query
            using ( new SetUser( testData.UserAccount ) )
            {
                queryResult = QueryEngine.ExecuteQuery( testData.Query, new QuerySettings( ) { SecureQuery = true } );
            }

            Assert.That( queryResult.DataTable, Has.Property( "Rows" ).Count.EqualTo( numEntities - 1 ) );

            // Check root column
            CheckQueryResult( queryResult, testData, 0, 0 );

            // Check the resource type column
            long? typeId = ( long ? )(queryResult.DataTable.Rows[ 0 ][ 1 ]);
            Assert.That( typeId, Is.EqualTo( testData.EntityTypes[0].Id ) );
        }


        private IQueryRepository MockSystemAccessRuleQueryFactory(long subject, IEnumerable<AccessRuleQuery> queries)
        {
            Mock<IQueryRepository> accessRuleFactory = new Mock<IQueryRepository>(MockBehavior.Strict);
            accessRuleFactory.Setup(r => r.GetQueries(It.Is<long>(s => s == subject), It.IsAny<EntityRef>(), It.IsAny<IList<long>>())).Returns(queries);
            return accessRuleFactory.Object;
        }

        /// <summary>
        /// Default rules giving access to new type.
        /// No system rules.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]        
        public void Test_DefaultRulesAllNewType_NoSystemRules()
        {
            StructuredQuery query;
            EntityType entityType;
            IEntity[] entities;
            const int numEntities = 3;
            UserAccount userAccount;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                entityType = EntityModel.Entity.Create<EntityType>();                
                entityType.Save();

                entities = new IEntity[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.Entities(entityType).ToReport());
            }

            using (new SetUser(userAccount))
            {
                queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(0));
        }

        /// <summary>        
        /// Default rules giving access to existing type.
        /// No system rules.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Test_DefaultRulesAllExistingType_NoSystemRules()
        {
            StructuredQuery query;
            UserAccount userAccount;
            QueryResult queryResult;

            using (new SecurityBypassContext())
            {
                var entityType = EntityModel.Entity.Get<EntityType>("core:resource");                

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.Entities(entityType).ToReport());
            }

            using (var scope = Factory.Current.BeginLifetimeScope(b => b.RegisterInstance(MockSystemAccessRuleQueryFactory(userAccount.Id, new List<AccessRuleQuery>())).Named<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository)))
            {                
                using (Factory.SetCurrentScope(scope))
                {                    
                    using (new SetUser(userAccount))
                    {
                        queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
                    }
                }                
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(0));
        }

        /// <summary>        
        /// Default rules not giving access to new type.
        /// System rules giving access.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Test_NoDefaultRules_SystemRules()
        {
            StructuredQuery query;
            UserAccount userAccount;
            int numEntities = 3;
            QueryResult queryResult;
            List<AccessRuleQuery> systemAccessRuleQueries;

            using (new SecurityBypassContext())
            {
                var entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                var entities = new IEntity[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                systemAccessRuleQueries = new List<AccessRuleQuery>()
                {
                    new AccessRuleQuery(-1, -1, entityType.Id, TestQueries.Entities(entityType), false)
                };
            }

            using (var scope = Factory.Current.BeginLifetimeScope(b => b.RegisterInstance(MockSystemAccessRuleQueryFactory(userAccount.Id, systemAccessRuleQueries)).Named<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository)))
            {
                using (Factory.SetCurrentScope(scope))
                {
                    using (new SetUser(userAccount))
                    {
                        queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
                    }
                }
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(0));
        }

        /// <summary>        
        /// Default rules not giving some access to new type.
        /// System rules giving some access.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Test_SomeDefaultRules_SomeSystemRules()
        {
            StructuredQuery query;
            UserAccount userAccount;
            int numEntities = 4;
            QueryResult queryResult;
            IEntity[] entities;
            List<AccessRuleQuery> systemAccessRuleQueries;
            const string matchingName = "A";
            const string matchingDescription = "B";

            using (new SecurityBypassContext())
            {
                var entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entities = new IEntity[numEntities];

                // All entities are called A
                // 2 entities also have description of B
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    entities[i].SetField("core:name", matchingName);
                    if (i % 2 == 0)
                    {
                        entities[i].SetField("core:description", matchingDescription);
                    }
                    
                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                // Create default access giving access to entities with name A
                new AccessRuleFactory().AddAllowReadQuery(
                    userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithName(entityType, matchingName).ToReport());

                // Create system access rule giving access to entities with description B
                systemAccessRuleQueries = new List<AccessRuleQuery>()
                {
                    new AccessRuleQuery(-1, -1, entityType.Id, TestQueries.EntitiesWithDescription(matchingDescription, entityType), false)
                };
            }

            using (var scope = Factory.Current.BeginLifetimeScope(b => b.RegisterInstance(MockSystemAccessRuleQueryFactory(userAccount.Id, systemAccessRuleQueries)).Named<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository)))
            {
                using (Factory.SetCurrentScope(scope))
                {
                    using (new SetUser(userAccount))
                    {
                        queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
                    }
                }
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(2));

            var ids = entities.Where(e => (string)e.GetField("core:name") == matchingName && (string) e.GetField("core:description") == matchingDescription).Select(e => e.Id);            

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[0],
                Is.EquivalentTo(ids));
        }

        /// <summary>        
        /// Default rules giving some access.
        /// No system rules giving access.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Test_SomeDefaultRules_NoSystemRules()
        {
            StructuredQuery query;
            UserAccount userAccount;
            int numEntities = 4;
            QueryResult queryResult;
            const string matchingName = "A";
            const string matchingDescription = "B";

            using (new SecurityBypassContext())
            {
                var entityType = EntityModel.Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                var entities = new IEntity[numEntities];

                // All entities are called A
                // 2 entities also have description of B
                for (int i = 0; i < numEntities; i++)
                {
                    entities[i] = EntityModel.Entity.Create(entityType);
                    entities[i].SetField("core:name", matchingName);
                    if (i % 2 == 0)
                    {
                        entities[i].SetField("core:description", matchingDescription);
                    }

                    entities[i].Save();
                }

                query = TestQueries.Entities(entityType);

                userAccount = new UserAccount();
                userAccount.Save();

                // Create default access giving access to entities with name A
                new AccessRuleFactory().AddAllowReadQuery(
                    userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithName(entityType, matchingName).ToReport());                
            }

            using (var scope = Factory.Current.BeginLifetimeScope(b => b.RegisterInstance(MockSystemAccessRuleQueryFactory(userAccount.Id, new List<AccessRuleQuery>())).Named<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository)))
            {
                using (Factory.SetCurrentScope(scope))
                {
                    using (new SetUser(userAccount))
                    {
                        queryResult = QueryEngine.ExecuteQuery(query, new QuerySettings() { SecureQuery = true });
                    }
                }
            }

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(0));            
        }

        [Test]
        [RunAsDefaultTenant]        
        public void Test_RelatedEntity_NoAccessRules_NoSystemRules_SecuresFlags()
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;
            List<AccessRuleQuery> systemAccessRuleQueries;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities);
                // Create a report of the R1, R2, R3
                testData.Query = TestQueries.Entities(testData.EntityTypes[1], testData.RelationshipsToFollow.Skip(1));

                // Create an access rule for the root entities and related entity
                AddReadAccessRule(testData, 0);                   

                // Create system rule only for root entities only               
                systemAccessRuleQueries = new List<AccessRuleQuery>
                {
                    new AccessRuleQuery(-1, -1, testData.EntityTypes[0].Id, TestQueries.Entities(testData.EntityTypes[0]), false)
                };

                // Specify the secures flag only for each of the related entities                
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], true, true);
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[1], true, true);                
            }

            using (var scope = Factory.Current.BeginLifetimeScope(b => b.RegisterInstance(MockSystemAccessRuleQueryFactory(testData.UserAccount.Id, systemAccessRuleQueries)).Named<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository)))
            {
                using (Factory.SetCurrentScope(scope))
                {
                    // Execute the query
                    using (new SetUser(testData.UserAccount))
                    {
                        queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
                    }
                }
            }

            var dbNullRows = Enumerable.Repeat(DBNull.Value, numEntities - 1).ToList();

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities - 1));
            
            // Check R1 column returns valid data.
            // R1 is secured by Root
            CheckQueryResult(queryResult, testData, 0, 1);
                        
            // Check R2 column returns valid data.
            // R2 is secured by R1
            CheckQueryResult(queryResult, testData, 1, 2);            
            
            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[2], Is.EquivalentTo(dbNullRows));            
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_RelatedEntity_AccessRules_NoSystemRules_SecuresFlags()
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;
            List<AccessRuleQuery> systemAccessRuleQueries;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities);
                // Create a report of the R1, R2, R3
                testData.Query = TestQueries.Entities(testData.EntityTypes[1], testData.RelationshipsToFollow.Skip(1));

                // Create an access rule for the root entities and related entity
                AddReadAccessRule(testData, 0);
                AddReadAccessRule(testData, 1);

                // Create system rule only for root entities only               
                systemAccessRuleQueries = new List<AccessRuleQuery>
                {
                    new AccessRuleQuery(-1, -1, testData.EntityTypes[0].Id, TestQueries.Entities(testData.EntityTypes[0]), false)
                };

                // Specify the secures flag only for each of the related entities                
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], true, true);
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[1], true, true);
            }

            using (var scope = Factory.Current.BeginLifetimeScope(b => b.RegisterInstance(MockSystemAccessRuleQueryFactory(testData.UserAccount.Id, systemAccessRuleQueries)).Named<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository)))
            {
                using (Factory.SetCurrentScope(scope))
                {
                    // Execute the query
                    using (new SetUser(testData.UserAccount))
                    {
                        queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
                    }
                }
            }

            var dbNullRows = Enumerable.Repeat(DBNull.Value, numEntities - 1).ToList();

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities - 1));

            // Check R1 column returns valid data.
            // R1 is secured by Root
            CheckQueryResult(queryResult, testData, 0, 1);

            // Check R2 column returns valid data.
            // R2 is secured by R1
            CheckQueryResult(queryResult, testData, 1, 2);

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[2], Is.EquivalentTo(dbNullRows));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_RelatedEntity_SomeAccessRules_SomeSystemRules_SecuresFlags()
        {
            const int numEntities = 3;
            RelatedEntityAccessSecurityTestData testData;
            QueryResult queryResult;
            List<AccessRuleQuery> systemAccessRuleQueries;

            using (new SecurityBypassContext())
            {
                // Root -> R1 -> R2 -> R3
                testData = SetupRelatedEntityAccessSecurityTest(numEntities);
                // Create a report of the R1, R2, R3
                testData.Query = TestQueries.Entities(testData.EntityTypes[1], testData.RelationshipsToFollow.Skip(1));

                // Create a some access rule for the root entities and related entity
                AddReadAccessRule(testData, 0);

                // Create a some access rule for related entity
                AddReadAccessRule(testData, 1);

                // Create system rule only for root and related entities
                systemAccessRuleQueries = new List<AccessRuleQuery>
                {
                    new AccessRuleQuery(-1, -1, testData.EntityTypes[0].Id, TestQueries.EntitiesWithDescription(testData.EntityTypes[0].Name, testData.EntityTypes[0]), false),
                    new AccessRuleQuery(-2, -2, testData.EntityTypes[1].Id, TestQueries.EntitiesWithDescription(testData.EntityTypes[1].Name, testData.EntityTypes[1]), false)
                };

                // Specify the secures flag only for each of the related entities                
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[0], true, true);
                SetRelationshipSecuresFlags(testData.RelationshipsToFollow[1], true, true);
            }

            using (var scope = Factory.Current.BeginLifetimeScope(b => b.RegisterInstance(MockSystemAccessRuleQueryFactory(testData.UserAccount.Id, systemAccessRuleQueries)).Named<IQueryRepository>(EntityAccessControlModule.SystemQueryRepository)))
            {
                using (Factory.SetCurrentScope(scope))
                {
                    // Execute the query
                    using (new SetUser(testData.UserAccount))
                    {
                        queryResult = QueryEngine.ExecuteQuery(testData.Query, new QuerySettings() { SecureQuery = true });
                    }
                }
            }

            var dbNullRows = Enumerable.Repeat(DBNull.Value, numEntities - 1).ToList();

            Assert.That(queryResult.DataTable, Has.Property("Rows").Count.EqualTo(numEntities - 1));

            // Check R1 column returns valid data.
            // R1 is secured by Root
            CheckQueryResult(queryResult, testData, 0, 1);

            // Check R2 column returns valid data.
            // R2 is secured by R1
            CheckQueryResult(queryResult, testData, 1, 2);

            Assert.That(from DataRow dataRow in queryResult.DataTable.Rows select dataRow[2], Is.EquivalentTo(dbNullRows));
        }
    }
}
