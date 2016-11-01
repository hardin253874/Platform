// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using EDC.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using ReadiNow.Connector.ImportSpreadsheet;
using ReadiNow.Connector.Interfaces;

namespace ReadiNow.Connector.Test.Spreadsheet
{
	/// <summary>
	///     Helper methods to test import function
	/// </summary>
	public static class ImportTestHelper
    {

        public static ImportRun CsvImport( ImportConfig config, string csvData )
        {
            ImportSettings settings = new ImportSettings
            {
                FileToken = "whatever",
                ImportConfigId = config.Id
            };


            IFileRepository fileRepository = MockFileRepository.FromText( csvData );

            using ( var scope = Factory.Current
                .BeginLifetimeScope( builder => builder.RegisterInstance( fileRepository ).Named<IFileRepository>( FileRepositoryModule.TemporaryFileRepositoryName ) ) )
            {
                Assert.That( scope.ResolveKeyed<IFileRepository>( FileRepositoryModule.TemporaryFileRepositoryName ), Is.InstanceOf<MockFileRepository>( ) );

                // Create import run
                var si = ( SpreadsheetImporter ) scope.Resolve<ISpreadsheetImporter>( );
                ImportRun importRun = si.CreateImportRunEntity( config, settings );
                long importRunId = importRun.Id;

                IImportRunWorker worker = scope.Resolve<IImportRunWorker>( );

                worker.StartImport( importRunId );
                return importRun;
            }
        }

        private static ImportConfig CreateImportConfig( EntityType entityType, ImportFormat importFormat, string sheetId )
        {
            ApiResourceMapping resourceMapping = new ApiResourceMapping( );
            resourceMapping.MappedType = entityType;
            resourceMapping.MappingSourceReference = sheetId;
            resourceMapping.ImportHeadingRow = 1;
            resourceMapping.ImportDataRow = 2;
            ImportConfig importConfig = new ImportConfig( );
            importConfig.ImportConfigMapping = resourceMapping;
            importConfig.ImportFileType_Enum = importFormat == ImportFormat.CSV ? ImportFileTypeEnum_Enumeration.ImportFileTypeCsv : ImportFileTypeEnum_Enumeration.ImportFileTypeExcel;
            return importConfig;
        }

        private static ImportRun CreateImportRun( ImportConfig importConfig, string fileToken )
        {
            SpreadsheetImporter importer = ( SpreadsheetImporter ) Factory.Current.Resolve<ISpreadsheetImporter>( );
            ImportSettings importSettings = new ImportSettings
            {
                FileToken = fileToken,
                ImportConfigId = importConfig.Id,
                TimeZoneName = ""
            };
            ImportRun importRun = importer.CreateImportRunEntity( importConfig, importSettings );
            return importRun;
        }

        /// <summary>
        ///     Get all the mapping columns from selected entity Type.
        /// </summary>
        private static void AddAllFields( ImportConfig importConfig, SampleTable sample ) // List<ColumnInfo> mappingColumnCollection, EntityType type, DbDataTable sampleDataTable )
        {
            EntityType type = importConfig.ImportConfigMapping.MappedType;
            var allFields = EntityTypeHelper.GetAllFields( type );

            foreach ( Field field in allFields )
            {
                if ( field.Name == "Alias" )
                    continue;

                SampleColumn column = sample.Columns.FirstOrDefault( col => col.Name == field.Name );
                if ( column == null )
                    continue;

                ApiFieldMapping fieldMapping = new ApiFieldMapping( );
                fieldMapping.Name = column.ColumnName;
                fieldMapping.MappedField = field;
                importConfig.ImportConfigMapping.ResourceMemberMappings.Add( fieldMapping.As<ApiMemberMapping>( ) );
            }
        }

