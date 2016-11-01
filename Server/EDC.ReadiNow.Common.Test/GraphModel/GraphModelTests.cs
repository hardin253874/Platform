// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.EntityGraph.GraphModel;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Core;

namespace ReadiNow.EntityGraph.Test.GraphModel
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class GraphModelTests
    {
        private IEntityRepository TestRepository
        {
            get { return Factory.GraphEntityRepository; }
            //get { return Factory.EntityRepository; }
        }

        [Test]
        [ExpectedException( typeof( DataNotLoadedException ) )]
        [RunAsDefaultTenant]
        public void LoadGraph_Typed_WithoutTypeInfo( )
        {
            string query = "id";

            TestRepository.Get<EntityType>( "core:report", query );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Typed_WithTypeInfo( )
        {
            string query = "isOfType.id";

            IEntity type = TestRepository.Get<EntityType>( "core:report", query );
            Assert.That( type, Is.Not.Null );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Untyped_WithoutTypeInfo( )
        {
            string query = "name";

            IEntity type = TestRepository.Get( "core:report", query );
            Assert.That( type, Is.Not.Null );
            var field = type.GetField<string>( "core:name" );
            Assert.That( field, Is.EqualTo( "Report" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Untyped_WithTypeInfo( )
        {
            string query = "name, isOfType.id";

            IEntity type = TestRepository.Get( "core:report", query );
            Assert.That( type, Is.Not.Null );
            var field = type.GetField<string>( "core:name" );
            Assert.That( field, Is.EqualTo( "Report" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Untyped_NonExistantId( )
        {
            string query = "name";

            IEntity type = TestRepository.Get( 99999999, query );
            Assert.That( type, Is.Null );
        }

        [Test]
        [ExpectedException]
        [RunAsDefaultTenant]
        public void LoadGraph_Untyped_NonExistantAlias( )
        {
            string query = "name";

            TestRepository.Get( "core:iDoNotExist", query );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Field_String( )
        {
            string query = "name, isOfType.id";

            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            Assert.That( type, Is.Not.Null );
            Assert.That( type.Name, Is.EqualTo( "Report" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Relationship_Forward( )
        {
            // 'inherits' is a forward relationship
            string query = "inherits.name, isOfType.id";

            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            Assert.That( type, Is.Not.Null );
            Assert.That( type.Inherits, Has.Count.GreaterThan( 0 ) );
            Assert.That( type.Inherits [ 0 ].Name, Is.Not.Empty );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Relationship_Reverse( )
        {
            // 'fields' is a reverse relationship from type
            string query = "fields.name, isOfType.id";

            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            Assert.That( type, Is.Not.Null );
            Assert.That( type.Fields, Has.Count.GreaterThan( 0 ) );
            Assert.That( type.Fields [ 0 ].Name, Is.Not.Empty );
        }

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException( typeof( DataNotLoadedException ) )]
        public void LoadGraph_Field_NotRequested( )
        {
            string query = "name, isOfType.id";
            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            string description = type.Description;
        }

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException( typeof( DataNotLoadedException ) )]
        public void LoadGraph_Relationship_NotRequested( )
        {
            string query = "name, isOfType.id";
            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            var data = type.Fields;
        }

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException( typeof( DataNotLoadedException ) )]
        public void LoadGraph_Relationship_Forward_NotRequested( )
        {
            string query = "derivedTypes.id, isOfType.id";
            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            var data = type.Inherits;
        }

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException( typeof( DataNotLoadedException ) )]
        public void LoadGraph_Relationship_Reverse_NotRequested( )
        {
            string query = "inherits.id, isOfType.id";
            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            var data = type.DerivedTypes;
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Basic( )
        {
            string query = "name, fields.name, isOfType.id";

            EntityType type = TestRepository.Get<EntityType>( "core:report", query );
            Assert.That( type, Is.Not.Null );
            Assert.That( type.Name, Is.EqualTo( "Report" ) );
            Assert.That( type.Fields, Has.Count.GreaterThan( 0 ) );
            Assert.That( type.Fields [ 0 ], Is.Not.Null );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadGraph_Basic_Concurrent( )
        {
            TestHelpers.TestConcurrent( 5, LoadGraph_Basic );
        }

    }
}
