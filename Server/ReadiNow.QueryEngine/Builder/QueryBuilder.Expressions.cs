// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using EDC.Common;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using Model = EDC.ReadiNow.Model;
using AggregateExpression = EDC.ReadiNow.Metadata.Query.Structured.AggregateExpression;
using ScriptExpression = EDC.ReadiNow.Metadata.Query.Structured.ScriptExpression;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ResourceExpression = EDC.ReadiNow.Metadata.Query.Structured.ResourceExpression;
using StructureViewExpression = EDC.ReadiNow.Metadata.Query.Structured.StructureViewExpression;
using ReadiNow.QueryEngine.Builder.SqlObjects;

namespace ReadiNow.QueryEngine.Builder
{
    /// <summary>
    /// QueryBuilder partial class.
    /// Contains members for processing SQL expressions.
    /// </summary>
    public partial class QueryBuilder
    {
        /// <summary>
        ///     Add () for SQL String
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <returns></returns>
        private SqlExpression BracketExpression( ScalarExpression expression, SqlQuery sqlQuery )
        {
            if ( expression is CalculationExpression || expression is LogicalExpression || expression is ComparisonExpression )
            {
                SqlExpression converted = ConvertExpression( expression, sqlQuery );
                SqlExpression bracketed = new SqlExpression( "(" + converted.Sql + ")" );
                SqlExpression.CopyTransforms( converted, bracketed );
                bracketed.DatabaseType = converted.DatabaseType;
                bracketed.DisplaySqlCallback = tmpExpr => "(" + converted.DisplaySql + ")";
                bracketed.ResultSqlCallback = tmpExpr => "(" + converted.ResultSql + ")";
                bracketed.ConditionSqlCallback = tmpExpr => "(" + converted.ConditionSql + ")";
                bracketed.OrderingSqlCallback = tmpExpr => "(" + converted.OrderingSql + ")";
                return bracketed;
            }

            return ConvertExpression( expression, sqlQuery );
        }

        /// <summary>
		///     Add () for SQL String
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <returns></returns>
		private string BracketNextLevelExpression( ScalarExpression expression, SqlQuery sqlQuery )
        {
            // Deprecated .. use BracketExpression instead.
            return BracketExpression( expression, sqlQuery ).Sql;
		}

		/// <summary>
		///     Handler exception for example expression:10/0
		/// </summary>
		/// <param name="avoidString">The avoid string.</param>
		/// <param name="Operator">The operator.</param>
		/// <returns></returns>
		private string CalculationAvoidSQLExceptionHandler( string avoidString, CalculationOperator Operator )
        {
			if ( Operator == CalculationOperator.Divide || Operator == CalculationOperator.Modulo )
			{
				return string.Format( " (case when {0}=0 then null else {0} end)", avoidString );
            }

			return avoidString;
		}

		/// <summary>
		///		Converts the aggregate expression.
		/// </summary>
		/// <param name="aggExpression">The aggregate expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertAggregateExpression( AggregateExpression aggExpression, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertAggregateExpressionImpl( aggExpression, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the aggregate expression.
		/// </summary>
		/// <param name="aggExpression">The aggregate expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertAggregateExpression( AggregateExpression aggExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertAggregateExpressionImpl( aggExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

