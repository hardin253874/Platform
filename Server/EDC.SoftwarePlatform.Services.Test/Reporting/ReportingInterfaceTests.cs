// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Autofac;
using Moq;
using EDC.Database.Types;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using ReadiNow.Reporting;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using EDC.ReadiNow.Expressions;
using Factory = EDC.ReadiNow.Core.Factory;
using ReadiNow.QueryEngine.ReportConverter;
using Report = EDC.ReadiNow.Model.Report;
using System.Threading.Tasks;
using System.Threading;
using ReadiNow.QueryEngine.CachingBuilder;
using ReadiNow.QueryEngine.CachingRunner;
using ReadiNow.Expressions;
using ReadiNow.Reporting.Result;
using ReadiNow.Reporting.Request;
using ReadiNow.Reporting.Definitions;

namespace EDC.SoftwarePlatform.Services.Test.Reporting
{

    [TestFixture]
    public class ReportingInterfaceTests
    {

        /// <summary>
        /// Tests the report with write only field.
        /// </summary>
        /// <param name="isFieldWriteOnly">if set to <c>true</c> [is field write only].</param>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [Explicit]
        public void TestReport_BU_Test( )
        {
            ( ( CachingQuerySqlBuilder ) Factory.QuerySqlBuilder ).Clear( );
            ( ( CachingQueryRunner ) Factory.QueryRunner ).Clear( );

            long reportId = Entity.GetByName( "Business Unit Risks Copy 1" ).First( ).Id;

            Action action = ( ) =>
                {
                    using ( new SecurityBypassContext( ) )
                    {
                        var reportInterface = new ReportingInterface( );
                        ReportResult result = reportInterface.RunReport( reportId, null );
                    }
                };
            Task task1 = Task.Factory.StartNew( action );

            Task task2 = Task.Factory.StartNew( ( ) => { Thread.Sleep( 50 ); action( ); } );
            Task.WaitAll(task1, task2);
        }

        /// <summary>
        /// Tests the report runs.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestReport_AA_Manager( )
        {
            long reportId = CodeNameResolver.GetInstance( "AA_Manager", Report.Report_Type ).Single().Id;

            ReportSettings settings = new ReportSettings( );
            settings.RequireFullMetadata = true;
            settings.SupportPaging = true;
            settings.PageSize = 200;
            settings.RefreshCachedResult = true;
            settings.RefreshCachedSql = true;
            settings.RefreshCachedStructuredQuery = true;

            var reportInterface = new ReportingInterface( );
            ReportResult result = reportInterface.RunReport( reportId, settings );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.GridData, Has.Count.GreaterThan( 0 ) );
        }