        public static ImportRun RunTest( EntityType entityType, string fileName, ImportFormat importFormat, string sheetName = null )
        {
            string fileToken;

            using ( Stream stream = SheetTestHelper.GetStream( fileName ) )
            {
                fileToken = FileRepositoryHelper.AddTemporaryFile( stream );
            }


            EntityType type = entityType;
            ImportConfig importConfig = CreateImportConfig( type, importFormat,  sheetName );

            ImportRun importRun = CreateImportRun( importConfig, fileToken );

            ISpreadsheetInspector inspector = Factory.Current.Resolve<ISpreadsheetInspector>( );
            SpreadsheetInfo info = inspector.GetSpreadsheetInfo( fileToken, importFormat );
            SampleTable sample = inspector.GetSampleTable( fileToken, importFormat, sheetName, 1, 2, null );
            AddAllFields( importConfig, sample );

            // Run import
            IImportRunWorker worker = Factory.Current.Resolve<IImportRunWorker>( );
            worker.StartImport( importRun.Id );

            return importRun;
        }

        /// <summary>
        ///     Get all the mapping columns from selected entity Type.
        /// </summary>
        public static void AddAllFields( ImportConfig importConfig ) // List<ColumnInfo> mappingColumnCollection, EntityType type, DbDataTable sampleDataTable )
		{
		    EntityType type = importConfig.ImportConfigMapping.MappedType;
		    var allFields = EntityTypeHelper.GetAllFields( type );

			foreach ( Field field in allFields )
			{
                if ( field.Name == "Alias" )
                    continue;

			    ApiFieldMapping fieldMapping = new ApiFieldMapping( );
			    fieldMapping.Name = field.Name;
			    fieldMapping.MappedField = field;
            }
		}

		/// <summary>
		///     create test entity type with boolean fields.
		/// </summary>
		public static EntityType CreateTestBooleanEntityType( )
		{
			var newEntity = Entity.Create<EntityType>( );
			var userResource = Entity.Get<EntityType>( new EntityRef( "userResource" ) );
			newEntity.Inherits.Add( userResource );
			newEntity.Name = "Test Boolean";
			newEntity.Description = "Test Boolean Description";
			newEntity.Alias = "testBoolean";
			newEntity.Save( );

			var fieldCollection = new List<Field>( );
			//Create string fields to test
			IEntity entity = Entity.Create( new EntityRef( "boolField" ) );
			var newField = entity.As<BoolField>( );
			newField.Name = "Boolean_True";
			newField.IsRequired = true;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "boolField" ) );
			newField = entity.As<BoolField>( );
			newField.Name = "Boolean_False";
			newField.IsRequired = true;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			var newstringField = entity.As<StringField>( );
			newstringField.Name = "Result";
			newstringField.Save( );
			fieldCollection.Add( newstringField.As<Field>( ) );

