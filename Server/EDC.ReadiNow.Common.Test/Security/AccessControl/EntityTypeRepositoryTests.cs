// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [FailOnEvent]
    public class EntityTypeRepositoryTests
    {
        [Test]
        public void Test_Creation_Null()
        {
            Assert.That(() => new EntityTypeRepository(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("getTypes"));
        }

        [Test]
        public void Test_DefaultCreation()
        {
            Assert.That(new EntityTypeRepository().GetTypes, 
                Is.EqualTo((Func<IEntity, ISet<long>>) PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf));
        }

        [Test]
        public void Test_Creation_Arg()
        {
            Func<IEntity, ISet<long>> getTypes = x => new HashSet<long>();
            Assert.That(new EntityTypeRepository(getTypes).GetTypes, Is.EqualTo(getTypes));
        }

        [Test]
        public void Test_GetEntityTypes_Null()
        {
            Assert.That(() => new EntityTypeRepository().GetEntityTypes(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityRefs"));

        }

        [Test]
        public void Test_GetEntityTypes_ContainsNull()
        {
            Assert.That(() => new EntityTypeRepository().GetEntityTypes(new EntityRef[] { null }),
                Throws.TypeOf<ArgumentException>().And.Property("ParamName").EqualTo("entityRefs"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetEntityTypes_SingleEntity()
        {
            IDictionary<long, ISet<EntityRef>> result;
            EntityRef testEntityRef;

            testEntityRef = new EntityRef("core", "allowAccess");

            // Sanity tests
            Assert.That(testEntityRef, Has.Property("Entity").Not.Null);
            Assert.That(testEntityRef.Entity.GetAllTypes().ToList( ), Has.Count.Positive);

            result = new EntityTypeRepository().GetEntityTypes(new[] {testEntityRef});

            Assert.That(
                result.Keys,
                Is.EquivalentTo(testEntityRef.Entity.GetAllTypes().Select(x => x.Id)));
            Assert.That(
                result.Values,
                Is.All.EquivalentTo(new[] {testEntityRef})
                    .Using(EntityRefComparer.Instance));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_GetEntityTypes_TwoIdenticalEntities()
        {
            IDictionary<long, ISet<EntityRef>> result;
            List<EntityRef> testEntityRefs;

            // Two different relations
            testEntityRefs = new List<EntityRef>()
            {
                Permissions.Read,
                Permissions.Modify
            };

            result = new EntityTypeRepository().GetEntityTypes(testEntityRefs);

            Assert.That(
                result.Keys,
                Is.EquivalentTo(testEntityRefs.SelectMany(er => er.Entity.GetAllTypes().Select(x => x.Id)).Distinct()));
            Assert.That(
                result.Values,
                Is.All.EquivalentTo(testEntityRefs)
                        .Using(EntityRefComparer.Instance));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_GetEntityTypes_TwoDifferentEntities()
        {
            IDictionary<long, ISet<EntityRef>> result;
            List<EntityRef> testEntityRefs;

            // Relation and a type
            testEntityRefs = new List<EntityRef>()
            {
                new EntityRef("core", "allowAccess"),
                new EntityRef("core", "securableEntity")
            };

            result = new EntityTypeRepository().GetEntityTypes(testEntityRefs);

            Assert.That(
                result.Keys,
                Is.EquivalentTo(testEntityRefs.SelectMany(er => er.Entity.GetAllTypes().Select(x => x.Id)).Distinct()));
            Assert.That(
                result.Values,
                Has.None.Not.SubsetOf(testEntityRefs));
            foreach (EntityRef entityRef in testEntityRefs)
            {
                foreach (long entityTypeId in entityRef.Entity.GetAllTypes().Select(et => et.Id))
                {
                    Assert.That(result.Keys, Contains.Item(entityTypeId));
                    Assert.That(result[entityTypeId], Contains.Item(entityRef));
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_GetEntityTypes_TwoSameOneDifferentEntities()
        {
            IDictionary<long, ISet<EntityRef>> result;
            List<EntityRef> testEntityRefs;

            // Relation and a type
            testEntityRefs = new List<EntityRef>()
            {
                new EntityRef("core", "read"),
                new EntityRef("core", "modify"),
                new EntityRef("core", "securableEntity")
            };

            result = new EntityTypeRepository().GetEntityTypes(testEntityRefs);

            Assert.That(
                result.Keys,
                Is.EquivalentTo(testEntityRefs.SelectMany(er => er.Entity.GetAllTypes().Select(x => x.Id)).Distinct()));
            Assert.That(
                result.Values,
                Has.None.Not.SubsetOf(testEntityRefs));
            foreach (EntityRef entityRef in testEntityRefs)
            {
                foreach (long entityTypeId in entityRef.Entity.GetAllTypes().Select(et => et.Id))
                {
                    Assert.That(result.Keys, Contains.Item(entityTypeId));
                    Assert.That(result[entityTypeId], Contains.Item(entityRef));
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_GetEntityTypes_EntityRefWithNoEntity()
        {
            IDictionary<long, ISet<EntityRef>> result;
            List<EntityRef> testEntityRefs;
            EntityRef entityRef;

            entityRef = new EntityRef(EntityTemporaryIdAllocator.AcquireId());
            try
            {
                // Sanity Check
                Assert.That(entityRef.Entity, Is.Null, "entityRef.Entity not null");

                testEntityRefs = new List<EntityRef>
                {
                    entityRef
                };

                result = new EntityTypeRepository().GetEntityTypes(testEntityRefs);

                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(EntityTypeRepository.TypelessId)
                                .And.Property("Value")
                                .EquivalentTo(testEntityRefs)
                                .Using(EntityRefComparer.Instance),
                            "Relationship entity IDs missing");
            }
            finally
            {
                EntityTemporaryIdAllocator.RelinquishId(entityRef.Id);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetEntityTypes_TypelessEntity()
        {
            IDictionary<long, ISet<EntityRef>> result;
            List<EntityRef> testEntityRefs;

            // Sanity Check
            Assert.That(new EntityRef(EntityId.Max).HasEntity, Is.False, "EntityId.Max.HasEntity");

            testEntityRefs = new List<EntityRef>()
            {
                new EntityRef(EntityTemporaryIdAllocator.AcquireId())
            };

            try
            {
                result = new EntityTypeRepository(x => new HashSet<long>()).GetEntityTypes(testEntityRefs);

                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result,
                            Has.Exactly(1)
                                .Property("Key")
                                .EqualTo(EntityTypeRepository.TypelessId)
                                .And.Property("Value")
                                .EquivalentTo(testEntityRefs)
                                .Using(EntityRefComparer.Instance),
                            "Relationship entity IDs missing");
            }
            finally
            {
                EntityTemporaryIdAllocator.RelinquishId(testEntityRefs[0].Id);
            }
        }
    }
}
