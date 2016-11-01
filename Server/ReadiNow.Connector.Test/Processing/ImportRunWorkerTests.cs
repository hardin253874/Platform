// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extras.Moq;
using EDC.IO;
using EDC.ReadiNow.Core;
using NUnit.Framework;
using ReadiNow.Connector.Processing;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using ReadiNow.Connector.ImportSpreadsheet;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Spreadsheet;
using ReadiNow.Connector.Test.Spreadsheet;
using EDC.ReadiNow.Common.Workflow;

namespace ReadiNow.Connector.Test.Processing
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    class ImportRunWorkerTests
    {
        const string MockToken = "SampleDataTests.xlsx";
        const long MockRunId = 1;


        [Test]
        [RunAsDefaultTenant]
        public void Test_Constructor( )
        {
            using (var mock = GetMock())
            {
                ImportRunWorker runner = mock.Create<ImportRunWorker>();
                Assert.That(runner, Is.Not.Null);
            }
        }

        [TestCase( 1 )]
        [TestCase( 2 )]
        [TestCase( 3 )]
        [TestCase( 4 )]
        [TestCase( 5 )]
        [RunAsDefaultTenant]
        public void Test_Constructor_Null_Argument( int arg )
        {
            IEntityRepository entityRepository = arg == 1 ? null : new Mock<IEntityRepository>( ).Object;
            IFileRepository fileRepository = arg == 2 ? null : new Mock<IFileRepository>( ).Object;
            RecordImporter.Factory recordImporterActivator = (readerToEntityAdapter, importReporter, mergeExisting, testRun) => new Mock<IRecordImporter>().Object;
            if ( arg == 3 ) recordImporterActivator = null;
            Func<ImportFormat, IDataFileReaderService> readerActivator = arg == 4
                ? null
                : ( Func<ImportFormat, IDataFileReaderService> ) ( impFormat => null );
            IReaderToEntityAdapterProvider readerToEntityAdapterProvider = arg == 5 ? null : new Mock<IReaderToEntityAdapterProvider>( ).Object;

            Assert.Throws<ArgumentNullException>( ( ) => new ImportRunWorker( entityRepository, fileRepository, recordImporterActivator, readerActivator, readerToEntityAdapterProvider ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_AlreadyStarted( )
        {
            using ( var mock = GetMock( ) )
            {
                GetImportRun( mock ).ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunStarted;

                Assert.Throws<Exception>( ( ) => RunAndAssertFailure( mock, null),
                    "Import run is not marked as queued, or has already been started." );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_NoImportConfig( )
        {
            using ( var mock = GetMock( ) )
            {
                GetImportRun( mock ).ImportConfigUsed = null;

                RunAndAssertFailure( mock, "Import configuration could not be loaded." );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_In_ReaderActivation( )
        {
            Func<ImportFormat, IDataFileReaderService> readerActivator = impFormat =>
            {
                if ( impFormat != ImportFormat.Excel )
                    Assert.Fail( "Expected activation request for Excel" );
                throw new Exception( "My error" );
            };
            
            using ( var mock = GetMock( ) )
            {
                mock.Provide(readerActivator);

                RunAndAssertFailure( mock, "My error" );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_ImportFileId_Not_Set( )
        {
            using ( var mock = GetMock( ) )
            {
                GetImportRun( mock ).ImportFileId = null;

                RunAndAssertFailure( mock, "File handle not set" );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_FileFailedToLoad( )
        {
            using (var mock = GetMock())
            {
                mock.Mock<IFileRepository>()
                    .Setup( f => f.Get( MockToken ) )
                    .Throws( new System.IO.FileNotFoundException( "My file error." ) );

                RunAndAssertFailure( mock, "Could not retrieve file. My file error." );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_NoResourceMapping( )
        {
            using ( var mock = GetMock( ) )
            {
                GetImportRun( mock ).ImportConfigUsed.ImportConfigMapping = null;

                RunAndAssertFailure( mock, "Import configuration has no mapping." );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_NoRecordsToImport( )
        {
            using ( var mock = GetMock( ) )
            {
                GetImportRun( mock ).ImportConfigUsed.ImportConfigMapping.ImportDataRow = 100;  // start beyond data

                RunAndAssertFailure( mock, "No records were found to be imported." );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Failure_NoRecordsSucceeded( )
        {
            using ( var mock = GetMock( ) )
            {
                // Create an int field and write string data to it.
                Field field = new IntField( ).AsWritable<Field>( );
                field.Name = "Field2";
                field.IsRequired = true;
                ApiFieldMapping mappedField = new ApiFieldMapping( );
                mappedField.MappedField = field;
                mappedField.Name = "A"; // text column on Test3 sheet

                var importConfig = GetImportRun( mock ).ImportConfigUsed;
                importConfig.ImportConfigMapping.MappingSourceReference = "Test3";
                importConfig.ImportConfigMapping.ImportDataRow = 16; // only ask for two rows
                var mapping = importConfig.ImportConfigMapping;
                mapping.MappedType.Fields.Add( field );
                mapping.ResourceMemberMappings.Clear( );
                mapping.ResourceMemberMappings.Add( mappedField.As<ApiMemberMapping>( ) );
                GetImportRun( mock ).Save( );

                RunAndAssertFailure( mock, "No records were successfully imported.\r\nRow 16: Value for 'Field2' was formatted incorrectly.\r\nRow 17: Value for 'Field2' was formatted incorrectly." );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Run( )
        {
            using ( var mock = GetMock( ) )
            {
                ImportRunWorker worker = mock.Create<ImportRunWorker>( );
                var importRun = GetImportRun( mock );

                Field field = new StringField( ).AsWritable<Field>( );
                field.Name = "Field1";

                EntityType type = new EntityType( );
                type.Fields.Add( field );

                ApiFieldMapping fieldMapping = new ApiFieldMapping( );
                fieldMapping.MappedField = field;
                fieldMapping.Name = "NotAValidExcelColumnReference";

                ApiResourceMapping mapping = importRun.ImportConfigUsed.ImportConfigMapping;
                mapping.ResourceMemberMappings.Add( fieldMapping.As<ApiMemberMapping>( ) );
                mapping.MappedType = type;
                importRun.Save( );

                worker.StartImport( MockRunId );

                Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ) );
                Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 4 ) );
                Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 4 ) );
                Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 0 ) );
                Assert.That( importRun.ImportRunFinished, Is.Not.Null );
            }
        }

        [TestCase( true )]
        [TestCase( false )]
        [RunAsDefaultTenant]
        public void Test_SuppressWorkflow( bool suppressWorkflows )
        {
            bool called = false;

            using ( var mock = GetMock( ) )
            {
                var mockSaver = new Mock<IEntitySaver>( MockBehavior.Strict );
                RecordImporter.Factory factory = ( readerToEntityAdapter, importReporter, resourceMapping, testRun ) =>
                    new RecordImporter(
                        readerToEntityAdapter,
                        mockSaver.Object,
                        importReporter, resourceMapping, testRun );

                mock.Provide( factory );
                mock.Provide( mockSaver );

                mockSaver.Setup( m => m.SaveEntities( It.IsAny<IEnumerable<IEntity>>( ) ) )
                    .Callback( ( ) =>
                    {
                        called = true;
                        Assert.That( WorkflowRunContext.Current.DisableTriggers, Is.EqualTo( suppressWorkflows ), "Suppress" );
                    }
                );


                ImportRunWorker worker = mock.Create<ImportRunWorker>( );
                var importRun = GetImportRun( mock );
                importRun.ImportConfigUsed.ImportConfigMapping.MappingSuppressWorkflows = suppressWorkflows;
                importRun.Save( );

                worker.StartImport( MockRunId );

                Assert.That( called, Is.True, "Called" );
            }
        }

        [TestCase( true )]
        [TestCase( false )]
        [RunAsDefaultTenant]
        public void Test_TestRun( bool testRun )
        {
            using ( var mock = GetMock( ) )
            {
                var mockSaver = new Mock<IEntitySaver>( MockBehavior.Strict );
                RecordImporter.Factory factory = ( readerToEntityAdapter, importReporter, resourceMapping, testRun1 ) =>
                    new RecordImporter(
                        readerToEntityAdapter,
                        mockSaver.Object,
                        importReporter, resourceMapping, testRun1 );

                mock.Provide( factory );
                mock.Provide( mockSaver );

                bool called = false;
                mockSaver.Setup( m => m.SaveEntities( It.IsAny<IEnumerable<IEntity>>( ) ) )
                    .Callback( ( ) => { called = true; } );

                ImportRunWorker worker = mock.Create<ImportRunWorker>( );
                var importRun = GetImportRun( mock );
                importRun.ImportTestRun = testRun;
                importRun.Save( );

                worker.StartImport( MockRunId );
                
                Assert.That( called, Is.EqualTo( !testRun ) );
            }
        }

        public AutoMock GetMock( )
        {
            ImportRun importRun = GetMockRun(MockToken);

            AutoMock mock = AutoMock.GetStrict();
            mock.Provide(importRun);
            mock.Provide(GetMockFileRepository(MockToken));
            mock.Provide(GetMockEntityRepository(MockRunId, importRun));
            
            // OK, that's enough mocking..
            mock.Provide( Factory.Current.Resolve<IReaderToEntityAdapterProvider>() );
            mock.Provide( Factory.Current.Resolve<Func<ImportFormat, IDataFileReaderService>>( ) );
            mock.Provide( Factory.Current.Resolve<RecordImporter.Factory>( ) );
            mock.Provide( Factory.Current.Resolve<IEntitySaver>( ) );
            return mock;
        }

        public ImportRun GetMockRun( string token )
        {
            EntityType type = new EntityType();

            ApiResourceMapping mapping = new ApiResourceMapping();
            mapping.MappedType = type;
            mapping.MappingSourceReference = "Test1";
            mapping.ImportHeadingRow = 3;
            mapping.ImportDataRow = 4;

            ImportConfig importConfig = new ImportConfig( );
            importConfig.ImportFileType_Enum = ImportFileTypeEnum_Enumeration.ImportFileTypeExcel;
            importConfig.ImportConfigMapping = mapping;

            ImportRun importRun = new ImportRun( );
            importRun.ImportConfigUsed = importConfig;
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunQueued;
            importRun.ImportFileId = token;
            return importRun;
        }

        public IEntityRepository GetMockEntityRepository(long runId, ImportRun importRun)
        {
            var entityRepository = new Mock<IEntityRepository>( );
            entityRepository.Setup(r => r.Get<ImportRun>( runId)).Returns(importRun);
            return entityRepository.Object;
        }

        public IFileRepository GetMockFileRepository( string fileToken )
        {
            Mock<IFileRepository> fileRepository = new Mock<IFileRepository>( MockBehavior.Strict );
            fileRepository.Setup( f => f.Get( fileToken ) ).Returns( SheetTestHelper.GetStream( fileToken ) );
            return fileRepository.Object;
        }

        ImportRun GetImportRun(AutoMock mock)
        {
            return mock.Container.Resolve<ImportRun>( );
        }

        void RunAndAssertFailure(AutoMock mock, string message)
        {
            ImportRunWorker worker = mock.Create<ImportRunWorker>( );
            worker.StartImport( MockRunId );

            var importRun = GetImportRun( mock );

            AssertImportFailed(importRun, message);
        }

        void AssertImportFailed( ImportRun importRun, string message)
        {
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunFailed ) );
            Assert.That( importRun.ImportRunFinished, Is.Not.Null );
            Assert.That( importRun.ImportMessages, Is.EqualTo( "Failed: " + message + "\r\n" ) );
        }

    }
}
