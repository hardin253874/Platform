// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database.Types;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests;
using NUnit.Framework;
using EDC.Common;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.EntityRequests.BulkRequests;

namespace EDC.ReadiNow.Test.EntityRequests
{
    [TestFixture("EntityInfoService")]
    [TestFixture("BulkRequestRunner")]
	[RunWithTransaction]
    public class EntityInfoServiceTests_Read
    {
        #region Helpers

        private readonly string _runner;

        public EntityInfoServiceTests_Read(string runner)
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

        private bool FoundResource(EntityData entity)
        {
            if ("Resource" == entity.Fields[0].Value.Value.ToString())
            {
                return true;
            }

            RelationshipData rel = entity.GetRelationship("core:inherits");

            if (rel != null)
            {
                return rel.Instances.Any(inst => FoundResource(inst.Entity));
            }

            return false;
        }

        private TypedValue GetSingleField(string resourceAlias, string fieldAlias)
        {
            var rq = new EntityMemberRequest();
            rq.Fields.Add(new EntityRef(fieldAlias));

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef(resourceAlias), rq);

            return result.Fields[0].Value;
        }

        #endregion

        #region Test Field Types
        [Test]
        [RunAsDefaultTenant]
        public void GetStringField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afString");
            Assert.AreEqual("data 01", tv.Value);
            Assert.AreEqual(typeof(StringType), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetMultilineField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afMultiline");
            Assert.AreEqual("multi \ntext \nfor \nTest 01", tv.Value);
            Assert.AreEqual(typeof(StringType), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetNumberField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afNumber");
            Assert.AreEqual(100, (int)tv.Value);
            Assert.AreEqual(typeof(Int32Type), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetDecimalField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afDecimal");
            Assert.AreEqual(100.111M, (decimal)tv.Value);
            Assert.AreEqual(typeof(DecimalType), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetCurrencyField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afCurrency");
            Assert.AreEqual(100.100M, (decimal)tv.Value);
            Assert.AreEqual(typeof(CurrencyType), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetAutonumberField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afAutonumber");
            Assert.AreEqual(50, (int)tv.Value);
            Assert.AreEqual(typeof(Int32Type), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetBooleanField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afBoolean");
            Assert.AreEqual(false, (bool)tv.Value);
            Assert.AreEqual(typeof(BoolType), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetDateField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afDate");
            Assert.AreEqual(new DateTime(2013, 6, 1), (DateTime)tv.Value);
            Assert.AreEqual(typeof(DateType), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetDateTimeField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afDateTime");
            Assert.AreEqual(new DateTime(2013, 6, 1, 3, 0, 0), (DateTime)tv.Value);
            Assert.AreEqual(typeof(DateTimeType), tv.Type.GetType());
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetTimeField()
        {
            TypedValue tv = GetSingleField("test:af01", "test:afTime");
            Assert.AreEqual(new DateTime(1753,1,1,13,0,0), (DateTime)tv.Value);
            Assert.AreEqual(typeof(TimeType), tv.Type.GetType());
        }
        #endregion

        [Test]
        [RunAsDefaultTenant]
        public void TestId()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), request);
            Assert.IsTrue(result.Id.Id > 0);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidId()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("name");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef(9999999L), request);
            Assert.IsNull(result);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestSameEntityTwice()
        {
            if (_runner == "EntityInfoService")
                Assert.Ignore();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();
            var arr = new [] { new EntityRef("test", "employee"), new EntityRef("test", "employee")};
            var result = svc.GetEntitiesData(arr, request);
            var resArr = result.ToArray();
            Assert.AreEqual(2, resArr.Length);
            Assert.IsTrue(resArr[0].Id.Id > 0);
            Assert.IsTrue(resArr[1].Id.Id > 0);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidAlias()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("name");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("invalidblah", "invalidblah"), request);
            Assert.IsNull(result);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidList()
        {
            if (_runner == "EntityInfoService")
                Assert.Ignore();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("name");
            var svc = GetService();
            var result = svc.GetEntitiesData((new EntityRef("invalidblah", "invalidblah")).ToEnumerable(), request);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidZeroList()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("name");
            var svc = GetService();
            var result = svc.GetEntitiesData((new EntityRef(0)).ToEnumerable(), request);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestAlias()
        {
            TypedValue tv = GetSingleField("core:type", "alias");
            Assert.AreEqual("core:type", tv.Value);
            Assert.AreEqual(typeof(StringType), tv.Type.GetType());
        }

        [TestCase("name", "isOfType", "False")] //forward,rel
        [TestCase("name", "fieldIsOnType", "True")] //forward,lookup
        [TestCase("name", "fieldOverriddenBy", "False")] //reverse,rel
        [TestCase("test:aaCoke", "test:drinkAllFields", "True")] //reverse,lookup (oneToOne)
        [RunAsDefaultTenant]
        public void TestRelationshipMetadata(string entity, string relationshipAlias, string isLookup)
        {
            var svc = GetService();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest(relationshipAlias + ".id");
            EntityData data = svc.GetEntityData(new EntityRef(entity), request);

            bool bIsLookup = isLookup == "True";
            Assert.AreEqual(bIsLookup, data.Relationships[0].IsLookup);
        }

        /// <summary>
        ///     Ensure that entityDatas when created from a set of entities does not duplicated entities that are refered to twice
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void EntityDataNotDuplicated()
        {
            var svc = GetService();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("isOfType.name, isOfType.isOfType.name");
            EntityData data = svc.GetEntityData(new EntityRef("core", "resource"), request);

            var foundNodes = new List<EntityData>();
            EntityData.WalkRelationshipTree(data, foundNodes.Add, null, null);

            IEnumerable<long> foundIds = foundNodes.Select(n => n.Id.Id);

            IList<long> enumerable = foundIds as IList<long> ?? foundIds.ToList();

            Assert.AreEqual(enumerable.Distinct().Count(), enumerable.Count(), "No Ids should be duplicated");
            Assert.AreEqual(2, enumerable.Count(), "Expected two entities"); // 'resource' and 'type'
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntitiesByTypeDerivedDirect()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("alias");

            var svc = GetService();
            IEnumerable<EntityData> result = svc.GetEntitiesByType(new EntityRef("core", "type"), true, rq);

            Assert.IsTrue(result.Any(x => x.Fields[0].Value.Value as string == "core:stringField"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntitiesByTypeDerivedRecursive()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("alias");

            var svc = GetService();
            IEnumerable<EntityData> result = svc.GetEntitiesByType(new EntityRef("core", "type"), true, rq);

            Assert.IsTrue(result.Any(x => x.Fields[0].Value.Value as string == "console:verticalStackContainerControl"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntityDataRecursive()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("derivedTypes*.alias, alias");

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("core", "type"), rq);

            IEnumerable<EntityData> all = Delegates.WalkGraph(result, ed => ed.Relationships[0].Instances.Select(i => i.Entity)).ToList();

            Assert.IsTrue(all.Any(x => x.Fields[0].Value.Value as string == "core:type"), "core:type");
            Assert.IsTrue(all.Any(x => x.Fields[0].Value.Value as string == "core:fieldType"), "core:fieldType");
            Assert.IsTrue(all.Any(x => x.Fields[0].Value.Value as string == "console:renderControlType"), "console:renderControlType");
            Assert.IsTrue(all.Any(x => x.Fields[0].Value.Value as string == "console:structureRenderControlType"), "console:structureRenderControlType");
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntityDataRecursiveWithRel()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("{instancesOfType, derivedTypes*.instancesOfType}.{ alias }");

            var svc = GetService();
            EntityData typeData = svc.GetEntityData(new EntityRef("core", "type"), rq);

            var types = Delegates.WalkGraph(typeData, type => type.Relationships.Count == 2
                    && type.Relationships[0].RelationshipTypeId.Alias == "instancesOfType"
                    && type.Relationships[1].RelationshipTypeId.Alias == "derivedTypes" ? type.Relationships[1].Entities : null);

            var instances = from type in types
                            from instance in type.GetRelationship("instancesOfType").Entities
                            select instance;

            Assert.IsTrue(instances.Any(x => x.Fields[0].Value.Value as string == "console:verticalStackContainerControl"), "console:verticalStackContainerControl");
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntitiesByTypeNonDerived()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("alias");

            var svc = GetService();
            IEnumerable<EntityData> result = svc.GetEntitiesByType(new EntityRef("core", "fieldType"), false, rq);

            Assert.IsTrue(result.Any());
        }

        [TestCase(false)]
        [TestCase(true)]
        [RunAsDefaultTenant]
        public void GetEntitiesByType_EnsureNoBaggage(bool includeDerived)
        {
            // The BulkEntityInfoService implementation  of GetEntitiesByType piggy-backs GetEntities
            // but it currently results in additional relationships being returned if the type (or sub type) being requested also appears in the result set.
            // For correctness this should not happen.

            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("alias");

            var svc = GetService();
            IEnumerable<EntityData> result = svc.GetEntitiesByType(new EntityRef("core", "type"), includeDerived, rq);

            EntityData type = result.Single(x => x.Fields[0].Value.Value as string == "core:type");
            Assert.IsNotNull(type);
            Assert.IsEmpty(type.Relationships, "No relationships were requested");
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntitiesFilteredDerived()
        {
            if (_runner == "EntityInfoService")
                Assert.Ignore();

            var request = new EntityRequest("core:resourceViewer", "name", QueryType.FilterInstances, "Test");
            request.Filter = "[Name] = 'Mail Boxes'";

            IEnumerable<EntityData> results = BulkRequestRunner.GetEntities(request);

            Assert.That(results.Count(), Is.EqualTo(1));

            EntityData resource = results.Single();
            Assert.That(resource.Fields, Has.Count.EqualTo(1));
            Assert.That(resource.Fields[0].Value.ValueString, Is.EqualTo("Mail Boxes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntitiesFilteredNonDerived()
        {
            if (_runner == "EntityInfoService")
                Assert.Ignore();

            var request = new EntityRequest("core:report", "name", QueryType.FilterExactInstances, "Test");
            request.Filter = "[Name] = 'Mail Boxes'";

            IEnumerable<EntityData> results = BulkRequestRunner.GetEntities(request);

            Assert.That(results.Count(), Is.EqualTo(1));

            EntityData resource = results.Single();
            Assert.That(resource.Fields, Has.Count.EqualTo(1));
            Assert.That(resource.Fields[0].Value.ValueString, Is.EqualTo("Mail Boxes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetTwoStrings()
        {
            var rq = new EntityMemberRequest();
            rq.Fields.Add(new EntityRef("name"));
            rq.Fields.Add(new EntityRef("description"));

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("core", "chart"), rq);

            Assert.AreEqual("Chart", result.Fields[0].Value.Value);
        }


        /// <summary>
        ///     Ensure that entityDatas when created from a set of entities does not duplicated entities that are refered to twice
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestAllFieldsOnRelated()
        {
            if (_runner == "BulkRequestRunner")
                Assert.Ignore("AllFields=True not supported by BulkRequestRunner");

            var svc = GetService();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("fields.*");
            Assert.IsTrue(request.Relationships[0].RequestedMembers.AllFields, "child.AllFields");

            EntityData data = svc.GetEntityData(new EntityRef("core", "type"), request);
            Assert.IsTrue(0 < data.Relationships[0].Instances[0].Entity.Fields.Count, "Returned parent fields");
        }

        /// <summary>
        ///     Ensure that entityDatas when created from a set of entities does not duplicated entities that are refered to twice
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestAllFieldsOnRoot()
        {
            if (_runner == "BulkRequestRunner")
                Assert.Ignore("AllFields=True not supported by BulkRequestRunner");

            var svc = GetService();

            EntityMemberRequest request = EntityRequestHelper.BuildRequest("*");
            Assert.IsTrue(request.AllFields, "parent.AllFields");

            EntityData data = svc.GetEntityData(new EntityRef("core", "field"), request);
            Assert.IsTrue(0 < data.Fields.Count, "Returned parent fields");
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestFields()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("fields.name");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), request);
            Assert.IsTrue(result.Relationships[0].Instances.Count > 0);
        }


        /// <summary>
        ///     Test the reverse direction on a request works.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestForwardAndReverseRelRequestSetCorrectly()
        {
            var svc = GetService();

            var requestForward =
                new EntityMemberRequest
                {
                    Relationships =
                        new List<RelationshipRequest>
								{
									new RelationshipRequest
										{
											RelationshipTypeId = new EntityRef( "core", "inherits" ),
											RequestedMembers = new EntityMemberRequest( ),
											IsReverse = false
										}
								}
                };

            var requestReverse =
                new EntityMemberRequest
                {
                    Relationships =
                        new List<RelationshipRequest>
								{
									new RelationshipRequest
										{
											RelationshipTypeId = new EntityRef( "core", "inherits" ),
											RequestedMembers = new EntityMemberRequest( ),
											IsReverse = true
										}
								}
                };

            EntityData dataForward = svc.GetEntityData("test:person", requestForward);
            EntityData dataReverse = svc.GetEntityData("test:person", requestReverse);

            Assert.IsTrue(dataForward.Relationships.Count == 1, "Forward Request succeeded");
            Assert.IsTrue(dataReverse.Relationships.Count == 1, "Reverse Request succeeded");

            Assert.AreEqual(false, dataForward.Relationships[0].IsReverse, "The forward relationship is not marked as reverse");
            Assert.AreEqual(true, dataReverse.Relationships[0].IsReverse, "The reverse relationship is marked as reverse");
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRecursiveRelationship()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("name, inherits.name, inherits*");

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), rq);

            // look for employee type
            Assert.IsTrue(FoundResource(result));
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRecursiveRelationship2()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("name, inherits*.{ name, relationships.name }");

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), rq);

            // look for employee type
            Assert.IsTrue(FoundResource(result));

            EntityData p1 = result.GetRelationship("core:inherits").Instances[0].Entity; // person
#pragma warning disable 168
            int k1 = p1.GetRelationship("core:relationships").Instances.Count();
            EntityData p2 = p1.GetRelationship("core:inherits").Instances[0].Entity; // actor
            int k2 = p2.GetRelationship("core:relationships").Instances.Count();
            EntityData p3 = p2.GetRelationship("core:inherits").Instances[0].Entity; // editable resource
            int k3 = p3.GetRelationship("core:relationships").Instances.Count();
            EntityData p4 = p3.GetRelationship("core:inherits").Instances[0].Entity; // resource
            int k4 = p4.GetRelationship("core:relationships").Instances.Count();
#pragma warning restore 168
            Assert.IsTrue(k4 > 0);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRecursiveRelationship3()
        {
            EntityMemberRequest rq = EntityRequestHelper.BuildRequest(@"
					relationships.fromType.inherits*.id,
					inherits*.id
					");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), rq);

            // Note: this test causes some instances to be revisited for the same relationship,
            // and can be used to ensure that multiple entries do not appear in the RelationshipData array.

            EntityData p1 = result.GetRelationship("core:inherits").Instances[0].Entity; // person
#pragma warning disable 168
            int k1 = p1.GetRelationship("core:relationships").Instances.Count();
            EntityData p2 = p1.GetRelationship("core:inherits").Instances[0].Entity; // actor
            int k2 = p2.GetRelationship("core:relationships").Instances.Count();
            EntityData p3 = p2.GetRelationship("core:inherits").Instances[0].Entity; // editable resource
            int k3 = p3.GetRelationship("core:relationships").Instances.Count();
            EntityData p4 = p3.GetRelationship("core:inherits").Instances[0].Entity; // resource
            int k4 = p4.GetRelationship("core:relationships").Instances.Count();
#pragma warning restore 168
            Assert.IsTrue(k4 > 0);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRecursiveRelationship4()
        {
            EntityMemberRequest rq = EntityRequestHelper.BuildRequest(@"inherits*.relationships.fromType.inherits*.name");

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), rq);

            // look for employee type
            //Assert.IsTrue(FoundResource(result));

            EntityData p1 = result.GetRelationship("core:inherits").Instances[0].Entity; // person
#pragma warning disable 168
            int k1 = p1.GetRelationship("core:relationships").Instances.Count();
            EntityData p2 = p1.GetRelationship("core:inherits").Instances[0].Entity; // actor
            int k2 = p2.GetRelationship("core:relationships").Instances.Count();
            EntityData p3 = p2.GetRelationship("core:inherits").Instances[0].Entity; // editable resource
            int k3 = p3.GetRelationship("core:relationships").Instances.Count();
            EntityData p4 = p3.GetRelationship("core:inherits").Instances[0].Entity; // resource
            int k4 = p4.GetRelationship("core:relationships").Instances.Count();
#pragma warning restore 168
            Assert.IsTrue(k4 > 0);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRecursiveReverseRelationship()
        {
            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("derivedTypes*, derivedTypes.alias");

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "person"), rq);

            RelationshipInstanceData child = result.Relationships[0].Instances.FirstOrDefault(x => x.Entity.Fields[0].Value.ValueString == "test:employee");
            Assert.IsNotNull(child, "Can't find child type");

            RelationshipInstanceData grandchild = child.Entity.Relationships[0].Instances.FirstOrDefault(x => x.Entity.Fields[0].Value.ValueString == "test:manager");
            Assert.IsNotNull(grandchild, "Can't find grandchild type");
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestFollowSameRelationshipTwice()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("isOfType.isOfType.id,isOfType.isOfType.id");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), request);
            Assert.IsTrue(result.Relationships[0].Instances.Count == 1);
            Assert.IsTrue(result.Relationships[0].Instances[0].Entity.Relationships[0].Instances.Count == 1);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRelatedId()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("fields.id");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), request);
            Assert.IsTrue(result.Relationships[0].Instances.Count > 0);
            Assert.IsTrue(result.Relationships[0].Instances[0].Entity.Id.Id > 0);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRelatedMetadata()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("fields.?");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "employee"), request);
            Assert.IsTrue(result.Relationships[0].Instances.Count == 0);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestRelationshipDirections()
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("inherits.id,derivedTypes.id");
            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("test", "person"), request);

            Assert.AreEqual(2, result.Relationships.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestReturnType()
        {
            // Verify that  the minimum allowed value (minInt) of the decimal places field reports itself as being an integer.
            // I.e. ask for an integer, and ensure that's what we get.

            EntityMemberRequest rq =
                EntityRequestHelper.BuildRequest("minInt");

            var svc = GetService();
            EntityData result = svc.GetEntityData(new EntityRef("core", "decimalPlaces"), rq);

            Assert.IsTrue(result.Fields[0].Value.Type is Int32Type);
        }

        [Test]
        [RunAsDefaultTenant]
		[ClearCaches( ClearCachesAttribute.Caches.EntityRelationshipCache | ClearCachesAttribute.Caches.BulkResultCache )]
        public void TestEntitiesReturnedInOrderRequested()
        {
            // Result IDs are expected to come back in the same order they were requested.
            // Eg spFieldValidator seems to rely on this

            // Get a list of IDs
            var type = Entity.Get<EntityType>("test:herb");
            List<EntityRef> ids = type.InstancesOfType.Select(e => new EntityRef(e.Id)).ToList();
            ids.Reverse();  // for good measure
            Assert.IsTrue(ids.Count > 0);

            // Request them
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();
            var results = svc.GetEntitiesData(ids, request).ToList();

            // Check
            for (int i = 0; i < ids.Count; i++)
            {
                Assert.AreEqual(ids[i].Id, results[i].Id.Id);
            }
        }

        [TestCase("#{0}.alias,-#{0}.?", "rev")]
        [TestCase("-#{0}.?,#{0}.alias", "rev")]
        [TestCase("#{0}.?,-#{0}.alias", "fwd")]
        [TestCase("-#{0}.alias,#{0}.?", "fwd")]
        [RunAsDefaultTenant]
        public void Test_SameRelInFwdAndRev(string requestString, string metadataOnlyDir)
        {
            EntityMemberRequest request = EntityRequestHelper.BuildRequest("id");
            var svc = GetService();

            var resource = new EntityRef("core", "chart");

            // get 'isOfType' relationship Id
            long relId = Entity.GetId("core:isOfType");
            Assert.IsTrue(relId > 0);

            // build request string to get relationship fields in fwd direction and then metaData for the same relationship in reverse direction
            var relRequestString = string.Format(requestString, relId);
            EntityMemberRequest relRequest = EntityRequestHelper.BuildRequest(relRequestString);
            EntityData result = svc.GetEntityData(resource, relRequest);

            if (metadataOnlyDir == "fwd")
            {
                Assert.IsTrue(!result.GetRelationship(relId, Direction.Forward).Entities.Any());
                Assert.IsTrue(result.GetRelationship(relId, Direction.Reverse).Entities.Any());
            }
            else
            {
                Assert.IsTrue(result.GetRelationship(relId, Direction.Forward).Entities.Any());
                Assert.IsTrue(!result.GetRelationship(relId, Direction.Reverse).Entities.Any());
            }
        }
        

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("securesTo", true)]
        [TestCase("securesFrom", false)]
        [TestCase("", false)]
        [TestCase("reverse,securesTo", false)]
        [TestCase("reverse,securesFrom", true)]
        [TestCase("reverse", false)]
        public void Test_GetImplicitRelated(string instructions, bool expectReturned)
        {
            if (_runner == "EntityInfoService")
                Assert.Ignore();

            bool securesTo = instructions.Contains("securesTo");
            bool securesFrom = instructions.Contains("securesFrom");
            bool isReverse = instructions.Contains("reverse");

            UserAccount userAccount;
            EntityType entityType;
            Relationship relationship;
            IEntity entity;
            IEntity relatedEntity;

            // Create user
            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.Save();

            // Create type (used for both ends)
            entityType = new EntityType();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Save();

            // Create relationship
            relationship = new Relationship();
            relationship.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            relationship.FromType = entityType;
            relationship.ToType = entityType;
            relationship.SecuresTo = securesTo;
            relationship.SecuresFrom = securesFrom;
            relationship.Save();

            // Create related entity (the one that may or may not be visible)
            relatedEntity = Entity.Create(new EntityRef(entityType));
            relatedEntity.SetField("core:name", "B"); // "B" so it does not match the access rule
            relatedEntity.Save();

            // Create initial entity (the one that will be explicitly granted permissions, and queried)
            entity = Entity.Create(new EntityRef(entityType));
            entity.SetField("core:name", "A"); // "A" so it will match the access rule
            var relInstances = entity.GetRelationships(relationship.Id, isReverse ? Direction.Reverse : Direction.Forward);
            relInstances.Add(relatedEntity);
            entity.SetRelationships(relationship.Id, relInstances, isReverse ? Direction.Reverse : Direction.Forward);
            entity.Save();

            // Grant access to initial entity
            new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    TestQueries.EntitiesWithNameA().ToReport());

            // Build request
            string query = string.Format("name,{0}#{1}.name", isReverse ? "-" : "", relationship.Id);
            EntityMemberRequest request = EntityRequestHelper.BuildRequest(query);
            var svc = GetService();

            // Run request
            EntityData result;
            using (new SetUser(userAccount))
            {
                result = svc.GetEntityData(new EntityRef(entity.Id), request);
            }

            // Check results
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Relationships, Has.Exactly(1).Not.Null);
            if (expectReturned)
            {
                Assert.That(result.Relationships[0].Instances, Has.Exactly(1).Not.Null);
                Assert.That(result.Relationships[0].Instances[0].Entity.Id.Id, Is.EqualTo(relatedEntity.Id));
            }
            else
            {
                Assert.That(result.Relationships[0].Instances, Is.Empty);                    
            }
        }


        [Test]
        [TestCase(false)]
        [TestCase(true)]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestWriteOnlyFields(bool isFieldWriteOnly)
        {
            try
            {
                var field = Entity.Get<Field>("test:afString", true);
                field.IsFieldWriteOnly = isFieldWriteOnly;
                field.Save();

                EntityMemberRequest request = EntityRequestHelper.BuildRequest("name, test:afString");
                var svc = GetService();

                if (_runner == "BulkRequestRunner")
                {
                    // Ensure we don't use cache.
                    // The writability flag of the field is stored in the bulk query
                    // cache. Without this, the change made to the IsFieldWriteOnly field
                    // at the start of this test is not respected.
                    request.RequestString = string.Empty;
                }

                EntityData result = svc.GetEntityData(new EntityRef("test", "af01"), request);

                Assert.AreEqual("Test 01", result.GetField("core:name").Value.ValueString);
                if (isFieldWriteOnly)
                {
                    Assert.That(result.GetField("test:afString").Value.Value, Is.StringMatching("[*]+"));
                    // Assert.IsNull(result.GetField("test:afString").Value.Value);
                }
                else
                {
                    Assert.IsNotNull(result.GetField("test:afString").Value.Value);
                }
            }
            finally
            {
                CacheManager.ClearCaches();
            }            
        }
    }


    /// <summary>
    /// Extension methods to help EntityInfoService tests.
    /// </summary>
    public static class Extension
    {
        /// <summary>
        ///     Locates the requested relationship, or returns null if not found.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="relationshipTypeId">The relationship type id.</param>
        /// <returns></returns>
        public static RelationshipData GetRelationship(this EntityData ed, EntityRef relationshipTypeId)
        {
            Direction dir = Entity.GetDirection(relationshipTypeId);

            // Warning: assumes forward direction
            return ed.Relationships.SingleOrDefault(r => r.RelationshipTypeId.Id == relationshipTypeId.Id && Entity.GetDirection(r.RelationshipTypeId) == dir);
        }
    }
}