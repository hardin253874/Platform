// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Service;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Model;

namespace ReadiNow.Connector.Test.Service
{
    /// <summary>
    /// Tests for ResourceResolver and ResourceResolverProvider. 
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class ResourceResolverTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_NullMapper( )
        {
            IResourceResolverProvider provider = GetResolverProvider( );

            Assert.Throws<ArgumentNullException>( ( ) => provider.GetResolverForResourceMapping( (ApiResourceMapping)null ) );
        }

        
        [TestCase( null )]
        [TestCase( "" )]
        [RunAsDefaultTenant]
        public void Test_WithMapping_NullIdentity( string identity )
        {
            IResourceResolverProvider provider = GetResolverProvider( );
            IResourceResolver resolver = provider.GetResolverForType( new EntityRef("test:person").Id );

            Assert.Throws<ArgumentNullException>( ( ) => resolver.ResolveResource( identity ) );
        }


        [TestCase( null )]
        [TestCase( "" )]
        [RunAsDefaultTenant]
        public void Test_WithType_NullIdentity( string identity )
        {
            IResourceResolverProvider provider = GetResolverProvider( );
            IResourceResolver resolver = provider.GetResolverForType( new EntityRef( "test:employee" ).Id );

            Assert.Throws<ArgumentNullException>( ( ) => resolver.ResolveResource( identity ) );
        }


        [TestCase( "David Quint", "test:employee" )]  // exact type
        [TestCase( "Peter Aylett", "test:employee" )] // derived type
        [RunAsDefaultTenant]
        public void Test_ResolveResource_WithMapping_NoIdentityField_ByName( string identity, string type )
        {
            IResourceResolverProvider provider = GetResolverProvider( );
            ApiResourceMapping mapping = new ApiResourceMapping( );
            mapping.MappedType = Entity.Get<EntityType>( type );
            mapping.Save( );
            IResourceResolver resolver = provider.GetResolverForResourceMapping( mapping );

            ResourceResolverEntry entry = resolver.ResolveResource( identity );
            IEntity entity = entry.Entity;
            Assert.That( entity, Is.Not.Null );
            Assert.That( entity.As<Resource>( ).Name, Is.EqualTo( identity ) );
        }


        [TestCase( "test:aaDavidQuint", "test:employee", "David Quint" )]  // exact type
        [TestCase( "test:aaPeterAylett", "test:employee", "Peter Aylett" )] // derived type
        [RunAsDefaultTenant]
        public void Test_ResolveResource_WithMapping_NoIdentityField_ByGuid( string alias, string type, string name )
        {
            IEntity entity1 = Entity.Get( new EntityRef( alias ) );
            string identity = entity1.UpgradeId.ToString();

            IResourceResolverProvider provider = GetResolverProvider( );
            ApiResourceMapping mapping = new ApiResourceMapping( );
            mapping.MappedType = Entity.Get<EntityType>( type );
            mapping.Save( );
            IResourceResolver resolver = provider.GetResolverForResourceMapping( mapping );

            ResourceResolverEntry entry = resolver.ResolveResource( identity );
            IEntity entity = entry.Entity;
            Assert.That( entity, Is.Not.Null );
            Assert.That( entity.As<Resource>( ).Name, Is.EqualTo( name ) );
        }


        [TestCase( "The 'name' field of every resource.", "core:field" )]        // derived type
        [TestCase( "The 'name' field of every resource.", "core:stringField" )]  // exact type
        [RunAsDefaultTenant]
        public void Test_ResolveResource_WithMapping_WithIdentityField_StringField( string identity, string type )
        {
            IResourceResolverProvider provider = GetResolverProvider( );
            ApiResourceMapping mapping = new ApiResourceMapping( );
            mapping.ResourceMappingIdentityField = Entity.Get<Field>( "core:description" );
            mapping.MappedType = Entity.Get<EntityType>( type );
            mapping.Save( );
            IResourceResolver resolver = provider.GetResolverForResourceMapping( mapping );

            ResourceResolverEntry entry = resolver.ResolveResource( identity );
            IEntity entity = entry.Entity;
            Assert.That( entity, Is.Not.Null );
            Assert.That( entity.As<Resource>( ).Name, Is.EqualTo( "Name" ) );
        }


        [TestCase( "Blahblah", "test:allFields", ResourceResolverError.ResourceNotFoundByField )]  // expect none
        [TestCase( "Name", "core:reportColumn", ResourceResolverError.ResourceNotFoundByField )]   // expect duplicates
        [RunAsDefaultTenant]
        public void Test_Resolve_WithMapping_NotFound( string identity, string type, ResourceResolverError expectedError )
        {
            IResourceResolverProvider provider = GetResolverProvider( );
            ApiResourceMapping mapping = new ApiResourceMapping( );
            mapping.MappedType = Entity.Get<EntityType>( "test:allFields" );
            mapping.Save( );
            IResourceResolver resolver = provider.GetResolverForResourceMapping( mapping );

            ResourceResolverEntry entry = resolver.ResolveResource( identity );
            Assert.That( entry.Entity, Is.Null );
            Assert.That( entry.Error, Is.EqualTo( expectedError ) );
        }


