// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Utc;
using ReadiNow.Reporting;
using AggregateExpression = EDC.ReadiNow.Metadata.Query.Structured.AggregateExpression;
using AggregateMethod = EDC.ReadiNow.Metadata.Query.Structured.AggregateMethod;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ScriptExpression = EDC.ReadiNow.Metadata.Query.Structured.ScriptExpression;
using StructureViewExpression = EDC.ReadiNow.Metadata.Query.Structured.StructureViewExpression;

namespace EDC.SoftwarePlatform.Services.Data
{
	/// <summary>
	///     Provides helper methods for interacting with data tables.
	/// </summary>
	public static class DbDataTableHelper
	{
		public static DbDataTable ConvertTo( QueryResult queryResult, List<ResultColumn> queryResultColumns, DataTable dataTable, DataTable aggregateDataTable, StructuredQuery sourceQuery, ClientAggregate clientAggregate, ReportObject reportObject )
		{
			var columns = new List<DbDataColumn>( );
			var databaseTypeDictionary = new Dictionary<Guid, DatabaseType>( );
			for ( int i = 0; i < queryResultColumns.Count; i++ )
			{
				queryResult.Columns[ i ] = queryResultColumns[ i ];
				ResultColumn resultColumn = queryResultColumns[ i ];
				DataColumn dataColumn = dataTable.Columns[ i ];

				var dbColumn = new DbDataColumn( );

				SelectColumn selectColumn = null;
				Guid columnId = Guid.Empty;
				// Check for a column id property
				if ( dataColumn.ExtendedProperties.ContainsKey( "ColumnId" ) )
				{
					columnId = ( Guid ) dataColumn.ExtendedProperties[ "ColumnId" ];
					selectColumn = sourceQuery.SelectColumns.FirstOrDefault( c => c.ColumnId == columnId );

					if ( !databaseTypeDictionary.ContainsKey( columnId ) )
					{
						databaseTypeDictionary.Add( columnId, resultColumn.ColumnType );
					}
				}

				// Have not found the column yet fallback to using the ordinal
				if ( selectColumn == null )
				{
					selectColumn = sourceQuery.SelectColumns[ dataColumn.Ordinal ];
				}


				//if current column in groupedcolumn, same as columntype
				//otherwise, use string type

				bool existAggregatedColumn = clientAggregate.AggregatedColumns.Any( gc => gc.ReportColumnId == columnId );

				if ( existAggregatedColumn )
				{
					dbColumn.Type = new StringType( );
				}
				else
				{
					// Determine column type
					if ( resultColumn.ColumnType != null )
					{
						if ( dataColumn.ExtendedProperties.ContainsKey( "DisplayPattern" ) && !string.IsNullOrEmpty( ( string ) dataColumn.ExtendedProperties[ "DisplayPattern" ] ) )
						{
							dbColumn.Type = new AutoIncrementType( );
						}
						else
						{
							dbColumn.Type = resultColumn.ColumnType;
						}
					}
					else
					{
						dbColumn.Type = GetColumnDatabaseType( sourceQuery, selectColumn );
						if ( dbColumn.Type is UnknownType )
						{
							dbColumn.Type = DatabaseType.ConvertFromType( dataColumn.DataType );
						}
					}
				}


				dbColumn.Id = selectColumn.ColumnId;
				dbColumn.Name = string.IsNullOrEmpty( selectColumn.DisplayName ) ? selectColumn.ColumnName : selectColumn.DisplayName;
				dbColumn.ColumnName = selectColumn.ColumnName;
				dbColumn.IsHidden = selectColumn.IsHidden;

				columns.Add( dbColumn );
			}

			var rows = new List<DbDataRow>( );

			// Convert the rows
			int rowIndex = 0;
			foreach ( DataRow aggregateDataRow in aggregateDataTable.Rows )
			{
				bool isGrandTotalRow = rowIndex == ( aggregateDataTable.Rows.Count - 1 );
				rowIndex++;

				var row = new DbDataRow( );

				// Convert each field
				foreach ( DataColumn dataColumn in dataTable.Columns )
				{
					Guid columnId = Guid.Empty;
					DatabaseType databaseType = DatabaseType.StringType;
					DatabaseType selectColumnType = DatabaseType.StringType;
					// Check for a column id property
					if ( dataColumn.ExtendedProperties.ContainsKey( "ColumnId" ) )
					{
						columnId = ( Guid ) dataColumn.ExtendedProperties[ "ColumnId" ];
						SelectColumn selectColumn = sourceQuery.SelectColumns.FirstOrDefault( c => c.ColumnId == columnId );
						databaseType = databaseTypeDictionary.ContainsKey( columnId ) ? databaseTypeDictionary[ columnId ] : DatabaseType.StringType;

						var expression = selectColumn.Expression as EntityExpression;

						if ( expression != null )
						{
							selectColumnType = GetSelectColumnCastType( expression, databaseType );
						}
					}

					bool existGroupedColumn = clientAggregate.GroupedColumns != null && clientAggregate.GroupedColumns.Any( gc => gc.ReportColumnId == columnId );
					bool existAggregatedColumn = clientAggregate.AggregatedColumns.Any( gc => gc.ReportColumnId == columnId );

					//not existing in GroupedColumn or AggregatedColumn
					if ( !existGroupedColumn && !existAggregatedColumn )
					{
						row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( null );
					}
					else if ( existGroupedColumn )
					{
						List<DataColumn> aggregatedDataColumns = GetAggregatedDataColumns( aggregateDataTable.Columns, columnId );

						DataColumn aggregatedDataColumn = aggregatedDataColumns.Count > 0 ? aggregatedDataColumns[ 0 ] : null;

						object obj = aggregateDataRow[ aggregatedDataColumn ];

						if ( !Convert.IsDBNull( obj ) )
						{
							string displayPattern = null;

							if ( dataColumn.ExtendedProperties.ContainsKey( "DisplayPattern" ) )
							{
								displayPattern = ( string ) dataColumn.ExtendedProperties[ "DisplayPattern" ];
							}

							string additionStringformating = GetAddtionalFormatString( columnId, reportObject );

							string cellValue = DatabaseTypeHelper.ConvertToString( columns[ dataColumn.Ordinal ].Type, aggregateDataRow[ aggregatedDataColumn ], displayPattern );
							if ( !string.IsNullOrEmpty( additionStringformating ) )
							{
								cellValue = string.Format( additionStringformating, aggregateDataRow[ aggregatedDataColumn ] );
							}


							row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( cellValue );
						}
						else
						{
							row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( null );
						}
					}
					else if ( existAggregatedColumn )
					{
						List<DataColumn> aggregatedDataColumns = GetAggregatedDataColumns( aggregateDataTable.Columns, columnId );
						var aggregatedValues = new List<string>( );
						foreach ( DataColumn aggregatedDataColumn in aggregatedDataColumns )
						{
							string columnName = aggregatedDataColumn.ColumnName;
							columnName = columnName.Split( ' ' )[ 0 ];

							int groupedColumnCount = clientAggregate.GroupedColumns != null ? clientAggregate.GroupedColumns.Count : 0;
							int columnIndex = aggregateDataTable.Columns.IndexOf( aggregatedDataColumn );

							int aggregateColumnIndex = columnIndex - groupedColumnCount;
							ReportAggregateField reportAggregateField = clientAggregate.AggregatedColumns[ aggregateColumnIndex ];
							AggregateMethod aggregateMethod = reportAggregateField.AggregateMethod;
							if ( clientAggregate.AggregatedColumns[ aggregateColumnIndex ] != null )
							{
								if ( aggregateMethod == AggregateMethod.Count || aggregateMethod == AggregateMethod.CountUniqueItems ||
								     aggregateMethod == AggregateMethod.CountUniqueNotBlanks || aggregateMethod == AggregateMethod.CountWithValues )
								{
									databaseType = DatabaseType.Int32Type;
								}
							}

							string displayPattern = DatabaseTypeHelper.GetDisplayFormatString( databaseType );

							string additionStringformating = GetAddtionalFormatString( columnId, reportObject );

							bool doFormat = !( aggregateMethod == AggregateMethod.Count || aggregateMethod == AggregateMethod.CountUniqueItems ||
							                   aggregateMethod == AggregateMethod.CountUniqueNotBlanks || aggregateMethod == AggregateMethod.CountWithValues );

							if ( string.IsNullOrEmpty( additionStringformating ) )
							{
								if ( doFormat )
								{
									if ( selectColumnType is CurrencyType )
									{
										//displayPattern = "{0:c3}";
									}
									else if ( selectColumnType is DecimalType )
									{
										displayPattern = "{0:N3}";
									}
								}
							}
							else
							{
								displayPattern = additionStringformating;
							}
							string aggregateValue = string.Empty;
							if ( doFormat )
							{
								if ( selectColumnType is DateTimeType ) // convert to local time
								{
									DateTime tempDate;
									if ( DateTime.TryParse( aggregateDataRow[ aggregatedDataColumn ].ToString( ), out tempDate ) )
									{
										tempDate = TimeZoneHelper.ConvertToLocalTime( tempDate, sourceQuery.TimeZoneName );
										aggregateValue = string.Format( displayPattern, tempDate );
									}
								}
								else
								{
									aggregateValue = string.Format( displayPattern, aggregateDataRow[ aggregatedDataColumn ] );
								}
							}
							else
								aggregateValue = aggregateDataRow[ aggregatedDataColumn ].ToString( );

							if ( selectColumnType is ChoiceRelationshipType && ( aggregateMethod == AggregateMethod.Max || aggregateMethod == AggregateMethod.Min ) )
							{
								aggregateValue = UpdateChoiceFieldXmlValue( aggregateValue );
							}

							if ( doFormat && selectColumnType is CurrencyType && !aggregateValue.StartsWith( "$" ) )
							{
								aggregateValue = string.Format( "{0:c2}", aggregateDataRow[ aggregatedDataColumn ] );
							}


							if ( columnName == "Count" )
							{
								columnName = "Count";
							}
							else if ( columnName == "CountWithValues" )
							{
								columnName = "Count with values";
							}
							else if ( columnName == "CountUniqueItems" )
							{
								columnName = "Count unique";
							}

							if ( reportAggregateField != null && ( ( isGrandTotalRow && reportAggregateField.ShowGrandTotals ) || ( !isGrandTotalRow && reportAggregateField.ShowSubTotals ) ) )
								aggregatedValues.Add( string.Format( "{0}: {1}", columnName, aggregateValue ) );
						}

						row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( string.Join( "\r", aggregatedValues ) );
					}
					else
					{
						row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( null );
					}
				}

				rows.Add( row );
			}

			var dbTable = new DbDataTable
			{
				TableName = dataTable.TableName,
				Columns = columns,
				Rows = rows
			};

			if ( dataTable.ExtendedProperties.ContainsKey( "Id" ) )
			{
				dbTable.Id = ( Guid ) dataTable.ExtendedProperties[ "Id" ];
			}

			if ( dataTable.ExtendedProperties.ContainsKey( "Name" ) )
			{
				dbTable.Name = ( string ) dataTable.ExtendedProperties[ "Name" ];
			}

			return dbTable;
		}

