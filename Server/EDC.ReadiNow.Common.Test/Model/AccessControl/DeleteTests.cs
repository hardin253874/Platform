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
    public class DeleteTests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase("", false)]
        [TestCase("core:read", false)]
        [TestCase("core:read,core:modify", false)]
        [TestCase("core:read,core:delete", true)]
        [TestCase("core:read,core:modify,core:delete", true)]
        //[TestCase("core:modify", false)] // unsupported
        //[TestCase("core:delete", true)]  // unsupported
        //[TestCase("core:modify,core:delete", true)] // unsupported
        public void Test_Delete(string permissionAliases, bool canDeleteEntity1)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            IEntity entity2 = null;

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
                Assert.That(() => entity1.Delete(),
                    canDeleteEntity1
                        ? (Constraint) Throws.Nothing
                        : (Constraint) Throws.TypeOf<PlatformSecurityException>());
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity2)),
                    Throws.TypeOf<PlatformSecurityException>(), "Entity 2 delete somehow worked");
            }            
        }
    }
}
