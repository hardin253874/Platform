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
using EDC.ReadiNow.Security;

namespace ReadiNow.EntityGraph.Test.GraphModel
{
    [TestFixture]
    class GraphSecurityTests
    {
        public GraphEntityRepository GraphEntityRepository
        {
            get { return new GraphEntityRepository(); }
        }

        public IEntityAccessControlService NoPermissionToTypeEntity
        {
            get
            {
                long typeId = new EntityRef("core:type").Id;

                // Pass through requests
                Mock<IEntityAccessControlService> mock = new Mock<IEntityAccessControlService>();
                mock.Setup(m => m.Check(It.IsAny<IList<EntityRef>>(), It.IsAny<IList<EntityRef>>()))
                    .Returns((IList<EntityRef> arg1, IList<EntityRef> arg2) => arg1.ToDictionary(k => k.Id, k => k.Id != typeId));
                mock.Setup(m => m.Demand(It.IsAny<IList<EntityRef>>(), It.IsAny<IList<EntityRef>>())).Callback(
                    (IList<EntityRef> arg1, IList<EntityRef> arg2) =>
                    {
                        if (arg1.Select(k => k.Id).Contains(typeId))
                            throw new PlatformSecurityException();
                    });
                return mock.Object;
            }
        }

        [Test]
        [ExpectedException(typeof(PlatformSecurityException))]
        [RunAsDefaultTenant]
        public void Test_Get_Denied()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long id = WellKnownAliases.CurrentTenant.Type;

            repo.Get(id, "name");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Get_Denied_SecurityBypassContext()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long id = WellKnownAliases.CurrentTenant.Type;

            using (new SecurityBypassContext())
            {
                var res = repo.Get(id, "name");
                Assert.That(res, Is.Not.Null);
            }                
        }

        [Test]
        [ExpectedException(typeof(PlatformSecurityException))]
        [RunAsDefaultTenant]
        public void Test_GetT_Denied()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long id = WellKnownAliases.CurrentTenant.Type;

            repo.Get<EntityType>(id, "name");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetT_Denied_SecurityBypassContext()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long id = WellKnownAliases.CurrentTenant.Type;

            using (new SecurityBypassContext())
            {
                var res = repo.Get<EntityType>(id, "name, isOfType.id");
                Assert.That(res, Is.Not.Null);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetT_Enum_SomeDenied()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long[] ids = new long[] { WellKnownAliases.CurrentTenant.Type, WellKnownAliases.CurrentTenant.Relationship };
            string preload = "name, isOfType.id";

            var results = repo.Get<EntityType>(ids, preload);
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(1));
            var list = results.ToList();
            Assert.That(list[0], Is.Not.Null);
            Assert.That(list[0].Name, Is.EqualTo("Relationship"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetT_Enum_SomeDenied_SecurityBypassContext()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long[] ids = new long[] { WellKnownAliases.CurrentTenant.Type, WellKnownAliases.CurrentTenant.Relationship };
            string preload = "name, isOfType.id";

            using (new SecurityBypassContext())
            {
                var results = repo.Get<EntityType>(ids, preload);
                Assert.That(results, Is.Not.Null);
                Assert.That(results, Has.Count.EqualTo(2));
            }
        }


        [Test]
        [RunAsDefaultTenant]
        public void GetT_Enum_AllDenied()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long[] ids = new long[] { WellKnownAliases.CurrentTenant.Type };
            string preload = "name, isOfType.id";

            var results = repo.Get<EntityType>(ids, preload);
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(0));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetRelationship_ImpliesSecurity()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long id = new EntityRef("core:fieldType").Id;
            string preload = "name, inherits.alias";            // inherits implies security in this direction

            IEntity entity = repo.Get(id, preload);
            var fwd = entity.GetRelationships(new EntityRef("core:inherits").Id, Direction.Forward);
            Assert.That(fwd.Where(e => e.GetField<string>("core:alias") == "core:type").Count(), Is.EqualTo(1));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetRelationship()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long id = new EntityRef("core:schema").Id;
            string preload = "name, derivedTypes.alias";            // inherits implies security in this direction

            IEntity entity = repo.Get(id, preload);
            var fwd = entity.GetRelationships(new EntityRef("core:inherits").Id, Direction.Reverse);
            Assert.That(fwd.Where(e => e.GetField<string>("core:alias") == "core:type").Count(), Is.EqualTo(0));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetRelationship_SecurityBypassContext()
        {
            var repo = GraphEntityRepository;
            repo.EntityAccessControlService = NoPermissionToTypeEntity;

            long id = new EntityRef("core:fieldType").Id;
            string preload = "name, inherits.alias";

            IEntity entity = repo.Get(id, preload);

            using (new SecurityBypassContext())
            {
                var fwd = entity.GetRelationships(new EntityRef("core:inherits").Id, Direction.Forward);
                Assert.That(fwd.Where(e => e.GetField<string>("core:alias") == "core:type").Count(), Is.EqualTo(1));
            }
        }

    }
}
