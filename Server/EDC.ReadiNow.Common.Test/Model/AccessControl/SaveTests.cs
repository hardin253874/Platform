// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EDC.ReadiNow.Test.Model.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    public class SaveTests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase("", false, false)]
        [TestCase("core:read", false, false)]
        [TestCase("core:read,core:modify", true, true)]
        [TestCase("core:read,core:delete", false, false)]
        [TestCase("core:read,core:modify,core:delete", true, true)]
        //[TestCase("core:modify", false, true)]  // unsupported
        //[TestCase("core:delete", false, false)] // unsupported
        //[TestCase( "core:modify,core:delete", false, true )] // unsupported
        public void Test_Save( string permissionAliases, bool canGetEntity, bool canSaveEntity )
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            IEntity entity2 = null;
            IEntity loadedEntity;

            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Test user " + Guid.NewGuid().ToString();
            userAccount.Save();

            entityType = new EntityType();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Save();

            entity1 = Entity.Create(new EntityRef(entityType));
            entity1.SetField("core:name", "A");
            entity1.Save();

            entity2 = Entity.Create(new EntityRef(entityType));
            entity2.SetField("core:name", "B");
            entity2.Save();

            if (permissionAliases.Length > 0)
            {
                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                                                          permissionAliases.Split(',').Select(x => new EntityRef(x)),
                                                          TestQueries.EntitiesWithNameA().ToReport());
            }

            using (new SetUser(userAccount))
            {
                // Only check read permission, even when getting a writable version
                loadedEntity = null;
                Assert.That(() => loadedEntity = Entity.Get<IEntity>(new EntityRef(entity1), true),
                    canGetEntity
                        ? (Constraint) Is.EqualTo(entity1).Using(EntityRefComparer.Instance)
                        : (Constraint) Throws.TypeOf<PlatformSecurityException>(), "Entity 1 Get is incorrect");
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity2), true),
                    Throws.TypeOf<PlatformSecurityException>(), "Entity 2 access failed");

                if (canGetEntity)
                {
                    // Requires modify permission
                    Assert.That(() => loadedEntity.Save(),
                        canSaveEntity
                            ? (Constraint)Throws.Nothing
                            : (Constraint)Throws.TypeOf<PlatformSecurityException>());
                }
            }
        }
    }
}
