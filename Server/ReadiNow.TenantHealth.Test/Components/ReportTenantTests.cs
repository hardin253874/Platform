// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Metadata.Query.Structured;
using Autofac;
using System.Net;
using EDC.SoftwarePlatform.WebApi.Controllers.Report;
using ReportSettings = ReadiNow.Reporting.Request.ReportSettings;
using System.Net.Http;
using System.Threading.Tasks;
using EDC.ReadiNow.Cache;
using Jil;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.SoftwarePlatform.WebApi;
using ReadiNow.TenantHealth.Test.Infrastructure;

namespace ReadiNow.TenantHealth.Test.Components
{
    /// <summary>
    /// Test reports in all tenants.
    /// </summary>
    [TestFixture]
    public class ReportTenantTests
    {
		[TestFixtureSetUp]
        public void FigureSetup()
        {
            // Register autofac modules for the web dll.
            AppPreload.RegisterModules( );
        }

        /// <summary>
        /// CSV list of names of reports to flag as ignored
        /// </summary>
        const string ReportsToIgnore = "H_Country Report,RPT_AF_Relationships,Textbooks";

		/// <summary>
		/// Clears the selected caches.
		/// </summary>
		private void ClearSelectedCaches( )
		{
			ICacheService cacheService = Factory.QueryRunner as ICacheService;
			cacheService?.Clear( );

			cacheService = Factory.QueryRepository as ICacheService;
			cacheService?.Clear( );

			cacheService = Factory.QuerySqlBuilder as ICacheService;
			cacheService?.Clear( );

			GC.Collect( );
		}

        [Test]
        [TestCaseSource( "RunReport_GetTestData" )]
        public void RunReport( TenantInfo tenant, long reportId, string reportName )
        {
			using ( tenant.GetTenantAdminContext( ) )
			using ( new MemoryGuard( 2 * TenantHealthHelpers.OneGb, ClearSelectedCaches ) )
			using ( new MemoryGuard( 3 * TenantHealthHelpers.OneGb, TenantHealthHelpers.ClearAllCaches ) )
			using ( new MemoryLogger( new KeyValuePair<string, object>( "Tenant", tenant.TenantName ), new KeyValuePair<string, object>( "Report Id", reportId ) ) )
			{
				ReportController controller = new ReportController( );

				ReportSettings settings = new ReportSettings
				{
					RequireFullMetadata = true,
					InitialRow = 0,
					PageSize = 1,
					SupportPaging = true
				};


				HttpResponseMessage result = controller.RunReport( reportId, settings );

				if ( result == null )
					throw new Exception( "Null HttpResponseMessage" );
				if ( result.StatusCode != HttpStatusCode.OK )
					throw new Exception( $"HttpStatusCode was {result.StatusCode}" );
				if ( result.Content == null )
					throw new Exception( "Null HttpResponseMessage.Content" );

				Task<string> asyncTask = result.Content.ReadAsStringAsync( );
				asyncTask.Wait( );
				string responseString = asyncTask.Result;

				ReportResult reportResult = JSON.Deserialize<ReportResult>( responseString, JilFormatter.GetDefaultOptions( ) );

				Assert.That( reportResult, Is.Not.Null, "result" );
				Assert.That( reportResult.Metadata, Is.Not.Null, "Metadata" );
				Assert.That( reportResult.Metadata.ReportColumns, Is.Not.Null, "ReportColumns" );
				Assert.That( reportResult.Metadata.ReportColumns.Count, Is.GreaterThan( 0 ), "ReportColumns.Count" );
				Assert.That( reportResult.Metadata.InvalidReportInformation, Is.Not.Null, "InvalidReportInformation missing" );
				Assert.That( reportResult.Metadata.InvalidReportInformation [ "nodes" ], Is.Null, "Report has invalid nodes" );
				Assert.That( reportResult.Metadata.InvalidReportInformation [ "columns" ], Is.Null, "Report has invalid columns" );
				Assert.That( reportResult.Metadata.InvalidReportInformation [ "conditions" ], Is.Null, "Report has invalid conditions" );
			}
		}

