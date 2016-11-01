// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    public class SelfServeTests
    {
        private UserAccount _selfServeUser;
        private UserAccount _selfServeUser2;
        private UserAccount _adminUser;

        [TestFixtureSetUp]
        public void Setup( )
        {
            using ( new TenantAdministratorContext( RunAsDefaultTenant.DefaultTenantName ) )
            {
                _selfServeUser = CreateSelfServeUser( "user1" );
                _selfServeUser2 = CreateSelfServeUser( "user2" );
                _adminUser = Entity.Get<UserAccount>( "core:administratorUserAccount" );
            }
        }

        [TestFixtureTearDown]
        public void Teardown( )
        {
            using ( new TenantAdministratorContext( RunAsDefaultTenant.DefaultTenantName ) )
            {
                _selfServeUser?.Delete( );
                _selfServeUser2?.Delete( );
            }
        }

        [TestCase( "console:screen" )]
        [TestCase( "core:report" )]
        [TestCase( "core:chart" )]
        [TestCase( "core:reportColumn" )]
        [TestCase( "core:chartSeries" )]
        [TestCase( "console:reportRenderControl" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_Can_CreateComponents( string alias )
        {
            using ( new SetUser( _selfServeUser ) )
            {
                IEntity entity = Entity.Create( alias );
                Assert.That( entity, Is.Not.Null );
            }
        }

        [TestCase( "console:screen" )]
        [TestCase( "core:report" )]
        [TestCase( "core:chart" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_Can_ReadOwnComponents( string alias )
        {
            using ( new SetUser( _selfServeUser ) )
            {
                IEntity entity = CreatePrivate( alias );

                IEntity entityRead = Entity.Get( entity.Id );
                Assert.That( entityRead, Is.Not.Null );
            }
        }

        [TestCase( "console:screen" )]
        [TestCase( "core:report" )]
        [TestCase( "core:chart" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_Can_ModifyOwnComponents( string alias )
        {
            using ( new SetUser( _selfServeUser ) )
            {
                IEntity entity = CreatePrivate( alias );

                IEntity entityModify = Entity.Get( entity.Id, true );
                entityModify.SetField( "core:name", "Something" );
                entityModify.Save( );
            }
        }

        [TestCase( "console:screen" )]
        [TestCase( "core:report" )]
        [TestCase( "core:chart" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_Can_DeleteOwnComponents( string alias )
        {
            using ( new SetUser( _selfServeUser ) )
            {
                IEntity entity = CreatePrivate( alias );

                Entity.Delete( entity.Id );
            }
        }

        [TestCase( "console:screen", "user", "user2" )]
        [TestCase( "core:report", "user", "user2" )]
        [TestCase( "core:chart", "user", "user2" )]
        [TestCase( "console:screen", "user", "admin" )]
        [TestCase( "core:report", "user", "admin" )]
        [TestCase( "core:chart", "user", "admin" )]
        [TestCase( "console:screen", "admin", "user2" )]
        [TestCase( "core:report", "admin", "user2" )]
        [TestCase( "core:chart", "admin", "user2" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_CanNot_ReadEachOther( string alias, string createdBy, string accessedBy )
        {
            UserAccount user1 = createdBy == "user" ? _selfServeUser : _adminUser;
            UserAccount user2 = accessedBy == "user2" ? _selfServeUser2 : _adminUser;
            long entityId;

            using ( new SetUser( user1 ) )
            {
                IEntity entity = CreatePrivate( alias, createdBy == "user" );
                entityId = entity.Id;
            }
            using ( new SetUser( user2 ) )
            {
                Assert.Throws<PlatformSecurityException>( ( ) => Entity.Get( entityId ) );
            }
        }

        [TestCase( "console:screen", "user", "user2" )]
        [TestCase( "core:report", "user", "user2" )]
        [TestCase( "core:chart", "user", "user2" )]
        [TestCase( "console:screen", "user", "admin" )]
        [TestCase( "core:report", "user", "admin" )]
        [TestCase( "core:chart", "user", "admin" )]
        [TestCase( "console:screen", "admin", "user2" )]
        [TestCase( "core:report", "admin", "user2" )]
        [TestCase( "core:chart", "admin", "user2" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_CanNot_DeleteEachOther( string alias, string createdBy, string accessedBy )
        {
            UserAccount user1 = createdBy == "user" ? _selfServeUser : _adminUser;
            UserAccount user2 = accessedBy == "user2" ? _selfServeUser2 : _adminUser;
            long entityId;

            using ( new SetUser( user1 ) )
            {
                IEntity entity = CreatePrivate( alias, createdBy == "user" );
                entityId = entity.Id;
            }
            using ( new SetUser( user2 ) )
            {
                Assert.Throws<PlatformSecurityException>( ( ) => Entity.Delete( entityId ) );
            }
        }

        [TestCase( "core:report", "console:reportRenderControl", "console:reportToRender" )]
        [TestCase( "core:chart", "console:chartRenderControl", "console:chartToRender" )]
        [TestCase( "console:customEditForm", "console:formRenderControl", "console:formToRender" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_CannotModify_PublicComponentOnPrivateScreen( string componentAlias, string controlAlias, string relAlias )
        {
            IEntity publicComponent;
            IEntity control;
            IEntity screen;

            using ( new SetUser( _adminUser ) )
            {
                publicComponent = Entity.Create( componentAlias );
                publicComponent.Save( );
            }
            using ( new SetUser( _selfServeUser ) )
            {
                screen = Entity.Create( "console:screen" );
                control = Entity.Create( controlAlias );
                control.SetRelationships( relAlias, new EntityRelationship<IEntity>( publicComponent ).ToEntityRelationshipCollection( ), Direction.Forward );
                screen.SetRelationships( "console:containedControlsOnForm", new EntityRelationship<IEntity>( control ).ToEntityRelationshipCollection( ), Direction.Forward );
                screen.Save( );
                control.Save( );

                // Cannot delete the public component
                Assert.Throws<PlatformSecurityException>( ( ) => Entity.Delete( publicComponent.Id ) );
                // But can delete the control that refers to it
                Entity.Delete( control.Id );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_CannotModify_PublicReportOnPrivateChart( )
        {
            IEntity publicReport;
            IEntity chart;

            using ( new SetUser( _adminUser ) )
            {
                publicReport = Entity.Create( "core:report" );
                publicReport.Save( );
            }
            using ( new SetUser( _selfServeUser ) )
            {
                chart = Entity.Create( "core:chart" );
                chart.SetRelationships( "core:chartReport", new EntityRelationship<IEntity>( publicReport ).ToEntityRelationshipCollection( ), Direction.Forward );
                chart.Save( );

                // Cannot delete the public component
                Assert.Throws<PlatformSecurityException>( ( ) => Entity.Delete( publicReport.Id ) );
                // But can delete the control that refers to it
                Entity.Delete( chart.Id );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SelfServe_CannotModify_PublicDefaultReport( )
        {
            EntityType type;
            Report report;

            using ( new SetUser( _adminUser ) )
            {
                type = Entity.Create<EntityType>( );
                report = Entity.Create<Report>( );
                type.DefaultDisplayReport = report;
                type.DefaultPickerReport = report.As<ResourcePicker>( );
                type.Save( );
            }
            using ( new SetUser( _selfServeUser ) )
            {
                Assert.Throws<PlatformSecurityException>( ( ) => Entity.Delete( report.Id ) );
                Assert.Throws<PlatformSecurityException>( ( ) => Entity.Delete( type.Id ) );
            }
        }


        private UserAccount CreateSelfServeUser( string userName )
        {
            Role role = Entity.Get<Role>( "core:selfServeRole" );
            UserAccount userAccount;

            userAccount = Entity.Create<UserAccount>( );
            userAccount.Name = userName;
            userAccount.UserHasRole.Add( role );
            userAccount.Save( );

            return userAccount;
        }

        private IEntity CreatePrivate( string alias, bool isPrivate = true )
        {
            PrivatelyOwnable entity = Entity.Create( alias ).As<PrivatelyOwnable>( );
            entity.IsPrivatelyOwned = isPrivate;
            entity.Save( );

            return entity;
        }
    }
}
