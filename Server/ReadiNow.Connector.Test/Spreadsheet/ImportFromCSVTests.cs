// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    ///     Summary description for ImportFromExcelTests
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class ImportFromCsvTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromCSV_TestCurrencyConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestCurrencyEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, "Currency.csv", ImportFormat.CSV );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ) );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 9 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 4 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 3: 'Currency' value must not be less than 10" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 6: 'Currency' value must not be greater than 100" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 7: 'Currency' value is required" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 8: 'Currency' value must not be less than 10" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromCSV_TestDateConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestDateEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, "DateTime.csv", ImportFormat.CSV );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ) );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 10 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 5 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 3: 'Entered Date' value must not be less than 1/06/2012" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 6: 'Entered Date' value must not be greater than 30/06/2012" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 7: 'Entered Date' value is required." ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 8: 'Entered Date' value must not be less than 1/06/2012" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 9: 'Entered Date' value must not be greater than 30/06/2012" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromCSV_TestDecimalConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestDecimalEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, "Decimal.csv", ImportFormat.CSV );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ) );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 9 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 4 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 3: 'Decimal' value must not be less than 10" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 6: 'Decimal' value must not be greater than 100" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 7: 'Decimal' value is required" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 8: 'Decimal' value must not be less than 10" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromCSV_TestNumericConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestNumericEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, "Numeric.csv", ImportFormat.CSV );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 7 ) );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 10 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 3 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 2: 'Numeric' value must not be less than 10" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 7: 'Numeric' value is required" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 11: 'Numeric' value must not be greater than 99" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 7 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromCSV_TestStringConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestStringEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, "String.csv", ImportFormat.CSV );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ) );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 9 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 4 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 2: 'State' length must not be less than 3" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 7: 'City' value is required." ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 8: 'City' value is required." ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 3: 'String1' length must not be greater than 20" ) ); // this should say line 4, but there's a problem in the .Net TextFieldParser

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromExcel_TestBooleanConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestBooleanEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, "Boolean.csv", ImportFormat.CSV );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 13 ) );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 14 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 1 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Line 15: Value for 'Boolean_False' was formatted incorrectly" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 13 ) );
        }
    }
}