		/// <summary>
		///     Converts the data table from one format to another format.
		/// </summary>
		/// <param name="queryResult">The query result.</param>
		/// <param name="sourceQuery">The source query.</param>
		/// <param name="reportObject">The report object.</param>
		/// <returns>An in-memory copy of the converted data table.</returns>
		public static DbDataTable ConvertTo( QueryResult queryResult, StructuredQuery sourceQuery, ReportObject reportObject )
		{
		    DbDataTable dbTable = null;
			DataTable dataTable = queryResult.DataTable;

			if ( queryResult.DataTable != null )
			{
				var columns = new List<DbDataColumn>( );

				// Convert the columns
				var columnDictionary = new Dictionary<DataColumn, SelectColumn>( );

				for ( int i = 0; i < queryResult.Columns.Count; i++ )
				{
					ResultColumn resultColumn = queryResult.Columns[ i ];
					DataColumn dataColumn = queryResult.DataTable.Columns[ i ];

					var dbColumn = new DbDataColumn( );

					SelectColumn selectColumn = null;

					// Check for a column id property
					if ( dataColumn.ExtendedProperties.ContainsKey( "ColumnId" ) )
					{
						var columnId = ( Guid ) dataColumn.ExtendedProperties[ "ColumnId" ];
						selectColumn = sourceQuery.SelectColumns.FirstOrDefault( c => c.ColumnId == columnId );
						if ( !columnDictionary.ContainsKey( dataColumn ) )
							columnDictionary.Add( dataColumn, selectColumn );
					}

					// Have not found the column yet fallback to using the ordinal
					if ( selectColumn == null )
					{
						selectColumn = sourceQuery.SelectColumns[ dataColumn.Ordinal ];
					}

					// Determine column type
					if ( resultColumn.ColumnType != null )
					{
						if ( dataColumn.ExtendedProperties.ContainsKey( "DisplayPattern" ) && !string.IsNullOrEmpty( ( string ) dataColumn.ExtendedProperties[ "DisplayPattern" ] ) )
						{
							dbColumn.Type = new AutoIncrementType( );
						}
						else
						{
							dbColumn.Type = resultColumn.ColumnType;
						}
					}
					else
					{
						dbColumn.Type = GetColumnDatabaseType( sourceQuery, selectColumn );
						if ( dbColumn.Type is UnknownType )
						{
							dbColumn.Type = DatabaseType.ConvertFromType( dataColumn.DataType );
						}
					}

					dbColumn.Id = selectColumn.ColumnId;
					dbColumn.Name = string.IsNullOrEmpty( selectColumn.DisplayName ) ? selectColumn.ColumnName : selectColumn.DisplayName;
					dbColumn.ColumnName = selectColumn.ColumnName;
					dbColumn.IsHidden = selectColumn.IsHidden;

					columns.Add( dbColumn );
				}

				var rows = new List<DbDataRow>( );

				// Convert the rows

				foreach ( DataRow dataRow in dataTable.Rows )
				{
					var row = new DbDataRow( );

					// Convert each field
					foreach ( DataColumn dataColumn in dataTable.Columns )
					{
						object obj = dataRow[ dataColumn ];

						if ( !Convert.IsDBNull( obj ) )
						{
							row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( DatabaseTypeHelper.ConvertToString( columns[ dataColumn.Ordinal ].Type, dataRow[ dataColumn ] ) );
						}
						else
						{
							row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( null );
						}
					}

					rows.Add( row );
				}

				dbTable = new DbDataTable
				{
					TableName = dataTable.TableName,
					Columns = columns,
					Rows = rows
				};

				if ( dataTable.ExtendedProperties.ContainsKey( "Id" ) )
				{
					dbTable.Id = ( Guid ) dataTable.ExtendedProperties[ "Id" ];
				}

				if ( dataTable.ExtendedProperties.ContainsKey( "Name" ) )
				{
					dbTable.Name = ( string ) dataTable.ExtendedProperties[ "Name" ];
				}
			}
			else if ( queryResult.AggregateDataTable != null )
			{
				var columns = new List<DbDataColumn>( );

				// Convert the columns
				for ( int i = 0; i < queryResult.AggregateColumns.Count; i++ )
				{
					ResultColumn resultColumn = queryResult.AggregateColumns[ i ];
					DataColumn dataColumn = queryResult.AggregateDataTable.Columns[ i ];

					var dbColumn = new DbDataColumn( );

					SelectColumn selectColumn = null;

					// Check for a column id property
					if ( dataColumn.ExtendedProperties.ContainsKey( "ColumnId" ) )
					{
						var columnId = ( Guid ) dataColumn.ExtendedProperties[ "ColumnId" ];
						selectColumn = sourceQuery.SelectColumns.FirstOrDefault( c => c.ColumnId == columnId );
					}

					// Have not found the column yet fallback to using the ordinal
					if ( selectColumn == null )
					{
						selectColumn = sourceQuery.SelectColumns[ dataColumn.Ordinal ];
					}

					// Determine column type
					if ( resultColumn.ColumnType != null )
					{
						if ( dataColumn.ExtendedProperties.ContainsKey( "DisplayPattern" ) && !string.IsNullOrEmpty( ( string ) dataColumn.ExtendedProperties[ "DisplayPattern" ] ) )
						{
							dbColumn.Type = new AutoIncrementType( );
						}
						else
						{
							dbColumn.Type = resultColumn.ColumnType;
						}
					}
					else
					{
						dbColumn.Type = GetColumnDatabaseType( sourceQuery, selectColumn );
						if ( dbColumn.Type is UnknownType )
						{
							dbColumn.Type = DatabaseType.ConvertFromType( dataColumn.DataType );
						}
					}

					dbColumn.Id = selectColumn.ColumnId;
					dbColumn.Name = string.IsNullOrEmpty( selectColumn.DisplayName ) ? selectColumn.ColumnName : selectColumn.DisplayName;
					dbColumn.ColumnName = resultColumn.AggregateColumn.AggregateMethod == AggregateMethod.List ? selectColumn.ColumnName : resultColumn.AggregateColumn.AggregateMethod.ToString( );
					dbColumn.IsHidden = selectColumn.IsHidden;

					columns.Add( dbColumn );
				}

				var rows = new List<DbDataRow>( );

				// Convert the rows

				foreach ( DataRow dataRow in dataTable.Rows )
				{
					var row = new DbDataRow( );

					// Convert each field
					foreach ( DataColumn dataColumn in dataTable.Columns )
					{
						object obj = dataRow[ dataColumn ];

						if ( !Convert.IsDBNull( obj ) )
						{
							row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( DatabaseTypeHelper.ConvertToString( columns[ dataColumn.Ordinal ].Type, dataRow[ dataColumn ] ) );
						}
						else
						{
							row.FieldsByOrdinal[ dataColumn.Ordinal ] = new DbDataField( null );
						}
					}

					rows.Add( row );
				}

				dbTable = new DbDataTable
				{
					TableName = dataTable.TableName,
					Columns = columns,
					Rows = rows
				};

				if ( dataTable.ExtendedProperties.ContainsKey( "Id" ) )
				{
					dbTable.Id = ( Guid ) dataTable.ExtendedProperties[ "Id" ];
				}

				if ( dataTable.ExtendedProperties.ContainsKey( "Name" ) )
				{
					dbTable.Name = ( string ) dataTable.ExtendedProperties[ "Name" ];
				}
			}


			return dbTable;
		}

