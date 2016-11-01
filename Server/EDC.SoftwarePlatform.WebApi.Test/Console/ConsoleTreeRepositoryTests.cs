// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Console;
using Moq;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Console
{
    using Report = EDC.ReadiNow.Model.Report;

    [TestFixture]
    public class ConsoleTreeRepositoryTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Ctor_Null( )
        {            
            Assert.That(
                ( ) => new ConsoleTreeRepository( null ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "userRoleRepository" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Ctor( )
        {
            IUserRoleRepository userRoleRepository;
            ConsoleTreeRepository consoleTreeRepository;

            userRoleRepository = new Mock<IUserRoleRepository>( MockBehavior.Strict ).Object;
            consoleTreeRepository = new ConsoleTreeRepository( userRoleRepository );

            Assert.That( consoleTreeRepository,
                Has.Property( "UserRoleRepository" ).EqualTo( userRoleRepository ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ShouldPrune_NullEntityData( )
        {
            IUserRoleRepository userRoleRepository;
            ConsoleTreeRepository consoleTreeRepository;

            userRoleRepository = new Mock<IUserRoleRepository>( MockBehavior.Strict ).Object;
            consoleTreeRepository = new ConsoleTreeRepository( userRoleRepository );
            long userId = 1;

            Assert.That(
                ( ) => !consoleTreeRepository.CanSeeElement( null, new HashSet<long>( ), userId, false ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "entityData" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ShouldPrune_NullSubjectIds( )
        {
            ConsoleTreeRepository consoleTreeRepository;

            consoleTreeRepository = CreateConsoleTreeRepository( );
            long userId = 1;

            Assert.That(
                ( ) => !consoleTreeRepository.CanSeeElement( new EntityData( ), null, userId, false ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "subjectIds" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( "", true )]
        [TestCase( "1", true )]
        [TestCase( "1,2", true )]
        public void ShouldPrune_NoRelationships( string subjectIds, bool expectedResult )
        {
            ConsoleTreeRepository consoleTreeRepository;
            long userId = 1;

            consoleTreeRepository = CreateConsoleTreeRepository( );

            Assert.That(
                !consoleTreeRepository.CanSeeElement(
                    CreateEntityData( ),
                    StringToLongSet( subjectIds ),
                    userId, false ),
                Is.EqualTo( expectedResult ) );
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( "", true )]
        [TestCase( "1", false )]
        [TestCase( "2", true )]
        [TestCase( "1,2", false )]
        public void ShouldPrune_OneRelationship( string subjectIds, bool expectedResult )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            long userId = 1;

            consoleTreeRepository = CreateConsoleTreeRepository( );
            entityData = CreateEntityData( );
            entityData.Relationships.Add( CreateRelationshipData( "core:allowedDisplayBy", 1 ) );

            Assert.That(
                !consoleTreeRepository.CanSeeElement(
                    entityData,
                    StringToLongSet( subjectIds ), userId, false ),
                Is.EqualTo( expectedResult ) );
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( "", true )]
        [TestCase( "1", false )]
        [TestCase( "2", false )]
        [TestCase( "3", true )]
        [TestCase( "1,2", false )]
        [TestCase( "1,3", false )]
        [TestCase( "2,3", false )]
        [TestCase( "1,2,3", false )]
        [TestCase( "3,4", true )]
        public void ShouldPrune_TwoRelationships( string subjectIds, bool expectedResult )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            long userId = 1;

            consoleTreeRepository = CreateConsoleTreeRepository( );
            entityData = CreateEntityData( );
            entityData.Relationships.Add( CreateRelationshipData( "core:allowedDisplayBy", 1, 2 ) );

            Assert.That(
                !consoleTreeRepository.CanSeeElement(
                    entityData,
                    StringToLongSet( subjectIds ),
                    userId, false ),
                Is.EqualTo( expectedResult ) );
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( "", true )]
        [TestCase( "1", false )]
        [TestCase( "2", true )]
        [TestCase( "3", true )]
        [TestCase( "1,2", false )]
        [TestCase( "1,3", false )]
        [TestCase( "2,3", true )]
        [TestCase( "1,2,3", false )]
        [TestCase( "3,4", true )]
        public void ShouldPrune_MixedRelationships( string subjectIds, bool expectedResult )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            long userId = 1;

            consoleTreeRepository = CreateConsoleTreeRepository( );
            entityData = CreateEntityData( );
            entityData.Relationships.Add( CreateRelationshipData( "core:allowedDisplayBy", 1 ) );
            entityData.Relationships.Add( CreateRelationshipData( "console:resourceInFolder", 2 ) );

            Assert.That(
                !consoleTreeRepository.CanSeeElement(
                    entityData,
                    StringToLongSet( subjectIds ),
                    userId, false ),
                Is.EqualTo( expectedResult ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Prune_NullEntityData( )
        {
            Assert.That(
                ( ) => CreateConsoleTreeRepository( ).Prune( null, 1 ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "entityData" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Prune_NoAppIcons( )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            const long userId = 1;

            consoleTreeRepository = CreateConsoleTreeRepository( userId, 2, 3 );
            entityData = CreateEntityData( );

            consoleTreeRepository.Prune( entityData, userId );

            Assert.That( entityData.GetRelationship( consoleTreeRepository.InstancesOfType ), Is.Null );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Prune_SingleInstanceOfRelationshipToEntityWithNoAllowDisplay( )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            const long userId = 1;
            const long appIconId = 2;

            consoleTreeRepository = CreateConsoleTreeRepository( userId );
            entityData = CreateEntityData( );
            entityData.Relationships.Add( CreateRelationshipData( "core:instancesOfType", appIconId ) );

            consoleTreeRepository.Prune( entityData, userId );

            Assert.That(
                entityData.GetRelationship( consoleTreeRepository.InstancesOfType ),
                Has.Property( "Instances" ).Empty );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Prune_SingleInstanceOfRelationshipToEntityWithAllowDisplayWrongSubject( )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            const long userId = 1;
            const long appIconId = 2;
            const long subjectId = 3;

            consoleTreeRepository = CreateConsoleTreeRepository( userId );
            entityData = CreateEntityData( );
            entityData.Relationships.Add( CreateRelationshipData( "core:instancesOfType", appIconId ) );
            entityData.GetRelationship( "core:instancesOfType" ).Entities.First( ).Relationships.Add(
                CreateRelationshipData( "core:allowedDisplayBy", subjectId ) );

            consoleTreeRepository.Prune( entityData, userId );

            Assert.That(
                entityData.GetRelationship( consoleTreeRepository.InstancesOfType ),
                Has.Property( "Instances" ).Empty );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Prune_SingleInstanceOfRelationshipToEntityWithAllowDisplayToUser( )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            const long userId = 1;
            const long appIconId = 2;

            consoleTreeRepository = CreateConsoleTreeRepository( userId );
            entityData = CreateEntityData( );
            entityData.Relationships.Add( CreateRelationshipData( "core:instancesOfType", appIconId ) );
            entityData.GetRelationship( "core:instancesOfType" ).Entities.First( ).Relationships.Add(
                CreateRelationshipData( "core:allowedDisplayBy", userId ) );

            consoleTreeRepository.Prune( entityData, userId );

            Assert.That(
                entityData.GetRelationship( consoleTreeRepository.InstancesOfType ),
                Has.Property( "Instances" ).Not.Empty );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Prune_SingleInstanceOfRelationshipToEntityWithAllowDisplayToRole( )
        {
            ConsoleTreeRepository consoleTreeRepository;
            EntityData entityData;
            const long userId = 1;
            const long appIconId = 2;
            const long roleId = 3;

            consoleTreeRepository = CreateConsoleTreeRepository( userId, roleId );
            entityData = CreateEntityData( );
            entityData.Relationships.Add( CreateRelationshipData( "core:instancesOfType", appIconId ) );
            entityData.GetRelationship( "core:instancesOfType" ).Entities.First( ).Relationships.Add(
                CreateRelationshipData( "core:allowedDisplayBy", roleId ) );

            consoleTreeRepository.Prune( entityData, userId );

            Assert.That(
                entityData.GetRelationship( consoleTreeRepository.InstancesOfType ),
                Has.Property( "Instances" ).Not.Empty );
        }

        [TestCase( "self", true )]
        [TestCase( "other", false )]
        [TestCase( "admin", false )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void PrivateReportVisibleToSelf( string viewBy, bool expectVisible )
        {
            UserAccount user1 = CreateSelfServeUser( "user1" );

            NavContainer folder = Entity.Get<NavContainer>( "console:homeSection" );

            IUserRoleRepository userRoleRepository;
            IConsoleTreeRepository consoleTreeRepository;

            Report report = Entity.Create<Report>( );
            report.ResourceInFolder.Add( folder );
            report.IsPrivatelyOwned = true;
            report.SecurityOwner = user1;
            report.Save( );

            UserAccount viewingUser = null;
            switch ( viewBy )
            {
                case "self":
                    viewingUser = user1;
                    break;
                case "other":
                    viewingUser = CreateSelfServeUser( "user2" );
                    break;
                case "admin":
                    viewingUser = Entity.Get<UserAccount>( "core:administratorUserAccount" );
                    break;
            }

            using ( new SetUser( viewingUser ) )
            {
                userRoleRepository = Factory.Current.Resolve<IUserRoleRepository>( );
                consoleTreeRepository = new ConsoleTreeRepository( userRoleRepository );
                EntityData tree = consoleTreeRepository.GetTree( );
                EntityData homeTopMenu = tree.GetRelationship( "core:instancesOfType", Direction.Forward ).Instances [ 0 ].Entity;
                EntityData homeSection = homeTopMenu.GetRelationship( "console:folderContents", Direction.Forward ).Instances [ 0 ].Entity;
                List<RelationshipInstanceData> contents = homeSection.GetRelationship( "console:folderContents", Direction.Forward ).Instances;
                bool containsReport = contents.Any( e => e.Entity.Id.Id == report.Id );
                Assert.That( containsReport, Is.EqualTo( expectVisible ) );
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

        private ConsoleTreeRepository CreateConsoleTreeRepository( )
        {
            return new ConsoleTreeRepository( new Mock<IUserRoleRepository>( MockBehavior.Strict ).Object );
        }

        private ConsoleTreeRepository CreateConsoleTreeRepository( long userId, params long [ ] subjectIds )
        {
            Mock<IUserRoleRepository> userRoleRepositoryMock;

            userRoleRepositoryMock = new Mock<IUserRoleRepository>( MockBehavior.Strict );
            userRoleRepositoryMock.Setup( urr => urr.GetUserRoles( userId ) )
                .Returns( ( ) => new HashSet<long>( subjectIds ) );

            return new ConsoleTreeRepository( userRoleRepositoryMock.Object );
        }

        private EntityData CreateEntityData( long id = 0 )
        {
            EntityData entityData;

            entityData = new EntityData( );
            entityData.Fields = new List<FieldData>( );
            entityData.Relationships = new List<RelationshipData>( );
            if ( id != 0 )
            {
                entityData.Id = new EntityRef( id );
            }

            return entityData;
        }

        private RelationshipData CreateRelationshipData( string alias, params long [ ] entities )
        {
            Direction direction = Entity.GetDirection( new EntityRef(alias) );
            RelationshipData relationshipData;

            relationshipData = new RelationshipData
            {
                RelationshipTypeId = new EntityRef( alias ),
                Instances = entities.Select( id => new RelationshipInstanceData { Entity = CreateEntityData( id ) } ).ToList( ),
                IsReverseActual = direction == Direction.Reverse
            };

            return relationshipData;
        }

        private ISet<long> StringToLongSet( string longs )
        {
            return new HashSet<long>(
                longs.Split(
                         new [ ] { ',' },
                         StringSplitOptions.RemoveEmptyEntries )
                     .Select( long.Parse ) );
        }
    }
}

