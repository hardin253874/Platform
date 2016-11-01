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
    public class UserRoleRepositoryTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_User_NoRoles()
        {
            UserRoleRepository roleRepository;
            UserAccount userAccount;
            ISet<long> result;

            using (DatabaseContext.GetContext(true))
            {
                userAccount = Entity.Create<UserAccount>();

                roleRepository = new UserRoleRepository();

                result = roleRepository.GetUserRoles(userAccount.Id);

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo( UserRoleRepository.EveryoneRoles.Count), "Unexpected role ID" );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_User_SingleRole()
        {
            UserRoleRepository roleRepository;
            Role role;
            UserAccount userAccount;
            ISet<long> result;

            using (DatabaseContext.GetContext(true))
            {
                role = Entity.Create<Role>();

                userAccount = Entity.Create<UserAccount>();
                userAccount.UserHasRole.Add(role);
                userAccount.Save();

                roleRepository = new UserRoleRepository();
                result = roleRepository.GetUserRoles(userAccount.Id);

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo( UserRoleRepository.EveryoneRoles.Count + 1) );
                Assert.That(result, Has.Some.EqualTo(role.Id), "Unexpected role ID");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_User_NestedRole()
        {
            UserRoleRepository roleRepository;
            Role parentRole;
            Role childRole;
            UserAccount userAccount;
            ISet<long> result;

            using (DatabaseContext.GetContext(true))
            {
                parentRole = Entity.Create<Role>();
                childRole = Entity.Create<Role>();
                parentRole.IncludesRoles.Add(childRole);
                parentRole.Save();

                userAccount = Entity.Create<UserAccount>();
                userAccount.UserHasRole.Add(childRole);
                userAccount.Save();

                roleRepository = new UserRoleRepository();
                result = roleRepository.GetUserRoles(userAccount.Id);

                Assert.That(result, Is.Not.Null);
                Assert.That( result, Has.Count.EqualTo( UserRoleRepository.EveryoneRoles.Count + 2 ) );
                Assert.That(result, Has.Some.EqualTo(parentRole.Id), "Parent role not found");
                Assert.That(result, Has.Some.EqualTo(childRole.Id), "Child role not found");
            }
        }

        [Test]
        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_Role_NestedRole( bool useChild )
        {
            UserRoleRepository roleRepository;
            Role parentRole;
            Role childRole;
            ISet<long> result;

            using ( DatabaseContext.GetContext( true ) )
            {
                parentRole = Entity.Create<Role>( );
                childRole = Entity.Create<Role>( );
                parentRole.IncludesRoles.Add( childRole );
                parentRole.Save( );

                roleRepository = new UserRoleRepository( );
                result = roleRepository.GetUserRoles( useChild ? childRole.Id : parentRole.Id );

                Assert.That( result, Is.Not.Null );
                Assert.That( result, Has.Some.EqualTo( parentRole.Id ), "Parent role not found" );
                if ( useChild )
                    Assert.That( result, Has.Some.EqualTo( childRole.Id ), "Child role not found" );
                else
                    Assert.That( result, Has.None.EqualTo( childRole.Id ), "Child role unexpected" );

                Assert.That( result, Has.Count.EqualTo( UserRoleRepository.EveryoneRoles.Count + ( useChild ? 2 : 1 ) ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [Explicit]  // This test is currently broken .. there is no cache invalidation on the set of roles that include the 'Everyone' role
        public void Test_User_NestedEveryone( ) // see TFS #27869
        {
            UserRoleRepository roleRepository;
            Role parentRole;
            Role everyoneRole;
            UserAccount userAccount;
            ISet<long> result;

            using ( DatabaseContext.GetContext( true ) )
            {
                parentRole = Entity.Create<Role>( );
                everyoneRole = Entity.Get<Role>( WellKnownAliases.CurrentTenant.EveryoneRole );
                parentRole.IncludesRoles.Add( everyoneRole );
                parentRole.Save( );

                userAccount = Entity.Create<UserAccount>( );
                userAccount.Save( );

                roleRepository = new UserRoleRepository( );
                result = roleRepository.GetUserRoles( userAccount.Id );

                Assert.That( result, Is.Not.Null );
                Assert.That( result, Has.Some.EqualTo( parentRole.Id ), "Parent role not found" );
                Assert.That( result, Has.Some.EqualTo( WellKnownAliases.CurrentTenant.EveryoneRole ), "Child role not found" );
            }
        }
    }
}
