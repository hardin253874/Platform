// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using ReadiNow.Connector.Test.Spreadsheet;

namespace ReadiNow.Connector.Test.Processing
{
    /// <summary>
    /// Tests for merging data
    /// </summary>
    [TestFixture]
    [RunAsDefaultTenant]
    class ImportMergeTests
    {
        [TestCase( "Unique", null, null, 1, 0, 2 )]
        [TestCase( "Unique\nUnique", null, "A resource with duplicate key fields already exists.", 1, 1, 2 )]
        [TestCase( "Existing", null, "A resource with duplicate key fields already exists.", 0, 1, 1 )]
        [TestCase( "Existing", "custom message", "Custom error", 0, 1, 1 )]
        [TestCase( "Existing", "key merges", null, 1, 0, 1 )]
        [TestCase( "Existing", "config merges", null, 1, 0, 1 )]
        [TestCase( "Existing\nNovel", null, "duplicate", 1, 1, 2 )]
        [TestCase( "Existing\nNovel", "key merges", null, 2, 0, 2 )]
        [TestCase( "Existing\nNovel", "config merges", null, 2, 0, 2 )]
        public void Test_ResourceKey_MergeScenarios( string csvData, string special, string expectErrorContains, int expectSucceed, int expectFail, int expectedInstances )
        {
            EntityType type = null;
            ImportConfig config = null;

            try
            {
                // Define schema
                Field keyField = Resource.Name_Field.As<Field>( );

                type = new EntityType
                {
                    ResourceKeys =
                {
                    new ResourceKey
                    {
                        ResourceKeyMessage = special == "custom message" ? "Custom error" : null,
                        MergeDuplicates = special == "key merges",
                        KeyFields = { keyField }
                    }
                }
                };
                type.Save( );

                // Existing instance
                IEntity instance = Entity.Create( type.Id );
                instance.SetField( keyField.Id, "Existing" );
                instance.Save( );

                // Define import config
                config = new ImportConfig
                {
                    ImportFileType_Enum = ImportFileTypeEnum_Enumeration.ImportFileTypeCsv,
                    ImportConfigMapping = new ApiResourceMapping
                    {
                        ImportMergeExisting = special == "config merges",
                        ImportHeadingRow = 0,
                        ImportDataRow = 1,
                        MappedType = type,
                        ResourceMemberMappings =
                    {
                        new ApiFieldMapping { Name = "1", MappedField = keyField }.As<ApiMemberMapping>( )
                    }
                    }
                };

                ImportRun importRun = ImportTestHelper.CsvImport( config, csvData );

                if ( expectSucceed > 0 )
                {
                    Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
                }
                else
                {
                    Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunFailed ), importRun.ImportMessages );
                }
                if ( !string.IsNullOrEmpty( expectErrorContains ) )
                {
                    Assert.That( importRun.ImportMessages, Contains.Substring( expectErrorContains ) );
                }

                Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( expectSucceed ), importRun.ImportMessages );
                Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( expectFail ), importRun.ImportMessages );
                int instances = Entity.GetInstancesOfType( type.Id ).Count( );
                Assert.That( instances, Is.EqualTo( expectedInstances ), importRun.ImportMessages );
            }
            finally
            {
                type?.Delete( );
                config?.Delete( );
            }
        }

        [TestCase( "Unique,X", "Hammer", 2 )]
        [TestCase( "Existing,X", "X", 1 )]
        [TestCase( "Existing,", "Hammer", 1 )]
        public void Test_ResourceKey_FieldMerge( string csvData, string expectedMergedValue, int expectedInstances )
        {
            EntityType type = null;
            ImportConfig config = null;

            try
            {
                // Define schema
                Field keyField = new StringField( ).As<Field>( );
                Field overwriteField = new StringField( ).As<Field>( );
                Field untouchedField = new StringField( ).As<Field>( );

                type = new EntityType
                {
                    Fields = { keyField, overwriteField, untouchedField },
                    ResourceKeys =
                {
                    new ResourceKey
                    {
                        Name = "Test resource key",
                        MergeDuplicates = true,
                        KeyFields = { keyField }
                    }
                }
                };
                type.Save( );

                // Existing instance
                IEntity instance = Entity.Create( type.Id );
                instance.SetField( keyField.Id, "Existing" );
                instance.SetField( overwriteField.Id, "Hammer" );
                instance.SetField( untouchedField.Id, "Can't touch this" );
                instance.Save( );

                // Define import config
                config = new ImportConfig
                {
                    ImportFileType_Enum = ImportFileTypeEnum_Enumeration.ImportFileTypeCsv,
                    ImportConfigMapping = new ApiResourceMapping
                    {
                        ImportMergeExisting = true,
                        ImportHeadingRow = 0,
                        ImportDataRow = 1,
                        MappedType = type,
                        ResourceMemberMappings =
                        {
                            new ApiFieldMapping { Name = "1", MappedField = keyField }.As<ApiMemberMapping>( ),
                            new ApiFieldMapping { Name = "2", MappedField = overwriteField }.As<ApiMemberMapping>( ),
                        }
                    }
                };

                ImportRun importRun = ImportTestHelper.CsvImport( config, csvData );

                Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );

                var instances = Entity.GetInstancesOfType( type.Id ).ToList( );
                Assert.That( instances.Count, Is.EqualTo( expectedInstances ) );

                IEntity existingInstance = instances.FirstOrDefault( e => e.GetField<string>( keyField.Id ) == "Existing" );
                Assert.That( existingInstance, Is.Not.Null, "Existing instance." );

                Assert.That( existingInstance.GetField( untouchedField.Id ), Is.EqualTo( "Can't touch this" ) );
                Assert.That( existingInstance.GetField( overwriteField.Id ), Is.EqualTo( expectedMergedValue ) );    // we got lazy and mapped the name column to both the name field and overwrite field.
            }
            finally
            {
                config?.Delete( );
                type?.Delete( );
            }
            
        }
    }
}