        [TestCase( "David Quint", "test:employee" )]  // exact type
        [TestCase( "Peter Aylett", "test:employee" )] // derived type
        [RunAsDefaultTenant]
        public void Test_ResolveResource_WithType_ByName( string identity, string type )
        {
            IResourceResolverProvider provider = GetResolverProvider( );
            long typeId = new EntityRef( type ).Id;
            IResourceResolver resolver = provider.GetResolverForType( typeId );

            ResourceResolverEntry entry = resolver.ResolveResource( identity );
            IEntity entity = entry.Entity;
            Assert.That( entity, Is.Not.Null );
            Assert.That( entity.As<Resource>( ).Name, Is.EqualTo( identity ) );
        }


        [TestCase( "test:aaDavidQuint", "test:employee", "David Quint" )]  // exact type
        [TestCase( "test:aaPeterAylett", "test:employee", "Peter Aylett" )] // derived type
        [RunAsDefaultTenant]
        public void Test_ResolveResource_WithType_ByGuid( string alias, string type, string name )
        {
            IEntity entity1 = Entity.Get( new EntityRef( alias ) );
            string identity = entity1.UpgradeId.ToString( );

            IResourceResolverProvider provider = GetResolverProvider( );
            long typeId = new EntityRef( type ).Id;
            IResourceResolver resolver = provider.GetResolverForType( typeId );

            ResourceResolverEntry entry = resolver.ResolveResource( identity );
            IEntity entity = entry.Entity;
            Assert.That( entity, Is.Not.Null );
            Assert.That( entity.As<Resource>( ).Name, Is.EqualTo( name ) );
        }


        [TestCase( "Blahblah", "test:allFields", ResourceResolverError.ResourceNotFoundByField )]  // expect none
        [TestCase( "Name", "core:reportColumn", ResourceResolverError.ResourceNotUniqueByField )]   // expect duplicates
        [RunAsDefaultTenant]
        public void Test_Resolve_WithType_NotFound( string identity, string type, ResourceResolverError expectedError )
        {
            IResourceResolverProvider provider = GetResolverProvider( );
            long typeId = new EntityRef( type ).Id;
            IResourceResolver resolver = provider.GetResolverForType( typeId );

            ResourceResolverEntry entry = resolver.ResolveResource( identity );
            Assert.That( entry.Entity, Is.Null );
            Assert.That( entry.Error, Is.EqualTo( expectedError ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_ResolveMultiple_Null( )
        {
            long typeId = new EntityType( ).Id;

            IResourceResolverProvider provider = GetResolverProvider( );
            IResourceResolver resolver = provider.GetResolverForType( typeId );

            Assert.Throws<ArgumentNullException>( ( ) => resolver.ResolveResources( null ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_ResolveMultiple_Empty( )
        {
            long typeId = new EntityType( ).Id;

            IResourceResolverProvider provider = GetResolverProvider( );
            IResourceResolver resolver = provider.GetResolverForType( typeId );

            var entities = resolver.ResolveResources( new string [ ] { } );
            Assert.That( entities.Count, Is.EqualTo( 0 ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_ResolveMultiple_WithType_ByGuid( )
        {
            string [ ] aliases = new [ ] { "test:aaDavidQuint", "test:aaPeterAylett" };
            string [ ] guids = aliases.Select( alias => Entity.Get( new EntityRef( alias ) ).UpgradeId.ToString( ) ).ToArray( );

            IResourceResolverProvider provider = GetResolverProvider( );
            long typeId = new EntityRef( "test:employee" ).Id;
            IResourceResolver resolver = provider.GetResolverForType( typeId );

            var entities = resolver.ResolveResources( guids );
            Assert.That( entities, Is.Not.Null );
            Assert.That( entities.Count, Is.EqualTo( 2 ) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_ResolveMultiple_WithType_ByName( )
        {
            string [ ] names = new [ ] { "David Quint", "Peter Aylett" };

            IResourceResolverProvider provider = GetResolverProvider( );
            long typeId = new EntityRef( "test:employee" ).Id;
            IResourceResolver resolver = provider.GetResolverForType( typeId );

            var entities = resolver.ResolveResources( names );
            Assert.That( entities, Is.Not.Null );
            Assert.That( entities.Count, Is.EqualTo( 2 ) );
        }


        private IResourceResolverProvider GetResolverProvider( )
        {
            return new ResourceResolverProvider( Factory.Current.Resolve<IEntityResolverProvider>( ) );
        }
    }
}