        //[Test]
        //[TestCaseSource( "RunReport_GetTestData" )]
        // Requires <system.web><machinekey ...> to be present
        public void RunReport_Web( TenantInfo tenant, long reportId, string reportName )
        {
            using ( tenant.GetTenantAdminContext( ) )
            {
                string uri = $"data/v1/report/{reportId}/?metadata=full&page=0,1";
                using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post ) )
                {
                    request.PopulateBodyString( @"{""conds"":[]}" );

                    HttpWebResponse response = request.GetResponse( );
                    Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );

                    ReportResult reportResult = request.DeserialiseResponseBody<ReportResult>( );

                    Assert.That( reportResult, Is.Not.Null, "result" );
                    Assert.That( reportResult.Metadata, Is.Not.Null, "Metadata" );
                    Assert.That( reportResult.Metadata.ReportColumns, Is.Not.Null, "ReportColumns" );
                    Assert.That( reportResult.Metadata.ReportColumns.Count, Is.GreaterThan(0), "ReportColumns.Count" );
                    Assert.That( reportResult.Metadata.InvalidReportInformation, Is.Not.Null, "InvalidReportInformation missing" );
                    Assert.That( reportResult.Metadata.InvalidReportInformation [ "nodes" ], Is.Null, "Report has invalid nodes" );
                    Assert.That( reportResult.Metadata.InvalidReportInformation [ "columns" ], Is.Null, "Report has invalid columns" );
                    Assert.That( reportResult.Metadata.InvalidReportInformation [ "conditions" ], Is.Null, "Report has invalid conditions" );
                }
            }
        }

        //[Explicit]
        //[Test]
        //[TestCaseSource( "RunReport_GetTestData" )]
        public void RunQuery( TenantInfo tenant, long reportId, string reportName )
        {
            using ( tenant.GetTenantAdminContext( ) )
            {
                // Load report
                Report report = Factory.GraphEntityRepository.Get<Report>( reportId, ReportHelpers.ReportPreloaderQuery );
                Assert.That( report, Is.Not.Null, "Report entity not null" );

                // Convert to structured query
                StructuredQuery structuredQuery = Factory.ReportToQueryConverter.Convert( report );

                Assert.That( structuredQuery, Is.Not.Null, "StructuredQuery entity not null" );

                // Check for errors reported during conversionduring reporting (e.g. missing schema entities can't be found)
                if ( structuredQuery.InvalidReportInformation != null )
                {
                    foreach ( var pair in structuredQuery.InvalidReportInformation )
                    {
                        Assert.That( pair.Value, Is.Empty, "Errors detected for " + pair.Key );
                    }
                }

                // Build query
                QuerySqlBuilderSettings buildSettings = new QuerySqlBuilderSettings {
                    Hint = "ReportTenantTests",
                    SecureQuery = true,
                    SupportPaging = true };
                QueryBuild queryBuild = Factory.NonCachedQuerySqlBuilder.BuildSql( structuredQuery, buildSettings );
                Assert.That( queryBuild, Is.Not.Null, "QueryBuild entity not null" );
                Assert.That( queryBuild.Sql, Is.Not.Null, "QueryBuild.Sql entity not null" );

                // Run query
                QuerySettings runSettings = new QuerySettings
                {
                    Hint = "ReportTenantTests",
                    FirstRow = 0,
                    PageSize = 1,
                    SecureQuery = true,
                    SupportPaging = true
                };
                QueryResult result = Factory.Current.ResolveNamed<IQueryRunner>( Factory.NonCachedKey ).ExecutePrebuiltQuery( structuredQuery, runSettings, queryBuild );
                //QueryResult result = Factory.NonCachedQueryRunner.ExecutePrebuiltQuery( structuredQuery, runSettings, queryBuild );
                Assert.That( result, Is.Not.Null, "QueryResult entity not null" );
            }
        }


        /// <summary>
        /// Fetch list of all reports across all tenants.
        /// </summary>
        public IEnumerable<TestCaseData> RunReport_GetTestData( )
        {
            return TenantHealthHelpers.GetInstancesAsTestData( "core:report", ReportsToIgnore );
        }
	}
}
