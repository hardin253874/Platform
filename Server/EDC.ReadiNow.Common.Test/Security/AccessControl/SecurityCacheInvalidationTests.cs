// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.Diagnostics;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using EDC.ReadiNow.Test.Model;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ResourceExpression = EDC.ReadiNow.Metadata.Query.Structured.ResourceExpression;
using ReadiNow.QueryEngine.ReportConverter;
using ReadiNow.QueryEngine.CachingBuilder;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    [FailOnEvent]
    public class SecurityCacheInvalidationTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_SingleFieldChange_SinglePermission( )
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            StringField field;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

                field = new StringField { Name = "Test Field" };
                field.Save( );

		        entityType = new EntityType { Name = "Test Type" };
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Fields.Add( field.As<Field>( ) );
		        entityType.Save( );

                entity = Entity.Create( entityType );
                entity.SetField( field, "a" );
                entity.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery(
                    userAccount.As<Subject>( ),
                    entityType.As<SecurableEntity>( ),
                    TestQueries.EntitiesWithField( entityType, field, "a" ).ToReport( ) );

                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ),
                    "Entry initially present in cache" );

            using ( new SetUser( userAccount ) )
            {
                Assert.That( Entity.Get( entity.Id ), Is.Not.Null );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity.Id )
                    .And.Property( "Value" ).EqualTo( true ),
                    "Read entry not added to cache" );

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    entity.SetField( field, "b" );
                    entity.Save( );
                    ctx.CommitTransaction( );
                }

                // NOTE: The following line may fail if other out of the box rules use the "name" field
                // in a security query, since they will be invalidated as well.
                Assert.That( cacheMonitor.ItemsRemoved,
                    Has.Some
                       .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entity.Id, new [ ] { Permissions.Read.Id } ) ) );
                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity.Id ),
                    "Entry not removed from cache" );
            }

            // Ensure the whole cache has not been invalidated
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries" );

            // Access the entity again and ensure the entry is readded to the cache.
            using ( new SetUser( userAccount ) )
            using ( new ForceSecurityTraceContext( entity.Id ) )
            {
                // Should not have access since the name field is "b", not "a"
                Assert.That( ( ) => Entity.Get( entity.Id ),
                    Throws.TypeOf<PlatformSecurityException>( ) );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity.Id )
                    .And.Property( "Value" ).EqualTo( false ),
                "Read entry not added to cache" );
        }

        [Test]
        [RunAsDefaultTenant]
        [Explicit( "This test is failing only on the TC agents" )]
        public void Test_SingleFieldChange_MultiplePermissions( )
        {
            UserAccount userAccount;
            EntityType entityType1;
            IEntity entity1;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

		        entityType1 = new EntityType( );
		        entityType1.Save( );

                entity1 = Entity.Create( entityType1 );
                entity1.SetField( "core:name", "a" );
                entity1.Save( );

                new AccessRuleFactory( ).AddAllowByQuery( userAccount.As<Subject>( ),
                    entityType1.As<SecurableEntity>( ),
                    new [ ] { Permissions.Read, Permissions.Modify },
                    TestQueries.EntitiesWithNameA( ).ToReport( ) );
                ctx.CommitTransaction( );

            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ),
                        "Entry initially present in cache" );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            using ( new SetUser( userAccount ) )
            {
                IEntity testEntity;
                testEntity = Entity.Get( entity1.Id, true );
                testEntity.Save( );
                ctx.CommitTransaction( );
            }

            // Useful for debugging
            //IList<long> cachedPermissionIds = CachingPerRuleSetEntityAccessControlChecker.Cache
            //    .Where(kvp => kvp.Key.UserId == userRuleSet && kvp.Key.EntityId == entity1.Id)
            //    .SelectMany(kvp  => kvp.Key.PermissionIds)
            //    .Distinct()
            //    .ToList();

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).EquivalentTo( new [ ] { Permissions.Modify.Id } )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read entry not added to cache" );
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).EquivalentTo( new [ ] { Permissions.Read.Id, Permissions.Modify.Id } )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Modify entry not added to cache" );

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    entity1.SetField( "core:name", "b" );
                    entity1.Save( );
                    ctx.CommitTransaction( );
                }

                // NOTE: The following line may fail if other out of the box rules use the "name" field
                // in a security query, since they will be invalidated as well.
                Assert.That(
                    cacheMonitor.ItemsRemoved,
                    Has.Some
                       .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entity1.Id, new [ ] { Permissions.Modify.Id } ) ) );
                Assert.That(
                    cacheMonitor.ItemsRemoved,
                    Has.Some
                       .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entity1.Id,
                            new [ ] { Permissions.Read.Id, Permissions.Modify.Id } ) ) );
                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).EquivalentTo( new [ ] { Permissions.Modify.Id } )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read entry not removed from cache" );
                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" )
                        .Property( "PermissionIds" )
                        .EquivalentTo( new [ ] { Permissions.Read.Id, Permissions.Modify.Id } )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read and modify entry not removed from cache" );
            }

            // Ensure the whole cache has not been invalidated
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries" );
        }

        // NOTE: Many of the entity model changes that could cause a security cache invalidation are already covered by the security query cache
        // invalidation tests.

        [Test]
        [RunAsDefaultTenant]
        public void Test_AuthChange( )
        {
            UserAccount userAccount;
            EntityType entityType1;
            IEntity entity1;
            IEntity entity2;
            AccessRule accessRule;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            userAccount = new UserAccount( );
            userAccount.Name = Guid.NewGuid( ).ToString( );
            userAccount.Save( );

		    entityType1 = new EntityType( );
            entityType1.Inherits.Add(UserResource.UserResource_Type);
            entityType1.Save( );

            entity1 = Entity.Create( entityType1 );
            entity1.SetField( "core:name", "a" );
            entity1.Save( );

            entity2 = Entity.Create( entityType1 );
            entity2.SetField( "core:name", "a" );
            entity2.Save( );

            var builder = ( CachingQuerySqlBuilder ) Factory.QuerySqlBuilder;
            builder.Clear( );

            var converter = ( CachingReportToQueryConverter ) Factory.ReportToQueryConverter;
            converter.Clear( );

            accessRule = new AccessRuleFactory( ).AddAllowReadQuery( userAccount.As<Subject>( ),
                entityType1.As<SecurableEntity>( ), TestQueries.EntitiesWithNameA( ).ToReport( ) );

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ),
                    "Entry initially present in cache" );

            using ( new SetUser( userAccount ) )
            {
                Assert.That( Entity.Get( entity1.Id ), Is.Not.Null );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read entry not added to cache" );
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity2.Id ),
                    "Entry not removed from cache" );

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    accessRule.AccessRuleEnabled = false;
                    accessRule.Save( );
                    ctx.CommitTransaction( );
                }

                Assert.That( cacheMonitor.ItemsRemoved,
                    Contains.Item( new RuleSetEntityPermissionTuple( userRuleSet, entity1.Id, new [ ] { Permissions.Read.Id } ) ) );
                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                        "Entry not removed from cache" );
                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity2.Id ),
                        "Entry not removed from cache" );
            }

            // Ensure the whole cache has not been invalidated
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries" );
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( InvalidationCause.Save )]
        [TestCase( InvalidationCause.Delete )]
        public void Test_SimpleRoleChange( InvalidationCause cause )
        {
            Role role1;
            UserAccount userAccount;
            EntityType entityType1;
            IEntity entity1;
            IEntity entity2;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

		        role1 = new Role( );
		        role1.Name = Guid.NewGuid( ).ToString( );
		        role1.RoleMembers.Add( userAccount );
		        role1.Save( );

		        entityType1 = new EntityType( );
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save( );

                entity1 = Entity.Create( entityType1 );
                entity1.SetField( "core:name", "a" );
                entity1.Save( );

                entity2 = Entity.Create( entityType1 );
                entity2.SetField( "core:name", "a" );
                entity2.Save( );

				new AccessRuleFactory( ).AddAllowReadQuery( userAccount.As<Subject>( ),
					entityType1.As<SecurableEntity>( ), TestQueries.EntitiesWithNameA( ).ToReport( ) );
				ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read entry not added to cache" );
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity2.Id ),
                    "Entry not removed from cache" );

            using ( new SetUser( userAccount ) )
            {
                Assert.That( Entity.Get( entity1.Id ), Is.Not.Null );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read entry not added to cache" );
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity2.Id ),
                    "Entry not removed from cache" );

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    if ( cause == InvalidationCause.Save )
                    {
                        role1.RoleMembers.Remove( userAccount );
                        role1.Save( );
                    }
                    else if ( cause == InvalidationCause.Delete )
                    {
                        role1.Delete( );
                    }
                    else
                    {
                        Assert.Fail( "Unknown invalidation cause" );
                    }
                    ctx.CommitTransaction( );
                }

                Assert.That( cacheMonitor.ItemsRemoved,
                    Has.Some
                       .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entity1.Id, new [ ] { Permissions.Read.Id } ) ) );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Entry not removed from cache" );
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity2.Id ),
                    "Entry not removed from cache" );

            // Ensure the whole cache has not been invalidated
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries" );
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( InvalidationCause.Save )]
        [TestCase( InvalidationCause.Delete )]
        public void Test_RemovedRoleChange( InvalidationCause cause )
        {
            Role role1;
            Role role2;
            UserAccount userAccount;
            EntityType entityType1;
            IEntity entity1;
            IEntity entity2;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

                role1 = new Role( );
                role1.Name = Guid.NewGuid( ).ToString( );
                role1.RoleMembers.Add( userAccount );
                role1.Save( );

                role2 = new Role( );
                role2.Name = Guid.NewGuid( ).ToString( );
                role2.IncludesRoles.Add( role1 );
                role2.Save( );

		        entityType1 = new EntityType( );
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save( );

                entity1 = Entity.Create( entityType1 );
                entity1.SetField( "core:name", "a" );
                entity1.Save( );

                entity2 = Entity.Create( entityType1 );
                entity2.SetField( "core:name", "a" );
                entity2.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery( userAccount.As<Subject>( ),
                    entityType1.As<SecurableEntity>( ), TestQueries.EntitiesWithNameA( ).ToReport( ) );
                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ),
                    "Entry initially present in cache" );

            using ( new SetUser( userAccount ) )
            {
                Assert.That( Entity.Get( entity1.Id ), Is.Not.Null );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read entry not added to cache" );
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity2.Id ),
                    "Entry not removed from cache" );

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    if ( cause == InvalidationCause.Save )
                    {
                        role2.IncludesRoles.Remove( role1 );
                        role2.Save( );
                    }
                    else if ( cause == InvalidationCause.Delete )
                    {
                        role2.Delete( );
                    }
                    else
                    {
                        Assert.Fail( "Unknown invalidation cause" );
                    }
                    ctx.CommitTransaction( );
                }

                Assert.That( cacheMonitor.ItemsRemoved,
                    Has.Some
                       .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entity1.Id, new [ ] { Permissions.Read.Id } ) ) );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Entry not removed from cache" );
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity2.Id ),
                    "Entry not removed from cache" );

            // Ensure the whole cache has not been invalidated
            //Assert.That(CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries");
            // TODO : Fix .. at the moment we are listening to the relationship between roles and rules; which is causing this to fail.
            // See EntityAccessControlChecker.AllowedEntityTypes ( .. call to cacheContext.EntityTypes.Add )
            // And RuleRepository.GetAccessRules ( .. call to cacheContext.EntityInvalidatingRelationshipTypes )
            // And CacheInvalidator.OnEntityChange ( .. second call to InvalidateEntities )
            // Removing any of these three lines makes this problem go away (But they're all required for something)
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( "Remove from entity 1" )]
        [TestCase( "Remove from entity 2" )]
        [TestCase( "Unrelated forward" )]
        [TestCase( "Unrelated reverse" )]
        [TestCase( "Unrelated" )]
        public void Test_SingleRelationshipChange( string test )
        {
            UserAccount userAccount;
            EntityType entityType1;
            EntityType entityType2;
            Relationship relationship;
            IEntity entity1;
            IEntity entity2;
            IEntity entity3;
            IEntity entity4;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;
            IEntityRelationshipCollection<IEntity> relationships;

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

		        entityType2 = new EntityType( );
		        entityType2.Save( );

		        entityType1 = new EntityType( );
		        entityType1.Save( );

                relationship = new Relationship( )
                {
                    FromType = entityType1,
                    ToType = entityType2,
                    RelType = Entity.Get<RelTypeEnum>( "relSingleLookup" )
                };

                entityType1.Relationships.Add( relationship );
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save( );

                entity2 = Entity.Create( entityType2 );                
                entity2.SetField( "core:name", "foo" );
                entity2.Save( );

                entity1 = Entity.Create( entityType1 );
                entity1.Save( );

                relationships = entity1.GetRelationships( relationship );
                relationships.Add( entity2 );
                entity1.SetRelationships( relationship, relationships );
                entity1.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery( userAccount.As<Subject>( ),
                    entityType1.As<SecurableEntity>( ), TestQueries.ToNamed( entityType1, relationship ).ToReport( ) );
                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ),
                    "Entry initially present in cache" );

            using ( new SetUser( userAccount ) )
            {
                Assert.That( Entity.Get( entity1.Id ), Is.Not.Null );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                    "Read entry not added to cache" );

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    if ( test == "Remove from entity 1" )
                    {
                        relationships = entity1.GetRelationships( relationship, Direction.Forward );
                        relationships.Remove( entity2 );
                        entity1.SetRelationships( relationship, relationships );
                        entity1.Save( );
                    }
                    else if ( test == "Remove from entity 2" )
                    {
                        relationships = entity2.GetRelationships( relationship, Direction.Reverse );
                        relationships.Remove( entity1 );
                        entity2.SetRelationships( relationship, relationships );
                        entity2.Save( );
                    }
                    else if ( test == "Unrelated forward" )
                    {
                        entity3 = Entity.Create( entityType1 );
                        entity3.Save( );

                        relationships = entity3.GetRelationships( relationship, Direction.Forward );
                        relationships.Add( entity2 );
                        entity3.SetRelationships( relationship, relationships );
                        entity3.Save( );
                    }
                    else if ( test == "Unrelated reverse" )
                    {
                        entity4 = Entity.Create( entityType2 );
                        entity4.Save( );

                        relationships = entity4.GetRelationships( relationship, Direction.Reverse );
                        relationships.Add( entity1 );
                        entity4.SetRelationships( relationship, relationships );
                        entity4.Save( );
                    }
                    else if ( test == "Unrelated" )
                    {
                        entity3 = Entity.Create( entityType1 );
                        entity3.Save( );

                        entity4 = Entity.Create( entityType2 );
                        entity4.Save( );

                        relationships = entity3.GetRelationships( relationship, Direction.Forward );
                        relationships.Add( entity4 );
                        entity3.SetRelationships( relationship, relationships );
                        entity3.Save( );
                    }
                    else
                    {
                        Assert.Fail( "Unknown test" );
                    }
                    ctx.CommitTransaction( );
                }

                // NOTE: The following line may fail if other out of the box rules use the "name" field
                // in a security query, since they will be invalidated as well.
                Assert.That( cacheMonitor.ItemsRemoved,
                    Is.EquivalentTo( new [ ] { new RuleSetEntityPermissionTuple( userRuleSet, entity1.Id, new [ ] { Permissions.Read.Id } ) } ) );
                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity1.Id ),
                        "Entry not removed from cache" );
            }

            // Ensure the whole cache has not been invalidated
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries" );
        }

        [Test]
        [RunAsDefaultTenant]
        [Ignore( "Seems to leave invalid data causing issues for other tests." )]
        public void Test_Everyone( )
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;
            AccessRule accessRule = null;

            try
            {
                CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    userAccount = new UserAccount( );
                    userAccount.Name = Guid.NewGuid( ).ToString( );
                    userAccount.Save( );

		            entityType = new EntityType( );
		            entityType.Save( );

                    entity = Entity.Create( entityType );
                    entity.Save( );
                    ctx.CommitTransaction( );
                }

                userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity.Id ),
                    "Entry initially present in cache" );

                using ( new SetUser( userAccount ) )
                {
                    Assert.That( ( ) => Entity.Get( entity.Id ),
                        Throws.TypeOf<PlatformSecurityException>( ) );
                }

                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity.Id ),
                    "Entry not present in cache" );

                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    accessRule = new AccessRuleFactory( ).AddAllowReadQuery( Entity.Get<Subject>( WellKnownAliases.CurrentTenant.EveryoneRole, true ),
                    entityType.As<SecurableEntity>( ), TestQueries.Entities( entityType ).ToReport( ) );
                    ctx.CommitTransaction( );
                }

                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ),
                    "Adding access to Everyone did not invalidate cache" );

                // Re-evaluate ruleset
                userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                    Has.Exactly( 0 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ),
                        "Adding access to Everyone did not invalidate cache" );

                using ( new SetUser( userAccount ) )
                {
                    Assert.That( Entity.Get( entity.Id ), Is.Not.Null );
                }

                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                        .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                        .And.Property( "Key" ).Property( "PermissionIds" ).Contains( Permissions.Read.Id )
                        .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity.Id ),
                        "Entry present in cache" );
            }
            finally
            {
                // Even though this is run in a transaction, we need to delete explicitly, otherwise the cache may
                // get poisoned by the transaction rollback.
                if ( accessRule != null )
                    accessRule.Delete( );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ShouldBeCached( )
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;
            CacheInvalidator<RuleSetEntityPermissionTuple, bool> invalidator;

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

		        entityType = new EntityType( );
		        entityType.Save( );

                entity = Entity.Create( entityType );
                entity.Save( );
                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            invalidator =
                ( CacheInvalidator<RuleSetEntityPermissionTuple, bool> ) CachingPerRuleSetEntityAccessControlChecker.CacheInvalidator;
            invalidator.DebugInvalidations.Add( new RuleSetEntityPermissionTuple( userRuleSet, entity.Id,
                new [ ] { Permissions.Read.Id } ) );

            using ( new SetUser( userAccount ) )
            {
                Assert.That(
                    ( ) => Entity.Get( entity.Id ),
                    Throws.TypeOf<PlatformSecurityException>( ),
                    "User somehow has access to entity" );
            }

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Exactly( 1 )
                    .Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet )
                    .And.Property( "Key" ).Property( "EntityId" ).EqualTo( entity.Id )
                    .And.Property( "Key" ).Property( "PermissionIds" ).EquivalentTo( new [ ] { Permissions.Read.Id } )
                    .And.Property( "Value" ).EqualTo( false ),
                    "Entry not cached" );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddRoleToUser( )
        {
            UserAccount userAccount;
            EntityType entityType1;
            Role role;
            IEntity entity1;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;

            // Technically, there are two but the operations on each should be the same.
            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            using ( new SecurityBypassContext( ) )
            {
                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

		        entityType1 = new EntityType( );
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save( );

                entity1 = Entity.Create( entityType1 );
                entity1.SetField( "core:name", "a" );
                entity1.Save( );
                ctx.CommitTransaction( );
            }

            using ( new SetUser( userAccount ) )
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check( new EntityRef( entity1 ), new [ ] { Permissions.Read } ),
                    Is.False );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache,
                Has.Some.Property( "Key" ).Property( "UserRuleSet" ).EqualTo( userRuleSet ) );

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    role = new Role { Name = "Test Role" };
                    role.Save( );

                    new AccessRuleFactory( ).AddAllowReadQuery(
                        role.As<Subject>( ),
                        entityType1.As<SecurableEntity>( ),
                        TestQueries.Entities( entityType1 ).ToReport( )
                        );

                    userAccount.UserHasRole.Add( role );
                    userAccount.Save( );
                    ctx.CommitTransaction( );
                }

                Assert.That( cacheMonitor.ItemsRemoved,
                    Has.Some.Property( "UserRuleSet" ).EqualTo( userRuleSet ) );
            }

            using ( new SetUser( userAccount ) )
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check( new EntityRef( entity1 ), new [ ] { Permissions.Read } ),
                    Is.True );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_RemoveRoleFromUser( )
        {
            UserAccount userAccount;
            Role role;
            EntityType entityType1;
            IEntity entity1;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;

            // Technically, there are two but the operations on each should be the same.
            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            using ( new SecurityBypassContext( ) )
            {
                role = new Role { Name = "Test Role" };
                role.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery(
                    role.As<Subject>( ),
                    Entity.Get( "core:resource" ).As<SecurableEntity>( ),
                    TestQueries.Entities( ).ToReport( )
                    );

                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.UserHasRole.Add( role );
                userAccount.Save( );

		        entityType1 = new EntityType( );
                entityType1.Inherits.Add(UserResource.UserResource_Type);

                entityType1.Save( );

                entity1 = Entity.Create( entityType1 );
                entity1.SetField( "core:name", "a" );
                entity1.Save( );
                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            using ( new SetUser( userAccount ) )
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check( new EntityRef( entity1 ), new [ ] { Permissions.Read } ),
                    Is.True );
            }

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> cacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    userAccount.UserHasRole.Remove( role );
                    userAccount.Save( );
                    ctx.CommitTransaction( );
                }
                Assert.That( cacheMonitor.ItemsRemoved,
                    Has.Some.Property( "UserRuleSet" ).EqualTo( userRuleSet ) );
            }

            using ( new SetUser( userAccount ) )
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check( new EntityRef( entity1 ), new [ ] { Permissions.Read } ),
                    Is.False );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AddUserToRole( )
        {
            UserAccount userAccount;
            Role role;
            EntityType entityType1;
            IEntity entity1;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;
            CachingUserRoleRepository cachingUserRoleRepository;

            // Technically, there are two but the operations on each should be the same.
            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );
            cachingUserRoleRepository = Factory.Current.Resolve<CachingUserRoleRepository>( );

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            using ( new SecurityBypassContext( ) )
            {
                role = new Role { Name = "Test Role" };
                role.Save( );

                new AccessRuleFactory( ).AddAllowReadQuery(
                    role.As<Subject>( ),
                    Entity.Get( "core:resource" ).As<SecurableEntity>( ),
                    TestQueries.Entities( ).ToReport( )
                    );

                userAccount = new UserAccount( );
                userAccount.Name = Guid.NewGuid( ).ToString( );
                userAccount.Save( );

		        entityType1 = new EntityType( );
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save( );

                entity1 = Entity.Create( entityType1 );
                entity1.SetField( "core:name", "a" );
                entity1.Save( );
                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            using ( new SetUser( userAccount ) )
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check( new EntityRef( entity1 ), new [ ] { Permissions.Read } ),
                    Is.False );
            }

            Assert.That( cachingUserRoleRepository.Cache.Keys( ),
                Has.Some.EqualTo( userAccount.Id ) );
            Assert.That( cachingUserRoleRepository.Cache [ userAccount.Id ],
                Is.EquivalentTo( new [ ] { WellKnownAliases.CurrentTenant.EveryoneRole } ) );
            Assert.That( ( ( CacheInvalidator<long, ISet<long>> ) cachingUserRoleRepository.CacheInvalidator ).EntityToCacheKey.GetKeys( userAccount.Id ),
                Is.EquivalentTo( new [ ] { WellKnownAliases.CurrentTenant.EveryoneRole, userAccount.Id } ) );

            using ( CacheMonitor<long, ISet<long>> cacheUserRoleRepositoryMonitor =
                new CacheMonitor<long, ISet<long>>( cachingUserRoleRepository.Cache ) )
            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> entityAccessControlCacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    role.RoleMembers.Add( userAccount );
                    role.Save( );
                    ctx.CommitTransaction( );
                }

                Assert.That( entityAccessControlCacheMonitor.ItemsRemoved,
                    Has.Some.Property( "UserRuleSet" ).EqualTo( userRuleSet ) );
                Assert.That( cacheUserRoleRepositoryMonitor.ItemsRemoved,
                    Has.Some.EqualTo( userAccount.Id ) );
                Assert.That( cachingUserRoleRepository.Cache.Keys( ),
                    Has.None.EqualTo( userRuleSet ) );
            }

            using ( new SetUser( userAccount ) )
            {
                Assert.That(
                    Factory.EntityAccessControlService.Check( new EntityRef( entity1 ), new [ ] { Permissions.Read } ),
                    Is.True );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource( "Test_FieldChange_Source" )]
        public void Test_FieldChange( Action<EntityType, Field> modifySchema, bool expectNoAccess )
        {
            UserAccount userAccount;
            Field field;
            EntityType entityType;
            IEntity entity;
            EntityRef entityRef;
            ResourceEntity rootEntity;
            StructuredQuery structuredQuery;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;
            CachingQueryRepository cachingQueryRepository;
            CachingReportToQueryConverter cachingReportToQueryConverter;
            const string testFieldValue = "test";
            AccessRule accessRule;
            long everyoneRoleId = new EntityRef( "core:everyoneRole" ).Id;
            Report report;

            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            using ( new SecurityBypassContext( ) )
            {
                userAccount = new UserAccount
                {
                    Name = "Test User Account" + Guid.NewGuid( )
                };
                userAccount.Save( );

                field = new Field
                {
                    Name = "Test",
                    IsRequired = false
                };
                field.IsOfType.Add( Entity.Get<EntityType>( new EntityRef( "core", "stringField" ) ) );
                field.Save( );

		        entityType = new EntityType( );
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Fields.Add( field );
		        entityType.SetField( "core:name", "Entity Type" );
		        entityType.Save( );

                entity = Entity.Create( entityType );
                entity.SetField( field.Id, testFieldValue );
                entity.Save( );

                rootEntity = new ResourceEntity
                {
                    EntityTypeId = entityType,
                    ExactType = false,
                    NodeId = Guid.NewGuid( ),
                    RelatedEntities = new List<ReadiNow.Metadata.Query.Structured.Entity>( )
                };

                structuredQuery = new StructuredQuery
                {
                    RootEntity = rootEntity,
                    SelectColumns = new List<SelectColumn>( )
                };
                structuredQuery.Conditions.Add( new QueryCondition
                {
                    Expression = new ResourceDataColumn( rootEntity, new EntityRef( field ) ),
                    Operator = ConditionType.Equal,
                    Argument = new TypedValue( testFieldValue )
                } );
                structuredQuery.SelectColumns.Add( new SelectColumn
                {
                    Expression = new IdExpression { NodeId = rootEntity.NodeId }
                } );

                report = structuredQuery.ToReport( );
                report.Name = "Test Report";
                report.Save( );

                accessRule = new AccessRuleFactory( ).AddAllowReadQuery(
                    userAccount.As<Subject>( ),
                    entityType.As<SecurableEntity>( ),
                    report );
                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );
            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>( );
            cachingReportToQueryConverter = Factory.Current.Resolve<CachingReportToQueryConverter>( );

            entityRef = new EntityRef( entity );
            using ( new SetUser( userAccount ) )
            {
                Assert.That( Entity.Get( entityRef ), Is.Not.Null,
                    "User cannot access entity initially" );

                Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache.Keys( ),
                    Has.Exactly( 1 )
                       .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entityRef.Id, new [ ] { Permissions.Read.Id } ) ) );
                Assert.That( cachingQueryRepository.Cache.Keys( ),
                    Has.Exactly( 1 )
                       .Property( "SubjectId" ).EqualTo( userAccount.Id ).And
                       .Property( "PermissionId" ).EqualTo( Permissions.Read.Id ).And
                       .Property( "EntityTypes" ).Contains( entityType.Id ) );
                Assert.That( cachingQueryRepository.Cache.Keys( ),
                    Has.Exactly( 1 )
                       .Property( "SubjectId" ).EqualTo( everyoneRoleId ).And
                       .Property( "PermissionId" ).EqualTo( Permissions.Read.Id ).And
                       .Property( "EntityTypes" ).Contains( entityType.Id ) );
            }

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> entityAccessCacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            using ( CacheMonitor<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>> queryCacheMonitor =
                new CacheMonitor<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>( cachingQueryRepository.Cache ) )
            using ( CacheMonitor<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue> reportToQueryConverterCacheMonitor =
                new CacheMonitor<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue>( cachingReportToQueryConverter.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    modifySchema( entityType, field );
                    ctx.CommitTransaction( );
                }

                Assert.That( entityAccessCacheMonitor.ItemsRemoved,
                    Has.Exactly( 1 )
                        .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entityRef.Id, new [ ] { Permissions.Read.Id } ) ) );
                Assert.That( reportToQueryConverterCacheMonitor.ItemsRemoved,
                    Has.Exactly( 1 )
                        .EqualTo( new CachingReportToQueryConverterKey( accessRule.AccessRuleReport, new ReportToQueryConverterSettings { ConditionsOnly = true } ) ) );
                Assert.That( queryCacheMonitor.ItemsRemoved,
                    Has.Exactly( 1 )
                        .Property( "SubjectId" ).EqualTo( userAccount.Id ).And
                        .Property( "PermissionId" ).EqualTo( Permissions.Read.Id ).And
                        .Property( "EntityTypes" ).Contains( entityType.Id ) );
            }

            using ( EventLogMonitor eventLogMonitor = new EventLogMonitor( ) )
            using ( new SetUser( userAccount ) )
            {
                if ( expectNoAccess )
                {
                    Assert.That( ( ) => Entity.Get( entityRef ),
                        Throws.TypeOf<PlatformSecurityException>( ),
                        "User can access entity after schema change" );

                    Assert.That( eventLogMonitor.Entries,
                        Has.Exactly( 1 )
                            .Property( "ThreadId" ).EqualTo( Thread.CurrentThread.ManagedThreadId ).And
                            .Property( "Level" ).EqualTo( EventLogLevel.Warning ).And
                            .Property( "Source" ).EndsWith( "::WriteInvalidSecurityReportMessage" ).And
                            .Property( "Message" ).Contains( "ignored due to errors when running the report" ) );
                }
                else
                {
                    Assert.That( Entity.Get( entityRef ),
                        Is.Not.Null,
                        "User cannot access entity after schema change" );
                }
            }

            // Ensure the whole cache has not been invalidated
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries" );
        }

        /// <summary>
        /// Test cases for <see cref="Test_FieldChange"/>.
        /// </summary>
        private IEnumerable<TestCaseData> Test_FieldChange_Source( )
        {
            yield return new TestCaseData(
                ( Action<EntityType, Field> ) ( ( et, f ) =>
                {
                    et.Fields.Clear( );
                    et.Save( );
                    Entity.Delete( f );
                } ), true )
                .SetName( "Clear fields then delete field" );
            yield return new TestCaseData(
                ( Action<EntityType, Field> ) ( ( et, f ) =>
                {
                    et.Fields.Remove( f );
                    et.Save( );
                    Entity.Delete( f );
                } ), true ).SetName( "Remove field then delete field" );
            yield return new TestCaseData(
                ( Action<EntityType, Field> ) ( ( et, f ) =>
                {
                    et.Fields.Clear( );
                    et.Save( );
                } ), false )
                .SetName( "Clear fields but do not delete field." );
            yield return new TestCaseData(
                ( Action<EntityType, Field> ) ( ( et, f ) =>
                {
                    et.Fields.Remove( f );
                    et.Save( );
                } ), false )
                .SetName( "Remove field but do not delete field." );
            yield return new TestCaseData(
                ( Action<EntityType, Field> ) ( ( et, f ) =>
                {
                    f.FieldIsOnType = null;
                    f.Save( );
                } ), false )
                .SetName( "Remove field on field instead of entity type." );
            yield return new TestCaseData(
                ( Action<EntityType, Field> ) ( ( et, f ) => Entity.Delete( f ) ), true )
                .SetName( "Delete field only." );
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource( "Test_RelationshipChange_Source" )]
        public void Test_RelationshipChange( Action<EntityType, EntityType, Relationship> modifySchema )
        {
            UserAccount userAccount;
            Relationship relationship;
            EntityType fromEntityType;
            EntityType toEntityType;
            IEntityRelationshipCollection<IEntity> relationships;
            IEntity fromEntity;
            IEntity toEntity;
            EntityRef entityRef;
            ResourceEntity rootEntity;
            RelatedResource relatedResource;
            StructuredQuery structuredQuery;
            CachingPerRuleSetEntityAccessControlChecker CachingPerRuleSetEntityAccessControlChecker;
            UserRuleSet userRuleSet;
            CachingQueryRepository cachingQueryRepository;
            CachingReportToQueryConverter cachingReportToQueryConverter;
            const string testFieldValue = "To Entity";
            AccessRule accessRule;


            using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
            using ( new SecurityBypassContext( ) )
            {
                userAccount = new UserAccount
                {
                    Name = "Test User Account" + Guid.NewGuid( )
                };
                userAccount.Save( );

		        fromEntityType = new EntityType( );
                fromEntityType.Inherits.Add(UserResource.UserResource_Type);
                fromEntityType.SetField( "core:name", "From Entity Type" );
		        fromEntityType.Save( );

		        toEntityType = new EntityType( );
                toEntityType.Inherits.Add(UserResource.UserResource_Type);
                toEntityType.SetField( "core:name", "To Entity Type" );
		        toEntityType.Save( );

                relationship = new Relationship
                {
                    Name = "Test",
                    FromType = fromEntityType,
                    ToType = toEntityType
                };
                relationship.Save( );

                toEntity = Entity.Create( toEntityType );
                toEntity.SetField( "core:name", testFieldValue );
                toEntity.Save( );

                fromEntity = Entity.Create( fromEntityType );
                fromEntity.SetField( "core:name", "From Entity" );
                relationships = fromEntity.GetRelationships( relationship );
                relationships.Add( toEntity );
                fromEntity.SetRelationships( relationship, relationships );
                fromEntity.Save( );

                relatedResource = new RelatedResource( relationship )
                {
                    NodeId = Guid.NewGuid( )
                };
                rootEntity = new ResourceEntity
                {
                    EntityTypeId = fromEntityType,
                    ExactType = false,
                    NodeId = Guid.NewGuid( ),
                    RelatedEntities = new List<ReadiNow.Metadata.Query.Structured.Entity>
                    {
                        relatedResource
                    }
                };

                structuredQuery = new StructuredQuery
                {
                    RootEntity = rootEntity,
                    SelectColumns = new List<SelectColumn>( )
                };
                structuredQuery.Conditions.Add( new QueryCondition
                {
                    Expression = new ResourceDataColumn( relatedResource, new EntityRef( "core:name" ) ),
                    Operator = ConditionType.Equal,
                    Argument = new TypedValue( testFieldValue )
                } );
                structuredQuery.SelectColumns.Add( new SelectColumn
                {
                    Expression = new IdExpression { NodeId = rootEntity.NodeId }
                } );

                accessRule = new AccessRuleFactory( ).AddAllowReadQuery(
                    userAccount.As<Subject>( ),
                    fromEntityType.As<SecurableEntity>( ),
                    structuredQuery.ToReport( ) );
                ctx.CommitTransaction( );
            }

            userRuleSet = Factory.Current.Resolve<IUserRuleSetProvider>( ).GetUserRuleSet( userAccount.Id, Permissions.Read );

            CachingPerRuleSetEntityAccessControlChecker = Factory.Current.Resolve<CachingPerRuleSetEntityAccessControlChecker>( );
            cachingQueryRepository = Factory.Current.Resolve<CachingQueryRepository>( );
            cachingReportToQueryConverter = Factory.Current.Resolve<CachingReportToQueryConverter>( );

            entityRef = new EntityRef( fromEntity );
            using ( new SetUser( userAccount ) )
            {
                Assert.That( Entity.Get( entityRef ), Is.Not.Null,
                    "User cannot access entity initially" );
            }

            using ( CacheMonitor<RuleSetEntityPermissionTuple, bool> entityAccessCacheMonitor =
                new CacheMonitor<RuleSetEntityPermissionTuple, bool>( CachingPerRuleSetEntityAccessControlChecker.Cache ) )
            using ( CacheMonitor<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>> queryCacheMonitor =
                new CacheMonitor<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>( cachingQueryRepository.Cache ) )
            using ( CacheMonitor<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue> reportToQueryConverterCacheMonitor =
                new CacheMonitor<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue>( cachingReportToQueryConverter.Cache ) )
            {
                using ( var ctx = DatabaseContext.GetContext( true, preventPostSaveActionsPropagating: true ) )
                {
                    modifySchema( fromEntityType, toEntityType, relationship );
                    ctx.CommitTransaction( );
                }

                Assert.That( entityAccessCacheMonitor.ItemsRemoved,
                    Has.Exactly( 1 )
                       .EqualTo( new RuleSetEntityPermissionTuple( userRuleSet, entityRef.Id, new [ ] { Permissions.Read.Id } ) ) );
                Assert.That( queryCacheMonitor.ItemsRemoved,
                    Has.Exactly( 1 )
                       .Property( "SubjectId" ).EqualTo( userAccount.Id ).And
                       .Property( "PermissionId" ).EqualTo( Permissions.Read.Id ).And
                       .Property( "EntityTypes" ).Contains( fromEntityType.Id ) );
                Assert.That( reportToQueryConverterCacheMonitor.ItemsRemoved,
                    Has.Exactly( 1 )
                       .EqualTo( new CachingReportToQueryConverterKey( accessRule.AccessRuleReport, new ReportToQueryConverterSettings { ConditionsOnly = true } ) ) );
            }

            using ( EventLogMonitor eventLogMonitor = new EventLogMonitor( ) )
            using ( new SetUser( userAccount ) )
            {
                Assert.That( ( ) => Entity.Get( entityRef ),
                    Throws.TypeOf<PlatformSecurityException>( ),
                    "User can access entity after schema change" );

                Assert.That( eventLogMonitor.Entries,
                    Has.Exactly( 1 )
                        .Property( "ThreadId" ).EqualTo( Thread.CurrentThread.ManagedThreadId ).And
                        .Property( "Level" ).EqualTo( EventLogLevel.Warning ).And
                        .Property( "Source" ).EndsWith( "::WriteInvalidSecurityReportMessage" ).And
                        .Property( "Message" ).Contains( "ignored due to errors when running the report" ) );
            }

            // Ensure the whole cache has not been invalidated
            Assert.That( CachingPerRuleSetEntityAccessControlChecker.Cache, Is.Not.Empty, "Removed all entries" );
        }

        /// <summary>
        /// Test cases for <see cref="Test_RelationshipChange"/>.
        /// </summary>
        private IEnumerable<TestCaseData> Test_RelationshipChange_Source( )
        {
            yield return new TestCaseData(
                ( Action<EntityType, EntityType, Relationship> ) ( ( fromET, toET, r ) =>
                {
                    Entity.Delete( r );
                } ) )
                .SetName( "Delete the relationship" );
            yield return new TestCaseData(
                ( Action<EntityType, EntityType, Relationship> ) ( ( fromET, toET, r ) =>
                {
                    r.FromType = null;
                    r.Save( );
                } ) )
                .SetName( "Set r.FromType to null" )
                .Ignore( "Failing. No idea why." );
            yield return new TestCaseData(
                ( Action<EntityType, EntityType, Relationship> ) ( ( fromET, toET, r ) =>
                {
                    r.ToType = null;
                    r.Save( );
                } ) )
                .SetName( "Set r.ToType to null" );
            yield return new TestCaseData(
                ( Action<EntityType, EntityType, Relationship> ) ( ( fromET, toET, r ) =>
                {
                    fromET.Relationships.Remove( r );
                    fromET.Save( );
                } ) )
                .SetName( "Remove from FromType.Relationships" )
                .Ignore( "Not looking for FromType changes yet" );
            yield return new TestCaseData(
                ( Action<EntityType, EntityType, Relationship> ) ( ( fromET, toET, r ) =>
                {
                    toET.ReverseRelationships.Remove( r );
                    toET.Save( );
                } ) )
                .SetName( "Remove from ToType.ReverseRelationships" );
        }

    }
}