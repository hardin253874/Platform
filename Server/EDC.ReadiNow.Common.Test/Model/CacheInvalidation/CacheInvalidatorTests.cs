// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.Collections.Generic;
using EDC.Diagnostics;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using Moq;
using NUnit.Framework;
using Quartz;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
	[RunWithTransaction]
    public class CacheInvalidatorTests
    {
        [Test]
        public void Test_Ctor()
        {
            CacheInvalidator<string, int> cacheInvalidator;
            ICache<string, int> cache;
            const string testName = "name";

            cache = new DictionaryCache<string, int>();

            cacheInvalidator = new CacheInvalidator<string, int>(cache, testName);

            Assert.That(cacheInvalidator, Has.Property("Name").EqualTo(testName));
            Assert.That(cacheInvalidator, Has.Property("Cache").EqualTo(cache));
            Assert.That(cacheInvalidator, Has.Property("EntityToCacheKey").Property("Keys").Empty);
            Assert.That(cacheInvalidator, Has.Property("FieldTypeToCacheKey").Property("Keys").Empty);
            Assert.That(cacheInvalidator, Has.Property("RelationshipTypeToCacheKey").Property("Keys").Empty);
            Assert.That(cacheInvalidator, Has.Property("EntityInvalidatingRelationshipTypesToCacheKey").Property("Keys").Empty);
            Assert.That(cacheInvalidator, Has.Property("EntityTypeToCacheKey").Property("Keys").Empty);
            Assert.That(cacheInvalidator, Has.Property("TraceCacheInvalidationFactory").Not.Null);
        }

        [Test]
        public void Test_OnEntityChange_NullEntities()
        {
            CacheInvalidator<string, string> cacheInvalidator;

            cacheInvalidator = new CacheInvalidator<string, string>(new DictionaryCache<string, string>(), "a");

            Assert.That(() => cacheInvalidator.OnEntityChange(null, InvalidationCause.Save, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));            
        }

        [Test]
        public void Test_OnEntityChange_EntitiesContainsNull()
        {
            CacheInvalidator<string, string> cacheInvalidator;

            cacheInvalidator = new CacheInvalidator<string, string>(new DictionaryCache<string, string>(), "a");

            Assert.That(() => cacheInvalidator.OnEntityChange(new IEntity[] { null }, InvalidationCause.Save, null),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        public void Test_OnEntityChange_InvalidCause()
        {
            CacheInvalidator<string, string> cacheInvalidator;

            cacheInvalidator = new CacheInvalidator<string, string>(new DictionaryCache<string, string>(), "a");

            Assert.That(() => cacheInvalidator.OnEntityChange(new IEntity[0], (InvalidationCause)42, null),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("cause"));
        }

        [Test]
        public void Test_OnEntityChange_Entities()
        {
            MockRepository mockRepository;
            CacheInvalidator<long, string> cacheInvalidator;
            ICache<long, string> cache;
            IEntity[] testEntities;
            Mock<IEntity> mockEntity;
            const int numEntities = 10;

            mockRepository = new MockRepository(MockBehavior.Loose);

            testEntities = new IEntity[numEntities];
            for (int i = 0; i < testEntities.Length; i++)
            {
                mockEntity = mockRepository.Create<IEntity>();
                mockEntity.SetupGet(e => e.Id).Returns(i);
                testEntities[i] = mockEntity.Object;
            }

            cache = new DictionaryCache<long, string>();
            for (int i = 0; i < numEntities; i++)
            {
                cache.Add(i, i.ToString());
            }

            cacheInvalidator = new CacheInvalidator<long, string>(cache, "a");

            // Make the second and third entities depend on the first entity
            using (CacheContext cacheContext = new CacheContext())
            {
                cacheContext.Entities.Add(testEntities[0].Id);        // Will convert to EntityRef using this ID
                cacheInvalidator.AddInvalidations(cacheContext, testEntities[1].Id);
                cacheInvalidator.AddInvalidations(cacheContext, testEntities[2].Id);
            }

            // Save the first entity.
            cacheInvalidator.OnEntityChange(new [] { testEntities[0] }, InvalidationCause.Save, null);

            Assert.That(
                cache.Select(ce => ce.Key),
                Is.EquivalentTo(
                    testEntities.Select(er => er.Id)
                                .Where(id => id != testEntities[1].Id && id != testEntities[2].Id)),
                "Second and third entities (only) have not been removed");
        }

        [Test]
        public void Test_OnEntityChange_EntityTypes()
        {
            MockRepository mockRepository;
            CacheInvalidator<long, string> cacheInvalidator;
            ICache<long, string> cache;
            IEntity[] testEntities;
            Mock<IEntity> mockEntity;
            const int numEntities = 10;
            const long typeIdOffset = 100;

            mockRepository = new MockRepository(MockBehavior.Loose);

            testEntities = new IEntity[numEntities];
            for (int i = 0; i < testEntities.Length; i++)
            {
                mockEntity = mockRepository.Create<IEntity>();
                mockEntity.SetupGet(e => e.Id).Returns(i);
                mockEntity.SetupGet(e => e.TypeIds).Returns(new [] { typeIdOffset + 100 });
                testEntities[i] = mockEntity.Object;
            }

            cache = new DictionaryCache<long, string>();
            for (int i = 0; i < numEntities; i++)
            {
                cache.Add(i, i.ToString());
            }

            cacheInvalidator = new CacheInvalidator<long, string>(cache, "a");

            // Make the second and third entities depend on the type of the first.
            using (CacheContext cacheContext = new CacheContext())
            {
                cacheContext.EntityTypes.Add(testEntities[0].TypeIds.First()); // Will convert to EntityRef using this ID
                cacheInvalidator.AddInvalidations(cacheContext, testEntities[1].Id);
                cacheInvalidator.AddInvalidations(cacheContext, testEntities[2].Id);
            }

            // Save the first entity (only)
            cacheInvalidator.OnEntityChange(new [] { testEntities[0] }, InvalidationCause.Save, null);

            Assert.That(
                cache.Select(ce => ce.Key),
                Is.EquivalentTo(
                    testEntities.Select(er => er.Id)
                                .Where(id => id != testEntities[1].Id && id != testEntities[2].Id)),
                "Second and third entities (only) have not been removed");
        }

        [Test]
        public void Test_OnRelationshipChange_NullRelationshipTypes()
        {
            CacheInvalidator<string, string> cacheInvalidator;

            cacheInvalidator = new CacheInvalidator<string, string>(new DictionaryCache<string, string>(), "a");

            Assert.That(() => cacheInvalidator.OnRelationshipChange(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("relationshipTypes"));
        }

        [Test]
        public void Test_OnRelationshipChange_RelationshipTypesContainsNull()
        {
            CacheInvalidator<string, string> cacheInvalidator;

            cacheInvalidator = new CacheInvalidator<string, string>(new DictionaryCache<string, string>(), "a");

            Assert.That(() => cacheInvalidator.OnRelationshipChange(new EntityRef[] { null }),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("relationshipTypes"));
        }

        [Test]
        public void Test_OnRelationshipChange()
        {
            CacheInvalidator<int, string> cacheInvalidator;
            ICache<int, string> cache;
            EntityRef[] testRelationshipTypes;
            const int numRelationshipTypes = 10;
            Func<long, bool> relationshipTypesToRemove;
                
            cache = new DictionaryCache<int, string>();
            for (int i = 0; i < numRelationshipTypes; i++)
            {
                cache.Add(i, i.ToString());
            }

            relationshipTypesToRemove = e => e % 2 == 0; // All even numbered relationship types.
            testRelationshipTypes = Enumerable.Range(0, numRelationshipTypes)
                                              .Where(i => relationshipTypesToRemove(i))
                                              .Select(i => new EntityRef(i)).ToArray();

            cacheInvalidator = new CacheInvalidator<int, string>(cache, "a");

            for (int i = 0; i < numRelationshipTypes; i++)
            {
                using (CacheContext cacheContext = new CacheContext())
                {
                    cacheContext.RelationshipTypes.Add(i);
                    cacheInvalidator.AddInvalidations(cacheContext, i);
                }
            }

            cacheInvalidator.OnRelationshipChange(testRelationshipTypes);

            Assert.That(cache.Where(ce => relationshipTypesToRemove(ce.Key)), Is.Empty);
        }

        [Test]
        public void Test_OnFieldChange_NullFieldTypes()
        {
            CacheInvalidator<int, string> cacheInvalidator;

            cacheInvalidator = new CacheInvalidator<int, string>(new DictionaryCache<int, string>(), "a");

            Assert.That(() => cacheInvalidator.OnFieldChange(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("fieldTypes"));
        }

        [Test]
        public void Test_OnFieldChange()
        {
            CacheInvalidator<int, string> cacheInvalidator;
            ICache<int, string> cache;
            long[] testFieldTypes;
            const int numFieldTypes = 10;
            Func<long, bool> fieldTypesToRemove;

            cache = new DictionaryCache<int, string>();
            for (int i = 0; i < numFieldTypes; i++)
            {
                cache.Add(i, i.ToString());
            }

            fieldTypesToRemove = e => e % 2 == 0; // All even numbered relationship types.
            testFieldTypes = Enumerable.Range(0, numFieldTypes)
                                       .Where(i => fieldTypesToRemove(i))
                                       .Select(i => (long) i)
                                       .ToArray();

            cacheInvalidator = new CacheInvalidator<int, string>(cache, "a");

            for (int i = 0; i < numFieldTypes; i++)
            {
                using (CacheContext cacheContext = new CacheContext())
                {
                    cacheContext.FieldTypes.Add(i);
                    cacheInvalidator.AddInvalidations(cacheContext, i);
                }
            }

            cacheInvalidator.OnFieldChange(testFieldTypes);

            Assert.That(cache.Where(ce => fieldTypesToRemove(ce.Key)), Is.Empty);
        }

        [Test]
        public void Test_AddInvalidations_NullCacheContext()
        {
            Assert.That(() => new CacheInvalidator<string, string>(new DictionaryCache<string, string>(), "a").AddInvalidations(null, string.Empty),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cacheContext"));
        }

        [Test]
        public void Test_AddInvalidations()
        {
            CacheInvalidator<string, string> cacheInvalidator;
            long testEntity;
            long testRelationshipType;
            long testFieldType;
            long testEntityInvalidatingRelationshipTypeRef;
            long testEntityTypeRef;
            
            cacheInvalidator = new CacheInvalidator<string, string>(
                new DictionaryCache<string, string>(), "foo");

            testEntity = 1;
            testRelationshipType = 2;
            testFieldType = 3;
            testEntityInvalidatingRelationshipTypeRef = 4;
            testEntityTypeRef = 5;
            using (CacheContext cacheContext = new CacheContext())
            {
                cacheContext.Entities.Add(testEntity);
                cacheContext.RelationshipTypes.Add(testRelationshipType);
                cacheContext.FieldTypes.Add(testFieldType);
                cacheContext.EntityInvalidatingRelationshipTypes.Add(testEntityInvalidatingRelationshipTypeRef);
                cacheContext.EntityTypes.Add(testEntityTypeRef);

                // Sanity check
                Assert.That(cacheInvalidator.EntityToCacheKey, Has.Property("Keys").Empty);
                Assert.That(cacheInvalidator.RelationshipTypeToCacheKey, Has.Property("Keys").Empty);
                Assert.That(cacheInvalidator.FieldTypeToCacheKey, Has.Property("Keys").Empty);
                Assert.That(cacheInvalidator.EntityInvalidatingRelationshipTypesToCacheKey, Has.Property("Keys").Empty);
                Assert.That(cacheInvalidator.EntityTypeToCacheKey, Has.Property("Keys").Empty);

                cacheInvalidator.AddInvalidations(cacheContext, "foo");

                Assert.That(cacheInvalidator.EntityToCacheKey, 
                    Has.Property("Keys").Exactly(1).EqualTo(testEntity));
                Assert.That(cacheInvalidator.RelationshipTypeToCacheKey,
                    Has.Property("Keys").Exactly(1).EqualTo(testRelationshipType));
                Assert.That(cacheInvalidator.FieldTypeToCacheKey,
                    Has.Property("Keys").Exactly(1).EqualTo(testFieldType));
                Assert.That(cacheInvalidator.EntityInvalidatingRelationshipTypesToCacheKey,
                    Has.Property("Keys").Exactly(1).EqualTo(testEntityInvalidatingRelationshipTypeRef));
                Assert.That(cacheInvalidator.EntityTypeToCacheKey,
                    Has.Property("Keys").Exactly(1).EqualTo(testEntityTypeRef));
            }
        }

        [Test]
        public void Test_ItemsRemoved()
        {
            CacheInvalidator<int, string> cacheInvalidator;
            ICache<int, string> cache;
            const int testKey1 = 42;
            const int testKey2 = 54;
            const string testValue1 = "foo";
            const string testValue2 = "bar";

            cache = new DictionaryCache<int, string>();
            cache.Add(testKey1, testValue1);
            cache.Add(testKey2, testValue2);

            cacheInvalidator = new CacheInvalidator<int, string>(cache, "foo");

            using (CacheContext cacheContext = new CacheContext())
            {
                cacheContext.Entities.Add(testKey1);
                cacheInvalidator.AddInvalidations(cacheContext, testKey1);
            }
            using (CacheContext cacheContext = new CacheContext())
            {
                cacheContext.Entities.Add(testKey2);
                cacheInvalidator.AddInvalidations(cacheContext, testKey2);
            }

            cache.Remove(testKey1);

            Assert.That(cacheInvalidator.EntityToCacheKey.Keys, Has.None.EqualTo(testKey1));
            Assert.That(cacheInvalidator.EntityToCacheKey.Keys, Has.Exactly(1).EqualTo(testKey2));
        }

        [Test]
        public void Test_GetTraceCacheInvalidationSetting()
        {
            bool oldSetting;
            bool newSetting;
            CacheInvalidator<int, int> cacheInvalidator;

            oldSetting = ConfigurationSettings.GetServerConfigurationSection().Security.CacheTracing;
            try
            {
                newSetting = !oldSetting;
                ConfigurationSettings.GetServerConfigurationSection().Security.CacheTracing = newSetting;

                cacheInvalidator = new CacheInvalidator<int, int>(new DictionaryCache<int, int>(), "Test");

                Assert.That(cacheInvalidator.TraceCacheInvalidations, Is.EqualTo(newSetting));
            }
            finally 
            {
                ConfigurationSettings.GetServerConfigurationSection().Security.CacheTracing = oldSetting;
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_TraceCacheInvalidations(bool traceCacheInvalidations)
        {
            CacheInvalidator<long, long> cacheInvalidator;
            ICache<long, long> cache;
            IList<IEntity> testEntities;
            IEntity testEntity1;
            IEntity testEntity2;
            const string cacheInvalidatorName = "Test";

            testEntity1 = Entity.Create<Folder>();
            testEntity1.Save();

            testEntity2 = Entity.Create<Folder>();
            testEntity2.Save();

            testEntities = new[] { testEntity1, testEntity2 };

            cache = new DictionaryCache<long, long>();
            cache.Add(testEntity1.Id, testEntity1.Id);

            cacheInvalidator = new TestCacheInvalidator<long, long>(cache, cacheInvalidatorName, 
                () => traceCacheInvalidations);

            using (CacheContext cacheContext = new CacheContext())
            {
                cacheContext.Entities.Add(testEntities.Select(e => e.Id));

                cacheInvalidator.AddInvalidations(cacheContext, testEntity1.Id);
                using (EventLogMonitor monitor = new EventLogMonitor())
                {
                    cacheInvalidator.OnEntityChange(testEntities, InvalidationCause.Save, null);

                    if (traceCacheInvalidations)
                    {
                        // Note: even only one of the entities has changed, both will appear in the message as it is processed in bulk.
                        Assert.That(monitor.Entries,
                            Has.Exactly(1)
                                .Property("Level").EqualTo(EventLogLevel.Trace).And
                                .Property("ThreadId").EqualTo(Thread.CurrentThread.ManagedThreadId).And
                                .Property("Message").EqualTo(
                                    string.Format(
                                        "Change to 'entity {0},{1}' caused cache invalidator '{2}' to remove entries '{3}'",
                                        testEntity1.Id, testEntity2.Id, cacheInvalidatorName, testEntity1.Id ) ) );
                    }
                }
            }
        }
    }
}


