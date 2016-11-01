// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.EventClasses;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    [TestFixture]
	[RunWithTransaction]
    public class TypeAddResourceBaseTypeEventTargetTests
    {
        [Test]
        public void Test_Statics()
        {
            Assert.That(TypeAddResourceBaseTypeEventTarget.CoreInheritsAlias, Is.EqualTo("inherits"), 
                "Invalid core:inherits alias");
            Assert.That(TypeAddResourceBaseTypeEventTarget.CoreNamespace, Is.EqualTo("core"), 
                "Invalid core:inherits namepsace");
            Assert.That(TypeAddResourceBaseTypeEventTarget.CoreResourceAlias, Is.EqualTo("resource"), 
                "Invalid core:resource alias");
            Assert.That(TypeAddResourceBaseTypeEventTarget.CoreNamespace, Is.EqualTo("core"), 
                "Invalid core:resource namepsace");
            Assert.That(TypeAddResourceBaseTypeEventTarget.TenantCoreResourceMap, Is.Not.Null, 
                "Resource map is null");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_OnBeforeSave_Mock()
        {
            TypeAddResourceBaseTypeEventTarget eventTarget;
            IEntity[] entities;
            MockRepository mockRepository;
            Mock<IEntity> fakeCoreResource;
            Mock<IEntityInternal> fakeCoreResourceInternal;
            Mock<IEntity> entityInheriting;
            Mock<IEntity> entityNotInheriting;
            EntityRef coreInherits;

            coreInherits = new EntityRef(TypeAddResourceBaseTypeEventTarget.CoreNamespace, 
                TypeAddResourceBaseTypeEventTarget.CoreInheritsAlias);

            mockRepository = new MockRepository(MockBehavior.Strict);
            fakeCoreResource = mockRepository.Create<IEntity>();
            fakeCoreResource.SetupGet(e => e.Alias).Returns(() => "resource");
            fakeCoreResource.SetupGet(e => e.Namespace).Returns(() => "core");
            fakeCoreResource.Setup(e => e.GetRelationships(
                                It.Is<EntityRef>(er => er.Alias == coreInherits.Alias && er.Namespace == coreInherits.Namespace)))
                            .Returns(() => new EntityRelationshipCollection<IEntity>());

            fakeCoreResourceInternal = fakeCoreResource.As<IEntityInternal>();
            fakeCoreResourceInternal.SetupGet(e => e.MutableId).Returns(() => new MutableIdKey(1));

            entityInheriting = mockRepository.Create<IEntity>();
            entityInheriting.SetupGet(e => e.Alias).Returns(() => "foo");
            // entityInheriting.SetupGet(e => e.Namespace).Returns(() => "bar"); // Checks alias first in lazy evaluation
            entityInheriting.Setup(e => e.GetRelationships(
                                It.Is<EntityRef>(er => er.Alias == coreInherits.Alias && er.Namespace == coreInherits.Namespace)))
                            .Returns(() => new EntityRelationshipCollection<IEntity>
                                {
                                    fakeCoreResource.Object
                                });

            entityNotInheriting = mockRepository.Create<IEntity>();
            entityNotInheriting.SetupGet(e => e.Alias).Returns(() => "foo");
            // entityNotInheriting.SetupGet(e => e.Namespace).Returns(() => "bar"); // Checks alias first in lazy evaluation
            entityNotInheriting.Setup(e => e.GetRelationships(
                                    It.Is<EntityRef>(er => er.Alias == coreInherits.Alias && er.Namespace == coreInherits.Namespace)))
                              .Returns(() => new EntityRelationshipCollection<IEntity>());
            entityNotInheriting.Setup(e => e.SetRelationships(
                                    It.Is<EntityRef>(er => er.Alias == coreInherits.Alias && er.Namespace == coreInherits.Namespace),
                                    It.Is<IEntityRelationshipCollection<IEntity>>(
                                        erc => erc.Entities.Count == 1 
                                            && erc.Entities[0].Alias == TypeAddResourceBaseTypeEventTarget.CoreResourceAlias
                                            && erc.Entities[0].Namespace == TypeAddResourceBaseTypeEventTarget.CoreNamespace)));

            entities = new []
            {
                fakeCoreResource.Object,
                entityInheriting.Object,
                entityNotInheriting.Object
            };

            eventTarget = new TypeAddResourceBaseTypeEventTarget();

            Assert.That(eventTarget.OnBeforeSave(entities, null), Is.False);
            mockRepository.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_OnBeforeSave_Integration()
        {
            TypeAddResourceBaseTypeEventTarget eventTarget;
            EntityType[] entities;
            EntityType coreResource;
            EntityType entityInheriting;
            EntityType entityNotInheriting;
            EntityRef coreResourceRef;

            coreResourceRef = new EntityRef(TypeAddResourceBaseTypeEventTarget.CoreNamespace,
                TypeAddResourceBaseTypeEventTarget.CoreResourceAlias);

            coreResource = Entity.Get<EntityType>(coreResourceRef);

            entityInheriting = Entity.Create<EntityType>();
            entityInheriting.Inherits.Add(coreResource);
            entityInheriting.Save();

            entityNotInheriting = Entity.Create<EntityType>();
            entityNotInheriting.Save();

            entities = new[]
            {
                coreResource,
                entityInheriting,
                entityNotInheriting
            };

            eventTarget = new TypeAddResourceBaseTypeEventTarget();

            Assert.That(eventTarget.OnBeforeSave(entities, null), Is.False);
            Assert.That(coreResource, Has.Property("Inherits").Count.EqualTo(0), 
                "coreResource has inheritance added");
            Assert.That(entityInheriting, Has.Property("Inherits").Count.EqualTo(1),
                "entityInheriting has inheritance added");
            Assert.That(entityNotInheriting, Has.Property("Inherits").Count.EqualTo(1),
                "entityNotInheriting has not had inheritance added");
        }

        [Test]
        [TestCase("", "resource", false)]
        [TestCase("core", "", false)]
        [TestCase("core", "resource", true)]
        public void Test_IsCoreResource(string ns, string alias, bool expectedResult)
        {
            Mock<IEntity> entity;

            entity = new Mock<IEntity>(MockBehavior.Loose);
            entity.SetupGet(e => e.Alias).Returns(() => alias);
            entity.SetupGet(e => e.Namespace).Returns(() => ns);

            Assert.That(new TypeAddResourceBaseTypeEventTarget().IsCoreResource(entity.Object),
                Is.EqualTo(expectedResult));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TypeOnBeforeSave()
        {
            IEnumerable<Target> onBeforeSave =
                Entity.Get("core:type").GetRelationships("core:onBeforeSave").Entities.Select(x => x.As<Target>());
            Assert.That(onBeforeSave,
                Has.Exactly(1).Property("Alias").EqualTo("core:addResourceBaseType"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NewTypeInheritsFromResource()
        {
            EntityType entityType;

            entityType = Entity.Create<EntityType>();
            entityType.Save();

            Assert.That(entityType.Inherits, Has.Exactly(1).Property("Alias").EqualTo(
                TypeAddResourceBaseTypeEventTarget.CoreNamespace + ":" + TypeAddResourceBaseTypeEventTarget.CoreResourceAlias));
        }
    }
}
