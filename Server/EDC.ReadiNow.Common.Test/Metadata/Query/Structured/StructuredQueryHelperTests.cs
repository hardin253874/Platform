// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using NUnit.Framework;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Metadata.Query.Structured
{
    [TestFixture]
	[RunWithTransaction]
    public class StructuredQueryHelperTests : IEqualityComparer<Tuple<Entity, IEnumerable<Entity>>>
    {
        [Test]
        public void Test_VisitNodes_NullNode()
        {
            Assert.That(() => StructuredQueryHelper.VisitNodes(null, (entity, descendants) => { }),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("node"));
        }

        [Test]
        public void Test_VisitNodes_NullVisitor()
        {
            Assert.That(() => StructuredQueryHelper.VisitNodes(new ResourceEntity(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("visitor"));
        }

        [Test]
        [TestCaseSource("Test_VisitNodes_Source")]
        public void Test_VisitNodes(Entity node, IEnumerable<Tuple<Entity, IEnumerable<Entity>>> expectedResults)
        {
            List<Tuple<Entity, IEnumerable<Entity>>> actualResults;

            actualResults = new List<Tuple<Entity, IEnumerable<Entity>>>();
            StructuredQueryHelper.VisitNodes(node,
                (entity, descendants) => actualResults.Add(new Tuple<Entity, IEnumerable<Entity>>(entity, descendants)));

            Assert.That(actualResults, Is.EquivalentTo(expectedResults).Using(this));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetCacheKeyToken_Stability( )
        {
            StructuredQuery sq1 = ReportHelpers.BuildFilterQuery( "Name='test1'", new ReadiNow.Model.EntityRef( "test:person" ), true );
            sq1.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq1.RootEntity, "core:name" ) } );

            StructuredQuery sq2 = ReportHelpers.BuildFilterQuery( "Name='test1'", new ReadiNow.Model.EntityRef( "test:person" ), true );
            sq2.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq2.RootEntity, "core:name" ) } );

            string token1 = sq1.CacheKeyToken( );
            string token2 = sq2.CacheKeyToken( );

            Assert.That( token1, Is.EqualTo( token2 ).And.Not.Null );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetCacheKeyToken_Concurrent( )
        {
            StructuredQuery sq1 = ReportHelpers.BuildFilterQuery( "Name='test1'", new ReadiNow.Model.EntityRef( "test:person" ), true );
            sq1.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq1.RootEntity, "core:name" ) } );

            StructuredQuery sq2 = ReportHelpers.BuildFilterQuery( "Name='test1'", new ReadiNow.Model.EntityRef( "test:person" ), true );
            sq2.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq2.RootEntity, "core:name" ) } );

            string token1 = null;
            string token2 = null;

            Task.WaitAll(
                Task.Factory.StartNew( ( ) => { token1 = sq1.CacheKeyToken( ); } ),
                Task.Factory.StartNew( ( ) => { token2 = sq2.CacheKeyToken( ); } )
                );
            
            Assert.That( token1, Is.EqualTo( token2 ).And.Not.Null );
        }

        private IEnumerable<TestCaseData> Test_VisitNodes_Source
        {
            get
            {
                ResourceEntity rootEntity;
                ResourceEntity firstLevelEntity1;
                ResourceEntity firstLevelEntity2;
                ResourceEntity secondLevelEntity;
                ResourceEntity thirdLevelEntity;

                rootEntity = new ResourceEntity();
                yield return new TestCaseData(rootEntity, new List<Tuple<Entity, IEnumerable<Entity>>>
                {
                    new Tuple<Entity, IEnumerable<Entity>>(rootEntity, new Entity[0])    
                });

                rootEntity = new ResourceEntity();
                firstLevelEntity1 = new RelatedResource();
                rootEntity.RelatedEntities.Add(firstLevelEntity1);
                yield return new TestCaseData(rootEntity, new List<Tuple<Entity, IEnumerable<Entity>>>
                {
                    new Tuple<Entity, IEnumerable<Entity>>(rootEntity, new Entity[0]),    
                    new Tuple<Entity, IEnumerable<Entity>>(firstLevelEntity1, new [] { rootEntity })    
                });

                rootEntity = new ResourceEntity();
                firstLevelEntity1 = new RelatedResource();
                rootEntity.RelatedEntities.Add(firstLevelEntity1);
                firstLevelEntity2 = new RelatedResource();
                rootEntity.RelatedEntities.Add(firstLevelEntity2);
                yield return new TestCaseData(rootEntity, new List<Tuple<Entity, IEnumerable<Entity>>>
                {
                    new Tuple<Entity, IEnumerable<Entity>>(rootEntity, new Entity[0]),    
                    new Tuple<Entity, IEnumerable<Entity>>(firstLevelEntity1, new [] { rootEntity }),
                    new Tuple<Entity, IEnumerable<Entity>>(firstLevelEntity2, new [] { rootEntity })    
                });

                rootEntity = new ResourceEntity();
                firstLevelEntity1 = new RelatedResource();
                rootEntity.RelatedEntities.Add(firstLevelEntity1);
                firstLevelEntity2 = new RelatedResource();
                rootEntity.RelatedEntities.Add(firstLevelEntity2);
                secondLevelEntity = new RelatedResource();
                firstLevelEntity1.RelatedEntities.Add(secondLevelEntity);
                yield return new TestCaseData(rootEntity, new List<Tuple<Entity, IEnumerable<Entity>>>
                {
                    new Tuple<Entity, IEnumerable<Entity>>(rootEntity, new Entity[0]),    
                    new Tuple<Entity, IEnumerable<Entity>>(firstLevelEntity1, new [] { rootEntity }),
                    new Tuple<Entity, IEnumerable<Entity>>(secondLevelEntity, new [] { rootEntity, firstLevelEntity1 }),
                    new Tuple<Entity, IEnumerable<Entity>>(firstLevelEntity2, new [] { rootEntity })    
                });

                rootEntity = new ResourceEntity();
                firstLevelEntity1 = new RelatedResource();
                rootEntity.RelatedEntities.Add(firstLevelEntity1);
                firstLevelEntity2 = new RelatedResource();
                rootEntity.RelatedEntities.Add(firstLevelEntity2);
                secondLevelEntity = new RelatedResource();
                firstLevelEntity1.RelatedEntities.Add(secondLevelEntity);
                thirdLevelEntity = new RelatedResource();
                secondLevelEntity.RelatedEntities.Add(thirdLevelEntity);
                yield return new TestCaseData(rootEntity, new List<Tuple<Entity, IEnumerable<Entity>>>
                {
                    new Tuple<Entity, IEnumerable<Entity>>(rootEntity, new Entity[0]),    
                    new Tuple<Entity, IEnumerable<Entity>>(firstLevelEntity1, new [] { rootEntity }),
                    new Tuple<Entity, IEnumerable<Entity>>(secondLevelEntity, new [] { rootEntity, firstLevelEntity1 }),
                    new Tuple<Entity, IEnumerable<Entity>>(thirdLevelEntity, new [] { rootEntity, firstLevelEntity1, secondLevelEntity }),
                    new Tuple<Entity, IEnumerable<Entity>>(firstLevelEntity2, new [] { rootEntity })    
                });
            }
        }

        public bool Equals(Tuple<Entity, IEnumerable<Entity>> x, Tuple<Entity, IEnumerable<Entity>> y)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            return x.Item1 == y.Item1
                && x.Item2.SequenceEqual(y.Item2);
        }

        public int GetHashCode(Tuple<Entity, IEnumerable<Entity>> obj)
        {
            throw new NotImplementedException();
        }

        [TestCase(false, 0)]
        [TestCase(true, 0)]
        public void Test_WalkExpressions_None( bool filteringOnly, int expected )
        {
            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity( );

            var res = StructuredQueryHelper.WalkExpressions( sq, filteringOnly );
            Assert.That( res.Count( ), Is.EqualTo( expected ) );
        }

        [TestCase(false, 3)]
        [TestCase(true, 0)]
        public void Test_WalkExpressions_Column( bool filteringOnly, int expected )
        {
            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity( );
            sq.SelectColumns.Add( new SelectColumn { Expression = new IdExpression( ) } );
            sq.SelectColumns.Add( new SelectColumn { Expression = new MutateExpression { Expression = new IdExpression( ) } } );

            var res = StructuredQueryHelper.WalkExpressions( sq, filteringOnly );
            Assert.That( res.Count( ), Is.EqualTo( expected ) );
        }

        [TestCase(false, 3)]
        [TestCase(true, 3)]
        public void Test_WalkExpressions_Condition( bool filteringOnly, int expected )
        {
            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity( );
            sq.Conditions.Add( new QueryCondition { Expression = new IdExpression() } );
            sq.Conditions.Add( new QueryCondition { Expression = new MutateExpression { Expression = new IdExpression( ) } } );
            
            var res = StructuredQueryHelper.WalkExpressions( sq, filteringOnly );
            Assert.That( res.Count( ), Is.EqualTo( expected ) );
        }

        [TestCase(false, 3)]
        [TestCase(true, 0)]
        public void Test_WalkExpressions_OrderBy( bool filteringOnly, int expected )
        {
            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity( );
            sq.OrderBy.Add( new OrderByItem { Expression = new IdExpression() } );
            sq.OrderBy.Add( new OrderByItem { Expression = new MutateExpression { Expression = new IdExpression( ) } } );
            
            var res = StructuredQueryHelper.WalkExpressions( sq, filteringOnly );
            Assert.That( res.Count( ), Is.EqualTo( expected ) );
        }

        [TestCase(false, 3)]
        [TestCase(true, 3)]
        public void Test_WalkExpressions_NodeCondition( bool filteringOnly, int expected )
        {
            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity { Conditions = new List<ScalarExpression>( ) };
            sq.RootEntity.Conditions.Add( new IdExpression( ) );
            sq.RootEntity.Conditions.Add( new MutateExpression { Expression = new IdExpression( ) } );

            var res = StructuredQueryHelper.WalkExpressions( sq, filteringOnly );
            Assert.That( res.Count( ), Is.EqualTo( expected ) );
        }

        [TestCase(false, 3)]
        [TestCase(true, 3)]
        public void Test_WalkExpressions_GroupBy( bool filteringOnly, int expected )
        {
            StructuredQuery sq = new StructuredQuery( );
            AggregateEntity root = new AggregateEntity( );
            sq.RootEntity = root;
            root.GroupedEntity = new ResourceEntity( );
            root.GroupBy.Add( new IdExpression( ) );
            root.GroupBy.Add( new MutateExpression { Expression = new IdExpression( ) } );

            var res = StructuredQueryHelper.WalkExpressions( sq, filteringOnly );
            Assert.That( res.Count( ), Is.EqualTo( expected ) );
        }

        [Test]
        public void Test_WalkNodes_Root( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            AssertNodes( sq, root );
        }

        [Test]
        public void Test_WalkNodes_Related( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );

            AssertNodes( sq, root, related1 );
        }

        [Test]
        public void Test_WalkNodes_RelatedRelated( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            var related2 = new RelatedResource( );
            related1.RelatedEntities.Add( related2 );

            AssertNodes( sq, root, related1, related2 );
        }

        [Test]
        public void Test_WalkNodes_RelatedPeers( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            var related2 = new RelatedResource( );
            root.RelatedEntities.Add( related2 );

            AssertNodes( sq, root, related1, related2 );
        }

        [Test]
        public void Test_WalkNodes_RelatedRelatedPeers( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var related1 = new RelatedResource( );
            root.RelatedEntities.Add( related1 );
            var related2 = new RelatedResource( );
            related1.RelatedEntities.Add( related2 );
            var related3 = new RelatedResource( );
            related1.RelatedEntities.Add( related3 );

            AssertNodes( sq, root, related1, related2, related3 );
        }

        [Test]
        public void Test_WalkNodes_RootAggregate( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var agg = new AggregateEntity( );
            sq.RootEntity = agg;
            var related2 = new RelatedResource( );
            agg.GroupedEntity = related2;
            var related3 = new RelatedResource( );
            related2.RelatedEntities.Add( related3 );

            AssertNodes( sq, agg, related2, related3 );
        }

        [Test]
        public void Test_WalkNodes_RelatedAggregate( )
        {
            StructuredQuery sq = new StructuredQuery( );
            var root = sq.RootEntity = new ResourceEntity( );
            var agg = new AggregateEntity( );
            root.RelatedEntities.Add( agg );
            var related2 = new RelatedResource( );
            agg.GroupedEntity = related2;
            var related3 = new RelatedResource( );
            related2.RelatedEntities.Add( related3 );

            AssertNodes( sq, root, agg, related2, related3 );
        }

        private void AssertNodes( StructuredQuery query, params Entity [ ] nodes )
        {
            var actual = StructuredQueryHelper.WalkNodes( query.RootEntity );
            Assert.That( actual, Is.EquivalentTo( nodes ), "Expected nodes" );
        }

        [Test]
        public void Test_FindNodePath_NullNode()
        {
            List<Entity> findNodePath = StructuredQueryHelper.FindNodePath(Guid.Empty, null);

            Assert.AreEqual(findNodePath.Count, 0);          
        }

        [Test]
        public void Test_FindNodePath_TwoLevel()
        {
            ResourceEntity rootEntity;
            ResourceEntity firstLevelEntity;
            Guid rootEntityNodeId = Guid.NewGuid();
            Guid firstLevelEntityNodeId = Guid.NewGuid();


            rootEntity = new ResourceEntity{ NodeId = rootEntityNodeId };
            firstLevelEntity = new RelatedResource { NodeId = firstLevelEntityNodeId };
            rootEntity.RelatedEntities.Add(firstLevelEntity);


            List<Entity> findNodePath = StructuredQueryHelper.FindNodePath(firstLevelEntityNodeId, rootEntity);

            Assert.AreEqual(findNodePath.Count, 1);  
        }

        [Test]
        public void Test_FindNodePath_ThreeLevel()
        {
            ResourceEntity rootEntity;
            ResourceEntity firstLevelEntity;
            ResourceEntity secondLevelEntity;
            Guid rootEntityNodeId = Guid.NewGuid();
            Guid firstLevelEntityNodeId = Guid.NewGuid();
            Guid secondLevelEntityNodeId = Guid.NewGuid();

            rootEntity = new ResourceEntity { NodeId = rootEntityNodeId };
            firstLevelEntity = new RelatedResource { NodeId = firstLevelEntityNodeId };
            secondLevelEntity = new RelatedResource { NodeId = secondLevelEntityNodeId };
            firstLevelEntity.RelatedEntities.Add(secondLevelEntity);
            rootEntity.RelatedEntities.Add(firstLevelEntity);


            List<Entity> findNodePath = StructuredQueryHelper.FindNodePath(secondLevelEntityNodeId, rootEntity);

            Assert.AreEqual(findNodePath.Count, 2);
        }

        [Test]
        public void Test_FindNodePath_FourLevel()
        {
            ResourceEntity rootEntity;
            ResourceEntity firstLevelEntity;
            ResourceEntity secondLevelEntity;
            ResourceEntity thirdLevelEntity;
            Guid rootEntityNodeId = Guid.NewGuid();
            Guid firstLevelEntityNodeId = Guid.NewGuid();
            Guid secondLevelEntityNodeId = Guid.NewGuid();
            Guid thirdLevelEntityNodeId = Guid.NewGuid();

            rootEntity = new ResourceEntity { NodeId = rootEntityNodeId };
            firstLevelEntity = new RelatedResource { NodeId = firstLevelEntityNodeId };
            secondLevelEntity = new RelatedResource { NodeId = secondLevelEntityNodeId };
            thirdLevelEntity = new RelatedResource { NodeId = thirdLevelEntityNodeId };
            secondLevelEntity.RelatedEntities.Add(thirdLevelEntity);
            firstLevelEntity.RelatedEntities.Add(secondLevelEntity);
            rootEntity.RelatedEntities.Add(firstLevelEntity);


            List<Entity> findNodePath = StructuredQueryHelper.FindNodePath(thirdLevelEntityNodeId, rootEntity);

            Assert.AreEqual(findNodePath.Count, 3);
        }

        [Test]
        public void Test_FindNodePath_FourLevelWithAggregate()
        {
            ResourceEntity rootEntity;
            ResourceEntity firstLevelEntity;
            AggregateEntity secondAggregateEntity;
            ResourceEntity secondLevelEntity;
            ResourceEntity thirdLevelEntity;
            
            Guid rootEntityNodeId = Guid.NewGuid();
            Guid firstLevelEntityNodeId = Guid.NewGuid();
            Guid secondLevelEntityNodeId = Guid.NewGuid();
            Guid thirdLevelEntityNodeId = Guid.NewGuid();

            rootEntity = new ResourceEntity { NodeId = rootEntityNodeId };
            firstLevelEntity = new RelatedResource { NodeId = firstLevelEntityNodeId };
            secondAggregateEntity = new AggregateEntity();
            secondLevelEntity = new RelatedResource { NodeId = secondLevelEntityNodeId };
            thirdLevelEntity = new RelatedResource { NodeId = thirdLevelEntityNodeId };
            secondLevelEntity.RelatedEntities.Add(thirdLevelEntity);
            secondAggregateEntity.GroupedEntity = secondLevelEntity;
            firstLevelEntity.RelatedEntities.Add(secondAggregateEntity);
            rootEntity.RelatedEntities.Add(firstLevelEntity);


            List<Entity> findNodePath = StructuredQueryHelper.FindNodePath(thirdLevelEntityNodeId, rootEntity);

            Assert.AreEqual(findNodePath.Count, 3);
        }


    }
}
