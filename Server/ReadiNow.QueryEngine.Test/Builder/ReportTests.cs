// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Diagnostics;
using ScriptExpression = EDC.ReadiNow.Metadata.Query.Structured.ScriptExpression;
using EDC.Database;
using EDC.ReadiNow.Core;
using ReadiNow.QueryEngine.ReportConverter;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.Builder
{
    [TestFixture]
	[RunWithTransaction]
    public class ReportTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Convert_AllFields()
        {
            IEntity entity = CodeNameResolver.GetInstance("AF_All Fields", "Report");
            var report = entity.As<EDC.ReadiNow.Model.Report>();

            using (new SecurityBypassContext())
            {
                EventLog.Application.WriteTrace("Before convert report");
                var sq = ReportToQueryConverter.Instance.Convert( report );
                EventLog.Application.WriteTrace("After convert report");
            }
        }

        [Test]
        [Explicit]  // for manual diagnostics
        [RunAsDefaultTenant]
        public void A_ClearAll()
        {
            if (ReportId == 0)
                ReportId = CodeNameResolver.GetInstance("Forms", "Report").Id;

            TestHelpers.ClearServerCaches();
        }

        [Test]
        [Explicit]  // for manual diagnostics
        [RunAsDefaultTenant]
        public void B_Clear_And_Warm()
        {
            if (ReportId == 0)
                ReportId = CodeNameResolver.GetInstance("Forms", "Report").Id;

            TestHelpers.ClearServerCaches();
            BulkPreloader.TenantWarmup();
        }
        static long ReportId;

        [Test]
        [Explicit]  // for manual diagnostics
        [RunAsDefaultTenant]
        public void C_WithoutPreload()
        {
            if (ReportId == 0)
                ReportId = CodeNameResolver.GetInstance("AF_All Fields", "Report").Id;


            using (new SecurityBypassContext())
            using (Profiler.Measure("C_WithoutPreload"))
            {
                var report = Entity.Get<Report>(ReportId);
                var sq = ReportToQueryConverter.Instance.Convert( report );
            }
        }

        [Test]
        [Explicit]  // for manual diagnostics
        [RunAsDefaultTenant]
        public void D_WithPreload()
        {
            if (ReportId == 0)
                ReportId = CodeNameResolver.GetInstance("AF_All Fields", "Report").Id;

            using (new SecurityBypassContext())
            using (Profiler.Measure("D_WithPreload"))
            {
                ReportHelpers.PreloadReport(ReportId);

                var report = Entity.Get<Report>(ReportId);
                var sq = ReportToQueryConverter.Instance.Convert( report );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void RunReportWithBrokenCalculation( )
        {
            // Build query
            ResourceEntity re = new ResourceEntity( new EntityRef( "test:person" ) );

            StructuredQuery sq = new StructuredQuery
            {
                RootEntity = re
            };
            ScriptExpression se = new ScriptExpression
            {
                Script = "BogusFieldName",
                NodeId = re.NodeId,
                ResultType = DatabaseType.StringType
            };
            sq.SelectColumns.Add( new SelectColumn { Expression = se } );

            // Run query
            QuerySettings settings = new QuerySettings();
            settings.FirstRow = 0;
            settings.PageSize = 1;
            settings.SupportPaging = true;
            settings.DebugMode = true;

            QueryResult result = Factory.QueryRunner.ExecuteQuery( sq, settings );

            // Check result
            string expectedMessage = "The name 'BogusFieldName' could not be matched on 'AA_Person'. (pos 1)";
            Assert.That( result.Columns[0].ColumnError, Is.EqualTo(expectedMessage) );
        }


        [Test]
        [RunAsDefaultTenant]
        public void RunReportWithBrokenCalculationAndCondition()
        {
            // Build query
            ResourceEntity re = new ResourceEntity(new EntityRef("test:person"));

            StructuredQuery sq = new StructuredQuery
            {
                RootEntity = re
            };
            ScriptExpression se = new ScriptExpression
            {
                Script = "BogusFieldName",
                NodeId = re.NodeId,
                ResultType = DatabaseType.StringType
            };
            var column = new SelectColumn {Expression = se};
            sq.SelectColumns.Add(column);
            sq.Conditions.Add(new QueryCondition
            {
                Expression = new ColumnReference { ColumnId = column.ColumnId },
                Operator = ConditionType.Contains,
                Argument = new TypedValue(Guid.NewGuid().ToString())                
            });

            // Run query
            QuerySettings settings = new QuerySettings();
            settings.FirstRow = 0;
            settings.PageSize = 1;
            settings.SupportPaging = true;
            settings.DebugMode = true;

            QueryResult result = Factory.QueryRunner.ExecuteQuery( sq, settings );

            // Check result
            string expectedMessage = "The name 'BogusFieldName' could not be matched on 'AA_Person'. (pos 1)";
            Assert.That(result.Columns[0].ColumnError, Is.EqualTo(expectedMessage));
        }


        [Test]
        [RunAsDefaultTenant]
        public void RunReportWithBrokenCalculationAndOrdering()
        {
            // Build query
            ResourceEntity re = new ResourceEntity(new EntityRef("test:person"));

            StructuredQuery sq = new StructuredQuery
            {
                RootEntity = re
            };
            ScriptExpression se = new ScriptExpression
            {
                Script = "BogusFieldName",
                NodeId = re.NodeId,
                ResultType = DatabaseType.StringType
            };
            var column = new SelectColumn { Expression = se };
            sq.SelectColumns.Add(column);            
            sq.OrderBy.Add(new OrderByItem
            {
                Expression = new ColumnReference { ColumnId = column.ColumnId },
                Direction = OrderByDirection.Ascending
            });

            // Run query
            QuerySettings settings = new QuerySettings();
            settings.FirstRow = 0;
            settings.PageSize = 1;
            settings.SupportPaging = true;
            settings.DebugMode = true;

            QueryResult result = Factory.QueryRunner.ExecuteQuery( sq, settings );

            // Check result
            string expectedMessage = "The name 'BogusFieldName' could not be matched on 'AA_Person'. (pos 1)";
            Assert.That(result.Columns[0].ColumnError, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCase(30, true)]
        [TestCase(0, false)]
        [RunAsDefaultTenant]
        public void RunReportWithCpuLimit(int limit, bool expectedSets)
        {
            IEntity entity = CodeNameResolver.GetInstance("AF_All Fields", "Report");
            var report = entity.As<EDC.ReadiNow.Model.Report>();

             var sq = ReportToQueryConverter.Instance.Convert(report);

            // Run query
            QuerySettings settings = new QuerySettings();
            settings.CpuLimitSeconds = limit;
            settings.RefreshCachedSql = true;
            settings.RefreshCachedResult = true;

            QueryResult result = Factory.QueryRunner.ExecuteQuery(sq, settings);

            bool foundSet = result.Sql.Contains("set QUERY_GOVERNOR_COST_LIMIT 30");
            bool foundUnset = result.Sql.Contains("set QUERY_GOVERNOR_COST_LIMIT 0");

            Assert.That(foundSet, Is.EqualTo(expectedSets), "Found the setting of the limit");
            Assert.That(foundUnset, Is.EqualTo(expectedSets), "found the unsetting of the limit");

        }
    }
}
