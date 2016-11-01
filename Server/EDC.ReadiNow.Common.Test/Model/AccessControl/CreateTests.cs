// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EDC.ReadiNow.Test.Model.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    public class CreateTests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_BasicSave(bool allowCreate)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity = null;
            string userName;

            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Test user " + Guid.NewGuid().ToString();
            userAccount.Save();

            entityType = new EntityType();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Save();

            if (allowCreate)
            {
                new AccessRuleFactory().AddAllow(userAccount.As<Subject>(), 
                    new [] { Permissions.Create }, entityType.As<SecurableEntity>());
            }

            userName = userAccount.Name;
            using (new SetUser(userAccount))
            {
                Assert.That(() => entity = Entity.Create(entityType),
                    Throws.Nothing);
                Assert.That(() => entity.Save(),
                    allowCreate ? (Constraint) Throws.Nothing 
                        : Throws.TypeOf<PlatformSecurityException>()
                            .And.EqualTo(new PlatformSecurityException(userName, new[] { Permissions.Create }, new [] { new EntityRef(entity)  })));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource("Test_TemporaryIds_Source")]
        [Ignore("Don't ignore when merging into trunk")]
        public void Test_TemporaryIds(long id, bool shouldRaiseException)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            MockRepository mockRepository;
            Mock<IEntity> mockEntity = null;
            Mock<IEntityInternal> mockEntityInternal = null;
            EntityModificationToken entityModificationToken;

            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Test user " + Guid.NewGuid().ToString();
            userAccount.Save();

            entityType = new EntityType();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Save();

            new AccessRuleFactory().AddAllowCreate(userAccount.As<Subject>(),
                entityType.As<SecurableEntity>());

            // Mock an Entity. Yes, I am completely insane.
            mockRepository = new MockRepository(MockBehavior.Loose);
            mockEntity = mockRepository.Create<IEntity>();
            mockEntity.SetupGet(e => e.TypeIds).Returns(() => new[] { entityType.Id });
            mockEntity.SetupGet(e => e.EntityTypes).Returns(() => new[] { entityType });
            mockEntity.Setup(e => e.IsReadOnly).Returns(() => false);
            mockEntity.SetupGet(e => e.Id).Returns(() => id);
            mockEntityInternal = mockEntity.As<IEntityInternal>();
            mockEntityInternal.SetupGet(ei => ei.IsTemporaryId).Returns(() => EntityTemporaryIdAllocator.IsAllocatedId(id));
            entityModificationToken = new EntityModificationToken();
            mockEntityInternal.SetupGet(ei => ei.ModificationToken).Returns(() => entityModificationToken);

            using (new SetUser(userAccount))
            {
                Assert.That(() => Entity.Save(new [] { mockEntity.Object }, false),
                    shouldRaiseException ? (Constraint)Throws.Nothing : Throws.TypeOf<PlatformSecurityException>());
            }
        }

        protected IEnumerable Test_TemporaryIds_Source()
        {
            yield return new TestCaseData(0, false);
            yield return new TestCaseData(EntityId.MinTemporary - 1, false);
            yield return new TestCaseData(EntityId.MinTemporary, true);
            yield return new TestCaseData(EntityId.MinTemporary + 1, true);
            yield return new TestCaseData(EntityId.Max - 1, true);
            // yield return new TestCaseData(EntityId.Max, true); // Tends to alternate between failing and succeeding
            yield return new TestCaseData(EntityId.Max + 1, false);
            yield return new TestCaseData(long.MaxValue, false);
        }        
    }
}
