// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.Builder
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class QueryOptimiserTests
    {
        [Test]
        public void NotPruned_ColumnReferesToRoot( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            AddColumn(sq, new IdExpression { NodeId = root.NodeId });

            var pruned = StructuredQueryHelper.PruneQueryTree(sq);

            Assert.That( pruned, Is.SameAs( sq ) );
            AssertNodes( pruned, root );
        }

        [Test]
        public void NotPruned_ColumnReferesToRelated( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            AddColumn( sq, new IdExpression { NodeId = related1.NodeId } );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.SameAs( sq ) );
            AssertNodes( pruned, root, related1 );
        }

        [Test]
        public void NotPruned_ColumnReferesToRelatedRelated( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            var related2 = new RelatedResource( );
            related1.RelatedEntities.Add( related2 );
            AddColumn( sq, new IdExpression { NodeId = related2.NodeId } );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.SameAs( sq ) );
            AssertNodes( pruned, root, related1, related2 );
        }

        [Test]
        public void Pruned_UnusedFirstRelationship( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            AddColumn( sq, new IdExpression { NodeId = root.NodeId } );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.Not.SameAs( sq ) );
            AssertNodes( pruned, root );
        }

        [Test]
        public void Pruned_UnusedChildRelationship( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            var related2 = new RelatedResource( );
            related1.RelatedEntities.Add( related2 );
            AddColumn( sq, new IdExpression { NodeId = related1.NodeId } );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.Not.SameAs( sq ) );
            AssertNodes( pruned, root, related1 );
        }

        [Test]
        public void Pruned_UnusedPeerRelationship( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            var related2 = new RelatedResource( );
            root.RelatedEntities.Add( related2 );
            AddColumn( sq, new IdExpression { NodeId = related1.NodeId } );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.Not.SameAs( sq ) );
            AssertNodes( pruned, root, related1 );
        }

        [Test]
        public void Pruned_UnusedChildAggregate( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var agg = new AggregateEntity( );
            root.RelatedEntities.Add( agg );
            var related2 = new RelatedResource( );
            agg.GroupedEntity = related2;
            var related3 = new RelatedResource( );
            related2.RelatedEntities.Add(related3);
            AddColumn( sq, new IdExpression { NodeId = root.NodeId } );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.Not.SameAs( sq ) );
            AssertNodes( pruned, root );
        }

        [Test]
        public void NotPruned_UnusedRootAggregate( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var agg = new AggregateEntity( );
            sq.RootEntity = agg;
            var related2 = new RelatedResource( );
            agg.GroupedEntity = related2;
            var related3 = new RelatedResource( );
            related2.RelatedEntities.Add( related3 );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.SameAs( sq ) );
            AssertNodes( pruned, agg, related2, related3 );
        }

        [Test]
        public void NotPruned_ChildrenOfUsedAggregate( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var agg = new AggregateEntity( );
            root.RelatedEntities.Add( agg );
            var related2 = new RelatedResource( );
            agg.GroupedEntity = related2;
            var related3 = new RelatedResource( );
            related2.RelatedEntities.Add( related3 );
            AddColumn( sq, new IdExpression { NodeId = agg.NodeId } );

            var pruned = StructuredQueryHelper.PruneQueryTree( sq );

            Assert.That( pruned, Is.SameAs( sq ) );
            AssertNodes( pruned, root, agg, related2, related3 );
        }

        private void AddColumn( StructuredQuery query, ScalarExpression expr )
        {
            query.SelectColumns.Add( new SelectColumn { Expression = expr } );
        }

        private void AssertNodes( StructuredQuery query, params Entity[] nodes )
        {
            var actual = StructuredQueryHelper.WalkNodes( query.RootEntity );
            
            // need to compare IDs instead of nodes because mutations result in clones
            Assert.That( actual.Select( n => n.NodeId ), Is.EquivalentTo( nodes.Select( n => n.NodeId ) ), "Expected nodes" );
        }
    }
}
