// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class CachingUserRoleRepositoryTests
    {
        [Test]
        public void Test_Creation_Null()
        {
            Assert.That(() => new CachingUserRoleRepository(null), 
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("roleRepository"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_User_NoRoles()
        {
            CachingUserRoleRepository cachingRoleRepository;
            UserAccount userAccount;

            using (DatabaseContext.GetContext(true))
            {
                userAccount = Entity.Create<UserAccount>();

                cachingRoleRepository = new CachingUserRoleRepository(new UserRoleRepository());

                Assert.That(cachingRoleRepository.IsCached(userAccount.Id), Is.False, "Cached");
                cachingRoleRepository.GetUserRoles(userAccount.Id);
                Assert.That(cachingRoleRepository.IsCached(userAccount.Id), Is.True, "Not cached");
            }
        }
    }
}
