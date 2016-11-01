// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.Diagnostics;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    [FailOnEvent(new[] {EventLogLevel.Error, EventLogLevel.Warning})]
    public class EntityMemberRequestFactoryTests
    {
        private DeferredChannelMessageContext _deferredChannelMessageContext;

        [SetUp]
        public void Setup()
        {
            _deferredChannelMessageContext = new DeferredChannelMessageContext();
        }

        [TearDown]
        public void Teardown()
        {
            if (_deferredChannelMessageContext != null)
            {
                _deferredChannelMessageContext.Dispose();
                _deferredChannelMessageContext = null;
            }
        }

        [Test]
        public void BuildEntityMemberRequest_Null()
        {
            Assert.That(() => new EntityMemberRequestFactory().BuildEntityMemberRequest(null, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityType"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void BuildEntityMemberRequest_NoRelationships()
        {
            EntityType entityType;
            EntityMemberRequest entityMemberRequest;

            entityType = new EntityType();
            entityType.Save();

            entityMemberRequest = new EntityMemberRequestFactory().BuildEntityMemberRequest(entityType, new[] { Permissions.Read } );

            Assert.That(entityMemberRequest, Has.Property("Relationships").Empty);
            Assert.That(entityMemberRequest, Has.Property("Fields").Empty);
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void BuildEntityMemberRequest_RelationshipsWithFlagsUnset()
        {
            EntityType entityType;
            Relationship fromRelationship;
            Relationship toRelationship;
            EntityMemberRequest entityMemberRequest;

            fromRelationship = new Relationship();
            fromRelationship.Save();

            toRelationship = new Relationship();
            toRelationship.Save();

            entityType = new EntityType();
            entityType.Relationships.Add(toRelationship);
            entityType.ReverseRelationships.Add(fromRelationship);
            entityType.Save();

            entityMemberRequest = new EntityMemberRequestFactory().BuildEntityMemberRequest(entityType, new [ ] { Permissions.Read } );

            Assert.That(entityMemberRequest, Has.Property("Relationships").Empty);
            Assert.That(entityMemberRequest, Has.Property("Fields").Empty);
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void BuildEntityMemberRequest_RelationshipsWithFlagsSet(bool securesFrom, bool securesTo)
        {
            EntityType entityType;
            Relationship fromRelationship;
            Relationship toRelationship;
            EntityMemberRequest entityMemberRequest;
            IList<EntityRef> expectedRelationshipTypeIds;

            fromRelationship = new Relationship();
            fromRelationship.SecuresTo = securesTo;
            fromRelationship.FromType = new EntityType();
            fromRelationship.Save();

            toRelationship = new Relationship();
            toRelationship.SecuresFrom = securesFrom;
            toRelationship.ToType = new EntityType();
            toRelationship.Save();

            entityType = new EntityType();
            entityType.Relationships.Add(toRelationship);
            entityType.ReverseRelationships.Add(fromRelationship);
            entityType.Save();

            entityMemberRequest = new EntityMemberRequestFactory().BuildEntityMemberRequest(entityType, new [ ] { Permissions.Read } );

            expectedRelationshipTypeIds = new List<EntityRef>();
            if (securesTo)
            {
                expectedRelationshipTypeIds.Add(new EntityRef(fromRelationship));
            }
            if (securesFrom)
            {
                expectedRelationshipTypeIds.Add(new EntityRef(toRelationship));
            }
            Assert.That(entityMemberRequest.Relationships.Select(r => r.RelationshipTypeId),
                Is.EquivalentTo(expectedRelationshipTypeIds).Using(EntityRefComparer.Instance));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void BuildEntityMemberRequest_ToAndFromRelationshipToSelf(bool securesFrom, bool securesTo)
        {
            EntityType entityType;
            Relationship reverseRelationship;
            Relationship forwardRelationship;
            EntityMemberRequest entityMemberRequest;
            IList<EntityRef> expectedRelationshipTypeIds;

            entityType = new EntityType();

            reverseRelationship = new Relationship();
            reverseRelationship.SecuresTo = securesTo;
            reverseRelationship.ToType = entityType;
            reverseRelationship.FromType = new EntityType();
            reverseRelationship.Save();

            forwardRelationship = new Relationship();
            forwardRelationship.SecuresFrom = securesFrom;
            forwardRelationship.ToType = new EntityType();
            forwardRelationship.FromType = entityType;
            forwardRelationship.Save();

            entityType.Relationships.Add(forwardRelationship);
            entityType.ReverseRelationships.Add(reverseRelationship);
            entityType.Save();

            entityMemberRequest = new EntityMemberRequestFactory().BuildEntityMemberRequest(entityType, new [ ] { Permissions.Read } );

            expectedRelationshipTypeIds = new List<EntityRef>();
            if (securesTo)
            {
                expectedRelationshipTypeIds.Add(new EntityRef(reverseRelationship));
            }
            if (securesFrom)
            {
                expectedRelationshipTypeIds.Add(new EntityRef(forwardRelationship));
            }
            Assert.That(entityMemberRequest.Relationships.Select(r => r.RelationshipTypeId),
                Is.EquivalentTo(expectedRelationshipTypeIds).Using(EntityRefComparer.Instance));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void BuildEntityMemberRequest_TwoEntitiesInCycle(bool securesFrom, bool securesTo)
        {
            EntityType entityType1;
            EntityType entityType2;
            Relationship fromRelationship;
            Relationship toRelationship;
            EntityMemberRequest entityMemberRequest;
            IList<EntityRef> expectedRelationshipTypeIds;

            entityType1 = new EntityType();
            entityType1.Save();
            entityType2 = new EntityType();
            entityType2.Save();

            fromRelationship = new Relationship();
            fromRelationship.SecuresTo = securesTo;
            fromRelationship.ToType = entityType2;
            fromRelationship.FromType = entityType1;
            fromRelationship.Save();

            toRelationship = new Relationship();
            toRelationship.SecuresFrom = securesFrom;
            toRelationship.ToType = entityType1;
            toRelationship.FromType = entityType2;
            toRelationship.Save();

            entityType2.Relationships.Add(toRelationship);
            entityType2.ReverseRelationships.Add(fromRelationship);
            entityType2.Save();

            entityMemberRequest = new EntityMemberRequestFactory().BuildEntityMemberRequest(entityType2, new [ ] { Permissions.Read } );

            expectedRelationshipTypeIds = new List<EntityRef>();
            if (securesTo)
            {
                expectedRelationshipTypeIds.Add(new EntityRef(fromRelationship));
            }
            if (securesFrom)
            {
                expectedRelationshipTypeIds.Add(new EntityRef(toRelationship));
            }
            Assert.That(entityMemberRequest.Relationships.Select(r => r.RelationshipTypeId),
                Is.EquivalentTo(expectedRelationshipTypeIds).Using(EntityRefComparer.Instance));
        }

        /// <summary>
        /// Mimic the behaviour of a report, workflow or form, where the security of 
        /// a number of entities will be tied to a single entity.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("rootEntityType", "")]
        [TestCase("parentEntityType1", "rootToParent1")]
        [TestCase("childEntityType11", "rootToParent1,parent1ToChild11")]
        [TestCase("parentEntityType2", "rootToParent2")]
        [TestCase("childEntityType21", "rootToParent2,parent2ToChild21")]
        [TestCase("grandChildEntityType211", "rootToParent2,parent2ToChild21,child21ToGrandChild211")]
        [TestCase("childEntityType22", "rootToParent2,parent2ToChild22")]
        public void BuildEntityMemberRequest_TreeSecuredByRoot(string entityType, string expectedRelationships)
        {
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationships;
            EntityMemberRequest entityMemberRequest;
            IList<IEntityRef> expectedRelationshipTypeIds;
            IList<IEntityRef> actualRelationshipTypeIds;

            // Build the following entity/relationship graph:
            //
            // root
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

            // Create the types
            entityTypes = new Dictionary<string, EntityType>();
            foreach (string type in new[]
            {
                rootEntityType,
                parentEntityType1,
                childEntityType11,
                parentEntityType2,
                childEntityType21,
                grandChildEntityType211,
                childEntityType22
            })
            {
                entityTypes[type] = new EntityType();
                entityTypes[type].Save();
            }

            // Create relationships and assign the to types for each
            relationships = new Dictionary<string, Relationship>();
            foreach (KeyValuePair<string, string> relationshipToEntityType in new Dictionary<string, string>
            {
                {rootToParent1, parentEntityType1},
                {parent1ToChild11, childEntityType11},
                {rootToParent2, parentEntityType2},
                {parent2ToChild21, childEntityType21},
                {child21ToGrandChild211, grandChildEntityType211},
                {parent2ToChild22, childEntityType22}
            })
            {
                relationships[relationshipToEntityType.Key] = new Relationship
                {
                    SecuresTo = true,
                    ToType = entityTypes[relationshipToEntityType.Value]
                };
                relationships[relationshipToEntityType.Key].Save();
            }

            // Add each relationship to the originating (from) entity type
            foreach (
                KeyValuePair<string, IEnumerable<string>> entityTypeToRelationship in
                    new Dictionary<string, IEnumerable<string>>
                    {
                        {rootEntityType, new[] {rootToParent1, rootToParent2}},
                        {parentEntityType1, new[] {parent1ToChild11}},
                        {childEntityType21, new[] {child21ToGrandChild211}},
                        {parentEntityType2, new[] {parent2ToChild21, parent2ToChild22}}
                    })
            {
                entityTypes[entityTypeToRelationship.Key].Relationships.AddRange(
                    entityTypeToRelationship.Value.Select(r => relationships[r]));
                entityTypes[entityTypeToRelationship.Key].Save();
            }

            entityMemberRequest = new EntityMemberRequestFactory().BuildEntityMemberRequest(entityTypes[entityType], new [ ] { Permissions.Read } );

            expectedRelationshipTypeIds =
                expectedRelationships.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => new EntityRef(relationships[s]))
                    .ToList<IEntityRef>();
            actualRelationshipTypeIds =
                Delegates.WalkGraph(entityMemberRequest.Relationships, rr => rr.RequestedMembers.Relationships)
                    .Select(rr => rr.RelationshipTypeId)
                    .ToList<IEntityRef>();
            Assert.That(actualRelationshipTypeIds,
                Is.EquivalentTo(expectedRelationshipTypeIds).Using(EntityRefComparer.Instance));
        }

        /// <summary>
        /// Mimic the behaviour of a report, workflow or form, where the security of 
        /// a number of entities will be tied to a single entity.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("rootEntityType1", "")]
        [TestCase("parentEntityType1", "root1ToParent1")]
        [TestCase("childEntityType11", "root1ToParent1,parent1ToChild11,root2ToParent2,parent2ToChild11")]
        [TestCase("parentEntityType2", "root2ToParent2")]
        [TestCase("rootEntityType2", "")]
        public void BuildEntityMemberRequest_TreeSecuredByMultipleRoots(string entityType,
            string expectedRelationships)
        {
            Dictionary<string, EntityType> entityTypes;
            Dictionary<string, Relationship> relationships;
            EntityMemberRequest entityMemberRequest;
            IList<IEntityRef> expectedRelationshipTypeIds;
            IList<IEntityRef> actualRelationshipTypeIds;

            // Build the following entity/relationship graph:
            //
            // root1
            //   |
            //   -> parentEntityType1
            //        |
            //        -> childEntityType11
            //        |
            //   -> parentEntityType2
            //   |      
            // root2

            const string rootEntityType1 = "rootEntityType1";
            const string parentEntityType1 = "parentEntityType1";
            const string childEntityType11 = "childEntityType11";
            const string parentEntityType2 = "parentEntityType2";
            const string rootEntityType2 = "rootEntityType2";
            const string root1ToParent1 = "root1ToParent1";
            const string parent1ToChild11 = "parent1ToChild11";
            const string parent2ToChild11 = "parent2ToChild11";
            const string root2ToParent2 = "root2ToParent2";

            // Create the types
            entityTypes = new Dictionary<string, EntityType>();
            foreach (string type in new[]
            {
                rootEntityType1,
                parentEntityType1,
                childEntityType11,
                parentEntityType2,
                rootEntityType2
            })
            {
                entityTypes[type] = new EntityType();
                entityTypes[type].Save();
            }

            // Create relationships and assign the to types for each
            relationships = new Dictionary<string, Relationship>();
            foreach (KeyValuePair<string, string> relationshipToEntityType in new Dictionary<string, string>
            {
                {root1ToParent1, parentEntityType1},
                {parent1ToChild11, childEntityType11},
                {parent2ToChild11, childEntityType11},
                {root2ToParent2, parentEntityType2}
            })
            {
                relationships[relationshipToEntityType.Key] = new Relationship
                {
                    SecuresTo = true,
                    ToType = entityTypes[relationshipToEntityType.Value]
                };
                relationships[relationshipToEntityType.Key].Save();
            }

            // Add each relationship to the originating (from) entity type
            foreach (
                KeyValuePair<string, IEnumerable<string>> entityTypeToRelationship in
                    new Dictionary<string, IEnumerable<string>>
                    {
                        {rootEntityType1, new[] {root1ToParent1}},
                        {parentEntityType1, new[] {parent1ToChild11}},
                        {parentEntityType2, new[] {parent2ToChild11}},
                        {rootEntityType2, new[] {root2ToParent2}}
                    })
            {
                entityTypes[entityTypeToRelationship.Key].Relationships.AddRange(
                    entityTypeToRelationship.Value.Select(r => relationships[r]));
                entityTypes[entityTypeToRelationship.Key].Save();
            }

            entityMemberRequest = new EntityMemberRequestFactory().BuildEntityMemberRequest(entityTypes[entityType], new [ ] { Permissions.Read } );

            expectedRelationshipTypeIds =
                expectedRelationships.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => new EntityRef(relationships[s]))
                    .ToList<IEntityRef>();
            actualRelationshipTypeIds =
                Delegates.WalkGraph(entityMemberRequest.Relationships, rr => rr.RequestedMembers.Relationships)
                    .Select(rr => rr.RelationshipTypeId)
                    .ToList<IEntityRef>();
            Assert.That(actualRelationshipTypeIds,
                Is.EquivalentTo(expectedRelationshipTypeIds).Using(EntityRefComparer.Instance));
        }

        ///// <summary>
        ///// Mimic the behaviour of a report, workflow or form, where the security of 
        ///// a number of entities will be tied to a single entity.
        ///// </summary>
        //[TestCase( "iconFileType" )]
        //[RunAsDefaultTenant]
        //public void BuildEntityMemberRequest_ByAlias( string alias )
        //{
        //    var factory = new EntityMemberRequestFactory( );
        //    EntityType type = Entity.Get<EntityType>( alias );

        //    var entityMemberRequest = factory.BuildEntityMemberRequest( type, new [ ] { Permissions.Read } );
        //    Assert.That( entityMemberRequest, Is.Not.Null );
        //}
    }
}
