// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using SqlException = System.Data.SqlClient.SqlException;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.Database;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.Database;

namespace ReadiNow.QueryEngine.Runner
{
    /// <summary>
    /// Executes a <see cref="StructuredQuery"/>.
    /// </summary>
    public class QueryRunner : IQueryRunner
    {
        internal const string TenantParameterName = "@tenant";
        internal const string QuickSearchParameterName = "@quicksearch";
        internal const string UserParameterName = "@user";
        internal const string FirstRowParameterName = "@first"; // @first = zero-based first row to show
        internal const string LastRowParameterName = "@last";   // @last  = zero-based first row to not show
        internal const string EntityBatchParameterName = "@entityBatch"; // See also: Entity.GetMatchesAsIds
        internal const string EntityListParameterName = "@entitylist";

        internal const int ExceededCpuErrorCode = -2146232060;      // Error code returned when QUERY_GOVERNOR_COST_LIMIT is exceeded for a query.
        internal const byte SeverityLevelResourceExhausted = 17;    // Indicates that SQL Server has run out of a configurable resource, such as locks. Can be corrected by the DBA.

        readonly IQuerySqlBuilder _querySqlBuilder;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="querySqlBuilder">Query builder to be used when converting structured queries to SQL.</param>
        /// <param name="databaseProvider"></param>
        public QueryRunner( IQuerySqlBuilder querySqlBuilder, IDatabaseProvider databaseProvider )
        {
            if (querySqlBuilder == null)
                throw new ArgumentNullException( nameof( querySqlBuilder ) );
            if (databaseProvider == null )
                throw new ArgumentNullException( nameof( databaseProvider ) );

            _querySqlBuilder = querySqlBuilder;
            DatabaseProvider = databaseProvider;
        }

        /// <summary>
        /// Query builder to be used when converting structured queries to SQL.
        /// </summary>
        internal IQuerySqlBuilder QuerySqlBuilder => _querySqlBuilder;

        /// <summary>
        /// DatabaseProvider to run the SQL commands.
        /// </summary>
        internal IDatabaseProvider DatabaseProvider { get; }

        /// <summary>
        /// Executes the specified query.
        /// </summary>
        /// <param name="query">The query. This cannot be null.</param>
        /// <param name="settings">Settings to control SQL generation. Defaults are used if omitted.</param>
        /// <returns>
        /// A <see cref="QueryResult"/> containing the query and related details.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="query"/> cannot be null.
        /// </exception>
        public QueryResult ExecuteQuery( StructuredQuery query, QuerySettings settings )
        {
            if ( query == null )
            {
                throw new ArgumentNullException( nameof( query ) );
            }
            if ( settings == null )
            {
                settings = new QuerySettings
                {
                    Hint = "QueryEngine"
                };
            }

            // Check security settings
            VerifySecuritySettings( settings );

            QueryBuild querySql;

            // Allow non-admins to run reports
            using ( new SecurityBypassContext( ) )
            {
                querySql = _querySqlBuilder.BuildSql( query, settings );
            }

            return ExecutePrebuiltQuery( query, settings, querySql );           
        }

