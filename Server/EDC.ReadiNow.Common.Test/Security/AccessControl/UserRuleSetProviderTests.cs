// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Security;
using NUnit.Framework;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    public class UserRuleSetProviderTests
    {
        [Test]
        public void Test_Null_IUserRoleRepository( )
        {
            Assert.Throws<ArgumentNullException>( ( ) => new UserRuleSetProvider( null, new Mock<IRuleRepository>( ).Object ) );
        }

        [Test]
        public void Test_Null_IRuleRepository( )
        {
            Assert.Throws<ArgumentNullException>( ( ) => new UserRuleSetProvider( new Mock<IUserRoleRepository>( ).Object, null ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_IUserRoleRepository_ReturnsNull( )
        {
            long userId = 1;
            Mock<IUserRoleRepository> mockRoles = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            Mock<IRuleRepository> mockRules = new Mock<IRuleRepository>( MockBehavior.Strict );

            mockRoles.Setup( x => x.GetUserRoles( userId ) ).Returns( ( ISet<long> ) null );

            UserRuleSetProvider userRuleSetprovider = new UserRuleSetProvider( mockRoles.Object, mockRules.Object );

            Assert.Throws<InvalidOperationException>( ( ) => userRuleSetprovider.GetUserRuleSet( userId, Permissions.Read ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_IRuleRepository_ReturnsNull( )
        {
            long userId = 1;
            Mock<IUserRoleRepository> mockRoles = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            Mock<IRuleRepository> mockRules = new Mock<IRuleRepository>( MockBehavior.Strict );

            mockRoles.Setup( x => x.GetUserRoles( userId ) ).Returns( new HashSet<long>( ) );
            mockRules.Setup( x => x.GetAccessRules( userId, Permissions.Read, null ) ).Returns( (ICollection<AccessRule>)null );

            UserRuleSetProvider userRuleSetprovider = new UserRuleSetProvider( mockRoles.Object, mockRules.Object );

            Assert.Throws<InvalidOperationException>( ( ) => userRuleSetprovider.GetUserRuleSet( userId, Permissions.Read ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_SingleRuleOnSubject( )
        {
            long userId = 1;
            AccessRule accessRule = new AccessRule( );
            Mock<IUserRoleRepository> mockRoles = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            Mock<IRuleRepository> mockRules = new Mock<IRuleRepository>( MockBehavior.Strict );
            UserRuleSet userRuleSet;

            mockRoles.Setup( x => x.GetUserRoles( userId ) ).Returns( new HashSet<long>( ) );
            mockRules.Setup( x => x.GetAccessRules( userId, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule } );

            UserRuleSetProvider userRuleSetprovider = new UserRuleSetProvider( mockRoles.Object, mockRules.Object );

            userRuleSet = null;
            Assert.That( ( ) => userRuleSet = userRuleSetprovider.GetUserRuleSet( userId, Permissions.Read ), Throws.Nothing );
            Assert.That( userRuleSet, Is.Not.Null );

            mockRoles.VerifyAll( );
            mockRules.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_WithRoleWithOverlappingRules( )
        {
            long userId = 1;
            long roleId = 2;
            AccessRule accessRule1 = new AccessRule( );
            AccessRule accessRule2 = new AccessRule( );
            AccessRule accessRule3 = new AccessRule( );
            Mock<IUserRoleRepository> mockRoles = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            Mock<IRuleRepository> mockRules = new Mock<IRuleRepository>( MockBehavior.Strict );
            UserRuleSet userRuleSet;

            mockRoles.Setup( x => x.GetUserRoles( userId ) ).Returns( new HashSet<long> { roleId } );
            mockRules.Setup( x => x.GetAccessRules( userId, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule1, accessRule2 } );
            mockRules.Setup( x => x.GetAccessRules( roleId, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule2, accessRule3 } );

            UserRuleSetProvider userRuleSetprovider = new UserRuleSetProvider( mockRoles.Object, mockRules.Object );

            userRuleSet = null;
            Assert.That( ( ) => userRuleSet = userRuleSetprovider.GetUserRuleSet( userId, Permissions.Read ), Throws.Nothing );
            Assert.That( userRuleSet, Is.Not.Null );
            Assert.That( userRuleSet.Key, Is.EqualTo( CryptoHelper.HashValues( new []{ accessRule1.Id, accessRule2.Id, accessRule3.Id }.OrderBy( id => id ) ) ) );

            mockRoles.VerifyAll( );
            mockRules.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TwoUsersSameTwoRoles( )
        {
            long user1 = 1;
            long user2 = 2;
            long role1 = 3;
            long role2 = 4;
            AccessRule accessRule1 = new AccessRule( );
            AccessRule accessRule2 = new AccessRule( );
            AccessRule accessRule3 = new AccessRule( );
            Mock<IUserRoleRepository> mockRoles = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            Mock<IRuleRepository> mockRules = new Mock<IRuleRepository>( MockBehavior.Strict );

            mockRoles.Setup( x => x.GetUserRoles( user1 ) ).Returns( new HashSet<long> { role1, role2 } );
            mockRoles.Setup( x => x.GetUserRoles( user2 ) ).Returns( new HashSet<long> { role1, role2 } );
            mockRules.Setup( x => x.GetAccessRules( user1, Permissions.Read, null ) ).Returns( new List<AccessRule> { } );
            mockRules.Setup( x => x.GetAccessRules( user2, Permissions.Read, null ) ).Returns( new List<AccessRule> { } );
            mockRules.Setup( x => x.GetAccessRules( role1, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule1  } );
            mockRules.Setup( x => x.GetAccessRules( role2, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule2, accessRule3 } );

            UserRuleSetProvider userRuleSetprovider = new UserRuleSetProvider( mockRoles.Object, mockRules.Object );

            UserRuleSet userRuleSet1 = userRuleSetprovider.GetUserRuleSet( user1, Permissions.Read );
            UserRuleSet userRuleSet2 = userRuleSetprovider.GetUserRuleSet( user2, Permissions.Read );

            Assert.That( userRuleSet1, Is.EqualTo( userRuleSet2 ) );
            Assert.That( userRuleSet1.GetHashCode( ), Is.EqualTo( userRuleSet2.GetHashCode( ) ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TwoUsersDifferentRoles( )
        {
            long user1 = 1;
            long user2 = 2;
            long role1 = 3;
            long role2 = 4;
            AccessRule accessRule1 = new AccessRule( );
            AccessRule accessRule2 = new AccessRule( );
            AccessRule accessRule3 = new AccessRule( );
            Mock<IUserRoleRepository> mockRoles = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            Mock<IRuleRepository> mockRules = new Mock<IRuleRepository>( MockBehavior.Strict );

            mockRoles.Setup( x => x.GetUserRoles( user1 ) ).Returns( new HashSet<long> { role1 } );
            mockRoles.Setup( x => x.GetUserRoles( user2 ) ).Returns( new HashSet<long> { role1, role2 } );
            mockRules.Setup( x => x.GetAccessRules( user1, Permissions.Read, null ) ).Returns( new List<AccessRule> { } );
            mockRules.Setup( x => x.GetAccessRules( user2, Permissions.Read, null ) ).Returns( new List<AccessRule> { } );
            mockRules.Setup( x => x.GetAccessRules( role1, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule1 } );
            mockRules.Setup( x => x.GetAccessRules( role2, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule2, accessRule3 } );

            UserRuleSetProvider userRuleSetprovider = new UserRuleSetProvider( mockRoles.Object, mockRules.Object );

            UserRuleSet userRuleSet1 = userRuleSetprovider.GetUserRuleSet( user1, Permissions.Read );
            UserRuleSet userRuleSet2 = userRuleSetprovider.GetUserRuleSet( user2, Permissions.Read );

            Assert.That( userRuleSet1, Is.Not.EqualTo( userRuleSet2 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TwoUsersSameTwoRoles_OneHasOwnRule( )
        {
            long user1 = 1;
            long user2 = 2;
            long role1 = 3;
            long role2 = 4;
            AccessRule accessRule1 = new AccessRule( );
            AccessRule accessRule2 = new AccessRule( );
            AccessRule accessRule3 = new AccessRule( );
            AccessRule accessRule4 = new AccessRule( );
            Mock<IUserRoleRepository> mockRoles = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            Mock<IRuleRepository> mockRules = new Mock<IRuleRepository>( MockBehavior.Strict );

            mockRoles.Setup( x => x.GetUserRoles( user1 ) ).Returns( new HashSet<long> { role1, role2 } );
            mockRoles.Setup( x => x.GetUserRoles( user2 ) ).Returns( new HashSet<long> { role1, role2 } );
            mockRules.Setup( x => x.GetAccessRules( user1, Permissions.Read, null ) ).Returns( new List<AccessRule> { } );
            mockRules.Setup( x => x.GetAccessRules( user2, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule4  } );
            mockRules.Setup( x => x.GetAccessRules( role1, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule1 } );
            mockRules.Setup( x => x.GetAccessRules( role2, Permissions.Read, null ) ).Returns( new List<AccessRule> { accessRule2, accessRule3 } );

            UserRuleSetProvider userRuleSetprovider = new UserRuleSetProvider( mockRoles.Object, mockRules.Object );

            UserRuleSet userRuleSet1 = userRuleSetprovider.GetUserRuleSet( user1, Permissions.Read );
            UserRuleSet userRuleSet2 = userRuleSetprovider.GetUserRuleSet( user2, Permissions.Read );

            Assert.That( userRuleSet1, Is.Not.EqualTo( userRuleSet2 ) );
        }
    }
}
