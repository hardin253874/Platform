// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    [FailOnEvent]
    public class CachingUserRuleSetProviderTests
    {
        [Test]
        public void Test_Creation_NullProvider( )
        {
            Assert.That( ( ) => new CachingUserRuleSetProvider( null ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "innerProvider" ) );
        }

        [Test]
        public void Test_Creation( )
        {
            CachingUserRuleSetProvider provider;
            Mock<IUserRuleSetProvider> mockProvider;

            mockProvider = new Mock<IUserRuleSetProvider>( MockBehavior.Strict );

            provider = new CachingUserRuleSetProvider( mockProvider.Object );

            mockProvider.VerifyAll( );
            Assert.That( provider, Has.Property( "Cache" ).Count.EqualTo( 0 ) );
            Assert.That( provider, Has.Property( "InnerProvider" ).EqualTo( mockProvider.Object ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddingNewRole( )
        {
            EntityType entityType1;
            EntityType entityType2;
            UserAccount userAccount;
            Role role1;
            Role role2;
            IUserRuleSetProvider userRuleSetProvider;
            UserRuleSet userRuleSet1;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entityType1 = EDC.ReadiNow.Model.Entity.Create<EntityType>();
                entityType1.Save();
                entityType2 = EDC.ReadiNow.Model.Entity.Create<EntityType>();
                entityType2.Save();

                role1 = new Role();
                role1.Save();
                role2 = new Role();
                role2.Save();

                userAccount = new UserAccount();
                userAccount.UserHasRole.Add(role1);
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(role1.As<Subject>(), entityType1.As<SecurableEntity>(), TestQueries.Entities(entityType1).ToReport());
                new AccessRuleFactory().AddAllowReadQuery(role2.As<Subject>(), entityType2.As<SecurableEntity>(), TestQueries.Entities(entityType2).ToReport());

                userRuleSetProvider = Factory.UserRuleSetProvider;
                userRuleSet1 = userRuleSetProvider.GetUserRuleSet(userAccount.Id, Permissions.Read);

                // Add a new role
                userAccount.UserHasRole.Add(role2);
                userAccount.Save();

                ctx.CommitTransaction();
            }

            UserRuleSet userRuleSet2 = userRuleSetProvider.GetUserRuleSet( userAccount.Id, Permissions.Read );
            Assert.That( userRuleSet1, Is.Not.EqualTo( userRuleSet2 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddingNewRuleToRole( )
        {
            EntityType entityType1;
            EntityType entityType2;
            UserAccount userAccount;
            Role role1;
            UserRuleSet userRuleSet1;
            IUserRuleSetProvider userRuleSetProvider;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entityType1 = EDC.ReadiNow.Model.Entity.Create<EntityType>();
                entityType1.Save();
                entityType2 = EDC.ReadiNow.Model.Entity.Create<EntityType>();
                entityType2.Save();

                role1 = new Role();
                role1.Save();

                userAccount = new UserAccount();
                userAccount.UserHasRole.Add(role1);
                userAccount.Save();

                new AccessRuleFactory().AddAllowReadQuery(role1.As<Subject>(), entityType1.As<SecurableEntity>(), TestQueries.Entities(entityType1).ToReport());

                userRuleSetProvider = Factory.UserRuleSetProvider;
                userRuleSet1 = userRuleSetProvider.GetUserRuleSet(userAccount.Id, Permissions.Read);

                // Add a new rule
                new AccessRuleFactory().AddAllowReadQuery(role1.As<Subject>(), entityType2.As<SecurableEntity>(), TestQueries.Entities(entityType2).ToReport());
                ctx.CommitTransaction();
            }

            UserRuleSet userRuleSet2 = userRuleSetProvider.GetUserRuleSet( userAccount.Id, Permissions.Read );
            Assert.That( userRuleSet1, Is.Not.EqualTo( userRuleSet2 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddingRuleDirectToUser( )
        {
            EntityType entityType;
            UserAccount userAccount;
            Role role1;
            IUserRuleSetProvider userRuleSetProvider;
            UserRuleSet userRuleSet1;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entityType = EDC.ReadiNow.Model.Entity.Create<EntityType>();
                entityType.Save();

                role1 = new Role();
                role1.Save();

                userAccount = new UserAccount();
                userAccount.UserHasRole.Add(role1);
                userAccount.Save();


                new AccessRuleFactory( ).AddAllowReadQuery( role1.As<Subject>( ), entityType.As<SecurableEntity>( ), TestQueries.Entities( entityType ).ToReport( ) );

                userRuleSetProvider = Factory.UserRuleSetProvider;
                userRuleSet1 = userRuleSetProvider.GetUserRuleSet( userAccount.Id, Permissions.Read );

                // Add a new role
                new AccessRuleFactory( ).AddAllowReadQuery( userAccount.As<Subject>( ), entityType.As<SecurableEntity>( ), TestQueries.Entities( entityType ).ToReport( ) );

                ctx.CommitTransaction();
            }

            UserRuleSet userRuleSet2 = userRuleSetProvider.GetUserRuleSet( userAccount.Id, Permissions.Read );
            Assert.That( userRuleSet1, Is.Not.EqualTo( userRuleSet2 ) );
        }
    }
}