		/// <summary>
		///     Get addtional FormatString from current report object's data view
		/// </summary>
		/// <param name="columnId"></param>
		/// <param name="reportObject"></param>
		/// <returns></returns>
		private static string GetAddtionalFormatString( Guid columnId, ReportObject reportObject )
		{
			var gridReportDataView = reportObject.DataViews.FirstOrDefault( dv => dv is GridReportDataView );
			if ( gridReportDataView != null && ( ( GridReportDataView ) gridReportDataView ).ColumnFormats != null )
			{
				var columnFormat = ( ( GridReportDataView ) gridReportDataView ).ColumnFormats.FirstOrDefault( cf => cf.QueryColumnId == columnId );
				if ( columnFormat != null )
				{
					return columnFormat.FormatString;
				}
				return string.Empty;
			}
			return string.Empty;
		}


		private static List<DataColumn> GetAggregatedDataColumns( DataColumnCollection aggregatedColumns, Guid columnId )
		{
			var aggregatedDataColumn = new List<DataColumn>( );

			foreach ( DataColumn aggregatedColumn in aggregatedColumns )
			{
				if ( aggregatedColumn.ExtendedProperties.ContainsKey( "ColumnId" ) )
				{
					try
					{
						if ( columnId == ( Guid ) aggregatedColumn.ExtendedProperties[ "ColumnId" ] )
						{
							aggregatedDataColumn.Add( aggregatedColumn );
						}
					}
					catch ( Exception exc )
					{
						EventLog.Application.WriteWarning( exc.Message );
					}
				}
			}

			return aggregatedDataColumn;
		}

