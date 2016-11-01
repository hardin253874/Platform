// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class CachingEntityMemberRequestFactoryTests
    {
        [Test]
        public void Ctor_Null()
        {
            Assert.That(() => new CachingEntityMemberRequestFactory(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("factory"));
        }

        [Test]
        public void Ctor()
        {
            CachingEntityMemberRequestFactory cachingEntityMemberRequestFactory;
            IEntityMemberRequestFactory entityMemberRequestFactory;

            entityMemberRequestFactory = new EntityMemberRequestFactory();
            cachingEntityMemberRequestFactory = new CachingEntityMemberRequestFactory(entityMemberRequestFactory);

            Assert.That(cachingEntityMemberRequestFactory, Has.Property("Factory").EqualTo(entityMemberRequestFactory));
            Assert.That(cachingEntityMemberRequestFactory, Has.Property("Cache").Count.EqualTo(0));
            Assert.That(cachingEntityMemberRequestFactory, Has.Property("CacheInvalidator").Not.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void BuildEntityMemberRequest(bool cached, bool useDifferentEntity)
        {
            var readPerm = new[ ] { Permissions.Read };

            MockRepository mockRepository;
            Mock<IEntityMemberRequestFactory> mockEntityMemberRequestFactory;
            EntityType entityType;
            EntityMemberRequest entityMemberRequest;
            CachingEntityMemberRequestFactory cachingEntityMemberRequestFactory;

            entityType = new EntityType();
            entityMemberRequest = new EntityMemberRequest();

            mockRepository = new MockRepository(MockBehavior.Strict);
            mockEntityMemberRequestFactory = mockRepository.Create<IEntityMemberRequestFactory>();
            mockEntityMemberRequestFactory
                .Setup(emrf => emrf.BuildEntityMemberRequest(entityType, readPerm ) )
                .Returns(() => entityMemberRequest);

            cachingEntityMemberRequestFactory = new CachingEntityMemberRequestFactory(
                mockEntityMemberRequestFactory.Object);
            Assert.That(cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityType, readPerm ),
                Is.EqualTo(entityMemberRequest));
            if (cached)
            {
                Assert.That(cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityType, readPerm ),
                    Is.EqualTo(entityMemberRequest));
            }
            Assert.That(cachingEntityMemberRequestFactory.Cache,
                Has.Count.EqualTo(1).And.Exactly(1).Property("Key").EqualTo(entityType.Id).And.Property("Value").EqualTo(entityMemberRequest));

            mockEntityMemberRequestFactory.Verify( emrf => emrf.BuildEntityMemberRequest( entityType, readPerm ), Times.Once() );

            if ( useDifferentEntity )
            {
                entityType = entityType.Clone( CloneOption.Shallow ).As<EntityType>( );
                mockEntityMemberRequestFactory.Verify( emrf => emrf.BuildEntityMemberRequest( entityType, readPerm ), Times.Never( ) );
            }

            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(Direction.Forward)]
        [TestCase(Direction.Reverse)]
        public void Invalidation_AddRelationship(Direction direction)
        {
            CachingEntityMemberRequestFactory cachingEntityMemberRequestFactory;
            EntityType entityType1;
            EntityType entityType2;
            Relationship relationshipType;
            IEntity entityOfType1;
            IEntity entityOfType2;
            IEntityRelationshipCollection<IEntity> relationships;
            ItemsRemovedEventHandler<long> cacheRemoval;
            List<long> itemsRemoved;

            var readPerm = new [ ] { Permissions.Read };

            // Setup the entities and relationships
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                entityType1 = new EntityType();
                entityType1.Save();
                entityType2 = new EntityType();
                entityType2.Save();
                if (direction == Direction.Forward)
                {
                    relationshipType = new Relationship
                    {
                        FromType = entityType1,
                        ToType = entityType2,
                        SecuresTo = true
                    };
                }
                else if (direction == Direction.Reverse)
                {
                    relationshipType = new Relationship
                    {
                        FromType = entityType2,
                        ToType = entityType1,
                        SecuresFrom = true
                    };
                }
                else
                {
                    throw new ArgumentException("Unknown direction", "direction");
                }
                relationshipType.Save();
                ctx.CommitTransaction();

            }

            // Create test entities
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entityOfType1 = Entity.Create(entityType1);
                entityOfType1.Save();
                entityOfType2 = Entity.Create(entityType2);
                entityOfType2.Save();
                ctx.CommitTransaction();
            }

            // Setup the test
            cachingEntityMemberRequestFactory =
                Factory.Current.Resolve<CachingEntityMemberRequestFactory>();

            cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityType2, readPerm );

            itemsRemoved = new List<long>( );
            cacheRemoval = (sender, args) => itemsRemoved.AddRange(args.Items);
            try
            {
                cachingEntityMemberRequestFactory.Cache.ItemsRemoved += cacheRemoval;

                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    // Create a relationship 
                    if (direction == Direction.Forward)
                    {
                        relationships = entityOfType1.GetRelationships(relationshipType, Direction.Forward);
                        relationships.Add(entityOfType2);
                        entityOfType1.SetRelationships(relationshipType, relationships, Direction.Forward);
                        entityOfType1.Save();
                    }
                    else if (direction == Direction.Reverse)
                    {
                        relationships = entityOfType2.GetRelationships(relationshipType, Direction.Reverse);
                        relationships.Add(entityOfType1);
                        entityOfType2.SetRelationships(relationshipType, relationships, Direction.Reverse);
                        entityOfType2.Save();
                    }
                    else
                    {
                        throw new ArgumentException("Unknown direction", "direction");
                    }

                    ctx.CommitTransaction();
                }
                // Ensure the cache has been invalidated
                Assert.That(itemsRemoved,
                    Is.Not.Contains( entityType2.Id ));
            }
            finally
            {
                cachingEntityMemberRequestFactory.Cache.ItemsRemoved -= cacheRemoval;
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(Direction.Forward)]
        [TestCase(Direction.Reverse)]
        public void Invalidation_AddRelationshipType(Direction direction)
        {
            CachingEntityMemberRequestFactory cachingEntityMemberRequestFactory;
            EntityType entityType1;
            EntityType entityType2;
            Relationship relationshipType;
            Relationship relationshipType2;
            IEntity entityOfType1;
            IEntity entityOfType2;
            ItemsRemovedEventHandler<long> cacheRemoval;
            List<long> itemsRemoved;

            var readPerm = new [ ] { Permissions.Read };

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                // Setup the entities and relationships
                entityType1 = new EntityType();
                entityType1.Save();
                entityType2 = new EntityType();
                entityType2.Save();
                if (direction == Direction.Forward)
                {
                    relationshipType = new Relationship
                    {
                        FromType = entityType1,
                        ToType = entityType2,
                        SecuresTo = true
                    };
                }
                else if (direction == Direction.Reverse)
                {
                    relationshipType = new Relationship
                    {
                        FromType = entityType2,
                        ToType = entityType1,
                        SecuresFrom = true
                    };
                }
                else
                {
                    throw new ArgumentException("Unknown direction", "direction");
                }
                relationshipType.Save();

                // Create test entities
                entityOfType1 = Entity.Create(entityType1);
                entityOfType1.Save();
                entityOfType2 = Entity.Create(entityType2);
                entityOfType2.Save();

                ctx.CommitTransaction();
            }

            // Setup the test
            cachingEntityMemberRequestFactory =
                Factory.Current.Resolve<CachingEntityMemberRequestFactory>();
            cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityType2, readPerm );

            itemsRemoved = new List<long>( );
            cacheRemoval = (sender, args) => itemsRemoved.AddRange(args.Items);
            try
            {
                cachingEntityMemberRequestFactory.Cache.ItemsRemoved += cacheRemoval;

                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    // Create a new relationship type
                    if (direction == Direction.Forward)
                    {
                        relationshipType2 = new Relationship
                        {
                            FromType = entityType1,
                            ToType = entityType2,
                            SecuresTo = true
                        };
                        relationshipType2.Save();
                    }
                    else if (direction == Direction.Reverse)
                    {
                        relationshipType2 = new Relationship
                        {
                            FromType = entityType2,
                            ToType = entityType1,
                            SecuresFrom = true
                        };
                        relationshipType2.Save();
                    }
                    else
                    {
                        throw new ArgumentException("Unknown direction", "direction");
                    }

                    ctx.CommitTransaction();
                }

                // Ensure the cache has been invalidated
                Assert.That(itemsRemoved,
                    Contains.Item( entityType2.Id ) );
                Assert.That(((CacheInvalidator<long, EntityMemberRequest>)cachingEntityMemberRequestFactory.CacheInvalidator)
                        .RelationshipTypeToCacheKey
                        .GetValues(relationshipType.Id),
                    Is.Not.Contains(entityType2));
            }
            finally
            {
                cachingEntityMemberRequestFactory.Cache.ItemsRemoved -= cacheRemoval;
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(Direction.Forward)]
        [TestCase(Direction.Reverse)]
        public void Invalidation_DeleteRelationshipType(Direction direction)
        {
            CachingEntityMemberRequestFactory cachingEntityMemberRequestFactory;
            EntityType entityType1;
            EntityType entityType2;
            Relationship relationshipType;
            IEntity entityOfType1;
            IEntity entityOfType2;
            ItemsRemovedEventHandler<long> cacheRemoval;
            List<long> itemsRemoved;

            var readPerm = new [ ] { Permissions.Read };

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                // Setup the entities and relationships
                entityType1 = new EntityType();
                entityType1.Save();
                entityType2 = new EntityType();
                entityType2.Save();
                if (direction == Direction.Forward)
                {
                    relationshipType = new Relationship
                    {
                        FromType = entityType1,
                        ToType = entityType2,
                        SecuresTo = true
                    };
                }
                else if (direction == Direction.Reverse)
                {
                    relationshipType = new Relationship
                    {
                        FromType = entityType2,
                        ToType = entityType1,
                        SecuresFrom = true
                    };
                }
                else
                {
                    throw new ArgumentException("Unknown direction", "direction");
                }
                relationshipType.Save();

                // Create test entities
                entityOfType1 = Entity.Create(entityType1);
                entityOfType1.Save();
                entityOfType2 = Entity.Create(entityType2);
                entityOfType2.Save();

                ctx.CommitTransaction();

            }

            // Setup the test
            cachingEntityMemberRequestFactory =
                Factory.Current.Resolve<CachingEntityMemberRequestFactory>();
            cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityType2, readPerm );

            itemsRemoved = new List<long>();
            cacheRemoval = (sender, args) => itemsRemoved.AddRange(args.Items);
            try
            {
                cachingEntityMemberRequestFactory.Cache.ItemsRemoved += cacheRemoval;

                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    // Delete the relationship
                    relationshipType.Delete();
                    ctx.CommitTransaction();
                }

                // Ensure the cache has been invalidated
                Assert.That(itemsRemoved,
                    Contains.Item( entityType2.Id ));
                Assert.That(((CacheInvalidator<long, EntityMemberRequest>)cachingEntityMemberRequestFactory.CacheInvalidator)
                        .RelationshipTypeToCacheKey
                        .GetValues(relationshipType.Id),
                    Is.Not.Contains(entityType2));
            }
            finally
            {
                cachingEntityMemberRequestFactory.Cache.ItemsRemoved -= cacheRemoval;
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("core:securesTo", 1)]
        [TestCase("core:securesFrom", 0)]
        public void Invalidation_ChangeSecuresFlagsTrueToFalse(string fieldAlias, int expectedInvalidatedEntityTypeIndex)
        {
            CachingEntityMemberRequestFactory cachingEntityMemberRequestFactory;
            EntityType[] entityTypes;
            Relationship relationshipType;
            CacheInvalidator<long, EntityMemberRequest> cacheInvalidator;

            var readPerm = new [ ] { Permissions.Read };

            // Setup the entities and relationships
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                entityTypes = new[]
                {
                    new EntityType { Name = "Entity Type 1"}, 
                    new EntityType { Name = "Entity Type 2"}
                };

                foreach (EntityType entityType in entityTypes)
                {
                    entityType.Save();
                }

                relationshipType = new Relationship
                {
                    FromType = entityTypes[0],
                    ToType = entityTypes[1],
                    SecuresTo = false,
                    SecuresFrom = false
                };
                relationshipType.SetField(fieldAlias, true);
                relationshipType.Save();

                ctx.CommitTransaction();
            }

            // Build EMRs for both entity types
            cachingEntityMemberRequestFactory =
                Factory.Current.Resolve<CachingEntityMemberRequestFactory>();
            cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityTypes[0], readPerm );
            cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityTypes[1], readPerm );

            cacheInvalidator =
                (CacheInvalidator<long, EntityMemberRequest>) cachingEntityMemberRequestFactory.CacheInvalidator;

            using(CacheMonitor<long, EntityMemberRequest> cacheMonitor = 
                new CacheMonitor<long, EntityMemberRequest>(cachingEntityMemberRequestFactory.Cache))
            {
                // Change the flag
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    relationshipType.SecuresTo = false;
                    relationshipType.Save();
                    ctx.CommitTransaction();
                }
                // Ensure the cache has been invalidated
                // Ensure the cache has been invalidated
                foreach (var et in entityTypes.Select(et => et.Id))
                    Assert.That(cacheMonitor.ItemsRemoved,
                        Contains.Item(et),
                        "Not removed");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("core:securesTo")]
        [TestCase("core:securesFrom")]
        public void Invalidation_ChangeSecuresFlagsFalseToTrue(string fieldAlias)
        {
            CachingEntityMemberRequestFactory cachingEntityMemberRequestFactory;
            EntityType[] entityTypes;
            Relationship relationshipType;
            CacheInvalidator<long, EntityMemberRequest> cacheInvalidator;

            var readPerm = new [ ] { Permissions.Read };

            // Setup the entities and relationships
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                entityTypes = new[]
                {
                    new EntityType { Name = "Entity Type 1"}, 
                    new EntityType { Name = "Entity Type 2"}
                };
                foreach (EntityType entityType in entityTypes)
                {
                    entityType.Save();
                }

                relationshipType = new Relationship
                {
                    FromType = entityTypes[0],
                    ToType = entityTypes[1],
                    SecuresTo = false,
                    SecuresFrom = false
                };
                relationshipType.Save();
                ctx.CommitTransaction();
            }

            // Build EMRs for both entity types
            cachingEntityMemberRequestFactory =
                Factory.Current.Resolve<CachingEntityMemberRequestFactory>();
            cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityTypes[0], readPerm );
            cachingEntityMemberRequestFactory.BuildEntityMemberRequest(entityTypes[1], readPerm );

            cacheInvalidator =
                (CacheInvalidator<long, EntityMemberRequest>)cachingEntityMemberRequestFactory.CacheInvalidator;


            using (CacheMonitor<long, EntityMemberRequest> cacheMonitor =
                new CacheMonitor<long, EntityMemberRequest>(cachingEntityMemberRequestFactory.Cache))
            {
                // Change the flag
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    relationshipType.SetField(fieldAlias, true);
                    relationshipType.Save();
                    ctx.CommitTransaction();
                }
                // Ensure the cache has been invalidated
                foreach (var et in entityTypes.Select(et => et.Id))
                    Assert.That(cacheMonitor.ItemsRemoved,
                        Contains.Item(et),
                        "Not removed");
            }
        }
    }
}
