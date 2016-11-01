// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class UserRuleSetsHasherTest
    {
        class DummyRuleSetProvider : Dictionary<Tuple<long, EntityRef>, UserRuleSet>, IUserRuleSetProvider
        {

            public void Add(long userId, EntityRef permission, UserRuleSet rs)
            {
                var key = new Tuple<long, EntityRef>(userId, permission);
                base.Add(key, rs);
            }

            public UserRuleSet GetUserRuleSet(long userId, EntityRef permission)
            {
                var key = new Tuple<long, EntityRef>(userId, permission);
                UserRuleSet rs;
                if (!base.TryGetValue(key, out rs))
                {
                    rs = new UserRuleSet(new List<long>());
                }
                    
                return rs;
            }

        }

        [Test]
        public void DifferentSetsDifferent()
        {
            var prov = new DummyRuleSetProvider();
            prov.Add(1, Permissions.Read, new UserRuleSet(new List<long> { 100 }));
            prov.Add(2, Permissions.Read, new UserRuleSet(new List<long> { 101 }));

            var hasher = new UserRuleSetsHasher(prov);

            Assert.That(hasher.GetUserRuleSetsHash(1), Is.Not.EqualTo(hasher.GetUserRuleSetsHash(2)));
        }

        [Test]
        public void EmptySetsSame()
        {
            var prov = new DummyRuleSetProvider();
            prov.Add(1, Permissions.Read, new UserRuleSet(new List<long>()));
            prov.Add(2, Permissions.Read, new UserRuleSet(new List<long>()));

            var hasher = new UserRuleSetsHasher(prov);

            Assert.That(hasher.GetUserRuleSetsHash(1), Is.EqualTo(hasher.GetUserRuleSetsHash(2)));
        }

        [Test]
        public void ReadAndWriteSetsDifferent()
        {
            var rs = new UserRuleSet(new List<long> { 100, 101 });
            var prov = new DummyRuleSetProvider();
            prov.Add(1, Permissions.Read, rs);
            prov.Add(2, Permissions.Modify, rs);

            var hasher = new UserRuleSetsHasher(prov);

            Assert.That(hasher.GetUserRuleSetsHash(1), Is.Not.EqualTo(hasher.GetUserRuleSetsHash(2)));
        }
    }
}
