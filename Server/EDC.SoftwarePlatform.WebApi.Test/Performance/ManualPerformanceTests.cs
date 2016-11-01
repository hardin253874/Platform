// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Services.Console;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Console;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Controllers.Report;
using EDC.SoftwarePlatform.WebApi.Test.Actions;
using EDC.SoftwarePlatform.WebApi.Test.EditForm;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using EDC.SoftwarePlatform.WebApi.Test.Report;
using NUnit.Framework;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using ReadiNow.Expressions;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.WebApi.Test.Performance
{
    // THESE TESTS ARE ALL EXPLICIT

    /// <summary>
    /// Test class for the Console Controller.
    /// </summary>
    [TestFixture]
    [Explicit]
    public class ManualPerformanceTests
    {
        #region Helpers

        /// <summary>
        /// Requests the web app domain to clear its caches.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ClearCaches( )
        {
            // Note: this WebAPI is only compiled in debug builds.
            using ( var request = new PlatformHttpRequest( "data/v1/console/clearcache", PlatformHttpMethod.Get ) )
            {
                var response = request.GetResponse( );
                Assert.AreEqual( HttpStatusCode.OK, response.StatusCode );
            }
        }

        /// <summary>
        /// Requests the web app domain to premwarm the tenant.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void PrewarmTenant( )
        {
            // Note: this WebAPI is only compiled in debug builds.
            using ( var request = new PlatformHttpRequest( "data/v1/console/prewarm", PlatformHttpMethod.Get ) )
            {
                var response = request.GetResponse( );
                Assert.AreEqual( HttpStatusCode.OK, response.StatusCode );
            }
        }

        /// <summary>
        /// Requests the web app domain to premwarm the tenant.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void Noop( )
        {
            // Note: this WebAPI is only compiled in debug builds.
            using ( var request = new PlatformHttpRequest( "data/v1/console/noop", PlatformHttpMethod.Get ) )
            {
                var response = request.GetResponse( );
                Assert.AreEqual( HttpStatusCode.OK, response.StatusCode );
            }
        }

        /// <summary>
        /// Requests the web app domain to clear its caches.
        /// Requests the web app domain to premwarm the tenant.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ClearCachesAndPrewarmTenant( )
        {
            ClearCaches( );
            PrewarmTenant( );
        }
        #endregion

        static long _reportId;
        static string _reportName = "AA_Employee"; //"Business Units (Full)";

        [Test]
        [RunAsDefaultTenant]
        public void RunReportWithMetadata()
        {
            if (_reportId == 0)
                _reportId = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance(_reportName, "Report").Id;

            using ( Profiler.Measure( "Run report test" ) )
            {
                ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full&page=0,200", _reportId), HttpStatusCode.OK);

                Assert.IsTrue(body.GridData.Count > 0, "No records returned");
                Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            }

            }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void RunReportWithMetadata_Concurrent( )
        {
            if ( _reportId == 0 )
                _reportId = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance( _reportName, "Report" ).Id;

            TestHelpers.TestConcurrent( 10, ( ) =>
            {

                ReportResult body = ReportControllerTestHelper.GetReportRequest( string.Format( @"data/v1/report/{0}?metadata=full&page=0,200", _reportId ), HttpStatusCode.OK );
            }, false, 10 );

        }

        static long _formId;
        static string _formName = "AA_Employee Form";

        [Test]
        [RunAsDefaultTenant]
        public void GetForm( )
        {
            if ( _formId == 0 )
                _formId = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance( _formName, "Custom Edit Form" ).Id;

            using ( Profiler.Measure( "Run form test" ) )
            using ( var request = new PlatformHttpRequest( string.Format( @"data/v1/form/{0}", _formId ), PlatformHttpMethod.Get ) )
            {
                HttpWebResponse response = request.GetResponse( );
                Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void GenerateForm( )
        {
            using ( Profiler.Measure( "Run form test" ) )
            using ( var request = new PlatformHttpRequest( @"data/v1/type/core/solution/generateform", PlatformHttpMethod.Get ) )
            {
                HttpWebResponse response = request.GetResponse( );
                Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void ToolboxObjects( )
        {
            // Request all definitions and enum types
            string rq = "name, inSolution.id,{ definitionUsedByReport, definitionUsedByReport.reportCharts, k:formsToEditType, k:defaultEditForm }.{name, inSolution.id}";
            string query = @"{'queries':['" + rq + @"'],'requests':[{'get':'instances','aliases':['core:definition'],'hint':'toolboxObjects-instOf-definition','rq':0}]}";
            query = query.Replace( "'", "\"" );
            JsonQueryResult res = TestEntity.RunBatchTest( query, HttpStatusCode.OK, 1 );
            Assert.That( res, Is.Not.Null );
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoggingTests( )
        {
            using ( Profiler.Measure( "Run logging test" ) )
            {
                for ( int i = 0; i < 100; i++ )
                    EventLog.Application.WriteInformation( "Hello" );
            }
        }

        static long reportId = 0;
        static long formId;
        static long typeId;
        static long tabReportId;
        static long tabRelId;
        static long tabEntityTypeId;
        static long listboxReportId;
        static long pickerTypeId;
        static long pickerReportId;

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void GrcScriptCutdown_Warmup( )
        {
             reportId = CodeNameResolver.GetInstance( "Business Units", "Report" ).Id;
             formId = CodeNameResolver.GetInstance( "Business Unit Form", "Custom Edit Form" ).Id;
             typeId = CodeNameResolver.GetInstance( "Business Unit", "Type" ).Id;
             tabReportId = CodeNameResolver.GetInstance( "Operational Impact List", "Report" ).Id;
             tabRelId = CodeNameResolver.GetInstance( "Business Function - Business Unit", "Relationship" ).Id; 
             tabEntityTypeId = CodeNameResolver.GetInstance( "Business Function", "Type" ).Id;
             listboxReportId = CodeNameResolver.GetInstance( "Division Picker", "Report" ).Id;
             pickerTypeId = CodeNameResolver.GetInstance( "Employee", "Type" ).Id;
             pickerReportId = CodeNameResolver.GetInstance( "Employee Picker 2", "Report" ).Id;
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        [TestCase( 1, 1, 0 )]
        [TestCase( 2, 1, 0 )]
        [TestCase( 50, 1, 0 )]
        [TestCase(50, 5, 0)]
        [TestCase(25, 3, 30)]
        [TestCase(50, 3, 30)]
        [TestCase(50, 3, 60)]
        [TestCase(50, 3, 240)]
        [TestCase(100, 3, 30)]
        [TestCase(100, 3, 60)]
        [TestCase(100, 3, 240)]
        [TestCase( 150, 3, 60 )]
        [TestCase( 200, 3, 60 )]
        [TestCase( 200, 3, 240 )]
        [TestCase( 300, 3, 60 )]
        [TestCase( 300, 3, 240 )]
        [TestCase( 500, 3, 240 )]
        [TestCase( 800, 3, 240 )]
        public void GrcScriptCutdown_With_Pace( int users, int repeats, int paceS )
        {
            GrcScriptCutdown(users, repeats, paceS * 1000);
        }

        public void GrcScriptCutdown( int numberOfUsers, int repeats, int paceMs = 0 )
        {
            if ( reportId == 0 )
                GrcScriptCutdown_Warmup( );

            // Run Report
            Action runReport = ( ) =>
            {
                ReportControllerTestHelper.GetReportFull( reportId );
                EntityControllerTestHelper.Query_RefreshReport( reportId );
                ActionControllerTestHelper.GetReportQuickMenu( reportId );
            };

            // Load Edit Form
            Action runEditForm = ( ) =>
            {
                FormControllerTestHelper.GetFormLayout( formId );
                EntityControllerTestHelper.Query_PendingTasks( typeId );
                ReportControllerTestHelper.GetRelationshipTabReportFull( tabReportId, tabRelId, false );
                ReportControllerTestHelper.GetEditFormListboxContents( listboxReportId );
                ActionControllerTestHelper.GetRelationshipTabQuickMenu( reportId, tabEntityTypeId );
            };

            // Show Owner picker
            Action runPicker = ( ) =>
            {
                ReportControllerTestHelper.GetReportFull( pickerReportId );
                EntityControllerTestHelper.Query_Name( pickerTypeId );
                ActionControllerTestHelper.GetPickerQuickMenu( pickerReportId, pickerTypeId );
            };

            // Save
            string createInstanceJson = GetNewBusinessUnitJson( );
            Action<long> createEntity = ( userId ) =>
            {
                var json = createInstanceJson.Replace( "99999999", userId.ToString( ) );
                json = json.Replace( "%NAME%", "BU-" + Guid.NewGuid( ).ToString( ) );
                EntityControllerTestHelper.RunSave( json );
            };

            var context = RequestContext.GetContext( );

            List<UserAccount> accounts = CreateGRCTestUsers( numberOfUsers, "BCP Manager,Compliance Manager,Risk Manager" );

            TestHelpers.TestConcurrent( numberOfUsers, ( threadNumber ) =>
            {
                var account = accounts [ threadNumber ];
                using ( new CustomContext( context ) )
                using ( new SetUser( account ) )
                {
                    runReport( );
                    runEditForm( );
                    runPicker( );
                    createEntity( account.Id );
                }
            }, true, repeats, paceMs );
        }

        private static List<UserAccount> CreateGRCTestUsers( int numberOfUsers, string roleNames )
        {
            var accounts = new List<UserAccount>( );
            bool maybePremade = true;

            var roles = roleNames.Split( ',' ).Select( roleName => Entity.GetByName<Role>( roleName ).Single( ) ).ToList( );

            for ( int i = 0; i < numberOfUsers; i++ )
            {
                string name = "NUnit-GRC-" + i;
                UserAccount userAccount = null;
                if ( maybePremade )
                {
                    userAccount = Entity.GetByName<UserAccount>( name ).FirstOrDefault( );
                    if ( userAccount == null )
                        maybePremade = false;
                }
                if ( userAccount == null )
                {
                    userAccount = new UserAccount( );
                    userAccount.Name = name;
                    userAccount.UserHasRole.AddRange( roles );
                    userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
                    userAccount.Save( );
                }
                accounts.Add( userAccount );
            }
            return accounts;
        }

        private string GetNewBusinessUnitJson( )
        {
            string fields = "";
            string rels = "";
            string entityRefs = "";

            EntityType entityType = Entity.GetByName<EntityType>( "Business Unit" ).Single( );

            Action<long> addEntityRef = ( id ) =>
            {
                entityRefs += ",\n{'id':" + id + ",'ns':null,'alias':null}";
            };
            Action<string, string, string> setField = ( field, type, value ) =>
            {
                long fieldId = Factory.ScriptNameResolver.GetMemberOfType( field, entityType.Id, MemberType.Field ).MemberId;
                fields += ",\n{'fieldId':" + fieldId + ",'typeName':'" + type + "','value':'" + value + "'}";
                addEntityRef( fieldId );
            };
            Action<string, string, string> setLookup = ( relationship, type, value ) =>
            {
                long relId = Factory.ScriptNameResolver.GetMemberOfType( relationship, entityType.Id, MemberType.Relationship ).MemberId;
                long entId = value == "%USER%" ? 99999999 : CodeNameResolver.GetInstance( value, type ).Id;
                rels += ",\n{'relTypeId':{'id':" + relId + ",'ns':null,'alias':null},'instances':[{'entity':" + entId + ",'relEntity':0,'dataState':'create'}],'removeExisting':true,'autoCardinality':true}";
                addEntityRef( relId );
                addEntityRef( entId );
            };

            long instanceId = 9007199254740986;
            addEntityRef( entityType.Id );
            addEntityRef( instanceId );

            setField( "Approval % Completed", "Decimal", "0" );
            setField( "Name", "String", "%NAME%" );
            setField( "Description", "Decimal", "TEST DESC" );
            setField( "Function Completed", "Bool", "true" );
            setField( "Approval Review Cycle", "Int32", "6" );
            setLookup( "Approval status", "Authorisation Status", "In Draft" );
            setLookup( "Owner", "Employee", "Peter Aylett" );
            setLookup( "Approval review period", "Period WMY", "months" );
            setLookup( "Owned by", "User Account", "%USER%" );

            string statement = @"{'ids':[" + instanceId + @"],
                'entities':[{'id':" + instanceId + @",'typeIds':[" + entityType.Id + @"],
                'fields':[" + fields.Substring( 1 ) + @"],
                'relationships':[" + rels.Substring( 1 ) + @"],'dataState':'create'}],
                'entityRefs':[" + entityRefs.Substring( 1 ) + @"]}";
            statement = statement.Replace( "'", @"""" );

            return statement;
        }


    }
}