        /// <summary>
        /// Tests the report with write only field.
        /// </summary>
        /// <param name="isFieldWriteOnly">if set to <c>true</c> [is field write only].</param>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestReport_AF_String( )
        {
            long reportId = Entity.GetByName( "AF_String" ).First( ).Id;

            var reportInterface = new ReportingInterface( );
            ReportResult result = reportInterface.RunReport( reportId, null );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.GridData, Has.Count.GreaterThan( 0 ) );
        }

        /// <summary>
        /// Tests that two consecutive calls to ReportInterface for the same report will only build the SQL once.
        /// </summary>
        /// <param name="isFieldWriteOnly">if set to <c>true</c> [is field write only].</param>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestReport_ReportToQueryConverter_Cached( )
        {
            long reportId = Entity.GetByName( "AF_String" ).First( ).Id;

            // Mock IReportToQueryConverter and run again
            Mock<IReportToQueryConverter> mockNonCached = new Mock<IReportToQueryConverter>( MockBehavior.Strict );
            Mock<IReportToQueryConverter> mockCached = new Mock<IReportToQueryConverter>( MockBehavior.Strict );

            using(var scope = Factory.Current.BeginLifetimeScope( builder => {
                builder.Register( ctx => mockNonCached.Object ).Keyed<IReportToQueryConverter>( Factory.NonCachedKey );
                builder.Register( ctx => mockCached.Object ).As<IReportToQueryConverter>( );
              }))
            using (Factory.SetCurrentScope(scope))
            {
                var reportInterface = new ReportingInterface( );

                IReportToQueryConverter realNonCached = new ReportToQueryConverter( );
                IReportToQueryConverter realCached = new CachingReportToQueryConverter( mockNonCached.Object );
            
                // Setup
                mockNonCached
                    .Setup( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ) )
                    .Returns( ( Report r, ReportToQueryConverterSettings s ) => realNonCached.Convert( r, s ) );

                mockCached
                    .Setup( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ) )
                    .Returns( ( Report r, ReportToQueryConverterSettings s ) => realCached.Convert( r, s ) );

                // First run
                ReportResult result = reportInterface.RunReport( reportId, null );
                Assert.That( result, Is.Not.Null );

                // Second run
                result = reportInterface.RunReport( reportId, null );
                Assert.That( result, Is.Not.Null );

                // Verify
                mockNonCached
                    .Verify( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ), Times.Exactly(1) );

                mockCached
                    .Verify( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ), Times.Exactly(2) );
            }
        }

        /// <summary>
        /// Tests that two consecutive calls to ReportInterface for the same report will only build the SQL once,
        /// even if there are faux relationship joins to different instances.
        /// </summary>
        /// <param name="isFieldWriteOnly">if set to <c>true</c> [is field write only].</param>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestReport_ReportToQueryConverter_WithFauxRelationship_Cached( )
        {
            long reportId = CodeNameResolver.GetInstance( "Template", "Report" ).Id;
            var type = Entity.GetByName<EntityType>( "AA_DogBreeds" ).First();
            long relationshipId = Factory.ScriptNameResolver.GetMemberOfType("AA_All Fields", type.Id, MemberType.Relationship).MemberId;
            long foreignId1 = CodeNameResolver.GetInstance( "Test 01", "AA_All Fields" ).Id;
            long foreignId2 = CodeNameResolver.GetInstance( "Test 02", "AA_All Fields" ).Id;

            ReportSettings settings = new ReportSettings
            {
                ReportRelationship = new ReportRelationshipSettings
                {
                    Direction = ReportRelationshipSettings.ReportRelationshipDirection.Forward,
                    RelationshipId = relationshipId
                },
                ReportOnType = type.Id
            };

            // Mock IReportToQueryConverter and run again
            Mock<IReportToQueryConverter> mockNonCached = new Mock<IReportToQueryConverter>( MockBehavior.Strict );
            Mock<IReportToQueryConverter> mockCached = new Mock<IReportToQueryConverter>( MockBehavior.Strict );

            using ( var scope = Factory.Current.BeginLifetimeScope( builder =>
            {
                builder.Register( ctx => mockNonCached.Object ).Keyed<IReportToQueryConverter>( Factory.NonCachedKey );
                builder.Register( ctx => mockCached.Object ).As<IReportToQueryConverter>( );
            } ) )
            using ( Factory.SetCurrentScope( scope ) )
            {
                var reportInterface = new ReportingInterface( );

                IReportToQueryConverter realNonCached = new ReportToQueryConverter( );
                IReportToQueryConverter realCached = new CachingReportToQueryConverter( mockNonCached.Object );

                // Setup
                mockNonCached
                    .Setup( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ) )
                    .Returns( ( Report r, ReportToQueryConverterSettings s ) => realNonCached.Convert( r, s ) );

                mockCached
                    .Setup( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ) )
                    .Returns( ( Report r, ReportToQueryConverterSettings s ) => realCached.Convert( r, s ) );

                // First run
                settings.ReportRelationship.EntityId = foreignId1;
                ReportResult result = reportInterface.RunReport( reportId, settings );
                Assert.That( result, Is.Not.Null );
                Assert.That( result.GridData [ 0 ].Values [ 0 ].Value, Is.EqualTo("Afghan Hound") );

                // Second run
                settings.ReportRelationship.EntityId = foreignId2;
                result = reportInterface.RunReport( reportId, settings );
                Assert.That( result, Is.Not.Null );
                Assert.That( result.GridData [ 0 ].Values [ 0 ].Value, Is.EqualTo( "Australian Cattle Dog" ) );

                // Verify
                mockNonCached
                    .Verify( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ), Times.Exactly( 1 ) );

                mockCached
                    .Verify( r2q => r2q.Convert( It.IsAny<Report>( ), It.IsAny<ReportToQueryConverterSettings>( ) ), Times.Exactly( 2 ) );
            }
        }

        /// <summary>
        /// Tests the report with write only field.
        /// </summary>
        /// <param name="isFieldWriteOnly">if set to <c>true</c> [is field write only].</param>
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        [Ignore]   // TODO : Fix me .. 
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestReportWithWriteOnlyField(bool isFieldWriteOnly)
        {
            try
            {
                var field = Entity.Get<Field>("test:afString", true);
                field.IsFieldWriteOnly = isFieldWriteOnly;
                field.Save();

                IEnumerable<IEntity> reports = Entity.GetByName("AF_String");

                var reportInterface = new ReportingInterface();
                ReportResult result = reportInterface.RunReport(reports.First().Id, null);

                if (isFieldWriteOnly)
                {
                    Assert.IsTrue(result.GridData.All(d => string.IsNullOrEmpty(d.Values[1].Value)), "We should not have any values");
                }
                else
                {
                    Assert.IsTrue(result.GridData.Any(d => !string.IsNullOrEmpty(d.Values[1].Value)), "We should have at least 1 value");
                }
            }
            finally
            {
                CacheManager.ClearCaches();
            }                        
        }

        /// <summary>
        /// Test for Bug 25079: Analyser: Applying an analyser to a calculated column when the report is in view mode throws a server error 'System.NullReferenceException: Object reference not set to an instance of an object.'
        /// </summary>        
        [Test]        
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestReportConditionForCalculatedFieldUnknownType_Bug25079()
        {
            using (new SecurityBypassContext())
            {
                // Get the report root type
                EntityType dogBreedsType = Entity.GetByName<EntityType>("AA_DogBreeds").FirstOrDefault();

                // Build the query and report
                var re = new ResourceEntity(dogBreedsType);

                var structuredQuery = new StructuredQuery
                {
                    RootEntity = re
                };

                // Build id column
                var idColumn = new SelectColumn
                {
                    Expression = new IdExpression
                    {
                        NodeId = re.NodeId
                    },
                    IsHidden = true,
                    DisplayName = "_id"
                };

                // Build calculated column
                var calcColumn = new SelectColumn
                {
                    Expression = new ReadiNow.Metadata.Query.Structured.ScriptExpression
                    {
                        NodeId = re.NodeId,
                        Script = @"iif(iif([Group] = 'Herding', 1, iif([Group] = 'Non-sporting', 2, iif([Group] = 'Scenthound', 3, iif([Group] = 'Terrier', 4, iif([Group] = 'toy', 5, iif([Group] = 'Working', 5, 0)))))) > 4, 'Requires attention', 'On track')",
                        ResultType = new StringType()
                    },
                    DisplayName = "Calc"
                };

                // Add columns to query
                structuredQuery.SelectColumns.Add(idColumn);
                structuredQuery.SelectColumns.Add(calcColumn);

                // Build condition on calc column
                var calcCondition = new QueryCondition
                {
                    Expression = new ColumnReference
                    {
                        ColumnId = calcColumn.ColumnId
                    },
                    Operator = ConditionType.Unspecified,
                    Argument = new TypedValue
                    {
                        Type = new UnknownType()
                    }
                };

                // Add condition to query
                structuredQuery.Conditions.Add(calcCondition);

                // Convert to report and save
                ReadiNow.Model.Report report = structuredQuery.ToReport();
                report.Name = "Test Calc Report " + Guid.NewGuid();
                report.Save();

                // Get strcutured query back so that we have entity ids
                // for structured query elements
                structuredQuery = ReportToQueryConverter.Instance.Convert( report );

                // Add the ad-hoc condition and run the report                
                var settings = new ReportSettings
                {
                    ReportParameters = new ReportParameters
                    {
                        AnalyserConditions = new List<SelectedColumnCondition>()
                    },
                    InitialRow = 0,
                    PageSize = 100
                };

                settings.ReportParameters.AnalyserConditions.Add(new SelectedColumnCondition
                {
                    Operator = ConditionType.Contains,
                    Value = "Requires attention",
                    ExpressionId = structuredQuery.Conditions[0].EntityId.ToString(CultureInfo.InvariantCulture),
                    Type = new StringType()
                });

                var reportInterface = new ReportingInterface();
                ReportResult result = reportInterface.RunReport(report, settings);

                // Verify that correct rows are returned
                Assert.IsTrue(result.GridData.All(r => r.Values[0].Value.ToLowerInvariant().Contains(settings.ReportParameters.AnalyserConditions[0].Value.ToLowerInvariant())));
            }            
        }


        /// <summary>
        /// Tests the reports report filters reports assigned to access rules.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestReportsReportFiltersReportsAssignedToAccessRules( )
        {
            using ( new SecurityBypassContext( ) )
            {
                var reportsReport = Entity.Get<ReadiNow.Model.Report>( "k:reportsReport" );

                var reportInterface = new ReportingInterface( );
                ReportResult result = reportInterface.RunReport( reportsReport, null );

                var errors = new StringBuilder( );

                foreach ( DataRow row in from row in result.GridData let report = Entity.Get<ReadiNow.Model.Report>( row.EntityId ) where report != null where report.ReportForAccessRule != null select row )
                {
                    if ( errors.Length > 0 )
                    {
                        errors.Append( "," );
                    }
                    errors.AppendFormat( "{0}", row.EntityId );
                }

                if ( errors.Length > 0 )
                {
                    errors.Insert( 0, "The following reports are assigned to access rules: " );
                    Assert.Fail( errors.ToString( ) );
                }
            }
        }


        /// <summary>
        /// Tests the reports report filters reports assigned to access rules.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_Rollup_Report_On_EditForm()
        {
            using (new SecurityBypassContext())
            {
                var report = CodeNameResolver.GetInstance( "AA_All Fields Rollup", "Report" );
                var typeId = Factory.ScriptNameResolver.GetTypeByName("AA_Herb");
                var instance = CodeNameResolver.GetInstance( "Basil", "AA_Herb");
                var relationshipId = Factory.ScriptNameResolver.GetMemberOfType("AA_All Fields", typeId, MemberType.Relationship).MemberId;

                ReportSettings settings = new ReportSettings
                {
                    ReportRelationship = new ReportRelationshipSettings
                    {
                        EntityId = instance.Id,
                        RelationshipId = relationshipId,
                        Direction = ReportRelationshipSettings.ReportRelationshipDirection.Forward
                    },
                    RequireFullMetadata = true
                };

                var reportInterface = new ReportingInterface();
                ReportResult result = reportInterface.RunReport( report.Id, null );

                Assert.That( result, Is.Not.Null );
                Assert.That( result.GridData, Has.Count.GreaterThan(0) );
                Assert.That( result.AggregateMetadata.Groups, Has.Count.EqualTo( 2 ) );
            }
        }


        /// <summary>
        /// Tests that when running a report and requesting a maximum number of columns
        /// that grouped columns are not counted.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_MaxCols_Returns_Grouped_Columns()
        {
            using (new SecurityBypassContext())
            {
                var report = Entity.GetByName<Report>("Temperature").First(r => r.InSolution != null && r.InSolution.Name == "Foster University");
              
                ReportSettings settings = new ReportSettings
                {
                   ColumnCount = 3,
                   RequireFullMetadata = true
                };

                var reportInterface = new ReportingInterface();
                ReportResult result = reportInterface.RunReport(report.Id, settings);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.GridData, Has.Count.GreaterThan(0));
                Assert.That(result.GridData[0].Values, Has.Count.EqualTo(4));
                Assert.That(result.Metadata.ReportColumns, Has.Count.EqualTo(4));                
                Assert.That(result.AggregateMetadata.Groups, Has.Count.EqualTo(1));
                Assert.That(result.AggregateMetadata.Groups[0], Has.Count.EqualTo(1));                
                long groupColumnId = result.AggregateMetadata.Groups[0].Keys.First();
                Assert.IsTrue(result.Metadata.ReportColumns.ContainsKey(groupColumnId.ToString(CultureInfo.InvariantCulture)));  
            }
        }
    }
}