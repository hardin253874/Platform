// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Model = EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core;
using IEntityRepository = EDC.ReadiNow.Model.IEntityRepository;
using EntityRef = EDC.ReadiNow.Model.EntityRef;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.Reporting.Definitions;
using ReadiNow.Reporting.Request;
using ReadiNow.Reporting.Result;
using ReadiNow.Reporting.Helpers;
using EDC.Exceptions;

namespace ReadiNow.Reporting
{
    public class ReportingInterface
    {
        /// <summary>
        /// Create a new <see cref="ReportingInterface"/>.
        /// </summary>
        public ReportingInterface()
        {
            // Consider passing this in on the constructor for future mocking/IoC.
            GraphEntityRepository = Factory.GraphEntityRepository;
            EntityAccessControlService = Factory.EntityAccessControlService;
            QueryRunner = Factory.QueryRunner;
            CachedReportToQueryConverter = Factory.ReportToQueryConverter;
            NonCachedReportToQueryConverter = Factory.NonCachedReportToQueryConverter;
            QueryRunnerCacheKeyProvider = Factory.Current.Resolve<IQueryRunnerCacheKeyProvider>( );
        }

		/// <summary>
		/// Initializes the <see cref="ReportingInterface"/> class.
		/// </summary>
	    static ReportingInterface( )
	    {
			ConfigurationSettings.Changed += ConfigurationSettings_Changed;
	    }

		/// <summary>
		/// Handles the Changed event of the ConfigurationSettings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		static void ConfigurationSettings_Changed( object sender, EventArgs e )
		{
			_cachedSecureReports = null;
		}


	    /// <summary>
        /// Used for loading the reports.
        /// </summary>
        public IEntityRepository GraphEntityRepository { get; private set; }

        /// <summary>
        /// Used for security checks.
        /// </summary>
        public IEntityAccessControlService EntityAccessControlService { get; private set; }

        /// <summary>
        /// Used for running report SQL.
        /// </summary>
        public IQueryRunner QueryRunner { get; private set; }

        /// <summary>
        /// Used for converting reports to structured queries.
        /// </summary>
        public IReportToQueryConverter CachedReportToQueryConverter { get; private set; }

        /// <summary>
        /// Used for converting reports to structured queries.
        /// </summary>
        public IReportToQueryConverter NonCachedReportToQueryConverter { get; private set; }

        /// <summary>
        /// Used for generating cache keys.
        /// </summary>
        public IQueryRunnerCacheKeyProvider QueryRunnerCacheKeyProvider { get; private set; }

		/// <summary>
		/// The cached value of whether to secure reports or not.
		/// </summary>
	    public static bool? _cachedSecureReports;

        /// <summary>
        /// Runs the report specified by ID.
        /// </summary>
        /// <param name="reportId">The report unique identifier.</param>
        /// <param name="settings">The settings for the report to be run.</param>
        /// <returns>ReportResult.</returns>
        /// <exception cref="System.ArgumentException">
        /// The report identifier resource is not a report.
        /// </exception>
        /// <exception cref="PlatformSecurityException">
        /// The user lacks read access to <paramref name="reportId"/>.
        /// </exception>
        public ReportResult RunReport( long reportId, ReportSettings settings )
        {
            ReportCompletionData completionData = PrepareReport( reportId, settings );
            return completionData.PerformRun( );
        }

        /// <summary>
        /// Runs the report.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="suppressPreload">Pass true if the report has already been preloaded.</param>
        /// <returns>ReportResult.</returns>
        /// <exception cref="System.ArgumentException">@The report identifier resource is not a report.;reportId</exception>
        public ReportResult RunReport( Model.Report report, ReportSettings settings, bool suppressPreload = false )
        {
            ReportCompletionData completionData = PrepareReport( report, settings, suppressPreload );
            return completionData.PerformRun( );
        }

        /// <summary>
        /// Runs the report specified by ID.
        /// </summary>
        /// <param name="reportId">The report unique identifier.</param>
        /// <param name="settings">The settings for the report to be run.</param>
        /// <returns>ReportResult.</returns>
        /// <exception cref="System.ArgumentException">
        /// The report identifier resource is not a report.
        /// </exception>
        /// <exception cref="PlatformSecurityException">
        /// The user lacks read access to <paramref name="reportId"/>.
        /// </exception>
        public ReportCompletionData PrepareReport( long reportId, ReportSettings settings )
        {
            using (Profiler.Measure("ReportingInterface.PrepareReport {0}", reportId))
            using ( CacheContext cacheContext = new CacheContext( ) )
            {
                // Check user has permission to report
                EntityAccessControlService.Demand( new[] { new EntityRef( reportId ) }, new[] { Permissions.Read });

                cacheContext.Entities.Add( reportId );

                using (CacheManager.ExpectCacheHits(true))
                using (new SecurityBypassContext())
                {
                    // Validate the report entity Identifier
                    Model.Report report;
                    using (Profiler.Measure("GraphEntityRepository.Get", reportId))
                    {
                        // Get the report from the graph database
                        report = GraphEntityRepository.Get<Model.Report>( reportId, ReportHelpers.ReportPreloaderQuery );

                        if ( report == null )
                            throw new WebArgumentException( "reportId" );
                    }

                    settings = settings ?? new ReportSettings( );
                    settings.UseStructuredQueryCache = true;

                    return PrepareReport(report, settings, true);
                }
            }
        }

