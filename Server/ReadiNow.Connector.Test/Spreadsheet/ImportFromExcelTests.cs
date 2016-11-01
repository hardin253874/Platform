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
    public class ImportFromExcelTests
    {
        private string ExcelFile = "Test File.xlsx";

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromExcel_TestCurrencyConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestCurrencyEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, ExcelFile, ImportFormat.Excel, "Currency" );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 9 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 4 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 3: 'Currency' value must not be less than 10" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 6: 'Currency' value must not be greater than 100" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 7: 'Currency' value is required" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 8: 'Currency' value must not be less than 10" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromExcel_TestDateConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestDateEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, ExcelFile, ImportFormat.Excel, "Date" );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 10 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 5 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 3: 'Entered Date' value must not be less than 1/06/2012" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 6: 'Entered Date' value must not be greater than 30/06/2012" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 7: 'Entered Date' value is required." ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 8: 'Entered Date' value must not be less than 1/06/2012" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 9: 'Entered Date' value must not be greater than 30/06/2012" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromExcel_TestDecimalConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestDecimalEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, ExcelFile, ImportFormat.Excel, "Decimal" );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 9 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 4 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 3: 'Decimal' value must not be less than 10" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 6: 'Decimal' value must not be greater than 100" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 7: 'Decimal' value is required" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 8: 'Decimal' value must not be less than 10" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromExcel_TestNumericConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestNumericEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, ExcelFile, ImportFormat.Excel, "Numeric" );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 7 ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 10 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 3 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 2: 'Numeric' value must not be less than 10" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 7: 'Numeric' value is required" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 11: 'Numeric' value must not be greater than 99" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 7 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromExcel_TestStringConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestStringEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, ExcelFile, ImportFormat.Excel, "String" );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 5 ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 9 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 4 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 2: 'State' length must not be less than 3" ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 7: 'City' value is required." ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 8: 'City' value is required." ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 3: 'String1' length must not be greater than 20" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 5 ) );

        }

        [Test]
        [RunAsDefaultTenant]
        public void ImportDataFromExcel_TestBooleanConstraints_Valid( )
        {
            EntityType type = ImportTestHelper.CreateTestBooleanEntityType( );
            ImportRun importRun = ImportTestHelper.RunTest( type, ExcelFile, ImportFormat.Excel, "Boolean" );

            //check for the import status
            Assert.That( importRun.ImportRunStatus_Enum, Is.EqualTo( WorkflowRunState_Enumeration.WorkflowRunCompleted ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 13 ), importRun.ImportMessages );
            Assert.That( importRun.ImportRecordsTotal, Is.EqualTo( 14 ) );
            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 1 ) );
            Assert.That( importRun.ImportMessages, Contains.Substring( "Row 15: Value for 'Boolean_True' was formatted incorrectly" ) );

            var instances = Entity.GetInstancesOfType( type, false );
            Assert.That( instances, Has.Count.EqualTo( 13 ) );
        }
    }
}