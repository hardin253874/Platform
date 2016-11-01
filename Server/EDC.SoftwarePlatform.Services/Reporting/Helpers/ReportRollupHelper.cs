// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting;

namespace ReadiNow.Reporting.Helpers
{
    /// <summary>
    /// Helper methods for performing activities specific to rollup reports.
    /// </summary>
    internal static class ReportRollupHelper
    {
        /// <summary>
        /// Patch up the client aggregate with counts _if_ rollup grand totals or sub totals are requested and there is no count included in the client aggregate.
        /// </summary>
        /// <param name="clientAggregate"></param>
        /// <param name="query"></param>
        public static void EnsureShowTotalsHasCount( StructuredQuery query, ClientAggregate clientAggregate )
        {
            // if ((rollupGrandTotals || rollupSubTotals) && clientAggregate.GroupedColumns.Any()) //<< remove commented code and delete the follow_if_ when the builder sets these flags!
			if ( clientAggregate.GroupedColumns.Count > 0 )
            {
                ReportGroupField reportGroupField = clientAggregate.GroupedColumns.First( );
                // Check to see if the first column group contains a count in the aggregate
                if ( clientAggregate.AggregatedColumns.FirstOrDefault( ac => ac.AggregateMethod == AggregateMethod.Count ) == null )
                {
                    // Inject a count aggregate for a column that is _not_ a group column
                    SelectColumn column = query.SelectColumns.FirstOrDefault( sc => clientAggregate.GroupedColumns.All( gc => gc.ReportColumnId != sc.ColumnId ) );
                    if ( column != null )
                    {
                        clientAggregate.AggregatedColumns.Add( new ReportAggregateField
                        {
                            AggregateMethod = AggregateMethod.Count,
                            IncludedCount = true,
                            ReportColumnId = column.ColumnId,
                            ReportColumnEntityId = column.EntityId,
                            ShowGrandTotals = reportGroupField.ShowGrandTotals,
                            ShowRowCounts = reportGroupField.ShowRowCounts,
                            ShowOptionLabel = reportGroupField.ShowOptionLabel,
                            ShowRowLabels = reportGroupField.ShowRowLabels,
                            ShowSubTotals = reportGroupField.ShowSubTotals
                        } );
                    }
                }
            }
        }

        /// <summary>
        /// Remove any columns from a report that are not required to achieve a rollup result.
        /// </summary>
        /// <remarks>
        /// If columns get removed here then the query optimiser will later remove various joins.
        /// This can affect whether some rows are repeated.
        /// Some aggregate types (e.g. max/min) are unaffected by this.
        /// Some types (e.g. Count) are very affected by this, so we replace columns with simpler ones, rather than removing them completely.
        /// Some types (e.g. Sum) in principle could be affected by this, but in practice are OK because they will reference the relationship branch that is relevant to them anyway.
        /// </remarks>
        /// <param name="query">The original query.</param>
        /// <param name="clientAggregate">Aggregate settings that are used to determine what columns are used.</param>
        /// <returns>A clone of the query, with unused columns removed.</returns>
        public static StructuredQuery RemoveUnusedColumns( StructuredQuery query, ClientAggregate clientAggregate, bool supportQuickSearch = false )
        {
            StructuredQuery queryCopy = query.DeepCopy( );

            // Determine columns that are used by the rollup
            HashSet<Guid> referencedColumns = new HashSet<Guid>( clientAggregate.AggregatedColumns.Select( a => a.ReportColumnId )
                .Concat( clientAggregate.GroupedColumns.Select( g => g.ReportColumnId ) ) );

            // Also include columns that are referenced by analyzer conditions
            foreach ( QueryCondition condition in query.Conditions )
            {
                ColumnReference colRefExpr = condition.Expression as ColumnReference;
                if ( colRefExpr == null )
                    continue;
                referencedColumns.Add( colRefExpr.ColumnId );
            }

            // Ensure that the inner report returns at least something. (This will typically be the ID column).
            if ( referencedColumns.Count == 0 && query.SelectColumns.Count > 0 )
            {
                referencedColumns.Add( query.SelectColumns [ 0 ].ColumnId );
            }

            // There are two types of optimisations. Either we can just pull out all unused columns, and let the structured query optimiser
            // remove the subsequent relationship joins - which is OK for things like max & min in particular.
            // But in some cases (e.g. for Count) we need to ensure we capture all relationships to get the true fanout.
            // This is safer, but less efficient.
            bool strictlyMaintainRowPresence = clientAggregate.AggregatedColumns.Any( ag => ag.AggregateMethod == AggregateMethod.Count );

            if ( !strictlyMaintainRowPresence )
            {
                // Remove all unused columns
                queryCopy.SelectColumns.RemoveAll( column => !referencedColumns.Contains( column.ColumnId ) );
            }
            else
            {
                // Visit each column and determine if it can be removed, or converted to a simpler type
                List<SelectColumn> columns = queryCopy.SelectColumns;
                List<Guid> toRemove = new List<Guid>( );
                for ( int i = 0; i < columns.Count; i++ )
                {
                    SelectColumn column = columns [ i ];
                    if ( referencedColumns.Contains( column.ColumnId ) )
                        continue;

                    // Replace field lookups with an equivalent ID column .. to maintain the relationship join, but remove the field join.
                    ResourceDataColumn fieldColumnExpr = column.Expression as ResourceDataColumn;
                    if ( fieldColumnExpr != null )
                    {
                        // TODO: we could delete this column entire IF the entire relationship path to it is 'to one' relationships, without any advance properties to enforce rows.                        
                        // UPDATE: for quick serach purpose, the column expression should be ResourceDataColumn but skip in select clause.
                        // however for performance reasons, if without quick search, still change to IdExpression
                        if (supportQuickSearch)
                            column.IsHidden = true;
                        else
                            column.Expression = new IdExpression { NodeId = fieldColumnExpr.NodeId };

                        continue;
                    }

                    // Remove aggregate expressions if they don't have group-bys. (A group-by would cause the aggregate to return more than one row).
                    AggregateExpression aggExpr = column.Expression as AggregateExpression;
                    if ( aggExpr != null )
                    {
                        AggregateEntity aggNode = StructuredQueryHelper.FindNode(queryCopy.RootEntity, aggExpr.NodeId) as AggregateEntity;
                        if (aggNode != null)
                        {
                            if (aggNode.GroupBy == null || aggNode.GroupBy.Count == 0)
                            {
                                toRemove.Add( column.ColumnId );
                            }
                        }
                    }

                    // Would be nice to remove calculated columns .. but probably too risky
                }
                queryCopy.SelectColumns.RemoveAll( column => toRemove.Contains( column.ColumnId ) );
            }

            // Remove any obsolete order-by instructions
            queryCopy.OrderBy.RemoveAll( orderBy =>
            {
                ColumnReference colRefExpr = orderBy.Expression as ColumnReference;
                if ( colRefExpr == null )
                    return false;

                bool colStillPresent = queryCopy.SelectColumns.Any( column => column.ColumnId == colRefExpr.ColumnId );
                return !colStillPresent;
            } );

            return queryCopy;
        }

    }
}