	    /// <summary>
        /// Converts an aggregate expression, such as max or sum.
        /// </summary>
        /// <param name="aggExpression">The agg expression.</param>
        /// <param name="sqlQuery">The SQL query.</param>
        private bool TryConvertAggregateExpressionImpl(AggregateExpression aggExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    // Check that source is applicable
			    // In a non-nested aggregate scenario, innerQuery will be the same as sqlQuery
			    SqlQuery innerQuery = ReferenceManager.FindQueryContainingEntity( aggExpression.NodeId, sqlQuery );
			    if ( innerQuery == null )
			    {
				    convertResult.SetFailure( ConvertResult.FailureReason.Aggregate );
					return false;
			    }

			    // Get field type
			    Field field = null;
			    FieldType fieldType = null;
			    if ( aggExpression.Expression is ResourceDataColumn )
			    {
				    field = Model.Entity.Get<Field>( ( ( ResourceDataColumn ) aggExpression.Expression ).FieldId );
				    fieldType = field.GetFieldType( );
			    }
			    // Find the sub query
			    SqlTable proxyTable = innerQuery.References.FindSqlTable( aggExpression.NodeId );
			    SqlQuery subQuery = proxyTable.SubQuery.Queries[ 0 ];
			    if ( subQuery == null )
			    {
				    convertResult.SetFailure( ConvertResult.FailureReason.Exception, "The aggregate expression NodeId does not point to a grouped entity." );
					return false;
			    }

			    // Special handling for XML lists
			    if ( aggExpression.AggregateMethod == AggregateMethod.List )
			    {
					return TryConvertAggregateXmlListExpressionImpl( aggExpression, sqlQuery, ref convertResult );
			    }

			    // Build the expression to go into the grouped sub query
			    string sql = null;
			    SqlExpression sqlExpr;
			    DatabaseType resultType = null;

			    // Delegate to perform pre-aggregate transforms
			    Func<SqlExpression, string> getPreAggregateSql = expr =>
			    {
				    string childSql = expr.PreAggregateTransform != null ? expr.PreAggregateTransform( expr, aggExpression.AggregateMethod ) : expr.Sql;
				    return childSql;
			    };

			    // Determine if aggregates have been nested
			    if ( innerQuery == sqlQuery )
			    {
				    // Handle non-nested case
				    bool handleLargeValues = false;

				    switch ( aggExpression.AggregateMethod )
				    {
					    case AggregateMethod.Count:
						    sql = "count(*)";
						    resultType = DatabaseType.Int32Type;
						    break;
					    case AggregateMethod.Sum:
						    sql = "sum({0})";
						    handleLargeValues = true;
						    break;
					    case AggregateMethod.Max:
						    sql = "max({0})";
						    break;
					    case AggregateMethod.Min:
						    sql = "min({0})";
						    break;
					    case AggregateMethod.Average:
						    sql = "avg(try_convert(decimal(38,9), {0}))";
						    resultType = DatabaseType.DecimalType;
						    break;
					    case AggregateMethod.Variance:
						    sql = "var({0})";
						    resultType = DatabaseType.DecimalType;
						    break;
					    case AggregateMethod.PopulationVariance:
						    sql = "varp({0})";
						    resultType = DatabaseType.DecimalType;
						    break;
					    case AggregateMethod.StandardDeviation:
						    sql = "stdev({0})";
						    resultType = DatabaseType.DecimalType;
						    break;
					    case AggregateMethod.PopulationStandardDeviation:
						    sql = "stdevp({0})";
						    resultType = DatabaseType.DecimalType;
						    break;
					    case AggregateMethod.CountWithValues:
						    sql = "count({0})";
						    resultType = DatabaseType.Int32Type;
						    break;
					    case AggregateMethod.CountUniqueItems:
						    sql = "count(distinct {0})";
						    resultType = DatabaseType.Int32Type;
						    break;
					    case AggregateMethod.CountUniqueNotBlanks:
						    sql = "count(distinct nullif({0},''))";
						    resultType = DatabaseType.Int32Type;
						    break;
				    }

				    // Insert the child expression
				    // (I.e. the expression within the aggregate function)
				    if ( aggExpression.AggregateMethod == AggregateMethod.Count )
				    {
					    sqlExpr = new SqlExpression( sql );
				    }
				    else
				    {
					    if ( aggExpression.Expression == null )
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Aggregate expression cannot be null, except when calculating a count." );
						    return false;
					    }

					    if ( sql == null )
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Sql statement cannot be null." );
						    return false;
					    }

					    // Evaluate child expression
					    SqlExpression childExpr = ConvertExpression( aggExpression.Expression, subQuery );

					    // Determine result type
					    if ( resultType == null || ( childExpr.DatabaseType is CurrencyType && resultType is DecimalType ) )
					    {
						    resultType = childExpr.DatabaseType;
					    }


					    string childSql = getPreAggregateSql( childExpr );

					    if ( handleLargeValues && childExpr.DatabaseType is Int32Type )
					    {
						    childSql = string.Concat( "convert(bigint, ", childSql, ")" );
					    }

					    if ( aggExpression.AggregateMethod == AggregateMethod.CountUniqueNotBlanks && aggExpression.Expression is ResourceDataColumn )
					    {
						    //if the field is decimal field, 
						    if ( fieldType != null && !string.IsNullOrEmpty( fieldType.DbFieldTable ) &&
						         ( fieldType.DbFieldTable == "Data_Decimal" || fieldType.DbFieldTable == "Data_Int" ) )
						    {
							    sql = string.Format( "count(distinct nullif({0},0))", childSql );
						    }
						    else
						    {
							    sql = string.Format( sql, childSql );
						    }
					    }
					    else if ( aggExpression.AggregateMethod == AggregateMethod.Average && aggExpression.Expression is ResourceDataColumn )
					    {
						    //if the field is decimal field, 
						    if ( fieldType != null && !string.IsNullOrEmpty( fieldType.DbFieldTable ) &&
						         ( fieldType.DbFieldTable == "Data_Decimal" ) )
						    {
							    sql = string.Format( sql, childSql );
							    sql = string.Format( "try_convert(decimal(38,3), {0})", sql );
						    }
						    else
						    {
							    sql = string.Format( sql, childSql );
						    }
					    }
					    else
					    {
						    sql = string.Format( sql, childSql );
					    }




					    sql = string.Format( sql, childSql );
					    sqlExpr = childExpr.PostAggregateTransform != null ? childExpr.PostAggregateTransform( sql, aggExpression.AggregateMethod ) : new SqlExpression( sql );
				    }
			    }
			    else
			    {
				    // Handle nested case

				    // Fix subQuery and proxyTable so they only points to the immediate child sub query, rather than the deep one
				    subQuery = innerQuery;
				    while ( subQuery.ParentQuery != sqlQuery )
				    {
					    subQuery = subQuery.ParentQuery;
				    }
				    proxyTable = subQuery.ProxyTable;

				    // Build recursive expression
				    SqlExpression child1;
				    string child1Sql;

				    switch ( aggExpression.AggregateMethod )
				    {
					    case AggregateMethod.Sum:
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );
						    sql = string.Format( "sum({0})", child1Sql );
						    resultType = child1.DatabaseType;
						    break;
					    case AggregateMethod.Min:
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );
						    sql = string.Format( "min({0})", child1Sql );
						    resultType = child1.DatabaseType;
						    break;
					    case AggregateMethod.Max:
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );
						    sql = string.Format( "max({0})", child1Sql );
						    resultType = child1.DatabaseType;
						    break;
					    case AggregateMethod.Count:
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );
						    sql = string.Format( "sum({0})", child1Sql ); // must use sum(count) when nesting count
						    resultType = DatabaseType.Int32Type;
						    break;
					    case AggregateMethod.CountWithValues:
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );
						    sql = string.Format( "count({0})", child1Sql );
						    resultType = DatabaseType.Int32Type;
						    break;
					    case AggregateMethod.CountUniqueItems:
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );
						    sql = string.Format( "count(distinct {0})", child1Sql );
						    resultType = DatabaseType.Int32Type;
						    break;
					    case AggregateMethod.CountUniqueNotBlanks:
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );

						    //if the field is decimal field, 
						    if ( fieldType != null && !string.IsNullOrEmpty( fieldType.DbFieldTable ) &&
						         ( fieldType.DbFieldTable == "Data_Decimal" || fieldType.DbFieldTable == "Data_Int" ) )
						    {
							    sql = string.Format( "count(distinct nullif({0},0))", child1Sql );
						    }
						    else
						    {
							    sql = string.Format( "count(distinct nullif({0},''))", child1Sql );
						    }
						    resultType = DatabaseType.Int32Type;
						    break;
					    case AggregateMethod.Average:
						    // Hack. Average is sum(sum())/sum(count()). Beware of count if average is triple nested
						    aggExpression.AggregateMethod = AggregateMethod.Sum;
						    child1 = ConvertAggregateExpression( aggExpression, subQuery );
						    child1Sql = getPreAggregateSql( child1 );
						    aggExpression.AggregateMethod = AggregateMethod.Count;
						    SqlExpression child2 = ConvertAggregateExpression( aggExpression, subQuery );
						    string child2Sql = getPreAggregateSql( child2 );
						    aggExpression.AggregateMethod = AggregateMethod.Average;
						    // Generate division at top level only. If count=0, then make the average return null.
						    sql = string.Format( "case when sum({1}) = 0 then null else sum({0}) / sum({1}) end", child1Sql, child2Sql );
						    resultType = DatabaseType.DecimalType;
						    break;
					    default:
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Unsupported for nested aggregates: " + aggExpression.AggregateMethod );
						    return false;
				    }

				    if ( child1 != null && child1.PostAggregateTransform != null )
				    {
					    sqlExpr = child1.PostAggregateTransform( sql, aggExpression.AggregateMethod );
				    }
				    else
				    {
					    sqlExpr = new SqlExpression( sql );
				    }
			    }

			    // Add the expression to the sub query
			    string aggColAlias = sqlQuery.AliasManager.CreateAlias( "aggCol" );
			    var aggColumn = new SqlSelectItem
			    {
				    Alias = aggColAlias,
				    Expression = sqlExpr
			    };
			    subQuery.SelectClause.Items.Add( aggColumn );

			    // Now generate a new expression for use in parent queries that refer to the column
			    // Note: Count, count with value, count unique items, count unique Not blanks and Sum should evaluate to zero if the group is empty. Others should evaluate to null.
			    string resSql = GetColumnSql(proxyTable, aggColAlias);
			    if ( aggExpression.AggregateMethod == AggregateMethod.Count || aggExpression.AggregateMethod == AggregateMethod.Sum || aggExpression.AggregateMethod == AggregateMethod.CountWithValues || aggExpression.AggregateMethod == AggregateMethod.CountUniqueItems || aggExpression.AggregateMethod == AggregateMethod.CountUniqueNotBlanks )
			    {
				    resSql = string.Format( "isnull({0}, 0)", resSql );
			    }
			    //ResourceDataColumn resourceDataColumn = aggExpression.Expression as ResourceDataColumn;
			    //if (resourceDataColumn != null)
			    //{
			    //    sqlExpr.DatabaseType = resourceDataColumn.CastType;
			    //}
			    sqlExpr.DatabaseType = resultType;

			    var resExpr = new SqlExpression( resSql );
			    SqlExpression.CopyTransforms( sqlExpr, resExpr );
			    resExpr.DatabaseType = resultType;

			    if ( aggExpression.Expression is ResourceExpression && ( aggExpression.AggregateMethod == AggregateMethod.Max || aggExpression.AggregateMethod == AggregateMethod.Min ) )
			    {
				    resExpr.IsResource = true;
			    }

			    convertResult.Expression = resExpr;
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
        }

		/// <summary>
		///		Converts the aggregate XML list expression.
		/// </summary>
		/// <param name="aggExpression">The aggregate expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertAggregateXmlListExpression( AggregateExpression aggExpression, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertAggregateXmlListExpressionImpl( aggExpression, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the aggregate XML list expression.
		/// </summary>
		/// <param name="aggExpression">The aggregate expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertAggregateXmlListExpression( AggregateExpression aggExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertAggregateXmlListExpressionImpl( aggExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		/// Converts an aggregate expression that compresses multiple rows into a single XML list.
		/// </summary>
		/// <param name="aggExpression">The agg expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
        private bool TryConvertAggregateXmlListExpressionImpl(AggregateExpression aggExpression, SqlQuery sqlQuery, ref ConvertResult convertResult)
        {
		    try
		    {
			    // Check that source is applicable
			    // In a non-nested aggregate scenario, innerQuery will be the same as sqlQuery
			    SqlQuery innerQuery = ReferenceManager.FindQueryContainingEntity( aggExpression.NodeId, sqlQuery );

			    // Find the sub query
			    SqlTable proxyTable = innerQuery.References.FindSqlTable( aggExpression.NodeId );
			    SqlQuery subQuery = proxyTable.SubQuery.Queries[ 0 ];

			    // Convert the expression being compressed
			    SqlExpression childExpr = ConvertExpression( aggExpression.Expression, subQuery, true );

			    // Set up an XML subquery to collect the results
			    SqlQuery query = new SqlQuery( sqlQuery.FullStatement )
			    {
				    ForClause = "for xml raw('e')",
				    FromClause =
				    {
					    RootTable = subQuery.FromClause.RootTable,
					    ConstrainInWhereClause = subQuery.FromClause.ConstrainInWhereClause
				    }
			    };

			    // Generate an 'id' attribute, if we can
			    EntityExpression ee = aggExpression.Expression as EntityExpression;
			    if ( ee != null )
			    {
				    SqlExpression idExpr = ConvertExpression( new IdExpression
				    {
					    NodeId = ee.NodeId
				    }, subQuery );
				    query.SelectClause.Items.Add(
					    new SqlSelectItem
					    {
						    Alias = "id",
						    Expression = idExpr
					    } );
			    }
			    // Generate a 'text' attribute containing the data
			    query.SelectClause.Items.Add(
				    new SqlSelectItem
				    {
					    Alias = "text",
					    Expression = childExpr
				    } );

			    // Sort by the same
			    query.OrderClause.Items.Add(
				    new SqlOrderItem
				    {
					    Expression = new SqlExpression( childExpr.OrderingSql )
				    } );

			    // Temporarily apply the constraint to the parent, by fetching it from the aggregate proxy parent
			    var child = query.FromClause.RootTable;
			    int k = child.Conditions.Count;
			    if ( query.FromClause.RootTable.ParentQuery.ProxyTable.Parent != null )
			    {
				    // This ensures that if a related branch was aggregated, that results are bound to the correct parent
				    var parent = query.FromClause.RootTable.ParentQuery.ProxyTable.Parent;
				    if ( child.Conditions.Count > 0 )
					    AddJoinCondition( child, parent, child.JoinColumn, child.ForeignColumn );
			    }

			    // Bind any group-by terms in the aggregate query such that the same constraint applies between the parent row and the child XML result
			    var groupByMap = proxyTable.GroupByMap;
			    if ( groupByMap != null )
			    {
				    foreach ( KeyValuePair<ScalarExpression, SqlExpression> pair in groupByMap )
				    {
					    SqlExpression groupByExpr = ConvertExpression( pair.Key, sqlQuery );
					    SqlExpression proxyGroupByExpr = pair.Value;
					    query.AddWhereCondition( new SqlExpression( "isnull({0},-1) = isnull({1},-1)", groupByExpr.Sql, proxyGroupByExpr.Sql ) );
				    }
			    }


                // Render the SQL
                // Note: we're being naughty and rendering before the main render phase
                SqlBuilderContext builder = new SqlBuilderContext( this );
			    builder.Indent( );
			    builder.Indent( );
			    builder.AppendOnNewLine( "(" );
			    builder.Indent( );

			    if ( aggExpression.Expression is ResourceExpression )
				    query.SelectClause.UseResultSql = true;


			    int conditionCountBeforeRender = child.Conditions.Count;
			    List<SqlTable> childSqlTables = new List<SqlTable>( subQuery.FromClause.RootTable.Children );
                
			    query.RenderSql( builder );

			    //after query renderSql, the child conditions will add the tenant filter $.TenantId = @tenant
			    k += ( child.Conditions.Count - conditionCountBeforeRender );

			    //after query renderSql, the rootTable children may different, set back to original.
			    if ( subQuery.FromClause.RootTable.Children.Count( ) != childSqlTables.Count( ) )
			    {
				    subQuery.FromClause.RootTable.Children = childSqlTables;
			    }

			    builder.EndIndent( );
			    builder.AppendOnNewLine( ")" );

			    // Note: we're being naughty and reusing this table in multiple contexts, so need to revert the join condition back to where it was
			    if ( child.Conditions.Count > k )
			    {
				    // Note: SqlFromClause.AddJoinCondition inserted the conditions at the front
				    child.Conditions.RemoveRange( 0, child.Conditions.Count - k );
			    }

			    string sql = builder.ToString( );
			    //the expression's sql query same as dsipaySql, resultSql,OrderingSql and ConditionSql now
			    //need rewrite the Aggregate List SQL method later.
			    SqlExpression sqlExpression = new SqlExpression( sql )
			    {
				    DatabaseType = childExpr.DatabaseType,
				    //    DisplaySqlCallback = childExpr.DisplaySqlCallback,
				    //    ResultSqlCallback = childExpr.ResultSqlCallback,
				    //    OrderingSqlCallback = childExpr.OrderingSqlCallback,
				    ConditionSqlCallback = childExpr.ConditionSqlCallback,
				    PreAggregateTransform = childExpr.PreAggregateTransform,
				    PostAggregateTransform = childExpr.PostAggregateTransform,
				    IsResource = true
			    };

			    convertResult.Expression = sqlExpression;
			    return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
        }

		/// <summary>
		///		Converts the script expression.
		/// </summary>
		/// <param name="scriptExpression">The script expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertScriptExpression( ScriptExpression scriptExpression, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertScriptExpressionImpl( scriptExpression, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		/// Tries to convert the script expression.
		/// </summary>
		/// <param name="scriptExpression">The script expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertScriptExpression( ScriptExpression scriptExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertScriptExpressionImpl( scriptExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Converts a script expression.
		/// </summary>
		/// <param name="scriptExpression">The script expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Incorrect number of arguments passed to LOG</exception>
        private bool TryConvertScriptExpressionImpl(ScriptExpression scriptExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    if ( scriptExpression.Calculation == null )
			    {
				    if ( scriptExpression.StaticError != null )
				    {
						convertResult.Expression = new SqlExpression( "null" )
					    {
						    StaticError = scriptExpression.StaticError
					    };
						return true;
				    }

					convertResult.SetFailure( ConvertResult.FailureReason.InvalidOperation, "Script must be pre-parsed." );
				    return false;
			    }

			    var result = ConvertExpression( scriptExpression.Calculation, sqlQuery );

			    // Null if the script was in error.
			    if ( scriptExpression.ExpressionTree != null )
			    {
				    int? decimalPlaces = scriptExpression.ExpressionTree.ResultType.DecimalPlaces;
				    if ( decimalPlaces != null )
					    result.DecimalPlacesCallback = ( ) => decimalPlaces;

				    if ( scriptExpression.ExpressionTree.ResultType.Type == DataType.Entity )
					    result.IsResource = true;

				    result.Constant = scriptExpression.ExpressionTree.ResultType.Constant;
			    }
			    else
			    {
				    result.Constant = true;
			    }

			    if ( scriptExpression.ResultType != null )
			    {
				    result.DatabaseType = scriptExpression.ResultType;
				    if ( scriptExpression.ExpressionTree != null )
				    {
					    result.ResourceTypeId = scriptExpression.ExpressionTree.ResultType.EntityTypeId;
				    }
			    }



			    convertResult.Expression = result;
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
        }

		/// <summary>
		///		Converts the calculation expression.
		/// </summary>
		/// <param name="calculationExpression">The calculation expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertCalculationExpression( CalculationExpression calculationExpression, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertCalculationExpressionImpl( calculationExpression, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the calculation expression.
		/// </summary>
		/// <param name="calculationExpression">The calculation expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertCalculationExpression( CalculationExpression calculationExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertCalculationExpressionImpl( calculationExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Converts the calculation expression.
		/// </summary>
		/// <param name="calculationExpression">The calculation expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Incorrect number of arguments passed to LOG</exception>
		private bool TryConvertCalculationExpressionImpl( CalculationExpression calculationExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    var sbExpression = new StringBuilder( );
			    DatabaseType resultType = null;

			    int expressionPosition;

			    var result = new SqlExpression( );

			    switch ( calculationExpression.Operator )
			    {
					    /* SQL Maths */
				    case CalculationOperator.Add:
					    expressionPosition = 1;
					    foreach ( ScalarExpression expression in calculationExpression.Expressions )
					    {
						    if ( expressionPosition > 1 )
						    {
							    sbExpression.Append( " + " );
						    }

						    sbExpression.Append( BracketNextLevelExpression( expression, sqlQuery ) );
						    expressionPosition++;
					    }

					    break;

				    case CalculationOperator.Subtract:

					    expressionPosition = 1;
					    foreach ( ScalarExpression expression in calculationExpression.Expressions )
					    {
						    if ( expressionPosition > 1 )
						    {
							    sbExpression.Append( " - " );
						    }

						    sbExpression.Append( BracketNextLevelExpression( expression, sqlQuery ) );

						    expressionPosition++;
					    }
					    break;

				    case CalculationOperator.Negate:
					    sbExpression.AppendFormat( "-({0})", BracketNextLevelExpression( calculationExpression.Expressions[ 0 ], sqlQuery ) );
					    break;

				    case CalculationOperator.Divide:
					    expressionPosition = 1;
					    foreach ( ScalarExpression expression in calculationExpression.Expressions )
					    {
						    if ( expressionPosition > 1 )
						    {
							    sbExpression.Append( " / " );
						    }

						    sbExpression.Append( expressionPosition > 1 ? CalculationAvoidSQLExceptionHandler( BracketNextLevelExpression( expression, sqlQuery ), CalculationOperator.Divide ) : BracketNextLevelExpression( expression, sqlQuery ) );
						    //if (expression is TreeTypeExpression)
						    //    sbExpression.Append(string.Format("({0})", ConvertExpression(expression, sqlQuery).Sql));

						    expressionPosition++;
					    }
					    break;

				    case CalculationOperator.Log:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "log(case when {0}<=0 then null else {0} end)", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
						    //support for sql 2012 only
					    else if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.Append( string.Format( "log(case when {0}<=0 then null else {0} end, {1})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to LOG" ); // TODO : add exceptions 
							return false;
					    }

					    break;

				    case CalculationOperator.Log10:
					    if ( calculationExpression.Expressions.Count > 0 )
					    {
						    sbExpression.Append( string.Format( "log10(case when {0}<=0 then null else {0} end)", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to LOG10" );
						    return false;
					    }
					    break;

				    case CalculationOperator.Multiply:
					    expressionPosition = 1;
					    foreach ( ScalarExpression expression in calculationExpression.Expressions )
					    {
						    if ( expressionPosition > 1 )
						    {
							    sbExpression.Append( " * " );
						    }

						    sbExpression.Append( BracketNextLevelExpression( expression, sqlQuery ) );
						    //if (expression is TreeTypeExpression)
						    //    sbExpression.Append(string.Format("({0})", ConvertExpression(expression, sqlQuery).Sql));

						    expressionPosition++;
					    }
					    break;

				    case CalculationOperator.Ceiling:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "ceiling({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to CEILING" );
						    return false;
					    }
					    break;

				    case CalculationOperator.Abs:

					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "abs({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function ABS" );
							return false;
					    }
					    break;

				    case CalculationOperator.Exp:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "exp({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Exp" );
							return false;
					    }
					    break;

				    case CalculationOperator.Floor:
					    if ( calculationExpression.Expressions.Count > 0 )
					    {
						    sbExpression.Append( string.Format( "floor({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Floor" );
							return false;
					    }
					    break;

				    case CalculationOperator.Power:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.Append( string.Format( "power({0}, {1})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Power" );
							return false;
					    }
					    break;

				    case CalculationOperator.Sign:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "sign({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Sign" );
							return false;
					    }
					    break;

				    case CalculationOperator.Sqrt:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "sqrt(case when {0} < 0 then null else {0} end)", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Sqrt" );
							return false;
					    }
					    break;

				    case CalculationOperator.Square:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "square({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Square" );
						    return false;
					    }
					    break;

				    case CalculationOperator.Round:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    // convert to address weird SQL issue where round(0.5,0) fails.
						    sbExpression.Append( string.Format( "round(try_convert(decimal(38,10),{0}), {1})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Round" );
							return false;
					    }
					    break;

					    // IsNull(value,default) SQL Function
				    case CalculationOperator.IsNull:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.AppendFormat( "isnull({0}, {1})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function IsNull" );
							return false;
					    }

					    break;

				    case CalculationOperator.Null:
					    sbExpression.Append( "null" );
					    break;

				    case CalculationOperator.Cast:
					    if ( calculationExpression.Expressions.Count == 1 || calculationExpression.Expressions.Count == 2 )
					    {
						    if ( calculationExpression.CastType != null && calculationExpression.CastType.GetDisplayName( ) != DatabaseType.UnknownType.GetDisplayName( ) )
						    {
							    bool convertFromUtcToLocal = false;
							    bool convertFromLocalToUtc = false;
							    string format = "try_convert({0}, {1})";
							    string targetType = calculationExpression.CastType.GetSqlDbTypeString( );
							    var innerExpr = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
							    string innerSql = innerExpr.Sql;

							    if ( calculationExpression.CastType is BoolType )
							    {
								    result.DisplaySqlCallback = ConvertBoolToResult;
								    if ( calculationExpression.InputType == DataType.String )
								    {
									    innerSql = string.Format( "case {0} when 'yes' then 'True' when 'no' then 'False' else {0} end", innerSql );
								    }
							    }
							    if ( calculationExpression.CastType is DateType )
							    {
								    format = "try_convert({0}, {1}, 126)"; // require ISO8601 format.
								    if ( calculationExpression.InputType == DataType.DateTime )
                                        convertFromUtcToLocal = true;
							    }
							    if ( calculationExpression.CastType is DateTimeType )
							    {
								    format = "try_convert({0}, {1}, 126)"; // require ISO8601 format.
								    if ( calculationExpression.InputType == DataType.Date )
                                        convertFromLocalToUtc = true;
								    if ( calculationExpression.InputType == DataType.String )
									    convertFromLocalToUtc = true;
							    }
							    if ( calculationExpression.CastType is TimeType )
							    {
								    if ( calculationExpression.InputType == DataType.String )
                                    {
									    format = "dateadd(DAY, -53690, try_convert(datetime, try_convert(time, try_convert({0}, {1}, 126))))"; // "-53690" this is the day offset for 17531-01-01
								    }
								    if ( calculationExpression.InputType == DataType.DateTime )
								    {
									    format = "dateadd(DAY, -53690, try_convert(datetime, try_convert(time, {1})))"; // "-53690" this is the day offset for 1753-01-01
									    convertFromUtcToLocal = true;
								    }
							    }
							    if ( calculationExpression.CastType is StringType )
							    {
								    if ( calculationExpression.InputType == DataType.Date )
                                    {
									    format = "try_convert({0}, {1}, 103)"; // dd/mm/yyyy
								    }
								    else if ( calculationExpression.InputType == DataType.DateTime )
                                    {
									    convertFromUtcToLocal = true;
									    format = "(try_convert({0}, {1}, 103) + ' ' + replace(replace(try_convert({0}, try_convert(time, {1}),0),'AM',' AM'),'PM',' PM'))"; // dd/mm/yyyy hh:miAM
								    }
								    if ( calculationExpression.InputType == DataType.Time )
                                    {
									    format = "replace(replace(try_convert({0}, try_convert(time, {1}),0),'AM',' AM'),'PM',' PM')"; // hh:miAM
								    }
							    }
							    if ( calculationExpression.Expressions.Count == 2 )
							    {
								    var precisionExpr = ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery );
								    targetType = targetType + "(" + precisionExpr.Sql + ")";
							    }

							    if ( convertFromUtcToLocal )
							    {
								    // Do conversions to local prior to cast
								    innerExpr.DatabaseType = DatabaseType.DateTimeType;
								    innerSql = ConvertExpressionToLocalTime( innerExpr ).Sql;
							    }

							    string castSql = string.Format( format,
								    targetType,
								    innerSql );

							    if ( convertFromLocalToUtc )
							    {
								    // Do conversions to UTC after cast
								    castSql = ConvertExpressionFromLocalTime( new SqlExpression( castSql )
								    {
									    DatabaseType = DatabaseType.DateTimeType
								    } ).Sql;
							    }

							    resultType = calculationExpression.CastType;
							    sbExpression.Append( castSql );
						    }
						    else
						    {
							    sbExpression.Append( ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql );
						    }
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Cast" );
						    return false;
					    }
					    break;

					    /*string functions*/
				    case CalculationOperator.Charindex:

					    if ( calculationExpression.Expressions.Count == 3 )
					    {
						    sbExpression.Append( string.Format( "try_convert(int,charindex({0}, {1}, {2}))", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 2 ], sqlQuery ).Sql ) );
					    }
					    else if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.Append( string.Format( "try_convert(int,charindex({0}, {1}))", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Charindex" );
							return false;
					    }
					    resultType = DatabaseType.Int32Type;
					    break;

				    case CalculationOperator.Concatenate:
				    case CalculationOperator.SmartOrderConcatenate:
					    //CONCAT ( 'Happy ', 'Birthday ', 11, '/', '25' ) -->Happy Birthday 11/25
					    if ( calculationExpression.Expressions.Count >= 2 )
					    {
						    sbExpression.Append( "(" );

						    // Pair enum so we can match the expressions and SqlExpressions to each other
						    var pairs = ( from expr in calculationExpression.Expressions
							    select new
							    {
								    Expr = expr,
								    SqlExpr = ConvertExpression( expr, sqlQuery )
							    } ).ToArray( );

						    var first = new First( );
						    foreach ( var pair in pairs )
						    {
							    if ( !first )
							    {
								    sbExpression.Append( " + " );
							    }

							    sbExpression.Append( string.Format( "isnull({0}, '')", pair.SqlExpr.DisplaySql ) );
						    }
						    sbExpression.Append( ")" );
						    resultType = DatabaseType.StringType;

						    if ( calculationExpression.Operator == CalculationOperator.SmartOrderConcatenate )
						    {
							    // Smart Concatenate generates multiple order-by clauses, one for each child expression. This allows natural ordering to be used.
							    result.OrderingSqlCallback = expr => string.Join( SqlOrderClause.OrderByDelimiter, pairs.Where( p => !( p.Expr is LiteralExpression ) ).Select( p => p.SqlExpr.OrderingSql ) );
							    result.OrderingSqlRequiresGrouping = true;
						    }
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Concatenate" );
							return false;
					    }
					    break;

				    case CalculationOperator.Left:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.Append( string.Format( "left({0}, {1})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql ) );
						    resultType = DatabaseType.StringType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Left" );
							return false;
					    }
					    break;

				    case CalculationOperator.Right:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.Append( string.Format( "right({0}, {1})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql ) );
						    resultType = DatabaseType.StringType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Right" );
							return false;
					    }
					    break;

				    case CalculationOperator.Replace:
					    if ( calculationExpression.Expressions.Count == 3 )
					    {
						    sbExpression.Append( string.Format( "replace({0}, {1}, {2})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 2 ], sqlQuery ).Sql ) );
						    resultType = DatabaseType.StringType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Replace" );
							return false;
					    }
					    break;

				    case CalculationOperator.StringLength:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    // Note: SQL returns a long by default for 'len'
						    sbExpression.Append( string.Format( "try_convert(int,len({0}))", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
						    resultType = DatabaseType.Int32Type;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function StringLength" );
						    return false;
					    }
					    break;

				    case CalculationOperator.Substring:
					    if ( calculationExpression.Expressions.Count == 3 )
					    {
						    sbExpression.Append( string.Format( "substring({0}, {1}, {2})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 2 ], sqlQuery ).Sql ) );
					    }
					    else if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.Append( string.Format( "substring({0}, {1}, 8000)", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql ) );
					    }
					    else if ( calculationExpression.Expressions.Count == 1 )
					    {
						    sbExpression.Append( string.Format( "substring({0}, 0, 8000)", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Substring" );
							return false;
					    }
					    resultType = DatabaseType.StringType;
					    break;

				    case CalculationOperator.ToLower:
					    if ( calculationExpression.Expressions.Count >= 1 )
					    {
						    sbExpression.Append( string.Format( "lower({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
						    resultType = DatabaseType.StringType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function ToLower" );
							return false;
					    }
					    break;

				    case CalculationOperator.ToUpper:
					    if ( calculationExpression.Expressions.Count >= 1 )
					    {
						    sbExpression.Append( string.Format( "upper({0})", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql ) );
						    resultType = DatabaseType.StringType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function TpUpper" );
							return false;
					    }
					    break;

				    case CalculationOperator.Year:
				    case CalculationOperator.Month:
				    case CalculationOperator.Day:
				    case CalculationOperator.Hour:
				    case CalculationOperator.Minute:
				    case CalculationOperator.Second:
				    case CalculationOperator.Quarter:
				    case CalculationOperator.DayOfYear:
				    case CalculationOperator.Week:
				    case CalculationOperator.WeekDay:
					    ConvertDateCalculationExpression( calculationExpression, sqlQuery, sbExpression );
					    resultType = DatabaseType.Int32Type;
					    break;

				    case CalculationOperator.Modulo:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    sbExpression.Append( string.Format( "{0} % {1}", ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery ).Sql, CalculationAvoidSQLExceptionHandler( ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery ).Sql, CalculationOperator.Modulo ) ) );
						    resultType = DatabaseType.Int32Type;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function Modulo" );
							return false;
					    }
					    break;

				    case CalculationOperator.TodayDateTime:
					    MarkSqlAsUncacheable( "Relies on TodayDateTime" );
					    var utcNow = DateTime.UtcNow;
					    sbExpression.AppendFormat( "datetimefromparts({0},{1},{2},{3},{4},{5},0)", utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second );
					    resultType = DatabaseType.DateTimeType;
					    break;

				    case CalculationOperator.TodayDate:
					    MarkSqlAsUncacheable( "Relies on TodayDate" );
					    DateTime localDate = TimeZoneHelper.GetLocalTime( _structuredQuery.TimeZoneName ).Date;
					    sbExpression.AppendFormat( "datetimefromparts({0},{1},{2},0,0,0,0)", localDate.Year, localDate.Month, localDate.Day );
					    resultType = DatabaseType.DateType;
					    break;

				    case CalculationOperator.Time:
					    MarkSqlAsUncacheable( "Relies on Time" );
					    DateTime localTime = TimeZoneHelper.GetLocalTime( _structuredQuery.TimeZoneName );
					    sbExpression.AppendFormat( "datetimefromparts(1753,1,1,{0},{1},{2},0)", localTime.Hour, localTime.Minute, localTime.Second );
					    resultType = DatabaseType.TimeType;
					    break;

				    case CalculationOperator.DateDescription:
					    SqlExpression sqlExpr = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
					    sbExpression.Append( GetDateDescriptionExpression( sqlExpr, calculationExpression.Expressions[ 0 ] ) );
					    resultType = DatabaseType.StringType;
					    break;

				    case CalculationOperator.DateAdd:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    // Date expression
						    SqlExpression arg0 = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
						    // Offset number
						    SqlExpression arg1 = ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery );

						    string format;
						    if ( calculationExpression.InputType == DataType.Time )
						    {
							    // Handle wrapping
							    format = "dateadd(DAY, -53690, try_convert(datetime, dateadd({0}, {1}, try_convert(time,{2}))))"; // "-53690" this is the day offset for 1753-01-01                                    
						    }
						    else if ( calculationExpression.InputType == DataType.DateTime )
                            {
							    // For date-time, we need to convert to local time, then do the date-add, then convert back again, in order to get a correct result.
							    arg0.DatabaseType = DatabaseType.DateTimeType;
							    arg0 = ConvertExpressionToLocalTime( arg0 );
							    format = "dateadd({0}, {1}, {2})";
						    }
						    else
						    {
							    format = "dateadd({0}, {1}, {2})";
						    }

						    if ( calculationExpression.DateTimePart != DateTimeParts.None )
						    {
							    string expr = string.Format( format,
								    calculationExpression.DateTimePart,
								    arg1.Sql, arg0.Sql );

							    if ( calculationExpression.InputType == DataType.DateTime )
                                {
								    expr = ConvertExpressionFromLocalTime( new SqlExpression( expr )
								    {
									    DatabaseType = DatabaseType.DateTimeType
								    } ).Sql;
							    }
							    sbExpression.Append( expr );

							    resultType = arg0.DatabaseType ?? DatabaseType.DateTimeType; //hmm
						    }
						    else
						    {
								convertResult.SetFailure( ConvertResult.FailureReason.Exception, "DateTimePart was not specified for the DateAdd function." );
								return false;
						    }
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function DateAdd." );
							return false;
					    }
					    break;

				    case CalculationOperator.DateDiff:
					    if ( calculationExpression.Expressions.Count == 2 )
					    {
						    // Start date
						    SqlExpression arg0 = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
						    // End date
						    SqlExpression arg1 = ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery );

						    bool isTime = ( arg0.DatabaseType is TimeType || arg1.DatabaseType is TimeType );

						    if ( calculationExpression.DateTimePart != DateTimeParts.None )
						    {
							    string formatString = "datediff({0}, {1}, {2})";
							    if ( isTime )
							    {
								    formatString = "datediff({0}, try_convert(nvarchar(8), {1}, 108), try_convert(nvarchar(8), {2}, 108))";
							    }

							    string expr = string.Format( formatString,
								    calculationExpression.DateTimePart,
								    arg0.Sql, arg1.Sql );
							    sbExpression.Append( expr );
							    resultType = DatabaseType.Int32Type;
						    }
						    else
						    {
								convertResult.SetFailure( ConvertResult.FailureReason.Exception, "DateTimePart was not specified for the DateDiff function." );
								return false;
						    }
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function DateDiff." );
							return false;
					    }
					    break;

				    case CalculationOperator.DateName:
					    if ( calculationExpression.Expressions.Count == 1 )
					    {
						    // Date expression
						    SqlExpression arg = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
						    SqlExpression adjusted = ConvertExpressionToLocalTime( arg );

						    if ( calculationExpression.DateTimePart != DateTimeParts.None )
						    {
							    string expr = string.Format( "datename({0}, {1})",
								    calculationExpression.DateTimePart,
								    adjusted.Sql );
							    sbExpression.Append( expr );
							    //resultType = arg.DatabaseType ?? DatabaseType.DateTimeType; //hmm

							    result.OrderingSqlRequiresGrouping = true;
							    result.OrderingSqlCallback = expr1 => string.Format( "datepart({0}, {1})",
								    calculationExpression.DateTimePart,
								    adjusted.Sql );

							    resultType = DatabaseType.StringType;
						    }
						    else
						    {
								convertResult.SetFailure( ConvertResult.FailureReason.Exception, "DateTimePart was not specified for the DateName function." );
								return false;
						    }
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function DateName." );
						    return false;
					    }
					    break;

				    case CalculationOperator.DateFromParts:
					    if ( calculationExpression.Expressions.Count == 3 )
					    {
						    // Arguments
						    SqlExpression year = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
						    SqlExpression month = ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery );
						    SqlExpression day = ConvertExpression( calculationExpression.Expressions[ 2 ], sqlQuery );

						    string expr = string.Format( "datefromparts({0}, {1}, {2})", year.Sql, month.Sql, day.Sql );
						    sbExpression.Append( expr );
						    resultType = DatabaseType.DateType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function DateDiff." );
							return false;
					    }
					    break;

				    case CalculationOperator.TimeFromParts:
					    if ( calculationExpression.Expressions.Count == 3 )
					    {
						    // Arguments
						    SqlExpression hour = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
						    SqlExpression minute = ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery );
						    SqlExpression second = ConvertExpression( calculationExpression.Expressions[ 2 ], sqlQuery );

						    string expr = string.Format( "datetimefromparts(1753, 1, 1, {0}, {1}, {2}, 0)", hour.Sql, minute.Sql, second.Sql );
						    sbExpression.Append( expr );
						    resultType = DatabaseType.TimeType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function DateDiff." );
							return false;
					    }
					    break;

				    case CalculationOperator.DateTimeFromParts:
					    if ( calculationExpression.Expressions.Count == 6 )
					    {
						    // Arguments
						    SqlExpression year = ConvertExpression( calculationExpression.Expressions[ 0 ], sqlQuery );
						    SqlExpression month = ConvertExpression( calculationExpression.Expressions[ 1 ], sqlQuery );
						    SqlExpression day = ConvertExpression( calculationExpression.Expressions[ 2 ], sqlQuery );
						    SqlExpression hour = ConvertExpression( calculationExpression.Expressions[ 3 ], sqlQuery );
						    SqlExpression minute = ConvertExpression( calculationExpression.Expressions[ 4 ], sqlQuery );
						    SqlExpression second = ConvertExpression( calculationExpression.Expressions[ 5 ], sqlQuery );

						    string expr = string.Format( "datetimefromparts({0}, {1}, {2}, {3}, {4}, {5}, 0)", year.Sql, month.Sql, day.Sql, hour.Sql, minute.Sql, second.Sql );
						    SqlExpression adjusted = ConvertExpressionFromLocalTime( new SqlExpression( expr )
						    {
							    DatabaseType = DatabaseType.DateTimeType
						    } );
						    sbExpression.Append( adjusted.Sql );
						    resultType = DatabaseType.DateTimeType;
					    }
					    else
					    {
							convertResult.SetFailure( ConvertResult.FailureReason.Exception, "Incorrect number of arguments passed to SQL function DateDiff." );
							return false;
					    }
					    break;
			    }

			    result.Sql = sbExpression.ToString( );
			    result.DatabaseType = resultType;

			    convertResult.Expression = result;
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
			    return false;
		    }
        }

        private void ConvertDateCalculationExpression(CalculationExpression calculationExpression, SqlQuery sqlQuery, StringBuilder sbExpression)
        {
            if (calculationExpression.Expressions.Count == 1)
            {
                SqlExpression arg = ConvertExpression(calculationExpression.Expressions[0], sqlQuery);
                SqlExpression adjusted = ConvertExpressionToLocalTime(arg);
                sbExpression.Append(string.Format("datepart({0}, {1})", calculationExpression.Operator.ToString().ToLowerInvariant(), adjusted.Sql));
            }
            else
			{
                throw new Exception("Incorrect number of arguments passed to SQL function " + calculationExpression.ToString());
			}                
        }

        /// <summary>
        ///     Clusters values together (e.g. dates by month) by converting them to a canonical form. (e.g. first of the month)
        /// </summary>
        /// <param name="scalarExpression">The cluster expression.</param>
        /// <param name="sqlExpression">The cluster expression.</param>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <returns></returns>
        private SqlExpression ApplyClusterOperation(SqlExpression sqlExpression, ClusterOperation operation, SqlQuery sqlQuery)
        {
            if (operation == ClusterOperation.None)
                return sqlExpression;

            // Adjust time zone
            SqlExpression adjusted = ConvertExpressionToLocalTime(sqlExpression);

            // Build a string pattern
            string pattern;

            if (operation.HasFlag(ClusterOperation.Weekday))
            {
                pattern = "datetimefromparts(1900, 4, datepart(weekday, {0}), 0, 0, 0, 0)"; // Sun=1, Sat=7, and the 1900/4/1 is a Sunday
            }
            else
            {
                pattern = string.Format("datetimefromparts({0}, {1}, {2}, {3}, 0, 0, 0)",
                // in principle any year will do. In practice some years causes a mismatch between db local-to-utc vs browser local-to-utc. eg. 1970 is bad. Also, if too old, then the convert to UTC will fail if used in conjunction with hour
                operation.HasFlag(ClusterOperation.Year) ? "datepart(yyyy, {0})" : "2000",
                operation.HasFlag(ClusterOperation.Month) ? "datepart(m, {0})" : (operation.HasFlag(ClusterOperation.Quarter) ? "datepart(q, {0})*3-2" : "1"),
                operation.HasFlag(ClusterOperation.Day) ? "datepart(d, {0})" : "2",     // use the 2nd of the month to mitigate risk of daylight savings issues at year/quarter/month boundary
                operation.HasFlag(ClusterOperation.Hour) ? "datepart(hh, {0})" : "0");  // the hour value should be 0 when the format without hour flag. the format datetime value group will display duplicate group header.
            }

            SqlExpression clustered = new SqlExpression(adjusted.Sql);
            clustered.DisplaySqlCallback = expr => ConvertClusterPatternToSql(expr, pattern);
            clustered.ResultSqlCallback = expr => ConvertClusterPatternToSql(expr, pattern);
            clustered.OrderingSqlCallback = expr => ConvertClusterPatternToSql(expr, pattern);
            clustered.DatabaseType = sqlExpression.DatabaseType;

            // Adjust the timezone back to UTC
            // Note that clustering must be done in local time, but we still need to ensure that the format is preserved,
            // which for date-time means UTC.
            SqlExpression result = ConvertExpressionFromLocalTime(clustered);
            result.Constant = sqlExpression.Constant;
            //the datetime format is preserved on local time not UTC time  
            //e.g.  datetimefromparts(2000, datepart(m, dbo.fnConvertToLocalTime(d2.Data, ''AUS Eastern Standard Time'')), 2, 0, 0, 0, 0)
            //otherwise the final sql results will be incorrect.
            //e.g. datetimefromparts(2000, datepart(m, dbo.fnConvertToUtc(dbo.fnConvertToLocalTime(d2.Data, ''AUS Eastern Standard Time''), ''AUS Eastern Standard Time'')), 2, 0, 0, 0, 0)
            result.DisplaySqlCallback = expr => ConvertClusterPatternToSql(clustered, pattern);
            result.ResultSqlCallback = expr => ConvertClusterPatternToSql(clustered, pattern);
            result.OrderingSqlCallback = expr => ConvertClusterPatternToSql(clustered, pattern);
            return result;
        }

        /// <summary>
        /// Convert the Cluster Pattern to SQL (resultsql, displaysql or orderSql)
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private string ConvertClusterPatternToSql(SqlExpression expr, string pattern)
        {
            return string.Format(pattern, expr.Sql);
        }

		/// <summary>
		///		Converts the column reference expression.
		/// </summary>
		/// <param name="columnReference">The column reference.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertColumnReferenceExpression( ColumnReference columnReference, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertColumnReferenceExpressionImpl( columnReference, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the column reference expression.
		/// </summary>
		/// <param name="columnReference">The column reference.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertColumnReferenceExpression( ColumnReference columnReference, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertColumnReferenceExpressionImpl( columnReference, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Converts the column reference expression into a SqlExpression.
		/// </summary>
		/// <param name="columnReference">The column reference.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Column reference could not be resolved.</exception>
		private bool TryConvertColumnReferenceExpressionImpl( ColumnReference columnReference, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    SqlExpression result = sqlQuery.References.FindSelectColumn( columnReference.ColumnId );
			    if ( result == null )
			    {
				    throw new Exception( string.Format( "Column reference '{0}' (entity {1}) could not be resolved.",
					    columnReference.ColumnId, columnReference.EntityId ) );
			    }

			    convertResult.Expression = result;
			    return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
			    return false;
		    }
        }

		/// <summary>
		///		Converts the comparison expression.
		/// </summary>
		/// <param name="comparisonExpression">The comparison expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertComparisonExpression( ComparisonExpression comparisonExpression, SqlQuery sqlQuery )
	    {
			var convertResult = new ConvertResult( );

		    if ( TryConvertComparisonExpressionImpl( comparisonExpression, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		/// Tries to convert the comparison expression.
		/// </summary>
		/// <param name="comparisonExpression">The comparison expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertComparisonExpression( ComparisonExpression comparisonExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertComparisonExpressionImpl( comparisonExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Converts the comparison expression.
		/// </summary>
		/// <param name="comparisonExpression">The comparison expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		private bool TryConvertComparisonExpressionImpl( ComparisonExpression comparisonExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    string sql;

			    if ( comparisonExpression != null && comparisonExpression.Operator == ComparisonOperator.Equal )
			    {
				    ScalarExpression leftExpr = comparisonExpression.Expressions[ 0 ];
				    ScalarExpression rightExpr = comparisonExpression.Expressions[ 1 ];
				    SqlExpression left = BracketExpression( leftExpr, sqlQuery );
				    SqlExpression right = BracketExpression( rightExpr, sqlQuery );

				    sql = ConvertEqualityTest( leftExpr, left.DatabaseType, left.Sql, right.Sql );
			    }
			    else if ( comparisonExpression != null && comparisonExpression.Expressions.Count == 2 )
			    {
				    string format;

				    switch ( comparisonExpression.Operator )
				    {
					    case ComparisonOperator.GreaterThan:
						    format = "{0} > {1}";
						    break;
					    case ComparisonOperator.GreaterThanEqual:
						    format = "{0} >= {1}";
						    break;
					    case ComparisonOperator.LessThan:
						    format = "{0} < {1}";
						    break;
					    case ComparisonOperator.LessThanEqual:
						    format = "{0} <= {1}";
						    break;
					    case ComparisonOperator.NotEqual:
						    format = "{0} <> {1}";
						    break;
					    case ComparisonOperator.Like:
						    format = "{0} like {1}";
						    break;
					    case ComparisonOperator.NotLike:
						    format = "{0} not like {1}";
						    break;
					    default:
						    convertResult.SetFailure(ConvertResult.FailureReason.InvalidOperation, comparisonExpression.Operator.ToString( ) );
							return false;
				    }

				    string left = BracketNextLevelExpression( comparisonExpression.Expressions[ 0 ], sqlQuery );
				    string right = BracketNextLevelExpression( comparisonExpression.Expressions[ 1 ], sqlQuery );
				    sql = string.Format( format, left, right );

			    }
			    else if ( comparisonExpression != null && comparisonExpression.Expressions.Count == 1 )
			    {
				    string arg = ConvertExpression( comparisonExpression.Expressions[ 0 ], sqlQuery ).Sql;
				    switch ( comparisonExpression.Operator )
				    {
					    case ComparisonOperator.IsNull:
						    sql = string.Format( "{0} is null ", arg );
						    break;
					    case ComparisonOperator.IsNotNull:
						    sql = string.Format( "{0} is not null ", arg );
						    break;
					    default:
							convertResult.SetFailure( ConvertResult.FailureReason.InvalidOperation, comparisonExpression.Operator.ToString( ) );
							return false;
				    }
			    }
			    else
			    {
					convertResult.SetFailure( ConvertResult.FailureReason.InvalidOperation );
					return false;
			    }

			    convertResult.Expression = new SqlExpression
			    {
				    BoolSql = sql,
				    DisplaySqlCallback = ConvertBoolToResult
			    };
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
        }

        private string ConvertBoolToResult(SqlExpression expr)
        {
            return string.Format("try_convert(bit, case when {0} then 1 else 0 end)", expr.BoolSql);
        }


        /// <summary>
        ///     Converts a requested structured query expression into a SqlExpression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="isUnderAggregateExpression">current resource expression is under another aggregate expression or not. if yes, the result sql call back without xml output</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        private SqlExpression ConvertExpression(ScalarExpression expression, SqlQuery sqlQuery, bool isUnderAggregateExpression = false)
        {
	        var result = new ConvertResult( );

	        if ( TryConvertExpressionImpl( expression, sqlQuery, isUnderAggregateExpression, ref result ) )
	        {
		        return result.Expression;
	        }

			throw result.GetConvertException();
        }

		/// <summary>
		///		Tries to convert the expression.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="isUnderAggregateExpression">if set to <c>true</c> [is under aggregate expression].</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
		private bool TryConvertExpression( ScalarExpression expression, SqlQuery sqlQuery, bool isUnderAggregateExpression, out SqlExpression sqlExpression )
		{
			var result = new ConvertResult( );

	        if ( TryConvertExpressionImpl( expression, sqlQuery, isUnderAggregateExpression, ref result ) )
	        {
		        sqlExpression = result.Expression;
		        return true;
	        }

			sqlExpression = null;
			return false;
		}

		/// <summary>
		/// Tries the convert expression implementation.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="isUnderAggregateExpression">if set to <c>true</c> [is under aggregate expression].</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
	    private bool TryConvertExpressionImpl( ScalarExpression expression, SqlQuery sqlQuery, bool isUnderAggregateExpression, ref ConvertResult convertResult )
	    {
		    if ( TryConvertTypedExpressionImpl( expression, sqlQuery, isUnderAggregateExpression, ref convertResult ) )
		    {
			    if ( _querySettings.CaptureExpressionMetadata )
			    {
					var exprMetadata = CreateResultColumn( _structuredQuery, expression, convertResult.Expression );
				    _queryResult.ExpressionTypes[ expression ] = exprMetadata;
			    }

			    // For rollups. Do cluster on all expressions immediately so they can be matched up on the client side.
			    if ( expression.ClusterOperation != ClusterOperation.None && _querySettings.FullAggregateClustering )
			    {
					convertResult.Expression = ApplyClusterOperation( convertResult.Expression, expression.ClusterOperation, sqlQuery );
			    }

			    return true;
		    }

			return false;
	    }

		/// <summary>
		///		Converts a requested structured query expression into a SqlExpression.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="isUnderAggregateExpression">current resource expression is under another aggregate expression or not. if yes, the result sql call back without xml output</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		private bool TryConvertTypedExpressionImpl( ScalarExpression expression, SqlQuery sqlQuery, bool isUnderAggregateExpression, ref ConvertResult convertResult )
        {
			// Check if this expression is mapped to a sub query
			SqlExpression mapped = sqlQuery.References.GetMappedExpression( expression );
			if ( mapped != null )
			{
				convertResult.Expression = mapped;
				return true;
			}

	        try
	        {
		        // Evaluate expression types
		        var resourceExpression = expression as ResourceExpression;

		        if ( resourceExpression != null )
		        {
			        // Needs to be described before ResourceDataColumn
			        return TryConvertResourceExpressionImpl( resourceExpression, sqlQuery, isUnderAggregateExpression, ref convertResult );
		        }

		        var column = expression as ResourceDataColumn;

		        if ( column != null )
		        {
			        return TryConvertResourceDataColumnExpressionImpl( column, sqlQuery, ref convertResult );
		        }

		        var idExpression = expression as IdExpression;

		        if ( idExpression != null )
		        {
			        return TryConvertIdExpressionImpl( idExpression, sqlQuery, ref convertResult );
		        }

		        var viewExpression = expression as StructureViewExpression;

		        if ( viewExpression != null )
		        {
			        return TryConvertStructureViewExpressionImpl( viewExpression, sqlQuery, ref convertResult );
		        }

		        var aggExpression = expression as AggregateExpression;

		        if ( aggExpression != null )
		        {
			        return TryConvertAggregateExpressionImpl( aggExpression, sqlQuery, ref convertResult );
		        }

		        var reference = expression as ColumnReference;

		        if ( reference != null )
		        {
			        return TryConvertColumnReferenceExpressionImpl( reference, sqlQuery, ref convertResult );
		        }

		        var scriptExpression = expression as ScriptExpression;

		        if ( scriptExpression != null )
		        {
			        return TryConvertScriptExpressionImpl( scriptExpression, sqlQuery, ref convertResult );
		        }

		        if ( expression is EntityExpression )
		        {
			        // Note: entity expression has a valid purpose in the logical query, but not in the physical SQL. Use with care.
			        return true;
		        }

		        var calculationExpression = expression as CalculationExpression;

		        if ( calculationExpression != null )
		        {
			        return TryConvertCalculationExpressionImpl( calculationExpression, sqlQuery, ref convertResult );
		        }

		        var elseExpression = expression as IfElseExpression;

		        if ( elseExpression != null )
		        {
			        return TryConvertIfElseExpressionImpl( elseExpression, sqlQuery, ref convertResult );
		        }

		        var comparisonExpression = expression as ComparisonExpression;

		        if ( comparisonExpression != null )
		        {
			        return TryConvertComparisonExpressionImpl( comparisonExpression, sqlQuery, ref convertResult );
		        }

		        var logicalExpression = expression as LogicalExpression;

		        if ( logicalExpression != null )
		        {
			        return TryConvertLogicalExpressionImpl( logicalExpression, sqlQuery, ref convertResult );
		        }

		        var literalExpression = expression as LiteralExpression;

		        if ( literalExpression != null )
		        {
			        return TryConvertLiteralExpressionImpl( literalExpression, ref convertResult );
		        }

		        var mutateExpression = expression as MutateExpression;

		        if ( mutateExpression != null )
		        {
			        return TryConvertMutateExpressionImpl( mutateExpression, sqlQuery, ref convertResult );
		        }
	        }
	        catch ( PlatformSecurityException exc )
	        {
				/////
				// Pass security exception on.
				/////
		        convertResult.SetFailure( ConvertResult.FailureReason.Security, null, exc );
		        return false;
	        }
	        catch ( Exception exc )
	        {
		        if ( expression == null )
		        {
					convertResult.SetFailure( ConvertResult.FailureReason.Query, "Expression was null.", exc );
			        return false;
		        }

		        string message = string.Format( "Failed to convert expression of type '{0}' with id '{1}' targeting EntityId '{2}'.", expression.GetType( ).Name, expression.ExpressionId.ToString( "B" ), expression.EntityId );

				convertResult.SetFailure( ConvertResult.FailureReason.Query, message, exc );
		        return false;
	        }

			convertResult.SetFailure( ConvertResult.FailureReason.InvalidOperation, expression.GetType( ).Name );
			return false;
		}

		/// <summary>
		///     Adjusts a date-time expression to convert it to client local-time.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns></returns>
		private SqlExpression ConvertExpressionToLocalTime( SqlExpression expression )
		{
			bool requiresOffset = expression.DatabaseType is DateTimeType;

			if ( !requiresOffset )
			{
				return expression;
			}

            string sql = string.Format("dbo.fnConvertToLocalTime({0}, '{1}')", expression.Sql, TimeZoneHelper.GetMsTimeZoneName(_structuredQuery.TimeZoneName));
			return new SqlExpression
				{
					Sql = sql,
                    DatabaseType = expression.DatabaseType
				};
        }

        /// <summary>
        ///     Adjusts a local date-time expression to convert it to UTC time.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private SqlExpression ConvertExpressionFromLocalTime(SqlExpression expression)
        {
            bool requiresOffset = expression.DatabaseType is DateTimeType;

            if (!requiresOffset)
            {
                return expression;
            }

            string sql = string.Format("dbo.fnConvertToUtc({0}, '{1}')", expression.Sql,  TimeZoneHelper.GetMsTimeZoneName(_structuredQuery.TimeZoneName));
            return new SqlExpression
            {
                Sql = sql,
                DatabaseType = expression.DatabaseType
            };
        }

		/// <summary>
		///		Converts the identifier expression.
		/// </summary>
		/// <param name="idExpression">The identifier expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertIdExpression( IdExpression idExpression, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertIdExpressionImpl( idExpression, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
			}

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Converts the identifier expression.
		/// </summary>
		/// <param name="idExpression">The identifier expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool ConvertIdExpression( IdExpression idExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertIdExpressionImpl( idExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }


		/// <summary>
		///		Converts a structure view expression request into a SqlExpression.
		/// </summary>
		/// <param name="idExpression">The id expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		private bool TryConvertIdExpressionImpl( IdExpression idExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
		{
		    try
		    {
			    // Find the 'Resource' table for the node this expression is pointing to
			    SqlTable parentTable = sqlQuery.References.FindSqlTable( idExpression.NodeId );

			    if ( parentTable.JoinHint == JoinHint.Unspecified )
			    {
				    parentTable.JoinHint = JoinHint.Optional;
			    }

			    string sql = GetColumnSql( parentTable, parentTable.IdColumn );

			    ResourceEntity resourceEntity = sqlQuery.References.FindEntity<ResourceEntity>( idExpression.NodeId );

			    var expression = new SqlExpression
			    {
				    Sql = sql,
				    ResourceTypeId = resourceEntity.EntityTypeId.Id
			    };

			    convertResult.Expression = expression;
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
			    return false;
		    }
		}

		/// <summary>
		///		Converts if else expression.
		/// </summary>
		/// <param name="ifElseExpr">If else expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertIfElseExpression( IfElseExpression ifElseExpr, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertIfElseExpressionImpl( ifElseExpr, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the if else expression.
		/// </summary>
		/// <param name="ifElseExpr">If else expr.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertIfElseExpression( IfElseExpression ifElseExpr, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertIfElseExpressionImpl( ifElseExpr, sqlQuery, ref convertResult );

			sqlExpression = convertResult.Expression;
		    return result;
	    }


		/// <summary>
		///		Converts if else expression.
		/// </summary>
		/// <param name="ifElseExpr">If else expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		private bool TryConvertIfElseExpressionImpl( IfElseExpression ifElseExpr, SqlQuery sqlQuery, ref ConvertResult convertResult )
		{
		    try
		    {
			    // Note: SQL server only supports nesting of cases or iifs to a depth of 10.
			    // So look for any else-if expression structures and recursively collapse them into a single case statement.

			    var expr = ConvertIfElseNestableExpression( ifElseExpr, sqlQuery );
			    convertResult.Expression = new SqlExpression
			    {
				    Sql = string.Concat( " case ", expr.Sql, " end " )
			    };
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
		}

        /// <summary>
        ///     Converts if else expression.
        /// </summary>
        /// <param name="expr">The if else expression.</param>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <returns></returns>
        private SqlExpression ConvertIfElseNestableExpression(IfElseExpression expr, SqlQuery sqlQuery)
        {
            var condition = ConvertExpression( expr.BooleanExpression, sqlQuery );
            var ifExpr = ConvertExpression( expr.IfBlockExpression, sqlQuery );
            string tail;

            var elseBlock = expr.ElseBlockExpression as IfElseExpression;
            if (elseBlock != null)
            {
                var elseExpr = ConvertIfElseNestableExpression(elseBlock, sqlQuery);
                tail = elseExpr.Sql;
            }
            else
            {
                var elseExpr = ConvertExpression(expr.ElseBlockExpression, sqlQuery);
                tail = "else " + elseExpr.Sql;
            }

            return new SqlExpression
				{
                    Sql = string.Format("when {0} then {1} {2}", condition.BoolSql, ifExpr.Sql, tail)
				};
        }

		/// <summary>
		///		Converts the literal expression.
		/// </summary>
		/// <param name="literalExpression">The literal expression.</param>
		/// <returns></returns>
	    private SqlExpression ConvertLiteralExpression( LiteralExpression literalExpression )
	    {
			var convertResult = new ConvertResult( );

			if ( TryConvertLiteralExpressionImpl( literalExpression, ref convertResult ) )
			{
				return convertResult.Expression;
			}

			throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the literal expression.
		/// </summary>
		/// <param name="literalExpression">The literal expression.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertLiteralExpression( LiteralExpression literalExpression, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertLiteralExpressionImpl( literalExpression, ref convertResult );

			sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Converts a literal (some constant value) to a SQL expression.
		/// </summary>
		/// <param name="literalExpression">The literal expression.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		private bool TryConvertLiteralExpressionImpl( LiteralExpression literalExpression, ref ConvertResult convertResult )
        {
		    try
		    {
			    SqlExpression result;
			    if ( literalExpression.Value.Type is BoolType )
			    {
				    bool isTrue = ( bool? ) literalExpression.Value.Value == true;
				    string sql = isTrue ? "convert(bit, 1)" : "convert(bit, 0)";
				    string order = isTrue ? "1" : "2";
				    result = new SqlExpression( sql )
				    {
					    OrderingSqlCallback = expr => order
				    };
			    }
			    else if ( literalExpression.Value.Type is DateTimeType )
			    {
				    // slight hack.. the literal expression contains a local value.. but we need UTC for the query.
				    DateTime dtLocal = ( DateTime ) literalExpression.Value.Value;
				    DateTime dtUtc = TimeZoneHelper.ConvertToUtc( dtLocal, _structuredQuery.TimeZoneName );
				    var adjusted = new TypedValue
				    {
					    Type = DatabaseType.DateTimeType,
					    Value = dtUtc
				    };
				    result = ConvertTypedValueToLiteral( adjusted );
			    }
			    else
			    {
				    result = ConvertTypedValueToLiteral( literalExpression.Value );
			    }

			    convertResult.Expression = result;
			    return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
			    return false;
		    }
        }

		/// <summary>
		///		Converts the mutate expression.
		/// </summary>
		/// <param name="mutateExpression">The mutate expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertMutateExpression( MutateExpression mutateExpression, SqlQuery sqlQuery )
	    {
			var convertResult = new ConvertResult( );

			if ( TryConvertMutateExpressionImpl( mutateExpression, sqlQuery, ref convertResult ) )
			{
				return convertResult.Expression;
			}

			throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the mutate expression.
		/// </summary>
		/// <param name="mutateExpression">The mutate expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertMutateExpression( MutateExpression mutateExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertMutateExpressionImpl( mutateExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Mutates an expression by calling one of its purpose specific callbacks.
		/// </summary>
		/// <param name="mutateExpression">The mutate expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
        private bool TryConvertMutateExpressionImpl( MutateExpression mutateExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    SqlExpression inner = ConvertExpression( mutateExpression.Expression, sqlQuery );

			    SqlExpression result = inner;

			    switch ( mutateExpression.MutateType )
			    {
				    case MutateType.DisplaySql:
					    result = new SqlExpression( inner.DisplaySql );
					    result.DatabaseType = DatabaseType.StringType;
					    break;
				    case MutateType.BoolSql:
					    result = new SqlExpression( inner.BoolSql );
					    result.DatabaseType = DatabaseType.BoolType;
					    break;
			    }

			    convertResult.Expression = result;
			    return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
        }

		/// <summary>
		///		Converts the logical expression.
		/// </summary>
		/// <param name="logicalExpression">The logical expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertLogicalExpression( LogicalExpression logicalExpression, SqlQuery sqlQuery )
	    {
			var convertResult = new ConvertResult( );

			if ( TryConvertLogicalExpressionImpl( logicalExpression, sqlQuery, ref convertResult ) )
			{
				return convertResult.Expression;
			}

			throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the logical expression.
		/// </summary>
		/// <param name="logicalExpression">The logical expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertLogicalExpression( LogicalExpression logicalExpression, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertLogicalExpressionImpl( logicalExpression, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Converts the logical expression.
		/// </summary>
		/// <param name="logicalExpression">The logical expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		private bool TryConvertLogicalExpressionImpl( LogicalExpression logicalExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    var sb = new StringBuilder( );
			    sb.Append( "( " );

				if ( logicalExpression != null )
			    {
				    var first = new First( );
				    foreach ( var expr in logicalExpression.Expressions )
				    {
					    if ( !first || logicalExpression.Operator == LogicalOperator.Not )
					    {
                            sb.Append( " " );
                            sb.Append( logicalExpression.Operator.ToString().ToLowerInvariant() );
						    sb.Append( " " );
					    }

					    SqlExpression sqlExpr = ConvertExpression( expr, sqlQuery );
					    sb.Append( "(" );
					    sb.Append( sqlExpr.BoolSql );
					    sb.Append( ")" );
				    }
			    }
				sb.Append( " )" );

				convertResult.Expression = new SqlExpression
			    {
				    BoolSql = sb.ToString( ),
				    DisplaySqlCallback = ConvertBoolToResult
			    };
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
			    return false;
		    }
        }

		/// <summary>
		///		Converts the resource data column expression.
		/// </summary>
		/// <param name="resourceDataColumn">The resource data column.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertResourceDataColumnExpression( ResourceDataColumn resourceDataColumn, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertResourceDataColumnExpressionImpl( resourceDataColumn, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the resource data column expression.
		/// </summary>
		/// <param name="resourceDataColumn">The resource data column.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertResourceDataColumnExpression( ResourceDataColumn resourceDataColumn, SqlQuery sqlQuery, out SqlExpression sqlExpression )
	    {
            var convertResult = new ConvertResult( );

		    bool result = TryConvertResourceDataColumnExpressionImpl( resourceDataColumn, sqlQuery, ref convertResult );

		    sqlExpression = convertResult.Expression;
		    return result;
	    }

	    /// <summary>
		///		Converts a resource data column expression request into a SqlExpression.
		/// </summary>
		/// <param name="resourceDataColumn">The resource data column.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		private bool TryConvertResourceDataColumnExpressionImpl( ResourceDataColumn resourceDataColumn, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
            try
            {
                // Calculated fields
                if (resourceDataColumn.ScriptExpression != null)
                {
                    SqlExpression scriptResult;
                    if (TryConvertScriptExpression(resourceDataColumn.ScriptExpression, sqlQuery, out scriptResult))
                    { 
                        convertResult.Expression = scriptResult;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                // Check if this expression has already been referenced, and if so then reuse it
                // Note: this means tables don't get re-registered.
                SqlExpression result = sqlQuery.References.FindExpression( resourceDataColumn.NodeId, resourceDataColumn.FieldId );
				if ( result != null )
				{
					convertResult.Expression = result;
					return true;
				}

				var field = Model.Entity.Get<Field>( resourceDataColumn.FieldId.Id );
                if ( field == null )
                    throw new Exception( string.Format("Field '{0}' could not be loaded.", resourceDataColumn.FieldId.Id) );
				FieldType fieldType = field.GetFieldType( );
				string fieldTable = fieldType.DbFieldTable;

				// Get the parent table we're joining to
				SqlTable parentTable = sqlQuery.References.FindSqlTable( resourceDataColumn.NodeId );

				// If the join hint was not specified, assume now that it is 'optional', as it is being used.
				if ( parentTable.JoinHint == JoinHint.Unspecified )
				{
					parentTable.JoinHint = JoinHint.Optional;
				}

				// Get the resource data table
				SqlTable dataTable =
					sqlQuery.CreateJoinedTable( "dbo." + fieldTable, "d", parentTable, JoinHint.Unspecified, // data-table to resource
						"EntityId", parentTable.IdColumn );

				bool isFieldWriteOnly = false;
                isFieldWriteOnly = field.IsFieldWriteOnly ?? false;

                string fieldIdParamName = RegisterSharedParameter( DbType.Int64, resourceDataColumn.FieldId.Id.ToString( CultureInfo.InvariantCulture ) );

				dataTable.Conditions.Add( "$.FieldId = " + ( isFieldWriteOnly ? "-1" : FormatEntity( resourceDataColumn.FieldId, fieldIdParamName ) ) );

				dataTable.FilterByTenant = true;

				string sql = GetColumnSql( dataTable, "Data" );
				result = new SqlExpression
				{
					Sql = sql,
					DatabaseType = field.ConvertToDatabaseType( )
				};

				//hack the cast type in result n sql 
				if ( fieldTable != "Data_NVarChar"
				     && resourceDataColumn.CastType != null
				     && resourceDataColumn.CastType.GetDbType( ) == DatabaseType.StringType.GetDbType( ) )
				{
					string targetType = resourceDataColumn.CastType.GetSqlDbTypeString( );
					result = new SqlExpression
					{
						Sql = string.Format( "try_convert({0}, {1})", targetType, result.Sql ),
						DatabaseType = resourceDataColumn.CastType
					};
				}

				// Force bool fields to take their default value if they're null or not found.
				if ( fieldType.Alias == "core:boolField" )
				{
					string parentIdColumn = GetColumnSql( parentTable, parentTable.IdColumn );
					result = new SqlExpression
					{
						Sql = string.Format( "isnull({0}, case when {1} is null then null else {2} end)", result.Sql, parentIdColumn, ( field.DefaultValue == "True" || field.DefaultValue == "true" ) ? 1 : 0 ),
						DatabaseType = result.DatabaseType
					};
				}
				else if ( fieldType.Alias == "core:decimalField" || fieldType.Alias == "core:currencyField" )
				{
					// ReSharper disable EmptyGeneralCatchClause
					try
					{
						object decimalPlacesObject = ( field ).GetField( new EntityRef( "core", "decimalPlaces" ) );
						int decimalPlaces;
						if ( decimalPlacesObject != null && int.TryParse( decimalPlacesObject.ToString( ), out decimalPlaces ) )
						{
							result = new SqlExpression
							{
								Sql = string.Format( "try_convert(decimal(38,{1}), {0})", result.Sql, decimalPlacesObject ),
								DatabaseType = result.DatabaseType,
								DecimalPlacesCallback = ( ) => decimalPlaces
							};
						}
						else
						{
							result = new SqlExpression
							{
								Sql = string.Format( "try_convert(decimal(38,3), {0})", result.Sql ),
								DatabaseType = result.DatabaseType,
								DecimalPlacesCallback = ( ) => 3
							};
						}
					}
					catch
					{
						//DO Nothing
					}
					// ReSharper restore EmptyGeneralCatchClause
				}

				result.FieldId = resourceDataColumn.FieldId.Id;

				// Store result in-case it is used again
				sqlQuery.References.RegisterExpression( resourceDataColumn.NodeId, resourceDataColumn.FieldId, result );

				convertResult.Expression = result;
				return true;
			}
			catch ( Exception exc )
			{
				convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
			}
        }

		/// <summary>
		///		Converts the resource expression.
		/// </summary>
		/// <param name="resourceExpression">The resource expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="isUnderAggregateExpression">if set to <c>true</c> [is under aggregate expression].</param>
		/// <returns></returns>
	    private SqlExpression ConvertResourceExpression( ResourceExpression resourceExpression, SqlQuery sqlQuery, bool isUnderAggregateExpression = false )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertResourceExpressionImpl( resourceExpression, sqlQuery, isUnderAggregateExpression, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		/// Tries to convert the resource expression.
		/// </summary>
		/// <param name="resourceExpression">The resource expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="isUnderAggregateExpression">if set to <c>true</c> [is under aggregate expression].</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertResourceExpression( ResourceExpression resourceExpression, SqlQuery sqlQuery, bool isUnderAggregateExpression, out SqlExpression sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertResourceExpressionImpl( resourceExpression, sqlQuery, isUnderAggregateExpression, ref convertResult );

			sqlExpression = convertResult.Expression;
				return result;
	    }

		/// <summary>
		///		Converts a RelationshipField expression request into a SqlExpression.
		/// </summary>
		/// <param name="resourceExpression">The resource expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="isUnderAggregateExpression">current resource expression is under another aggregate expression or not. if yes, the result sql call back without xml output</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
        private bool TryConvertResourceExpressionImpl(ResourceExpression resourceExpression, SqlQuery sqlQuery, bool isUnderAggregateExpression, ref ConvertResult convertResult)
        {
		    try
		    {
			    // Generate a standard expression as if this were an ID column.
			    var idExpr = new IdExpression
			    {
				    NodeId = resourceExpression.NodeId
			    };
			    SqlExpression result = ConvertIdExpression( idExpr, sqlQuery );
			    result.IsResource = true;

			    EntityRef orderField = resourceExpression.OrderFieldId;
			    EntityRef isOfType = new EntityRef( "core:isOfType" );
			    // If no ordering field was specified, use enumOrder, if the type was an enum.
			    ResourceEntity resourceEntity = sqlQuery.References.FindEntity<ResourceEntity>( resourceExpression.NodeId );

			    bool isEnum = false;
			    if ( orderField == null )
			    {
				    EntityType type = Model.Entity.Get<EntityType>( resourceEntity.EntityTypeId );
				    if ( type.Is<EnumType>( ) )
				    {
					    orderField = new EntityRef( "core:enumOrder" );
					    isEnum = true;
				    }
				    else
				    {
					    orderField = new EntityRef( "core:name" );
					    isEnum = false;
				    }
			    }

			    result.DatabaseType = isEnum ? ( DatabaseType ) DatabaseType.ChoiceRelationshipType : DatabaseType.InlineRelationshipType;

			    // TODO : This should share logic with ConvertResourceDataColumnExpression to ensure security, and that same table is picked.
			    // Then set callbacks to decorate behavior in rendering and ordering.
			    // ReSharper disable ImplicitlyCapturedClosure
			    result.ResultSqlCallback = expr =>
			    {
				    var field = Model.Entity.Get<Field>( resourceExpression.FieldId.Id );
				    FieldType fieldType = field.GetFieldType( );
				    string fieldTable = !string.IsNullOrEmpty( fieldType.DbFieldTable )
					    ? fieldType.DbFieldTable
					    : "Data_NVarChar";

					string fieldIdParamName = RegisterSharedParameter( DbType.Int64, resourceExpression.FieldId.Id.ToString( CultureInfo.InvariantCulture ) );

				    string sql = string.Format( !isUnderAggregateExpression ? "( select {1} id, Data text from dbo.{0} where EntityId = {1} and FieldId = {2} and TenantId = @tenant for xml raw('e') )" :
						"( select Data text from {0} where EntityId = {1} and FieldId = {2} and TenantId = @tenant )", fieldTable, expr.Sql, FormatEntity( resourceExpression.FieldId, fieldIdParamName ) );
				    return sql;
			    };
			    result.DisplaySqlCallback = expr =>
			    {
				    var field = Model.Entity.Get<Field>( resourceExpression.FieldId.Id );
				    FieldType fieldType = field.GetFieldType( );
				    string fieldTable = !string.IsNullOrEmpty( fieldType.DbFieldTable )
					    ? fieldType.DbFieldTable
					    : "Data_NVarChar";

					string fieldIdParamName = RegisterSharedParameter( DbType.Int64, resourceExpression.FieldId.Id.ToString( CultureInfo.InvariantCulture ) );

				    string sql =
					    string.Format(
						    "( select Data from dbo.{0} where EntityId = {1} and FieldId = {2} and TenantId = @tenant )",
							fieldTable, expr.Sql, FormatEntity( resourceExpression.FieldId, fieldIdParamName ) );
				    return sql;
			    };
			    result.DecimalPlacesCallback = ( ) =>
			    {
				    var field = Model.Entity.Get<Field>( resourceExpression.FieldId.Id );
				    CurrencyField cf = field.As<CurrencyField>( );
				    int? decimalPlaces = null;
				    if ( cf != null )
				    {
					    decimalPlaces = cf.DecimalPlaces ?? 2;
				    }
				    else
				    {
					    DecimalField df = field.As<DecimalField>( );
					    if ( df != null )
					    {
						    decimalPlaces = df.DecimalPlaces ?? 3;
					    }
				    }
				    return decimalPlaces;
			    };

			    result.ConditionSqlCallback = expr => expr.Sql;

			    result.OrderingSqlCallback = expr =>
			    {
					string orderFieldParamName = RegisterSharedParameter( DbType.Int64, orderField.Id.ToString( CultureInfo.InvariantCulture ) );

				    string sql =
					    string.Format(
						    "( select Data from dbo.{0} where EntityId = {1} and FieldId = {2} and TenantId = @tenant )",
						    isEnum ? "Data_Int" : "Data_NVarChar",
							expr.Sql, FormatEntity( orderField, orderFieldParamName ) );
				    return sql;
			    };

			    result.PreAggregateTransform = ( expr, method ) =>
			    {
				    if ( method != AggregateMethod.Max && method != AggregateMethod.Min && method != AggregateMethod.List )
				    {
					    return expr.Sql;
				    }
				    var orderColumn = new ResourceDataColumn
				    {
					    NodeId = resourceExpression.NodeId,
					    FieldId = orderField,
				    };
				    SqlExpression resultExpr = ConvertResourceDataColumnExpression( orderColumn, sqlQuery );
				    return resultExpr.Sql;
			    };

			    result.PostAggregateTransform = ( aggExpr, method ) =>
			    {
				    if ( method != AggregateMethod.Max && method != AggregateMethod.Min && method != AggregateMethod.List )
				    {
					    return new SqlExpression( aggExpr );
				    }

					string entityTypeIdParamName = RegisterSharedParameter( DbType.Int64, resourceEntity.EntityTypeId.Id.ToString( CultureInfo.InvariantCulture ) );
					string isOfTypeParamName = RegisterSharedParameter( DbType.Int64, isOfType.Id.ToString( CultureInfo.InvariantCulture ) );
					string orderFieldParamName = RegisterSharedParameter( DbType.Int64, orderField.Id.ToString( CultureInfo.InvariantCulture ) );

				    string sql = string.Format(
					    " ( select top 1 o.EntityId from dbo.Data_Int o join dbo.Relationship t on o.EntityId = t.FromId and t.ToId = {0} and t.TypeId = {1} and t.TenantId = @tenant where o.Data = {2} and o.FieldId = {3} and o.TenantId = @tenant )",
						FormatEntity( resourceEntity.EntityTypeId, entityTypeIdParamName ), FormatEntity( isOfType, isOfTypeParamName ), aggExpr,
						FormatEntity( orderField, orderFieldParamName ) );

				    var sqlExpr = new SqlExpression( sql );
				    SqlExpression.CopyTransforms( result, sqlExpr );
				    return sqlExpr;
			    };

				convertResult.Expression = result;
				return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
        }

		/// <summary>
		///		Converts the structure view expression.
		/// </summary>
		/// <param name="structureViewExpression">The structure view expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns></returns>
	    private SqlExpression ConvertStructureViewExpression( StructureViewExpression structureViewExpression, SqlQuery sqlQuery )
	    {
		    var convertResult = new ConvertResult( );

		    if ( TryConvertStructureViewExpressionImpl( structureViewExpression, sqlQuery, ref convertResult ) )
		    {
			    return convertResult.Expression;
		    }

		    throw convertResult.GetConvertException( );
	    }

		/// <summary>
		///		Tries to convert the structure view expression.
		/// </summary>
		/// <param name="structureViewExpression">The structure view expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="sqlExpression">The SQL expression.</param>
		/// <returns></returns>
	    private bool TryConvertStructureViewExpression( StructureViewExpression structureViewExpression, SqlQuery sqlQuery, out SqlExpression  sqlExpression )
	    {
			var convertResult = new ConvertResult( );

		    bool result = TryConvertStructureViewExpressionImpl( structureViewExpression, sqlQuery, ref convertResult );

			sqlExpression = convertResult.Expression;
		    return result;
	    }

		/// <summary>
		///		Converts a resource data column expression request into a SqlExpression.
		/// </summary>
		/// <param name="structureViewExpression">The structure view expression.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <param name="convertResult">The convert result.</param>
		/// <returns></returns>
		private bool TryConvertStructureViewExpressionImpl( StructureViewExpression structureViewExpression, SqlQuery sqlQuery, ref ConvertResult convertResult )
        {
		    try
		    {
			    // Find the 'Resource' table for the node this expression is pointing to
			    SqlTable parentTable = sqlQuery.References.FindSqlTable( structureViewExpression.NodeId );

			    // Get structure view details
			    var structureView = Model.Entity.Get<StructureView>( structureViewExpression.StructureViewId );
			    EntityRef rootRelationshipId = new EntityRef( "isRootForStructureView" );			    
			    IEntity structureHierarchyRelationshipId = structureView.StructureHierarchyRelationship;
                EntityRef nameFieldId = new EntityRef("core:name");
                string directionSuffix = (structureView.FollowRelationshipInReverse ?? false) ? "Fwd" : "Rev";
                bool detectRoots = structureView.DetectRootLevels ?? false;                

				string structureViewExpressionParamName = RegisterSharedParameter( DbType.Int64, structureViewExpression.StructureViewId.Id.ToString( CultureInfo.InvariantCulture ) );
				string rootRelationshipIdParamName = RegisterSharedParameter( DbType.Int64, rootRelationshipId.Id.ToString( CultureInfo.InvariantCulture ) );                
				string structureHierarchyRelationshipIdParamName = RegisterSharedParameter( DbType.Int64, structureHierarchyRelationshipId.Id.ToString( CultureInfo.InvariantCulture ) );
                string nameFieldIdParamName = RegisterSharedParameter(DbType.Int64, nameFieldId.Id.ToString(CultureInfo.InvariantCulture));
                string detectRootsParamName = RegisterSharedParameter(DbType.Int32, detectRoots ? "1" : "0");

		        string parentTableIdSql = GetColumnSql( parentTable, parentTable.IdColumn );

                string sqlFormat = string.Format("dbo.fnGetStructureLevelNames{0}(@tenant, {1}, {2}, {3}, {4}, {5}, {6}, char(2), char(3), {{0}}, {{1}})",
                    directionSuffix,
                    parentTableIdSql,
					FormatEntity( structureViewExpression.StructureViewId, structureViewExpressionParamName ),
					FormatEntity( rootRelationshipId, rootRelationshipIdParamName ),
					FormatEntity( structureHierarchyRelationshipId, structureHierarchyRelationshipIdParamName ),
                    FormatEntity( nameFieldId, nameFieldIdParamName ),
                    detectRootsParamName
                );
                
		        string sql = string.Format(sqlFormat, "DEFAULT", 0);

                // Sort the structure view based on the full path
                string orderingSql = string.Format(sqlFormat, "NULL", 1);

			    var expression = new SqlExpression
			    {
                    Sql = sql,
                    OrderingSqlCallback = expr => orderingSql
			    };

		        convertResult.Expression = expression;
			    return true;
		    }
		    catch ( Exception exc )
		    {
			    convertResult.SetFailure( ConvertResult.FailureReason.Native, null, exc );
				return false;
		    }
        }

        /// <summary>
		///     Converts a typed value to a literal expression.
        /// </summary>
		/// <param name="typedValue">The typed value.</param>
        /// <returns></returns>
		private SqlExpression ConvertTypedValueToLiteral( TypedValue typedValue )
        {
			string sql;
			if ( typedValue.Type is IdentifierType )
			{
				string sValue = typedValue.Value.ToString( );
				long id;
				EntityRef entityRef;
				string paramName;

				if ( long.TryParse( sValue, out id ) )
				{
					entityRef = new EntityRef( id );
					paramName = RegisterSharedParameter( DbType.Int64, id.ToString( CultureInfo.InvariantCulture ) );
				}
				else
				{
					entityRef = new EntityRef( sValue );
					paramName = RegisterSharedParameter( DbType.String, sValue );
				}

				sql = FormatEntity( entityRef, paramName );
			}
            else if (typedValue.Type is UnknownType)
            {
                sql = "null";
            }
			else if ( typedValue.Type is StringType )
			{
				sql = "(" + RegisterSharedParameter( DbType.String, ( string ) typedValue.Value ) + " collate Latin1_General_CI_AI)";
			}
			else
			{
				sql = typedValue.Type.ConvertToSqlLiteral( typedValue.Value );
			}

	        return new SqlExpression( sql );
		}

		/// <summary>
		///     Gets the date description expression.
		/// </summary>
		/// <param name="sqlExprToConstrain">The SQL expression to constrain.</param>
		/// <param name="exprToConstrain">The expression to constrain.</param>
		/// <returns></returns>
		private string GetDateDescriptionExpression( SqlExpression sqlExprToConstrain, ScalarExpression exprToConstrain )
            {
			string expr = sqlExprToConstrain.Sql;

			// Determine if field is DateTime
			// Note: this is hacky as it only works with ResourceDataColumn at the moment.
			bool convertUtcToLocal = false;

			var column = exprToConstrain as ResourceDataColumn;

			if ( column != null )
            {
				ResourceDataColumn rdc = column;
				IEntity field = Model.Entity.Get( rdc.FieldId );
				convertUtcToLocal = Model.Entity.Is<DateTimeField>( field );
			}

			// Delegate to perform UTC adjustments
			Func<DateTime, string> format = value =>
            {
					DateTime adjusted = value;
					if ( convertUtcToLocal )
                {
						adjusted = TimeZoneHelper.ConvertToUtc( value, _structuredQuery.TimeZoneName );
					}
					string result = "'" + adjusted.ToString( "s" ) + "'";
					return result;
                };


			// Determine boundaries (in local time zone, then converted to UTC)
            MarkSqlAsUncacheable( "Relies on UtcNow" );
			DateTime localNow = TimeZoneHelper.ConvertToLocalTime( DateTime.UtcNow, _structuredQuery.TimeZoneName );

			DateTime locToday = localNow.Date;
			string today = format( locToday );
			string yesterday = format( locToday.AddDays( -1 ) );
			string tomorrow = format( locToday.AddDays( 1 ) );
			string tomorrowEnd = format( locToday.AddDays( 2 ) );

			DateTime locThisWeek = locToday.AddDays( -( int ) locToday.DayOfWeek ); // back to the last Sunday, including today
			string thisWeek = format( locThisWeek );
			string lastWeek = format( locThisWeek.AddDays( -7 ) );
			string nextWeek = format( locThisWeek.AddDays( 7 ) );
			string nextWeekEnd = format( locThisWeek.AddDays( 14 ) );

			var locThisMonth = new DateTime( locToday.Year, locToday.Month, 1 );
			string thisMonth = format( locThisMonth );
			string lastMonth = format( locThisMonth.AddMonths( -1 ) );
			string nextMonth = format( locThisMonth.AddMonths( 1 ) );
			string nextMonthEnd = format( locThisMonth.AddMonths( 2 ) );

			var locThisYear = new DateTime( locToday.Year, 1, 1 );
			string thisYear = format( locThisYear );
			string lastYear = format( locThisYear.AddYears( -1 ) );
			string nextYear = format( locThisYear.AddYears( 1 ) );
			string nextYearEnd = format( locThisYear.AddYears( 2 ) );

			// Build SQL
			var sb = new StringBuilder( );
			sb.Append( "case" );
			sb.AppendFormat( "\n when {0} is null then null", expr );
			string sql = "\n when " + expr + " >= {0} and " + expr + " < {1} then '{2}'";

			sb.AppendFormat( sql, today, tomorrow, "Today" );
			sb.AppendFormat( sql, yesterday, today, "Yesterday" );
			sb.AppendFormat( sql, tomorrow, tomorrowEnd, "Tomorrow" );
			for ( int i = 0; i < 7; i++ )
        {
				sb.AppendFormat( sql, format( locThisWeek.AddDays( i ) ), format( locThisWeek.AddDays( i + 1 ) ), ( ( DayOfWeek ) i ).ToString( ) );
            }

			// Past
			sb.AppendFormat( sql, lastWeek, thisWeek, "Last week" );
			sb.AppendFormat( sql, thisMonth, today, "Earlier this month" );
			sb.AppendFormat( sql, lastMonth, thisMonth, "Last month" );
			sb.AppendFormat( sql, thisYear, today, "Earlier this year" );
			sb.AppendFormat( sql, lastYear, thisYear, "Last year" );
			sb.AppendFormat( "\n when {0} < {1} then '{2}'", expr, lastYear, "Older" );

			// Future
			sb.AppendFormat( sql, nextWeek, nextWeekEnd, "Next week" );
			sb.AppendFormat( sql, today, nextMonth, "Later this month" );
			sb.AppendFormat( sql, nextMonth, nextMonthEnd, "Next month" );
			sb.AppendFormat( sql, today, nextYear, "Later this year" );
			sb.AppendFormat( sql, nextYear, nextYearEnd, "Next year" );
			sb.AppendFormat( "\n when {0} >= {1} then '{2}'", expr, nextYearEnd, "Future" );

			sb.AppendFormat( "\n else 'Other' end" );

			return sb.ToString( );
        }


        public class AggExpressionException : Exception
        {
            public AggExpressionException()
                : base("The aggregate expression NodeId could not be found.")
            { }
        }

		/// <summary>
		///		Convert Result
		/// </summary>
	    private class ConvertResult
	    {
		    /// <summary>
		    /// The failure message
		    /// </summary>
		    private string _failureMessage;

		    /// <summary>
		    /// The inner exception
		    /// </summary>
		    private Exception _innerException;

		    /// <summary>
		    /// The failure reason
		    /// </summary>
		    private FailureReason _failure;

		    /// <summary>
		    /// Gets or sets the expression.
		    /// </summary>
		    /// <value>
		    /// The expression.
		    /// </value>
		    internal SqlExpression Expression
		    {
			    get;
			    set;
		    }

		    /// <summary>
		    /// Sets the failure.
		    /// </summary>
		    /// <param name="failure">The failure reason.</param>
		    /// <param name="failureMessage">The failure message.</param>
		    /// <param name="innerException">The inner exception.</param>
		    internal void SetFailure( FailureReason failure, string failureMessage = null, Exception innerException = null )
		    {
			    _failure = failure;
			    _failureMessage = failureMessage;
			    _innerException = innerException;
		    }

		    /// <summary>
		    /// Gets the convert exception.
		    /// </summary>
		    /// <returns></returns>
		    internal Exception GetConvertException( )
		    {
			    switch ( _failure )
			    {
					case FailureReason.Native:
					    return _innerException;
					case FailureReason.Exception:
						if ( _failureMessage != null && _innerException != null )
					    {
						    return new Exception( _failureMessage, _innerException );
					    }

						if ( _innerException != null )
						{
							return new Exception( string.Empty, _innerException );
						}

						if ( _failureMessage != null )
						{
							return new Exception( _failureMessage );
						}

					    return new Exception( );
					case FailureReason.InvalidOperation:
						if ( _failureMessage != null && _innerException != null )
					    {
						    return new InvalidOperationException( _failureMessage, _innerException );
					    }

						if ( _innerException != null )
						{
							return new InvalidOperationException( string.Empty, _innerException );
						}

						if ( _failureMessage != null )
						{
							return new InvalidOperationException( _failureMessage );
						}

						return new InvalidOperationException( );
					case FailureReason.Security:

						if ( _failureMessage != null )
						{
							return new PlatformSecurityException( _failureMessage );
						}

						return new PlatformSecurityException( );
					case FailureReason.Aggregate:
					    if ( _failureMessage != null && _innerException != null )
					    {
						    return new AggregateException( _failureMessage, _innerException );
					    }

						if ( _innerException != null )
						{
							return new AggregateException( _innerException );
						}

						if ( _failureMessage != null )
						{
							return new AggregateException( _failureMessage );
						}

					    return new AggregateException( );
					case FailureReason.Query:
						if ( _failureMessage != null && _innerException != null )
					    {
						    return new QueryException( _failureMessage, _innerException );
					    }

						if ( _innerException != null )
						{
							return new QueryException( string.Empty, _innerException );
						}

						if ( _failureMessage != null )
						{
							return new QueryException( _failureMessage );
						}

						return new QueryException( );
					default:
					    return new InvalidOperationException( );
			    }
		    }

			/// <summary>
			///		Failure reason.
			/// </summary>
		    internal enum FailureReason
		    {
				Native = 0,

				Exception = 1,

				InvalidOperation = 2,

				Security = 3,

				Aggregate = 4,

				Query = 5
		    }
	    }
    }
}