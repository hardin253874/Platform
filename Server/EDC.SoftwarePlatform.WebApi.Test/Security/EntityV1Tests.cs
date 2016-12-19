// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Controllers.Tablet;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using JsonRelationshipData = EDC.SoftwarePlatform.WebApi.Controllers.Entity.JsonRelationshipData;
using Relationship = EDC.ReadiNow.Model.Relationship;

namespace EDC.SoftwarePlatform.WebApi.Test.Security
{
    [TestFixture]
    //[Category("ExtendedTests")]
    [Category("SecurityTests")]
    public class EntityV1Tests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_SimpleCreate(bool allowCreate)
        {
            EntityType entityType;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            long id = EntityId.Max;
            long newId;
            IEntity newEntity;

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                if (allowCreate)
                {
                    new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(),
                        entityType.As<SecurableEntity>());

                    // Sanity Check.
                    using (new SetUser(userAccount))
                    {
                        Assert.That(Factory.EntityAccessControlService.CanCreate(entityType), Is.True,
                            "User cannot create type");
                    }
                }

                // Actual test
                using (var  request = new PlatformHttpRequest(@"data/v1/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonEntityQueryResult
                    {
                        Ids = new List<long> { id },
                        Entities = new List<JsonEntity>
                        {
                            new JsonEntity
                            {
                                Id = id,
                                TypeIds = new List<long>{ entityType.Id },
                                Fields = new List<JsonFieldData>(),
                                Relationships = new List<JsonRelationshipData>(),
                                DataState = DataState.Create
                            }
                        },
                        EntityRefs = new List<JsonEntityRef>
                        {
                            new JsonEntityRef(new EntityRef(id)),
                            new JsonEntityRef(new EntityRef(entityType))
                        }
                    });

                    response = request.GetResponse();

                    if (allowCreate)
                    {
                        Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                            "Web service call failed");

                        newId = request.DeserialiseResponseBody<long>();
                        entitiesToDelete.Add(newId);

                        newEntity = Entity.Get(newId);
                        Assert.That(newEntity, Is.Not.Null, "New entity does not exist");
                        Assert.That(newEntity, Has.Property("TypeIds").Contains(entityType.Id),
                            "New entity missing correct type");
                    }
                    else
                    {
                        Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.Forbidden),
                            "Web service call failed");
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
        public void Test_CreateWithField()
        {
            EntityType entityType;
            StringField field;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            long id = EntityId.Max;
            long newId;
            IEntity newEntity;
            const string fieldValue = "foo";

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                field = Entity.Create<StringField>();
                field.Name = "Test field " + Guid.NewGuid();
                field.Save();
                entitiesToDelete.Add(field.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Fields.Add(field.As<Field>());
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>());

                using (var request = new PlatformHttpRequest(@"data/v1/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonEntityQueryResult
                    {
                        Ids = new List<long> { id },
                        Entities = new List<JsonEntity>
                        {
                            new JsonEntity
                            {
                                Id = id,
                                TypeIds = new List<long>{ entityType.Id },
                                Fields = new List<JsonFieldData>
                                {
                                    new JsonFieldData
                                    {
                                        FieldId = field.Id,
                                        Value = fieldValue,
                                        TypeName = "String"
                                    }
                                },
                                Relationships = new List<JsonRelationshipData>(),
                                DataState = DataState.Create
                            }
                        },
                        EntityRefs = new List<JsonEntityRef>
                        {
                            new JsonEntityRef(new EntityRef(id)),
                            new JsonEntityRef(new EntityRef(entityType)),
                            new JsonEntityRef(new EntityRef(field))
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");

                    newId = request.DeserialiseResponseBody<long>();
                    entitiesToDelete.Add(newId);

                    newEntity = Entity.Get(newId);
                    Assert.That(newEntity, Is.Not.Null, "New entity does not exist");
                    Assert.That(newEntity, Has.Property("TypeIds").Contains(entityType.Id),
                        "New entity missing correct type");
                    Assert.That(newEntity.GetField(field), Is.EqualTo(fieldValue),
                        "Field value incorrect");
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
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Test_CreateWithRelationship(bool allowCreateEntity1, bool allowReadEntityType2)
        {
            EntityType entityType1;
            EntityType entityType2;
            Relationship relationship;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            long entity1Id = EntityId.Max;
            IEntity entity2;
            long newId;
            IEntity newEntity;

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType1 = Entity.Create<EntityType>();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();
                entitiesToDelete.Add(entityType1.Id);

                entityType2 = Entity.Create<EntityType>();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();
                entitiesToDelete.Add(entityType2.Id);

                entity2 = Entity.Create(entityType2);
                entity2.Save();
                entitiesToDelete.Add(entity2.Id);

                relationship = Entity.Create<Relationship>();
                relationship.FromType = entityType1;
                relationship.ToType = entityType2;
                relationship.RelType = Entity.Get<RelTypeEnum>("core:relManyToMany");
                relationship.Save();
                entitiesToDelete.Add(relationship.Id);

                if (allowCreateEntity1)
                {
                    new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(),
                        entityType1.As<SecurableEntity>());
                }
                if(allowReadEntityType2)
                {
                    new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                        entityType2.As<SecurableEntity>(), TestQueries.Entities().ToReport());
                }

                using (var request = new PlatformHttpRequest(@"data/v1/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonEntityQueryResult
                    {
                        Ids = new List<long> { entity1Id },
                        Entities = new List<JsonEntity>
                        {
                            new JsonEntity
                            {
                                Id = entity1Id,
                                TypeIds = new List<long>{ entityType1.Id },
                                Fields = new List<JsonFieldData>(),
                                Relationships = new List<JsonRelationshipData>
                                {
                                    new JsonRelationshipData
                                    {
                                        RelTypeId = new JsonEntityRef(relationship),
                                        Instances = new List<JsonRelationshipInstanceData>
                                        {
                                            new JsonRelationshipInstanceData
                                            {
                                                Entity = entity2.Id,
                                                RelEntity = 0,
                                                DataState = DataState.Create
                                            }
                                        }
                                    }
                                },
                                DataState = DataState.Create
                            }
                        },
                        EntityRefs = new List<JsonEntityRef>
                        {
                            new JsonEntityRef(new EntityRef(entity1Id)),
                            new JsonEntityRef(new EntityRef(entity2.Id)),
                            new JsonEntityRef(new EntityRef(entityType1)),
                            new JsonEntityRef(new EntityRef(entityType2)),
                            new JsonEntityRef(new EntityRef(relationship))
                        }
                    });

                    response = request.GetResponse();

                    if (allowCreateEntity1 && allowReadEntityType2)
                    {
                        Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                            "Web service call failed");

                        newId = request.DeserialiseResponseBody<long>();
                        entitiesToDelete.Add(newId);

                        newEntity = Entity.Get(newId);
                        Assert.That(newEntity, Is.Not.Null, "New entity does not exist");
                        Assert.That(newEntity, Has.Property("TypeIds").Contains(entityType1.Id),
                            "New entity missing correct type");
                        Assert.That(newEntity.GetRelationships(relationship), Has.Count.EqualTo(1),
                            "Relationship count incorrect");
                    }
                    else
                    {
                        Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.Forbidden),
                            "Web service call failed");                        
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
        [TestCase("", HttpStatusCode.Forbidden)]
        [TestCase("core:read", HttpStatusCode.Forbidden)]
        //[TestCase("core:modify", HttpStatusCode.Forbidden)] // this is an invalid configuration
        [TestCase("core:read,core:modify", HttpStatusCode.OK)]
        [TestCase("core:read,core:modify,core:delete", HttpStatusCode.OK)]
        public void Test_ModifyField(string permissions, HttpStatusCode expectedResult)
        {
            EntityType entityType;
            StringField field;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            IEntity entity;
            IEntity modifiedEntity;
            const string initialFieldValue = "foo";
            const string newFieldValue = "bar";
            string[] splitPermissions;

            splitPermissions = permissions.Split(new [] { ',' }, 
                StringSplitOptions.RemoveEmptyEntries);

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                field = Entity.Create<StringField>();
                field.Name = "Test field " + Guid.NewGuid();
                field.Save();
                entitiesToDelete.Add(field.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Fields.Add(field.As<Field>());
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                entity = Entity.Create(entityType);
                entity.SetField(field, initialFieldValue);
                entity.Save();
                entitiesToDelete.Add(entity.Id);

                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    splitPermissions.Select(s => new EntityRef(s)),
                    TestQueries.Entities().ToReport());

                using (var request = new PlatformHttpRequest(@"data/v1/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonEntityQueryResult
                    {
                        Ids = new List<long> { entity.Id },
                        Entities = new List<JsonEntity>
                        {
                            new JsonEntity
                            {
                                Id = entity.Id,
                                TypeIds = new List<long>{ entityType.Id },
                                Fields = new List<JsonFieldData>
                                {
                                    new JsonFieldData
                                    {
                                        FieldId = field.Id,
                                        Value = newFieldValue,
                                        TypeName = "String"
                                    }
                                },
                                Relationships = new List<JsonRelationshipData>(),
                                DataState = DataState.Update
                            }
                        },
                        EntityRefs = new List<JsonEntityRef>
                        {
                            new JsonEntityRef(new EntityRef(entity.Id)),
                            new JsonEntityRef(new EntityRef(entityType)),
                            new JsonEntityRef(new EntityRef(field))
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(expectedResult),
                        "Web service call failed");

                    if (expectedResult == HttpStatusCode.OK)
                    {
                        modifiedEntity = Entity.Get(entity.Id);
                        Assert.That(modifiedEntity, Is.Not.Null, "Entity not found");
                        Assert.That(modifiedEntity.GetField(field), Is.EqualTo(newFieldValue),
                            "Field value incorrect");
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
        [TestCase("", "", true, HttpStatusCode.Forbidden)]
        [TestCase("core:read", "", true, HttpStatusCode.Forbidden)]
        [TestCase("", "core:read", true, HttpStatusCode.Forbidden)]
        [TestCase("core:modify", "", true, HttpStatusCode.Forbidden)]
        [TestCase("", "core:modify", true, HttpStatusCode.Forbidden)]
        [TestCase("core:read", "core:read", true, HttpStatusCode.Forbidden)]
        [TestCase("core:read,core:modify", "core:read", true, HttpStatusCode.OK)]
        [TestCase("core:read", "core:read,core:modify", true, HttpStatusCode.Forbidden)]
        [TestCase("core:read,core:modify", "core:read,core:modify", true, HttpStatusCode.OK)]
        [TestCase("core:read,core:modify", "core:read", false, HttpStatusCode.OK)]
        [TestCase("core:read", "core:read,core:modify", false, HttpStatusCode.Forbidden)]
        [TestCase("core:read,core:modify", "core:read,core:modify", false, HttpStatusCode.OK)]
        public void Test_ModifyRelationship(string entity1Permissions,
            string entity2Permissions, bool forward, HttpStatusCode expectedResult)
        {
            EntityType entityType1;
            EntityType entityType2;
            Relationship relationship;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            IEntity entity1;
            IEntity entity2;
            long newId;
            IEntity newEntity;
            string[] splitEntity1Permissions;
            string[] splitEntity2Permissions;

            splitEntity1Permissions = entity1Permissions.Split(new[] {','},
                StringSplitOptions.RemoveEmptyEntries);
            splitEntity2Permissions = entity2Permissions.Split(new[] {','},
                StringSplitOptions.RemoveEmptyEntries);

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType1 = Entity.Create<EntityType>();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();
                entitiesToDelete.Add(entityType1.Id);

                entity1 = Entity.Create(entityType1);
                entity1.Save();
                entitiesToDelete.Add(entity1.Id);

                entityType2 = Entity.Create<EntityType>();
                entityType2.Inherits.Add(UserResource.UserResource_Type);
                entityType2.Save();
                entitiesToDelete.Add(entityType2.Id);

                entity2 = Entity.Create(entityType2);
                entity2.Save();
                entitiesToDelete.Add(entity2.Id);

                relationship = Entity.Create<Relationship>();
                relationship.FromType = entityType1;
                relationship.ToType = entityType2;
                relationship.RelType = Entity.Get<RelTypeEnum>("core:relManyToMany");
                relationship.Save();
                entitiesToDelete.Add(relationship.Id);

                new AccessRuleFactory().AddAllowByQuery(
                    userAccount.As<Subject>(),
                    entityType1.As<SecurableEntity>(),
                    splitEntity1Permissions.Select(p => new EntityRef(p)),
                    TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllowByQuery(
                    userAccount.As<Subject>(),
                    entityType2.As<SecurableEntity>(),
                    splitEntity2Permissions.Select(p => new EntityRef(p)),
                    TestQueries.Entities().ToReport());

                using (var request = new PlatformHttpRequest(@"data/v1/entity", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonEntityQueryResult
                    {
                        Ids = new List<long> {entity1.Id},
                        Entities = new List<JsonEntity>
                        {
                            new JsonEntity
                            {
                                Id = entity1.Id,
                                TypeIds = new List<long> { entityType1.Id },
                                Fields = new List<JsonFieldData>(),
                                Relationships = new List<JsonRelationshipData>
                                {
                                    new JsonRelationshipData
                                    {
                                        RelTypeId = new JsonEntityRef(relationship),
                                        Instances = new List<JsonRelationshipInstanceData>
                                        {
                                            new JsonRelationshipInstanceData
                                            {
                                                Entity = entity2.Id,
                                                RelEntity = 0,
                                                DataState = DataState.Create
                                            }
                                        },
                                        IsReverse = !forward
                                    }
                                },
                                DataState = DataState.Create
                            }
                        },
                        EntityRefs = new List<JsonEntityRef>
                        {
                            new JsonEntityRef(new EntityRef(entity1)),
                            new JsonEntityRef(new EntityRef(entity2)),
                            new JsonEntityRef(new EntityRef(entityType1)),
                            new JsonEntityRef(new EntityRef(entityType2)),
                            new JsonEntityRef(new EntityRef(relationship))
                        }
                    });

                    response = request.GetResponse();
                    Assert.That(response, Has.Property("StatusCode").EqualTo(expectedResult),
                        "Web service call failed");

                    if (expectedResult == HttpStatusCode.OK)
                    {
                        newId = request.DeserialiseResponseBody<long>();
                        entitiesToDelete.Add(newId);

                        newEntity = Entity.Get(newId);
                        Assert.That(newEntity, Is.Not.Null, "New entity does not exist");
                        Assert.That(newEntity, Has.Property("TypeIds").Contains(entityType1.Id),
                            "New entity missing correct type");
                        if (forward)
                        {
                            Assert.That(newEntity.GetRelationships(relationship), Has.Count.EqualTo(1),
                                "Relationship count incorrect");
                        }
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
        [TestCase("", HttpStatusCode.Forbidden)]
        [TestCase("core:read", HttpStatusCode.Forbidden)]
        //[TestCase("core:delete", HttpStatusCode.Forbidden)]  // this is an invalid configuration
        [TestCase("core:read,core:modify", HttpStatusCode.Forbidden)]
        [TestCase("core:read,core:delete", HttpStatusCode.OK)]
        public void Test_Delete(string permissions, HttpStatusCode expectedResult)
        {
            EntityType entityType;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            IEntity entity;
            IEntity modifiedEntity;
            string[] splitPermissions;

            splitPermissions = permissions.Split(new[] {','},
                StringSplitOptions.RemoveEmptyEntries);

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                entity = Entity.Create(entityType);
                entity.Save();
                entitiesToDelete.Add(entity.Id);

                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    splitPermissions.Select(s => new EntityRef(s)),
                    TestQueries.Entities().ToReport());

                using (var request = new PlatformHttpRequest(@"data/v1/entity?id=" + entity.Id, PlatformHttpMethod.Delete, userAccount))
                {
                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(expectedResult), "Web service call failed");

                    if (expectedResult != HttpStatusCode.OK) return;

                    EntityCache.Instance.Remove(entity.Id);
                    modifiedEntity = Entity.Get(entity.Id);
                    Assert.That(modifiedEntity, Is.Null, "Entity not deleted");
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
        [TestCase("", "", HttpStatusCode.Forbidden)]
        [TestCase("", "core:create", HttpStatusCode.Forbidden)]
        [TestCase("core:read", "", HttpStatusCode.Forbidden)]
        [TestCase("core:read", "core:create", HttpStatusCode.OK)]
        [TestCase("core:read", "core:create,core:read", HttpStatusCode.OK)]
        [TestCase("core:read,core:modify,core:delete", "core:create,core:read", HttpStatusCode.OK)]
        public void Test_Clone(string entityPermissions, string entityTypePermissions, HttpStatusCode expectedResult)
        {
            EntityType entityType;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            IEntity entity;
            IEntity clonedEntity;
            string[] splitEntityPermissions;
            string[] splitEntityTypePermissions;
            const string initialName = "foo";
            const string newName = "bar";
            long cloneId;

            splitEntityPermissions = entityPermissions.Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);
            splitEntityTypePermissions = entityTypePermissions.Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                entity = Entity.Create(entityType);
                entity.SetField("core:name", initialName);
                entity.Save();
                entitiesToDelete.Add(entity.Id);

                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    splitEntityPermissions.Select(s => new EntityRef(s)),
                    TestQueries.Entities().ToReport());
                new AccessRuleFactory().AddAllow(userAccount.As<Subject>(),                    
                    splitEntityTypePermissions.Select(s => new EntityRef(s)),
                    entityType.As<SecurableEntity>());

                using (var request = new PlatformHttpRequest(@"data/v1/entity/clone", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new EntityCloneRequest
                    {
                        Id = new JsonEntityRef(new EntityRef(entity)),
                        Name = newName
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(expectedResult),
                        "Web service call failed");
                    if (expectedResult == HttpStatusCode.OK)
                    {
                        cloneId = request.DeserialiseResponseBody<long>();
                        entitiesToDelete.Add(cloneId);

                        clonedEntity = Entity.Get(cloneId);
                        Assert.That(clonedEntity, Is.Not.Null, "Entity not found");
                        Assert.That(clonedEntity.GetField("core:name"), Is.EqualTo(newName), "Name incorrect");
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
        [TestCase("", HttpStatusCode.Forbidden)]
        [TestCase("core:modify", HttpStatusCode.Forbidden)]
        [TestCase("core:read", HttpStatusCode.OK)]
        [TestCase("core:read,core:modify", HttpStatusCode.OK)]
        public void Test_Get(string entityPermissions, HttpStatusCode expectedResult)
        {
            EntityType entityType;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            IEntity entity;
            string[] splitEntityPermissions;
            const string initialName = "foo";
            JsonEntityQueryResult jsonEntityQueryResult;

            splitEntityPermissions = entityPermissions.Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                entity = Entity.Create(entityType);
                entity.SetField("core:name", initialName);
                entity.Save();
                entitiesToDelete.Add(entity.Id);

                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    splitEntityPermissions.Select(s => new EntityRef(s)),
                    TestQueries.Entities().ToReport());

                using (var request = new PlatformHttpRequest(string.Format(@"data/v1/entity/{0}?request=name", entity.Id), 
                        PlatformHttpMethod.Get, userAccount))
                {
                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(expectedResult),
                        "Web service call failed");
                    jsonEntityQueryResult = request.DeserialiseResponseBody<JsonEntityQueryResult>();
                    if (expectedResult == HttpStatusCode.OK)
                    {
                        Assert.That(jsonEntityQueryResult.Ids, Is.EquivalentTo(new long[] {entity.Id}));
                    }
                    else
                    {
                        Assert.That(jsonEntityQueryResult.Ids, Is.Null);
                        Assert.That(jsonEntityQueryResult.Entities, Is.Empty);
                        Assert.That(jsonEntityQueryResult.EntityRefs, Is.Empty);
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
        [TestCase("", false)]
        [TestCase("core:modify", false)]
        [TestCase("core:read", true)]
        [TestCase("core:read,core:modify", true)]
        public void Test_Post(string entityPermissions, bool expectEntity)
        {
            EntityType entityType;
            List<long> entitiesToDelete;
            UserAccount userAccount;
            HttpWebResponse response;
            IEntity entity;
            string[] splitEntityPermissions;
            ReportDataDefinition result;

            splitEntityPermissions = entityPermissions.Split(new[] {','},
                StringSplitOptions.RemoveEmptyEntries);

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                entity = Entity.Create(entityType);
                entity.SetField("core:name", "A");
                entity.Save();
                entitiesToDelete.Add(entity.Id);

                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    splitEntityPermissions.Select(s => new EntityRef(s)),
                    TestQueries.Entities().ToReport());

                using (var request = new PlatformHttpRequest(@"data/v1/entity/query", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new JsonStructuredQuery
                    {
                        Root = new JsonEntityInQuery
                        {
                            Id = entityType.Id.ToString(CultureInfo.InvariantCulture)
                        }
                    });

                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");
                    result = request.DeserialiseResponseBody<ReportDataDefinition>();
                    if (expectEntity)
                    {
                        Assert.That(result.ReportDataRows, Has.Count.EqualTo(1));
                        Assert.That(result.ReportDataRows, Has.Exactly(1).Property("Id").EqualTo(entity.Id));
                    }
                    else
                    {
                        Assert.That(result.ReportDataRows, Is.Empty);
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
        public void Test_PasswordEmptyInResponse()
        {
            long administratorId;
            long passwordFieldId;
            long nameFieldId;
            List<long> entitiesToDelete;
            HttpWebResponse response;
            JsonEntityQueryResult jsonEntityQueryResult;
            JsonEntity jsonEntity;

            entitiesToDelete = new List<long>();
            try
            {
                administratorId = Entity.GetId("core:administratorUserAccount");
                Assert.That(administratorId, Is.Positive, "Missing administrator account");

                passwordFieldId = Entity.GetId("core:password");
                Assert.That(passwordFieldId, Is.Positive, "Missing password field");

                nameFieldId = Entity.GetId("core:name");
                Assert.That(nameFieldId, Is.Positive, "Missing name field");

                using (PlatformHttpRequest request = new PlatformHttpRequest(string.Format(@"data/v1/entity/{0}?request=name,password", administratorId)))
                {
                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");
                    jsonEntityQueryResult = request.DeserialiseResponseBody<JsonEntityQueryResult>();

                    Assert.That(jsonEntityQueryResult.Entities, Has.Count.EqualTo(1), "Incorrect Entities count");
                    jsonEntity = jsonEntityQueryResult.Entities.First();
                    Assert.That(jsonEntity.Fields, Has.Exactly(1).Property("FieldId").EqualTo(passwordFieldId)
                                                                 .And.Property("Value").Matches("[*]+"));
                    Assert.That(jsonEntity.Fields, Has.Exactly(1).Property("FieldId").EqualTo(nameFieldId)
                                                                 .And.Property("Value").EqualTo("Administrator"));
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
    }
}
