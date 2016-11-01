// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.Reporting.Helpers;
using EDC.ReadiNow.Metadata.Reporting;
using AggregateMethod = EDC.ReadiNow.Metadata.Query.Structured.AggregateMethod;

namespace EDC.SoftwarePlatform.Services.Test.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ReportRollupHelperTests
    {
        [Test]
        public void RemoveUnusedColumns_ReturnClone()
        {
            StructuredQuery query = new StructuredQuery( );
            ClientAggregate agg = new ClientAggregate( );
            var result = ReportRollupHelper.RemoveUnusedColumns( query, agg );
            Assert.That(result, Is.Not.SameAs(query));
        }

        [Test]
        public void RemoveUnusedColumns_GroupedColumn_Not_Removed()
        {
            SelectColumn col1 = new SelectColumn( );
            SelectColumn col2 = new SelectColumn( );           
            StructuredQuery query = new StructuredQuery();
            query.SelectColumns.Add(col1);
            query.SelectColumns.Add(col2);
            
            ClientAggregate agg = new ClientAggregate( );
            agg.GroupedColumns.Add( new ReportGroupField { ReportColumnId = col2.ColumnId } );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 1 ) );
            Assert.That( result.SelectColumns [ 0 ].ColumnId, Is.EqualTo( col2.ColumnId ) );
        }

        [Test]
        public void RemoveUnusedColumns_AggregateColumn_Not_Removed( )
        {
            SelectColumn col1 = new SelectColumn( );
            SelectColumn col2 = new SelectColumn( );
            StructuredQuery query = new StructuredQuery( );
            query.SelectColumns.Add( col1 );
            query.SelectColumns.Add( col2 );

            ClientAggregate agg = new ClientAggregate( );
            agg.AggregatedColumns.Add( new ReportAggregateField { ReportColumnId = col2.ColumnId, AggregateMethod = AggregateMethod.Max } );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 1 ) );
            Assert.That( result.SelectColumns [ 0 ].ColumnId, Is.EqualTo( col2.ColumnId ) );
        }

        [Test]
        public void RemoveUnusedColumns_EnsureAtLeastOneColumnRemains( )
        {
            SelectColumn col1 = new SelectColumn( );
            SelectColumn col2 = new SelectColumn( );
            StructuredQuery query = new StructuredQuery( );
            query.SelectColumns.Add( col1 );
            query.SelectColumns.Add( col2 );

            ClientAggregate agg = new ClientAggregate( );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 1 ) );
            Assert.That( result.SelectColumns [ 0 ].ColumnId, Is.EqualTo( col1.ColumnId ) );
        }

        [Test]
        public void RemoveUnusedColumns_EnsureUnusedOrderbyRemoved( )
        {
            SelectColumn col1 = new SelectColumn( );
            SelectColumn col2 = new SelectColumn( );
            SelectColumn col3 = new SelectColumn( );
            OrderByItem order1 = new OrderByItem { Expression = new ColumnReference { ColumnId = col1.ColumnId }, Direction = OrderByDirection.Ascending };
            OrderByItem order2 = new OrderByItem { Expression = new ColumnReference { ColumnId = col2.ColumnId }, Direction = OrderByDirection.Descending };
            StructuredQuery query = new StructuredQuery( );
            query.SelectColumns.Add( col1 );
            query.SelectColumns.Add( col2 );
            query.SelectColumns.Add( col3 );
            query.OrderBy.Add( order1 );
            query.OrderBy.Add( order2 );

            ClientAggregate agg = new ClientAggregate( );
            agg.AggregatedColumns.Add( new ReportAggregateField { ReportColumnId = col2.ColumnId, AggregateMethod = AggregateMethod.Max } );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 1 ), "Cols" );
            Assert.That( result.SelectColumns [ 0 ].ColumnId, Is.EqualTo( col2.ColumnId ) ); 

            Assert.That( result.OrderBy.Count, Is.EqualTo( 1 ), "Order" );
            Assert.That( result.OrderBy [ 0 ].Direction, Is.EqualTo( OrderByDirection.Descending ) );
        }

        [Test]
        public void RemoveUnusedColumns_EnsureAnalyzerColumn_Not_Removed( )
        {
            SelectColumn col1 = new SelectColumn( );
            SelectColumn col2 = new SelectColumn( );
            QueryCondition cond1 = new QueryCondition { Expression = new ColumnReference { ColumnId = col2.ColumnId } };
            StructuredQuery query = new StructuredQuery( );
            query.SelectColumns.Add( col1 );
            query.SelectColumns.Add( col2 );
            query.Conditions.Add( cond1 );

            ClientAggregate agg = new ClientAggregate( );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 1 ) );
            Assert.That( result.SelectColumns [ 0 ].ColumnId, Is.EqualTo( col2.ColumnId ) );
        }

        [Test]
        public void RemoveUnusedColumns_WithCount_Ensure_FieldExpression_Replaced( )
        {
            SelectColumn col1 = new SelectColumn { Expression = new ResourceDataColumn( ) };
            SelectColumn col2 = new SelectColumn { Expression = new ResourceDataColumn( ) };
            StructuredQuery query = new StructuredQuery( );
            query.SelectColumns.Add( col1 );
            query.SelectColumns.Add( col2 );

            ClientAggregate agg = new ClientAggregate( );
            agg.AggregatedColumns.Add( new ReportAggregateField { AggregateMethod = AggregateMethod.Count } );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 2 ) );
            Assert.That( result.SelectColumns [ 0 ].Expression, Is.TypeOf<IdExpression>( ) );
            Assert.That( result.SelectColumns [ 1 ].Expression, Is.TypeOf<IdExpression>( ) );
        }

        [Test]
        public void RemoveUnusedColumns_SupportQuickSearch_WithCount_Ensure_FieldExpression_NotReplaced()
        {
            SelectColumn col1 = new SelectColumn { Expression = new ResourceDataColumn() };
            SelectColumn col2 = new SelectColumn { Expression = new ResourceDataColumn() };
            StructuredQuery query = new StructuredQuery();
            query.SelectColumns.Add(col1);
            query.SelectColumns.Add(col2);

            ClientAggregate agg = new ClientAggregate();
            agg.AggregatedColumns.Add(new ReportAggregateField { AggregateMethod = AggregateMethod.Count });

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns(query, agg, true);

            Assert.That(result.SelectColumns.Count, Is.EqualTo(2));
            Assert.That(result.SelectColumns[0].Expression, Is.TypeOf<ResourceDataColumn>());
            Assert.That(result.SelectColumns[1].Expression, Is.TypeOf<ResourceDataColumn>());
            Assert.IsTrue(result.SelectColumns[0].IsHidden);
            Assert.IsTrue(result.SelectColumns[1].IsHidden);
        }

        [Test]
        public void RemoveUnusedColumns_WithCount_Ensure_AggregateExpressionWithoutGrouping_Removed( )
        {
            ResourceEntity resNode = new ResourceEntity( );
            AggregateEntity aggNode = new AggregateEntity { GroupedEntity = resNode };
            SelectColumn col1 = new SelectColumn { Expression = new ResourceDataColumn( ) };
            SelectColumn col2 = new SelectColumn { Expression = new AggregateExpression { NodeId = aggNode.NodeId } };
            StructuredQuery query = new StructuredQuery( );
            query.RootEntity = aggNode;
            query.SelectColumns.Add( col1 );
            query.SelectColumns.Add( col2 );

            ClientAggregate agg = new ClientAggregate( );
            agg.AggregatedColumns.Add( new ReportAggregateField { AggregateMethod = AggregateMethod.Count } );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 1 ) );
            Assert.That( result.SelectColumns [ 0 ].Expression, Is.TypeOf<IdExpression>( ) );
        }

        [Test]
        public void RemoveUnusedColumns_WithCount_Ensure_AggregateExpressionWithGrouping_Removed( )
        {
            ResourceEntity resNode = new ResourceEntity( );
            AggregateEntity aggNode = new AggregateEntity { GroupedEntity = resNode };
            aggNode.GroupBy.Add( new ResourceDataColumn( ) );
            SelectColumn col1 = new SelectColumn { Expression = new ResourceDataColumn( ) };
            SelectColumn col2 = new SelectColumn { Expression = new AggregateExpression { NodeId = aggNode.NodeId } };
            StructuredQuery query = new StructuredQuery( );
            query.RootEntity = aggNode;
            query.SelectColumns.Add( col1 );
            query.SelectColumns.Add( col2 );

            ClientAggregate agg = new ClientAggregate( );
            agg.AggregatedColumns.Add( new ReportAggregateField { AggregateMethod = AggregateMethod.Count } );

            StructuredQuery result = ReportRollupHelper.RemoveUnusedColumns( query, agg );

            Assert.That( result.SelectColumns.Count, Is.EqualTo( 2 ) );
            Assert.That( result.SelectColumns [ 0 ].Expression, Is.TypeOf<IdExpression>( ) );
            Assert.That( result.SelectColumns [ 1 ].Expression, Is.TypeOf<AggregateExpression>( ) );
        }
    }
}
