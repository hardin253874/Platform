// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    public class GetEntitiesOfTypeTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_BasicSecurity()
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            IEntity entity2 = null;

            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "GetEntitiesOfType test user " + Guid.NewGuid().ToString();
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

            new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(), 
                TestQueries.EntitiesWithNameA().ToReport());

            // Sanity Check. Check directly to avoid any caching or side effect issue.
            IDictionary<long, bool> results = new EntityAccessControlChecker().CheckAccess(
                new[] { new EntityRef(entity1), new EntityRef(entity2) },
                new[] { Permissions.Read },
                new EntityRef(userAccount));
            Assert.That(results, Has.Exactly(1).Property("Key").EqualTo(entity1.Id).And.Property("Value").True,
                "EntityAccessControlChecker.CheckAccess: No access to Entity ID 1");
            Assert.That(results, Has.Exactly(1).Property("Key").EqualTo(entity2.Id).And.Property("Value").False,
                "EntityAccessControlChecker.CheckAccess: Access to Entity ID 2");

            using (new SetUser(userAccount))
            {
                IEnumerable<IEntity> entities = Entity.GetInstancesOfType(entityType, true, "name");
                Assert.That(entities.Count(), Is.EqualTo(1),
                            "Entity.GetInstancesOfType: Incorrect count");
                Assert.That(entities, Has.Exactly(1).Property("Id").EqualTo(entity1.Id),
                            "Entity.GetInstancesOfType: Incorrect Id");
            }
        }
    }
}
