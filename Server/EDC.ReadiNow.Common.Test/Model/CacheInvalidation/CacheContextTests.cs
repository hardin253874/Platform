// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache.Providers;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
	[RunWithTransaction]
    public class CacheContextTests
    {
        [Test]
        public void Test_Ctor()
        {
            using (CacheContext cacheContext = new CacheContext())
            {
                Assert.That(cacheContext, Has.Property("Entities").Count.EqualTo(0));
                Assert.That(cacheContext, Has.Property("RelationshipTypes").Count.EqualTo(0));
                Assert.That(cacheContext, Has.Property("FieldTypes").Count.EqualTo(0));
                Assert.That(cacheContext, Has.Property("EntityInvalidatingRelationshipTypes").Count.EqualTo(0));
                Assert.That(cacheContext, Has.Property("EntityTypes").Count.EqualTo(0));
                Assert.That(cacheContext, Has.Property("ContextType").EqualTo(ContextType.New));
            }
        }

        [Test]
        public void Test_IsSet_OneLevel()
        {
            Assert.That(CacheContext.IsSet(), Is.False, "Set beforehand");
            using (new CacheContext())
            {
                Assert.That(CacheContext.IsSet(), Is.True, "Not set");
            }
            Assert.That(CacheContext.IsSet(), Is.False, "Set afterwards");
        }

        [Test]
        public void Test_IsSet_TwoLevels()
        {
            Assert.That(CacheContext.IsSet(), Is.False, "Set beforehand");
            using (new CacheContext())
            {
                Assert.That(CacheContext.IsSet(), Is.True, "Not pre-second level");
                using (new CacheContext())
                {
                    Assert.That(CacheContext.IsSet(), Is.True, "Not second level");
                }
                Assert.That(CacheContext.IsSet(), Is.True, "Not post-second level");
            }
            Assert.That(CacheContext.IsSet(), Is.False, "Set afterwards");
        }

        [Test]
        public void Test_GetContext_NoContext()
        {
            CacheContext cacheContext;

            using ( cacheContext = CacheContext.GetContext( ) )
            {
                Assert.That( cacheContext, Has.Property( "ContextType" ).EqualTo( ContextType.Detached ) );
                Assert.That( cacheContext, Has.Property( "Entities" ).Count.EqualTo( 0 ) );
                Assert.That( cacheContext, Has.Property( "RelationshipTypes" ).Count.EqualTo( 0 ) );
                Assert.That( cacheContext, Has.Property( "FieldTypes" ).Count.EqualTo( 0 ) );
                Assert.That( cacheContext, Has.Property( "EntityInvalidatingRelationshipTypes" ).Count.EqualTo( 0 ) );
            }
        }

        [Test]
        public void Test_GetContext()
        {
            Assert.That(CacheContext.IsSet(), Is.False, "Set initially");

            using (CacheContext newCacheContext = new CacheContext())
            {
                using (CacheContext attachedCacheContext = CacheContext.GetContext())
                {
                    Assert.That(newCacheContext.Entities,
                        Is.EquivalentTo(attachedCacheContext.Entities), "Entities mismatch");
                    Assert.That(attachedCacheContext.ContextType,
                        Is.EqualTo(ContextType.Attached), "Incorrect context type");
                }

                Assert.That(CacheContext.IsSet(), Is.True, "Attached Dispose() removed context");
            }

            Assert.That(CacheContext.IsSet(), Is.False, "New Dispose() did not remove context");
        }

        [Test]
        public void Test_AddEntities_NullEntities()
        {
            using (CacheContext cacheContext = new CacheContext())
            {
                Assert.That(() => cacheContext.Entities.Add(null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("items"));
            }
        }

        [Test]
        public void Test_AddEntities_SingleLevel()
        {
            long entity;

            entity = 1;
            using (CacheContext cacheContext = new CacheContext())
            {
                cacheContext.Entities.Add(entity);

                Assert.That(cacheContext.Entities.Contains(entity));
            }
        }

        [Test]
        public void Test_AddEntities_MultipleLevels()
        {
            long[] entities;
            const int numEntities = 4;

            entities = new long[numEntities];
            for (int i = 0; i < numEntities; i++)
            {
                entities[i] = i;
            }

            using (CacheContext outerCacheContext = new CacheContext())
            {
                outerCacheContext.Entities.Add(entities[0]);

                Assert.That(outerCacheContext.Entities.Contains(entities[0]), Is.True,
                        "Outer cache context missing entity 0");

                using (CacheContext innerCacheContext = new CacheContext())
                {
                    innerCacheContext.Entities.Add(entities[1]);

                    Assert.That(innerCacheContext.Entities.Contains(entities[1]), Is.True,
                        "Inner cache context missing entity 1");
                    Assert.That(outerCacheContext.Entities.Contains(entities[1]), Is.True,
                        "Outer cache context missing entity 1");

                    outerCacheContext.Entities.Add(entities[2]);

                    // Should be at that cache level and all ancesters, not children or descendants
                    Assert.That(innerCacheContext.Entities.Contains(entities[2]), Is.False,
                        "Inner cache context contains entity 2");
                    Assert.That(outerCacheContext.Entities.Contains(entities[2]), Is.True,
                        "Inner cache context missing entity 2");
                }

                outerCacheContext.Entities.Add(entities[3]);

                Assert.That(outerCacheContext.Entities, Is.EquivalentTo(entities),
                        "Outer cache context missing one or more entities");
            }
        }

        [Test]
        public void Test_DetachedContext()
        {
            long testEntityRef;
            long testRelationshipTypeRef;
            long testFieldTypeRef;

            testEntityRef = 1;
            testRelationshipTypeRef = 2;
            testFieldTypeRef = 3;
            using (CacheContext outerCacheContext = new CacheContext())
            {
                using (CacheContext detachedCacheContext = new CacheContext(ContextType.Detached))
                {
                    detachedCacheContext.Entities.Add(testEntityRef);
                    detachedCacheContext.RelationshipTypes.Add(testRelationshipTypeRef);
                    detachedCacheContext.FieldTypes.Add(testFieldTypeRef);

                    Assert.That(outerCacheContext, Has.Property("Entities").Count.EqualTo(0));
                    Assert.That(outerCacheContext, Has.Property("RelationshipTypes").Count.EqualTo(0));
                    Assert.That(outerCacheContext, Has.Property("FieldTypes").Count.EqualTo(0));
                    Assert.That(outerCacheContext, Has.Property("EntityInvalidatingRelationshipTypes").Count.EqualTo(0));
                }
            }
        }

        [Test]
        public void Test_AddInvalidationsFor_NullCacheInvalidator()
        {
            using (CacheContext cacheContext = new CacheContext())
            {
                Assert.That(() => cacheContext.AddInvalidationsFor<int, int>(null, 1),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cacheInvalidator"));
            }
        }

        [Test]
        [TestCase(new long[] { 1 }, new long[0], new long[0], new long[0])]
        [TestCase(new long[0], new long[] { 1 }, new long[0], new long[0])]
        [TestCase(new long[0], new long[0], new long[] { 1 }, new long[0])]
        [TestCase(new long[0], new long[0], new long[0], new long[] { 1 })]
        [TestCase(new long[] { 1, 2 }, new long[] { 1, 2 }, new long[] { 1, 2 }, new long[] { 1, 2 })]
        [TestCase(new long[] { 1, 2 }, new long[] { 3, 4 }, new long[] { 5, 6 }, new long[] { 7, 8 })]
        public void Test_AddInvalidationsFor(long[] entities, long[] relationshipTypes, long[] fieldTypes, long[] entityInvalidatingRelationshipTypes)
        {
            CacheInvalidator<long, long> cacheInvalidator;
            const int testKey = 1;
            IList<long> entityRefs;
            IList<long> relationshipTypeRefs;
            IList<long> fieldTypeRefs;
            IList<long> entityInvalidatingRelationshipTypeRefs;
            
            entityRefs = entities.ToList();
            relationshipTypeRefs = relationshipTypes.ToList();
            fieldTypeRefs = fieldTypes.ToList();
            entityInvalidatingRelationshipTypeRefs = entityInvalidatingRelationshipTypes.ToList();

            cacheInvalidator = new CacheInvalidator<long, long>(new DictionaryCache<long, long>(), "test");
            using (CacheContext originalCacheContext = new CacheContext())
            {
                originalCacheContext.Entities.Add(entityRefs);
                originalCacheContext.RelationshipTypes.Add(relationshipTypeRefs);
                originalCacheContext.FieldTypes.Add(fieldTypeRefs);
                originalCacheContext.EntityInvalidatingRelationshipTypes.Add(entityInvalidatingRelationshipTypeRefs);

                cacheInvalidator.AddInvalidations(originalCacheContext, testKey);
            }

            using (CacheContext outerCacheContext = new CacheContext())
            using (CacheContext innerCacheContext = new CacheContext())
            {
                Assert.That(innerCacheContext.Entities, Is.Empty, "Entities not initially empty");
                Assert.That(innerCacheContext.RelationshipTypes, Is.Empty, "RelationshipTypes not initially empty");
                Assert.That(innerCacheContext.FieldTypes, Is.Empty, "FieldTypes not initially empty");
                Assert.That(innerCacheContext.EntityInvalidatingRelationshipTypes, Is.Empty, "EntityInvalidatingRelationshipTypes not initially empty");

                innerCacheContext.AddInvalidationsFor(cacheInvalidator, testKey);
                Assert.That(innerCacheContext.Entities,
                    Is.EquivalentTo(entityRefs).Using(EntityRefComparer.Instance),
                    "Unexpected Entities");
                Assert.That(innerCacheContext.RelationshipTypes,
                    Is.EquivalentTo(relationshipTypeRefs).Using(EntityRefComparer.Instance),
                    "Unexpected RelationshipTypes");
                Assert.That(innerCacheContext.FieldTypes,
                    Is.EquivalentTo(fieldTypeRefs).Using(EntityRefComparer.Instance),
                    "Unexpected FieldTypes");
                Assert.That(innerCacheContext.EntityInvalidatingRelationshipTypes,
                    Is.EquivalentTo(entityInvalidatingRelationshipTypeRefs).Using(EntityRefComparer.Instance),
                    "Unexpected EntityInvalidatingRelationshipTypes");

                Assert.That(outerCacheContext.Entities,
                    Is.EquivalentTo(entityRefs).Using(EntityRefComparer.Instance),
                    "Unexpected Entities in outer cache context");
                Assert.That(outerCacheContext.RelationshipTypes,
                    Is.EquivalentTo(relationshipTypeRefs).Using(EntityRefComparer.Instance),
                    "Unexpected RelationshipTypes in outer cache context");
                Assert.That(outerCacheContext.FieldTypes,
                    Is.EquivalentTo(fieldTypeRefs).Using(EntityRefComparer.Instance),
                    "Unexpected FieldTypes in outer cache context");
                Assert.That(outerCacheContext.EntityInvalidatingRelationshipTypes,
                    Is.EquivalentTo(entityInvalidatingRelationshipTypeRefs).Using(EntityRefComparer.Instance),
                    "Unexpected EntityInvalidatingRelationshipTypes in outer cache context");
            }
        }
    }
}