        /// <summary>
        /// Executes the specified query.
        /// </summary>
        /// <param name="query">The query. This cannot be null.</param>
        /// <param name="settings">Settings to control SQL generation. Defaults are used if omitted.</param>
        /// <param name="queryBuild">The prebuilt query.</param>
        /// <returns>
        /// A <see cref="QueryResult"/> containing the query and related details.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="query"/> cannot be null.
        /// </exception>
        public QueryResult ExecutePrebuiltQuery(StructuredQuery query, QuerySettings settings, QueryBuild queryBuild )
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof( query ));
            }
            if ( queryBuild == null)
            {
                throw new ArgumentNullException( nameof( queryBuild ) );
            }
            if (settings == null)
            {
                settings = new QuerySettings
                {
                    Hint = "QueryEngine"
                };
            }

            using (Profiler.Measure("QueryRunner.ExecuteQuery {0}", settings.Hint))
            {
                QueryResult result = new QueryResult( queryBuild );
                string sql;
                DataTable dataTable;

                if ( settings.ResultSchemaOnly )
                {
                    // This is a bit of a hack. Ideally someone who wants schema only could just use the SqlBuilder .. but that isn't working.
                    // Likely due to the mutate that's currently happening in the runner caching layer. Maybe we can move it to the builder layer.
                    return result;
                }

                // Check security settings (ok if it happens twice)
                VerifySecuritySettings( settings );

                sql = queryBuild.Sql;

                using (IDatabaseContext context = DatabaseProvider.GetContext())
                {
                    IDbDataAdapter adapter = context.CreateDataAdapter(sql);
                    using (adapter as IDisposable)
                    {
                        try
                        {
                            AddAllParameters( settings, queryBuild, context, adapter );

                            dataTable = new DataTable();
                            FillFromAdapter(dataTable, context, adapter, settings);
                        }
                        catch (SqlException ex)
                        {
                            if (ex.ErrorCode == ExceededCpuErrorCode && ex.Class == SeverityLevelResourceExhausted)     // Exceeded report CPU limit
                            {
                                EventLog.Application.WriteTrace("{0}\r\nFailed to run report query due to Cpu Limit exceeded. Hint: {1}\r\n\r\n{2}\r\n\r\n{3}", ex.Message, settings.Hint, ex, sql);
                                throw new ExceededReportCpuLimitException(ex);
                            }

                            EventLog.Application.WriteError("{0}\r\nFailed to run report query due to sql error. Hint: {1}\r\n\r\n{2}\r\n\r\n{3}", ex.Message, settings.Hint, ex, sql);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            EventLog.Application.WriteError("{0}\r\nFailed to run report query. Hint: {1}\r\n\r\n{2}\r\n\r\n{3}", ex.Message, settings.Hint, ex, sql);
                            throw;
                        }
                    }

                    StructureViewHelper.SecureStructureViewData(settings, result, dataTable);

                    MetadataHelper.CaptureMetadata(query, settings, result, dataTable);
                }

                // Note: identify cache dependencies after getting SQL, so that any calculations are resolved into the structured query.
                IdentifyCacheDependencies( queryBuild.FinalStructuredQuery, settings );

                return result;
            }
        }

        /// <summary>
        /// Check cache dependencies. (Note: wrapped, so we can write tests guaranteeing we have the same implementation)
        /// </summary>
        internal static void IdentifyCacheDependencies(StructuredQuery query, QuerySqlBuilderSettings settings)
        {
            // Note: if we are suppressing the root type check, then it means that the caller will be joining the query into a larger query.
            // (I.e. this is a security subquery in a secured report).
            // If that is the case, then the parent query will already be registering invalidation watches for the type of that node.
            // So we don't need to further add them for the security query as well.
            // This is an important optimisation because there are security reports that apply to all resources, and get joined into nodes that
            // are only for specific resource types. So without ignoring the root, we would basically invalidate every report as part of every entity change.

            StructuredQueryHelper.IdentifyResultCacheDependencies(query, false, settings.SuppressRootTypeCheck || settings.SupportRootIdFilter);
        }

        /// <summary>
        /// Verify that current user is appropriately set.
        /// </summary>
        /// <param name="settings"></param>
        private static void VerifySecuritySettings( QuerySettings settings )
        {
            if ( settings.RunAsUser == 0 )
            {
                settings.RunAsUser = RequestContext.GetContext( ).Identity.Id;

                if ( settings.RunAsUser == 0 )
                {
                    if ( SecurityBypassContext.IsActive )
                        settings.SecureQuery = false;
                    else
                        throw new Exception( "Cannot run report as super-admin outside of an explicit SecurityBypassContext" );
                }
            }
        }

        /// <summary>
        /// Add all database query parameters.
        /// </summary>
        private void AddAllParameters( QuerySettings settings, QueryBuild result, IDatabaseContext context, IDbDataAdapter adapter )
        {
            IDbCommand selectCommand = adapter.SelectCommand;

            selectCommand.AddParameter( TenantParameterName, DatabaseType.IdentifierType, RequestContext.TenantId );
            if ( result.DataReliesOnCurrentUser )
            {
                selectCommand.AddParameter( UserParameterName, DatabaseType.IdentifierType, settings.RunAsUser );
            }
	        
			if ( result.EntityBatchDataTable != null )
	        {
                selectCommand.AddTableValuedParameter( EntityBatchParameterName, result.EntityBatchDataTable );
	        }

	        AddQuickSearchParameter( settings, selectCommand );
            AddFauxRelationshipParameters( settings, selectCommand );
            AddRootIdFilterParameter( settings, selectCommand );

            if ( settings.ValueList != null )
            {
                selectCommand.AddStringListParameter( "@valueList", settings.ValueList );
            }

            if ( settings.SupportPaging )
            {
                SetPage( settings.FirstRow, settings.FirstRow + settings.PageSize, selectCommand );
            }

	        if ( result.SharedParameters != null )
	        {
				foreach ( KeyValuePair<ParameterValue, string> parameter in result.SharedParameters )
		        {
                    selectCommand.AddParameter( parameter.Value, parameter.Key.Type, parameter.Key.Value );
		        }
	        }
        }

        /// <summary>
        /// Add quick search information.
        /// </summary>
        private void AddQuickSearchParameter( QuerySettings settings, IDbCommand selectCommand )
        {
            if ( !string.IsNullOrWhiteSpace( settings.QuickSearchTerm ) )
            {
                if ( !settings.SupportQuickSearch )
                {
                    throw new Exception( "Cannot apply quicksearch because SupportQuickSearch was not turned on during query evaluation." );
                }

                string quickSearchLike = SqlBuilder.BuildSafeLikeParameter( settings.QuickSearchTerm, "%", "%" );
                selectCommand.AddParameterWithValue( QuickSearchParameterName, quickSearchLike, 500 );
            }
        }

        /// <summary>
        /// Add parameters for filtering a report to be used on an edit form.
        /// </summary>
        private void AddFauxRelationshipParameters( QuerySettings settings, IDbCommand selectCommand )
        {
            if ( settings.TargetResource != 0 )
            {
                selectCommand.AddParameterWithValue( "@targetResource", settings.TargetResource );
            }
            if ( settings.IncludeResources != null )
            {
                selectCommand.AddIdListParameter( "@includeResources", settings.IncludeResources );
            }
            if ( settings.ExcludeResources != null )
            {
                selectCommand.AddIdListParameter( "@excludeResources", settings.ExcludeResources );
            }
        }

        /// <summary>
        /// Add root ID filter information.
        /// </summary>
        private void AddRootIdFilterParameter( QuerySettings settings, IDbCommand selectCommand)
        {
            if (settings.RootIdFilterList != null)
            {
                if (!settings.SupportRootIdFilter)
                {
                    throw new Exception("Cannot apply rood ID filter because SupportRootIdFilter was not turned on during query evaluation.");
                }
                selectCommand.AddIdListParameter(EntityListParameterName, settings.RootIdFilterList);
            }
            else
            {
                if (settings.SupportRootIdFilter)
                {
                    throw new Exception("Query expects RoodIdFilterList but none was provided.");
                }
                if (selectCommand.CommandText.Contains(EntityListParameterName))
                {
                    throw new Exception("Query expects EntityList but none was provided.");
                }
            }
        }

        /// <summary>
        /// Set the rows to be returned in the query.
        /// </summary>
        private void SetPage(int firstRow, int lastRow, IDbCommand selectCommand)
        {
            if (firstRow < 0)
            {
                throw new ArgumentException("Cannot be negative", nameof( firstRow ));
            }
            if (lastRow < firstRow)
            {
                throw new ArgumentException("lastRow < firstRow", nameof( lastRow ));
            }
            if (selectCommand == null)
            {
                throw new ArgumentNullException(nameof( selectCommand ));
            }

            // Zero based
            selectCommand.AddParameterWithValue(FirstRowParameterName, firstRow);
            selectCommand.AddParameterWithValue(LastRowParameterName, lastRow);
        }                

        /// <summary>
        /// Fill data table from adapter
        /// </summary>
        private void FillFromAdapter(DataTable dataTable, IDatabaseContext context, IDbDataAdapter adapter, QuerySettings settings)
        {
            using ( Profiler.Measure( "Report {0} run SQL", settings.Hint ) )
            {
                context.Fill( adapter, dataTable );
            }
        }

        public class ExceededReportCpuLimitException: EDC.ReadiNow.Services.Exceptions.TenantResourceLimitException
        {
            const string customerMessage = "The report has been prevented from running due to identified performance issues. Please contact your administrator.";
            const string reasonCode = "ReportCpuExceeded";

            public ExceededReportCpuLimitException(SqlException ex) : base(ex.Message, customerMessage, reasonCode, ex)
            {
            }

        }
    }
}