        /// <summary>
        /// Runs the report.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="reportSettings">The settings.</param>
        /// <param name="suppressPreload">Pass true if the report has already been preloaded.</param>
        /// <returns>ReportResult.</returns>
        /// <exception cref="System.ArgumentException">@The report identifier resource is not a report.;reportId</exception>
        public ReportCompletionData PrepareReport( Model.Report report, ReportSettings reportSettings, bool suppressPreload = false )
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }
            if ( reportSettings == null )
            {
                reportSettings = new ReportSettings( );
            }

            StructuredQuery structuredQuery;
            PreparedQuery preparedReport;
            PreparedQuery preparedRollup;
            
            using ( EDC.ReadiNow.Diagnostics.Profiler.Measure( "Prepare report run" ) )
            using ( MessageContext messageContext = new MessageContext( "Reports" ) ) 
            using ( new SecurityBypassContext( ) )
            {
                // Get the structured query
                structuredQuery = GetStructuredQuery( report, reportSettings, suppressPreload );

                // Handle metadata-only request
                if ( reportSettings.RequireSchemaMetadata )
                {
                    ReportResult reportResult = new ReportResult( report, structuredQuery, null, null, null, reportSettings );
                    return new ReportCompletionData( reportResult );
                }

                // Prepare query settings
                preparedReport = PrepareReportRun( structuredQuery, reportSettings );
                preparedReport.QuerySettings.Hint = "Rpt-" + report.Id.ToString( );

                // Handle rollups                                           
                preparedRollup = PrepareReportRollupRun( report, preparedReport.StructuredQuery, reportSettings, preparedReport.QuerySettings );
            }

            Func<ReportResult> resultCallback = ( ) =>
                {
                    ReportResult reportResult = null;
                    QueryResult queryResult = null;
                    QueryResult rollupResult = null;

                    using ( new SecurityBypassContext( ) )
                    {
                        // Execute the query
                        queryResult = QueryRunner.ExecuteQuery( preparedReport.StructuredQuery, preparedReport.QuerySettings );

                        // Execute the rollup query
                        if ( preparedRollup.StructuredQuery != null )
                        {
                            rollupResult = QueryRunner.ExecuteQuery( preparedRollup.StructuredQuery, preparedRollup.QuerySettings );
                        }

                        // Package up the result.
                        reportResult = new ReportResult( report,
                            preparedReport.StructuredQuery, queryResult,
                            preparedRollup.ClientAggregate, rollupResult,
                            reportSettings );
                    }

                    return reportResult;
                };

            // Create cache key (null indicates report is not cacheable)
            IQueryRunnerCacheKey reportCacheKey = null;
            IQueryRunnerCacheKey rollupCacheKey = null;
            ReportResultCacheKey reportResultCacheKey = null;
            
            reportCacheKey = QueryRunnerCacheKeyProvider.CreateCacheKey( preparedReport.StructuredQuery, preparedReport.QuerySettings );
            if ( reportCacheKey != null )
            {
                if ( preparedRollup.StructuredQuery != null )
                {
                    rollupCacheKey = QueryRunnerCacheKeyProvider.CreateCacheKey( preparedRollup.StructuredQuery, preparedRollup.QuerySettings );
                }
                reportResultCacheKey = new ReportResultCacheKey( reportSettings, reportCacheKey, rollupCacheKey );
            }

            // Create completion result
            ReportCompletionData completionData = new ReportCompletionData( );
            completionData.ResultCallback = resultCallback;
            completionData.ResultCacheKey = reportResultCacheKey;
            completionData.CacheContextDuringPreparation = CacheContext.GetContext( );

