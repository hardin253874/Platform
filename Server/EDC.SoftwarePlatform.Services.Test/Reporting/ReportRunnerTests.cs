// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;
using EntityModel = EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics;
using System.Data;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Core.Cache;
using ReadiNow.Reporting;
using ReadiNow.Reporting.Request;

namespace EDC.SoftwarePlatform.Services.Test.Reporting
{
    [TestFixture]
    public class ReportRunnerTests
    {
        static Dictionary<string, long> reportLookup = new Dictionary<string, long>();
        static List<TestCaseData> testCaseData;

        /// <summary>
        /// Build list of reports to test.
        /// </summary>
        /// <remarks>
        /// Note: We're not passing the reportId through the params because we don't want the test-case name to vary from build-to-build, confusing TeamCity.
        /// </remarks>
        public static IEnumerable<TestCaseData> GetReports()
        {
            var includeInFastTests = new List<string> { "AA_All Fields", "AA_Employee", "AF_All Fields" };
            var skipReports = new List<string> { "Drinks" }; // TODO: Repair the Drinks report.

            if (testCaseData != null)
                return testCaseData;

            try
            {
				const string sql = @"select Id,name from _vReport where TenantId = (select Id from _vTenant where name='EDC') order by name";
                List<TestCaseData> res = new List<TestCaseData>();

                using (DatabaseContext ctx = DatabaseContext.GetContext())
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        string name = reader.GetString(1);

                        if (skipReports.Contains(name))
                            continue;   

                        if (reportLookup.ContainsKey(name))
                        {
                            EventLog.Application.WriteWarning("Multiple reports named: " + name);
                        }
                        else
                        {
                            reportLookup[name] = id;

                            var testCase = new TestCaseData(name);
                            if (!includeInFastTests.Contains(name))
                                testCase = testCase.SetCategory("ExtendedTests");
                            res.Add(testCase);
                        }
                    }
                }

                testCaseData = res;
                return res;
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError(ex.ToString());
                throw;
            }
        }

        [Test, TestCaseSource("GetReports")]
        [RunAsDefaultTenant]
        public void ReportingInterface_RunReport_NoSecurity(string reportName)
        {
            BulkPreloader.TenantWarmupIfNotWarm();

            long reportId = reportLookup[reportName];

            using (new SecurityBypassContext())
            using (CacheManager.EnforceCacheHitRules())
            //using (Profiler.Measure("Run report test"))
            {
                ReportingInterface ri = new ReportingInterface();
                ri.RunReport(reportId, new ReportSettings()
                {
                    InitialRow = 0,
                    PageSize = 1,
                    RequireFullMetadata = true,
                    RequireBasicMetadata = true
                });
            }
        }

        [Test, TestCaseSource("GetReports")]
        [RunAsDefaultTenant]
        public void ReportingInterface_RunReport_WithSecurity(string reportName)
        {
            BulkPreloader.TenantWarmupIfNotWarm();
            
            long reportId = reportLookup[reportName];

            using (CacheManager.EnforceCacheHitRules())
            //using (Profiler.Measure("Run report test"))
            {
                ReportingInterface ri = new ReportingInterface();
                ri.RunReport(reportId, new ReportSettings
                {
                    InitialRow = 0,
                    PageSize = 1,
                    RequireFullMetadata = true,
                    RequireBasicMetadata = true
                });
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Check_StructuredQuery_Invalidation_On_ReportInterface( )
        {
            using ( CacheManager.EnforceCacheHitRules( false ) )
            {
                Report report;
                ResourceReportNode rootNode;
                ResourceArgument exprType;

                // Create report
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    report = new EntityModel.Report();
                    rootNode = new ResourceReportNode();
                    var reportType = EntityModel.Entity.Get<EntityType>("test:person"); ;
                    rootNode.ResourceReportNodeType = reportType;
                    report.RootNode = rootNode.As<ReportNode>();
                    var column = new EntityModel.ReportColumn( );
                    exprType = new ResourceArgument();
                    exprType.ConformsToType = reportType;
                    var expr = new EntityModel.ResourceExpression();
                    expr.ReportExpressionResultType = exprType.As<ActivityArgument>();
                    expr.SourceNode = rootNode.As<ReportNode>();
                    column.ColumnExpression = expr.As<ReportExpression>();
                    report.ReportColumns.Add(column);
                    report.Save();
                    ctx.CommitTransaction();
                }

                ReportingInterface ri = new ReportingInterface( );
                var settings = new ReportSettings
                {
                    InitialRow = 0,
                    PageSize = 1,
                    RequireFullMetadata = true,
                    RequireBasicMetadata = true
                };

                // Run and expect 1 column
                var result = ri.RunReport( report.Id, settings );
                Assert.AreEqual( 1, result.ReportQueryColumns.Count );

                // Add column
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    var column2 = new EntityModel.ReportColumn();
                    var expr2 = new EntityModel.ResourceExpression();
                    expr2.SourceNode = rootNode.As<ReportNode>();
                    expr2.ReportExpressionResultType = exprType.As<ActivityArgument>();
                    column2.ColumnExpression = expr2.As<ReportExpression>();
                    report.ReportColumns.Add(column2);
                    report.Save();
                    ctx.CommitTransaction();
                }

                // Run and expect 2 columns
                result = ri.RunReport( report.Id, settings );
                Assert.AreEqual( 2, result.ReportQueryColumns.Count );
            }
        }        

    }
}