		/// <summary>
		///     Gets the type of the column database.
		/// </summary>
		/// <param name="sourceQuery">The source query.</param>
		/// <param name="selectColumn">The select column.</param>
		/// <returns></returns>
		private static DatabaseType GetColumnDatabaseType( StructuredQuery sourceQuery, SelectColumn selectColumn )
		{
			DatabaseType columnType = DatabaseType.UnknownType;

			var field = selectColumn.Expression as ResourceDataColumn;

			if ( field != null )
			{
				ResourceDataColumn dataField = field;
				if ( dataField.CastType != null && dataField.CastType.GetDisplayName( ) != DatabaseType.UnknownType.GetDisplayName( ) )
					columnType = dataField.CastType;
				else
					columnType = GetFieldDataType( dataField.FieldId );
			}
			else if ( selectColumn.Expression is StructureViewExpression )
			{
				columnType = DatabaseType.StructureLevelsType;
			}
			else
			{
				var expression = selectColumn.Expression as CalculationExpression;

				if ( expression != null )
				{
					//TODO hack now
					if ( expression.DisplayType != null && expression.DisplayType != DatabaseType.UnknownType )
						return expression.DisplayType;
				}
				else
				{
					var reference = selectColumn.Expression as ColumnReference;

					if ( reference != null )
					{
						ColumnReference columnReference = reference;
						// Sanity check. Prevent infinite recursion
						if ( columnReference.ColumnId != selectColumn.ColumnId )
						{
							SelectColumn referencedColumn = sourceQuery.SelectColumns.FirstOrDefault( c => c.ColumnId == columnReference.ColumnId );
							columnType = GetColumnDatabaseType( sourceQuery, referencedColumn );
						}
					}
					else
					{
						var aggregateExpression = selectColumn.Expression as AggregateExpression;

						if ( aggregateExpression != null )
						{
						    if (aggregateExpression.Expression is StructureViewExpression)
						    {
                                columnType = DatabaseType.StructureLevelsType;
						    }
						    else
						    {
                                columnType = (aggregateExpression.Expression is ResourceDataColumn) ? ((ResourceDataColumn)(aggregateExpression.Expression)).CastType : DatabaseType.StringType;   
						    }							
						}
						else if ( selectColumn.Expression is IdExpression )
						{
							columnType = DatabaseType.IdentifierType;
						}
					}
				}
			}

			return columnType;
		}


