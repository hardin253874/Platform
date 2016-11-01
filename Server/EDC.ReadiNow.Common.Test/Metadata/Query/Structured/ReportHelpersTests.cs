// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Metadata.Query.Structured
{
    [TestFixture]
    [RunWithTransaction]
    public class ReportHelpersTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void BuildFilterQuery_Basic( )
        {
            StructuredQuery query = ReportHelpers.BuildFilterQuery( "Name='test'", new ReadiNow.Model.EntityRef( "test:person" ), false );

            Assert.That( query, Is.Not.Null );
            Assert.That( query.RootEntity, Is.Not.Null );
            Assert.That( query.RootEntity.Conditions.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        [Ignore]
        public void TestPreload( )
        {
            // Find report
            IEntity entity = CodeNameResolver.GetInstance( "Forms", "Report" );
            EntityRef field = ( new EntityRef( "core:name" ) );
            EntityRef rel = ( new EntityRef( "core:reportColumns" ) );
            var report = entity.As<EDC.ReadiNow.Model.Report>( );

            // Clear caches
            TestHelpers.ClearServerCaches( );

            // Ensure empty
            Assert.IsFalse( EntityFieldCache.Instance.ContainsKey( entity.Id ) && EntityFieldCache.Instance [ entity.Id ].ContainsField( field.Id ), "Field false" );
            Assert.IsFalse( EntityRelationshipCache.Instance.ContainsKey( new EntityRelationshipCacheKey( entity.Id, Direction.Forward ) )
                && EntityRelationshipCache.Instance [ new EntityRelationshipCacheKey( entity.Id, Direction.Forward ) ].ContainsKey( rel.Id ), "Rel false" );

            // Preload report
            ReportHelpers.PreloadReport( entity.Id );

            // Ensure present
            Assert.IsTrue( EntityFieldCache.Instance.ContainsKey( entity.Id ) && EntityFieldCache.Instance [ entity.Id ].ContainsField( field.Id ), "Field true" );
            Assert.IsTrue( EntityRelationshipCache.Instance.ContainsKey( new EntityRelationshipCacheKey( entity.Id, Direction.Forward ) )
                && EntityRelationshipCache.Instance [ new EntityRelationshipCacheKey( entity.Id, Direction.Forward ) ].ContainsKey( rel.Id ), "Rel true" );
        }

        
    }
}
