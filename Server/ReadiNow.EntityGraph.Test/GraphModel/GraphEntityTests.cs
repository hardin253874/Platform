// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using ReadiNow.EntityGraph.GraphModel;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Test;

namespace ReadiNow.EntityGraph.Test.GraphModel
{
    [TestFixture]
    class GraphEntityTests
    {
        public GraphEntityRepository GraphEntityRepository
        {
            get { return new GraphEntityRepository(); }
        }

        public IEntityAccessControlService MockEntityAccessControlService
        {
            get
            {
                // Pass through requests
                Mock<IEntityAccessControlService> mock = new Mock<IEntityAccessControlService>();
                mock.Setup(m => m.Check(It.IsAny<IList<EntityRef>>(), It.IsAny<IList<EntityRef>>()))
                    .Returns((IList<EntityRef> arg1, IList<EntityRef> arg2) => arg1.ToDictionary(k => k.Id, k => true));
                mock.Setup(m => m.Demand(It.IsAny<IList<EntityRef>>(), It.IsAny<IList<EntityRef>>()));
                return mock.Object;
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Id()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.Id, Is.EqualTo(id));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TypeIds()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.TypeIds, Is.EquivalentTo( new long[] { WellKnownAliases.CurrentTenant.Type } ));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TypeIds_NotLoaded()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id, inherits.id";

            EntityType entity = repo.Get<EntityType>(id, preload);
            IEntity inherits = entity.Inherits.First();
            Assert.Throws<DataNotLoadedException>(() => { var x = inherits.TypeIds; });
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_EntityTypes()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.EntityTypes, Has.Count.EqualTo(1));
            Assert.That(entity.EntityTypes.First().Id, Is.EqualTo(WellKnownAliases.CurrentTenant.Type));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_EntityTypes_NotLoaded()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id, inherits.id";

            EntityType entity = repo.Get<EntityType>(id, preload);
            IEntity inherits = entity.Inherits.First();
            Assert.Throws<DataNotLoadedException>(() => { var x = inherits.EntityTypes; });
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_IsT_True()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.Is<EntityType>(), Is.True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_IsT_False()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.Is<Report>(), Is.False);
        }

        [Test]
        [ExpectedException(typeof(DataNotLoadedException))]
        [RunAsDefaultTenant]
        public void Test_IsT_NotLoaded()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name";

            IEntity entity = repo.Get(id, preload);
            entity.Is<EntityType>();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AsT_True()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.As<EntityType>(), Is.Not.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AsT_False()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name, isOfType.id";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.As<Report>(), Is.Null);
        }

        [Test]
        [ExpectedException(typeof(DataNotLoadedException))]
        [RunAsDefaultTenant]
        public void Test_AsT_NotLoaded()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Relationship;
            string preload = "name";

            IEntity entity = repo.Get(id, preload);
            entity.As<EntityType>();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetField()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.GetField("core:name"), Is.EqualTo("Type"));
        }

        [Test]
        [ExpectedException(typeof(DataNotLoadedException))]
        [RunAsDefaultTenant]
        public void Test_GetField_NotLoaded()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity.GetField("core:description"), Is.EqualTo("Type"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetRelationship()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name, inherits.alias, derivedTypes.alias";

            IEntity entity = repo.Get(id, preload);
            var fwd = entity.GetRelationships(new EntityRef("core:inherits").Id, Direction.Forward);
            Assert.That(fwd, Is.Not.Empty);
            Assert.That(fwd.Where(e => e.GetField<string>("core:alias") == "core:schema").Count(), Is.EqualTo(1), "A");
            Assert.That(fwd.Where(e => e.GetField<string>("core:alias") == "core:fieldType").Count(), Is.EqualTo(0), "B");

            var rev = entity.GetRelationships(new EntityRef("core:inherits").Id, Direction.Reverse);
            Assert.That(rev, Is.Not.Empty);
            Assert.That(rev.Where(e => e.GetField<string>("core:alias") == "core:schema").Count(), Is.EqualTo(0), "C");
            Assert.That(rev.Where(e => e.GetField<string>("core:alias") == "core:fieldType").Count(), Is.EqualTo(1), "D");
        }

        [Test]
        [ExpectedException(typeof(DataNotLoadedException))]
        [RunAsDefaultTenant]
        public void Test_GetRelationship_NotLoaded_Fwd()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name, derivedTypes.alias";

            IEntity entity = repo.Get(id, preload);
            entity.GetRelationships(new EntityRef("core:inherits").Id, Direction.Forward);
        }

        [Test]
        [ExpectedException(typeof(DataNotLoadedException))]
        [RunAsDefaultTenant]
        public void Test_GetRelationship_NotLoaded_Rev()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name, inherits.alias";

            IEntity entity = repo.Get(id, preload);
            entity.GetRelationships(new EntityRef("core:inherits").Id, Direction.Reverse);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetRelationshipT()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name, isOfType.id, { inherits, derivedTypes }.{ alias, isOfType.id }";

            IEntity entity = repo.Get(id, preload);
            var fwd = entity.GetRelationships<EntityType>(new EntityRef("core:inherits").Id, Direction.Forward);
            Assert.That(fwd, Is.Not.Empty);
            Assert.That(fwd.Where(e => e.Alias == "core:schema").Count(), Is.EqualTo(1), "A");
            Assert.That(fwd.Where(e => e.Alias == "core:fieldType").Count(), Is.EqualTo(0), "B");

            var rev = entity.GetRelationships<EntityType>(new EntityRef("core:inherits").Id, Direction.Reverse);
            Assert.That(rev, Is.Not.Empty);
            Assert.That(rev.Where(e => e.Alias == "core:schema").Count(), Is.EqualTo(0), "C");
            Assert.That(rev.Where(e => e.Alias == "core:fieldType").Count(), Is.EqualTo(1), "D");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Equals()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "isOfType.isOfType.id";

            IEntity entity = repo.Get(id, preload);
            IEntity entity2 = entity.GetRelationships(WellKnownAliases.CurrentTenant.IsOfType, Direction.Forward).First();
            Assert.That(entity, Is.EqualTo(entity2));
        }
    }
}
