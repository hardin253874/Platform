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
    class CardinalityTests
    {
        [Test]
        public void Test_OneToOne( )
        {
            EntityType type1 = null;
            EntityType type2 = null;
            Relationship rel;

            try
            {
                // Define schema
                type1 = new EntityType( );
                type1.Save( );

                type2 = new EntityType( );
                type2.Save( );

                rel = new Relationship
                {
                    Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne,
                    FromType = type1,
                    ToType = type2
                };
                rel.Save( );

                // Target
                IEntity instance2 = Entity.Create( type2.Id );
                instance2.Save( );

                // Source 1
                IEntity instance1 = Entity.Create( type1.Id );
                instance1.GetRelationships( rel.Id, Direction.Forward ).Add( instance2 );
                instance1.Save( );

                // Conflicting 1b
                IEntity instance1B = Entity.Create( type1.Id );
                instance1B.GetRelationships( rel.Id, Direction.Forward ).Add( instance2 );
                Assert.Throws<CardinalityViolationException>( ( ) => instance1B.Save( ) );

                var instances = Entity.GetInstancesOfType( type1 ).ToList( );
                Assert.That( instances, Has.Count.EqualTo( 1 ), "Final instance count" );
            }
            finally
            {
                type1?.Delete( );
                type2?.Delete( );
            }
        }

        
        [Test]
        public void Test_OneToOne_Import_27716( )
        {
            EntityType type1 = null;
            EntityType type2 = null;
            ImportConfig config = null;
            Relationship rel;

            try
            {
                // Define schema
                type1 = new EntityType( );
                type1.Save( );

                type2 = new EntityType( );
                type2.Save( );

                rel = new Relationship
                {
                    Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne,
                    FromType = type1,
                    ToType = type2
                };
                rel.Save( );

                // Existing instance
                IEntity instance2 = Entity.Create( type2.Id );
                instance2.SetField( Resource.Name_Field, "Target" );
                instance2.Save( );

                // Define import config
                config = new ImportConfig
                {
                    ImportFileType_Enum = ImportFileTypeEnum_Enumeration.ImportFileTypeCsv,
                    ImportConfigMapping = new ApiResourceMapping
                    {
                        ImportHeadingRow = 0,
                        ImportDataRow = 1,
                        MappedType = type1,
                        ResourceMemberMappings =
                        {
                            new ApiRelationshipMapping { Name = "1", MappedRelationship = rel, MapRelationshipInReverse = false }.As<ApiMemberMapping>( )
                        }
                    }
                };

                string csvData = "Target";

                // Run import a first time
                ImportRun importRun = ImportTestHelper.CsvImport( config, csvData );
                Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );

                // Check that instance and relationship got created
                IEntity instance1 = Entity.GetInstancesOfType( type1 ).FirstOrDefault( );
                Assert.That( instance1, Is.Not.Null );
                var relValues = instance1.GetRelationships( rel.Id, Direction.Forward ).ToList( );
                Assert.That( relValues, Has.Count.EqualTo( 1 ) );
                Assert.That( relValues[ 0 ].Id, Is.EqualTo( instance2.Id ) );

                // Run import a second time
                importRun = ImportTestHelper.CsvImport( config, csvData );
                Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunFailed ), importRun.ImportMessages );
                var instances = Entity.GetInstancesOfType( type1 ).ToList();
                Assert.That( importRun.ImportMessages, Contains.Substring( "Line 1: The source item cannot be associated with Target because it is already associated with another record" ) );
                Assert.That( instances, Has.Count.EqualTo( 1 ), "Final instance count" );

            }
            finally
            {
                type1?.Delete( );
                type2?.Delete( );
                config?.Delete( );
            }
        }

    }
}
