// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EDC.Common;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.SoftwarePlatform.WebApi.Test.Security
{
    [TestFixture]
    [Category("ExtendedTests")]
    [Category("SecurityTests")]
    public class EntityV2Tests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase(false)]
        [TestCase(true)]
        public void Test_Get(bool grantAccess)
        {
            UserAccount userAccount;
            List<long> entitiesToDelete;
            EntityType entityType;
            IEntity entity;
            HttpWebResponse response;
            JsonQueryResult result;

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = Entity.Create<UserAccount>();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                entity = Entity.Create(new EntityRef(entityType));
                entity.SetField("core:name", "A"); // "A" so it will match the access rule
                entity.Save();
                entitiesToDelete.Add(entity.Id);

                if (grantAccess)
                {
                    new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                        entityType.As<SecurableEntity>(),
                        TestQueries.EntitiesWithNameA().ToReport());
                }

                // Actual test
                using (
                    var request = new PlatformHttpRequest(
                        string.Format(@"data/v2/entity/a/{0}?request=name", entity.Id), PlatformHttpMethod.Get,
                        userAccount))
                {
                    response = request.GetResponse();
                    Assert.That(response.StatusCode,
                        Is.EqualTo(grantAccess ? HttpStatusCode.OK : HttpStatusCode.Forbidden));

                    result = request.DeserialiseResponseBody<JsonQueryResult>();
                    if (grantAccess)
                    {
                        Assert.That(result.Ids, Has.Exactly(1).EqualTo(entity.Id));
                    }
                    else
                    {
                        Assert.That(result.Ids, Is.Empty);
                        Assert.That(result.Members, Is.Empty);
                        Assert.That(result.Entities, Is.Empty);
                    }
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(entitiesToDelete);
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(false)]
        [TestCase(true)]
        public void Test_GetEntitiesOfType(bool grantAccess)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            IEntity entity2 = null;
            HttpWebResponse response;
            JsonQueryResult result;

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity1 = Entity.Create(new EntityRef(entityType));
                entity1.SetField("core:name", "A"); // "A" so it will match the access rule
                entity1.Save();

                entity2 = Entity.Create(new EntityRef(entityType));
                entity2.SetField("core:name", "B");
                entity2.Save();

                if (grantAccess)
                {
                    new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                        entityType.As<SecurableEntity>(),
                        TestQueries.EntitiesWithNameA().ToReport());

                    // First Sanity Check. Check directly to avoid any caching or side effect issue.
                    IDictionary<long, bool> results = new EntityAccessControlChecker().CheckAccess(
                        new[] {new EntityRef(entity1), new EntityRef(entity2)},
                        new[] {Permissions.Read},
                        new EntityRef(userAccount));
                    Assert.That(results, Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").True,
                        "EntityAccessControlChecker.CheckAccess: No access to Entity ID 1");
                    Assert.That(results, Has.Exactly(1).Property("Key").EqualTo(entity2.Id).And.Property("Value").False,
                        "EntityAccessControlChecker.CheckAccess: Access to Entity ID 2");

                    // Second sanity check.
                    using (new SetUser(userAccount))
                    {
                        IEnumerable<IEntity> entities = Entity.GetInstancesOfType(entityType, true, "name");
                        Assert.That(entities.Count(), Is.EqualTo(1),
                            "Entity.GetInstancesOfType: Incorrect count");
                        Assert.That(entities, Has.Exactly(1).Property("Id").EqualTo(entity1.Id),
                            "Entity.GetInstancesOfType: Incorrect Id");
                    }
                }

                // Actual test
                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        Queries = new[] {"id"},
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest
                            {
                                Ids = new[] {entityType.Id},
                                QueryIndex = 0,
                                QueryType = QueryType.Instances
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    if (grantAccess)
                    {
                        result = request.DeserialiseResponseBody<JsonQueryResult>();

                        // Check entity results
                        Assert.That(result.Results, Has.Count.EqualTo(1));
                        Assert.That(result.Results.First().Code, Is.EqualTo(HttpStatusCode.OK));
                        Assert.That(result.Results.First().Ids, Is.EquivalentTo(new[] {entity1.Id}));
                    }
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity1 != null)
                {
                    try
                    {
                        entity1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity2 != null)
                {
                    try
                    {
                        entity2.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType != null)
                {
                    // Should clean up access control entities if missed above
                    try
                    {
                        entityType.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(QueryType.Basic)]
        [TestCase(QueryType.BasicWithDemand)]
        public void Test_GetEntityWithFields(QueryType queryType)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            HttpWebResponse response;
            JsonQueryResult result;
            const string testName = "A";
            const string testDescription = "foo";
            const string testAlias = "bar";

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity1 = Entity.Create(new EntityRef(entityType));
                entity1.SetField("core:name", testName); // "A" so it will match the access rule
                entity1.SetField("core:description", testDescription);
                entity1.SetField("core:alias", testAlias);
                entity1.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA().ToReport());

                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        Queries = new[] {"name, alias, description"},
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest
                            {
                                Ids = new[] {entity1.Id},
                                QueryIndex = 0,
                                QueryType = queryType
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    result = request.DeserialiseResponseBody<JsonQueryResult>();

                    // Check entity results
                    Assert.That(result.Results, Has.Count.EqualTo(1));
                    Assert.That(result.Results.First().Code, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(result.Results.First().Ids, Is.EquivalentTo(new[] {entity1.Id}));

                    // Check the requested fields are returned
                    Assert.That(result.Members, Has.Count.EqualTo(3));
                    Assert.That(result.Members.Skip(0).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:name"));
                    Assert.That(result.Members.Skip(1).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:alias"));
                    Assert.That(result.Members.Skip(2).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:description"));

                    // Ensure the field values are loaded
                    Assert.That(result.Entities, Has.Count.EqualTo(1));
                    Assert.That(result.Entities.First().Value,
                        Has.Exactly(1)
                            .Property("Key")
                            .EqualTo(result.Members.First(x => x.Value.Alias == "core:name").Key.ToString())
							.And.JsonProperty( "Value" ).EqualTo( testName ) );
                    Assert.That(result.Entities.First().Value,
                        Has.Exactly(1)
                            .Property("Key")
                            .EqualTo(result.Members.First(x => x.Value.Alias == "core:description").Key.ToString())
							.And.JsonProperty( "Value" ).EqualTo( testDescription ) );
                    Assert.That(result.Entities.First().Value,
                        Has.Exactly(1)
                            .Property("Key")
                            .EqualTo(result.Members.First(x => x.Value.Alias == "core:alias").Key.ToString())
							.And.JsonProperty( "Value" ).EqualTo( "core:" + testAlias ) );
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity1 != null)
                {
                    try
                    {
                        entity1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(QueryType.Basic, HttpStatusCode.OK)]
        [TestCase(QueryType.BasicWithDemand, HttpStatusCode.Forbidden)]
        public void Test_GetMultipleEntities(QueryType queryType, HttpStatusCode expectedResult)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            IEntity entity2 = null;
            IEntity entity3 = null;
            HttpWebResponse response;
            JsonQueryResult result;

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity1 = Entity.Create(new EntityRef(entityType));
                entity1.SetField("core:name", "A");
                entity1.Save();

                entity2 = Entity.Create(new EntityRef(entityType));
                entity2.SetField("core:name", "B");
                entity2.Save();

                entity3 = Entity.Create(new EntityRef(entityType));
                entity3.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA().ToReport());

                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        Queries = new[] {"id"},
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest
                            {
                                Ids = new[] {entity1.Id, entity2.Id, entity3.Id},
                                QueryIndex = 0,
                                QueryType = queryType
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    result = request.DeserialiseResponseBody<JsonQueryResult>();

                    Assert.That(result.Results, Has.Count.EqualTo(1));
                    Assert.That(result.Results.First().Code, Is.EqualTo(expectedResult));
                    Assert.That(result.Results.First().Ids, Is.Not.Null);
                    if (expectedResult == HttpStatusCode.OK)
                    {
                        Assert.That(result.Results.First().Ids, Is.EquivalentTo(new[] {entity1.Id}));
                    }
                    else
                    {
                        Assert.That(result.Results.First().Ids, Is.Empty);
                    }
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity1 != null)
                {
                    try
                    {
                        entity1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity2 != null)
                {
                    try
                    {
                        entity2.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity3 != null)
                {
                    try
                    {
                        entity3.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(QueryType.BasicWithDemand, "", "", HttpStatusCode.Forbidden)]
        [TestCase(QueryType.BasicWithDemand, "", "core:read", HttpStatusCode.Forbidden)]
        [TestCase(QueryType.BasicWithDemand, "core:read", "", HttpStatusCode.OK)]
        [TestCase(QueryType.BasicWithDemand, "core:read", "core:read", HttpStatusCode.OK)]
        [TestCase(QueryType.BasicWithDemand, "core:modify", "core:read", HttpStatusCode.Forbidden)]
        [TestCase(QueryType.Basic, "", "", HttpStatusCode.NotFound)]
        [TestCase(QueryType.Basic, "", "core:read", HttpStatusCode.NotFound)]
        [TestCase(QueryType.Basic, "core:read", "", HttpStatusCode.OK)]
        [TestCase(QueryType.Basic, "core:read", "core:read", HttpStatusCode.OK)]
        [TestCase(QueryType.Basic, "core:modify", "core:read", HttpStatusCode.NotFound)]
        public void Test_GetSingleRelatonship(QueryType queryType, string entityType1Permissions,
            string entityType2Permissions,
            HttpStatusCode expectedResponse)
        {
            UserAccount userAccount = null;
            EntityType entityType1 = null;
            EntityType entityType2 = null;
            Relationship relationshipType = null;
            IEntityRelationshipCollection<IEntity> relationshipCollection;
            IEntity entity1 = null;
            IEntity entity2 = null;
            IEntity entity3 = null;
            HttpWebResponse response;
            JsonQueryResult result;
            string[] splitEntityType1Permissions;
            string[] splitEntityType2Permissions;

            splitEntityType1Permissions = entityType1Permissions.Split(new[] {','},
                StringSplitOptions.RemoveEmptyEntries);
            splitEntityType2Permissions = entityType2Permissions.Split(new[] {','},
                StringSplitOptions.RemoveEmptyEntries);

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType2 = new EntityType();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();

                entity2 = Entity.Create(new EntityRef(entityType2));
                entity2.SetField("core:name", "A");
                entity2.Save();

                entity3 = Entity.Create(new EntityRef(entityType2));
                entity3.SetField("core:name", "B");
                entity3.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                relationshipType = new Relationship();
                relationshipType.Name = "relationshipTest";
                relationshipType.Alias = "relationshipTest";
                relationshipType.FromType = entityType1;
                relationshipType.FromName = "From";
                relationshipType.ToType = entityType2;
                relationshipType.ToName = "To";
                relationshipType.RelType = Entity.Get<RelTypeEnum>("relComponents");
                relationshipType.Cardinality = Entity.Get<CardinalityEnum>("oneToMany");
                relationshipType.Save();

                entityType1.Relationships.Add(relationshipType);
                entityType1.Save();

                entity1 = Entity.Create(new EntityRef(entityType1));
                relationshipCollection = entity1.GetRelationships(relationshipType);
                relationshipCollection.Add(entity2);
                relationshipCollection.Add(entity3);
                entity1.SetRelationships(relationshipType, relationshipCollection);
                entity1.Save();

                new AccessRuleFactory().AddAllowByQuery(
                    userAccount.As<Subject>(),
                    entityType1.As<SecurableEntity>(),
                    splitEntityType1Permissions.Select(s => new EntityRef(s)),
                    TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllowByQuery(
                    userAccount.As<Subject>(),
                    entityType2.As<SecurableEntity>(),
                    splitEntityType2Permissions.Select(s => new EntityRef(s)),
                    TestQueries.EntitiesWithNameA().ToReport());

                // Sanity check
                using (new SetUser(userAccount))
                {
                    IDictionary<long, bool> checkResult = Factory.EntityAccessControlService.Check(
                        new[] {new EntityRef(entity1), new EntityRef(entity2), new EntityRef(entity3)},
                        new[] {Permissions.Read});

                    if (splitEntityType1Permissions.Contains("core:read"))
                    {
                        Assert.That(checkResult,
                            Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").True);
                    }
                    if (splitEntityType2Permissions.Contains("core:read"))
                    {
                        Assert.That(checkResult,
                            Has.Exactly(1).Property("Key").EqualTo(entity2.Id).And.Property("Value").True);
                    }
                    Assert.That(checkResult,
                        Has.Exactly(1).Property("Key").EqualTo(entity3.Id).And.Property("Value").False);
                }

                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        // Use Id rather than alias to circumvent alias caching
                        Queries = new[] {"id, #" + relationshipType.Id + ".{id}"},
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest()
                            {
                                Ids = new[] {entity1.Id},
                                QueryIndex = 0,
                                QueryType = queryType
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    result = request.DeserialiseResponseBody<JsonQueryResult>();

                    Assert.That(result.Results, Has.Count.EqualTo(1));
                    Assert.That(result.Results.First().Code, Is.EqualTo(expectedResponse));
                    Assert.That(result.Results.First().Ids, Is.Not.Null);
                    Assert.That(result.Results.First().Ids,
                        Is.EquivalentTo(expectedResponse == HttpStatusCode.OK ? new[] {entity1.Id} : new long[0]));
                    if (splitEntityType2Permissions.Contains("core:read") && expectedResponse == HttpStatusCode.OK)
                    {
	                    var field = ( ( dynamic ) result.Entities[ entity1.Id ][ relationshipType.Id.ToString( ) ] ).f;

	                    var value = (long)field[ 0 ];

	                    Assert.That( value, Is.EqualTo( entity2.Id ) );
                    }
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity1 != null)
                {
                    try
                    {
                        entity1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity2 != null)
                {
                    try
                    {
                        entity2.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity3 != null)
                {
                    try
                    {
                        entity3.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (relationshipType != null)
                {
                    try
                    {
                        relationshipType.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType1 != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType2 != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(QueryType.BasicWithDemand, "0", "0", HttpStatusCode.OK)]
        [TestCase(QueryType.BasicWithDemand, "1", "", HttpStatusCode.Forbidden)]
        [TestCase(QueryType.BasicWithDemand, "2", "2", HttpStatusCode.OK)]
        [TestCase(QueryType.BasicWithDemand, "0,1", "", HttpStatusCode.Forbidden)]
        [TestCase(QueryType.BasicWithDemand, "1,2", "", HttpStatusCode.Forbidden)]
        [TestCase(QueryType.BasicWithDemand, "0,2", "0,2", HttpStatusCode.OK)]
        [TestCase(QueryType.BasicWithDemand, "0,1,2", "", HttpStatusCode.Forbidden)]
        [TestCase(QueryType.Basic, "0", "0", HttpStatusCode.OK)]
        [TestCase(QueryType.Basic, "1", "", HttpStatusCode.NotFound)]
        [TestCase(QueryType.Basic, "2", "2", HttpStatusCode.OK)]
        [TestCase(QueryType.Basic, "0,1", "0", HttpStatusCode.OK)]
        [TestCase(QueryType.Basic, "1,2", "2", HttpStatusCode.OK)]
        [TestCase(QueryType.Basic, "0,2", "0,2", HttpStatusCode.OK)]
        [TestCase(QueryType.Basic, "0,1,2", "0,2", HttpStatusCode.OK)]
        public void Test_DerivedTypes(QueryType queryType, string entitiesIndexesToRequest,
            string expectedEntityIndexReturned, HttpStatusCode expectedResult)
        {
            UserAccount userAccount = null;
            EntityType entityType1 = null;
            EntityType entityType2 = null;
            IEntity[] entities;
            HttpWebResponse response;
            JsonQueryResult result;
            long[] splitEntityIds;
            long[] splitExpectedEntityIds;

            entities = new IEntity[3];
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entityType2 = new EntityType();
                entityType2.Inherits.Add(entityType1);
                entityType2.Save();

                entities[0] = Entity.Create(new EntityRef(entityType1));
                entities[0].SetField("core:name", "A");
                entities[0].Save();

                entities[1] = Entity.Create(new EntityRef(entityType1));
                entities[1].SetField("core:name", "B");
                entities[1].Save();

                entities[2] = Entity.Create(new EntityRef(entityType2));
                entities[2].SetField("core:name", "B");
                entities[2].Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType1.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA().ToReport());
                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType2.As<SecurableEntity>(),
                    TestQueries.Entities().ToReport());

                // Sanity check
                using (new SetUser(userAccount))
                {
                    IDictionary<long, bool> checkResult = Factory.EntityAccessControlService.Check(
                        entities.Select(e => new EntityRef(e)).ToList(),
                        new[] {Permissions.Read});

                    Assert.That(checkResult,
                        Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").True);
                    Assert.That(checkResult,
                        Has.Exactly(1).Property("Key").EqualTo(entities[1].Id).And.Property("Value").False);
                    Assert.That(checkResult,
                        Has.Exactly(1).Property("Key").EqualTo(entities[2].Id).And.Property("Value").True);
                }

                splitEntityIds =
                    entitiesIndexesToRequest.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => entities[int.Parse(x)].Id)
                        .ToArray();
                splitExpectedEntityIds =
                    expectedEntityIndexReturned.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => entities[int.Parse(x)].Id)
                        .ToArray();

                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        Queries = new[] {"id"},
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest
                            {
                                Ids = splitEntityIds.ToArray(),
                                QueryIndex = 0,
                                QueryType = queryType
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    result = request.DeserialiseResponseBody<JsonQueryResult>();

                    Assert.That(result.Results, Has.Count.EqualTo(1));
                    Assert.That(result.Results.First().Code, Is.EqualTo(expectedResult));
                    Assert.That(result.Results.First().Ids, Is.EquivalentTo(splitExpectedEntityIds));
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    Entity.Delete(entities.Select(x => x.Id));
                }
                catch (Exception)
                {
                }

                if (entityType1 != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType2 != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(QueryType.BasicWithDemand, HttpStatusCode.Forbidden)]
        [TestCase(QueryType.Basic, HttpStatusCode.NotFound)]
        public void Test_NoAccess(QueryType queryType, HttpStatusCode expectedResult)
        {
            UserAccount userAccount = null;
            EntityType entityType1 = null;
            IEntity entity1 = null;
            HttpWebResponse response;
            JsonQueryResult result;

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType1 = new EntityType();
                entityType1.Save();

                entity1 = Entity.Create(new EntityRef(entityType1));
                entity1.Save();

                // Sanity check
                using (new SetUser(userAccount))
                {
                    IDictionary<long, bool> checkResult = Factory.EntityAccessControlService.Check(
                        new[] {new EntityRef(entity1)},
                        new[] {Permissions.Read});

                    Assert.That(checkResult,
                        Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").False);
                }

                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        Queries = new[] {"id"},
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest
                            {
                                Ids = new[] {entity1.Id},
                                QueryIndex = 0,
                                QueryType = queryType
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    result = request.DeserialiseResponseBody<JsonQueryResult>();

                    Assert.That(result.Results, Has.Count.EqualTo(1));
                    Assert.That(result.Results.First().Code, Is.EqualTo(expectedResult));
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity1 != null)
                {
                    try
                    {
                        entity1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType1 != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_AccessControlFields_ForInstance(bool grantAccess)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            HttpWebResponse response;
            JsonQueryResult result;
            const string testName = "A";
            const bool denied = false;
            const bool granted = true;

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity1 = Entity.Create(new EntityRef(entityType));
                entity1.SetField("core:name", testName); // "A" so it will match the access rule
                entity1.Save();

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA().ToReport());

                if (grantAccess)
                {
                    new AccessRuleFactory().AddAllowModifyQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                        TestQueries.EntitiesWithNameA().ToReport());

                    new AccessRuleFactory().AddAllowDeleteQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                        TestQueries.EntitiesWithNameA().ToReport());
                }
                

                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        Queries = new[] { "name, canModify, canDelete" },
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest
                            {
                                Ids = new[] {entity1.Id},
                                QueryIndex = 0,
                                QueryType = QueryType.Basic
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    result = request.DeserialiseResponseBody<JsonQueryResult>();

                    // Check entity results
                    Assert.That(result.Results, Has.Count.EqualTo(1));
                    Assert.That(result.Results.First().Code, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(result.Results.First().Ids, Is.EquivalentTo(new[] { entity1.Id }));

                    // Check the requested fields are returned
                    Assert.That(result.Members, Has.Count.EqualTo(3));
                    Assert.That(result.Members.Skip(0).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:name"));
                    Assert.That(result.Members.Skip(1).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:canModify"));
                    Assert.That(result.Members.Skip(2).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:canDelete"));

                    // Ensure the field values are loaded
                    Assert.That(result.Entities, Has.Count.EqualTo(1));

                    if (grantAccess)
                    {
                        Assert.That(result.Entities.First().Value,
                        Has.Exactly(1)
                            .Property("Key")
                            .EqualTo(result.Members.First(x => x.Value.Alias == "core:name").Key.ToString())
                            .And.JsonProperty("Value").EqualTo(testName));
                        Assert.That(result.Entities.First().Value,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(result.Members.First(x => x.Value.Alias == "core:canModify").Key.ToString())
								.And.JsonProperty( "Value" ).EqualTo( granted ) );
                        Assert.That(result.Entities.First().Value,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(result.Members.First(x => x.Value.Alias == "core:canDelete").Key.ToString())
								.And.JsonProperty( "Value" ).EqualTo( granted ) );
                    }
                    else
                    {
                        Assert.That(result.Entities.First().Value,
                        Has.Exactly(1)
                            .Property("Key")
                            .EqualTo(result.Members.First(x => x.Value.Alias == "core:name").Key.ToString())
							.And.JsonProperty( "Value" ).EqualTo( testName ) );
                        Assert.That(result.Entities.First().Value,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(result.Members.First(x => x.Value.Alias == "core:canModify").Key.ToString())
								.And.JsonProperty( "Value" ).EqualTo( denied ) );
                        Assert.That(result.Entities.First().Value,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(result.Members.First(x => x.Value.Alias == "core:canDelete").Key.ToString())
								.And.JsonProperty( "Value" ).EqualTo( denied ) );
                    }
                    
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entity1 != null)
                {
                    try
                    {
                        entity1.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_AccessControlFields_ForType(bool grantAccess)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            HttpWebResponse response;
            JsonQueryResult result;
            const string testName = "A";
            const bool denied = false;
            const bool granted = true;

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.SetField("core:name", testName);
                entityType.Save();

                if (grantAccess)
                {
                    new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(), entityType.As<SecurableEntity>());
                }

                using (
                    var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonQueryBatchRequest
                    {
                        Queries = new[] { "name, core:canCreateType" },
                        Requests = new[]
                        {
                            new JsonQuerySingleRequest
                            {
                                Ids = new[] {entityType.Id},
                                QueryIndex = 0,
                                QueryType = QueryType.Basic
                            }
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    result = request.DeserialiseResponseBody<JsonQueryResult>();

                    // Check entity results
                    Assert.That(result.Results, Has.Count.EqualTo(1));
                    Assert.That(result.Results.First().Code, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(result.Results.First().Ids, Is.EquivalentTo(new[] { entityType.Id }));

                    // Check the requested fields are returned
                    Assert.That(result.Members, Has.Count.EqualTo(2));
                    Assert.That(result.Members.Skip(0).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:name"));
                    Assert.That(result.Members.Skip(1).First().Value,
                        Is.Not.Null.And.Property("Alias").EqualTo("core:canCreateType"));

                    // Ensure the field values are loaded
                    Assert.That(result.Entities, Has.Count.EqualTo(1));

                    if (grantAccess)
                    {
                        Assert.That(result.Entities.First().Value,
                        Has.Exactly(1)
                            .Property("Key")
                            .EqualTo(result.Members.First(x => x.Value.Alias == "core:name").Key.ToString())
							.And.JsonProperty( "Value" ).EqualTo( testName ) );
                        Assert.That(result.Entities.First().Value,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(result.Members.First(x => x.Value.Alias == "core:canCreateType").Key.ToString())
								.And.JsonProperty( "Value" ).EqualTo( granted ) );
                    }
                    else
                    {
                        Assert.That(result.Entities.First().Value,
                        Has.Exactly(1)
                            .Property("Key")
                            .EqualTo(result.Members.First(x => x.Value.Alias == "core:name").Key.ToString())
							.And.JsonProperty( "Value" ).EqualTo( testName ) );
                        Assert.That(result.Entities.First().Value,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(result.Members.First(x => x.Value.Alias == "core:canCreateType").Key.ToString())
								.And.JsonProperty( "Value" ).EqualTo( denied ) );
                    }
                    
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try
                    {
                        userAccount.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (entityType != null)
                {
                    // Should clean up access control entities
                    try
                    {
                        entityType.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
