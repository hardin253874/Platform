// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.ImportFromExcelTest
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class ImportSpreadsheetControllerTests
    {
        private const string TestFileName = "TestFile.xlsx";

        private static string _fileToken;
        
        [TestFixtureSetUp]
        public static void TestClassInitialize()
        {
            RunAsTenantAttribute asTenantAttribute = new RunAsTenantAttribute("EDC");
            asTenantAttribute.BeforeTest(null);

            using ( Stream stream = GetStream( ) )
            {
                _fileToken = Factory.TemporaryFileRepository.Put( stream );
            }

            asTenantAttribute.AfterTest(null);
        }

        public static Stream GetStream( )
        {
            Assembly assembly = Assembly.GetExecutingAssembly( );
            return assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.ImportFromExcel." + TestFileName );
        }    

        /// <summary>
        /// Test 
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetSpreadsheetInfo()
        {
            // Make request
            string uri = $"data/v2/importSpreadsheet/sheet?fileId={_fileToken}&fileFormat=Excel&fileName={TestFileName}&sheet=";

            using ( var request = new PlatformHttpRequest( uri ) )
            {
                HttpWebResponse response = request.GetResponse();
                // check that it worked (200)
                Assert.That(response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );
            }
        }

        /// <summary>
        /// Test 
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetSheetInfo()
        {
            // Make request
            string uri = $"data/v2/importSpreadsheet/sample?hrow=2&drow=3&fileId={_fileToken}&fileFormat=Excel&sheet=Assembly+Point";

            using ( var request = new PlatformHttpRequest( uri ) )
            {
                HttpWebResponse response = request.GetResponse();

                // check that it worked (200)
                Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );
            }
        }

        /// <summary>
        /// Test 
        /// </summary>
        [TestCase( "Australia/Perth", 10 )]
        [TestCase( "Australia/Sydney", 8 )]
        [TestCase( null, 18 )] // utc
        [RunAsDefaultTenant]
        public void TestTimeZones( string timeZone, int expectedHour )
        {
            EntityType type = null;
            ImportConfig config = null;

            try
            {
                // Create Type
                type = new EntityType {
                    Fields = {
                        new DateTimeField { Name = "Incident Date Time" }.As<Field>( )
                    }
                };
                type.Save( );

                var field = type.Fields[ 0 ];

                // Create Configuration
                config = new ImportConfig {
                    ImportFileType_Enum = ImportFileTypeEnum_Enumeration.ImportFileTypeExcel,
                    ImportConfigMapping = new ApiResourceMapping {
                        MappingSourceReference = "Incident",
                        ImportHeadingRow = 4,
                        ImportDataRow = 5,
                        MappedType = type,
                        ResourceMemberMappings = {
                            new ApiFieldMapping { Name = "A", MappedField = Resource.Name_Field.As<Field>(), }.As<ApiMemberMapping>( ),
                            new ApiFieldMapping { Name = "C", MappedField = field }.As<ApiMemberMapping>( )
                        }
                    }
                };
                config.Save( );

                // Run upload
                long importRunId;
                var status = TestUpload( config, false, request =>
                {
                    if ( timeZone == null )
                        request.HttpWebRequest.Headers.Remove( "Tz" );
                    else
                        request.HttpWebRequest.Headers[ "Tz" ] = timeZone;
                }, out importRunId );

                Assert.That( importRunId, Is.GreaterThan( 0 ) );
                Assert.That( status.ImportStatus, Is.EqualTo( ImportStatus.Success ), status.ImportMessages );
                Assert.That( status.RecordsSucceeded, Is.EqualTo( 7 ), status.ImportMessages );
                Assert.That( status.RecordsFailed, Is.EqualTo( 0 ), status.ImportMessages );

                // Imported value was 21/09/2012 18:00
                // Perth timezone is +8
                // Expect UTC of 21/09/2012 10:00

                // Look for uploaded entity
                IEntity entity = Factory.ScriptNameResolver.GetInstance( "Power Outage", type.Id );
                Assert.That( entity, Is.Not.Null, "Find entity" );
                DateTime? value = entity.GetField<DateTime>( field.Id );
                Assert.That( value, Is.EqualTo( new DateTime( 2012, 09, 21, expectedHour, 0, 0, DateTimeKind.Utc ) ), "Check date-time value");
            }
            finally
            {
                config?.Delete( );
                type?.Delete( );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestTestRun( )
        {
            // TestRun should not save data

            EntityType type = null;
            ImportConfig config = null;

            try
            {
                // Create Type
                type = new EntityType
                {
                    Fields = {
                        new DateTimeField { Name = "Incident Date Time" }.As<Field>( )
                    }
                };
                type.Save( );

                var field = type.Fields [ 0 ];

                // Create Configuration
                config = new ImportConfig
                {
                    ImportFileType_Enum = ImportFileTypeEnum_Enumeration.ImportFileTypeExcel,
                    ImportConfigMapping = new ApiResourceMapping
                    {
                        MappingSourceReference = "Incident",
                        ImportHeadingRow = 4,
                        ImportDataRow = 5,
                        MappedType = type,
                        ResourceMemberMappings = {
                            new ApiFieldMapping { Name = "A", MappedField = Resource.Name_Field.As<Field>(), }.As<ApiMemberMapping>( ),
                            new ApiFieldMapping { Name = "C", MappedField = field }.As<ApiMemberMapping>( )
                        }
                    }
                };
                config.Save( );

                // Run upload
                long importRunId;
                const bool testRun = true;
                var status = TestUpload( config, testRun, null, out importRunId );

                Assert.That( importRunId, Is.GreaterThan( 0 ) );
                Assert.That( status.ImportStatus, Is.EqualTo( ImportStatus.Success ), status.ImportMessages );
                Assert.That( status.RecordsSucceeded, Is.EqualTo( 7 ), status.ImportMessages );
                Assert.That( status.RecordsFailed, Is.EqualTo( 0 ), status.ImportMessages );

                // Look for uploaded entity
                IEntity entity = Factory.ScriptNameResolver.GetInstance( "Power Outage", type.Id );
                Assert.That( entity, Is.Null, "Find entity" );
            }
            finally
            {
                config?.Delete( );
                type?.Delete( );
            }
        }

        private static ImportResultInfo TestUpload( ImportConfig config, bool testRun, Action<PlatformHttpRequest> callback, out long importRunId )
        {
            // ImportSpreadsheetData
            string uri = $"data/v2/importSpreadsheet/import?config={config.Id}&file={_fileToken}&filename={TestFileName}";
            if ( testRun )
                uri += "&testrun=true";

            using ( var request = new PlatformHttpRequest( uri ) )
            {
                if ( callback != null )
                    callback( request );

                HttpWebResponse response = request.GetResponse( );
                // check that it worked (200)
                Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );

                importRunId = request.DeserialiseResponseBody<long>( );
            }

            // GetImportStatus
            for (int count = 0; count < 50; count++ )
            {
                uri = $"data/v2/importSpreadsheet/import/{importRunId}";
                using ( var request = new PlatformHttpRequest( uri ) )
                {
                    HttpWebResponse response = request.GetResponse( );
                    // check that it worked (200)
                    Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );

                    ImportResultInfo status = request.DeserialiseResponseBody<ImportResultInfo>( );
                    if ( status.ImportStatus != ImportStatus.InProgress )
                        return status;
                }
                Thread.Sleep( 500 );
            }
            throw new Exception( "Took too long.." );

        }
    }
}