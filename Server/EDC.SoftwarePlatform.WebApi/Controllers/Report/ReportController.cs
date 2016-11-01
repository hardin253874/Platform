// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using EDC.Database;
using EDC.Monitoring;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Monitoring.Reports;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Utc;
using ReadiNow.Reporting;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Services.Exceptions;
using Service = ReadiNow.Reporting.Definitions;
using ServiceRequest = ReadiNow.Reporting.Request;
using ServiceResult = ReadiNow.Reporting.Result;
using ReportSettings = ReadiNow.Reporting.Request.ReportSettings;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Report controller class.
	/// </summary>
	[RoutePrefix( "data/v1/report" )]
	public class ReportController : ApiController
	{
        ReportResultCache _reportResultCache;

        public ReportController( )
        {
            _reportResultCache = Factory.Current.Resolve<ReportResultCache>( );
        }
        

        #region Report running and results

		/// <summary>
		///     Runs a report for a given report identifier and optional query string settings.
		/// </summary>
		/// <param name="rid">The rid.</param>
		/// <param name="ns">The ns.</param>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     HttpResponseMessage{ReportResult}.
		/// </returns>
		[Route( "{rid}" )]
		[Route( "{ns}/{alias}" )]
        [HttpGet]
		public HttpResponseMessage Get( string rid = null, string ns = null, string alias = null )
		{
            var queryString = Request.RequestUri.ParseQueryString();

            using ( Profiler.Measure( "ReportController.Get" ) )
            using ( var context = ReportMessageContext( "Reports", queryString ) )
            {
                context.Append( ( ) => "Report (Get)" );

                EntityRef reportRef = MakeEntityRef( rid, ns, alias );

                ReportSettings settings = SettingsFromQuery( queryString, null );
                return RunReport( reportRef.Id, settings );
			}
		}

		/// <summary>
		///     Runs a report for a given report identifier, analyser filter conditions and optional query string settings.
		/// </summary>
		/// <param name="reportParameters">The report parameters.</param>
		/// <param name="rid">The rid.</param>
		/// <param name="ns">The ns.</param>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     HttpResponseMessage{ReportResult}.
		/// </returns>
		[Route( "{rid}" )]
		[Route( "{ns}/{alias}" )]
        [HttpPost]
		public HttpResponseMessage PostReport( [FromBody] ReportParameters reportParameters, string rid = null, string ns = null, string alias = null )
		{
            var queryString = Request.RequestUri.ParseQueryString( );
            
            using ( Profiler.Measure( "ReportController.PostReport" ) )
            using ( var context = ReportMessageContext( "Reports", queryString ) )
            {
                context.Append( ( ) => "Report (Post)" );

                EntityRef reportRef = MakeEntityRef(rid, ns, alias);

                ReportSettings settings = SettingsFromQuery( queryString, reportParameters );
                return RunReport( reportRef.Id, settings );
			}
		}

		/// <summary>
		///     Runs the report entity for report.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <returns></returns>
		[Route( "builder/entity/{rid}" )]
        [HttpPost]
		public HttpResponseMessage RunReportEntityForReport( [FromBody] EntityNugget entityData )
		{
            var queryString = Request.RequestUri.ParseQueryString( );
            
            using ( Profiler.Measure( "ReportController.RunReportEntityForReport" ) )
            using ( var context = ReportMessageContext( "Reports", queryString ) )
            {
                context.Append( ( ) => "Report (Builder)" );

                // Decode settings
                ReportSettings reportSettings = SettingsFromQuery( queryString, null );
                reportSettings.UseStructuredQueryCache = false;

                // Get report from entity nugget
                IEntity reportEntity = EntityNugget.DecodeEntity( entityData );
                if ( reportEntity == null )
                {
                    throw new WebArgumentException( "Report does not exist" );
                }

                var report = ReadiNow.Model.Entity.As<ReadiNow.Model.Report>( reportEntity );
                if ( report == null )
                {
                    throw new WebArgumentException( "Report does not exist" );
                }

                // Run report
                var reportingInterface = new ReportingInterface( );

                ServiceResult.ReportResult reportResult = reportingInterface.RunReport( report, reportSettings, true );
                if (reportResult == null)
                    return new HttpResponseMessage<ReportResult>( HttpStatusCode.NotFound );

                // Pack result
                var settings = new ReportSettings
                {
                    RequireFullMetadata = true
                };

                var json = PackReportResponse( reportResult, settings );
                return JsonResponse.CreateResponse( json );
            }
		}	    

		/// <summary>
		///     Gets the name of the next available report.
		/// </summary>
		/// <param name="rid">The rid.</param>
		/// <param name="ns">The ns.</param>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		[Route( "builder/newname/{rid}" )]
		[Route( "builder/newname/{ns}/{alias}" )]
        [HttpGet]
		public HttpResponseMessage<string> GetNextAvailableReportName( string rid = null, string ns = null, string alias = null )
		{
            EntityRef reportId = MakeEntityRef(rid, ns, alias);

            var report = ReadiNow.Model.Entity.Get<ReadiNow.Model.Report>(reportId, false);

            if ( report == null )
            {
                throw new WebArgumentException( "Report does not exist" );
            }

            string availableReportName;
            string reportNameToCheck = report.Name;
            int count = 0;

            while ( true )
            {
                // Check whether the report exists by name.                    
                IEnumerable<IEntity> findResults = ReadiNow.Model.Entity.GetByName<ReadiNow.Model.Report>( reportNameToCheck );

                if ( findResults != null && findResults.Any( ) )
                {
                    count++;
                    reportNameToCheck = string.Format( "{0} Copy {1}", report.Name, count );
                }
                else
                {
                    availableReportName = reportNameToCheck;
                    break;
                }
            }

            return string.IsNullOrEmpty( availableReportName ) ? new HttpResponseMessage<string>( HttpStatusCode.NotFound ) : new HttpResponseMessage<string>( availableReportName );
        }

		#endregion Report running and results

		#region Report Running Private Methods

		/// <summary>
		///     Runs the report with the appropriate identifier and report settings.
		/// </summary>
		/// <param name="reportId">The report unique identifier.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>
		///     HttpResponseMessage{ReportResult}.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">settings</exception>
		internal HttpResponseMessage RunReport( long reportId, ReportSettings settings )
		{
			if ( settings == null )
				throw new ArgumentNullException( "settings" );

		    if ( reportId <= 0 || EntityId.IsTemporary( reportId ) )
		        throw new WebArgumentException( "reportId" );

			using ( PerformanceCounters.Measure( ReportsPerformanceCounters.CategoryName, "Run" ) )
			using ( Profiler.Measure( "ReportController.RunReport" ) )
			{
				try
				{
					using ( new EntitySnapshotContext( ) )
					{
						var reportingInterface = new ReportingInterface( );

                        // Preliminary processing of report
						ReportCompletionData reportCompletion = reportingInterface.PrepareReport( reportId, settings );

                        // Check cache, then do full processing if necessary
                        string jsonReportResult =
                            _reportResultCache.GetReportResult( reportCompletion,
                                reportResult => PackReportResponse( reportResult, settings ) );

                        return JsonResponse.CreateResponse( jsonReportResult );
						//return new HttpResponseMessage<ReportResult>( webApiReportResult );
					}
				}
				catch ( PlatformSecurityException )
				{
					throw;
				}
                catch ( TenantResourceLimitException )
                {
                    throw;
                }
                catch ( Exception ex )
				{
					string reportName;

					try
					{
						reportName = ReadiNow.Model.Entity.GetName( reportId );
					}
					catch
					{
						reportName = null;
					}

					/////
					// Only show the report name if possible.
					/////
					EventLog.Application.WriteError( $"Failed to run report {reportName} with Id '{reportId}'.\n{ex}" );

                    throw;
				}
			}
		}

        /// <summary>
        ///     Returns a message context object that will be used to capture all report generation activity.
        /// </summary>
        /// <param name="name">The report parameters.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns>
        ///     Services.Reporting.Definitions.ReportSettings.
        /// </returns>
        /// <exception cref="System.FormatException"></exception>
        internal static MessageContext ReportMessageContext( string name, NameValueCollection queryString )
        {
            // note: a key without a value will have an empty string
            bool log = queryString [ "log" ] != null;

            return new MessageContext( "Reports", log ? MessageContextBehavior.Capturing : MessageContextBehavior.Default,
                messageContext =>
                {
                    if ( log )
                    {
                        string msg = messageContext.GetMessage( );
                        EventLog.Application.WriteInformation( msg );
                    }
                });
        }

		/// <summary>
		///     Returns a report settings object from the from query string and optionally the analyser column conditions.
		/// </summary>
		/// <param name="queryString">The query string.</param>
		/// <param name="reportParameters">The report parameters.</param>
		/// <returns>
		///     Services.Reporting.Definitions.ReportSettings.
		/// </returns>
		/// <exception cref="System.FormatException"></exception>
		internal static ReportSettings SettingsFromQuery( NameValueCollection queryString, ReportParameters reportParameters )
		{
			using ( Profiler.Measure( "SettingsFromQuery" ) )
            using ( MessageContext messageContext = new MessageContext( "Reports" ) )
			{
				var settings = new ReportSettings( );
				// Parse the query string first.
				foreach ( string key in queryString.Keys )
				{
                    messageContext.Append( ( ) => string.Format( "Param {0}={1}", key, queryString [ key ] ) );

					switch ( key.ToLower( ).Trim( ) )
					{
                        case "metadata":
							string metaSwitch = queryString.Get( key ).Trim( ).ToLower( );
							if ( !string.IsNullOrEmpty( metaSwitch ) )
							{
								switch ( metaSwitch )
								{
									case "full":
										settings.RequireFullMetadata = true;
										break;
									case "basic":
										settings.RequireBasicMetadata = true;
										break;
                                    case "colbasic":
                                        settings.RequireColumnBasicMetadata = true;
								        break;
                                    case "schema":
								        settings.RequireSchemaMetadata = true;
								        break;
								}
							}
							break;

						case "page":
							string pagingValue = queryString.Get( key );
							if ( !string.IsNullOrEmpty( pagingValue ) )
							{
								string[ ] pagingElements = pagingValue.Split( ',' );
								if ( pagingElements.Length == 2 )
								{
									int start, size;
									if ( int.TryParse( pagingElements[ 0 ], out start ) &&
									     int.TryParse( pagingElements[ 1 ], out size ) )
									{
										settings.SupportPaging = true;
										settings.InitialRow = start;
										settings.PageSize = size;
									}
								}
							}
							break;

						case "cols":
							int columnCount;
							string columnCountValue = queryString.Get( key );
							if ( !string.IsNullOrEmpty( columnCountValue ) && int.TryParse( columnCountValue, out columnCount ) )
							{
								settings.ColumnCount = columnCount;
							}
							break;

						case "type":
							long reportOnType;
							string reportOnTypeValue = queryString.Get( key );
							if ( !string.IsNullOrEmpty( reportOnTypeValue ) && long.TryParse( reportOnTypeValue, out reportOnType ) )
							{
								settings.ReportOnType = reportOnType;
							}
							break;

						case "relationship":
							string relationshipValue = queryString.Get( key );
							if ( !string.IsNullOrEmpty( relationshipValue ) )
							{
								string[ ] relationshipElements = relationshipValue.Split( ',' );
								if ( relationshipElements.Length == 3 )
								{
									EntityRef entityId = WebApiHelpers.GetId( relationshipElements[ 0 ] );
									EntityRef relationshipId = WebApiHelpers.GetId( relationshipElements[ 1 ] );
									if ( entityId != null && entityId.Id > 0 &&
									     relationshipId != null && relationshipId.Id > 0 &&
									     !string.IsNullOrEmpty( relationshipElements[ 2 ] ) &&
									     ( relationshipElements[ 2 ].Trim( ).ToLower( ) == "fwd" || relationshipElements[ 2 ].Trim( ).ToLower( ) == "rev" ) )
									{
										settings.ReportRelationship = new ServiceRequest.ReportRelationshipSettings
										{
											EntityId = entityId.Id,
											RelationshipId = relationshipId.Id,
											Direction = relationshipElements[ 2 ].Trim( ).ToLower( ) == "fwd" ?
                                                ServiceRequest.ReportRelationshipSettings.ReportRelationshipDirection.Forward :
                                                ServiceRequest.ReportRelationshipSettings.ReportRelationshipDirection.Reverse
										};
									}
								}
							}
							break;

                        case "log":
                            // don't do anything .. handled elsewhere .. but don't fall through to failure either
                            break;

                        case "refreshcachedresult":
                            settings.RefreshCachedResult = true;
                            break;

                        case "refreshcachedsql":
                            settings.RefreshCachedSql = true;
                            break;

                        case "refreshcachedquery":
                            settings.RefreshCachedStructuredQuery = true;
                            break;

                        default:
							throw new WebArgumentException( string.Format( "Invalid Parameter Type {0}", key.ToLower( ).Trim( ) ) );
					}
				}

				// Parse the analyser conditions if any.
				if ( reportParameters != null )
				{
					UnpackReportParameters( reportParameters, settings );
				}

				// Time zone handling
				string timezone = HttpContext.Current.Request.Headers.Get( "Tz" );
				if ( string.IsNullOrEmpty( timezone ) )
				{
					return settings;
				}
				settings.Timezone = TimeZoneHelper.GetTimeZoneInfo( timezone );

                settings.CpuLimitSeconds = EDC.ReadiNow.Configuration.EntityWebApiSettings.Current.ReportCpuLimitSeconds;

                return settings;
			}
		}

		/// <summary>
		///     Unpacks the report parameters.
		/// </summary>
		/// <param name="reportParameters">The report parameters.</param>
		/// <param name="settings">The settings.</param>
		private static void UnpackReportParameters( ReportParameters reportParameters, ReportSettings settings )
		{
			// Parse the analyser conditions if any.
			if ( reportParameters == null )
			{
				return;
			}
			settings.ReportParameters = new ServiceRequest.ReportParameters( );
			// Check for sort operators
			if ( reportParameters.SortColumns != null )
			{
				settings.ReportParameters.SortColumns = new List<Service.ReportSortOrder>( reportParameters.SortColumns.Select( sc =>
					new Service.ReportSortOrder
					{
						ColumnId = sc.ColumnId, Order = sc.Order
					} ) );
			}
			// Check for analyser options
			if ( reportParameters.AnalyserConditions != null && reportParameters.AnalyserConditions.Count > 0 )
			{
				settings.ReportParameters.AnalyserConditions = new List<Service.SelectedColumnCondition>( reportParameters.AnalyserConditions.Select( ac => new Service.SelectedColumnCondition
				{
					ExpressionId = ac.ExpressionId,
					Operator = ToConditionType( ac.Operator ),
					Type = ac.Type != null ? DatabaseType.ConvertFromDisplayName( ac.Type ) : null,
					Value = ac.Value,
					EntityIdentifiers = ac.EntityIdentifiers != null ? new List<long>( ac.EntityIdentifiers.Keys ) : null
				} ) );
			}
			// Check for faux relationships
			if ( reportParameters.RelationshipEntities != null &&
				 ( ( reportParameters.RelationshipEntities.ExcludedEntityIdentifiers != null && reportParameters.RelationshipEntities.ExcludedEntityIdentifiers.Count > 0 ) ||
				   ( reportParameters.RelationshipEntities.IncludedEntityIdentifiers != null && reportParameters.RelationshipEntities.IncludedEntityIdentifiers.Count > 0 ) ) )
			{
				ProcessFauxRelationships( reportParameters, settings );
			}

            if (reportParameters.RelatedEntityFilters != null &&
				reportParameters.RelatedEntityFilters.Count > 0 )
            {
                ProcessRelatedEntityFilters(reportParameters, settings);
            }

			// Check for rollups
			if ( reportParameters.GroupAggregateRules != null )
			{
				ProcessGroupAggregateRules( reportParameters.GroupAggregateRules, settings );
			}
			// Check for any quick search value
			if ( !string.IsNullOrEmpty( reportParameters.QuickSearch ) )
			{
				settings.QuickSearch = reportParameters.QuickSearch;
			}
			if ( reportParameters.FilteredEntityIdentifiers != null && reportParameters.FilteredEntityIdentifiers.Count > 0 )
		    {
		        settings.FilteredEntityIdentifiers = reportParameters.FilteredEntityIdentifiers;
		    }


            //set the isReset Flag
		    settings.ReportParameters.IsReset = reportParameters.IsReset;
		}

		/// <summary>
		///     Processes the group aggregate rules.
		/// </summary>
		/// <param name="aggregateParameters">The aggregate parameters.</param>
		/// <param name="settings">The settings.</param>
		private static void ProcessGroupAggregateRules( ReportMetadataAggregate aggregateParameters, ReportSettings settings )
		{
			var aggregateDetail = new Service.ReportMetadataAggregate
			{
                IncludeRollup = aggregateParameters.IncludeRollup,
				ShowGrandTotals = aggregateParameters.ShowGrandTotals,
				ShowSubTotals = aggregateParameters.ShowSubTotals,
				ShowCount = aggregateParameters.ShowCount,
				ShowGroupLabel = aggregateParameters.ShowGroupLabel,
				ShowOptionLabel = aggregateParameters.ShowOptionLabel,
				Groups = aggregateParameters.Groups != null ? new List<Dictionary<long, Service.GroupingDetail>>( aggregateParameters.Groups.Count ) : null,
				Aggregates = aggregateParameters.Aggregates != null ? new Dictionary<long, List<Service.AggregateDetail>>( aggregateParameters.Aggregates.Count ) : null,
                IgnoreRows = aggregateParameters.IgnoreRows
			};

			if ( aggregateParameters.Groups != null )
			{
				foreach ( var groupingDetails in aggregateParameters.Groups )
				{
					var details = new Dictionary<long, Service.GroupingDetail>( groupingDetails.Count );
					foreach ( var kvp in groupingDetails )
					{
						details[ kvp.Key ] = new Service.GroupingDetail
						{
							Style = kvp.Value.Style,
							Value = kvp.Value.Value,
							Values = kvp.Value.Values != null ? new Dictionary<long, string>( kvp.Value.Values ) : null,
                            Collapsed = kvp.Value.Collapsed
						};
					}
					aggregateDetail.Groups.Add( details );
				}
			}

			if ( aggregateParameters.Aggregates != null )
			{
				foreach ( var kvp in aggregateParameters.Aggregates )
				{
					var details = new List<Service.AggregateDetail>( kvp.Value.Count );
					details.AddRange( kvp.Value.Select( detail => new Service.AggregateDetail
					{
						Style = detail.Style, Type = detail.Type
					} ) );
					aggregateDetail.Aggregates[ kvp.Key ] = details;
				}
			}

			settings.ReportParameters.GroupAggregateRules = aggregateDetail;
		}

		/// <summary>
		///     Processes the faux relationships.
		/// </summary>
		/// <param name="reportParameters">The report parameters.</param>
		/// <param name="settings">The settings.</param>
		private static void ProcessFauxRelationships( ReportParameters reportParameters, ReportSettings settings )
		{
			if ( settings.ReportRelationship != null )
			{
				settings.ReportRelationship.IncludedEntityIds =
					reportParameters.RelationshipEntities.IncludedEntityIdentifiers != null ? new List<long>( reportParameters.RelationshipEntities.IncludedEntityIdentifiers ) : null;
				settings.ReportRelationship.ExcludedEntityIds =
					reportParameters.RelationshipEntities.ExcludedEntityIdentifiers != null ? new List<long>( reportParameters.RelationshipEntities.ExcludedEntityIdentifiers ) : null;
			}
		}


        /// <summary>
        /// Processes the related entity filters.
        /// </summary>
        /// <param name="reportParameters">The report parameters.</param>
        /// <param name="settings">The settings.</param>
        private static void ProcessRelatedEntityFilters(ReportParameters reportParameters, ReportSettings settings)
        {
            if (reportParameters.RelatedEntityFilters != null)
            {
                 settings.RelatedEntityFilters = reportParameters.RelatedEntityFilters.Select(f => new ServiceRequest.RelatedEntityFilterSettings
                 {
                     RelatedEntityIds = new HashSet<long>(f.RelatedEntityIds),
                     RelationshipId = f.RelationshipId,
                     RelationshipDirection = f.RelationshipDirection
                 }).ToList();
            }
        }


		/// <summary>
		///     Automatics the type of the condition.
		/// </summary>
		/// <param name="condition">The condition.</param>
		/// <returns>
		///     ConditionType.
		/// </returns>
		private static ConditionType ToConditionType( string condition )
		{
			ConditionType conditionType;
			return Enum.TryParse( condition, out conditionType ) ? conditionType : ConditionType.Unspecified;
		}

		/// <summary>
		///     Packs the report response.
		/// </summary>
		/// <param name="result">The result returned from the reporting engine.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>
		///     ReportResult.
		/// </returns>
		private static string PackReportResponse( ServiceResult.ReportResult result, ReportSettings settings )
		{
			using ( Profiler.Measure( "PackReportResponse" ) )
			{
				// Metadata
				ReportMetadata metadata = null;
				bool getMetadata = ( ( settings.RequireBasicMetadata || settings.RequireFullMetadata ) && result.Metadata != null );
				if ( getMetadata )
				{
					using ( Profiler.Measure( "PackReportMetadata" ) )
					{
						metadata = PackReportMetadata( result.Metadata );
					}
				}

                if (settings.RequireColumnBasicMetadata && result.Metadata != null)
			    {
                    using (Profiler.Measure("PackReportColumnMetadata"))
                    {
                        metadata = PackReportColumnMetadata(result.Metadata);
                    }
			    }

				// Data
				List<DataRow> gridData;
				using ( Profiler.Measure( "PackReportGridData" ) )
				{
					gridData = PackReportGridData( result.GridData );
				}

				ReportResult reportResult = new ReportResult
				{
					Metadata = metadata,
					GridData = gridData
				};

                string json = JsonResponse.CreateJson( reportResult );
                return json;


			}
		}


        /// <summary>
        /// Packs the report column metadata.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <returns></returns>
	    private static ReportMetadata PackReportColumnMetadata( ServiceResult.ReportMetadata metadata)
	    {
            // Create the dictionary of report columns
            Dictionary<string, ReportColumn> reportColumns = null;
			if ( metadata.ReportColumns != null && metadata.ReportColumns.Count > 0 )
            {
                reportColumns = new Dictionary<string, ReportColumn>(metadata.ReportColumns.Count);
                foreach (var reportColumn in metadata.ReportColumns)
                {
                    ServiceResult.ReportColumn column = reportColumn.Value;
                    reportColumns[reportColumn.Key] =
                        new ReportColumn
                        {
                            Ordinal = column.Ordinal,
                            Title = column.Title,
                            Type = column.Type,                            
                            TypeId = column.TypeId,
                            FieldId = column.FieldId,
                            RelationshipTypeId = column.RelationshipTypeId,
                            RelationshipIsReverse = column.RelationshipIsReverse,
                            EntityNameField = column.EntityNameField,
                            IsAggregateColumn = column.IsAggregateColumn,
                            ColumnError = column.ColumnError
                        };
                }
            }

            // Return the JSON friendly metadata structure
            return new ReportMetadata
            {              
                ReportTitle = "",
                ReportStyle = "",
                ReportColumns = reportColumns
            };
	    }


	    /// <summary>
		///     Packs the report metadata.
		/// </summary>
		/// <param name="metadata">The metadata.</param>
		/// <returns>
		///     ReportMetadata.
		/// </returns>
		private static ReportMetadata PackReportMetadata( ServiceResult.ReportMetadata metadata )
		{
			// Conditional formatting types for a given type
			Dictionary<string, List<string>> conditionalFormatStyles = null;
			if ( metadata.TypeConditionalFormatStyles != null && metadata.TypeConditionalFormatStyles.Count > 0 )
			{
				conditionalFormatStyles = new Dictionary<string, List<string>>( metadata.TypeConditionalFormatStyles.Count );
				foreach ( var style in metadata.TypeConditionalFormatStyles )
				{
					conditionalFormatStyles[ style.Key ] = new List<string>( style.Value.Select( s => s.ToString( ) ) );
				}
			}

			Dictionary<long, List<ChoiceItemDefinition>> choiceSelections = null;
			if ( metadata.ChoiceSelections != null && metadata.ChoiceSelections.Count > 0 )
			{
				choiceSelections = new Dictionary<long, List<ChoiceItemDefinition>>( metadata.ChoiceSelections.Count );
				foreach ( var choiceSelection in metadata.ChoiceSelections )
				{
					choiceSelections[ choiceSelection.Key ] = new List<ChoiceItemDefinition>( choiceSelection.Value.Select( csv => new ChoiceItemDefinition
					{
						DisplayName = csv.DisplayName, EntityIdentifier = csv.EntityIdentifier
					} ) );
				}
			}
			// Create the dictionary of analyser columns
			Dictionary<string, ReportAnalyserColumn> analyserColumns = null;
			if ( metadata.AnalyserColumns != null && metadata.AnalyserColumns.Count > 0 )
			{
				analyserColumns = new Dictionary<string, ReportAnalyserColumn>( metadata.AnalyserColumns.Count );
				foreach ( var analyserColumn in metadata.AnalyserColumns )
				{
					var reportAnalyserColumn = new ReportAnalyserColumn
					{
						Ordinal = analyserColumn.Value.Ordinal,
						Title = analyserColumn.Value.Title,
						Type = analyserColumn.Value.Type.GetDisplayName( ),
						AnalyserType = analyserColumn.Value.AnalyserType,
						TypeId = analyserColumn.Value.TypeId,
                        FilteredEntityIds = analyserColumn.Value.FilteredEntityIds,
						Operator = analyserColumn.Value.Operator != null ? analyserColumn.Value.Operator.ToString( ) : null,
						DefaultOperator = analyserColumn.Value.DefaultOperator.ToString( ),
						Value = analyserColumn.Value.Value ?? null,
                        ConditionParameterPickerId = analyserColumn.Value.ConditionParameterPickerId 
					};
					if ( analyserColumn.Value.Values != null && analyserColumn.Value.Values.Count > 0 )
					{
						reportAnalyserColumn.Values = new Dictionary<long, string>( analyserColumn.Value.Values );
					}

					if ( analyserColumn.Value.ReportColumnId > 0 )
					{
						reportAnalyserColumn.ReportColumnId = analyserColumn.Value.ReportColumnId;
					}
					analyserColumns[ analyserColumn.Key ] = reportAnalyserColumn;
				}
			}
			// Create the list of conditional format rules
			Dictionary<string, ReportColumnConditionalFormat> conditionalFormatRules = null;
			if ( metadata.ConditionalFormatRules != null && metadata.ConditionalFormatRules.Count > 0 )
			{
				conditionalFormatRules = new Dictionary<string, ReportColumnConditionalFormat>( metadata.ConditionalFormatRules.Count );
				foreach ( var conditionalFormatRule in metadata.ConditionalFormatRules )
				{
					var columnFormat = new ReportColumnConditionalFormat
					{
						Style = conditionalFormatRule.Value.Style.ToString( ), ShowValue = conditionalFormatRule.Value.ShowValue
					};
					var rules = new List<ReportConditionalFormatRule>( conditionalFormatRule.Value.Rules.Count );
					rules.AddRange( conditionalFormatRule.Value.Rules.Select( conditionRule => new ReportConditionalFormatRule
					{
						Operator = conditionRule.Operator != null ? conditionRule.Operator.ToString( ) : null,
						Value = conditionRule.Value,
						Values = conditionRule.Values,
						BackgroundColor = conditionRule.BackgroundColor != null ? new ReportConditionColor
						{
							Alpha = conditionRule.BackgroundColor.Alpha,
							Blue = conditionRule.BackgroundColor.Blue,
							Green = conditionRule.BackgroundColor.Green,
							Red = conditionRule.BackgroundColor.Red
						} : null,
						ForegroundColor = conditionRule.ForegroundColor != null ? new ReportConditionColor
						{
							Alpha = conditionRule.ForegroundColor.Alpha,
							Blue = conditionRule.ForegroundColor.Blue,
							Green = conditionRule.ForegroundColor.Green,
							Red = conditionRule.ForegroundColor.Red
						} : null,
						PercentageBounds = conditionRule.PercentageBounds != null ? new ReportPercentageBounds
						{
							LowerBounds = conditionRule.PercentageBounds.LowerBounds,
							UpperBounds = conditionRule.PercentageBounds.UpperBounds
						} : null,
						ImageEntityId = conditionRule.ImageEntityId,
                        CfEntityId = conditionRule.CfEntityId,
					} ) );
					columnFormat.Rules = rules;
					conditionalFormatRules[ conditionalFormatRule.Key ] = columnFormat;
				}
			}
			Dictionary<string, ReportColumnValueFormat> valueFormatRules = null;
			if ( metadata.ValueFormatRules != null && metadata.ValueFormatRules.Count > 0 )
			{
				valueFormatRules = new Dictionary<string, ReportColumnValueFormat>( metadata.ValueFormatRules.Count );
				foreach ( var reportColumnValueFormat in metadata.ValueFormatRules )
				{
					valueFormatRules[ reportColumnValueFormat.Key ] = new ReportColumnValueFormat
					{
						HideDisplayValue = reportColumnValueFormat.Value.HideDisplayValue,
						Alignment = reportColumnValueFormat.Value.Alignment,
						Prefix = reportColumnValueFormat.Value.Prefix,
						Suffix = reportColumnValueFormat.Value.Suffix,
						DecimalPlaces = reportColumnValueFormat.Value.DecimalPlaces,
						DateTimeFormat = reportColumnValueFormat.Value.DateTimeFormat,
						NumberOfLines = reportColumnValueFormat.Value.NumberOfLines ?? 0,
						ImageScaleId = reportColumnValueFormat.Value.ImageScaleId,
						ImageSizeId = reportColumnValueFormat.Value.ImageSizeId,
						ImageHeight = reportColumnValueFormat.Value.ImageHeight,
						ImageWidth = reportColumnValueFormat.Value.ImageWidth
					};
				}
			}

			Dictionary<string, List<DateTimeValueFormat>> formatValueTypeSelectors = null;

			if ( metadata.FormatValueTypeSelectors != null && metadata.FormatValueTypeSelectors.Count > 0 )
			{
				formatValueTypeSelectors = new Dictionary<string, List<DateTimeValueFormat>>( metadata.FormatValueTypeSelectors.Count );
				foreach ( var formatValueTypeSelector in metadata.FormatValueTypeSelectors )
				{
					var enumerations = new List<DateTimeValueFormat>( formatValueTypeSelector.Value.Count );
					long ordinal = 1;
					enumerations.AddRange( formatValueTypeSelector.Value.Select( keyValuePair => new DateTimeValueFormat
					{
						Ordinal = ordinal++, EnumeratedName = keyValuePair.Key, DisplayName = keyValuePair.Value
					} ) );
					formatValueTypeSelectors[ formatValueTypeSelector.Key ] = enumerations;
				}
			}

			// Create the dictionary of report columns
			Dictionary<string, ReportColumn> reportColumns = null;
			if ( metadata.ReportColumns != null && metadata.ReportColumns.Count > 0 )
			{
				reportColumns = new Dictionary<string, ReportColumn>( metadata.ReportColumns.Count );
				foreach ( var reportColumn in metadata.ReportColumns )
				{
                    ServiceResult.ReportColumn column = reportColumn.Value;
					reportColumns[ reportColumn.Key ] =
						new ReportColumn
						{
							Ordinal = column.Ordinal,
							Title = column.Title,
							Type = column.Type,
							OperatorType = column.OperatorType,
							TypeId = column.TypeId,
							IsHidden = column.IsHidden,
                            IsAggregateColumn = column.IsAggregateColumn,
							IsRequired = column.IsRequired,
							IsReadOnly = column.IsReadOnly,
							FieldId = column.FieldId,
                            RelationshipTypeId = column.RelationshipTypeId,
                            RelationshipIsReverse = column.RelationshipIsReverse,
							DefaultValue = column.DefaultValue,
							Cardinality = column.Cardinality,
							// String parameters
							IsMultiLine = column.MultiLine,
							MinimumLength = column.MinimumLength,
							MaximumLength = column.MaximumLength,
							RegularExpression = column.RegularExpression,
							RegularExpressionError = column.RegularExpressionErrorMessage,
							// Decimal Parameters
							MinimumDecimal = column.MinimumDecimal,
							MaximumDecimal = column.MaximumDecimal,
							DecimalPlaces = column.DecimalPlaces,
							// Date/Date-Time/Time Parameters
							MinimumDate = column.MinimumDate,
							MaximumDate = column.MaximumDate,
							AutoNumberDisplayPattern = column.AutoNumberDisplayPattern,
							EntityNameField = column.EntityNameField,
                            ColumnError = column.ColumnError
						};
				}
			}

			////
			// Handle the rollup stuff
			////

			// Metadata
			ReportMetadataAggregate aggregateMetadata = null;
			if ( metadata.AggregateMetadata != null )
			{
				aggregateMetadata = new ReportMetadataAggregate
				{
                    IncludeRollup = metadata.AggregateMetadata.IncludeRollup,
					ShowGrandTotals = metadata.AggregateMetadata.ShowGrandTotals,
					ShowSubTotals = metadata.AggregateMetadata.ShowSubTotals,
					ShowCount = metadata.AggregateMetadata.ShowCount,
					ShowGroupLabel = metadata.AggregateMetadata.ShowGroupLabel,
					ShowOptionLabel = metadata.AggregateMetadata.ShowOptionLabel,
				};
				if ( metadata.AggregateMetadata.Groups != null && metadata.AggregateMetadata.Groups.Count > 0 )
				{
					aggregateMetadata.Groups = new List<Dictionary<long, GroupingDetail>>( );
					foreach ( var groups in metadata.AggregateMetadata.Groups )
					{
						var group = new Dictionary<long, GroupingDetail>( );
						foreach ( var kvp in groups )
						{
							group[ kvp.Key ] = new GroupingDetail
							{
								Style = kvp.Value.Style,
								Value = kvp.Value.Value = kvp.Value.Value,
								Values = kvp.Value.Values != null && kvp.Value.Values.Count > 0 ? new Dictionary<long, string>( kvp.Value.Values ) : null,
                                Collapsed = kvp.Value.Collapsed
							};
						}
						aggregateMetadata.Groups.Add( group );
					}
				}
				if ( metadata.AggregateMetadata.Aggregates != null && metadata.AggregateMetadata.Aggregates.Count > 0 )
				{
					aggregateMetadata.Aggregates = new Dictionary<long, List<AggregateDetail>>( metadata.AggregateMetadata.Aggregates.Count );
					foreach ( var keyValuePair in metadata.AggregateMetadata.Aggregates )
					{
						aggregateMetadata.Aggregates[ keyValuePair.Key ] = new List<AggregateDetail>( keyValuePair.Value.Select( ad => new AggregateDetail
						{
							Style = ad.Style, Type = ad.Type
						} ) );
					}
				}
			}

			// Data
			List<ReportDataAggregate> aggregateData = null;
			if ( metadata.AggregateData != null && metadata.AggregateData.Count > 0 )
			{
				aggregateData = new List<ReportDataAggregate>( metadata.AggregateData.Count );
				foreach ( ServiceResult.ReportDataAggregate reportDataAggregate in metadata.AggregateData )
				{
					// Populate groups
					List<Dictionary<long, CellValue>> groupHeadings = null;
					if ( reportDataAggregate.GroupHeadings != null && reportDataAggregate.GroupHeadings.Count > 0 )
					{
						groupHeadings = new List<Dictionary<long, CellValue>>( reportDataAggregate.GroupHeadings.Count );
						foreach ( var groupHeading in reportDataAggregate.GroupHeadings )
						{
							var groupValues = new Dictionary<long, CellValue>( groupHeading.Count );
							foreach ( var pair in groupHeading )
							{
								groupValues[ pair.Key ] = new CellValue
								{
									Value = pair.Value.Value,
									Values = pair.Value.Values != null && pair.Value.Values.Count > 0 ? new Dictionary<long, string>( pair.Value.Values ) : null,
									ConditionalFormatIndex = pair.Value.ConditionalFormatIndex
								};
							}
							groupHeadings.Add( groupValues );
						}
					}
					// Populate rollups
					Dictionary<long, List<AggregateItem>> aggregates = null;
					if ( reportDataAggregate.Aggregates != null && reportDataAggregate.Aggregates.Count > 0 )
					{
						aggregates = new Dictionary<long, List<AggregateItem>>( reportDataAggregate.Aggregates.Count );
						foreach ( var aggregate in reportDataAggregate.Aggregates )
						{
							if ( aggregate.Value != null && aggregate.Value.Count > 0 )
							{
								aggregates[ aggregate.Key ] = new List<AggregateItem>( aggregate.Value.Select( ai => new AggregateItem
								{
									AggregateValue = ai.Value,
									AggregateValues = ai.Values != null && ai.Values.Count > 0 ? new Dictionary<long, string>( ai.Values ) : null
								} ) );
							}
						}
					}
					// Populate Aggregate data
					aggregateData.Add( new ReportDataAggregate
					{
						Total = reportDataAggregate.Total,
						GroupBitmap = reportDataAggregate.GroupBitmap,
						GroupHeadings = groupHeadings,
						Aggregates = aggregates
					} );
				}
			}


			List<ReportSortOrder> sortOrder = metadata.SortOrders == null ? null : new List<ReportSortOrder>( metadata.SortOrders.Select( so => new ReportSortOrder
			{
				ColumnId = so.ColumnId, Order = so.Order
			} ) );
			// Return the JSON friendly metadata structure
			return new ReportMetadata
			{
				ReportTitle = metadata.ReportTitle,
				HideAddButton = metadata.HideAddButton,
				HideNewButton = metadata.HideNewButton,
				HideDeleteButton = metadata.HideDeleteButton,
				HideActionBar = metadata.HideActionBar,
				HideReportHeader = metadata.HideReportHeader,
				ReportStyle = metadata.ReportStyle,
				DefaultFormId = metadata.DefaultFormId ?? 0,
                ResourceViewerFormId = metadata.ResourceViewerFormId ?? 0,
                TypeConditionalFormatStyles = conditionalFormatStyles,
				AnalyserColumns = analyserColumns,
				ConditionalFormatRules = conditionalFormatRules,
				ValueFormatRules = valueFormatRules,
				FormatValueTypeSelectors = formatValueTypeSelectors,
				ReportColumns = reportColumns,
				SortOrders = sortOrder,
				ChoiceSelections = choiceSelections,
				InlineReportPickers = metadata.InlineReportPickers != null && metadata.InlineReportPickers.Count > 0 ? new Dictionary<long, long>( metadata.InlineReportPickers ) : null,
				AggregateMetadata = aggregateMetadata,
				AggregateData = aggregateData,
                InvalidReportInformation = metadata.InvalidReportInformation,
                Modified = metadata.Modified
			};
		}

		/// <summary>
		///     Packs the report grid data.
		/// </summary>
		/// <param name="gridData">The grid data.</param>
		/// <returns>
		///     List{DataRow}.
		/// </returns>
		private static List<DataRow> PackReportGridData( List<ServiceResult.DataRow> gridData )
		{
			if ( gridData == null || gridData.Count == 0 )
			{
				return null;
			}
			return new List<DataRow>( gridData.Select( gd => new DataRow
			{
				EntityId = gd.EntityId,
				Values = new List<CellValue>( gd.Values.Select( cv => new CellValue
				{
					Value = cv.Value,
					Values = cv.Values != null ? new Dictionary<long, string>( cv.Values ) : null, ConditionalFormatIndex = cv.ConditionalFormatIndex
				} ).ToList( ) )
			} ) );
		}

        /// <summary>
        ///     Create an EntityRef object from an id or NS and alias.
        ///     Either the ID can be passed, or the alias can be passed (in which case the NS is optional).
        /// </summary>
        public static EntityRef MakeEntityRef( string id, string ns, string alias )
        {
            using ( MessageContext messageContext = new MessageContext( "Reports" ) )
            {
                EntityRef result = WebApiHelpers.MakeEntityRef( id, ns, alias );

                // Make me nicer
                messageContext.Append( ( ) =>
                {
                    try
                    {
                        long entityId = result.Id;
                        string msg = string.Format( "Name: {0} (Type: {0})",
                            EDC.ReadiNow.Model.Entity.GetName( entityId ),
                            EDC.ReadiNow.Model.Entity.Get( entityId ).TypeIds.First( ) );
                        return msg;
                    }
                    catch
                    {
                        return "Invalid ID/alias";
                    }
                } );

                return result;
            }
        }

		#endregion Report Running Private Methods
	}
}