// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class SecuresFlagEntityAccessControlCheckerTests
    {
        [Test]
        public void Test_CheckAccess_NullEntities()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccess(null, new EntityRef[0], new EntityRef(1)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        public void Test_CheckAccess_NullPermissions()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccess(new EntityRef[0], null, new EntityRef(1)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CheckAccess_SingleEntityNoRelationships()
        {
            SecuresFlagEntityAccessControlChecker securesFlagEntityAccessControlChecker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> entityAccessControlCheckerMock;
            EntityType entityType;
            IEntity entity;
            IDictionary<long, bool> checkResult;
            
            mockRepository = new MockRepository(MockBehavior.Strict);

            // Return false for any access control checks
            entityAccessControlCheckerMock = mockRepository.Create<IEntityAccessControlChecker>();
            entityAccessControlCheckerMock
                .Setup(eacc =>eacc.CheckAccess(It.IsAny<IList<EntityRef>>(), It.IsAny<IList<EntityRef>>(), It.IsAny<EntityRef>()))
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>((entities, permissions, user) => entities.ToDictionary(er => er.Id, er => false));

            entityType = new EntityType();
            entityType.Save();

            entity = Entity.Create(entityType);
            entity.Save();

            securesFlagEntityAccessControlChecker = new SecuresFlagEntityAccessControlChecker(
                entityAccessControlCheckerMock.Object, 
                new EntityMemberRequestFactory(), 
                new EntityTypeRepository());
            checkResult = securesFlagEntityAccessControlChecker.CheckAccess(
                new [] { new EntityRef(entity)  }, 
                new [] { Permissions.Read }, 
                RequestContext.GetContext().Identity.Id);

            Assert.That(checkResult, Has.Exactly(1).EqualTo(new KeyValuePair<long, bool>(entity.Id, false)));
            entityAccessControlCheckerMock.Verify(
                eacc => eacc.CheckAccess(It.IsAny<IList<EntityRef>>(), It.IsAny<IList<EntityRef>>(), It.IsAny<EntityRef>()), 
                Times.Exactly(1));
            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CheckAccess_SingleEntityOneRelationshipNoRelatedEntity()
        {
            SecuresFlagEntityAccessControlChecker securesFlagEntityAccessControlChecker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> entityAccessControlCheckerMock;
            EntityType entityType;
            EntityType relatedEntityType;
            IEntity entity;
            IDictionary<long, bool> checkResult;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> checkAccess;

            mockRepository = new MockRepository(MockBehavior.Strict);

            entityType = new EntityType();
            entityType.Save();

            relatedEntityType = new EntityType();
            relatedEntityType.Relationships.Add(new Relationship
            {
                ToType = entityType,
                SecuresTo = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
            });
            relatedEntityType.Save();

            entity = Entity.Create(entityType);
            entity.Save();

            // Return false for any access control checks
            checkAccess = eacc => eacc.CheckAccess(
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(new EntityRef(entity), EntityRefComparer.Instance)),
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(Permissions.Read, EntityRefComparer.Instance)),
                It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id)));
            entityAccessControlCheckerMock = mockRepository.Create<IEntityAccessControlChecker>();
            entityAccessControlCheckerMock
                .Setup(checkAccess)
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>((entities, permissions, user) => entities.ToDictionary(er => er.Id, er => false));

            securesFlagEntityAccessControlChecker = new SecuresFlagEntityAccessControlChecker(
                entityAccessControlCheckerMock.Object,
                new EntityMemberRequestFactory(),
                new EntityTypeRepository());
            checkResult = securesFlagEntityAccessControlChecker.CheckAccess(
                new[] { new EntityRef(entity) },
                new[] { Permissions.Read },
                RequestContext.GetContext().Identity.Id);

            Assert.That(checkResult, Has.Exactly(1).EqualTo(new KeyValuePair<long, bool>(entity.Id, false)));
            entityAccessControlCheckerMock.Verify(
                checkAccess, 
                Times.Exactly(1));
            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CheckAccess_TwoEntitiesOneToRelationshipWithRelatedEntity()
        {
            SecuresFlagEntityAccessControlChecker securesFlagEntityAccessControlChecker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> entityAccessControlCheckerMock;
            EntityType entityType;
            EntityType relatedEntityType;
            IEntity entity;
            IEntity relatedEntity;
            IDictionary<long, bool> checkResult;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> entityCheckAccess;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> relatedEntityCheckAccess;
            IEntityRelationshipCollection<IEntity> relationships;
            Relationship relationship;

            mockRepository = new MockRepository(MockBehavior.Strict);

            entityType = new EntityType();
            entityType.Save();

            relatedEntityType = new EntityType();
            relatedEntityType.Save();

            relationship = new Relationship
            {
                FromType = relatedEntityType,
                ToType = entityType,
                SecuresTo = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
            };
            relationship.Save();
            
            entity = Entity.Create(entityType);
            entity.Save();

            relatedEntity = Entity.Create(relatedEntityType);
            relatedEntity.Save();

            relationships = relatedEntity.GetRelationships(relationship);
            relationships.Add(entity);
            relatedEntity.SetRelationships(relationship, relationships);
            relatedEntity.Save();
            Assert.That(relatedEntity.GetRelationships(relationship, Direction.Forward).Entities.Select(e => e.Id), 
                Contains.Item(entity.Id));
            Assert.That(entity.GetRelationships(relationship, Direction.Reverse).Entities.Select(e => e.Id),
                Contains.Item(relatedEntity.Id));

            // Return false for any access control checks
            entityCheckAccess = eacc => eacc.CheckAccess(
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(new EntityRef(entity), EntityRefComparer.Instance)),
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(Permissions.Read, EntityRefComparer.Instance)),
                It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id)));
            relatedEntityCheckAccess = eacc => eacc.CheckAccess(
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(new EntityRef(relatedEntity), EntityRefComparer.Instance)),
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(Permissions.Read, EntityRefComparer.Instance)),
                It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id)));
            entityAccessControlCheckerMock = mockRepository.Create<IEntityAccessControlChecker>();
            entityAccessControlCheckerMock
                .Setup(entityCheckAccess)
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>((entities, permissions, user) => entities.ToDictionary(er => er.Id, er => false));
            entityAccessControlCheckerMock
                .Setup(relatedEntityCheckAccess)
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>((entities, permissions, user) => entities.ToDictionary(er => er.Id, er => false));

            securesFlagEntityAccessControlChecker = new SecuresFlagEntityAccessControlChecker(
                entityAccessControlCheckerMock.Object,
                new EntityMemberRequestFactory(),
                new EntityTypeRepository());
            checkResult = securesFlagEntityAccessControlChecker.CheckAccess(
                new[] { new EntityRef(entity) },
                new[] { Permissions.Read },
                RequestContext.GetContext().Identity.Id);

            Assert.That(checkResult, Has.Exactly(1).EqualTo(new KeyValuePair<long, bool>(entity.Id, false)));
            entityAccessControlCheckerMock.Verify(entityCheckAccess, Times.Exactly(1));
            entityAccessControlCheckerMock.Verify(relatedEntityCheckAccess, Times.Exactly(1));
            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_CheckAccess_TwoEntitiesOneFromRelationshipWithRelatedEntity()
        {
            SecuresFlagEntityAccessControlChecker securesFlagEntityAccessControlChecker;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> entityAccessControlCheckerMock;
            EntityType entityType;
            EntityType relatedEntityType;
            IEntity entity;
            IEntity relatedEntity;
            IDictionary<long, bool> checkResult;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> entityCheckAccess;
            Expression<Func<IEntityAccessControlChecker, IDictionary<long, bool>>> relatedEntityCheckAccess;
            IEntityRelationshipCollection<IEntity> relationships;
            Relationship relationship;

            mockRepository = new MockRepository(MockBehavior.Strict);

            entityType = new EntityType();
            entityType.Save();

            relatedEntityType = new EntityType();
            relatedEntityType.Save();

            relationship = new Relationship
            {
                FromType = entityType,
                ToType = relatedEntityType,
                SecuresFrom = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
            };
            relationship.Save();

            entity = Entity.Create(entityType);
            entity.Save();

            relatedEntity = Entity.Create(relatedEntityType);
            relatedEntity.Save();

            relationships = entity.GetRelationships(relationship);
            relationships.Add(relatedEntity);
            entity.SetRelationships(relationship, relationships);
            entity.Save();
            Assert.That(relatedEntity.GetRelationships(relationship, Direction.Reverse).Entities.Select(e => e.Id),
                Contains.Item(entity.Id));
            Assert.That(entity.GetRelationships(relationship, Direction.Forward).Entities.Select(e => e.Id),
                Contains.Item(relatedEntity.Id));

            // Return false for any access control checks
            entityCheckAccess = eacc => eacc.CheckAccess(
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(new EntityRef(entity), EntityRefComparer.Instance)),
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(Permissions.Read, EntityRefComparer.Instance)),
                It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id)));
            relatedEntityCheckAccess = eacc => eacc.CheckAccess(
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(new EntityRef(relatedEntity), EntityRefComparer.Instance)),
                It.Is<IList<EntityRef>>(x => x.Count == 1 && x.Contains(Permissions.Read, EntityRefComparer.Instance)),
                It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id)));
            entityAccessControlCheckerMock = mockRepository.Create<IEntityAccessControlChecker>();
            entityAccessControlCheckerMock
                .Setup(entityCheckAccess)
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>((entities, permissions, user) => entities.ToDictionary(er => er.Id, er => false));
            entityAccessControlCheckerMock
                .Setup(relatedEntityCheckAccess)
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>((entities, permissions, user) => entities.ToDictionary(er => er.Id, er => false));

            securesFlagEntityAccessControlChecker = new SecuresFlagEntityAccessControlChecker(
                entityAccessControlCheckerMock.Object,
                new EntityMemberRequestFactory(),
                new EntityTypeRepository());
            checkResult = securesFlagEntityAccessControlChecker.CheckAccess(
                new[] { new EntityRef(entity) },
                new[] { Permissions.Read },
                RequestContext.GetContext().Identity.Id);

            Assert.That(checkResult, Has.Exactly(1).EqualTo(new KeyValuePair<long, bool>(entity.Id, false)));
            entityAccessControlCheckerMock.Verify(entityCheckAccess, Times.Exactly(1));
            entityAccessControlCheckerMock.Verify(relatedEntityCheckAccess, Times.Exactly(1));
            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(Direction.Forward, "rootEntityType", "", "read")]
        [TestCase(Direction.Forward, "parentEntityType1", "rootEntityType", "read")]
        [TestCase(Direction.Forward, "childEntityType11", "rootEntityType,parentEntityType1", "read")]
        [TestCase(Direction.Forward, "parentEntityType2", "rootEntityType", "read")]
        [TestCase(Direction.Forward, "childEntityType21", "rootEntityType,parentEntityType2", "read")]
        [TestCase(Direction.Forward, "grandChildEntityType211", "rootEntityType,parentEntityType2,childEntityType21", "read")]
        [TestCase(Direction.Reverse, "rootEntityType", "", "read")]
        [TestCase(Direction.Reverse, "parentEntityType1", "rootEntityType", "read")]
        [TestCase(Direction.Reverse, "childEntityType11", "rootEntityType,parentEntityType1", "read")]
        [TestCase(Direction.Reverse, "parentEntityType2", "rootEntityType", "read")]
        [TestCase(Direction.Reverse, "childEntityType21", "rootEntityType,parentEntityType2", "read")]
        [TestCase(Direction.Reverse, "grandChildEntityType211", "rootEntityType,parentEntityType2,childEntityType21", "read")]
        [TestCase(Direction.Forward, "parentEntityType1", "rootEntityType", "read,modify")]
        [TestCase(Direction.Forward, "parentEntityType1", "rootEntityType", "read,delete")]
        [TestCase(Direction.Forward, "parentEntityType1", "rootEntityType", "read,modify,delete")]
        public void Test_CheckAccess_TreeSecuredByRoot_NonCreate(Direction direction, string entityType, string expectedEntities, string permissionAliases)
        {
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationshipTypes;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> entityAccessControlCheckerMock;
            List<IList<string>> actualCheckedEntities;
            Dictionary<string, IEntity> entities;
            IEntityRelationshipCollection<IEntity> entityRealtionships;
            SecuresFlagEntityAccessControlChecker securesFlagEntityAccessControlChecker;
            IDictionary<long, bool> result;

            // Build the following entity/relationship graph:
            //
            // rootEntityType
            //   |
            //   -> parentEntityType1
            //   |    |
            //   |    -> childEntityType11
            //   |
            //   -> parentEntityType2
            //        |
            //        -> childEntityType21
            //        |     |
            //        |     -> grandChildEntityType211
            //        |
            //        -> childEntityType21

            const string rootEntityType = "rootEntityType";
            const string parentEntityType1 = "parentEntityType1";
            const string childEntityType11 = "childEntityType11";
            const string parentEntityType2 = "parentEntityType2";
            const string childEntityType21 = "childEntityType21";
            const string grandChildEntityType211 = "grandChildEntityType211";
            const string childEntityType22 = "childEntityType22";
            const string rootToParent1 = "rootToParent1";
            const string parent1ToChild11 = "parent1ToChild11";
            const string rootToParent2 = "rootToParent2";
            const string parent2ToChild21 = "parent2ToChild21";
            const string child21ToGrandChild211 = "child21ToGrandChild211";
            const string parent2ToChild22 = "parent2ToChild22";
            string[] types = 
            {
                rootEntityType,
                parentEntityType1,
                childEntityType11,
                parentEntityType2,
                childEntityType21,
                grandChildEntityType211,
                childEntityType22
            };
            List<Tuple<string, string, string>> relationships = new List<Tuple<string, string, string>>
            {
                new Tuple<string, string, string>(rootEntityType, rootToParent1, parentEntityType1),
                new Tuple<string, string, string>(parentEntityType1, parent1ToChild11, childEntityType11),
                new Tuple<string, string, string>(rootEntityType, rootToParent2, parentEntityType2),
                new Tuple<string, string, string>(parentEntityType2, parent2ToChild21, childEntityType21),
                new Tuple<string, string, string>(childEntityType21, child21ToGrandChild211, grandChildEntityType211),
                new Tuple<string, string, string>(parentEntityType2, parent2ToChild22, childEntityType22)
            };

            // Speed up tests
            using (new SecurityBypassContext())
            {
                // Create the types and a instance of each
                entityTypes = new Dictionary<string, EntityType>();
                entities = new Dictionary<string, IEntity>();
                foreach (string type in types)
                {
                    entityTypes[type] = new EntityType();
                    entityTypes[type].Save();

                    entities[type] = Entity.Create(entityTypes[type]);
                    entities[type].Save();
                }

                // Create relationships and assign the to types for each
                relationshipTypes = new Dictionary<string, Relationship>();
                foreach (Tuple<string, string, string> relationship in relationships)
                {
                    if (direction == Direction.Forward)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            FromType = entityTypes[relationship.Item1],
                            ToType = entityTypes[relationship.Item3],
                            SecuresTo = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRealtionships =
                            entities[relationship.Item1].GetRelationships(relationshipTypes[relationship.Item2]);
                        entityRealtionships.Add(entities[relationship.Item3]);
                        entities[relationship.Item1].SetRelationships(relationshipTypes[relationship.Item2],
                            entityRealtionships);
                        entities[relationship.Item1].Save();
                    }
                    else if (direction == Direction.Reverse)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            FromType = entityTypes[relationship.Item3],
                            ToType = entityTypes[relationship.Item1],
                            SecuresFrom = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRealtionships =
                            entities[relationship.Item3].GetRelationships(relationshipTypes[relationship.Item2]);
                        entityRealtionships.Add(entities[relationship.Item1]);
                        entities[relationship.Item3].SetRelationships(relationshipTypes[relationship.Item2],
                            entityRealtionships);
                        entities[relationship.Item3].Save();
                    }
                    else
                    {
                        Assert.Fail("Unknown direction");
                    }
                }
            }

            actualCheckedEntities = new List<IList<string>>();

            mockRepository = new MockRepository(MockBehavior.Strict);
            entityAccessControlCheckerMock = mockRepository.Create<IEntityAccessControlChecker>();
            entityAccessControlCheckerMock
                .Setup(
                    eacc => eacc.CheckAccess(
                        It.IsAny<IList<EntityRef>>(),
                        It.Is<IList<EntityRef>>(ps => ps.SequenceEqual(permissionAliases.Select(s => new EntityRef(s)), EntityRefComparer.Instance)),
                        It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id))))
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>(
                    (entitiesToCheck, permissions, user) =>
                    {
                        // Lookup the key so we can match it to the test method arguments
                        actualCheckedEntities.Add(
                            entitiesToCheck
                                .Select(er => entities.First(kvp => EntityRefComparer.Instance.Equals(new EntityRef(kvp.Value), er)).Key)
                                .ToList());

                        // Grant access to every entity except for entityType
                        return entitiesToCheck.ToDictionary(
                            er => er.Id, 
                            er => entities[entityType].Id != er.Id);
                    });

            securesFlagEntityAccessControlChecker = new SecuresFlagEntityAccessControlChecker(
                entityAccessControlCheckerMock.Object,
                new EntityMemberRequestFactory(),
                new EntityTypeRepository());
            result = securesFlagEntityAccessControlChecker.CheckAccess(
                new [] { new EntityRef(entities[entityType]) },
                permissionAliases.Select(s => new EntityRef(s)).ToList(),
                RequestContext.GetContext().Identity.Id);

            if (!string.IsNullOrWhiteSpace(expectedEntities))
            {
                Assert.That(actualCheckedEntities, Has.Count.EqualTo(2));
                Assert.That(actualCheckedEntities[0], Is.EquivalentTo(new[] { entityType }));
                Assert.That(actualCheckedEntities[1],
                    Is.EquivalentTo(expectedEntities.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)));
                Assert.That(result,
                    Is.EquivalentTo(new[] { new KeyValuePair<long, bool>(entities[entityType].Id, true) }));
            }
            else
            {
                Assert.That(actualCheckedEntities, Has.Count.EqualTo(1));
                Assert.That(actualCheckedEntities[0], Is.EquivalentTo(new[] { entityType }));
                Assert.That(result,
                    Is.EquivalentTo(new[] { new KeyValuePair<long, bool>(entities[entityType].Id, false) }));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(Direction.Forward, "rootEntityType", "")]
        [TestCase(Direction.Forward, "parentEntityType1", "rootEntityType")]
        [TestCase(Direction.Forward, "childEntityType11", "rootEntityType,parentEntityType1")]
        [TestCase(Direction.Forward, "parentEntityType2", "rootEntityType")]
        [TestCase(Direction.Forward, "childEntityType21", "rootEntityType,parentEntityType2")]
        [TestCase(Direction.Forward, "grandChildEntityType211", "rootEntityType,parentEntityType2,childEntityType21")]
        [TestCase(Direction.Reverse, "rootEntityType", "")]
        [TestCase(Direction.Reverse, "parentEntityType1", "rootEntityType")]
        [TestCase(Direction.Reverse, "childEntityType11", "rootEntityType,parentEntityType1")]
        [TestCase(Direction.Reverse, "parentEntityType2", "rootEntityType")]
        [TestCase(Direction.Reverse, "childEntityType21", "rootEntityType,parentEntityType2")]
        [TestCase(Direction.Reverse, "grandChildEntityType211", "rootEntityType,parentEntityType2,childEntityType21")]
        public void Test_CheckAccess_TreeSecuredByRoot_Create(Direction direction, string entityType, string expectedEntities)
        {
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationshipTypes;
            MockRepository mockRepository;
            Mock<IEntityAccessControlChecker> entityAccessControlCheckerMock;
            List<IList<long>> actualCheckedEntities;
            List<IList<string>> actualCanCreateTypes;
            Dictionary<string, IEntity> entities;
            SecuresFlagEntityAccessControlChecker securesFlagEntityAccessControlChecker;
            EntityTypeRepository entityTypeRepository;
            IDictionary<long, bool> result;

            // Build the following entity/relationship graph:
            //
            // rootEntityType
            //   |
            //   -> parentEntityType1
            //   |    |
            //   |    -> childEntityType11
            //   |
            //   -> parentEntityType2
            //        |
            //        -> childEntityType21
            //        |     |
            //        |     -> grandChildEntityType211
            //        |
            //        -> childEntityType21

            const string rootEntityType = "rootEntityType";
            const string parentEntityType1 = "parentEntityType1";
            const string childEntityType11 = "childEntityType11";
            const string parentEntityType2 = "parentEntityType2";
            const string childEntityType21 = "childEntityType21";
            const string grandChildEntityType211 = "grandChildEntityType211";
            const string childEntityType22 = "childEntityType22";
            const string rootToParent1 = "rootToParent1";
            const string parent1ToChild11 = "parent1ToChild11";
            const string rootToParent2 = "rootToParent2";
            const string parent2ToChild21 = "parent2ToChild21";
            const string child21ToGrandChild211 = "child21ToGrandChild211";
            const string parent2ToChild22 = "parent2ToChild22";
            string[] types = 
            {
                rootEntityType,
                parentEntityType1,
                childEntityType11,
                parentEntityType2,
                childEntityType21,
                grandChildEntityType211,
                childEntityType22
            };
            List<Tuple<string, string, string>> relationships = new List<Tuple<string, string, string>>
            {
                new Tuple<string, string, string>(rootEntityType, rootToParent1, parentEntityType1),
                new Tuple<string, string, string>(parentEntityType1, parent1ToChild11, childEntityType11),
                new Tuple<string, string, string>(rootEntityType, rootToParent2, parentEntityType2),
                new Tuple<string, string, string>(parentEntityType2, parent2ToChild21, childEntityType21),
                new Tuple<string, string, string>(childEntityType21, child21ToGrandChild211, grandChildEntityType211),
                new Tuple<string, string, string>(parentEntityType2, parent2ToChild22, childEntityType22)
            };

            // Speed up tests
            using (new SecurityBypassContext())
            {
                // Create the types and a instance of each
                entityTypes = new Dictionary<string, EntityType>();
                entities = new Dictionary<string, IEntity>();
                foreach (string type in types)
                {
                    entityTypes[type] = new EntityType();
                    entityTypes[type].Save();

                    entities[type] = Entity.Create(entityTypes[type]);

                    // Do not save - force a create check due to a temporary ID
                    // entities[type].Save();
                }

                // Create relationships and assign the to types for each
                relationshipTypes = new Dictionary<string, Relationship>();
                foreach (Tuple<string, string, string> relationship in relationships)
                {
                    if (direction == Direction.Forward)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            FromType = entityTypes[relationship.Item1],
                            ToType = entityTypes[relationship.Item3],
                            SecuresTo = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();
                    }
                    else if (direction == Direction.Reverse)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            FromType = entityTypes[relationship.Item3],
                            ToType = entityTypes[relationship.Item1],
                            SecuresFrom = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();
                    }
                    else
                    {
                        Assert.Fail("Unknown direction");
                    }
                }
            }

            entityTypeRepository = new EntityTypeRepository();
            actualCheckedEntities = new List<IList<long>>();
            actualCanCreateTypes = new List<IList<string>>();

            mockRepository = new MockRepository(MockBehavior.Strict);
            entityAccessControlCheckerMock = mockRepository.Create<IEntityAccessControlChecker>();
            entityAccessControlCheckerMock
                .Setup(
                    eacc => eacc.CheckTypeAccess(
                        It.IsAny<IList<EntityType>>(),
                        It.Is<EntityRef>( er => EntityRefComparer.Instance.Equals( er, Permissions.Create.Id ) ),
                        It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id))))
                .Returns<IList<EntityType>, EntityRef>(
                    (ets, user) =>
                    {
                        actualCanCreateTypes.Add(ets.Select(et => et.Name).ToList());
                        return ets.Distinct(new EntityEqualityComparer()).ToDictionary(et => et.Id, et => et.Id == entityTypes[entityType].Id);
                    });
            entityAccessControlCheckerMock
                .Setup(
                    eacc => eacc.CheckAccess(
                        It.IsAny<IList<EntityRef>>(),
                        It.IsAny<IList<EntityRef>>(),
                        It.Is<EntityRef>(er => EntityRefComparer.Instance.Equals(er, RequestContext.GetContext().Identity.Id))))
                .Returns<IList<EntityRef>, IList<EntityRef>, EntityRef>(
                    (es, permissions, user) =>
                    {
                        actualCheckedEntities.Add(es.Select(e => e.Id).ToList());
                        return es.Distinct(EntityRefComparer.Instance).ToDictionary(er => er.Id, er => er.Id == entities[entityType].Id);
                    });

            securesFlagEntityAccessControlChecker = new SecuresFlagEntityAccessControlChecker(
                entityAccessControlCheckerMock.Object,
                new EntityMemberRequestFactory(),
                entityTypeRepository);
            result = securesFlagEntityAccessControlChecker.CheckAccess(
                new[] { new EntityRef(entities[entityType]) },
                new[] { Permissions.Create },
                RequestContext.GetContext().Identity.Id);

            if (!string.IsNullOrWhiteSpace(expectedEntities))
            {
                Assert.That(actualCanCreateTypes, Has.Count.EqualTo(0));
                Assert.That(actualCheckedEntities, Has.Count.EqualTo(1));
                Assert.That(actualCheckedEntities[0], Is.EquivalentTo(new[] { entities[entityType].Id }));
                Assert.That(result,
                    Is.EquivalentTo(new[] { new KeyValuePair<long, bool>(entities[entityType].Id, true) }));
            }
            else
            {
                Assert.That(actualCanCreateTypes, Has.Count.EqualTo(0));
                Assert.That(actualCheckedEntities, Has.Count.EqualTo(1));
                Assert.That(actualCheckedEntities[0], Is.EquivalentTo(new[] { entities[entityType].Id }));
                Assert.That(result,
                    Is.EquivalentTo(new[] { new KeyValuePair<long, bool>(entities[entityType].Id, true) }));
            }
        }

        [Test]
        public void Test_CheckAccessControlByRelationship_NullPermissions()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccessControlByRelationship(null, new EntityRef(1), new HashSet<EntityRef>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));            
        }

        [Test]
        public void Test_CheckAccessControlByRelationship_PermissionsContainsNull()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccessControlByRelationship(new List<EntityRef> { null }, new EntityRef(1), new HashSet<EntityRef>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        public void Test_CheckAccessControlByRelationship_NullUser()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccessControlByRelationship(new List<EntityRef>(), null, new HashSet<EntityRef>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        public void Test_CheckAccessControlByRelationship_NullEntitiesToCheck()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccessControlByRelationship(new List<EntityRef>(), new EntityRef(1), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entitiesToCheck"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccessControlByRelationshipType_NullPermission( )
        {
            Assert.That( ( ) => new SecuresFlagEntityAccessControlChecker( ).CheckAccessControlByRelationshipType( null, new EntityRef( ), new HashSet<EntityRef>( ) ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "permission" ) );
        }

        [Test]
        public void Test_CheckAccessControlByRelationshipType_NullUser()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccessControlByRelationshipType( new EntityRef( ), null, new HashSet<EntityRef>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        public void Test_CheckAccessControlByRelationshipType_NullEntitiesToCheck()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckAccessControlByRelationshipType( new EntityRef( ), new EntityRef(1), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entitiesToCheck"));
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        // Sanity tests
        [TestCase(null, "rootEntityType", false)]
        [TestCase(null, "parentEntityType1", false)]
        [TestCase(null, "childEntityType11", false)]
        // Basic tests
        [TestCase("rootEntityType", "rootEntityType", true)]
        [TestCase("parentEntityType1", "parentEntityType1", true)]
        [TestCase("childEntityType11", "childEntityType11", true)]
        // Follow the hierarchy down
        [TestCase("rootEntityType", "childEntityType11", true)]
        [TestCase("rootEntityType", "parentEntityType1", true)]
        [TestCase("parentEntityType1", "childEntityType11", true)]
        [TestCase("childEntityType11", "childEntityType11", true)]
        // Negative cases
        [TestCase("childEntityType11", "rootEntityType", false)]
        [TestCase("childEntityType11", "parentEntityType1", false)]
        [TestCase("parentEntityType1", "rootEntityType", false)]
        public void Test_CheckAccess_BasicCreate(string grantCreateOnType, string testType, bool expectedResult)
        {
            UserAccount userAccount;
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationshipTypes;

            // Build the following entity/relationship graph:
            //
            // rootEntityType
            //   |
            //   -> parentEntityType1
            //        ^
            //        -- childEntityType11
            //
            // The basic idea is that when create access is 
            // granted to either root or parent, the user should
            // be able to create a child.

            const string rootEntityType = "rootEntityType";
            const string parentEntityType1 = "parentEntityType1";
            const string childEntityType11 = "childEntityType11";
            const string rootToParent1 = "rootToParent1";
            const string parent1ToChild11 = "parent1ToChild11";
            string[] types = 
            {
                rootEntityType,
                parentEntityType1,
                childEntityType11
            };
            List<Tuple<string, string, string, Direction>> relationships = new List<Tuple<string, string, string, Direction>>
            {
                new Tuple<string, string, string, Direction>(rootEntityType, rootToParent1, parentEntityType1, Direction.Forward),
                new Tuple<string, string, string, Direction>(parentEntityType1, parent1ToChild11, childEntityType11, Direction.Reverse)
            };

            // Speed up tests
            using (new SecurityBypassContext())
            {
                // Create the types and a instance of each
                entityTypes = new Dictionary<string, EntityType>();
                foreach (string type in types)
                {
                    entityTypes[type] = new EntityType
                    {
                        Name = type
                    };
                    entityTypes[type].Inherits.Add(UserResource.UserResource_Type);
                    entityTypes[type].Save();
                }

                // Create relationships and assign the to types for each
                relationshipTypes = new Dictionary<string, Relationship>();
                foreach (Tuple<string, string, string, Direction> relationship in relationships)
                {
                    if (relationship.Item4 == Direction.Forward)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item1],
                            ToType = entityTypes[relationship.Item3],
                            SecuresTo = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();
                    }
                    else if (relationship.Item4 == Direction.Reverse)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item3],
                            ToType = entityTypes[relationship.Item1],
                            SecuresFrom = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();
                    }
                    else
                    {
                        Assert.Fail("Unknown direction");
                    }
                }
            }

            userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            if (!string.IsNullOrWhiteSpace(grantCreateOnType))
            {
                new AccessRuleFactory().AddAllowCreate(
                    userAccount.As<Subject>(),
                    entityTypes[grantCreateOnType].As<SecurableEntity>());
            }

            using (new SetUser(userAccount))
            {
                Assert.That(
                    Factory.EntityAccessControlService.CanCreate(entityTypes[testType]),
                    Is.EqualTo(expectedResult));
            }
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        // Sanity tests
        [TestCase(null, "rootEntityType", false)]
        [TestCase(null, "parentEntityType1", false)]
        [TestCase(null, "childEntityType11", false)]
        // Basic tests
        [TestCase("rootEntityType", "rootEntityType", true)]
        [TestCase("parentEntityType1", "parentEntityType1", true)]
        [TestCase("childEntityType11", "childEntityType11", true)]
        // Follow the hierarchy down
        [TestCase("rootEntityType", "childEntityType11", true)]
        [TestCase("rootEntityType", "parentEntityType1", true)]
        [TestCase("parentEntityType1", "childEntityType11", true)]
        [TestCase("childEntityType11", "childEntityType11", true)]
        // Negative cases
        [TestCase("childEntityType11", "rootEntityType", false)]
        [TestCase("childEntityType11", "parentEntityType1", false)]
        [TestCase("parentEntityType1", "rootEntityType", false)]
        public void Test_CheckAccess_BasicRead(string grantReadOnType, string testType, bool expectedResult)
        {
            UserAccount userAccount;
            Dictionary<string, IEntity> entities;
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationshipTypes;
            IEntityRelationshipCollection<IEntity> entityRelationships;

            // Build the following entity/relationship graph:
            //
            // rootEntityType
            //   |
            //   -> parentEntityType1
            //        ^
            //        -- childEntityType11
            //
            // The basic idea is that when read access is 
            // granted to either root or parent, the user should
            // be able to read a child.

            const string rootEntityType = "rootEntityType";
            const string parentEntityType1 = "parentEntityType1";
            const string childEntityType11 = "childEntityType11";
            const string rootToParent1 = "rootToParent1";
            const string parent1ToChild11 = "parent1ToChild11";
            string[] types = 
            {
                rootEntityType,
                parentEntityType1,
                childEntityType11
            };
            List<Tuple<string, string, string, Direction>> relationships = new List<Tuple<string, string, string, Direction>>
            {
                new Tuple<string, string, string, Direction>(rootEntityType, rootToParent1, parentEntityType1, Direction.Forward),
                new Tuple<string, string, string, Direction>(parentEntityType1, parent1ToChild11, childEntityType11, Direction.Reverse)
            };

            // Speed up tests
            using (new SecurityBypassContext())
            {
                // Create the types and a instance of each
                entityTypes = new Dictionary<string, EntityType>();
                entities = new Dictionary<string, IEntity>();
                foreach (string type in types)
                {
                    entityTypes[type] = new EntityType
                    {
                        Name = type
                    };
                    entityTypes[type].Inherits.Add(UserResource.UserResource_Type);
                    entityTypes[type].Save();

                    entities[type] = Entity.Create(entityTypes[type]);
                    entities[type].Save();
                }

                // Create relationships and assign the to types for each
                relationshipTypes = new Dictionary<string, Relationship>();
                foreach (Tuple<string, string, string, Direction> relationship in relationships)
                {
                    if (relationship.Item4 == Direction.Forward)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item1],
                            ToType = entityTypes[relationship.Item3],
                            SecuresTo = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRelationships = entities[relationship.Item1].GetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]), 
                            Direction.Forward);
                        entityRelationships.Add(entities[relationship.Item3]);
                        entities[relationship.Item1].SetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            entityRelationships);
                        entities[relationship.Item1].Save();
                    }
                    else if (relationship.Item4 == Direction.Reverse)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item3],
                            ToType = entityTypes[relationship.Item1],
                            SecuresFrom = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRelationships = entities[relationship.Item3].GetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            Direction.Reverse);
                        entityRelationships.Add(entities[relationship.Item1]);
                        entities[relationship.Item3].SetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            entityRelationships);
                        entities[relationship.Item3].Save();
                    }
                    else
                    {
                        Assert.Fail("Unknown direction");
                    }
                }
            }

            userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            if (!string.IsNullOrWhiteSpace(grantReadOnType))
            {
                new AccessRuleFactory().AddAllowReadQuery(
                    userAccount.As<Subject>(),
                    entityTypes[grantReadOnType].As<SecurableEntity>(),
                    TestQueries.Entities().ToReport());
            }

            using (new ForceSecurityTraceContext(entities[testType].Id))
            using (new SetUser(userAccount))
            {
                //Console.WriteLine("Types:");
                //foreach (string type in types)
                //{
                //    Console.WriteLine("{0}: {1}", type, entities[type].Id);
                //}
                //Console.WriteLine("Relationships:");
                //foreach (KeyValuePair<string, Relationship> relationshipKeyValuePair in relationshipTypes)
                //{
                //    Console.WriteLine("{0}: {1}", 
                //        relationshipKeyValuePair.Key,
                //        relationshipKeyValuePair.Value.Id);
                //}

                Assert.That(
                    Factory.EntityAccessControlService.Check(
                        new EntityRef(entities[testType]),
                        new [] { Permissions.Read }),
                    Is.EqualTo(expectedResult));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_HashSet_Hashing()
        {
            HashSet<EntityType> entityTypesHashSet;
            EntityType entityType1;
            EntityType entityType2;
            long tenantId;

            tenantId = RequestContext.GetContext().Tenant.Id;
            entityType1 = new EntityType(new ActivationData(1, tenantId));
            entityType2 = new EntityType(new ActivationData(1, tenantId)); // Note same ID so should be the same type

            entityTypesHashSet = new HashSet<EntityType>();
            entityTypesHashSet.Add(entityType1);
            Assert.That(entityTypesHashSet.Contains(entityType1), Is.True, "Contains entity type 1");
            Assert.That(entityTypesHashSet.Contains(entityType2), Is.False, "Does not contain entity type 2");

            entityTypesHashSet = new HashSet<EntityType>(new EntityIdEqualityComparer<EntityType>());
            entityTypesHashSet.Add(entityType1);
            Assert.That(entityTypesHashSet.Contains(entityType1), Is.True, "Contains entity type 1");
            Assert.That(entityTypesHashSet.Contains(entityType2), Is.True, "Contains entity type 2");
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        // Sanity tests
        [TestCase(null, "rootEntityType", false)]
        [TestCase(null, "parentEntityType1", false)]
        [TestCase(null, "childEntityType11", false)]
        // Granting access to one entity should give access to all
        [TestCase("rootEntityType", "rootEntityType,parentEntityType1,childEntityType11", true)]
        [TestCase("parentEntityType1", "rootEntityType,parentEntityType1,childEntityType11", true)]
        [TestCase("childEntityType11", "rootEntityType,parentEntityType1,childEntityType11", true)]
        public void Test_CheckAccess_BasicRead_ThreeEntityUnidirectionalCycle(string grantReadOnType, string testTypes, bool expectedResult)
        {
            UserAccount userAccount;
            Dictionary<string, IEntity> entities;
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationshipTypes;
            IEntityRelationshipCollection<IEntity> entityRelationships;

            // Build the following entity/relationship graph:
            //
            // rootEntityType  <-------------
            //   |                          |
            //   -> parentEntityType1       |
            //        |                     |
            //        -> childEntityType11 -|
            //
            // The basic idea is that when read access is 
            // granted to any entity, access should be 
            // granted to all.

            const string rootEntityType = "rootEntityType";
            const string parentEntityType1 = "parentEntityType1";
            const string childEntityType11 = "childEntityType11";
            const string rootToParent1 = "rootToParent1";
            const string parent1ToChild11 = "parent1ToChild11";
            const string child11ToRoot = "child11ToRoot";
            string[] types = 
            {
                rootEntityType,
                parentEntityType1,
                childEntityType11
            };
            List<Tuple<string, string, string, Direction>> relationships = new List<Tuple<string, string, string, Direction>>
            {
                new Tuple<string, string, string, Direction>(rootEntityType, rootToParent1, parentEntityType1, Direction.Forward),
                new Tuple<string, string, string, Direction>(parentEntityType1, parent1ToChild11, childEntityType11, Direction.Forward),
                new Tuple<string, string, string, Direction>(childEntityType11, child11ToRoot, rootEntityType, Direction.Forward)
            };

            // Speed up tests
            using (new SecurityBypassContext())
            {
                // Create the types and a instance of each
                entityTypes = new Dictionary<string, EntityType>();
                entities = new Dictionary<string, IEntity>();
                foreach (string type in types)
                {
                    entityTypes[type] = new EntityType
                    {
                        Name = type
                    };
                    entityTypes[type].Inherits.Add(UserResource.UserResource_Type);
                    entityTypes[type].Save();

                    entities[type] = Entity.Create(entityTypes[type]);                    
                    entities[type].Save();
                }

                // Create relationships and assign the to types for each
                relationshipTypes = new Dictionary<string, Relationship>();
                foreach (Tuple<string, string, string, Direction> relationship in relationships)
                {
                    if (relationship.Item4 == Direction.Forward)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item1],
                            ToType = entityTypes[relationship.Item3],
                            SecuresTo = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRelationships = entities[relationship.Item1].GetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            Direction.Forward);
                        entityRelationships.Add(entities[relationship.Item3]);
                        entities[relationship.Item1].SetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            entityRelationships);
                        entities[relationship.Item1].Save();
                    }
                    else if (relationship.Item4 == Direction.Reverse)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item3],
                            ToType = entityTypes[relationship.Item1],
                            SecuresFrom = true,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRelationships = entities[relationship.Item3].GetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            Direction.Reverse);
                        entityRelationships.Add(entities[relationship.Item1]);
                        entities[relationship.Item3].SetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            entityRelationships);
                        entities[relationship.Item3].Save();
                    }
                    else
                    {
                        Assert.Fail("Unknown direction");
                    }
                }
            }

            userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            if (!string.IsNullOrWhiteSpace(grantReadOnType))
            {
                new AccessRuleFactory().AddAllowReadQuery(
                    userAccount.As<Subject>(),
                    entityTypes[grantReadOnType].As<SecurableEntity>(),
                    TestQueries.Entities().ToReport());
            }

            using (new SetUser(userAccount))
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check(
                        testTypes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(name => new EntityRef(entities[name])).ToList(),
                        new[] { Permissions.Read }),
                    Has.All.Property("Value").EqualTo(expectedResult));
            }
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        // Sanity tests
        [TestCase(null, "entityTypeA", false)]
        [TestCase(null, "entityTypeB", false)]
        // Granting access to one entity should give access to both
        [TestCase("entityTypeA", "entityTypeA,entityTypeB", true)]
        [TestCase("entityTypeB", "entityTypeA,entityTypeB", true)]
        public void Test_CheckAccess_BasicRead_TwoEntitiesBidirectionalCycle(string grantReadOnType, string testTypes, bool expectedResult)
        {
            UserAccount userAccount;
            Dictionary<string, IEntity> entities;
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationshipTypes;
            IEntityRelationshipCollection<IEntity> entityRelationships;

            // Build the following entity/relationship graph:
            //
            // entityTypeA <---> entityTypeB
            //
            // The basic idea is that when read access is 
            // granted to either A or B, the user should
            // be able to read the other.

            const string entityTypeA = "entityTypeA";
            const string entityTypeB = "entityTypeB";
            const string entityTypeAToB = "entityTypeAToB";
            string[] types = 
            {
                entityTypeA,
                entityTypeB
            };
            List<Tuple<string, string, string, Direction, bool, bool>> relationships = new List<Tuple<string, string, string, Direction, bool, bool>>
            {
                new Tuple<string, string, string, Direction, bool, bool>(entityTypeA, entityTypeAToB, entityTypeB, Direction.Forward, true, true)
            };

            // Speed up tests
            using (new SecurityBypassContext())
            {
                // Create the types and a instance of each
                entityTypes = new Dictionary<string, EntityType>();
                entities = new Dictionary<string, IEntity>();
                foreach (string type in types)
                {
                    entityTypes[type] = new EntityType
                    {
                        Name = type
                    };
                    entityTypes[type].Inherits.Add(UserResource.UserResource_Type);
                    entityTypes[type].Save();

                    entities[type] = Entity.Create(entityTypes[type]);                    
                    entities[type].Save();
                }

                // Create relationships and assign the to types for each
                relationshipTypes = new Dictionary<string, Relationship>();
                foreach (Tuple<string, string, string, Direction, bool, bool> relationship in relationships)
                {
                    if (relationship.Item4 == Direction.Forward)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item1],
                            ToType = entityTypes[relationship.Item3],
                            SecuresTo = relationship.Item5,
                            SecuresFrom = relationship.Item6,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRelationships = entities[relationship.Item1].GetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            Direction.Forward);
                        entityRelationships.Add(entities[relationship.Item3]);
                        entities[relationship.Item1].SetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            entityRelationships);
                        entities[relationship.Item1].Save();
                    }
                    else if (relationship.Item4 == Direction.Reverse)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item3],
                            ToType = entityTypes[relationship.Item1],
                            SecuresTo = relationship.Item5,
                            SecuresFrom = relationship.Item6,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();

                        entityRelationships = entities[relationship.Item3].GetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            Direction.Reverse);
                        entityRelationships.Add(entities[relationship.Item1]);
                        entities[relationship.Item3].SetRelationships(
                            new EntityRef(relationshipTypes[relationship.Item2]),
                            entityRelationships);
                        entities[relationship.Item3].Save();
                    }
                    else
                    {
                        Assert.Fail("Unknown direction");
                    }
                }
            }

            userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            if (!string.IsNullOrWhiteSpace(grantReadOnType))
            {
                new AccessRuleFactory().AddAllowReadQuery(
                    userAccount.As<Subject>(),
                    entityTypes[grantReadOnType].As<SecurableEntity>(),
                    TestQueries.Entities().ToReport());
            }

            using (new SetUser(userAccount))
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check(
                        testTypes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(name => new EntityRef(entities[name])).ToList(),
                        new[] { Permissions.Read }),
                    Has.All.Property("Value").EqualTo(expectedResult));
            }
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        // Sanity tests
        [TestCase(null, "entityTypeA", false)]
        [TestCase(null, "entityTypeB", false)]
        // Granting access to one entity should give access to both
        [TestCase("entityTypeA", "entityTypeA,entityTypeB", true)]
        [TestCase("entityTypeB", "entityTypeA,entityTypeB", true)]
        public void Test_CheckAccess_BasicCreate_TwoEntitiesBidirectionalCycle(string grantReadOnType, string testTypes, bool expectedResult)
        {
            UserAccount userAccount;
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationshipTypes;
            IEntityAccessControlService entityEntityAccessControlService;

            // Build the following entity/relationship graph:
            //
            // entityTypeA <---> entityTypeB
            //
            // The basic idea is that when create access is 
            // granted to either type A or B, the user should
            // be able to create the other type.

            const string entityTypeA = "entityTypeA";
            const string entityTypeB = "entityTypeB";
            const string entityTypeAToB = "entityTypeAToB";
            string[] types = 
            {
                entityTypeA,
                entityTypeB
            };
            List<Tuple<string, string, string, Direction, bool, bool>> relationships = new List<Tuple<string, string, string, Direction, bool, bool>>
            {
                new Tuple<string, string, string, Direction, bool, bool>(entityTypeA, entityTypeAToB, entityTypeB, Direction.Forward, true, true)
            };

            // Speed up tests
            using (new SecurityBypassContext())
            {
                // Create the types and a instance of each
                entityTypes = new Dictionary<string, EntityType>();
                foreach (string type in types)
                {
                    entityTypes[type] = new EntityType
                    {
                        Name = type
                    };
                    entityTypes[type].Inherits.Add(UserResource.UserResource_Type);
                    entityTypes[type].Save();
                }

                // Create relationships and assign the to types for each
                relationshipTypes = new Dictionary<string, Relationship>();
                foreach (Tuple<string, string, string, Direction, bool, bool> relationship in relationships)
                {
                    if (relationship.Item4 == Direction.Forward)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item1],
                            ToType = entityTypes[relationship.Item3],
                            SecuresTo = relationship.Item5,
                            SecuresFrom = relationship.Item6,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();
                    }
                    else if (relationship.Item4 == Direction.Reverse)
                    {
                        relationshipTypes[relationship.Item2] = new Relationship
                        {
                            Name = relationship.Item2,
                            FromType = entityTypes[relationship.Item3],
                            ToType = entityTypes[relationship.Item1],
                            SecuresTo = relationship.Item5,
                            SecuresFrom = relationship.Item6,
                            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                        };
                        relationshipTypes[relationship.Item2].Save();
                    }
                    else
                    {
                        Assert.Fail("Unknown direction");
                    }
                }
            }

            userAccount = new UserAccount
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            if (!string.IsNullOrWhiteSpace(grantReadOnType))
            {
                new AccessRuleFactory().AddAllowCreate(
                    userAccount.As<Subject>(),
                    entityTypes[grantReadOnType].As<SecurableEntity>());
            }

            using (new SetUser(userAccount))
            {
                entityEntityAccessControlService = Factory.EntityAccessControlService;

                Assert.That(
                    entityEntityAccessControlService.CanCreate(
                        testTypes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(name => entityTypes[name])
                                 .ToList()),
                    Has.All.Property("Value").EqualTo(expectedResult),
                    "CanCreate failed");

                Assert.That(
                    entityEntityAccessControlService.Check(
                        testTypes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(name => new EntityRef(Entity.Create(entityTypes[name])))
                                 .ToList(),
                        new[] { Permissions.Create }),
                    Has.All.Property("Value").EqualTo(expectedResult),
                    "Check failed");

            }
        }

        [Test]
        public void Test_CheckTypeAccess_NullEntityTypes()
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckTypeAccess( (IList<EntityType>)null, new EntityRef( ), new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        public void Test_CheckTypeAccess_EntityTypesContainsNull( )
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckTypeAccess( new EntityType[] { null }, new EntityRef( ), new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullPermission( )
        {
            Assert.That( ( ) => new SecuresFlagEntityAccessControlChecker( ).CheckTypeAccess( new EntityType [ 0 ], null, new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "permission" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullUser( )
        {
            Assert.That(() => new SecuresFlagEntityAccessControlChecker().CheckTypeAccess( new EntityType[0], new EntityRef( ), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent]
        [TestCase(null, "")]
        [TestCase("A", "A, B, C, D")]
        [TestCase("B", "A, B, C, D")]
        [TestCase("C", "C")]
        [TestCase("D", "A, B, C, D")]
        public void Test_CanCreateMultiple_Cycle(string grantCreateOnEntityType, 
            string expectedCreateableEntityTypes)
        {
            //  Create the following arrangement:
            //
            //   +---------------+        +---------------+       +---------------+
            //   | Entity Type A |  ----> | Entity Type B | ----> | Entity Type C |
            //   +---------------+        +---------------+       +---------------+
            //          ^                         |
            //          |    +---------------+    |
            //          |----| Entity Type D |<---|
            //               +---------------+
            //
            //  where the secures 'to' flag is set on a relationship between
            //  each entity type.

            IDictionary<string, EntityType> nameToEntityType;
            UserAccount userAccount;
            IDictionary<long, bool> results;
            string createableTypeNames;

            nameToEntityType = new Dictionary<string, EntityType>();
            foreach (string name in new[] {"A", "B", "C", "D"})
            {
                nameToEntityType[name] = new EntityType
                {
                    Name = name
                };
                nameToEntityType[name].Inherits.Add(UserResource.UserResource_Type);
                nameToEntityType[name].Save();
            }
            foreach (Tuple<string, string> fromTo in new []
            {
                new Tuple<string, string>("A", "B"),
                new Tuple<string, string>("B", "C"),
                new Tuple<string, string>("B", "D"),
                new Tuple<string, string>("D", "A")
            })
            {
                Relationship relationship;

                relationship = new Relationship
                {
                    FromType = nameToEntityType[fromTo.Item1],
                    ToType = nameToEntityType[fromTo.Item2],
                    Name = $"From {fromTo.Item1} to {fromTo.Item2}",
                    SecuresTo = true,
                    Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                };
                relationship.Save();
            }

            userAccount = new UserAccount()
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            if (grantCreateOnEntityType != null)
            {
                new AccessRuleFactory().AddAllowCreate(
                    userAccount.As<Subject>(),
                    nameToEntityType[grantCreateOnEntityType].As<SecurableEntity>()
                    );
            }

            using (new SetUser(userAccount))
            {
                results = Factory.EntityAccessControlService.CanCreate(
                    nameToEntityType.Values.ToList());
            }
            createableTypeNames =
                string.Join(
                    ", ",
                    results.Where(kvp => kvp.Value)
                           .Select(kvp => Entity.Get<EntityType>(kvp.Key).Name)
                           .OrderBy(s => s));

            Assert.That(createableTypeNames, Is.EqualTo(expectedCreateableEntityTypes));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent]
        [TestCase(null, "")]
        [TestCase("A", "A, B, D, E")]
        [TestCase("B", "B")]
        [TestCase("C", "B, C, D, E")]
        [TestCase("D", "D, E")]
        [TestCase("E", "E")]
        public void Test_CanCreateAncestorAndDescendant(string grantCreateOnEntityType,
            string expectedCreateableEntityTypes)
        {
            //  Create the following arrangement:
            //
            //                            +---------------+       +---------------+
            //                            | Parent Type C | ----> | Parent Type D |
            //                            +---------------+   R   +---------------+
            //                                    ^                        ^
            //                                Inherits                 Inherits
            //                                    |                        |
            //   +---------------+        +---------------+       +---------------+
            //   | Entity Type A |  ----> | Entity Type B |       | Entity Type E |
            //   +---------------+    R   +---------------+       +---------------+
            //
            //  where the secures 'to' flag is set on a relationship between
            //  each entity type. 

            IDictionary<string, EntityType> nameToEntityType;
            UserAccount userAccount;
            IDictionary<long, bool> results;
            string createableTypeNames;

            nameToEntityType = new Dictionary<string, EntityType>();
            foreach (string name in new[] { "A", "B", "C", "D", "E" })
            {
                nameToEntityType[name] = new EntityType
                {
                    Name = name
                };
                nameToEntityType[name].Inherits.Add(UserResource.UserResource_Type);
                nameToEntityType[name].Save();
            }
            foreach (Tuple<string, string> fromTo in new[]
            {
                new Tuple<string, string>("A", "B"),
                new Tuple<string, string>("C", "D")
            })
            {
                Relationship relationship;

                relationship = new Relationship
                {
                    FromType = nameToEntityType[fromTo.Item1],
                    ToType = nameToEntityType[fromTo.Item2],
                    Name = $"From {fromTo.Item1} to {fromTo.Item2}",
                    SecuresTo = true,
                    Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                };
                relationship.Save();
            }
            foreach (Tuple<string, string> inherits in new[]
            {
                new Tuple<string, string>("B", "C"),
                new Tuple<string, string>("E", "D")
            })
            {
                nameToEntityType[inherits.Item1].Inherits.Add(nameToEntityType[inherits.Item2]);
                nameToEntityType[inherits.Item1].Save();
            }

            userAccount = new UserAccount()
            {
                Name = "Test User " + Guid.NewGuid()
            };
            userAccount.Save();

            if (grantCreateOnEntityType != null)
            {
                new AccessRuleFactory().AddAllowCreate(
                    userAccount.As<Subject>(),
                    nameToEntityType[grantCreateOnEntityType].As<SecurableEntity>()
                    );
            }

            using (new SetUser(userAccount))
            {
                results = Factory.EntityAccessControlService.CanCreate(
                    nameToEntityType.Values.ToList());
            }
            createableTypeNames =
                string.Join(
                    ", ",
                    results.Where(kvp => kvp.Value)
                           .Select(kvp => Entity.Get<EntityType>(kvp.Key).Name)
                           .OrderBy(s => s));

            Assert.That(createableTypeNames, Is.EqualTo(expectedCreateableEntityTypes));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(0, new[] { true, true, true })]
        [TestCase(1, new[] { false, true, true })]
        [TestCase(2, new[] { false, false, true })]
        public void Test_CycleWithSameEntityType(int grantReadOn, bool[] canRead)
        {
            const int numEntities = 3;

            if (canRead.Length != numEntities)
            {
                throw new ArgumentException("Incorrect length", "canRead");
            }

            UserAccount userAccount;
            EntityType entityType;
            Relationship relationship;
            List<IEntity> entities;

            //  Create the following arrangement:
            //
            //   +-----------+        +----------+       +----------+
            //   | Entity 0  |  ----> | Entity 1 | ----> | Entity 2 |
            //   +-----------+        +----------+       +----------+
            //
            //  where each entity is of the same entity type, each relationship
            //  is of the same type and has the secures 'to' type flag set.

            entityType = new EntityType {Name = "Test Entity Type"};
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Save();

            relationship = new Relationship
            {
                FromType = entityType,
                ToType = entityType,
                SecuresTo = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
            };
            relationship.Save();
            entityType.Relationships.Add(relationship);

            entities = new List<IEntity>();
            for (int i = 0; i < numEntities; i++)
            {
                IEntity entity;
                IEntity previousEntity;
                IEntityRelationshipCollection<IEntity> relationships;

                entity = Entity.Create(entityType);
                entity.SetField("core:name", i == grantReadOn ? "A" : "Not A");
                entity.Save();

                previousEntity = entities.LastOrDefault();
                if (previousEntity != null)
                {
                    relationships = previousEntity.GetRelationships(relationship, Direction.Forward);
                    relationships.Add(entity);
                    previousEntity.SetRelationships(relationship, relationships);
                    previousEntity.Save();
                }

                entities.Add(entity);
            }

            userAccount = new UserAccount { Name = "Test User " + Guid.NewGuid() };
            userAccount.Save();

            new AccessRuleFactory().AddAllowReadQuery(
                userAccount.As<Subject>(),
                entityType.As<SecurableEntity>(),
                TestQueries.EntitiesWithNameA().ToReport());

            using (new SetUser(userAccount))
            {
                IDictionary<long, bool> result = Factory.EntityAccessControlService.Check(
                    entities.Select(e => new EntityRef(e)).ToList(),
                    new[] {Permissions.Read});

                Assert.That(result.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value), Is.EquivalentTo(canRead),
                    "Incorrect results");
            }
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_SecuresFlagsWithBaseClasses(bool securesTo)
        {
            EntityType relatedType;
            EntityType baseType;
            EntityType derivedType;
            Relationship relationshipType;
            IEntity relatedEntity;
            IEntity derivedEntity;
            IEntityRelationshipCollection<IEntity> relationships;
            UserAccount userAccount;

            // Create entity types with the following arrangement:
            //
            //   +----------+                  +----------+       
            //   | Related  | ---------------> |   Base   | 
            //   +----------+   Secures To     +----------+       
            //                                       ^
            //                                       |
            //                                    Inherits
            //                                       |
            //                                 +----------+
            //                                 | Derived  |
            //                                 +----------+
            //
            // then create an instance of "Related" and "Derived"
            // with the relationship between them and ensure the 
            // secures 'To' type flag is checked.

            baseType = new EntityType{ Name = "Base type" };
            baseType.Inherits.Add(UserResource.UserResource_Type);
            baseType.Save();

            derivedType = new EntityType { Name = "Derived type" };
            derivedType.Inherits.Add(baseType);
            derivedType.Save();

            relatedType = new EntityType { Name = "Related Type" };
            relatedType.Inherits.Add(UserResource.UserResource_Type);
            relatedType.Save();

            relationshipType = new Relationship();
            relationshipType.Name = "relationship";
            relationshipType.FromType = relatedType;
            relationshipType.ToType = baseType;
            relationshipType.SecuresTo = securesTo;
            relationshipType.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            relationshipType.Save();

            relatedEntity = Entity.Create(relatedType);
            relatedEntity.SetField("core:name", "Related Entity");
            relatedEntity.Save();

            derivedEntity = Entity.Create(derivedType);
            derivedEntity.SetField("core:name", "Derived Entity");
            derivedEntity.Save();

            relationships = relatedEntity.GetRelationships(relationshipType);
            relationships.Add(derivedEntity);
            relatedEntity.SetRelationships(relationshipType, relationships);
            relatedEntity.Save();

            userAccount = new UserAccount { Name = "Test User " + Guid.NewGuid() };
            userAccount.Save();

            new AccessRuleFactory().AddAllowReadQuery(
                userAccount.As<Subject>(),
                relatedType.As<SecurableEntity>(),
                TestQueries.Entities().ToReport());

            using (new SetUser(userAccount))
            {
                IDictionary<long, bool> result = Factory.EntityAccessControlService.Check(
                    new[] { new EntityRef(derivedEntity) },
                    new[] { Permissions.Read });

                Assert.That(result, 
                            Has.Exactly(1).Property("Key").EqualTo(derivedEntity.Id)
                                .And.Property("Value").EqualTo(securesTo),
                            "Incorrect results");
            }
        }
    }
}