            return completionData;
        }


        /// <summary>
        /// Get the structured query, possibly from cache.
        /// </summary>
        /// <param name="report">The report to convert.</param>
        /// <param name="settings">The report run settings.</param>
        /// <param name="suppressPreload">True if we should suppress preloading.</param>
        /// <returns>The structured query.</returns>
        private StructuredQuery GetStructuredQuery( Model.Report report, ReportSettings settings, bool suppressPreload )
        {
            using ( MessageContext msg = new MessageContext( "Reports" ) )
            {
                StructuredQuery immutableStructuredQuery;
                StructuredQuery structuredQuery;
                
                bool useStructuredQueryCache = settings.UseStructuredQueryCache;

                ReportToQueryConverterSettings converterSettings = new ReportToQueryConverterSettings
                {
                    SuppressPreload = suppressPreload,
                    RefreshCachedStructuredQuery = settings.RefreshCachedStructuredQuery,
                    SchemaOnly = settings.RequireSchemaMetadata
                   
                };
                
                if ( settings != null && settings.UseStructuredQueryCache )
                {
                    // don't allow mutations of cached copy
                    immutableStructuredQuery = CachedReportToQueryConverter.Convert( report, converterSettings );
                }
                else
                {
                    // don't allow mutations, just so we can log it correctly
                    immutableStructuredQuery = NonCachedReportToQueryConverter.Convert( report, converterSettings );
                }

                structuredQuery = immutableStructuredQuery.DeepCopy( ); // so we can mutate it (in case we need to)

                // Logging
                msg.Append( ( ) => new String( '-', 50 ) );
                msg.Append( ( ) => "GetStructuredQuery" );
                msg.Append( ( ) => "suppressPreload = " + suppressPreload );
                msg.Append( ( ) => "useStructuredQueryCache = " + useStructuredQueryCache );
                msg.Append( ( ) => "Structured Query:\n" + StructuredQueryHelper.ToXml( immutableStructuredQuery ) );
                msg.Append( ( ) => new String( '-', 50 ) );

                return structuredQuery;
            }
        }

        /// <summary>
        /// Applies report settings to the structured query and also builds an appropriate query settings object.
        /// </summary>
        /// <param name="structuredQuery"></param>
        /// <param name="queryReportSettings"></param>
        /// <returns></returns>
        private static PreparedQuery PrepareReportRun( StructuredQuery structuredQuery, ReportSettings queryReportSettings )
        {
            QuerySettings querySettings;

            // Build the query engine settings
            bool secureReports;

	        if ( ! _cachedSecureReports.HasValue )
	        {
		        DatabaseConfiguration dbConfiguration;
		        dbConfiguration = ConfigurationSettings.GetDatabaseConfigurationSection( );
		        if ( dbConfiguration != null )
		        {
			        _cachedSecureReports = dbConfiguration.ConnectionSettings.SecureReports;
		        }
	        }

			secureReports = _cachedSecureReports.Value;

	        // Set the time zone for the report
            if ( queryReportSettings.Timezone != null )
            {
                structuredQuery.TimeZoneName = queryReportSettings.Timezone.StandardName;
            }

            // Update the query engine settings
            querySettings = new QuerySettings
            {
                SecureQuery = secureReports,
                SupportPaging = queryReportSettings.SupportPaging,
                FirstRow = queryReportSettings.InitialRow,
                PageSize = queryReportSettings.PageSize,
                QuickSearchTerm = queryReportSettings.QuickSearch,
                SupportQuickSearch = !string.IsNullOrWhiteSpace( queryReportSettings.QuickSearch ),
                FullAggregateClustering = true,
                RefreshCachedResult = queryReportSettings.RefreshCachedResult,
                RefreshCachedSql = queryReportSettings.RefreshCachedSql,
                CpuLimitSeconds = queryReportSettings.CpuLimitSeconds
            };

            if ( queryReportSettings.ReportOnType.HasValue )
            {
                Model.IEntity typeEntity = Model.Entity.Get<Model.EntityType>( queryReportSettings.ReportOnType );
                if ( typeEntity == null )
                {
                    throw new WebArgumentException( "Not a valid type" );
                }

                ( ( ResourceEntity ) structuredQuery.RootEntity ).EntityTypeId = queryReportSettings.ReportOnType.Value;
            }

            if ( queryReportSettings.ReportParameters != null )
            {
                // Apply any filters for analyser
                if ( queryReportSettings.ReportParameters.AnalyserConditions != null &&
					queryReportSettings.ReportParameters.AnalyserConditions.Count > 0 )
                {
                    ApplyAnalyserConditions( structuredQuery, queryReportSettings.ReportParameters.AnalyserConditions );
                }
                // Apply any filters for sorting
                if ( queryReportSettings.ReportParameters.SortColumns != null )
                {
                    ApplySortOrder( structuredQuery, queryReportSettings.ReportParameters.SortColumns );
                }
                // Determine if main row report is to be ignored
                querySettings.ResultSchemaOnly = queryReportSettings.ReportParameters.GroupAggregateRules != null && queryReportSettings.ReportParameters.GroupAggregateRules.IgnoreRows;
            }

            if ( queryReportSettings.ReportRelationship != null )
            {
                ApplyRelatedResourceCondition( structuredQuery, queryReportSettings.ReportRelationship, querySettings );
            }

            if ( queryReportSettings.RelatedEntityFilters != null )
            {
                ApplyRelatedEntityFilters( structuredQuery, queryReportSettings.RelatedEntityFilters );
            }

            if ( queryReportSettings.FilteredEntityIdentifiers != null &&
				queryReportSettings.FilteredEntityIdentifiers.Count > 0 )
            {
                ApplyFilteredEntityIdentifiers( structuredQuery, queryReportSettings.FilteredEntityIdentifiers );
            }

            PreparedQuery preparedQuery = new PreparedQuery
            {
                QuerySettings = querySettings,
                StructuredQuery = structuredQuery
            };

            return preparedQuery;
        }


        /// <summary>
        /// Generate client-aggregate information for any rollup instructions passed in the query parameters, but not otherwise
        /// part of the underlying report.
        /// </summary>
        /// <param name="groupAggregateRules">The aggregate settings.</param>
        /// <param name="query">The query.</param>
        /// <returns>A client aggregate object.</returns>
        private static ClientAggregate ApplyAdhocAggregates(ReportMetadataAggregate groupAggregateRules, StructuredQuery query)
        {
            ClientAggregate clientAggregate = new ClientAggregate();
            clientAggregate.IncludeRollup = groupAggregateRules.IncludeRollup;
			if ( groupAggregateRules.Groups != null && groupAggregateRules.Groups.Count > 0 )
            {
                clientAggregate.GroupedColumns = new List<ReportGroupField>();
                foreach (KeyValuePair<long, GroupingDetail> kvp in groupAggregateRules.Groups.SelectMany(group => group))
                {
                    long columnEntityId = kvp.Key;
                    GroupingDetail groupingDetail = kvp.Value;
                    
                    clientAggregate.GroupedColumns.Add( new ReportGroupField
                        {
                            ReportColumnId = query.SelectColumns.First(sc => sc.EntityId == columnEntityId).ColumnId,
                            GroupMethod = (GroupMethod) Enum.Parse(typeof(GroupMethod), groupingDetail.Style.Substring(5), true), // Remove 'group' from the string here
                            ReportColumnEntityId = columnEntityId,
                            ShowGrandTotals = groupAggregateRules.ShowGrandTotals,
                            ShowSubTotals = groupAggregateRules.ShowSubTotals,
                            ShowRowCounts = groupAggregateRules.ShowCount,
                            ShowRowLabels = groupAggregateRules.ShowGroupLabel,
                            ShowOptionLabel = groupAggregateRules.ShowOptionLabel
                        });
                }
            }
			if ( groupAggregateRules.Aggregates != null && groupAggregateRules.Aggregates.Count > 0 )
            {
                clientAggregate.AggregatedColumns = new List<ReportAggregateField>();
                foreach (KeyValuePair<long, List<AggregateDetail>> kvp in groupAggregateRules.Aggregates)
                {
                    long columnEntityId = kvp.Key;  // will be zero for count, which does not apply to any specific column
                    List<AggregateDetail> aggregates = kvp.Value;

                    foreach ( AggregateDetail aggregateDetail in aggregates )
                    {
                        clientAggregate.AggregatedColumns.Add(new ReportAggregateField
                        {
                            ReportColumnId = columnEntityId == 0 ? Guid.Empty : query.SelectColumns.First( sc => sc.EntityId == columnEntityId ).ColumnId,
                            AggregateMethod = (AggregateMethod)Enum.Parse(typeof(AggregateMethod), aggregateDetail.Style.Substring(3), true), // Remove 'agg' from the string here
                            ReportColumnEntityId = columnEntityId,
                            ShowGrandTotals = groupAggregateRules.ShowGrandTotals,
                            ShowSubTotals = groupAggregateRules.ShowSubTotals,
                            ShowRowCounts = groupAggregateRules.ShowCount,
                            ShowRowLabels = groupAggregateRules.ShowGroupLabel,
                            ShowOptionLabel = groupAggregateRules.ShowOptionLabel,
                            IncludedCount = columnEntityId == 0
                        });
                    }
                }
            }
            return clientAggregate;
        }


        private PreparedQuery PrepareReportRollupRun( Model.Report report, StructuredQuery structuredQuery, ReportSettings reportSettings, QuerySettings nonRollupQuerySettings )
        {
            StructuredQuery rollupQuery = null;
            ClientAggregate clientAggregate = null;
            StructuredQuery optimisedQuery;
            QuerySettings rollupSettings;
            bool adhocRollup;
            bool reportRollup;

            adhocRollup = reportSettings.ReportParameters != null && reportSettings.ReportParameters.GroupAggregateRules != null;
			reportRollup = !adhocRollup && report.ReportColumns.Any( rc => rc.ColumnRollup.Count > 0 || rc.ColumnGrouping.Count > 0 );
            
            if ( adhocRollup )
            {
                clientAggregate = ApplyAdhocAggregates( reportSettings.ReportParameters.GroupAggregateRules, structuredQuery );
            }
            else if ( reportRollup )
            {
                clientAggregate = new ClientAggregate( report, structuredQuery );
                clientAggregate.IncludeRollup = true;
            }
            else if ( report.RollupGrandTotals != null || report.RollupSubTotals != null || report.RollupOptionLabels != null )
            {
                return new PreparedQuery
                {
                    ClientAggregate = new ClientAggregate( )
                };
            }
            else
            {
                return new PreparedQuery( );
            }

            // Clone the query, so that runs and rollups won't intefere with each others caches if they mutate the structure
            // In particular, calculated columns get evaluated during execution and mutate the query .. but only if the result doesn't come from cache, but this interferes with the rollup cache key.
            // Ideally, both calculations and optimisations would be provided in layers, and both applied, and cached, before either normal or rollup executions are run.
            rollupQuery = structuredQuery.DeepCopy( );

            // A poor proxy for determining that this is not a pivot chart.
            bool isGroupedReport = !( reportSettings.ReportParameters != null &&
                reportSettings.ReportParameters.GroupAggregateRules != null &&
                reportSettings.ReportParameters.GroupAggregateRules.IgnoreRows );

            if ( isGroupedReport )
            {
                ReportRollupHelper.EnsureShowTotalsHasCount( rollupQuery, clientAggregate );
            }

            // Remove unused columns
            bool supportQuickSearch = !string.IsNullOrWhiteSpace(reportSettings.QuickSearch);
            optimisedQuery = ReportRollupHelper.RemoveUnusedColumns( rollupQuery, clientAggregate, supportQuickSearch);

            rollupSettings = new QuerySettings
                {
                    SecureQuery = nonRollupQuerySettings.SecureQuery,
                    SupportClientAggregate = true,
                    SupportPaging = false,
                    QuickSearchTerm = reportSettings.QuickSearch,
                    SupportQuickSearch = supportQuickSearch, // rollups query support quick search.   
                    ClientAggregate = clientAggregate,
                    AdditionalOrderColumns = BuildAdditionOrderColumnDictionary( optimisedQuery, clientAggregate ),
                    FullAggregateClustering = true,
                    Hint = "RptRollup-" + report.Id,
                    TargetResource = nonRollupQuerySettings.TargetResource,
                    IncludeResources = nonRollupQuerySettings.IncludeResources,
                    ExcludeResources = nonRollupQuerySettings.ExcludeResources
                };
            // Note : do not apply quick search filter to rollups (for scalability reasons)
            

            PreparedQuery preparedQuery = new PreparedQuery
            {
                ClientAggregate = clientAggregate,
                StructuredQuery = optimisedQuery,
                QuerySettings = rollupSettings
            };

            return preparedQuery;
        }


        /// <summary>
        /// Builds the addition order column dictionary.
        /// </summary>
        /// <remarks>
        /// Lifted from the report info service
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <param name="clientAggregate">The client aggregate.</param>
        /// <returns>System.Collections.Generic.Dictionary{System.Guid,EDC.ReadiNow.Metadata.Query.Structured.SelectColumn}.</returns>
        private Dictionary<Guid, SelectColumn> BuildAdditionOrderColumnDictionary(StructuredQuery query, ClientAggregate clientAggregate)
        {
            Dictionary<Guid, SelectColumn> additionalOrderColumns = new Dictionary<Guid, SelectColumn>();
            foreach (ReportAggregateField reportAggregateField in clientAggregate.AggregatedColumns)
            {
                if (reportAggregateField.AggregateMethod == AggregateMethod.Max || reportAggregateField.AggregateMethod == AggregateMethod.Min)
                {
                    SelectColumn currentSelectColumn = query.SelectColumns.FirstOrDefault(sc => sc.ColumnId == reportAggregateField.ReportColumnId);
                    if (currentSelectColumn != null && currentSelectColumn.Expression is ResourceExpression && ((ResourceExpression)currentSelectColumn.Expression).CastType is ChoiceRelationshipType)
                    {
                        //add choice field column's order column in additional order column dictionary.
                        if (!additionalOrderColumns.ContainsKey(currentSelectColumn.ColumnId))
                        {
                            SelectColumn orderColumn = new SelectColumn
                            {
                                ColumnId = Guid.NewGuid(),
                                ColumnName = "EnumOrder",
                                DisplayName = "EnumOrder",
                                Expression =
                                new ResourceDataColumn
                                {
                                    NodeId = ((EntityExpression)currentSelectColumn.Expression).NodeId,
                                    FieldId = new Model.EntityRef("core:enumOrder")
                                }
                            };
                            query.SelectColumns.Add(orderColumn);
                            additionalOrderColumns.Add(currentSelectColumn.ColumnId, orderColumn);

                        }
                    }
                    else if (currentSelectColumn != null && currentSelectColumn.Expression is AggregateExpression &&
                             ((AggregateExpression)currentSelectColumn.Expression).Expression is ResourceExpression &&
                             ((ResourceExpression)(((AggregateExpression)currentSelectColumn.Expression).Expression)).CastType is ChoiceRelationshipType)
                    {

                        //add aggregated choice field column's order column in additional order column dictionary.                             
                        if (!additionalOrderColumns.ContainsKey(currentSelectColumn.ColumnId))
                        {
                            SelectColumn orderColumn = new SelectColumn
                            {
                                ColumnId = Guid.NewGuid(),
                                ColumnName = "EnumOrder",
                                DisplayName = "EnumOrder",
                                Expression =
                                new AggregateExpression
                                {
                                    NodeId = ((AggregateExpression)currentSelectColumn.Expression).NodeId,
                                    Expression = currentSelectColumn.Expression
                                }
                            };
                            additionalOrderColumns.Add(currentSelectColumn.ColumnId, orderColumn);

                        }
                    }
                }
            }

            return additionalOrderColumns;
        }


        /// <summary>
        /// Applies the analyser conditions.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        /// <param name="selectedColumnConditions">The selected column conditions.</param>
        private static void ApplyAnalyserConditions(StructuredQuery structuredQuery, IEnumerable<SelectedColumnCondition> selectedColumnConditions)
        {
            foreach (SelectedColumnCondition columnCondition in selectedColumnConditions)
            {
                QueryCondition queryCondition = structuredQuery.Conditions.FirstOrDefault(sqc => sqc.EntityId.ToString(CultureInfo.InvariantCulture) == columnCondition.ExpressionId);

                if ( columnCondition.ExpressionId == "_id" )
                {
                    queryCondition = new QueryCondition( );
                    queryCondition.Expression = new IdExpression { NodeId = structuredQuery.RootEntity.NodeId };
                    structuredQuery.Conditions.Add( queryCondition );
                }
                if (queryCondition == null)
                {
                    // Adhoc condition for column
                    SelectColumn column = structuredQuery.SelectColumns.FirstOrDefault(sqc => sqc.EntityId.ToString(CultureInfo.InvariantCulture) == columnCondition.ExpressionId);
                    if (column == null)
                        continue;
                    queryCondition = new QueryCondition();
                    queryCondition.Argument = new TypedValue();
                    queryCondition.Argument.Type = columnCondition.Type;
                    queryCondition.Expression = new ColumnReference { ColumnId = column.ColumnId };
                    structuredQuery.Conditions.Add(queryCondition);
                }

				if ( columnCondition.EntityIdentifiers != null && columnCondition.EntityIdentifiers.Count > 0 )
                {
                    queryCondition.Operator = columnCondition.Operator;
                    long sourceId = queryCondition.Argument.SourceEntityTypeId;
                    DatabaseType type = ConditionTypeHelper.GetArgumentType(columnCondition.Operator, queryCondition.Argument.Type);
                    queryCondition.Arguments.Clear();

                    var isStructureLevelOperator = ConditionTypeHelper.IsStructureLevelOperator(queryCondition.Operator);

                    foreach (long filterEntityId in columnCondition.EntityIdentifiers)
                    {
                        object value;
                        if (isStructureLevelOperator && type is StringType)
                            value = filterEntityId.ToString();
                        else
                            value = filterEntityId;

                        queryCondition.Arguments.Add(new TypedValue
                        {
                            Type = type,
                            SourceEntityTypeId = sourceId,
                            Value = value
                        });
                    }
                }
                else 
                {
                    queryCondition.Operator = columnCondition.Operator;
                    if ( queryCondition.Argument != null )
                    {
                        if ( columnCondition.Type != null )
                        {
                            DatabaseType argType = queryCondition.Argument.Type is UnknownType ? columnCondition.Type : queryCondition.Argument.Type;
                            queryCondition.Argument.Type = ConditionTypeHelper.GetArgumentType( columnCondition.Operator, argType );
                        }
                        queryCondition.Argument.Value = columnCondition.Value;
                    }
                }
            }
        }


        /// <summary>
        /// Applies the sort order.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        /// <param name="reportSortOrders">The report sort orders.</param>
        private static void ApplySortOrder(StructuredQuery structuredQuery, IEnumerable<ReportSortOrder> reportSortOrders)
        {
            // Check that all sort orders are valid, that is the column ID GUIDs are in the report _and_ the Order is an
            // orderBy direction _or_ empty.
            OrderByDirection orderByDirection = OrderByDirection.Ascending;
            IEnumerable<ReportSortOrder> sortOrders = reportSortOrders as IList<ReportSortOrder> ?? reportSortOrders.ToList();
            if (!sortOrders.All(col => structuredQuery.SelectColumns.Any(sc => sc.EntityId.ToString(CultureInfo.InvariantCulture) == col.ColumnId)) ||
                !sortOrders.All(col => Enum.TryParse(col.Order, out orderByDirection) || col.Order == string.Empty))
            {
                throw new ArgumentOutOfRangeException("reportSortOrders");
            }

            // Clear any existing sort order clauses that may be saved with the report
            structuredQuery.OrderBy.Clear();
            foreach (ReportSortOrder sortOrder in sortOrders.Where(sortOrder => Enum.TryParse(sortOrder.Order, out orderByDirection)))
            {
                long sortEntityColumnId;
                if (!long.TryParse(sortOrder.ColumnId, out sortEntityColumnId))
                {
                    continue;
                }
                SelectColumn sortColumn = structuredQuery.SelectColumns.FirstOrDefault(sc => sc.EntityId == sortEntityColumnId);
                if (sortColumn != null && sortColumn.Expression != null && sortColumn.Expression.ExpressionId != Guid.Empty && sortColumn.ColumnId != Guid.Empty)
                {
                    structuredQuery.OrderBy.Add(new OrderByItem
                    {
                        Direction = orderByDirection,
                        Expression = new ColumnReference { EntityId = sortEntityColumnId, ExpressionId = sortColumn.Expression.ExpressionId, ColumnId = sortColumn.ColumnId }
                    });
                }
            }
        }


        /// <summary>
        /// Applies the related resource condition to the structured query.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        /// <param name="relationshipSettings">The relationship settings.</param>
        private static void ApplyRelatedResourceCondition( StructuredQuery structuredQuery, ReportRelationshipSettings relationshipSettings, QuerySettings querySettings )
        {
            RelationshipDirection direction = relationshipSettings.Direction == ReportRelationshipSettings.ReportRelationshipDirection.Forward ? RelationshipDirection.Forward : RelationshipDirection.Reverse;

            // Find existing equivalent relation.
            RelatedResource relation = (
                from related in structuredQuery.RootEntity.RelatedEntities
                let rr = related as RelatedResource
                where rr != null && rr.RelationshipTypeId.Id == relationshipSettings.RelationshipId && rr.RelationshipDirection == direction && rr.Recursive == RecursionMode.None
                select rr ).FirstOrDefault( );

            if ( relation == null )
            {
                // Add if not already found
                relation = new RelatedResource
                {
                    NodeId = Guid.NewGuid( ),
                    RelationshipDirection = direction,
                    RelationshipTypeId = new Model.EntityRef( relationshipSettings.RelationshipId ),
                    ResourceMustExist = true,
                };
                List<Entity> relatedEntities = new List<Entity>( );
                if ( structuredQuery.RootEntity.RelatedEntities != null )
                {
                    relatedEntities.AddRange( structuredQuery.RootEntity.RelatedEntities );
                }
                relatedEntities.Add( relation );
                structuredQuery.RootEntity.RelatedEntities = relatedEntities;
            }

            // Add in the unsaved changes prior to the SQL generation and report run
            if ( relationshipSettings.EntityId != 0 ||
				( relationshipSettings.IncludedEntityIds != null && relationshipSettings.IncludedEntityIds.Count > 0 ) ||
				( relationshipSettings.ExcludedEntityIds != null && relationshipSettings.ExcludedEntityIds.Count > 0 ) )
            {
                relation.FauxRelationships = new FauxRelationships
                {
                    HasTargetResource = relationshipSettings.EntityId != 0, 
                    IsTargetResourceTemporary = Model.EntityId.IsTemporary(relationshipSettings.EntityId),
					HasIncludedResources = relationshipSettings.IncludedEntityIds != null && relationshipSettings.IncludedEntityIds.Count > 0,
					HasExcludedResources = relationshipSettings.ExcludedEntityIds != null && relationshipSettings.ExcludedEntityIds.Count > 0
                };
            }

            // Set execute-time information into query settings object
            querySettings.TargetResource = relationshipSettings.EntityId;
            querySettings.IncludeResources = relationshipSettings.IncludedEntityIds?.Distinct();
            querySettings.ExcludeResources = relationshipSettings.ExcludedEntityIds?.Distinct();

            // In the event that the entity that is passed into us has yet to be actually created then it will be 0 and should not be used in the query condition.
            if (relationshipSettings.EntityId != 0)
            {
                var condition = new QueryCondition
                {
                    Expression = new IdExpression { NodeId = relation.NodeId },
                    Operator = ConditionType.Equal,
                    Parameter = "@targetResource"
                };
                List<QueryCondition> queryConditions = new List<QueryCondition>( );
                if ( structuredQuery.Conditions != null )
                {
                    queryConditions.AddRange( structuredQuery.Conditions );
                }
                queryConditions.Add( condition );
                structuredQuery.Conditions = queryConditions;
            }
        }


        /// <summary>
        /// Constrains the query by the specified relationships.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        /// <param name="reportRelationshipFilters">The report relationship filters.</param>
        private static void ApplyRelatedEntityFilters(StructuredQuery structuredQuery, IEnumerable<RelatedEntityFilterSettings> reportRelationshipFilters)
        {
            var newQueryConditions = new List<QueryCondition>();           

            foreach (RelatedEntityFilterSettings filter in reportRelationshipFilters)
            {
                // We do not have an entity value for this control
				if ( filter.RelatedEntityIds.Count <= 0 )
					continue;

                // Find existing equivalent relation
                // Note: we need to ensure that we accurately match an existing relationship if it's already there, otherwise we can't show fields on relationships within relationship tab reports properly.
                RelatedResource relation = (
                    from related in structuredQuery.RootEntity.RelatedEntities
                    let rr = related as RelatedResource
                    where rr != null &&
                          rr.RelationshipTypeId.Id == filter.RelationshipId &&
                          rr.RelationshipDirection == filter.RelationshipDirection &&
                          rr.Recursive == RecursionMode.None
                    select rr).FirstOrDefault();

                if (relation == null)
                {
                    // Add if not already found
                    relation = new RelatedResource
                    {
                        NodeId = Guid.NewGuid(),
                        RelationshipDirection = filter.RelationshipDirection,
                        RelationshipTypeId = new Model.EntityRef(filter.RelationshipId),
                        ResourceMustExist = true,

                    };

                    var relatedEntities = new List<Entity>();
                    if (structuredQuery.RootEntity.RelatedEntities != null)
                    {
                        relatedEntities.AddRange(structuredQuery.RootEntity.RelatedEntities);
                    }
                    relatedEntities.Add(relation);
                    structuredQuery.RootEntity.RelatedEntities = relatedEntities;
                }

                // Add condition                
                var condition = new QueryCondition
                {
                    Expression = new IdExpression { NodeId = relation.NodeId },
                    Operator = filter.RelatedEntityIds.Count == 1 ? ConditionType.Equal : ConditionType.AnyOf,
                    Arguments = filter.RelatedEntityIds.Select(id => new TypedValue
                    {
                        Type = new IdentifierType(),
                        Value = id
                    }).ToList()
                };

                newQueryConditions.Add(condition);
            }

			if ( newQueryConditions.Count <= 0 )
				return;

            var queryConditions = new List<QueryCondition>();
            if (structuredQuery.Conditions != null)
            {
                // Add existing conditions
                queryConditions.AddRange(structuredQuery.Conditions);
            }
            // Add new conditions
            queryConditions.AddRange(newQueryConditions);
            structuredQuery.Conditions = queryConditions;
        }


        /// <summary>
        /// Constrains the query by the filtered entity Ids.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        /// <param name="filteredEntityIds">The filtered entity Ids.</param>
        private static void ApplyFilteredEntityIdentifiers(StructuredQuery structuredQuery, List<long> filteredEntityIds)
        {
            var condition = new QueryCondition
            {
                Expression = new IdExpression { NodeId = structuredQuery.RootEntity.NodeId },
                Operator = filteredEntityIds.Count == 1 ? ConditionType.Equal : ConditionType.AnyOf,
                Arguments = filteredEntityIds.Select(id => new TypedValue
                {
                    Type = new IdentifierType(),
                    Value = id
                }).ToList()
            };

            var queryConditions = new List<QueryCondition>();
            if (structuredQuery.Conditions != null)
            {
                // Add existing conditions
                queryConditions.AddRange(structuredQuery.Conditions);
            }
            // Add new conditions

            queryConditions.Add(condition);
            structuredQuery.Conditions = queryConditions;
        }
    }
}