		/// <summary>
		///     Gets the type of the field data.
		/// </summary>
		/// <param name="fieldEntityRef">The field entity preference.</param>
		/// <returns>DatabaseType.</returns>
		private static DatabaseType GetFieldDataType( EntityRef fieldEntityRef )
		{
			DatabaseType type = DatabaseType.UnknownType;

			if ( fieldEntityRef != null )
			{
				try
				{
					var field = Entity.Get<Field>( fieldEntityRef );
					string readiNowType = string.Format( "EDC.Database.Types.{0}, {1}", field.GetFieldType().ReadiNowType, typeof ( DatabaseType ).Assembly.FullName );

					var typeType = Type.GetType( readiNowType );

					if ( typeType != null )
					{
						return Activator.CreateInstance( typeType ) as DatabaseType;
					}
				}
				catch
				{
					type = DatabaseType.UnknownType;
				}
			}

			return type;
		}

		private static DatabaseType GetSelectColumnCastType( EntityExpression expression, DatabaseType databaseType )
		{
			var column = expression as ResourceDataColumn;

			if ( column != null )
			{
				return column.CastType;
			}

			var aggregateExpression = expression as AggregateExpression;

			if ( aggregateExpression != null )
			{
				return GetSelectColumnCastType( ( EntityExpression ) ( aggregateExpression.Expression ), databaseType );
			}
			if ( expression is IdExpression )
			{
				return DatabaseType.IdentifierType;
			}
			if ( expression is ScriptExpression )
			{
				return databaseType;
			}
			return DatabaseType.StringType;
		}

		private static string UpdateChoiceFieldXmlValue( string xml )
		{
			if ( xml.IndexOf( "<e id=", StringComparison.Ordinal ) > 0 )
			{
				return xml.Substring( xml.IndexOf( "<e id=", StringComparison.Ordinal ) );
			}
			return xml;
		}
	}
}