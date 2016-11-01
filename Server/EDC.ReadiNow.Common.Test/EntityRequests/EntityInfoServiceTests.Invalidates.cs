// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests;
using NUnit.Framework;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.EntityRequests
{
    // Various tests to ensure that queries still function correctly when changes invalidate any cached results
    [TestFixture("EntityInfoService")]
    [TestFixture("BulkRequestRunner")]
	[RunWithTransaction]
    public class EntityInfoServiceTests_Invalidates
    {
        #region Helpers

        private readonly string _runner;

        private readonly List<IEntity> _toDelete = new List<IEntity>();

        public EntityInfoServiceTests_Invalidates(string runner)
        {
            _runner = runner;
        }

        private IEntityInfoRead GetService()
        {
            switch (_runner)
            {
                case "EntityInfoService":
                    return new EntityInfoService();
                case "BulkRequestRunner":
                    return new BulkRequestRunnerMockService();
            }
            throw new InvalidOperationException("No test runner specified.");
        }

        private IEntity Create(string nsAlias, string name = "EntityInfoServiceTests_Invalidates")
        {
            var entity = CreateNoSave(nsAlias, name);
            entity.Save();
            return entity;
        }

        private IEntity CreateNoSave(string nsAlias, string name = "EntityInfoServiceTests_Invalidates")
        {
            var entity = Entity.Create(nsAlias);
            entity.SetField("core:name", name);
            _toDelete.Add(entity);
            return entity;
        }

        [TearDown]
        public void TestFinalize()
        {
            using (new AdministratorContext())
            {
                Entity.Delete(_toDelete.Select(e => e.Id));
            }
        }

        #endregion

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterDelete_GetID()
        {
            var entity = Create("test:allFields");
            
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();
            
            // Check create
            EntityData result = svc.GetEntityData(entity.Id, request);
            Assert.AreEqual(entity.Id, result.Id.Id, "Expect created");

            // Delete
            Entity.Delete(entity.Id);

            // Check deleted
            result = svc.GetEntityData(entity.Id, request);
            Assert.IsNull(result);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterDeleteRelated_Forward()
        {
            var from = Create("test:allFields");
            var to = Create("test:herb");
            from.GetRelationships("test:herbs").Add(to);
            from.Save();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id, test:herbs.id");
            var svc = GetService();

            // Check create
            EntityData result = svc.GetEntityData(from.Id, request);
            Assert.AreEqual(from.Id, result.Id.Id, "Expect from");
            Assert.AreEqual(to.Id, result.Relationships[0].Instances[0].Entity.Id.Id, "Expect to");

            // Delete
            Entity.Delete(to.Id);

            // Check deleted
            result = svc.GetEntityData(from.Id, request);
            Assert.AreEqual(from.Id, result.Id.Id, "Expect from again");
            Assert.AreEqual(0, result.Relationships[0].Instances.Count, "Expect no to");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterDeleteRelated_Reverse()
        {
            var from = Create("test:allFields");
            var to = Create("test:herb");
            from.GetRelationships("test:herbs").Add(to);
            from.Save();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id, test:herbAllFields.id");
            var svc = GetService();

            // Check create
            EntityData result = svc.GetEntityData(to.Id, request);
            Assert.AreEqual(to.Id, result.Id.Id, "Expect to");
            Assert.AreEqual(from.Id, result.Relationships[0].Instances[0].Entity.Id.Id, "Expect from");

            // Delete
            Entity.Delete(from.Id);

            // Check deleted
            result = svc.GetEntityData(to.Id, request);
            Assert.AreEqual(to.Id, result.Id.Id, "Expect to again");
            Assert.AreEqual(0, result.Relationships[0].Instances.Count, "Expect no from");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterAddRelated_Forward()
        {
            var from = Create("test:allFields");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id, test:herbs.id");
            var svc = GetService();

            // Check absent
            EntityData result = svc.GetEntityData(from.Id, request);
            Assert.AreEqual(from.Id, result.Id.Id, "Expect from");
            Assert.AreEqual(0, result.Relationships[0].Instances.Count, "Expect no to");

            // Add relationship
            var to = Create("test:herb");
            from.GetRelationships("test:herbs").Add(to);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                from.Save();

                ctx.CommitTransaction();
            }

            // Check add relationship
            result = svc.GetEntityData(from.Id, request);
            Assert.AreEqual(from.Id, result.Id.Id, "Expect from again");
            Assert.AreEqual(to.Id, result.Relationships[0].Instances[0].Entity.Id.Id, "Expect to");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterAddRelated_Reverse()
        {
            var to = Create("test:herb");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id, test:herbAllFields.id");
            var svc = GetService();

            // Check absent
            EntityData result = svc.GetEntityData(to.Id, request);
            Assert.AreEqual(to.Id, result.Id.Id, "Expect to");
            Assert.AreEqual(0, result.Relationships[0].Instances.Count, "Expect no from");

            // Add relationship
            var from = Create("test:allFields");
            from.GetRelationships("test:herbs").Add(to);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                from.Save();

                ctx.CommitTransaction();
            }

            // Check add relationship
            result = svc.GetEntityData(to.Id, request);
            Assert.AreEqual(to.Id, result.Id.Id, "Expect to again");
            Assert.AreEqual(from.Id, result.Relationships[0].Instances[0].Entity.Id.Id, "Expect from");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterAddAtomicRelated_Forward()
        {
            var from = Create("test:allFields");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id, test:herbs.id");
            var svc = GetService();

            // Check absent
            EntityData result = svc.GetEntityData(from.Id, request);
            Assert.AreEqual(from.Id, result.Id.Id, "Expect from");
            Assert.AreEqual(0, result.Relationships[0].Instances.Count, "Expect no to");

            // Add relationship
            var to = CreateNoSave("test:herb");
            from.GetRelationships("test:herbs").Add(to);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                from.Save();

                ctx.CommitTransaction();
            }            

            // Check add relationship
            result = svc.GetEntityData(from.Id, request);
            Assert.AreEqual(from.Id, result.Id.Id, "Expect from again");
            Assert.AreEqual(to.Id, result.Relationships[0].Instances[0].Entity.Id.Id, "Expect to");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterAddAtomicRelated_Reverse()
        {
            var to = Create("test:herb");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id, test:herbAllFields.id");
            var svc = GetService();

            // Check absent
            EntityData result = svc.GetEntityData(to.Id, request);
            Assert.AreEqual(to.Id, result.Id.Id, "Expect to");
            Assert.AreEqual(0, result.Relationships[0].Instances.Count, "Expect no from");

            // Add relationship
            var from = CreateNoSave("test:allFields");
            from.GetRelationships("test:herbs").Add(to);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                from.Save();

                ctx.CommitTransaction();
            }

            // Check add relationship
            result = svc.GetEntityData(to.Id, request);
            Assert.AreEqual(to.Id, result.Id.Id, "Expect to again");
            Assert.AreEqual(from.Id, result.Relationships[0].Instances[0].Entity.Id.Id, "Expect from");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterFieldChange_GetField()
        {
            var entity = Create("test:allFields", "AAA");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("name");
            var svc = GetService();

            // Check create
            EntityData result = svc.GetEntityData(entity.Id, request);
            Assert.AreEqual(entity.Id, result.Id.Id, "Expect created");
            Assert.AreEqual("AAA", result.Fields[0].Value.ValueString);

            // Change field
            entity.SetField("core:name", "BBB");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entity.Save();

                ctx.CommitTransaction();
            }

            // Check updated
            result = svc.GetEntityData(entity.Id, request);
            Assert.AreEqual("BBB", result.Fields[0].Value.ValueString);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterFieldChangeToNull_GetField()
        {
            var entity = Create("test:allFields", "AAA");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("name");
            var svc = GetService();

            // Check create
            EntityData result = svc.GetEntityData(entity.Id, request);
            Assert.AreEqual(entity.Id, result.Id.Id, "Expect created");
            Assert.AreEqual("AAA", result.Fields[0].Value.ValueString);

            // Change field
            entity.SetField("core:name", null);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entity.Save();

                ctx.CommitTransaction();
            }

            // Check updated
            result = svc.GetEntityData(entity.Id, request);
            Assert.AreEqual(null, result.Fields[0].Value.ValueString);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AfterFieldChangeFromNull_GetField()
        {
            var entity = Create("test:allFields", "AAA");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("test:afString");
            var svc = GetService();

            // Check create
            EntityData result = svc.GetEntityData(entity.Id, request);
            Assert.AreEqual(entity.Id, result.Id.Id, "Expect created");
            Assert.AreEqual(null, result.Fields[0].Value.ValueString);

            // Change field
            entity.SetField("test:afString", "ZZZ");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entity.Save();

                ctx.CommitTransaction();
            }            

            // Check updated
            result = svc.GetEntityData(entity.Id, request);
            Assert.AreEqual("ZZZ", result.Fields[0].Value.ValueString);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NewInstance_GetInstancesOfTypeManual()
        {
            if (_runner == "EntityInfoService")
                Assert.Ignore();

            var typeId = Entity.GetId("test:herb");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("instancesOfType.id");
            var svc = GetService();

            // Check before create
            EntityData result = svc.GetEntityData(typeId, request);
            int count = result.Relationships[0].Instances.Count;

            // Create the instance
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                Create("test:herb", "AAA");

                ctx.CommitTransaction();
            }            

            // Check after create
            result = svc.GetEntityData(typeId, request);
            int count2 = result.Relationships[0].Instances.Count;
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NewInstance_GetEntitiesByType()
        {
            if (_runner == "EntityInfoService")
                Assert.Ignore();

            var typeId = Entity.GetId("test:herb");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();

            // Check before create
            var result = svc.GetEntitiesByType(typeId, false, request);
            int count = result.Count();

            // Create the instance
            Create("test:herb", "AAA");

            // Check after create
            result = svc.GetEntitiesByType(typeId, false, request);
            int count2 = result.Count();
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
		[ClearCaches( ClearCachesAttribute.Caches.EntityRelationshipCache | ClearCachesAttribute.Caches.BulkResultCache )]
        public void Test_NewHerbById_GetEntitiesUsingInstancesOfType()
        {
            long typeId = Entity.GetId("test:herb");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("instancesOfType.{alias,name,description}");
            var svc = GetService();

            // Check before create
            var result = svc.GetEntityData(typeId, request);
            int count = result.Relationships[0].Instances.Count;
            Assert.IsTrue(count > 1);

            // Create the instance
            var entity = Entity.Create(new long[] { typeId });
            entity.SetField("core:name", "AAA");
            _toDelete.Add(entity);
            
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entity.Save();

                ctx.CommitTransaction();
            }

            // Check after create
            var result2 = svc.GetEntityData(typeId, request);
            int count2 = result2.Relationships[0].Instances.Count;
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NewHerbByAlias_GetEntitiesUsingInstancesOfType()
        {
            EntityRef typeId = new EntityRef("test:herb");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("instancesOfType.{alias,name,description}");
            var svc = GetService();

            // Check before create
            var result = svc.GetEntityData(typeId, request);
            int count = result.Relationships[0].Instances.Count;
            Assert.IsTrue(count > 1);

            // Create the instance
            var entity = Entity.Create(typeId);
            entity.SetField("core:name", "AAA");
            _toDelete.Add(entity);
            
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entity.Save();

                ctx.CommitTransaction();
            }

            // Check after create
            var result2 = svc.GetEntityData(typeId, request);
            int count2 = result2.Relationships[0].Instances.Count;
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NewById_GetEntitiesUsingInstancesOfType()
        {
            long typeId = Entity.GetId("test:herb");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("instancesOfType.{alias,name,description}");
            var svc = GetService();

            // Check before create
            var result = svc.GetEntityData(typeId, request);
            int count = result.Relationships[0].Instances.Count;
            Assert.IsTrue(count > 1);

            // Create the instance
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                Create("test:herb", "AAA");

                ctx.CommitTransaction();
            }            

            // Check after create
            var result2 = svc.GetEntityData(typeId, request);
            int count2 = result2.Relationships[0].Instances.Count;
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NewDefinition_GetEntitiesUsingInstancesOfType()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("instancesOfType.{alias,name,description}");
            var svc = GetService();

            // Check before create
            var result = svc.GetEntityData(new EntityRef("core:definition"), request);
            int count = result.Relationships[0].Instances.Count;
            Assert.IsTrue(count > 1);

            // Create the instance            
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                Create("core:definition", "Our test defn");

                ctx.CommitTransaction();
            }

            // Check after create
            var result2 = svc.GetEntityData(new EntityRef("core:definition"), request);
            int count2 = result2.Relationships[0].Instances.Count;
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
		[ClearCaches( ClearCachesAttribute.Caches.EntityRelationshipCache | ClearCachesAttribute.Caches.BulkResultCache )]
        public void Test_NewDefinitionOnRelated_GetEntitiesUsingInstancesOfType()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("instancesOfType.{alias,name,description}");
            var svc = GetService();

            // Check before create
            var result = svc.GetEntityData(new EntityRef("core:definition"), request);
            int count = result.Relationships[0].Instances.Count;
            Assert.IsTrue(count > 1);

            // Define a new form & definition
            var defn = new EntityData
            {
                Fields = new List<FieldData>(),
                TypeIds = new EntityRef("core", "definition").ToEnumerable().ToList(),
                DataState = DataState.Create
            };
            defn.Fields.Add(new FieldData
            {
                FieldId = new EntityRef("name"),
                Value = new TypedValue("TestDefnT " + Guid.NewGuid().ToString())
            });
            var form = new EntityData
            {
                Fields = new List<FieldData>(),
                TypeIds = new EntityRef("console", "customEditForm").ToEnumerable().ToList(),
                DataState = DataState.Create
            };
            form.Fields.Add(new FieldData
            {
                FieldId = new EntityRef("name"),
                Value = new TypedValue("TestFormT " + Guid.NewGuid().ToString())
            });
            form.Relationships = new List<RelationshipData> {
                new RelationshipData
                {
                    RelationshipTypeId = new EntityRef("console", "typeToEditWithForm"),
                    RemoveExisting = true
                }};
            form.Relationships[0].Instances = new List<RelationshipInstanceData> {
                new RelationshipInstanceData
                {
                    Entity = defn,
                    DataState = DataState.Create
                } };
            var svcWrite = new EntityInfoService();

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                EntityRef id = svcWrite.CreateEntity(form);

                ctx.CommitTransaction();
            }            

            // Check after create
            var result2 = svc.GetEntityData(new EntityRef("core:definition"), request);
            int count2 = result2.Relationships[0].Instances.Count;
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NewDerivedInstance_GetEntitiesByType()
        {
            var typeId = Entity.GetId("test:person");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();

            // Check before create
            var result = svc.GetEntitiesByType(typeId, true, request);
            int count = result.Count();

            // Create the instance
            Create("test:employee", "AAA");

            // Check after create
            result = svc.GetEntitiesByType(typeId, true, request);
            int count2 = result.Count();
            Assert.AreEqual(count + 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_DeleteInstance_GetInstancesOfTypeManual()
        {
            var typeId = Entity.GetId("test:herb");

            // Create the instance
            var entity = Create("test:herb", "AAA");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("instancesOfType.id");
            var svc = GetService();

            // Check before delete
            EntityData result = svc.GetEntityData(typeId, request);
            int count = result.Relationships[0].Instances.Count;

            // Delete
            entity.Delete();

            // Check after delete
            result = svc.GetEntityData(typeId, request);
            int count2 = result.Relationships[0].Instances.Count;
            Assert.AreEqual(count - 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_DeleteInstance_GetEntitiesByType()
        {
            var typeId = Entity.GetId("test:herb");

            // Create the instance
            var entity = Create("test:herb", "AAA");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();

            // Check before delete
            var result = svc.GetEntitiesByType(typeId, false, request);
            int count = result.Count();

            // Delete
            entity.Delete();

            // Check after delete
            result = svc.GetEntitiesByType(typeId, false, request);
            int count2 = result.Count();
            Assert.AreEqual(count - 1, count2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_DeleteDerivedInstance_GetEntitiesByType()
        {
            var typeId = Entity.GetId("test:person");

            // Create the instance
            var entity = Create("test:employee", "AAA");

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();

            // Check before delete
            var result = svc.GetEntitiesByType(typeId, true, request);
            int count = result.Count();

            // Delete
            entity.Delete();

            // Check after delete
            result = svc.GetEntitiesByType(typeId, true, request);
            int count2 = result.Count();
            Assert.AreEqual(count - 1, count2);
        }
    }

}