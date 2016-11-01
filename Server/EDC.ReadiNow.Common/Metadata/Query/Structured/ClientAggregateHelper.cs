// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autofac;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Model;
using EDC.Xml;
using System.Collections.Generic;
using System.Xml;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// A static helper class for dealing with ClientAggregate objects.
    /// </summary>
    public static class ClientAggregateHelper
    {
        #region Entity Model To Client Aggregate Functionality        

        /// <summary>
        /// Grouped columns for report.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="query">The query.</param>
        /// <returns>List{ReportAggregateField}.</returns>
        internal static List<ReportGroupField> GroupedColumnsForReport(Report report, StructuredQuery query)
        {
            var converter = Factory.Current.Resolve<IReportToQueryPartsConverter>( );
            
            return (from column in report.ReportColumns 
                    from columnGrouping in column.ColumnGrouping
                    orderby columnGrouping.GroupingPriority
                    where converter.IsValidExpression( column.ColumnExpression )
                    select new ReportGroupField
                               {
                                   ReportColumnId = query.SelectColumns.First(sc => sc.EntityId == column.Id).ColumnId,
                                   ReportColumnEntityId = column.Id,
                                   GroupMethod = GroupingMethodFromEntityEnum(columnGrouping.GroupingMethod),
                                   ShowGrandTotals = report.RollupGrandTotals ?? false,
                                   ShowSubTotals = report.RollupSubTotals ?? false,
                                   ShowRowCounts = report.RollupRowCounts ?? false,
                                   ShowRowLabels = report.RollupRowLabels ?? false,
                                   ShowOptionLabel = report.RollupOptionLabels ?? false,
                                   Collapsed = columnGrouping.GroupingCollapsed ?? false
                               }).ToList();            
        }

        

            /// <summary>
            /// Aggregated columns for report.
            /// </summary>
            /// <param name="report">The report.</param>
            /// <param name="query">The query.</param>
            /// <returns>List{ReportAggregateField}.</returns>
        internal static List<ReportAggregateField> AggregatedColumnsForReport(Report report, StructuredQuery query)
        {
            var converter = Factory.Current.Resolve<IReportToQueryPartsConverter>( );
            
            return ( from column in report.ReportColumns
                    from rollup in column.ColumnRollup
                    where converter.IsValidExpression( column.ColumnExpression )
                    select new ReportAggregateField
                        {
                            ReportColumnId = column.Id == 0 ? Guid.Empty : query.SelectColumns.First(sc => sc.EntityId == column.Id).ColumnId,
                            ReportColumnEntityId = column.Id,
                            AggregateMethod = AggregateMethodFromEntityEnum(rollup.RollupMethod),
                            ShowGrandTotals = report.RollupGrandTotals ?? false,
                            ShowSubTotals = report.RollupSubTotals ?? false,
                            ShowRowCounts = report.RollupRowCounts ?? false,
                            ShowRowLabels = report.RollupRowLabels ?? false,
                            ShowOptionLabel = report.RollupOptionLabels ?? false,
                            IncludedCount = column.Id == 0
                        }).ToList();
        }


        private static AggregateMethod AggregateMethodFromEntityEnum(AggregateMethodEnum methodEnum)
        {
            string[] enumerationStringArray = methodEnum.Alias.Split(':');
            string entityEnum = enumerationStringArray.Count() == 1 ? enumerationStringArray[0].Substring(3) : enumerationStringArray[1].Substring(3);
            AggregateMethod method;
            return Enum.TryParse(entityEnum, true, out method) ? method : AggregateMethod.List;
        }

        private static GroupMethod GroupingMethodFromEntityEnum(GroupingMethodEnum groupingMethod)
        {
            string[] enumerationStringArray = groupingMethod.Alias.Split(':');
            string entityEnum = enumerationStringArray.Count() == 1 ? enumerationStringArray[0].Substring(5) : enumerationStringArray[1].Substring(5);
            GroupMethod method;
            return Enum.TryParse(entityEnum, true, out method) ? method : GroupMethod.List;
        }

        #endregion
    }
}
