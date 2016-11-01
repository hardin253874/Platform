// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ReadiNow.Connector.ImportSpreadsheet;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class SampleDataCreatorTests
    {
        [Test]
        public void Test_CSV( )
        {
            string csv =
                "TestString,TestNumber\nA,1\nB,2\nC,3\nD,4\nE,5\nA,1\nB,2\nC,3\nD,4\nE,5\n,A,1\nB,2\nC,3\nD,4\nE,5\n";

            var service = new CsvFileReaderService( );
            var settings = new DataFileReaderSettings( );
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetCsvStream( csv ) )
            {
                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 2 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 10 ) );

                SampleColumn column = table.Columns[ 0 ];
                Assert.That( column.ColumnName, Is.EqualTo( "1" ) );
                Assert.That( column.Name, Is.EqualTo( "TestString" ) );
                column = table.Columns[ 1 ];
                Assert.That( column.ColumnName, Is.EqualTo( "2" ) );
                Assert.That( column.Name, Is.EqualTo( "TestNumber" ) );

                SampleRow row = table.Rows[ 0 ];
                Assert.That( row.Values, Has.Count.EqualTo( 2 ) );
                Assert.That( row.Values[ 0 ], Is.EqualTo( "A" ) );
                Assert.That( row.Values[ 1 ], Is.EqualTo( "1" ) );
                row = table.Rows[ 9 ];
                Assert.That( row.Values, Has.Count.EqualTo( 2 ) );
                Assert.That( row.Values[ 0 ], Is.EqualTo( "E" ) );
                Assert.That( row.Values[ 1 ], Is.EqualTo( "5" ) );
            }
        }

        [Test]
        public void Test_CSV_NoData( )
        {
            string csv = "";

            var service = new CsvFileReaderService( );
            var settings = new DataFileReaderSettings( );
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetCsvStream( csv ) )
            {
                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 0 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 0 ) );
            }
        }

        [Test]
        public void Test_CSV_HeadingBeyondData( )
        {
            string csv =
                "\n1,2\nA,B";

            var service = new CsvFileReaderService( );
            var settings = new DataFileReaderSettings( );
            settings.FirstDataRowNumber = 2;
            settings.HeadingRowNumber = 3;
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetCsvStream( csv ) )
            {
                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 2 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 0 ) );

                SampleColumn column = table.Columns [ 0 ];
                Assert.That( column.ColumnName, Is.EqualTo( "1" ) );
                Assert.That( column.Name, Is.EqualTo( "A" ) );
                column = table.Columns [ 1 ];
                Assert.That( column.ColumnName, Is.EqualTo( "2" ) );
                Assert.That( column.Name, Is.EqualTo( "B" ) );
            }
        }

        [Test]
        public void Test_NoHeadingRow( )
        {
            string csv =
                "\nA,B\nC,D";

            var service = new CsvFileReaderService( );
            var settings = new DataFileReaderSettings( );
            settings.FirstDataRowNumber = 1;
            settings.HeadingRowNumber = 0;
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetCsvStream( csv ) )
            {
                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 2 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 2 ) );

                SampleColumn column = table.Columns [ 0 ];
                Assert.That( column.ColumnName, Is.EqualTo( "1" ) );
                Assert.That( column.Name, Is.EqualTo( "1" ) );
                column = table.Columns [ 1 ];
                Assert.That( column.ColumnName, Is.EqualTo( "2" ) );
                Assert.That( column.Name, Is.EqualTo( "2" ) );
            }
        }


        [Test]
        public void Test_Excel_Test1_HeadingOnEveryColumn_NotEnoughData( )
        {
            var service = new ExcelFileReaderService( );
            SampleDataCreator creator = new SampleDataCreator( );
            
            using ( Stream stream = SheetTestHelper.GetStream( "SampleDataTests.xlsx" ) ) // IMPORTANT: Ensure TestRowNumbers has the right number of rows
            {
                var settings = new DataFileReaderSettings( );
                settings.SheetId = SheetTestHelper.GetSheetId( "SampleDataTests.xlsx", "Test1" );
                settings.HeadingRowNumber = 3;
                settings.FirstDataRowNumber = 4;

                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 3 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 4 ) );

                SampleColumn column = table.Columns [ 0 ];
                Assert.That( column.ColumnName, Is.EqualTo( "A" ) );
                Assert.That( column.Name, Is.EqualTo( "Heading1" ) );
                column = table.Columns [ 2 ];
                Assert.That( column.ColumnName, Is.EqualTo( "C" ) );
                Assert.That( column.Name, Is.EqualTo( "Heading3" ) );

                SampleRow row = table.Rows [ 0 ];
                Assert.That( row.Values, Has.Count.EqualTo( 3 ) );
                Assert.That( row.Values [ 2 ], Is.EqualTo( "3" ) );
            }
        }

        [Test]
        public void Test_Excel_Test2_NoHeadingRow( )
        {
            var service = new ExcelFileReaderService( );
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetStream( "SampleDataTests.xlsx" ) ) // IMPORTANT: Ensure TestRowNumbers has the right number of rows
            {
                var settings = new DataFileReaderSettings( );
                settings.SheetId = SheetTestHelper.GetSheetId( "SampleDataTests.xlsx", "Test2" );
                settings.HeadingRowNumber = 0;
                settings.FirstDataRowNumber = 3;

                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 3 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 2 ) );

                SampleColumn column = table.Columns [ 0 ];
                Assert.That( column.ColumnName, Is.EqualTo( "A" ) );
                Assert.That( column.Name, Is.EqualTo( "A" ) );
                column = table.Columns [ 2 ];
                Assert.That( column.ColumnName, Is.EqualTo( "C" ) );
                Assert.That( column.Name, Is.EqualTo( "C" ) );

                SampleRow row = table.Rows [ 0 ];
                Assert.That( row.Values, Has.Count.EqualTo( 3 ) );
                Assert.That( row.Values [ 2 ], Is.EqualTo( "3" ) );
            }
        }

        [Test]
        public void Test_Excel_Test3_TooMuchData( )
        {
            var service = new ExcelFileReaderService( );
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetStream( "SampleDataTests.xlsx" ) ) // IMPORTANT: Ensure TestRowNumbers has the right number of rows
            {
                var settings = new DataFileReaderSettings( );
                settings.SheetId = SheetTestHelper.GetSheetId( "SampleDataTests.xlsx", "Test3" );
                settings.HeadingRowNumber = 3;
                settings.FirstDataRowNumber = 4;

                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 2 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 10 ) );

                SampleRow row = table.Rows [ 9 ];
                Assert.That( row.Values, Has.Count.EqualTo( 2 ) );
                Assert.That( row.Values [ 1 ], Is.EqualTo( "10" ) );
            }
        }

        [Test]
        public void Test_Excel_Test4_AdditionalColumnsFromDataRows( )
        {
            var service = new ExcelFileReaderService( );
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetStream( "SampleDataTests.xlsx" ) ) // IMPORTANT: Ensure TestRowNumbers has the right number of rows
            {
                var settings = new DataFileReaderSettings( );
                settings.SheetId = SheetTestHelper.GetSheetId( "SampleDataTests.xlsx", "Test4" );
                settings.HeadingRowNumber = 3;
                settings.FirstDataRowNumber = 4;

                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 4 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 2 ) );

                Assert.That( table.Columns [ 0 ].ColumnName, Is.EqualTo( "A" ) );
                Assert.That( table.Columns [ 0 ].Name, Is.EqualTo( "Heading 1" ) );
                Assert.That( table.Columns [ 1 ].ColumnName, Is.EqualTo( "B" ) );
                Assert.That( table.Columns [ 1 ].Name, Is.EqualTo( "Heading 2" ) );
                Assert.That( table.Columns [ 2 ].ColumnName, Is.EqualTo( "C" ) );
                Assert.That( table.Columns [ 2 ].Name, Is.EqualTo( "C" ) );
                Assert.That( table.Columns [ 3 ].ColumnName, Is.EqualTo( "D" ) );
                Assert.That( table.Columns [ 3 ].Name, Is.EqualTo( "D" ) );
            }
        }

        [Test]
        public void Test_Excel_Test5_GapsInColumns( )
        {
            var service = new ExcelFileReaderService( );
            SampleDataCreator creator = new SampleDataCreator( );

            using ( Stream stream = SheetTestHelper.GetStream( "SampleDataTests.xlsx" ) ) // IMPORTANT: Ensure TestRowNumbers has the right number of rows
            {
                var settings = new DataFileReaderSettings( );
                settings.SheetId = SheetTestHelper.GetSheetId( "SampleDataTests.xlsx", "Test5" );
                settings.HeadingRowNumber = 3;
                settings.FirstDataRowNumber = 4;

                SampleTable table = creator.CreateSample( stream, settings, service );

                Assert.That( table, Is.Not.Null );
                Assert.That( table.Columns, Has.Count.EqualTo( 2 ) );
                Assert.That( table.Rows, Has.Count.EqualTo( 2 ) );

                Assert.That( table.Columns [ 0 ].ColumnName, Is.EqualTo( "A" ) );
                Assert.That( table.Columns [ 0 ].Name, Is.EqualTo( "Heading1" ) );
                Assert.That( table.Columns [ 1 ].ColumnName, Is.EqualTo( "C" ) );
                Assert.That( table.Columns [ 1 ].Name, Is.EqualTo( "Heading2" ) );

            }
        }

    }
}