			newEntity.Fields.AddRange( fieldCollection );
			newEntity.Save( );
			return newEntity;
		}

		/// <summary>
		///     create test entity type with currency field.
		/// </summary>
		public static EntityType CreateTestCurrencyEntityType( )
		{
			var newEntity = Entity.Create<EntityType>( );
			var userResource = Entity.Get<EntityType>( new EntityRef( "userResource" ) );
			newEntity.Inherits.Add( userResource );
			newEntity.Name = "Test Currency";
			newEntity.Description = "Test Currency Description";
			newEntity.Alias = "testCurrency";
			newEntity.Save( );

			var fieldCollection = new List<Field>( );
			//Create string fields to test
			IEntity entity = Entity.Create( new EntityRef( "currencyField" ) );
			var newField = entity.As<CurrencyField>( );
			newField.Name = "Currency";
			newField.IsRequired = true;
			newField.MinDecimal = 10;
			newField.MaxDecimal = 100;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			var newstringField = entity.As<StringField>( );
			newstringField.Name = "Result";
			newstringField.Save( );
			fieldCollection.Add( newstringField.As<Field>( ) );

			newEntity.Fields.AddRange( fieldCollection );
			newEntity.Save( );
			return newEntity;
		}

		/// <summary>
		///     create test entity type with datetime field.
		/// </summary>
		public static EntityType CreateTestDateEntityType( )
		{
			var newEntity = Entity.Create<EntityType>( );
			var userResource = Entity.Get<EntityType>( new EntityRef( "userResource" ) );
			newEntity.Inherits.Add( userResource );
			newEntity.Name = "Test DateTime";
			newEntity.Description = "Test DateTime Description";
			newEntity.Alias = "testDateTime";
			newEntity.Save( );

			var fieldCollection = new List<Field>( );
			//Create string fields to test
			IEntity entity = Entity.Create( new EntityRef( "dateField" ) );
			var newField = entity.As<DateField>( );
			newField.Name = "Entered Date";
			newField.IsRequired = true;
			newField.MinDate = new DateTime( 2012, 06, 01 );
			newField.MaxDate = new DateTime( 2012, 06, 30 );
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			var newstringField = entity.As<StringField>( );
			newstringField.Name = "Result";
			newstringField.Save( );
			fieldCollection.Add( newstringField.As<Field>( ) );

			newEntity.Fields.AddRange( fieldCollection );
			newEntity.Save( );
			return newEntity;
		}

		/// <summary>
		///     create test entity type with Decimal field.
		/// </summary>
		public static EntityType CreateTestDecimalEntityType( )
		{
			var newEntity = Entity.Create<EntityType>( );
			var userResource = Entity.Get<EntityType>( new EntityRef( "userResource" ) );
			newEntity.Inherits.Add( userResource );
			newEntity.Name = "Test Decimal";
			newEntity.Description = "Test Decimal Description";
			newEntity.Alias = "testDecimal";
			newEntity.Save( );

			var fieldCollection = new List<Field>( );
			//Create string fields to test
			IEntity entity = Entity.Create( new EntityRef( "decimalField" ) );
			var newField = entity.As<DecimalField>( );
			newField.Name = "Decimal";
			newField.IsRequired = true;
			newField.MinDecimal = 10;
			newField.MaxDecimal = 100;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			var newstringField = entity.As<StringField>( );
			newstringField.Name = "Result";
			newstringField.Save( );
			fieldCollection.Add( newstringField.As<Field>( ) );

			newEntity.Fields.AddRange( fieldCollection );
			newEntity.Save( );
			return newEntity;
		}

		/// <summary>
		///     create test entity type with numeric field.
		/// </summary>
		public static EntityType CreateTestNumericEntityType( )
		{
			var newEntity = Entity.Create<EntityType>( );
			var userResource = Entity.Get<EntityType>( new EntityRef( "userResource" ) );
			newEntity.Inherits.Add( userResource );
			newEntity.Name = "Test Numeric";
			newEntity.Description = "Test Numeric Description";
			newEntity.Alias = "testNumeric";
			newEntity.Save( );

			var fieldCollection = new List<Field>( );
			//Create string fields to test
			IEntity entity = Entity.Create( new EntityRef( "intField" ) );
			var newField = entity.As<IntField>( );
			newField.Name = "Numeric";
			newField.IsRequired = true;
			newField.MinInt = 10;
			newField.MaxInt = 99;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			var newstringField = entity.As<StringField>( );
			newstringField.Name = "Result";
			newstringField.Save( );
			fieldCollection.Add( newstringField.As<Field>( ) );

			newEntity.Fields.AddRange( fieldCollection );
			newEntity.Save( );
			return newEntity;
		}

		/// <summary>
		///     create test entity type with string fields.
		/// </summary>
		public static EntityType CreateTestStringEntityType( )
		{
			var newEntity = Entity.Create<EntityType>( );
			var userResource = Entity.Get<EntityType>( new EntityRef( "userResource" ) );
			newEntity.Inherits.Add( userResource );
			newEntity.Name = "Test String";
			newEntity.Description = "Test String Description";
			newEntity.Alias = "testString";
			newEntity.Save( );

			var fieldCollection = new List<Field>( );
			//Create string fields to test
			IEntity entity = Entity.Create( new EntityRef( "stringField" ) );
			var newField = entity.As<StringField>( );
			newField.Name = "String1";
			newField.IsRequired = true;
			newField.MaxLength = 20;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			newField = entity.As<StringField>( );
			newField.Name = "City";
			newField.IsRequired = true;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			newField = entity.As<StringField>( );
			newField.Name = "State";
			newField.MinLength = 3;
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			entity = Entity.Create( new EntityRef( "stringField" ) );
			newField = entity.As<StringField>( );
			newField.Name = "Country";
			newField.Save( );
			fieldCollection.Add( newField.As<Field>( ) );

			newEntity.Fields.AddRange( fieldCollection );
			newEntity.Save( );
			return newEntity;
		}

	}
}