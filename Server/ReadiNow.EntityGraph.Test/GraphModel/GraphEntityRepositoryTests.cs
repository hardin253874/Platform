// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using ReadiNow.EntityGraph.GraphModel;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security.AccessControl;

namespace ReadiNow.EntityGraph.Test.GraphModel
{
    [TestFixture]
    class GraphEntityRepositoryTests
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
        public void Get()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity, Is.Not.Null);

            string fieldValue = entity.GetField<string>(new EntityRef("core:name"));
            Assert.That(fieldValue, Is.EqualTo("Type"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        [RunAsDefaultTenant]
        public void Get_NullQuery()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = null;

            repo.Get(id, preload);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Get_InvalidId()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = 1;
            string preload = "name";

            IEntity entity = repo.Get(id, preload);
            Assert.That(entity, Is.Null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        [RunAsDefaultTenant]
        public void Get_TemporaryId()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = new EntityType().Id;
            string preload = "name";

            repo.Get(id, preload);
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetT()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name, isOfType.id";

            EntityType entity = repo.Get<EntityType>(id, preload);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Name, Is.EqualTo("Type"));
        }

        [Test]
        [ExpectedException(typeof(DataNotLoadedException))]
        [RunAsDefaultTenant]
        public void GetT_TypeNotRequested()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name"; // we didn't request isOfType.

            EntityType entity = repo.Get<EntityType>(id, preload);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Name, Is.EqualTo("Type"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetT_InvalidId()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = 1;
            string preload = "name, isOfType.id";

            EntityType entity = repo.Get<EntityType>(id, preload);
            Assert.That(entity, Is.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetT_WrongType()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long id = WellKnownAliases.CurrentTenant.Type;
            string preload = "name, isOfType.id";

            Report entity = repo.Get<Report>(id, preload);
            Assert.That(entity, Is.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Get_Enum()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long[] ids = new long[] { WellKnownAliases.CurrentTenant.Type, WellKnownAliases.CurrentTenant.Relationship };
            string preload = "name, isOfType.id";

            var results = repo.Get(ids, preload);
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(2));
            var list = results.ToList();
            Assert.That(list[0], Is.Not.Null);
            Assert.That(list[1], Is.Not.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetT_Enum()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long[] ids = new long[] { WellKnownAliases.CurrentTenant.Type, WellKnownAliases.CurrentTenant.Relationship };
            string preload = "name, isOfType.id";

            var results = repo.Get<EntityType>(ids, preload);
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(2));
            var list = results.ToList();
            Assert.That(list[0], Is.Not.Null);
            Assert.That(list[0].Name, Is.EqualTo( "Type"));
            Assert.That(list[1], Is.Not.Null);
            Assert.That(list[1].Name, Is.EqualTo("Relationship"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetT_Enum_SomeNotFound()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long[] ids = new long[] { WellKnownAliases.CurrentTenant.Type, 1 };
            string preload = "name, isOfType.id";

            var results = repo.Get<EntityType>(ids, preload);
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(1));
            var list = results.ToList();
            Assert.That(list[0], Is.Not.Null);
            Assert.That(list[0].Name, Is.EqualTo("Type"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]  // Is this what we want? Or should it just return empty
        [RunAsDefaultTenant]
        public void GetT_Enum_EmptyList()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            long[] ids = new long[] { };
            string preload = "name, isOfType.id";

            var results = repo.Get<EntityType>(ids, preload);
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(0));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        [RunAsDefaultTenant]
        public void GetT_Enum_Null()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = MockEntityAccessControlService;

            string preload = "name, isOfType.id";
            var results = repo.Get<EntityType>(null, preload);
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        [RunAsDefaultTenant]
        public void EntityAccessControlService_NotSet()
        {
            var repo = new GraphEntityRepository();
            string preload = "name";
            repo.Get(1, preload);
        }

        #region Unsupported operations on IEntityRepository
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Unsupported_Create()
        {
            var repo = GraphEntityRepository;
            repo.Create(1);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Unsupported_CreateT()
        {
            var repo = GraphEntityRepository;
            repo.Create<Report>();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Unsupported_Get_NoPreloadQuery()
        {
            var repo = GraphEntityRepository;
            repo.Get(1);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Unsupported_GetT_NoPreloadQuery()
        {
            var repo = GraphEntityRepository;
            repo.Get<Report>(1);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Unsupported_Get_Enum_NoPreloadQuery()
        {
            var repo = GraphEntityRepository;
            repo.Get(new long[] { 1 });
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Unsupported_GetT_Enum_NoPreloadQuery()
        {
            var repo = GraphEntityRepository;
            repo.Get<Report>(new long[] { 1 });
        }
        #endregion

    }
}
