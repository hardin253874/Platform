// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    [TestFixture]
    [RunWithTransaction]
    public class PerUserRuleSetCacheTest
    {


        [Test]
        [RunAsDefaultTenant]
        public void DifferentUsersWithDifferentRuleSets()
        {
            var cacheFactory = new CacheFactory { CacheName = "DifferentUsersWithDifferentRuleSets", MetadataCache = true };

            var cache = cacheFactory.Create<string, string>();

            UserAccount player1, player2;

            CreateTwoUsersWithDifferentSecurity(out player1, out player2);

            using (new SetUser(player1))
            {
                cache.Add("first", "player1 value");

                Assert.That(cache["first"], Is.EqualTo("player1 value"), "What we put in is what we get out");
            }

            using (new SetUser(player2))
            {
                string val;
                var found = cache.TryGetValue("first", out val);

                Assert.That(found, Is.False, "What the other user put in does not come out");
            }

            using (new SetUser(player1))
            {
                string val;
                var found = cache.TryGetValue("first", out val);

                Assert.That(found, Is.True);
                Assert.That(val, Is.EqualTo("player1 value"), "What the second user put in did not overwrite what the first user put in");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void DifferentUsersWithSameRuleSet()
        {
            var cacheFactory = new CacheFactory { CacheName = "DifferentUsersWithSameRuleSet", MetadataCache = true };

            var cache = cacheFactory.Create<string, string>();

            UserAccount player1, player2;

            CreateTwoUsersWithSameSecurity(out player1, out player2);

            using (new SetUser(player1))
            {
                cache.Add("first", "player1 value");

                Assert.That(cache["first"], Is.EqualTo("player1 value"), "What we put in is what we get out");
            }

            using (new SetUser(player2))
            {
                string val;
                var found = cache.TryGetValue("first", out val);

                Assert.That(found, Is.True, "What the other user put in we get out");
                Assert.That(val, Is.EqualTo("player1 value"), "What the other user put in we get out");
            }
        }


        void CreateTwoUsersWithDifferentSecurity(out UserAccount player1, out UserAccount player2)
        {
            player1 = CreateUserWithRole(CreateRole());
            player2 = CreateUserWithRole(CreateRole());
        }

        void CreateTwoUsersWithSameSecurity(out UserAccount player1, out UserAccount player2)
        {
            var role = new Role();
            var derivedRole = CreateDerivedRole(role);

            player1 = CreateUserWithRole(role);
            player2 = CreateUserWithRole(derivedRole);
        }

      

        UserAccount CreateUserWithRole(Role role)
        {
            UserAccount userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.UserHasRole.Add(role);
            userAccount.Save();
            return userAccount;
        }

        Role CreateRole()
        {
            var role = Entity.Create<Role>();
            role.Name = "test role " + Guid.NewGuid();
            role.Save();

            new AccessRuleFactory().AddAllowReadQuery(role.As<Subject>(),
                    Person.Person_Type.As<SecurableEntity>(), TestQueries.EntitiesWithNameA().ToReport());

            return role;
        }

        Role CreateDerivedRole(Role parent)
        {
            var role = Entity.Create<Role>();
            role.Name = "test role " + Guid.NewGuid();
            role.IncludesRoles.Add(parent);
            role.Save();

            new AccessRuleFactory().AddAllowReadQuery(role.As<Subject>(),
                    Person.Person_Type.As<SecurableEntity>(), TestQueries.EntitiesWithNameA().ToReport());

            return role;
        }
    }
}
