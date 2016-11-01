// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.AccessControl;
using EDC.SoftwarePlatform.WebApi.Test.ImportFromExcelTest;

namespace EDC.SoftwarePlatform.WebApi.Test.Connectors
{
    [TestFixture]
    public class ConnectorImportFileTests
    {
        const string ApiKey = "a48c9559-befa-40f6-9396-41a92855bd57"; // or whatever
        string TenantName = RunAsDefaultTenant.DefaultTenantName;
        string ApiAddress = "testuploadapi";
        string EndpointAddress = "testimport";
        
        EntityType type;
        ImportConfig importConfig;
        
        List<IEntity> cleanup;

        [TestFixtureSetUp]
        public void Setup( )
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            UserAccount userAccount;
            AccessRule accessRule;
            Api api;

            // Define key and user
            using ( new TenantAdministratorContext( TenantName ) )
            {
                // Create Type
                type = new EntityType
                {
                    Fields = {
                        new StringField { Name = "Incident Description" }.As<Field>( )
                    }
                };
                type.Inherits.Add(UserResource.UserResource_Type);
                type.Save( );

                // Define access
                userAccount = new UserAccount
                {
                    Name = "Test user " + Guid.NewGuid( ),
                    AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                    Password = "HelloWorld123!@#"
                };
                userAccount.Save( );

                var field = type.Fields [ 0 ];

                // Create import config
                importConfig = new ImportConfig
                {
                    Name = "Test import config " + Guid.NewGuid( ),
                    ImportFileType_Enum = ImportFileTypeEnum_Enumeration.ImportFileTypeExcel,
                    ImportConfigMapping = new ApiResourceMapping
                    {
                        MappingSourceReference = "Incident",
                        ImportHeadingRow = 4,
                        ImportDataRow = 5,
                        MappedType = type,
                        ResourceMemberMappings =
                        {
                            new ApiFieldMapping { Name = "A", MappedField = Resource.Name_Field.As<Field>( ), }.As<ApiMemberMapping>( ),
                            new ApiFieldMapping { Name = "B", MappedField = field }.As<ApiMemberMapping>( )
                        }
                    }
                };

                // Create API
                api = new Api
                {
                    Name = "Test API " + Guid.NewGuid( ),
                    ApiAddress = ApiAddress,
                    ApiEnabled = true,
                    ApiEndpoints =
                    {
                        new ApiSpreadsheetEndpoint
                        {
                            Name = "Test spreadsheet endpoint " + Guid.NewGuid( ),
                            ApiEndpointAddress = EndpointAddress,
                            ApiEndpointEnabled = true,
                            EndpointImportConfig = importConfig
                        }.As<ApiEndpoint>( )
                    },
                    ApiKeys = {
                        new ApiKey
                        {
                            Name = ApiKey,
                            ApiKeyUserAccount = userAccount,
                            ApiKeyEnabled = true
                        }
                    }
                };
                api.Save( );

                IAccessRuleFactory accessControlHelper = new AccessRuleFactory( );
                accessRule = accessControlHelper.AddAllowCreate( userAccount.As<Subject>( ), type.As<SecurableEntity>( ) );
                //accessRule2 = accessControlHelper.AddAllowByQuery( userAccount.As<Subject>( ), type.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type ).ToReport( ) );
                

                IEntity apiKey = api.ApiKeys? [ 0 ];

                cleanup = new List<IEntity> { userAccount, api, type, accessRule, importConfig, apiKey };
            }
        }

        [TestFixtureTearDown]
        public void Teardown( )
        {
            using ( new TenantAdministratorContext( TenantName ) )
            {
                if ( cleanup != null )
                {
                    Entity.Delete( cleanup.Where( e => e != null ).Select( e => e.Id ) );
                }
            }
        }

        [Test]
        public void Test_PostCreate_OK( )
        {
            string uri = string.Format( "upload/{0}/{1}/{2}?key={3}", TenantName, ApiAddress, EndpointAddress, ApiKey );

            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post, null, true ) )
            {
                Stream testFile = ImportSpreadsheetControllerTests.GetStream( );
                request.PopulateBodyStream( testFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" );

                var response = request.GetResponse( );

                // check that it worked (200)
                Assert.AreEqual( HttpStatusCode.OK, response.StatusCode );
            }

            // Check run started
            using ( new TenantAdministratorContext( TenantName ) )
            {
                //Thread.Sleep( 1000 );
                ImportRun run = importConfig.ImportRuns.SingleOrDefault( );
                Assert.That( run, Is.Not.Null );

                // Poll progress
                int count = 25;
                while ( run.ImportRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunStarted || run.ImportRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunQueued )
                {
                    Thread.Sleep( 100 );

                    run = Entity.Get<ImportRun>( run.Id );
                    Assert.That( run, Is.Not.Null );
                    if ( count-- == 0 )
                        throw new Exception( "Timed out: " + run.ImportRunStatus_Enum );
                }
                Assert.That( run.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), run.ImportMessages );
                Assert.That( run.ImportRecordsSucceeded, Is.EqualTo( 7 ) );

                // Check instances
                var instances = Entity.GetInstancesOfType( type.Id ).ToList( );
                Assert.That( instances, Has.Count.EqualTo( 7 ) );
            }
        }

        [Test]
        public void Test_PostCreate_InvalidRequest_NoContent( )
        {
            string uri = string.Format( "upload/{0}/{1}/{2}?key={3}", TenantName, ApiAddress, EndpointAddress, ApiKey );

            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post, null, true ) )
            {
                var response = request.GetResponse( );

                // check that it worked (411)
                Assert.AreEqual( HttpStatusCode.LengthRequired, response.StatusCode );
            }
        }
    }
}
