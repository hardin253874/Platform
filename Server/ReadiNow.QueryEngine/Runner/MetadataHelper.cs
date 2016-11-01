// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Data;
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.Runner
{
    /// <summary>
    /// Encapsulates result column metadata processing.
    /// </summary>
    class MetadataHelper
    {
        /// <summary>
        /// TODO: Refactor
        /// </summary>
        internal static void CaptureMetadata(StructuredQuery query, QuerySettings settings, QueryResult result, DataTable dataTable)
        {

            if (settings.SupportClientAggregate)
            {
                int totalGroupedColumns = settings.ClientAggregate.GroupedColumns?.Count ?? 0;

                int totalColumns = totalGroupedColumns + settings.ClientAggregate.AggregatedColumns.Count;

                for (int index = 0; index < totalColumns; index++)
                {
                    int columnIndex;
                    DataColumn tableColumn = dataTable.Columns[index];
                    if (index < totalGroupedColumns)
                    {
                        columnIndex = index;
                        if (settings.ClientAggregate.GroupedColumns != null)
                        {
                            tableColumn.ExtendedProperties["ColumnId"] = settings.ClientAggregate.GroupedColumns[columnIndex].ReportColumnId;
                        }
                    }
                    else
                    {
                        columnIndex = index - totalGroupedColumns;
                        if (settings.ClientAggregate.AggregatedColumns != null)
                        {
                            tableColumn.ExtendedProperties["ColumnId"] = settings.ClientAggregate.AggregatedColumns[columnIndex].ReportColumnId;
                        }
                    }
                    tableColumn.ExtendedProperties["ColumnIndex"] = columnIndex;
                }

                result.AggregateDataTable = dataTable;
            }
            else
            {
                int index = 0;
                foreach (SelectColumn selectColumn in query.SelectColumns)
                {
                    // Set column ID into data table column object (why??)
                    DataColumn tableColumn = dataTable.Columns[index];
                    tableColumn.ExtendedProperties["ColumnId"] = selectColumn.ColumnId;
                    // Set data type into result column info, for columns (e.g. calculated) where we can only determine it dynamically at the moment
                    ResultColumn resultColumn = result.Columns[index];
                    if (resultColumn.ColumnType == null)
                    {
                        if (selectColumn.Expression is AggregateExpression && ((AggregateExpression)selectColumn.Expression).Expression is ResourceExpression)
                        {
                            if (((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.Count ||
                                                          ((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.CountUniqueItems ||
                                                          ((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.CountUniqueNotBlanks ||
                                                          ((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.CountWithValues)
                            {
                                resultColumn.ColumnType = new EDC.Database.Types.Int32Type();
                            }
                            else
                            {
                                resultColumn.ColumnType = ((ResourceExpression)((AggregateExpression)selectColumn.Expression).Expression).CastType;
                                resultColumn.IsResource = true;
                                if (resultColumn.ColumnType == null)
                                    resultColumn.ColumnType = new EDC.Database.Types.ChoiceRelationshipType();
                            }
                        }
                        else if (selectColumn.Expression is AggregateExpression && ((AggregateExpression)selectColumn.Expression).Expression is ResourceDataColumn)
                        {
                            if (((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.Count ||
                                                           ((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.CountUniqueItems ||
                                                           ((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.CountUniqueNotBlanks ||
                                                           ((AggregateExpression)selectColumn.Expression).AggregateMethod == AggregateMethod.CountWithValues)
                            {
                                resultColumn.ColumnType = new EDC.Database.Types.Int32Type();
                            }
                            else
                            {
                                resultColumn.ColumnType = ((ResourceDataColumn)((AggregateExpression)selectColumn.Expression).Expression).CastType;
                                //resultColumn.IsResource = true;
                            }
                        }
                        else if (selectColumn.Expression is StructureViewExpression)
                        {
                            resultColumn.ColumnType = new EDC.Database.Types.StructureLevelsType();
                        }
                        else
                        {
                            ResourceExpression resourceExpression = selectColumn.Expression as ResourceExpression;
                            if (resourceExpression != null)
                            {
                                resultColumn.ColumnType = resourceExpression.CastType;
                                resultColumn.IsResource = true;
                            }
                            else
                            {
                                resultColumn.ColumnType = DatabaseType.ConvertFromType(tableColumn.DataType);
                            }
                        }
                    }

                    index++;
                }
                result.DataTable = dataTable;
            }
        }                

    }
}
